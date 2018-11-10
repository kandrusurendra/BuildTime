using System;
using System.Diagnostics;
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
    public partial class ChartCtrlHost : UserControl
    {
        public ChartCtrlHost()
        {
            m_chartData = new List<ProjectInfo>();
            InitializeComponent();
            UpdateChart();
        }

        public Charting.Chart Chart { get { return this.chart1; } }

        public List<ProjectInfo> ChartData
        {
            get { return m_chartData; }
            set
            {
                m_chartData = value;
                UpdateChart();
            }
        }

        private void UpdateChart()
        {
            var BuildGraphChart = this.chart1;
            int minChartHeight = 40;
            BuildGraphChart.Height = minChartHeight + m_chartData.Count*this.GetBarHeight();
            BuildGraphChart.Series.Clear();
            BuildGraphChart.Series.Add(new Charting.Series());
            BuildGraphChart.Series[0].ChartType = Charting.SeriesChartType.RangeBar;
            BuildGraphChart.Series[0].XValueType = Charting.ChartValueType.Auto;
            BuildGraphChart.Series[0].YValueType = Charting.ChartValueType.Auto;
            BuildGraphChart.Legends[0].Enabled = false;

            foreach (ProjectInfo info in this.m_chartData)
            {
                int projCount = BuildGraphChart.Series[0].Points.Count;
                int idx = BuildGraphChart.Series[0].Points.AddXY(projCount + 1, info.startTime, info.endTime);
                BuildGraphChart.Series[0].Points[idx].AxisLabel = info.projectName;
                BuildGraphChart.Series[0].Points[idx].ToolTip = info.toolTip;
            }
        }

        private int GetBarHeight()
        {
            int[] heights = { 8, 10, 12, 15, 18, 22, 27, 35, 45 };

            int idx = this.zoomLevelTrackbar.Value;
            Debug.Assert(idx >= 0 && idx <= 8);

            return heights[idx];
        }

        private List<ProjectInfo> m_chartData;

        private void zoomLevelTrackbar_ValueChanged(object sender, EventArgs e)
        {
            UpdateChart();
        }

        private void zoomLevelTrackbar_Scroll(object sender, EventArgs e)
        {

        }
    }
}
