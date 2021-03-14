using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;

using System.Windows.Forms.DataVisualization.Charting;
using winformchart = System.Windows.Forms.DataVisualization.Charting;

namespace VSBuildTimer
{
    /// <summary>
    /// ViewModel: class to facilitate data-binding with WPF DataGrid.
    /// </summary>
    public class ViewModel
    {
        public ViewModel()
        {
            this.MyDataSource = new ObservableCollection<ProjectPresentationInfo>();
            this.ViewSource = new CollectionViewSource();
            this.ViewSource.Source = this.MyDataSource;
        }

        public ObservableCollection<ProjectPresentationInfo> MyDataSource { get; set; }

        public CollectionViewSource ViewSource { get; set; }
    }

    /// <summary>
    /// Interaction logic for BuildTimerCtrl.xaml
    /// </summary>
    public partial class BuildTimerCtrl : UserControl
    {
        //
        // Public interface
        //
        public BuildTimerCtrl(BuildTimerWindowPane windowPane)
        {
            m_windowPane = windowPane;
            m_viewModel = new ViewModel();
            this.DataContext = m_viewModel;

            InitializeComponent();
            this.UpdateUI(new List<ProjectBuildInfo>());
            this.OutputString(PackageUtils.PackageVersionString());
        }

        public void Initialize(IBuildInfoExtractionStrategy infoExtractor, IEventRouter evtRouter, 
                                WindowStatus status, SettingsManager settingsManager)
        {
            this.EvtRouter = evtRouter;
            this.BuildInfoExtractor = infoExtractor;
            this.WindowStatus = status;
            this.SettingsManager = settingsManager;

            this.WinFormChartCtrl.ZoomLevel = this.SettingsManager.GetSettings().ZoomLevel;
            this.UpdateData();
        }
        private IBuildInfoExtractionStrategy BuildInfoExtractor
        {
            set
            {
                m_buildInfoExtractor = value;
                if (m_buildInfoExtractor != null)
                {
                    m_buildInfoExtractor.BuildInfoUpdated += this.OnBuildInfoUpdated;
                }
            }

            get
            {
                return m_buildInfoExtractor;
            }
        }

        private IEventRouter EvtRouter
        {
            set
            {
                m_evtRouter = value;
                if (m_evtRouter != null)
                {
                    m_evtRouter.OnShutdown += this.OnShutdown;
                }
            }
        }
        
        private SettingsManager SettingsManager { get; set; }

        private WindowStatus WindowStatus
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

        public void OutputString(string message)
        {
            if (OutputWindow != null)
                OutputWindow.AppendText(message);
        }

        public void UpdateData()
        {
            if (BuildInfoExtractor != null)
            {
                var buildInfo = BuildInfoExtractor.GetBuildProgressInfo();
                this.UpdateUI(buildInfo);
            }
        }

        //
        // Implementation
        //
        private void RefreshValues(object sender, EventArgs args)
        {
            InvalidateVisual();
        }

        private void OnBuildInfoUpdated(object sender, EventArgs args)
        {
            this.Dispatcher.BeginInvoke(new Action(this.UpdateData));
        }

        private void OnClearOutputWindow(object sender, RoutedEventArgs args)
        {
            this.OutputWindow.Clear();
        }

        private void OnExportGrid(object sender, RoutedEventArgs args)
        {
            var fileDlg = new System.Windows.Forms.SaveFileDialog();
            fileDlg.Filter = "csv files (*.csv)|*.csv";
            if (fileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    using (System.IO.StreamWriter writer = System.IO.File.CreateText(fileDlg.FileName))
                    {
                        var buildInfo = this.BuildInfoExtractor.GetBuildProgressInfo();
                        BuildInfoUtils.WriteBuildInfoToCSV(buildInfo, writer);
                    }
                }
                catch (Exception e)
                {
                    var package = m_windowPane.Package as VSBuildTimerPackage;
                    package.LogMessage("Failed to export build information: " + e.Message, LogLevel.Error);
                }
            }
        }

