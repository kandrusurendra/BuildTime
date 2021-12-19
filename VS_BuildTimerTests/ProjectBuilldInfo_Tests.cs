using System;
using System.Collections.Generic;
using VSBuildTimer;
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
        public void ExtractProjectName()
        {
            List<string> strings = new List<string>{
                "1>------ Rebuild All started: Project: adv-file-ops, Configuration: Debug Win32 ------"
            ,   "1>------ Neues Erstellen gestartet: Projekt: adv-file-ops, Konfiguration: Release x64 ------"
            ,   "1>------ 已启动生成: 项目: adv-file-ops, 配置: Debug x64 ------"
            };

            foreach (string s in strings)
            {
                string projName = BuildInfoUtils.ExtractProjectName(s);
                Assert.IsTrue(projName != null);
                Assert.AreEqual("adv-file-ops", projName);
            }
        }

        [TestMethod]
        public void ExtractProjectNameAndID_ValidString_ContainsFullStop()
        {
            string s = "2>------ Rebuild All started: Project: FASTER.core, Configuration: Debug Win32 ------";
            Tuple<int, string> val = BuildInfoUtils.ExtractProjectNameAndID(s);
            Assert.IsTrue(val != null);
            Assert.AreEqual(2, val.Item1);
            Assert.AreEqual("FASTER.core", val.Item2);
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
        public void CreateToolTipText()
        {
            var buildInfo = new ProjectBuildInfo("projA", 0, new DateTime(2018, 5, 5, 2, 3, 4), new TimeSpan(0, 0, 0, 10));
            string text = BuildInfoUtils.CreateToolTipText(buildInfo);
            Assert.IsTrue(text.Length > 0);
            Assert.IsTrue(text.Contains("2:03:04"));
            Assert.IsTrue(text.Contains("2:03:14"));
        }
    }
}
