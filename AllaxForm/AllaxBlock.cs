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

        public int layer_index;
        public int index_in_layer;
        private Label label;
        public int connectors;
        public List<byte> init_sequence = new List<byte>();

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

            this.Paint += paint;
            SetStyle(ControlStyles.ResizeRedraw, true);
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

            label = new Label();
            label.Text = "S";
            label.Font = new Font(FontFamily.GenericSansSerif, (float)0.5 * height);
            label.Size = this.Size;
            label.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(label);
        }

        private List<bool> inputsToDraw;
        public void drawPBlockWeb(List<bool> inputs)
        {
            this.inputsToDraw = inputs; Invalidate();
        }
        public void removePBlockWeb()
        {
            this.inputsToDraw = null;
            this.label.Visible = true;
        }
        private void paint(object sender, System.Windows.Forms.PaintEventArgs e)

        {
            if (this.inputsToDraw == null) return;
            this.label.Visible = false;
            Graphics g = this.CreateGraphics();
            Pen pen = new Pen(Color.Red, 2);
            List<Point> topC = this.topConnectorsRelative();
            List<Point> botC = this.bottomConnectorsRelative();
            for (int i = 0; i < this.connectors; i++)
            {
                if (inputsToDraw[i] == false) continue;
                Point from = topC[i]; Point to = botC[init_sequence[i] - 1];
                int vertical_correction_f = (int)(this.Size.Height * 0.07);
                int vertical_correction_t = (int)(this.Size.Height * 0.1);
                g.DrawLine(pen, from.X, from.Y+vertical_correction_f , to.X, to.Y-vertical_correction_t);
            }
        }


        private void Pblock(int width, int height)
        {
            this.Size = new Size(width, height);
            this.BackColor = Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            label = new Label();
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

            label = new Label();
            label.Text = "K";
            label.Font = new Font(FontFamily.GenericSansSerif, (float)0.5 * height);
            label.Size = this.Size;
            label.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(label);
        }

        public List<Point> topConnectorsRelative()
        {
            List<Point> res = new List<Point>(); int step;
            /*switch (this.type)
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
            }*/
            step = this.Size.Width / (this.connectors+1);
            for (int i = 1; i < (this.connectors + 1); i++)
                res.Add(new Point(step * i, 0));
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
