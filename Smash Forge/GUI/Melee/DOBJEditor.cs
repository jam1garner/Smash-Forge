using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MeleeLib.GCX;
using MeleeLib.DAT;
using SmashForge.Filetypes.Melee;

namespace SmashForge.Gui.Melee
{

    public partial class DOBJEditor : Form
    {
        private TextureNode selectedTexture = null;
        private MeleeDataObjectNode meleeDataObjectNode = null;



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

            foreach (var cbv in Enum.GetValues(typeof(GXTexMapID))) CBTexID.Items.Add(cbv);
            foreach (var cbv in Enum.GetValues(typeof(GXTexGenSrc))) CBTexGenSrc.Items.Add(cbv);
            foreach (var cbv in Enum.GetValues(typeof(GXTexFilter)))
            {
                CBTexMag.Items.Add(cbv);
                CBMinFilter.Items.Add(cbv);
            }
            foreach (var cbv in Enum.GetValues(typeof(GXAnisotropy))) CBAnisotrophy.Items.Add(cbv);

            Reload();

            if (DOBJ.Material.Textures.Length > 0)
                textureListBox.SelectedIndex = 0;

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

            foreach (DatTexture t in DOBJ.Material.Textures)
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
            EnabledPanelContents(tabControl2, false);
            ButtonChooseTexture.Enabled = false;

            if (textureListBox.SelectedItem != null)
            {
                EnabledPanelContents(tabControl2, true);
                ButtonChooseTexture.Enabled = true;
                selectedTexture = (TextureNode)textureListBox.SelectedItem;
                if (selectedTexture != null && selectedTexture.Texture != null && selectedTexture.Texture.ImageData != null)
                    TempBitmap = selectedTexture.Texture.GetStaticBitmap();

                textureFlagsTB.Text = selectedTexture.Texture.UnkFlags.ToString("X");

                CBWrapS.SelectedItem = (selectedTexture.Texture.WrapS);
                CBWrapT.SelectedItem = (selectedTexture.Texture.WrapT);

                CBTexGenSrc.SelectedIndex = selectedTexture.Texture.GXTexGenSrc;
                CBTexID.SelectedItem = selectedTexture.Texture.TexMapID;
                CBTexMag.SelectedItem = selectedTexture.Texture.MagFilter;
                TBBlending.Text = selectedTexture.Texture.Blending.ToString();

                TBTX.Text = selectedTexture.Texture.TX.ToString();
                TBTY.Text = selectedTexture.Texture.TY.ToString();
                TBTZ.Text = selectedTexture.Texture.TZ.ToString();
                TBRX.Text = selectedTexture.Texture.RX.ToString();
                TBRY.Text = selectedTexture.Texture.RY.ToString();
                TBRZ.Text = selectedTexture.Texture.RZ.ToString();
                TBSX.Text = selectedTexture.Texture.SX.ToString();
                TBSY.Text = selectedTexture.Texture.SY.ToString();
                TBSZ.Text = selectedTexture.Texture.SZ.ToString();

                CBEnableLOD.Checked = (selectedTexture.Texture.LOD != null);
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

        }

