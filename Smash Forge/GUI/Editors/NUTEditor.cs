using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Threading;
using WeifenLuo.WinFormsUI.Docking;
using Smash_Forge.Rendering;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects;


namespace Smash_Forge
{
    public partial class NUTEditor : EditorBase
    {
        private NUT currentNut;
        private FileSystemWatcher fw;
        private Dictionary<NutTexture,string> fileFromTexture = new Dictionary<NutTexture, string>();
        private Dictionary<string, NutTexture> textureFromFile = new Dictionary<string, NutTexture>();

        // Rendering Stuff
        private Texture textureToRender = null;
        Framebuffer pngExportFramebuffer;
        private bool renderR = true;
        private bool renderG = true;
        private bool renderB = true;
        private bool renderAlpha = true;
        private bool keepAspectRatio = false;
        private int currentMipLevel = 0;

        private bool dontModify;

        ContextMenu TextureMenu = new ContextMenu();
        ContextMenu NUTMenu = new ContextMenu();

        public NUTEditor()
        {
            InitializeComponent();
            FilePath = "";
            Text = "New NUT";

            SetupFileSystemWatcher();
            SetupContextMenus();
        }

        private void SetupFileSystemWatcher()
        {
            fw = new FileSystemWatcher();
            fw.Path = Path.GetTempPath();
            fw.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            fw.EnableRaisingEvents = false;
            fw.Changed += new FileSystemEventHandler(OnChanged);
            fw.Filter = "";
        }

        private void SetupContextMenus()
        {
            // Texture Context Menu
            MenuItem replace = new MenuItem("Replace");
            replace.Click += replaceToolStripMenuItem_Click;
            TextureMenu.MenuItems.Add(replace);

            MenuItem export = new MenuItem("Export");
            export.Click += exportTextureToolStripMenuItem_Click;
            TextureMenu.MenuItems.Add(export);

            MenuItem remove = new MenuItem("Remove");
            remove.Click += RemoveToolStripMenuItem1_Click_1;
            TextureMenu.MenuItems.Add(remove);

            // NUT Context Menu
            MenuItem import = new MenuItem("Import New Texture");
            import.Click += importToolStripMenuItem_Click;
            NUTMenu.MenuItems.Add(import);

            MenuItem exportall = new MenuItem("Export to Folder");
            exportall.Click += exportNutToFolder;
            NUTMenu.MenuItems.Add(exportall);

            MenuItem exportAllPng = new MenuItem("Export to Folder as PNG");
            exportAllPng.Click += exportNutAsPngToolStripMenuItem_Click;
            NUTMenu.MenuItems.Add(exportAllPng);

            MenuItem exportAllPngAlpha = new MenuItem("Export to Folder as PNG (Separate Alpha)");
            exportAllPngAlpha.Click += exportNutAsPngSeparateAlphaToolStripMenuItem_Click;
            NUTMenu.MenuItems.Add(exportAllPngAlpha);

            MenuItem importall = new MenuItem("Import from Folder");
            importall.Click += importNutFromFolder;
            NUTMenu.MenuItems.Add(importall);

            MenuItem texid = new MenuItem("Set TEXID for NUT");
            texid.Click += texIDToolStripMenuItem_Click;
            NUTMenu.MenuItems.Add(texid);
        }

        public NUTEditor(NUT nut) : this()
        {
            SelectNUT(nut);
        }

        public NUTEditor(string filePath) : this()
        {
            NUT nut = new NUT(filePath);
            FilePath = filePath;
            Edited = false;
            SelectNUT(nut);
        }

        public override void Save()
        {
            if (FilePath.Equals(""))
            {
                SaveAs();
                return;
            }
            FileOutput fileOutput = new FileOutput();
            byte[] n = currentNut.Rebuild();

            fileOutput.writeBytes(n);
            fileOutput.save(FilePath);
            Edited = false;
        }

