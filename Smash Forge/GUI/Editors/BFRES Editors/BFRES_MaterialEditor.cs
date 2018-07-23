using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syroot.NintenTools.Bfres;
using Syroot.NintenTools.Bfres.GX2;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using WeifenLuo.WinFormsUI.Docking;

namespace Smash_Forge
{
    public partial class BFRES_MaterialEditor : DockContent
    {
        public BFRES.Mesh poly;
        public BFRES.MaterialData mat;
        public int TexPanelHeight = 31;
        public PictureBox[] picboxArray;
        public ImageList il = new ImageList();
        public string SelectedMatParam = "";

        public List<string> ColorBoxMatParmList = new List<string>(new string[] {
           "edge_light_color",
        });

        public BFRES_MaterialEditor()
        {
            InitializeComponent();
            ResetParamUIData();
        }
        public void LoadMaterial(BFRES.Mesh p)
         {
            listView1.Items.Clear();
            dataGridView3.Rows.Clear();
            dataGridView4.Rows.Clear();
            tabTextureMaps.Controls.Clear();
            TexPanelHeight = 31;

            poly = p;

            Console.WriteLine("Material Editor");
            Console.WriteLine(p.Text);


            mat = p.material;
            textBox1.Text = mat.Name;

            int CurParam = 0;
            foreach (var prm in mat.matparam)
            {
                string DisplayValue = "";

                //   listBox2.Items.Add(prm);
                var item = new ListViewItem(prm.Key);

                Color SetColor = Color.White;

                if (prm.Value.Type == ShaderParamType.Float)
                {
                    DisplayValue = mat.matparam[prm.Key].Value_float.ToString();
                }
                if (prm.Value.Type == ShaderParamType.Float2)
                {
                    DisplayValue = mat.matparam[prm.Key].Value_float2.ToString();
                }
                if (prm.Value.Type == ShaderParamType.Float3)
                {
                    DisplayValue = mat.matparam[prm.Key].Value_float3.ToString();

                    bool IsColor = prm.Key.Contains("Color") || prm.Key.Contains("color");

                    if (IsColor)
                    {
                        int someIntX = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float3.X * 255);
                        int someIntY = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float3.Y * 255);
                        int someIntZ = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float3.Z * 255);

                        if (someIntX <= 255 && someIntY <= 255 && someIntZ <= 255)
                        {
                            Console.WriteLine($"{prm.Key} R {someIntX} G {someIntY} B {someIntZ}");

                            SetColor = Color.FromArgb(
                        255,
                        someIntX,
                        someIntY,
                        someIntZ
                        );
                        }

                    }

                }
                if (prm.Value.Type == ShaderParamType.Float4)
                {
                    DisplayValue = mat.matparam[prm.Key].Value_float4.ToString();


          

                    bool IsColor = prm.Key.Contains("Color") || prm.Key.Contains("color") || prm.Key.Contains("konst0");


                    if (IsColor)
                    {


                        int someIntX = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float4.X * 255);
                        int someIntY = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float4.Y * 255);
                        int someIntZ = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float4.Z * 255);

                        if (someIntX <= 255 && someIntY <= 255 && someIntZ <= 255)
                        {
                            Console.WriteLine($"{prm.Key} R {someIntX} G {someIntY} B {someIntZ}");

                            SetColor = Color.FromArgb(
                         255,
                        someIntX,
                        someIntY,
                        someIntZ
                        );
                        }
                    }
                }

                item.UseItemStyleForSubItems = false;
                item.SubItems.Add(DisplayValue);
                item.SubItems.Add("");
                item.SubItems[2].BackColor = SetColor;
                listView1.View = View.Details;
                listView1.Items.Add(item);
                CurParam++;
            }
            il.ImageSize = new Size(10, 10);
            listView1.SmallImageList = il;
            listView1.FullRowSelect = true;

            foreach (var rnd in mat.renderinfo)
            {
                if (rnd.Type == RenderInfoType.Int32)
                    dataGridView4.Rows.Add(rnd.Name, rnd.Value_Int.ToString(), "Int");
                if (rnd.Type == RenderInfoType.Single)
                    dataGridView4.Rows.Add(rnd.Name, rnd.Value_Float.ToString(), "Float");
                if (rnd.Type == RenderInfoType.String)
                    dataGridView4.Rows.Add(rnd.Name, rnd.Value_String.ToString(), "String");
            }

            foreach (var op in mat.shaderassign.options)
            {
                dataGridView3.Rows.Add(op.Key, op.Value);
            }

            Panel[] arr = new Panel[mat.textures.Count];
            Button[] Btnarr = new Button[mat.textures.Count];
            picboxArray = new PictureBox[mat.textures.Count];

            if (BFRES.IsSwitchBFRES == true)
            {
                foreach (BRTI tex in BNTX.textures)
                {
                    foreach (var texure in mat.textures)
                    {
                        if (tex.Text == texure.Name)
                        {
                            Bitmap bmp = textureRGBA(tex.texture, tex.display);

                            SetTexturePanel(bmp, texure);
                        }
                    }
                }
            }
            else
            {
                foreach (var texure in mat.textures)
                {
                    if (BFRES.FTEXtextures.ContainsKey(texure.Name))
                    {
                        Bitmap bmp = FTEXRGBA(BFRES.FTEXtextures[texure.Name].texture, BFRES.FTEXtextures[texure.Name].texture.display);

                        SetTexturePanel(bmp, texure);
                    }
                }
            }    
        }
        private void SetTexturePanel(Bitmap bmp, BFRES.MatTexture texure)
        {
            Panel panel = new Panel();
            PictureBox picbox = new PictureBox();
            Button PanelHide = new Button();
            Button ChangeTex = new Button();

            PanelHide.Text = "Hide " + texure.Name;
            ChangeTex.Text = "Change Texture";

            picbox.Image = bmp;


            panel.Location = new System.Drawing.Point(7, TexPanelHeight);
            PanelHide.Location = new System.Drawing.Point(7, TexPanelHeight - 27);
            panel.Size = new Size(360, 142);
            picbox.Size = new Size(137, 137);
            PanelHide.Size = new Size(360, 22);

            picbox.SizeMode = PictureBoxSizeMode.StretchImage;

            PanelHide.Click += new System.EventHandler(panelHide_Click);
            picbox.Click += new System.EventHandler(TextureBoxClickedOn);
            ChangeTex.Click += new System.EventHandler(ChangeTexClickedOn);

            Label TextueType = new Label();
            Label SamplerText = new Label();

            ChangeTex.Location = new System.Drawing.Point(144, 3);
            TextueType.Location = new System.Drawing.Point(144, 66);
            SamplerText.Location = new System.Drawing.Point(144, 88);

            ChangeTex.Size = new Size(150, 26);

       
            TextueType.Text = $"Type = {texure.Type.ToString()}";
            SamplerText.Text = $"Sampler = {texure.SamplerName}";

            if (texure.FragShaderSampler != "")
                SamplerText.Text = $"Sampler = {texure.FragShaderSampler}";

            TexPanelHeight = TexPanelHeight + 170;

            panel.Controls.Add(picbox);
            panel.Controls.Add(TextueType);
            panel.Controls.Add(SamplerText);
            panel.Controls.Add(ChangeTex);

            tabTextureMaps.Controls.Add(panel);
            tabTextureMaps.Controls.Add(PanelHide);
        }
        private void ChangeTexClickedOn(object sender, EventArgs e)
        {
            if (BFRES.IsSwitchBFRES)
            {
                BNTX_TextureList edit = new BNTX_TextureList();
                edit.Show();
            }
        }

        private void TextureBoxClickedOn(object sender, EventArgs e)
        {
            BNTXMaterialTextureEditor edit = new BNTXMaterialTextureEditor();
            edit.Show();
           if (BFRES.IsSwitchBFRES)
               edit.LoadTexture(poly, BNTX.textured[mat.textures[0].Name]);
           else
                edit.LoadTexture(poly, null, BFRES.FTEXtextures[mat.textures[0].Name]);
        }
        private void OpenTextureEditor(BRTI tex)
        {
         
        }
        private void SetWrapModeItems(ComboBox c)
        {
            foreach (GX2TexClamp wr in Enum.GetValues(typeof(GX2TexClamp)))
            {
                c.Items.Add(wr);
            }
        }
 
        public Bitmap FTEXRGBA(FTEX.FTEX_Texture t, int id)
        {
            Bitmap bitmap = new Bitmap(t.width, t.height);
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, t.width, t.height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }
        public static Bitmap textureRGBA(BRTI.BRTI_Texture t, int id)
        {
            Bitmap bitmap = new Bitmap(t.width, t.height);
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, t.width, t.height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        private void RenderTexture(bool justRenderAlpha = false)
        {
            if (!MaterialsTab.SelectedTab.Text.Equals("Textures"))
                return;

        }
        private void panelHide_Click(object sender, EventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            poly.Text = textBox1.Text;
        }

        private void BFRES_MaterialEditor_Load(object sender, EventArgs e)
        {

        }

        private void glControl1_Load(object sender, EventArgs e)
        {

        }

        private void tabTextureMaps_Click(object sender, EventArgs e)
        {

        }


        private void panel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void listView1_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            if (listView1.Sorting == SortOrder.Ascending)         
                listView1.Sorting = SortOrder.Descending;          
            else
                listView1.Sorting = SortOrder.Ascending;
        }
        private void ResetParamUIData()
        {
            FloatNumUD.Visible = false;
            FloatNumUD2.Visible = false;
            FloatNumUD3.Visible = false;
            FloatNumUD4.Visible = false;
            FloatNumUD5.Visible = false;
            FloatNumUD6.Visible = false;
            FloatNumUD7.Visible = false;
            FloatNumUD8.Visible = false;
            FloatLabel1.Visible = false;
            FloatLabel2.Visible = false;
            FloatLabel3.Visible = false;
            FloatLabel4.Visible = false;
            FloatLabel5.Visible = false;
            FloatLabel6.Visible = false;
            FloatLabel7.Visible = false;
            FloatLabel8.Visible = false;
            boolLabel.Visible = false;
            boolParam.Visible = false;
            colorboxLabel.Visible = false;
            paramColorBox.Visible = false;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                SelectedMatParam = listView1.SelectedItems[0].Text;

                var prm = mat.matparam[SelectedMatParam];

                ResetParamUIData();

                if (prm.Type == ShaderParamType.Float)
                {
                    FloatNumUD.Value = (decimal)prm.Value_float;
                    FloatLabel1.Text = "Float";
                    FloatLabel1.Visible = true;
                    FloatNumUD.Visible = true;
                }
                if (prm.Type == ShaderParamType.Float2)
                {
                    FloatNumUD.Value = (decimal)prm.Value_float2.X;
                    FloatNumUD2.Value = (decimal)prm.Value_float2.Y;
                    FloatLabel1.Text = "X";
                    FloatLabel2.Text = "Y";
                    FloatLabel1.Visible = true;
                    FloatLabel2.Visible = true;
                    FloatNumUD.Visible = true;
                    FloatNumUD2.Visible = true;
                }
                if (prm.Type == ShaderParamType.Float3)
                {
                    FloatNumUD.Value = (decimal)prm.Value_float3.X;
                    FloatNumUD2.Value = (decimal)prm.Value_float3.Y;
                    FloatNumUD3.Value = (decimal)prm.Value_float3.Z;
                    FloatLabel1.Text = "X";
                    FloatLabel2.Text = "Y";
                    FloatLabel3.Text = "Z";
                    FloatLabel1.Visible = true;
                    FloatLabel2.Visible = true;
                    FloatLabel3.Visible = true;
                    FloatNumUD.Visible = true;
                    FloatNumUD2.Visible = true;
                    FloatNumUD3.Visible = true;

                    bool IsColor = listView1.SelectedItems[0].Text.Contains("Color") || listView1.SelectedItems[0].Text.Contains("color") || listView1.SelectedItems[0].Text.Contains("konst0");

                    if (IsColor)
                    {
                        colorboxLabel.Visible = true;
                        paramColorBox.BackColor = listView1.SelectedItems[0].SubItems[2].BackColor;
                        paramColorBox.Visible = true;
                    }

                }
                if (prm.Type == ShaderParamType.Float4)
                {
                    FloatNumUD.Value = (decimal)prm.Value_float4.X;
                    FloatNumUD2.Value = (decimal)prm.Value_float4.Y;
                    FloatNumUD3.Value = (decimal)prm.Value_float4.Z;
                    FloatNumUD4.Value = (decimal)prm.Value_float4.W;
                    FloatLabel1.Text = "X";
                    FloatLabel2.Text = "Y";
                    FloatLabel3.Text = "Z";
                    FloatLabel4.Text = "W";
                    FloatLabel1.Visible = true;
                    FloatLabel2.Visible = true;
                    FloatLabel3.Visible = true;
                    FloatLabel4.Visible = true;
                    FloatNumUD.Visible = true;
                    FloatNumUD2.Visible = true;
                    FloatNumUD3.Visible = true;
                    FloatNumUD4.Visible = true;

                    bool IsColor = listView1.SelectedItems[0].Text.Contains("Color") || listView1.SelectedItems[0].Text.Contains("color") || listView1.SelectedItems[0].Text.Contains("konst0");

                    if (IsColor)
                    {
                        colorboxLabel.Visible = true;
                        paramColorBox.BackColor = listView1.SelectedItems[0].SubItems[2].BackColor;
                        paramColorBox.Visible = true;
                    }
                }
                if (prm.Type == ShaderParamType.TexSrt)
                {

                }
                if (prm.Type == ShaderParamType.Bool)
                {

                }
            }
        }

        private void paramColorBox_Click(object sender, EventArgs e)
        {
            if (SelectedMatParam != "")
            {
                var prm = mat.matparam[SelectedMatParam];

                ColorDialog clr = new ColorDialog();
                if (clr.ShowDialog() == DialogResult.OK)
                {
                    paramColorBox.BackColor = clr.Color;

                    string Value = "";

                    if (prm.Type == ShaderParamType.Float4)
                    {
                        prm.Value_float4.X = (float)clr.Color.R / 255;
                        prm.Value_float4.Y = (float)clr.Color.G / 255;
                        prm.Value_float4.Z = (float)clr.Color.B / 255;
                        prm.Value_float4.W = (float)clr.Color.A / 255;
                        Value = prm.Value_float4.ToString();
                    }
                    if (prm.Type == ShaderParamType.Float3)
                    {
                        prm.Value_float3.X = (float)clr.Color.R / 255;
                        prm.Value_float3.Y = (float)clr.Color.G / 255;
                        prm.Value_float3.Z = (float)clr.Color.B / 255;
                        Value = prm.Value_float3.ToString();
                    }
                    listView1.SelectedItems[0].SubItems[1].Text = Value;
                    listView1.SelectedItems[0].SubItems[2].BackColor = clr.Color;
                }

                mat.matparam[SelectedMatParam] = prm;
        
            }
        }

        private void FloatNumUD_ValueChanged(object sender, EventArgs e)
        {
            if (SelectedMatParam != "")
            {
                string Value = "";

                var prm = mat.matparam[SelectedMatParam];
                if (prm.Type == ShaderParamType.Float)
                {
                    Value = prm.Value_float.ToString();

                    if (sender == FloatNumUD)
                        prm.Value_float = (float)FloatNumUD.Value;
                }
                if (prm.Type == ShaderParamType.Float2)
                {
                    Value = prm.Value_float2.ToString();

                    if (sender == FloatNumUD)
                        prm.Value_float2.X = (float)FloatNumUD.Value;
                    if (sender == FloatNumUD2)
                        prm.Value_float2.Y = (float)FloatNumUD2.Value;
                }
                if (prm.Type == ShaderParamType.Float3)
                {
                    Value = prm.Value_float3.ToString();

                    if (sender == FloatNumUD)
                        prm.Value_float3.X = (float)FloatNumUD.Value;
                    if (sender == FloatNumUD2)
                        prm.Value_float3.Y = (float)FloatNumUD2.Value;
                    if (sender == FloatNumUD3)
                        prm.Value_float3.Z = (float)FloatNumUD3.Value;
                }
                if (prm.Type == ShaderParamType.Float4)
                {
                    Value = prm.Value_float4.ToString();

                    if (sender == FloatNumUD)
                        prm.Value_float4.X = (float)FloatNumUD.Value;
                    if (sender == FloatNumUD2)
                        prm.Value_float4.Y = (float)FloatNumUD2.Value;
                    if (sender == FloatNumUD3)
                        prm.Value_float4.Z = (float)FloatNumUD3.Value;
                    if (sender == FloatNumUD4)
                        prm.Value_float4.W = (float)FloatNumUD4.Value;
                }

              mat.matparam[SelectedMatParam] = prm;
              listView1.SelectedItems[0].SubItems[1].Text = Value;
            }
        }
    }
}
