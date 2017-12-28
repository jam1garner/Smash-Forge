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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Smash_Forge
{
    public partial class NUTEditor : DockContent
    {
        private NUT selected;
        private FileSystemWatcher fw;
        private Dictionary<NUT_Texture,string> fileFromTexture = new Dictionary<NUT_Texture, string>();
        private Dictionary<string,NUT_Texture> textureFromFile = new Dictionary<string, NUT_Texture>();
        private bool dontModify;
        private bool renderR = true;
        private bool renderG = true;
        private bool renderB = true;
        private bool renderAlpha = true;
        private bool preserveAspectRatio = false;

        public NUTEditor()
        {
            InitializeComponent();
            FillForm();
            if (Runtime.TextureContainers.Count > 0)
                listBox1.SelectedIndex = 0;

            fw = new FileSystemWatcher();
            fw.Path = Path.GetTempPath();
            fw.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            fw.EnableRaisingEvents = false;
            fw.Changed += new FileSystemEventHandler(OnChanged);
            fw.Filter = "";
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("File modified!");
            string filename = e.FullPath;
            //Thread.Sleep(1000);
            //importBack(filename);
        }

        public void FillForm()
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            foreach (NUT n in Runtime.TextureContainers)
            {
                listBox1.Items.Add(n);
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex >= 0)
            {
                SelectNUT((NUT)listBox1.SelectedItem);
            }
        }

        private void SelectNUT(NUT n)
        {
            selected = n;
            
            listBox2.Items.Clear();
            foreach (NUT_Texture tex in n.Nodes)
            {
                listBox2.Items.Add(tex);
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex >= 0)
            {
                NUT_Texture tex = ((NUT_Texture)listBox2.SelectedItem);
                textBox1.Text = tex.ToString();
                label2.Text = "Format: " + (tex.type == PixelInternalFormat.Rgba ? "" + tex.utype : "" + tex.type);
                label3.Text = "Width: " + tex.Width;
                label4.Text = "Height:" + tex.Height;
                label5.Text = "Mipmap:" + tex.mipmaps.Count;
                RenderTexture();
            }
            else
            {
                textBox1.Text = "";
                label2.Text = "Format:";
                label3.Text = "Width:";
                label4.Text = "Height:";
                label5.Text = "Mipmap:";
            }
        }

        private void RenderTexture()
        {
            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.ClearColor(Color.White);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha,BlendingFactorDest.OneMinusSrcAlpha);

            if (listBox1.SelectedItem == null || listBox2.SelectedItem == null)
                return;

            int width = ((NUT_Texture)listBox2.SelectedItem).Width;
            int height = ((NUT_Texture)listBox2.SelectedItem).Height; 

            int texture = ((NUT)listBox1.SelectedItem).draw[((NUT_Texture)listBox2.SelectedItem).HASHID];
            bool alphaOverride = renderAlpha && !renderR && !renderG && !renderB;
            RenderTools.DrawTexturedQuad(texture, width, height, renderR, renderG, renderB, renderAlpha, alphaOverride, preserveAspectRatio);

            glControl1.Height = glControl1.Width;

            glControl1.SwapBuffers();

            if (!Runtime.hasCheckedTexShaderCompilation)
            {
                Runtime.shaders["Texture"].displayCompilationWarning("Texture");
                Runtime.hasCheckedTexShaderCompilation = true;
            }
        }

        private void openNUTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Namco Universal Texture (.nut)|*.nut|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith(".nut"))
                    {
                        Runtime.TextureContainers.Add(new NUT(ofd.FileName));
                        FillForm();
                    }
                }
            }
        }

        private void saveNUTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Namco Universal Texture (.nut)|*.nut|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".nut") && selected != null)
                    {
                        selected.Save(sfd.FileName);
                    }
                }
            }
        }

        private void newNUTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NUT n = new NUT();
            Runtime.TextureContainers.Add(n);
            FillForm();
        }

        private void RemoveToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex >= 0 && selected != null)
            {
                NUT_Texture tex = ((NUT_Texture)listBox2.SelectedItem);
                GL.DeleteTexture(selected.draw[tex.HASHID]);
                selected.draw.Remove(tex.HASHID);
                selected.Nodes.Remove(tex);
                FillForm();
                listBox1.SelectedItem = selected;
                listBox2.SelectedIndex = 0;
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selected != null)
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Supported Formats|*.dds;*.png|" + 
                                 "Direct Draw Surface (.dds)|*.dds|" +
                                 "Portable Networks Graphic (.png)|*.png|" +
                                 "All files(*.*)|*.*";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        int texId;
                        bool isTex = int.TryParse(Path.GetFileNameWithoutExtension(ofd.FileName), NumberStyles.HexNumber,
                            new CultureInfo("en-US"), out texId);

                        if (isTex)
                            foreach (NUT_Texture te in selected.Nodes)
                                if (texId == te.HASHID)
                                    isTex = false;

                        NUT_Texture tex = null;

                        if (ofd.FileName.EndsWith(".dds") && selected != null)
                        {
                            DDS dds = new DDS(new FileData(ofd.FileName));
                            tex = dds.toNUT_Texture();
                        }
                        if (ofd.FileName.EndsWith(".png") && selected != null)
                        {
                            tex = fromPNG(ofd.FileName, 1);
                        }

                        if (tex != null)
                        {
                            if (isTex)
                                tex.HASHID = texId;
                            else
                                tex.HASHID = 0x40FFFF00 | (selected.Nodes.Count);
                            selected.Nodes.Add(tex);
                            selected.draw.Add(tex.HASHID, NUT.loadImage(tex));
                            FillForm();
                            listBox1.SelectedItem = selected;
                        }
                    }
                }
        }


        public static NUT_Texture fromPNG(string fname, int mipcount)
        {
            Bitmap bmp = new Bitmap(fname);
            NUT_Texture tex = new NUT_Texture();

            tex.mipmaps.Add(fromPNG(bmp));
            for (int i = 1; i < mipcount; i++)
            {
                if(bmp.Width / (int)Math.Pow(2, i) < 4 || bmp.Height / (int)Math.Pow(2, i) < 4) break;
                tex.mipmaps.Add(fromPNG(Pixel.ResizeImage(bmp, bmp.Width / (int)Math.Pow(2, i), bmp.Height / (int)Math.Pow(2, i))));
            }
            tex.Width = bmp.Width;
            tex.Height = bmp.Height;
            tex.type = PixelInternalFormat.Rgba;
            tex.utype = OpenTK.Graphics.OpenGL.PixelFormat.Rgba;

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

        private void exportAsDDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem == null) return;
            using (var sfd = new SaveFileDialog())
            {
                NUT_Texture tex = (NUT_Texture)(listBox2.SelectedItem);
                if (tex.type == PixelInternalFormat.Rgba)
                    sfd.Filter = "Portable Networks Graphic (.png)|*.png|" +
                                 "All files(*.*)|*.*";
                else
                    sfd.Filter = "Direct Draw Surface (.dds)|*.dds|" +
                                 "All files(*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // use png instead
                    if (sfd.FileName.EndsWith(".png") && selected != null && tex.type == PixelInternalFormat.Rgba)
                    {
                        ExportPNG(sfd.FileName, tex);
                    }
                    if (sfd.FileName.EndsWith(".dds") && selected != null)
                    {
                        DDS dds = new DDS();
                        dds.fromNUT_Texture(tex);
                        dds.Save(sfd.FileName);
                    }
                }
            }
        }

        private void ExportPNG(string fname, NUT_Texture tex)
        {
            if (tex.mipmaps.Count > 1)
                MessageBox.Show("RGBA texture exported as PNG do not preserve mipmaps");

            switch (tex.utype)
            {
                case OpenTK.Graphics.OpenGL.PixelFormat.Rgba:
                    Pixel.fromRGBA(new FileData(tex.mipmaps[0]), tex.Width, tex.Height).Save(fname);
                    break;
                case OpenTK.Graphics.OpenGL.PixelFormat.AbgrExt:
                    Pixel.fromABGR(new FileData(tex.mipmaps[0]), tex.Width, tex.Height).Save(fname);
                    break;
                case OpenTK.Graphics.OpenGL.PixelFormat.Bgra:
                    Pixel.fromBGRA(new FileData(tex.mipmaps[0]), tex.Width, tex.Height).Save(fname);
                    break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if(listBox2.SelectedItem != null && !textBox1.Text.Equals(""))
            {
                int oldid = ((NUT_Texture)listBox2.SelectedItem).HASHID;
                int newid = -1;
                int.TryParse(textBox1.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out newid);
                if (newid == -1)
                    textBox1.Text = ((NUT_Texture)listBox2.SelectedItem).HASHID.ToString("x");
                if(oldid!=newid)
                {
                    if (!selected.draw.ContainsKey(newid))
                    {
                        ((NUT_Texture)listBox2.SelectedItem).HASHID = newid;
                        selected.draw.Add(newid, selected.draw[oldid]);
                        selected.draw.Remove(oldid);
                    }
                    else
                    {
                        textBox1.Text = (newid + 1).ToString("x") + "";
                    }
                }

                // weird solution to refresh the listbox item
                listBox2.DisplayMember = "test";
                listBox2.DisplayMember = "";
            }
        }

        private void NUTEditor_Resize(object sender, EventArgs e)
        {

            RenderTexture();
            
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                NUT_Texture tex = (NUT_Texture)(listBox2.SelectedItem);

                if (tex.type == PixelInternalFormat.Rgba)
                    ofd.Filter = "Portable Networks Graphic (.png)|*.png|" +
                                 "All files(*.*)|*.*";
                else
                    ofd.Filter = "Direct Draw Surface (.dds)|*.dds|" +
                                 "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    NUT_Texture ntex = null;
                    if (ofd.FileName.EndsWith(".dds") && selected != null)
                    {
                        DDS dds = new DDS(new FileData(ofd.FileName));
                        ntex = dds.toNUT_Texture();
                    }

                    if (ofd.FileName.EndsWith(".png") && selected != null)
                        ntex = fromPNG(ofd.FileName, 1);
                    
                    tex.Height = ntex.Height;
                    tex.Width = ntex.Width;
                    tex.type = ntex.type;
                    tex.mipmaps = ntex.mipmaps;
                    tex.utype = ntex.utype;

                    if (ntex == null)
                        return;
                    
                    GL.DeleteTexture(selected.draw[tex.HASHID]);
                    selected.draw.Remove(tex.HASHID);
                    selected.draw.Add(tex.HASHID, NUT.loadImage(tex));

                    FillForm();
                    listBox1.SelectedItem = selected;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Clear all NUTs from the list? You'll lose any unsaved work!", "Clear all NUTs?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                listBox1.Items.Clear();
                listBox2.Items.Clear();
                Runtime.TextureContainers.Clear();
                ListBox.ObjectCollection items = listBox1.Items;
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    Runtime.TextureContainers.RemoveAt(i);
                    Runtime.TextureContainers.ElementAt(i).Destroy();
                }

            }
            else if (dialogResult == DialogResult.No)
            {
                //do nothing. Alternatively the else if statement can be removed.
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //if (dontAskBeforeRemovingNUTsToolStripMenuItem.Checked == false)
            //{
                DialogResult dialogResult = MessageBox.Show("Remove this NUT from the list?\nHint: Options -> Don't ask before removing NUTs", "Remove NUTs?", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
            //}
            deleteSelectedNUTs();


        }

        //TODO: It doesn't work.
        private void KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                deleteSelectedNUTs();
            }
        }
        
        private void dontAskBeforeRemovingNUTsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*if (!dontAskBeforeRemovingNUTsToolStripMenuItem.Checked)
                dontAskBeforeRemovingNUTsToolStripMenuItem.Checked = true;
            else
                dontAskBeforeRemovingNUTsToolStripMenuItem.Checked = false;*/
        }
        /// <summary>
        /// Deletes all selected NUTs.
        /// Although the function can delete multiple NUTs, the rest of the application has not been updated to support selecting more than one at once...
        /// </summary>
        private void deleteSelectedNUTs()
        {
            listBox2.Items.Clear();
            while (listBox1.SelectedItems.Count > 0)
            {
                listBox1.Items.Remove(listBox1.SelectedItems[0]);
            }
            foreach (NUT nut in listBox1.SelectedItems)
            {
                nut.Destroy();
                Runtime.TextureContainers.Remove(nut);
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
            if(File.Exists(path))
                File.Delete(path);
        }

        private void extractAndOpenInDefaultEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tempFileName;
            bool setupFileModifying = false;
            dontModify = true;
            fw.EnableRaisingEvents = true;
            if (!fileFromTexture.ContainsKey((NUT_Texture) (listBox2.SelectedItem)))
            {
                tempFileName = Path.GetTempFileName();
                DeleteIfExists(Path.ChangeExtension(tempFileName, ".dds"));
                File.Move(tempFileName, Path.ChangeExtension(tempFileName, ".dds"));
                tempFileName = Path.ChangeExtension(tempFileName, ".dds");
                fileFromTexture.Add((NUT_Texture)listBox2.SelectedItem, tempFileName);
                textureFromFile.Add(tempFileName, (NUT_Texture)listBox2.SelectedItem);
                setupFileModifying = true;
            }
            else
            {
                tempFileName = fileFromTexture[(NUT_Texture) listBox2.SelectedItem];
            }

            DDS dds = new DDS();
            dds.fromNUT_Texture((NUT_Texture)(listBox2.SelectedItem));
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
            if (!fileFromTexture.ContainsKey((NUT_Texture)(listBox2.SelectedItem)))
            {
                tempFileName = Path.GetTempFileName();
                DeleteIfExists(Path.ChangeExtension(tempFileName, ".dds"));
                File.Move(tempFileName, Path.ChangeExtension(tempFileName, ".dds"));
                tempFileName = Path.ChangeExtension(tempFileName, ".dds");
                fileFromTexture.Add((NUT_Texture)(listBox2.SelectedItem), tempFileName);
                textureFromFile.Add(tempFileName, (NUT_Texture)listBox2.SelectedItem);
                setupFileModifying = true;
            }
            else
            {
                tempFileName = fileFromTexture[(NUT_Texture)listBox2.SelectedItem];
            }

            DDS dds = new DDS();
            dds.fromNUT_Texture((NUT_Texture)(listBox2.SelectedItem));
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
            
            NUT_Texture tex = textureFromFile[filename];

            try
            {
                DDS dds = new DDS(new FileData(filename));
                NUT_Texture ntex = dds.toNUT_Texture();

                tex.Height = ntex.Height;
                tex.Width = ntex.Width;
                tex.type = ntex.type;
                tex.mipmaps = ntex.mipmaps;
                tex.utype = ntex.utype;

                GL.DeleteTexture(selected.draw[tex.HASHID]);
                selected.draw.Remove(tex.HASHID);
                selected.draw.Add(tex.HASHID, NUT.loadImage(tex));

                FillForm();
                listBox1.SelectedItem = selected;
                listBox2.SelectedItem = tex;
                RenderTexture();
            }
            catch
            {
                Console.WriteLine("Could not be open for editing");
            }
        }

        private void importEditedFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importBack(fileFromTexture[(NUT_Texture)listBox2.SelectedItem]);
        }

        private void exportNutToFolder(object sender, EventArgs e)
        {
            using (FolderSelectDialog f = new FolderSelectDialog())
            {
                if (f.ShowDialog() == DialogResult.OK && listBox1.SelectedItem != null)
                {
                    if (!Directory.Exists(f.SelectedPath))
                        Directory.CreateDirectory(f.SelectedPath);
                    foreach (NUT_Texture tex in selected.Nodes)
                    {
                        string filename = Path.Combine(f.SelectedPath, $"{tex.HASHID.ToString("X")}.dds");
                        if(tex.type == PixelInternalFormat.Rgba)
                        {
                            ExportPNG(filename, tex);
                        }else
                        {
                            DDS dds = new DDS();
                            dds.fromNUT_Texture(tex);
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
                    if (!Directory.Exists(f.SelectedPath))
                        Directory.CreateDirectory(f.SelectedPath);
                    NUT nut;
                    if (listBox1.SelectedItem == null)
                        nut = new NUT();
                    else
                        nut = selected;

                    foreach (var texPath in Directory.GetFiles(f.SelectedPath))
                    {
                        if (!(texPath.ToLower().EndsWith(".dds") || texPath.ToLower().EndsWith(".png"))) return;
                        int texId;
                        bool isTex = int.TryParse(Path.GetFileNameWithoutExtension(texPath), NumberStyles.HexNumber,
                            new CultureInfo("en-US"), out texId);

                        NUT_Texture texture = null;
                        foreach (NUT_Texture tex in nut.Nodes)
                            if (tex.HASHID == texId)
                                texture = tex;

                        if (texture == null)
                        {
                            //new texture
                            NUT_Texture tex = null;
                            if (texPath.ToLower().EndsWith(".png"))
                                tex = fromPNG(texPath, 1);
                            if (texPath.ToLower().EndsWith(".dds"))
                            {
                                DDS dds = new DDS(new FileData(texPath));
                                tex = dds.toNUT_Texture();
                            }
                            if (isTex)
                                tex.HASHID = texId;
                            else
                                tex.HASHID = nut.Nodes.Count;
                            nut.Nodes.Add(tex);
                            nut.draw.Add(tex.HASHID, NUT.loadImage(tex));
                        }
                        else
                        {
                            //old texture
                            NUT_Texture tex = texture;

                            NUT_Texture ntex = null;
                            if (texPath.ToLower().EndsWith(".png"))
                                ntex = fromPNG(texPath, 1);
                            if (texPath.ToLower().EndsWith(".dds"))
                            {
                                DDS dds = new DDS(new FileData(texPath));
                                ntex = dds.toNUT_Texture();
                            }

                            tex.Height = ntex.Height;
                            tex.Width = ntex.Width;
                            tex.type = ntex.type;
                            tex.mipmaps = ntex.mipmaps;
                            tex.utype = ntex.utype;

                            GL.DeleteTexture(selected.draw[tex.HASHID]);
                            selected.draw.Remove(tex.HASHID);
                            selected.draw.Add(tex.HASHID, NUT.loadImage(tex));
                        }
                    }
                    if (!Runtime.TextureContainers.Contains(nut))
                        Runtime.TextureContainers.Add(nut);
                    FillForm();
                }
            }
        }

        private void saveNUTZLIBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Namco Universal Texture (.nut)|*.nut|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".nut") && selected != null)
                    {
                        FileOutput o = new FileOutput();
                        o.writeBytes(FileData.DeflateZLIB(selected.Rebuild()));
                        o.save(sfd.FileName);
                    }
                }
            }
        }

        private void texIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selected != null)
                using (var v = new NUT_TexIDEditor())
                {
                    v.Set(selected);
                    v.ShowDialog();
                    if (v.exitStatus == NUT_TexIDEditor.Opened)
                    {
                        v.Apply();
                        listBox2.Refresh();
                        listBox1.Refresh();
                        Refresh();
                        listBox1.SelectedItem = listBox1.SelectedItem;
                    }
                }
        }


        private void renderChannelR_Click_1(object sender, EventArgs e)
        {
            if (renderR)
            {
                renderR = false;
                renderChannelR.ForeColor = Color.DarkGray;
            }

            else
            {
                renderR = true;
                renderChannelR.ForeColor = Color.Red;
            }

            // Uniforms need to be udpated.
            RenderTexture(); 
        }

        private void renderChannelG_Click(object sender, EventArgs e)
        {
            if (renderG)
            {
                renderG = false;
                renderChannelG.ForeColor = Color.DarkGray;
            }
            else
            {
                renderG = true;
                renderChannelG.ForeColor = Color.Green;
            }

            // Uniforms need to be udpated.
            RenderTexture();
        }

        private void renderChannelB_Click_1(object sender, EventArgs e)
        {
            if (renderB)
            {
                renderB = false;
                renderChannelB.ForeColor = Color.DarkGray;
            }

            else
            {
                renderB = true;
                renderChannelB.ForeColor = Color.Blue;
            }

            // Uniforms need to be udpated.
            RenderTexture(); 
        }

        private void renderChannelA_Click_1(object sender, EventArgs e)
        {
            if (renderAlpha)
            {
                renderAlpha = false;
                renderChannelA.ForeColor = Color.DarkGray;
            }

            else
            {
                renderAlpha = true;
                renderChannelA.ForeColor = Color.Black;
            }

            // Uniforms need to be udpated.
            RenderTexture(); 
        }

        private void aspectRatioCB_CheckedChanged(object sender, EventArgs e)
        {
            preserveAspectRatio = aspectRatioCB.Checked;
            RenderTexture();
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
    }
}
