namespace Smash_Forge
{
    partial class VBNViewport
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.nupdFrameRate = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nupdFrame = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.nupdMaxFrame = new System.Windows.Forms.NumericUpDown();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnNextFrame = new System.Windows.Forms.Button();
            this.btnLastFrame = new System.Windows.Forms.Button();
            this.btnPrevFrame = new System.Windows.Forms.Button();
            this.btnFirstFrame = new System.Windows.Forms.Button();
            this.glControl1 = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 8, 16));
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nupdFrameRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupdFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupdMaxFrame)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.checkBox2);
            this.groupBox2.Controls.Add(this.checkBox1);
            this.groupBox2.Controls.Add(this.nupdFrameRate);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.nupdFrame);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.nupdMaxFrame);
            this.groupBox2.Controls.Add(this.btnPlay);
            this.groupBox2.Controls.Add(this.btnNextFrame);
            this.groupBox2.Controls.Add(this.btnLastFrame);
            this.groupBox2.Controls.Add(this.btnPrevFrame);
            this.groupBox2.Controls.Add(this.btnFirstFrame);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox2.Location = new System.Drawing.Point(0, 407);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(624, 91);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.Location = new System.Drawing.Point(307, 14);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(87, 23);
            this.button1.TabIndex = 12;
            this.button1.Text = "Reset Camera";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(251, 17);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(50, 17);
            this.checkBox2.TabIndex = 11;
            this.checkBox2.Text = "Loop";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(112, 17);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(133, 17);
            this.checkBox1.TabIndex = 10;
            this.checkBox1.Text = "Use Camera Animation";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // nupdFrameRate
            // 
            this.nupdFrameRate.Location = new System.Drawing.Point(54, 14);
            this.nupdFrameRate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nupdFrameRate.Name = "nupdFrameRate";
            this.nupdFrameRate.Size = new System.Drawing.Size(52, 20);
            this.nupdFrameRate.TabIndex = 9;
            this.nupdFrameRate.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nupdFrameRate.ValueChanged += new System.EventHandler(this.nupdSpeed_ValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Speed:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(458, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Frame:";
            // 
            // nupdFrame
            // 
            this.nupdFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nupdFrame.Location = new System.Drawing.Point(499, 14);
            this.nupdFrame.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nupdFrame.Name = "nupdFrame";
            this.nupdFrame.Size = new System.Drawing.Size(52, 20);
            this.nupdFrame.TabIndex = 7;
            this.nupdFrame.ValueChanged += new System.EventHandler(this.nupdFrame_ValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(554, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(12, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "/";
            // 
            // nupdMaxFrame
            // 
            this.nupdMaxFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nupdMaxFrame.Enabled = false;
            this.nupdMaxFrame.Location = new System.Drawing.Point(569, 14);
            this.nupdMaxFrame.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nupdMaxFrame.Name = "nupdMaxFrame";
            this.nupdMaxFrame.Size = new System.Drawing.Size(52, 20);
            this.nupdMaxFrame.TabIndex = 5;
            this.nupdMaxFrame.ValueChanged += new System.EventHandler(this.nupdFrame_ValueChanged);
            // 
            // btnPlay
            // 
            this.btnPlay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlay.Location = new System.Drawing.Point(94, 40);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(433, 44);
            this.btnPlay.TabIndex = 4;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnNextFrame
            // 
            this.btnNextFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNextFrame.Location = new System.Drawing.Point(533, 40);
            this.btnNextFrame.Name = "btnNextFrame";
            this.btnNextFrame.Size = new System.Drawing.Size(35, 44);
            this.btnNextFrame.TabIndex = 3;
            this.btnNextFrame.Text = ">";
            this.btnNextFrame.UseVisualStyleBackColor = true;
            this.btnNextFrame.Click += new System.EventHandler(this.btnNextFrame_Click);
            // 
            // btnLastFrame
            // 
            this.btnLastFrame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLastFrame.Location = new System.Drawing.Point(575, 40);
            this.btnLastFrame.Name = "btnLastFrame";
            this.btnLastFrame.Size = new System.Drawing.Size(35, 44);
            this.btnLastFrame.TabIndex = 2;
            this.btnLastFrame.Text = ">|";
            this.btnLastFrame.UseVisualStyleBackColor = true;
            this.btnLastFrame.Click += new System.EventHandler(this.btnLastFrame_Click);
            // 
            // btnPrevFrame
            // 
            this.btnPrevFrame.Location = new System.Drawing.Point(53, 40);
            this.btnPrevFrame.Name = "btnPrevFrame";
            this.btnPrevFrame.Size = new System.Drawing.Size(35, 44);
            this.btnPrevFrame.TabIndex = 1;
            this.btnPrevFrame.Text = "<";
            this.btnPrevFrame.UseVisualStyleBackColor = true;
            this.btnPrevFrame.Click += new System.EventHandler(this.btnPrevFrame_Click);
            // 
            // btnFirstFrame
            // 
            this.btnFirstFrame.Location = new System.Drawing.Point(12, 40);
            this.btnFirstFrame.Name = "btnFirstFrame";
            this.btnFirstFrame.Size = new System.Drawing.Size(35, 44);
            this.btnFirstFrame.TabIndex = 0;
            this.btnFirstFrame.Text = "|<";
            this.btnFirstFrame.UseVisualStyleBackColor = true;
            this.btnFirstFrame.Click += new System.EventHandler(this.btnFirstFrame_Click);
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.CausesValidation = false;
            this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl1.Location = new System.Drawing.Point(0, 0);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(624, 407);
            this.glControl1.TabIndex = 9;
            this.glControl1.VSync = false;
            this.glControl1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseClick);
            this.glControl1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.glControl1_DoubleClick);
            this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseMove);
            // 
            // VBNViewport
            // 
            this.ClientSize = new System.Drawing.Size(624, 498);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.groupBox2);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.Name = "VBNViewport";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.VBNViewport_KeyPress);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nupdFrameRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupdFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupdMaxFrame)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nupdFrame;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nupdMaxFrame;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnNextFrame;
        private System.Windows.Forms.Button btnLastFrame;
        private System.Windows.Forms.Button btnPrevFrame;
        private System.Windows.Forms.Button btnFirstFrame;
        private OpenTK.GLControl glControl1;
        private System.Windows.Forms.NumericUpDown nupdFrameRate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.GroupBox groupBox2;
    }
}
