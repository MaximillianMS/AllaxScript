using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AllaxForm
{
    public static class PanelSerializator
    {
        [Serializable()]
        public class LayerPanelData
        {
            public AllaxBlock.BLOCK_TYPE type;
            public List<BlockPanelData> blocks;
            public int layer_index;
        }
        [Serializable()]
        public class BlockPanelData
        {
            public List<byte> Init;
            public AllaxBlock.BLOCK_TYPE type;
            public int layer_index;
            public int index_in_layer;
        }
        [Serializable()]
        public class PanelData
        {
            public int wordsize;
            public int blocks_wide;
            public int blocks_tall;
            public List<LayerPanelData> layers = new List<LayerPanelData>();
        }
        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Zip(bytes);
        }
        public static byte[] Zip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }
        public static byte[] Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return mso.ToArray();
            }
        }
        public static void Serialize(AllaxPanel panel, FileStream FS)
        {
            var PD = new PanelData() { wordsize = panel.wordsize, blocks_wide = panel.blocks_wide, blocks_tall = panel.blocks_tall, layers = new List<LayerPanelData>(panel.layers.Count)};
            foreach(var L in panel.layers)
            {
                var LD = new LayerPanelData() { type = L.type, layer_index = L.layer_index, blocks = new List<BlockPanelData>(L.blocks.Count) };
                foreach (var B in L.blocks)
                {
                    LD.blocks.Add(new BlockPanelData() { index_in_layer = B.index_in_layer, layer_index = B.layer_index, type = B.type, Init = new List<byte>(B.init_sequence), });
                }
                PD.layers.Add(LD);
            }
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, PD);
                stream.Flush();
                stream.Position = 0;
                var arr = stream.ToArray();
                arr = Zip(Zip(arr));
                FS.Write(arr, 0, arr.Length);
            }
        }
        public static PanelData DeSerialize(FileStream FS)
        {
            var arr = new byte[FS.Length - FS.Position];
            FS.Read(arr, (int)FS.Position, arr.Length);
            arr = Unzip(Unzip(arr));
            PanelData PD;
            using (var stream = new MemoryStream(arr))
            {
                var formatter = new BinaryFormatter();
                PD = (PanelData)formatter.Deserialize(stream);
            }
            return PD;
        }
    }
    public partial class AllaxPanel : Panel
    {
        // All values as fractions of the panel's size
        public double block_height = 1.0 / 25;
        public double wide_block_width = 1;
        public double narrow_block_width = (1.0 / 4) * (3.0/4);
        public double block_width_distance = (1.0 / 4) * (1.0 / 4);
        public double block_height_distance = (3.0 / 16) / 13;

        public List<List<bool>> coloreds = new List<List<bool>>();

        public struct Layer
        {
            public AllaxBlock.BLOCK_TYPE type;
            public List<AllaxBlock> blocks;
            public int layer_index;
        }

        public readonly int wordsize;
        public readonly int blocks_wide;
        public readonly int blocks_tall;
        public List<Layer> layers = new List<Layer>();
  
        public AllaxPanel(int wordsize, int blocks_wide, int blocks_tall)
        {
            this.wordsize = wordsize; this.blocks_wide = blocks_wide; this.blocks_tall = blocks_tall;
            this.Paint += testpaint;
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.StandardClick, true);
            SetStyle(ControlStyles.StandardDoubleClick, true);
            this.initializeSizes();
           
            //this.initializeDragDrop();
        }

        private void addColorLayer()
        {
            List<bool> n = new List<bool>();
            for (int i = 0; i < this.wordsize * this.blocks_wide; i++)
                n.Add(false);
            this.coloreds.Add(n);
        }

        /*private void initializeDragDrop()
        {
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(allax_DragEnter);
            this.DragDrop += new DragEventHandler(allax_DragDrop);
        }
        public void allax_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        public void allax_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }*/


        private void initializeSizes()
        {
            /* public const double block_height = 1.0 / 25;
             public const double wide_block_width = 1;
             public const double narrow_block_width = (1.0 / 4) * (3.0 / 4);
             public const double block_width_distance = (1.0 / 4) * (1.0 / 4);
             public const double block_height_distance = (3.0 / 16) / 13;*/
            block_height = (1.0 / blocks_tall) * (3.0 / 5);
            block_height_distance = (1.0 / blocks_tall) * (2.0 / 5);

            wide_block_width = 1;
            narrow_block_width = (1.0 / blocks_wide) * (3.0 / 4);
            block_width_distance = (1.0 / blocks_wide) * (1.0 / 4);
    }
        public void setLayerColors(int layerindex, List<bool> coloring_mask)
        {
            this.coloreds[layerindex] = coloring_mask;
        }
        public void paint()
        {
            Graphics g = this.CreateGraphics();
            Pen pen = new Pen(Color.Black, 3);
            Pen penc = new Pen(Color.Red, 3);
            for (int i = 0; i < this.layers.Count - 1; i++)
            {
                // drawing lines from layer i to layer i+1
                List<Point> topC = new List<Point>();
                foreach (AllaxBlock b in this.layers[i].blocks)
                {
                    foreach (Point p in b.bottomConnectorsRelative())
                    {
                        Point abs = new Point(p.X + b.Location.X, p.Y + b.Location.Y);
                        topC.Add(abs);
                    }
                }
                List<Point> botC = new List<Point>();
                foreach (AllaxBlock b in this.layers[i + 1].blocks)
                {
                    foreach (Point p in b.topConnectorsRelative())
                    {
                        Point abs = new Point(p.X + b.Location.X, p.Y + b.Location.Y);
                        botC.Add(abs);
                    }
                }

                for (int j = 0; j < topC.Count; j++)
                {
                    Pen p;
                    if (this.coloreds[i][j] == true)
                        p = penc;
                    else
                        p = pen;
                    g.DrawLine(p,
                        topC[j].X, topC[j].Y,
                        botC[j].X, botC[j].Y);
                }
            }
        }

        private void testpaint(object sender, System.Windows.Forms.PaintEventArgs e)

        {
            paint();
        }

        public void addKLayer()
        {
            AllaxBlock newblock = new AllaxBlock(AllaxBlock.BLOCK_TYPE.K,
                (int)(wide_block_width * this.Size.Width),
                (int)(block_height * this.Size.Height));
            int height_pos = (int)(this.Size.Height * (block_height + block_height_distance)) * this.layers.Count;
            newblock.Location = new Point(0, height_pos);
            Layer newblockl = new Layer();
            newblockl.blocks = new List<AllaxBlock>();
            newblockl.blocks.Add(newblock);
            newblockl.type = AllaxBlock.BLOCK_TYPE.K;
            newblockl.layer_index = this.layers.Count;
            newblock.index_in_layer = 0;
            newblock.layer_index = newblockl.layer_index;
            newblock.connectors = this.wordsize * this.blocks_wide;
            this.layers.Add(newblockl);
            this.Controls.Add(newblock); addColorLayer();
            Invalidate();
        }

        public void addPLayer()
        {
            AllaxBlock newblock = new AllaxBlock(AllaxBlock.BLOCK_TYPE.P,
                (int)(wide_block_width * this.Size.Width),
                (int)(block_height * this.Size.Height));
            int height_pos = (int)(this.Size.Height * (block_height + block_height_distance)) * this.layers.Count;
            newblock.Location = new Point(0, height_pos);
            Layer newblockl = new Layer();
            newblockl.blocks = new List<AllaxBlock>();
            newblockl.blocks.Add(newblock);
            newblockl.type = AllaxBlock.BLOCK_TYPE.P;
            newblockl.layer_index = this.layers.Count;
            newblock.index_in_layer = 0;
            newblock.layer_index = newblockl.layer_index;
            newblock.connectors = this.wordsize * this.blocks_wide;
            this.layers.Add(newblockl);
            this.Controls.Add(newblock); addColorLayer();
            Invalidate();
        }

        public void addSLayer()
        {
            Layer newblockl = new Layer();
            newblockl.blocks = new List<AllaxBlock>();
            for (int i=0; i<this.blocks_wide; i++)
            {
                AllaxBlock newblock = new AllaxBlock(AllaxBlock.BLOCK_TYPE.S,
                    (int)(narrow_block_width * this.Size.Width),
                    (int)(block_height * this.Size.Height));
                int height_pos = (int)(this.Size.Height * (block_height + block_height_distance)) * this.layers.Count;
                newblock.Location = new Point(
                    (int)((narrow_block_width + block_width_distance) * i * this.Size.Width) + (int)(block_width_distance * this.Size.Width/2), 
                    height_pos);
                newblockl.blocks.Add(newblock);
                newblock.connectors = this.wordsize;
                newblock.index_in_layer = i;
                newblock.layer_index = this.layers.Count;
                this.Controls.Add(newblock);
            }
            newblockl.layer_index = this.layers.Count;
            newblockl.type = AllaxBlock.BLOCK_TYPE.S;
            this.layers.Add(newblockl); addColorLayer();
            Invalidate();
        }
    }
}
