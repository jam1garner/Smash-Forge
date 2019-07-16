namespace SmashForge
{
    partial class ATKD_Editor
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.UnqSubactions_UpDownBox = new System.Windows.Forms.NumericUpDown();
            this.CmnSubactions_UpDownBox = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UnqSubactions_UpDownBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CmnSubactions_UpDownBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.UnqSubactions_UpDownBox);
            this.groupBox1.Controls.Add(this.CmnSubactions_UpDownBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(334, 75);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Header Data (don\'t change)";
            // 
            // UnqSubactions_UpDownBox
            // 
            this.UnqSubactions_UpDownBox.Location = new System.Drawing.Point(116, 45);
            this.UnqSubactions_UpDownBox.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.UnqSubactions_UpDownBox.Name = "UnqSubactions_UpDownBox";
            this.UnqSubactions_UpDownBox.Size = new System.Drawing.Size(86, 20);
            this.UnqSubactions_UpDownBox.TabIndex = 3;
            // 
            // CmnSubactions_UpDownBox
            // 
            this.CmnSubactions_UpDownBox.Location = new System.Drawing.Point(116, 19);
            this.CmnSubactions_UpDownBox.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.CmnSubactions_UpDownBox.Name = "CmnSubactions_UpDownBox";
            this.CmnSubactions_UpDownBox.Size = new System.Drawing.Size(86, 20);
            this.CmnSubactions_UpDownBox.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Unique Subactions";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Common Subactions";
            // 
            // dataGridView
            // 
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.Location = new System.Drawing.Point(0, 75);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.Size = new System.Drawing.Size(334, 372);
            this.dataGridView.TabIndex = 1;
            this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellValueChanged);
            // 
            // ATKD_Editor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 447);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ATKD_Editor";
            this.Text = "ATKD Editor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ATKD_Editor_FormClosed);
            this.Load += new System.EventHandler(this.ATKD_Editor_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UnqSubactions_UpDownBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CmnSubactions_UpDownBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown UnqSubactions_UpDownBox;
        private System.Windows.Forms.NumericUpDown CmnSubactions_UpDownBox;
        private System.Windows.Forms.DataGridView dataGridView;
    }
}