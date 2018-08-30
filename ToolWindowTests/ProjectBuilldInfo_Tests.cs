using System;
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

    }
}
