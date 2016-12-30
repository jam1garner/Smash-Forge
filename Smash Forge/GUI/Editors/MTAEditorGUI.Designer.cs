namespace Smash_Forge {
    partial class MTAEditorGUI
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
            this.colorAnimList = new System.Windows.Forms.ListBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // colorAnimList
            // 
            this.colorAnimList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.colorAnimList.FormattingEnabled = true;
            this.colorAnimList.ItemHeight = 16;
            this.colorAnimList.Location = new System.Drawing.Point(12, 187);
            this.colorAnimList.Name = "colorAnimList";
            this.colorAnimList.Size = new System.Drawing.Size(300, 436);
            this.colorAnimList.TabIndex = 0;
            this.colorAnimList.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(12, 12);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(300, 169);
            this.treeView1.TabIndex = 1;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // MTAEditorGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 635);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.colorAnimList);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MTAEditorGUI";
            this.Text = "MTAEditorGUI";
            this.Load += new System.EventHandler(this.MTAEditorGUI_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox colorAnimList;
        private System.Windows.Forms.TreeView treeView1;
    }
}