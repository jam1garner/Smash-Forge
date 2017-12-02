namespace Smash_Forge.GUI.Menus
{
    partial class CameraSettings
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
            this.groupHorizontalRotation = new System.Windows.Forms.GroupBox();
            this.numericHorizontalRadians = new System.Windows.Forms.NumericUpDown();
            this.numericHorizontalDegrees = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupVerticalRotation = new System.Windows.Forms.GroupBox();
            this.numericVerticalRadians = new System.Windows.Forms.NumericUpDown();
            this.numericVerticalDegrees = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupPosition = new System.Windows.Forms.GroupBox();
            this.numericPositionY = new System.Windows.Forms.NumericUpDown();
            this.numericPositionX = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numericZoom = new System.Windows.Forms.NumericUpDown();
            this.buttonApply = new System.Windows.Forms.Button();
            this.fovSlider = new System.Windows.Forms.TrackBar();
            this.depthSlider = new System.Windows.Forms.TrackBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.renderDepthTB = new System.Windows.Forms.TextBox();
            this.fovTB = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupHorizontalRotation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericHorizontalRadians)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericHorizontalDegrees)).BeginInit();
            this.groupVerticalRotation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericVerticalRadians)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericVerticalDegrees)).BeginInit();
            this.groupPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPositionY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPositionX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericZoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fovSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.depthSlider)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupHorizontalRotation
            // 
            this.groupHorizontalRotation.Controls.Add(this.numericHorizontalRadians);
            this.groupHorizontalRotation.Controls.Add(this.numericHorizontalDegrees);
            this.groupHorizontalRotation.Controls.Add(this.label2);
            this.groupHorizontalRotation.Controls.Add(this.label1);
            this.groupHorizontalRotation.Location = new System.Drawing.Point(12, 164);
            this.groupHorizontalRotation.Name = "groupHorizontalRotation";
            this.groupHorizontalRotation.Size = new System.Drawing.Size(379, 58);
            this.groupHorizontalRotation.TabIndex = 0;
            this.groupHorizontalRotation.TabStop = false;
            this.groupHorizontalRotation.Text = "Horizontal Rotation";
            // 
            // numericHorizontalRadians
            // 
            this.numericHorizontalRadians.DecimalPlaces = 3;
            this.numericHorizontalRadians.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericHorizontalRadians.Location = new System.Drawing.Point(102, 32);
            this.numericHorizontalRadians.Name = "numericHorizontalRadians";
            this.numericHorizontalRadians.Size = new System.Drawing.Size(69, 20);
            this.numericHorizontalRadians.TabIndex = 3;
            this.numericHorizontalRadians.ValueChanged += new System.EventHandler(this.numericHorizontalRadians_ValueChanged);
            // 
            // numericHorizontalDegrees
            // 
            this.numericHorizontalDegrees.DecimalPlaces = 1;
            this.numericHorizontalDegrees.Location = new System.Drawing.Point(9, 32);
            this.numericHorizontalDegrees.Name = "numericHorizontalDegrees";
            this.numericHorizontalDegrees.Size = new System.Drawing.Size(69, 20);
            this.numericHorizontalDegrees.TabIndex = 2;
            this.numericHorizontalDegrees.ValueChanged += new System.EventHandler(this.numericHorizontalDegrees_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(99, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Radians";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Degrees";
            // 
            // groupVerticalRotation
            // 
            this.groupVerticalRotation.Controls.Add(this.numericVerticalRadians);
            this.groupVerticalRotation.Controls.Add(this.numericVerticalDegrees);
            this.groupVerticalRotation.Controls.Add(this.label3);
            this.groupVerticalRotation.Controls.Add(this.label4);
            this.groupVerticalRotation.Location = new System.Drawing.Point(12, 228);
            this.groupVerticalRotation.Name = "groupVerticalRotation";
            this.groupVerticalRotation.Size = new System.Drawing.Size(379, 58);
            this.groupVerticalRotation.TabIndex = 1;
            this.groupVerticalRotation.TabStop = false;
            this.groupVerticalRotation.Text = "Vertical Rotation";
            // 
            // numericVerticalRadians
            // 
            this.numericVerticalRadians.DecimalPlaces = 3;
            this.numericVerticalRadians.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericVerticalRadians.Location = new System.Drawing.Point(102, 32);
            this.numericVerticalRadians.Name = "numericVerticalRadians";
            this.numericVerticalRadians.Size = new System.Drawing.Size(69, 20);
            this.numericVerticalRadians.TabIndex = 3;
            this.numericVerticalRadians.ValueChanged += new System.EventHandler(this.numericVerticalRadians_ValueChanged);
            // 
            // numericVerticalDegrees
            // 
            this.numericVerticalDegrees.DecimalPlaces = 1;
            this.numericVerticalDegrees.Location = new System.Drawing.Point(9, 32);
            this.numericVerticalDegrees.Name = "numericVerticalDegrees";
            this.numericVerticalDegrees.Size = new System.Drawing.Size(69, 20);
            this.numericVerticalDegrees.TabIndex = 2;
            this.numericVerticalDegrees.ValueChanged += new System.EventHandler(this.numericVerticalDegrees_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(99, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Radians";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Degrees";
            // 
            // groupPosition
            // 
            this.groupPosition.Controls.Add(this.label7);
            this.groupPosition.Controls.Add(this.numericZoom);
            this.groupPosition.Controls.Add(this.numericPositionY);
            this.groupPosition.Controls.Add(this.numericPositionX);
            this.groupPosition.Controls.Add(this.label5);
            this.groupPosition.Controls.Add(this.label6);
            this.groupPosition.Location = new System.Drawing.Point(12, 292);
            this.groupPosition.Name = "groupPosition";
            this.groupPosition.Size = new System.Drawing.Size(379, 58);
            this.groupPosition.TabIndex = 2;
            this.groupPosition.TabStop = false;
            this.groupPosition.Text = "Position";
            // 
            // numericPositionY
            // 
            this.numericPositionY.DecimalPlaces = 1;
            this.numericPositionY.Location = new System.Drawing.Point(102, 32);
            this.numericPositionY.Name = "numericPositionY";
            this.numericPositionY.Size = new System.Drawing.Size(69, 20);
            this.numericPositionY.TabIndex = 3;
            // 
            // numericPositionX
            // 
            this.numericPositionX.DecimalPlaces = 1;
            this.numericPositionX.Location = new System.Drawing.Point(9, 32);
            this.numericPositionX.Name = "numericPositionX";
            this.numericPositionX.Size = new System.Drawing.Size(69, 20);
            this.numericPositionX.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(99, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Y";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(14, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "X";
            // 
            // numericZoom
            // 
            this.numericZoom.DecimalPlaces = 1;
            this.numericZoom.Location = new System.Drawing.Point(191, 32);
            this.numericZoom.Name = "numericZoom";
            this.numericZoom.Size = new System.Drawing.Size(69, 20);
            this.numericZoom.TabIndex = 2;
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(152, 366);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(100, 40);
            this.buttonApply.TabIndex = 4;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // fovSlider
            // 
            this.fovSlider.Location = new System.Drawing.Point(140, 19);
            this.fovSlider.Maximum = 180;
            this.fovSlider.Minimum = 1;
            this.fovSlider.Name = "fovSlider";
            this.fovSlider.Size = new System.Drawing.Size(233, 45);
            this.fovSlider.TabIndex = 36;
            this.fovSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.fovSlider.Value = 1;
            this.fovSlider.Scroll += new System.EventHandler(this.fovSlider_Scroll);
            // 
            // depthSlider
            // 
            this.depthSlider.Location = new System.Drawing.Point(137, 19);
            this.depthSlider.Maximum = 500000;
            this.depthSlider.Minimum = 10;
            this.depthSlider.Name = "depthSlider";
            this.depthSlider.Size = new System.Drawing.Size(236, 45);
            this.depthSlider.SmallChange = 5;
            this.depthSlider.TabIndex = 34;
            this.depthSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.depthSlider.Value = 10;
            this.depthSlider.Scroll += new System.EventHandler(this.depthSlider_Scroll);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.renderDepthTB);
            this.groupBox1.Controls.Add(this.depthSlider);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(379, 70);
            this.groupBox1.TabIndex = 38;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Render Depth";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.fovTB);
            this.groupBox2.Controls.Add(this.fovSlider);
            this.groupBox2.Location = new System.Drawing.Point(12, 88);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(379, 70);
            this.groupBox2.TabIndex = 39;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Camera FOV";
            // 
            // renderDepthTB
            // 
            this.renderDepthTB.Location = new System.Drawing.Point(6, 19);
            this.renderDepthTB.Name = "renderDepthTB";
            this.renderDepthTB.Size = new System.Drawing.Size(100, 20);
            this.renderDepthTB.TabIndex = 35;
            // 
            // fovTB
            // 
            this.fovTB.Location = new System.Drawing.Point(6, 19);
            this.fovTB.Name = "fovTB";
            this.fovTB.Size = new System.Drawing.Size(100, 20);
            this.fovTB.TabIndex = 37;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(188, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(14, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Z";
            // 
            // CameraSettings
            // 
            this.AcceptButton = this.buttonApply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 423);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.groupPosition);
            this.Controls.Add(this.groupVerticalRotation);
            this.Controls.Add(this.groupHorizontalRotation);
            this.Name = "CameraSettings";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Camera Settings";
            this.Load += new System.EventHandler(this.CameraPosition_Load);
            this.groupHorizontalRotation.ResumeLayout(false);
            this.groupHorizontalRotation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericHorizontalRadians)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericHorizontalDegrees)).EndInit();
            this.groupVerticalRotation.ResumeLayout(false);
            this.groupVerticalRotation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericVerticalRadians)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericVerticalDegrees)).EndInit();
            this.groupPosition.ResumeLayout(false);
            this.groupPosition.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPositionY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPositionX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericZoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fovSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.depthSlider)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupHorizontalRotation;
        private System.Windows.Forms.NumericUpDown numericHorizontalDegrees;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericHorizontalRadians;
        private System.Windows.Forms.GroupBox groupVerticalRotation;
        private System.Windows.Forms.NumericUpDown numericVerticalRadians;
        private System.Windows.Forms.NumericUpDown numericVerticalDegrees;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupPosition;
        private System.Windows.Forms.NumericUpDown numericPositionY;
        private System.Windows.Forms.NumericUpDown numericPositionX;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericZoom;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.TrackBar fovSlider;
        private System.Windows.Forms.TrackBar depthSlider;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox renderDepthTB;
        private System.Windows.Forms.TextBox fovTB;
        private System.Windows.Forms.Label label7;
    }
}