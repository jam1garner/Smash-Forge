namespace Smash_Forge.GUI.Melee
{
    partial class DOBJEditor
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.numericTransparency = new System.Windows.Forms.NumericUpDown();
            this.numericGlossiness = new System.Windows.Forms.NumericUpDown();
            this.buttonSPC = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonAMB = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonDIF = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.flagsTB = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.textureListBox = new System.Windows.Forms.ListBox();
            this.textureGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.textureFlagsTB = new System.Windows.Forms.TextBox();
            this.buttonImportTexture = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericTransparency)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericGlossiness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.textureGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(271, 219);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Material Data";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.numericTransparency, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.numericGlossiness, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.buttonSPC, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.buttonAMB, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.buttonDIF, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flagsTB, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(265, 200);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // numericTransparency
            // 
            this.numericTransparency.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numericTransparency.Location = new System.Drawing.Point(81, 172);
            this.numericTransparency.Name = "numericTransparency";
            this.numericTransparency.Size = new System.Drawing.Size(181, 20);
            this.numericTransparency.TabIndex = 3;
            this.numericTransparency.ValueChanged += new System.EventHandler(this.numericTransparency_ValueChanged);
            // 
            // numericGlossiness
            // 
            this.numericGlossiness.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.numericGlossiness.Location = new System.Drawing.Point(81, 138);
            this.numericGlossiness.Name = "numericGlossiness";
            this.numericGlossiness.Size = new System.Drawing.Size(181, 20);
            this.numericGlossiness.TabIndex = 3;
            this.numericGlossiness.ValueChanged += new System.EventHandler(this.numericGlossiness_ValueChanged);
            // 
            // buttonSPC
            // 
            this.buttonSPC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSPC.Location = new System.Drawing.Point(81, 106);
            this.buttonSPC.Name = "buttonSPC";
            this.buttonSPC.Size = new System.Drawing.Size(181, 19);
            this.buttonSPC.TabIndex = 4;
            this.buttonSPC.UseVisualStyleBackColor = true;
            this.buttonSPC.Click += new System.EventHandler(this.buttonSPC_Click);
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 142);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Glossiness";
            // 
            // buttonAMB
            // 
            this.buttonAMB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAMB.Location = new System.Drawing.Point(81, 73);
            this.buttonAMB.Name = "buttonAMB";
            this.buttonAMB.Size = new System.Drawing.Size(181, 19);
            this.buttonAMB.TabIndex = 3;
            this.buttonAMB.UseVisualStyleBackColor = true;
            this.buttonAMB.Click += new System.EventHandler(this.buttonAMB_Click);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 176);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Transparency";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 109);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Specular";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Diffuse";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Ambient";
            // 
            // buttonDIF
            // 
            this.buttonDIF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDIF.Location = new System.Drawing.Point(81, 40);
            this.buttonDIF.Name = "buttonDIF";
            this.buttonDIF.Size = new System.Drawing.Size(181, 19);
            this.buttonDIF.TabIndex = 0;
            this.buttonDIF.UseVisualStyleBackColor = true;
            this.buttonDIF.Click += new System.EventHandler(this.buttonDIF_Click);
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(43, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Flags";
            // 
            // flagsTB
            // 
            this.flagsTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.flagsTB.Location = new System.Drawing.Point(81, 6);
            this.flagsTB.Name = "flagsTB";
            this.flagsTB.Size = new System.Drawing.Size(181, 20);
            this.flagsTB.TabIndex = 6;
            this.flagsTB.TextChanged += new System.EventHandler(this.flagsTB_TextChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(158, 237);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(125, 121);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // textureListBox
            // 
            this.textureListBox.FormattingEnabled = true;
            this.textureListBox.Location = new System.Drawing.Point(12, 237);
            this.textureListBox.Name = "textureListBox";
            this.textureListBox.Size = new System.Drawing.Size(140, 121);
            this.textureListBox.TabIndex = 3;
            this.textureListBox.SelectedIndexChanged += new System.EventHandler(this.textureListBox_SelectedIndexChanged);
            // 
            // textureGroupBox
            // 
            this.textureGroupBox.Controls.Add(this.tableLayoutPanel2);
            this.textureGroupBox.Location = new System.Drawing.Point(12, 364);
            this.textureGroupBox.Name = "textureGroupBox";
            this.textureGroupBox.Size = new System.Drawing.Size(271, 238);
            this.textureGroupBox.TabIndex = 4;
            this.textureGroupBox.TabStop = false;
            this.textureGroupBox.Text = "Selected Texture Settings";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 54F));
            this.tableLayoutPanel2.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.textureFlagsTB, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonImportTexture, 1, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.43836F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 83.56165F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(265, 219);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(47, 11);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Texture Flags";
            // 
            // textureFlagsTB
            // 
            this.textureFlagsTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textureFlagsTB.Location = new System.Drawing.Point(124, 8);
            this.textureFlagsTB.Name = "textureFlagsTB";
            this.textureFlagsTB.Size = new System.Drawing.Size(138, 20);
            this.textureFlagsTB.TabIndex = 1;
            this.textureFlagsTB.TextChanged += new System.EventHandler(this.textureFlagsTB_TextChanged);
            // 
            // buttonImportTexture
            // 
            this.buttonImportTexture.Location = new System.Drawing.Point(124, 39);
            this.buttonImportTexture.Name = "buttonImportTexture";
            this.buttonImportTexture.Size = new System.Drawing.Size(138, 23);
            this.buttonImportTexture.TabIndex = 2;
            this.buttonImportTexture.Text = "Import From File";
            this.buttonImportTexture.UseVisualStyleBackColor = true;
            this.buttonImportTexture.Click += new System.EventHandler(this.buttonImportTexture_Click);
            // 
            // DOBJEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(293, 608);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.textureGroupBox);
            this.Controls.Add(this.textureListBox);
            this.Controls.Add(this.groupBox1);
            this.Name = "DOBJEditor";
            this.Text = "DOBJ Editor";
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericTransparency)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericGlossiness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.textureGroupBox.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.NumericUpDown numericTransparency;
        private System.Windows.Forms.NumericUpDown numericGlossiness;
        private System.Windows.Forms.Button buttonSPC;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonAMB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonDIF;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ListBox textureListBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox flagsTB;
        private System.Windows.Forms.GroupBox textureGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textureFlagsTB;
        private System.Windows.Forms.Button buttonImportTexture;
    }
}