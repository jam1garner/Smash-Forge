using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Diagnostics;
using SFGraphics.GLObjects.Textures;

namespace Smash_Forge.GUI.Menus
{
    public partial class TextureSelector : Form
    {

        private static readonly int imageWidth = 64;
        private static readonly int imageHeight = 64;

        private ImageList imageList = new ImageList()
        {
            ColorDepth = ColorDepth.Depth32Bit,
            ImageSize = new Size(imageWidth, imageHeight)
        };

        public int TextureId { get { return selectedTextureId; } }
        private int selectedTextureId = -1;

        public TextureSelector()
        {
            InitializeComponent();
            InitializeImageList();
        }

        private void InitializeImageList()
        {
            AddImagesFromAllTextureContainers();

            listView1.LargeImageList = imageList;
        }

        private void AddImagesFromAllTextureContainers()
        {
            int totalTextureCount = GetTotalTextureCount();

            // This may take a while for larger NUTs.
            ProgressAlert progressAlert = new ProgressAlert();
            progressAlert.Show();

            int index = 0;
            foreach (NUT nut in Runtime.TextureContainers)
            {
                foreach (var texture in nut.glTexByHashId)
                {
                    // Use the texture ID in hex for the display text and image key.
                    listView1.Items.Add(texture.Key.ToString("X"), texture.Key.ToString("X"));

                    Bitmap bitmap = Rendering.TextureToBitmap.RenderBitmap((Texture2D)texture.Value, imageWidth, imageHeight);
                    imageList.Images.Add(texture.Key.ToString("X"), bitmap);
                    // StackOverflow makes the bad exceptions go away.
                    var dummy = imageList.Handle;
                    bitmap.Dispose();

                    // Update progress.
                    progressAlert.ProgressValue = (int)(((double)index / totalTextureCount) * 100);
                    progressAlert.Message = String.Format("Rendering {0}...", texture.Key.ToString("X"));
                    progressAlert.Refresh();
                    index += 1;
                }
            }
            // Finished
            progressAlert.ProgressValue = 100;
            progressAlert.Refresh();
            progressAlert.Close();
        }

        private static int GetTotalTextureCount()
        {
            int totalTextureCount = 0;
            foreach (NUT nut in Runtime.TextureContainers)
            {
                totalTextureCount += nut.glTexByHashId.Count;
            }
            return totalTextureCount;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            UpdateSelectedTextureId();
        }

        private void UpdateSelectedTextureId()
        {
            int value = -1;
            if (int.TryParse(listView1.SelectedItems[0].Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out value))
                selectedTextureId = value;
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            UpdateSelectedTextureId();
            Close();
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            UpdateSelectedTextureId();
            Close();
        }

        private void TextureSelector_FormClosed(object sender, FormClosedEventArgs e)
        {
            // TODO: Properly dispose images.
        }
    }
}
