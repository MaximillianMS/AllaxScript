using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Allax;

namespace FormsGUI
{
    public partial class CreateRuleDialog : Form
    {
        public CreateRuleDialog()
        {
            InitializeComponent();
        }

        public CreateRuleDialog(byte sBlockCount, byte wordLength) : this()
        {
            this.wordLength = wordLength;    
            this.sBlockCount = sBlockCount;
            startingInputMaskedTextBox.Mask = "".PadLeft(wordLength, '0');
            sBlockInputCountNumericUpDown.Maximum = sBlockCount;
            sBlockLayerCountNumericUpDown.Maximum = sBlockCount;
            SolverTypeComboBox.SelectedIndex = 1;
        }

        public byte wordLength = 16;
        public byte sBlockCount = 4;

        public Allax.Rule rule;

        private void createButton_Click(object sender, EventArgs e)
        {
            rule = new Allax.Rule(AvailableSolverTypes.GetAllTypes()[SolverTypeComboBox.SelectedIndex], (int)sBlockLayerCountNumericUpDown.Value, (int)sBlockInputCountNumericUpDown.Value, false);
            if (useConstInputCheckBox.Checked)
            {
                if (!startingInputMaskedTextBox.MaskCompleted)
                {
                    MessageBox.Show("Начальное заполнение не задано");
                    return;
                }
                List<bool> l = new List<bool>();
                foreach (char c in startingInputMaskedTextBox.Text)
                {
                    l.Add(Convert.ToBoolean(c));
                }
                Allax.SolverInputs i = new Allax.SolverInputs(Allax.WayConverter.ToLong(l), l.Count);
                rule.UseCustomInput = true;
                rule.Input = i;
                rule.MaxStartBlocks = sBlockCount;
            }
            this.DialogResult = DialogResult.OK;
        }

        private void CreateRuleDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
                return;
            this.DialogResult = DialogResult.Cancel;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void useConstInputCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (useConstInputCheckBox.Checked)
            {
                startingInputMaskedTextBox.Enabled = true;
                sBlockInputCountLabel.Enabled = false;
                sBlockInputCountNumericUpDown.Enabled = false;
            }
            else
            {
                startingInputMaskedTextBox.Enabled = false;
                sBlockInputCountLabel.Enabled = true;
                sBlockInputCountNumericUpDown.Enabled = true;
            }
        }

        private void startingInputMaskedTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '0' && e.KeyChar != '1')
            {
                e.Handled = true;
            }
        }
    }
}
