using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Smash_Forge.GUI.Menus;

namespace Smash_Forge.GUI
{
    public partial class RenderSettings : Form
    {
        private bool disableRuntimeUpdates;
        private List<Color> hitboxColors;
        List<string> areaLightIDList = new List<string>();


        public RenderSettings()
        {
            InitializeComponent();

            disableRuntimeUpdates = true;
            nudHitboxAlpha.Value = Runtime.hitboxAlpha;
            nudHurtboxAlpha.Value = Runtime.hurtboxAlpha;

            checkBox1.Checked = Runtime.renderModel;
            checkBox2.Checked = Runtime.renderBones;
            checkBox3.Checked = Runtime.renderPath;
            checkBox4.Checked = Runtime.renderHitboxes;
            checkBox5.Checked = Runtime.renderFloor;
            backgroundCB.Checked = Runtime.renderBackGround;
            checkBox6.Checked = Runtime.renderLVD;
            checkBox7.Checked = Runtime.renderCollisions;
            checkBox8.Checked = Runtime.renderSpawns;
            checkBox9.Checked = Runtime.renderRespawns;
            checkBox10.Checked = Runtime.renderItemSpawners;
            checkBox11.Checked = Runtime.renderGeneralPoints;
            checkBox12.Checked = Runtime.renderCollisionNormals;
            checkBox13.Checked = Runtime.renderHurtboxes;
            checkBox14.Checked = Runtime.renderHurtboxesZone;
            checkBox15.Checked = Runtime.renderECB;
            checkBox16.Checked = Runtime.renderInterpolatedHitboxes;
            checkBox17.Checked = Runtime.renderSpecialBubbles;
            checkBox18.Checked = Runtime.renderHitboxesNoOverlap;
            checkBox19.Checked = Runtime.renderHitboxAngles;
            checkBox21.Checked = Runtime.renderLedgeGrabboxes;
            checkBox22.Checked = Runtime.renderTetherLedgeGrabboxes;
            checkBox23.Checked = Runtime.renderReverseLedgeGrabboxes;
            swagViewing.Checked = Runtime.renderSwag;
            materialLightingCB.Checked = Runtime.renderMaterialLighting;
            useNormCB.Checked = Runtime.renderNormalMap;
            boundingCB.Checked = Runtime.renderBoundingBox;
            wireframeCB.Checked = Runtime.renderModelWireframe;
            modelSelectCB.Checked = Runtime.renderModelSelection;
            wireframeCB.Enabled = checkBox1.Checked;
            modelSelectCB.Enabled = checkBox1.Checked;
            renderFogCB.Checked = Runtime.renderFog;

            UpdateDebugButtonsFromRenderType();

            cameraLightCB.Checked = Runtime.cameraLight;
            diffuseCB.Checked = Runtime.renderDiffuse;
            specularCB.Checked = Runtime.renderSpecular;
            fresnelCB.Checked = Runtime.renderFresnel;
            reflectionCB.Checked = Runtime.renderReflection;
            ambTB.Text = Runtime.ambInten + "";
            difTB.Text = Runtime.difInten + "";
            spcTB.Text = Runtime.spcInten + "";
            frsTB.Text = Runtime.frsInten + "";
            refTB.Text = Runtime.ref_inten + "";

            RendererLabel.Text = "Renderer: " + Runtime.renderer;
            OpenGLVersionLabel.Text = "OpenGL Version: " + Runtime.GLSLVersion;

            depthTestCB.Checked = Runtime.useDepthTest;
            zScaleTB.Text = Runtime.zScale + "";

            renderAlphaCB.Checked = Runtime.renderAlpha;
            vertColorCB.Checked = Runtime.renderVertColor;
            renderModeComboBox.SelectedIndex = (int)Runtime.renderType;

            pbHurtboxColor.BackColor = Runtime.hurtboxColor;
            pbHurtboxColorHi.BackColor = Runtime.hurtboxColorHi;
            pbHurtboxColorMed.BackColor = Runtime.hurtboxColorMed;
            pbHurtboxColorLw.BackColor = Runtime.hurtboxColorLow;
            pbHurtboxColorSelected.BackColor = Runtime.hurtboxColorSelected;
            pbHitboxAnglesColor.BackColor = Runtime.hitboxAnglesColor;
            pbWindboxColor.BackColor = Runtime.windboxColor;
            pbGrabboxColor.BackColor = Runtime.grabboxColor;
            pbSearchboxColor.BackColor = Runtime.searchboxColor;

            pbCounterColor.BackColor = Runtime.counterBubbleColor;
            pbReflectColor.BackColor = Runtime.reflectBubbleColor;
            pbAbsorbColor.BackColor = Runtime.absorbBubbleColor;
            pbShieldColor.BackColor = Runtime.shieldBubbleColor;

            textParamDir.Text = Runtime.paramDir;
            disableRuntimeUpdates = false;




        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            //Disable all the checkboxes for LVD
            checkChanged();
            checkBox7.Enabled = checkBox6.Checked;
            checkBox8.Enabled = checkBox6.Checked;
            checkBox9.Enabled = checkBox6.Checked;
            checkBox10.Enabled = checkBox6.Checked;
            checkBox11.Enabled = checkBox6.Checked;
            checkBox12.Enabled = checkBox6.Checked && checkBox7.Checked;
        }

