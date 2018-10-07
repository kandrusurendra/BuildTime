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
        public string toolTip;
    }

    public partial class ChartCtrlHost : UserControl
    {
        public ChartCtrlHost()
        {
            InitializeComponent();
        }

        public Charting.Chart Chart { get { return this.chart1; } }

        public List<ProjectInfo> ChartData
        {
            get { return chartData; }
            set
            {
                chartData = value;
                UpdateChart();
            }
        }

        private void UpdateChart()
        {
            var BuildGraphChart = this.chart1;

            BuildGraphChart.Height = System.Math.Max(this.panel1.Height, chartData.Count*this.GetBarHeight());
            BuildGraphChart.Series.Clear();
            BuildGraphChart.Series.Add(new Charting.Series());
            BuildGraphChart.Series[0].ChartType = Charting.SeriesChartType.RangeBar;
            BuildGraphChart.Series[0].XValueType = Charting.ChartValueType.Auto;
            BuildGraphChart.Series[0].YValueType = Charting.ChartValueType.Auto;

            //var xAxis = BuildGraphChart.ChartAreas[0].AxisX;
            //xAxis.Interval = 1;
            //xAxis.MajorGrid.Enabled = true;
            //xAxis.MajorGrid.Interval = 1;
            //xAxis.MajorGrid.LineDashStyle = Charting.ChartDashStyle.Dot;


            foreach (ProjectInfo info in this.chartData)
            {
                int projCount = BuildGraphChart.Series[0].Points.Count;
                int idx = BuildGraphChart.Series[0].Points.AddXY(projCount + 1, info.startTime, info.endTime);
                BuildGraphChart.Series[0].Points[idx].AxisLabel = info.projectName;
                BuildGraphChart.Series[0].Points[idx].ToolTip = info.toolTip;
            }

        }

        private int GetBarHeight()
        {
            double ratio = (zoomLevelTrackbar.Value - zoomLevelTrackbar.Minimum) / (double)(zoomLevelTrackbar.Maximum - zoomLevelTrackbar.Minimum);
            int min = 5, max = 35;
            return min + (int)((max - min) * ratio);
        }

        private List<ProjectInfo> chartData;

        private void zoomLevelTrackbar_ValueChanged(object sender, EventArgs e)
        {
            UpdateChart();
        }
    }
}
