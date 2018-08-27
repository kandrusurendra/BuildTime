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
    public interface IBuildInfoExtractionStrategy
    {
        List<ProjectBuildInfo> ExtractBuildInfo();
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
        public List<ProjectBuildInfo> ExtractBuildInfo()
        {
            count = (count + 1) % 5;

            ProjectBuildInfo[] dummyInfo = new ProjectBuildInfo[7] {
                  new ProjectBuildInfo("proj1", 1, new DateTime(2018,5,5, 1, 1, 1), new TimeSpan(0,0,10))
                , new ProjectBuildInfo("proj2", 2, new DateTime(2018,5,5, 1, 1, 1), new TimeSpan(0,0,20))
                , new ProjectBuildInfo("proj3", 3, new DateTime(2018,5,5, 1, 1, 11), new TimeSpan(0, 0, 30))
                , new ProjectBuildInfo("proj4", 4, new DateTime(2018,5,5, 1, 1, 41), new TimeSpan(0, 0, 10))
                , new ProjectBuildInfo("proj5", 5, new DateTime(2018,5,5, 1, 1, 52), new TimeSpan(0, 0, 6))
                , new ProjectBuildInfo("proj6", 6, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 6))
                , new ProjectBuildInfo("proj7", 7, new DateTime(2018,5,5, 1, 2, 0 ), new TimeSpan(0, 0, 26))
            };

            List<ProjectBuildInfo> buildInfo = new List<ProjectBuildInfo>();
            buildInfo.Add(dummyInfo[count]);
            buildInfo.Add(dummyInfo[count+1]);
            buildInfo.Add(dummyInfo[count+2]);
            return buildInfo;
        }

        private int count = 0;
    }

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
#if false
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outWindow != null)
            {
                MessageBox.Show("output window found");
                Guid ID = Guid.NewGuid();
                outWindow.CreatePane(ID, "MY OUTPUT", 1, 1);
                IVsOutputWindowPane generalPane = null;
                outWindow.GetPane(ref ID, out generalPane);

                generalPane.OutputString("Hello World!");
                generalPane.Activate(); // Brings this pane into view
            }
#endif

#if false
            EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(DTE));
            Debug.Assert(dte != null);
            OutputWindowPanes panes = dte.ToolWindows.OutputWindow.OutputWindowPanes;

            OutputWindowPane pane = panes.Cast<OutputWindowPane>().First(wnd => wnd.Name == "Build");
            if(pane!=null)
            {
                pane.Activate();
                pane.TextDocument.Selection.SelectAll();
                var buildOutputStr = pane.TextDocument.Selection.Text;

                BuildInfoGrid.ItemsSource = BuildInfoUtils.ExtractBuildInfo(buildOutputStr);
            }
            else
            {
                MessageBox.Show("Build output window not found");
            }
#endif

            var buildInfo = m_buildInfoExtractor.ExtractBuildInfo();

            // Update build-info grid.
            BuildInfoGrid.ItemsSource = buildInfo;

            // Update build graph.
            BuildGraphChart.Series.Clear();
            BuildGraphChart.Series.Add(new winformchart.Series());
            BuildGraphChart.Series[0].ChartType = winformchart.SeriesChartType.BoxPlot;
            BuildGraphChart.Series[0].YValuesPerPoint = 4;

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
                BuildGraphChart.Series[0].Points.AddXY(projInfo.ProjectName, new object[] { startTimeSecs, endTimeSecs, startTimeSecs, endTimeSecs });
            }
        }



        //
        // Variables
        //
        private BuildTimerWindowPane m_windowPane;
        private IBuildInfoExtractionStrategy m_buildInfoExtractor;
    }
}
