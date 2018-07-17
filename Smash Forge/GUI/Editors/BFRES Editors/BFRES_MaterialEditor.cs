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

namespace Smash_Forge.GUI.Editors
{
    public partial class BFRES_MaterialEditor : Form
    {
        public BFRES.Mesh poly;
        public BFRES.MaterialData mat;
        public int TexPanelHeight = 31;
        public PictureBox[] picboxArray;

        public List<string> ColorBoxMatParmList = new List<string>(new string[] {
           "edge_light_color",
        });

        public BFRES_MaterialEditor(BFRES.Mesh p)
        {
            InitializeComponent();
            ResetParamDataTabs();



            poly = p;

            Console.WriteLine("Material Editor");
            Console.WriteLine(p.Text);

            mat = p.material;
            textBox1.Text = mat.Name;

            int CurParam = 0;
            foreach (var prm in mat.matparam)
            {

                //   listBox2.Items.Add(prm);
                dataGridView1.Rows.Add(prm.Value);

                if (prm.Value.Type == ShaderParamType.Float3)
                {
                    bool IsColor = prm.Key.Contains("Color") || prm.Key.Contains("color");

                    if (IsColor)
                    {
                        int someIntX = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float3.X * 255);
                        int someIntY = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float3.Y * 255);
                        int someIntZ = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float3.Z * 255);

                        if (someIntX <= 255 && someIntY <= 255 && someIntZ <= 255)
                        {
                            Console.WriteLine($"{prm.Key} R {someIntX} G {someIntY} B {someIntZ}");

                            dataGridView1.Rows[CurParam].Cells[1].Style.BackColor = Color.FromArgb(
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
                      bool IsColor = prm.Key.Contains("Color") || prm.Key.Contains("color");

                    if (IsColor)
                    {
                     

                        int someIntX = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float4.X * 255);
                        int someIntY = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float4.Y * 255);
                        int someIntZ = (int)Math.Ceiling(mat.matparam[prm.Key].Value_float4.Z * 255);

                        if (someIntX <= 255 && someIntY <= 255 && someIntZ <= 255)
                        {
                            Console.WriteLine($"{prm.Key} R {someIntX} G {someIntY} B {someIntZ}");

                            dataGridView1.Rows[CurParam].Cells[1].Style.BackColor = Color.FromArgb(
                         255,
                        someIntX,
                        someIntY,
                        someIntZ
                        );
                        }
                    }

                

                }
                dataGridView1.Rows[CurParam].Cells[2].Value = prm.Value.Type.ToString();

                CurParam++;
            }

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
        private void TextureBoxClickedOn(object sender, EventArgs e)
        {
            BNTXMaterialTextureEditor edit = new BNTXMaterialTextureEditor();
            edit.Show();
            if (BFRES.IsSwitchBFRES)
                edit.LoadTexture(BNTX.textured[mat.textures[0].Name]);
        }
        private void OpenTextureEditor(BRTI tex)
        {
            BNTXMaterialTextureEditor edit = new BNTXMaterialTextureEditor();
            edit.Show();
            edit.LoadTexture(tex);
        }
        private void SetWrapModeItems(ComboBox c)
        {
            foreach (GX2TexClamp wr in Enum.GetValues(typeof(GX2TexClamp)))
            {
                c.Items.Add(wr);
            }
        }
        private void ParamHideTab(TabPage p)
        {
            ParamDataControl.TabPages.Remove(p);
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
        public Bitmap textureRGBA(BRTI.BRTI_Texture t, int id)
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
        private void ParamShowTab(TabPage p)
        {
            ParamDataControl.TabPages.Add(p);
        }
        private void ResetParamDataTabs()
        {
            ParamHideTab(FloatParam);
            ParamHideTab(Float2Param);
            ParamHideTab(Float3Param);
            ParamHideTab(Float4Param);
            ParamHideTab(SRTParam);
            ParamHideTab(boolParam);
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

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                Console.WriteLine("Test " + dataGridView1.SelectedRows.Count);
                for (int index = 0; index < dataGridView1.SelectedRows.Count; index++)
                {
                    var selectedRow = dataGridView1.SelectedRows[index];
                    var prm = (BFRES.ShaderParam)selectedRow.DataBoundItem;

                    Console.WriteLine(prm.Name);


                    ResetParamDataTabs();


                    if (prm.Type == ShaderParamType.Float)
                        ParamShowTab(FloatParam);
                    if (prm.Type == ShaderParamType.Float2)
                        ParamShowTab(Float2Param);
                    if (prm.Type == ShaderParamType.Float3)
                        ParamShowTab(Float3Param);
                    if (prm.Type == ShaderParamType.Float4)
                        ParamShowTab(Float4Param);
                    if (prm.Type == ShaderParamType.TexSrt)
                        ParamShowTab(SRTParam);
                    if (prm.Type == ShaderParamType.Bool)
                        ParamShowTab(boolParam);
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
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
    }
}
