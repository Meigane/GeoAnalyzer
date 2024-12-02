using System;
using System.Windows.Forms;

namespace GeoAnalyzer.WinForm.Forms
{
    partial class SpatialAutocorrelationDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private ComboBox cboAttributes;
        private Label lblAttributes;
        private Label lblWeightMethod;
        private ComboBox cboWeightMethod;
        private Label lblKNN;
        private NumericUpDown numKNN;
        private Label lblDistance;
        private NumericUpDown numDistance;
        private Button btnOK;
        private Button btnCancel;

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
            this.components = new System.ComponentModel.Container();

            // 属性选择
            this.lblAttributes = new Label();
            this.lblAttributes.Location = new System.Drawing.Point(20, 20);
            this.lblAttributes.Size = new System.Drawing.Size(100, 20);
            this.lblAttributes.Text = "选择属性字段:";
            this.lblAttributes.AutoSize = true;

            this.cboAttributes = new ComboBox();
            this.cboAttributes.Location = new System.Drawing.Point(130, 20);
            this.cboAttributes.Size = new System.Drawing.Size(240, 21);
            this.cboAttributes.DropDownStyle = ComboBoxStyle.DropDownList;

            // 空间权重方法
            this.lblWeightMethod = new Label();
            this.lblWeightMethod.Location = new System.Drawing.Point(20, 60);
            this.lblWeightMethod.Size = new System.Drawing.Size(100, 20);
            this.lblWeightMethod.Text = "空间权重方法:";
            this.lblWeightMethod.AutoSize = true;

            this.cboWeightMethod = new ComboBox();
            this.cboWeightMethod.Location = new System.Drawing.Point(130, 60);
            this.cboWeightMethod.Size = new System.Drawing.Size(240, 21);
            this.cboWeightMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cboWeightMethod.Items.AddRange(new object[] { "K最近邻", "距离阈值" });
            this.cboWeightMethod.SelectedIndex = 0;
            this.cboWeightMethod.SelectedIndexChanged += new EventHandler(WeightMethod_SelectedIndexChanged);

            // K值设置
            this.lblKNN = new Label();
            this.lblKNN.Location = new System.Drawing.Point(20, 100);
            this.lblKNN.Size = new System.Drawing.Size(100, 20);
            this.lblKNN.Text = "K值:";
            this.lblKNN.AutoSize = true;

            this.numKNN = new NumericUpDown();
            this.numKNN.Location = new System.Drawing.Point(130, 100);
            this.numKNN.Size = new System.Drawing.Size(120, 21);
            this.numKNN.Minimum = 1;
            this.numKNN.Maximum = 100;
            this.numKNN.Value = 8;

            // 距离阈值设置
            this.lblDistance = new Label();
            this.lblDistance.Location = new System.Drawing.Point(20, 100);
            this.lblDistance.Size = new System.Drawing.Size(100, 20);
            this.lblDistance.Text = "距离阈值:";
            this.lblDistance.AutoSize = true;

            this.numDistance = new NumericUpDown();
            this.numDistance.Location = new System.Drawing.Point(130, 100);
            this.numDistance.Size = new System.Drawing.Size(120, 21);
            this.numDistance.Minimum = -1;
            this.numDistance.Maximum = 10000;
            this.numDistance.DecimalPlaces = 2;
            this.numDistance.Value = -1;

            // 按钮
            this.btnOK = new Button();
            this.btnOK.Location = new System.Drawing.Point(190, 140);
            this.btnOK.Size = new System.Drawing.Size(75, 25);
            this.btnOK.Text = "确定";
            this.btnOK.DialogResult = DialogResult.OK;

            this.btnCancel = new Button();
            this.btnCancel.Location = new System.Drawing.Point(280, 140);
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.Text = "取消";
            this.btnCancel.DialogResult = DialogResult.Cancel;

            // 窗体设置
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 190);
            this.Controls.AddRange(new Control[] {
                this.lblAttributes,
                this.cboAttributes,
                this.lblWeightMethod,
                this.cboWeightMethod,
                this.lblKNN,
                this.numKNN,
                this.lblDistance,
                this.numDistance,
                this.btnOK,
                this.btnCancel
            });

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpatialAutocorrelationDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "空间自相关分析";

            // 初始化显示状态
            WeightMethod_SelectedIndexChanged(null, EventArgs.Empty);
        }

        private void WeightMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isKNN = cboWeightMethod.SelectedIndex == 0;
            lblKNN.Visible = numKNN.Visible = isKNN;
            lblDistance.Visible = numDistance.Visible = !isKNN;
        }

        #endregion
    }
}