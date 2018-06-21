namespace Smash_Forge
{
    partial class PolygonFormatEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PolygonFormatEditor));
            this.weightTypeLabel = new System.Windows.Forms.Label();
            this.normalTypeLabel = new System.Windows.Forms.Label();
            this.weightTypeComboBox = new System.Windows.Forms.ComboBox();
            this.normalTypeComboBox = new System.Windows.Forms.ComboBox();
            this.vertexColorCB = new System.Windows.Forms.CheckBox();
            this.uvCountLabel = new System.Windows.Forms.Label();
            this.uvCountUpDown = new System.Windows.Forms.NumericUpDown();
            this.applyButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // weightTypeLabel
            // 
            this.weightTypeLabel.AutoSize = true;
            this.weightTypeLabel.Location = new System.Drawing.Point(6, 6);
            this.weightTypeLabel.Name = "weightTypeLabel";
            this.weightTypeLabel.Size = new System.Drawing.Size(141, 25);
            this.weightTypeLabel.Text = "Weight Type";
            // 
            // normalTypeLabel
            // 
            this.normalTypeLabel.AutoSize = true;
            this.normalTypeLabel.Location = new System.Drawing.Point(153, 6);
            this.normalTypeLabel.Name = "normalTypeLabel";
            this.normalTypeLabel.Size = new System.Drawing.Size(141, 25);
            this.normalTypeLabel.Text = "Normal Type";
            // 
            // weightTypeComboBox
            // 
            this.weightTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.weightTypeComboBox.FormattingEnabled = true;
            this.weightTypeComboBox.Location = new System.Drawing.Point(6, 26);
            this.weightTypeComboBox.Margin = new System.Windows.Forms.Padding(6);
            this.weightTypeComboBox.Name = "weightTypeComboBox";
            this.weightTypeComboBox.Size = new System.Drawing.Size(141, 33);
            this.weightTypeComboBox.Text = "Weight Type";
            this.weightTypeComboBox.TabIndex = 0;
            // 
            // normalTypeComboBox
            // 
            this.normalTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.normalTypeComboBox.FormattingEnabled = true;
            this.normalTypeComboBox.Location = new System.Drawing.Point(153, 26);
            this.normalTypeComboBox.Margin = new System.Windows.Forms.Padding(6);
            this.normalTypeComboBox.Name = "normalTypeComboBox";
            this.normalTypeComboBox.Size = new System.Drawing.Size(141, 33);
            this.normalTypeComboBox.TabIndex = 1;
            // 
            // vertexColorCB
            // 
            this.vertexColorCB.AutoSize = true;
            this.vertexColorCB.Location = new System.Drawing.Point(6, 59);
            this.vertexColorCB.Margin = new System.Windows.Forms.Padding(2);
            this.vertexColorCB.Name = "vertexColorCB";
            this.vertexColorCB.Size = new System.Drawing.Size(110, 17);
            this.vertexColorCB.Text = "Has Vertex Color";
            this.vertexColorCB.UseVisualStyleBackColor = true;
            this.vertexColorCB.TabIndex = 2;
            // 
            // uvCountLabel
            // 
            this.uvCountLabel.AutoSize = true;
            this.uvCountLabel.Location = new System.Drawing.Point(153, 61);
            this.uvCountLabel.Name = "uvCountLabel";
            this.uvCountLabel.Size = new System.Drawing.Size(70, 25);
            this.uvCountLabel.Text = "UV channels: ";
            // 
            // uvCountUpDown
            // 
            this.uvCountUpDown.Location = new System.Drawing.Point(235, 59);
            this.uvCountUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.uvCountUpDown.DecimalPlaces = 0;
            this.uvCountUpDown.Minimum = 0x0;
            this.uvCountUpDown.Maximum = 0xF;
            this.uvCountUpDown.Value = 0;
            this.uvCountUpDown.Name = "uvCountUpDown";
            this.uvCountUpDown.Size = new System.Drawing.Size(40, 17);
            this.uvCountUpDown.TabIndex = 3;
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(100, 86);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(100, 23);
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.TabIndex = 4;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // PolygonFormatEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 120);
            this.Controls.Add(this.weightTypeLabel);
            this.Controls.Add(this.normalTypeLabel);
            this.Controls.Add(this.weightTypeComboBox);
            this.Controls.Add(this.normalTypeComboBox);
            this.Controls.Add(this.vertexColorCB);
            this.Controls.Add(this.uvCountLabel);
            this.Controls.Add(this.uvCountUpDown);
            this.Controls.Add(this.applyButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PolygonFormatEditor";
            this.Text = "Polygon Format Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label weightTypeLabel;
        private System.Windows.Forms.Label normalTypeLabel;
        private System.Windows.Forms.ComboBox weightTypeComboBox;
        private System.Windows.Forms.ComboBox normalTypeComboBox;
        private System.Windows.Forms.CheckBox vertexColorCB;
        private System.Windows.Forms.Label uvCountLabel;
        private System.Windows.Forms.NumericUpDown uvCountUpDown;
        private System.Windows.Forms.Button applyButton;
    }
}