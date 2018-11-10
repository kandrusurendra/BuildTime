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
    public partial class BuildTimerCtrl : UserControl, ILogger
    {
        //
        // Public interface
        //
        #region PUBLIC_INTERFACE
        public BuildTimerCtrl(BuildTimerWindowPane windowPane)
        {
            m_windowPane = windowPane;

            InitializeComponent();

            var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            this.LogMessage(string.Format("Visual Studio Build Timer {0}.{1}.{2}",
                v.Major, 
                v.Minor, 
                v.Build         // Version is in the form Major.Minor.Revision.Build. 
                                // What would be the build number following microsoft
                                // conventions, is for me the revision and vice versa.
            ), LogLevel.UserInfo);

            this.LogMessage(string.Format("Build {0}", v.Revision), LogLevel.DebugInfo);
        }

        public IBuildInfoExtractionStrategy BuildInfoExtractor { get; set; }

        public IEventRouter EvtRouter
        {
            set
            {
                if (m_evtRouter != null)
                {
                    m_evtRouter.OutputPaneUpdated -= this.OnOutputPaneUpdated;
                }

                m_evtRouter = value;

                if (m_evtRouter != null)
                {
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

        public void LogMessage(string message, LogLevel level)
        {
#if DEBUG
            var minLevel = LogLevel.DebugInfo;
#else
            var minLevel = LogLevel.UserInfo;
#endif
            if (level >= minLevel)
                OutputWindow.AppendText(DateTime.Now + " - " + message + "\n");
        }

#endregion


        //
        // Implementation
        //
        #region IMPLEMENTATION
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

        private void OnGridDoubleClick(object sender, EventArgs args)
        {
            var extractor = BuildInfoExtractor;
            if (extractor!=null)
                this.UpdateUI(extractor.GetBuildProgressInfo());
        }

        private void OnOutputPaneUpdated(object sender, OutputWndEventArgs args)
        {
            if (args.WindowPane != null && args.WindowPane.Name == "Build")
            {
                var extractor = BuildInfoExtractor;
                if (extractor != null)
                    this.UpdateUI(extractor.GetBuildProgressInfo());
            }
        }

        private void UpdateUI(List<ProjectBuildInfo> buildInfo)
        {
            var BuildGraphChart = this.WinFormChartCtrl;
            if (buildInfo == null || buildInfo.Count == 0)
            {
                BuildGraphChart.ChartData = new List<WinFormsControls.ProjectInfo>();
                BuildInfoGrid.ItemsSource = new List<ProjectPresentationInfo>();
                return;
            }
            else
            {
                DateTime? minStartTime = buildInfo.Min(projectInfo => projectInfo.BuildStartTime);

                // Projects must have a non-empty BuildStartTime in order to be in the list.
                System.Diagnostics.Debug.Assert(minStartTime.HasValue);

                // Update build-info grid.
                var presentationInfo = buildInfo.Select(projectInfo => new ProjectPresentationInfo(minStartTime.Value, projectInfo));
                BuildInfoGrid.ItemsSource = presentationInfo;

                var infoSorted = presentationInfo.ToList();
                infoSorted.Sort((i1, i2) =>
                {
                    if (!i1.BuildStartTime_Absolute.HasValue) return 1;
                    else if (!i2.BuildStartTime_Absolute.HasValue) return -1;
                    else return (i1.BuildStartTime_Absolute.Value.CompareTo(i2.BuildStartTime_Absolute.Value));
                });

                var ctrlProjInfo = new List<WinFormsControls.ProjectInfo>();
                foreach (ProjectPresentationInfo projInfo in infoSorted)
                {
                    DateTime origin = infoSorted[0].BuildStartTime_Absolute.HasValue ? 
                        infoSorted[0].BuildStartTime_Absolute.Value : new DateTime(2000, 1, 1);
                    double startTimeSecs = 0, endTimeSecs = 0;
                    if (projInfo.BuildDuration.HasValue && projInfo.BuildStartTime_Absolute.HasValue)
                    {
                        startTimeSecs = (projInfo.BuildStartTime_Absolute.Value - origin).TotalSeconds;
                        endTimeSecs = startTimeSecs + projInfo.BuildDuration.Value.TotalSeconds;
                    }

                    ctrlProjInfo.Add(new WinFormsControls.ProjectInfo
                    {
                        startTime = startTimeSecs,
                        endTime = endTimeSecs,
                        projectName = projInfo.ProjectName,
                        toolTip = BuildInfoUtils.CreateToolTipText(projInfo)
                    });
                }
                BuildGraphChart.ChartData = ctrlProjInfo;
            }
        }
        #endregion


        //
        // Debug code
        //
        #region DEBUG_CODE
        private int GetCount()
        {
            return m_count_++;
        }

        private IVsOutputWindowPane GetDebugPane()
        {
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane; // P.S. There's also the GUID_OutWindowDebugPane available.
            IVsOutputWindowPane debugPane = null;
                    outWindow.GetPane(ref generalPaneGuid, out debugPane);

            if (debugPane == null)
            {
                outWindow.CreatePane(generalPaneGuid, "debug", 1, 0);
                outWindow.GetPane(ref generalPaneGuid, out debugPane);
            }
            return debugPane;
        }

        private void wfHost_LayoutUpdated(object sender, EventArgs e)
        {
#if DEBUG
            var debugPane = GetDebugPane();
            if (debugPane != null)
            {
                debugPane.OutputString(string.Format("wfHost-layoutUpdated {0}: width={1}, height={2}\n", GetCount(), this.wfHost.Width, this.wfHost.Height));
                debugPane.OutputString(string.Format("wfHost-layoutUpdated {0}: width={1}, height={2} - actual \n", GetCount(), this.wfHost.ActualWidth, this.wfHost.ActualHeight));
                debugPane.OutputString(string.Format("wfHost-layoutUpdated {0}: width={1}, height={2} - chart  \n", GetCount(), this.WinFormChartCtrl.Width, this.WinFormChartCtrl.Height));
                debugPane.Activate(); // Brings this pane into view
            }
#endif
        }


        private void wfHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //this.WinFormChartCtrl.Width = (int)(e.NewSize.Width);
            //this.WinFormChartCtrl.Height = (int)(e.NewSize.Height);
            //this.WinFormChartCtrl.Size;
#if DEBUG
            var debugPane = GetDebugPane();
            if (debugPane != null)
            {
                //debugPane.OutputString(string.Format("wfHost-sizeChanged {0}: width={1}, height={2}\n",           GetCount(), this.wfHost.Width, this.wfHost.Height));
                debugPane.OutputString(string.Format("wfHost-sizeChanged {0}: width={1}, height={2} - actual \n", GetCount(), this.wfHost.ActualWidth, this.wfHost.ActualHeight));
                //debugPane.OutputString(string.Format("wfHost-sizeChanged {0}: width={1}, height={2} - chart  \n", GetCount(), this.WinFormChartCtrl.Width, this.WinFormChartCtrl.Height));
                debugPane.Activate(); // Brings this pane into view
            }
#endif
        }


        private void DManager_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var dx = e.NewSize.Width - e.PreviousSize.Width;
            var dy = e.NewSize.Height - e.PreviousSize.Height;

            if (!this.timelineAnchorable.IsFloating)
            {
                if (e.PreviousSize.Width > 0.0)
                {
                    double newWidth = this.timelineAnchorablePane.DockWidth.Value + dx;
                    this.timelineAnchorablePane.DockWidth = new GridLength(newWidth, GridUnitType.Pixel);
                }
                

                this.timelineAnchorablePane.DockHeight = new GridLength(e.NewSize.Height - 10, GridUnitType.Pixel);
                this.wfHost.Height = e.NewSize.Height - 60;
            }


#if DEBUG
            var debugPane = GetDebugPane();
            if (debugPane != null)
            {
                debugPane.OutputString(string.Format("dmanager-sizeChanged {0}: width={1}, height={2}\n", 
                    GetCount(), this.DManager.ActualWidth, this.DManager.ActualHeight));
                debugPane.OutputString(string.Format("> main pane dock (width,height) = ({0},{1})\n", this.MainPane.DockWidth, this.MainPane.DockHeight));
                debugPane.OutputString(string.Format("> time pane dock (width,height) = ({0},{1})\n", this.timelineAnchorablePane.DockWidth, this.timelineAnchorablePane.DockHeight));
            }
#endif            
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //// Try to use screen dimensions of wpf control.
            //if (WinFormChartCtrl.Width > TimelineWndGrid.Width || 
            //    WinFormChartCtrl.Height > TimelineWndGrid.Height)
            //{
            //    wfHost.Visibility = System.Windows.Visibility.Hidden;
            //    WinFormChartCtrl.Visible = false;
            //}
            //else
            //{
            //    wfHost.Visibility = System.Windows.Visibility.Visible;
            //    WinFormChartCtrl.Visible = true;
            //}
        }

        private void timelineAnchorablePane_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var debugPane = GetDebugPane();
            if (debugPane != null)
            {
                debugPane.OutputString(string.Format("timelinePane-propChanged {0}: {1}\n", GetCount(), e.PropertyName));
                if (e.PropertyName=="DockWidth" || e.PropertyName=="DockHeight")
                {
                    //this.timelineAnchorablePane.DockWidth
                    debugPane.OutputString(string.Format("> main pane dock (width,height) = ({0},{1})\n", this.MainPane.DockWidth, this.MainPane.DockHeight));
                    debugPane.OutputString(string.Format("> time pane dock (width,height) = ({0},{1})\n", this.timelineAnchorablePane.DockWidth, this.timelineAnchorablePane.DockHeight));
                }
            }
        }
#endregion


        //
        // Variables
        //
        private BuildTimerWindowPane m_windowPane;
        private IEventRouter m_evtRouter;
        private WindowStatus currentState = null;

        private int m_count_ = 0;
    }
}
