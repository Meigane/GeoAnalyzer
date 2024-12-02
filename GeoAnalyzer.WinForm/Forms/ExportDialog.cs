using System;
using System.Windows.Forms;
using System.IO;
using GeoAnalyzer.WinForm.LayerEvents;

namespace GeoAnalyzer.WinForm.Forms
{
    public partial class ExportDialog : Form
    {
        public string SelectedFormat { get; private set; }
        public string FilePath { get; private set; }

        public ExportDialog()
        {
            InitializeComponent();
            cboExportFormat.SelectedIndex = 0;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = GetFileFilter();
                saveFileDialog.Title = "选择导出文件位置";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = saveFileDialog.FileName;
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                MessageBox.Show("请选择导出文件路径。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SelectedFormat = cboExportFormat.SelectedItem.ToString();
            FilePath = txtFilePath.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string GetFileFilter()
        {
            switch (cboExportFormat.SelectedItem.ToString())
            {
                case "Shapefile":
                    return "Shapefile (*.shp)|*.shp";
                case "GeoJSON":
                    return "GeoJSON (*.geojson)|*.geojson";
                case "KML":
                    return "KML (*.kml)|*.kml";
                case "CSV":
                    return "CSV (*.csv)|*.csv";
                default:
                    return "All files (*.*)|*.*";
            }
        }
    }
}