        private void checkChanged()
        {
            if (!disableRuntimeUpdates)
            {
                Runtime.renderModel = checkBox1.Checked;
                Runtime.renderBones = checkBox2.Checked;
                Runtime.renderHitboxes = checkBox4.Checked;
                Runtime.renderPath = checkBox3.Checked;
                Runtime.renderFloor = checkBox5.Checked;
                Runtime.renderLVD = checkBox6.Checked;
                Runtime.renderCollisions = checkBox7.Checked;
                Runtime.renderSpawns = checkBox8.Checked;
                Runtime.renderRespawns = checkBox9.Checked;
                Runtime.renderItemSpawners = checkBox10.Checked;
                Runtime.renderGeneralPoints = checkBox11.Checked;
                Runtime.renderCollisionNormals = checkBox12.Checked;
                Runtime.renderHurtboxes = checkBox13.Checked;
                Runtime.renderHurtboxesZone = checkBox14.Checked;
                Runtime.renderECB = checkBox15.Checked;
                Runtime.renderInterpolatedHitboxes = checkBox16.Checked;
                Runtime.renderSpecialBubbles = checkBox17.Checked;
                Runtime.renderHitboxesNoOverlap = checkBox18.Checked;
                Runtime.renderHitboxAngles = checkBox19.Checked;
                Runtime.renderLedgeGrabboxes = checkBox21.Checked;
                Runtime.renderTetherLedgeGrabboxes = checkBox22.Checked;
                Runtime.renderReverseLedgeGrabboxes = checkBox23.Checked;
            }
            checkBox12.Enabled = checkBox6.Checked && checkBox7.Checked;
            wireframeCB.Enabled = checkBox1.Checked;
            modelSelectCB.Enabled = checkBox1.Checked;
        }

        private void checkChanged(object sender, EventArgs e)
        {
            checkChanged();
        }

        private void populateColorsFromRuntime()
        {
            listViewKbColors.Items.Clear();
            
            switch (Runtime.hitboxRenderMode)
            {
                case Hitbox.RENDER_NORMAL:
                    // disable controls
                    hitboxColors = new List<Color>();
                    break;
                case Hitbox.RENDER_KNOCKBACK:
                    // enable controls
                    hitboxColors = Runtime.hitboxKnockbackColors;
                    break;
                case Hitbox.RENDER_ID:
                    // enable controls
                    hitboxColors = Runtime.hitboxIdColors;
                    break;
            }
            // Populate
            foreach (Color c in hitboxColors)
            {
                ListViewItem color = new ListViewItem(System.Drawing.ColorTranslator.ToHtml(c));
                color.BackColor = Color.FromArgb(Runtime.hitboxAlpha, c);
                listViewKbColors.Items.Add(color);
            }

            // Ensure columns resize
            listViewKbColors.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            // Disable horizontal scroll bar
            listViewKbColors.Columns[0].Width = listViewKbColors.Columns[0].Width - 4;
        }

        private void RenderSettings_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = Runtime.hitboxRenderMode;
            populateColorsFromRuntime();
        }

        private void renderMode_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Runtime.renderType = (Runtime.RenderTypes)renderModeComboBox.SelectedIndex;

