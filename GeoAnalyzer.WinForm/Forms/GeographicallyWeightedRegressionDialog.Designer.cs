namespace GeoAnalyzer.WinForm.Forms
{
    partial class GeographicallyWeightedRegressionDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 因变量选择
            this.lblDependent = new System.Windows.Forms.Label();
            this.cboDependentVar = new System.Windows.Forms.ComboBox();

            // 自变量选择
            this.lblIndependent = new System.Windows.Forms.Label();
            this.lstIndependentVars = new System.Windows.Forms.CheckedListBox();

            // 带宽设置
            this.lblBandwidth = new System.Windows.Forms.Label();
            this.numBandwidth = new System.Windows.Forms.NumericUpDown();

            // 按钮
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.numBandwidth)).BeginInit();
            this.SuspendLayout();

            // lblDependent
            this.lblDependent.AutoSize = true;
            this.lblDependent.Location = new System.Drawing.Point(20, 20);
            this.lblDependent.Name = "lblDependent";
            this.lblDependent.Size = new System.Drawing.Size(65, 12);
            this.lblDependent.TabIndex = 0;
            this.lblDependent.Text = "因变量:";

            // cboDependentVar
            this.cboDependentVar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDependentVar.Location = new System.Drawing.Point(100, 20);
            this.cboDependentVar.Name = "cboDependentVar";
            this.cboDependentVar.Size = new System.Drawing.Size(260, 21);
            this.cboDependentVar.TabIndex = 1;

            // lblIndependent
            this.lblIndependent.AutoSize = true;
            this.lblIndependent.Location = new System.Drawing.Point(20, 60);
            this.lblIndependent.Name = "lblIndependent";
            this.lblIndependent.Size = new System.Drawing.Size(65, 12);
            this.lblIndependent.TabIndex = 2;
            this.lblIndependent.Text = "自变量:";

            // lstIndependentVars
            this.lstIndependentVars.CheckOnClick = true;
            this.lstIndependentVars.Location = new System.Drawing.Point(20, 80);
            this.lstIndependentVars.Name = "lstIndependentVars";
            this.lstIndependentVars.Size = new System.Drawing.Size(340, 130);
            this.lstIndependentVars.TabIndex = 3;

            // lblBandwidth
            this.lblBandwidth.AutoSize = true;
            this.lblBandwidth.Location = new System.Drawing.Point(20, 230);
            this.lblBandwidth.Name = "lblBandwidth";
            this.lblBandwidth.Size = new System.Drawing.Size(65, 12);
            this.lblBandwidth.TabIndex = 4;
            this.lblBandwidth.Text = "带宽:";

            // numBandwidth
            this.numBandwidth.DecimalPlaces = 2;
            this.numBandwidth.Location = new System.Drawing.Point(100, 230);
            this.numBandwidth.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numBandwidth.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numBandwidth.Name = "numBandwidth";
            this.numBandwidth.Size = new System.Drawing.Size(120, 20);
            this.numBandwidth.TabIndex = 5;
            this.numBandwidth.Value = new decimal(new int[] { 10, 0, 0, 65536 });

            // btnOK
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(200, 280);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "确定";
            this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);

            // btnCancel
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(290, 280);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "取消";

            // GeographicallyWeightedRegressionDialog
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 350);
            this.Controls.Add(this.lblDependent);
            this.Controls.Add(this.cboDependentVar);
            this.Controls.Add(this.lblIndependent);
            this.Controls.Add(this.lstIndependentVars);
            this.Controls.Add(this.lblBandwidth);
            this.Controls.Add(this.numBandwidth);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GeographicallyWeightedRegressionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "地理加权回归分析";
            ((System.ComponentModel.ISupportInitialize)(this.numBandwidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblDependent;
        private System.Windows.Forms.ComboBox cboDependentVar;
        private System.Windows.Forms.Label lblIndependent;
        private System.Windows.Forms.CheckedListBox lstIndependentVars;
        private System.Windows.Forms.Label lblBandwidth;
        private System.Windows.Forms.NumericUpDown numBandwidth;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}