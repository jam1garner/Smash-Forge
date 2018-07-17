namespace Smash_Forge
{
    partial class BNTXEditor
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
            this.importPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tEXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tEXToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pNGToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.glControl1 = new OpenTK.GLControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textureListBox = new System.Windows.Forms.ListBox();
            this.previewGroupBox = new System.Windows.Forms.GroupBox();
            this.mipMapGroupBox = new System.Windows.Forms.GroupBox();
            this.mipLevelTrackBar = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.minMipLevelLabel = new System.Windows.Forms.Label();
            this.maxMipLevelLabel = new System.Windows.Forms.Label();
            this.renderChannelB = new System.Windows.Forms.Button();
            this.renderChannelG = new System.Windows.Forms.Button();
            this.renderChannelA = new System.Windows.Forms.Button();
            this.renderChannelR = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.previewGroupBox.SuspendLayout();
            this.mipMapGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mipLevelTrackBar)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(792, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Visible = false;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importPNGToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // importPNGToolStripMenuItem
            // 
            this.importPNGToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tEXToolStripMenuItem,
            this.pNGToolStripMenuItem});
            this.importPNGToolStripMenuItem.Name = "importPNGToolStripMenuItem";
            this.importPNGToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.importPNGToolStripMenuItem.Text = "Import";
            // 
            // tEXToolStripMenuItem
            // 
            this.tEXToolStripMenuItem.Name = "tEXToolStripMenuItem";
            this.tEXToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            // 
            // pNGToolStripMenuItem
            // 
            this.pNGToolStripMenuItem.Name = "pNGToolStripMenuItem";
            this.pNGToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tEXToolStripMenuItem1,
            this.pNGToolStripMenuItem1});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // tEXToolStripMenuItem1
            // 
            this.tEXToolStripMenuItem1.Name = "tEXToolStripMenuItem1";
            this.tEXToolStripMenuItem1.Size = new System.Drawing.Size(67, 22);
            // 
            // pNGToolStripMenuItem1
            // 
            this.pNGToolStripMenuItem1.Name = "pNGToolStripMenuItem1";
            this.pNGToolStripMenuItem1.Size = new System.Drawing.Size(67, 22);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // glControl1
            // 
            this.glControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(16, 19);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(624, 345);
            this.glControl1.TabIndex = 7;
            this.glControl1.VSync = false;
            this.glControl1.Load += new System.EventHandler(this.glControl1_Load);
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.textureListBox);
            this.groupBox1.Controls.Add(this.previewGroupBox);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(795, 512);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // textureListBox
            // 
            this.textureListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textureListBox.FormattingEnabled = true;
            this.textureListBox.Location = new System.Drawing.Point(0, 26);
            this.textureListBox.Margin = new System.Windows.Forms.Padding(2);
            this.textureListBox.Name = "textureListBox";
            this.textureListBox.Size = new System.Drawing.Size(144, 472);
            this.textureListBox.TabIndex = 31;
            this.textureListBox.SelectedIndexChanged += new System.EventHandler(this.textureListBox_SelectedIndexChanged);
            this.textureListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textureListBox_MouseDown);
            // 
            // previewGroupBox
            // 
            this.previewGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.previewGroupBox.Controls.Add(this.glControl1);
            this.previewGroupBox.Location = new System.Drawing.Point(149, 142);
            this.previewGroupBox.Name = "previewGroupBox";
            this.previewGroupBox.Size = new System.Drawing.Size(646, 378);
            this.previewGroupBox.TabIndex = 30;
            this.previewGroupBox.TabStop = false;
            this.previewGroupBox.Text = "Preview";
            this.previewGroupBox.Resize += new System.EventHandler(this.previewBox_Resize);
            // 
            // mipMapGroupBox
            // 
            this.mipMapGroupBox.Controls.Add(this.mipLevelTrackBar);
            this.mipMapGroupBox.Controls.Add(this.label6);
            this.mipMapGroupBox.Controls.Add(this.minMipLevelLabel);
            this.mipMapGroupBox.Controls.Add(this.maxMipLevelLabel);
            this.mipMapGroupBox.Location = new System.Drawing.Point(415, 6);
            this.mipMapGroupBox.Name = "mipMapGroupBox";
            this.mipMapGroupBox.Size = new System.Drawing.Size(210, 111);
            this.mipMapGroupBox.TabIndex = 29;
            this.mipMapGroupBox.TabStop = false;
            this.mipMapGroupBox.Text = "Mip Maps";
            // 
            // mipLevelTrackBar
            // 
            this.mipLevelTrackBar.Location = new System.Drawing.Point(70, 19);
            this.mipLevelTrackBar.Name = "mipLevelTrackBar";
            this.mipLevelTrackBar.Size = new System.Drawing.Size(134, 45);
            this.mipLevelTrackBar.TabIndex = 22;
            this.mipLevelTrackBar.Scroll += new System.EventHandler(this.mipLevelTrackBar_Scroll);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 23;
            this.label6.Text = "Mip Level";
            // 
            // minMipLevelLabel
            // 
            this.minMipLevelLabel.AutoSize = true;
            this.minMipLevelLabel.Location = new System.Drawing.Point(71, 67);
            this.minMipLevelLabel.Name = "minMipLevelLabel";
            this.minMipLevelLabel.Size = new System.Drawing.Size(13, 13);
            this.minMipLevelLabel.TabIndex = 24;
            this.minMipLevelLabel.Text = "1";
            // 
            // maxMipLevelLabel
            // 
            this.maxMipLevelLabel.AutoSize = true;
            this.maxMipLevelLabel.Location = new System.Drawing.Point(148, 67);
            this.maxMipLevelLabel.Name = "maxMipLevelLabel";
            this.maxMipLevelLabel.Size = new System.Drawing.Size(43, 13);
            this.maxMipLevelLabel.TabIndex = 25;
            this.maxMipLevelLabel.Text = "Total: 0";
            // 
            // renderChannelB
            // 
            this.renderChannelB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelB.ForeColor = System.Drawing.Color.Blue;
            this.renderChannelB.Location = new System.Drawing.Point(62, 93);
            this.renderChannelB.Name = "renderChannelB";
            this.renderChannelB.Size = new System.Drawing.Size(24, 24);
            this.renderChannelB.TabIndex = 22;
            this.renderChannelB.Text = "B";
            this.renderChannelB.UseVisualStyleBackColor = true;
            this.renderChannelB.Click += new System.EventHandler(this.renderChannelB_Click);
            // 
            // renderChannelG
            // 
            this.renderChannelG.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelG.ForeColor = System.Drawing.Color.Green;
            this.renderChannelG.Location = new System.Drawing.Point(36, 93);
            this.renderChannelG.Name = "renderChannelG";
            this.renderChannelG.Size = new System.Drawing.Size(24, 24);
            this.renderChannelG.TabIndex = 23;
            this.renderChannelG.Text = "G";
            this.renderChannelG.UseVisualStyleBackColor = true;
            this.renderChannelG.Click += new System.EventHandler(this.renderChannelG_Click);
            // 
            // renderChannelA
            // 
            this.renderChannelA.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelA.Location = new System.Drawing.Point(88, 93);
            this.renderChannelA.Name = "renderChannelA";
            this.renderChannelA.Size = new System.Drawing.Size(24, 24);
            this.renderChannelA.TabIndex = 24;
            this.renderChannelA.Text = "A";
            this.renderChannelA.UseVisualStyleBackColor = true;
            this.renderChannelA.Click += new System.EventHandler(this.renderChannelA_Click);
            // 
            // renderChannelR
            // 
            this.renderChannelR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelR.ForeColor = System.Drawing.Color.Red;
            this.renderChannelR.Location = new System.Drawing.Point(10, 93);
            this.renderChannelR.Name = "renderChannelR";
            this.renderChannelR.Size = new System.Drawing.Size(24, 24);
            this.renderChannelR.TabIndex = 21;
            this.renderChannelR.Text = "R";
            this.renderChannelR.UseVisualStyleBackColor = true;
            this.renderChannelR.Click += new System.EventHandler(this.renderChannelR_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(107, 40);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Anti Alias Mode";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "MipMap Count";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(107, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Format";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Width";
            this.label2.Click += new System.EventHandler(this.label2_Click_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Height";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.mipMapGroupBox);
            this.groupBox2.Controls.Add(this.renderChannelR);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.renderChannelB);
            this.groupBox2.Controls.Add(this.renderChannelA);
            this.groupBox2.Controls.Add(this.renderChannelG);
            this.groupBox2.Location = new System.Drawing.Point(155, 13);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(631, 123);
            this.groupBox2.TabIndex = 32;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Texture settings";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(107, 65);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 30;
            this.label7.Text = "Alignment";
            // 
            // BNTXEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(795, 512);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "BNTXEditor";
            this.Text = "BNTX Texture Editor";
            this.Load += new System.EventHandler(this.BNTXEditor_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.previewGroupBox.ResumeLayout(false);
            this.mipMapGroupBox.ResumeLayout(false);
            this.mipMapGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mipLevelTrackBar)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importPNGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tEXToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pNGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tEXToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem pNGToolStripMenuItem1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button renderChannelB;
        private System.Windows.Forms.Button renderChannelG;
        private System.Windows.Forms.Button renderChannelA;
        private System.Windows.Forms.Button renderChannelR;
        private System.Windows.Forms.GroupBox mipMapGroupBox;
        private System.Windows.Forms.TrackBar mipLevelTrackBar;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label minMipLevelLabel;
        private System.Windows.Forms.Label maxMipLevelLabel;
        private System.Windows.Forms.GroupBox previewGroupBox;
        private System.Windows.Forms.ListBox textureListBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label7;
    }
}