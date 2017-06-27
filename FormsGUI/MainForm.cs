using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using Allax;
using AllaxForm;
using System.Threading;

namespace FormsGUI
{
    public partial class MainForm : Form
    {
        
        Engine eng;
        ISPNet net;
        SPNetSettings currentSettings;
        AllaxPanel SPNetGraph;
        List<string> layerList = new List<string>();
        List<Solution> solutions = new List<Solution>();
        delegate void RefreshSolutionsCallback();
        delegate void AllTasksDoneCallback(ITask task);
        delegate void RefreshProgressBarCallback(double progress);
        private static object syncRoot = new object();
        public MainForm()
        {
            InitializeComponent();
            eng = new Engine();
            eng.ONSOLUTIONFOUND += this.E_AddSolution;
            eng.ONPROGRESSCHANGED += this.E_ProgressChanged;
            eng.ONTASKDONE += E_OnTaskDone;
            eng.ONALLTASKSDONE += E_OnAllTasksDone;
            tasksProgressBar.Width = solutionsListBox.Size.Width;
            createSPNet(16, 4);
            /*
            SPNetSettings settings = new SPNetSettings(16, 4);
            net = eng.CreateSPNetInstance(settings);
            addFullRound();
            addLastRound();
            */
            //DoTesting();
            //this.Close();
        }
        /*private void DoTesting()
        {
            addRound();
            addRound();
            addRound();
            addRound();
            addRound();
            addRound();
            addRound();
            runAnalysis();
            toolStrip1.Enabled = false;
            menuStrip1.Enabled = false;
            layersListBox.Enabled = false;
            solutionsListBox.Items.Clear();
            solutionsPanel.Width = 250;

        }*/
        private void E_OnTaskDone(ITask T)
        {
            //int i = 0;
        }
        private void E_OnAllTasksDone(ITask T)
        {
            if (layersListBox.InvokeRequired)
            {
                AllTasksDoneCallback d = new AllTasksDoneCallback(E_OnAllTasksDone);
                if (!this.Disposing)
                    this.Invoke(d, new object[] { T });
            }
            else
            {
                layersListBox.Enabled = true;
                addToolStripMenuItem.Enabled = true;
                sPNetToolStripMenuItem.Enabled = true;
                finishAnalysisToolStripMenuItem.Enabled = false;
                fileToolStripMenuItem.Enabled = true;
                MessageBox.Show("All done!");
                //this.Close();
            }
        }
        public void E_AddSolution(Solution s)
        {
            lock (syncRoot)
            {
                solutions.Add(s);
                solutions.Sort();
                refreshSolutions();
            }
        }
        public void E_ProgressChanged(double progress)
        {
            if (tasksProgressBar.InvokeRequired)
            {
                RefreshProgressBarCallback d = new RefreshProgressBarCallback(E_ProgressChanged);
                if (!Disposing)
                    this.Invoke(d, new object[] { progress });
            }
            else
            {
                tasksProgressBar.Value = Convert.ToInt32(progress * 100);
            }
            //double Progress = progress;
        }
        private void addFullRound(bool sameBlocks = true)
        {
            var PBlockInit = new List<byte> { 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 16 };
            var SBlockInit = new List<byte> { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 };

            addKLayer();
            addSLayer();
            addPLayer();
            EditSBlock d = new EditSBlock(currentSettings.SBoxSize, PBlockInit, true);
            if (d.ShowDialog() == DialogResult.OK)
            {
                PBlockInit = d.Value;

            }
            else
            {
                DelLastRound(net);
                return;
            }
            int layer = net.GetLayers().Count - 1;
            InitPLayer(layer, PBlockInit);

            if (sameBlocks)
            {
                d = new EditSBlock(currentSettings.SBoxSize, SBlockInit, false);
                if (d.ShowDialog() == DialogResult.OK)
                {
                    SBlockInit = d.Value;
                    InitSLayer(layer - 1, SBlockInit);
                }
                else
                {
                    DelLastRound(net);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста введите заполнение для " + currentSettings.SBoxCount + " S-Блоков\n" + "В случае отмены ввода хотя-бы одного блока создание слоя будет отменено");
                for (int i = 0; i < currentSettings.SBoxCount; i++)
                {
                    var B = net.GetLayers()[layer].GetBlocks()[i];
                    d = new EditSBlock(currentSettings.SBoxSize, new List<byte>());
                    if (d.ShowDialog() == DialogResult.OK)
                    {
                        B.Init(d.Value);
                    }
                    else
                    {
                        DelLastRound(net);
                        return;
                    }
                }
            }
            //refreshList();

        }
        private void addLastRound()
        {
            addKLayer();
            addSLayer();
            addKLayer();
            refreshLayers();
        }

        private void addRound()
        {
            /*if (net.GetLayers().Count >= 6)
                DelLastRound(net);*/
            addFullRound();
            //AddLastRound(net);
            refreshLayers();

        }

        private void InitPLayer(int L, List<byte> PBlockInit)
        {
            net.GetLayers()[L].GetBlocks()[0].Init(PBlockInit);
            SPNetGraph.layers[L].blocks[0].init_sequence = PBlockInit;
        }
        private void InitSLayer(int L, List<byte> SBlockInit)
        { 
            for (int i = 0; i < net.GetLayers()[L].GetBlocks().Count; i++)
            {
                var B = net.GetLayers()[L].GetBlocks()[i];
                B.Init(SBlockInit);
                SPNetGraph.layers[L].blocks[i].init_sequence = SBlockInit;
            }
        }
        static void AddRound(Allax.ISPNet Net)
        {
            DelLastRound(Net);
            AddFullRound(Net);
            AddLastRound(Net);
        }
        static void AddFullRound(Allax.ISPNet Net)
        {
            Net.AddLayer(Allax.LayerType.KLayer);
            Net.AddLayer(Allax.LayerType.SLayer);
            Net.AddLayer(Allax.LayerType.PLayer);
        }
        static void AddLastRound(Allax.ISPNet Net)
        {
            Net.AddLayer(Allax.LayerType.KLayer);
            Net.AddLayer(Allax.LayerType.SLayer);
            Net.AddLayer(Allax.LayerType.KLayer);
        }
        static void DelLastRound(Allax.ISPNet Net)
        {
            Net.DeleteLayer((byte)(Net.GetLayers().Count-1));
            Net.DeleteLayer((byte)(Net.GetLayers().Count - 1));
            Net.DeleteLayer((byte)(Net.GetLayers().Count - 1));
        }

        private void addPLayer()
        {
            net.AddLayer(Allax.LayerType.PLayer);
            SPNetGraph.addPLayer();
        }
        private void addSLayer()
        {
            net.AddLayer(Allax.LayerType.SLayer);
            SPNetGraph.addSLayer();
        }
        private void addKLayer()
        {
            net.AddLayer(Allax.LayerType.KLayer);
            SPNetGraph.addKLayer();
        }

        private void refreshLayers()
        {
            layerList.Clear();
            foreach (ILayer l in net.GetLayers())
            {
                layerList.Add(l.GetLayerType().ToString());
            }
            layersListBox.Items.Clear();
            layersListBox.Items.AddRange(layerList.ToArray());
        }

        private void refreshSolutions()
        {
            if (solutionsListBox.InvokeRequired)
            {
                RefreshSolutionsCallback d = new RefreshSolutionsCallback(refreshSolutions);
                if (!this.Disposing)
                    this.Invoke(d);
            }
            else
            {
                List<string> items = new List<string>();
                foreach (var s in solutions)
                {
                    //if (items.Contains(s.ToString()))
                    //    continue;
                    items.Add(s.ToString());
                }
                solutionsListBox.Items.Clear();
                solutionsListBox.Items.AddRange(items.ToArray());
                //Magical constant that makes the width similar to the actual width of a string
                /*int longestLength = (int)(solutionsListBox.Items[solutionsListBox.Items.Count - 1].ToString().Length * (solutionsListBox.Font.SizeInPoints - 2.75));//*0.72));
                if (solutionsListBox.Size.Width < longestLength)
                    solutionsPanel.Width = longestLength;*/
            }
        }


        private void sLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sPNetToolStripMenuItem.Enabled = true;
            addRound();
        }
        
