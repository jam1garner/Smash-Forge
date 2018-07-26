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
using WeifenLuo.WinFormsUI.Docking;
using OpenTK.Graphics.OpenGL;
using Smash_Forge.Rendering;

namespace Smash_Forge
{
    public partial class UIPreview : DockContent
    {
        public string chr_00_loc, chr_11_loc, chr_13_loc, stock_90_loc;

        public UIPreview(NUT chr_00, NUT chr_11, NUT chr_13, NUT stock_90)
        {
            InitializeComponent();
            if (chr_00 == null) chr_00 = new NUT();
            if (chr_11 == null) chr_11 = new NUT();
            if (chr_13 == null) chr_13 = new NUT();
            if (stock_90 == null) stock_90 = new NUT();
            this.chr_00 = chr_00;
            this.chr_11 = chr_11;
            this.chr_13 = chr_13;
            this.stock_90 = stock_90;

            ContextMenu cm = new ContextMenu();
            MenuItem snapShot = new MenuItem("Begin Snapshot");
            snapShot.Click += SnapShotMode;
            cm.MenuItems.Add(snapShot);
            MenuItem snapShot2 = new MenuItem("Snapshot");
            snapShot2.Click += LetsDance;
            cm.MenuItems.Add(snapShot2);
            chr_00_renderer.ContextMenu = cm;
            chr_11_renderer.ContextMenu = cm;
            chr_13_renderer.ContextMenu = cm;

            stock_90_renderer.AllowDrop = true;
            chr_00_renderer.AllowDrop = true;
            chr_11_renderer.AllowDrop = true;
            chr_13_renderer.AllowDrop = true;
            //stock_90_renderer.ContextMenu = cm;
        }

        NUT chr_00, chr_11, chr_13, stock_90;

