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
            this.MyDataSource = new ObservableCollection<ProjectBuildInfo>();
            this.ViewSource = new CollectionViewSource();
            this.ViewSource.Source = this.MyDataSource;
        }

        public ObservableCollection<ProjectBuildInfo> MyDataSource { get; set; }

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
            this.OutputString(BuildTimerPackage.GetVersionString());
        }

        public void Initialize(IBuildInfoExtractionStrategy infoExtractor, IEventRouter evtRouter, 
                                WindowStatus status, SettingsManager settingsManager)
        {
            this.EvtRouter = evtRouter;
            this.BuildInfoExtractor = infoExtractor;
            this.WindowStatus = status;
            this.SettingsManager = settingsManager;

            this.WinFormChartCtrl.ZoomLevel = this.SettingsManager.GetSettings().ZoomLevel;
            this.WinFormChartCtrl.ZoomLevelChanged += this.OnZoomLevelChanged;
            this.UpdateUI();
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

        private IEventRouter EvtRouter { get; set; }
        
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

        private void RefreshValues(object sender, EventArgs args)
        {
            InvalidateVisual();
        }

        private void OnBuildInfoUpdated(object sender, EventArgs args)
        {
            this.Dispatcher.BeginInvoke(new Action(this.UpdateUI));
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
                    var package = m_windowPane.Package as BuildTimerPackage;
                    package.LogMessage("Failed to export build information: " + e.Message, LogLevel.Error);
                }
            }
        }

        private void OnZoomLevelChanged(object sender, EventArgs args)
        {
            var settings = this.SettingsManager.GetSettings();
            settings.ZoomLevel = this.WinFormChartCtrl.ZoomLevel;
            this.SettingsManager.SetSettings(settings);
        }

        private void UpdateUI()
        {
            if (BuildInfoExtractor != null)
            {
                var buildInfo = BuildInfoExtractor.GetBuildProgressInfo();
                this.UpdateUI(buildInfo);
            }
        }

        private void UpdateUI(List<ProjectBuildInfo> buildInfo)
        {
            //
            // Hide placeholder or chart depending on the state.
            //
            if (buildInfo == null || buildInfo.Count == 0)
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
                m_viewModel.MyDataSource.Clear();
                foreach (var info in buildInfo)
                    m_viewModel.MyDataSource.Add(info);
                m_viewModel.ViewSource.View.Refresh();

                // update chart.
                var sortedInfo = buildInfo.ToList();
                sortedInfo.Sort((i1, i2) =>
                {
                    if (!i1.BuildStartTime.HasValue) return 1;
                    else if (!i2.BuildStartTime.HasValue) return -1;
                    else return (i1.BuildStartTime.Value.CompareTo(i2.BuildStartTime.Value));
                });

                var ctrlProjInfo = new List<WinFormsControls.ProjectInfo>();
                foreach (var projInfo in sortedInfo)
                {
                    DateTime origin = sortedInfo[0].BuildStartTime.HasValue ?
                        sortedInfo[0].BuildStartTime.Value : new DateTime(2000, 1, 1);
                    double startTimeSecs = 0, endTimeSecs = 0;
                    if (projInfo.BuildDuration.HasValue && projInfo.BuildStartTime.HasValue)
                    {
                        startTimeSecs = (projInfo.BuildStartTime.Value - origin).TotalSeconds;
                        endTimeSecs = startTimeSecs + projInfo.BuildDuration.Value.TotalSeconds;
                    }

                    ctrlProjInfo.Add(new WinFormsControls.ProjectInfo{
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
        private IBuildInfoExtractionStrategy m_buildInfoExtractor;
        private WindowStatus currentState = null;
        private int m_count_ = 0;
    }
}
