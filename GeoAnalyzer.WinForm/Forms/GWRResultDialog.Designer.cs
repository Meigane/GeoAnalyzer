using System;
using System.Windows.Forms;

namespace GeoAnalyzer.WinForm.Forms
{
    partial class GWRResultDialog
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox txtGlobalStats;
        private Panel panelChart;
        private Panel panelCoefficients;
        private TextBox txtCsvPath;
        private Button btnOpenCsv;
        private Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtGlobalStats = new TextBox();
            this.panelChart = new Panel();
            this.panelCoefficients = new Panel();
            this.txtCsvPath = new TextBox();
            this.btnOpenCsv = new Button();
            this.btnClose = new Button();

            // 
            // txtGlobalStats
            // 
            this.txtGlobalStats.Location = new System.Drawing.Point(12, 12);
            this.txtGlobalStats.Multiline = true;
            this.txtGlobalStats.ReadOnly = true;
            this.txtGlobalStats.ScrollBars = ScrollBars.Vertical;
            this.txtGlobalStats.Size = new System.Drawing.Size(300, 200);
            this.txtGlobalStats.TabIndex = 0;

            // 
            // panelChart
            // 
            this.panelChart.Location = new System.Drawing.Point(324, 12);
            this.panelChart.Size = new System.Drawing.Size(450, 200);
            this.panelChart.TabIndex = 1;

            // 
            // panelCoefficients
            // 
            this.panelCoefficients.Location = new System.Drawing.Point(12, 224);
            this.panelCoefficients.Size = new System.Drawing.Size(762, 200);
            this.panelCoefficients.TabIndex = 2;

            // 
            // txtCsvPath
            // 
            this.txtCsvPath.Location = new System.Drawing.Point(12, 436);
            this.txtCsvPath.ReadOnly = true;
            this.txtCsvPath.Size = new System.Drawing.Size(600, 23);
            this.txtCsvPath.TabIndex = 3;

            // 
            // btnOpenCsv
            // 
            this.btnOpenCsv.Location = new System.Drawing.Point(618, 435);
            this.btnOpenCsv.Size = new System.Drawing.Size(75, 25);
            this.btnOpenCsv.TabIndex = 4;
            this.btnOpenCsv.Text = "打开CSV";
            this.btnOpenCsv.Click += new EventHandler(this.btnOpenCsv_Click);

            // 
            // btnClose
            // 
            this.btnClose.DialogResult = DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(699, 435);
            this.btnClose.Size = new System.Drawing.Size(75, 25);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "关闭";

            // 
            // GWRResultDialog
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(786, 472);
            this.Controls.AddRange(new Control[] {
                this.txtGlobalStats,
                this.panelChart,
                this.panelCoefficients,
                this.txtCsvPath,
                this.btnOpenCsv,
                this.btnClose
            });
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GWRResultDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "地理加权回归分析结果";
        }
    }
} 