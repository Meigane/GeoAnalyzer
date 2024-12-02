using System;
using System.Windows.Forms;
using System.Linq;
using GeoAnalyzer.Core.Models.Geometry;
using GeoAnalyzer.WinForm.LayerEvents;
using EventArgs = System.EventArgs;

namespace GeoAnalyzer.WinForm.Forms
{
    public partial class GeographicallyWeightedRegressionDialog : Form
    {
        protected Feature Layer { get; private set; }

        public string DependentVar => cboDependentVar.SelectedItem?.ToString();
        public string[] IndependentVars => lstIndependentVars.CheckedItems.Cast<string>().ToArray();
        public double Bandwidth => (double)numBandwidth.Value;

        public GeographicallyWeightedRegressionDialog()
        {
            InitializeComponent();
        }

        public GeographicallyWeightedRegressionDialog(Feature layer) : this()
        {
            Layer = layer;
            LoadAttributes();
        }

        private void LoadAttributes()
        {
            if (Layer != null)
            {
                var attributes = Layer.GetAttributeNames().ToList();
                cboDependentVar.Items.AddRange(attributes.ToArray());
                lstIndependentVars.Items.AddRange(attributes.ToArray());

                if (cboDependentVar.Items.Count > 0)
                    cboDependentVar.SelectedIndex = 0;
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (cboDependentVar.SelectedIndex == -1)
            {
                MessageBox.Show("请选择因变量。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (lstIndependentVars.CheckedItems.Count == 0)
            {
                MessageBox.Show("请至少选择一个自变量。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}