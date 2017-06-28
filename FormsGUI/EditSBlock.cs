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

        public EditSBlock(byte SBlockSize, List<byte> previousValues, bool isPBlock = false):this()
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
            Value.Clear();
            foreach(string s in textBox.Text.Split(' '))
            {
                byte t;
                Byte.TryParse(s, out t);
                Value.Add(t);
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

        private bool verifyValues()
        {
            return ((Func<List<byte>, bool, bool>)((arg, ZeroSeed) =>
            {
                List<byte> argCopy = arg;
                if (!ZeroSeed)
                    argCopy = arg.Select(i => --i).ToList();
                List<int> Temp = new List<int>(Enumerable.Repeat(0, argCopy.Count));
                foreach (var Ind in argCopy)
                {
                    if (Ind >= argCopy.Count)
                        return false;
                    Temp[Ind] += 1;
                }
                return Temp.All(x => x == 1);
            }))(Value, !isPBlock) && Value.Count == (int)Math.Pow(2, SBlockSize);
        }

        private void EditSBlock_FormClosing(object sender, FormClosingEventArgs e)
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