            UpdateDebugButtonsFromRenderType();

        }

        private void UpdateDebugButtonsFromRenderType()
        {
            ShowHideDebugButtonsFromRenderType();

            if (Runtime.renderType == Runtime.RenderTypes.UVCoords || Runtime.renderType == Runtime.RenderTypes.UVTestPattern)
            {
                debug1CB.Visible = false;
                debug2CB.Visible = false;

                radioButton1.Visible = true;
                radioButton2.Visible = true;
                radioButton3.Visible = true;
            }
            else if (Runtime.renderType == Runtime.RenderTypes.AmbientOcclusion)
            {
                debug1CB.Text = "aoMinGain";
                debug1CB.Visible = true;
                debug2CB.Visible = false;

                radioButton1.Visible = false;
                radioButton2.Visible = false;
                radioButton3.Visible = false;
            }
            else if (Runtime.renderType == Runtime.RenderTypes.SelectedBoneWeights)
            {
                debug1CB.Text = "Color Ramp";
                debug1CB.Visible = false;
                debug2CB.Visible = false;

                radioButton1.Visible = true;
                radioButton2.Visible = true;
                radioButton3.Visible = true;

                radioButton1.Text = "BnW";
                radioButton2.Text = "Color 1";
                radioButton3.Text = "Color 2";
            }
            else if (Runtime.renderType == Runtime.RenderTypes.Normals)
            {
                debug1CB.Visible = false;
                debug2CB.Visible = false;

                radioButton1.Visible = false;
                radioButton2.Visible = false;
                radioButton3.Visible = false;
            }
            else
            {
                debug1CB.Visible = false;
                debug2CB.Visible = false;

                radioButton1.Visible = false;
                radioButton2.Visible = false;
                radioButton3.Visible = false;
            }


        }

        private void ShowHideDebugButtonsFromRenderType()
        {
            renderChannelR.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            renderChannelG.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            renderChannelB.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            renderChannelA.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            debug1CB.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            debug2CB.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton1.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton2.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton3.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton1.Checked = Runtime.uvChannel == Runtime.UVChannel.Channel1 && Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton2.Checked = Runtime.uvChannel == Runtime.UVChannel.Channel2 && Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton3.Checked = Runtime.uvChannel == Runtime.UVChannel.Channel3 && Runtime.renderType != Runtime.RenderTypes.Shaded;
        }

        private void swagViewing_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderSwag = swagViewing.Checked;
        }

        private void lightCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderMaterialLighting = materialLightingCB.Checked;
            
            diffuseCB.Enabled = materialLightingCB.Checked;
            fresnelCB.Enabled = materialLightingCB.Checked;
            specularCB.Enabled = materialLightingCB.Checked;
            reflectionCB.Enabled = materialLightingCB.Checked;
        }

        private void cb_normals_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderAlpha= renderAlphaCB.Checked;
        }

        private void cb_vertcolor_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderVertColor = vertColorCB.Checked;
        }

        private void diffuseCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderDiffuse = diffuseCB.Checked;
        }

        private void fresnelCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderFresnel = fresnelCB.Checked;
        }

        private void specularCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderSpecular = specularCB.Checked;
        }

        private void reflectionCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderReflection = reflectionCB.Checked;
        }

        private void useNormCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderNormalMap = useNormCB.Checked;
        }

        private void backgroundCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderBackGround = backgroundCB.Checked;
        }

        private void boundingCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderBoundingBox = boundingCB.Checked;
        }

        private void modelSelectCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderModelSelection = modelSelectCB.Checked;
        }

        private void wireframeCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderModelWireframe = wireframeCB.Checked;
        }

        private void cameraLightCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.cameraLight = cameraLightCB.Checked;
        }

        private void difTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(difTB.Text, out i))
            {
                difTB.BackColor = Color.White;
                Runtime.difInten = i;
            }
            else
                difTB.BackColor = Color.Red;
        }

        private void spcTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(spcTB.Text, out i))
            {
                spcTB.BackColor = Color.White;
                Runtime.spcInten = i;
            }
            else
                spcTB.BackColor = Color.Red;
        }

        private void frsTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(frsTB.Text, out i))
            {
                frsTB.BackColor = Color.White;
                Runtime.frsInten = i;
            }
            else
                frsTB.BackColor = Color.Red;
        }

        private void ambTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(ambTB.Text, out i))
            {
                ambTB.BackColor = Color.White;
                Runtime.ambInten = i;
            }
            else
                ambTB.BackColor = Color.Red;
        }

        private void refTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(refTB.Text, out i))
            {
                refTB.BackColor = Color.White;
                Runtime.ref_inten = i;
            }
            else
                refTB.BackColor = Color.Red;
        }

        private void modelscaleTB_TextChanged(object sender, EventArgs e)
        {
            if (disableRuntimeUpdates)
                return;

            float i = 0;
            if (float.TryParse(modelscaleTB.Text, out i))
            {
                modelscaleTB.BackColor = Color.White;
                Runtime.model_scale = i;
            }
            else
                modelscaleTB.BackColor = Color.Red;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            Runtime.hitboxRenderMode = cb.SelectedIndex;
            populateColorsFromRuntime();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ColorDialog hitboxColorDialog = new ColorDialog();
            if (hitboxColorDialog.ShowDialog() == DialogResult.OK)
            {
                hitboxColors.Add(hitboxColorDialog.Color);
                populateColorsFromRuntime();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listViewKbColors.Items.Count <= 1)
                return;  // don't allow no colours for hitboxes
            int index = listViewKbColors.SelectedIndices[0];
            hitboxColors.RemoveAt(index);
            populateColorsFromRuntime();
            int newSelectionIndex = index - 1 <= 0 ? 0 : index - 1;
            listViewKbColors.Items[newSelectionIndex].Selected = true;
        }

        private void btnColorUp_Click(object sender, EventArgs e)
        {
            int index = listViewKbColors.SelectedIndices[0];
            if (index < 1)
                return;
            Color color = hitboxColors.ElementAt(index);
            hitboxColors.RemoveAt(index);
            hitboxColors.Insert(index - 1, color);
            populateColorsFromRuntime();
            listViewKbColors.Items[index - 1].Selected = true;
        }

        private void btnColorDown_Click(object sender, EventArgs e)
        {
            int index = listViewKbColors.SelectedIndices[0];
            if (index >= listViewKbColors.Items.Count - 1)
                return;
            Color color = hitboxColors.ElementAt(index);
            hitboxColors.RemoveAt(index);
            hitboxColors.Insert(index + 1, color);
            populateColorsFromRuntime();
            listViewKbColors.Items[index + 1].Selected = true;
        }

        private void nudHitboxAlpha_ValueChanged(object sender, EventArgs e)
        {
            if (!disableRuntimeUpdates)
            {
                Runtime.hitboxAlpha = (int)nudHitboxAlpha.Value;
            }
        }

        private void nudHurtboxAlpha_ValueChanged(object sender, EventArgs e)
        {
            if (!disableRuntimeUpdates)
            {
                Runtime.hurtboxAlpha = (int)nudHurtboxAlpha.Value;
            }
        }

        private void pbHurtboxColor_Click(object sender, EventArgs e)
        {
            ColorDialog hurtboxColorDialog = new ColorDialog();
            if (hurtboxColorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.hurtboxColor = Color.FromArgb(0xFF, hurtboxColorDialog.Color);
                pbHurtboxColor.BackColor = Runtime.hurtboxColor;
            }
        }

        private void pbHurtboxColorLw_Click(object sender, EventArgs e)
        {
            ColorDialog hurtboxColorDialog = new ColorDialog();
            if (hurtboxColorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.hurtboxColorLow = Color.FromArgb(0xFF, hurtboxColorDialog.Color);
                pbHurtboxColor.BackColor = Runtime.hurtboxColorLow;
            }
        }

        private void pbHurtboxColorMed_Click(object sender, EventArgs e)
        {
            ColorDialog hurtboxColorDialog = new ColorDialog();
            if (hurtboxColorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.hurtboxColorMed = Color.FromArgb(0xFF, hurtboxColorDialog.Color);
                pbHurtboxColor.BackColor = Runtime.hurtboxColorMed;
            }
        }

        private void pbHurtboxColorHi_Click(object sender, EventArgs e)
        {
            ColorDialog hurtboxColorDialog = new ColorDialog();
            if (hurtboxColorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.hurtboxColorHi = Color.FromArgb(0xFF, hurtboxColorDialog.Color);
                pbHurtboxColor.BackColor = Runtime.hurtboxColorHi;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Runtime.paramDir = textParamDir.Text;
        }

        private void pbHurtboxColorSelected_Click(object sender, EventArgs e)
        {
            ColorDialog hurtboxColorDialog = new ColorDialog();
            if (hurtboxColorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.hurtboxColorSelected = Color.FromArgb(0xFF, hurtboxColorDialog.Color);
                pbHurtboxColorSelected.BackColor = Runtime.hurtboxColorSelected;
            }
        }

        private void label35_Click(object sender, EventArgs e)
        {

        }

      
        private void label33_Click(object sender, EventArgs e)
        {

        }

       
        private void label47_Click(object sender, EventArgs e)
        {

        }

        public int Clamp(float i)
        {
            i *= 255;
            i = (int)i;
            if (i > 255)
                return 255;
            if (i < 0)
                return 0;
            return (int)i;
        }


        private void pbWindboxColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.windboxColor = Color.FromArgb(0xFF, colorDialog.Color);
                pbWindboxColor.BackColor = Runtime.windboxColor;
            }
        }

        private void pbGrabboxColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.grabboxColor = Color.FromArgb(0xFF, colorDialog.Color);
                pbGrabboxColor.BackColor = Runtime.grabboxColor;
            }
        }

        private void pbSearchboxColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.searchboxColor = Color.FromArgb(0xFF, colorDialog.Color);
                pbSearchboxColor.BackColor = Runtime.searchboxColor;
            }
        }

        private void depthTestCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.useDepthTest = depthTestCB.Checked;
        }

        private void zScaleTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(zScaleTB.Text, out i))
            {
                zScaleTB.BackColor = Color.White;
                Runtime.zScale = i;
            }
            else
                zScaleTB.BackColor = Color.Red;
        }

        private void difColorButton_Click(object sender, EventArgs e)
        {           
            /*
            if (colorForm == null || colorForm.IsDisposed)
                colorForm = new LightColorEditor(VBNViewport.diffuseLight);
            colorForm.Show();*/
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
    
        }




        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

      

        private void depthSlider_Scroll(object sender, EventArgs e)
        {

        }

        private void fovLabel_Click(object sender, EventArgs e)
        {

        }

        private void pbCounterColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.counterBubbleColor = Color.FromArgb(0xFF, colorDialog.Color);
                pbCounterColor.BackColor = Runtime.counterBubbleColor;
            }
        }

        private void pbReflectColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.reflectBubbleColor = Color.FromArgb(0xFF, colorDialog.Color);
                pbReflectColor.BackColor = Runtime.reflectBubbleColor;
            }
        }

        private void pbAbsorbColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.absorbBubbleColor = Color.FromArgb(0xFF, colorDialog.Color);
                pbAbsorbColor.BackColor = Runtime.absorbBubbleColor;
            }
        }

        private void pbShieldColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.shieldBubbleColor = Color.FromArgb(0xFF, colorDialog.Color);
                pbShieldColor.BackColor = Runtime.shieldBubbleColor;
            }
        }

        private void areaLightBoundingBoxCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.drawAreaLightBoundingBoxes = areaLightBoundingBoxCB.Checked;
        }

        private void stageLightingCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderStageLighting = stageLightingCB.Checked;

            renderFogCB.Enabled = stageLightingCB.Checked;
        }

        private void renderFogCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderFog = renderFogCB.Checked;
        }

        private void renderChannelR_Click(object sender, EventArgs e)
        {
            if (Runtime.renderR)
            {
                Runtime.renderR = false;
                renderChannelR.ForeColor = Color.DarkGray;
            }

            else
            {
                Runtime.renderR = true;
                renderChannelR.ForeColor = Color.Red;
            }
        }

        private void renderChannelG_Click(object sender, EventArgs e)
        {
            if (Runtime.renderG)
            {
                Runtime.renderG = false;
                renderChannelG.ForeColor = Color.DarkGray;
            }
            else
            {
                Runtime.renderG = true;
                renderChannelG.ForeColor = Color.Green;
            }
        }

        private void renderChannelB_Click(object sender, EventArgs e)
        {
            if (Runtime.renderB)
            {
                Runtime.renderB = false;
                renderChannelB.ForeColor = Color.DarkGray;
            }

            else
            {
                Runtime.renderB = true;
                renderChannelB.ForeColor = Color.Blue;
            }
        }

        private void renderChannelA_Click(object sender, EventArgs e)
        {
            if (Runtime.renderAlpha)
            {
                Runtime.renderAlpha = false;
                renderChannelA.ForeColor = Color.DarkGray;
            }

            else
            {
                Runtime.renderAlpha = true;
                renderChannelA.ForeColor = Color.Black;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.uvChannel = Runtime.UVChannel.Channel1;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.uvChannel = Runtime.UVChannel.Channel2;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.uvChannel = Runtime.UVChannel.Channel3;
        }

        private void debugShadingCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.useDebugShading = Runtime.renderType != Runtime.RenderTypes.Shaded;
            if (Runtime.renderType == Runtime.RenderTypes.Shaded)
            {
                Runtime.renderType = Runtime.RenderTypes.Normals;
                renderModeComboBox.SelectedIndex = (int)Runtime.RenderTypes.Normals;
            }

            debugModeLabel.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            renderChannelR.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            renderChannelG.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            renderChannelB.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            renderChannelA.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            renderModeComboBox.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            debug1CB.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            debug2CB.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton1.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton2.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton3.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
        }

        private void debug1CB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.debug1 = debug1CB.Checked;
        }

        private void debug2CB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.debug2 = debug2CB.Checked;
        }

        private void pbHitboxAnglesColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.hitboxAnglesColor = Color.FromArgb(0xFF, colorDialog.Color);
                pbHitboxAnglesColor.BackColor = Runtime.hitboxAnglesColor;
            }
        }
    }
    
}
