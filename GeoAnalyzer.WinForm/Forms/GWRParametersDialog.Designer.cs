using System;
using System.Windows.Forms;

namespace GeoAnalyzer.WinForm.Forms
{
    partial class GWRParametersDialog
    {
        private System.ComponentModel.IContainer components = null;

        private NumericUpDown numBandwidth;
        private ComboBox cboKernelType;
        private Label lblBandwidth;
        private Label lblKernelType;
        private Label lblOutputPath;
        private TextBox txtOutputPath;
        private Button btnBrowse;
        private Button btnOK;
        private Button btnCancel;

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
            this.numBandwidth = new NumericUpDown();
            this.cboKernelType = new ComboBox();
            this.lblBandwidth = new Label();
            this.lblKernelType = new Label();
            this.btnOK = new Button();
            this.btnCancel = new Button();

            ((System.ComponentModel.ISupportInitialize)(this.numBandwidth)).BeginInit();
            this.SuspendLayout();

            // 
            // lblBandwidth
            // 
            this.lblBandwidth.AutoSize = true;
            this.lblBandwidth.Location = new System.Drawing.Point(20, 20);
            this.lblBandwidth.Name = "lblBandwidth";
            this.lblBandwidth.Size = new System.Drawing.Size(65, 15);
            this.lblBandwidth.Text = "带宽:";

            // 
            // numBandwidth
            // 
            this.numBandwidth.DecimalPlaces = 2;
            this.numBandwidth.Increment = new decimal(new int[] { 1, 0, 0, 65536 }); // 0.1
            this.numBandwidth.Location = new System.Drawing.Point(100, 20);
            this.numBandwidth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numBandwidth.Minimum = new decimal(new int[] { 1, 0, 0, 131072 }); // 0.01
            this.numBandwidth.Name = "numBandwidth";
            this.numBandwidth.Size = new System.Drawing.Size(120, 23);
            this.numBandwidth.Value = new decimal(new int[] { 1, 0, 0, 0 });

            // 
            // lblKernelType
            // 
            this.lblKernelType.AutoSize = true;
            this.lblKernelType.Location = new System.Drawing.Point(20, 60);
            this.lblKernelType.Name = "lblKernelType";
            this.lblKernelType.Size = new System.Drawing.Size(65, 15);
            this.lblKernelType.Text = "核函数:";

            // 
            // cboKernelType
            // 
            this.cboKernelType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboKernelType.FormattingEnabled = true;
            this.cboKernelType.Location = new System.Drawing.Point(100, 60);
            this.cboKernelType.Name = "cboKernelType";
            this.cboKernelType.Size = new System.Drawing.Size(200, 23);

            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(320, 140);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 25);
            this.btnOK.Text = "确定";

            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(410, 140);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.Text = "取消";

            // 
            // lblOutputPath
            // 
            this.lblOutputPath = new Label();
            this.lblOutputPath.AutoSize = true;
            this.lblOutputPath.Location = new System.Drawing.Point(20, 100);
            this.lblOutputPath.Text = "保存位置:";

            // 
            // txtOutputPath
            // 
            this.txtOutputPath = new TextBox();
            this.txtOutputPath.Location = new System.Drawing.Point(100, 100);
            this.txtOutputPath.Size = new System.Drawing.Size(300, 23);
            this.txtOutputPath.ReadOnly = true;

            // 
            // btnBrowse
            // 
            this.btnBrowse = new Button();
            this.btnBrowse.Location = new System.Drawing.Point(410, 99);
            this.btnBrowse.Size = new System.Drawing.Size(75, 25);
            this.btnBrowse.Text = "浏览...";

            // 
            // GWRParametersDialog
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 180);
            this.Controls.AddRange(new Control[] {
                this.lblBandwidth,
                this.numBandwidth,
                this.lblKernelType,
                this.cboKernelType,
                this.btnOK,
                this.btnCancel,
                this.lblOutputPath,
                this.txtOutputPath,
                this.btnBrowse
            });
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GWRParametersDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "GWR参数设置";

            ((System.ComponentModel.ISupportInitialize)(this.numBandwidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
} 