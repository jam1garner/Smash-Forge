namespace Smash_Forge
{
    partial class LVDEditor
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.name = new System.Windows.Forms.TextBox();
            this.subname = new System.Windows.Forms.TextBox();
            this.collisionGroup = new System.Windows.Forms.GroupBox();
            this.lines = new System.Windows.Forms.TreeView();
            this.vertices = new System.Windows.Forms.TreeView();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.passthroughAngle = new System.Windows.Forms.NumericUpDown();
            this.noWallJump = new System.Windows.Forms.CheckBox();
            this.rightLedge = new System.Windows.Forms.CheckBox();
            this.leftLedge = new System.Windows.Forms.CheckBox();
            this.flag4 = new System.Windows.Forms.CheckBox();
            this.flag3 = new System.Windows.Forms.CheckBox();
            this.flag2 = new System.Windows.Forms.CheckBox();
            this.flag1 = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.xVert = new System.Windows.Forms.NumericUpDown();
            this.yVert = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.xStart = new System.Windows.Forms.NumericUpDown();
            this.yStart = new System.Windows.Forms.NumericUpDown();
            this.zStart = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanel1.SuspendLayout();
            this.collisionGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.passthroughAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xVert)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yVert)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zStart)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.name);
            this.flowLayoutPanel1.Controls.Add(this.subname);
            this.flowLayoutPanel1.Controls.Add(this.collisionGroup);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(280, 810);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // name
            // 
            this.name.Location = new System.Drawing.Point(3, 3);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(277, 20);
            this.name.TabIndex = 1;
            this.name.TextChanged += new System.EventHandler(this.nameChange);
            // 
            // subname
            // 
            this.subname.Location = new System.Drawing.Point(3, 29);
            this.subname.Name = "subname";
            this.subname.Size = new System.Drawing.Size(277, 20);
            this.subname.TabIndex = 2;
            this.subname.TextChanged += new System.EventHandler(this.nameChange);
            // 
            // collisionGroup
            // 
            this.collisionGroup.Controls.Add(this.zStart);
            this.collisionGroup.Controls.Add(this.yStart);
            this.collisionGroup.Controls.Add(this.xStart);
            this.collisionGroup.Controls.Add(this.label10);
            this.collisionGroup.Controls.Add(this.label9);
            this.collisionGroup.Controls.Add(this.label8);
            this.collisionGroup.Controls.Add(this.label7);
            this.collisionGroup.Controls.Add(this.yVert);
            this.collisionGroup.Controls.Add(this.xVert);
            this.collisionGroup.Controls.Add(this.lines);
            this.collisionGroup.Controls.Add(this.vertices);
            this.collisionGroup.Controls.Add(this.comboBox1);
            this.collisionGroup.Controls.Add(this.label6);
            this.collisionGroup.Controls.Add(this.label5);
            this.collisionGroup.Controls.Add(this.passthroughAngle);
            this.collisionGroup.Controls.Add(this.noWallJump);
            this.collisionGroup.Controls.Add(this.rightLedge);
            this.collisionGroup.Controls.Add(this.leftLedge);
            this.collisionGroup.Controls.Add(this.flag4);
            this.collisionGroup.Controls.Add(this.flag3);
            this.collisionGroup.Controls.Add(this.flag2);
            this.collisionGroup.Controls.Add(this.flag1);
            this.collisionGroup.Controls.Add(this.label4);
            this.collisionGroup.Controls.Add(this.label3);
            this.collisionGroup.Controls.Add(this.label2);
            this.collisionGroup.Controls.Add(this.button2);
            this.collisionGroup.Controls.Add(this.button1);
            this.collisionGroup.Controls.Add(this.label1);
            this.collisionGroup.Location = new System.Drawing.Point(3, 55);
            this.collisionGroup.Name = "collisionGroup";
            this.collisionGroup.Size = new System.Drawing.Size(277, 557);
            this.collisionGroup.TabIndex = 0;
            this.collisionGroup.TabStop = false;
            this.collisionGroup.Text = "Collision Editing";
            // 
            // lines
            // 
            this.lines.Location = new System.Drawing.Point(6, 344);
            this.lines.Name = "lines";
            this.lines.Size = new System.Drawing.Size(259, 134);
            this.lines.TabIndex = 22;
            this.lines.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.lines_AfterSelect);
            // 
            // vertices
            // 
            this.vertices.Location = new System.Drawing.Point(6, 149);
            this.vertices.Name = "vertices";
            this.vertices.Size = new System.Drawing.Size(259, 134);
            this.vertices.TabIndex = 21;
            this.vertices.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.vertices_AfterSelect);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Iron",
            "Snow",
            "Ice",
            "Wood",
            "LargeBubbles",
            "Hurt",
            "Brick",
            "Stone2",
            "Metal2",
            "Water",
            "Bubbles",
            "Clouds",
            "Ice2",
            "NebuIron",
            "Danbouru",
            "Rock",
            "Gamewatch",
            "Grass",
            "SnowIce",
            "Fence",
            "Soil",
            "Sand",
            "MasterFortress",
            "Carpet"});
            this.comboBox1.Location = new System.Drawing.Point(10, 526);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 20;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 510);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Physics Material";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 488);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Passthrough Angle";
            // 
            // passthroughAngle
            // 
            this.passthroughAngle.Location = new System.Drawing.Point(109, 486);
            this.passthroughAngle.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.passthroughAngle.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.passthroughAngle.Name = "passthroughAngle";
            this.passthroughAngle.Size = new System.Drawing.Size(59, 20);
            this.passthroughAngle.TabIndex = 17;
            this.passthroughAngle.ValueChanged += new System.EventHandler(this.passthroughAngle_ValueChanged);
            // 
            // noWallJump
            // 
            this.noWallJump.AutoSize = true;
            this.noWallJump.Location = new System.Drawing.Point(181, 533);
            this.noWallJump.Name = "noWallJump";
            this.noWallJump.Size = new System.Drawing.Size(92, 17);
            this.noWallJump.TabIndex = 16;
            this.noWallJump.Text = "No Wall Jump";
            this.noWallJump.UseVisualStyleBackColor = true;
            this.noWallJump.CheckedChanged += new System.EventHandler(this.lineFlagChange);
            // 
            // rightLedge
            // 
            this.rightLedge.AutoSize = true;
            this.rightLedge.Location = new System.Drawing.Point(181, 510);
            this.rightLedge.Name = "rightLedge";
            this.rightLedge.Size = new System.Drawing.Size(84, 17);
            this.rightLedge.TabIndex = 15;
            this.rightLedge.Text = "Right Ledge";
            this.rightLedge.UseVisualStyleBackColor = true;
            this.rightLedge.CheckedChanged += new System.EventHandler(this.lineFlagChange);
            // 
            // leftLedge
            // 
            this.leftLedge.AutoSize = true;
            this.leftLedge.Location = new System.Drawing.Point(181, 487);
            this.leftLedge.Name = "leftLedge";
            this.leftLedge.Size = new System.Drawing.Size(77, 17);
            this.leftLedge.TabIndex = 14;
            this.leftLedge.Text = "Left Ledge";
            this.leftLedge.UseVisualStyleBackColor = true;
            this.leftLedge.CheckedChanged += new System.EventHandler(this.lineFlagChange);
            // 
            // flag4
            // 
            this.flag4.AutoSize = true;
            this.flag4.Location = new System.Drawing.Point(122, 94);
            this.flag4.Name = "flag4";
            this.flag4.Size = new System.Drawing.Size(92, 17);
            this.flag4.TabIndex = 13;
            this.flag4.Text = "Drop Through";
            this.flag4.UseVisualStyleBackColor = true;
            this.flag4.CheckedChanged += new System.EventHandler(this.flagChange);
            // 
            // flag3
            // 
            this.flag3.AutoSize = true;
            this.flag3.Location = new System.Drawing.Point(9, 94);
            this.flag3.Name = "flag3";
            this.flag3.Size = new System.Drawing.Size(49, 17);
            this.flag3.TabIndex = 12;
            this.flag3.Text = "flag3";
            this.flag3.UseVisualStyleBackColor = true;
            this.flag3.CheckedChanged += new System.EventHandler(this.flagChange);
            // 
            // flag2
            // 
            this.flag2.AutoSize = true;
            this.flag2.Location = new System.Drawing.Point(122, 71);
            this.flag2.Name = "flag2";
            this.flag2.Size = new System.Drawing.Size(83, 17);
            this.flag2.TabIndex = 11;
            this.flag2.Text = "Rig Collision";
            this.flag2.UseVisualStyleBackColor = true;
            this.flag2.CheckedChanged += new System.EventHandler(this.flagChange);
            // 
            // flag1
            // 
            this.flag1.AutoSize = true;
            this.flag1.Location = new System.Drawing.Point(9, 71);
            this.flag1.Name = "flag1";
            this.flag1.Size = new System.Drawing.Size(110, 17);
            this.flag1.TabIndex = 10;
            this.flag1.Text = "Use Start Position";
            this.flag1.UseVisualStyleBackColor = true;
            this.flag1.CheckedChanged += new System.EventHandler(this.flagChange);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(109, 328);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Lines";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(102, 298);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Y";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 298);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "X";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(212, 123);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(49, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "-";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(157, 124);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(49, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "+";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(106, 133);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Vertices";
            // 
            // xVert
            // 
            this.xVert.DecimalPlaces = 3;
            this.xVert.Location = new System.Drawing.Point(29, 296);
            this.xVert.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.xVert.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.xVert.Name = "xVert";
            this.xVert.Size = new System.Drawing.Size(67, 20);
            this.xVert.TabIndex = 23;
            // 
            // yVert
            // 
            this.yVert.DecimalPlaces = 3;
            this.yVert.Location = new System.Drawing.Point(122, 296);
            this.yVert.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.yVert.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.yVert.Name = "yVert";
            this.yVert.Size = new System.Drawing.Size(67, 20);
            this.yVert.TabIndex = 24;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(69, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "Start Position";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 45);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(14, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "X";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(92, 45);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(14, 13);
            this.label9.TabIndex = 27;
            this.label9.Text = "Y";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(175, 47);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(14, 13);
            this.label10.TabIndex = 28;
            this.label10.Text = "Z";
            // 
            // xStart
            // 
            this.xStart.DecimalPlaces = 3;
            this.xStart.Location = new System.Drawing.Point(31, 43);
            this.xStart.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.xStart.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.xStart.Name = "xStart";
            this.xStart.Size = new System.Drawing.Size(61, 20);
            this.xStart.TabIndex = 29;
            this.xStart.ValueChanged += new System.EventHandler(this.changeStart);
            // 
            // yStart
            // 
            this.yStart.DecimalPlaces = 3;
            this.yStart.Location = new System.Drawing.Point(112, 43);
            this.yStart.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.yStart.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.yStart.Name = "yStart";
            this.yStart.Size = new System.Drawing.Size(61, 20);
            this.yStart.TabIndex = 30;
            this.yStart.ValueChanged += new System.EventHandler(this.changeStart);
            // 
            // zStart
            // 
            this.zStart.DecimalPlaces = 3;
            this.zStart.Location = new System.Drawing.Point(195, 43);
            this.zStart.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.zStart.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.zStart.Name = "zStart";
            this.zStart.Size = new System.Drawing.Size(61, 20);
            this.zStart.TabIndex = 31;
            this.zStart.ValueChanged += new System.EventHandler(this.changeStart);
            // 
            // LVDEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(280, 810);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "LVDEditor";
            this.Text = "LVDEditor";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.collisionGroup.ResumeLayout(false);
            this.collisionGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.passthroughAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xVert)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yVert)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zStart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox collisionGroup;
        private System.Windows.Forms.TextBox name;
        private System.Windows.Forms.TextBox subname;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox flag4;
        private System.Windows.Forms.CheckBox flag3;
        private System.Windows.Forms.CheckBox flag2;
        private System.Windows.Forms.CheckBox flag1;
        private System.Windows.Forms.CheckBox noWallJump;
        private System.Windows.Forms.CheckBox rightLedge;
        private System.Windows.Forms.CheckBox leftLedge;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown passthroughAngle;
        private System.Windows.Forms.TreeView lines;
        private System.Windows.Forms.TreeView vertices;
        private System.Windows.Forms.NumericUpDown zStart;
        private System.Windows.Forms.NumericUpDown yStart;
        private System.Windows.Forms.NumericUpDown xStart;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown yVert;
        private System.Windows.Forms.NumericUpDown xVert;
    }
}