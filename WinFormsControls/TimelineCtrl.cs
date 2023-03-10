using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Charting = System.Windows.Forms.DataVisualization.Charting;

namespace WinFormsControls
{
    public struct ProjectInfo
    {
        public double startTime;
        public double endTime;
        public string projectName;
        public string configuration;
        public bool? buildSucceeded;
        public string toolTip;
    }


    public partial class TimelineCtrl : UserControl
    {
        public TimelineCtrl()
        {
            m_chartData = new List<ProjectInfo>();
            InitializeComponent();
            UpdateChart();
        }

        public Charting.Chart Chart { get { return this.timelineChart; } }

        public List<ProjectInfo> ChartData
        {
            get { return m_chartData; }
            set
            {
                m_chartData = value;
                UpdateChart();
            }
        }

        public double ZoomLevel
        {
            get
            {
                return (this.zoomTrackbar.Value - this.zoomTrackbar.Minimum) /
                       (double)(this.zoomTrackbar.Maximum - this.zoomTrackbar.Minimum);
            }

            set
            {
                value = System.Math.Min(1.0, System.Math.Max(0.0, value));
                this.zoomTrackbar.Value = System.Convert.ToInt32(
                    this.zoomTrackbar.Minimum + value * (this.zoomTrackbar.Maximum - this.zoomTrackbar.Minimum));
            }
        }

        public event System.EventHandler ZoomLevelChanged = (sender, args) => { };

        private static System.Drawing.Color GetBarColor(bool? buildSuccess)
        {
            if (buildSuccess.HasValue)
            {
                return (buildSuccess.Value == true) ? System.Drawing.Color.LightGreen : System.Drawing.Color.Red;
            }
            else
            {
                return System.Drawing.Color.LightBlue;
            }
        }

        private void UpdateChart()
        {
            int newChartHeight = 40 + m_chartData.Count * this.GetBarHeight();

            var chart = this.timelineChart;
            chart.Height = newChartHeight;
            chart.Series.Clear();
            chart.Series.Add(new Charting.Series());
            chart.Series[0].ChartType = Charting.SeriesChartType.RangeBar;
            chart.Series[0].XValueType = Charting.ChartValueType.Auto;
            chart.Series[0].YValueType = Charting.ChartValueType.Auto;
            chart.Legends[0].Enabled = false;
            chart.ChartAreas[0].AxisY.Title = "time (seconds)";

            // Set IntervalAutoMode to variable. This adjusts number of labels displayed:
            // not to many so that they fit in the available space, not to few either.
            chart.ChartAreas[0].AxisX.IntervalAutoMode = Charting.IntervalAutoMode.VariableCount;

            foreach (ProjectInfo info in this.m_chartData)
            {
                int projCount = chart.Series[0].Points.Count;
                int idx = chart.Series[0].Points.AddXY(projCount + 1, info.startTime, info.endTime);
                chart.Series[0].Points[idx].Color = GetBarColor(info.buildSucceeded);
                chart.Series[0].Points[idx].AxisLabel = info.projectName;
                chart.Series[0].Points[idx].ToolTip = info.toolTip;
            }
        }

        private int GetBarHeight()
        {
            int minHeight = 5;
            int maxHeight = 50;

            double ratio = (this.zoomTrackbar.Value-this.zoomTrackbar.Minimum) /
                            (double)(this.zoomTrackbar.Maximum-this.zoomTrackbar.Minimum);
            return (int)(minHeight + (maxHeight - minHeight) * ratio);
        }

        private List<ProjectInfo> m_chartData;

        private void zoomTrackbar_ValueChanged(object sender, EventArgs e)
        {
            UpdateChart();
            ZoomLevelChanged(sender, e);
        }

        private void chartAreaPanel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
