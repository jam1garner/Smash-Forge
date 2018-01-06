namespace Smash_Forge
{
    partial class VertexTool
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
            this.vertexListBox = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.boneWeightList = new System.Windows.Forms.ListBox();
            this.WeightValue = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeightValue)).BeginInit();
            this.SuspendLayout();
            // 
            // vertexListBox
            // 
            this.vertexListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vertexListBox.FormattingEnabled = true;
            this.vertexListBox.Location = new System.Drawing.Point(3, 16);
            this.vertexListBox.Name = "vertexListBox";
            this.vertexListBox.Size = new System.Drawing.Size(224, 120);
            this.vertexListBox.TabIndex = 0;
            this.vertexListBox.SelectedIndexChanged += new System.EventHandler(this.vertexListBox_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.vertexListBox);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(230, 139);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select Vertex List";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.WeightValue);
            this.groupBox2.Controls.Add(this.boneWeightList);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 139);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(230, 222);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Weight Tool";
            // 
            // boneWeightList
            // 
            this.boneWeightList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.boneWeightList.FormattingEnabled = true;
            this.boneWeightList.Location = new System.Drawing.Point(6, 121);
            this.boneWeightList.Name = "boneWeightList";
            this.boneWeightList.Size = new System.Drawing.Size(218, 95);
            this.boneWeightList.TabIndex = 0;
            this.boneWeightList.SelectedIndexChanged += new System.EventHandler(this.boneWeightList_SelectedIndexChanged);
            // 
            // WeightValue
            // 
            this.WeightValue.DecimalPlaces = 6;
            this.WeightValue.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.WeightValue.Location = new System.Drawing.Point(12, 19);
            this.WeightValue.Name = "WeightValue";
            this.WeightValue.Size = new System.Drawing.Size(86, 20);
            this.WeightValue.TabIndex = 1;
            // 
            // VertexTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(230, 361);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "VertexTool";
            this.Text = "Vertex Tool";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.WeightValue)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ListBox vertexListBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox boneWeightList;
        private System.Windows.Forms.NumericUpDown WeightValue;
    }
}