using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using SmashForge.Rendering.Meshes;

namespace SmashForge
{
    public partial class BntxTextureList : Form
    {

        public ImageList il = new ImageList();
        public List<Bitmap> texturesCol = new List<Bitmap>();

        private bool loadedC = false;
        private bool loadedA = false;

        private Mesh3D screenTriangle;
        public string currentTextureOpen = "";

        public void LoadTexture(string texture)
        {
            currentTextureOpen = texture;
        }

        public BntxTextureList()
        {
            InitializeComponent();

            int count = 0;

            foreach (BNTX bntx in Runtime.bntxList)
            {
                foreach (BRTI tex in bntx.textures)
                {
                    Bitmap color = new Bitmap(BfresMaterialEditor.TextureRgba(tex.texture, tex.display));

                    il.Images.Add(color);
                    texturesCol.Add(color);

                    color.Dispose();
                    string[] row1 = { tex.Height.ToString(), tex.Width.ToString() };
                    listView1.Items.Add(tex.Text, count++).SubItems.AddRange(row1);
                }
            }
            il.ColorDepth = ColorDepth.Depth32Bit;
            il.ImageSize = new Size(30, 30);
            listView1.SmallImageList = il;

            Rendering.OpenTkSharedResources.InitializeSharedResources();
            if (Rendering.OpenTkSharedResources.SetupStatus == Rendering.OpenTkSharedResources.SharedResourceStatus.Initialized)
            {
                screenTriangle = Rendering.ScreenDrawing.CreateScreenTriangle();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string texName = listView1.SelectedItems[0].Text;
            }
        }

        private void RenderTextureColor(BRTI tex)
        {
            if (!loadedC || glControl1 == null)
                return;

            if (Rendering.OpenTkSharedResources.SetupStatus != Rendering.OpenTkSharedResources.SharedResourceStatus.Initialized)
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

            Rendering.ScreenDrawing.DrawTexturedQuad(tex.display, width, height, screenTriangle, true, true, true, false);

            glControl1.SwapBuffers();
        }
        private void RenderTextureAlpha(BRTI tex)
        {
            if (!loadedA || glControl2 == null)
                return;

            if (Rendering.OpenTkSharedResources.SetupStatus != Rendering.OpenTkSharedResources.SharedResourceStatus.Initialized)
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

            Rendering.ScreenDrawing.DrawTexturedQuad(tex.display, width, height, screenTriangle, false, false, false, true);

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
            loadedC = true;
        }

        private void glControl2_Load(object sender, EventArgs e)
        {
            loadedA = true;
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
