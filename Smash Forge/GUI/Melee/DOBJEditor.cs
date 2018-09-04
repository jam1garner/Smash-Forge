using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.DAT;

namespace Smash_Forge.GUI.Melee
{
    public partial class DOBJEditor : Form
    {
        private TextureNode selectedTexture = null;
        private MeleeDataObjectNode meleeDataObjectNode = null;

        private class TextureNode : ListViewItem
        {
            public DatTexture Texture;

            public TextureNode(DatTexture Texture)
            {
                this.Texture = Texture;
                Text = Texture.UnkFlags.ToString("X") + "_" + Texture.ImageData.Format.ToString();
            }
        }

        private DatDOBJ DOBJ;
        private Bitmap TempBitmap;

        public DOBJEditor(DatDOBJ DOBJ, MeleeDataObjectNode meleeDataObjectNode)
        {
            InitializeComponent();
            this.DOBJ = DOBJ;
            this.meleeDataObjectNode = meleeDataObjectNode;
            Reload();
        }

        public void Reload()
        {
            buttonDIF.BackColor = DOBJ.Material.MaterialColor.DIF;
            buttonSPC.BackColor = DOBJ.Material.MaterialColor.SPC;
            buttonAMB.BackColor = DOBJ.Material.MaterialColor.AMB;
            numericGlossiness.Value = (decimal)DOBJ.Material.MaterialColor.Glossiness;
            numericTransparency.Value = (decimal)DOBJ.Material.MaterialColor.Transparency;
            flagsTB.Text = DOBJ.Material.Flags.ToString("X");

            textureListBox.Items.Clear();

            foreach(DatTexture t in DOBJ.Material.Textures)
            {
                textureListBox.Items.Add(new TextureNode(t));
            }
        }

        private void textureListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TempBitmap != null)
            {
                TempBitmap.Dispose();
                TempBitmap = null;
            }

            if(textureListBox.SelectedItem != null)
            {
                selectedTexture = (TextureNode)textureListBox.SelectedItem;
                if (selectedTexture != null && selectedTexture.Texture != null)
                    TempBitmap = selectedTexture.Texture.GetBitmap();

                textureFlagsTB.Text = selectedTexture.Texture.UnkFlags.ToString("X");
            }
            pictureBox1.Image = TempBitmap;
        }

        private void buttonDIF_Click(object sender, EventArgs e)
        {
            Color color = GetColor(DOBJ.Material.MaterialColor.DIF);
            DOBJ.Material.MaterialColor.DIF = color;
            buttonDIF.BackColor = color;
        }

        private Color GetColor(Color c)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = c;

                if (cd.ShowDialog() == DialogResult.OK)
                {
                    return cd.Color;
                }

                return c;
            }
        }

        private void buttonAMB_Click(object sender, EventArgs e)
        {
            Color color = GetColor(DOBJ.Material.MaterialColor.AMB);
            DOBJ.Material.MaterialColor.AMB = color;
            buttonAMB.BackColor = color;
        }

        private void buttonSPC_Click(object sender, EventArgs e)
        {
            DOBJ.Material.MaterialColor.SPC = GetColor(DOBJ.Material.MaterialColor.SPC);
        }

        private void numericGlossiness_ValueChanged(object sender, EventArgs e)
        {
            DOBJ.Material.MaterialColor.Glossiness = (float)numericGlossiness.Value;
        }

        private void numericTransparency_ValueChanged(object sender, EventArgs e)
        {
            DOBJ.Material.MaterialColor.Transparency = (float)numericTransparency.Value;
        }

        private void flagsTB_TextChanged(object sender, EventArgs e)
        {
            DOBJ.Material.Flags = GuiTools.TryParseTBInt(flagsTB, true);
        }

        private void textureFlagsTB_TextChanged(object sender, EventArgs e)
        {
            selectedTexture.Texture.UnkFlags = GuiTools.TryParseTBUint(textureFlagsTB, true);
            meleeDataObjectNode.RefreshRendering();
        }
    }
}
