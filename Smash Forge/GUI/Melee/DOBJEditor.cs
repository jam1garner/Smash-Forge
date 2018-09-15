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

            foreach (var suit in Enum.GetValues(typeof(GXAlphaOp))) alphaOpComboBox.Items.Add(suit);
            foreach (var suit in Enum.GetValues(typeof(GXCompareType)))
            {
                alphaComp1ComboBox.Items.Add(suit); alphaComp2ComboBox.Items.Add(suit); depthFuncComboBox.Items.Add(suit);
            }
            foreach (var suit in Enum.GetValues(typeof(GXBlendFactor)))
            {
                srcFactorComboBox.Items.Add(suit);
                dstFactorComboBox.Items.Add(suit);
            }
            foreach (var suit in Enum.GetValues(typeof(GXLogicOp))) blendOpComboBox.Items.Add(suit);
            foreach (var suit in Enum.GetValues(typeof(GXBlendMode))) blendModeComboBox.Items.Add(suit);

            Reload();
            pixelProcessingTableLayout.Visible = pixelProcessingCB.Checked;
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

            pixelProcessingCB.Checked = DOBJ.Material.PixelProcessing != null;
        }

        private void RefreshPixelProcessingValues()
        {
            pixelFlagsTB.Text = DOBJ.Material.PixelProcessing.Flags.ToString("X");
            alphaOpComboBox.SelectedItem = DOBJ.Material.PixelProcessing.AlphaOp;
            alphaComp1ComboBox.SelectedItem = DOBJ.Material.PixelProcessing.AlphaComp0;
            alphaComp2ComboBox.SelectedItem = DOBJ.Material.PixelProcessing.AlphaComp1;
            blendModeComboBox.SelectedItem = DOBJ.Material.PixelProcessing.BlendMode;
            blendOpComboBox.SelectedItem = DOBJ.Material.PixelProcessing.BlendOp;
            depthFuncComboBox.SelectedItem = DOBJ.Material.PixelProcessing.DepthFunction;
            alphaRef1TB.Text = DOBJ.Material.PixelProcessing.AlphaRef0.ToString();
            alphaRef2TB.Text = DOBJ.Material.PixelProcessing.AlphaRef1.ToString();
            dstAlphaTB.Text = DOBJ.Material.PixelProcessing.DestinationAlpha.ToString();
            srcFactorComboBox.SelectedItem = DOBJ.Material.PixelProcessing.SrcFactor;
            dstFactorComboBox.SelectedItem = DOBJ.Material.PixelProcessing.DstFactor;
        }

        private void textureListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TempBitmap != null)
            {
                //TempBitmap.Dispose();
                TempBitmap = null;
            }

            if(textureListBox.SelectedItem != null)
            {
                selectedTexture = (TextureNode)textureListBox.SelectedItem;
                if (selectedTexture != null && selectedTexture.Texture != null)
                    TempBitmap = selectedTexture.Texture.GetStaticBitmap();

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
            {
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
                        selectedTexture.Texture.SetFromDXT1(new FileData(d.bdata).getSection(0, (int)(d.header.width * d.header.height / 2)), (int)d.header.width, (int)d.header.height);
                        meleeDataObjectNode.RefreshRenderTextures();
                    }
                }
            }
        }

        private void CBWrapT_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedTexture == null || CBWrapT.SelectedItem == null)
                return;
            selectedTexture.Texture.WrapT = (GXWrapMode)CBWrapT.SelectedItem;
            meleeDataObjectNode.RefreshRenderTextures();
        }

        private void CBWrapS_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedTexture == null || CBWrapS.SelectedItem == null)
                return;
            selectedTexture.Texture.WrapS = (GXWrapMode)CBWrapS.SelectedItem;
            meleeDataObjectNode.RefreshRenderTextures();
        }

        private void pixelProcessingCB_CheckedChanged(object sender, EventArgs e)
        {
            pixelProcessingTableLayout.Visible = pixelProcessingCB.Checked;

            if (pixelProcessingCB.Checked)
            {
                if (DOBJ.Material.PixelProcessing == null)
                    DOBJ.Material.PixelProcessing = new DatPixelProcessing();
                RefreshPixelProcessingValues();
            }
            else
                DOBJ.Material.PixelProcessing = null;

        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void TBPixelFlags_TextChanged(object sender, EventArgs e)
        {
            if(DOBJ.Material.PixelProcessing != null)
            DOBJ.Material.PixelProcessing.Flags = (byte)GuiTools.TryParseTBInt(pixelFlagsTB, true);
        }

        private void CBAlphaOp_SelectedValueChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.AlphaOp = (GXAlphaOp)alphaOpComboBox.SelectedItem;
        }

        private void CBAlphaComp1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.AlphaComp0 = (GXCompareType)alphaComp1ComboBox.SelectedItem;
        }

        private void CBAlphaComp2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.AlphaComp1 = (GXCompareType)alphaComp2ComboBox.SelectedItem;
        }

        private void CBSrcFactor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.SrcFactor = (GXBlendFactor)srcFactorComboBox.SelectedItem;
        }

        private void CBDstFactor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.DstFactor = (GXBlendFactor)dstFactorComboBox.SelectedItem;
        }

        private void CBBlendMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.BlendMode = (GXBlendMode)blendModeComboBox.SelectedItem;
        }

        private void CBBlendOp_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.BlendOp = (GXLogicOp)blendOpComboBox.SelectedItem;
        }

        private void CBDepthFunc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.DepthFunction = (GXCompareType)depthFuncComboBox.SelectedItem;
        }

        private void TBDstAlpha_TextChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.DestinationAlpha = (byte)GuiTools.TryParseTBInt(dstAlphaTB, false);
        }

        private void TBAlphaRef1_TextChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.AlphaRef0 = (byte)GuiTools.TryParseTBInt(alphaRef1TB, false);
        }

        private void TBAlphaRef2_TextChanged(object sender, EventArgs e)
        {
            if (DOBJ.Material.PixelProcessing != null)
                DOBJ.Material.PixelProcessing.AlphaRef1 = (byte)GuiTools.TryParseTBInt(alphaRef2TB, false);
        }
    }
}
