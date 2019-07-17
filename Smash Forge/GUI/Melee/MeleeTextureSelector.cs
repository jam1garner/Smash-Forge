using MeleeLib.DAT;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmashForge.Filetypes.Melee;

namespace SmashForge.Gui.Melee
{
    public partial class MeleeTextureSelector : Form
    {
        public TextureNode SelectedTexture;
        public MeleeDataObjectNode meleeDataObjectNode;

        public DatTexture Selected;

        public MeleeTextureSelector(MeleeDataObjectNode mdon, TextureNode Texture)
        {
            InitializeComponent();

            this.SelectedTexture = Texture;
            this.meleeDataObjectNode = mdon;
            DialogResult = DialogResult.None;

            DatDOBJ[] DOBJS = mdon.GetRoot().Root.GetDataObjects();

            ImageList imageList1 = new ImageList();
            imageList1.ColorDepth = ColorDepth.Depth24Bit;
            listView1.Scrollable = true;
            listView1.View = View.LargeIcon;
            imageList1.ImageSize = new Size(115, 115);
            listView1.LargeImageList = imageList1;

            // Grab all unique textures
            List<byte[]> Used = new List<byte[]>();
            List<DatTexture> Textures = new List<DatTexture>();
            for(int i = 0; i < DOBJS.Length; i++)
            {
                foreach(DatTexture t in DOBJS[i].Material.Textures)
                {
                    if (t.ImageData == null || Used.Contains(t.ImageData.Data))
                    {
                        continue;
                    }
                    else
                    {
                        Used.Add(t.ImageData.Data);
                        
                        ListViewItem lstItem = new ListViewItem();
                        lstItem.ImageIndex = Textures.Count;
                        lstItem.Text = "Texture_" + lstItem.ImageIndex + "_" + (t.ImageData != null ? t.ImageData.Format.ToString() + "\n" + t.ImageData.Width + "x" + t.ImageData.Height : "");
                        lstItem.Tag = t;
                        listView1.Items.Add(lstItem);
                        imageList1.Images.Add(t.GetStaticBitmap());
                        Textures.Add(t);
                    }
                }
            }

            // Setup Combo Boxes

            foreach (var enu in Enum.GetValues(typeof(TPL_TextureFormat))) CBFormat.Items.Add(enu);
            foreach (var enu in Enum.GetValues(typeof(TPL_PaletteFormat))) CBPalette.Items.Add(enu);
            CBFormat.SelectedItem = TPL_TextureFormat.CMP;
            CBPalette.SelectedItem = TPL_PaletteFormat.RGB565;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CBFormat.SelectedItem == null)
                CBFormat.SelectedItem = TPL_TextureFormat.CMP;
            if (SelectedTexture != null)
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    if((TPL_TextureFormat)CBFormat.SelectedItem == TPL_TextureFormat.CMP)
                        ofd.Filter = "DDS|*.dds";
                    else
                        ofd.Filter = "PNG|*.png";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        Selected = SelectedTexture.Texture;
                        if ((TPL_TextureFormat)CBFormat.SelectedItem == TPL_TextureFormat.CMP)
                        {
                            Dds d = new Dds(new FileData(ofd.FileName));
                            if (d.header.ddspf.fourCc != 0x31545844)
                            {
                                MessageBox.Show("Error Importing Texture:\nOnly DXT1 Files are supported currently");
                                return;
                            }
                            Selected.SetFromDXT1(new FileData(d.bdata).GetSection(0, (int)(d.header.width * d.header.height / 2)), (int)d.header.width, (int)d.header.height);
                        }
                        else
                        {
                            Bitmap b = new Bitmap(ofd.FileName);
                            Selected.SetFromBitmap(b, (MeleeLib.TPL_TextureFormat)CBFormat.SelectedItem, (MeleeLib.TPL_PaletteFormat)CBPalette.SelectedItem);
                            b.Dispose();
                        }
                        DialogResult = DialogResult.OK;
                        CloseForm();
                        Close();
                    }
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Selected = (DatTexture)listView1.SelectedItems[0].Tag;
            }
            else
                Selected = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(Selected != null)
            {
                DialogResult = DialogResult.OK;
                SelectedTexture.Texture.ImageData = Selected.ImageData;
                SelectedTexture.Texture.Palette = Selected.Palette;
                SelectedTexture.Texture.ResetStaticTexture();
                CloseForm();
                Close();
            }
        }

        private void MeleeTextureSelector_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseForm();
        }

        private void CloseForm()
        {
            foreach (Image i in listView1.LargeImageList.Images)
            {
                i.Dispose();
            }
            listView1.LargeImageList.Images.Clear();
        }

        private void CBFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            CBPalette.Enabled = false;
            if ((TPL_TextureFormat)CBFormat.SelectedItem == TPL_TextureFormat.CI8 ||
                (TPL_TextureFormat)CBFormat.SelectedItem == TPL_TextureFormat.CI4 ||
                (TPL_TextureFormat)CBFormat.SelectedItem == TPL_TextureFormat.CI14X2)
            {
                CBPalette.Enabled = true;
            }
        }
    }
}
