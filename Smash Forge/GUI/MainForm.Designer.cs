namespace Smash_Forge
{
	partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openVBNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openStageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openNUDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearWorkspaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveNUDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.edittToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addBoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hashMatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collisionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spawnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.respawnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cameraBoundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blastzonesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.propertiesWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animationsWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boneTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewportWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.meshListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectTreeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.dockPanel1 = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.glControl1 = new OpenTK.GLControl();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.edittToolStripMenuItem,
            this.animationToolStripMenuItem,
            this.toolStripMenuItem1,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1217, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openVBNToolStripMenuItem,
            this.openStageToolStripMenuItem,
            this.openCharacterToolStripMenuItem,
            this.openNUDToolStripMenuItem,
            this.clearWorkspaceToolStripMenuItem,
            this.saveNUDToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openVBNToolStripMenuItem
            // 
            this.openVBNToolStripMenuItem.Name = "openVBNToolStripMenuItem";
            this.openVBNToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openVBNToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.openVBNToolStripMenuItem.Text = "Open";
            this.openVBNToolStripMenuItem.Click += new System.EventHandler(this.openVBNToolStripMenuItem_Click);
            // 
            // openStageToolStripMenuItem
            // 
            this.openStageToolStripMenuItem.Name = "openStageToolStripMenuItem";
            this.openStageToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.openStageToolStripMenuItem.Text = "Open Stage";
            this.openStageToolStripMenuItem.Click += new System.EventHandler(this.openStageToolStripMenuItem_Click);
            // 
            // openCharacterToolStripMenuItem
            // 
            this.openCharacterToolStripMenuItem.Name = "openCharacterToolStripMenuItem";
            this.openCharacterToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.openCharacterToolStripMenuItem.Text = "Open Character";
            this.openCharacterToolStripMenuItem.Click += new System.EventHandler(this.openCharacterToolStripMenuItem_Click);
            // 
            // openNUDToolStripMenuItem
            // 
            this.openNUDToolStripMenuItem.Name = "openNUDToolStripMenuItem";
            this.openNUDToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.openNUDToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.openNUDToolStripMenuItem.Text = "Save";
            this.openNUDToolStripMenuItem.Click += new System.EventHandler(this.openNUDToolStripMenuItem_Click);
            // 
            // clearWorkspaceToolStripMenuItem
            // 
            this.clearWorkspaceToolStripMenuItem.Name = "clearWorkspaceToolStripMenuItem";
            this.clearWorkspaceToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.clearWorkspaceToolStripMenuItem.Text = "Clear Workspace";
            this.clearWorkspaceToolStripMenuItem.Click += new System.EventHandler(this.clearWorkspaceToolStripMenuItem_Click);
            // 
            // saveNUDToolStripMenuItem
            // 
            this.saveNUDToolStripMenuItem.Name = "saveNUDToolStripMenuItem";
            this.saveNUDToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.saveNUDToolStripMenuItem.Text = "Save NUD";
            this.saveNUDToolStripMenuItem.Click += new System.EventHandler(this.saveNUDToolStripMenuItem_Click);
            // 
            // edittToolStripMenuItem
            // 
            this.edittToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addBoneToolStripMenuItem,
            this.hashMatchToolStripMenuItem,
            this.addToolStripMenuItem});
            this.edittToolStripMenuItem.Name = "edittToolStripMenuItem";
            this.edittToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.edittToolStripMenuItem.Text = "Edit";
            // 
            // addBoneToolStripMenuItem
            // 
            this.addBoneToolStripMenuItem.Name = "addBoneToolStripMenuItem";
            this.addBoneToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.addBoneToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.addBoneToolStripMenuItem.Text = "Add Bone";
            this.addBoneToolStripMenuItem.Click += new System.EventHandler(this.addBoneToolStripMenuItem_Click);
            // 
            // hashMatchToolStripMenuItem
            // 
            this.hashMatchToolStripMenuItem.Name = "hashMatchToolStripMenuItem";
            this.hashMatchToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.hashMatchToolStripMenuItem.Text = "Hash Match";
            this.hashMatchToolStripMenuItem.Click += new System.EventHandler(this.hashMatchToolStripMenuItem_Click);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.collisionToolStripMenuItem,
            this.spawnToolStripMenuItem,
            this.respawnToolStripMenuItem,
            this.cameraBoundsToolStripMenuItem,
            this.blastzonesToolStripMenuItem});
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.addToolStripMenuItem.Text = "Add";
            // 
            // collisionToolStripMenuItem
            // 
            this.collisionToolStripMenuItem.Name = "collisionToolStripMenuItem";
            this.collisionToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.collisionToolStripMenuItem.Text = "Collision";
            this.collisionToolStripMenuItem.Click += new System.EventHandler(this.collisionToolStripMenuItem_Click);
            // 
            // spawnToolStripMenuItem
            // 
            this.spawnToolStripMenuItem.Name = "spawnToolStripMenuItem";
            this.spawnToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.spawnToolStripMenuItem.Text = "Spawn";
            this.spawnToolStripMenuItem.Click += new System.EventHandler(this.spawnToolStripMenuItem_Click);
            // 
            // respawnToolStripMenuItem
            // 
            this.respawnToolStripMenuItem.Name = "respawnToolStripMenuItem";
            this.respawnToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.respawnToolStripMenuItem.Text = "Respawn";
            this.respawnToolStripMenuItem.Click += new System.EventHandler(this.respawnToolStripMenuItem_Click);
            // 
            // cameraBoundsToolStripMenuItem
            // 
            this.cameraBoundsToolStripMenuItem.Name = "cameraBoundsToolStripMenuItem";
            this.cameraBoundsToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.cameraBoundsToolStripMenuItem.Text = "Camera Bounds";
            this.cameraBoundsToolStripMenuItem.Click += new System.EventHandler(this.cameraBoundsToolStripMenuItem_Click);
            // 
            // blastzonesToolStripMenuItem
            // 
            this.blastzonesToolStripMenuItem.Name = "blastzonesToolStripMenuItem";
            this.blastzonesToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.blastzonesToolStripMenuItem.Text = "Blastzones";
            this.blastzonesToolStripMenuItem.Click += new System.EventHandler(this.blastzonesToolStripMenuItem_Click);
            // 
            // animationToolStripMenuItem
            // 
            this.animationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripSeparator1,
            this.clearToolStripMenuItem});
            this.animationToolStripMenuItem.Name = "animationToolStripMenuItem";
            this.animationToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.animationToolStripMenuItem.Text = "Animation";
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(107, 6);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem1,
            this.exportToolStripMenuItem1,
            this.toolStripSeparator2,
            this.toolStripMenuItem2});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(64, 20);
            this.toolStripMenuItem1.Text = "Moveset";
            // 
            // importToolStripMenuItem1
            // 
            this.importToolStripMenuItem1.Name = "importToolStripMenuItem1";
            this.importToolStripMenuItem1.Size = new System.Drawing.Size(110, 22);
            this.importToolStripMenuItem1.Text = "Import";
            this.importToolStripMenuItem1.Click += new System.EventHandler(this.importToolStripMenuItem1_Click);
            // 
            // exportToolStripMenuItem1
            // 
            this.exportToolStripMenuItem1.Name = "exportToolStripMenuItem1";
            this.exportToolStripMenuItem1.Size = new System.Drawing.Size(110, 22);
            this.exportToolStripMenuItem1.Text = "Export";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(107, 6);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(110, 22);
            this.toolStripMenuItem2.Text = "Clear";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.propertiesWindowToolStripMenuItem,
            this.animationsWindowToolStripMenuItem,
            this.boneTreeToolStripMenuItem,
            this.viewportWindowToolStripMenuItem,
            this.meshListToolStripMenuItem,
            this.projectTreeToolStripMenuItem,
            this.renderSettingsToolStripMenuItem,
            this.toolStripSeparator3,
            this.aboutToolStripMenuItem1});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // propertiesWindowToolStripMenuItem
            // 
            this.propertiesWindowToolStripMenuItem.CheckOnClick = true;
            this.propertiesWindowToolStripMenuItem.Name = "propertiesWindowToolStripMenuItem";
            this.propertiesWindowToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.propertiesWindowToolStripMenuItem.Text = "Properties Window";
            // 
            // animationsWindowToolStripMenuItem
            // 
            this.animationsWindowToolStripMenuItem.CheckOnClick = true;
            this.animationsWindowToolStripMenuItem.Name = "animationsWindowToolStripMenuItem";
            this.animationsWindowToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.animationsWindowToolStripMenuItem.Text = "Animations";
            this.animationsWindowToolStripMenuItem.Click += new System.EventHandler(this.animationsWindowToolStripMenuItem_Click);
            // 
            // boneTreeToolStripMenuItem
            // 
            this.boneTreeToolStripMenuItem.Name = "boneTreeToolStripMenuItem";
            this.boneTreeToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.boneTreeToolStripMenuItem.Text = "Bone Tree";
            this.boneTreeToolStripMenuItem.Click += new System.EventHandler(this.boneTreeToolStripMenuItem_Click);
            // 
            // viewportWindowToolStripMenuItem
            // 
            this.viewportWindowToolStripMenuItem.Name = "viewportWindowToolStripMenuItem";
            this.viewportWindowToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.viewportWindowToolStripMenuItem.Text = "Viewport";
            this.viewportWindowToolStripMenuItem.Click += new System.EventHandler(this.viewportWindowToolStripMenuItem_Click);
            // 
            // meshListToolStripMenuItem
            // 
            this.meshListToolStripMenuItem.Name = "meshListToolStripMenuItem";
            this.meshListToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.meshListToolStripMenuItem.Text = "Mesh List";
            this.meshListToolStripMenuItem.Click += new System.EventHandler(this.meshListToolStripMenuItem_Click);
            // 
            // projectTreeToolStripMenuItem
            // 
            this.projectTreeToolStripMenuItem.Name = "projectTreeToolStripMenuItem";
            this.projectTreeToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.projectTreeToolStripMenuItem.Text = "Project Tree";
            this.projectTreeToolStripMenuItem.Click += new System.EventHandler(this.projectTreeToolStripMenuItem_Click);
            // 
            // renderSettingsToolStripMenuItem
            // 
            this.renderSettingsToolStripMenuItem.Name = "renderSettingsToolStripMenuItem";
            this.renderSettingsToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.renderSettingsToolStripMenuItem.Text = "Render Settings";
            this.renderSettingsToolStripMenuItem.Click += new System.EventHandler(this.renderSettingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(171, 6);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(174, 22);
            this.aboutToolStripMenuItem1.Text = "About";
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(947, 4);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(53, 17);
            this.radioButton1.TabIndex = 9;
            this.radioButton1.Text = "Stage";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Checked = true;
            this.radioButton2.Location = new System.Drawing.Point(870, 4);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(71, 17);
            this.radioButton2.TabIndex = 10;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Character";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // dockPanel1
            // 
            this.dockPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel1.DockBackColor = System.Drawing.SystemColors.AppWorkspace;
            this.dockPanel1.DockBottomPortion = 150D;
            this.dockPanel1.DockLeftPortion = 200D;
            this.dockPanel1.DockRightPortion = 290D;
            this.dockPanel1.DockTopPortion = 150D;
            this.dockPanel1.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World, ((byte)(0)));
            this.dockPanel1.Location = new System.Drawing.Point(0, 24);
            this.dockPanel1.Name = "dockPanel1";
            this.dockPanel1.RightToLeftLayout = true;
            this.dockPanel1.Size = new System.Drawing.Size(1217, 741);
            this.dockPanel1.TabIndex = 16;
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(12, 27);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(10, 10);
            this.glControl1.TabIndex = 19;
            this.glControl1.VSync = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1217, 765);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.dockPanel1);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.menuStrip1);
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smash Forge";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_Close);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openVBNToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openNUDToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem edittToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addBoneToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem animationToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.ToolStripMenuItem hashMatchToolStripMenuItem;
        private VBNViewport Viewport;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel1;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem propertiesWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem animationsWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewportWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem boneTreeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openStageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearWorkspaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem meshListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectTreeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openCharacterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveNUDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collisionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spawnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem respawnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cameraBoundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blastzonesToolStripMenuItem;
        private OpenTK.GLControl glControl1;
    }
}

