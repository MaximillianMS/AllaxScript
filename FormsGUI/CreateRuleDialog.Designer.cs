namespace FormsGUI
{
    partial class CreateRuleDialog
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
            this.sBlockLayerCountNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.createButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.startingInputMaskedTextBox = new System.Windows.Forms.MaskedTextBox();
            this.SolverTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.sBlockInputCountNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.sBlockInputCountLabel = new System.Windows.Forms.Label();
            this.useConstInputCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.sBlockLayerCountNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sBlockInputCountNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Укажите параметры правила:";
            // 
            // sBlockLayerCountNumericUpDown
            // 
            this.sBlockLayerCountNumericUpDown.Location = new System.Drawing.Point(350, 193);
            this.sBlockLayerCountNumericUpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.sBlockLayerCountNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.sBlockLayerCountNumericUpDown.Name = "sBlockLayerCountNumericUpDown";
            this.sBlockLayerCountNumericUpDown.Size = new System.Drawing.Size(60, 20);
            this.sBlockLayerCountNumericUpDown.TabIndex = 2;
            this.sBlockLayerCountNumericUpDown.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 195);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(297, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Максимальное колличество активных S-Блоков на слое:";
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(350, 230);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(60, 23);
            this.createButton.TabIndex = 5;
            this.createButton.Text = "Создать";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(11, 230);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Отмена";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // startingInputMaskedTextBox
            // 
            this.startingInputMaskedTextBox.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
            this.startingInputMaskedTextBox.Enabled = false;
            this.startingInputMaskedTextBox.Location = new System.Drawing.Point(11, 83);
            this.startingInputMaskedTextBox.Name = "startingInputMaskedTextBox";
            this.startingInputMaskedTextBox.Size = new System.Drawing.Size(399, 20);
            this.startingInputMaskedTextBox.TabIndex = 9;
            this.startingInputMaskedTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.startingInputMaskedTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.startingInputMaskedTextBox_KeyPress);
            // 
            // SolverTypeComboBox
            // 
            this.SolverTypeComboBox.FormattingEnabled = true;
            this.SolverTypeComboBox.Items.AddRange(new object[] {
            "Жадный",
            "Частичный",
            "Полный"});
            this.SolverTypeComboBox.Location = new System.Drawing.Point(289, 157);
            this.SolverTypeComboBox.Name = "SolverTypeComboBox";
            this.SolverTypeComboBox.Size = new System.Drawing.Size(121, 21);
            this.SolverTypeComboBox.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 160);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(133, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Способ поиска решений:";
            // 
            // sBlockInputCountNumericUpDown
            // 
            this.sBlockInputCountNumericUpDown.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.sBlockInputCountNumericUpDown.Location = new System.Drawing.Point(350, 123);
            this.sBlockInputCountNumericUpDown.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.sBlockInputCountNumericUpDown.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.sBlockInputCountNumericUpDown.Name = "sBlockInputCountNumericUpDown";
            this.sBlockInputCountNumericUpDown.Size = new System.Drawing.Size(60, 20);
            this.sBlockInputCountNumericUpDown.TabIndex = 1;
            this.sBlockInputCountNumericUpDown.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // sBlockInputCountLabel
            // 
            this.sBlockInputCountLabel.AutoSize = true;
            this.sBlockInputCountLabel.Location = new System.Drawing.Point(11, 125);
            this.sBlockInputCountLabel.Name = "sBlockInputCountLabel";
            this.sBlockInputCountLabel.Size = new System.Drawing.Size(170, 13);
            this.sBlockInputCountLabel.TabIndex = 3;
            this.sBlockInputCountLabel.Text = "Колличество входных S-Блоков:";
            // 
            // useConstInputCheckBox
            // 
            this.useConstInputCheckBox.AutoSize = true;
            this.useConstInputCheckBox.Location = new System.Drawing.Point(11, 44);
            this.useConstInputCheckBox.Name = "useConstInputCheckBox";
            this.useConstInputCheckBox.Size = new System.Drawing.Size(285, 17);
            this.useConstInputCheckBox.TabIndex = 12;
            this.useConstInputCheckBox.Text = "Использовать константное начальное заполнение";
            this.useConstInputCheckBox.UseVisualStyleBackColor = true;
            this.useConstInputCheckBox.CheckedChanged += new System.EventHandler(this.useConstInputCheckBox_CheckedChanged);
            // 
            // CreateRuleDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 269);
            this.Controls.Add(this.useConstInputCheckBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.SolverTypeComboBox);
            this.Controls.Add(this.startingInputMaskedTextBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.sBlockInputCountLabel);
            this.Controls.Add(this.sBlockLayerCountNumericUpDown);
            this.Controls.Add(this.sBlockInputCountNumericUpDown);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CreateRuleDialog";
            this.Text = "Создание правила";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CreateRuleDialog_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.sBlockLayerCountNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sBlockInputCountNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown sBlockLayerCountNumericUpDown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.MaskedTextBox startingInputMaskedTextBox;
        private System.Windows.Forms.ComboBox SolverTypeComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown sBlockInputCountNumericUpDown;
        private System.Windows.Forms.Label sBlockInputCountLabel;
        private System.Windows.Forms.CheckBox useConstInputCheckBox;
    }
}