        private void runAnalysis()
        {
            var R1 = new Allax.Rule(AvailableSolverTypes.BruteforceSolver, 2, 2);
            var R2 = new Allax.Rule(AvailableSolverTypes.GreedySolver, 2, 2);
            var R3 = new Allax.Rule(AvailableSolverTypes.AdvancedSolver, 2, 2);
            //var F = new Allax.ADDSOLUTIONHANDLER(AddSolution);
            var AP = new AnalisysParams(new Algorithm(new List<Allax.Rule> { R1, R2, R3}, AnalisysType.Linear));
            //DelLastRound(net);
            addLastRound();
            eng.PerformAnalisys(AP);  
        }

        private void sPNetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runAnalysis();
            //toolStrip1.Enabled = false;
            //menuStrip1.Enabled = false;
            addToolStripMenuItem.Enabled = false;
            sPNetToolStripMenuItem.Enabled = false;
            finishAnalysisToolStripMenuItem.Enabled = true;
            fileToolStripMenuItem.Enabled = false;
            layersListBox.Enabled = false;
            solutionsListBox.Items.Clear();
            solutionsPanel.Width = 250;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.eng.AbortAnalisys();
            Thread.Sleep(3000);
        }
        
        private void createSPNet(byte wordLength, byte sBlockSize)
        {
            tableLayoutPanel1.Controls.RemoveByKey("AllaxPanel");
            SPNetSettings settings = new SPNetSettings(wordLength, sBlockSize);
            currentSettings = settings;
            net = eng.CreateSPNetInstance(settings);
            solutions.Clear();
            layerList.Clear();
            refreshLayers();
            refreshSolutions();
            SPNetGraph = new AllaxPanel(settings.WordLength, settings.SBoxCount, 24);
            SPNetGraph.Dock = DockStyle.Fill;
            SPNetGraph.Parent = tableLayoutPanel1;
            SPNetGraph.Name = "AllaxPanel";
            //SPNetGraph.BackColor = System.Drawing.Color.Aquamarine;
            tableLayoutPanel1.Controls.Add(SPNetGraph, 1, 0);
            
        }

