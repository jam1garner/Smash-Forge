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

namespace Smash_Forge
{
    public partial class _3DSTexEditor : EditorBase
    {
        public _3DSTexEditor()
        {
            InitializeComponent();
            FilePath = "";
            Text = "New 3DS TEX";
        }

        public _3DSTexEditor(string fname) : this()
        {
            FilePath = fname;
            OpenTEX(fname);
            Edited = false;
        }

        public override void Save()
        {
            if (FilePath.Equals(""))
            {
                SaveAs();
                return;
            }
            ExportTEX(FilePath);
            Edited = false;
        }

        public override void SaveAs()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Smash for 3DS TEX|*.tex|" +
                             "All files(*.*)|*.*";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".tex"))
                    {
                        FilePath = sfd.FileName;
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

        public void OpenTEX(string fname)
        {
            Edited = true;
            FileData d = new FileData(fname);
            d.Endian = System.IO.Endianness.Little;

            int width = d.readInt();
            int height = d.readInt();
            int type = d.readByte();
            int dunno = d.readByte();
            d.skip(2); // padding

            string name = "";

            int i = d.readByte();
            while (i != 0x00)
            {
                name += (char)i;
                i = d.readByte();
            }

            if (type > 0xD || type < 0)
                return;

            nameBox.Text = name;
            formatSelector.SelectedIndex = type;
            label3.Text = "Width: " + width;
            label4.Text = "Height: " + height;

            byte[] data = d.getSection(0x80, d.size() - 0x80);

            pictureBox1.Image = _3DS.DecodeImage(data, width, height, (_3DS.Tex_Formats)type);
            
            pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        public void OpenPNG(string filename)
        {
            Edited = true;
            pictureBox1.Image = new Bitmap(filename);

            if (formatSelector.SelectedIndex < 0)
                formatSelector.SelectedIndex = 0x0D;
            
            pictureBox1.Image = _3DS.DecodeImage(_3DS.EncodeImage(new Bitmap(pictureBox1.Image), (_3DS.Tex_Formats)formatSelector.SelectedIndex), pictureBox1.Image.Width, pictureBox1.Image.Height, (_3DS.Tex_Formats)formatSelector.SelectedIndex);
            
            nameBox.Text = Path.GetFileNameWithoutExtension(filename);
            label3.Text = "Width: " + pictureBox1.Image.Width;
            label4.Text = "Height: " + pictureBox1.Image.Height;
        }

        private void tEXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Smash for 3DS TEX|*.tex|" +
                             "All files(*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    OpenTEX(ofd.FileName);
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
                pictureBox1.Image.Save(save.FileName);
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

        public void ExportTEX(string filename)
        {
            FileOutput o = new FileOutput();
            o.Endian = Endianness.Little;

            o.writeInt(pictureBox1.Image.Width);
            o.writeInt(pictureBox1.Image.Height);
            o.writeByte(formatSelector.SelectedIndex);
            o.writeByte(1);
            o.writeShort(0);
            o.writeString(nameBox.Text);
            for (int i = 0; i < 0x74 - nameBox.Text.Length; i++)
                o.writeByte(0);

            pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            o.writeBytes(_3DS.EncodeImage(new Bitmap(pictureBox1.Image), (_3DS.Tex_Formats)formatSelector.SelectedIndex));

            pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            o.save(filename);
        }

        private void tEXToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Smash for 3DS TEX|*.tex|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK)
            {
                ExportTEX(save.FileName);
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
                    if (Path.GetExtension(ofd.FileName).ToLower().Equals(".png"))
                    {
                        OpenPNG(ofd.FileName);
                    }
                    if (Path.GetExtension(ofd.FileName).ToLower().Equals(".tex"))
                    {
                        OpenTEX(ofd.FileName);
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
                    pictureBox1.Image.Save(save.FileName);
                }
                if (Path.GetExtension(save.FileName).ToLower().Equals(".tex"))
                {
                    ExportTEX(save.FileName);
                }
            }
        }
    }
}
