namespace Smash_Forge
{
    partial class LVDEditor
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
            this.Collisions = new System.Windows.Forms.TabPage();
            this.Spawns = new System.Windows.Forms.TabPage();
            this.Bounds = new System.Windows.Forms.TabPage();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.tabControl1.SuspendLayout();
            this.Collisions.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.Collisions);
            this.tabControl1.Controls.Add(this.Spawns);
            this.tabControl1.Controls.Add(this.Bounds);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(314, 701);
            this.tabControl1.TabIndex = 0;
            // 
            // Collisions
            // 
            this.Collisions.Controls.Add(this.treeView1);
            this.Collisions.Location = new System.Drawing.Point(4, 22);
            this.Collisions.Name = "Collisions";
            this.Collisions.Padding = new System.Windows.Forms.Padding(3);
            this.Collisions.Size = new System.Drawing.Size(306, 675);
            this.Collisions.TabIndex = 0;
            this.Collisions.Text = "Collisions";
            this.Collisions.UseVisualStyleBackColor = true;
            // 
            // Spawns
            // 
            this.Spawns.Location = new System.Drawing.Point(4, 22);
            this.Spawns.Name = "Spawns";
            this.Spawns.Padding = new System.Windows.Forms.Padding(3);
            this.Spawns.Size = new System.Drawing.Size(306, 580);
            this.Spawns.TabIndex = 1;
            this.Spawns.Text = "Spawns";
            this.Spawns.UseVisualStyleBackColor = true;
            // 
            // Bounds
            // 
            this.Bounds.Location = new System.Drawing.Point(4, 22);
            this.Bounds.Name = "Bounds";
            this.Bounds.Padding = new System.Windows.Forms.Padding(3);
            this.Bounds.Size = new System.Drawing.Size(306, 675);
            this.Bounds.TabIndex = 3;
            this.Bounds.Text = "Bounds";
            this.Bounds.UseVisualStyleBackColor = true;
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(3, 2);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(302, 331);
            this.treeView1.TabIndex = 0;
            // 
            // LVDEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(314, 701);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "LVDEditor";
            this.Text = "LVDEditor";
            this.tabControl1.ResumeLayout(false);
            this.Collisions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage Collisions;
        private System.Windows.Forms.TabPage Spawns;
        private System.Windows.Forms.TabPage Bounds;
        private System.Windows.Forms.TreeView treeView1;
    }
}