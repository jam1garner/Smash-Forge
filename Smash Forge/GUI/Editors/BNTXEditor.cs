using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects;
using SFGraphics.GLObjects.GLObjectManagement;
using Smash_Forge.Rendering;
using Smash_Forge.Rendering.Meshes;

namespace Smash_Forge
{
    public partial class BNTXEditor : EditorBase
    {
        private BNTX BNTX;
        private BRTI BRTI;

        private Texture textureToRender = null;

        private bool renderR = true;
        private bool renderG = true;
        private bool renderB = true;
        private bool renderAlpha = true;
        private bool keepAspectRatio = false;
        private int currentMipLevel = 0;

        private Mesh3D screenTriangle;

        private FileSystemWatcher fw;

        ContextMenu TextureMenu = new ContextMenu();

        public BNTXEditor()
        {
            InitializeComponent();
            FilePath = "";
            Text = "New Binary Texture";

            fw = new FileSystemWatcher();
            fw.Path = Path.GetTempPath();
            fw.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            fw.EnableRaisingEvents = false;
            fw.Changed += new FileSystemEventHandler(OnChanged);
            fw.Filter = "";

            MenuItem export = new MenuItem("Export");
            export.Click += exportTextureToolStripMenuItem_Click;
            TextureMenu.MenuItems.Add(export);

            MenuItem replace = new MenuItem("Replace");
            replace.Click += replaceTextureToolStripMenuItem_Click;
            TextureMenu.MenuItems.Add(replace);

            OpenTKSharedResources.InitializeSharedResources();
            if (OpenTKSharedResources.SetupStatus == OpenTKSharedResources.SharedResourceStatus.Initialized)
            {
                screenTriangle = ScreenDrawing.CreateScreenTriangle();
            }
        }

        public void SelectBNTX(BNTX b)
        {
            BNTX = b;
            FillForm();
        }

        public BNTXEditor(BNTX bntx) : this()
        {
            SelectBNTX(bntx);
        }

        public BNTXEditor(byte[] data) : this()
        {
            BNTX b = new BNTX();
            b.ReadBNTXFile(data);
            FilePath = "";
            OpenBNTX(data);
            Edited = false;
            SelectBNTX(b);
        }

        public override void Save()
        {
            if (FilePath.Equals(""))
            {
                SaveAs();
                return;
            }
            ExportBNTX(FilePath);
            Edited = false;
        }

