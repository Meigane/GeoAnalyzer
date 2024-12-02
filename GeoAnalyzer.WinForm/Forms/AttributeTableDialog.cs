using System;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Text;
using GeoAnalyzer.Core.Models.Geometry;
using OfficeOpenXml;
using GeoAnalyzer.WinForm.LayerEvents;
using EventArgs = System.EventArgs;

namespace GeoAnalyzer.WinForm.Forms
{
    public partial class AttributeTableDialog : Form
    {
        private Feature layer;
        private BindingSource bindingSource;

        public AttributeTableDialog(Feature layer)
        {
            InitializeComponent();
            
            // 配置 DataGridView - 先禁用自动调整大小
            gridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;  // 先禁用自动调整
            gridView.Dock = DockStyle.Fill;
            gridView.AllowUserToAddRows = false;
            gridView.ReadOnly = true;
            gridView.ColumnHeadersVisible = true;
            gridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            gridView.ColumnHeadersHeight = 30;
            gridView.EnableHeadersVisualStyles = false;
            gridView.BorderStyle = BorderStyle.Fixed3D;
            gridView.RowHeadersVisible = true;
            gridView.AllowUserToOrderColumns = true;
            gridView.MultiSelect = false;
            
            // 设置窗体属性
            this.Text = "属性表";
            this.Size = new Size(800, 600);
            
            this.layer = layer;
            LoadData();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // 确保所有控件可见
            foreach (Control ctrl in Controls)
            {
                ctrl.Visible = true;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            try
            {
                // 在窗体显示后再启用自动调整大小
                gridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                gridView.Refresh();
                this.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"调整列宽时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            if (layer?.Properties != null)
            {
                try
                {
                    var table = new System.Data.DataTable();
                    
                    // 添加列
                    foreach (var name in layer.GetAttributeNames())
                    {
                        table.Columns.Add(name);
                    }

                    // 如果是包含多个要素的图层，需要为每个要素添加一行
                    if (layer.Features != null && layer.Features.Count > 0)
                    {
                        foreach (var feature in layer.Features)
                        {
                            var row = table.NewRow();
                            foreach (var name in feature.GetAttributeNames())
                            {
                                var value = feature.GetAttribute(name);
                                row[name] = value;
                            }
                            table.Rows.Add(row);
                        }
                    }
                    else
                    {
                        var row = table.NewRow();
                        foreach (var name in layer.GetAttributeNames())
                        {
                            var value = layer.GetAttribute(name);
                            row[name] = value;
                        }
                        table.Rows.Add(row);
                    }

                    // 绑定数据时暂时禁用列自动调整
                    gridView.SuspendLayout();
                    bindingSource = new BindingSource();
                    bindingSource.DataSource = table;
                    gridView.DataSource = bindingSource;

                    // 手动设置每列的初始宽度
                    foreach (DataGridViewColumn col in gridView.Columns)
                    {
                        col.Width = 100;  // 设置一个固定的初始宽度
                        col.HeaderText = col.Name;
                        col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        col.HeaderCell.Style.Font = new Font(gridView.Font, FontStyle.Bold);
                    }

                    gridView.ResumeLayout();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载数据失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV文件 (*.csv)|*.csv|Excel文件 (*.xlsx)|*.xlsx";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string extension = Path.GetExtension(dialog.FileName).ToLower();
                        if (extension == ".csv")
                        {
                            ExportToCSV(dialog.FileName);
                        }
                        else if (extension == ".xlsx")
                        {
                            ExportToExcel(dialog.FileName);
                        }

                        MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportToCSV(string filePath)
        {
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                DataTable table = (DataTable)bindingSource.DataSource;

                // 写入列头
                var headers = new List<string>();
                foreach (DataColumn column in table.Columns)
                {
                    headers.Add(EscapeCSV(column.ColumnName));
                }
                writer.WriteLine(string.Join(",", headers));

                // 写入数据行
                foreach (DataRow row in table.Rows)
                {
                    var fields = new List<string>();
                    foreach (var item in row.ItemArray)
                    {
                        fields.Add(EscapeCSV(item?.ToString() ?? ""));
                    }
                    writer.WriteLine(string.Join(",", fields));
                }
            }
        }

        private string EscapeCSV(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains("\"") || field.Contains(",") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        private void ExportToExcel(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("属性表");
                DataTable table = (DataTable)bindingSource.DataSource;

                // 写入列头
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = table.Columns[i].ColumnName;
                    // 设置列头样式
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                // 写入数据
                for (int row = 0; row < table.Rows.Count; row++)
                {
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        worksheet.Cells[row + 2, col + 1].Value = table.Rows[row][col];
                    }
                }

                // 自动调整宽
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // 添加表格边框
                var tableRange = worksheet.Cells[1, 1, table.Rows.Count + 1, table.Columns.Count];
                tableRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                tableRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                tableRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                tableRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                // 保存文件
                package.SaveAs(new FileInfo(filePath));
            }
        }
    }
} 