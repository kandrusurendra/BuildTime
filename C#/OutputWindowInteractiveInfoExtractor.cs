﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    public class OutputWindowInterativeInfoExtractor : IBuildInfoExtractionStrategy
    {
        public OutputWindowInterativeInfoExtractor(IEventRouter evtRouter)
        {
            if (evtRouter == null)
                throw new System.ArgumentNullException("evtRouter");

            this.evtRouter = evtRouter;
            this.evtRouter.OutputPaneUpdated += this.OnOutputPaneUpdated;
        }

        public IEventRouter EventRouter { get { return this.evtRouter; } }

        public List<ProjectBuildInfo> ExtractBuildInfo()
        {
            return this.projectBuildInfo;
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
            if (newBuildOutputStr.StartsWith(this.buildOutputStr))
            {
                // Text has been appended to the already existing text of the window.
                // Extract the newly added text (also known as diff).
                diff = newBuildOutputStr.Substring(this.buildOutputStr.Length);
                currentBuildInfo = this.projectBuildInfo;
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
            this.projectBuildInfo = CalculateProjectBuildInfo(currentBuildInfo, diff, DateTime.Now);

            // Update the build-output string, so that the diff can be calculated correctly next time.
            this.buildOutputStr = newBuildOutputStr;
        }

        public static List<ProjectBuildInfo> CalculateProjectBuildInfo(
            List<ProjectBuildInfo> prevBuildInfo,
            string newBuildOutput,
            System.DateTime currentTime)
        {
            // All projects should already have a start time. If not it is impossible to calculate the end time.
            bool startTimesValid = prevBuildInfo.All(buildInfo => buildInfo.BuildStartTime.HasValue);
            if (!startTimesValid) throw new System.ArgumentException("Projects with an invalid start time found.");

            List<ProjectBuildInfo> newBuildInfo = new List<ProjectBuildInfo>(prevBuildInfo);

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


        private readonly IEventRouter evtRouter;
        private string buildOutputStr = "";
        private List<ProjectBuildInfo> projectBuildInfo = new List<ProjectBuildInfo>();
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
        };

        public List<ProjectBuildInfo> ExtractBuildInfo()
        {
            count = (count + 1) % dummyProjectList.Count;
            List<ProjectBuildInfo> buildInfo = new List<ProjectBuildInfo>();
            for (int i = 0; i < 15; ++i)
            {
                buildInfo.Add(dummyProjectList[(count + i) % dummyProjectList.Count]);
            }

            return buildInfo;
        }

        private int count = 0;
    }
}