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
            this.matPropList = new System.Windows.Forms.ListBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
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
            this.colorAnimList.Size = new System.Drawing.Size(289, 409);
            this.colorAnimList.TabIndex = 0;
            this.colorAnimList.Visible = false;
            this.colorAnimList.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
            this.colorAnimList.SelectedIndexChanged += new System.EventHandler(this.colorAnimList_SelectedIndexChanged);
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.LabelEdit = true;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(289, 171);
            this.treeView1.TabIndex = 1;
            this.treeView1.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeView1_AfterLabelEdit);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // matPropList
            // 
            this.matPropList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.matPropList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.matPropList.FormattingEnabled = true;
            this.matPropList.ItemHeight = 16;
            this.matPropList.Location = new System.Drawing.Point(0, 0);
            this.matPropList.Name = "matPropList";
            this.matPropList.Size = new System.Drawing.Size(289, 409);
            this.matPropList.TabIndex = 2;
            this.matPropList.Visible = false;
            this.matPropList.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.matPropList_DrawItem);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.matPropList);
            this.splitContainer1.Panel2.Controls.Add(this.colorAnimList);
            this.splitContainer1.Size = new System.Drawing.Size(289, 586);
            this.splitContainer1.SplitterDistance = 171;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 3;
            // 
            // MTAEditorGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(289, 586);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MTAEditorGUI";
            this.Text = "MTAEditorGUI";
            this.Load += new System.EventHandler(this.MTAEditorGUI_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox colorAnimList;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ListBox matPropList;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}