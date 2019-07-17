using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace SmashForge
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
            OpenTex(fname);
            Edited = false;
        }

        public override void Save()
        {
            if (FilePath.Equals(""))
            {
                SaveAs();
                return;
            }
            ExportTex(FilePath);
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

        public void OpenTex(string fname)
        {
            Edited = true;
            FileData d = new FileData(fname);
            d.endian = System.IO.Endianness.Little;

            int width = d.ReadInt();
            int height = d.ReadInt();
            int type = d.ReadByte();
            int dunno = d.ReadByte();
            d.Skip(2); // padding

            string name = "";

            int i = d.ReadByte();
            while (i != 0x00)
            {
                name += (char)i;
                i = d.ReadByte();
            }

            if (type > 0xD || type < 0)
                return;

            nameBox.Text = name;
            formatSelector.SelectedIndex = type;
            label3.Text = "Width: " + width;
            label4.Text = "Height: " + height;

            byte[] data = d.GetSection(0x80, d.Size() - 0x80);

            pictureBox1.Image = _3DS.DecodeImage(data, width, height, (_3DS.Tex_Formats)type);
            
            pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        public void OpenPng(string filename)
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
                    OpenTex(ofd.FileName);
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
                    OpenPng(ofd.FileName);
                }
            }
        }

        public void ExportTex(string filename)
        {
            FileOutput o = new FileOutput();
            o.endian = Endianness.Little;

            o.WriteInt(pictureBox1.Image.Width);
            o.WriteInt(pictureBox1.Image.Height);
            o.WriteByte(formatSelector.SelectedIndex);
            o.WriteByte(1);
            o.WriteShort(0);
            o.WriteString(nameBox.Text);
            for (int i = 0; i < 0x74 - nameBox.Text.Length; i++)
                o.WriteByte(0);

            pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            o.WriteBytes(_3DS.EncodeImage(new Bitmap(pictureBox1.Image), (_3DS.Tex_Formats)formatSelector.SelectedIndex));

            pictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            o.Save(filename);
        }

        private void tEXToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Smash for 3DS TEX|*.tex|All files(*.*)|*.*";
            DialogResult result = save.ShowDialog();

            if (result == DialogResult.OK)
            {
                ExportTex(save.FileName);
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
                        OpenPng(ofd.FileName);
                    }
                    if (Path.GetExtension(ofd.FileName).ToLower().Equals(".tex"))
                    {
                        OpenTex(ofd.FileName);
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
                    ExportTex(save.FileName);
                }
            }
        }
    }
}
