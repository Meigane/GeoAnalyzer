using System.Windows.Forms;

namespace GeoAnalyzer.WinForm.Forms
{
    partial class AttributeTableDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // 声明控件
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnExport;
        private System.Windows.Forms.DataGridView gridView;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblCount;

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

            // 初始化工具栏
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnExport = new System.Windows.Forms.ToolStripButton();
            this.btnExport.Text = "导出";
            this.toolStrip.Items.Add(this.btnExport);
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.Top;

            // 初始化数据网格
            this.gridView = new System.Windows.Forms.DataGridView();
            this.gridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridView.AllowUserToAddRows = false;
            this.gridView.AllowUserToDeleteRows = false;
            this.gridView.ReadOnly = true;
            this.gridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridView.MultiSelect = false;
            this.gridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
            this.gridView.ColumnHeadersVisible = true;
            this.gridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            this.gridView.ColumnHeadersHeight = 30;
            this.gridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.gridView.EnableHeadersVisualStyles = false;
            this.gridView.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            this.gridView.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font(this.gridView.Font, System.Drawing.FontStyle.Bold);
            this.gridView.Margin = new System.Windows.Forms.Padding(0);

            // 初始化状态栏
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip.Items.Add(this.lblCount);
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.Bottom;

            // 设置窗体属性
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "AttributeTableDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "属性表";

            // 添加控件到窗体
            this.Controls.Add(this.gridView);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.statusStrip);

            // 绑定事件
            this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);

            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridView)).BeginInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}