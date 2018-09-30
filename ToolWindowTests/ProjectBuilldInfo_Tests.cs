using System;
using System.Collections.Generic;
using Microsoft.Samples.VisualStudio.IDE.ToolWindow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToolWindowTests
{
    [TestClass]
    public class ProjectBuilldInfo_Tests
    {
        [TestMethod]
        public void StringToDateTime_ValidDateTimeString()
        {
            string s = "22/07/2018 13:26:35";
            DateTime? dt = BuildInfoUtils.StringToDateTime(s);
            DateTime expected = new System.DateTime(2018, 7, 22, 13, 26, 35);

            Assert.IsTrue(dt.HasValue);
            Assert.AreEqual(expected, dt.Value);
        }

        [TestMethod]
        public void StringToDateTime_InvalidString()
        {
            string s = "22/07/20__18 13:26:35";
            DateTime? dt = BuildInfoUtils.StringToDateTime(s);
            Assert.IsFalse(dt.HasValue);
        }

        [TestMethod]
        public void StringToTime_ValidTimeString()
        {
            {
                string s = "13:26:35.450";
                TimeSpan? dt = BuildInfoUtils.StringToTime(s);
                TimeSpan expected = new TimeSpan(0, 13, 26, 35, 450);

                Assert.IsTrue(dt.HasValue);
                Assert.AreEqual(expected, dt.Value);
            }

            {
                string s = "13:26:35.45";
                TimeSpan? dt = BuildInfoUtils.StringToTime(s);
                TimeSpan expected = new TimeSpan(0, 13, 26, 35, 450);

                Assert.IsTrue(dt.HasValue);
                Assert.AreEqual(expected, dt.Value);
            }
        }

        [TestMethod]
        public void ExtractProjectNameAndID_ValidString()
        {
            string s = "2>------ Rebuild All started: Project: Lib3D, Configuration: Debug Win32 ------";
            Tuple<int, string> val = BuildInfoUtils.ExtractProjectNameAndID(s);
            Assert.IsTrue(val!=null);
            Assert.AreEqual(2, val.Item1);
            Assert.AreEqual("Lib3D", val.Item2);
        }

        [TestMethod]
        public void ExtractStartTimeAndID_ValidString()
        {
            {
                string s = "1>Build started 22/07/2018 16:28:43.";
                Tuple<int, DateTime> val = BuildInfoUtils.ExtractStartTimeAndID(s);
                Assert.IsTrue(val != null);
                Assert.AreEqual(1, val.Item1);
                Assert.AreEqual(new DateTime(2018, 7, 22, 16, 28, 43), val.Item2);
            }

            {
                string s = "3>Build started 30/08/2018 19:56:50.\r";
                Tuple<int, DateTime> val = BuildInfoUtils.ExtractStartTimeAndID(s);
                Assert.IsTrue(val != null);
                Assert.AreEqual(3, val.Item1);
                Assert.AreEqual(new DateTime(2018, 8, 30, 19, 56, 50), val.Item2);
            }
        }

        [TestMethod]
        public void ExtractDurationAndID_ValidString()
        {
            string s = "3>Time Elapsed 01:02:03.57";
            Tuple<int, TimeSpan> val = BuildInfoUtils.ExtractDurationAndID(s);
            Assert.IsTrue(val != null);
            Assert.AreEqual(3, val.Item1);
            Assert.AreEqual(new TimeSpan(0,1,2,3,570), val.Item2);
        }

        [TestMethod]
        public void ExtractPresentationInfo()
        {
            List<ProjectBuildInfo> dummyProjectList = new List<ProjectBuildInfo>{
            // Project name               , ID, Start time (absolute)          , Duration
              new ProjectBuildInfo("projA",  0, new DateTime(2018,5,5, 2, 3, 4), new TimeSpan(0, 0,  0, 10))
            , new ProjectBuildInfo("projB",  1, new DateTime(2018,5,5, 1, 1, 1), new TimeSpan(0, 0,  0, 20))
            , new ProjectBuildInfo("projC",  2, new DateTime(2018,5,5, 4, 5, 6), null )
            , new ProjectBuildInfo("projD",  3, new DateTime(2018,5,5, 8, 9,10), new TimeSpan(0, 0,  1, 37))
            , new ProjectBuildInfo("projE",  4, null, null)
            };

            List<ProjectPresentationInfo> presentationInfo = BuildInfoUtils.ExtractPresentationInfo(dummyProjectList);

            // check relative start times
            Assert.AreEqual(new TimeSpan(1, 2, 3), presentationInfo[0].BuildStartTime_Relative);
            Assert.AreEqual(new TimeSpan(0, 0, 0), presentationInfo[1].BuildStartTime_Relative);
            Assert.AreEqual(new TimeSpan(3, 4, 5), presentationInfo[2].BuildStartTime_Relative);
            Assert.AreEqual(new TimeSpan(7, 8, 9), presentationInfo[3].BuildStartTime_Relative);
            Assert.IsFalse (presentationInfo[4].BuildStartTime_Relative.HasValue);

            // check relative end times; should be relative start time + duration
            Assert.AreEqual(new TimeSpan(1, 2, 13), presentationInfo[0].BuildEndTime_Relative);
            Assert.AreEqual(new TimeSpan(0, 0, 20), presentationInfo[1].BuildEndTime_Relative);
            Assert.AreEqual(new TimeSpan(7, 9, 46), presentationInfo[3].BuildEndTime_Relative);
            Assert.IsFalse(presentationInfo[2].BuildEndTime_Relative.HasValue);
            Assert.IsFalse(presentationInfo[4].BuildEndTime_Relative.HasValue);
        }

        [TestMethod]
        public void CreateToolTipText()
        {
            var buildInfo = new ProjectBuildInfo("projA", 0, new DateTime(2018, 5, 5, 2, 3, 4), new TimeSpan(0, 0, 0, 10));
            var presentationInfo = new ProjectPresentationInfo(new DateTime(2018, 5, 5, 0, 0, 0), buildInfo);
            string text = BuildInfoUtils.CreateToolTipText(presentationInfo);
            Assert.IsTrue(text.Length > 0);
            Assert.IsTrue(text.Contains("2:03:04"));
            Assert.IsTrue(text.Contains("2:03:14"));
        }
    }
}