        public override void SaveAs()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Nintendo Switch BNTX|*.bntx|" +
                             "All files(*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".bntx"))
                    {
                        FilePath = sfd.FileName; //Todo call in BNTX file to rebuild
                        Save();
                    }
                }
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File modified!");
            string filename = e.FullPath;
        }

        public void FillForm()
        {
            textureListBox.Items.Clear();
            foreach (BRTI tex in BNTX.textures)
            {
                textureListBox.Items.Add(tex);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
        public void OpenBNTX(byte[] fname)
        {
            Edited = true;

            foreach (BRTI tex in BNTX.textures)
            {
                textureListBox.Items.Add(tex);
            }
        }

        public static BRTI.BRTI_Texture fromPNG(string filename, int mipcount)
        {
            Bitmap bmp = new Bitmap(filename);
            BRTI.BRTI_Texture tex = new BRTI.BRTI_Texture();

            tex.mipmaps.Add(fromPNG(bmp));
            for (int i = 0; i < mipcount; i++)
            {
                if (bmp.Width / (int)Math.Pow(2, i) < 4 || bmp.Height / (int)Math.Pow(2, i) < 4) break;
                tex.mipmaps.Add(fromPNG(Pixel.ResizeImage(bmp, bmp.Width / (int)Math.Pow(2, i), bmp.Height / (int)Math.Pow(2, i))));
                tex.mipmaps[0] = tex.mipmaps[0];
            }
            tex.width = bmp.Width;
            tex.height = bmp.Height;
            tex.pixelInternalFormat = PixelInternalFormat.Rgba;
            tex.pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;

            return tex;
        }

        private static byte[] fromPNG(Bitmap bmp)
        {
            BitmapData bmpData =
                bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] pix = new byte[bytes];

            Marshal.Copy(ptr, pix, 0, bytes);

            bmp.UnlockBits(bmpData);

            // swap red and blue channels
            for (int i = 0; i < pix.Length; i += 4)
            {
                byte temp = pix[i];
                pix[i] = pix[i + 2];
                pix[i + 2] = temp;
            }

            return pix;
        }

        private void exportTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textureListBox.SelectedItem == null) return;
            using (var sfd = new SaveFileDialog())
            {
                BRTI tex = (BRTI)(textureListBox.SelectedItem);
                sfd.Filter = "Direct Draw Surface (.png)|*.png|" +
                                "All files(*.*)|*.*";

                sfd.FileName = tex.Text;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // use png instead
                    if (sfd.FileName.EndsWith(".png"))
                    {
                        tex.ExportAsImage(tex.texture, tex.display, sfd.FileName + ".png");
                    }
                    if (sfd.FileName.EndsWith(".dds"))
                    {
                    }
                }
            }
        }

        public void ExportBNTX(string filename)
        {
            FileOutput o = new FileOutput();
            o.Endian = Endianness.Little;

            o.save(filename);
        }

        private void replaceTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                BRTI t = (BRTI)(textureListBox.SelectedItem);

                ofd.Filter = "Portable Network Graphic (.png)|*.png;|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Edited = true;
                    BRTI.BRTI_Texture newTexture = null;
                    if (Path.GetExtension(ofd.FileName).ToLower().Equals(".png"))
                    {
                        newTexture =  fromPNG(ofd.FileName, 1);
                    }
                    if (Path.GetExtension(ofd.FileName).ToLower().Equals(".dds"))
                    {
                        DDS dds = new DDS(new FileData(ofd.FileName));
                        newTexture = dds.toBRTITexture();
                    }

                    t.texture.height = newTexture.height;
                    t.texture.width = newTexture.width;
                    t.texture.pixelInternalFormat = newTexture.pixelInternalFormat;
                    t.texture.mipmaps = newTexture.mipmaps;
                    t.texture.pixelFormat = newTexture.pixelFormat;
                    newTexture.mipmaps.Add(t.texture.mipmaps[0]);

                    GL.DeleteTexture(t.texture.display);

                    BNTX.glTexByName.Add(ofd.FileName, BRTI.CreateTexture2D(t.texture));

                    if (newTexture == null)
                        return;

                    FillForm();
                }
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Portable Network Graphic, Smash for 3DS Tex (.png, .tex)|*.png;*.tex;|" +
                             "All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (Path.GetExtension(save.FileName).ToLower().Equals(".png"))
                {
    
                }
                if (Path.GetExtension(save.FileName).ToLower().Equals(".tex"))
                {
                    ExportBNTX(save.FileName);
                }
            }
        }

        private void formatSelector_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textureListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (textureListBox.SelectedIndex >= 0)
            {
                BRTI b = ((BRTI)textureListBox.SelectedItem);
                

                label1.Text = "Width: " + b.texture.width;
                label2.Text = "Height: " + b.texture.height;

                uint Format = b.format >> 8;
                byte DataType = (byte)(b.format & 0xFF);

                Formats.BNTXImageFormat f = (Formats.BNTXImageFormat)Format;
                Formats.BNTXImageTypes t = (Formats.BNTXImageTypes)DataType;

                label3.Text = f.ToString() + " " + t.ToString();

                // Render the selected texture.
                if (BNTX.glTexByName.ContainsKey(b.Text))
                    textureToRender = BNTX.glTexByName[b.Text];
            }
            else
            {
                label1.Text = "Width: " + "";
                label2.Text = "Height: " + "";
            }
            glControl1.Invalidate();
        }

        private void RenderTexture()
        {
            if (OpenTKSharedResources.SetupStatus != OpenTKSharedResources.SharedResourceStatus.Initialized || glControl1 == null)
                return;

            if (!OpenTKSharedResources.shaders["Texture"].ProgramCreatedSuccessfully)
                return;

            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);

            if (textureListBox.SelectedItem == null)
            {
                glControl1.SwapBuffers();
                return;
            }

            int width = ((BRTI)textureListBox.SelectedItem).Width;
            int height = ((BRTI)textureListBox.SelectedItem).Height;

            int texture = ((BRTI)textureListBox.SelectedItem).display;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            if (textureToRender != null)
            {
                ScreenDrawing.DrawTexturedQuad(textureToRender.Id, width, height, screenTriangle, renderR, renderG, renderB, renderAlpha, keepAspectRatio, 1,
                    currentMipLevel);
            }

            glControl1.SwapBuffers();
        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            SetUpRendering();
        }

        private void BNTXEditor_Load(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void glControl2_Load(object sender, EventArgs e)
        {
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture();
            GLObjectManager.DeleteUnusedGLObjects();
        }

        private void renderChannelR_Click(object sender, EventArgs e)
        {
            renderR = !renderR;
            renderChannelR.ForeColor = renderR ? Color.Red : Color.DarkGray;

            // Uniforms need to be udpated.
            glControl1.Invalidate();
        }

        private void renderChannelG_Click(object sender, EventArgs e)
        {
            renderG = !renderG;
            renderChannelG.ForeColor = renderG ? Color.Green : Color.DarkGray;

            // Uniforms need to be udpated.
            glControl1.Invalidate();
        }

        private void renderChannelB_Click(object sender, EventArgs e)
        {
            renderB = !renderB;
            renderChannelB.ForeColor = renderB ? Color.Blue : Color.DarkGray;

            // Uniforms need to be udpated.
            glControl1.Invalidate();
        }

        private void renderChannelA_Click(object sender, EventArgs e)
        {
            renderAlpha = !renderAlpha;
            renderChannelA.ForeColor = renderAlpha ? Color.Black : Color.DarkGray;

            // Uniforms need to be udpated.
            glControl1.Invalidate();
        }

        private void mipLevelTrackBar_Scroll(object sender, EventArgs e)
        {
            currentMipLevel = mipLevelTrackBar.Value;
            glControl1.Invalidate();
        }

        private void glControl1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void previewBox_Resize(object sender, EventArgs e)
        {
            int size = Math.Min(previewGroupBox.Width, previewGroupBox.Height);
            glControl1.Width = size;
            glControl1.Height = size;
        }

        private void textureListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int itemindex = textureListBox.IndexFromPoint(e.Location);
                if (textureListBox.Items[itemindex] is BRTI)
                {
                    textureListBox.SelectedIndex = itemindex;
                    TextureMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
                }
            }
        }
        private void SetUpRendering()
        {
            // Make sure the shaders and textures are ready for rendering.
            OpenTKSharedResources.InitializeSharedResources();
            if (OpenTKSharedResources.SetupStatus == OpenTKSharedResources.SharedResourceStatus.Initialized)
            {
                BNTX.RefreshGlTexturesByName();
            }
        }
    }
}
