using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio;


namespace VSBuildTimer
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

        public string Configuration { get; set; }

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

    public class ProjectPresentationInfo
    {
        // Input parameters
        public ProjectPresentationInfo(DateTime referenceTime, ProjectBuildInfo buildInfo)
        {
            if (buildInfo == null) throw new ArgumentNullException("build info");
            if (referenceTime == null) throw new ArgumentNullException("reference time");
            m_buildInfo = buildInfo;
            m_referenceTime = referenceTime;
        }

        // Transformed data
        public string ProjectName
        {
            get { return m_buildInfo.ProjectName; }
        }

        public string Configuration
        {
            get { return m_buildInfo.Configuration; }
        }

        public int ProjectId
        {
            get { return m_buildInfo.ProjectId; }
        }

        public DateTime? BuildStartTime_Absolute
        {
            get { return m_buildInfo.BuildStartTime; }
        }

        public TimeSpan? BuildDuration
        {
            get { return m_buildInfo.BuildDuration; }
        }

        public TimeSpan? BuildStartTime_Relative
        {
            get
            {
                if (m_buildInfo.BuildStartTime.HasValue)
                    return m_buildInfo.BuildStartTime - m_referenceTime;
                return null;
            }
        }
        
        public TimeSpan? BuildEndTime_Relative
        {
            get
            {
                if (m_buildInfo.BuildEndTime.HasValue)
                {
                    return m_buildInfo.BuildEndTime - m_referenceTime;
                }
                return null;
            }
        }

        public bool? BuildSucceeded
        {
            get { return m_buildInfo.BuildSucceeded; }
        }

        private readonly DateTime m_referenceTime;
        private readonly ProjectBuildInfo m_buildInfo;
    }

    public static class BuildInfoUtils
    {
        public static bool IsBuildOutputPane(EnvDTE.OutputWindowPane pane)
        {
            if (pane != null)
            {
                try
                {
                    Guid paneID = Guid.Parse(pane.Guid);
                    return (paneID == VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid);
                }
                catch(System.Exception)
                {
                    return false;
                }
            }
            return false;
        }

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

        public static int? ExtractProjectID(string s)
        {
            string pattern = @"(\d+)>.*";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(s);
            if (m.Success)
            {
                int projID = int.Parse(m.Groups[1].Captures[0].ToString());
                return projID;
            }
            return null;
        }

        public static string ExtractProjectName(string s)
        {
            //Example pattern to match: "2>------ Rebuild All started: Project: Lib3D, Configuration: Debug Win32 ------";
            string pattern = @"\d+>.+:.+:\s*(.*),.*";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(s);
            if (m.Success)
            {
                string projName = m.Groups[1].Captures[0].ToString();
                return projName;
            }
            return null;
        }

        public static Tuple<int,string> ExtractProjectNameAndID(string s)
        {
            int? id = ExtractProjectID(s);
            string name = ExtractProjectName(s);
            if (id.HasValue && name!=null)
            {
                return new Tuple<int, string>(id.Value, name);
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

        public static List<ProjectPresentationInfo> ExtractPresentationInfo(IEnumerable<ProjectBuildInfo> buildInfo)
        {
            DateTime? minStartTime = buildInfo.Min(projectInfo => projectInfo.BuildStartTime);

            // Projects must have a non-empty BuildStartTime in order to be in the list.
            System.Diagnostics.Debug.Assert(minStartTime.HasValue);

            // Update build-info grid.
            var presentationInfo = buildInfo.Select(projectInfo => new ProjectPresentationInfo(minStartTime.Value, projectInfo));
            return presentationInfo.ToList();
        }

        public static string CreateToolTipText(ProjectPresentationInfo info)
        {
            string TimeSpanToStr(TimeSpan? t)
            {
                return (t.HasValue) ? t.Value.ToString(@"dd\.hh\:mm\:ss\.f") : "?";
            };

            if (info != null)
            {
                return
                    "Project: " + info.ProjectName + "\n" +
                    "Config:  " + info.Configuration + "\n" + 
                    "start time: " + TimeSpanToStr(info.BuildStartTime_Relative)  + " (absolute: " + info.BuildStartTime_Absolute + ")\n" +
                    "duration:   " + TimeSpanToStr(info.BuildDuration) + "\n" +
                    "end time:   " + TimeSpanToStr(info.BuildEndTime_Relative) + "\n";
            }
            return "";
        }

        public static void WriteBuildInfoToCSV(IEnumerable<ProjectBuildInfo> buildInfo, System.IO.TextWriter w)
        {
            if (buildInfo == null)
                throw new ArgumentNullException("buildInfo");

            if (w == null)
                throw new ArgumentNullException("w");

            w.Write("\"Project Name\",Configuration,\"Start time (abs)\",Duration,\"End time\",Succeeded\n");
            foreach (var projInfo in buildInfo)
            {
                w.Write("\""+projInfo.ProjectName + "\",");
                w.Write(projInfo.Configuration    + ",");
                w.Write(projInfo.BuildStartTime   + ",");
                w.Write(projInfo.BuildDuration    + ",");
                w.Write(projInfo.BuildEndTime     + ",");
                w.Write((projInfo.BuildSucceeded.HasValue ? projInfo.BuildSucceeded.Value.ToString() : "?") + "\n");
            }
        }
    }
}
