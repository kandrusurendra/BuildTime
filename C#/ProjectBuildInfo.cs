using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    public struct ProjectBuildInfo
    {
        string projectName;
        int projectId;
        DateTime? buildStartTime;
        DateTime? buildEndTime;
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

        public static Tuple<int,string> ExtractProjectNameAndID(string s)
        {
            //Example pattern to match: "2>------ Rebuild All started: Project: Lib3D, Configuration: Debug Win32 ------";
            string pattern = @"(\d+)>.+Project:\s+(\w+)";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(s);
            if (m.Success)
            {
                //for (int i = 1; i <= 2; i++)
                //{
                //    Group g = m.Groups[i];
                //    Console.WriteLine("Group" + i + "='" + g + "'");
                //    CaptureCollection cc = g.Captures;
                //    for (int j = 0; j < cc.Count; j++)
                //    {
                //        Capture c = cc[j];
                //        string temp = c.ToString();
                //        System.Console.WriteLine("Capture" + j + "='" + temp + "', Position=" + c.Index);
                //    }
                //}
                int projID = int.Parse(m.Groups[1].Captures[0].ToString());
                string projName = m.Groups[2].Captures[0].ToString();
                return new Tuple<int, string>(projID, projName);
            }
            return null;
        }

        public static Tuple<int,DateTime> ExtractStartTimeAndID(string s)
        {
            // Example string to match "1>Build started 22/07/2018 16:28:43."
            string pattern = @"^(\d+)>\s*Build started\s+(.+)\.$";
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


        public static List<ProjectBuildInfo> ExtractBuildInfo(string text)
        {
            return new List<ProjectBuildInfo>();
        }
    }
}
