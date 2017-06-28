namespace FormsGUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createNetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveNetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadNetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sLayerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bruteForceRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.specificRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sPNetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.linearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.differrentialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.finishAnalysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tasksProgressBar = new System.Windows.Forms.ProgressBar();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.layersListBox = new System.Windows.Forms.ListBox();
            this.solutionsPanel = new System.Windows.Forms.Panel();
            this.solutionsListBox = new System.Windows.Forms.ListBox();
            this.addRuleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьСкриншотСетиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.solutionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.addToolStripMenuItem,
            this.analysisToolStripMenuItem,
            this.сохранитьСкриншотСетиToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1264, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createNetToolStripMenuItem,
            this.saveNetToolStripMenuItem,
            this.loadNetToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.fileToolStripMenuItem.Text = "Файл";
            // 
            // createNetToolStripMenuItem
            // 
            this.createNetToolStripMenuItem.Name = "createNetToolStripMenuItem";
            this.createNetToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.createNetToolStripMenuItem.Text = "Создать SP-Сеть";
            this.createNetToolStripMenuItem.Click += new System.EventHandler(this.createNetToolStripMenuItem_Click);
            // 
            // saveNetToolStripMenuItem
            // 
            this.saveNetToolStripMenuItem.Name = "saveNetToolStripMenuItem";
            this.saveNetToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.saveNetToolStripMenuItem.Text = "Сохранить Сеть";
            this.saveNetToolStripMenuItem.Click += new System.EventHandler(this.saveNetToolStripMenuItem_Click);
            // 
            // loadNetToolStripMenuItem
            // 
            this.loadNetToolStripMenuItem.Name = "loadNetToolStripMenuItem";
            this.loadNetToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.loadNetToolStripMenuItem.Text = "Загрузить Сеть";
            this.loadNetToolStripMenuItem.Click += new System.EventHandler(this.loadNetToolStripMenuItem_Click);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sLayerToolStripMenuItem,
            this.addRuleToolStripMenuItem,
            this.clearRulesToolStripMenuItem});
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.addToolStripMenuItem.Text = "Добавить";
            // 
            // sLayerToolStripMenuItem
            // 
            this.sLayerToolStripMenuItem.Name = "sLayerToolStripMenuItem";
            this.sLayerToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.sLayerToolStripMenuItem.Text = "Раунд";
            this.sLayerToolStripMenuItem.Click += new System.EventHandler(this.sLayerToolStripMenuItem_Click);
            // 
            // addRuleToolStripMenuItem
            // 
            this.addRuleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bruteForceRuleToolStripMenuItem,
            this.specificRuleToolStripMenuItem});
            this.addRuleToolStripMenuItem.Name = "addRuleToolStripMenuItem";
            this.addRuleToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.addRuleToolStripMenuItem.Text = "Правило";
            // 
            // bruteForceRuleToolStripMenuItem
            // 
            this.bruteForceRuleToolStripMenuItem.Name = "bruteForceRuleToolStripMenuItem";
            this.bruteForceRuleToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.bruteForceRuleToolStripMenuItem.Text = "Полный Перебор";
            // 
            // specificRuleToolStripMenuItem
            // 
            this.specificRuleToolStripMenuItem.Name = "specificRuleToolStripMenuItem";
            this.specificRuleToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.specificRuleToolStripMenuItem.Text = "Конкретный Вход";
            // 
            // analysisToolStripMenuItem
            // 
            this.analysisToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sPNetToolStripMenuItem,
            this.finishAnalysisToolStripMenuItem});
            this.analysisToolStripMenuItem.Name = "analysisToolStripMenuItem";
            this.analysisToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.analysisToolStripMenuItem.Text = "Анализ";
            // 
            // sPNetToolStripMenuItem
            // 
            this.sPNetToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.linearToolStripMenuItem,
            this.differrentialToolStripMenuItem});
            this.sPNetToolStripMenuItem.Enabled = false;
            this.sPNetToolStripMenuItem.Name = "sPNetToolStripMenuItem";
            this.sPNetToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.sPNetToolStripMenuItem.Text = "Начать";
            // 
            // linearToolStripMenuItem
            // 
            this.linearToolStripMenuItem.Name = "linearToolStripMenuItem";
            this.linearToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.linearToolStripMenuItem.Text = "Линейный";
            this.linearToolStripMenuItem.Click += new System.EventHandler(this.linearToolStripMenuItem_Click);
            // 
            // differrentialToolStripMenuItem
            // 
            this.differrentialToolStripMenuItem.Name = "differrentialToolStripMenuItem";
            this.differrentialToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.differrentialToolStripMenuItem.Text = "Дифференциальный";
            this.differrentialToolStripMenuItem.Click += new System.EventHandler(this.differrentialToolStripMenuItem_Click);
            // 
            // finishAnalysisToolStripMenuItem
            // 
            this.finishAnalysisToolStripMenuItem.Enabled = false;
            this.finishAnalysisToolStripMenuItem.Name = "finishAnalysisToolStripMenuItem";
            this.finishAnalysisToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.finishAnalysisToolStripMenuItem.Text = "Прервать";
            this.finishAnalysisToolStripMenuItem.Click += new System.EventHandler(this.finishAnalysisToolStripMenuItem_Click);
            // 
            // tasksProgressBar
            // 
            this.tasksProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tasksProgressBar.Location = new System.Drawing.Point(0, 712);
            this.tasksProgressBar.Margin = new System.Windows.Forms.Padding(0);
            this.tasksProgressBar.Name = "tasksProgressBar";
            this.tasksProgressBar.Size = new System.Drawing.Size(278, 20);
            this.tasksProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.tasksProgressBar.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 800F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.layersListBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.solutionsPanel, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 738);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // layersListBox
            // 
            this.layersListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.layersListBox.FormattingEnabled = true;
            this.layersListBox.IntegralHeight = false;
            this.layersListBox.Location = new System.Drawing.Point(3, 3);
            this.layersListBox.Name = "layersListBox";
            this.layersListBox.Size = new System.Drawing.Size(174, 732);
            this.layersListBox.TabIndex = 0;
            // 
            // solutionsPanel
            // 
            this.solutionsPanel.Controls.Add(this.solutionsListBox);
            this.solutionsPanel.Controls.Add(this.tasksProgressBar);
            this.solutionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.solutionsPanel.Location = new System.Drawing.Point(983, 3);
            this.solutionsPanel.Name = "solutionsPanel";
            this.solutionsPanel.Size = new System.Drawing.Size(278, 732);
            this.solutionsPanel.TabIndex = 6;
            // 
            // solutionsListBox
            // 
            this.solutionsListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.solutionsListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.solutionsListBox.FormattingEnabled = true;
            this.solutionsListBox.IntegralHeight = false;
            this.solutionsListBox.Location = new System.Drawing.Point(0, 0);
            this.solutionsListBox.Name = "solutionsListBox";
            this.solutionsListBox.Size = new System.Drawing.Size(278, 712);
            this.solutionsListBox.TabIndex = 13;
            this.solutionsListBox.DoubleClick += new System.EventHandler(this.solutionsListBox_DoubleClick);
            // 
            // сохранитьСкриншотСетиToolStripMenuItem
            // 
            this.addRuleToolStripMenuItem.Name = "addRuleToolStripMenuItem";
            this.addRuleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addRuleToolStripMenuItem.Text = "Правило";
            this.addRuleToolStripMenuItem.Click += new System.EventHandler(this.addRuleToolStripMenuItem_Click);
            // 
            // clearRulesToolStripMenuItem
            // 
            this.clearRulesToolStripMenuItem.Name = "clearRulesToolStripMenuItem";
            this.clearRulesToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.clearRulesToolStripMenuItem.Text = "Удалить Правила";
            this.clearRulesToolStripMenuItem.Click += new System.EventHandler(this.clearRulesToolStripMenuItem_Click);
            this.сохранитьСкриншотСетиToolStripMenuItem.Name = "сохранитьСкриншотСетиToolStripMenuItem";
            this.сохранитьСкриншотСетиToolStripMenuItem.Size = new System.Drawing.Size(163, 20);
            this.сохранитьСкриншотСетиToolStripMenuItem.Text = "Сохранить скриншот сети";
            this.сохранитьСкриншотСетиToolStripMenuItem.Click += new System.EventHandler(this.сохранитьСкриншотСетиToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 762);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Анализ SP-Сетей";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.solutionsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sLayerToolStripMenuItem;
        private System.Windows.Forms.ProgressBar tasksProgressBar;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListBox layersListBox;
        private System.Windows.Forms.Panel solutionsPanel;
        private System.Windows.Forms.ListBox solutionsListBox;
        private System.Windows.Forms.ToolStripMenuItem createNetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem analysisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sPNetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem finishAnalysisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveNetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadNetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem linearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem differrentialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addRuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearRulesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bruteForceRuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem specificRuleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сохранитьСкриншотСетиToolStripMenuItem;
    }
}

