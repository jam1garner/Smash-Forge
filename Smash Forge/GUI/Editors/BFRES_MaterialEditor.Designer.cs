namespace Smash_Forge.GUI.Editors
{
    partial class BFRES_MaterialEditor
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabTextureMaps = new System.Windows.Forms.TabPage();
            this.rgbTexControl = new OpenTK.GLControl();
            this.label1 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabTextureMaps.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabTextureMaps);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(577, 408);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button1);
            this.tabPage1.Controls.Add(this.textBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(569, 382);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Material Info";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(490, 357);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Apply";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(3, 6);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(569, 503);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Render Info";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabTextureMaps
            // 
            this.tabTextureMaps.Controls.Add(this.rgbTexControl);
            this.tabTextureMaps.Controls.Add(this.label1);
            this.tabTextureMaps.Controls.Add(this.listBox1);
            this.tabTextureMaps.Location = new System.Drawing.Point(4, 22);
            this.tabTextureMaps.Name = "tabTextureMaps";
            this.tabTextureMaps.Padding = new System.Windows.Forms.Padding(3);
            this.tabTextureMaps.Size = new System.Drawing.Size(569, 382);
            this.tabTextureMaps.TabIndex = 2;
            this.tabTextureMaps.Text = "Texture Maps";
            this.tabTextureMaps.UseVisualStyleBackColor = true;
            this.tabTextureMaps.Click += new System.EventHandler(this.tabTextureMaps_Click);
            // 
            // rgbTexControl
            // 
            this.rgbTexControl.BackColor = System.Drawing.Color.Black;
            this.rgbTexControl.Location = new System.Drawing.Point(317, 6);
            this.rgbTexControl.Name = "rgbTexControl";
            this.rgbTexControl.Size = new System.Drawing.Size(246, 227);
            this.rgbTexControl.TabIndex = 2;
            this.rgbTexControl.VSync = false;
            this.rgbTexControl.Load += new System.EventHandler(this.glControl1_Load);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Sampler";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(401, 257);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(160, 238);
            this.listBox1.TabIndex = 0;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // BFRES_MaterialEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 408);
            this.Controls.Add(this.tabControl1);
            this.Name = "BFRES_MaterialEditor";
            this.Text = "BFRES_MaterialEditor";
            this.Load += new System.EventHandler(this.BFRES_MaterialEditor_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabTextureMaps.ResumeLayout(false);
            this.tabTextureMaps.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabPage tabTextureMaps;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label1;
        private OpenTK.GLControl rgbTexControl;
    }
}