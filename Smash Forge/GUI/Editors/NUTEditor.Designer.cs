namespace Smash_Forge
{
    partial class NUTEditor
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
            this.textureListBox = new System.Windows.Forms.ListBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.editingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAndOpenInDefaultEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAndPickAProgramToEditWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importEditedFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.renderChannelB = new System.Windows.Forms.Button();
            this.renderChannelG = new System.Windows.Forms.Button();
            this.renderChannelA = new System.Windows.Forms.Button();
            this.renderChannelR = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.maxMipLevelLabel = new System.Windows.Forms.Label();
            this.minMipLevelLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.mipLevelTrackBar = new System.Windows.Forms.TrackBar();
            this.aspectRatioCB = new System.Windows.Forms.CheckBox();
            this.glControl1 = new OpenTK.GLControl();
            this.previewBox = new System.Windows.Forms.GroupBox();
            this.preserveAspectRatioCB = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mipLevelTrackBar)).BeginInit();
            this.previewBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // textureListBox
            // 
            this.textureListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textureListBox.FormattingEnabled = true;
            this.textureListBox.Location = new System.Drawing.Point(11, 22);
            this.textureListBox.Margin = new System.Windows.Forms.Padding(2);
            this.textureListBox.Name = "textureListBox";
            this.textureListBox.Size = new System.Drawing.Size(91, 433);
            this.textureListBox.TabIndex = 2;
            this.textureListBox.SelectedIndexChanged += new System.EventHandler(this.textureListBox_SelectedIndexChanged);
            this.textureListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBox2_MouseDown);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editingToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(584, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // editingToolStripMenuItem
            // 
            this.editingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractAndOpenInDefaultEditorToolStripMenuItem,
            this.extractAndPickAProgramToEditWithToolStripMenuItem,
            this.importEditedFileToolStripMenuItem});
            this.editingToolStripMenuItem.Name = "editingToolStripMenuItem";
            this.editingToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.editingToolStripMenuItem.Text = "Editing";
            // 
            // extractAndOpenInDefaultEditorToolStripMenuItem
            // 
            this.extractAndOpenInDefaultEditorToolStripMenuItem.Name = "extractAndOpenInDefaultEditorToolStripMenuItem";
            this.extractAndOpenInDefaultEditorToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.extractAndOpenInDefaultEditorToolStripMenuItem.Text = "Edit With Default Program";
            this.extractAndOpenInDefaultEditorToolStripMenuItem.Click += new System.EventHandler(this.extractAndOpenInDefaultEditorToolStripMenuItem_Click);
            // 
            // extractAndPickAProgramToEditWithToolStripMenuItem
            // 
            this.extractAndPickAProgramToEditWithToolStripMenuItem.Name = "extractAndPickAProgramToEditWithToolStripMenuItem";
            this.extractAndPickAProgramToEditWithToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.extractAndPickAProgramToEditWithToolStripMenuItem.Text = "Pick Program to Edit With";
            this.extractAndPickAProgramToEditWithToolStripMenuItem.Click += new System.EventHandler(this.extractAndPickAProgramToEditWithToolStripMenuItem_Click);
            // 
            // importEditedFileToolStripMenuItem
            // 
            this.importEditedFileToolStripMenuItem.Name = "importEditedFileToolStripMenuItem";
            this.importEditedFileToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.importEditedFileToolStripMenuItem.Text = "Import Edited File";
            this.importEditedFileToolStripMenuItem.Click += new System.EventHandler(this.importEditedFileToolStripMenuItem_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 84);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Format:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 18);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(236, 59);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Texture ID";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.textBox1.Location = new System.Drawing.Point(141, 2);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(72, 20);
            this.textBox1.TabIndex = 3;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 22);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Width:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(120, 22);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Height";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 44);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Mipmap:";
            // 
            // renderChannelB
            // 
            this.renderChannelB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelB.ForeColor = System.Drawing.Color.Blue;
            this.renderChannelB.Location = new System.Drawing.Point(180, 58);
            this.renderChannelB.Name = "renderChannelB";
            this.renderChannelB.Size = new System.Drawing.Size(24, 24);
            this.renderChannelB.TabIndex = 18;
            this.renderChannelB.Text = "B";
            this.renderChannelB.UseVisualStyleBackColor = true;
            this.renderChannelB.Click += new System.EventHandler(this.renderChannelB_Click_1);
            // 
            // renderChannelG
            // 
            this.renderChannelG.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelG.ForeColor = System.Drawing.Color.Green;
            this.renderChannelG.Location = new System.Drawing.Point(154, 58);
            this.renderChannelG.Name = "renderChannelG";
            this.renderChannelG.Size = new System.Drawing.Size(24, 24);
            this.renderChannelG.TabIndex = 19;
            this.renderChannelG.Text = "G";
            this.renderChannelG.UseVisualStyleBackColor = true;
            this.renderChannelG.Click += new System.EventHandler(this.renderChannelG_Click);
            // 
            // renderChannelA
            // 
            this.renderChannelA.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelA.Location = new System.Drawing.Point(206, 58);
            this.renderChannelA.Name = "renderChannelA";
            this.renderChannelA.Size = new System.Drawing.Size(24, 24);
            this.renderChannelA.TabIndex = 20;
            this.renderChannelA.Text = "A";
            this.renderChannelA.UseVisualStyleBackColor = true;
            this.renderChannelA.Click += new System.EventHandler(this.renderChannelA_Click_1);
            // 
            // renderChannelR
            // 
            this.renderChannelR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelR.ForeColor = System.Drawing.Color.Red;
            this.renderChannelR.Location = new System.Drawing.Point(128, 58);
            this.renderChannelR.Name = "renderChannelR";
            this.renderChannelR.Size = new System.Drawing.Size(24, 24);
            this.renderChannelR.TabIndex = 17;
            this.renderChannelR.Text = "R";
            this.renderChannelR.UseVisualStyleBackColor = true;
            this.renderChannelR.Click += new System.EventHandler(this.renderChannelR_Click_1);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.preserveAspectRatioCB);
            this.groupBox1.Controls.Add(this.maxMipLevelLabel);
            this.groupBox1.Controls.Add(this.minMipLevelLabel);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.mipLevelTrackBar);
            this.groupBox1.Controls.Add(this.aspectRatioCB);
            this.groupBox1.Controls.Add(this.renderChannelR);
            this.groupBox1.Controls.Add(this.renderChannelA);
            this.groupBox1.Controls.Add(this.renderChannelG);
            this.groupBox1.Controls.Add(this.renderChannelB);
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(107, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(465, 102);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Texture Settings";
            // 
            // maxMipLevelLabel
            // 
            this.maxMipLevelLabel.AutoSize = true;
            this.maxMipLevelLabel.Location = new System.Drawing.Point(420, 50);
            this.maxMipLevelLabel.Name = "maxMipLevelLabel";
            this.maxMipLevelLabel.Size = new System.Drawing.Size(13, 13);
            this.maxMipLevelLabel.TabIndex = 25;
            this.maxMipLevelLabel.Text = "0";
            // 
            // minMipLevelLabel
            // 
            this.minMipLevelLabel.AutoSize = true;
            this.minMipLevelLabel.Location = new System.Drawing.Point(312, 50);
            this.minMipLevelLabel.Name = "minMipLevelLabel";
            this.minMipLevelLabel.Size = new System.Drawing.Size(13, 13);
            this.minMipLevelLabel.TabIndex = 24;
            this.minMipLevelLabel.Text = "1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(246, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 23;
            this.label6.Text = "Mip Level";
            // 
            // mipLevelTrackBar
            // 
            this.mipLevelTrackBar.Location = new System.Drawing.Point(305, 18);
            this.mipLevelTrackBar.Name = "mipLevelTrackBar";
            this.mipLevelTrackBar.Size = new System.Drawing.Size(134, 45);
            this.mipLevelTrackBar.TabIndex = 22;
            this.mipLevelTrackBar.Scroll += new System.EventHandler(this.mipLevelTrackBar_Scroll);
            // 
            // aspectRatioCB
            // 
            this.aspectRatioCB.AutoSize = true;
            this.aspectRatioCB.Location = new System.Drawing.Point(10, 102);
            this.aspectRatioCB.Name = "aspectRatioCB";
            this.aspectRatioCB.Size = new System.Drawing.Size(132, 17);
            this.aspectRatioCB.TabIndex = 21;
            this.aspectRatioCB.Text = "Preserve Aspect Ratio";
            this.aspectRatioCB.UseVisualStyleBackColor = true;
            this.aspectRatioCB.CheckedChanged += new System.EventHandler(this.aspectRatioCB_CheckedChanged);
            // 
            // glControl1
            // 
            this.glControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl1.AutoSize = true;
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(10, 19);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(449, 295);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = false;
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.glControl1_KeyPress);
            // 
            // previewBox
            // 
            this.previewBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.previewBox.Controls.Add(this.glControl1);
            this.previewBox.Location = new System.Drawing.Point(107, 135);
            this.previewBox.Name = "previewBox";
            this.previewBox.Size = new System.Drawing.Size(465, 320);
            this.previewBox.TabIndex = 13;
            this.previewBox.TabStop = false;
            this.previewBox.Text = "Preview";
            this.previewBox.Resize += new System.EventHandler(this.previewBox_Resize);
            // 
            // preserveAspectRatioCB
            // 
            this.preserveAspectRatioCB.AutoSize = true;
            this.preserveAspectRatioCB.Location = new System.Drawing.Point(249, 64);
            this.preserveAspectRatioCB.Name = "preserveAspectRatioCB";
            this.preserveAspectRatioCB.Size = new System.Drawing.Size(132, 17);
            this.preserveAspectRatioCB.TabIndex = 26;
            this.preserveAspectRatioCB.Text = "Preserve Aspect Ratio";
            this.preserveAspectRatioCB.UseVisualStyleBackColor = true;
            this.preserveAspectRatioCB.CheckedChanged += new System.EventHandler(this.preserveAspectRatioCB_CheckedChanged);
            // 
            // NUTEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 473);
            this.Controls.Add(this.previewBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textureListBox);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "NUTEditor";
            this.Text = "NUT Editor";
            this.Load += new System.EventHandler(this.NUTEditor_Load);
            this.Resize += new System.EventHandler(this.NUTEditor_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mipLevelTrackBar)).EndInit();
            this.previewBox.ResumeLayout(false);
            this.previewBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox textureListBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAndOpenInDefaultEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAndPickAProgramToEditWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importEditedFileToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button renderChannelB;
        private System.Windows.Forms.Button renderChannelG;
        private System.Windows.Forms.Button renderChannelA;
        private System.Windows.Forms.Button renderChannelR;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox aspectRatioCB;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.GroupBox previewBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TrackBar mipLevelTrackBar;
        private System.Windows.Forms.Label maxMipLevelLabel;
        private System.Windows.Forms.Label minMipLevelLabel;
        private System.Windows.Forms.CheckBox preserveAspectRatioCB;
    }
}