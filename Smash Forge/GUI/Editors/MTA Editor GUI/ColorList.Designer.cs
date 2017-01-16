namespace Smash_Forge
{
    partial class ColorList
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
            this.colorAnimList = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // colorAnimList
            // 
            this.colorAnimList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.colorAnimList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.colorAnimList.FormattingEnabled = true;
            this.colorAnimList.ItemHeight = 16;
            this.colorAnimList.Location = new System.Drawing.Point(0, 0);
            this.colorAnimList.Name = "colorAnimList";
            this.colorAnimList.Size = new System.Drawing.Size(240, 322);
            this.colorAnimList.TabIndex = 1;
            // 
            // ColorList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.colorAnimList);
            this.Name = "ColorList";
            this.Size = new System.Drawing.Size(240, 322);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox colorAnimList;
    }
}
