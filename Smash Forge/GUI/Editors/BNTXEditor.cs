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

namespace Smash_Forge
{
    public partial class BNTXEditor : EditorBase
    {
        private BRTI BRTI;
        private bool _loaded = false;

        public BNTXEditor()
        {
            InitializeComponent();
            FilePath = "";
            Text = "New Binary Texture";
        }

        public BNTXEditor(string fname) : this()
        {
            BNTX b = new BNTX();
            b.ReadBNTXFile(fname);
            FilePath = fname;
            OpenBNTX(fname);
            Edited = false;
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

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
        public void OpenBNTX(string fname)
        {
            Edited = true;

            foreach (BRTI tex in BNTX.textures)
            {
                listBox1.Items.Add(tex);
            }

            //    nameBox.Text = name;
            //   formatSelector.SelectedIndex = type;
            //   label3.Text = "Width: " + width;
            //  label4.Text = "Height: " + height;


            //     pictureBox1.Image = _3DS.DecodeImage(data, width, height, (_3DS.Tex_Formats)type);

            //     pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        public void OpenPNG(string filename)
        {
            Edited = true;
       

         //   if (formatSelector.SelectedIndex < 0)
           //     formatSelector.SelectedIndex = 0x0D;
            
   //         pictureBox1.Image = _3DS.DecodeImage(_3DS.EncodeImage(new Bitmap(pictureBox1.Image), (_3DS.Tex_Formats)formatSelector.SelectedIndex), pictureBox1.Image.Width, pictureBox1.Image.Height, (_3DS.Tex_Formats)formatSelector.SelectedIndex);
            
          //  nameBox.Text = Path.GetFileNameWithoutExtension(filename);
   
        }

        private void tEXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Smash for 3DS TEX|*.tex|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    OpenBNTX(ofd.FileName);
                }
            }
        }

        private void pNGToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Portable Network Graphic|*.png|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK)
            {
           
            }
        }

        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Portable Network Graphic|*.png|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    OpenPNG(ofd.FileName);
                }
            }
        }

        public void ExportBNTX(string filename)
        {
            FileOutput o = new FileOutput();
            o.Endian = Endianness.Little;

      

          //  pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);


       //     pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            o.save(filename);
        }

        private void tEXToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Smash for 3DS TEX|*.tex|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK)
            {
                ExportBNTX(save.FileName);
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Portable Network Graphic, Smash for 3DS Tex (.png, .tex)|*.png;*.tex;|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    BRTI.BRTI_Texture newTexture = null;
                    if (Path.GetExtension(ofd.FileName).ToLower().Equals(".png"))
                    {
                        OpenPNG(ofd.FileName);
                    }
                    if (Path.GetExtension(ofd.FileName).ToLower().Equals(".dds"))
                    {
                        DDS dds = new DDS(new FileData(ofd.FileName));
                        newTexture = dds.toBRTITexture();
                    }
                    if (Path.GetExtension(ofd.FileName).ToLower().Equals(".tex"))
                    {
                        OpenBNTX(ofd.FileName);
                    }
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

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                BRTI b = ((BRTI)listBox1.SelectedItem);

                label1.Text = "Width: " + b.texture.width;
                label2.Text = "Height: " + b.texture.height;

                switch (b.format >> 8)
                {
                    case (uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC1:
                        {
                            label3.Text = "Format: BC1/DXT1";
                        }
                        break;
                    case (uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC2:
                        {
                            label3.Text = "Format: BC2/DXT2";
                        }
                        break;
                    case (uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC3:
                        {
                            label3.Text = "Format: BC3/DXT5";
                        }
                        break;
                    case (uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC4:
                        {
                            label3.Text = "Format: BC4/ATI1";
                        }
                        break;
                    case (uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC5:
                        {
                            label3.Text = "Format: BC5/ATI2";
                        }
                        break;
                    case (uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC6:
                        {
                            label3.Text = "Format: BC6";
                        }
                        break;
                    case (uint)Formats.BNTXImageFormat.IMAGE_FORMAT_BC7:
                        {
                            label3.Text = "Format: BC7";
                        }
                        break;
                    default:
                        label3.Text = "Format: Unsupported format";
                        break;
                }
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
            if (!_loaded || glControl1 == null)
                return;

            glControl1.MakeCurrent();
            GL.Viewport(glControl1.ClientRectangle);

            if (listBox1.SelectedItem == null)
            {
                glControl1.SwapBuffers();
                return;
            }

            int width = ((BRTI)listBox1.SelectedItem).Width;
            int height = ((BRTI)listBox1.SelectedItem).Height;

      //      int texture = NUT.draw[((BRTI)listBox1.SelectedItem).HASHID];

      //      Rendering.RenderTools.DrawTexturedQuad(texture, width, height);

            glControl1.SwapBuffers();

            if (!Runtime.shaders["Texture"].hasCheckedCompilation())
            {
                Runtime.shaders["Texture"].DisplayCompilationWarning("Texture");
            }
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
            // Load RGB channel



        }

        private void BNTXEditor_Load(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void glControl2_Load(object sender, EventArgs e)
        {
            //Load Alpha channel


        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }
    }
}
