namespace Smash_Forge
{
    partial class MeshMover
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.posIncBox = new System.Windows.Forms.TextBox();
            this.posXTB = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.posYTB = new System.Windows.Forms.TrackBar();
            this.posZTB = new System.Windows.Forms.TrackBar();
            this.rotZTB = new System.Windows.Forms.TrackBar();
            this.rotYTB = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.rotXTB = new System.Windows.Forms.TrackBar();
            this.rotIncBox = new System.Windows.Forms.TextBox();
            this.scaIncBox = new System.Windows.Forms.TextBox();
            this.scaXTB = new System.Windows.Forms.TrackBar();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.posXTB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.posYTB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.posZTB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotZTB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotYTB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotXTB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scaXTB)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.posZTB);
            this.groupBox1.Controls.Add(this.posYTB);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.posXTB);
            this.groupBox1.Controls.Add(this.posIncBox);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(294, 178);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Position";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rotIncBox);
            this.groupBox2.Controls.Add(this.rotZTB);
            this.groupBox2.Controls.Add(this.rotYTB);
            this.groupBox2.Controls.Add(this.rotXTB);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 178);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(294, 348);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Rotate";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.scaIncBox);
            this.groupBox3.Controls.Add(this.scaXTB);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox3.Location = new System.Drawing.Point(0, 341);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(294, 185);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Scale";
            // 
            // posIncBox
            // 
            this.posIncBox.Location = new System.Drawing.Point(12, 19);
            this.posIncBox.Name = "posIncBox";
            this.posIncBox.Size = new System.Drawing.Size(100, 20);
            this.posIncBox.TabIndex = 0;
            // 
            // posXTB
            // 
            this.posXTB.Location = new System.Drawing.Point(32, 50);
            this.posXTB.Name = "posXTB";
            this.posXTB.Size = new System.Drawing.Size(250, 45);
            this.posXTB.TabIndex = 1;
            this.posXTB.Value = 5;
            this.posXTB.ValueChanged += new System.EventHandler(this.posXTB_ValueChanged);
            this.posXTB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "X";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Y";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Z";
            // 
            // posYTB
            // 
            this.posYTB.Location = new System.Drawing.Point(32, 85);
            this.posYTB.Name = "posYTB";
            this.posYTB.Size = new System.Drawing.Size(250, 45);
            this.posYTB.TabIndex = 5;
            this.posYTB.Value = 5;
            this.posYTB.ValueChanged += new System.EventHandler(this.posYTB_ValueChanged);
            this.posYTB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUp);
            // 
            // posZTB
            // 
            this.posZTB.Location = new System.Drawing.Point(32, 121);
            this.posZTB.Name = "posZTB";
            this.posZTB.Size = new System.Drawing.Size(250, 45);
            this.posZTB.TabIndex = 6;
            this.posZTB.Value = 5;
            this.posZTB.ValueChanged += new System.EventHandler(this.posZTB_ValueChanged);
            this.posZTB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUp);
            // 
            // rotZTB
            // 
            this.rotZTB.Location = new System.Drawing.Point(32, 116);
            this.rotZTB.Name = "rotZTB";
            this.rotZTB.Size = new System.Drawing.Size(250, 45);
            this.rotZTB.TabIndex = 12;
            this.rotZTB.Value = 5;
            this.rotZTB.ValueChanged += new System.EventHandler(this.rotZTB_ValueChanged);
            this.rotZTB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUp);
            // 
            // rotYTB
            // 
            this.rotYTB.Location = new System.Drawing.Point(32, 80);
            this.rotYTB.Name = "rotYTB";
            this.rotYTB.Size = new System.Drawing.Size(250, 45);
            this.rotYTB.TabIndex = 11;
            this.rotYTB.Value = 5;
            this.rotYTB.ValueChanged += new System.EventHandler(this.rotYTB_ValueChanged);
            this.rotYTB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUp);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Z";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 80);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Y";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 45);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(14, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "X";
            // 
            // rotXTB
            // 
            this.rotXTB.Location = new System.Drawing.Point(32, 45);
            this.rotXTB.Name = "rotXTB";
            this.rotXTB.Size = new System.Drawing.Size(250, 45);
            this.rotXTB.TabIndex = 7;
            this.rotXTB.Value = 5;
            this.rotXTB.ValueChanged += new System.EventHandler(this.rotXTB_ValueChanged);
            this.rotXTB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUp);
            // 
            // rotIncBox
            // 
            this.rotIncBox.Location = new System.Drawing.Point(12, 19);
            this.rotIncBox.Name = "rotIncBox";
            this.rotIncBox.Size = new System.Drawing.Size(100, 20);
            this.rotIncBox.TabIndex = 7;
            // 
            // scaIncBox
            // 
            this.scaIncBox.Location = new System.Drawing.Point(12, 20);
            this.scaIncBox.Name = "scaIncBox";
            this.scaIncBox.Size = new System.Drawing.Size(100, 20);
            this.scaIncBox.TabIndex = 13;
            // 
            // scaXTB
            // 
            this.scaXTB.Location = new System.Drawing.Point(15, 46);
            this.scaXTB.Name = "scaXTB";
            this.scaXTB.Size = new System.Drawing.Size(267, 45);
            this.scaXTB.TabIndex = 14;
            this.scaXTB.Value = 5;
            this.scaXTB.ValueChanged += new System.EventHandler(this.scaXTB_ValueChanged);
            this.scaXTB.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUp);
            // 
            // MeshMover
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 526);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "MeshMover";
            this.Text = "MeshMover";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.posXTB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.posYTB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.posZTB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotZTB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotYTB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotXTB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scaXTB)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TrackBar posXTB;
        private System.Windows.Forms.TextBox posIncBox;
        private System.Windows.Forms.TrackBar posZTB;
        private System.Windows.Forms.TrackBar posYTB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox rotIncBox;
        private System.Windows.Forms.TrackBar rotZTB;
        private System.Windows.Forms.TrackBar rotYTB;
        private System.Windows.Forms.TrackBar rotXTB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox scaIncBox;
        private System.Windows.Forms.TrackBar scaXTB;
    }
}