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

        public void init(BLOCK_TYPE type, int width, int height)
        {
            InitializeComponent();
            this.type = type;
            switch (type)
            {
                case BLOCK_TYPE.S:
                    this.Sblock(width, height);
                    break;
                case BLOCK_TYPE.P:
                    this.Pblock(width, height);
                    break;
                case BLOCK_TYPE.K:
                    this.Kblock(width, height);
                    break;
            }
        }

        public AllaxBlock(BLOCK_TYPE type, int width, int height)
        {
            this.init(type, width, height);
        }

        public AllaxBlock(BLOCK_TYPE type, int scale)
        {
            switch (type)
            {
                case BLOCK_TYPE.S:
                    this.init(type, 3 * scale, 1 * scale);
                    break;
                case BLOCK_TYPE.P:
                case BLOCK_TYPE.K:
                    this.init(type, (int)((4 * 3 + 0.5) * scale), 1 * scale);
                    break;
            }
        }

        private void AllaxBlock_MouseDown(object sender, MouseEventArgs e)
        {
            this.DoDragDrop(this, DragDropEffects.Copy |
               DragDropEffects.Move);
        }

        private void Sblock(int width, int height)
        {
            this.Size = new Size(width, height);
            this.BackColor = Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            Label label = new Label();
            label.Text = "S";
            label.Font = new Font(FontFamily.GenericSansSerif, (float)0.5 * height);
            label.Size = this.Size;
            label.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(label);
        }


        private void Pblock(int width, int height)
        {
            this.Size = new Size(width, height);
            this.BackColor = Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            Label label = new Label();
            label.Text = "P";
            label.Font = new Font(FontFamily.GenericSansSerif, (float)0.5 * height);
            label.Size = this.Size;
            label.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(label);
        }
        private void Kblock(int width, int height)
        {
            this.Size = new Size(width, height);
            this.BackColor = Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            Label label = new Label();
            label.Text = "K";
            label.Font = new Font(FontFamily.GenericSansSerif, (float)0.5 * height);
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
