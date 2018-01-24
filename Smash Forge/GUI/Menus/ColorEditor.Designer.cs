namespace Smash_Forge.GUI.Menus
{
    partial class ColorEditor
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
            this.colorWTB = new System.Windows.Forms.TextBox();
            this.colorZTB = new System.Windows.Forms.TextBox();
            this.colorYTB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.colorLabelX = new System.Windows.Forms.Label();
            this.colorLabelY = new System.Windows.Forms.Label();
            this.colorLabelZ = new System.Windows.Forms.Label();
            this.colorButton = new System.Windows.Forms.Button();
            this.colorLabelW = new System.Windows.Forms.Label();
            this.colorTrackBarX = new System.Windows.Forms.TrackBar();
            this.colorTrackBarY = new System.Windows.Forms.TrackBar();
            this.colorTrackBarZ = new System.Windows.Forms.TrackBar();
            this.colorTrackBarW = new System.Windows.Forms.TrackBar();
            this.colorXTB = new System.Windows.Forms.TextBox();
            this.editModeComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.colorTrackBarX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.colorTrackBarY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.colorTrackBarZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.colorTrackBarW)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.editModeComboBox);
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Location = new System.Drawing.Point(15, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.groupBox1.Size = new System.Drawing.Size(638, 457);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Color";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 132F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 364F));
            this.tableLayoutPanel1.Controls.Add(this.colorWTB, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.colorZTB, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.colorYTB, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.colorLabelX, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.colorLabelY, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.colorLabelZ, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.colorButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.colorLabelW, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.colorTrackBarX, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.colorTrackBarY, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.colorTrackBarZ, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.colorTrackBarW, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.colorXTB, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 97);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(604, 344);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // colorWTB
            // 
            this.colorWTB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.colorWTB.Location = new System.Drawing.Point(138, 292);
            this.colorWTB.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.colorWTB.Name = "colorWTB";
            this.colorWTB.Size = new System.Drawing.Size(92, 31);
            this.colorWTB.TabIndex = 20;
            this.colorWTB.TextChanged += new System.EventHandler(this.redTB_TextChanged);
            // 
            // colorZTB
            // 
            this.colorZTB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.colorZTB.Location = new System.Drawing.Point(138, 222);
            this.colorZTB.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.colorZTB.Name = "colorZTB";
            this.colorZTB.Size = new System.Drawing.Size(92, 31);
            this.colorZTB.TabIndex = 19;
            this.colorZTB.TextChanged += new System.EventHandler(this.valueTB_TextChanged);
            // 
            // colorYTB
            // 
            this.colorYTB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.colorYTB.Location = new System.Drawing.Point(138, 154);
            this.colorYTB.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.colorYTB.Name = "colorYTB";
            this.colorYTB.Size = new System.Drawing.Size(92, 31);
            this.colorYTB.TabIndex = 18;
            this.colorYTB.TextChanged += new System.EventHandler(this.satTB_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Color";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // colorLabelX
            // 
            this.colorLabelX.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.colorLabelX.AutoSize = true;
            this.colorLabelX.Location = new System.Drawing.Point(6, 89);
            this.colorLabelX.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.colorLabelX.Name = "colorLabelX";
            this.colorLabelX.Size = new System.Drawing.Size(51, 25);
            this.colorLabelX.TabIndex = 1;
            this.colorLabelX.Text = "Hue";
            // 
            // colorLabelY
            // 
            this.colorLabelY.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.colorLabelY.AutoSize = true;
            this.colorLabelY.Location = new System.Drawing.Point(6, 157);
            this.colorLabelY.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.colorLabelY.Name = "colorLabelY";
            this.colorLabelY.Size = new System.Drawing.Size(110, 25);
            this.colorLabelY.TabIndex = 2;
            this.colorLabelY.Text = "Saturation";
            this.colorLabelY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // colorLabelZ
            // 
            this.colorLabelZ.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.colorLabelZ.AutoSize = true;
            this.colorLabelZ.Location = new System.Drawing.Point(6, 225);
            this.colorLabelZ.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.colorLabelZ.Name = "colorLabelZ";
            this.colorLabelZ.Size = new System.Drawing.Size(67, 25);
            this.colorLabelZ.TabIndex = 3;
            this.colorLabelZ.Text = "Value";
            this.colorLabelZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // colorButton
            // 
            this.colorButton.Location = new System.Drawing.Point(138, 6);
            this.colorButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.colorButton.Name = "colorButton";
            this.colorButton.Size = new System.Drawing.Size(96, 44);
            this.colorButton.TabIndex = 5;
            this.colorButton.UseVisualStyleBackColor = true;
            // 
            // colorLabelW
            // 
            this.colorLabelW.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.colorLabelW.AutoSize = true;
            this.colorLabelW.Location = new System.Drawing.Point(6, 295);
            this.colorLabelW.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.colorLabelW.Name = "colorLabelW";
            this.colorLabelW.Size = new System.Drawing.Size(51, 25);
            this.colorLabelW.TabIndex = 6;
            this.colorLabelW.Text = "Red";
            // 
            // colorTrackBarX
            // 
            this.colorTrackBarX.Location = new System.Drawing.Point(246, 74);
            this.colorTrackBarX.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.colorTrackBarX.Maximum = 360;
            this.colorTrackBarX.Name = "colorTrackBarX";
            this.colorTrackBarX.Size = new System.Drawing.Size(352, 56);
            this.colorTrackBarX.TabIndex = 7;
            this.colorTrackBarX.TickStyle = System.Windows.Forms.TickStyle.None;
            this.colorTrackBarX.Value = 360;
            this.colorTrackBarX.Scroll += new System.EventHandler(this.hueTrackBar_Scroll);
            // 
            // colorTrackBarY
            // 
            this.colorTrackBarY.Location = new System.Drawing.Point(246, 142);
            this.colorTrackBarY.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.colorTrackBarY.Name = "colorTrackBarY";
            this.colorTrackBarY.Size = new System.Drawing.Size(352, 56);
            this.colorTrackBarY.TabIndex = 8;
            this.colorTrackBarY.TickStyle = System.Windows.Forms.TickStyle.None;
            this.colorTrackBarY.Scroll += new System.EventHandler(this.satTrackBar_Scroll);
            // 
            // colorTrackBarZ
            // 
            this.colorTrackBarZ.Location = new System.Drawing.Point(246, 210);
            this.colorTrackBarZ.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.colorTrackBarZ.Name = "colorTrackBarZ";
            this.colorTrackBarZ.Size = new System.Drawing.Size(352, 56);
            this.colorTrackBarZ.TabIndex = 9;
            this.colorTrackBarZ.TickStyle = System.Windows.Forms.TickStyle.None;
            this.colorTrackBarZ.Scroll += new System.EventHandler(this.valueTrackBar_Scroll);
            // 
            // colorTrackBarW
            // 
            this.colorTrackBarW.LargeChange = 250;
            this.colorTrackBarW.Location = new System.Drawing.Point(246, 278);
            this.colorTrackBarW.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.colorTrackBarW.Maximum = 100;
            this.colorTrackBarW.Name = "colorTrackBarW";
            this.colorTrackBarW.Size = new System.Drawing.Size(352, 60);
            this.colorTrackBarW.TabIndex = 10;
            this.colorTrackBarW.TickStyle = System.Windows.Forms.TickStyle.None;
            this.colorTrackBarW.Scroll += new System.EventHandler(this.redTrackBar_Scroll);
            // 
            // colorXTB
            // 
            this.colorXTB.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.colorXTB.Location = new System.Drawing.Point(138, 86);
            this.colorXTB.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.colorXTB.Name = "colorXTB";
            this.colorXTB.Size = new System.Drawing.Size(92, 31);
            this.colorXTB.TabIndex = 17;
            this.colorXTB.TextChanged += new System.EventHandler(this.hueTB_TextChanged);
            // 
            // editModeComboBox
            // 
            this.editModeComboBox.FormattingEnabled = true;
            this.editModeComboBox.Items.AddRange(new object[] {
            "RGB",
            "HSV",
            "Temperature (K)"});
            this.editModeComboBox.Location = new System.Drawing.Point(258, 41);
            this.editModeComboBox.Name = "editModeComboBox";
            this.editModeComboBox.Size = new System.Drawing.Size(328, 33);
            this.editModeComboBox.TabIndex = 6;
            this.editModeComboBox.SelectedIndexChanged += new System.EventHandler(this.editModeComboBox_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 44);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(166, 25);
            this.label6.TabIndex = 7;
            this.label6.Text = "Color Edit Mode";
            // 
            // ColorEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(672, 483);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "ColorEditor";
            this.Text = "ColorEditor";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.colorTrackBarX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.colorTrackBarY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.colorTrackBarZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.colorTrackBarW)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox colorWTB;
        private System.Windows.Forms.TextBox colorZTB;
        private System.Windows.Forms.TextBox colorYTB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label colorLabelX;
        private System.Windows.Forms.Label colorLabelY;
        private System.Windows.Forms.Label colorLabelZ;
        private System.Windows.Forms.Button colorButton;
        private System.Windows.Forms.Label colorLabelW;
        private System.Windows.Forms.TrackBar colorTrackBarX;
        private System.Windows.Forms.TrackBar colorTrackBarY;
        private System.Windows.Forms.TrackBar colorTrackBarZ;
        private System.Windows.Forms.TrackBar colorTrackBarW;
        private System.Windows.Forms.TextBox colorXTB;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox editModeComboBox;
    }
}