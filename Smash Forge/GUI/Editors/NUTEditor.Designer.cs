namespace SmashForge
{
    partial class NutEditor
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
            this.formatLabel = new System.Windows.Forms.Label();
            this.renderChannelB = new System.Windows.Forms.Button();
            this.renderChannelG = new System.Windows.Forms.Button();
            this.renderChannelA = new System.Windows.Forms.Button();
            this.renderChannelR = new System.Windows.Forms.Button();
            this.settingsGroupBox = new System.Windows.Forms.GroupBox();
            this.generalGroupBox = new System.Windows.Forms.GroupBox();
            this.textureIdLabel = new System.Windows.Forms.Label();
            this.textureIdTB = new System.Windows.Forms.TextBox();
            this.mipmapGroupBox = new System.Windows.Forms.GroupBox();
            this.mipLevelTrackBar = new System.Windows.Forms.TrackBar();
            this.mipLevelLabel = new System.Windows.Forms.Label();
            this.minMipLevelLabel = new System.Windows.Forms.Label();
            this.maxMipLevelLabel = new System.Windows.Forms.Label();
            this.dimensionsGroupBox = new System.Windows.Forms.GroupBox();
            this.heightLabel = new System.Windows.Forms.Label();
            this.widthLabel = new System.Windows.Forms.Label();
            this.preserveAspectRatioCB = new System.Windows.Forms.CheckBox();
            this.glControl1 = new OpenTK.GLControl();
            this.previewGroupBox = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.displayGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip1.SuspendLayout();
            this.settingsGroupBox.SuspendLayout();
            this.generalGroupBox.SuspendLayout();
            this.mipmapGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mipLevelTrackBar)).BeginInit();
            this.dimensionsGroupBox.SuspendLayout();
            this.previewGroupBox.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.displayGroupBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textureListBox
            // 
            this.textureListBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.textureListBox.FormattingEnabled = true;
            this.textureListBox.Location = new System.Drawing.Point(0, 24);
            this.textureListBox.Margin = new System.Windows.Forms.Padding(2);
            this.textureListBox.Name = "textureListBox";
            this.textureListBox.Size = new System.Drawing.Size(97, 655);
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
            this.menuStrip1.Size = new System.Drawing.Size(1045, 24);
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
            // formatLabel
            // 
            this.formatLabel.AutoSize = true;
            this.formatLabel.Location = new System.Drawing.Point(5, 40);
            this.formatLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.formatLabel.Name = "formatLabel";
            this.formatLabel.Size = new System.Drawing.Size(42, 13);
            this.formatLabel.TabIndex = 15;
            this.formatLabel.Text = "Format:";
            // 
            // renderChannelB
            // 
            this.renderChannelB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderChannelB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelB.ForeColor = System.Drawing.Color.Blue;
            this.renderChannelB.Location = new System.Drawing.Point(69, 3);
            this.renderChannelB.Name = "renderChannelB";
            this.renderChannelB.Size = new System.Drawing.Size(27, 26);
            this.renderChannelB.TabIndex = 18;
            this.renderChannelB.Text = "B";
            this.renderChannelB.UseVisualStyleBackColor = true;
            this.renderChannelB.Click += new System.EventHandler(this.renderChannelB_Click_1);
            // 
            // renderChannelG
            // 
            this.renderChannelG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderChannelG.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelG.ForeColor = System.Drawing.Color.Green;
            this.renderChannelG.Location = new System.Drawing.Point(36, 3);
            this.renderChannelG.Name = "renderChannelG";
            this.renderChannelG.Size = new System.Drawing.Size(27, 26);
            this.renderChannelG.TabIndex = 19;
            this.renderChannelG.Text = "G";
            this.renderChannelG.UseVisualStyleBackColor = true;
            this.renderChannelG.Click += new System.EventHandler(this.renderChannelG_Click);
            // 
            // renderChannelA
            // 
            this.renderChannelA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderChannelA.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelA.Location = new System.Drawing.Point(102, 3);
            this.renderChannelA.Name = "renderChannelA";
            this.renderChannelA.Size = new System.Drawing.Size(27, 26);
            this.renderChannelA.TabIndex = 20;
            this.renderChannelA.Text = "A";
            this.renderChannelA.UseVisualStyleBackColor = true;
            this.renderChannelA.Click += new System.EventHandler(this.renderChannelA_Click_1);
            // 
            // renderChannelR
            // 
            this.renderChannelR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderChannelR.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderChannelR.ForeColor = System.Drawing.Color.Red;
            this.renderChannelR.Location = new System.Drawing.Point(3, 3);
            this.renderChannelR.Name = "renderChannelR";
            this.renderChannelR.Size = new System.Drawing.Size(27, 26);
            this.renderChannelR.TabIndex = 17;
            this.renderChannelR.Text = "R";
            this.renderChannelR.UseVisualStyleBackColor = true;
            this.renderChannelR.Click += new System.EventHandler(this.renderChannelR_Click_1);
            // 
            // settingsGroupBox
            // 
            this.settingsGroupBox.Controls.Add(this.flowLayoutPanel1);
            this.settingsGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.settingsGroupBox.Location = new System.Drawing.Point(800, 24);
            this.settingsGroupBox.Name = "settingsGroupBox";
            this.settingsGroupBox.Size = new System.Drawing.Size(245, 655);
            this.settingsGroupBox.TabIndex = 12;
            this.settingsGroupBox.TabStop = false;
            this.settingsGroupBox.Text = "Texture Settings";
            // 
            // generalGroupBox
            // 
            this.generalGroupBox.Controls.Add(this.textureIdLabel);
            this.generalGroupBox.Controls.Add(this.formatLabel);
            this.generalGroupBox.Controls.Add(this.textureIdTB);
            this.generalGroupBox.Location = new System.Drawing.Point(3, 3);
            this.generalGroupBox.Name = "generalGroupBox";
            this.generalGroupBox.Size = new System.Drawing.Size(233, 73);
            this.generalGroupBox.TabIndex = 29;
            this.generalGroupBox.TabStop = false;
            this.generalGroupBox.Text = "General";
            // 
            // textureIdLabel
            // 
            this.textureIdLabel.AutoSize = true;
            this.textureIdLabel.Location = new System.Drawing.Point(5, 17);
            this.textureIdLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.textureIdLabel.Name = "textureIdLabel";
            this.textureIdLabel.Size = new System.Drawing.Size(57, 13);
            this.textureIdLabel.TabIndex = 4;
            this.textureIdLabel.Text = "Texture ID";
            // 
            // textureIdTB
            // 
            this.textureIdTB.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.textureIdTB.Location = new System.Drawing.Point(94, 14);
            this.textureIdTB.Margin = new System.Windows.Forms.Padding(2);
            this.textureIdTB.Name = "textureIdTB";
            this.textureIdTB.Size = new System.Drawing.Size(134, 20);
            this.textureIdTB.TabIndex = 3;
            this.textureIdTB.TextChanged += new System.EventHandler(this.textureIdTB_TextChanged);
            // 
            // mipmapGroupBox
            // 
            this.mipmapGroupBox.Controls.Add(this.mipLevelTrackBar);
            this.mipmapGroupBox.Controls.Add(this.mipLevelLabel);
            this.mipmapGroupBox.Controls.Add(this.minMipLevelLabel);
            this.mipmapGroupBox.Controls.Add(this.maxMipLevelLabel);
            this.mipmapGroupBox.Location = new System.Drawing.Point(3, 142);
            this.mipmapGroupBox.Name = "mipmapGroupBox";
            this.mipmapGroupBox.Size = new System.Drawing.Size(236, 94);
            this.mipmapGroupBox.TabIndex = 28;
            this.mipmapGroupBox.TabStop = false;
            this.mipmapGroupBox.Text = "Mipmaps";
            // 
            // mipLevelTrackBar
            // 
            this.mipLevelTrackBar.Location = new System.Drawing.Point(64, 19);
            this.mipLevelTrackBar.Name = "mipLevelTrackBar";
            this.mipLevelTrackBar.Size = new System.Drawing.Size(169, 45);
            this.mipLevelTrackBar.TabIndex = 22;
            this.mipLevelTrackBar.Scroll += new System.EventHandler(this.mipLevelTrackBar_Scroll);
            // 
            // mipLevelLabel
            // 
            this.mipLevelLabel.AutoSize = true;
            this.mipLevelLabel.Location = new System.Drawing.Point(5, 25);
            this.mipLevelLabel.Name = "mipLevelLabel";
            this.mipLevelLabel.Size = new System.Drawing.Size(53, 13);
            this.mipLevelLabel.TabIndex = 23;
            this.mipLevelLabel.Text = "Mip Level";
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
            this.maxMipLevelLabel.Location = new System.Drawing.Point(184, 67);
            this.maxMipLevelLabel.Name = "maxMipLevelLabel";
            this.maxMipLevelLabel.Size = new System.Drawing.Size(43, 13);
            this.maxMipLevelLabel.TabIndex = 25;
            this.maxMipLevelLabel.Text = "Total: 0";
            // 
            // dimensionsGroupBox
            // 
            this.dimensionsGroupBox.Controls.Add(this.heightLabel);
            this.dimensionsGroupBox.Controls.Add(this.widthLabel);
            this.dimensionsGroupBox.Location = new System.Drawing.Point(3, 82);
            this.dimensionsGroupBox.Name = "dimensionsGroupBox";
            this.dimensionsGroupBox.Size = new System.Drawing.Size(233, 54);
            this.dimensionsGroupBox.TabIndex = 27;
            this.dimensionsGroupBox.TabStop = false;
            this.dimensionsGroupBox.Text = "Dimensions";
            // 
            // heightLabel
            // 
            this.heightLabel.AutoSize = true;
            this.heightLabel.Location = new System.Drawing.Point(100, 16);
            this.heightLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.heightLabel.Name = "heightLabel";
            this.heightLabel.Size = new System.Drawing.Size(38, 13);
            this.heightLabel.TabIndex = 7;
            this.heightLabel.Text = "Height";
            // 
            // widthLabel
            // 
            this.widthLabel.AutoSize = true;
            this.widthLabel.Location = new System.Drawing.Point(5, 16);
            this.widthLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.widthLabel.Name = "widthLabel";
            this.widthLabel.Size = new System.Drawing.Size(38, 13);
            this.widthLabel.TabIndex = 7;
            this.widthLabel.Text = "Width:";
            // 
            // preserveAspectRatioCB
            // 
            this.preserveAspectRatioCB.AutoSize = true;
            this.preserveAspectRatioCB.Location = new System.Drawing.Point(6, 57);
            this.preserveAspectRatioCB.Name = "preserveAspectRatioCB";
            this.preserveAspectRatioCB.Size = new System.Drawing.Size(132, 17);
            this.preserveAspectRatioCB.TabIndex = 26;
            this.preserveAspectRatioCB.Text = "Preserve Aspect Ratio";
            this.preserveAspectRatioCB.UseVisualStyleBackColor = true;
            this.preserveAspectRatioCB.CheckedChanged += new System.EventHandler(this.preserveAspectRatioCB_CheckedChanged);
            // 
            // glControl1
            // 
            this.glControl1.AutoSize = true;
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(8, 19);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(666, 630);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = false;
            this.glControl1.Load += new System.EventHandler(this.glControl1_Load);
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.glControl1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.glControl1_KeyPress);
            this.glControl1.Resize += new System.EventHandler(this.glControl1_Resize);
            // 
            // previewGroupBox
            // 
            this.previewGroupBox.Controls.Add(this.glControl1);
            this.previewGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewGroupBox.Location = new System.Drawing.Point(97, 24);
            this.previewGroupBox.Name = "previewGroupBox";
            this.previewGroupBox.Size = new System.Drawing.Size(703, 655);
            this.previewGroupBox.TabIndex = 13;
            this.previewGroupBox.TabStop = false;
            this.previewGroupBox.Text = "Preview";
            this.previewGroupBox.Resize += new System.EventHandler(this.previewBox_Resize);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.generalGroupBox);
            this.flowLayoutPanel1.Controls.Add(this.dimensionsGroupBox);
            this.flowLayoutPanel1.Controls.Add(this.mipmapGroupBox);
            this.flowLayoutPanel1.Controls.Add(this.displayGroupBox);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(239, 636);
            this.flowLayoutPanel1.TabIndex = 30;
            // 
            // displayGroupBox
            // 
            this.displayGroupBox.Controls.Add(this.tableLayoutPanel1);
            this.displayGroupBox.Controls.Add(this.preserveAspectRatioCB);
            this.displayGroupBox.Location = new System.Drawing.Point(3, 242);
            this.displayGroupBox.Name = "displayGroupBox";
            this.displayGroupBox.Size = new System.Drawing.Size(233, 100);
            this.displayGroupBox.TabIndex = 30;
            this.displayGroupBox.TabStop = false;
            this.displayGroupBox.Text = "Display Settings";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.renderChannelR, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.renderChannelG, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.renderChannelB, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.renderChannelA, 3, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(132, 32);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // NUTEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1045, 679);
            this.Controls.Add(this.previewGroupBox);
            this.Controls.Add(this.settingsGroupBox);
            this.Controls.Add(this.textureListBox);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "NutEditor";
            this.Text = "NUT Editor";
            this.Load += new System.EventHandler(this.NUTEditor_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.settingsGroupBox.ResumeLayout(false);
            this.generalGroupBox.ResumeLayout(false);
            this.generalGroupBox.PerformLayout();
            this.mipmapGroupBox.ResumeLayout(false);
            this.mipmapGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mipLevelTrackBar)).EndInit();
            this.dimensionsGroupBox.ResumeLayout(false);
            this.dimensionsGroupBox.PerformLayout();
            this.previewGroupBox.ResumeLayout(false);
            this.previewGroupBox.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.displayGroupBox.ResumeLayout(false);
            this.displayGroupBox.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
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
        private System.Windows.Forms.Label formatLabel;
        private System.Windows.Forms.Button renderChannelB;
        private System.Windows.Forms.Button renderChannelG;
        private System.Windows.Forms.Button renderChannelA;
        private System.Windows.Forms.Button renderChannelR;
        private System.Windows.Forms.GroupBox settingsGroupBox;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.GroupBox previewGroupBox;
        private System.Windows.Forms.Label mipLevelLabel;
        private System.Windows.Forms.TrackBar mipLevelTrackBar;
        private System.Windows.Forms.Label maxMipLevelLabel;
        private System.Windows.Forms.Label minMipLevelLabel;
        private System.Windows.Forms.CheckBox preserveAspectRatioCB;
        private System.Windows.Forms.Label textureIdLabel;
        private System.Windows.Forms.TextBox textureIdTB;
        private System.Windows.Forms.Label widthLabel;
        private System.Windows.Forms.Label heightLabel;
        private System.Windows.Forms.GroupBox dimensionsGroupBox;
        private System.Windows.Forms.GroupBox mipmapGroupBox;
        private System.Windows.Forms.GroupBox generalGroupBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox displayGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}