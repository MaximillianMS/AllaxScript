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
    public partial class AllaxBlock : Panel
    {
        public enum BLOCK_TYPE {S, P, K,};

        public BLOCK_TYPE type; 

        public AllaxBlock(BLOCK_TYPE type, int scale)
        {
            InitializeComponent();
            this.type = type;
            switch (type) {
                case BLOCK_TYPE.S:
                    this.Sblock(scale);
                    break;
                case BLOCK_TYPE.P:
                    this.Pblock(scale);
                    break;
                case BLOCK_TYPE.K:
                    this.Kblock(scale);
                    break;
            }
            this.MouseDown += AllaxBlock_MouseDown;
        }

        private void AllaxBlock_MouseDown(object sender, MouseEventArgs e)
        {
            this.DoDragDrop(this, DragDropEffects.Copy |
               DragDropEffects.Move);
        }

        private void Sblock(int scale)
        {
            this.Size = new Size(3 * scale, 1 * scale);
            this.BackColor = Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            Label label = new Label();
            label.Text = "S";
            label.Font = new Font(FontFamily.GenericSansSerif, (float)0.5 * scale);
            label.Size = this.Size;
            label.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(label);
        }


        private void Pblock(int scale)
        {
            this.Size = new Size((int)((4*3+0.5) * scale), 1 * scale);
            this.BackColor = Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            Label label = new Label();
            label.Text = "P";
            label.Font = new Font(FontFamily.GenericSansSerif, (float)0.5 * scale);
            label.Size = this.Size;
            label.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(label);
        }
        private void Kblock(int scale)
        {
            this.Size = new Size((int)((4 * 3 + 0.5) * scale), 1 * scale);
            this.BackColor = Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            Label label = new Label();
            label.Text = "K";
            label.Font = new Font(FontFamily.GenericSansSerif, (float)0.5 * scale);
            label.Size = this.Size;
            label.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(label);
        }

        public List<Point> topConnectorsRelative()
        {
            List<Point> res = new List<Point>(); int step;
            switch (this.type)
            {
                case (BLOCK_TYPE.K):
                case (BLOCK_TYPE.P):
                    step = this.Size.Width / (4 * 4 + 1);
                    for (int i = 1; i < (4 * 4 + 1); i++)
                        res.Add(new Point(step * i, 0));
                    break;
                case (BLOCK_TYPE.S):
                    step = this.Size.Width / (4 + 1);
                    for (int i = 1; i < (4 + 1); i++)
                        res.Add(new Point(step * i, 0));
                    break;
            }
            return res;
        }

        public List<Point> bottomConnectorsRelative()
        {
            List<Point> res = this.topConnectorsRelative();
            for (int i=0;i<res.Count;i++)
            {
                res[i] = new Point(res[i].X, this.Size.Height);
            }
            return res;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
