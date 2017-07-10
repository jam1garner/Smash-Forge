namespace Smash_Forge
{
    partial class ACMDPreviewEditor
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
            this.cb_section = new System.Windows.Forms.ComboBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.cb_crc = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cb_section
            // 
            this.cb_section.Dock = System.Windows.Forms.DockStyle.Top;
            this.cb_section.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_section.FormattingEnabled = true;
            this.cb_section.Items.AddRange(new object[] {
            "GAME",
            "SOUND",
            "EXPRESSION",
            "EFFECT"});
            this.cb_section.Location = new System.Drawing.Point(0, 21);
            this.cb_section.Name = "cb_section";
            this.cb_section.Size = new System.Drawing.Size(420, 21);
            this.cb_section.TabIndex = 0;
            this.cb_section.SelectedIndexChanged += new System.EventHandler(this.updateSelection);
            this.cb_section.TextUpdate += new System.EventHandler(this.cb_section_TextUpdate);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 42);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(420, 565);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            this.richTextBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.richTextBox1_KeyPress);
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button1.Location = new System.Drawing.Point(0, 584);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(420, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Compile";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cb_crc
            // 
            this.cb_crc.Dock = System.Windows.Forms.DockStyle.Top;
            this.cb_crc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_crc.FormattingEnabled = true;
            this.cb_crc.Location = new System.Drawing.Point(0, 0);
            this.cb_crc.Name = "cb_crc";
            this.cb_crc.Size = new System.Drawing.Size(420, 21);
            this.cb_crc.TabIndex = 3;
            this.cb_crc.SelectedIndexChanged += new System.EventHandler(this.updateCrc);
            // 
            // ACMDPreviewEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 607);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.cb_section);
            this.Controls.Add(this.cb_crc);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ACMDPreviewEditor";
            this.Text = "ACMDPreviewEditor";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cb_section;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox cb_crc;
    }
}