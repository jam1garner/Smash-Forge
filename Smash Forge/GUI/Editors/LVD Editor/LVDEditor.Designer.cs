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
            this.collisionGroup = new System.Windows.Forms.GroupBox();
            this.name = new System.Windows.Forms.TextBox();
            this.subname = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.xtext = new System.Windows.Forms.TextBox();
            this.ytext = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.flag1 = new System.Windows.Forms.CheckBox();
            this.flag2 = new System.Windows.Forms.CheckBox();
            this.flag3 = new System.Windows.Forms.CheckBox();
            this.flag4 = new System.Windows.Forms.CheckBox();
            this.leftLedge = new System.Windows.Forms.CheckBox();
            this.rightLedge = new System.Windows.Forms.CheckBox();
            this.noWallJump = new System.Windows.Forms.CheckBox();
            this.passthroughAngle = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.vertices = new System.Windows.Forms.TreeView();
            this.lines = new System.Windows.Forms.TreeView();
            this.flowLayoutPanel1.SuspendLayout();
            this.collisionGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.passthroughAngle)).BeginInit();
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
            // collisionGroup
            // 
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
            this.collisionGroup.Controls.Add(this.ytext);
            this.collisionGroup.Controls.Add(this.label3);
            this.collisionGroup.Controls.Add(this.xtext);
            this.collisionGroup.Controls.Add(this.label2);
            this.collisionGroup.Controls.Add(this.button2);
            this.collisionGroup.Controls.Add(this.button1);
            this.collisionGroup.Controls.Add(this.label1);
            this.collisionGroup.Location = new System.Drawing.Point(3, 55);
            this.collisionGroup.Name = "collisionGroup";
            this.collisionGroup.Size = new System.Drawing.Size(277, 537);
            this.collisionGroup.TabIndex = 0;
            this.collisionGroup.TabStop = false;
            this.collisionGroup.Text = "Collision Editing";
            // 
            // name
            // 
            this.name.Location = new System.Drawing.Point(3, 3);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(277, 20);
            this.name.TabIndex = 1;
            // 
            // subname
            // 
            this.subname.Location = new System.Drawing.Point(3, 29);
            this.subname.Name = "subname";
            this.subname.Size = new System.Drawing.Size(277, 20);
            this.subname.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(106, 81);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Vertices";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(157, 72);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(49, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "+";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(212, 71);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(49, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "-";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 246);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "X";
            // 
            // xtext
            // 
            this.xtext.Location = new System.Drawing.Point(34, 243);
            this.xtext.Name = "xtext";
            this.xtext.Size = new System.Drawing.Size(58, 20);
            this.xtext.TabIndex = 5;
            // 
            // ytext
            // 
            this.ytext.Location = new System.Drawing.Point(125, 243);
            this.ytext.Name = "ytext";
            this.ytext.Size = new System.Drawing.Size(58, 20);
            this.ytext.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(105, 246);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Y";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(109, 276);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Lines";
            // 
            // flag1
            // 
            this.flag1.AutoSize = true;
            this.flag1.Location = new System.Drawing.Point(9, 19);
            this.flag1.Name = "flag1";
            this.flag1.Size = new System.Drawing.Size(49, 17);
            this.flag1.TabIndex = 10;
            this.flag1.Text = "flag1";
            this.flag1.UseVisualStyleBackColor = true;
            // 
            // flag2
            // 
            this.flag2.AutoSize = true;
            this.flag2.Location = new System.Drawing.Point(64, 19);
            this.flag2.Name = "flag2";
            this.flag2.Size = new System.Drawing.Size(83, 17);
            this.flag2.TabIndex = 11;
            this.flag2.Text = "Rig Collision";
            this.flag2.UseVisualStyleBackColor = true;
            // 
            // flag3
            // 
            this.flag3.AutoSize = true;
            this.flag3.Location = new System.Drawing.Point(9, 42);
            this.flag3.Name = "flag3";
            this.flag3.Size = new System.Drawing.Size(49, 17);
            this.flag3.TabIndex = 12;
            this.flag3.Text = "flag3";
            this.flag3.UseVisualStyleBackColor = true;
            // 
            // flag4
            // 
            this.flag4.AutoSize = true;
            this.flag4.Location = new System.Drawing.Point(64, 42);
            this.flag4.Name = "flag4";
            this.flag4.Size = new System.Drawing.Size(92, 17);
            this.flag4.TabIndex = 13;
            this.flag4.Text = "Drop Through";
            this.flag4.UseVisualStyleBackColor = true;
            // 
            // leftLedge
            // 
            this.leftLedge.AutoSize = true;
            this.leftLedge.Location = new System.Drawing.Point(181, 435);
            this.leftLedge.Name = "leftLedge";
            this.leftLedge.Size = new System.Drawing.Size(77, 17);
            this.leftLedge.TabIndex = 14;
            this.leftLedge.Text = "Left Ledge";
            this.leftLedge.UseVisualStyleBackColor = true;
            // 
            // rightLedge
            // 
            this.rightLedge.AutoSize = true;
            this.rightLedge.Location = new System.Drawing.Point(181, 458);
            this.rightLedge.Name = "rightLedge";
            this.rightLedge.Size = new System.Drawing.Size(84, 17);
            this.rightLedge.TabIndex = 15;
            this.rightLedge.Text = "Right Ledge";
            this.rightLedge.UseVisualStyleBackColor = true;
            // 
            // noWallJump
            // 
            this.noWallJump.AutoSize = true;
            this.noWallJump.Location = new System.Drawing.Point(181, 481);
            this.noWallJump.Name = "noWallJump";
            this.noWallJump.Size = new System.Drawing.Size(92, 17);
            this.noWallJump.TabIndex = 16;
            this.noWallJump.Text = "No Wall Jump";
            this.noWallJump.UseVisualStyleBackColor = true;
            // 
            // passthroughAngle
            // 
            this.passthroughAngle.Location = new System.Drawing.Point(109, 434);
            this.passthroughAngle.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.passthroughAngle.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.passthroughAngle.Name = "passthroughAngle";
            this.passthroughAngle.Size = new System.Drawing.Size(59, 20);
            this.passthroughAngle.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 436);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Passthrough Angle";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 458);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Physics Material";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(10, 474);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 20;
            // 
            // vertices
            // 
            this.vertices.Location = new System.Drawing.Point(6, 97);
            this.vertices.Name = "vertices";
            this.vertices.Size = new System.Drawing.Size(259, 134);
            this.vertices.TabIndex = 21;
            this.vertices.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.vertices_AfterSelect);
            // 
            // lines
            // 
            this.lines.Location = new System.Drawing.Point(6, 292);
            this.lines.Name = "lines";
            this.lines.Size = new System.Drawing.Size(259, 134);
            this.lines.TabIndex = 22;
            this.lines.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.lines_AfterSelect);
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
        private System.Windows.Forms.TextBox ytext;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox xtext;
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
    }
}