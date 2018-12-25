﻿using System;
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

    public class ProjectPresentationInfo
    {
        // Input parameters
        public ProjectPresentationInfo(DateTime referenceTime, ProjectBuildInfo buildInfo)
        {
            if (buildInfo == null) throw new ArgumentNullException("build info");
            if (referenceTime == null) throw new ArgumentNullException("reference time");
            _buildInfo = buildInfo;
            _referenceTime = referenceTime;
        }

        // Transformed data
        public string ProjectName
        {
            get { return _buildInfo.ProjectName; }
        }

        public int ProjectId
        {
            get { return _buildInfo.ProjectId; }
        }

        public DateTime? BuildStartTime_Absolute
        {
            get { return _buildInfo.BuildStartTime; }
        }

        public TimeSpan? BuildDuration
        {
            get { return _buildInfo.BuildDuration; }
        }

        public TimeSpan? BuildStartTime_Relative
        {
            get
            {
                if (_buildInfo.BuildStartTime.HasValue)
                    return _buildInfo.BuildStartTime - _referenceTime;
                return null;
            }
        }
        
        public TimeSpan? BuildEndTime_Relative
        {
            get
            {
                if (_buildInfo.BuildEndTime.HasValue)
                {
                    return _buildInfo.BuildEndTime - _referenceTime;
                }
                return null;
            }
        }

        public bool? BuildSucceeded
        {
            get { return _buildInfo.BuildSucceeded; }
        }

        private readonly DateTime _referenceTime;
        private readonly ProjectBuildInfo _buildInfo;
    }

    public interface IBuildInfoExtractionStrategy
    {
        List<ProjectBuildInfo> GetBuildProgressInfo();
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

        public static Tuple<int,string> ExtractProjectNameAndID(string s)
        {
            // Trim configuration information if any.
            // Unfortunately this is not very robust, VS2017 version
            // output has the form "... Project: projectName, Configuration: Debug Win32".
            // This is not guaranteed to stay like this, so try to remove various variants
            // of the configuration information. Start by trying the most exact versions
            // and progressively move to more relaxed. In any case the Configuration info
            // must be after the project name.
            // e.g. "2>------ Rebuild All started: Project: FASTER.core, Configuration: Debug Win32 ------";
            string sLower = s.ToLower();
            int projectNamePos = sLower.IndexOf("project:") + "project:".Length;
            int configInfoPos = -1;
            if ((configInfoPos = sLower.LastIndexOf(", configuration")) >= projectNamePos)
            {
                s = s.Substring(0, configInfoPos);
            }
            else if ((configInfoPos = s.LastIndexOf("configuration")) >= projectNamePos)
            {
                s = s.Substring(0, configInfoPos);
            }

            //Example pattern to match: "2>------ Rebuild All started: Project: Lib3D, Configuration: Debug Win32 ------";
            string pattern = @"(\d+)>.+Project:\s+(.*)";
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
                    "ID: " + info.ProjectId + "\n" + 
                    "start time: " + TimeSpanToStr(info.BuildStartTime_Relative)  + " (absolute: " + info.BuildStartTime_Absolute + ")\n" +
                    "duration:   " + TimeSpanToStr(info.BuildDuration) + "\n" +
                    "end time:   " + TimeSpanToStr(info.BuildEndTime_Relative) + "\n";
            }
            return "";
        }
    }
}