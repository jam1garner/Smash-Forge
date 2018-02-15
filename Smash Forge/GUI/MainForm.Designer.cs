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
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nUTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dSTexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openVBNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openStageToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openCharacterToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.open3DSCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openNUDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reloadShadersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportErrorLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.saveConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearWorkspaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.parametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importParamsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportParamsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.clearToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.texturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.open3DSTEXEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDATTextureEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.stageLightingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importWiiUNUTAsPS3NUTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.masterpiecesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nESROMInjectorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.forgeWikiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.dockPanel1 = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.glControl1 = new OpenTK.GLControl();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.animationToolStripMenuItem,
            this.toolStripMenuItem1,
            this.parametersToolStripMenuItem,
            this.texturesToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.masterpiecesToolStripMenuItem,
            this.cameraToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1217, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openVBNToolStripMenuItem,
            this.openNUDToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.reloadShadersToolStripMenuItem,
            this.exportErrorLogToolStripMenuItem,
            this.exportModelToolStripMenuItem,
            this.toolStripSeparator4,
            this.saveConfigToolStripMenuItem,
            this.clearWorkspaceToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modelToolStripMenuItem,
            this.nUTToolStripMenuItem,
            this.dSTexToolStripMenuItem});
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.newToolStripMenuItem.Text = "New";
            // 
            // modelToolStripMenuItem
            // 
            this.modelToolStripMenuItem.Name = "modelToolStripMenuItem";
            this.modelToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.modelToolStripMenuItem.Text = "Model";
            this.modelToolStripMenuItem.Click += new System.EventHandler(this.modelToolStripMenuItem_Click);
            // 
            // nUTToolStripMenuItem
            // 
            this.nUTToolStripMenuItem.Name = "nUTToolStripMenuItem";
            this.nUTToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.nUTToolStripMenuItem.Text = "NUT";
            this.nUTToolStripMenuItem.Click += new System.EventHandler(this.nUTToolStripMenuItem_Click);
            // 
            // dSTexToolStripMenuItem
            // 
            this.dSTexToolStripMenuItem.Name = "dSTexToolStripMenuItem";
            this.dSTexToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.dSTexToolStripMenuItem.Text = "3DS Tex";
            this.dSTexToolStripMenuItem.Click += new System.EventHandler(this.dSTexToolStripMenuItem_Click);
            // 
            // openVBNToolStripMenuItem
            // 
            this.openVBNToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem,
            this.openStageToolStripMenuItem1,
            this.openCharacterToolStripMenuItem1,
            this.open3DSCharacterToolStripMenuItem});
            this.openVBNToolStripMenuItem.Name = "openVBNToolStripMenuItem";
            this.openVBNToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.openVBNToolStripMenuItem.Text = "Open";
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.openFileToolStripMenuItem.Text = "Open File";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // openStageToolStripMenuItem1
            // 
            this.openStageToolStripMenuItem1.Name = "openStageToolStripMenuItem1";
            this.openStageToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.openStageToolStripMenuItem1.Size = new System.Drawing.Size(252, 22);
            this.openStageToolStripMenuItem1.Text = "Open Stage";
            this.openStageToolStripMenuItem1.Click += new System.EventHandler(this.openStageToolStripMenuItem1_Click);
            // 
            // openCharacterToolStripMenuItem1
            // 
            this.openCharacterToolStripMenuItem1.Name = "openCharacterToolStripMenuItem1";
            this.openCharacterToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
            this.openCharacterToolStripMenuItem1.Size = new System.Drawing.Size(252, 22);
            this.openCharacterToolStripMenuItem1.Text = "Open Character";
            this.openCharacterToolStripMenuItem1.ToolTipText = "Opens supported file in a characters folder";
            this.openCharacterToolStripMenuItem1.Click += new System.EventHandler(this.openCharacterToolStripMenuItem_Click);
            // 
            // open3DSCharacterToolStripMenuItem
            // 
            this.open3DSCharacterToolStripMenuItem.Name = "open3DSCharacterToolStripMenuItem";
            this.open3DSCharacterToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D3)));
            this.open3DSCharacterToolStripMenuItem.Size = new System.Drawing.Size(252, 22);
            this.open3DSCharacterToolStripMenuItem.Text = "Open 3DS Character";
            this.open3DSCharacterToolStripMenuItem.Click += new System.EventHandler(this.open3DSCharacterToolStripMenuItem_Click);
            // 
            // openNUDToolStripMenuItem
            // 
            this.openNUDToolStripMenuItem.Name = "openNUDToolStripMenuItem";
            this.openNUDToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.openNUDToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.openNUDToolStripMenuItem.Text = "Save";
            this.openNUDToolStripMenuItem.Click += new System.EventHandler(this.openNUDToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // reloadShadersToolStripMenuItem
            // 
            this.reloadShadersToolStripMenuItem.Name = "reloadShadersToolStripMenuItem";
            this.reloadShadersToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.reloadShadersToolStripMenuItem.Text = "Reload Shaders";
            this.reloadShadersToolStripMenuItem.Click += new System.EventHandler(this.reloadShadersToolStripMenuItem_Click);
            // 
            // exportErrorLogToolStripMenuItem
            // 
            this.exportErrorLogToolStripMenuItem.Name = "exportErrorLogToolStripMenuItem";
            this.exportErrorLogToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.exportErrorLogToolStripMenuItem.Text = "Export Shader Error Logs";
            this.exportErrorLogToolStripMenuItem.Click += new System.EventHandler(this.exportErrorLogToolStripMenuItem_Click);
            // 
            // exportModelToolStripMenuItem
            // 
            this.exportModelToolStripMenuItem.Enabled = false;
            this.exportModelToolStripMenuItem.Name = "exportModelToolStripMenuItem";
            this.exportModelToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.exportModelToolStripMenuItem.Text = "Export Model";
            this.exportModelToolStripMenuItem.Visible = false;
            this.exportModelToolStripMenuItem.Click += new System.EventHandler(this.saveAsDAEToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(199, 6);
            // 
            // saveConfigToolStripMenuItem
            // 
            this.saveConfigToolStripMenuItem.Name = "saveConfigToolStripMenuItem";
            this.saveConfigToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.saveConfigToolStripMenuItem.Text = "Save Config";
            this.saveConfigToolStripMenuItem.Click += new System.EventHandler(this.saveConfigToolStripMenuItem_Click);
            // 
            // clearWorkspaceToolStripMenuItem
            // 
            this.clearWorkspaceToolStripMenuItem.Name = "clearWorkspaceToolStripMenuItem";
            this.clearWorkspaceToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.clearWorkspaceToolStripMenuItem.Text = "Clear Workspace";
            this.clearWorkspaceToolStripMenuItem.Click += new System.EventHandler(this.clearWorkspaceToolStripMenuItem_Click);
            // 
            // animationToolStripMenuItem
            // 
            this.animationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.toolStripSeparator1,
            this.clearToolStripMenuItem});
            this.animationToolStripMenuItem.Enabled = false;
            this.animationToolStripMenuItem.Name = "animationToolStripMenuItem";
            this.animationToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.animationToolStripMenuItem.Text = "Animation";
            this.animationToolStripMenuItem.Visible = false;
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
            this.toolStripMenuItem1.Enabled = false;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(64, 20);
            this.toolStripMenuItem1.Text = "Moveset";
            this.toolStripMenuItem1.Visible = false;
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
            this.exportToolStripMenuItem1.Click += new System.EventHandler(this.exportToolStripMenuItem1_Click);
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
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // parametersToolStripMenuItem
            // 
            this.parametersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importParamsToolStripMenuItem,
            this.exportParamsToolStripMenuItem,
            this.toolStripSeparator5,
            this.clearToolStripMenuItem1});
            this.parametersToolStripMenuItem.Enabled = false;
            this.parametersToolStripMenuItem.Name = "parametersToolStripMenuItem";
            this.parametersToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
            this.parametersToolStripMenuItem.Text = "Parameters";
            this.parametersToolStripMenuItem.Visible = false;
            // 
            // importParamsToolStripMenuItem
            // 
            this.importParamsToolStripMenuItem.Name = "importParamsToolStripMenuItem";
            this.importParamsToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.importParamsToolStripMenuItem.Text = "Import";
            this.importParamsToolStripMenuItem.Click += new System.EventHandler(this.importParamsToolStripMenuItem_Click);
            // 
            // exportParamsToolStripMenuItem
            // 
            this.exportParamsToolStripMenuItem.Name = "exportParamsToolStripMenuItem";
            this.exportParamsToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.exportParamsToolStripMenuItem.Text = "Export";
            this.exportParamsToolStripMenuItem.Click += new System.EventHandler(this.exportParamsToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(107, 6);
            // 
            // clearToolStripMenuItem1
            // 
            this.clearToolStripMenuItem1.Name = "clearToolStripMenuItem1";
            this.clearToolStripMenuItem1.Size = new System.Drawing.Size(110, 22);
            this.clearToolStripMenuItem1.Text = "Clear";
            this.clearToolStripMenuItem1.Click += new System.EventHandler(this.clearParamsToolStripMenuItem_Click);
            // 
            // texturesToolStripMenuItem
            // 
            this.texturesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.open3DSTEXEditorToolStripMenuItem,
            this.openDATTextureEditorToolStripMenuItem});
            this.texturesToolStripMenuItem.Enabled = false;
            this.texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
            this.texturesToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.texturesToolStripMenuItem.Text = "Textures";
            this.texturesToolStripMenuItem.Visible = false;
            // 
            // open3DSTEXEditorToolStripMenuItem
            // 
            this.open3DSTEXEditorToolStripMenuItem.Name = "open3DSTEXEditorToolStripMenuItem";
            this.open3DSTEXEditorToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.open3DSTEXEditorToolStripMenuItem.Text = "Open 3DS TEX Editor";
            this.open3DSTEXEditorToolStripMenuItem.Click += new System.EventHandler(this.open3DSTEXEditorToolStripMenuItem_Click);
            // 
            // openDATTextureEditorToolStripMenuItem
            // 
            this.openDATTextureEditorToolStripMenuItem.Name = "openDATTextureEditorToolStripMenuItem";
            this.openDATTextureEditorToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.openDATTextureEditorToolStripMenuItem.Text = "Open DAT Texture Editor";
            this.openDATTextureEditorToolStripMenuItem.Click += new System.EventHandler(this.openDATTextureEditorToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator6,
            this.stageLightingToolStripMenuItem,
            this.renderSettingsToolStripMenuItem,
            this.toolStripSeparator3,
            this.aboutToolStripMenuItem1});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(153, 6);
            // 
            // stageLightingToolStripMenuItem
            // 
            this.stageLightingToolStripMenuItem.Name = "stageLightingToolStripMenuItem";
            this.stageLightingToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.stageLightingToolStripMenuItem.Text = "Stage Lighting";
            this.stageLightingToolStripMenuItem.Click += new System.EventHandler(this.stageLightingToolStripMenuItem_Click);
            // 
            // renderSettingsToolStripMenuItem
            // 
            this.renderSettingsToolStripMenuItem.Name = "renderSettingsToolStripMenuItem";
            this.renderSettingsToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.renderSettingsToolStripMenuItem.Text = "Render Settings";
            this.renderSettingsToolStripMenuItem.Click += new System.EventHandler(this.renderSettingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(153, 6);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(156, 22);
            this.aboutToolStripMenuItem1.Text = "About";
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importWiiUNUTAsPS3NUTToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // importWiiUNUTAsPS3NUTToolStripMenuItem
            // 
            this.importWiiUNUTAsPS3NUTToolStripMenuItem.Name = "importWiiUNUTAsPS3NUTToolStripMenuItem";
            this.importWiiUNUTAsPS3NUTToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.importWiiUNUTAsPS3NUTToolStripMenuItem.Text = "Import Wii U NUT as PS3 NUT";
            this.importWiiUNUTAsPS3NUTToolStripMenuItem.Click += new System.EventHandler(this.importWiiUNUTAsPS3NUTToolStripMenuItem_Click);
            // 
            // masterpiecesToolStripMenuItem
            // 
            this.masterpiecesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nESROMInjectorToolStripMenuItem});
            this.masterpiecesToolStripMenuItem.Name = "masterpiecesToolStripMenuItem";
            this.masterpiecesToolStripMenuItem.Size = new System.Drawing.Size(88, 20);
            this.masterpiecesToolStripMenuItem.Text = "Masterpieces";
            // 
            // nESROMInjectorToolStripMenuItem
            // 
            this.nESROMInjectorToolStripMenuItem.Name = "nESROMInjectorToolStripMenuItem";
            this.nESROMInjectorToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.nESROMInjectorToolStripMenuItem.Text = "NES ROM Injector";
            this.nESROMInjectorToolStripMenuItem.Click += new System.EventHandler(this.nESROMInjectorToolStripMenuItem_Click);
            // 
            // cameraToolStripMenuItem
            // 
            this.cameraToolStripMenuItem.Enabled = false;
            this.cameraToolStripMenuItem.Name = "cameraToolStripMenuItem";
            this.cameraToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.cameraToolStripMenuItem.Text = "Camera";
            this.cameraToolStripMenuItem.Visible = false;
            this.cameraToolStripMenuItem.Click += new System.EventHandler(this.cameraToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.forgeWikiToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // forgeWikiToolStripMenuItem
            // 
            this.forgeWikiToolStripMenuItem.Name = "forgeWikiToolStripMenuItem";
            this.forgeWikiToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.forgeWikiToolStripMenuItem.Text = "Forge Wiki";
            this.forgeWikiToolStripMenuItem.Click += new System.EventHandler(this.forgeWikiToolStripMenuItem_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // dockPanel1
            // 
            this.dockPanel1.BackgroundImage = global::Smash_Forge.Properties.Resources.ForgeBack;
            this.dockPanel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.dockPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.glControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(1060, 7);
            this.glControl1.Margin = new System.Windows.Forms.Padding(4);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(10, 10);
            this.glControl1.TabIndex = 19;
            this.glControl1.VSync = false;
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(1077, 4);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(110, 17);
            this.checkBox1.TabIndex = 22;
            this.checkBox1.Text = "Big Endian Bones";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.UpdateProgress);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Control;
            this.pictureBox1.Location = new System.Drawing.Point(1187, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(24, 24);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 25;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1217, 765);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.dockPanel1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smash Forge";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_Close);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openVBNToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openNUDToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem animationToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem clearWorkspaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderSettingsToolStripMenuItem;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.ToolStripMenuItem texturesToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openStageToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openCharacterToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        public System.Windows.Forms.PictureBox pictureBox1;
        public System.Windows.Forms.ToolStripMenuItem exportModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem open3DSTEXEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem masterpiecesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nESROMInjectorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportErrorLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parametersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importParamsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem openDATTextureEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportParamsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem stageLightingToolStripMenuItem;
        public System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nUTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dSTexToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadShadersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem open3DSCharacterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem forgeWikiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importWiiUNUTAsPS3NUTToolStripMenuItem;
    }
}

