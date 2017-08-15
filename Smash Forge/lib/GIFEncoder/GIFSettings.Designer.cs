namespace Gif.Components
{
    partial class GIFSettings
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
            this.label1 = new System.Windows.Forms.Label();
            this.nupdInitialFrame = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nupdMaxFrame = new System.Windows.Forms.NumericUpDown();
            this.tbQuality = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.cbRepeat = new System.Windows.Forms.CheckBox();
            this.btStart = new System.Windows.Forms.Button();
            this.nupdScaleFactor = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nupdInitialFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupdMaxFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbQuality)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupdScaleFactor)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "From frame";
            // 
            // nupdInitialFrame
            // 
            this.nupdInitialFrame.Location = new System.Drawing.Point(77, 7);
            this.nupdInitialFrame.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nupdInitialFrame.Name = "nupdInitialFrame";
            this.nupdInitialFrame.Size = new System.Drawing.Size(47, 20);
            this.nupdInitialFrame.TabIndex = 1;
            this.nupdInitialFrame.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(130, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "to frame";
            // 
            // nupdMaxFrame
            // 
            this.nupdMaxFrame.Location = new System.Drawing.Point(181, 7);
            this.nupdMaxFrame.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nupdMaxFrame.Name = "nupdMaxFrame";
            this.nupdMaxFrame.Size = new System.Drawing.Size(47, 20);
            this.nupdMaxFrame.TabIndex = 3;
            this.nupdMaxFrame.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // tbQuality
            // 
            this.tbQuality.Location = new System.Drawing.Point(57, 40);
            this.tbQuality.Maximum = 255;
            this.tbQuality.Minimum = 1;
            this.tbQuality.Name = "tbQuality";
            this.tbQuality.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tbQuality.Size = new System.Drawing.Size(171, 45);
            this.tbQuality.TabIndex = 4;
            this.tbQuality.Value = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Quality";
            // 
            // cbRepeat
            // 
            this.cbRepeat.AutoSize = true;
            this.cbRepeat.Checked = true;
            this.cbRepeat.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRepeat.Location = new System.Drawing.Point(234, 8);
            this.cbRepeat.Name = "cbRepeat";
            this.cbRepeat.Size = new System.Drawing.Size(61, 17);
            this.cbRepeat.TabIndex = 6;
            this.cbRepeat.Text = "Repeat";
            this.cbRepeat.UseVisualStyleBackColor = true;
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(366, 4);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(75, 23);
            this.btStart.TabIndex = 7;
            this.btStart.Text = "Start";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.start_Click);
            // 
            // nupdScaleFactor
            // 
            this.nupdScaleFactor.DecimalPlaces = 1;
            this.nupdScaleFactor.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nupdScaleFactor.Location = new System.Drawing.Point(301, 38);
            this.nupdScaleFactor.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nupdScaleFactor.Name = "nupdScaleFactor";
            this.nupdScaleFactor.Size = new System.Drawing.Size(47, 20);
            this.nupdScaleFactor.TabIndex = 8;
            this.nupdScaleFactor.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(231, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Scale factor";
            // 
            // GIFSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(453, 95);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.nupdScaleFactor);
            this.Controls.Add(this.btStart);
            this.Controls.Add(this.cbRepeat);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbQuality);
            this.Controls.Add(this.nupdMaxFrame);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.nupdInitialFrame);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GIFSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GIF Settings";
            ((System.ComponentModel.ISupportInitialize)(this.nupdInitialFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupdMaxFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbQuality)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nupdScaleFactor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nupdInitialFrame;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nupdMaxFrame;
        private System.Windows.Forms.TrackBar tbQuality;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbRepeat;
        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.NumericUpDown nupdScaleFactor;
        private System.Windows.Forms.Label label4;
    }
}