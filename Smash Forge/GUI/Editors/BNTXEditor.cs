using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Framebuffers;
using SFGraphics.GLObjects.GLObjectManagement;
using SFGraphics.GLObjects.Textures;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SmashForge.Rendering;
using SmashForge.Rendering.Meshes;

namespace SmashForge
{
    public partial class BntxEditor : EditorBase
    {
        private BNTX bntx;
        private BRTI selectedTexture;

        private Texture textureToRender = null;
        Framebuffer pngExportFramebuffer;
        Mesh3D screenTriangle;

        private bool renderR = true;
        private bool renderG = true;
        private bool renderB = true;
        private bool renderAlpha = true;
        private bool keepAspectRatio = false;
        private int currentMipLevel = 0;

        private FileSystemWatcher fw;

        ContextMenu textureMenu = new ContextMenu();

        public BntxEditor()
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
            textureMenu.MenuItems.Add(export);

            MenuItem replace = new MenuItem("Replace");
            replace.Click += replaceTextureToolStripMenuItem_Click;
            textureMenu.MenuItems.Add(replace);

            OpenTkSharedResources.InitializeSharedResources();
            if (OpenTkSharedResources.SetupStatus == OpenTkSharedResources.SharedResourceStatus.Initialized)
            {
                screenTriangle = ScreenDrawing.CreateScreenTriangle();
            }
        }

        public void SelectBntx(BNTX b)
        {
            bntx = b;
            FillForm();
        }

        public BntxEditor(BNTX bntx) : this()
        {
            SelectBntx(bntx);
        }

        public BntxEditor(byte[] data) : this()
        {
            BNTX b = new BNTX();
            b.ReadBNTXFile(data);
            FilePath = "";
            OpenBntx(data);
            Edited = false;
            SelectBntx(b);
        }

        public override void Save()
        {
            if (FilePath.Equals(""))
            {
                SaveAs();
                return;
            }
            ExportBntx(FilePath);
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
            foreach (BRTI tex in bntx.textures)
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
        public void OpenBntx(byte[] fname)
        {
            Edited = true;

            foreach (BRTI tex in bntx.textures)
            {
                textureListBox.Items.Add(tex);
            }
        }

        public static BRTI.BRTI_Texture FromPng(string filename, int mipcount)
        {
            Bitmap bmp = new Bitmap(filename);
            BRTI.BRTI_Texture tex = new BRTI.BRTI_Texture();

            tex.mipmaps.Add(FromPng(bmp));
            for (int i = 0; i < mipcount; i++)
            {
                if (bmp.Width / (int)Math.Pow(2, i) < 4 || bmp.Height / (int)Math.Pow(2, i) < 4) break;
                tex.mipmaps.Add(FromPng(Pixel.ResizeImage(bmp, bmp.Width / (int)Math.Pow(2, i), bmp.Height / (int)Math.Pow(2, i))));
                tex.mipmaps[0] = tex.mipmaps[0];
            }
            tex.width = bmp.Width;
            tex.height = bmp.Height;
            tex.pixelInternalFormat = PixelInternalFormat.Rgba;
            tex.pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;

            return tex;
        }

        private static byte[] FromPng(Bitmap bmp)
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

        public void ExportBntx(string filename)
        {
            FileOutput o = new FileOutput();
            o.endian = Endianness.Little;

            o.Save(filename);
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
                        newTexture = FromPng(ofd.FileName, 1);
                    }
                    if (Path.GetExtension(ofd.FileName).ToLower().Equals(".dds"))
                    {
                        Dds dds = new Dds(new FileData(ofd.FileName));
                        newTexture = dds.ToBrtiTexture();
                    }

                    t.texture.height = newTexture.height;
                    t.texture.width = newTexture.width;
                    t.texture.pixelInternalFormat = newTexture.pixelInternalFormat;
                    t.texture.mipmaps = newTexture.mipmaps;
                    t.texture.pixelFormat = newTexture.pixelFormat;
                    newTexture.mipmaps.Add(t.texture.mipmaps[0]);

                    bntx.glTexByName.Add(ofd.FileName, BRTI.CreateTexture2D(t.texture));

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
                    ExportBntx(save.FileName);
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
                selectedTexture = ((BRTI)textureListBox.SelectedItem);


                label1.Text = "Width: " + selectedTexture.texture.width;
                label2.Text = "Height: " + selectedTexture.texture.height;

                uint format = selectedTexture.format >> 8;
                byte dataType = (byte)(selectedTexture.format & 0xFF);

                Formats.BNTXImageFormat f = (Formats.BNTXImageFormat)format;
                Formats.BNTXImageTypes t = (Formats.BNTXImageTypes)dataType;

                label3.Text = f.ToString() + " " + t.ToString();

                // Render the selected texture.
                if (bntx.glTexByName.ContainsKey(selectedTexture.Text))
                    textureToRender = bntx.glTexByName[selectedTexture.Text];
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
            if (OpenTkSharedResources.SetupStatus != OpenTkSharedResources.SharedResourceStatus.Initialized || glControl1 == null)
                return;

            if (!OpenTkSharedResources.shaders["Texture"].LinkStatusIsOk)
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

            Texture texture = ((BRTI)textureListBox.SelectedItem).display;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            if (textureToRender != null)
            {
                ScreenDrawing.DrawTexturedQuad(textureToRender, width, height, screenTriangle, renderR, renderG, renderB, renderAlpha, keepAspectRatio, 1,
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

            int width = Math.Max(1, selectedTexture.texture.width >> currentMipLevel);
            int height = Math.Max(1, selectedTexture.texture.height >> currentMipLevel);

            label1.Text = "Width: " + width;
            label2.Text = "Height: " + height;

            glControl1.Invalidate();
        }

        private void glControl1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void previewBox_Resize(object sender, EventArgs e)
        {
            int padding = 25;
            int size = Math.Min(previewGroupBox.Width - padding, previewGroupBox.Height - padding);
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
                    textureMenu.Show(this, new System.Drawing.Point(e.X, e.Y));
                }
            }
        }
        private void SetUpRendering()
        {
            // Make sure the shaders and textures are ready for rendering.
            OpenTkSharedResources.InitializeSharedResources();
            if (OpenTkSharedResources.SetupStatus == OpenTkSharedResources.SharedResourceStatus.Initialized)
            {
                bntx.RefreshGlTexturesByName();
                pngExportFramebuffer = new Framebuffer(FramebufferTarget.Framebuffer, glControl1.Width, glControl1.Height);
                screenTriangle = ScreenDrawing.CreateScreenTriangle();
            }
        }
    }
}
