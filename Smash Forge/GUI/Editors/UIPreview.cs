using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using WeifenLuo.WinFormsUI.Docking;
using OpenTK.Graphics.OpenGL;
using SmashForge.Rendering;
using SmashForge.Rendering.Meshes;

namespace SmashForge
{
    public partial class UiPreview : DockContent
    {
        public string chr00Loc, chr11Loc, chr13Loc, stock90Loc;

        private Mesh3D screenTriangle;

        public UiPreview(NUT chr00, NUT chr11, NUT chr13, NUT stock90)
        {
            InitializeComponent();
            if (chr00 == null) chr00 = new NUT();
            if (chr11 == null) chr11 = new NUT();
            if (chr13 == null) chr13 = new NUT();
            if (stock90 == null) stock90 = new NUT();
            this.chr00 = chr00;
            this.chr11 = chr11;
            this.chr13 = chr13;
            this.stock90 = stock90;

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

        NUT chr00, chr11, chr13, stock90;

        private void LetsDance(object sender, EventArgs e)
        {
            Control c = MainForm.Instance.GetActiveModelViewport();

            if (!(c is ModelViewport)) return;
            ModelViewport view =(ModelViewport)c;
            view.currentMode = ModelViewport.Mode.Normal;

            NUT n = null;
            if (((MenuItem)sender).GetContextMenu().SourceControl == stock_90_renderer)
                n = stock90;
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_00_renderer)
                n = chr00;
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_11_renderer)
                n = chr11;
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_13_renderer)
                n = chr13;
            if (n == null) return;

            byte[] data = RenderTools.DXT5ScreenShot(view.glViewport, view.shootX, view.shootY, view.shootWidth, view.shootHeight);
            int id = n.Nodes.Count > 0 ? ((NutTexture)n.Nodes[0]).HashId : 0x280052B7;
            n.Nodes.Clear();
            n.glTexByHashId.Clear();

            NutTexture tex = new NutTexture();
            tex.Width = view.shootWidth;
            tex.Height = view.shootHeight;
            tex.surfaces.Add(new TextureSurface());
            tex.surfaces[0].mipmaps.Add(FlipDxt5(data, tex.Width, tex.Height));
            tex.pixelInternalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
            tex.HashId = id;
            n.Nodes.Add(tex);
            n.glTexByHashId.Add(tex.HashId, NUT.CreateTexture2D(tex));
            ((MenuItem)sender).GetContextMenu().SourceControl.Invalidate();

            if (((MenuItem)sender).GetContextMenu().SourceControl == stock_90_renderer)
            {
                if (stock90Loc != null)
                    stock90.Save(stock90Loc);
            }
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_00_renderer)
            {
                if (chr00Loc != null)
                    chr00.Save(chr00Loc);
            }
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_11_renderer)
            {
                if (chr11Loc != null)
                    chr11.Save(chr13Loc);
            }
            if (((MenuItem)sender).GetContextMenu().SourceControl == chr_13_renderer)
            {
                if (chr13Loc != null)
                    chr13.Save(chr13Loc);
            }
        }

        public byte[] FlipDxt5(byte[] i, int width, int height)
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
            RenderTexture(chr_00_renderer, chr00);
        }

        private void chr_11_renderer_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(chr_11_renderer, chr11);
        }

        private void chr_13_renderer_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture(chr_13_renderer, chr13);                                                           /*Grady, look!*/
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
                        Dds dds = new Dds(new FileData(filePath));
                        if(sender == chr_13_renderer)
                        {
                            chr13 = ReplaceTexture(dds.ToNutTexture(), 416, 416, chr13);
                            if (chr13Loc != null)
                                chr13.Save(chr13Loc);
                        }
                        if (sender == chr_00_renderer)
                        {
                            chr00 = ReplaceTexture(dds.ToNutTexture(), 128, 128, chr00);
                            if (chr00Loc != null)
                                chr00.Save(chr00Loc);
                        }
                        if (sender == chr_11_renderer)
                        {
                            chr11 = ReplaceTexture(dds.ToNutTexture(), 384, 384, chr13);
                            if (chr11Loc != null)
                                chr11.Save(chr11Loc);
                        }
                    }
                    if (filePath.ToLower().EndsWith(".png"))
                    {
                        if (sender == stock_90_renderer)
                        {
                            stock90 = ReplaceTexture(NutEditor.FromPng(filePath, 0), 64, 64, chr13);
                            if (stock90Loc != null)
                                stock90.Save(stock90Loc);
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
            RenderTexture(stock_90_renderer, stock90);
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
                ScreenDrawing.DrawTexturedQuad(nut.glTexByHashId[tex.HashId], tex.Width, tex.Height, screenTriangle, true, true, true, true, true);
            }

            glControl1.SwapBuffers();
        }
    }
}
