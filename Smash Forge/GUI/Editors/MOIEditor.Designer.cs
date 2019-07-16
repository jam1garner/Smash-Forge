namespace SmashForge
{
    partial class MOIEditor
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.n1 = new System.Windows.Forms.NumericUpDown();
            this.n2 = new System.Windows.Forms.NumericUpDown();
            this.n4 = new System.Windows.Forms.NumericUpDown();
            this.n3 = new System.Windows.Forms.NumericUpDown();
            this.n6 = new System.Windows.Forms.NumericUpDown();
            this.n5 = new System.Windows.Forms.NumericUpDown();
            this.n8 = new System.Windows.Forms.NumericUpDown();
            this.n7 = new System.Windows.Forms.NumericUpDown();
            this.u2 = new System.Windows.Forms.NumericUpDown();
            this.u1 = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.n1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.n7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.u2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.u1)).BeginInit();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 18);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(168, 160);
            this.listBox1.TabIndex = 0;
            this.listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // listBox2
            // 
            this.listBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox2.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBox2.FormattingEnabled = true;
            this.listBox2.Location = new System.Drawing.Point(12, 310);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(168, 160);
            this.listBox2.TabIndex = 1;
            this.listBox2.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
            this.listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
            // 
            // n1
            // 
            this.n1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.n1.Location = new System.Drawing.Point(12, 184);
            this.n1.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.n1.Minimum = new decimal(new int[] {
            -727379968,
            232,
            0,
            -2147483648});
            this.n1.Name = "n1";
            this.n1.Size = new System.Drawing.Size(79, 20);
            this.n1.TabIndex = 2;
            this.n1.ValueChanged += new System.EventHandler(this.changedValue);
            // 
            // n2
            // 
            this.n2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.n2.Location = new System.Drawing.Point(101, 184);
            this.n2.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.n2.Minimum = new decimal(new int[] {
            -727379968,
            232,
            0,
            -2147483648});
            this.n2.Name = "n2";
            this.n2.Size = new System.Drawing.Size(79, 20);
            this.n2.TabIndex = 3;
            this.n2.ValueChanged += new System.EventHandler(this.changedValue);
            // 
            // n4
            // 
            this.n4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.n4.Location = new System.Drawing.Point(101, 210);
            this.n4.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.n4.Minimum = new decimal(new int[] {
            -727379968,
            232,
            0,
            -2147483648});
            this.n4.Name = "n4";
            this.n4.Size = new System.Drawing.Size(79, 20);
            this.n4.TabIndex = 5;
            this.n4.ValueChanged += new System.EventHandler(this.changedValue);
            // 
            // n3
            // 
            this.n3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.n3.Location = new System.Drawing.Point(12, 210);
            this.n3.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.n3.Minimum = new decimal(new int[] {
            -727379968,
            232,
            0,
            -2147483648});
            this.n3.Name = "n3";
            this.n3.Size = new System.Drawing.Size(79, 20);
            this.n3.TabIndex = 4;
            this.n3.ValueChanged += new System.EventHandler(this.changedValue);
            // 
            // n6
            // 
            this.n6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.n6.Location = new System.Drawing.Point(101, 236);
            this.n6.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.n6.Minimum = new decimal(new int[] {
            -727379968,
            232,
            0,
            -2147483648});
            this.n6.Name = "n6";
            this.n6.Size = new System.Drawing.Size(79, 20);
            this.n6.TabIndex = 7;
            this.n6.ValueChanged += new System.EventHandler(this.changedValue);
            // 
            // n5
            // 
            this.n5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.n5.Location = new System.Drawing.Point(12, 236);
            this.n5.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.n5.Minimum = new decimal(new int[] {
            -727379968,
            232,
            0,
            -2147483648});
            this.n5.Name = "n5";
            this.n5.Size = new System.Drawing.Size(79, 20);
            this.n5.TabIndex = 6;
            this.n5.ValueChanged += new System.EventHandler(this.changedValue);
            // 
            // n8
            // 
            this.n8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.n8.Location = new System.Drawing.Point(101, 262);
            this.n8.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.n8.Minimum = new decimal(new int[] {
            -727379968,
            232,
            0,
            -2147483648});
            this.n8.Name = "n8";
            this.n8.Size = new System.Drawing.Size(79, 20);
            this.n8.TabIndex = 9;
            this.n8.ValueChanged += new System.EventHandler(this.changedValue);
            // 
            // n7
            // 
            this.n7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.n7.Location = new System.Drawing.Point(12, 262);
            this.n7.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.n7.Minimum = new decimal(new int[] {
            -727379968,
            232,
            0,
            -2147483648});
            this.n7.Name = "n7";
            this.n7.Size = new System.Drawing.Size(79, 20);
            this.n7.TabIndex = 8;
            this.n7.ValueChanged += new System.EventHandler(this.changedValue);
            // 
            // u2
            // 
            this.u2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.u2.Location = new System.Drawing.Point(101, 476);
            this.u2.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.u2.Minimum = new decimal(new int[] {
            -727379968,
            232,
            0,
            -2147483648});
            this.u2.Name = "u2";
            this.u2.Size = new System.Drawing.Size(79, 20);
            this.u2.TabIndex = 11;
            this.u2.ValueChanged += new System.EventHandler(this.changedValue);
            // 
            // u1
            // 
            this.u1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.u1.Location = new System.Drawing.Point(12, 476);
            this.u1.Maximum = new decimal(new int[] {
            -727379968,
            232,
            0,
            0});
            this.u1.Minimum = new decimal(new int[] {
            -727379968,
            232,
            0,
            -2147483648});
            this.u1.Name = "u1";
            this.u1.Size = new System.Drawing.Size(79, 20);
            this.u1.TabIndex = 10;
            this.u1.ValueChanged += new System.EventHandler(this.changedValue);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(12, 502);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(168, 55);
            this.button1.TabIndex = 12;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MOIEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(192, 569);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.u2);
            this.Controls.Add(this.u1);
            this.Controls.Add(this.n8);
            this.Controls.Add(this.n7);
            this.Controls.Add(this.n6);
            this.Controls.Add(this.n5);
            this.Controls.Add(this.n4);
            this.Controls.Add(this.n3);
            this.Controls.Add(this.n2);
            this.Controls.Add(this.n1);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.listBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "MOIEditor";
            this.Text = "MOIEditor";
            this.Load += new System.EventHandler(this.MOIEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.n1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.n7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.u2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.u1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.NumericUpDown n1;
        private System.Windows.Forms.NumericUpDown n2;
        private System.Windows.Forms.NumericUpDown n4;
        private System.Windows.Forms.NumericUpDown n3;
        private System.Windows.Forms.NumericUpDown n6;
        private System.Windows.Forms.NumericUpDown n5;
        private System.Windows.Forms.NumericUpDown n8;
        private System.Windows.Forms.NumericUpDown n7;
        private System.Windows.Forms.NumericUpDown u2;
        private System.Windows.Forms.NumericUpDown u1;
        private System.Windows.Forms.Button button1;
    }
}