namespace Poly2RegresionTest
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.zedGraphControl1 = new ZedGraph.ZedGraphControl();
            this.MenuFileOpen = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.dgvLearningSource = new System.Windows.Forms.DataGridView();
            this.dgvTestingSource = new System.Windows.Forms.DataGridView();
            this.Learning = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLearningSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTestingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // zedGraphControl1
            // 
            this.zedGraphControl1.Location = new System.Drawing.Point(12, 12);
            this.zedGraphControl1.Name = "zedGraphControl1";
            this.zedGraphControl1.ScrollGrace = 0D;
            this.zedGraphControl1.ScrollMaxX = 0D;
            this.zedGraphControl1.ScrollMaxY = 0D;
            this.zedGraphControl1.ScrollMaxY2 = 0D;
            this.zedGraphControl1.ScrollMinX = 0D;
            this.zedGraphControl1.ScrollMinY = 0D;
            this.zedGraphControl1.ScrollMinY2 = 0D;
            this.zedGraphControl1.Size = new System.Drawing.Size(561, 356);
            this.zedGraphControl1.TabIndex = 0;
            this.zedGraphControl1.UseExtendedPrintDialog = true;
            // 
            // MenuFileOpen
            // 
            this.MenuFileOpen.Location = new System.Drawing.Point(463, 393);
            this.MenuFileOpen.Name = "MenuFileOpen";
            this.MenuFileOpen.Size = new System.Drawing.Size(75, 23);
            this.MenuFileOpen.TabIndex = 1;
            this.MenuFileOpen.Text = "ReadDataSet";
            this.MenuFileOpen.UseVisualStyleBackColor = true;
            this.MenuFileOpen.Click += new System.EventHandler(this.MenuFileOpen_Click_1);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // dgvLearningSource
            // 
            this.dgvLearningSource.AllowUserToOrderColumns = true;
            this.dgvLearningSource.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLearningSource.Location = new System.Drawing.Point(579, 12);
            this.dgvLearningSource.Name = "dgvLearningSource";
            this.dgvLearningSource.RowTemplate.Height = 23;
            this.dgvLearningSource.Size = new System.Drawing.Size(145, 356);
            this.dgvLearningSource.TabIndex = 2;
            // 
            // dgvTestingSource
            // 
            this.dgvTestingSource.AllowUserToOrderColumns = true;
            this.dgvTestingSource.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTestingSource.Location = new System.Drawing.Point(744, 12);
            this.dgvTestingSource.Name = "dgvTestingSource";
            this.dgvTestingSource.RowTemplate.Height = 23;
            this.dgvTestingSource.Size = new System.Drawing.Size(145, 356);
            this.dgvTestingSource.TabIndex = 3;
            // 
            // Learning
            // 
            this.Learning.Location = new System.Drawing.Point(579, 396);
            this.Learning.Name = "Learning";
            this.Learning.Size = new System.Drawing.Size(75, 23);
            this.Learning.TabIndex = 4;
            this.Learning.Text = "Learning";
            this.Learning.UseVisualStyleBackColor = true;
            this.Learning.Click += new System.EventHandler(this.Learning_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(905, 431);
            this.Controls.Add(this.Learning);
            this.Controls.Add(this.dgvTestingSource);
            this.Controls.Add(this.dgvLearningSource);
            this.Controls.Add(this.MenuFileOpen);
            this.Controls.Add(this.zedGraphControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dgvLearningSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTestingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ZedGraph.ZedGraphControl zedGraphControl1;
        private System.Windows.Forms.Button MenuFileOpen;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.DataGridView dgvLearningSource;
        private System.Windows.Forms.DataGridView dgvTestingSource;
        private System.Windows.Forms.Button Learning;
    }
}

