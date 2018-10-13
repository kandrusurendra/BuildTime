using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    public class OutputWindowInterativeInfoExtractor : IBuildInfoExtractionStrategy
    {
        public OutputWindowInterativeInfoExtractor(IEventRouter evtRouter, ILogger logger=null)
        {
            if (evtRouter == null)
                throw new System.ArgumentNullException("evtRouter");

            this.m_evtRouter = evtRouter;
            this.m_evtRouter.OutputPaneUpdated += this.OnOutputPaneUpdated;

            m_logger = (logger != null) ? logger : new NullLogger();
        }

        public IEventRouter EventRouter { get { return this.m_evtRouter; } }

        public List<ProjectBuildInfo> GetBuildProgressInfo()
        {
            return this.m_projectBuildInfo;
        }

        private void OnOutputPaneUpdated(object sender, OutputWndEventArgs args)
        {
            if (args.WindowPane.Name == "Build")
            {
                args.WindowPane.TextDocument.Selection.SelectAll();
                this.UpdateBuildInfo(args.WindowPane.TextDocument.Selection.Text);
            }
        }

        private void UpdateBuildInfo(string newBuildOutputStr)
        {
            string diff;
            List<ProjectBuildInfo> currentBuildInfo;
            if (newBuildOutputStr.StartsWith(this.m_buildOutputStr))
            {
                // Text has been appended to the already existing text of the window.
                // Extract the newly added text (also known as diff).
                diff = newBuildOutputStr.Substring(this.m_buildOutputStr.Length);
                currentBuildInfo = this.m_projectBuildInfo;
            }
            else
            {
                // Window has been cleared and new text has been added.
                // Set the diff equal to the entire content of the window and
                // reset the project build info.
                diff = newBuildOutputStr;
                currentBuildInfo = new List<ProjectBuildInfo>();
            }


            // Given the newly added text and the previous build info, calculate the new build info.
            List<int> newlyStarted, newlyFinished;
            this.m_projectBuildInfo = CalculateProjectBuildInfo(
                currentBuildInfo, diff, DateTime.Now,
                out newlyStarted, out newlyFinished);

            // Update the build-output string, so that the diff can be calculated correctly next time.
            this.m_buildOutputStr = newBuildOutputStr;

            foreach (int id in newlyStarted)
            {
                var projInfo = this.m_projectBuildInfo.First(info => info.ProjectId == id);
                this.m_logger.LogMessage(string.Format("Build of {0} started.", projInfo.ProjectName), 
                    LogLevel.UserInfo);
            }

            foreach (int id in newlyFinished)
            {
                var projInfo = this.m_projectBuildInfo.First(info => info.ProjectId == id);
                this.m_logger.LogMessage(string.Format("Build of {0} finished.", projInfo.ProjectName),
                    LogLevel.UserInfo);
            }
        }

        public static List<ProjectBuildInfo> CalculateProjectBuildInfo(
            List<ProjectBuildInfo> prevBuildInfo,
            string newBuildOutput,
            System.DateTime currentTime,
            out List<int> newlyStartedProjects,
            out List<int> newlyFinishedProjects)
        {
            // All projects should already have a start time. If not it is impossible to calculate the end time.
            bool startTimesValid = prevBuildInfo.All(buildInfo => buildInfo.BuildStartTime.HasValue);
            if (!startTimesValid) throw new System.ArgumentException("Projects with an invalid start time found.");

            List<ProjectBuildInfo> newBuildInfo = new List<ProjectBuildInfo>(prevBuildInfo);

            // Initialize output lists
            newlyStartedProjects = new List<int>();
            newlyFinishedProjects = new List<int>();

            string[] lines = newBuildOutput.Split('\n');
            foreach (string line in lines)
            {
                // Check if this line contains a project name and id. If the id has not been
                // encountered before, this is a new project and should be added to the list.
                {
                    Tuple<int, string> nameAndId = BuildInfoUtils.ExtractProjectNameAndID(line);
                    if (nameAndId != null)
                    {
                        if (!prevBuildInfo.Any(buildInfo => buildInfo.ProjectId == nameAndId.Item1))   // check if any existing item has this id
                        {
                            // this project is encountered for the first time.
                            var projInfo = new ProjectBuildInfo
                            {
                                ProjectId = nameAndId.Item1,
                                ProjectName = nameAndId.Item2,
                                BuildStartTime = currentTime
                            };
                            newBuildInfo.Add(projInfo);
                            newlyFinishedProjects.Add(projInfo.ProjectId);
                            continue;
                        }
                    }
                }

                // Check if this line contains info about a project build being completed.
                // If yes, calculate the build duration and update the info.
                {
                    Tuple<bool, int> resultAndID = BuildInfoUtils.ExtractBuildResultAndProjectID(line);
                    if (resultAndID != null)
                    {
                        var projInfo = newBuildInfo.FirstOrDefault(buildInfo => buildInfo.ProjectId == resultAndID.Item2);
                        if (projInfo != null)
                        {
                            System.Diagnostics.Debug.Assert(projInfo.BuildStartTime.HasValue);
                            projInfo.BuildDuration = currentTime - projInfo.BuildStartTime.Value;
                            projInfo.BuildSucceeded = resultAndID.Item1;
                            newlyFinishedProjects.Add(projInfo.ProjectId);
                            continue;
                        }
                    }
                }

                // If it reaches this point, no project start/end info could be extracted.
                // However we can still get the id of the project and update its end time
                // with the current one (i.e. continuously updating the end time until 
                // build is completed).
                {
                    int? projID = BuildInfoUtils.ExtractProjectID(line);
                    if (projID.HasValue)
                    {
                        var projInfo = newBuildInfo.FirstOrDefault(buildInfo => buildInfo.ProjectId == projID.Value);
                        if (projInfo != null)
                        {
                            System.Diagnostics.Debug.Assert(projInfo.BuildStartTime.HasValue);
                            projInfo.BuildDuration = currentTime - projInfo.BuildStartTime.Value;
                            continue;
                        }
                    }
                }
            }
            return newBuildInfo;
        }


        private readonly IEventRouter m_evtRouter;
        private readonly ILogger m_logger;
        private string m_buildOutputStr = "";
        private List<ProjectBuildInfo> m_projectBuildInfo = new List<ProjectBuildInfo>();
    }

    public class FakeInfoExtractor : IBuildInfoExtractionStrategy
    {
        static readonly List<ProjectBuildInfo> dummyProjectList = new List<ProjectBuildInfo>{
              new ProjectBuildInfo("projA", 1, new DateTime(2018,5,5, 1, 1, 1), new TimeSpan(0,0,0,10,137))
            , new ProjectBuildInfo("projB", 2, new DateTime(2018,5,5, 1, 1, 1), new TimeSpan(0,0,0,20,876))
            , new ProjectBuildInfo("projC", 3, new DateTime(2018,5,5, 1, 1, 11), new TimeSpan(0, 0, 30))
            , new ProjectBuildInfo("projD", 4, new DateTime(2018,5,5, 1, 1, 37), new TimeSpan(0, 0, 52))
            , new ProjectBuildInfo("projE", 5, new DateTime(2018,5,5, 1, 1, 52), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projF", 6, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projG", 7, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 0, 26, 981))
            , new ProjectBuildInfo("projH", 8, new DateTime(2018,5,5, 1, 1, 41), null)
            , new ProjectBuildInfo("projI", 9, new DateTime(2018,5,5, 1, 1, 11), new TimeSpan(0, 0, 30))
            , new ProjectBuildInfo("projJ",10, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projK",11, new DateTime(2018,5,5, 1, 1, 52), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projL",12, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 0, 26, 543))
            , new ProjectBuildInfo("projM",13, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 26))
            , new ProjectBuildInfo("projN",14, new DateTime(2018,5,5, 1, 1, 41), new TimeSpan(0, 0, 10))
            , new ProjectBuildInfo("projO",15, new DateTime(2018,5,5, 1, 1, 11), new TimeSpan(0, 0, 30))
            , new ProjectBuildInfo("projP",16, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projQ",17, new DateTime(2018,5,5, 1, 1, 52), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projR",18, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 26))
            , new ProjectBuildInfo("projS",19, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 17))
            , new ProjectBuildInfo("projT",20, new DateTime(2018,5,5, 1, 2, 0 ), null)
            , new ProjectBuildInfo("projU",21, new DateTime(2018,5,5, 1, 1, 52), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projV",22, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 26))
            , new ProjectBuildInfo("projW",23, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 26))
            , new ProjectBuildInfo("projX",24, new DateTime(2018,5,5, 1, 1, 41), new TimeSpan(0, 0, 10))
            , new ProjectBuildInfo("projY",25, new DateTime(2018,5,5, 1, 1, 11), new TimeSpan(0, 0, 30))
            , new ProjectBuildInfo("projZ",26, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("proj1",27, new DateTime(2018,5,5, 1, 1, 52), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("proj2",28, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 26))
            , new ProjectBuildInfo("projA",28+ 1, new DateTime(2018,5,5, 1, 1, 1), new TimeSpan(0,0,0,10,137))
            , new ProjectBuildInfo("projB",28+ 2, new DateTime(2018,5,5, 1, 1, 1), new TimeSpan(0,0,0,20,876))
            , new ProjectBuildInfo("projC",28+ 3, new DateTime(2018,5,5, 1, 1, 11), new TimeSpan(0, 0, 30))
            , new ProjectBuildInfo("projD",28+ 4, new DateTime(2018,5,5, 1, 1, 37), new TimeSpan(0, 0, 52))
            , new ProjectBuildInfo("projE",28+ 5, new DateTime(2018,5,5, 1, 1, 52), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projF",28+ 6, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projG",28+ 7, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 0, 26, 981))
            , new ProjectBuildInfo("projH",28+ 8, new DateTime(2018,5,5, 1, 1, 41), null)
            , new ProjectBuildInfo("projI",28+ 9, new DateTime(2018,5,5, 1, 1, 11), new TimeSpan(0, 0, 30))
            , new ProjectBuildInfo("projJ",28+10, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projK",28+11, new DateTime(2018,5,5, 1, 1, 52), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projL",28+12, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 0, 26, 543))
            , new ProjectBuildInfo("projM",28+13, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 26))
            , new ProjectBuildInfo("projN",28+14, new DateTime(2018,5,5, 1, 1, 41), new TimeSpan(0, 0, 10))
            , new ProjectBuildInfo("projO",28+15, new DateTime(2018,5,5, 1, 1, 11), new TimeSpan(0, 0, 30))
            , new ProjectBuildInfo("projP",28+16, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projQ",28+17, new DateTime(2018,5,5, 1, 1, 52), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projR",28+18, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 26))
            , new ProjectBuildInfo("projS",28+19, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 17))
            , new ProjectBuildInfo("projT",28+20, new DateTime(2018,5,5, 1, 2, 0 ), null)
            , new ProjectBuildInfo("projU",28+21, new DateTime(2018,5,5, 1, 1, 52), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("projV",28+22, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 26))
            , new ProjectBuildInfo("projW",28+23, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 26))
            , new ProjectBuildInfo("projX",28+24, new DateTime(2018,5,5, 1, 1, 41), new TimeSpan(0, 0, 10))
            , new ProjectBuildInfo("projY",28+25, new DateTime(2018,5,5, 1, 1, 11), new TimeSpan(0, 0, 30))
            , new ProjectBuildInfo("projZ",28+26, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("proj1",28+27, new DateTime(2018,5,5, 1, 1, 52), new TimeSpan(0, 0, 6))
            , new ProjectBuildInfo("proj2",28+28, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 26))
        };

        public List<ProjectBuildInfo> GetBuildProgressInfo()
        {
            return new List<ProjectBuildInfo>(dummyProjectList);
        }
    }
}
