namespace Smash_Forge
{
    partial class AnimListPanel
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
            this.lstAnims = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lstAnims
            // 
            this.lstAnims.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstAnims.FormattingEnabled = true;
            this.lstAnims.Location = new System.Drawing.Point(0, 0);
            this.lstAnims.Name = "lstAnims";
            this.lstAnims.Size = new System.Drawing.Size(140, 125);
            this.lstAnims.TabIndex = 1;
            this.lstAnims.SelectedIndexChanged += new System.EventHandler(this.lstAnims_SelectedIndexChanged);
            // 
            // AnimListPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(140, 125);
            this.Controls.Add(this.lstAnims);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideOnClose = true;
            this.Name = "AnimListPanel";
            this.ShowIcon = false;
            this.Text = "Animations";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ListBox lstAnims;
    }
}
