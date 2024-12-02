using System;
using System.Windows.Forms;
using System.Drawing;

namespace GeoAnalyzer.WinForm.Forms
{
    partial class SpatialAutocorrelationResultDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                // 清理其他非托管资源
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            try
            {
                this.components = new System.ComponentModel.Container();
                
                // 初始化所有控件
                this.lblGlobalMoran = new System.Windows.Forms.Label();
                this.lblZScore = new System.Windows.Forms.Label();
                this.lblPValue = new System.Windows.Forms.Label();
                this.txtInterpretation = new System.Windows.Forms.TextBox();
                this.panelChart = new System.Windows.Forms.Panel();
                this.txtCsvPath = new System.Windows.Forms.TextBox();
                this.btnOpenCsv = new System.Windows.Forms.Button();

                // 设置控件属性
                this.lblGlobalMoran.AutoSize = true;
                this.lblGlobalMoran.Location = new Point(12, 15);
                this.lblGlobalMoran.Size = new Size(200, 20);

                this.lblZScore.AutoSize = true;
                this.lblZScore.Location = new Point(12, 45);
                this.lblZScore.Size = new Size(200, 20);

                this.lblPValue.AutoSize = true;
                this.lblPValue.Location = new Point(12, 75);
                this.lblPValue.Size = new Size(200, 20);

                this.txtInterpretation.Location = new Point(12, 105);
                this.txtInterpretation.Multiline = true;
                this.txtInterpretation.ReadOnly = true;
                this.txtInterpretation.ScrollBars = ScrollBars.Vertical;
                this.txtInterpretation.Size = new Size(560, 150);

                this.panelChart.Location = new Point(12, 265);
                this.panelChart.Size = new Size(560, 300);

                this.txtCsvPath.Location = new Point(12, 575);
                this.txtCsvPath.ReadOnly = true;
                this.txtCsvPath.Size = new Size(460, 25);

                this.btnOpenCsv.Location = new Point(482, 574);
                this.btnOpenCsv.Size = new Size(90, 27);
                this.btnOpenCsv.Text = "打开CSV";
                this.btnOpenCsv.Click += new EventHandler(this.btnOpenCsv_Click);

                // 设置窗体属性
                this.ClientSize = new Size(584, 621);
                this.Controls.AddRange(new Control[] {
                    this.lblGlobalMoran,
                    this.lblZScore,
                    this.lblPValue,
                    this.txtInterpretation,
                    this.panelChart,
                    this.txtCsvPath,
                    this.btnOpenCsv
                });

                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.StartPosition = FormStartPosition.CenterParent;
                this.Text = "空间自相关分析结果";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化组件时出错: {ex.Message}\n\n{ex.StackTrace}", 
                    "初始化错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                throw;
            }
        }

        #region Windows Form Designer generated code
        private System.Windows.Forms.Label lblGlobalMoran;
        private System.Windows.Forms.Label lblZScore;
        private System.Windows.Forms.Label lblPValue;
        private System.Windows.Forms.TextBox txtInterpretation;
        private System.Windows.Forms.Panel panelChart;
        private System.Windows.Forms.TextBox txtCsvPath;
        private System.Windows.Forms.Button btnOpenCsv;
        #endregion
    }
} 