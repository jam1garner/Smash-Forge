namespace Smash_Forge
{
    partial class NUS3BANKEditor
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.bankListBox = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.soundListBox = new System.Windows.Forms.ListBox();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wAVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iDSPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(551, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // bankListBox
            // 
            this.bankListBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.bankListBox.FormattingEnabled = true;
            this.bankListBox.Location = new System.Drawing.Point(0, 24);
            this.bankListBox.Name = "bankListBox";
            this.bankListBox.Size = new System.Drawing.Size(120, 318);
            this.bankListBox.TabIndex = 1;
            this.bankListBox.SelectedIndexChanged += new System.EventHandler(this.bankListBox_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.trackBar1);
            this.groupBox1.Controls.Add(this.soundListBox);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(120, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(431, 318);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "NUS3BANK Contents";
            // 
            // soundListBox
            // 
            this.soundListBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.soundListBox.FormattingEnabled = true;
            this.soundListBox.Location = new System.Drawing.Point(3, 16);
            this.soundListBox.Name = "soundListBox";
            this.soundListBox.Size = new System.Drawing.Size(193, 299);
            this.soundListBox.TabIndex = 0;
            this.soundListBox.DoubleClick += new System.EventHandler(this.soundListBox_DoubleClick);
            this.soundListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.soundListBox_MouseDown);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(202, 16);
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(223, 45);
            this.trackBar1.TabIndex = 1;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportAsToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 48);
            // 
            // exportAsToolStripMenuItem
            // 
            this.exportAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iDSPToolStripMenuItem,
            this.wAVToolStripMenuItem});
            this.exportAsToolStripMenuItem.Name = "exportAsToolStripMenuItem";
            this.exportAsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exportAsToolStripMenuItem.Text = "Export As";
            // 
            // wAVToolStripMenuItem
            // 
            this.wAVToolStripMenuItem.Name = "wAVToolStripMenuItem";
            this.wAVToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.wAVToolStripMenuItem.Text = "WAV";
            this.wAVToolStripMenuItem.Click += new System.EventHandler(this.wAVToolStripMenuItem_Click);
            // 
            // iDSPToolStripMenuItem
            // 
            this.iDSPToolStripMenuItem.Name = "iDSPToolStripMenuItem";
            this.iDSPToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.iDSPToolStripMenuItem.Text = "IDSP";
            this.iDSPToolStripMenuItem.Click += new System.EventHandler(this.iDSPToolStripMenuItem_Click);
            // 
            // NUS3BANKEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 342);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.bankListBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "NUS3BANKEditor";
            this.Text = "NUS3BANKEditor";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ListBox bankListBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox soundListBox;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exportAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wAVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iDSPToolStripMenuItem;
    }
}