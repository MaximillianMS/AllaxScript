﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using Allax;
using AllaxForm;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization.Formatters.Binary;

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
        List<Rule> activeRules = new List<Rule>();
        bool isLastRoundAdded = false;
        bool analysisActive = false;
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
            if (this.InvokeRequired)
            {
                AllTasksDoneCallback d = new AllTasksDoneCallback(E_OnAllTasksDone);
                try
                {
                    this.Invoke(d, new object[] { T });
                }
                catch(ObjectDisposedException)
                {
                    ;
                }
            }
            else
            {
                //rulesListBox.Enabled = true;
                addToolStripMenuItem.Enabled = true;
                sPNetToolStripMenuItem.Enabled = true;
                finishAnalysisToolStripMenuItem.Enabled = false;
                fileToolStripMenuItem.Enabled = true;
                MessageBox.Show("All done!");
                analysisActive = false;
                removeDuplicateSolutionsToolStripMenuItem.Enabled = true;
                //this.Close();
            }
        }
        public void E_AddSolution(Solution s)
        {
            lock (syncRoot)
            {
                if ((!removeDuplicateSolutionsToolStripMenuItem.Checked) || (!solutions.Contains(s, new SolutionEqualityComparer())))
                {
                    solutions.Add(s);
                    solutions.Sort();
                    refreshSolutions();
                }
            }
        }
        public void E_ProgressChanged(double progress)
        {
            if (tasksProgressBar.InvokeRequired)
            {
                RefreshProgressBarCallback d = new RefreshProgressBarCallback(E_ProgressChanged);
                try
                {
                    this.Invoke(d, new object[] { progress });
                }
                catch (ObjectDisposedException)
                {
                    ;
                }
            }
            else
            {
                tasksProgressBar.Value = Convert.ToInt32(progress * 100);
            }
            //double Progress = progress;
        }
        private bool addFullRound(bool sameBlocks = true)
        {
            var PBlockInit = new List<byte> { 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15, 4, 8, 12, 16 };
            var SBlockInit = new List<byte> { 14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7 };

            addKLayer();
            addSLayer();
            addPLayer();
            EditSBlock d = new EditSBlock(currentSettings.WordLength, PBlockInit, true);
            if (d.ShowDialog() == DialogResult.OK)
            {
                PBlockInit = d.Value;

            }
            else
            {
                DelLastRound();
                return false;
            }
            int layer = net.GetLayers().Count - 1;
            InitPLayer(layer, PBlockInit);
            sameBlocks = MessageBox.Show("Использовать различные S-Блоки в данном раунде?", "Различные S-Блоки?", MessageBoxButtons.YesNo) != DialogResult.Yes;
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
                    DelLastRound();
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста введите заполнение для " + currentSettings.SBoxCount + " S-Блоков\n" + "В случае отмены ввода хотя-бы одного блока создание слоя будет отменено");
                for (int i = 0; i < currentSettings.SBoxCount; i++)
                {
                    var B = net.GetLayers()[layer - 1].GetBlocks()[i];
                    d = new EditSBlock(currentSettings.SBoxSize, new List<byte>());
                    if (d.ShowDialog() == DialogResult.OK)
                    {
                        B.Init(d.Value);
                        SPNetGraph.layers[layer - 1].blocks[i].init_sequence = d.Value;
                    }
                    else
                    {
                        DelLastRound();
                        return false;
                    }
                }
            }
            return true;
            //refreshList();

        }
        private void addLastRound()
        {
            addKLayer();
            addSLayer();
            addKLayer();
            isLastRoundAdded = true;
            refreshLayers();
        }

        private bool addRound()
        {
            if (isLastRoundAdded)
                DelLastRound();
            var ret = addFullRound();
            refreshLayers();
            return ret;

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
        private void DelLastRound()
        {
            net.DeleteLayer((byte)(net.GetLayers().Count - 1));
            net.DeleteLayer((byte)(net.GetLayers().Count - 1));
            net.DeleteLayer((byte)(net.GetLayers().Count - 1));
            SPNetGraph.removeLayer();
            SPNetGraph.removeLayer();
            SPNetGraph.removeLayer();
            isLastRoundAdded = false;
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
            //rulesListBox.Items.Clear();
            //rulesListBox.Items.AddRange(layerList.ToArray());
        }

        private void refreshSolutions()
        {
            if (solutionsListBox.InvokeRequired)
            {
                RefreshSolutionsCallback d = new RefreshSolutionsCallback(refreshSolutions);
                try
                {
                    this.Invoke(d);
                }
                catch (ObjectDisposedException)
                {
                    ;
                }
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
            if (addRound())
                sPNetToolStripMenuItem.Enabled = true;
        }
        
        private void runAnalysis(bool isDifferential = false)
        {
            if (activeRules.Count == 0)
            {
                activeRules.Add(new Allax.Rule(AvailableSolverTypes.BruteforceSolver, 2, 2));
                activeRules.Add(new Allax.Rule(AvailableSolverTypes.GreedySolver, 2, 2));
                refreshRules();
                
            }
            //var R3 = new Allax.Rule(AvailableSolverTypes.AdvancedSolver, 2, 2);
            //var F = new Allax.ADDSOLUTIONHANDLER(AddSolution);
            var AP = new AnalisysParams(new Algorithm(activeRules, AnalisysType.Linear));
            AP.Alg.Type = isDifferential ? AnalisysType.Differencial : AnalisysType.Linear;
            if (!isLastRoundAdded)
                addLastRound();
            analysisActive = true;
            eng.PerformAnalisys(AP);  
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.eng.AbortAnalisys();
            Thread.Sleep(1000);
        }
        
        private void createSPNet(byte wordLength, byte sBlockSize)
        {
            tableLayoutPanel1.Controls.RemoveByKey("AllaxPanel");
            SPNetSettings settings = new SPNetSettings(wordLength, sBlockSize);
            currentSettings = settings;
            net = eng.CreateSPNetInstance(settings);
            solutions.Clear();
            layerList.Clear();
            activeRules.Clear();
            isLastRoundAdded = false;
            refreshLayers();
            refreshSolutions();
            SPNetGraph = new AllaxPanel(settings.SBoxSize, settings.SBoxCount, 15, allaxBlock_DoubleClick);
            
            //SPNetGraph.DoubleClick += allaxBlock_DoubleClick;
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
                fs = File.OpenWrite(fd.FileName + "sol");
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, solutions);
                    stream.Flush();
                    stream.Position = 0;
                    var arr = stream.ToArray();
                    arr = Engine.Zip(Engine.Zip(arr));
                    fs.Write(arr, 0, arr.Length);
                }
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
                sPNetToolStripMenuItem.Enabled = true;
                fs.Close();
                if (File.Exists(fd.FileName + "sol"))
                {
                    fs = File.OpenRead(fd.FileName + "sol");
                    var arr = new byte[fs.Length - fs.Position];
                    fs.Read(arr, (int)fs.Position, arr.Length);
                    arr = Engine.Unzip(Engine.Unzip(arr));
                    using (var stream = new MemoryStream(arr))
                    {
                        var formatter = new BinaryFormatter();
                        solutions = (List<Solution>)formatter.Deserialize(stream);
                    }
                    refreshSolutions();
                }

            }
        }

        private void LoadNet(PanelSerializator.PanelData data)
        {
            createSPNet((byte)(data.wordsize * data.blocks_wide), (byte)data.wordsize);
            for (int i = 0; i < data.layers.Count(); i+=3)
            {
                if (data.layers[i + 2].type == AllaxBlock.BLOCK_TYPE.K)
                {
                    addLastRound();
                }
                else
                {
                    addKLayer();
                    addSLayer();
                    addPLayer();
                    var PBlockInit = data.layers[i + 2].blocks[0].Init;
                    InitPLayer(i + 2, PBlockInit);
                    for (int j = 0; j < data.layers[i + 1].blocks.Count; j++)
                    {
                        var SBlockInit = data.layers[i + 1].blocks[j].Init;
                        var B = net.GetLayers()[i + 1].GetBlocks()[j];
                        B.Init(SBlockInit);
                        SPNetGraph.layers[i + 1].blocks[j].init_sequence = SBlockInit;
                    }
                }
            }
            refreshLayers();
              
        }

        private void finishAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            eng.AbortAnalisys();
            Thread.Sleep(1000);
            analysisActive = false;
            addToolStripMenuItem.Enabled = true;
            sPNetToolStripMenuItem.Enabled = true;
            finishAnalysisToolStripMenuItem.Enabled = false;
            fileToolStripMenuItem.Enabled = true;
            removeDuplicateSolutionsToolStripMenuItem.Enabled = true;
            //rulesListBox.Enabled = true;
        }

        private void allaxBlock_DoubleClick(object sender, EventArgs e)
        {
            if (!analysisActive)
            {
                AllaxBlock b = (AllaxBlock)sender;
                if (b.type == AllaxBlock.BLOCK_TYPE.S)
                {
                    EditSBlock s = new EditSBlock(currentSettings.SBoxSize, b.init_sequence);
                    if (s.ShowDialog() == DialogResult.OK)
                        net.GetLayers()[b.layer_index].GetBlocks()[b.index_in_layer].Init(s.Value);
                }
                else if (b.type == AllaxBlock.BLOCK_TYPE.P)
                {
                    EditSBlock s = new EditSBlock(currentSettings.WordLength, b.init_sequence, true);
                    if (s.ShowDialog() == DialogResult.OK)
                        net.GetLayers()[b.layer_index].GetBlocks()[b.index_in_layer].Init(s.Value);
                }
            }
        }

        private void solutionsListBox_DoubleClick(object sender, EventArgs e)
        {
            if (solutionsListBox.SelectedItem != null)
            {
                var s = solutions[solutionsListBox.SelectedIndex];
                for(int i = 0; i < s.Way.layers.Count - 2; i++)
                {
                    var l = s.Way.layers[i];
                    List<bool> redConnectors = new List<bool>();
                    for (int j = 0; j < l.blocks.Count; j++)
                    {
                        var b = l.blocks[j];
                        redConnectors.AddRange(WayConverter.ToList(b.Outputs, b.BlockSize));
                        
                        if (b.Inputs != 0)
                        {
                            if (l.type != LayerType.PLayer)
                                SPNetGraph.layers[i].blocks[j].BackColor = System.Drawing.Color.Red;
                            else
                                SPNetGraph.layers[i].blocks[j].drawPBlockWeb(WayConverter.ToList(b.Inputs, b.BlockSize));
                                                        
                        }
                        else
                        {
                            SPNetGraph.layers[i].blocks[j].BackColor = System.Drawing.Color.WhiteSmoke;
                            SPNetGraph.layers[i].blocks[j].removePBlockWeb();
                        }
                    }
                    SPNetGraph.setLayerColors(i, redConnectors);
                    SPNetGraph.paint();
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void linearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            solutionsListBox.Items.Clear();
            solutions.Clear();
            runAnalysis(false);
            //toolStrip1.Enabled = false;
            //menuStrip1.Enabled = false;
            addToolStripMenuItem.Enabled = false;
            sPNetToolStripMenuItem.Enabled = false;
            finishAnalysisToolStripMenuItem.Enabled = true;
            fileToolStripMenuItem.Enabled = false;
            removeDuplicateSolutionsToolStripMenuItem.Enabled = false;
            //rulesListBox.Enabled = false;
            solutionsPanel.Width = 250;
        }

        private void differrentialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            solutionsListBox.Items.Clear();
            solutions.Clear();
            runAnalysis(true);
            //toolStrip1.Enabled = false;
            //menuStrip1.Enabled = false;
            addToolStripMenuItem.Enabled = false;
            sPNetToolStripMenuItem.Enabled = false;
            finishAnalysisToolStripMenuItem.Enabled = true;
            fileToolStripMenuItem.Enabled = false;
            removeDuplicateSolutionsToolStripMenuItem.Enabled = false;
            //rulesListBox.Enabled = false;
            solutionsPanel.Width = 250;
        }
        delegate void SaverFunc();
        private void Saver()
        {
            Thread.Sleep(500);
            Rectangle bounds = SPNetGraph.Bounds;
            bounds.Inflate(3, 3);
            var point = this.Location;
            point.Offset(SPNetGraph.Parent.Location);
            point.Offset(SPNetGraph.Location);
            point.Offset(6, this.menuStrip1.Height + 6);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(point, Point.Empty, bounds.Size);
                }
                SaveFileDialog d = new SaveFileDialog()
                { Filter = "PNG (*.PNG) | *.PNG",
                    Title = "Сохранить скриншот"};
                DialogResult res= DialogResult.None;
                var logicToInvokeInsideUIThread = new MethodInvoker(() =>
                { 

                    res = d.ShowDialog();
                    
                });

                if (InvokeRequired)
                {
                    Invoke(logicToInvokeInsideUIThread);
                }
                else
                {
                    logicToInvokeInsideUIThread.Invoke();
                }
                if (res==DialogResult.OK)
                {
                    bitmap.Save(d.FileName, ImageFormat.Png);
                }
            }
        }
        private void сохранитьСкриншотСетиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*WindowState = FormWindowState.Maximized;
            SaverFunc sf = new SaverFunc(Saver);
            sf.BeginInvoke(null, null);*/
            Bitmap bmp = new Bitmap(SPNetGraph.Width, SPNetGraph.Height);
            SPNetGraph.DrawToBitmap(bmp, new Rectangle(Point.Empty, SPNetGraph.Size));
            foreach (var l in SPNetGraph.layers)
            {
                foreach (var b in l.blocks)
                {
                    if (b.type == AllaxBlock.BLOCK_TYPE.P)
                    {
                        b.paint(bmp);
                    }
                }
            }
            SPNetGraph.paint(bmp);
            SaveFileDialog fd = new SaveFileDialog
            {
                Filter = "PNG (*.png) | *.png",

                Title = "Сохранить изображение сети"
            };

            if (fd.ShowDialog() == DialogResult.OK)
            {
                bmp.Save(fd.FileName, ImageFormat.Png);
            }
        }

        private void addRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateRuleDialog d = new CreateRuleDialog(currentSettings.SBoxCount, currentSettings.WordLength);
            if (d.ShowDialog() == DialogResult.OK)
            {
                activeRules.Add(d.rule);
            }
            refreshRules();
        }

        private void refreshRules()
        {
            rulesTableLayoutPanel.Controls.Clear();
            foreach (Rule r in activeRules)
            {
                var l = new Label();
                l.Text += "Тип решения: " + r.SolverType.ToString() + "\n";
                l.Text += "Max S-Блоков на слое:" + r.MaxActiveBlocksOnLayer + "\n";
                l.Text += r.UseCustomInput ? "Константный вход" : "Max S-блоков на входе" + r.MaxStartBlocks;
                l.Height = 40;
                l.Width = 200;
                rulesTableLayoutPanel.Controls.Add(l);
            }
        }

        private void clearRulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            activeRules.Clear();
            refreshRules();
        }
    }
}
