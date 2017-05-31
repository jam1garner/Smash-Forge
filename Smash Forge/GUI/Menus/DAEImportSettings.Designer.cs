namespace Smash_Forge
{
    partial class DAEImportSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DAEImportSettings));
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.scaleTB = new System.Windows.Forms.TextBox();
            this.smoothCB = new System.Windows.Forms.CheckBox();
            this.vbnFileLabel = new System.Windows.Forms.Label();
            this.vbnButton = new System.Windows.Forms.Button();
            this.vertcolorCB = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(116, 309);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Import";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(239, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Note: Noesis exported DAEs are the most reliable";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(3, 3);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(65, 17);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Text = "Flip UVs";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.scaleTB);
            this.panel1.Controls.Add(this.smoothCB);
            this.panel1.Controls.Add(this.vbnFileLabel);
            this.panel1.Controls.Add(this.vbnButton);
            this.panel1.Controls.Add(this.vertcolorCB);
            this.panel1.Controls.Add(this.checkBox5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.comboBox2);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Controls.Add(this.checkBox4);
            this.panel1.Controls.Add(this.checkBox2);
            this.panel1.Controls.Add(this.checkBox1);
            this.panel1.Location = new System.Drawing.Point(15, 55);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(278, 248);
            this.panel1.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 145);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Scale";
            // 
            // scaleTB
            // 
            this.scaleTB.Location = new System.Drawing.Point(43, 142);
            this.scaleTB.Name = "scaleTB";
            this.scaleTB.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.scaleTB.Size = new System.Drawing.Size(42, 20);
            this.scaleTB.TabIndex = 12;
            this.scaleTB.Text = "1";
            // 
            // smoothCB
            // 
            this.smoothCB.AutoSize = true;
            this.smoothCB.Location = new System.Drawing.Point(4, 119);
            this.smoothCB.Name = "smoothCB";
            this.smoothCB.Size = new System.Drawing.Size(103, 17);
            this.smoothCB.TabIndex = 11;
            this.smoothCB.Text = "Smooth Normals";
            this.smoothCB.UseVisualStyleBackColor = true;
            // 
            // vbnFileLabel
            // 
            this.vbnFileLabel.AutoSize = true;
            this.vbnFileLabel.Location = new System.Drawing.Point(85, 173);
            this.vbnFileLabel.Name = "vbnFileLabel";
            this.vbnFileLabel.Size = new System.Drawing.Size(0, 13);
            this.vbnFileLabel.TabIndex = 10;
            // 
            // vbnButton
            // 
            this.vbnButton.Location = new System.Drawing.Point(4, 168);
            this.vbnButton.Name = "vbnButton";
            this.vbnButton.Size = new System.Drawing.Size(75, 23);
            this.vbnButton.TabIndex = 9;
            this.vbnButton.Text = "Open VBN";
            this.vbnButton.UseVisualStyleBackColor = true;
            this.vbnButton.Click += new System.EventHandler(this.vbnButton_Click);
            // 
            // vertcolorCB
            // 
            this.vertcolorCB.AutoSize = true;
            this.vertcolorCB.Location = new System.Drawing.Point(5, 96);
            this.vertcolorCB.Name = "vertcolorCB";
            this.vertcolorCB.Size = new System.Drawing.Size(195, 17);
            this.vertcolorCB.TabIndex = 8;
            this.vertcolorCB.Text = "Ignore Vertex Colors (sets all to 127)";
            this.vertcolorCB.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(4, 72);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(99, 17);
            this.checkBox5.TabIndex = 7;
            this.checkBox5.Text = "Import Textures";
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.CheckedChanged += new System.EventHandler(this.checkBox5_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 227);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Bone Type";
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(76, 224);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(199, 21);
            this.comboBox2.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 200);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Vert Type";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(76, 197);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(199, 21);
            this.comboBox1.TabIndex = 5;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(3, 26);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(73, 17);
            this.checkBox4.TabIndex = 6;
            this.checkBox4.Text = "Rotate 90";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(4, 49);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(173, 17);
            this.checkBox2.TabIndex = 3;
            this.checkBox2.Text = "Remove ### from Mesh Name";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Import Options";
            // 
            // DAEImportSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(305, 344);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DAEImportSettings";
            this.Text = "DAEImportSettings";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox1;
        public System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox vertcolorCB;
        private System.Windows.Forms.CheckBox smoothCB;
        public System.Windows.Forms.Label vbnFileLabel;
        private System.Windows.Forms.Button vbnButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox scaleTB;
    }
}