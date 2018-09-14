using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    public class ProjectBuildInfo
    {
        public ProjectBuildInfo() { }

        public ProjectBuildInfo(string name, int id, DateTime? startTime, TimeSpan? duration, bool? buildSucceeded=true)
        {
            ProjectName = name;
            ProjectId = id;
            BuildStartTime = startTime;
            BuildDuration = duration;
            BuildSucceeded = buildSucceeded;
        }

        public string ProjectName { get; set; }

        public int ProjectId { get; set; }

        public DateTime? BuildStartTime { get; set; }

        public TimeSpan? BuildDuration { get; set; }

        public DateTime? BuildEndTime
        {
            get {
                if (BuildStartTime.HasValue && BuildDuration.HasValue)
                {
                    return (BuildStartTime + BuildDuration);
                }
                return null;
            }
        }

        public bool? BuildSucceeded { get; set; }
    }

    public interface IBuildInfoExtractionStrategy
    {
        List<ProjectBuildInfo> ExtractBuildInfo();
    }

    public static class BuildInfoUtils
    {
        public static DateTime? StringToDateTime(string s)
        {
            try
            {
                // e.g. 22/07/2018 13:26:35
                DateTime dt = DateTime.ParseExact(s, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.DefaultThreadCurrentCulture);
                return dt;
            }
            catch(System.Exception e)
            {
                System.Console.WriteLine(e.Message);
                return null;
            }
        }

        public static TimeSpan? StringToTime(string s)
        {
            try
            {
                return TimeSpan.ParseExact(s, "c", null);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
                return null;
            }
        }

        public static Tuple<int,string> ExtractProjectNameAndID(string s)
        {
            //Example pattern to match: "2>------ Rebuild All started: Project: Lib3D, Configuration: Debug Win32 ------";
            string pattern = @"(\d+)>.+Project:\s+(\w+)";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(s);
            if (m.Success)
            {
                int projID = int.Parse(m.Groups[1].Captures[0].ToString());
                string projName = m.Groups[2].Captures[0].ToString();
                return new Tuple<int, string>(projID, projName);
            }
            return null;
        }

        public static Tuple<int,DateTime> ExtractStartTimeAndID(string s)
        {
            // Example string to match "1>Build started 22/07/2018 16:28:43."
            string pattern = @"^(\d+)>\s*Build started\s+(.+)\..*";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(s);
            if (m.Success)
            {
                int projID = int.Parse(m.Groups[1].Captures[0].ToString());
                string dtStr = m.Groups[2].Captures[0].ToString();
                DateTime? dt = StringToDateTime(dtStr);
                if (!dt.HasValue)
                    throw new FormatException("Cannot convert '" + dtStr + "' to a valid date and time.");
                return new Tuple<int, DateTime>(projID, dt.Value);
            }
            return null;
        }

        public static Tuple<int, TimeSpan> ExtractDurationAndID(string s)
        {
            // Example string to match "3>Time Elapsed 01:02:03.57"
            string pattern = @"^(\d+)>\s*Time Elapsed\s+([^\s]+)\s*$";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(s);
            if (m.Success)
            {
                int projID = int.Parse(m.Groups[1].Captures[0].ToString());
                string timeStr = m.Groups[2].Captures[0].ToString();
                TimeSpan? time = StringToTime(timeStr);
                if (!time.HasValue)
                    throw new FormatException("Cannot convert '" + timeStr + "' to a valid time.");
                return new Tuple<int, TimeSpan>(projID, time.Value);
            }
            return null;
        }

        public static Tuple<bool,int> ExtractBuildResultAndProjectID(string s)
        {
            string[] patterns = new string[]{
                @"(\d+)>\s*Build succeeded.*",
                @"(\d+)>\s*Build FAILED.*"
            };

            for (int i=0;i<patterns.Length;++i)
            {
                string pattern = patterns[i];
                bool success = (i == 0);    // success is True if 1st pattern is matched, otherwise is False.
                Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                Match m = r.Match(s);
                if (m.Success)
                {
                    int projID = int.Parse(m.Groups[1].Captures[0].ToString());
                    return new Tuple<bool,int>(success,projID);
                }
            }

            // None of the patterns was matched => this line doesn't contain relevant information.
            return null;
        }

        public static List<ProjectBuildInfo> ExtractBuildInfo(string text)
        {
            string[] lines = text.Split('\n');
            List<ProjectBuildInfo> projectList = new List<ProjectBuildInfo>(); 
            foreach (string line in lines)
            {
                var nameAndID = ExtractProjectNameAndID(line);
                if (nameAndID != null)
                {
                    var buildInfo = new ProjectBuildInfo();
                    buildInfo.ProjectId = nameAndID.Item1;
                    buildInfo.ProjectName = nameAndID.Item2;
                    projectList.Add(buildInfo);
                    continue;
                }

                var startTimeAndID = ExtractStartTimeAndID(line);
                if (startTimeAndID != null)
                {
                    var buildInfo = projectList.First(x => x.ProjectId == startTimeAndID.Item1);
                    buildInfo.BuildStartTime = startTimeAndID.Item2;
                    continue;
                }

                var durationAndID = ExtractDurationAndID(line);
                if (durationAndID != null)
                {
                    var buildInfo = projectList.First(x => x.ProjectId == durationAndID.Item1);
                    buildInfo.BuildDuration = durationAndID.Item2;
                    continue;
                }
            }
            return projectList;
        }
    }
}
