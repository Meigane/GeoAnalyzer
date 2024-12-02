using System;
using System.Windows.Forms;

namespace GeoAnalyzer.WinForm.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // 菜单栏
        private System.Windows.Forms.MenuStrip menuStrip;
        
        // 文件菜单
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        
        // 视图菜单
        private System.Windows.Forms.ToolStripMenuItem viewMenu;
        private System.Windows.Forms.ToolStripMenuItem layerManagerMenuItem;
        private System.Windows.Forms.ToolStripMenuItem attributeTableMenuItem;
        
        // 数据菜单
        private System.Windows.Forms.ToolStripMenuItem dataMenu;
        private System.Windows.Forms.ToolStripMenuItem importShapefileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportShapefileMenuItem;

        // 空间分析菜单
        private System.Windows.Forms.ToolStripMenuItem spatialAnalysisMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spatialClusteringMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spatialAutocorrelationMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gwrMenuItem;

        // 状态栏
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        
        // 内容面板
        private System.Windows.Forms.Panel contentPanel;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Panel rightPanel;

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
            Console.WriteLine("开始初始化组件...");
            try
            {
                // 初始化所有菜单项
                this.menuStrip = new System.Windows.Forms.MenuStrip();
                this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
                this.viewMenu = new System.Windows.Forms.ToolStripMenuItem();
                this.dataMenu = new System.Windows.Forms.ToolStripMenuItem();
                this.spatialAnalysisMenuItem = new System.Windows.Forms.ToolStripMenuItem();

                // 初始化文件菜单项 - 只保留退出
                this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                
                // 初始化视图菜单项
                this.layerManagerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                this.attributeTableMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                
                // 初始化数据菜单项
                this.importShapefileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                this.exportShapefileMenuItem = new System.Windows.Forms.ToolStripMenuItem();

                // 初始化空间分析菜单项
                this.spatialClusteringMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                this.spatialAutocorrelationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                this.gwrMenuItem = new System.Windows.Forms.ToolStripMenuItem();

                Console.WriteLine("菜单项初始化完成，开始设置菜单项属性...");

                // 设置文件菜单
                this.fileMenu.Text = "文件(&F)";
                this.exitMenuItem.Text = "退出(&X)";
                
                // 设置视图菜单
                this.viewMenu.Text = "视图(&V)";
                this.layerManagerMenuItem.Text = "图层管理器(&L)";
                this.attributeTableMenuItem.Text = "属性表(&A)";
                
                // 设置数据菜单
                this.dataMenu.Text = "数据(&D)";
                this.importShapefileMenuItem.Text = "导入Shapefile(&I)";
                this.exportShapefileMenuItem.Text = "导出Shapefile(&E)";

                // 设置空间分析菜单
                this.spatialAnalysisMenuItem.Text = "空间分析(&S)";
                this.spatialClusteringMenuItem.Text = "空间聚类分析(&C)";
                this.spatialAutocorrelationMenuItem.Text = "空间自相关分析(&A)";
                this.gwrMenuItem.Text = "地理加权回归(&G)";

                Console.WriteLine("开始构建菜单层次结构...");

                // 构建菜单层次结构
                this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    this.exitMenuItem
                });

                this.viewMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    this.layerManagerMenuItem,
                    this.attributeTableMenuItem
                });

                this.dataMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    this.importShapefileMenuItem,
                    this.exportShapefileMenuItem
                });

                this.spatialAnalysisMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    this.spatialClusteringMenuItem,
                    this.spatialAutocorrelationMenuItem,
                    this.gwrMenuItem
                });

                // 将主菜单项添加到菜单栏
                this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                    this.fileMenu,
                    this.viewMenu,
                    this.dataMenu,
                    this.spatialAnalysisMenuItem
                });

                Console.WriteLine("开始初始化面板...");

                // 初始化面板
                this.contentPanel = new System.Windows.Forms.Panel();
                this.leftPanel = new System.Windows.Forms.Panel();
                this.rightPanel = new System.Windows.Forms.Panel();

                // 设置面板属性
                this.contentPanel.SuspendLayout();
                this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
                this.contentPanel.Name = "contentPanel";

                this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
                this.leftPanel.Width = 250;
                this.leftPanel.Name = "leftPanel";

                this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
                this.rightPanel.Name = "rightPanel";

                // 添加控件到窗体
                this.contentPanel.Controls.Add(this.rightPanel);
                this.contentPanel.Controls.Add(this.leftPanel);
                this.Controls.Add(this.contentPanel);
                this.Controls.Add(this.menuStrip);

                // 设置窗体属性
                this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 27F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(2389, 1728);
                this.MainMenuStrip = this.menuStrip;
                this.Name = "MainForm";
                this.Text = "GeoAnalyzer";

                Console.WriteLine("完成布局设置...");

                // 恢复布局
                this.contentPanel.ResumeLayout(false);
                this.ResumeLayout(false);
                this.PerformLayout();

                Console.WriteLine("组件初始化完成");

                // 设置空间分析菜单项的事件处理
                this.spatialClusteringMenuItem.Click += new System.EventHandler(this.OnSpatialClustering);
                this.spatialAutocorrelationMenuItem.Click += new System.EventHandler(this.OnSpatialAutocorrelation);
                this.gwrMenuItem.Click += new System.EventHandler(this.OnGeographicallyWeightedRegression);

                Console.WriteLine("空间分析菜单事件已绑定");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化组件时发生错误: {ex.Message}");
                Console.WriteLine($"错误堆栈: {ex.StackTrace}");
                MessageBox.Show($"初始化界面时发生错误：{ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 