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
    public partial class ChartCtrlHost : UserControl
    {
        public ChartCtrlHost()
        {
            InitializeComponent();
        }

        public Charting.Chart Chart { get { return this.chart1; } }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
