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
            this.clearRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sPNetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.linearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.differrentialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.finishAnalysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьСкриншотСетиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tasksProgressBar = new System.Windows.Forms.ProgressBar();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.solutionsPanel = new System.Windows.Forms.Panel();
            this.solutionsListBox = new System.Windows.Forms.ListBox();
            this.rulesTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.removeDuplicateSolutionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.sLayerToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.sLayerToolStripMenuItem.Text = "Раунд";
            this.sLayerToolStripMenuItem.Click += new System.EventHandler(this.sLayerToolStripMenuItem_Click);
            // 
            // addRuleToolStripMenuItem
            // 
            this.addRuleToolStripMenuItem.Name = "addRuleToolStripMenuItem";
            this.addRuleToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.addRuleToolStripMenuItem.Text = "Правило";
            this.addRuleToolStripMenuItem.Click += new System.EventHandler(this.addRuleToolStripMenuItem_Click);
            // 
            // clearRulesToolStripMenuItem
            // 
            this.clearRulesToolStripMenuItem.Name = "clearRulesToolStripMenuItem";
            this.clearRulesToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.clearRulesToolStripMenuItem.Text = "Удалить Правила";
            this.clearRulesToolStripMenuItem.Click += new System.EventHandler(this.clearRulesToolStripMenuItem_Click);
            // 
            // analysisToolStripMenuItem
            // 
            this.analysisToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sPNetToolStripMenuItem,
            this.finishAnalysisToolStripMenuItem,
            this.removeDuplicateSolutionsToolStripMenuItem});
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
            // сохранитьСкриншотСетиToolStripMenuItem
            // 
            this.сохранитьСкриншотСетиToolStripMenuItem.Name = "сохранитьСкриншотСетиToolStripMenuItem";
            this.сохранитьСкриншотСетиToolStripMenuItem.Size = new System.Drawing.Size(163, 20);
            this.сохранитьСкриншотСетиToolStripMenuItem.Text = "Сохранить скриншот сети";
            this.сохранитьСкриншотСетиToolStripMenuItem.Click += new System.EventHandler(this.сохранитьСкриншотСетиToolStripMenuItem_Click);
            // 
            // tasksProgressBar
            // 
            this.tasksProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tasksProgressBar.Location = new System.Drawing.Point(0, 691);
            this.tasksProgressBar.Margin = new System.Windows.Forms.Padding(0);
            this.tasksProgressBar.Name = "tasksProgressBar";
            this.tasksProgressBar.Size = new System.Drawing.Size(278, 20);
            this.tasksProgressBar.TabIndex = 5;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 800F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.solutionsPanel, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.rulesTableLayoutPanel, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 717);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // solutionsPanel
            // 
            this.solutionsPanel.Controls.Add(this.solutionsListBox);
            this.solutionsPanel.Controls.Add(this.tasksProgressBar);
            this.solutionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.solutionsPanel.Location = new System.Drawing.Point(1003, 3);
            this.solutionsPanel.Name = "solutionsPanel";
            this.solutionsPanel.Size = new System.Drawing.Size(278, 711);
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
            this.solutionsListBox.Size = new System.Drawing.Size(278, 691);
            this.solutionsListBox.TabIndex = 13;
            this.solutionsListBox.DoubleClick += new System.EventHandler(this.solutionsListBox_DoubleClick);
            // 
            // rulesTableLayoutPanel
            // 
            this.rulesTableLayoutPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.rulesTableLayoutPanel.ColumnCount = 1;
            this.rulesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rulesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.rulesTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rulesTableLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.rulesTableLayoutPanel.Name = "rulesTableLayoutPanel";
            this.rulesTableLayoutPanel.RowCount = 2;
            this.rulesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rulesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rulesTableLayoutPanel.Size = new System.Drawing.Size(194, 711);
            this.rulesTableLayoutPanel.TabIndex = 7;
            // 
            // removeDuplicateSolutionsToolStripMenuItem
            // 
            this.removeDuplicateSolutionsToolStripMenuItem.Checked = true;
            this.removeDuplicateSolutionsToolStripMenuItem.CheckOnClick = true;
            this.removeDuplicateSolutionsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.removeDuplicateSolutionsToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.removeDuplicateSolutionsToolStripMenuItem.Name = "removeDuplicateSolutionsToolStripMenuItem";
            this.removeDuplicateSolutionsToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.removeDuplicateSolutionsToolStripMenuItem.Text = "Убирать одинаковые преобладания";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 741);
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
        private System.Windows.Forms.ToolStripMenuItem сохранитьСкриншотСетиToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel rulesTableLayoutPanel;
        private System.Windows.Forms.ToolStripMenuItem removeDuplicateSolutionsToolStripMenuItem;
    }
}

