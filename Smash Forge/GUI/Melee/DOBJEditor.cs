using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MeleeLib.GCX;
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

            CBWrapS.Items.Add(GXWrapMode.CLAMP);
            CBWrapS.Items.Add(GXWrapMode.MIRROR);
            CBWrapS.Items.Add(GXWrapMode.REPEAT);
            CBWrapT.Items.Add(GXWrapMode.CLAMP);
            CBWrapT.Items.Add(GXWrapMode.MIRROR);
            CBWrapT.Items.Add(GXWrapMode.REPEAT);

            foreach (var suit in Enum.GetValues(typeof(GXAlphaOp))) CBAlphaOp.Items.Add(suit);
            foreach (var suit in Enum.GetValues(typeof(GXCompareType)))
            {
                CBAlphaComp1.Items.Add(suit); CBAlphaComp2.Items.Add(suit); CBDepthFunc.Items.Add(suit);
            }
            foreach (var suit in Enum.GetValues(typeof(GXBlendFactor)))
            {
                CBSrcFactor.Items.Add(suit);
                CBDstFactor.Items.Add(suit);
            }
            foreach (var suit in Enum.GetValues(typeof(GXLogicOp))) CBBlendOp.Items.Add(suit);
            foreach (var suit in Enum.GetValues(typeof(GXBlendMode))) CBBlendMode.Items.Add(suit);


            Reload();
        }

        public void Reload()
        {
            selectedTexture = null;
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

            if (DOBJ.Material.PixelProcessing != null)
            {
                tableLayoutPanel3.Visible = true;
                TBPixelFlags.Text = DOBJ.Material.PixelProcessing.Flags.ToString("X");
                CBAlphaOp.SelectedItem = DOBJ.Material.PixelProcessing.AlphaOp;
                CBAlphaComp1.SelectedItem = DOBJ.Material.PixelProcessing.AlphaComp0;
                CBAlphaComp2.SelectedItem = DOBJ.Material.PixelProcessing.AlphaComp1;
                CBBlendMode.SelectedItem = DOBJ.Material.PixelProcessing.BlendMode;
                CBBlendOp.SelectedItem = DOBJ.Material.PixelProcessing.BlendOp;
                CBDepthFunc.SelectedItem = DOBJ.Material.PixelProcessing.DepthFunction;
                TBAlphaRef1.Text = DOBJ.Material.PixelProcessing.AlphaRef0.ToString();
                TBAlphaRef2.Text = DOBJ.Material.PixelProcessing.AlphaRef1.ToString();
                TBDstAlpha.Text = DOBJ.Material.PixelProcessing.DestinationAlpha.ToString();
                CBSrcFactor.SelectedItem = DOBJ.Material.PixelProcessing.SrcFactor;
                CBDstFactor.SelectedItem = DOBJ.Material.PixelProcessing.DstFactor;
            }
            else
                tableLayoutPanel3.Visible = false;
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
                
                CBWrapS.SelectedItem = (selectedTexture.Texture.WrapS);
                CBWrapT.SelectedItem = (selectedTexture.Texture.WrapT);
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

        private void buttonImportTexture_Click(object sender, EventArgs e)
        {
            if (selectedTexture != null)
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "DDS|*.dds";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        DDS d = new DDS(new FileData(ofd.FileName));
                        if (d.header.ddspf.fourCC != 0x31545844)
                        {
                            MessageBox.Show("Error Importing Texture:\nOnly DXT1 Files are supported currently");
                            return;
                        }
                        selectedTexture.Texture.SetFromDXT1(new FileData(d.bdata).getSection(0, (int)(d.header.width*d.header.height/2)), (int)d.header.width, (int)d.header.height);
                        meleeDataObjectNode.RefreshRenderTextures();
                    }
                }
        }

        private void CBWrapT_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedTexture == null || CBWrapT.SelectedItem == null) return;
            selectedTexture.Texture.WrapT = (GXWrapMode)CBWrapT.SelectedItem;
            meleeDataObjectNode.RefreshRenderTextures();
        }

        private void CBWrapS_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedTexture == null || CBWrapS.SelectedItem == null) return;
            selectedTexture.Texture.WrapS = (GXWrapMode)CBWrapS.SelectedItem;
            meleeDataObjectNode.RefreshRenderTextures();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPixelEnabled.Checked)
            {
                DOBJ.Material.PixelProcessing = new DatPixelProcessing();
            }
            else
            {
                DOBJ.Material.PixelProcessing = null;
            }
            Reload();
        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void TBPixelFlags_TextChanged(object sender, EventArgs e)
        {
            if(DOBJ.Material.PixelProcessing != null)
            DOBJ.Material.PixelProcessing.Flags = (byte)GuiTools.TryParseTBInt(TBPixelFlags, true);
        }

        private void CBAlphaOp_SelectedValueChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.AlphaOp = (GXAlphaOp)CBAlphaOp.SelectedItem;
        }

        private void CBAlphaComp1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.AlphaComp0 = (GXCompareType)CBAlphaComp1.SelectedItem;
        }

        private void CBAlphaComp2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.AlphaComp1 = (GXCompareType)CBAlphaComp2.SelectedItem;
        }

        private void CBSrcFactor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.SrcFactor = (GXBlendFactor)CBSrcFactor.SelectedItem;
        }

        private void CBDstFactor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.DstFactor = (GXBlendFactor)CBDstFactor.SelectedItem;
        }

        private void CBBlendMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.BlendMode = (GXBlendMode)CBBlendMode.SelectedItem;
        }

        private void CBBlendOp_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.BlendOp = (GXLogicOp)CBBlendOp.SelectedItem;
        }

        private void CBDepthFunc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.DepthFunction = (GXCompareType)CBDepthFunc.SelectedItem;
        }

        private void TBDstAlpha_TextChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.DestinationAlpha = (byte)GuiTools.TryParseTBInt(TBDstAlpha, false);
        }

        private void TBAlphaRef1_TextChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.AlphaRef0 = (byte)GuiTools.TryParseTBInt(TBAlphaRef1, false);
        }

        private void TBAlphaRef2_TextChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.AlphaRef1 = (byte)GuiTools.TryParseTBInt(TBAlphaRef2, false);
        }
    }
}
