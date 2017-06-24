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
    public partial class EditSBlock : Form
    {
        public List<byte> SValues = new List<byte>();
        public EditSBlock()
        {
            InitializeComponent();
            textBox.Text = "14 4 13 1 2 15 11 8 3 10 6 12 5 9 0 7";
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            foreach(string s in textBox.Text.Split(' '))
            {
                SValues.Add(Byte.Parse(s));
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
