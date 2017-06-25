﻿using System;
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
        List<Solution> solutions = new List<Solution>();
        public MainForm()
        {
            InitializeComponent();
            e = new Engine();
            e.ONSOLUTIONFOUND += this.E_AddSolution;
            e.ONPROGRESSCHANGED += this.E_ProgressChanged;
            e.ONTASKDONE += E_OnTaskDone;
            e.ONALLTASKSDONE += E_OnAllTasksDone;
            
            SPNetSettings settings = new SPNetSettings(16, 4);
            net = e.CreateSPNetInstance(settings);
            addFullRound();
            addLastRound();
        }

        private void E_OnTaskDone(Allax.Task T)
        {
            //int i = 0;
        }
        private void E_OnAllTasksDone(Allax.Task T)
        {
            MessageBox.Show("All done!");
        }
        public void E_AddSolution(Solution s)
        {
            MessageBox.Show("Solution found " + s.P.ToPrevalence());
        }
        public void E_ProgressChanged(double progress)
        {
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
            refreshList();
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
            var R1 = new Allax.Rule(AvailableSolverTypes.BruteforceSolver);
            var R2 = new Allax.Rule(AvailableSolverTypes.GreedySolver);
            var R3 = new Allax.Rule(AvailableSolverTypes.AdvancedSolver);
            //var F = new Allax.ADDSOLUTIONHANDLER(AddSolution);
            var AP = new AnalisysParams(new Algorithm(new List<Allax.Rule> { R1, R2, R3}, AnalisysType.Linear));
            e.PerformAnalisys(AP);  
        }

        private void sPNetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runAnalysis();
            solutionsListBox.Items.Clear();
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addRound();
        }
    }
}