        private void CBWrapT_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CBWrapT.SelectedItem == null)
                return;
            selectedTexture.Texture.WrapT = (GXWrapMode)CBWrapT.SelectedItem;
            meleeDataObjectNode.RefreshRenderTextures();
        }

        private void CBWrapS_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CBWrapS.SelectedItem == null)
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
            if (DOBJ.Material.PixelProcessing != null)
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedTexture != null)
                using (MeleeTextureSelector ts = new MeleeTextureSelector(meleeDataObjectNode, selectedTexture))
                {
                    if (ts.ShowDialog() == DialogResult.OK)
                    {
                        meleeDataObjectNode.RefreshRenderTextures();
                        pictureBox1.Image = selectedTexture.Texture.GetStaticBitmap();
                    }
                }
        }

        private void TBBlending_TextChanged(object sender, EventArgs e)
        {
            float v;
            if (float.TryParse(TBBlending.Text, out v))
                selectedTexture.Texture.Blending = v;
        }

        private void CBTexGenSrc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CBTexGenSrc.SelectedItem == null)
                return;
            selectedTexture.Texture.GXTexGenSrc = (int)CBTexGenSrc.SelectedItem;
        }

        private void CBTexID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CBTexID.SelectedItem == null)
                return;
            selectedTexture.Texture.TexMapID = (GXTexMapID)CBTexID.SelectedItem;
        }

        private void CBTexMag_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CBTexMag.SelectedItem == null)
                return;
            selectedTexture.Texture.MagFilter = (GXTexFilter)CBTexMag.SelectedItem;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedTexture == null) return;
            DOBJ.Material.RemoveTexture(selectedTexture.Texture);
            Reload();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DOBJ.Material.AddTexture(new DatTexture());
            Reload();
        }

        private void TBTX_TextChanged(object sender, EventArgs e)
        {
            if (selectedTexture == null) return;
            if (sender == TBTX) selectedTexture.Texture.TX = GuiTools.TryParseTBFloat(TBTX);
            if (sender == TBTY) selectedTexture.Texture.TY = GuiTools.TryParseTBFloat(TBTY);
            if (sender == TBTZ) selectedTexture.Texture.TZ = GuiTools.TryParseTBFloat(TBTZ);
            if (sender == TBRX) selectedTexture.Texture.RX = GuiTools.TryParseTBFloat(TBRX);
            if (sender == TBRY) selectedTexture.Texture.RY = GuiTools.TryParseTBFloat(TBRY);
            if (sender == TBRZ) selectedTexture.Texture.RZ = GuiTools.TryParseTBFloat(TBRZ);
            if (sender == TBSX) selectedTexture.Texture.SX = GuiTools.TryParseTBFloat(TBSX);
            if (sender == TBSY) selectedTexture.Texture.SY = GuiTools.TryParseTBFloat(TBSY);
            if (sender == TBSZ) selectedTexture.Texture.SZ = GuiTools.TryParseTBFloat(TBSZ);
        }

        private void EnabledPanelContents(Control panel, bool enabled)
        {
            Queue<Control> cons = new Queue<Control>();
            cons.Enqueue(panel);
            while (cons.Count > 0)
            {
                Control ctrl = cons.Dequeue();
                ctrl.Enabled = enabled;
                foreach (Control c in ctrl.Controls)
                    cons.Enqueue(c);
            }
        }

        private void CBEnableLOD_CheckedChanged(object sender, EventArgs e)
        {
            if (CBEnableLOD.Checked)
            {
                EnabledPanelContents(tableLayoutPanel4, true);
                if (selectedTexture.Texture.LOD == null)
                    selectedTexture.Texture.LOD = new DatTextureLOD();
                CBAnisotrophy.SelectedItem = selectedTexture.Texture.LOD.Anisotropy;
                CBMinFilter.SelectedItem = selectedTexture.Texture.LOD.MinFilter;
                TBBias.Text = selectedTexture.Texture.LOD.Bias.ToString();
                CBBiasClamp.Checked = selectedTexture.Texture.LOD.BiasClamp;
                CBEnableEdgeLOD.Checked = selectedTexture.Texture.LOD.EnableEdgeLOD;
            }
            else
            {
                EnabledPanelContents(tableLayoutPanel4, false);
                selectedTexture.Texture.LOD = null;
            }
        }

        private void CBEnableEdgeLOD_CheckedChanged(object sender, EventArgs e)
        {
            selectedTexture.Texture.LOD.EnableEdgeLOD = CBEnableEdgeLOD.Checked;
        }

        private void CBBiasClamp_CheckedChanged(object sender, EventArgs e)
        {
            selectedTexture.Texture.LOD.BiasClamp = CBBiasClamp.Checked;
        }

        private void TBBias_TextChanged(object sender, EventArgs e)
        {
            selectedTexture.Texture.LOD.Bias = GuiTools.TryParseTBFloat(TBBias);
        }

        private void CBMinFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedTexture.Texture.LOD.MinFilter = (GXTexFilter)CBMinFilter.SelectedItem;
        }

        private void CBAnisotrophy_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedTexture.Texture.LOD.Anisotropy = (GXAnisotropy)CBAnisotrophy.SelectedItem;
        }
    }
    public enum GXTexGenSrc
    {
        Position = 0,
        Normal = 1,
        Binormal = 2,
        Tangent = 3,
        Tex0 = 4,
        Tex1 = 5,
        Tex2 = 6,
        Tex3 = 7,
        Tex4 = 8,
        Tex5 = 9,
        Tex6 = 10,
        Tex7 = 11,
        TexCoord0 = 12,
        TexCoord1 = 13,
        TexCoord2 = 14,
        TexCoord3 = 15,
        TexCoord4 = 16,
        TexCoord5 = 17,
        TexCoord6 = 18,
        Color0 = 19,
        Color1 = 20,
    }

    public class TextureNode : ListViewItem
    {
        public DatTexture Texture;

        public TextureNode(DatTexture Texture)
        {
            this.Texture = Texture;
            Text = Texture.UnkFlags.ToString("X") + (Texture.ImageData == null ? "" : "_" + Texture.ImageData.Format.ToString());
        }
    }
}
