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
            this.applyButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // weightTypeLabel
            // 
            this.weightTypeLabel.AutoSize = true;
            this.weightTypeLabel.Location = new System.Drawing.Point(6, 6);
            this.weightTypeLabel.Name = "weightTypeLabel";
            this.weightTypeLabel.Size = new System.Drawing.Size(141, 25);
            this.weightTypeLabel.TabIndex = 1;
            this.weightTypeLabel.Text = "Weight Type";
            // 
            // normalTypeLabel
            // 
            this.normalTypeLabel.AutoSize = true;
            this.normalTypeLabel.Location = new System.Drawing.Point(153, 6);
            this.normalTypeLabel.Name = "normalTypeLabel";
            this.normalTypeLabel.Size = new System.Drawing.Size(141, 25);
            this.normalTypeLabel.TabIndex = 1;
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
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(100, 76);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(100, 23);
            this.applyButton.TabIndex = 2;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // PolygonFormatEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 110);
            this.Controls.Add(this.weightTypeLabel);
            this.Controls.Add(this.normalTypeLabel);
            this.Controls.Add(this.weightTypeComboBox);
            this.Controls.Add(this.normalTypeComboBox);
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
        private System.Windows.Forms.Button applyButton;
    }
}