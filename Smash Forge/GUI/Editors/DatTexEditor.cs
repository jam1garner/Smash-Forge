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

namespace Smash_Forge
{
    public partial class DatTexEditor : DockContent
    {
        public DatTexEditor(DAT dat)
        {
            InitializeComponent();
            this.dat = dat;
        }

        private DAT dat;

        private class DatTexture
        {
            public string name = "datTex";
            public int tobjOffset;
            public int textureOffset;
            public int textureDataOffsest;
            public Bitmap image;

            public override string ToString()
            {
                return name;
            }
        }

        public void refreshTextureList()
        {
            listBox1.Items.Clear();
            foreach(int image in dat.tobjLinker.Keys)
            {
                object[] texture = dat.tobjLinker[image]; //testOffset, image, imageOffset, imageDataOffset
                DatTexture temp = new DatTexture();
                temp.tobjOffset = (int)texture[0];
                temp.image = (Bitmap)texture[1];
                temp.textureOffset = (int)texture[2];
                temp.textureDataOffsest = (int)texture[3];
                foreach(TreeNode t in dat.tree)
                    if (((int)t.Tag) == temp.textureDataOffsest)
                        temp.name = t.Text;
                listBox1.Items.Add(temp);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = ((DatTexture)listBox1.SelectedItem).image;
        }

        private void DatTexEditor_Load(object sender, EventArgs e)
        {
            refreshTextureList();
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)// && listBox1.GetChildAtPoint(new System.Drawing.Point(e.X, e.Y)) != null)
                contextMenuStrip1.Show(new System.Drawing.Point(e.X, e.Y));
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(SaveFileDialog sfd = new SaveFileDialog())
            {
                if(sfd.ShowDialog() == DialogResult.OK)
                {
                    ((DatTexture)listBox1.SelectedItem).image.Save(sfd.FileName);
                }
            }
        }
    }
}
