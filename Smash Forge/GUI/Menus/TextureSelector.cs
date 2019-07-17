using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using SFGraphics.GLObjects.Textures;

namespace SmashForge.Gui.Menus
{
    public partial class TextureSelector : Form
    {

        private static readonly int imageWidth = 80;
        private static readonly int imageHeight = 80;

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
            // Reuse the same context to avoid CPU bottlenecks.
            using (OpenTK.GameWindow window = Rendering.OpenTkSharedResources.CreateGameWindowContext(imageWidth, imageHeight))
            {
                RenderTexturesAddToImageList();
            }
        }

        private void RenderTexturesAddToImageList()
        {
            foreach (NUT nut in Runtime.textureContainers)
            {
                foreach (var texture in nut.glTexByHashId)
                {
                    // Use the texture ID in hex for the display text and image key.
                    listView1.Items.Add(texture.Key.ToString("X"), texture.Key.ToString("X"));

                    Bitmap bitmap = Rendering.TextureToBitmap.RenderBitmapUseExistingContext((Texture2D)texture.Value, imageWidth, imageHeight);
                    imageList.Images.Add(texture.Key.ToString("X"), bitmap);
                    // StackOverflow makes the bad exceptions go away.
                    var dummy = imageList.Handle;
                    bitmap.Dispose();
                }
            }
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

        }
    }
}
