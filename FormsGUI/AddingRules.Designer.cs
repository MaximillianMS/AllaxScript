namespace FormsGUI
{
    partial class AddingRules
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
            this.bOK = new System.Windows.Forms.Button();
            this.cBAlg = new System.Windows.Forms.ComboBox();
            this.lSearchAlg = new System.Windows.Forms.Label();
            this.lRuleHeader = new System.Windows.Forms.Label();
            this.lMaxActive = new System.Windows.Forms.Label();
            this.nUDMaxActive = new System.Windows.Forms.NumericUpDown();
            this.cB = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.bNext = new System.Windows.Forms.Button();
            this.bPrev = new System.Windows.Forms.Button();
            this.lTotalRules = new System.Windows.Forms.Label();
            this.lMaxStart = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nUDMaxActive)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // bOK
            // 
            this.bOK.Location = new System.Drawing.Point(281, 223);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(75, 23);
            this.bOK.TabIndex = 0;
            this.bOK.Text = "ОК";
            this.bOK.UseVisualStyleBackColor = true;
            // 
            // cBAlg
            // 
            this.cBAlg.FormattingEnabled = true;
            this.cBAlg.Location = new System.Drawing.Point(130, 34);
            this.cBAlg.Name = "cBAlg";
            this.cBAlg.Size = new System.Drawing.Size(226, 21);
            this.cBAlg.TabIndex = 1;
            // 
            // lSearchAlg
            // 
            this.lSearchAlg.AutoSize = true;
            this.lSearchAlg.Location = new System.Drawing.Point(29, 37);
            this.lSearchAlg.Name = "lSearchAlg";
            this.lSearchAlg.Size = new System.Drawing.Size(95, 13);
            this.lSearchAlg.TabIndex = 2;
            this.lSearchAlg.Text = "Алгоритм поиска";
            // 
            // lRuleHeader
            // 
            this.lRuleHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lRuleHeader.AutoSize = true;
            this.lRuleHeader.Location = new System.Drawing.Point(127, 9);
            this.lRuleHeader.Name = "lRuleHeader";
            this.lRuleHeader.Size = new System.Drawing.Size(71, 13);
            this.lRuleHeader.TabIndex = 3;
            this.lRuleHeader.Text = "Правило №1";
            this.lRuleHeader.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lMaxActive
            // 
            this.lMaxActive.AutoSize = true;
            this.lMaxActive.Location = new System.Drawing.Point(29, 72);
            this.lMaxActive.Name = "lMaxActive";
            this.lMaxActive.Size = new System.Drawing.Size(277, 13);
            this.lMaxActive.TabIndex = 4;
            this.lMaxActive.Text = "Максимальное количество активных блоков на слой";
            // 
            // nUDMaxActive
            // 
            this.nUDMaxActive.Location = new System.Drawing.Point(312, 70);
            this.nUDMaxActive.Name = "nUDMaxActive";
            this.nUDMaxActive.Size = new System.Drawing.Size(44, 20);
            this.nUDMaxActive.TabIndex = 5;
            this.nUDMaxActive.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cB
            // 
            this.cB.AutoSize = true;
            this.cB.Location = new System.Drawing.Point(32, 126);
            this.cB.Name = "cB";
            this.cB.Size = new System.Drawing.Size(297, 17);
            this.cB.TabIndex = 6;
            this.cB.Text = "Использовать определенные биты открытого текста";
            this.cB.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(32, 168);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(324, 20);
            this.textBox1.TabIndex = 7;
            // 
            // bNext
            // 
            this.bNext.Location = new System.Drawing.Point(231, 194);
            this.bNext.Name = "bNext";
            this.bNext.Size = new System.Drawing.Size(75, 23);
            this.bNext.TabIndex = 8;
            this.bNext.Text = "Следующее";
            this.bNext.UseVisualStyleBackColor = true;
            // 
            // bPrev
            // 
            this.bPrev.Location = new System.Drawing.Point(46, 194);
            this.bPrev.Name = "bPrev";
            this.bPrev.Size = new System.Drawing.Size(75, 23);
            this.bPrev.TabIndex = 9;
            this.bPrev.Text = "Следующее";
            this.bPrev.UseVisualStyleBackColor = true;
            // 
            // lTotalRules
            // 
            this.lTotalRules.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lTotalRules.AutoSize = true;
            this.lTotalRules.Location = new System.Drawing.Point(127, 199);
            this.lTotalRules.Name = "lTotalRules";
            this.lTotalRules.Size = new System.Drawing.Size(84, 13);
            this.lTotalRules.TabIndex = 10;
            this.lTotalRules.Text = "Правило 1 из 1";
            this.lTotalRules.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lMaxStart
            // 
            this.lMaxStart.AutoSize = true;
            this.lMaxStart.Location = new System.Drawing.Point(29, 99);
            this.lMaxStart.Name = "lMaxStart";
            this.lMaxStart.Size = new System.Drawing.Size(240, 13);
            this.lMaxStart.TabIndex = 11;
            this.lMaxStart.Text = "Максимальное количество стартовых блоков";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(312, 96);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(44, 20);
            this.numericUpDown1.TabIndex = 12;
            this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // AddingRules
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 257);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.lMaxStart);
            this.Controls.Add(this.lTotalRules);
            this.Controls.Add(this.bPrev);
            this.Controls.Add(this.bNext);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.cB);
            this.Controls.Add(this.nUDMaxActive);
            this.Controls.Add(this.lMaxActive);
            this.Controls.Add(this.lRuleHeader);
            this.Controls.Add(this.lSearchAlg);
            this.Controls.Add(this.cBAlg);
            this.Controls.Add(this.bOK);
            this.Name = "AddingRules";
            this.Text = "Добавление правил для анализа";
            ((System.ComponentModel.ISupportInitialize)(this.nUDMaxActive)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bOK;
        private System.Windows.Forms.ComboBox cBAlg;
        private System.Windows.Forms.Label lSearchAlg;
        private System.Windows.Forms.Label lRuleHeader;
        private System.Windows.Forms.Label lMaxActive;
        private System.Windows.Forms.NumericUpDown nUDMaxActive;
        private System.Windows.Forms.CheckBox cB;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button bNext;
        private System.Windows.Forms.Button bPrev;
        private System.Windows.Forms.Label lTotalRules;
        private System.Windows.Forms.Label lMaxStart;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
    }
}