        private void createNetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNetDialog d = new CreateNetDialog();
            d.ShowDialog();
            if (d.DialogResult == DialogResult.OK)
            {
                createSPNet(d.wordLength, d.sBlockSize);
                //addFullRound();
                //addLastRound();
            }
        }

        private void saveNetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fs = File.OpenWrite("sb.db");
            eng.SerializeDB(fs, false);
            fs.Close();
            SaveFileDialog fd = new SaveFileDialog
            {
                Filter = "Файл SP-сети (*.spnet) | *.spnet",
                
                Title = "Сохранить сеть"
            };
            
            if (fd.ShowDialog() == DialogResult.OK)
            {
                fs = File.OpenWrite(fd.FileName);
                PanelSerializator.Serialize(SPNetGraph, fs);
                fs.Close();
            }

        }

        private void loadNetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists("sb.db"))
            {
                var fs = File.OpenRead("sb.db");
                eng.InjectSBlockDB(fs, false);
                fs.Close();
            }
            
            OpenFileDialog fd = new OpenFileDialog
            {
                Filter = "Файл SP-сети (*.spnet) | *.spnet",

                Title = "Загрузить сеть"
            };

            if (fd.ShowDialog() == DialogResult.OK)
            {
                var fs = File.OpenRead(fd.FileName);
                var data = PanelSerializator.DeSerialize(fs);
                LoadNet(data);
                fs.Close();
            }
        }

        private void LoadNet(PanelSerializator.PanelData data)
        {
            //createSPNet(data.wordsize, data)
        }

        private void finishAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            eng.AbortAnalisys();
            Thread.Sleep(1000);
            addToolStripMenuItem.Enabled = true;
            sPNetToolStripMenuItem.Enabled = true;
            finishAnalysisToolStripMenuItem.Enabled = false;
            fileToolStripMenuItem.Enabled = true;
            layersListBox.Enabled = true;
        }

        private void solutionsListBox_DoubleClick(object sender, EventArgs e)
        {
            if (solutionsListBox.SelectedItem != null)
            {
                var s = solutions[solutionsListBox.SelectedIndex];
                for(int i = 0; i < s.Way.layers.Count; i++)
                {
                    var l = s.Way.layers[i];
                    for (int j = 0; j < l.blocks.Count; j++)
                    {
                        var b = l.blocks[j];
                        if (b.Inputs != 0)
                        {
                            SPNetGraph.layers[i].blocks[j].BackColor = System.Drawing.Color.Red;
                            //SPNetGraph.layers[i].blocks[j].
                        }
                        else
                        {
                            SPNetGraph.layers[i].blocks[j].BackColor = System.Drawing.Color.WhiteSmoke;
                        }
                    }
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
