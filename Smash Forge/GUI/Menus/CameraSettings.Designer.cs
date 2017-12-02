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
            this.fovLabel = new System.Windows.Forms.Label();
            this.fovSlider = new System.Windows.Forms.TrackBar();
            this.renderDepthLabel = new System.Windows.Forms.Label();
            this.depthSlider = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.fovSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.depthSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // fovLabel
            // 
            this.fovLabel.AutoSize = true;
            this.fovLabel.Location = new System.Drawing.Point(116, 60);
            this.fovLabel.Name = "fovLabel";
            this.fovLabel.Size = new System.Drawing.Size(57, 13);
            this.fovLabel.TabIndex = 33;
            this.fovLabel.Text = "FOV Slider";
            // 
            // fovSlider
            // 
            this.fovSlider.Location = new System.Drawing.Point(12, 76);
            this.fovSlider.Maximum = 180;
            this.fovSlider.Minimum = 1;
            this.fovSlider.Name = "fovSlider";
            this.fovSlider.Size = new System.Drawing.Size(265, 45);
            this.fovSlider.TabIndex = 32;
            this.fovSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.fovSlider.Value = 1;
            // 
            // renderDepthLabel
            // 
            this.renderDepthLabel.AutoSize = true;
            this.renderDepthLabel.Location = new System.Drawing.Point(126, 9);
            this.renderDepthLabel.Name = "renderDepthLabel";
            this.renderDepthLabel.Size = new System.Drawing.Size(36, 13);
            this.renderDepthLabel.TabIndex = 31;
            this.renderDepthLabel.Text = "Depth";
            // 
            // depthSlider
            // 
            this.depthSlider.Location = new System.Drawing.Point(12, 25);
            this.depthSlider.Maximum = 500000;
            this.depthSlider.Minimum = 10;
            this.depthSlider.Name = "depthSlider";
            this.depthSlider.Size = new System.Drawing.Size(265, 45);
            this.depthSlider.SmallChange = 5;
            this.depthSlider.TabIndex = 30;
            this.depthSlider.Value = 10;
            // 
            // CameraSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 366);
            this.Controls.Add(this.fovLabel);
            this.Controls.Add(this.fovSlider);
            this.Controls.Add(this.renderDepthLabel);
            this.Controls.Add(this.depthSlider);
            this.Name = "CameraSettings";
            this.Text = "CameraSettings";
            ((System.ComponentModel.ISupportInitialize)(this.fovSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.depthSlider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label fovLabel;
        private System.Windows.Forms.TrackBar fovSlider;
        private System.Windows.Forms.Label renderDepthLabel;
        private System.Windows.Forms.TrackBar depthSlider;
    }
}