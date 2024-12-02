namespace GeoAnalyzer.WinForm.Forms
{
    partial class SpatialClusteringDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // 控件声明为类的字段
        private System.Windows.Forms.NumericUpDown numClusters;
        private System.Windows.Forms.CheckedListBox lstAttributes;
        private System.Windows.Forms.Label lblClusters;
        private System.Windows.Forms.Label lblAttributes;
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
            this.numClusters = new System.Windows.Forms.NumericUpDown();
            this.lstAttributes = new System.Windows.Forms.CheckedListBox();
            this.lblClusters = new System.Windows.Forms.Label();
            this.lblAttributes = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numClusters)).BeginInit();
            this.SuspendLayout();

            // 
            // numClusters
            // 
            this.numClusters.Location = new System.Drawing.Point(100, 20);
            this.numClusters.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numClusters.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            this.numClusters.Name = "numClusters";
            this.numClusters.Size = new System.Drawing.Size(120, 23);
            this.numClusters.TabIndex = 1;
            this.numClusters.Value = new decimal(new int[] { 3, 0, 0, 0 });

            // 
            // lstAttributes
            // 
            this.lstAttributes.CheckOnClick = true;
            this.lstAttributes.FormattingEnabled = true;
            this.lstAttributes.Location = new System.Drawing.Point(20, 80);
            this.lstAttributes.Name = "lstAttributes";
            this.lstAttributes.Size = new System.Drawing.Size(340, 130);
            this.lstAttributes.TabIndex = 2;

            // 
            // lblClusters
            // 
            this.lblClusters.AutoSize = true;
            this.lblClusters.Location = new System.Drawing.Point(20, 20);
            this.lblClusters.Name = "lblClusters";
            this.lblClusters.Size = new System.Drawing.Size(65, 15);
            this.lblClusters.TabIndex = 0;
            this.lblClusters.Text = "聚类数量:";

            // 
            // lblAttributes
            // 
            this.lblAttributes.AutoSize = true;
            this.lblAttributes.Location = new System.Drawing.Point(20, 60);
            this.lblAttributes.Name = "lblAttributes";
            this.lblAttributes.Size = new System.Drawing.Size(65, 15);
            this.lblAttributes.TabIndex = 3;
            this.lblAttributes.Text = "选择属性:";

            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(200, 230);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "确定";

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
            // SpatialClusteringDialog
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 300);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblAttributes);
            this.Controls.Add(this.lstAttributes);
            this.Controls.Add(this.lblClusters);
            this.Controls.Add(this.numClusters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpatialClusteringDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "空间聚类分析";
            ((System.ComponentModel.ISupportInitialize)(this.numClusters)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}