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

namespace Smash_Forge
{
    public partial class NUTEditor : Form
    {
        private NUT selected;
        private Dictionary<NUT.NUD_Texture, string> extractedImages = new Dictionary<NUT.NUD_Texture, string>();

        public NUTEditor()
        {
            InitializeComponent();
            FillForm();
            if (Runtime.TextureContainers.Count > 0)
                listBox1.SelectedIndex = 0;
        }

        private void FillForm()
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
            foreach (NUT.NUD_Texture tex in n.textures)
            {
                listBox2.Items.Add(tex);
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex >= 0)
            {
                NUT.NUD_Texture tex = ((NUT.NUD_Texture)listBox2.SelectedItem);
                textBox1.Text = tex.ToString();
                label2.Text = "Format: " + tex.type;
                label3.Text = "Width: " + tex.width;
                label4.Text = "Height:" + tex.height;
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
            GL.ClearColor(Color.Red);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Enable(EnableCap.Texture2D);

            if (listBox1.SelectedItem == null || listBox2.SelectedItem == null)
                return;

            int rt = ((NUT)listBox1.SelectedItem).draw[((NUT.NUD_Texture)listBox2.SelectedItem).id];

            GL.BindTexture(TextureTarget.Texture2D, rt);
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(1, 1);
            GL.Vertex2(1, -1);
            GL.TexCoord2(0, 1);
            GL.Vertex2(-1, -1);
            GL.TexCoord2(0, 0);
            GL.Vertex2(-1, 1);
            GL.TexCoord2(1, 0);
            GL.Vertex2(1, 1);
            GL.End();

            glControl1.SwapBuffers();
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
                NUT.NUD_Texture tex = ((NUT.NUD_Texture)listBox2.SelectedItem);
                GL.DeleteTexture(selected.draw[tex.id]);
                selected.draw.Remove(tex.id);
                selected.textures.Remove(tex);
                FillForm();
                listBox1.SelectedItem = selected;
                listBox2.SelectedIndex = 0;
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Direct Draw Surface (.dds)|*.dds|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith(".dds") && selected != null)
                    {
                        DDS dds = new DDS(new FileData(ofd.FileName));
                        NUT.NUD_Texture tex = dds.toNUT_Texture();
                        tex.id = 0x40FFFF00 | (selected.textures.Count);
                        selected.textures.Add(tex);
                        selected.draw.Add(tex.id, NUT.loadImage(tex));
                        FillForm();
                        listBox1.SelectedItem = selected;
                    }
                }
            }
        }

        private void exportAsDDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Direct Draw Surface (.dds)|*.dds|" +
                             "All Files (*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".dds") && selected != null)
                    {
                        DDS dds = new DDS();
                        dds.fromNUT_Texture((NUT.NUD_Texture)(listBox2.SelectedItem));
                        dds.Save(sfd.FileName);
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if(listBox2.SelectedItem != null && !textBox1.Text.Equals(""))
            {
                int oldid = ((NUT.NUD_Texture)listBox2.SelectedItem).id;
                int newid = -1;
                int.TryParse(textBox1.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out newid);
                if (newid == -1)
                    textBox1.Text = ((NUT.NUD_Texture)listBox2.SelectedItem).id.ToString("x");
                if(oldid!=newid)
                {
                    if (!selected.draw.ContainsKey(newid))
                    {
                        ((NUT.NUD_Texture)listBox2.SelectedItem).id = newid;

                        selected.draw.Add(newid, selected.draw[oldid]);
                        selected.draw.Remove(oldid);
                    }
                    else
                    {
                        textBox1.Text = (newid + 1).ToString("x") + "";
                    }
                }

                //listBox2.Refresh();
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
                ofd.Filter = "Direct Draw Surface (.dds)|*.dds|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith(".dds") && selected != null)
                    {

                        NUT.NUD_Texture tex = (NUT.NUD_Texture)(listBox2.SelectedItem);

                        DDS dds = new DDS(new FileData(ofd.FileName));
                        NUT.NUD_Texture ntex = dds.toNUT_Texture();

                        tex.height = ntex.height;
                        tex.width = ntex.width;
                        tex.type = ntex.type;
                        tex.mipmaps = ntex.mipmaps;
                        tex.utype = ntex.utype;

                        GL.DeleteTexture(selected.draw[tex.id]);
                        selected.draw.Remove(tex.id);
                        selected.draw.Add(tex.id, NUT.loadImage(tex));

                        FillForm();
                        listBox1.SelectedItem = selected;
                    }
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
            string tempFileName = Path.GetTempFileName();
            DDS dds = new DDS();
            dds.fromNUT_Texture((NUT.NUD_Texture)(listBox2.SelectedItem));
            dds.Save(tempFileName);
            DeleteIfExists(Path.ChangeExtension(tempFileName, ".dds"));
            File.Move(tempFileName, Path.ChangeExtension(tempFileName, ".dds"));
            tempFileName = Path.ChangeExtension(tempFileName, ".dds");
            System.Diagnostics.Process.Start(tempFileName).WaitForExit();
            importBack(tempFileName);
        }

        private void extractAndPickAProgramToEditWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tempFileName = Path.GetTempFileName();
            DDS dds = new DDS();
            dds.fromNUT_Texture((NUT.NUD_Texture)(listBox2.SelectedItem));
            dds.Save(tempFileName);
            DeleteIfExists(Path.ChangeExtension(tempFileName, ".dds"));
            File.Move(tempFileName, Path.ChangeExtension(tempFileName, ".dds"));
            tempFileName = Path.ChangeExtension(tempFileName, ".dds");
            ShowOpenWithDialog(tempFileName).WaitForExit();
            importBack(tempFileName);
        }

        private void importBack(string filename)
        {
            NUT.NUD_Texture tex = (NUT.NUD_Texture)(listBox2.SelectedItem);

            DDS dds = new DDS(new FileData(Path.ChangeExtension(extractedImages[(NUT.NUD_Texture)(listBox2.SelectedItem)], ".dds")));
            NUT.NUD_Texture ntex = dds.toNUT_Texture();

            tex.height = ntex.height;
            tex.width = ntex.width;
            tex.type = ntex.type;
            tex.mipmaps = ntex.mipmaps;
            tex.utype = ntex.utype;

            GL.DeleteTexture(selected.draw[tex.id]);
            selected.draw.Remove(tex.id);
            selected.draw.Add(tex.id, NUT.loadImage(tex));

            FillForm();
            listBox1.SelectedItem = selected;
            listBox2.SelectedItem = tex;
            RenderTexture();
        }
    }
}
