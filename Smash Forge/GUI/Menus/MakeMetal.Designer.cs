namespace Smash_Forge.GUI.Menus
{
    partial class MakeMetal
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MakeMetal));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.difIDTextBox = new System.Windows.Forms.TextBox();
            this.useDifTexCB = new System.Windows.Forms.CheckBox();
            this.nrmGroupBox = new System.Windows.Forms.GroupBox();
            this.noNrmradioButton = new System.Windows.Forms.RadioButton();
            this.preserveNrmradioButton = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Property4W = new System.Windows.Forms.TextBox();
            this.Property4Z = new System.Windows.Forms.TextBox();
            this.Property4Y = new System.Windows.Forms.TextBox();
            this.Property4X = new System.Windows.Forms.TextBox();
            this.Property3W = new System.Windows.Forms.TextBox();
            this.Property3Z = new System.Windows.Forms.TextBox();
            this.Property3Y = new System.Windows.Forms.TextBox();
            this.Property3X = new System.Windows.Forms.TextBox();
            this.Property2W = new System.Windows.Forms.TextBox();
            this.Property2Z = new System.Windows.Forms.TextBox();
            this.Property2Y = new System.Windows.Forms.TextBox();
            this.Property2X = new System.Windows.Forms.TextBox();
            this.Property1W = new System.Windows.Forms.TextBox();
            this.Property1Z = new System.Windows.Forms.TextBox();
            this.Property1Y = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.Property1X = new System.Windows.Forms.TextBox();
            this.Apply = new System.Windows.Forms.Button();
            this.stageGroupBox = new System.Windows.Forms.GroupBox();
            this.lowResradioButton = new System.Windows.Forms.RadioButton();
            this.highResradioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.nrmGroupBox.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.stageGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.difIDTextBox);
            this.groupBox1.Controls.Add(this.useDifTexCB);
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(286, 84);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Diffuse Map";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Tex ID";
            // 
            // difIDTextBox
            // 
            this.difIDTextBox.Location = new System.Drawing.Point(67, 42);
            this.difIDTextBox.Name = "difIDTextBox";
            this.difIDTextBox.Size = new System.Drawing.Size(100, 20);
            this.difIDTextBox.TabIndex = 1;
            this.difIDTextBox.Text = "40000001";
            this.difIDTextBox.TextChanged += new System.EventHandler(this.difIDTextBox_TextChanged);
            // 
            // useDifTexCB
            // 
            this.useDifTexCB.AutoSize = true;
            this.useDifTexCB.Checked = true;
            this.useDifTexCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useDifTexCB.Location = new System.Drawing.Point(13, 19);
            this.useDifTexCB.Name = "useDifTexCB";
            this.useDifTexCB.Size = new System.Drawing.Size(137, 17);
            this.useDifTexCB.TabIndex = 0;
            this.useDifTexCB.Text = "Override Diffuse Tex ID";
            this.useDifTexCB.UseVisualStyleBackColor = true;
            this.useDifTexCB.CheckedChanged += new System.EventHandler(this.useDifTexCB_CheckedChanged);
            // 
            // nrmGroupBox
            // 
            this.nrmGroupBox.Controls.Add(this.noNrmradioButton);
            this.nrmGroupBox.Controls.Add(this.preserveNrmradioButton);
            this.nrmGroupBox.Location = new System.Drawing.Point(8, 98);
            this.nrmGroupBox.Name = "nrmGroupBox";
            this.nrmGroupBox.Size = new System.Drawing.Size(286, 78);
            this.nrmGroupBox.TabIndex = 1;
            this.nrmGroupBox.TabStop = false;
            this.nrmGroupBox.Text = "Normal Map";
            // 
            // noNrmradioButton
            // 
            this.noNrmradioButton.AutoSize = true;
            this.noNrmradioButton.Location = new System.Drawing.Point(21, 45);
            this.noNrmradioButton.Name = "noNrmradioButton";
            this.noNrmradioButton.Size = new System.Drawing.Size(104, 17);
            this.noNrmradioButton.TabIndex = 1;
            this.noNrmradioButton.Text = "No Normal Maps";
            this.noNrmradioButton.UseVisualStyleBackColor = true;
            this.noNrmradioButton.CheckedChanged += new System.EventHandler(this.noNrmradioButton_CheckedChanged);
            // 
            // preserveNrmradioButton
            // 
            this.preserveNrmradioButton.AutoSize = true;
            this.preserveNrmradioButton.Checked = true;
            this.preserveNrmradioButton.Location = new System.Drawing.Point(21, 22);
            this.preserveNrmradioButton.Name = "preserveNrmradioButton";
            this.preserveNrmradioButton.Size = new System.Drawing.Size(166, 17);
            this.preserveNrmradioButton.TabIndex = 0;
            this.preserveNrmradioButton.TabStop = true;
            this.preserveNrmradioButton.Text = "Preserve Existing Normal Map";
            this.preserveNrmradioButton.UseVisualStyleBackColor = true;
            this.preserveNrmradioButton.CheckedChanged += new System.EventHandler(this.prserveNrmRadioButton_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel1);
            this.groupBox3.Location = new System.Drawing.Point(8, 267);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(286, 169);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Material Properties";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.Controls.Add(this.Property4W, 4, 3);
            this.tableLayoutPanel1.Controls.Add(this.Property4Z, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.Property4Y, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.Property4X, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.Property3W, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.Property3Z, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.Property3Y, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.Property3X, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.Property2W, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.Property2Z, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.Property2Y, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.Property2X, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.Property1W, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.Property1Z, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.Property1Y, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.Property1X, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(266, 138);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // Property4W
            // 
            this.Property4W.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property4W.Location = new System.Drawing.Point(227, 110);
            this.Property4W.Name = "Property4W";
            this.Property4W.Size = new System.Drawing.Size(34, 20);
            this.Property4W.TabIndex = 19;
            this.Property4W.Text = "1.0";
            this.Property4W.TextChanged += new System.EventHandler(this.Property4W_TextChanged);
            // 
            // Property4Z
            // 
            this.Property4Z.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property4Z.Location = new System.Drawing.Point(187, 110);
            this.Property4Z.Name = "Property4Z";
            this.Property4Z.Size = new System.Drawing.Size(32, 20);
            this.Property4Z.TabIndex = 18;
            this.Property4Z.Text = "0.3";
            this.Property4Z.TextChanged += new System.EventHandler(this.Property4Z_TextChanged);
            // 
            // Property4Y
            // 
            this.Property4Y.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property4Y.Location = new System.Drawing.Point(148, 110);
            this.Property4Y.Name = "Property4Y";
            this.Property4Y.Size = new System.Drawing.Size(32, 20);
            this.Property4Y.TabIndex = 17;
            this.Property4Y.Text = "0.3";
            this.Property4Y.TextChanged += new System.EventHandler(this.Property4Y_TextChanged);
            // 
            // Property4X
            // 
            this.Property4X.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property4X.Location = new System.Drawing.Point(109, 110);
            this.Property4X.Name = "Property4X";
            this.Property4X.Size = new System.Drawing.Size(32, 20);
            this.Property4X.TabIndex = 16;
            this.Property4X.Text = "0.3";
            this.Property4X.TextChanged += new System.EventHandler(this.property4X_TextChanged);
            // 
            // Property3W
            // 
            this.Property3W.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property3W.Location = new System.Drawing.Point(227, 75);
            this.Property3W.Name = "Property3W";
            this.Property3W.Size = new System.Drawing.Size(34, 20);
            this.Property3W.TabIndex = 15;
            this.Property3W.Text = "1.0";
            this.Property3W.TextChanged += new System.EventHandler(this.Property3W_TextChanged);
            // 
            // Property3Z
            // 
            this.Property3Z.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property3Z.Location = new System.Drawing.Point(187, 75);
            this.Property3Z.Name = "Property3Z";
            this.Property3Z.Size = new System.Drawing.Size(32, 20);
            this.Property3Z.TabIndex = 14;
            this.Property3Z.Text = "0.0";
            this.Property3Z.TextChanged += new System.EventHandler(this.Property3Z_TextChanged);
            // 
            // Property3Y
            // 
            this.Property3Y.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property3Y.Location = new System.Drawing.Point(148, 75);
            this.Property3Y.Name = "Property3Y";
            this.Property3Y.Size = new System.Drawing.Size(32, 20);
            this.Property3Y.TabIndex = 13;
            this.Property3Y.Text = "0.0";
            this.Property3Y.TextChanged += new System.EventHandler(this.Property3Y_TextChanged);
            // 
            // Property3X
            // 
            this.Property3X.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property3X.Location = new System.Drawing.Point(109, 75);
            this.Property3X.Name = "Property3X";
            this.Property3X.Size = new System.Drawing.Size(32, 20);
            this.Property3X.TabIndex = 12;
            this.Property3X.Text = "3.7";
            this.Property3X.TextChanged += new System.EventHandler(this.Property3X_TextChanged);
            // 
            // Property2W
            // 
            this.Property2W.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property2W.Location = new System.Drawing.Point(227, 41);
            this.Property2W.Name = "Property2W";
            this.Property2W.Size = new System.Drawing.Size(34, 20);
            this.Property2W.TabIndex = 11;
            this.Property2W.Text = "1.0";
            this.Property2W.TextChanged += new System.EventHandler(this.Property2W_TextChanged);
            // 
            // Property2Z
            // 
            this.Property2Z.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property2Z.Location = new System.Drawing.Point(187, 41);
            this.Property2Z.Name = "Property2Z";
            this.Property2Z.Size = new System.Drawing.Size(32, 20);
            this.Property2Z.TabIndex = 10;
            this.Property2Z.Text = "0.6";
            this.Property2Z.TextChanged += new System.EventHandler(this.Property2Z_TextChanged);
            // 
            // Property2Y
            // 
            this.Property2Y.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property2Y.Location = new System.Drawing.Point(148, 41);
            this.Property2Y.Name = "Property2Y";
            this.Property2Y.Size = new System.Drawing.Size(32, 20);
            this.Property2Y.TabIndex = 9;
            this.Property2Y.Text = "0.6";
            this.Property2Y.TextChanged += new System.EventHandler(this.Property2Y_TextChanged);
            // 
            // Property2X
            // 
            this.Property2X.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property2X.Location = new System.Drawing.Point(109, 41);
            this.Property2X.Name = "Property2X";
            this.Property2X.Size = new System.Drawing.Size(32, 20);
            this.Property2X.TabIndex = 8;
            this.Property2X.Text = "0.6";
            this.Property2X.TextChanged += new System.EventHandler(this.Property2X_TextChanged);
            // 
            // Property1W
            // 
            this.Property1W.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property1W.Location = new System.Drawing.Point(227, 7);
            this.Property1W.Name = "Property1W";
            this.Property1W.Size = new System.Drawing.Size(34, 20);
            this.Property1W.TabIndex = 7;
            this.Property1W.Text = "3.0";
            this.Property1W.TextChanged += new System.EventHandler(this.Property1W_TextChanged);
            // 
            // Property1Z
            // 
            this.Property1Z.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property1Z.Location = new System.Drawing.Point(187, 7);
            this.Property1Z.Name = "Property1Z";
            this.Property1Z.Size = new System.Drawing.Size(32, 20);
            this.Property1Z.TabIndex = 6;
            this.Property1Z.Text = "3.0";
            this.Property1Z.TextChanged += new System.EventHandler(this.Property1Z_TextChanged);
            // 
            // Property1Y
            // 
            this.Property1Y.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property1Y.Location = new System.Drawing.Point(148, 7);
            this.Property1Y.Name = "Property1Y";
            this.Property1Y.Size = new System.Drawing.Size(32, 20);
            this.Property1Y.TabIndex = 5;
            this.Property1Y.Text = "3.0";
            this.Property1Y.TextChanged += new System.EventHandler(this.Property1Y_TextChanged);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "NU_reflectionColor";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "NU_fresnelParams";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "NU_fresnelColor";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 113);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "NU_aoMinGain";
            // 
            // Property1X
            // 
            this.Property1X.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Property1X.Location = new System.Drawing.Point(109, 7);
            this.Property1X.Name = "Property1X";
            this.Property1X.Size = new System.Drawing.Size(32, 20);
            this.Property1X.TabIndex = 4;
            this.Property1X.Text = "3.0";
            this.Property1X.TextChanged += new System.EventHandler(this.Property1X_TextChanged);
            // 
            // Apply
            // 
            this.Apply.Location = new System.Drawing.Point(100, 442);
            this.Apply.Name = "Apply";
            this.Apply.Size = new System.Drawing.Size(98, 29);
            this.Apply.TabIndex = 4;
            this.Apply.Text = "Apply";
            this.Apply.UseVisualStyleBackColor = true;
            this.Apply.Click += new System.EventHandler(this.Apply_Click);
            // 
            // stageGroupBox
            // 
            this.stageGroupBox.Controls.Add(this.lowResradioButton);
            this.stageGroupBox.Controls.Add(this.highResradioButton);
            this.stageGroupBox.Location = new System.Drawing.Point(8, 183);
            this.stageGroupBox.Name = "stageGroupBox";
            this.stageGroupBox.Size = new System.Drawing.Size(286, 78);
            this.stageGroupBox.TabIndex = 5;
            this.stageGroupBox.TabStop = false;
            this.stageGroupBox.Text = "Stage Cube Map";
            // 
            // lowResradioButton
            // 
            this.lowResradioButton.AutoSize = true;
            this.lowResradioButton.Location = new System.Drawing.Point(21, 45);
            this.lowResradioButton.Name = "lowResradioButton";
            this.lowResradioButton.Size = new System.Drawing.Size(114, 17);
            this.lowResradioButton.TabIndex = 1;
            this.lowResradioButton.Text = "Rough (10101000)";
            this.lowResradioButton.UseVisualStyleBackColor = true;
            this.lowResradioButton.CheckedChanged += new System.EventHandler(this.lowResradioButton_CheckedChanged);
            // 
            // highResradioButton
            // 
            this.highResradioButton.AutoSize = true;
            this.highResradioButton.Checked = true;
            this.highResradioButton.Location = new System.Drawing.Point(21, 22);
            this.highResradioButton.Name = "highResradioButton";
            this.highResradioButton.Size = new System.Drawing.Size(113, 17);
            this.highResradioButton.TabIndex = 0;
            this.highResradioButton.TabStop = true;
            this.highResradioButton.Text = "Glossy (10102000)";
            this.highResradioButton.UseVisualStyleBackColor = true;
            this.highResradioButton.CheckedChanged += new System.EventHandler(this.highResradioButton_CheckedChanged);
            // 
            // MakeMetal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 482);
            this.Controls.Add(this.stageGroupBox);
            this.Controls.Add(this.Apply);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.nrmGroupBox);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MakeMetal";
            this.Text = "Make Metal";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.nrmGroupBox.ResumeLayout(false);
            this.nrmGroupBox.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.stageGroupBox.ResumeLayout(false);
            this.stageGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox difIDTextBox;
        private System.Windows.Forms.CheckBox useDifTexCB;
        private System.Windows.Forms.GroupBox nrmGroupBox;
        private System.Windows.Forms.RadioButton noNrmradioButton;
        private System.Windows.Forms.RadioButton preserveNrmradioButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button Apply;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox Property4W;
        private System.Windows.Forms.TextBox Property4Z;
        private System.Windows.Forms.TextBox Property4Y;
        private System.Windows.Forms.TextBox Property4X;
        private System.Windows.Forms.TextBox Property3W;
        private System.Windows.Forms.TextBox Property3Z;
        private System.Windows.Forms.TextBox Property3Y;
        private System.Windows.Forms.TextBox Property3X;
        private System.Windows.Forms.TextBox Property2W;
        private System.Windows.Forms.TextBox Property2Z;
        private System.Windows.Forms.TextBox Property2Y;
        private System.Windows.Forms.TextBox Property2X;
        private System.Windows.Forms.TextBox Property1W;
        private System.Windows.Forms.TextBox Property1Z;
        private System.Windows.Forms.TextBox Property1Y;
        private System.Windows.Forms.TextBox Property1X;
        private System.Windows.Forms.GroupBox stageGroupBox;
        private System.Windows.Forms.RadioButton lowResradioButton;
        private System.Windows.Forms.RadioButton highResradioButton;
    }
}