        private void LetsDance(object sender, EventArgs e)
        {
            Control c = MainForm.Instance.GetActiveModelViewport();

            if (!(c is ModelViewport)) return;
            ModelViewport view =(ModelViewport)c;
            view.currentMode = ModelViewport.Mode.Normal;

            NUT n = null;
            if (((MenuItem)sender).GetContextMenu().SourceControl == stock_90_renderer)
                n = stock_90;
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_00_renderer)
                n = chr_00;
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_11_renderer)
                n = chr_11;
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_13_renderer)
                n = chr_13;
            if (n == null) return;

            byte[] data = RenderTools.DXT5ScreenShot(view.glViewport, view.shootX, view.shootY, view.shootWidth, view.shootHeight);
            int id = n.Nodes.Count > 0 ? ((NutTexture)n.Nodes[0]).HashId : 0x280052B7;
            n.Nodes.Clear();
            n.glTexByHashId.Clear();

            NutTexture tex = new NutTexture();
            tex.Width = view.shootWidth;
            tex.Height = view.shootHeight;
            tex.surfaces.Add(new TextureSurface());
            tex.surfaces[0].mipmaps.Add(FlipDXT5(data, tex.Width, tex.Height));
            tex.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
            tex.HashId = id;
            n.Nodes.Add(tex);
            n.glTexByHashId.Add(tex.HashId, NUT.CreateTexture2D(tex));
            ((MenuItem)sender).GetContextMenu().SourceControl.Invalidate();

            if (((MenuItem)sender).GetContextMenu().SourceControl == stock_90_renderer)
            {
                if (stock_90_loc != null)
                    stock_90.Save(stock_90_loc);
            }
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_00_renderer)
            {
                if (chr_00_loc != null)
                    chr_00.Save(chr_00_loc);
            }
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_11_renderer)
            {
                if (chr_11_loc != null)
                    chr_11.Save(chr_13_loc);
            }
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_13_renderer)
            {
                if (chr_13_loc != null)
                    chr_13.Save(chr_13_loc);
            }
        }

        public byte[] FlipDXT5(byte[] i, int width, int height)
        {
            // oh dear lord why god flipping dxt5 alpha channel

            byte[] o = new byte[i.Length];

            for (int h = 0; h < height / 4; h++)
                for (int w = 0; w < width / 4; w++)
                {
                    for (int b = 0; b < 16; b++)
                    {
                        o[(w + (height / 4 - h - 1) * (width / 4)) * 16 + b] = i[(w + h * (width / 4)) * 16 + b];
                    }
                    o[(w + (height / 4 - h - 1) * (width / 4)) * 16 + 12] = (i[(w + h * (width / 4)) * 16 + 15]);
                    o[(w + (height / 4 - h - 1) * (width / 4)) * 16 + 13] = (i[(w + h * (width / 4)) * 16 + 14]);
                    o[(w + (height / 4 - h - 1) * (width / 4)) * 16 + 14] = (i[(w + h * (width / 4)) * 16 + 13]);
                    o[(w + (height / 4 - h - 1) * (width / 4)) * 16 + 15] = (i[(w + h * (width / 4)) * 16 + 12]);

                    long block = (((long)i[(w + h * (width / 4)) * 16 + 2]) << 40) |
                        (((long)i[(w + h * (width / 4)) * 16 + 3] & 0xFF) << 32) |
/*I'm not bored, I'm hungry*/(((long)i[(w + h * (width / 4)) * 16 + 4] & 0xFF) << 24) |
                        (((long)i[(w + h * (width / 4)) * 16 + 5] & 0xFF) << 16) |
                        (((long)i[(w + h * (width / 4)) * 16 + 6] & 0xFF) << 8) |
                        (((long)i[(w + h * (width / 4)) * 16 + 7] & 0xFF));


                    int row12 = ((i[(w + h * (width / 4)) * 16 + 7] << 16) & 0x00ff0000) |
((i[(w + h * (width / 4)) * 16 + 6] << 8) & 0x0000ff00) |
((i[(w + h * (width / 4)) * 16 + /*just get the rows 3 bits per pixel*/ 5]) & 0x000000ff);
                    int row34 = ((i[(w + h * (width / 4)) * 16 + 4] << 16) & 0x00ff0000) |
((i[(w + h * (width / 4)) * 16 + 3] << 8) & 0x0000ff00) |
((i[(w + h * (width / 4)) * 16 + 2]) & 0x000000ff);

                    row12 = ((row12 & 0x00000fff) << 12) | ((row12 & 0x00fff000) >> 12);
                    row34 = (/*reorder da nibbles*/(row34 & 0x00000fff) << 12) | ((row34 & 0x00fff000) >> 12);

                    o[(w + (height / 4 - h - 1) * (width / 4)) * 16 + 7] = (byte)((row34 & 0x00ff0000) >> 16);
                    o[(w + (height / 4 - h - 1) * (width / 4)) * 16 + 6] = (byte)((row34 & 0x0000ff00) >> 8);
                    o[(w + (height / 4 - h - 1) * (width / 4)) * 16 + 5] = (byte)((row34 & 0x000000ff));
                    o[(w + (height / 4 - h - 1) * (width / 4)) * 16 + 4] = (byte)((row12 & 0x00ff0000) >> 16);
                    o[(w + (height / 4 - h - 1) * (width / 4)) * 16 + 3] = (byte)((row12 & 0x0000ff00) >> 8);
                    o[(w + (height / 4 - h - 1) * (width / 4)) * 16 + 2] = (byte)((row12 & 0x000000ff));
                }

            return o;
        }

        private void SnapShotMode(object sender, EventArgs e)
        {
            Control c = MainForm.Instance.GetActiveModelViewport();
            
            if (!(c is ModelViewport)) return;
            ModelViewport view = (ModelViewport)c;
            view.currentMode = ModelViewport.Mode.Photoshoot;
            //view.HideAll();

            if (((MenuItem)sender).GetContextMenu().SourceControl == stock_90_renderer)
            {
                view.shootWidth = 64;
                view.shootHeight = 64;
            }
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_00_renderer)
            {
                view.shootWidth = 128;
                view.shootHeight = 128;
            }
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_11_renderer)
            {
                view.shootWidth = 384;
                view.shootHeight = 384;
            }
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_13_renderer)
            {
                view.shootWidth = 416;
                view.shootHeight = 416;
            }
            Runtime.renderFloor = false;
            Runtime.renderBackGround = false;
            Runtime.renderBones = false;
        }

        private void chr_00_renderer_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(chr_00_renderer, chr_00);
        }

        private void chr_11_renderer_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(chr_11_renderer, chr_11);
        }

        private void chr_13_renderer_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(chr_13_renderer, chr_13);                                                           /*Grady, look!*/
        }

        private void PSButton_Click(object sender, EventArgs e)
        {

        }

        private void chr_13_renderer_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string filePath in files)
                {
                    if (filePath.ToLower().EndsWith(".dds"))
                    {
                        DDS dds = new DDS(new FileData(filePath));
                        if(sender == chr_13_renderer)
                        {
                            chr_13 = ReplaceTexture(dds.ToNutTexture(), 416, 416, chr_13);
                            if (chr_13_loc != null)
                                chr_13.Save(chr_13_loc);
                        }
                        if (sender == chr_00_renderer)
                        {
                            chr_00 = ReplaceTexture(dds.ToNutTexture(), 128, 128, chr_00);
                            if (chr_00_loc != null)
                                chr_00.Save(chr_00_loc);
                        }
                        if (sender == chr_11_renderer)
                        {
                            chr_11 = ReplaceTexture(dds.ToNutTexture(), 384, 384, chr_13);
                            if (chr_11_loc != null)
                                chr_11.Save(chr_11_loc);
                        }
                    }
                    if (filePath.ToLower().EndsWith(".png"))
                    {
                        if (sender == stock_90_renderer)
                        {
                            stock_90 = ReplaceTexture(NUTEditor.fromPNG(filePath, 0), 64, 64, chr_13);
                            if (stock_90_loc != null)
                                stock_90.Save(stock_90_loc);
                        }
                    }
                }
                ((GLControl)sender).Invalidate();
            }
        }

        private NUT ReplaceTexture(NutTexture tex, int width, int height, NUT nut)
        {
            if (tex.Width == width && tex.Height == height)
            {
                tex.HashId = 0x280052B7;
                if (nut != null && nut.Nodes.Count > 0)
                {
                    tex.HashId = ((NutTexture)nut.Nodes[0]).HashId;
                }
                if(nut == null)
                    nut = new NUT();
                nut.Nodes.Clear();
                nut.glTexByHashId.Clear();
                nut.Nodes.Add(tex);
                nut.glTexByHashId.Add(tex.HashId, NUT.CreateTexture2D(tex));
            }
            else
            {
                MessageBox.Show("Dimensions must be "+width+"x"+height);
            }
            return nut;
        }

        private void Drag_Enter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void UIPreview_Load(object sender, EventArgs e)
        {
        }

        private void stock_90_renderer_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(stock_90_renderer, stock_90);
        }
        
        private void RenderTexture(GLControl glControl1, NUT nut)
        {
            if (nut == null) return;
            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);

            foreach(NutTexture tex in nut.Nodes)
            {
                ScreenDrawing.DrawTexturedQuad(nut.glTexByHashId[tex.HashId].Id, tex.Width, tex.Height, true, true, true, true, true);
            }

            glControl1.SwapBuffers();
        }
    }
}
