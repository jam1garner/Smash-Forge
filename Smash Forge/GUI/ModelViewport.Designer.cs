namespace Smash_Forge
{
    partial class ModelViewport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelViewport));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.ViewComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.weightToolButton = new System.Windows.Forms.ToolStripButton();
            this.ResetCamera = new System.Windows.Forms.ToolStripButton();
            this.CameraSettings = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.modeBone = new System.Windows.Forms.ToolStripButton();
            this.modeMesh = new System.Windows.Forms.ToolStripButton();
            this.modePolygon = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.stripPos = new System.Windows.Forms.ToolStripButton();
            this.stripRot = new System.Windows.Forms.ToolStripButton();
            this.stripSca = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.RenderButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.GIFButton = new System.Windows.Forms.ToolStripButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.animationTrackBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.totalFrame = new System.Windows.Forms.NumericUpDown();
            this.currentFrame = new System.Windows.Forms.NumericUpDown();
            this.endButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.beginButton = new System.Windows.Forms.Button();
            this.prevButton = new System.Windows.Forms.Button();
            this.playButton = new System.Windows.Forms.Button();
            this.glViewport = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(new OpenTK.Graphics.ColorFormat(8, 8, 8, 8), 24, 8, 16));
            this.toolStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.totalFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.currentFrame)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ViewComboBox,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.weightToolButton,
            this.ResetCamera,
            this.CameraSettings,
            this.toolStripSeparator3,
            this.toolStripLabel2,
            this.modeBone,
            this.modeMesh,
            this.modePolygon,
            this.toolStripSeparator1,
            this.stripPos,
            this.stripRot,
            this.stripSca,
            this.toolStripSeparator4,
            this.RenderButton,
            this.toolStripButton1,
            this.GIFButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(662, 31);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // ViewComboBox
            // 
            this.ViewComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ViewComboBox.Items.AddRange(new object[] {
            "Model Viewer",
            "Model Editor",
            "Animation Editor",
            "LVD Editor",
            "ACMD Editor",
            "Clean"});
            this.ViewComboBox.Name = "ViewComboBox";
            this.ViewComboBox.Size = new System.Drawing.Size(121, 31);
            this.ViewComboBox.ToolTipText = "The current view for the Viewport";
            this.ViewComboBox.SelectedIndexChanged += new System.EventHandler(this.ViewComboBox_SelectedIndexChanged);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(48, 28);
            this.toolStripLabel1.Text = "Camera";
            // 
            // weightToolButton
            // 
            this.weightToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.weightToolButton.Enabled = false;
            this.weightToolButton.Image = ((System.Drawing.Image)(resources.GetObject("weightToolButton.Image")));
            this.weightToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.weightToolButton.Name = "weightToolButton";
            this.weightToolButton.Size = new System.Drawing.Size(28, 28);
            this.weightToolButton.Text = "toolStripButton1";
            this.weightToolButton.Visible = false;
            this.weightToolButton.Click += new System.EventHandler(this.weightToolButton_Click);
            // 
            // ResetCamera
            // 
            this.ResetCamera.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ResetCamera.Image = global::Smash_Forge.Properties.Resources.strip_camreset;
            this.ResetCamera.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ResetCamera.Name = "ResetCamera";
            this.ResetCamera.Size = new System.Drawing.Size(28, 28);
            this.ResetCamera.Text = "Reset Camera";
            this.ResetCamera.ToolTipText = "Resets the camera to default view";
            this.ResetCamera.Click += new System.EventHandler(this.ResetCamera_Click);
            // 
            // CameraSettings
            // 
            this.CameraSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.CameraSettings.Image = global::Smash_Forge.Properties.Resources.strip_camsettings;
            this.CameraSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.CameraSettings.Name = "CameraSettings";
            this.CameraSettings.Size = new System.Drawing.Size(28, 28);
            this.CameraSettings.Text = "Camera Settings";
            this.CameraSettings.ToolTipText = "Change camera settings";
            this.CameraSettings.Click += new System.EventHandler(this.CameraSettings_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 31);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(72, 28);
            this.toolStripLabel2.Text = "Select Mode";
            // 
            // modeBone
            // 
            this.modeBone.CheckOnClick = true;
            this.modeBone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.modeBone.Image = global::Smash_Forge.Properties.Resources.icon_bone;
            this.modeBone.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.modeBone.Name = "modeBone";
            this.modeBone.Size = new System.Drawing.Size(28, 28);
            this.modeBone.Text = "toolStripButton3";
            this.modeBone.ToolTipText = "Bones";
            this.modeBone.Click += new System.EventHandler(this.viewStripButtons);
            // 
            // modeMesh
            // 
            this.modeMesh.Checked = true;
            this.modeMesh.CheckOnClick = true;
            this.modeMesh.CheckState = System.Windows.Forms.CheckState.Checked;
            this.modeMesh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.modeMesh.Image = global::Smash_Forge.Properties.Resources.icon_mesh;
            this.modeMesh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.modeMesh.Name = "modeMesh";
            this.modeMesh.Size = new System.Drawing.Size(28, 28);
            this.modeMesh.Text = "toolStripButton1";
            this.modeMesh.ToolTipText = "Mesh";
            this.modeMesh.Click += new System.EventHandler(this.viewStripButtons);
            // 
            // modePolygon
            // 
            this.modePolygon.CheckOnClick = true;
            this.modePolygon.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.modePolygon.Enabled = false;
            this.modePolygon.Image = global::Smash_Forge.Properties.Resources.icon_polygon;
            this.modePolygon.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.modePolygon.Name = "modePolygon";
            this.modePolygon.Size = new System.Drawing.Size(28, 28);
            this.modePolygon.Text = "toolStripButton2";
            this.modePolygon.ToolTipText = "Polygons";
            this.modePolygon.Click += new System.EventHandler(this.viewStripButtons);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // stripPos
            // 
            this.stripPos.Checked = true;
            this.stripPos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.stripPos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stripPos.Image = global::Smash_Forge.Properties.Resources.strip_pos;
            this.stripPos.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stripPos.Name = "stripPos";
            this.stripPos.Size = new System.Drawing.Size(28, 28);
            this.stripPos.Text = "toolStripButton2";
            this.stripPos.ToolTipText = "Position";
            this.stripPos.Click += new System.EventHandler(this.viewStripButtonsBone);
            // 
            // stripRot
            // 
            this.stripRot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stripRot.Image = global::Smash_Forge.Properties.Resources.strip_rot;
            this.stripRot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stripRot.Name = "stripRot";
            this.stripRot.Size = new System.Drawing.Size(28, 28);
            this.stripRot.Text = "toolStripButton3";
            this.stripRot.ToolTipText = "Rotation";
            this.stripRot.Click += new System.EventHandler(this.viewStripButtonsBone);
            // 
            // stripSca
            // 
            this.stripSca.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stripSca.Image = global::Smash_Forge.Properties.Resources.strip_sca;
            this.stripSca.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stripSca.Name = "stripSca";
            this.stripSca.Size = new System.Drawing.Size(28, 28);
            this.stripSca.Text = "toolStripButton4";
            this.stripSca.ToolTipText = "Scale";
            this.stripSca.Click += new System.EventHandler(this.viewStripButtonsBone);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 31);
            // 
            // RenderButton
            // 
            this.RenderButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.RenderButton.Image = global::Smash_Forge.Properties.Resources.strip_render;
            this.RenderButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RenderButton.Name = "RenderButton";
            this.RenderButton.Size = new System.Drawing.Size(28, 28);
            this.RenderButton.Text = "toolStripButton1";
            this.RenderButton.ToolTipText = "Render Viewport to File";
            this.RenderButton.Click += new System.EventHandler(this.RenderButton_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::Smash_Forge.Properties.Resources.strip_render;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(28, 28);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.ToolTipText = "Render Viewport to File (No Alpha)";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // GIFButton
            // 
            this.GIFButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.GIFButton.Image = global::Smash_Forge.Properties.Resources.strip_gif;
            this.GIFButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.GIFButton.Name = "GIFButton";
            this.GIFButton.Size = new System.Drawing.Size(28, 28);
            this.GIFButton.Text = "toolStripButton2";
            this.GIFButton.ToolTipText = "Render GIF of Current Animation";
            this.GIFButton.Click += new System.EventHandler(this.GIFButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.animationTrackBar);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.totalFrame);
            this.groupBox1.Controls.Add(this.currentFrame);
            this.groupBox1.Controls.Add(this.endButton);
            this.groupBox1.Controls.Add(this.nextButton);
            this.groupBox1.Controls.Add(this.beginButton);
            this.groupBox1.Controls.Add(this.prevButton);
            this.groupBox1.Controls.Add(this.playButton);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 348);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(662, 120);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Animation Controls";
            // 
            // animationTrackBar
            // 
            this.animationTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.animationTrackBar.Location = new System.Drawing.Point(12, 16);
            this.animationTrackBar.Name = "animationTrackBar";
            this.animationTrackBar.Size = new System.Drawing.Size(638, 45);
            this.animationTrackBar.TabIndex = 9;
            this.animationTrackBar.ValueChanged += new System.EventHandler(this.animationTrackBar_ValueChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(479, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Frame:";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(580, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(12, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "/";
            // 
            // totalFrame
            // 
            this.totalFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.totalFrame.Enabled = false;
            this.totalFrame.Location = new System.Drawing.Point(602, 67);
            this.totalFrame.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.totalFrame.Name = "totalFrame";
            this.totalFrame.Size = new System.Drawing.Size(54, 20);
            this.totalFrame.TabIndex = 6;
            this.totalFrame.ValueChanged += new System.EventHandler(this.totalFrame_ValueChanged);
            // 
            // currentFrame
            // 
            this.currentFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.currentFrame.Location = new System.Drawing.Point(520, 67);
            this.currentFrame.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.currentFrame.Name = "currentFrame";
            this.currentFrame.Size = new System.Drawing.Size(54, 20);
            this.currentFrame.TabIndex = 5;
            this.currentFrame.ValueChanged += new System.EventHandler(this.currentFrame_ValueChanged);
            // 
            // endButton
            // 
            this.endButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.endButton.Location = new System.Drawing.Point(437, 64);
            this.endButton.Name = "endButton";
            this.endButton.Size = new System.Drawing.Size(38, 45);
            this.endButton.TabIndex = 4;
            this.endButton.Text = ">>";
            this.endButton.UseVisualStyleBackColor = true;
            this.endButton.Click += new System.EventHandler(this.endButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nextButton.Location = new System.Drawing.Point(399, 64);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(32, 45);
            this.nextButton.TabIndex = 3;
            this.nextButton.Text = ">";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // beginButton
            // 
            this.beginButton.Location = new System.Drawing.Point(6, 64);
            this.beginButton.Name = "beginButton";
            this.beginButton.Size = new System.Drawing.Size(38, 44);
            this.beginButton.TabIndex = 2;
            this.beginButton.Text = "<<";
            this.beginButton.UseVisualStyleBackColor = true;
            this.beginButton.Click += new System.EventHandler(this.beginButton_Click);
            // 
            // prevButton
            // 
            this.prevButton.Location = new System.Drawing.Point(50, 64);
            this.prevButton.Name = "prevButton";
            this.prevButton.Size = new System.Drawing.Size(34, 44);
            this.prevButton.TabIndex = 1;
            this.prevButton.Text = "<";
            this.prevButton.UseVisualStyleBackColor = true;
            this.prevButton.Click += new System.EventHandler(this.prevButton_Click);
            // 
            // playButton
            // 
            this.playButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.playButton.Location = new System.Drawing.Point(90, 64);
            this.playButton.MinimumSize = new System.Drawing.Size(44, 44);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(303, 44);
            this.playButton.TabIndex = 0;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // glViewport
            // 
            this.glViewport.BackColor = System.Drawing.Color.Black;
            this.glViewport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glViewport.Location = new System.Drawing.Point(0, 0);
            this.glViewport.Name = "glViewport";
            this.glViewport.Size = new System.Drawing.Size(662, 468);
            this.glViewport.TabIndex = 0;
            this.glViewport.VSync = false;
            this.glViewport.Click += new System.EventHandler(this.glViewport_Click);
            this.glViewport.Paint += new System.Windows.Forms.PaintEventHandler(this.Render);
            this.glViewport.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.glViewport_MouseDoubleClick);
            this.glViewport.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glViewport_MouseMove);
            this.glViewport.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glViewport_MouseUp);
            this.glViewport.Resize += new System.EventHandler(this.glViewport_Resize);
            // 
            // ModelViewport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 468);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.glViewport);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ModelViewport";
            this.Text = "ModelViewport";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModelViewport_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ModelViewport_FormClosed);
            this.Load += new System.EventHandler(this.ModelViewport_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ModelViewport_KeyPress);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.totalFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.currentFrame)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenTK.GLControl glViewport;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Button endButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button beginButton;
        private System.Windows.Forms.Button prevButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown totalFrame;
        private System.Windows.Forms.NumericUpDown currentFrame;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton weightToolButton;
        private System.Windows.Forms.TrackBar animationTrackBar;
        private System.Windows.Forms.ToolStripButton ResetCamera;
        private System.Windows.Forms.ToolStripButton CameraSettings;
        public System.Windows.Forms.ToolStripComboBox ViewComboBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton RenderButton;
        private System.Windows.Forms.ToolStripButton GIFButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripButton modeBone;
        private System.Windows.Forms.ToolStripButton modeMesh;
        private System.Windows.Forms.ToolStripButton modePolygon;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton stripPos;
        private System.Windows.Forms.ToolStripButton stripRot;
        private System.Windows.Forms.ToolStripButton stripSca;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    }
}