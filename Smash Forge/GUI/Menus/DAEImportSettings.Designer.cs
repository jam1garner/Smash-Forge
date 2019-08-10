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
            this.label5 = new System.Windows.Forms.Label();
            this.scaleTB = new System.Windows.Forms.TextBox();
            this.vbnFileLabel = new System.Windows.Forms.Label();
            this.openVbnButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.boneTypeComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.vertTypeComboBox = new System.Windows.Forms.ComboBox();
            this.rotate90CB = new System.Windows.Forms.CheckBox();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.vertColorComboBox = new System.Windows.Forms.ComboBox();
            this.optionsGroupBox.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // importButton
            // 
            this.importButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.importButton.Location = new System.Drawing.Point(6, 361);
            this.importButton.Margin = new System.Windows.Forms.Padding(6);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(572, 44);
            this.importButton.TabIndex = 0;
            this.importButton.Text = "Import";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(111, 18);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 25);
            this.label5.TabIndex = 13;
            this.label5.Text = "Scale";
            // 
            // scaleTB
            // 
            this.scaleTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.scaleTB.Location = new System.Drawing.Point(189, 15);
            this.scaleTB.Margin = new System.Windows.Forms.Padding(6);
            this.scaleTB.Name = "scaleTB";
            this.scaleTB.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.scaleTB.Size = new System.Drawing.Size(377, 31);
            this.scaleTB.TabIndex = 12;
            this.scaleTB.Text = "1";
            // 
            // vbnFileLabel
            // 
            this.vbnFileLabel.AutoSize = true;
            this.vbnFileLabel.Location = new System.Drawing.Point(890, 700);
            this.vbnFileLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.vbnFileLabel.Name = "vbnFileLabel";
            this.vbnFileLabel.Size = new System.Drawing.Size(0, 25);
            this.vbnFileLabel.TabIndex = 10;
            // 
            // openVbnButton
            // 
            this.openVbnButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.openVbnButton.Location = new System.Drawing.Point(6, 305);
            this.openVbnButton.Margin = new System.Windows.Forms.Padding(6);
            this.openVbnButton.Name = "openVbnButton";
            this.openVbnButton.Size = new System.Drawing.Size(572, 44);
            this.openVbnButton.TabIndex = 9;
            this.openVbnButton.Text = "Open VBN";
            this.openVbnButton.UseVisualStyleBackColor = true;
            this.openVbnButton.Click += new System.EventHandler(this.openVbnButton_Click);
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(44, 140);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(133, 25);
            this.label4.TabIndex = 5;
            this.label4.Text = "Weight Type";
            // 
            // boneTypeComboBox
            // 
            this.boneTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.boneTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boneTypeComboBox.FormattingEnabled = true;
            this.boneTypeComboBox.Location = new System.Drawing.Point(189, 136);
            this.boneTypeComboBox.Margin = new System.Windows.Forms.Padding(6);
            this.boneTypeComboBox.Name = "boneTypeComboBox";
            this.boneTypeComboBox.Size = new System.Drawing.Size(377, 33);
            this.boneTypeComboBox.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(49, 79);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(128, 25);
            this.label3.TabIndex = 5;
            this.label3.Text = "Vertex Type";
            // 
            // vertTypeComboBox
            // 
            this.vertTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.vertTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vertTypeComboBox.FormattingEnabled = true;
            this.vertTypeComboBox.Location = new System.Drawing.Point(189, 75);
            this.vertTypeComboBox.Margin = new System.Windows.Forms.Padding(6);
            this.vertTypeComboBox.Name = "vertTypeComboBox";
            this.vertTypeComboBox.Size = new System.Drawing.Size(377, 33);
            this.vertTypeComboBox.TabIndex = 5;
            // 
            // rotate90CB
            // 
            this.rotate90CB.AutoSize = true;
            this.rotate90CB.Location = new System.Drawing.Point(6, 6);
            this.rotate90CB.Margin = new System.Windows.Forms.Padding(6);
            this.rotate90CB.Name = "rotate90CB";
            this.rotate90CB.Size = new System.Drawing.Size(314, 29);
            this.rotate90CB.TabIndex = 6;
            this.rotate90CB.Text = "Rotate model by 90 degrees";
            this.rotate90CB.UseVisualStyleBackColor = true;
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.flowLayoutPanel1);
            this.optionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.optionsGroupBox.Margin = new System.Windows.Forms.Padding(6);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Padding = new System.Windows.Forms.Padding(6);
            this.optionsGroupBox.Size = new System.Drawing.Size(599, 458);
            this.optionsGroupBox.TabIndex = 5;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Import Options";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.rotate90CB);
            this.flowLayoutPanel1.Controls.Add(this.tableLayoutPanel1);
            this.flowLayoutPanel1.Controls.Add(this.openVbnButton);
            this.flowLayoutPanel1.Controls.Add(this.importButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(6, 30);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(6);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(587, 422);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.16783F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.83217F));
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.scaleTB, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.boneTypeComboBox, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.vertTypeComboBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.vertColorComboBox, 1, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 47);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(572, 246);
            this.tableLayoutPanel1.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(49, 202);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 25);
            this.label1.TabIndex = 14;
            this.label1.Text = "Vertex Color";
            // 
            // vertColorComboBox
            // 
            this.vertColorComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.vertColorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vertColorComboBox.FormattingEnabled = true;
            this.vertColorComboBox.Items.AddRange(new object[] {
            "Preserve existing",
            "Set to white",
            "Divide by 2"});
            this.vertColorComboBox.Location = new System.Drawing.Point(186, 198);
            this.vertColorComboBox.Name = "vertColorComboBox";
            this.vertColorComboBox.Size = new System.Drawing.Size(383, 33);
            this.vertColorComboBox.TabIndex = 15;
            // 
            // DAEImportSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 458);
            this.Controls.Add(this.optionsGroupBox);
            this.Controls.Add(this.vbnFileLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
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
        private System.Windows.Forms.CheckBox rotate90CB;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.ComboBox boneTypeComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox vertTypeComboBox;
        public System.Windows.Forms.Label vbnFileLabel;
        private System.Windows.Forms.Button openVbnButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox scaleTB;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox vertColorComboBox;
    }
}