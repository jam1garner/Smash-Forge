namespace Smash_Forge
{
    partial class BfresMaterialEditor
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
            this.MaterialsTab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.ShaderMdllabel2 = new System.Windows.Forms.Label();
            this.ShaderArchivelabel1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridView4 = new System.Windows.Forms.DataGridView();
            this.Column8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabTextureMaps = new System.Windows.Forms.TabPage();
            this.TextureRefListView = new System.Windows.Forms.ListView();
            this.Texture = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MaterialParamsTab = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.colorboxLabel = new System.Windows.Forms.Label();
            this.paramColorBox = new System.Windows.Forms.PictureBox();
            this.boolLabel = new System.Windows.Forms.Label();
            this.FloatLabel8 = new System.Windows.Forms.Label();
            this.FloatLabel6 = new System.Windows.Forms.Label();
            this.FloatLabel4 = new System.Windows.Forms.Label();
            this.FloatLabel2 = new System.Windows.Forms.Label();
            this.FloatLabel7 = new System.Windows.Forms.Label();
            this.FloatLabel5 = new System.Windows.Forms.Label();
            this.FloatLabel3 = new System.Windows.Forms.Label();
            this.FloatLabel1 = new System.Windows.Forms.Label();
            this.FloatNumUD2 = new System.Windows.Forms.NumericUpDown();
            this.FloatNumUD8 = new System.Windows.Forms.NumericUpDown();
            this.FloatNumUD6 = new System.Windows.Forms.NumericUpDown();
            this.FloatNumUD4 = new System.Windows.Forms.NumericUpDown();
            this.FloatNumUD7 = new System.Windows.Forms.NumericUpDown();
            this.FloatNumUD5 = new System.Windows.Forms.NumericUpDown();
            this.FloatNumUD3 = new System.Windows.Forms.NumericUpDown();
            this.boolParam = new System.Windows.Forms.ComboBox();
            this.FloatNumUD = new System.Windows.Forms.NumericUpDown();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dataGridView3 = new System.Windows.Forms.DataGridView();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaterialsTab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView4)).BeginInit();
            this.tabTextureMaps.SuspendLayout();
            this.MaterialParamsTab.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.paramColorBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView3)).BeginInit();
            this.SuspendLayout();
            // 
            // MaterialsTab
            // 
            this.MaterialsTab.Controls.Add(this.tabPage1);
            this.MaterialsTab.Controls.Add(this.tabPage2);
            this.MaterialsTab.Controls.Add(this.tabTextureMaps);
            this.MaterialsTab.Controls.Add(this.MaterialParamsTab);
            this.MaterialsTab.Controls.Add(this.tabPage3);
            this.MaterialsTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MaterialsTab.Location = new System.Drawing.Point(0, 0);
            this.MaterialsTab.Name = "MaterialsTab";
            this.MaterialsTab.SelectedIndex = 0;
            this.MaterialsTab.Size = new System.Drawing.Size(382, 567);
            this.MaterialsTab.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.ShaderMdllabel2);
            this.tabPage1.Controls.Add(this.ShaderArchivelabel1);
            this.tabPage1.Controls.Add(this.textBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(374, 541);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Material Info";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // ShaderMdllabel2
            // 
            this.ShaderMdllabel2.AutoSize = true;
            this.ShaderMdllabel2.Location = new System.Drawing.Point(4, 56);
            this.ShaderMdllabel2.Name = "ShaderMdllabel2";
            this.ShaderMdllabel2.Size = new System.Drawing.Size(73, 13);
            this.ShaderMdllabel2.TabIndex = 2;
            this.ShaderMdllabel2.Text = "Shader Model";
            // 
            // ShaderArchivelabel1
            // 
            this.ShaderArchivelabel1.AutoSize = true;
            this.ShaderArchivelabel1.Location = new System.Drawing.Point(4, 33);
            this.ShaderArchivelabel1.Name = "ShaderArchivelabel1";
            this.ShaderArchivelabel1.Size = new System.Drawing.Size(80, 13);
            this.ShaderArchivelabel1.TabIndex = 1;
            this.ShaderArchivelabel1.Text = "Shader Archive";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(3, 6);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(363, 20);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridView4);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(374, 541);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Render Info";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridView4
            // 
            this.dataGridView4.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView4.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column8,
            this.Column9,
            this.Column10});
            this.dataGridView4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView4.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView4.Location = new System.Drawing.Point(3, 3);
            this.dataGridView4.Name = "dataGridView4";
            this.dataGridView4.Size = new System.Drawing.Size(368, 535);
            this.dataGridView4.TabIndex = 0;
            // 
            // Column8
            // 
            this.Column8.HeaderText = "Name";
            this.Column8.Name = "Column8";
            // 
            // Column9
            // 
            this.Column9.HeaderText = "Value";
            this.Column9.Name = "Column9";
            // 
            // Column10
            // 
            this.Column10.HeaderText = "Type";
            this.Column10.Name = "Column10";
            // 
            // tabTextureMaps
            // 
            this.tabTextureMaps.AutoScroll = true;
            this.tabTextureMaps.Controls.Add(this.TextureRefListView);
            this.tabTextureMaps.Location = new System.Drawing.Point(4, 22);
            this.tabTextureMaps.Name = "tabTextureMaps";
            this.tabTextureMaps.Padding = new System.Windows.Forms.Padding(3);
            this.tabTextureMaps.Size = new System.Drawing.Size(374, 541);
            this.tabTextureMaps.TabIndex = 2;
            this.tabTextureMaps.Text = "Texture Maps";
            this.tabTextureMaps.UseVisualStyleBackColor = true;
            this.tabTextureMaps.Click += new System.EventHandler(this.tabTextureMaps_Click);
            // 
            // TextureRefListView
            // 
            this.TextureRefListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Texture});
            this.TextureRefListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextureRefListView.Location = new System.Drawing.Point(3, 3);
            this.TextureRefListView.Name = "TextureRefListView";
            this.TextureRefListView.Size = new System.Drawing.Size(368, 535);
            this.TextureRefListView.TabIndex = 0;
            this.TextureRefListView.UseCompatibleStateImageBehavior = false;
            this.TextureRefListView.View = System.Windows.Forms.View.Details;
            this.TextureRefListView.SelectedIndexChanged += new System.EventHandler(this.TextureRefListView_SelectedIndexChanged);
            // 
            // Texture
            // 
            this.Texture.Text = "Texture";
            this.Texture.Width = 126;
            // 
            // MaterialParamsTab
            // 
            this.MaterialParamsTab.Controls.Add(this.panel1);
            this.MaterialParamsTab.Controls.Add(this.listView1);
            this.MaterialParamsTab.Location = new System.Drawing.Point(4, 22);
            this.MaterialParamsTab.Name = "MaterialParamsTab";
            this.MaterialParamsTab.Padding = new System.Windows.Forms.Padding(3);
            this.MaterialParamsTab.Size = new System.Drawing.Size(374, 541);
            this.MaterialParamsTab.TabIndex = 3;
            this.MaterialParamsTab.Text = "Material Params";
            this.MaterialParamsTab.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.colorboxLabel);
            this.panel1.Controls.Add(this.paramColorBox);
            this.panel1.Controls.Add(this.boolLabel);
            this.panel1.Controls.Add(this.FloatLabel8);
            this.panel1.Controls.Add(this.FloatLabel6);
            this.panel1.Controls.Add(this.FloatLabel4);
            this.panel1.Controls.Add(this.FloatLabel2);
            this.panel1.Controls.Add(this.FloatLabel7);
            this.panel1.Controls.Add(this.FloatLabel5);
            this.panel1.Controls.Add(this.FloatLabel3);
            this.panel1.Controls.Add(this.FloatLabel1);
            this.panel1.Controls.Add(this.FloatNumUD2);
            this.panel1.Controls.Add(this.FloatNumUD8);
            this.panel1.Controls.Add(this.FloatNumUD6);
            this.panel1.Controls.Add(this.FloatNumUD4);
            this.panel1.Controls.Add(this.FloatNumUD7);
            this.panel1.Controls.Add(this.FloatNumUD5);
            this.panel1.Controls.Add(this.FloatNumUD3);
            this.panel1.Controls.Add(this.boolParam);
            this.panel1.Controls.Add(this.FloatNumUD);
            this.panel1.Location = new System.Drawing.Point(6, 333);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(362, 205);
            this.panel1.TabIndex = 1;
            // 
            // colorboxLabel
            // 
            this.colorboxLabel.AutoSize = true;
            this.colorboxLabel.Location = new System.Drawing.Point(272, 114);
            this.colorboxLabel.Name = "colorboxLabel";
            this.colorboxLabel.Size = new System.Drawing.Size(31, 13);
            this.colorboxLabel.TabIndex = 27;
            this.colorboxLabel.Text = "Color";
            // 
            // paramColorBox
            // 
            this.paramColorBox.Location = new System.Drawing.Point(275, 131);
            this.paramColorBox.Name = "paramColorBox";
            this.paramColorBox.Size = new System.Drawing.Size(45, 45);
            this.paramColorBox.TabIndex = 26;
            this.paramColorBox.TabStop = false;
            this.paramColorBox.Click += new System.EventHandler(this.paramColorBox_Click);
            // 
            // boolLabel
            // 
            this.boolLabel.AutoSize = true;
            this.boolLabel.Location = new System.Drawing.Point(272, 6);
            this.boolLabel.Name = "boolLabel";
            this.boolLabel.Size = new System.Drawing.Size(35, 13);
            this.boolLabel.TabIndex = 25;
            this.boolLabel.Text = "label9";
            // 
            // FloatLabel8
            // 
            this.FloatLabel8.AutoSize = true;
            this.FloatLabel8.Location = new System.Drawing.Point(144, 140);
            this.FloatLabel8.Name = "FloatLabel8";
            this.FloatLabel8.Size = new System.Drawing.Size(35, 13);
            this.FloatLabel8.TabIndex = 24;
            this.FloatLabel8.Text = "label8";
            // 
            // FloatLabel6
            // 
            this.FloatLabel6.AutoSize = true;
            this.FloatLabel6.Location = new System.Drawing.Point(144, 98);
            this.FloatLabel6.Name = "FloatLabel6";
            this.FloatLabel6.Size = new System.Drawing.Size(35, 13);
            this.FloatLabel6.TabIndex = 23;
            this.FloatLabel6.Text = "label7";
            // 
            // FloatLabel4
            // 
            this.FloatLabel4.AutoSize = true;
            this.FloatLabel4.Location = new System.Drawing.Point(144, 50);
            this.FloatLabel4.Name = "FloatLabel4";
            this.FloatLabel4.Size = new System.Drawing.Size(35, 13);
            this.FloatLabel4.TabIndex = 22;
            this.FloatLabel4.Text = "label6";
            // 
            // FloatLabel2
            // 
            this.FloatLabel2.AutoSize = true;
            this.FloatLabel2.Location = new System.Drawing.Point(144, 7);
            this.FloatLabel2.Name = "FloatLabel2";
            this.FloatLabel2.Size = new System.Drawing.Size(35, 13);
            this.FloatLabel2.TabIndex = 21;
            this.FloatLabel2.Text = "label5";
            // 
            // FloatLabel7
            // 
            this.FloatLabel7.AutoSize = true;
            this.FloatLabel7.Location = new System.Drawing.Point(11, 140);
            this.FloatLabel7.Name = "FloatLabel7";
            this.FloatLabel7.Size = new System.Drawing.Size(35, 13);
            this.FloatLabel7.TabIndex = 20;
            this.FloatLabel7.Text = "label4";
            // 
            // FloatLabel5
            // 
            this.FloatLabel5.AutoSize = true;
            this.FloatLabel5.Location = new System.Drawing.Point(11, 98);
            this.FloatLabel5.Name = "FloatLabel5";
            this.FloatLabel5.Size = new System.Drawing.Size(35, 13);
            this.FloatLabel5.TabIndex = 19;
            this.FloatLabel5.Text = "label3";
            // 
            // FloatLabel3
            // 
            this.FloatLabel3.AutoSize = true;
            this.FloatLabel3.Location = new System.Drawing.Point(11, 50);
            this.FloatLabel3.Name = "FloatLabel3";
            this.FloatLabel3.Size = new System.Drawing.Size(35, 13);
            this.FloatLabel3.TabIndex = 18;
            this.FloatLabel3.Text = "label2";
            // 
            // FloatLabel1
            // 
            this.FloatLabel1.AutoSize = true;
            this.FloatLabel1.Location = new System.Drawing.Point(11, 7);
            this.FloatLabel1.Name = "FloatLabel1";
            this.FloatLabel1.Size = new System.Drawing.Size(35, 13);
            this.FloatLabel1.TabIndex = 17;
            this.FloatLabel1.Text = "label1";
            // 
            // FloatNumUD2
            // 
            this.FloatNumUD2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FloatNumUD2.DecimalPlaces = 3;
            this.FloatNumUD2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.FloatNumUD2.Location = new System.Drawing.Point(147, 25);
            this.FloatNumUD2.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.FloatNumUD2.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.FloatNumUD2.Name = "FloatNumUD2";
            this.FloatNumUD2.Size = new System.Drawing.Size(113, 20);
            this.FloatNumUD2.TabIndex = 16;
            this.FloatNumUD2.ValueChanged += new System.EventHandler(this.FloatNumUD_ValueChanged);
            // 
            // FloatNumUD8
            // 
            this.FloatNumUD8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FloatNumUD8.DecimalPlaces = 3;
            this.FloatNumUD8.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.FloatNumUD8.Location = new System.Drawing.Point(147, 158);
            this.FloatNumUD8.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.FloatNumUD8.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.FloatNumUD8.Name = "FloatNumUD8";
            this.FloatNumUD8.Size = new System.Drawing.Size(113, 20);
            this.FloatNumUD8.TabIndex = 15;
            this.FloatNumUD8.ValueChanged += new System.EventHandler(this.FloatNumUD_ValueChanged);
            // 
            // FloatNumUD6
            // 
            this.FloatNumUD6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FloatNumUD6.DecimalPlaces = 3;
            this.FloatNumUD6.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.FloatNumUD6.Location = new System.Drawing.Point(147, 116);
            this.FloatNumUD6.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.FloatNumUD6.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.FloatNumUD6.Name = "FloatNumUD6";
            this.FloatNumUD6.Size = new System.Drawing.Size(113, 20);
            this.FloatNumUD6.TabIndex = 14;
            this.FloatNumUD6.ValueChanged += new System.EventHandler(this.FloatNumUD_ValueChanged);
            // 
            // FloatNumUD4
            // 
            this.FloatNumUD4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FloatNumUD4.DecimalPlaces = 3;
            this.FloatNumUD4.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.FloatNumUD4.Location = new System.Drawing.Point(147, 68);
            this.FloatNumUD4.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.FloatNumUD4.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.FloatNumUD4.Name = "FloatNumUD4";
            this.FloatNumUD4.Size = new System.Drawing.Size(113, 20);
            this.FloatNumUD4.TabIndex = 13;
            this.FloatNumUD4.ValueChanged += new System.EventHandler(this.FloatNumUD_ValueChanged);
            // 
            // FloatNumUD7
            // 
            this.FloatNumUD7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FloatNumUD7.DecimalPlaces = 3;
            this.FloatNumUD7.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.FloatNumUD7.Location = new System.Drawing.Point(14, 158);
            this.FloatNumUD7.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.FloatNumUD7.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.FloatNumUD7.Name = "FloatNumUD7";
            this.FloatNumUD7.Size = new System.Drawing.Size(113, 20);
            this.FloatNumUD7.TabIndex = 12;
            this.FloatNumUD7.ValueChanged += new System.EventHandler(this.FloatNumUD_ValueChanged);
            // 
            // FloatNumUD5
            // 
            this.FloatNumUD5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FloatNumUD5.DecimalPlaces = 3;
            this.FloatNumUD5.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.FloatNumUD5.Location = new System.Drawing.Point(14, 116);
            this.FloatNumUD5.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.FloatNumUD5.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.FloatNumUD5.Name = "FloatNumUD5";
            this.FloatNumUD5.Size = new System.Drawing.Size(113, 20);
            this.FloatNumUD5.TabIndex = 11;
            this.FloatNumUD5.ValueChanged += new System.EventHandler(this.FloatNumUD_ValueChanged);
            // 
            // FloatNumUD3
            // 
            this.FloatNumUD3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FloatNumUD3.DecimalPlaces = 3;
            this.FloatNumUD3.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.FloatNumUD3.Location = new System.Drawing.Point(14, 68);
            this.FloatNumUD3.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.FloatNumUD3.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.FloatNumUD3.Name = "FloatNumUD3";
            this.FloatNumUD3.Size = new System.Drawing.Size(113, 20);
            this.FloatNumUD3.TabIndex = 10;
            this.FloatNumUD3.ValueChanged += new System.EventHandler(this.FloatNumUD_ValueChanged);
            // 
            // boolParam
            // 
            this.boolParam.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boolParam.FormattingEnabled = true;
            this.boolParam.Location = new System.Drawing.Point(275, 22);
            this.boolParam.Name = "boolParam";
            this.boolParam.Size = new System.Drawing.Size(84, 21);
            this.boolParam.TabIndex = 9;
            // 
            // FloatNumUD
            // 
            this.FloatNumUD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FloatNumUD.DecimalPlaces = 3;
            this.FloatNumUD.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.FloatNumUD.Location = new System.Drawing.Point(14, 25);
            this.FloatNumUD.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.FloatNumUD.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.FloatNumUD.Name = "FloatNumUD";
            this.FloatNumUD.Size = new System.Drawing.Size(113, 20);
            this.FloatNumUD.TabIndex = 8;
            this.FloatNumUD.ValueChanged += new System.EventHandler(this.FloatNumUD_ValueChanged);
            // 
            // listView1
            // 
            this.listView1.AllowColumnReorder = true;
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader6,
            this.columnHeader1});
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(6, 6);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(365, 321);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Name";
            this.columnHeader4.Width = 140;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Value";
            this.columnHeader6.Width = 90;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Color";
            this.columnHeader1.Width = 40;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.dataGridView3);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(374, 541);
            this.tabPage3.TabIndex = 4;
            this.tabPage3.Text = "Shader Options";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dataGridView3
            // 
            this.dataGridView3.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView3.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column6,
            this.Column7});
            this.dataGridView3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView3.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView3.Location = new System.Drawing.Point(3, 3);
            this.dataGridView3.Name = "dataGridView3";
            this.dataGridView3.Size = new System.Drawing.Size(368, 535);
            this.dataGridView3.TabIndex = 0;
            this.dataGridView3.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView3_CellContentClick);
            // 
            // Column6
            // 
            this.Column6.HeaderText = "Name";
            this.Column6.Name = "Column6";
            this.Column6.Width = 222;
            // 
            // Column7
            // 
            this.Column7.HeaderText = "Value";
            this.Column7.Name = "Column7";
            // 
            // BfresMaterialEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 567);
            this.Controls.Add(this.MaterialsTab);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "BfresMaterialEditor";
            this.Text = "Material Editor";
            this.Load += new System.EventHandler(this.BFRES_MaterialEditor_Load);
            this.MaterialsTab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView4)).EndInit();
            this.tabTextureMaps.ResumeLayout(false);
            this.MaterialParamsTab.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.paramColorBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloatNumUD)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl MaterialsTab;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabTextureMaps;
        private System.Windows.Forms.TabPage MaterialParamsTab;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dataGridView3;
        private System.Windows.Forms.DataGridView dataGridView4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column10;
        private System.Windows.Forms.NumericUpDown FloatNumUD;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ComboBox boolParam;
        private System.Windows.Forms.NumericUpDown FloatNumUD2;
        private System.Windows.Forms.NumericUpDown FloatNumUD8;
        private System.Windows.Forms.NumericUpDown FloatNumUD4;
        private System.Windows.Forms.NumericUpDown FloatNumUD7;
        private System.Windows.Forms.NumericUpDown FloatNumUD5;
        private System.Windows.Forms.NumericUpDown FloatNumUD3;
        private System.Windows.Forms.Label FloatLabel8;
        private System.Windows.Forms.Label FloatLabel6;
        private System.Windows.Forms.Label FloatLabel4;
        private System.Windows.Forms.Label FloatLabel2;
        private System.Windows.Forms.Label FloatLabel7;
        private System.Windows.Forms.Label FloatLabel5;
        private System.Windows.Forms.Label FloatLabel3;
        private System.Windows.Forms.Label FloatLabel1;
        private System.Windows.Forms.Label boolLabel;
        private System.Windows.Forms.Label colorboxLabel;
        private System.Windows.Forms.PictureBox paramColorBox;
        private System.Windows.Forms.NumericUpDown FloatNumUD6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.Label ShaderMdllabel2;
        private System.Windows.Forms.Label ShaderArchivelabel1;
        private System.Windows.Forms.ListView TextureRefListView;
        private System.Windows.Forms.ColumnHeader Texture;
    }
}