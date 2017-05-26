namespace Smash_Forge
{
    partial class NUT_TexIDEditor
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
            this.typeCB = new System.Windows.Forms.ComboBox();
            this.characterCB = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.typeTB = new System.Windows.Forms.TextBox();
            this.slotUD = new System.Windows.Forms.NumericUpDown();
            this.charTB = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.slotUD)).BeginInit();
            this.SuspendLayout();
            // 
            // typeCB
            // 
            this.typeCB.FormattingEnabled = true;
            this.typeCB.Location = new System.Drawing.Point(73, 37);
            this.typeCB.Name = "typeCB";
            this.typeCB.Size = new System.Drawing.Size(114, 21);
            this.typeCB.TabIndex = 0;
            this.typeCB.SelectedIndexChanged += new System.EventHandler(this.typeCB_SelectedIndexChanged);
            // 
            // characterCB
            // 
            this.characterCB.FormattingEnabled = true;
            this.characterCB.Location = new System.Drawing.Point(73, 65);
            this.characterCB.Name = "characterCB";
            this.characterCB.Size = new System.Drawing.Size(114, 21);
            this.characterCB.TabIndex = 1;
            this.characterCB.SelectedIndexChanged += new System.EventHandler(this.characterCB_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Character";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 96);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Slot";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(54, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Change Texture ID";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(73, 128);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Go";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // typeTB
            // 
            this.typeTB.Location = new System.Drawing.Point(193, 37);
            this.typeTB.Name = "typeTB";
            this.typeTB.Size = new System.Drawing.Size(27, 20);
            this.typeTB.TabIndex = 8;
            // 
            // slotUD
            // 
            this.slotUD.Location = new System.Drawing.Point(73, 96);
            this.slotUD.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.slotUD.Name = "slotUD";
            this.slotUD.Size = new System.Drawing.Size(114, 20);
            this.slotUD.TabIndex = 9;
            // 
            // charTB
            // 
            this.charTB.Location = new System.Drawing.Point(193, 66);
            this.charTB.Name = "charTB";
            this.charTB.Size = new System.Drawing.Size(27, 20);
            this.charTB.TabIndex = 10;
            // 
            // NUT_TexIDEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(232, 163);
            this.Controls.Add(this.charTB);
            this.Controls.Add(this.slotUD);
            this.Controls.Add(this.typeTB);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.characterCB);
            this.Controls.Add(this.typeCB);
            this.Name = "NUT_TexIDEditor";
            this.Text = "TexID Editor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.NUT_TexIDEditor_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.slotUD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox typeCB;
        private System.Windows.Forms.ComboBox characterCB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox typeTB;
        private System.Windows.Forms.NumericUpDown slotUD;
        private System.Windows.Forms.TextBox charTB;
    }
}