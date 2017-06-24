using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Allax;

namespace FormsGUI
{
    public partial class MainForm : Form
    {
        
        Engine e;
        ISPNet net;
        List<string> layerList = new List<string>();
        public MainForm()
        {
            var F = new Allax.ADDSOLUTIONHANDLER(AddSolution);
            e = new Engine(new EngineSettings(F));
            InitializeComponent();
            SPNetSettings settings = new SPNetSettings(16, 4);
            //settings.sblock_count = 4;
            //settings.word_length = 16;
            //settings.db = e.GetSBlockDBInstance();
            net = e.GetSPNetInstance();
            //addRound();
            //addLastRound();
        }

        private bool AddSolution(Solution s)
        {
            return true;
        }

        private void addRound()
        {
            net.AddLayer(LayerType.KLayer);
            net.AddLayer(LayerType.SLayer);
            net.AddLayer(LayerType.PLayer);
            var PBlockInit = new List<byte> { 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 16 };
            net.GetLayers()[net.GetLayers().Count - 1].GetBlocks()[0].Init(PBlockInit);
            refreshList();

        }
        private void addLastRound()
        {
            net.AddLayer(LayerType.KLayer);
            net.AddLayer(LayerType.SLayer);
            net.AddLayer(LayerType.KLayer);
            refreshList();
        }

        private void refreshList()
        {
            layerList.Clear();
            foreach (ILayer l in net.GetLayers())
            {
                layerList.Add(l.GetLayerType().ToString());
            }
            layersListBox.Items.Clear();
            layersListBox.Items.AddRange(layerList.ToArray());
        }

        private void sLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditSBlock f = new EditSBlock();
            f.ShowDialog();
            addRound();
            int size = net.GetLayers().Count;
            foreach (IBlock b in net.GetLayers()[size - 2].GetBlocks())
            {
                b.Init(f.SValues);
            }
        }
        
        private void runAnalysis()
        {
            var R1 = new Allax.Rule(AvailableSolverTypes.BaseSolver);
            var R2 = new Allax.Rule(AvailableSolverTypes.HeuristicSolver);
            var F = new Allax.ADDSOLUTIONHANDLER(AddSolution);
            //var AP = new AnalisysParams(new Algorithm(new List<Allax.Rule> { R1, R2 }, AnalisysType.Linear), F);
            //net.PerformAnalisys(AP);
        }

        private void sPNetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runAnalysis();
        }
    }
}
