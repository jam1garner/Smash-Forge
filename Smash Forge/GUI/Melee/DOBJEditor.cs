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
        private class TextureNode : ListViewItem
        {
            public DatTexture Texture;

            public TextureNode(DatTexture Texture)
            {
                this.Texture = Texture;
                Text = Texture.UnkFlags.ToString("x") + "_" + Texture.ImageData.Format.ToString();
            }
        }

        private DatDOBJ DOBJ;
        private Bitmap TempBitmap;

        public DOBJEditor(DatDOBJ DOBJ)
        {
            InitializeComponent();
            this.DOBJ = DOBJ;
            Reload();
        }

        public void Reload()
        {
            buttonDIF.BackColor = DOBJ.Material.MaterialColor.DIF;
            buttonSPC.BackColor = DOBJ.Material.MaterialColor.SPC;
            buttonAMB.BackColor = DOBJ.Material.MaterialColor.AMB;
            numericGlossiness.Value = (decimal)DOBJ.Material.MaterialColor.Glossiness;
            numericTransparency.Value = (decimal)DOBJ.Material.MaterialColor.Transparency;

            listBox1.Items.Clear();

            foreach(DatTexture t in DOBJ.Material.Textures)
            {
                listBox1.Items.Add(new TextureNode(t));
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TempBitmap != null)
            {
                TempBitmap.Dispose();
                TempBitmap = null;
            }

            if(listBox1.SelectedItem != null)
            {
                if (((TextureNode)listBox1.SelectedItem).Texture != null)
                    TempBitmap = ((TextureNode)listBox1.SelectedItem).Texture.GetBitmap();
            }
            pictureBox1.Image = TempBitmap;
        }

        private void buttonDIF_Click(object sender, EventArgs e)
        {
            DOBJ.Material.MaterialColor.DIF = GetColor(DOBJ.Material.MaterialColor.DIF);
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
            DOBJ.Material.MaterialColor.AMB = GetColor(DOBJ.Material.MaterialColor.AMB);
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
    }
}
