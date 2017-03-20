using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using GraphicsMagick;

namespace Smash_Forge
{
    public partial class HeightMapEditor : DockContent
    {
        public HeightMapEditor()
        {
            InitializeComponent();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HGHT heightMap = (HGHT)listBox1.SelectedItem;
            using(SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Supported Filetypes (.png, .hght)|*.png;*.hght";
                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName.EndsWith(".png"))
                    {
                        using (FileStream f = new FileStream(sfd.FileName, FileMode.Create))
                            heightMap.toMagickImage().Write(f, MagickFormat.Png48);
                    }

                    if (sfd.FileName.EndsWith(".hght"))
                    {
                        heightMap.Write(sfd.FileName);
                    }
                }
            }
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Portable Network Graphics (.png)|*.png";
                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith(".png"))
                    {
                        MagickReadSettings settings = new MagickReadSettings();
                        settings.Format = MagickFormat.Png48;
                        MagickImage image = new MagickImage(ofd.FileName,settings);
                        HGHT heightMap = (HGHT)listBox1.SelectedItem;
                        heightMap.fromMagickImage(image);
                        heightMap.generateBitmap();
                    }
                }
            }
        }

        private void addHeightMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Portable Network Graphics (.png)|*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith(".png"))
                    {
                        MagickReadSettings settings = new MagickReadSettings();
                        settings.Format = MagickFormat.Png48;
                        MagickImage image = new MagickImage(ofd.FileName, settings);
                        HGHT heightMap = new HGHT(image);
                        heightMap.name = "New Heightmap";
                        heightMap.generateBitmap();
                        listBox1.Items.Add(heightMap);
                    }
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = ((HGHT)listBox1.SelectedItem).bitmap;
        }
    }
}
