using System;
using System.Windows.Forms;
using System.Linq;
using GeoAnalyzer.Core.Models.Geometry;

namespace GeoAnalyzer.WinForm.Forms
{
    public partial class SpatialRegressionDialog : Form
    {
        private readonly Feature _layer;

        public string DependentVar => cboDependentVar.SelectedItem?.ToString();
        public string[] IndependentVars => lstIndependentVars.CheckedItems.Cast<string>().ToArray();
        public Feature Layer => _layer;

        public SpatialRegressionDialog(Feature layer)
        {
            _layer = layer ?? throw new ArgumentNullException(nameof(layer));
            InitializeComponent();
            LoadAttributes();

            // 绑定事件处理
            cboDependentVar.SelectedIndexChanged += CboDependentVar_SelectedIndexChanged;
            btnOK.Click += BtnOK_Click;
        }

        private void LoadAttributes()
        {
            if (_layer != null)
            {
                cboDependentVar.BeginUpdate();
                lstIndependentVars.BeginUpdate();
                try
                {
                    var attributes = _layer.GetAttributeNames().ToList();

                    cboDependentVar.Items.Clear();
                    lstIndependentVars.Items.Clear();

                    cboDependentVar.Items.AddRange(attributes.ToArray());
                    lstIndependentVars.Items.AddRange(attributes.ToArray());

                    if (cboDependentVar.Items.Count > 0)
                    {
                        cboDependentVar.SelectedIndex = 0;
                    }
                }
                finally
                {
                    cboDependentVar.EndUpdate();
                    lstIndependentVars.EndUpdate();
                }
            }
        }

        private void CboDependentVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool ValidateInputs()
        {
            if (cboDependentVar.SelectedIndex == -1)
            {
                MessageBox.Show("请选择因变量。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (lstIndependentVars.CheckedItems.Count == 0)
            {
                MessageBox.Show("请至少选择一个自变量。", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void UpdateButtonState()
        {
            btnOK.Enabled = cboDependentVar.SelectedIndex != -1 &&
                           lstIndependentVars.CheckedItems.Count > 0;
        }

        private void LstIndependentVars_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // 使用 BeginInvoke 来确保在 CheckedItems 集合更新后更新按钮状态
            BeginInvoke(new Action(() => UpdateButtonState()));
        }
    }
}