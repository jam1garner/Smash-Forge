namespace Smash_Forge.GUI
{
    partial class RenderSettings
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
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.checkBox7 = new System.Windows.Forms.CheckBox();
            this.checkBox8 = new System.Windows.Forms.CheckBox();
            this.checkBox9 = new System.Windows.Forms.CheckBox();
            this.checkBox10 = new System.Windows.Forms.CheckBox();
            this.checkBox12 = new System.Windows.Forms.CheckBox();
            this.checkBox15 = new System.Windows.Forms.CheckBox();
            this.checkBox14 = new System.Windows.Forms.CheckBox();
            this.checkBox13 = new System.Windows.Forms.CheckBox();
            this.wireframeCB = new System.Windows.Forms.CheckBox();
            this.modelSelectCB = new System.Windows.Forms.CheckBox();
            this.boundingCB = new System.Windows.Forms.CheckBox();
            this.backgroundCB = new System.Windows.Forms.CheckBox();
            this.swagViewing = new System.Windows.Forms.CheckBox();
            this.depthSlider = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cameraLightCB = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.difTB = new System.Windows.Forms.TextBox();
            this.spcTB = new System.Windows.Forms.TextBox();
            this.frsTB = new System.Windows.Forms.TextBox();
            this.ambTB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.refTB = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.fovSlider = new System.Windows.Forms.TrackBar();
            this.useNormCB = new System.Windows.Forms.CheckBox();
            this.reflectionCB = new System.Windows.Forms.CheckBox();
            this.specularCB = new System.Windows.Forms.CheckBox();
            this.fresnelCB = new System.Windows.Forms.CheckBox();
            this.diffuseCB = new System.Windows.Forms.CheckBox();
            this.lightCheckBox = new System.Windows.Forms.CheckBox();
            this.cb_normals = new System.Windows.Forms.CheckBox();
            this.renderMode = new System.Windows.Forms.ComboBox();
            this.cb_vertcolor = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.modelscaleTB = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.checkBox11 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.depthSlider)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fovSlider)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(16, 22);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(93, 17);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "Render Model";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(16, 91);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(94, 17);
            this.checkBox2.TabIndex = 1;
            this.checkBox2.Text = "Render Bones";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Location = new System.Drawing.Point(16, 226);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(125, 17);
            this.checkBox3.TabIndex = 2;
            this.checkBox3.Text = "Render Camera Path";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Checked = true;
            this.checkBox4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox4.Location = new System.Drawing.Point(16, 135);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(105, 17);
            this.checkBox4.TabIndex = 3;
            this.checkBox4.Text = "Render Hitboxes";
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Checked = true;
            this.checkBox5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox5.Location = new System.Drawing.Point(16, 249);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(87, 17);
            this.checkBox5.TabIndex = 4;
            this.checkBox5.Text = "Render Floor";
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.Checked = true;
            this.checkBox6.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox6.Location = new System.Drawing.Point(16, 322);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(85, 17);
            this.checkBox6.TabIndex = 5;
            this.checkBox6.Text = "Render LVD";
            this.checkBox6.UseVisualStyleBackColor = true;
            this.checkBox6.CheckedChanged += new System.EventHandler(this.checkBox6_CheckedChanged);
            // 
            // checkBox7
            // 
            this.checkBox7.AutoSize = true;
            this.checkBox7.Checked = true;
            this.checkBox7.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox7.Location = new System.Drawing.Point(48, 345);
            this.checkBox7.Name = "checkBox7";
            this.checkBox7.Size = new System.Drawing.Size(107, 17);
            this.checkBox7.TabIndex = 6;
            this.checkBox7.Text = "Render Collisions";
            this.checkBox7.UseVisualStyleBackColor = true;
            this.checkBox7.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox8
            // 
            this.checkBox8.AutoSize = true;
            this.checkBox8.Checked = true;
            this.checkBox8.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox8.Location = new System.Drawing.Point(48, 391);
            this.checkBox8.Name = "checkBox8";
            this.checkBox8.Size = new System.Drawing.Size(102, 17);
            this.checkBox8.TabIndex = 7;
            this.checkBox8.Text = "Render Spawns";
            this.checkBox8.UseVisualStyleBackColor = true;
            this.checkBox8.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox9
            // 
            this.checkBox9.AutoSize = true;
            this.checkBox9.Checked = true;
            this.checkBox9.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox9.Location = new System.Drawing.Point(48, 414);
            this.checkBox9.Name = "checkBox9";
            this.checkBox9.Size = new System.Drawing.Size(114, 17);
            this.checkBox9.TabIndex = 8;
            this.checkBox9.Text = "Render Respawns";
            this.checkBox9.UseVisualStyleBackColor = true;
            this.checkBox9.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox10
            // 
            this.checkBox10.AutoSize = true;
            this.checkBox10.Checked = true;
            this.checkBox10.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox10.Location = new System.Drawing.Point(48, 437);
            this.checkBox10.Name = "checkBox10";
            this.checkBox10.Size = new System.Drawing.Size(134, 17);
            this.checkBox10.TabIndex = 9;
            this.checkBox10.Text = "Render Item Spawners";
            this.checkBox10.UseVisualStyleBackColor = true;
            this.checkBox10.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox12
            // 
            this.checkBox12.AutoSize = true;
            this.checkBox12.Location = new System.Drawing.Point(77, 368);
            this.checkBox12.Name = "checkBox12";
            this.checkBox12.Size = new System.Drawing.Size(158, 17);
            this.checkBox12.TabIndex = 11;
            this.checkBox12.Text = "Render Passthrough Angles";
            this.checkBox12.UseVisualStyleBackColor = true;
            this.checkBox12.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox15
            // 
            this.checkBox15.AutoSize = true;
            this.checkBox15.Location = new System.Drawing.Point(16, 203);
            this.checkBox15.Name = "checkBox15";
            this.checkBox15.Size = new System.Drawing.Size(196, 17);
            this.checkBox15.TabIndex = 25;
            this.checkBox15.Text = "Render Environment Collision Boxes";
            this.checkBox15.UseVisualStyleBackColor = true;
            this.checkBox15.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox14
            // 
            this.checkBox14.AutoSize = true;
            this.checkBox14.Checked = true;
            this.checkBox14.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox14.Location = new System.Drawing.Point(48, 179);
            this.checkBox14.Name = "checkBox14";
            this.checkBox14.Size = new System.Drawing.Size(145, 17);
            this.checkBox14.TabIndex = 24;
            this.checkBox14.Text = "Render Hurtboxes Zones";
            this.checkBox14.UseVisualStyleBackColor = true;
            this.checkBox14.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // checkBox13
            // 
            this.checkBox13.AutoSize = true;
            this.checkBox13.Checked = true;
            this.checkBox13.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox13.Location = new System.Drawing.Point(16, 158);
            this.checkBox13.Name = "checkBox13";
            this.checkBox13.Size = new System.Drawing.Size(112, 17);
            this.checkBox13.TabIndex = 23;
            this.checkBox13.Text = "Render Hurtboxes";
            this.checkBox13.UseVisualStyleBackColor = true;
            this.checkBox13.CheckedChanged += new System.EventHandler(this.checkChanged);
            // 
            // wireframeCB
            // 
            this.wireframeCB.AutoSize = true;
            this.wireframeCB.Location = new System.Drawing.Point(48, 68);
            this.wireframeCB.Name = "wireframeCB";
            this.wireframeCB.Size = new System.Drawing.Size(104, 17);
            this.wireframeCB.TabIndex = 22;
            this.wireframeCB.Text = "Show Wireframe";
            this.wireframeCB.UseVisualStyleBackColor = true;
            this.wireframeCB.CheckedChanged += new System.EventHandler(this.wireframeCB_CheckedChanged);
            // 
            // modelSelectCB
            // 
            this.modelSelectCB.AutoSize = true;
            this.modelSelectCB.Location = new System.Drawing.Point(48, 44);
            this.modelSelectCB.Name = "modelSelectCB";
            this.modelSelectCB.Size = new System.Drawing.Size(132, 17);
            this.modelSelectCB.TabIndex = 21;
            this.modelSelectCB.Text = "Show Model Selection";
            this.modelSelectCB.UseVisualStyleBackColor = true;
            this.modelSelectCB.CheckedChanged += new System.EventHandler(this.modelSelectCB_CheckedChanged);
            // 
            // boundingCB
            // 
            this.boundingCB.AutoSize = true;
            this.boundingCB.Location = new System.Drawing.Point(16, 296);
            this.boundingCB.Name = "boundingCB";
            this.boundingCB.Size = new System.Drawing.Size(141, 17);
            this.boundingCB.TabIndex = 20;
            this.boundingCB.Text = "Render Bounding Boxes";
            this.boundingCB.UseVisualStyleBackColor = true;
            this.boundingCB.CheckedChanged += new System.EventHandler(this.boundingCB_CheckedChanged);
            // 
            // backgroundCB
            // 
            this.backgroundCB.AutoSize = true;
            this.backgroundCB.Location = new System.Drawing.Point(16, 272);
            this.backgroundCB.Name = "backgroundCB";
            this.backgroundCB.Size = new System.Drawing.Size(122, 17);
            this.backgroundCB.TabIndex = 19;
            this.backgroundCB.Text = "Render Background";
            this.backgroundCB.UseVisualStyleBackColor = true;
            this.backgroundCB.CheckedChanged += new System.EventHandler(this.backgroundCB_CheckedChanged);
            // 
            // swagViewing
            // 
            this.swagViewing.AutoSize = true;
            this.swagViewing.Location = new System.Drawing.Point(48, 112);
            this.swagViewing.Name = "swagViewing";
            this.swagViewing.Size = new System.Drawing.Size(109, 17);
            this.swagViewing.TabIndex = 18;
            this.swagViewing.Text = "Show Swag Data";
            this.swagViewing.UseVisualStyleBackColor = true;
            this.swagViewing.CheckedChanged += new System.EventHandler(this.swagViewing_CheckedChanged);
            // 
            // depthSlider
            // 
            this.depthSlider.Location = new System.Drawing.Point(12, 32);
            this.depthSlider.Maximum = 60000;
            this.depthSlider.Minimum = 1;
            this.depthSlider.Name = "depthSlider";
            this.depthSlider.Size = new System.Drawing.Size(265, 45);
            this.depthSlider.TabIndex = 13;
            this.depthSlider.Value = 1;
            this.depthSlider.ValueChanged += new System.EventHandler(this.depthSlider_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(127, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Depth";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cameraLightCB);
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.fovSlider);
            this.groupBox1.Controls.Add(this.useNormCB);
            this.groupBox1.Controls.Add(this.reflectionCB);
            this.groupBox1.Controls.Add(this.specularCB);
            this.groupBox1.Controls.Add(this.fresnelCB);
            this.groupBox1.Controls.Add(this.diffuseCB);
            this.groupBox1.Controls.Add(this.lightCheckBox);
            this.groupBox1.Controls.Add(this.cb_normals);
            this.groupBox1.Controls.Add(this.renderMode);
            this.groupBox1.Controls.Add(this.cb_vertcolor);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.depthSlider);
            this.groupBox1.Location = new System.Drawing.Point(301, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(283, 500);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Model Render Settings";
            // 
            // cameraLightCB
            // 
            this.cameraLightCB.AutoSize = true;
            this.cameraLightCB.Location = new System.Drawing.Point(181, 300);
            this.cameraLightCB.Name = "cameraLightCB";
            this.cameraLightCB.Size = new System.Drawing.Size(88, 17);
            this.cameraLightCB.TabIndex = 32;
            this.cameraLightCB.Text = "Camera Light";
            this.cameraLightCB.UseVisualStyleBackColor = true;
            this.cameraLightCB.CheckedChanged += new System.EventHandler(this.cameraLightCB_CheckedChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.modelscaleTB, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.difTB, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.spcTB, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.frsTB, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.ambTB, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.refTB, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 335);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(277, 162);
            this.tableLayoutPanel1.TabIndex = 31;
            // 
            // difTB
            // 
            this.difTB.Location = new System.Drawing.Point(141, 3);
            this.difTB.Name = "difTB";
            this.difTB.Size = new System.Drawing.Size(100, 20);
            this.difTB.TabIndex = 30;
            this.difTB.TextChanged += new System.EventHandler(this.difTB_TextChanged);
            // 
            // spcTB
            // 
            this.spcTB.Location = new System.Drawing.Point(141, 29);
            this.spcTB.Name = "spcTB";
            this.spcTB.Size = new System.Drawing.Size(100, 20);
            this.spcTB.TabIndex = 31;
            this.spcTB.TextChanged += new System.EventHandler(this.spcTB_TextChanged);
            // 
            // frsTB
            // 
            this.frsTB.Location = new System.Drawing.Point(141, 55);
            this.frsTB.Name = "frsTB";
            this.frsTB.Size = new System.Drawing.Size(100, 20);
            this.frsTB.TabIndex = 32;
            this.frsTB.TextChanged += new System.EventHandler(this.frsTB_TextChanged);
            // 
            // ambTB
            // 
            this.ambTB.Location = new System.Drawing.Point(141, 81);
            this.ambTB.Name = "ambTB";
            this.ambTB.Size = new System.Drawing.Size(100, 20);
            this.ambTB.TabIndex = 33;
            this.ambTB.TextChanged += new System.EventHandler(this.ambTB_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 34;
            this.label2.Text = "Diffuse Intensity";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 26);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 35;
            this.label5.Text = "Specular Intensity";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 52);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 13);
            this.label6.TabIndex = 36;
            this.label6.Text = "Fresnel Intensity";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 78);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 13);
            this.label7.TabIndex = 37;
            this.label7.Text = "Ambient Intensity";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 104);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(97, 13);
            this.label8.TabIndex = 38;
            this.label8.Text = "Reflection Intensity";
            // 
            // refTB
            // 
            this.refTB.Location = new System.Drawing.Point(141, 107);
            this.refTB.Name = "refTB";
            this.refTB.Size = new System.Drawing.Size(100, 20);
            this.refTB.TabIndex = 39;
            this.refTB.TextChanged += new System.EventHandler(this.refTB_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(117, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 29;
            this.label4.Text = "FOV Slider";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 28;
            this.label3.Text = "Render Mode";
            // 
            // fovSlider
            // 
            this.fovSlider.Location = new System.Drawing.Point(12, 83);
            this.fovSlider.Maximum = 30;
            this.fovSlider.Minimum = 1;
            this.fovSlider.Name = "fovSlider";
            this.fovSlider.Size = new System.Drawing.Size(265, 45);
            this.fovSlider.TabIndex = 27;
            this.fovSlider.Value = 1;
            this.fovSlider.Scroll += new System.EventHandler(this.fovSlider_Scroll);
            // 
            // useNormCB
            // 
            this.useNormCB.AutoSize = true;
            this.useNormCB.Location = new System.Drawing.Point(12, 300);
            this.useNormCB.Name = "useNormCB";
            this.useNormCB.Size = new System.Drawing.Size(110, 17);
            this.useNormCB.TabIndex = 26;
            this.useNormCB.Text = "Use Normal Maps";
            this.useNormCB.UseVisualStyleBackColor = true;
            this.useNormCB.CheckedChanged += new System.EventHandler(this.useNormCB_CheckedChanged);
            // 
            // reflectionCB
            // 
            this.reflectionCB.AutoSize = true;
            this.reflectionCB.Enabled = false;
            this.reflectionCB.Location = new System.Drawing.Point(38, 277);
            this.reflectionCB.Name = "reflectionCB";
            this.reflectionCB.Size = new System.Drawing.Size(74, 17);
            this.reflectionCB.TabIndex = 23;
            this.reflectionCB.Text = "Reflection";
            this.reflectionCB.UseVisualStyleBackColor = true;
            this.reflectionCB.CheckedChanged += new System.EventHandler(this.reflectionCB_CheckedChanged);
            // 
            // specularCB
            // 
            this.specularCB.AutoSize = true;
            this.specularCB.Enabled = false;
            this.specularCB.Location = new System.Drawing.Point(38, 254);
            this.specularCB.Name = "specularCB";
            this.specularCB.Size = new System.Drawing.Size(68, 17);
            this.specularCB.TabIndex = 22;
            this.specularCB.Text = "Specular";
            this.specularCB.UseVisualStyleBackColor = true;
            this.specularCB.CheckedChanged += new System.EventHandler(this.specularCB_CheckedChanged);
            // 
            // fresnelCB
            // 
            this.fresnelCB.AutoSize = true;
            this.fresnelCB.Enabled = false;
            this.fresnelCB.Location = new System.Drawing.Point(38, 231);
            this.fresnelCB.Name = "fresnelCB";
            this.fresnelCB.Size = new System.Drawing.Size(60, 17);
            this.fresnelCB.TabIndex = 21;
            this.fresnelCB.Text = "Fresnel";
            this.fresnelCB.UseVisualStyleBackColor = true;
            this.fresnelCB.CheckedChanged += new System.EventHandler(this.fresnelCB_CheckedChanged);
            // 
            // diffuseCB
            // 
            this.diffuseCB.AutoSize = true;
            this.diffuseCB.Enabled = false;
            this.diffuseCB.Location = new System.Drawing.Point(38, 208);
            this.diffuseCB.Name = "diffuseCB";
            this.diffuseCB.Size = new System.Drawing.Size(59, 17);
            this.diffuseCB.TabIndex = 20;
            this.diffuseCB.Text = "Diffuse";
            this.diffuseCB.UseVisualStyleBackColor = true;
            this.diffuseCB.CheckedChanged += new System.EventHandler(this.diffuseCB_CheckedChanged);
            // 
            // lightCheckBox
            // 
            this.lightCheckBox.AutoSize = true;
            this.lightCheckBox.Location = new System.Drawing.Point(12, 185);
            this.lightCheckBox.Name = "lightCheckBox";
            this.lightCheckBox.Size = new System.Drawing.Size(196, 17);
            this.lightCheckBox.TabIndex = 19;
            this.lightCheckBox.Text = "Material Lighting (Experimental WIP)";
            this.lightCheckBox.UseVisualStyleBackColor = true;
            this.lightCheckBox.CheckedChanged += new System.EventHandler(this.lightCheckBox_CheckedChanged);
            // 
            // cb_normals
            // 
            this.cb_normals.AutoSize = true;
            this.cb_normals.Checked = true;
            this.cb_normals.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_normals.Location = new System.Drawing.Point(181, 162);
            this.cb_normals.Name = "cb_normals";
            this.cb_normals.Size = new System.Drawing.Size(94, 17);
            this.cb_normals.TabIndex = 17;
            this.cb_normals.Text = "Show Normals";
            this.cb_normals.UseVisualStyleBackColor = true;
            this.cb_normals.CheckedChanged += new System.EventHandler(this.cb_normals_CheckedChanged);
            // 
            // renderMode
            // 
            this.renderMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.renderMode.FormattingEnabled = true;
            this.renderMode.Items.AddRange(new object[] {
            "Texture",
            "Normals(Color)",
            "Normals(BnW)",
            "Normal Maps",
            "Vertex Color",
            "Ambient Occlusion"});
            this.renderMode.Location = new System.Drawing.Point(87, 134);
            this.renderMode.Name = "renderMode";
            this.renderMode.Size = new System.Drawing.Size(190, 21);
            this.renderMode.TabIndex = 16;
            this.renderMode.SelectedIndexChanged += new System.EventHandler(this.checkBox6_CheckedChanged);
            this.renderMode.SelectionChangeCommitted += new System.EventHandler(this.renderMode_SelectionChangeCommitted);
            // 
            // cb_vertcolor
            // 
            this.cb_vertcolor.AutoSize = true;
            this.cb_vertcolor.Checked = true;
            this.cb_vertcolor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_vertcolor.Location = new System.Drawing.Point(12, 162);
            this.cb_vertcolor.Name = "cb_vertcolor";
            this.cb_vertcolor.Size = new System.Drawing.Size(118, 17);
            this.cb_vertcolor.TabIndex = 15;
            this.cb_vertcolor.Text = "Show Vertex Colors";
            this.cb_vertcolor.UseVisualStyleBackColor = true;
            this.cb_vertcolor.CheckedChanged += new System.EventHandler(this.cb_vertcolor_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox11);
            this.groupBox2.Controls.Add(this.checkBox15);
            this.groupBox2.Controls.Add(this.checkBox1);
            this.groupBox2.Controls.Add(this.checkBox14);
            this.groupBox2.Controls.Add(this.checkBox7);
            this.groupBox2.Controls.Add(this.checkBox13);
            this.groupBox2.Controls.Add(this.checkBox6);
            this.groupBox2.Controls.Add(this.wireframeCB);
            this.groupBox2.Controls.Add(this.checkBox8);
            this.groupBox2.Controls.Add(this.modelSelectCB);
            this.groupBox2.Controls.Add(this.checkBox5);
            this.groupBox2.Controls.Add(this.boundingCB);
            this.groupBox2.Controls.Add(this.swagViewing);
            this.groupBox2.Controls.Add(this.backgroundCB);
            this.groupBox2.Controls.Add(this.checkBox9);
            this.groupBox2.Controls.Add(this.checkBox4);
            this.groupBox2.Controls.Add(this.checkBox12);
            this.groupBox2.Controls.Add(this.checkBox10);
            this.groupBox2.Controls.Add(this.checkBox2);
            this.groupBox2.Controls.Add(this.checkBox3);
            this.groupBox2.Location = new System.Drawing.Point(1, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(300, 497);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Display Settings";
            // 
            // modelscaleTB
            // 
            this.modelscaleTB.Location = new System.Drawing.Point(141, 133);
            this.modelscaleTB.Name = "modelscaleTB";
            this.modelscaleTB.Size = new System.Drawing.Size(100, 20);
            this.modelscaleTB.TabIndex = 33;
            this.modelscaleTB.TextChanged += new System.EventHandler(this.modelscaleTB_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 130);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(66, 13);
            this.label9.TabIndex = 40;
            this.label9.Text = "Model Scale";
            // 
            // checkBox11
            // 
            this.checkBox11.AutoSize = true;
            this.checkBox11.Location = new System.Drawing.Point(48, 461);
            this.checkBox11.Name = "checkBox11";
            this.checkBox11.Size = new System.Drawing.Size(133, 17);
            this.checkBox11.TabIndex = 26;
            this.checkBox11.Text = "Render General Points";
            this.checkBox11.UseVisualStyleBackColor = true;
            // 
            // RenderSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 524);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = global::Smash_Forge.Resources.Resources.forge2;
            this.Name = "RenderSettings";
            this.Text = "Render Settings";
            this.Load += new System.EventHandler(this.RenderSettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.depthSlider)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fovSlider)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.CheckBox checkBox7;
        private System.Windows.Forms.CheckBox checkBox8;
        private System.Windows.Forms.CheckBox checkBox9;
        private System.Windows.Forms.CheckBox checkBox10;
        private System.Windows.Forms.CheckBox checkBox12;
        private System.Windows.Forms.TrackBar depthSlider;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox renderMode;
        private System.Windows.Forms.CheckBox cb_vertcolor;
        private System.Windows.Forms.CheckBox cb_normals;
        private System.Windows.Forms.CheckBox swagViewing;
        private System.Windows.Forms.CheckBox lightCheckBox;
        private System.Windows.Forms.CheckBox reflectionCB;
        private System.Windows.Forms.CheckBox specularCB;
        private System.Windows.Forms.CheckBox fresnelCB;
        private System.Windows.Forms.CheckBox diffuseCB;
        private System.Windows.Forms.CheckBox useNormCB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar fovSlider;
        private System.Windows.Forms.CheckBox backgroundCB;
        private System.Windows.Forms.CheckBox boundingCB;
        private System.Windows.Forms.CheckBox wireframeCB;
        private System.Windows.Forms.CheckBox modelSelectCB;
        private System.Windows.Forms.CheckBox checkBox14;
        private System.Windows.Forms.CheckBox checkBox13;
        private System.Windows.Forms.CheckBox cameraLightCB;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox difTB;
        private System.Windows.Forms.TextBox spcTB;
        private System.Windows.Forms.TextBox frsTB;
        private System.Windows.Forms.TextBox ambTB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox refTB;
        private System.Windows.Forms.CheckBox checkBox15;
        private System.Windows.Forms.TextBox modelscaleTB;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox11;
    }
}