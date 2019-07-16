namespace SmashForge
{
    partial class UIPreview
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
            this.chr_00_renderer = new OpenTK.GLControl();
            this.chr_11_renderer = new OpenTK.GLControl();
            this.chr_13_renderer = new OpenTK.GLControl();
            this.stock_90_renderer = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // chr_00_renderer
            // 
            this.chr_00_renderer.BackColor = System.Drawing.Color.Black;
            this.chr_00_renderer.Location = new System.Drawing.Point(12, 82);
            this.chr_00_renderer.Name = "chr_00_renderer";
            this.chr_00_renderer.Size = new System.Drawing.Size(128, 128);
            this.chr_00_renderer.TabIndex = 0;
            this.chr_00_renderer.VSync = false;
            this.chr_00_renderer.DragEnter += new System.Windows.Forms.DragEventHandler(this.Drag_Enter);
            this.chr_00_renderer.Paint += new System.Windows.Forms.PaintEventHandler(this.chr_00_renderer_Paint);
            // 
            // chr_11_renderer
            // 
            this.chr_11_renderer.BackColor = System.Drawing.Color.Black;
            this.chr_11_renderer.Location = new System.Drawing.Point(12, 216);
            this.chr_11_renderer.Name = "chr_11_renderer";
            this.chr_11_renderer.Size = new System.Drawing.Size(192, 192);
            this.chr_11_renderer.TabIndex = 1;
            this.chr_11_renderer.VSync = false;
            this.chr_11_renderer.DragEnter += new System.Windows.Forms.DragEventHandler(this.Drag_Enter);
            this.chr_11_renderer.Paint += new System.Windows.Forms.PaintEventHandler(this.chr_11_renderer_Paint);
            // 
            // chr_13_renderer
            // 
            this.chr_13_renderer.BackColor = System.Drawing.Color.Black;
            this.chr_13_renderer.Location = new System.Drawing.Point(12, 414);
            this.chr_13_renderer.Name = "chr_13_renderer";
            this.chr_13_renderer.Size = new System.Drawing.Size(224, 224);
            this.chr_13_renderer.TabIndex = 2;
            this.chr_13_renderer.VSync = false;
            this.chr_13_renderer.DragDrop += new System.Windows.Forms.DragEventHandler(this.chr_13_renderer_DragDrop);
            this.chr_13_renderer.DragEnter += new System.Windows.Forms.DragEventHandler(this.Drag_Enter);
            this.chr_13_renderer.Paint += new System.Windows.Forms.PaintEventHandler(this.chr_13_renderer_Paint);
            // 
            // stock_90_renderer
            // 
            this.stock_90_renderer.BackColor = System.Drawing.Color.Black;
            this.stock_90_renderer.Location = new System.Drawing.Point(12, 12);
            this.stock_90_renderer.Name = "stock_90_renderer";
            this.stock_90_renderer.Size = new System.Drawing.Size(64, 64);
            this.stock_90_renderer.TabIndex = 3;
            this.stock_90_renderer.VSync = false;
            this.stock_90_renderer.DragEnter += new System.Windows.Forms.DragEventHandler(this.Drag_Enter);
            this.stock_90_renderer.Paint += new System.Windows.Forms.PaintEventHandler(this.stock_90_renderer_Paint);
            // 
            // UIPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(254, 647);
            this.Controls.Add(this.stock_90_renderer);
            this.Controls.Add(this.chr_13_renderer);
            this.Controls.Add(this.chr_11_renderer);
            this.Controls.Add(this.chr_00_renderer);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "UIPreview";
            this.Text = "UIPreview";
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl chr_00_renderer;
        private OpenTK.GLControl chr_11_renderer;
        private OpenTK.GLControl chr_13_renderer;
        private OpenTK.GLControl stock_90_renderer;
    }
}