namespace Smash_Forge
{
    partial class MeshList
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
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.polyContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editMaterialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyMaterialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.flipUVsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flipUVsHorizontalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smoothNormalsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.detachToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.meshContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mergeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.belowToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.singleBindToBoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.nudContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeMetalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.mergeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.belowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openEditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.materialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAsXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFromXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addBlankMeshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.polyContextMenu.SuspendLayout();
            this.meshContextMenu.SuspendLayout();
            this.nudContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.CheckBoxes = true;
            this.treeView1.HideSelection = false;
            this.treeView1.LabelEdit = true;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(199, 280);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView1_AfterLabelEdit);
            this.treeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCheck);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView1_KeyDown);
            this.treeView1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.treeView1_KeyPress);
            this.treeView1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView1_MouseDown);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numericUpDown1.Location = new System.Drawing.Point(79, 286);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(117, 20);
            this.numericUpDown1.TabIndex = 1;
            this.numericUpDown1.Visible = false;
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 288);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Mesh Index";
            this.label1.Visible = false;
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.Location = new System.Drawing.Point(46, 283);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(105, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Match To NUD";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // polyContextMenu
            // 
            this.polyContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editMaterialToolStripMenuItem,
            this.copyMaterialToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteToolStripMenuItem,
            this.duplicateToolStripMenuItem,
            this.toolStripSeparator2,
            this.flipUVsToolStripMenuItem,
            this.flipUVsHorizontalToolStripMenuItem,
            this.smoothNormalsToolStripMenuItem,
            this.toolStripSeparator3,
            this.detachToolStripMenuItem});
            this.polyContextMenu.Name = "polyContextMenu";
            this.polyContextMenu.Size = new System.Drawing.Size(183, 198);
            this.polyContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // editMaterialToolStripMenuItem
            // 
            this.editMaterialToolStripMenuItem.Name = "editMaterialToolStripMenuItem";
            this.editMaterialToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.editMaterialToolStripMenuItem.Text = "Edit Material";
            this.editMaterialToolStripMenuItem.Click += new System.EventHandler(this.editMaterialToolStripMenuItem_Click);
            // 
            // copyMaterialToolStripMenuItem
            // 
            this.copyMaterialToolStripMenuItem.Name = "copyMaterialToolStripMenuItem";
            this.copyMaterialToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.copyMaterialToolStripMenuItem.Text = "Copy Material";
            this.copyMaterialToolStripMenuItem.Click += new System.EventHandler(this.copyMaterialToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(179, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // duplicateToolStripMenuItem
            // 
            this.duplicateToolStripMenuItem.Name = "duplicateToolStripMenuItem";
            this.duplicateToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.duplicateToolStripMenuItem.Text = "Duplicate";
            this.duplicateToolStripMenuItem.Visible = false;
            this.duplicateToolStripMenuItem.Click += new System.EventHandler(this.duplicateToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(179, 6);
            // 
            // flipUVsToolStripMenuItem
            // 
            this.flipUVsToolStripMenuItem.Name = "flipUVsToolStripMenuItem";
            this.flipUVsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.flipUVsToolStripMenuItem.Text = "Flip UVs (Vertical)";
            this.flipUVsToolStripMenuItem.Click += new System.EventHandler(this.flipUVsToolStripMenuItem_Click);
            // 
            // flipUVsHorizontalToolStripMenuItem
            // 
            this.flipUVsHorizontalToolStripMenuItem.Name = "flipUVsHorizontalToolStripMenuItem";
            this.flipUVsHorizontalToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.flipUVsHorizontalToolStripMenuItem.Text = "Flip UVs (Horizontal)";
            this.flipUVsHorizontalToolStripMenuItem.Click += new System.EventHandler(this.flipUVsHorizontalToolStripMenuItem_Click);
            // 
            // smoothNormalsToolStripMenuItem
            // 
            this.smoothNormalsToolStripMenuItem.Name = "smoothNormalsToolStripMenuItem";
            this.smoothNormalsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.smoothNormalsToolStripMenuItem.Text = "Smooth Normals";
            this.smoothNormalsToolStripMenuItem.Click += new System.EventHandler(this.smoothNormalsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(179, 6);
            // 
            // detachToolStripMenuItem
            // 
            this.detachToolStripMenuItem.Name = "detachToolStripMenuItem";
            this.detachToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.detachToolStripMenuItem.Text = "Detach";
            this.detachToolStripMenuItem.Click += new System.EventHandler(this.detachToolStripMenuItem_Click);
            // 
            // meshContextMenu
            // 
            this.meshContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mergeToolStripMenuItem1,
            this.toolStripMenuItem1,
            this.singleBindToBoneToolStripMenuItem,
            this.toolStripSeparator5,
            this.deleteToolStripMenuItem1});
            this.meshContextMenu.Name = "meshContextMenu";
            this.meshContextMenu.Size = new System.Drawing.Size(178, 98);
            // 
            // mergeToolStripMenuItem1
            // 
            this.mergeToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboveToolStripMenuItem1,
            this.belowToolStripMenuItem1});
            this.mergeToolStripMenuItem1.Name = "mergeToolStripMenuItem1";
            this.mergeToolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
            this.mergeToolStripMenuItem1.Text = "Merge";
            // 
            // aboveToolStripMenuItem1
            // 
            this.aboveToolStripMenuItem1.Name = "aboveToolStripMenuItem1";
            this.aboveToolStripMenuItem1.Size = new System.Drawing.Size(108, 22);
            this.aboveToolStripMenuItem1.Text = "Above";
            this.aboveToolStripMenuItem1.Click += new System.EventHandler(this.aboveToolStripMenuItem1_Click);
            // 
            // belowToolStripMenuItem1
            // 
            this.belowToolStripMenuItem1.Name = "belowToolStripMenuItem1";
            this.belowToolStripMenuItem1.Size = new System.Drawing.Size(108, 22);
            this.belowToolStripMenuItem1.Text = "Below";
            this.belowToolStripMenuItem1.Click += new System.EventHandler(this.belowToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
            this.toolStripMenuItem1.Text = "Reposition";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // singleBindToBoneToolStripMenuItem
            // 
            this.singleBindToBoneToolStripMenuItem.Name = "singleBindToBoneToolStripMenuItem";
            this.singleBindToBoneToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.singleBindToBoneToolStripMenuItem.Text = "Single Bind to Bone";
            this.singleBindToBoneToolStripMenuItem.Click += new System.EventHandler(this.singleBindToBoneToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(174, 6);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
            this.deleteToolStripMenuItem1.Text = "Delete";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.deleteToolStripMenuItem1_Click);
            // 
            // nudContextMenu
            // 
            this.nudContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.makeMetalToolStripMenuItem,
            this.materialToolStripMenuItem,
            this.toolStripSeparator4,
            this.mergeToolStripMenuItem,
            this.addBlankMeshToolStripMenuItem,
            this.openEditToolStripMenuItem});
            this.nudContextMenu.Name = "nudContextMenu";
            this.nudContextMenu.Size = new System.Drawing.Size(161, 164);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveToolStripMenuItem.Text = "Save as NUD";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // makeMetalToolStripMenuItem
            // 
            this.makeMetalToolStripMenuItem.Name = "makeMetalToolStripMenuItem";
            this.makeMetalToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.makeMetalToolStripMenuItem.Text = "Make Metal";
            this.makeMetalToolStripMenuItem.Click += new System.EventHandler(this.makeMetalToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(149, 6);
            // 
            // mergeToolStripMenuItem
            // 
            this.mergeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.belowToolStripMenuItem,
            this.aboveToolStripMenuItem});
            this.mergeToolStripMenuItem.Name = "mergeToolStripMenuItem";
            this.mergeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.mergeToolStripMenuItem.Text = "Merge";
            // 
            // belowToolStripMenuItem
            // 
            this.belowToolStripMenuItem.Name = "belowToolStripMenuItem";
            this.belowToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.belowToolStripMenuItem.Text = "Below";
            this.belowToolStripMenuItem.Click += new System.EventHandler(this.belowToolStripMenuItem_Click);
            // 
            // aboveToolStripMenuItem
            // 
            this.aboveToolStripMenuItem.Name = "aboveToolStripMenuItem";
            this.aboveToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.aboveToolStripMenuItem.Text = "Above";
            this.aboveToolStripMenuItem.Click += new System.EventHandler(this.aboveToolStripMenuItem_Click);
            // 
            // openEditToolStripMenuItem
            // 
            this.openEditToolStripMenuItem.Enabled = false;
            this.openEditToolStripMenuItem.Name = "openEditToolStripMenuItem";
            this.openEditToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openEditToolStripMenuItem.Text = "Open Edit";
            this.openEditToolStripMenuItem.Click += new System.EventHandler(this.openEditToolStripMenuItem_Click);
            // 
            // materialToolStripMenuItem
            // 
            this.materialToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportAsXMLToolStripMenuItem,
            this.importFromXMLToolStripMenuItem});
            this.materialToolStripMenuItem.Name = "materialToolStripMenuItem";
            this.materialToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.materialToolStripMenuItem.Text = "Material";
            // 
            // exportAsXMLToolStripMenuItem
            // 
            this.exportAsXMLToolStripMenuItem.Name = "exportAsXMLToolStripMenuItem";
            this.exportAsXMLToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.exportAsXMLToolStripMenuItem.Text = "Export as XML";
            this.exportAsXMLToolStripMenuItem.Click += new System.EventHandler(this.exportAsXMLToolStripMenuItem_Click);
            // 
            // importFromXMLToolStripMenuItem
            // 
            this.importFromXMLToolStripMenuItem.Name = "importFromXMLToolStripMenuItem";
            this.importFromXMLToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.importFromXMLToolStripMenuItem.Text = "Import from XML";
            this.importFromXMLToolStripMenuItem.Click += new System.EventHandler(this.importFromXMLToolStripMenuItem_Click);
            // 
            // addBlankMeshToolStripMenuItem
            // 
            this.addBlankMeshToolStripMenuItem.Name = "addBlankMeshToolStripMenuItem";
            this.addBlankMeshToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.addBlankMeshToolStripMenuItem.Text = "Add Blank Mesh";
            this.addBlankMeshToolStripMenuItem.Click += new System.EventHandler(this.addBlankMeshToolStripMenuItem_Click);
            // 
            // MeshList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(199, 306);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.treeView1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MeshList";
            this.Text = "MeshList";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.polyContextMenu.ResumeLayout(false);
            this.meshContextMenu.ResumeLayout(false);
            this.nudContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ContextMenuStrip polyContextMenu;
        private System.Windows.Forms.ToolStripMenuItem editMaterialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flipUVsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip meshContextMenu;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem singleBindToBoneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem duplicateToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip nudContextMenu;
        private System.Windows.Forms.ToolStripMenuItem makeMetalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem belowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem smoothNormalsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyMaterialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openEditToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem detachToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mergeToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboveToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem belowToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem flipUVsHorizontalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem materialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAsXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importFromXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addBlankMeshToolStripMenuItem;
    }
}