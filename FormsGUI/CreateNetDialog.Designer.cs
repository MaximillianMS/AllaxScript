namespace FormsGUI
{
    partial class CreateNetDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.wordLengthNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.sBlockSizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.createButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.wordLengthNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sBlockSizeNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(141, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Укажите параметры сети:";
            // 
            // wordLengthNumericUpDown
            // 
            this.wordLengthNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.wordLengthNumericUpDown.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.wordLengthNumericUpDown.Location = new System.Drawing.Point(136, 53);
            this.wordLengthNumericUpDown.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.wordLengthNumericUpDown.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.wordLengthNumericUpDown.Name = "wordLengthNumericUpDown";
            this.wordLengthNumericUpDown.Size = new System.Drawing.Size(60, 20);
            this.wordLengthNumericUpDown.TabIndex = 1;
            this.wordLengthNumericUpDown.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // sBlockSizeNumericUpDown
            // 
            this.sBlockSizeNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.sBlockSizeNumericUpDown.Location = new System.Drawing.Point(351, 51);
            this.sBlockSizeNumericUpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.sBlockSizeNumericUpDown.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.sBlockSizeNumericUpDown.Name = "sBlockSizeNumericUpDown";
            this.sBlockSizeNumericUpDown.Size = new System.Drawing.Size(60, 20);
            this.sBlockSizeNumericUpDown.TabIndex = 2;
            this.sBlockSizeNumericUpDown.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Полная длина слова";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(249, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Размер S-Блока";
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(351, 105);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(60, 23);
            this.createButton.TabIndex = 5;
            this.createButton.Text = "Создать";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(16, 105);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Отмена";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // CreateNetDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 140);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.sBlockSizeNumericUpDown);
            this.Controls.Add(this.wordLengthNumericUpDown);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CreateNetDialog";
            this.Text = "Создание Сети";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CreateNetDialog_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.wordLengthNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sBlockSizeNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown wordLengthNumericUpDown;
        private System.Windows.Forms.NumericUpDown sBlockSizeNumericUpDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Button cancelButton;
    }
}