using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using Allax;
using System.Threading;

namespace FormsGUI
{
    public partial class MainForm : Form
    {
        
        Engine e;
        ISPNet net;
        List<string> layerList = new List<string>();
        List<Solution> solutions = new List<Solution>();
        delegate void RefreshSolutionsCallback();
        delegate void RefreshProgressBarCallback(double progress);
        private static object syncRoot = new object();
        public MainForm()
        {
            InitializeComponent();
            e = new Engine();
            e.ONSOLUTIONFOUND += this.E_AddSolution;
            e.ONPROGRESSCHANGED += this.E_ProgressChanged;
            e.ONTASKDONE += E_OnTaskDone;
            e.ONALLTASKSDONE += E_OnAllTasksDone;
            tasksProgressBar.Width = solutionsListBox.Size.Width;
            SPNetSettings settings = new SPNetSettings(16, 4);
            net = e.CreateSPNetInstance(settings);
            addFullRound();
            addLastRound();
        }

        private void E_OnTaskDone(ITask T)
        {
            //int i = 0;
        }
        private void E_OnAllTasksDone(ITask T)
        {
            MessageBox.Show("All done!");
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
                this.Invoke(d, new object[] { progress });
            }
            else
            {
                tasksProgressBar.Value = Convert.ToInt32(progress * 100);
            }
            //double Progress = progress;
        }
        private void addFullRound()
        {
            net.AddLayer(LayerType.KLayer);
            net.AddLayer(LayerType.SLayer);
            net.AddLayer(LayerType.PLayer);
            var PBlockInit = new List<byte> { 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 16 };
            var SBlockInit = new List<byte> { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 };
            int layer = net.GetLayers().Count - 1;
            InitPLayer(layer, PBlockInit, net);
            InitSLayer(layer - 1, SBlockInit, net);
            //refreshList();

        }
        private void addLastRound()
        {
            net.AddLayer(LayerType.KLayer);
            net.AddLayer(LayerType.SLayer);
            net.AddLayer(LayerType.KLayer);
            refreshLayers();
        }

        private void addRound()
        {
            DelLastRound(net);
            addFullRound();
            addLastRound();
        }

        static void InitPLayer(int L, List<byte> PBlockInit, ISPNet Net)
        {
            Net.GetLayers()[L].GetBlocks()[0].Init(PBlockInit);
        }
        static void InitSLayer(int L, List<byte> SBlockInit, ISPNet Net)
        { 
            foreach (var B in Net.GetLayers()[L].GetBlocks())
            {
                B.Init(SBlockInit);
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
                int longestLength = (int)(solutionsListBox.Items[solutionsListBox.Items.Count - 1].ToString().Length * (solutionsListBox.Font.SizeInPoints - 2.75));//*0.72));
                if (solutionsListBox.Size.Width < longestLength)
                    solutionsPanel.Width = longestLength;
            }
        }


        private void sLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditSBlock f = new EditSBlock();
            //f.ShowDialog();
            addRound();
            //int size = net.GetLayers().Count;
            /*foreach (IBlock b in net.GetLayers()[size - 2].GetBlocks())
            {
                b.Init(f.SValues);
            }*/
        }
        
        private void runAnalysis()
        {
            var R1 = new Allax.Rule(AvailableSolverTypes.BruteforceSolver, 2, 2);
            var R2 = new Allax.Rule(AvailableSolverTypes.GreedySolver, 2, 2);
            var R3 = new Allax.Rule(AvailableSolverTypes.AdvancedSolver, 2, 2);
            //var F = new Allax.ADDSOLUTIONHANDLER(AddSolution);
            var AP = new AnalisysParams(new Algorithm(new List<Allax.Rule> { R1, R2, R3}, AnalisysType.Linear));
            e.PerformAnalisys(AP);  
        }

        private void sPNetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runAnalysis();
            solutionsListBox.Items.Clear();
            solutionsPanel.Width = 250;
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addRound();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.e.AbortAnalisys();
            Thread.Sleep(2000);
        }
    }
}
