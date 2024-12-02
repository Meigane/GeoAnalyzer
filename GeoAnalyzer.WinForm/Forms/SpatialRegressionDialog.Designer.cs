namespace GeoAnalyzer.WinForm.Forms
{
    partial class SpatialRegressionDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // 控件声明
        private System.Windows.Forms.ComboBox cboDependentVar;
        private System.Windows.Forms.CheckedListBox lstIndependentVars;
        private System.Windows.Forms.Label lblDependent;
        private System.Windows.Forms.Label lblIndependent;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;

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
            this.cboDependentVar = new System.Windows.Forms.ComboBox();
            this.lstIndependentVars = new System.Windows.Forms.CheckedListBox();
            this.lblDependent = new System.Windows.Forms.Label();
            this.lblIndependent = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // 
            // lblDependent
            // 
            this.lblDependent.AutoSize = true;
            this.lblDependent.Location = new System.Drawing.Point(20, 20);
            this.lblDependent.Name = "lblDependent";
            this.lblDependent.Size = new System.Drawing.Size(65, 15);
            this.lblDependent.TabIndex = 0;
            this.lblDependent.Text = "因变量:";

            // 
            // cboDependentVar
            // 
            this.cboDependentVar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDependentVar.FormattingEnabled = true;
            this.cboDependentVar.Location = new System.Drawing.Point(100, 20);
            this.cboDependentVar.Name = "cboDependentVar";
            this.cboDependentVar.Size = new System.Drawing.Size(260, 23);
            this.cboDependentVar.TabIndex = 1;

            // 
            // lblIndependent
            // 
            this.lblIndependent.AutoSize = true;
            this.lblIndependent.Location = new System.Drawing.Point(20, 60);
            this.lblIndependent.Name = "lblIndependent";
            this.lblIndependent.Size = new System.Drawing.Size(65, 15);
            this.lblIndependent.TabIndex = 2;
            this.lblIndependent.Text = "自变量:";

            // 
            // lstIndependentVars
            // 
            this.lstIndependentVars.CheckOnClick = true;
            this.lstIndependentVars.FormattingEnabled = true;
            this.lstIndependentVars.Location = new System.Drawing.Point(20, 80);
            this.lstIndependentVars.Name = "lstIndependentVars";
            this.lstIndependentVars.Size = new System.Drawing.Size(340, 130);
            this.lstIndependentVars.TabIndex = 3;
            this.lstIndependentVars.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.LstIndependentVars_ItemCheck);

            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(200, 230);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "确定";
            this.btnOK.Enabled = false;

            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(290, 230);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "取消";

            // 
            // SpatialRegressionDialog
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lstIndependentVars);
            this.Controls.Add(this.lblIndependent);
            this.Controls.Add(this.cboDependentVar);
            this.Controls.Add(this.lblDependent);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpatialRegressionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "空间回归分析";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}