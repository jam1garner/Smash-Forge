namespace SmashForge
{
    partial class MTAEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MTAEditor));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.compileButton = new System.Windows.Forms.Button();
            this.loadViewportButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.DetectUrls = false;
            this.richTextBox1.HideSelection = false;
            this.richTextBox1.Location = new System.Drawing.Point(0, 36);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(214, 281);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // compileButton
            // 
            this.compileButton.Location = new System.Drawing.Point(12, 7);
            this.compileButton.Name = "compileButton";
            this.compileButton.Size = new System.Drawing.Size(75, 23);
            this.compileButton.TabIndex = 1;
            this.compileButton.Text = "Compile";
            this.compileButton.UseVisualStyleBackColor = true;
            this.compileButton.UseWaitCursor = true;
            this.compileButton.Click += new System.EventHandler(this.compileButton_Click);
            // 
            // loadViewportButton
            // 
            this.loadViewportButton.Location = new System.Drawing.Point(93, 7);
            this.loadViewportButton.Name = "loadViewportButton";
            this.loadViewportButton.Size = new System.Drawing.Size(102, 23);
            this.loadViewportButton.TabIndex = 2;
            this.loadViewportButton.Text = "Load in Viewport";
            this.loadViewportButton.UseVisualStyleBackColor = true;
            this.loadViewportButton.Click += new System.EventHandler(this.loadViewportButton_Click);
            // 
            // MTAEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(215, 321);
            this.Controls.Add(this.loadViewportButton);
            this.Controls.Add(this.compileButton);
            this.Controls.Add(this.richTextBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MTAEditor";
            this.Text = "MTAEditor";
            this.Load += new System.EventHandler(this.MTAEditor_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button compileButton;
        private System.Windows.Forms.Button loadViewportButton;
    }
}