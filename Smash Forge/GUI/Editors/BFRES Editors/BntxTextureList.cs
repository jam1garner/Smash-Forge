using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Smash_Forge.Rendering.Meshes;

namespace Smash_Forge
{
    public partial class BntxTextureList : Form
    {

        public ImageList il = new ImageList();
        public List<Bitmap> texturesCol = new List<Bitmap>();

        private bool _loadedC = false;
        private bool _loadedA = false;

        private Mesh3D screenTriangle;
        public string CurrentTextureOpen = "";

        public void LoadTexture(string texture)
        {
            CurrentTextureOpen = texture;
        }

        public BntxTextureList()
        {
            InitializeComponent();

            int count = 0;

            foreach (BNTX bntx in Runtime.BNTXList)
            {
                foreach (BRTI tex in bntx.textures)
                {
                    Bitmap color = new Bitmap(BfresMaterialEditor.textureRGBA(tex.texture, tex.display));

                    il.Images.Add(color);
                    texturesCol.Add(color);

                   
                    string[] row1 = { tex.Height.ToString(), tex.Width.ToString() };
                    listView1.Items.Add(tex.Text, count++).SubItems.AddRange(row1);
                }
            }
            il.ColorDepth = ColorDepth.Depth32Bit;
            il.ImageSize = new Size(30, 30);
            listView1.SmallImageList = il;

            Rendering.OpenTKSharedResources.InitializeSharedResources();
            if (Rendering.OpenTKSharedResources.SetupStatus == Rendering.OpenTKSharedResources.SharedResourceStatus.Initialized)
            {
                screenTriangle = Rendering.ScreenDrawing.CreateScreenTriangle();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string TexName = listView1.SelectedItems[0].Text;
            }
        }

        private void RenderTextureColor(BRTI tex)
        {
            if (!_loadedC || glControl1 == null)
                return;

            if (Rendering.OpenTKSharedResources.SetupStatus != Rendering.OpenTKSharedResources.SharedResourceStatus.Initialized)
                return;

            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);

            if (listView1.SelectedItems == null)
            {
                glControl1.SwapBuffers();
                return;
            }

            int width = tex.Width;
            int height = tex.Height;

            int texture = tex.display;

            Rendering.ScreenDrawing.DrawTexturedQuad(texture, width, height, screenTriangle, true, true, true, false);

            glControl1.SwapBuffers();
        }
        private void RenderTextureAlpha(BRTI tex)
        {
            if (!_loadedA || glControl2 == null)
                return;

            if (Rendering.OpenTKSharedResources.SetupStatus != Rendering.OpenTKSharedResources.SharedResourceStatus.Initialized)
                return;

            glControl2.MakeCurrent();
            GL.Viewport(glControl2.ClientRectangle);

            if (listView1.SelectedItems == null)
            {
                glControl2.SwapBuffers();
                return;
            }

            int width = tex.Width;
            int height = tex.Height;

            int texture = tex.display;

            Rendering.ScreenDrawing.DrawTexturedQuad(texture, width, height, screenTriangle, false, false, false, true);

            glControl2.SwapBuffers();
        }

        private void previewBox_Resize(object sender, EventArgs e)
        {
        
        }

        private void OkBut_Click(object sender, EventArgs e)
        {



            this.Close();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            _loadedC = true;
        }

        private void glControl2_Load(object sender, EventArgs e)
        {
            _loadedA = true;
        }

        private void BNTX_TextureList_Load(object sender, EventArgs e)
        {

        }

        private void panel2_Resize(object sender, EventArgs e)
        {
            int size = Math.Min(panel2.Width, panel2.Height);
            glControl2.Width = size;
            glControl2.Height = size;
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            int size = Math.Min(panel1.Width, panel1.Height);
            glControl1.Width = size;
            glControl1.Height = size;
        }
    }
}
