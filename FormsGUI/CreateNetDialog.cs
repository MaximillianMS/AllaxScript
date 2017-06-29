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
    public partial class CreateNetDialog : Form
    {
        public CreateNetDialog()
        {
            InitializeComponent();
        }

        public byte wordLength = 16;
        public byte sBlockSize = 4;
        public byte sBlockCount = 4;

        private void createButton_Click(object sender, EventArgs e)
        {
            if ((wordLengthNumericUpDown.Value % sBlockSizeNumericUpDown.Value != 0)||(wordLengthNumericUpDown.Value<sBlockSizeNumericUpDown.Value))
            {
                MessageBox.Show("Недопустимые значения длины слова и размера S-Блока");
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                wordLength = (byte)wordLengthNumericUpDown.Value;
                sBlockSize = (byte)sBlockSizeNumericUpDown.Value;
                sBlockCount = (byte)(wordLength / sBlockSize);
            }
        }

        private void CreateNetDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
                return;
            this.DialogResult = DialogResult.Cancel;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
