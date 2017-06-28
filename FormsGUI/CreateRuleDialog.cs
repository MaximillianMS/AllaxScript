using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            startingInputMaskedTextBox.Mask = "".PadLeft(16, '0');
        }

        public byte wordLength = 16;
        public byte sBlockCount = 4;

        private void createButton_Click(object sender, EventArgs e)
        {
            if (sBlockInputCountNumericUpDown.Value % sBlockLayerCountNumericUpDown.Value != 0)
            {
                MessageBox.Show("Недопустимые значения длины слова и размера S-Блока");
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                wordLength = (byte)sBlockInputCountNumericUpDown.Value;
                //sBlockSize = (byte)sBlockLayerCountNumericUpDown.Value;
                //sBlockCount = (byte)(wordLength / sBlockSize);
            }
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
                startingInputMaskedTextBox.Enabled = true;
                sBlockInputCountLabel.Enabled = false;
                sBlockInputCountNumericUpDown.Enabled = false;
            }
        }

        private void startingInputMaskedTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '0' || e.KeyChar != '1')
            {
                e.Handled = true;
            }
        }
    }
}
