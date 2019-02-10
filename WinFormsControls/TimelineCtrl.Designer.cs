namespace WinFormsControls
{
    partial class TimelineCtrl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimelineCtrl));
            this.timelineCtrlLayout = new System.Windows.Forms.TableLayoutPanel();
            this.chartAreaPanel = new System.Windows.Forms.Panel();
            this.timelineChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.zoomAreaLayout = new System.Windows.Forms.TableLayoutPanel();
            this.zoomTrackbar = new System.Windows.Forms.TrackBar();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.timelineCtrlLayout.SuspendLayout();
            this.chartAreaPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timelineChart)).BeginInit();
            this.zoomAreaLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zoomTrackbar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // timelineCtrlLayout
            // 
            this.timelineCtrlLayout.AutoSize = true;
            this.timelineCtrlLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.timelineCtrlLayout.ColumnCount = 1;
            this.timelineCtrlLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.timelineCtrlLayout.Controls.Add(this.zoomAreaLayout, 0, 1);
            this.timelineCtrlLayout.Controls.Add(this.chartAreaPanel, 0, 0);
            this.timelineCtrlLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timelineCtrlLayout.Location = new System.Drawing.Point(0, 0);
            this.timelineCtrlLayout.Name = "timelineCtrlLayout";
            this.timelineCtrlLayout.RowCount = 2;
            this.timelineCtrlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.timelineCtrlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.timelineCtrlLayout.Size = new System.Drawing.Size(467, 480);
            this.timelineCtrlLayout.TabIndex = 0;
            // 
            // chartAreaPanel
            // 
            this.chartAreaPanel.AutoScroll = true;
            this.chartAreaPanel.BackColor = System.Drawing.Color.Transparent;
            this.chartAreaPanel.Controls.Add(this.timelineChart);
            this.chartAreaPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartAreaPanel.Location = new System.Drawing.Point(0, 0);
            this.chartAreaPanel.Margin = new System.Windows.Forms.Padding(0);
            this.chartAreaPanel.Name = "chartAreaPanel";
            this.chartAreaPanel.Size = new System.Drawing.Size(467, 439);
            this.chartAreaPanel.TabIndex = 2;
            this.chartAreaPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.chartAreaPanel_Paint);
            // 
            // timelineChart
            // 
            this.timelineChart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timelineChart.BackColor = System.Drawing.Color.Transparent;
            chartArea1.Name = "ChartArea1";
            this.timelineChart.ChartAreas.Add(chartArea1);
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            this.timelineChart.Legends.Add(legend1);
            this.timelineChart.Location = new System.Drawing.Point(0, 0);
            this.timelineChart.Margin = new System.Windows.Forms.Padding(0);
            this.timelineChart.Name = "timelineChart";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.timelineChart.Series.Add(series1);
            this.timelineChart.Size = new System.Drawing.Size(461, 439);
            this.timelineChart.TabIndex = 1;
            this.timelineChart.Text = "timelineChart";
            // 
            // zoomAreaLayout
            // 
            this.zoomAreaLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.zoomAreaLayout.ColumnCount = 3;
            this.zoomAreaLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.zoomAreaLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.zoomAreaLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.zoomAreaLayout.Controls.Add(this.zoomTrackbar, 1, 0);
            this.zoomAreaLayout.Controls.Add(this.pictureBox1, 0, 0);
            this.zoomAreaLayout.Controls.Add(this.pictureBox2, 2, 0);
            this.zoomAreaLayout.Location = new System.Drawing.Point(0, 439);
            this.zoomAreaLayout.Margin = new System.Windows.Forms.Padding(0);
            this.zoomAreaLayout.Name = "zoomAreaLayout";
            this.zoomAreaLayout.RowCount = 1;
            this.zoomAreaLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.zoomAreaLayout.Size = new System.Drawing.Size(467, 41);
            this.zoomAreaLayout.TabIndex = 3;
            this.zoomTrackbar.ValueChanged += new System.EventHandler(this.zoomTrackbar_ValueChanged);
            // 
            // zoomTrackbar
            // 
            this.zoomTrackbar.BackColor = System.Drawing.Color.White;
            this.zoomTrackbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoomTrackbar.Location = new System.Drawing.Point(50, 5);
            this.zoomTrackbar.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.zoomTrackbar.Maximum = 20;
            this.zoomTrackbar.MaximumSize = new System.Drawing.Size(400, 50);
            this.zoomTrackbar.MinimumSize = new System.Drawing.Size(200, 40);
            this.zoomTrackbar.Name = "zoomTrackbar";
            this.zoomTrackbar.Size = new System.Drawing.Size(367, 40);
            this.zoomTrackbar.TabIndex = 0;
            this.zoomTrackbar.Value = 10;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(44, 35);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(420, 3);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(44, 35);
            this.pictureBox2.TabIndex = 2;
            this.pictureBox2.TabStop = false;
            // 
            // TimelineCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.timelineCtrlLayout);
            this.Name = "TimelineCtrl";
            this.Size = new System.Drawing.Size(467, 480);
            this.timelineCtrlLayout.ResumeLayout(false);
            this.chartAreaPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.timelineChart)).EndInit();
            this.zoomAreaLayout.ResumeLayout(false);
            this.zoomAreaLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zoomTrackbar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel timelineCtrlLayout;
        private System.Windows.Forms.Panel chartAreaPanel;
        private System.Windows.Forms.DataVisualization.Charting.Chart timelineChart;
        private System.Windows.Forms.TableLayoutPanel zoomAreaLayout;
        private System.Windows.Forms.TrackBar zoomTrackbar;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}
