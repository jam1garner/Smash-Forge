namespace Smash_Forge.GUI.Menus
{
    partial class CameraPosition
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericHorizontalDegrees = new System.Windows.Forms.NumericUpDown();
            this.numericHorizontalRadians = new System.Windows.Forms.NumericUpDown();
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
            this.groupZoom = new System.Windows.Forms.GroupBox();
            this.numericZoom = new System.Windows.Forms.NumericUpDown();
            this.buttonApply = new System.Windows.Forms.Button();
            this.groupHorizontalRotation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericHorizontalDegrees)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericHorizontalRadians)).BeginInit();
            this.groupVerticalRotation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericVerticalRadians)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericVerticalDegrees)).BeginInit();
            this.groupPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPositionY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPositionX)).BeginInit();
            this.groupZoom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // groupHorizontalRotation
            // 
            this.groupHorizontalRotation.Controls.Add(this.numericHorizontalRadians);
            this.groupHorizontalRotation.Controls.Add(this.numericHorizontalDegrees);
            this.groupHorizontalRotation.Controls.Add(this.label2);
            this.groupHorizontalRotation.Controls.Add(this.label1);
            this.groupHorizontalRotation.Location = new System.Drawing.Point(12, 12);
            this.groupHorizontalRotation.Name = "groupHorizontalRotation";
            this.groupHorizontalRotation.Size = new System.Drawing.Size(183, 58);
            this.groupHorizontalRotation.TabIndex = 0;
            this.groupHorizontalRotation.TabStop = false;
            this.groupHorizontalRotation.Text = "Horizontal Rotation";
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(99, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Radians";
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
            // groupVerticalRotation
            // 
            this.groupVerticalRotation.Controls.Add(this.numericVerticalRadians);
            this.groupVerticalRotation.Controls.Add(this.numericVerticalDegrees);
            this.groupVerticalRotation.Controls.Add(this.label3);
            this.groupVerticalRotation.Controls.Add(this.label4);
            this.groupVerticalRotation.Location = new System.Drawing.Point(12, 76);
            this.groupVerticalRotation.Name = "groupVerticalRotation";
            this.groupVerticalRotation.Size = new System.Drawing.Size(183, 58);
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
            this.groupPosition.Controls.Add(this.numericPositionY);
            this.groupPosition.Controls.Add(this.numericPositionX);
            this.groupPosition.Controls.Add(this.label5);
            this.groupPosition.Controls.Add(this.label6);
            this.groupPosition.Location = new System.Drawing.Point(12, 140);
            this.groupPosition.Name = "groupPosition";
            this.groupPosition.Size = new System.Drawing.Size(183, 58);
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
            // groupZoom
            // 
            this.groupZoom.Controls.Add(this.numericZoom);
            this.groupZoom.Location = new System.Drawing.Point(12, 204);
            this.groupZoom.Name = "groupZoom";
            this.groupZoom.Size = new System.Drawing.Size(183, 46);
            this.groupZoom.TabIndex = 3;
            this.groupZoom.TabStop = false;
            this.groupZoom.Text = "Zoom";
            // 
            // numericZoom
            // 
            this.numericZoom.DecimalPlaces = 1;
            this.numericZoom.Location = new System.Drawing.Point(9, 19);
            this.numericZoom.Name = "numericZoom";
            this.numericZoom.Size = new System.Drawing.Size(69, 20);
            this.numericZoom.TabIndex = 2;
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(57, 256);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(100, 40);
            this.buttonApply.TabIndex = 4;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // CameraPosition
            // 
            this.AcceptButton = this.buttonApply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(213, 303);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.groupZoom);
            this.Controls.Add(this.groupPosition);
            this.Controls.Add(this.groupVerticalRotation);
            this.Controls.Add(this.groupHorizontalRotation);
            this.Name = "CameraPosition";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Camera Position";
            this.Load += new System.EventHandler(this.CameraPosition_Load);
            this.groupHorizontalRotation.ResumeLayout(false);
            this.groupHorizontalRotation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericHorizontalDegrees)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericHorizontalRadians)).EndInit();
            this.groupVerticalRotation.ResumeLayout(false);
            this.groupVerticalRotation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericVerticalRadians)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericVerticalDegrees)).EndInit();
            this.groupPosition.ResumeLayout(false);
            this.groupPosition.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericPositionY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPositionX)).EndInit();
            this.groupZoom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericZoom)).EndInit();
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
        private System.Windows.Forms.GroupBox groupZoom;
        private System.Windows.Forms.NumericUpDown numericZoom;
        private System.Windows.Forms.Button buttonApply;
    }
}