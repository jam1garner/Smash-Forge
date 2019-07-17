namespace SmashForge
{
    partial class NudMaterialEditor
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Diffuse");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("NormalMap");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Ramp");
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("DummyRamp");
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Texture");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NudMaterialEditor));
            this.matsComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.propertiesListView = new System.Windows.Forms.ListView();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.propertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.paramsFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.addDelPropertyTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.deleteMatPropertyButton = new System.Windows.Forms.Button();
            this.colorSelect = new System.Windows.Forms.Button();
            this.addMatPropertyButton = new System.Windows.Forms.Button();
            this.matPropertyComboBox = new System.Windows.Forms.ComboBox();
            this.selectedPropGroupBox = new System.Windows.Forms.GroupBox();
            this.selectedPropFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.propertyNameLabel = new System.Windows.Forms.Label();
            this.paramsLabel = new System.Windows.Forms.Panel();
            this.paramTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.param4TrackBar = new System.Windows.Forms.TrackBar();
            this.param3TrackBar = new System.Windows.Forms.TrackBar();
            this.param2TrackBar = new System.Windows.Forms.TrackBar();
            this.param4TB = new System.Windows.Forms.TextBox();
            this.param3TB = new System.Windows.Forms.TextBox();
            this.param2TB = new System.Windows.Forms.TextBox();
            this.param1TB = new System.Windows.Forms.TextBox();
            this.param4Label = new System.Windows.Forms.Label();
            this.param1Label = new System.Windows.Forms.Label();
            this.param3Label = new System.Windows.Forms.Label();
            this.param2Label = new System.Windows.Forms.Label();
            this.param1TrackBar = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.glControlTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.texRgbGlControl = new OpenTK.GLControl();
            this.texAlphaGlControl = new OpenTK.GLControl();
            this.texParamsTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.magFilterComboBox = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.minFilterComboBox = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.wrapXComboBox = new System.Windows.Forms.ComboBox();
            this.mapModeComboBox = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.wrapYComboBox = new System.Windows.Forms.ComboBox();
            this.mipDetailComboBox = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.texturesListView = new System.Windows.Forms.ListView();
            this.texIdTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.label10 = new System.Windows.Forms.Label();
            this.textureIdTB = new System.Windows.Forms.TextBox();
            this.dummyRampCB = new System.Windows.Forms.CheckBox();
            this.sphereMapCB = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.generalFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.flagsButton = new System.Windows.Forms.Button();
            this.flagsPanel = new System.Windows.Forms.Panel();
            this.flagsTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.flagsTB = new System.Windows.Forms.TextBox();
            this.flagsLabel = new System.Windows.Forms.Label();
            this.alphaTestButton = new System.Windows.Forms.Button();
            this.alphaTestPanel = new System.Windows.Forms.Panel();
            this.alphaTestFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.alphaTestCB = new System.Windows.Forms.CheckBox();
            this.alphaFuncRefPanel = new System.Windows.Forms.Panel();
            this.alphaTestTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.refAlphaTB = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.alphaFuncComboBox = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.alphaBlendButton = new System.Windows.Forms.Button();
            this.alphaBlendPanel = new System.Windows.Forms.Panel();
            this.srcDstTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.dstTB = new System.Windows.Forms.TextBox();
            this.srcTB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.miscButton = new System.Windows.Forms.Button();
            this.miscPanel = new System.Windows.Forms.Panel();
            this.miscFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.zBufferTB = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cullModeComboBox = new System.Windows.Forms.ComboBox();
            this.shadowCB = new System.Windows.Forms.CheckBox();
            this.GlowCB = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.deleteMaterialButton = new System.Windows.Forms.Button();
            this.addMaterialButton = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.presetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadPresetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.savePresetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.propertiesGroupBox.SuspendLayout();
            this.paramsFlowLayout.SuspendLayout();
            this.addDelPropertyTableLayout.SuspendLayout();
            this.selectedPropGroupBox.SuspendLayout();
            this.selectedPropFlowLayout.SuspendLayout();
            this.paramsLabel.SuspendLayout();
            this.paramTableLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.param4TrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.param3TrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.param2TrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.param1TrackBar)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.glControlTableLayout.SuspendLayout();
            this.texParamsTableLayout.SuspendLayout();
            this.texIdTableLayout.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.generalFlowLayout.SuspendLayout();
            this.flagsPanel.SuspendLayout();
            this.flagsTableLayout.SuspendLayout();
            this.alphaTestPanel.SuspendLayout();
            this.alphaTestFlowLayout.SuspendLayout();
            this.alphaFuncRefPanel.SuspendLayout();
            this.alphaTestTableLayout.SuspendLayout();
            this.alphaBlendPanel.SuspendLayout();
            this.srcDstTableLayout.SuspendLayout();
            this.miscPanel.SuspendLayout();
            this.miscFlowLayout.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // matsComboBox
            // 
            this.matsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.matsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.matsComboBox.FormattingEnabled = true;
            this.matsComboBox.Location = new System.Drawing.Point(127, 10);
            this.matsComboBox.Name = "matsComboBox";
            this.matsComboBox.Size = new System.Drawing.Size(368, 21);
            this.matsComboBox.TabIndex = 1;
            this.matsComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(77, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Material";
            // 
            // propertiesListView
            // 
            this.propertiesListView.Dock = System.Windows.Forms.DockStyle.Top;
            this.propertiesListView.HideSelection = false;
            this.propertiesListView.LabelWrap = false;
            this.propertiesListView.Location = new System.Drawing.Point(3, 16);
            this.propertiesListView.Name = "propertiesListView";
            this.propertiesListView.ShowGroups = false;
            this.propertiesListView.Size = new System.Drawing.Size(555, 198);
            this.propertiesListView.TabIndex = 12;
            this.propertiesListView.UseCompatibleStateImageBehavior = false;
            this.propertiesListView.SelectedIndexChanged += new System.EventHandler(this.propertiesListView_SelectedIndexChanged);
            this.propertiesListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.propertiesListView_KeyDown);
            // 
            // propertiesGroupBox
            // 
            this.propertiesGroupBox.Controls.Add(this.paramsFlowLayout);
            this.propertiesGroupBox.Controls.Add(this.propertiesListView);
            this.propertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesGroupBox.Location = new System.Drawing.Point(0, 0);
            this.propertiesGroupBox.Name = "propertiesGroupBox";
            this.propertiesGroupBox.Size = new System.Drawing.Size(561, 591);
            this.propertiesGroupBox.TabIndex = 25;
            this.propertiesGroupBox.TabStop = false;
            this.propertiesGroupBox.Text = "Properties";
            // 
            // paramsFlowLayout
            // 
            this.paramsFlowLayout.Controls.Add(this.addDelPropertyTableLayout);
            this.paramsFlowLayout.Controls.Add(this.selectedPropGroupBox);
            this.paramsFlowLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.paramsFlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.paramsFlowLayout.Location = new System.Drawing.Point(3, 214);
            this.paramsFlowLayout.Name = "paramsFlowLayout";
            this.paramsFlowLayout.Size = new System.Drawing.Size(555, 374);
            this.paramsFlowLayout.TabIndex = 19;
            this.paramsFlowLayout.WrapContents = false;
            this.paramsFlowLayout.Resize += new System.EventHandler(this.flowLayout_Resize);
            // 
            // addDelPropertyTableLayout
            // 
            this.addDelPropertyTableLayout.ColumnCount = 4;
            this.addDelPropertyTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.addDelPropertyTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.addDelPropertyTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.addDelPropertyTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.addDelPropertyTableLayout.Controls.Add(this.deleteMatPropertyButton, 3, 0);
            this.addDelPropertyTableLayout.Controls.Add(this.colorSelect, 0, 0);
            this.addDelPropertyTableLayout.Controls.Add(this.addMatPropertyButton, 2, 0);
            this.addDelPropertyTableLayout.Controls.Add(this.matPropertyComboBox, 1, 0);
            this.addDelPropertyTableLayout.Location = new System.Drawing.Point(3, 3);
            this.addDelPropertyTableLayout.Name = "addDelPropertyTableLayout";
            this.addDelPropertyTableLayout.RowCount = 1;
            this.addDelPropertyTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.addDelPropertyTableLayout.Size = new System.Drawing.Size(425, 40);
            this.addDelPropertyTableLayout.TabIndex = 20;
            // 
            // deleteMatPropertyButton
            // 
            this.deleteMatPropertyButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.deleteMatPropertyButton.Location = new System.Drawing.Point(393, 9);
            this.deleteMatPropertyButton.Name = "deleteMatPropertyButton";
            this.deleteMatPropertyButton.Size = new System.Drawing.Size(29, 21);
            this.deleteMatPropertyButton.TabIndex = 25;
            this.deleteMatPropertyButton.Text = "x";
            this.deleteMatPropertyButton.UseVisualStyleBackColor = true;
            this.deleteMatPropertyButton.Click += new System.EventHandler(this.deleteMatPropertyButton_Click);
            // 
            // colorSelect
            // 
            this.colorSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.colorSelect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.colorSelect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.colorSelect.Location = new System.Drawing.Point(3, 8);
            this.colorSelect.Name = "colorSelect";
            this.colorSelect.Size = new System.Drawing.Size(26, 23);
            this.colorSelect.TabIndex = 24;
            this.colorSelect.UseVisualStyleBackColor = false;
            this.colorSelect.Click += new System.EventHandler(this.colorSelect_Click);
            // 
            // addMatPropertyButton
            // 
            this.addMatPropertyButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.addMatPropertyButton.Location = new System.Drawing.Point(358, 9);
            this.addMatPropertyButton.Name = "addMatPropertyButton";
            this.addMatPropertyButton.Size = new System.Drawing.Size(29, 21);
            this.addMatPropertyButton.TabIndex = 20;
            this.addMatPropertyButton.Text = "+";
            this.addMatPropertyButton.UseVisualStyleBackColor = true;
            this.addMatPropertyButton.Click += new System.EventHandler(this.addMatPropertyButton_Click);
            // 
            // matPropertyComboBox
            // 
            this.matPropertyComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.matPropertyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.matPropertyComboBox.FormattingEnabled = true;
            this.matPropertyComboBox.Location = new System.Drawing.Point(35, 9);
            this.matPropertyComboBox.Name = "matPropertyComboBox";
            this.matPropertyComboBox.Size = new System.Drawing.Size(317, 21);
            this.matPropertyComboBox.TabIndex = 21;
            this.matPropertyComboBox.SelectedIndexChanged += new System.EventHandler(this.matPropertyComboBox_SelectedIndexChanged);
            // 
            // selectedPropGroupBox
            // 
            this.selectedPropGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.selectedPropGroupBox.Controls.Add(this.selectedPropFlowLayout);
            this.selectedPropGroupBox.Location = new System.Drawing.Point(3, 49);
            this.selectedPropGroupBox.Name = "selectedPropGroupBox";
            this.selectedPropGroupBox.Size = new System.Drawing.Size(425, 290);
            this.selectedPropGroupBox.TabIndex = 21;
            this.selectedPropGroupBox.TabStop = false;
            this.selectedPropGroupBox.Text = "Selected Property";
            // 
            // selectedPropFlowLayout
            // 
            this.selectedPropFlowLayout.Controls.Add(this.propertyNameLabel);
            this.selectedPropFlowLayout.Controls.Add(this.paramsLabel);
            this.selectedPropFlowLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectedPropFlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.selectedPropFlowLayout.Location = new System.Drawing.Point(3, 16);
            this.selectedPropFlowLayout.Name = "selectedPropFlowLayout";
            this.selectedPropFlowLayout.Size = new System.Drawing.Size(419, 271);
            this.selectedPropFlowLayout.TabIndex = 0;
            this.selectedPropFlowLayout.WrapContents = false;
            this.selectedPropFlowLayout.Resize += new System.EventHandler(this.flowLayout_Resize);
            // 
            // propertyNameLabel
            // 
            this.propertyNameLabel.AutoSize = true;
            this.propertyNameLabel.Location = new System.Drawing.Point(3, 0);
            this.propertyNameLabel.Name = "propertyNameLabel";
            this.propertyNameLabel.Size = new System.Drawing.Size(52, 13);
            this.propertyNameLabel.TabIndex = 15;
            this.propertyNameLabel.Text = "Property: ";
            // 
            // paramsLabel
            // 
            this.paramsLabel.Controls.Add(this.paramTableLayout);
            this.paramsLabel.Location = new System.Drawing.Point(3, 16);
            this.paramsLabel.Name = "paramsLabel";
            this.paramsLabel.Size = new System.Drawing.Size(413, 178);
            this.paramsLabel.TabIndex = 16;
            // 
            // paramTableLayout
            // 
            this.paramTableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.paramTableLayout.ColumnCount = 3;
            this.paramTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.paramTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.paramTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.paramTableLayout.Controls.Add(this.param4TrackBar, 2, 3);
            this.paramTableLayout.Controls.Add(this.param3TrackBar, 2, 2);
            this.paramTableLayout.Controls.Add(this.param2TrackBar, 2, 1);
            this.paramTableLayout.Controls.Add(this.param4TB, 1, 3);
            this.paramTableLayout.Controls.Add(this.param3TB, 1, 2);
            this.paramTableLayout.Controls.Add(this.param2TB, 1, 1);
            this.paramTableLayout.Controls.Add(this.param1TB, 1, 0);
            this.paramTableLayout.Controls.Add(this.param4Label, 0, 3);
            this.paramTableLayout.Controls.Add(this.param1Label, 0, 0);
            this.paramTableLayout.Controls.Add(this.param3Label, 0, 2);
            this.paramTableLayout.Controls.Add(this.param2Label, 0, 1);
            this.paramTableLayout.Controls.Add(this.param1TrackBar, 2, 0);
            this.paramTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.paramTableLayout.Location = new System.Drawing.Point(0, 0);
            this.paramTableLayout.Name = "paramTableLayout";
            this.paramTableLayout.RowCount = 4;
            this.paramTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.paramTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.paramTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.paramTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.paramTableLayout.Size = new System.Drawing.Size(413, 178);
            this.paramTableLayout.TabIndex = 17;
            // 
            // param4TrackBar
            // 
            this.param4TrackBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.param4TrackBar.Location = new System.Drawing.Point(133, 135);
            this.param4TrackBar.Maximum = 200;
            this.param4TrackBar.Name = "param4TrackBar";
            this.param4TrackBar.Size = new System.Drawing.Size(277, 40);
            this.param4TrackBar.TabIndex = 24;
            this.param4TrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.param4TrackBar.Scroll += new System.EventHandler(this.param4TrackBar_Scroll);
            this.param4TrackBar.Leave += new System.EventHandler(this.param4TrackBar_Leave);
            // 
            // param3TrackBar
            // 
            this.param3TrackBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.param3TrackBar.Location = new System.Drawing.Point(133, 91);
            this.param3TrackBar.Maximum = 200;
            this.param3TrackBar.Name = "param3TrackBar";
            this.param3TrackBar.Size = new System.Drawing.Size(277, 38);
            this.param3TrackBar.TabIndex = 23;
            this.param3TrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.param3TrackBar.Scroll += new System.EventHandler(this.param3TrackBar_Scroll);
            this.param3TrackBar.Leave += new System.EventHandler(this.param3TrackBar_Leave);
            // 
            // param2TrackBar
            // 
            this.param2TrackBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.param2TrackBar.Location = new System.Drawing.Point(133, 47);
            this.param2TrackBar.Maximum = 200;
            this.param2TrackBar.Name = "param2TrackBar";
            this.param2TrackBar.Size = new System.Drawing.Size(277, 38);
            this.param2TrackBar.TabIndex = 22;
            this.param2TrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.param2TrackBar.Scroll += new System.EventHandler(this.param2TrackBar_Scroll);
            this.param2TrackBar.Leave += new System.EventHandler(this.param2TrackBar_Leave);
            // 
            // param4TB
            // 
            this.param4TB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.param4TB.Location = new System.Drawing.Point(52, 145);
            this.param4TB.Name = "param4TB";
            this.param4TB.Size = new System.Drawing.Size(75, 20);
            this.param4TB.TabIndex = 20;
            this.param4TB.TextChanged += new System.EventHandler(this.param4TB_TextChanged);
            // 
            // param3TB
            // 
            this.param3TB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.param3TB.Location = new System.Drawing.Point(52, 100);
            this.param3TB.Name = "param3TB";
            this.param3TB.Size = new System.Drawing.Size(75, 20);
            this.param3TB.TabIndex = 19;
            this.param3TB.TextChanged += new System.EventHandler(this.param3TB_TextChanged);
            // 
            // param2TB
            // 
            this.param2TB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.param2TB.Location = new System.Drawing.Point(52, 56);
            this.param2TB.Name = "param2TB";
            this.param2TB.Size = new System.Drawing.Size(75, 20);
            this.param2TB.TabIndex = 18;
            this.param2TB.TextChanged += new System.EventHandler(this.param2TB_TextChanged);
            // 
            // param1TB
            // 
            this.param1TB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.param1TB.Location = new System.Drawing.Point(52, 12);
            this.param1TB.Name = "param1TB";
            this.param1TB.Size = new System.Drawing.Size(75, 20);
            this.param1TB.TabIndex = 17;
            this.param1TB.TextChanged += new System.EventHandler(this.param1TB_TextChanged);
            // 
            // param4Label
            // 
            this.param4Label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.param4Label.AutoSize = true;
            this.param4Label.Location = new System.Drawing.Point(3, 148);
            this.param4Label.Name = "param4Label";
            this.param4Label.Size = new System.Drawing.Size(43, 13);
            this.param4Label.TabIndex = 15;
            this.param4Label.Text = "Param4";
            this.toolTip1.SetToolTip(this.param4Label, "Param 4");
            // 
            // param1Label
            // 
            this.param1Label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.param1Label.AutoSize = true;
            this.param1Label.Location = new System.Drawing.Point(3, 15);
            this.param1Label.Name = "param1Label";
            this.param1Label.Size = new System.Drawing.Size(43, 13);
            this.param1Label.TabIndex = 15;
            this.param1Label.Text = "Param1";
            this.toolTip1.SetToolTip(this.param1Label, "Param 1");
            // 
            // param3Label
            // 
            this.param3Label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.param3Label.AutoSize = true;
            this.param3Label.Location = new System.Drawing.Point(3, 103);
            this.param3Label.Name = "param3Label";
            this.param3Label.Size = new System.Drawing.Size(43, 13);
            this.param3Label.TabIndex = 15;
            this.param3Label.Text = "Param3";
            this.toolTip1.SetToolTip(this.param3Label, "Param 3");
            // 
            // param2Label
            // 
            this.param2Label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.param2Label.AutoSize = true;
            this.param2Label.Location = new System.Drawing.Point(3, 59);
            this.param2Label.Name = "param2Label";
            this.param2Label.Size = new System.Drawing.Size(43, 13);
            this.param2Label.TabIndex = 15;
            this.param2Label.Text = "Param2";
            this.toolTip1.SetToolTip(this.param2Label, "Param 2");
            // 
            // param1TrackBar
            // 
            this.param1TrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.param1TrackBar.Location = new System.Drawing.Point(133, 3);
            this.param1TrackBar.Maximum = 200;
            this.param1TrackBar.Name = "param1TrackBar";
            this.param1TrackBar.Size = new System.Drawing.Size(277, 38);
            this.param1TrackBar.TabIndex = 21;
            this.param1TrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.param1TrackBar.Scroll += new System.EventHandler(this.param1TrackBar_Scroll);
            this.param1TrackBar.Leave += new System.EventHandler(this.param1TrackBar_Leave);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel1);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(555, 585);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Textures";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.glControlTableLayout, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.texParamsTableLayout, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.texturesListView, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.texIdTableLayout, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(549, 566);
            this.tableLayoutPanel1.TabIndex = 28;
            // 
            // glControlTableLayout
            // 
            this.glControlTableLayout.ColumnCount = 2;
            this.glControlTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.glControlTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.glControlTableLayout.Controls.Add(this.texRgbGlControl, 0, 0);
            this.glControlTableLayout.Controls.Add(this.texAlphaGlControl, 1, 0);
            this.glControlTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControlTableLayout.Location = new System.Drawing.Point(3, 3);
            this.glControlTableLayout.Name = "glControlTableLayout";
            this.glControlTableLayout.RowCount = 1;
            this.glControlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.glControlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 158F));
            this.glControlTableLayout.Size = new System.Drawing.Size(543, 158);
            this.glControlTableLayout.TabIndex = 26;
            this.glControlTableLayout.Resize += new System.EventHandler(this.glControlTableLayout_Resize);
            // 
            // texRgbGlControl
            // 
            this.texRgbGlControl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.texRgbGlControl.BackColor = System.Drawing.Color.Black;
            this.texRgbGlControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.texRgbGlControl.Location = new System.Drawing.Point(70, 14);
            this.texRgbGlControl.Name = "texRgbGlControl";
            this.texRgbGlControl.Size = new System.Drawing.Size(130, 130);
            this.texRgbGlControl.TabIndex = 23;
            this.texRgbGlControl.VSync = false;
            this.texRgbGlControl.Paint += new System.Windows.Forms.PaintEventHandler(this.texRgbGlControl_Paint);
            // 
            // texAlphaGlControl
            // 
            this.texAlphaGlControl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.texAlphaGlControl.BackColor = System.Drawing.Color.Black;
            this.texAlphaGlControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.texAlphaGlControl.Location = new System.Drawing.Point(342, 14);
            this.texAlphaGlControl.Name = "texAlphaGlControl";
            this.texAlphaGlControl.Size = new System.Drawing.Size(130, 130);
            this.texAlphaGlControl.TabIndex = 25;
            this.texAlphaGlControl.VSync = false;
            this.texAlphaGlControl.Paint += new System.Windows.Forms.PaintEventHandler(this.texAlphaGlControl_Paint);
            // 
            // texParamsTableLayout
            // 
            this.texParamsTableLayout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.texParamsTableLayout.ColumnCount = 2;
            this.texParamsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.99999F));
            this.texParamsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00001F));
            this.texParamsTableLayout.Controls.Add(this.magFilterComboBox, 1, 3);
            this.texParamsTableLayout.Controls.Add(this.label14, 0, 2);
            this.texParamsTableLayout.Controls.Add(this.label15, 1, 2);
            this.texParamsTableLayout.Controls.Add(this.minFilterComboBox, 0, 3);
            this.texParamsTableLayout.Controls.Add(this.label12, 0, 0);
            this.texParamsTableLayout.Controls.Add(this.wrapXComboBox, 0, 1);
            this.texParamsTableLayout.Controls.Add(this.mapModeComboBox, 1, 5);
            this.texParamsTableLayout.Controls.Add(this.label13, 1, 0);
            this.texParamsTableLayout.Controls.Add(this.label11, 1, 4);
            this.texParamsTableLayout.Controls.Add(this.wrapYComboBox, 1, 1);
            this.texParamsTableLayout.Controls.Add(this.mipDetailComboBox, 0, 5);
            this.texParamsTableLayout.Controls.Add(this.label16, 0, 4);
            this.texParamsTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texParamsTableLayout.Location = new System.Drawing.Point(3, 371);
            this.texParamsTableLayout.Name = "texParamsTableLayout";
            this.texParamsTableLayout.RowCount = 6;
            this.texParamsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.texParamsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.texParamsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.texParamsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.texParamsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.texParamsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.texParamsTableLayout.Size = new System.Drawing.Size(543, 192);
            this.texParamsTableLayout.TabIndex = 11;
            // 
            // magFilterComboBox
            // 
            this.magFilterComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.magFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.magFilterComboBox.FormattingEnabled = true;
            this.magFilterComboBox.Location = new System.Drawing.Point(274, 98);
            this.magFilterComboBox.Name = "magFilterComboBox";
            this.magFilterComboBox.Size = new System.Drawing.Size(266, 21);
            this.magFilterComboBox.TabIndex = 15;
            this.magFilterComboBox.SelectedIndexChanged += new System.EventHandler(this.magFilterComboBox_SelectedIndexChanged);
            // 
            // label14
            // 
            this.label14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(3, 80);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(49, 13);
            this.label14.TabIndex = 12;
            this.label14.Text = "Min Filter";
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(274, 80);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(53, 13);
            this.label15.TabIndex = 12;
            this.label15.Text = "Mag Filter";
            // 
            // minFilterComboBox
            // 
            this.minFilterComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.minFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.minFilterComboBox.FormattingEnabled = true;
            this.minFilterComboBox.Location = new System.Drawing.Point(3, 98);
            this.minFilterComboBox.Name = "minFilterComboBox";
            this.minFilterComboBox.Size = new System.Drawing.Size(265, 21);
            this.minFilterComboBox.TabIndex = 15;
            this.minFilterComboBox.SelectedIndexChanged += new System.EventHandler(this.minFilterComboBox_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 18);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(67, 13);
            this.label12.TabIndex = 12;
            this.label12.Text = "WrapModeX";
            // 
            // wrapXComboBox
            // 
            this.wrapXComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.wrapXComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.wrapXComboBox.FormattingEnabled = true;
            this.wrapXComboBox.Location = new System.Drawing.Point(3, 36);
            this.wrapXComboBox.Name = "wrapXComboBox";
            this.wrapXComboBox.Size = new System.Drawing.Size(265, 21);
            this.wrapXComboBox.TabIndex = 15;
            this.wrapXComboBox.SelectedIndexChanged += new System.EventHandler(this.wrapXComboBox_SelectedIndexChanged);
            // 
            // mapModeComboBox
            // 
            this.mapModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.mapModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mapModeComboBox.FormattingEnabled = true;
            this.mapModeComboBox.Location = new System.Drawing.Point(274, 163);
            this.mapModeComboBox.Name = "mapModeComboBox";
            this.mapModeComboBox.Size = new System.Drawing.Size(266, 21);
            this.mapModeComboBox.TabIndex = 15;
            this.mapModeComboBox.SelectedIndexChanged += new System.EventHandler(this.mapModeComboBox_SelectedIndexChanged);
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(274, 18);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(67, 13);
            this.label13.TabIndex = 12;
            this.label13.Text = "WrapModeY";
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(274, 142);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(58, 13);
            this.label11.TabIndex = 12;
            this.label11.Text = "Map Mode";
            // 
            // wrapYComboBox
            // 
            this.wrapYComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.wrapYComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.wrapYComboBox.FormattingEnabled = true;
            this.wrapYComboBox.Location = new System.Drawing.Point(274, 36);
            this.wrapYComboBox.Name = "wrapYComboBox";
            this.wrapYComboBox.Size = new System.Drawing.Size(266, 21);
            this.wrapYComboBox.TabIndex = 15;
            this.wrapYComboBox.SelectedIndexChanged += new System.EventHandler(this.wrapYComboBox_SelectedIndexChanged);
            // 
            // mipDetailComboBox
            // 
            this.mipDetailComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.mipDetailComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mipDetailComboBox.FormattingEnabled = true;
            this.mipDetailComboBox.Location = new System.Drawing.Point(3, 163);
            this.mipDetailComboBox.Name = "mipDetailComboBox";
            this.mipDetailComboBox.Size = new System.Drawing.Size(265, 21);
            this.mipDetailComboBox.TabIndex = 15;
            this.mipDetailComboBox.SelectedIndexChanged += new System.EventHandler(this.mipDetailComboBox_SelectedIndexChanged);
            // 
            // label16
            // 
            this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(3, 142);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(54, 13);
            this.label16.TabIndex = 12;
            this.label16.Text = "Mip Detail";
            // 
            // texturesListView
            // 
            this.texturesListView.Alignment = System.Windows.Forms.ListViewAlignment.Left;
            this.texturesListView.AllowColumnReorder = true;
            this.texturesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.texturesListView.HideSelection = false;
            this.texturesListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5});
            this.texturesListView.LabelWrap = false;
            this.texturesListView.Location = new System.Drawing.Point(3, 167);
            this.texturesListView.MultiSelect = false;
            this.texturesListView.Name = "texturesListView";
            this.texturesListView.Size = new System.Drawing.Size(543, 158);
            this.texturesListView.TabIndex = 3;
            this.texturesListView.TileSize = new System.Drawing.Size(100, 10);
            this.texturesListView.UseCompatibleStateImageBehavior = false;
            this.texturesListView.SelectedIndexChanged += new System.EventHandler(this.texturesListView_SelectedIndexChanged);
            this.texturesListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.texturesListView_MouseDoubleClick);
            // 
            // texIdTableLayout
            // 
            this.texIdTableLayout.ColumnCount = 2;
            this.texIdTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.texIdTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.texIdTableLayout.Controls.Add(this.label10, 0, 0);
            this.texIdTableLayout.Controls.Add(this.textureIdTB, 1, 0);
            this.texIdTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.texIdTableLayout.Location = new System.Drawing.Point(3, 331);
            this.texIdTableLayout.Name = "texIdTableLayout";
            this.texIdTableLayout.RowCount = 1;
            this.texIdTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.texIdTableLayout.Size = new System.Drawing.Size(543, 34);
            this.texIdTableLayout.TabIndex = 27;
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(211, 10);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(57, 13);
            this.label10.TabIndex = 12;
            this.label10.Text = "Texture ID";
            // 
            // textureIDTB
            // 
            this.textureIdTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textureIdTB.Location = new System.Drawing.Point(274, 7);
            this.textureIdTB.Name = "textureIDTB";
            this.textureIdTB.Size = new System.Drawing.Size(266, 20);
            this.textureIdTB.TabIndex = 17;
            this.textureIdTB.TextChanged += new System.EventHandler(this.textureIdTB_TextChanged);
            // 
            // dummyRampCB
            // 
            this.dummyRampCB.Location = new System.Drawing.Point(0, 0);
            this.dummyRampCB.Name = "dummyRampCB";
            this.dummyRampCB.Size = new System.Drawing.Size(104, 24);
            this.dummyRampCB.TabIndex = 0;
            // 
            // sphereMapCB
            // 
            this.sphereMapCB.Location = new System.Drawing.Point(0, 0);
            this.sphereMapCB.Name = "sphereMapCB";
            this.sphereMapCB.Size = new System.Drawing.Size(104, 24);
            this.sphereMapCB.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 65);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(569, 617);
            this.tabControl1.TabIndex = 19;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(561, 591);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.generalFlowLayout);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(555, 585);
            this.groupBox3.TabIndex = 27;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Material";
            // 
            // generalFlowLayout
            // 
            this.generalFlowLayout.Controls.Add(this.flagsButton);
            this.generalFlowLayout.Controls.Add(this.flagsPanel);
            this.generalFlowLayout.Controls.Add(this.alphaTestButton);
            this.generalFlowLayout.Controls.Add(this.alphaTestPanel);
            this.generalFlowLayout.Controls.Add(this.alphaBlendButton);
            this.generalFlowLayout.Controls.Add(this.alphaBlendPanel);
            this.generalFlowLayout.Controls.Add(this.miscButton);
            this.generalFlowLayout.Controls.Add(this.miscPanel);
            this.generalFlowLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.generalFlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.generalFlowLayout.Location = new System.Drawing.Point(3, 16);
            this.generalFlowLayout.Name = "generalFlowLayout";
            this.generalFlowLayout.Size = new System.Drawing.Size(549, 566);
            this.generalFlowLayout.TabIndex = 25;
            this.generalFlowLayout.WrapContents = false;
            this.generalFlowLayout.Resize += new System.EventHandler(this.flowLayout_Resize);
            // 
            // flagsButton
            // 
            this.flagsButton.Location = new System.Drawing.Point(3, 3);
            this.flagsButton.Name = "flagsButton";
            this.flagsButton.Size = new System.Drawing.Size(422, 23);
            this.flagsButton.TabIndex = 0;
            this.flagsButton.Text = "Material Flags";
            this.flagsButton.UseVisualStyleBackColor = true;
            this.flagsButton.Click += new System.EventHandler(this.flagsButton_Click);
            // 
            // flagsPanel
            // 
            this.flagsPanel.Controls.Add(this.flagsTableLayout);
            this.flagsPanel.Location = new System.Drawing.Point(3, 32);
            this.flagsPanel.Name = "flagsPanel";
            this.flagsPanel.Size = new System.Drawing.Size(422, 41);
            this.flagsPanel.TabIndex = 1;
            // 
            // flagsTableLayout
            // 
            this.flagsTableLayout.ColumnCount = 2;
            this.flagsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37F));
            this.flagsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 63F));
            this.flagsTableLayout.Controls.Add(this.flagsTB, 1, 0);
            this.flagsTableLayout.Controls.Add(this.flagsLabel, 0, 0);
            this.flagsTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flagsTableLayout.Location = new System.Drawing.Point(0, 0);
            this.flagsTableLayout.Name = "flagsTableLayout";
            this.flagsTableLayout.RowCount = 1;
            this.flagsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.flagsTableLayout.Size = new System.Drawing.Size(422, 41);
            this.flagsTableLayout.TabIndex = 26;
            // 
            // flagsTB
            // 
            this.flagsTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.flagsTB.Location = new System.Drawing.Point(159, 10);
            this.flagsTB.Name = "flagsTB";
            this.flagsTB.Size = new System.Drawing.Size(260, 20);
            this.flagsTB.TabIndex = 15;
            this.flagsTB.TextChanged += new System.EventHandler(this.flagsTB_TextChanged);
            // 
            // flagsLabel
            // 
            this.flagsLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.flagsLabel.AutoSize = true;
            this.flagsLabel.Location = new System.Drawing.Point(121, 14);
            this.flagsLabel.Name = "flagsLabel";
            this.flagsLabel.Size = new System.Drawing.Size(32, 13);
            this.flagsLabel.TabIndex = 10;
            this.flagsLabel.Text = "Flags";
            // 
            // alphaTestButton
            // 
            this.alphaTestButton.Location = new System.Drawing.Point(3, 79);
            this.alphaTestButton.Name = "alphaTestButton";
            this.alphaTestButton.Size = new System.Drawing.Size(422, 23);
            this.alphaTestButton.TabIndex = 2;
            this.alphaTestButton.Text = "Alpha Testing";
            this.alphaTestButton.UseVisualStyleBackColor = true;
            this.alphaTestButton.Click += new System.EventHandler(this.alphaTestButton_Click);
            // 
            // alphaTestPanel
            // 
            this.alphaTestPanel.AutoSize = true;
            this.alphaTestPanel.Controls.Add(this.alphaTestFlowLayout);
            this.alphaTestPanel.Location = new System.Drawing.Point(3, 108);
            this.alphaTestPanel.Name = "alphaTestPanel";
            this.alphaTestPanel.Size = new System.Drawing.Size(422, 93);
            this.alphaTestPanel.TabIndex = 3;
            // 
            // alphaTestFlowLayout
            // 
            this.alphaTestFlowLayout.AutoSize = true;
            this.alphaTestFlowLayout.Controls.Add(this.alphaTestCB);
            this.alphaTestFlowLayout.Controls.Add(this.alphaFuncRefPanel);
            this.alphaTestFlowLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.alphaTestFlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.alphaTestFlowLayout.Location = new System.Drawing.Point(0, 0);
            this.alphaTestFlowLayout.Name = "alphaTestFlowLayout";
            this.alphaTestFlowLayout.Size = new System.Drawing.Size(422, 93);
            this.alphaTestFlowLayout.TabIndex = 24;
            this.alphaTestFlowLayout.WrapContents = false;
            this.alphaTestFlowLayout.Resize += new System.EventHandler(this.flowLayout_Resize);
            // 
            // alphaTestCB
            // 
            this.alphaTestCB.AutoSize = true;
            this.alphaTestCB.Location = new System.Drawing.Point(3, 3);
            this.alphaTestCB.Name = "alphaTestCB";
            this.alphaTestCB.Size = new System.Drawing.Size(91, 17);
            this.alphaTestCB.TabIndex = 25;
            this.alphaTestCB.Text = "Alpha Testing";
            this.alphaTestCB.UseVisualStyleBackColor = true;
            this.alphaTestCB.CheckedChanged += new System.EventHandler(this.alphaTestCB_CheckedChanged);
            // 
            // alphaFuncRefPanel
            // 
            this.alphaFuncRefPanel.Controls.Add(this.alphaTestTableLayout);
            this.alphaFuncRefPanel.Location = new System.Drawing.Point(3, 26);
            this.alphaFuncRefPanel.Name = "alphaFuncRefPanel";
            this.alphaFuncRefPanel.Size = new System.Drawing.Size(416, 64);
            this.alphaFuncRefPanel.TabIndex = 27;
            // 
            // alphaTestTableLayout
            // 
            this.alphaTestTableLayout.AutoSize = true;
            this.alphaTestTableLayout.ColumnCount = 2;
            this.alphaTestTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37F));
            this.alphaTestTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 63F));
            this.alphaTestTableLayout.Controls.Add(this.refAlphaTB, 1, 1);
            this.alphaTestTableLayout.Controls.Add(this.label5, 0, 1);
            this.alphaTestTableLayout.Controls.Add(this.alphaFuncComboBox, 1, 0);
            this.alphaTestTableLayout.Controls.Add(this.label9, 0, 0);
            this.alphaTestTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.alphaTestTableLayout.Location = new System.Drawing.Point(0, 0);
            this.alphaTestTableLayout.Name = "alphaTestTableLayout";
            this.alphaTestTableLayout.RowCount = 2;
            this.alphaTestTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.alphaTestTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.alphaTestTableLayout.Size = new System.Drawing.Size(416, 64);
            this.alphaTestTableLayout.TabIndex = 26;
            // 
            // refAlphaTB
            // 
            this.refAlphaTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.refAlphaTB.Location = new System.Drawing.Point(156, 38);
            this.refAlphaTB.Name = "refAlphaTB";
            this.refAlphaTB.Size = new System.Drawing.Size(257, 20);
            this.refAlphaTB.TabIndex = 15;
            this.refAlphaTB.TextChanged += new System.EventHandler(this.refAlphaTB_TextChanged);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(63, 41);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Reference Alpha";
            // 
            // alphaFuncComboBox
            // 
            this.alphaFuncComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.alphaFuncComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.alphaFuncComboBox.FormattingEnabled = true;
            this.alphaFuncComboBox.Location = new System.Drawing.Point(156, 5);
            this.alphaFuncComboBox.Name = "alphaFuncComboBox";
            this.alphaFuncComboBox.Size = new System.Drawing.Size(257, 21);
            this.alphaFuncComboBox.TabIndex = 19;
            this.alphaFuncComboBox.SelectedIndexChanged += new System.EventHandler(this.AlphaFuncCB_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(92, 9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(58, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "AlphaFunc";
            // 
            // alphaBlendButton
            // 
            this.alphaBlendButton.Location = new System.Drawing.Point(3, 207);
            this.alphaBlendButton.Name = "alphaBlendButton";
            this.alphaBlendButton.Size = new System.Drawing.Size(422, 23);
            this.alphaBlendButton.TabIndex = 4;
            this.alphaBlendButton.Text = "Alpha Blending";
            this.alphaBlendButton.UseVisualStyleBackColor = true;
            this.alphaBlendButton.Click += new System.EventHandler(this.alphaBlendButton_Click);
            // 
            // alphaBlendPanel
            // 
            this.alphaBlendPanel.Controls.Add(this.srcDstTableLayout);
            this.alphaBlendPanel.Location = new System.Drawing.Point(3, 236);
            this.alphaBlendPanel.Name = "alphaBlendPanel";
            this.alphaBlendPanel.Size = new System.Drawing.Size(422, 62);
            this.alphaBlendPanel.TabIndex = 5;
            // 
            // srcDstTableLayout
            // 
            this.srcDstTableLayout.AutoSize = true;
            this.srcDstTableLayout.ColumnCount = 2;
            this.srcDstTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37F));
            this.srcDstTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 63F));
            this.srcDstTableLayout.Controls.Add(this.dstTB, 1, 1);
            this.srcDstTableLayout.Controls.Add(this.srcTB, 1, 0);
            this.srcDstTableLayout.Controls.Add(this.label3, 0, 1);
            this.srcDstTableLayout.Controls.Add(this.label2, 0, 0);
            this.srcDstTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.srcDstTableLayout.Location = new System.Drawing.Point(0, 0);
            this.srcDstTableLayout.Name = "srcDstTableLayout";
            this.srcDstTableLayout.RowCount = 2;
            this.srcDstTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.srcDstTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.srcDstTableLayout.Size = new System.Drawing.Size(422, 62);
            this.srcDstTableLayout.TabIndex = 8;
            // 
            // dstTB
            // 
            this.dstTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dstTB.Location = new System.Drawing.Point(159, 36);
            this.dstTB.Name = "dstTB";
            this.dstTB.Size = new System.Drawing.Size(260, 20);
            this.dstTB.TabIndex = 15;
            this.dstTB.TextChanged += new System.EventHandler(this.dstTB_TextChanged);
            // 
            // srcTB
            // 
            this.srcTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.srcTB.Location = new System.Drawing.Point(159, 5);
            this.srcTB.Name = "srcTB";
            this.srcTB.Size = new System.Drawing.Size(260, 20);
            this.srcTB.TabIndex = 15;
            this.srcTB.TextChanged += new System.EventHandler(this.srcTB_TextChanged);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(100, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "DstFactor";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(100, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "SrcFactor";
            // 
            // miscButton
            // 
            this.miscButton.Location = new System.Drawing.Point(3, 304);
            this.miscButton.Name = "miscButton";
            this.miscButton.Size = new System.Drawing.Size(419, 23);
            this.miscButton.TabIndex = 6;
            this.miscButton.Text = "Misc";
            this.miscButton.UseVisualStyleBackColor = true;
            this.miscButton.Click += new System.EventHandler(this.miscButton_Click);
            // 
            // miscPanel
            // 
            this.miscPanel.Controls.Add(this.miscFlowLayout);
            this.miscPanel.Location = new System.Drawing.Point(3, 333);
            this.miscPanel.Name = "miscPanel";
            this.miscPanel.Size = new System.Drawing.Size(422, 125);
            this.miscPanel.TabIndex = 7;
            // 
            // miscFlowLayout
            // 
            this.miscFlowLayout.Controls.Add(this.tableLayoutPanel3);
            this.miscFlowLayout.Controls.Add(this.shadowCB);
            this.miscFlowLayout.Controls.Add(this.GlowCB);
            this.miscFlowLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.miscFlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.miscFlowLayout.Location = new System.Drawing.Point(0, 0);
            this.miscFlowLayout.Name = "miscFlowLayout";
            this.miscFlowLayout.Size = new System.Drawing.Size(422, 125);
            this.miscFlowLayout.TabIndex = 23;
            this.miscFlowLayout.Resize += new System.EventHandler(this.flowLayout_Resize);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 63F));
            this.tableLayoutPanel3.Controls.Add(this.label7, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.zBufferTB, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.cullModeComboBox, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(419, 69);
            this.tableLayoutPanel3.TabIndex = 22;
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(78, 45);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(74, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "z-Buffer Offset";
            // 
            // zBufferTB
            // 
            this.zBufferTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.zBufferTB.Location = new System.Drawing.Point(158, 41);
            this.zBufferTB.Name = "zBufferTB";
            this.zBufferTB.Size = new System.Drawing.Size(258, 20);
            this.zBufferTB.TabIndex = 15;
            this.zBufferTB.TextChanged += new System.EventHandler(this.zBufferTB_TextChanged);
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(98, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Cull Mode";
            // 
            // cullModeComboBox
            // 
            this.cullModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cullModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cullModeComboBox.FormattingEnabled = true;
            this.cullModeComboBox.Location = new System.Drawing.Point(158, 6);
            this.cullModeComboBox.Name = "cullModeComboBox";
            this.cullModeComboBox.Size = new System.Drawing.Size(258, 21);
            this.cullModeComboBox.TabIndex = 18;
            this.cullModeComboBox.SelectedIndexChanged += new System.EventHandler(this.cullModeComboBox_SelectedIndexChanged);
            // 
            // shadowCB
            // 
            this.shadowCB.AutoSize = true;
            this.shadowCB.Location = new System.Drawing.Point(3, 78);
            this.shadowCB.Name = "shadowCB";
            this.shadowCB.Size = new System.Drawing.Size(70, 17);
            this.shadowCB.TabIndex = 17;
            this.shadowCB.Text = "Shadows";
            this.shadowCB.UseVisualStyleBackColor = true;
            this.shadowCB.CheckedChanged += new System.EventHandler(this.shadowCB_CheckedChanged);
            // 
            // GlowCB
            // 
            this.GlowCB.AutoSize = true;
            this.GlowCB.Location = new System.Drawing.Point(3, 101);
            this.GlowCB.Name = "GlowCB";
            this.GlowCB.Size = new System.Drawing.Size(50, 17);
            this.GlowCB.TabIndex = 18;
            this.GlowCB.Text = "Glow";
            this.GlowCB.UseVisualStyleBackColor = true;
            this.GlowCB.CheckedChanged += new System.EventHandler(this.GlowCB_CheckedChanged);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(561, 591);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Textures";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.propertiesGroupBox);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(561, 591);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Properties";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // headerPanel
            // 
            this.headerPanel.Controls.Add(this.tableLayoutPanel4);
            this.headerPanel.Controls.Add(this.menuStrip1);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Location = new System.Drawing.Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(569, 65);
            this.headerPanel.TabIndex = 26;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.deleteMaterialButton, 3, 0);
            this.tableLayoutPanel4.Controls.Add(this.matsComboBox, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.addMaterialButton, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(569, 41);
            this.tableLayoutPanel4.TabIndex = 24;
            // 
            // deleteMaterialButton
            // 
            this.deleteMaterialButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.deleteMaterialButton.Location = new System.Drawing.Point(536, 10);
            this.deleteMaterialButton.Name = "deleteMaterialButton";
            this.deleteMaterialButton.Size = new System.Drawing.Size(29, 21);
            this.deleteMaterialButton.TabIndex = 22;
            this.deleteMaterialButton.Text = "x";
            this.deleteMaterialButton.UseVisualStyleBackColor = true;
            this.deleteMaterialButton.Click += new System.EventHandler(this.deleteMaterialButton_Click);
            // 
            // addMaterialButton
            // 
            this.addMaterialButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.addMaterialButton.Location = new System.Drawing.Point(501, 10);
            this.addMaterialButton.Name = "addMaterialButton";
            this.addMaterialButton.Size = new System.Drawing.Size(29, 21);
            this.addMaterialButton.TabIndex = 21;
            this.addMaterialButton.Text = "+";
            this.addMaterialButton.UseVisualStyleBackColor = true;
            this.addMaterialButton.Click += new System.EventHandler(this.addMaterialButton_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.presetToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(569, 24);
            this.menuStrip1.TabIndex = 23;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // presetToolStripMenuItem
            // 
            this.presetToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadPresetToolStripMenuItem,
            this.savePresetToolStripMenuItem});
            this.presetToolStripMenuItem.Name = "presetToolStripMenuItem";
            this.presetToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.presetToolStripMenuItem.Text = "Preset";
            // 
            // loadPresetToolStripMenuItem
            // 
            this.loadPresetToolStripMenuItem.Name = "loadPresetToolStripMenuItem";
            this.loadPresetToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.loadPresetToolStripMenuItem.Text = "Load preset";
            this.loadPresetToolStripMenuItem.Click += new System.EventHandler(this.loadPresetButton_Click);
            // 
            // savePresetToolStripMenuItem
            // 
            this.savePresetToolStripMenuItem.Name = "savePresetToolStripMenuItem";
            this.savePresetToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.savePresetToolStripMenuItem.Text = "Save preset";
            this.savePresetToolStripMenuItem.Click += new System.EventHandler(this.savePresetToolStripMenuItem_Click);
            // 
            // NUDMaterialEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(569, 682);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.headerPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(300, 630);
            this.Name = "NudMaterialEditor";
            this.TabText = "Material";
            this.Text = "NUDMaterialEditor";
            this.Scroll += new System.Windows.Forms.ScrollEventHandler(this.NUDMaterialEditor_Scroll);
            this.propertiesGroupBox.ResumeLayout(false);
            this.paramsFlowLayout.ResumeLayout(false);
            this.addDelPropertyTableLayout.ResumeLayout(false);
            this.selectedPropGroupBox.ResumeLayout(false);
            this.selectedPropFlowLayout.ResumeLayout(false);
            this.selectedPropFlowLayout.PerformLayout();
            this.paramsLabel.ResumeLayout(false);
            this.paramTableLayout.ResumeLayout(false);
            this.paramTableLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.param4TrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.param3TrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.param2TrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.param1TrackBar)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.glControlTableLayout.ResumeLayout(false);
            this.texParamsTableLayout.ResumeLayout(false);
            this.texParamsTableLayout.PerformLayout();
            this.texIdTableLayout.ResumeLayout(false);
            this.texIdTableLayout.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.generalFlowLayout.ResumeLayout(false);
            this.generalFlowLayout.PerformLayout();
            this.flagsPanel.ResumeLayout(false);
            this.flagsTableLayout.ResumeLayout(false);
            this.flagsTableLayout.PerformLayout();
            this.alphaTestPanel.ResumeLayout(false);
            this.alphaTestPanel.PerformLayout();
            this.alphaTestFlowLayout.ResumeLayout(false);
            this.alphaTestFlowLayout.PerformLayout();
            this.alphaFuncRefPanel.ResumeLayout(false);
            this.alphaFuncRefPanel.PerformLayout();
            this.alphaTestTableLayout.ResumeLayout(false);
            this.alphaTestTableLayout.PerformLayout();
            this.alphaBlendPanel.ResumeLayout(false);
            this.alphaBlendPanel.PerformLayout();
            this.srcDstTableLayout.ResumeLayout(false);
            this.srcDstTableLayout.PerformLayout();
            this.miscPanel.ResumeLayout(false);
            this.miscFlowLayout.ResumeLayout(false);
            this.miscFlowLayout.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox matsComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView propertiesListView;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.GroupBox propertiesGroupBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox sphereMapCB;
        private System.Windows.Forms.CheckBox dummyRampCB;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel srcDstTableLayout;
        private System.Windows.Forms.TextBox srcTB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox zBufferTB;
        private System.Windows.Forms.ComboBox cullModeComboBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox alphaFuncComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox shadowCB;
        private System.Windows.Forms.CheckBox GlowCB;
        private System.Windows.Forms.Label flagsLabel;
        private System.Windows.Forms.TextBox flagsTB;
        private System.Windows.Forms.Button addMaterialButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TextBox dstTB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button deleteMaterialButton;
        private System.Windows.Forms.Button addMatPropertyButton;
        private System.Windows.Forms.ComboBox matPropertyComboBox;
        private System.Windows.Forms.Button colorSelect;
        private System.Windows.Forms.TableLayoutPanel paramTableLayout;
        private System.Windows.Forms.TrackBar param4TrackBar;
        private System.Windows.Forms.TrackBar param3TrackBar;
        private System.Windows.Forms.TrackBar param2TrackBar;
        private System.Windows.Forms.TextBox param4TB;
        private System.Windows.Forms.TextBox param3TB;
        private System.Windows.Forms.TextBox param2TB;
        private System.Windows.Forms.TextBox param1TB;
        private System.Windows.Forms.Label param4Label;
        private System.Windows.Forms.Label param1Label;
        private System.Windows.Forms.Label param3Label;
        private System.Windows.Forms.Label param2Label;
        private System.Windows.Forms.TrackBar param1TrackBar;
        private System.Windows.Forms.Label propertyNameLabel;
        private System.Windows.Forms.Button deleteMatPropertyButton;
        private System.Windows.Forms.FlowLayoutPanel alphaTestFlowLayout;
        private System.Windows.Forms.CheckBox alphaTestCB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox refAlphaTB;
        private System.Windows.Forms.FlowLayoutPanel generalFlowLayout;
        private System.Windows.Forms.Button flagsButton;
        private System.Windows.Forms.Panel flagsPanel;
        private System.Windows.Forms.TableLayoutPanel flagsTableLayout;
        private System.Windows.Forms.Button alphaTestButton;
        private System.Windows.Forms.Panel alphaTestPanel;
        private System.Windows.Forms.Button alphaBlendButton;
        private System.Windows.Forms.Panel alphaBlendPanel;
        private System.Windows.Forms.Button miscButton;
        private System.Windows.Forms.Panel miscPanel;
        private System.Windows.Forms.FlowLayoutPanel miscFlowLayout;
        private System.Windows.Forms.FlowLayoutPanel paramsFlowLayout;
        private System.Windows.Forms.Panel paramsLabel;
        private System.Windows.Forms.TableLayoutPanel addDelPropertyTableLayout;
        private System.Windows.Forms.GroupBox selectedPropGroupBox;
        private System.Windows.Forms.FlowLayoutPanel selectedPropFlowLayout;
        private System.Windows.Forms.TableLayoutPanel alphaTestTableLayout;
        private System.Windows.Forms.Panel alphaFuncRefPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem presetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadPresetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem savePresetToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel glControlTableLayout;
        private OpenTK.GLControl texRgbGlControl;
        private OpenTK.GLControl texAlphaGlControl;
        private System.Windows.Forms.TableLayoutPanel texParamsTableLayout;
        private System.Windows.Forms.ComboBox magFilterComboBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox minFilterComboBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox wrapXComboBox;
        private System.Windows.Forms.ComboBox mapModeComboBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox wrapYComboBox;
        private System.Windows.Forms.ComboBox mipDetailComboBox;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TableLayoutPanel texIdTableLayout;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textureIdTB;
        private System.Windows.Forms.ListView texturesListView;
    }
}