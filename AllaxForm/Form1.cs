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
            panel = new AllaxPanel(4, 4, 12);
            panel.Size = new Size(500, 500);
            panel.BackColor = Color.AliceBlue;
            panel.addKLayer();
            panel.addPLayer();
            panel.addSLayer();
            this.Controls.Add(panel);
            panel.setLayerColors(0, new List<bool>() { true, false, true, true, true, false, true, true, true, false, true, true, true, false, true, true, });
            //this.panel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
        }

        private void panel1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            panel.addSLayer();
        }
    }
}
