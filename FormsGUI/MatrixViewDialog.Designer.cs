namespace FormsGUI
{
    partial class MatrixViewDialog
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.corrMatrixTabPage = new System.Windows.Forms.TabPage();
            this.corrMatrixDataGridView = new System.Windows.Forms.DataGridView();
            this.diffMatrixTabPage = new System.Windows.Forms.TabPage();
            this.diffMatrixDataGridView = new System.Windows.Forms.DataGridView();
            this.tabControl1.SuspendLayout();
            this.corrMatrixTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.corrMatrixDataGridView)).BeginInit();
            this.diffMatrixTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.diffMatrixDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.corrMatrixTabPage);
            this.tabControl1.Controls.Add(this.diffMatrixTabPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(634, 566);
            this.tabControl1.TabIndex = 0;
            // 
            // corrMatrixTabPage
            // 
            this.corrMatrixTabPage.Controls.Add(this.corrMatrixDataGridView);
            this.corrMatrixTabPage.Location = new System.Drawing.Point(4, 22);
            this.corrMatrixTabPage.Name = "corrMatrixTabPage";
            this.corrMatrixTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.corrMatrixTabPage.Size = new System.Drawing.Size(626, 540);
            this.corrMatrixTabPage.TabIndex = 0;
            this.corrMatrixTabPage.Text = "Матрица Корреляции";
            this.corrMatrixTabPage.UseVisualStyleBackColor = true;
            // 
            // corrMatrixDataGridView
            // 
            this.corrMatrixDataGridView.AllowUserToAddRows = false;
            this.corrMatrixDataGridView.AllowUserToDeleteRows = false;
            this.corrMatrixDataGridView.AllowUserToResizeColumns = false;
            this.corrMatrixDataGridView.AllowUserToResizeRows = false;
            this.corrMatrixDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.corrMatrixDataGridView.ColumnHeadersVisible = false;
            this.corrMatrixDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.corrMatrixDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.corrMatrixDataGridView.Location = new System.Drawing.Point(3, 3);
            this.corrMatrixDataGridView.Name = "corrMatrixDataGridView";
            this.corrMatrixDataGridView.ReadOnly = true;
            this.corrMatrixDataGridView.RowHeadersVisible = false;
            this.corrMatrixDataGridView.Size = new System.Drawing.Size(620, 534);
            this.corrMatrixDataGridView.TabIndex = 1;
            // 
            // diffMatrixTabPage
            // 
            this.diffMatrixTabPage.Controls.Add(this.diffMatrixDataGridView);
            this.diffMatrixTabPage.Location = new System.Drawing.Point(4, 22);
            this.diffMatrixTabPage.Name = "diffMatrixTabPage";
            this.diffMatrixTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.diffMatrixTabPage.Size = new System.Drawing.Size(626, 540);
            this.diffMatrixTabPage.TabIndex = 1;
            this.diffMatrixTabPage.Text = "Матрица Разностей";
            this.diffMatrixTabPage.UseVisualStyleBackColor = true;
            // 
            // diffMatrixDataGridView
            // 
            this.diffMatrixDataGridView.AllowUserToAddRows = false;
            this.diffMatrixDataGridView.AllowUserToDeleteRows = false;
            this.diffMatrixDataGridView.AllowUserToResizeColumns = false;
            this.diffMatrixDataGridView.AllowUserToResizeRows = false;
            this.diffMatrixDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.diffMatrixDataGridView.ColumnHeadersVisible = false;
            this.diffMatrixDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.diffMatrixDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.diffMatrixDataGridView.Location = new System.Drawing.Point(3, 3);
            this.diffMatrixDataGridView.Name = "diffMatrixDataGridView";
            this.diffMatrixDataGridView.ReadOnly = true;
            this.diffMatrixDataGridView.Size = new System.Drawing.Size(620, 534);
            this.diffMatrixDataGridView.TabIndex = 1;
            // 
            // MatrixViewDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(634, 566);
            this.Controls.Add(this.tabControl1);
            this.MinimizeBox = false;
            this.Name = "MatrixViewDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Просмотр матриц";
            this.tabControl1.ResumeLayout(false);
            this.corrMatrixTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.corrMatrixDataGridView)).EndInit();
            this.diffMatrixTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.diffMatrixDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage corrMatrixTabPage;
        private System.Windows.Forms.DataGridView corrMatrixDataGridView;
        private System.Windows.Forms.TabPage diffMatrixTabPage;
        private System.Windows.Forms.DataGridView diffMatrixDataGridView;
    }
}