        public override void SaveAs()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Namco Universal Texture (.nut)|*.nut|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".nut") && currentNut != null)
                    {
                        FilePath = sfd.FileName;
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
            foreach (NutTexture tex in currentNut.Nodes)
            {
                textureListBox.Items.Add(tex);
            }
        }

        public void SelectNUT(NUT n)
        {
            currentNut = n;
            FillForm();
        }

        private void textureListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (textureListBox.SelectedIndex >= 0)
            {
                NutTexture tex = ((NutTexture)textureListBox.SelectedItem);
                if (tex == null)
                    return;

                // Render the selected texture.
                textureToRender = currentNut.glTexByHashId[tex.HASHID];

                SetGeneralAndDimensionsText(tex);

                if (tex.surfaces.Count == 6)
                {
                    SetCubeMapText(tex);
                    Rendering.RenderTools.dummyTextures[NUD.DummyTextures.StageMapHigh] = NUT.CreateTextureCubeMap(tex);
                }
                else
                {
                    SetMipMapText(tex);
                }
            }
            else
            {
                SetDefaultGeneralAndDimensionsText();
            }

            // Render on index changed rather than every frame.
            glControl1.Invalidate();
        }

        private void SetMipMapText(NutTexture tex)
        {
            // Display the total mip maps.
            mipmapGroupBox.Text = "Mipmaps";
            mipLevelLabel.Text = "Mip Level";
            mipLevelTrackBar.Maximum = tex.surfaces[0].mipmaps.Count - 1;
            minMipLevelLabel.Text = "1";
            maxMipLevelLabel.Text = "Total:" + tex.surfaces[0].mipmaps.Count + "";
        }

        private void SetCubeMapText(NutTexture tex)
        {
            // Display the current face instead of mip map information.
            mipmapGroupBox.Text = "Cube Map Faces";
            SetCurrentCubeMapFaceLabel(mipLevelTrackBar.Value);
            mipLevelTrackBar.Maximum = tex.surfaces.Count - 1;
            minMipLevelLabel.Text = "";
            maxMipLevelLabel.Text = "";
        }

        private void SetDefaultGeneralAndDimensionsText()
        {
            textureIdTB.Text = "";
            formatLabel.Text = "Format:";
            widthLabel.Text = "Width:";
            heightLabel.Text = "Height:";
        }

        private void SetGeneralAndDimensionsText(NutTexture tex)
        {
            textureIdTB.Text = tex.ToString();
            formatLabel.Text = "Format: " + (tex.pixelInternalFormat == PixelInternalFormat.Rgba ? "" + tex.pixelFormat : "" + tex.pixelInternalFormat);
            widthLabel.Text = "Width: " + tex.Width;
            heightLabel.Text = "Height:" + tex.Height;
        }

        private void SetCurrentCubeMapFaceLabel(int index)
        {
            switch (index)
            {
                case 0:
                    mipLevelLabel.Text = "Face: X+";
                    break;
                case 1:
                    mipLevelLabel.Text = "Face: X-";
                    break;
                case 2:
                    mipLevelLabel.Text = "Face: Y+";
                    break;
                case 3:
                    mipLevelLabel.Text = "Face: Y-";
                    break;
                case 4:
                    mipLevelLabel.Text = "Face: Z+";
                    break;
                case 5:
                    mipLevelLabel.Text = "Face: Z-";
                    break;
                default:
                    break;
            }
        }

        private void RenderTexture()
        {
            if (RenderTools.OpenTKStatus != RenderTools.OpenTKSetupStatus.Succeeded || glControl1 == null)
                return;

            if (!Runtime.shaders["Texture"].ProgramCreatedSuccessfully())
                return;

            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);

            if (textureListBox.SelectedItem == null)
            {
                glControl1.SwapBuffers();
                return;
            }

            int width = ((NutTexture)textureListBox.SelectedItem).Width;
            int height = ((NutTexture)textureListBox.SelectedItem).Height;

            // Draw the texture to the screen.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            Rendering.ScreenDrawing.DrawTexturedQuad(textureToRender.Id, width, height, renderR, renderG, renderB, renderAlpha, keepAspectRatio, 1,
                currentMipLevel);

            glControl1.SwapBuffers();
        }

        private void RenderTextureToPng(NutTexture nutTexture, string outputPath, bool r = true, bool g = true, bool b = true, bool a = false)
        {
            if (RenderTools.OpenTKStatus != RenderTools.OpenTKSetupStatus.Succeeded || glControl1 == null)
                return;

            // Load the OpenGL texture and export the image data.
            Texture2D texture = (Texture2D)currentNut.glTexByHashId[nutTexture.HASHID];
            using (Bitmap image = Rendering.TextureToBitmap.RenderBitmap(texture, r, g, b, a))
            {
                image.Save(outputPath);
            }
        }

        private void exportNutAsPngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderSelectDialog f = new FolderSelectDialog())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    if (!Directory.Exists(f.SelectedPath))
                        Directory.CreateDirectory(f.SelectedPath);

                    foreach (NutTexture texture in currentNut.Nodes)
                    {
                        string texId = texture.HASHID.ToString("X");
                        RenderTextureToPng(texture, f.SelectedPath + "\\" + texId + ".png", true, true, true, true);
                    }
                }
            }
        }

        private void exportNutAsPngSeparateAlphaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderSelectDialog f = new FolderSelectDialog())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    if (!Directory.Exists(f.SelectedPath))
                        Directory.CreateDirectory(f.SelectedPath);

                    foreach (NutTexture texture in currentNut.Nodes)
                    {
                        string texId = texture.HASHID.ToString("X");
                        RenderTextureToPng(texture, f.SelectedPath + "\\" + texId + "_rgb.png");
                        RenderTextureToPng(texture, f.SelectedPath + "\\" + texId + "_alpha.png", false, false, false, true);
                    }
                }
            }
        }

        private void exportTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNut == null || textureListBox.SelectedItem == null)
                return;
            using (var sfd = new SaveFileDialog())
            {
                NutTexture tex = (NutTexture)(textureListBox.SelectedItem);

                sfd.FileName = tex.ToString() + ".dds";
                sfd.Filter = "Supported Formats|*.dds;*.png|" +
                             "DirectDraw Surface (.dds)|*.dds|" +
                             "Portable Network Graphics (.png)|*.png|" +
                             "All files(*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string extension = Path.GetExtension(sfd.FileName).ToLowerInvariant();

                    if (extension == ".dds")
                    {
                        ExportDDS(sfd.FileName, tex);
                    }
                    else if (extension == ".png")
                    {
                        ExportPNG(sfd.FileName, tex);
                    }
                }
            }
        }

        private void ExportDDS(string filename, NutTexture tex)
        {
            DDS dds = new DDS();
            dds.FromNutTexture(tex);
            dds.Save(filename);
        }

        private void ExportPNG(string filename, NutTexture tex)
        {
            if (tex.surfaces[0].mipmaps.Count > 1)
                MessageBox.Show("Note: Textures exported as PNG do not preserve mipmaps.");

            switch (tex.pixelFormat)
            {
                case OpenTK.Graphics.OpenGL.PixelFormat.Rgba:
                    Pixel.fromRGBA(new FileData(tex.surfaces[0].mipmaps[0]), tex.Width, tex.Height).Save(filename);
                    break;
                case OpenTK.Graphics.OpenGL.PixelFormat.AbgrExt:
                    Pixel.fromABGR(new FileData(tex.surfaces[0].mipmaps[0]), tex.Width, tex.Height).Save(filename);
                    break;
                case OpenTK.Graphics.OpenGL.PixelFormat.Bgra:
                    Pixel.fromBGRA(new FileData(tex.surfaces[0].mipmaps[0]), tex.Width, tex.Height).Save(filename);
                    break;
                default:
                    RenderTextureToPng(tex, filename, true, true, true, true);
                    break;
            }
        }

        private void RemoveToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            if (textureListBox.SelectedIndex >= 0 && currentNut != null)
            {
                NutTexture tex = ((NutTexture)textureListBox.SelectedItem);
                //GL.DeleteTexture(NUT.glTexByHashId[tex.HASHID]);
                currentNut.glTexByHashId.Remove(tex.HASHID);
                currentNut.Nodes.Remove(tex);
                FillForm();
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNut == null) return;
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Formats|*.dds;*.png|" + 
                             "DirectDraw Surface (.dds)|*.dds|" +
                             "Portable Network Graphics (.png)|*.png|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    
                    int texId;
                    bool isTex = int.TryParse(Path.GetFileNameWithoutExtension(ofd.FileName), NumberStyles.HexNumber,
                        new CultureInfo("en-US"), out texId);

                    if (isTex)
                        foreach (NutTexture te in currentNut.Nodes)
                            if (texId == te.HASHID)
                                isTex = false;

                    NutTexture tex = null;
                    string extension = Path.GetExtension(ofd.FileName).ToLowerInvariant();
                    if (extension == ".dds")
                    {
                        DDS dds = new DDS(new FileData(ofd.FileName));
                        tex = dds.ToNutTexture();
                    }
                    else if (extension == ".png")
                    {
                        tex = fromPNG(ofd.FileName, 1);
                    }
                    else
                    {
                        return;
                    }

                    Edited = true;

                    if (isTex)
                        tex.HASHID = texId;
                    else
                        tex.HASHID = 0x40FFFF00 | (currentNut.Nodes.Count);

                    if (currentNut.glTexByHashId.ContainsKey(tex.HASHID))
                        currentNut.glTexByHashId.Remove(tex.HASHID);

                    currentNut.Nodes.Add(tex);
                    currentNut.glTexByHashId.Add(tex.HASHID, NUT.CreateTexture2D(tex));
                    FillForm();
                }
            }
        }

        public static NutTexture fromPNG(string fname, int mipcount)
        {
            Bitmap bmp = new Bitmap(fname);
            NutTexture tex = new NutTexture();

            tex.surfaces.Add(new TextureSurface());
            tex.surfaces[0].mipmaps.Add(fromPNG(bmp));
            for (int i = 1; i < mipcount; i++)
            {
                if (bmp.Width / (int)Math.Pow(2, i) < 4 || bmp.Height / (int)Math.Pow(2, i) < 4) break;
                tex.surfaces[0].mipmaps.Add(fromPNG(Pixel.ResizeImage(bmp, bmp.Width / (int)Math.Pow(2, i), bmp.Height / (int)Math.Pow(2, i))));
            }
            tex.Width = bmp.Width;
            tex.Height = bmp.Height;
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
            for(int i = 0; i < pix.Length; i += 4)
            {
                byte temp = pix[i];
                pix[i] = pix[i + 2];
                pix[i + 2] = temp;
            }

            return pix;
        }

        private void textureIdTB_TextChanged(object sender, EventArgs e)
        {
            if (textureListBox.SelectedItem != null && !textureIdTB.Text.Equals(""))
            {
                UpdateSelectedTexId();
            }
        }

        private void UpdateSelectedTexId()
        {
            int oldid = ((NutTexture)textureListBox.SelectedItem).HASHID;
            int newid = -1;
            int.TryParse(textureIdTB.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out newid);
            if (newid == -1)
                textureIdTB.Text = ((NutTexture)textureListBox.SelectedItem).HASHID.ToString("x");
            if (oldid != newid)
            {
                Edited = true;
                if (!currentNut.glTexByHashId.ContainsKey(newid))
                {
                    ((NutTexture)textureListBox.SelectedItem).HASHID = newid;
                    currentNut.glTexByHashId.Add(newid, currentNut.glTexByHashId[oldid]);
                    currentNut.glTexByHashId.Remove(oldid);
                }
                else
                {
                    textureIdTB.Text = (newid + 1).ToString("x");
                }
            }

            // Weird solution to refresh the listbox item
            textureListBox.DisplayMember = "test";
            textureListBox.DisplayMember = "";
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNut == null || textureListBox.SelectedItem == null)
                return;
            using (var ofd = new OpenFileDialog())
            {
                NutTexture texture = (NutTexture)(textureListBox.SelectedItem);

                ofd.Filter = "Supported Formats|*.dds;*.png|" + 
                             "DirectDraw Surface (.dds)|*.dds|" +
                             "Portable Network Graphics (.png)|*.png|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    NutTexture newTexture = null;
                    string extension = Path.GetExtension(ofd.FileName).ToLowerInvariant();
                    if (extension == ".dds")
                    {
                        DDS dds = new DDS(new FileData(ofd.FileName));
                        newTexture = dds.ToNutTexture();
                    }
                    else if (extension == ".png")
                    {
                        newTexture = fromPNG(ofd.FileName, 1);
                    }
                    else
                    {
                        return;
                    }

                    texture.Height = newTexture.Height;
                    texture.Width = newTexture.Width;
                    texture.pixelInternalFormat = newTexture.pixelInternalFormat;
                    texture.surfaces = newTexture.surfaces;
                    texture.pixelFormat = newTexture.pixelFormat;

                    Edited = true;
                    
                    //GL.DeleteTexture(NUT.glTexByHashId[texture.HASHID]);
                    currentNut.glTexByHashId.Remove(texture.HASHID);
                    currentNut.glTexByHashId.Add(texture.HASHID, NUT.CreateTexture2D(texture));

                    FillForm();
                }
            }
        }

        public static Process ShowOpenWithDialog(string path)
        {
            var args = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
            args += ",OpenAs_RunDLL " + path;
            return Process.Start("rundll32.exe", args);
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private void extractAndOpenInDefaultEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tempFileName;
            bool setupFileModifying = false;
            dontModify = true;
            fw.EnableRaisingEvents = true;
            if (!fileFromTexture.ContainsKey((NutTexture) (textureListBox.SelectedItem)))
            {
                tempFileName = Path.GetTempFileName();
                DeleteIfExists(Path.ChangeExtension(tempFileName, ".dds"));
                File.Move(tempFileName, Path.ChangeExtension(tempFileName, ".dds"));
                tempFileName = Path.ChangeExtension(tempFileName, ".dds");
                fileFromTexture.Add((NutTexture)textureListBox.SelectedItem, tempFileName);
                textureFromFile.Add(tempFileName, (NutTexture)textureListBox.SelectedItem);
                setupFileModifying = true;
            }
            else
            {
                tempFileName = fileFromTexture[(NutTexture) textureListBox.SelectedItem];
            }

            DDS dds = new DDS();
            dds.FromNutTexture((NutTexture)(textureListBox.SelectedItem));
            dds.Save(tempFileName);
            System.Diagnostics.Process.Start(tempFileName);
            if (setupFileModifying)
            {
                if (fw.Filter.Equals("*.*"))
                    fw.Filter = Path.GetFileName(tempFileName);
                else
                    fw.Filter += "|" + Path.GetFileName(tempFileName);
                Console.WriteLine(fw.Filter);
            }
            dontModify = false;
        }

        private void extractAndPickAProgramToEditWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tempFileName;
            bool setupFileModifying = false;
            dontModify = true;
            fw.EnableRaisingEvents = true;
            if (!fileFromTexture.ContainsKey((NutTexture)(textureListBox.SelectedItem)))
            {
                tempFileName = Path.GetTempFileName();
                DeleteIfExists(Path.ChangeExtension(tempFileName, ".dds"));
                File.Move(tempFileName, Path.ChangeExtension(tempFileName, ".dds"));
                tempFileName = Path.ChangeExtension(tempFileName, ".dds");
                fileFromTexture.Add((NutTexture)(textureListBox.SelectedItem), tempFileName);
                textureFromFile.Add(tempFileName, (NutTexture)textureListBox.SelectedItem);
                setupFileModifying = true;
            }
            else
            {
                tempFileName = fileFromTexture[(NutTexture)textureListBox.SelectedItem];
            }

            DDS dds = new DDS();
            dds.FromNutTexture((NutTexture)(textureListBox.SelectedItem));
            dds.Save(tempFileName);
            ShowOpenWithDialog(tempFileName);
            if (setupFileModifying)
            {
                if (fw.Filter.Equals("*.*"))
                    fw.Filter = Path.GetFileName(tempFileName);
                else
                    fw.Filter += "|" + Path.GetFileName(tempFileName);
                Console.WriteLine(fw.Filter);
            }

            dontModify = false;
        }

        private void importBack(string filename)
        {
            if (dontModify)
                return;
            
            NutTexture tex = textureFromFile[filename];

            try
            {
                DDS dds = new DDS(new FileData(filename));
                NutTexture ntex = dds.ToNutTexture();

                tex.Height = ntex.Height;
                tex.Width = ntex.Width;
                tex.pixelInternalFormat = ntex.pixelInternalFormat;
                tex.surfaces = ntex.surfaces;
                tex.pixelFormat = ntex.pixelFormat;

                //GL.DeleteTexture(NUT.glTexByHashId[tex.HASHID]);
                currentNut.glTexByHashId.Remove(tex.HASHID);
                currentNut.glTexByHashId.Add(tex.HASHID, NUT.CreateTexture2D(tex));

                FillForm();
                textureListBox.SelectedItem = tex;
                glControl1.Invalidate();
            }
            catch
            {
                Console.WriteLine("Could not be open for editing");
            }
        }

        private void importEditedFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importBack(fileFromTexture[(NutTexture)textureListBox.SelectedItem]);
        }

        private void exportNutToFolder(object sender, EventArgs e)
        {
            using (FolderSelectDialog f = new FolderSelectDialog())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    if (!Directory.Exists(f.SelectedPath))
                        Directory.CreateDirectory(f.SelectedPath);
                    foreach (NutTexture tex in currentNut.Nodes)
                    {
                        if(tex.pixelInternalFormat == PixelInternalFormat.Rgba)
                        {
                            string filename = Path.Combine(f.SelectedPath, $"{tex.HASHID.ToString("X")}.png");
                            ExportPNG(filename, tex);
                        }else
                        {
                            string filename = Path.Combine(f.SelectedPath, $"{tex.HASHID.ToString("X")}.dds");
                            DDS dds = new DDS();
                            dds.FromNutTexture(tex);
                            dds.Save(filename);
                        }
                    }

                    Process.Start("explorer.exe", f.SelectedPath);
                }
            }
        }

        private void importNutFromFolder(object sender, EventArgs e)
        {
            using (FolderSelectDialog f = new FolderSelectDialog())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    Edited = true;
                    if (!Directory.Exists(f.SelectedPath))
                        Directory.CreateDirectory(f.SelectedPath);
                    NUT nut;
                    nut = currentNut;

                    foreach (var texPath in Directory.GetFiles(f.SelectedPath))
                    {
                        string extension = Path.GetExtension(texPath).ToLowerInvariant();
                        if (!(extension == ".dds" || extension == ".png"))
                            continue;
                        int texId;
                        bool isTex = int.TryParse(Path.GetFileNameWithoutExtension(texPath), NumberStyles.HexNumber,
                            new CultureInfo("en-US"), out texId);

                        NutTexture texture = null;
                        if (isTex)
                            foreach (NutTexture tex in nut.Nodes)
                                if (tex.HASHID == texId)
                                    texture = tex;

                        if (texture == null)
                        {
                            //new texture
                            NutTexture tex = null;
                            if (extension == ".dds")
                            {
                                DDS dds = new DDS(new FileData(texPath));
                                tex = dds.ToNutTexture();
                            }
                            else if (extension == ".png")
                            {
                                tex = fromPNG(texPath, 1);
                            }

                            if (isTex)
                                tex.HASHID = texId;
                            else
                                tex.HASHID = nut.Nodes.Count;
                            nut.Nodes.Add(tex);
                            currentNut.glTexByHashId.Add(tex.HASHID, NUT.CreateTexture2D(tex));
                            FillForm();
                        }
                        else
                        {
                            //existing texture
                            NutTexture tex = texture;

                            NutTexture ntex = null;
                            if (extension == ".dds")
                            {
                                DDS dds = new DDS(new FileData(texPath));
                                ntex = dds.ToNutTexture();
                            }
                            else if (extension == ".png")
                            {
                                ntex = fromPNG(texPath, 1);
                            }

                            tex.Height = ntex.Height;
                            tex.Width = ntex.Width;
                            tex.pixelInternalFormat = ntex.pixelInternalFormat;
                            tex.surfaces = ntex.surfaces;
                            tex.pixelFormat = ntex.pixelFormat;

                            //GL.DeleteTexture(NUT.glTexByHashId[tex.HASHID]);
                            currentNut.glTexByHashId.Remove(tex.HASHID);
                            currentNut.glTexByHashId.Add(tex.HASHID, NUT.CreateTexture2D(tex));
                            FillForm();
                        }
                    }
                    if (!Runtime.TextureContainers.Contains(nut))
                        Runtime.TextureContainers.Add(nut);
                }
            }
            FillForm();
        }

        private void saveNUTZLIBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Namco Universal Texture (.nut)|*.nut|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".nut") && currentNut != null)
                    {
                        FileOutput o = new FileOutput();
                        o.writeBytes(FileData.DeflateZLIB(currentNut.Rebuild()));
                        o.save(sfd.FileName);
                    }
                }
            }
        }

        private void texIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentNut != null)
            {
                if (currentNut.Nodes.Count == 0)
                    return;

                using (var texIdSelector = new TexIdSelector())
                {
                    texIdSelector.Set(((NutTexture)currentNut.Nodes[0]).HASHID);
                    texIdSelector.ShowDialog();
                    if (texIdSelector.exitStatus == TexIdSelector.ExitStatus.Opened)
                    {
                        currentNut.ChangeTextureIds(texIdSelector.getNewTexId());
                        FillForm();
                        Edited = true;
                    }
                }
            }
        }

        private void renderChannelR_Click_1(object sender, EventArgs e)
        {
            renderR = !renderR;
            renderChannelR.ForeColor = renderR ? Color.Red : Color.DarkGray;

            glControl1.Invalidate();
        }

        private void renderChannelG_Click(object sender, EventArgs e)
        {
            renderG = !renderG;
            renderChannelG.ForeColor = renderG ? Color.Green : Color.DarkGray;

            glControl1.Invalidate();
        }

        private void renderChannelB_Click_1(object sender, EventArgs e)
        {
            renderB = !renderB;
            renderChannelB.ForeColor = renderB ? Color.Blue : Color.DarkGray;

            glControl1.Invalidate();            
        }

        private void renderChannelA_Click_1(object sender, EventArgs e)
        {
            renderAlpha = !renderAlpha;
            renderChannelA.ForeColor = renderAlpha ? Color.Black : Color.DarkGray;

            glControl1.Invalidate();           
        }

        private void glControl1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // toggle channel rendering
            if (e.KeyChar == 'r')
                renderChannelR.PerformClick();
            if (e.KeyChar == 'g')
                renderChannelG.PerformClick();
            if (e.KeyChar == 'b')
                renderChannelB.PerformClick();
            if (e.KeyChar == 'a')
                renderChannelA.PerformClick();
        }

        private void NUTEditor_Load(object sender, EventArgs e)
        {
            if (OpenTK.Graphics.GraphicsContext.CurrentContext != null)
            {
                // Make sure the shaders and textures are ready for rendering.
                RenderTools.SetUpOpenTkRendering();
                pngExportFramebuffer = new Framebuffer(FramebufferTarget.Framebuffer, glControl1.Width, glControl1.Height);
                currentNut.RefreshGlTexturesByHashId();
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            RenderTexture();
            GLObjectManager.DeleteUnusedGLObjects();
        }

        private void listBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int itemindex = textureListBox.IndexFromPoint(e.Location);
                if(itemindex == -1)
                {
                    NUTMenu.Show(this, new System.Drawing.Point(e.X + 15, e.Y));

                }
                else if (textureListBox.Items[itemindex] is NutTexture)
                {
                    textureListBox.SelectedIndex = itemindex;
                    TextureMenu.Show(this, new System.Drawing.Point(e.X + 15, e.Y));
                }
            }
        }

        private void previewBox_Resize(object sender, EventArgs e)
        {
            int padding = 25;
            int size = Math.Min(previewGroupBox.Width - padding, previewGroupBox.Height - padding);
            glControl1.Width = size;
            glControl1.Height = size;
        }

        private void mipLevelTrackBar_Scroll(object sender, EventArgs e)
        {

            NutTexture tex = ((NutTexture)textureListBox.SelectedItem);
            if (tex.surfaces.Count == 6)
            {
                // Create a new texture for the selected surface at the first mip level.
                currentMipLevel = 0;
                SetCurrentCubeMapFaceLabel(mipLevelTrackBar.Value);
                textureToRender = NUT.CreateTexture2D(tex, mipLevelTrackBar.Value);
            }
            else
            {
                // Regular texture.
                currentMipLevel = mipLevelTrackBar.Value;
            }

            glControl1.Invalidate();
        }

        private void preserveAspectRatioCB_CheckedChanged(object sender, EventArgs e)
        {
            keepAspectRatio = preserveAspectRatioCB.Checked;
            glControl1.Invalidate();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            // Update the display again.
            glControl1.Invalidate();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {

        }
    }
}
