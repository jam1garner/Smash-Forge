namespace Smash_Forge
{
    partial class TexIdSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TexIdSelector));
            this.typeComboBox = new System.Windows.Forms.ComboBox();
            this.characterComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.applyButton = new System.Windows.Forms.Button();
            this.typeTB = new System.Windows.Forms.TextBox();
            this.slotUD = new System.Windows.Forms.NumericUpDown();
            this.charTB = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.slotUD)).BeginInit();
            this.SuspendLayout();
            // 
            // typeComboBox
            // 
            this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeComboBox.FormattingEnabled = true;
            this.typeComboBox.Location = new System.Drawing.Point(146, 71);
            this.typeComboBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.typeComboBox.Name = "typeComboBox";
            this.typeComboBox.Size = new System.Drawing.Size(224, 33);
            this.typeComboBox.TabIndex = 0;
            this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.typeCB_SelectedIndexChanged);
            // 
            // characterComboBox
            // 
            this.characterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.characterComboBox.FormattingEnabled = true;
            this.characterComboBox.Location = new System.Drawing.Point(146, 125);
            this.characterComboBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.characterComboBox.Name = "characterComboBox";
            this.characterComboBox.Size = new System.Drawing.Size(224, 33);
            this.characterComboBox.TabIndex = 1;
            this.characterComboBox.SelectedIndexChanged += new System.EventHandler(this.characterCB_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 77);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 25);
            this.label1.TabIndex = 3;
            this.label1.Text = "Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 131);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 25);
            this.label2.TabIndex = 4;
            this.label2.Text = "Character";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 185);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 25);
            this.label3.TabIndex = 5;
            this.label3.Text = "Slot";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(108, 17);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(192, 25);
            this.label4.TabIndex = 6;
            this.label4.Text = "Change Texture ID";
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(146, 235);
            this.applyButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(150, 44);
            this.applyButton.TabIndex = 7;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // typeTB
            // 
            this.typeTB.Location = new System.Drawing.Point(452, 71);
            this.typeTB.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.typeTB.Name = "typeTB";
            this.typeTB.Size = new System.Drawing.Size(50, 31);
            this.typeTB.TabIndex = 8;
            this.typeTB.TextChanged += new System.EventHandler(this.typeTB_TextChanged);
            // 
            // slotUD
            // 
            this.slotUD.Location = new System.Drawing.Point(146, 185);
            this.slotUD.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.slotUD.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.slotUD.Name = "slotUD";
            this.slotUD.Size = new System.Drawing.Size(228, 31);
            this.slotUD.TabIndex = 9;
            this.slotUD.ValueChanged += new System.EventHandler(this.slotUD_ValueChanged);
            // 
            // charTB
            // 
            this.charTB.Location = new System.Drawing.Point(452, 127);
            this.charTB.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.charTB.Name = "charTB";
            this.charTB.Size = new System.Drawing.Size(50, 31);
            this.charTB.TabIndex = 10;
            this.charTB.TextChanged += new System.EventHandler(this.charTB_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(386, 77);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 25);
            this.label5.TabIndex = 11;
            this.label5.Text = "hex:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(386, 133);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 25);
            this.label6.TabIndex = 12;
            this.label6.Text = "hex:";
            // 
            // TexIdSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(522, 312);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.charTB);
            this.Controls.Add(this.slotUD);
            this.Controls.Add(this.typeTB);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.characterComboBox);
            this.Controls.Add(this.typeComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "TexIdSelector";
            this.Text = "TexID Editor";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.NUT_TexIDEditor_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.slotUD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox typeComboBox;
        private System.Windows.Forms.ComboBox characterComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button applyButton;
        public System.Windows.Forms.TextBox typeTB;
        public System.Windows.Forms.NumericUpDown slotUD;
        public System.Windows.Forms.TextBox charTB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}