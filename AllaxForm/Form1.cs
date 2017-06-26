using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AllaxForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            AllaxBlock pblock = new AllaxBlock(AllaxBlock.BLOCK_TYPE.P, 30);
            AllaxBlock sblock4 = new AllaxBlock(AllaxBlock.BLOCK_TYPE.S, 30);
            sblock4.Location = new Point(0,200);
            this.Controls.Add(pblock);
            this.Controls.Add(sblock4);

            List<Point> pconnectors = pblock.topConnectorsRelative();
        }
    }
}
