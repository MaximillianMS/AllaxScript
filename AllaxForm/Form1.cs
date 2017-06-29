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
        AllaxPanel panel;
        public Form1()
        {
            InitializeComponent();
            panel = new AllaxPanel(4, 4, 12, block_DoubleClick);
            panel.Size = new Size(1000, 500);
            panel.BackColor = Color.AliceBlue;
            panel.addKLayer();
            panel.addPLayer();
            panel.addKLayer();
            panel.layers[1].blocks[0].init_sequence = new List<byte>() { 1, 3, 5, 7, 9, 11, 13, 15, 2, 4, 6, 8, 10, 12, 14, 16 };
            panel.layers[1].blocks[0].drawPBlockWeb(new List<bool>() { true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, });
            this.Controls.Add(panel);
            panel.setLayerColors(0, new List<bool>() { true, false, true, true, true, false, true, true, true, false, true, true, true, false, true, true, });
            this.panel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            //panel.layers[1].blocks[0].removePBlockWeb();
        }

        private void block_DoubleClick(object sender, EventArgs e)
        {

        }

        private void panel1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            panel.removeLayer();
        }
    }
}
