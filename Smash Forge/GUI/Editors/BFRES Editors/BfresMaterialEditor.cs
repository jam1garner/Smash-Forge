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
using Smash_Forge.Rendering;

namespace Smash_Forge
{
    public partial class BfresMaterialEditor : DockContent
    {
        public BFRES.Mesh poly;
        public BFRES.MaterialData mat;
        public ImageList paramColorList = new ImageList();
        public ImageList textureImageList = new ImageList();
        public string SelectedMatParam = "";
        public string SelectedTexture = "";

        public List<string> ColorBoxMatParmList = new List<string>(new string[] {
           "edge_light_color",
        });

        public BfresMaterialEditor()
        {
            InitializeComponent();
            ResetParamUIData();
        }
        public void LoadMaterial(BFRES.Mesh p)
         {
            listView1.Items.Clear();
            dataGridView3.Rows.Clear();
            dataGridView4.Rows.Clear();

            textureImageList.ColorDepth = ColorDepth.Depth32Bit;
            textureImageList.ImageSize = new Size(64, 64);

            poly = p;
            mat = p.material;

            FillForm();

            textBox1.Text = mat.Name;
            ShaderArchivelabel1.Text += $" {mat.shaderassign.ShaderArchive}";
            ShaderMdllabel2.Text += $" {mat.shaderassign.ShaderModel}";

            paramColorList.ImageSize = new Size(10, 10);
            listView1.SmallImageList = paramColorList;
            listView1.FullRowSelect = true;  

            TextureRefListView.SmallImageList = textureImageList;
            TextureRefListView.FullRowSelect = true;
        }

