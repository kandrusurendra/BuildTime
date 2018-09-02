using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;

using System.Windows.Forms.DataVisualization.Charting;
using winformchart = System.Windows.Forms.DataVisualization.Charting;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    /// <summary>
    /// Interaction logic for BuildTimerCtrl.xaml
    /// </summary>
    public partial class BuildTimerCtrl : UserControl
    {
        public BuildTimerCtrl(BuildTimerWindowPane windowPane, IBuildInfoExtractionStrategy infoExtractor)
        {
            m_windowPane = windowPane;
            m_buildInfoExtractor = infoExtractor;
            InitializeComponent();
        }

        private WindowStatus currentState = null;
        /// <summary>
        /// This is the object that will keep track of the state of the IVsWindowFrame
        /// that is hosting this control. The pane should set this property once
        /// the frame is created to enable us to stay up to date.
        /// </summary>
        public WindowStatus CurrentState
        {
            get { return currentState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                currentState = value;
                // Subscribe to the change notification so we can update our UI
                currentState.StatusChange += new EventHandler<EventArgs>(RefreshValues);
                // Update the display now
                RefreshValues(this, null);
            }
        }

        /// <summary>
        /// This method is the call back for state changes events
        /// </summary>
        /// <param name="sender">Event senders</param>
        /// <param name="arguments">Event arguments</param>
        private void RefreshValues(object sender, EventArgs arguments)
        {
            //xText.Text = currentState.X.ToString(CultureInfo.CurrentCulture);
            //yText.Text = currentState.Y.ToString(CultureInfo.CurrentCulture);
            //widthText.Text = currentState.Width.ToString(CultureInfo.CurrentCulture);
            //heightText.Text = currentState.Height.ToString(CultureInfo.CurrentCulture);
            //dockedCheckBox.IsChecked = currentState.IsDockable;
            InvalidateVisual();
        }

        private void OnUpdateBuildTimesBtnClick(object sender, EventArgs args)
        {
            var buildInfo = m_buildInfoExtractor.ExtractBuildInfo();

            // Update build-info grid.
            BuildInfoGrid.ItemsSource = buildInfo;

            // Update build graph.
            BuildGraphChart.Series.Clear();
            BuildGraphChart.Series.Add(new winformchart.Series());
            BuildGraphChart.Series[0].ChartType = winformchart.SeriesChartType.RangeBar;
            BuildGraphChart.Series[0].XValueType = winformchart.ChartValueType.Auto;
            BuildGraphChart.Series[0].YValueType = winformchart.ChartValueType.Auto;

            List<ProjectBuildInfo> buildInfoSorted = buildInfo.ToList();
            buildInfoSorted.Sort((i1, i2) => {
                if (!i1.BuildStartTime.HasValue) return 1;
                else if (!i2.BuildStartTime.HasValue) return -1;
                else return (i1.BuildStartTime.Value.CompareTo(i2.BuildStartTime.Value));
            });

            foreach (var projInfo in buildInfoSorted)
            {
                DateTime origin = buildInfoSorted[0].BuildStartTime.HasValue ? buildInfoSorted[0].BuildStartTime.Value 
                                                                             : new DateTime(2000, 1, 1);
                double startTimeSecs = 0, endTimeSecs = 0;
                if (projInfo.BuildDuration.HasValue && projInfo.BuildStartTime.HasValue)
                {
                    startTimeSecs = (projInfo.BuildStartTime.Value - origin).TotalSeconds;
                    endTimeSecs = startTimeSecs + projInfo.BuildDuration.Value.TotalSeconds;
                }

                int projCount = BuildGraphChart.Series[0].Points.Count;
                int idx = BuildGraphChart.Series[0].Points.AddXY(projCount+1, startTimeSecs, endTimeSecs);
                BuildGraphChart.Series[0].Points[idx].AxisLabel = projInfo.ProjectName;
            }
        }



        //
        // Variables
        //
        private BuildTimerWindowPane m_windowPane;
        private IBuildInfoExtractionStrategy m_buildInfoExtractor;
        private List<ProjectBuildInfo> m_projectInfo;
    }

    public class OutputWindowBuildInfoExtractor : IBuildInfoExtractionStrategy
    {
        public List<ProjectBuildInfo> ExtractBuildInfo()
        {
            EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(DTE));
            Debug.Assert(dte != null);
            OutputWindowPanes panes = dte.ToolWindows.OutputWindow.OutputWindowPanes;

            OutputWindowPane pane = panes.Cast<OutputWindowPane>().First(wnd => wnd.Name == "Build");
            if (pane != null)
            {
                pane.Activate();
                pane.TextDocument.Selection.SelectAll();
                var buildOutputStr = pane.TextDocument.Selection.Text;

                return BuildInfoUtils.ExtractBuildInfo(buildOutputStr);
            }
            else
            {
                MessageBox.Show("Build output window not found");
                return null;
            }
        }
    }

    public class FakeBuildInfoExtractor : IBuildInfoExtractionStrategy
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
