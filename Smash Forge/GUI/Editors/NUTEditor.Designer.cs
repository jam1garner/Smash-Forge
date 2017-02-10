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
            this.glControl1 = new OpenTK.GLControl();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openNUTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newNUTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveNUTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAsDDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAsDDSToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAndOpenInDefaultEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAndPickAProgramToEditWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importEditedFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importNUTFromFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // glControl1
            // 
            this.glControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(195, 22);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(236, 240);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = false;
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(14, 22);
            this.listBox1.Margin = new System.Windows.Forms.Padding(2);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(84, 316);
            this.listBox1.TabIndex = 1;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // listBox2
            // 
            this.listBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox2.FormattingEnabled = true;
            this.listBox2.Location = new System.Drawing.Point(100, 22);
            this.listBox2.Margin = new System.Windows.Forms.Padding(2);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(91, 316);
            this.listBox2.TabIndex = 2;
            this.listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Hash ID";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(197, 333);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Format:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
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
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label1);
            this.tableLayoutPanel1.Controls.Add(this.textBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(195, 267);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(236, 59);
            this.tableLayoutPanel1.TabIndex = 8;
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
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.textureToolStripMenuItem,
            this.editingToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(440, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openNUTToolStripMenuItem,
            this.newNUTToolStripMenuItem,
            this.saveNUTToolStripMenuItem,
            this.dumpToolStripMenuItem,
            this.importNUTFromFolderToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openNUTToolStripMenuItem
            // 
            this.openNUTToolStripMenuItem.Name = "openNUTToolStripMenuItem";
            this.openNUTToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openNUTToolStripMenuItem.Text = "Open NUT";
            this.openNUTToolStripMenuItem.Click += new System.EventHandler(this.openNUTToolStripMenuItem_Click);
            // 
            // newNUTToolStripMenuItem
            // 
            this.newNUTToolStripMenuItem.Name = "newNUTToolStripMenuItem";
            this.newNUTToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newNUTToolStripMenuItem.Text = "New NUT";
            this.newNUTToolStripMenuItem.Click += new System.EventHandler(this.newNUTToolStripMenuItem_Click);
            // 
            // saveNUTToolStripMenuItem
            // 
            this.saveNUTToolStripMenuItem.Name = "saveNUTToolStripMenuItem";
            this.saveNUTToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveNUTToolStripMenuItem.Text = "Save NUT";
            this.saveNUTToolStripMenuItem.Click += new System.EventHandler(this.saveNUTToolStripMenuItem_Click);
            // 
            // textureToolStripMenuItem
            // 
            this.textureToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.exportAsDDSToolStripMenuItem,
            this.exportAsDDSToolStripMenuItem1,
            this.replaceToolStripMenuItem});
            this.textureToolStripMenuItem.Name = "textureToolStripMenuItem";
            this.textureToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.textureToolStripMenuItem.Text = "Texture";
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // exportAsDDSToolStripMenuItem
            // 
            this.exportAsDDSToolStripMenuItem.Name = "exportAsDDSToolStripMenuItem";
            this.exportAsDDSToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.exportAsDDSToolStripMenuItem.Text = "Export";
            this.exportAsDDSToolStripMenuItem.Click += new System.EventHandler(this.exportAsDDSToolStripMenuItem_Click);
            // 
            // exportAsDDSToolStripMenuItem1
            // 
            this.exportAsDDSToolStripMenuItem1.Name = "exportAsDDSToolStripMenuItem1";
            this.exportAsDDSToolStripMenuItem1.Size = new System.Drawing.Size(117, 22);
            this.exportAsDDSToolStripMenuItem1.Text = "Remove";
            this.exportAsDDSToolStripMenuItem1.Click += new System.EventHandler(this.RemoveToolStripMenuItem1_Click_1);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.replaceToolStripMenuItem.Text = "Replace";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.replaceToolStripMenuItem_Click);
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
            // dumpToolStripMenuItem
            // 
            this.dumpToolStripMenuItem.Name = "dumpToolStripMenuItem";
            this.dumpToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.dumpToolStripMenuItem.Text = "Export Entire NUT";
            this.dumpToolStripMenuItem.Click += new System.EventHandler(this.exportNutToFolder);
            // 
            // importNUTFromFolderToolStripMenuItem
            // 
            this.importNUTFromFolderToolStripMenuItem.Name = "importNUTFromFolderToolStripMenuItem";
            this.importNUTFromFolderToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.importNUTFromFolderToolStripMenuItem.Text = "Import Entire NUT";
            this.importNUTFromFolderToolStripMenuItem.Click += new System.EventHandler(this.importNutFromFolder);
            // 
            // NUTEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 353);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "NUTEditor";
            this.Text = "NUT Editor";
            this.Resize += new System.EventHandler(this.NUTEditor_Resize);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openNUTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newNUTToolStripMenuItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripMenuItem textureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAsDDSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAsDDSToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveNUTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAndOpenInDefaultEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAndPickAProgramToEditWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importEditedFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dumpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importNUTFromFolderToolStripMenuItem;
    }
}