        private void OnShutdown(object sender, System.EventArgs args)
        {
            this.SettingsManager.GetSettings().ZoomLevel = this.WinFormChartCtrl.ZoomLevel;
        }
        private void UpdateUI(List<ProjectBuildInfo> buildInfo)
        {
            //
            // Hide placeholder or chart depending on the state.
            //
            if (buildInfo.Count == 0)
            {
                this.placeHolder.Visibility = Visibility.Visible;
                this.wfHost.Visibility = Visibility.Hidden;
            }
            else
            {
                this.placeHolder.Visibility = Visibility.Hidden;
                this.wfHost.Visibility = Visibility.Visible;
            }

            //
            // Update grid and chart with new data.
            //
            var BuildGraphChart = this.WinFormChartCtrl;
            if (buildInfo == null || buildInfo.Count == 0)
            {
                // update model; grid will update accordingly.
                m_viewModel.MyDataSource.Clear();
                m_viewModel.ViewSource.View.Refresh();
                
                // update chart
                BuildGraphChart.ChartData = new List<WinFormsControls.ProjectInfo>();
                return;
            }
            else
            {
                // update model; grid will update accordingly.
                var presentationInfo = BuildInfoUtils.ExtractPresentationInfo(buildInfo);
                m_viewModel.MyDataSource.Clear();
                foreach (var info in presentationInfo)
                    m_viewModel.MyDataSource.Add(info);
                m_viewModel.ViewSource.View.Refresh();

                // update chart.
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
                        configuration = projInfo.Configuration,
                        buildSucceeded = projInfo.BuildSucceeded,
                        toolTip = BuildInfoUtils.CreateToolTipText(projInfo)
                    });
                }
                BuildGraphChart.ChartData = ctrlProjInfo;
            }
        }

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

        private void DebugOutput(string s)
        {
#if DEBUG
            var debugPane = GetDebugPane();
            if (debugPane != null)
            {
                debugPane.Activate();
                debugPane.OutputString(s);
            }
#endif
        }

        private void wfHost_LayoutUpdated(object sender, EventArgs e)
        {
            DebugOutput(string.Format("wfHost-layoutUpdated {0}: width={1}, height={2}\n", GetCount(), this.wfHost.Width, this.wfHost.Height));
            DebugOutput(string.Format("wfHost-layoutUpdated {0}: width={1}, height={2} - actual \n", GetCount(), this.wfHost.ActualWidth, this.wfHost.ActualHeight));
            DebugOutput(string.Format("wfHost-layoutUpdated {0}: width={1}, height={2} - chart  \n", GetCount(), this.WinFormChartCtrl.Width, this.WinFormChartCtrl.Height));
        }

        private void wfHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DebugOutput(string.Format("wfHost-sizeChanged {0}: width={1}, height={2} - actual \n", GetCount(), this.wfHost.ActualWidth, this.wfHost.ActualHeight));
        }

        private void DManager_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var dx = e.NewSize.Width - e.PreviousSize.Width;
            var dy = e.NewSize.Height - e.PreviousSize.Height;

            if (!this.timelineAnchorable.IsFloating)
            {
                if (e.PreviousSize.Width > 0.0)
                {
                    double newWidth = Math.Max(0.0, this.timelineAnchorablePane.DockWidth.Value + dx);
                    this.timelineAnchorablePane.DockWidth = new GridLength(newWidth, GridUnitType.Pixel);
                }
                
                this.timelineAnchorablePane.DockHeight = new GridLength(e.NewSize.Height-10, GridUnitType.Pixel);
            }

            DebugOutput(string.Format("dmanager-sizeChanged {0}: width={1}, height={2}\n", 
                GetCount(), this.DManager.ActualWidth, this.DManager.ActualHeight));
            DebugOutput(string.Format("> main pane dock (width,height) = ({0},{1})\n", this.MainPane.DockWidth, this.MainPane.DockHeight));
            DebugOutput(string.Format("> time pane dock (width,height) = ({0},{1})\n", this.timelineAnchorablePane.DockWidth, this.timelineAnchorablePane.DockHeight));
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void OnGridDoubleClick(object sender, EventArgs args)
        {
        }

        private void timelineAnchorablePane_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DebugOutput(string.Format("timelinePane-propChanged {0}: {1}\n", GetCount(), e.PropertyName));
            if (e.PropertyName=="DockWidth" || e.PropertyName=="DockHeight")
            {
                //this.timelineAnchorablePane.DockWidth
                DebugOutput(string.Format("> main pane dock (width,height) = ({0},{1})\n", this.MainPane.DockWidth, this.MainPane.DockHeight));
                DebugOutput(string.Format("> time pane dock (width,height) = ({0},{1})\n", this.timelineAnchorablePane.DockWidth, this.timelineAnchorablePane.DockHeight));
            }
        }
        #endregion


        private BuildTimerWindowPane m_windowPane;
        private readonly ViewModel m_viewModel;
        private IEventRouter m_evtRouter;
        private IBuildInfoExtractionStrategy m_buildInfoExtractor;
        private WindowStatus currentState = null;
        private int m_count_ = 0;
    }
}
