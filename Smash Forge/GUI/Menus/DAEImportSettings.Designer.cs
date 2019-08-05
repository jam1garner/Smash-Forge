namespace SmashForge
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
            this.flipUVCB = new System.Windows.Forms.CheckBox();
            this.vertColorDivCB = new System.Windows.Forms.CheckBox();
            this.stageMatCB = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.scaleTB = new System.Windows.Forms.TextBox();
            this.smoothNrmCB = new System.Windows.Forms.CheckBox();
            this.vbnFileLabel = new System.Windows.Forms.Label();
            this.openVbnButton = new System.Windows.Forms.Button();
            this.vertcolorCB = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.boneTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.vertTypeComboBox = new System.Windows.Forms.ComboBox();
            this.rotate90CB = new System.Windows.Forms.CheckBox();
            this.transUvVerticalCB = new System.Windows.Forms.CheckBox();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.optionsGroupBox.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // importButton
            // 
            this.importButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.importButton.Location = new System.Drawing.Point(3, 286);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(277, 23);
            this.importButton.TabIndex = 0;
            this.importButton.Text = "Import";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // flipUVCB
            // 
            this.flipUVCB.AutoSize = true;
            this.flipUVCB.Location = new System.Drawing.Point(3, 118);
            this.flipUVCB.Name = "flipUVCB";
            this.flipUVCB.Size = new System.Drawing.Size(65, 17);
            this.flipUVCB.TabIndex = 2;
            this.flipUVCB.Text = "Flip UVs";
            this.flipUVCB.UseVisualStyleBackColor = true;
            // 
            // vertColorDivCB
            // 
            this.vertColorDivCB.AutoSize = true;
            this.vertColorDivCB.Location = new System.Drawing.Point(3, 141);
            this.vertColorDivCB.Name = "vertColorDivCB";
            this.vertColorDivCB.Size = new System.Drawing.Size(171, 17);
            this.vertColorDivCB.TabIndex = 15;
            this.vertColorDivCB.Text = "Divide vertex color values by 2";
            this.vertColorDivCB.UseVisualStyleBackColor = true;
            // 
            // stageMatCB
            // 
            this.stageMatCB.AutoSize = true;
            this.stageMatCB.Location = new System.Drawing.Point(3, 95);
            this.stageMatCB.Name = "stageMatCB";
            this.stageMatCB.Size = new System.Drawing.Size(116, 17);
            this.stageMatCB.TabIndex = 14;
            this.stageMatCB.Text = "Use Stage Material";
            this.stageMatCB.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Scale";
            // 
            // scaleTB
            // 
            this.scaleTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scaleTB.Location = new System.Drawing.Point(141, 3);
            this.scaleTB.Name = "scaleTB";
            this.scaleTB.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.scaleTB.Size = new System.Drawing.Size(133, 20);
            this.scaleTB.TabIndex = 12;
            this.scaleTB.Text = "1";
            // 
            // smoothNrmCB
            // 
            this.smoothNrmCB.AutoSize = true;
            this.smoothNrmCB.Location = new System.Drawing.Point(3, 26);
            this.smoothNrmCB.Name = "smoothNrmCB";
            this.smoothNrmCB.Size = new System.Drawing.Size(103, 17);
            this.smoothNrmCB.TabIndex = 11;
            this.smoothNrmCB.Text = "Smooth Normals";
            this.smoothNrmCB.UseVisualStyleBackColor = true;
            // 
            // vbnFileLabel
            // 
            this.vbnFileLabel.AutoSize = true;
            this.vbnFileLabel.Location = new System.Drawing.Point(445, 364);
            this.vbnFileLabel.Name = "vbnFileLabel";
            this.vbnFileLabel.Size = new System.Drawing.Size(0, 13);
            this.vbnFileLabel.TabIndex = 10;
            // 
            // openVbnButton
            // 
            this.openVbnButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.openVbnButton.Location = new System.Drawing.Point(3, 257);
            this.openVbnButton.Name = "openVbnButton";
            this.openVbnButton.Size = new System.Drawing.Size(277, 23);
            this.openVbnButton.TabIndex = 9;
            this.openVbnButton.Text = "Open VBN";
            this.openVbnButton.UseVisualStyleBackColor = true;
            this.openVbnButton.Click += new System.EventHandler(this.openVbnButton_Click);
            // 
            // vertcolorCB
            // 
            this.vertcolorCB.AutoSize = true;
            this.vertcolorCB.Location = new System.Drawing.Point(3, 72);
            this.vertcolorCB.Name = "vertcolorCB";
            this.vertcolorCB.Size = new System.Drawing.Size(193, 17);
            this.vertcolorCB.TabIndex = 8;
            this.vertcolorCB.Text = "Ignore vertex colors (sets all to 127)";
            this.vertcolorCB.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 56);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Weight Type";
            // 
            // boneTypeComboBox
            // 
            this.boneTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boneTypeComboBox.FormattingEnabled = true;
            this.boneTypeComboBox.Location = new System.Drawing.Point(141, 59);
            this.boneTypeComboBox.Name = "boneTypeComboBox";
            this.boneTypeComboBox.Size = new System.Drawing.Size(133, 21);
            this.boneTypeComboBox.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Vert Type";
            // 
            // vertTypeComboBox
            // 
            this.vertTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vertTypeComboBox.FormattingEnabled = true;
            this.vertTypeComboBox.Location = new System.Drawing.Point(141, 31);
            this.vertTypeComboBox.Name = "vertTypeComboBox";
            this.vertTypeComboBox.Size = new System.Drawing.Size(133, 21);
            this.vertTypeComboBox.TabIndex = 5;
            // 
            // rotate90CB
            // 
            this.rotate90CB.AutoSize = true;
            this.rotate90CB.Location = new System.Drawing.Point(3, 49);
            this.rotate90CB.Name = "rotate90CB";
            this.rotate90CB.Size = new System.Drawing.Size(114, 17);
            this.rotate90CB.TabIndex = 6;
            this.rotate90CB.Text = "Rotate 90 degrees";
            this.rotate90CB.UseVisualStyleBackColor = true;
            // 
            // transUvVerticalCB
            // 
            this.transUvVerticalCB.AutoSize = true;
            this.transUvVerticalCB.Location = new System.Drawing.Point(3, 3);
            this.transUvVerticalCB.Name = "transUvVerticalCB";
            this.transUvVerticalCB.Size = new System.Drawing.Size(163, 17);
            this.transUvVerticalCB.TabIndex = 3;
            this.transUvVerticalCB.Text = "Translate UVs vertically by -1";
            this.transUvVerticalCB.UseVisualStyleBackColor = true;
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.flowLayoutPanel1);
            this.optionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(291, 333);
            this.optionsGroupBox.TabIndex = 5;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Import Options";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.transUvVerticalCB);
            this.flowLayoutPanel1.Controls.Add(this.smoothNrmCB);
            this.flowLayoutPanel1.Controls.Add(this.rotate90CB);
            this.flowLayoutPanel1.Controls.Add(this.vertcolorCB);
            this.flowLayoutPanel1.Controls.Add(this.stageMatCB);
            this.flowLayoutPanel1.Controls.Add(this.flipUVCB);
            this.flowLayoutPanel1.Controls.Add(this.vertColorDivCB);
            this.flowLayoutPanel1.Controls.Add(this.tableLayoutPanel1);
            this.flowLayoutPanel1.Controls.Add(this.openVbnButton);
            this.flowLayoutPanel1.Controls.Add(this.importButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(285, 314);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.scaleTB, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.boneTypeComboBox, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.vertTypeComboBox, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 164);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(277, 87);
            this.tableLayoutPanel1.TabIndex = 14;
            // 
            // DAEImportSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 333);
            this.Controls.Add(this.optionsGroupBox);
            this.Controls.Add(this.vbnFileLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DAEImportSettings";
            this.Text = "DAEImportSettings";
            this.optionsGroupBox.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button importButton;
        public System.Windows.Forms.CheckBox flipUVCB;
        private System.Windows.Forms.CheckBox rotate90CB;
        private System.Windows.Forms.CheckBox transUvVerticalCB;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.ComboBox boneTypeComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox vertTypeComboBox;
        private System.Windows.Forms.CheckBox vertcolorCB;
        private System.Windows.Forms.CheckBox smoothNrmCB;
        public System.Windows.Forms.Label vbnFileLabel;
        private System.Windows.Forms.Button openVbnButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox scaleTB;
        public System.Windows.Forms.CheckBox stageMatCB;
        private System.Windows.Forms.CheckBox vertColorDivCB;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}