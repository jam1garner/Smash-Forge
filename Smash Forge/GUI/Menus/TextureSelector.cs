using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge.GUI.Menus
{
    public partial class TextureSelector : Form
    {
        private int imageWidth = 64;
        private int imageHeight = 64;

        public TextureSelector()
        {
            InitializeComponent();
            InitializeImageList();

            for (int i = 0; i < 10; i++)
            {
                listView1.Items.Add("texture", "image");
            }
        }

        private void InitializeImageList()
        {
            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(imageWidth, imageHeight);

            imageList.Images.Add("image", Properties.Resources.UVPattern);

            listView1.LargeImageList = imageList;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: change the selected texture hash.
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            // TODO: apply the selection.
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            // TODO: apply the selection.
        }
    }
}