        public void FillForm()
        {
            BFRES.MaterialData material = mat;

            InitializeTextureListView(material);
            InitializeParamListView(material);
            InitializeShaderOptionList(material);
            InitializeRenderInfoList(material);
        }
        private void InitializeRenderInfoList(BFRES.MaterialData mat)
        {
            foreach (var rnd in mat.renderinfo)
            {
                if (rnd.Type == RenderInfoType.Int32)
                    dataGridView4.Rows.Add(rnd.Name, rnd.Value_Int.ToString(), "Int");
                if (rnd.Type == RenderInfoType.Single)
                    dataGridView4.Rows.Add(rnd.Name, rnd.Value_Float.ToString(), "Float");
                if (rnd.Type == RenderInfoType.String)
                    dataGridView4.Rows.Add(rnd.Name, rnd.Value_String.ToString(), "String");
            }
        }
        private void InitializeShaderOptionList(BFRES.MaterialData mat)
        {
            foreach (var op in mat.shaderassign.options)
            {
                dataGridView3.Rows.Add(op.Key, op.Value);
            }
        }
        private void InitializeParamListView(BFRES.MaterialData mat)
        {
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
                        Vector3 col = mat.matparam[prm.Key].Value_float3;

                        float someIntX = col.X;
                        float someIntY = col.Y;
                        float someIntZ = col.Z;

                        someIntX = (float)Math.Pow(someIntX, 2.2f);
                        someIntY = (float)Math.Pow(someIntY, 2.2f);
                        someIntZ = (float)Math.Pow(someIntZ, 2.2f);

                        if (someIntX <= 1 && someIntY <= 1 && someIntZ <= 1)
                        {

                            SetColor = Color.FromArgb(
                         255,
                        (int)Math.Ceiling(someIntX * 255.0f),
                        (int)Math.Ceiling(someIntY * 255.0f),
                        (int)Math.Ceiling(someIntZ * 255.0f)
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
                        Vector4 col = mat.matparam[prm.Key].Value_float4;

                        float someIntX = col.X;
                        float someIntY = col.Y;
                        float someIntZ = col.Z;

                        someIntX = (float)Math.Pow(someIntX, 2.2f);
                        someIntY = (float)Math.Pow(someIntY, 2.2f);
                        someIntZ = (float)Math.Pow(someIntZ, 2.2f);

                        System.Diagnostics.Debug.WriteLine($"{prm.Key} R {someIntX} G {someIntY} B {someIntZ}");

                        if (someIntX <= 1 && someIntY <= 1 && someIntZ <= 1)
                        {

                            SetColor = Color.FromArgb(
                         255,
                        (int)Math.Ceiling(someIntX * 255.0f),
                        (int)Math.Ceiling(someIntY * 255.0f),
                        (int)Math.Ceiling(someIntZ * 255.0f)
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
        }
        private void InitializeTextureListView(BFRES.MaterialData mat)
        {
            int CurTex = 0;
            TextureRefListView.Items.Clear();
            if (BFRES.IsSwitchBFRES == true)
            {
                foreach (BNTX bntx in Runtime.BNTXList)
                {
                    foreach (var texure in mat.textures)
                    {
                        if (bntx.glTexByName.ContainsKey(texure.Name))
                        {
                            Bitmap bitmap = TextureToBitmap.RenderBitmapUseExistingContext((SFGraphics.GLObjects.Textures.Texture2D)bntx.glTexByName[texure.Name], 64, 64);

                            textureImageList.Images.Add(texure.Name, bitmap);

                            TextureRefListView.Items.Add(texure.Name, CurTex++);

                            if (bitmap != null)
                            {
                                bitmap.Dispose();
                                GC.Collect();
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (FTEXContainer ftexC in Runtime.FTEXContainerList)
                {

                    foreach (var texure in mat.textures)
                    {
                        if (ftexC.glTexByName.ContainsKey(texure.Name))
                        {
                            Bitmap bitmap = TextureToBitmap.RenderBitmapUseExistingContext((SFGraphics.GLObjects.Textures.Texture2D)ftexC.glTexByName[texure.Name], 64, 64);

                            textureImageList.Images.Add(texure.Name, bitmap);

                            TextureRefListView.Items.Add(texure.Name, CurTex++);

                            if (bitmap != null)
                            {
                                bitmap.Dispose();
                                GC.Collect();
                            }
                        }
                    }
                }
            }
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

                        if (mat.shaderassign.ShaderArchive == "")
                        {

                        }

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

        private void TextureRefListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TextureRefListView.SelectedItems.Count > 0)
            {
                SelectedTexture = TextureRefListView.SelectedItems[0].Text;
                foreach (BFRES.MatTexture matTexure in mat.textures)
                {
                    if (matTexure.Name == SelectedTexture)
                    {
                        textureTypeLabel.Text = $"Type {matTexure.Type.ToString()}";
                        samplerLabel.Text = $"Sampler {matTexure.SamplerName}";
                        pxelSamplerLabel.Text = $"Pixel Sampler {matTexure.FragShaderSampler}";
                    }
                }
            }
            else
            {
                textureTypeLabel.Text = $"Type";
                samplerLabel.Text = $"Sampler";
                pxelSamplerLabel.Text = $"Pixel Sampler";
            }
        }

        private void TextureBoxClickedOn(object sender, EventArgs e)
        {
            
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

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (BFRES.IsSwitchBFRES)
            {
                foreach (BNTX bntx in Runtime.BNTXList)
                {
                    if (bntx.glTexByName.ContainsKey(SelectedTexture))
                    {
                        BntxTextureList edit = new BntxTextureList();
                        edit.LoadTexture(SelectedTexture);
                        edit.Show();
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (BFRES.IsSwitchBFRES)
            {
                foreach (BNTX bntx in Runtime.BNTXList)
                {
                    if (bntx.glTexByName.ContainsKey(SelectedTexture))
                    {
                        BNTXMaterialTextureEditor edit = new BNTXMaterialTextureEditor();
                        edit.LoadTexture(poly, bntx.glTexByName[SelectedTexture], SelectedTexture);
                        edit.Show();
                    }
                }
            }
        }
    }
}
