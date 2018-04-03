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
            this.importButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.flipUVCB = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.vertColorDivCB = new System.Windows.Forms.CheckBox();
            this.stageMatCB = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.scaleTB = new System.Windows.Forms.TextBox();
            this.smoothNrmCB = new System.Windows.Forms.CheckBox();
            this.vbnFileLabel = new System.Windows.Forms.Label();
            this.openVbnButton = new System.Windows.Forms.Button();
            this.vertcolorCB = new System.Windows.Forms.CheckBox();
            this.importTexCB = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.boneTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.vertTypeComboBox = new System.Windows.Forms.ComboBox();
            this.rotate90CB = new System.Windows.Forms.CheckBox();
            this.transUvVerticalCB = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // importButton
            // 
            this.importButton.Location = new System.Drawing.Point(234, 715);
            this.importButton.Margin = new System.Windows.Forms.Padding(6);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(150, 44);
            this.importButton.TabIndex = 0;
            this.importButton.Text = "Import";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(74, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(486, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Note: Noesis exported DAEs are the most reliable";
            // 
            // flipUVCB
            // 
            this.flipUVCB.AutoSize = true;
            this.flipUVCB.Location = new System.Drawing.Point(6, 6);
            this.flipUVCB.Margin = new System.Windows.Forms.Padding(6);
            this.flipUVCB.Name = "flipUVCB";
            this.flipUVCB.Size = new System.Drawing.Size(125, 29);
            this.flipUVCB.TabIndex = 2;
            this.flipUVCB.Text = "Flip UVs";
            this.flipUVCB.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.vertColorDivCB);
            this.panel1.Controls.Add(this.stageMatCB);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.scaleTB);
            this.panel1.Controls.Add(this.smoothNrmCB);
            this.panel1.Controls.Add(this.vbnFileLabel);
            this.panel1.Controls.Add(this.openVbnButton);
            this.panel1.Controls.Add(this.vertcolorCB);
            this.panel1.Controls.Add(this.importTexCB);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.boneTypeComboBox);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.vertTypeComboBox);
            this.panel1.Controls.Add(this.rotate90CB);
            this.panel1.Controls.Add(this.transUvVerticalCB);
            this.panel1.Controls.Add(this.flipUVCB);
            this.panel1.Location = new System.Drawing.Point(30, 106);
            this.panel1.Margin = new System.Windows.Forms.Padding(6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(556, 598);
            this.panel1.TabIndex = 3;
            // 
            // vertColorDivCB
            // 
            this.vertColorDivCB.AutoSize = true;
            this.vertColorDivCB.Location = new System.Drawing.Point(8, 229);
            this.vertColorDivCB.Margin = new System.Windows.Forms.Padding(6);
            this.vertColorDivCB.Name = "vertColorDivCB";
            this.vertColorDivCB.Size = new System.Drawing.Size(338, 29);
            this.vertColorDivCB.TabIndex = 15;
            this.vertColorDivCB.Text = "Divide vertex color values by 2";
            this.vertColorDivCB.UseVisualStyleBackColor = true;
            // 
            // stageMatCB
            // 
            this.stageMatCB.AutoSize = true;
            this.stageMatCB.Location = new System.Drawing.Point(8, 319);
            this.stageMatCB.Margin = new System.Windows.Forms.Padding(6);
            this.stageMatCB.Name = "stageMatCB";
            this.stageMatCB.Size = new System.Drawing.Size(227, 29);
            this.stageMatCB.TabIndex = 14;
            this.stageMatCB.Text = "Use Stage Material";
            this.stageMatCB.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 377);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 25);
            this.label5.TabIndex = 13;
            this.label5.Text = "Scale";
            // 
            // scaleTB
            // 
            this.scaleTB.Location = new System.Drawing.Point(86, 371);
            this.scaleTB.Margin = new System.Windows.Forms.Padding(6);
            this.scaleTB.Name = "scaleTB";
            this.scaleTB.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.scaleTB.Size = new System.Drawing.Size(80, 31);
            this.scaleTB.TabIndex = 12;
            this.scaleTB.Text = "1";
            // 
            // smoothNrmCB
            // 
            this.smoothNrmCB.AutoSize = true;
            this.smoothNrmCB.Location = new System.Drawing.Point(8, 273);
            this.smoothNrmCB.Margin = new System.Windows.Forms.Padding(6);
            this.smoothNrmCB.Name = "smoothNrmCB";
            this.smoothNrmCB.Size = new System.Drawing.Size(202, 29);
            this.smoothNrmCB.TabIndex = 11;
            this.smoothNrmCB.Text = "Smooth Normals";
            this.smoothNrmCB.UseVisualStyleBackColor = true;
            // 
            // vbnFileLabel
            // 
            this.vbnFileLabel.AutoSize = true;
            this.vbnFileLabel.Location = new System.Drawing.Point(170, 431);
            this.vbnFileLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.vbnFileLabel.Name = "vbnFileLabel";
            this.vbnFileLabel.Size = new System.Drawing.Size(0, 25);
            this.vbnFileLabel.TabIndex = 10;
            // 
            // openVbnButton
            // 
            this.openVbnButton.Location = new System.Drawing.Point(8, 421);
            this.openVbnButton.Margin = new System.Windows.Forms.Padding(6);
            this.openVbnButton.Name = "openVbnButton";
            this.openVbnButton.Size = new System.Drawing.Size(150, 44);
            this.openVbnButton.TabIndex = 9;
            this.openVbnButton.Text = "Open VBN";
            this.openVbnButton.UseVisualStyleBackColor = true;
            this.openVbnButton.Click += new System.EventHandler(this.openVbnButton_Click);
            // 
            // vertcolorCB
            // 
            this.vertcolorCB.AutoSize = true;
            this.vertcolorCB.Location = new System.Drawing.Point(8, 185);
            this.vertcolorCB.Margin = new System.Windows.Forms.Padding(6);
            this.vertcolorCB.Name = "vertcolorCB";
            this.vertcolorCB.Size = new System.Drawing.Size(387, 29);
            this.vertcolorCB.TabIndex = 8;
            this.vertcolorCB.Text = "Ignore vertex colors (sets all to 127)";
            this.vertcolorCB.UseVisualStyleBackColor = true;
            // 
            // importTexCB
            // 
            this.importTexCB.AutoSize = true;
            this.importTexCB.Location = new System.Drawing.Point(8, 138);
            this.importTexCB.Margin = new System.Windows.Forms.Padding(6);
            this.importTexCB.Name = "importTexCB";
            this.importTexCB.Size = new System.Drawing.Size(193, 29);
            this.importTexCB.TabIndex = 7;
            this.importTexCB.Text = "Import Textures";
            this.importTexCB.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 535);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 25);
            this.label4.TabIndex = 5;
            this.label4.Text = "Weight Type";
            // 
            // boneTypeComboBox
            // 
            this.boneTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boneTypeComboBox.FormattingEnabled = true;
            this.boneTypeComboBox.Location = new System.Drawing.Point(152, 529);
            this.boneTypeComboBox.Margin = new System.Windows.Forms.Padding(6);
            this.boneTypeComboBox.Name = "boneTypeComboBox";
            this.boneTypeComboBox.Size = new System.Drawing.Size(394, 33);
            this.boneTypeComboBox.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 483);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 25);
            this.label3.TabIndex = 5;
            this.label3.Text = "Vert Type";
            // 
            // vertTypeComboBox
            // 
            this.vertTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vertTypeComboBox.FormattingEnabled = true;
            this.vertTypeComboBox.Location = new System.Drawing.Point(152, 477);
            this.vertTypeComboBox.Margin = new System.Windows.Forms.Padding(6);
            this.vertTypeComboBox.Name = "vertTypeComboBox";
            this.vertTypeComboBox.Size = new System.Drawing.Size(394, 33);
            this.vertTypeComboBox.TabIndex = 5;
            // 
            // rotate90CB
            // 
            this.rotate90CB.AutoSize = true;
            this.rotate90CB.Location = new System.Drawing.Point(6, 50);
            this.rotate90CB.Margin = new System.Windows.Forms.Padding(6);
            this.rotate90CB.Name = "rotate90CB";
            this.rotate90CB.Size = new System.Drawing.Size(221, 29);
            this.rotate90CB.TabIndex = 6;
            this.rotate90CB.Text = "Rotate 90 degrees";
            this.rotate90CB.UseVisualStyleBackColor = true;
            // 
            // transUvVerticalCB
            // 
            this.transUvVerticalCB.AutoSize = true;
            this.transUvVerticalCB.Location = new System.Drawing.Point(8, 94);
            this.transUvVerticalCB.Margin = new System.Windows.Forms.Padding(6);
            this.transUvVerticalCB.Name = "transUvVerticalCB";
            this.transUvVerticalCB.Size = new System.Drawing.Size(325, 29);
            this.transUvVerticalCB.TabIndex = 3;
            this.transUvVerticalCB.Text = "Translate UVs vertically by -1";
            this.transUvVerticalCB.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 75);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(151, 25);
            this.label2.TabIndex = 4;
            this.label2.Text = "Import Options";
            // 
            // DAEImportSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(610, 779);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.importButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "DAEImportSettings";
            this.Text = "DAEImportSettings";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.CheckBox flipUVCB;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox rotate90CB;
        private System.Windows.Forms.CheckBox transUvVerticalCB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.ComboBox boneTypeComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox vertTypeComboBox;
        public System.Windows.Forms.CheckBox importTexCB;
        private System.Windows.Forms.CheckBox vertcolorCB;
        private System.Windows.Forms.CheckBox smoothNrmCB;
        public System.Windows.Forms.Label vbnFileLabel;
        private System.Windows.Forms.Button openVbnButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox scaleTB;
        public System.Windows.Forms.CheckBox stageMatCB;
        private System.Windows.Forms.CheckBox vertColorDivCB;
    }
}