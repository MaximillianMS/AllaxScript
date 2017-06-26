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
        public List<byte> Value = new List<byte>();
        private byte SBlockSize;
        private string ValidationErrorString = "Некорректное заполнение S-Блока";
        private bool isPBlock = false;
        public EditSBlock()
        {
            InitializeComponent();
            textBox.Text = "14 4 13 1 2 15 11 8 3 10 6 12 5 9 0 7";
        }

        public EditSBlock(byte SBlockSize, List<byte> previousValues, bool isPBlock = false):base()
        {
            textBox.Text = string.Join<byte>(" ", previousValues);
            this.SBlockSize = SBlockSize;
            if (isPBlock)
            {
                this.isPBlock = true;
                this.Text = "Редактировать P-Блок";
                this.label1.Text = "Вектор перестановки";
                ValidationErrorString = "Некорректное заполнение P-Блока";
            }
                       
        }

        
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            foreach(string s in textBox.Text.Split(' '))
            {
                Value.Add(Byte.Parse(s));
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (!verifyValues())
            {
                MessageBox.Show(ValidationErrorString);
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private bool verifyPValues()
        {
            return true;
        }

        private bool verifyValues()
        {
            if (isPBlock)
            {
                return verifyPValues();
            }
            return true;
        }
    }
}
