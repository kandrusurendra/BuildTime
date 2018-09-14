using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        public BuildTimerCtrl(BuildTimerWindowPane windowPane)
        {
            m_windowPane = windowPane;

            InitializeComponent();

            EvtLoggerTxtBox.AppendText("hello world!!\n");
        }

        public IBuildInfoExtractionStrategy BuildInfoExtractor { get; set; }

        public IEventRouter EvtRouter
        {
            set
            {
                if (m_evtRouter != null)
                {
                    m_evtRouter.BuildStarted -= this.OnBuildStarted;
                    m_evtRouter.BuildCompleted -= this.OnBuildCompleted;
                    m_evtRouter.OutputPaneUpdated -= this.OnOutputPaneUpdated;
                }

                m_evtRouter = value;

                if (m_evtRouter != null)
                {
                    m_evtRouter.BuildStarted += this.OnBuildStarted;
                    m_evtRouter.BuildCompleted += this.OnBuildCompleted;
                    m_evtRouter.OutputPaneUpdated += this.OnOutputPaneUpdated;
                }
            }
        }

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
            var extractor = BuildInfoExtractor;
            if (extractor!=null)
                this.UpdateUI(extractor.ExtractBuildInfo());
        }

        private void OnBuildStarted(object sender, EventArgs args)
        {
            //this.EvtLoggerTxtBox.Text += "<build started - time:" + System.DateTime.Now + "\n"; 
            this.EvtLoggerTxtBox.AppendText("<build started - time:" + System.DateTime.Now + "\n");
        }

        private void OnBuildCompleted(object sender, EventArgs args)
        {
            //this.EvtLoggerTxtBox.Text += ">build completed - time:" + System.DateTime.Now + "\n";
            this.EvtLoggerTxtBox.AppendText("<build completed - time:" + System.DateTime.Now + "\n");
        }

        private void OnOutputPaneUpdated(object sender, OutputWndEventArgs args)
        {
            if (args.WindowPane.Name == "Build")
            {
                this.EvtLoggerTxtBox.AppendText("<Output wnd '" + args.WindowPane.Name + "' updated at" + System.DateTime.Now + "\n");
                var extractor = BuildInfoExtractor;
                if (extractor != null)
                    this.UpdateUI(extractor.ExtractBuildInfo());
            }
        }

        private void UpdateUI(List<ProjectBuildInfo> buildInfo)
        {
            if (buildInfo == null)
                return;

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
                int idx = BuildGraphChart.Series[0].Points.AddXY(projCount + 1, startTimeSecs, endTimeSecs);
                BuildGraphChart.Series[0].Points[idx].AxisLabel = projInfo.ProjectName;
            }
        }

        //
        // Variables
        //
        private BuildTimerWindowPane m_windowPane;
        private IEventRouter m_evtRouter;
        private WindowStatus currentState = null;
    }
}
