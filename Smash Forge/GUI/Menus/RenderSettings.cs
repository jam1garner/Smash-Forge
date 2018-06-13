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
using SFGraphics.GLObjects.Textures;

namespace Smash_Forge.GUI
{
    public partial class RenderSettings : Form
    {
        private bool disableRuntimeUpdates;
        private List<Color> hitboxColors;

        public RenderSettings()
        {
            InitializeComponent();
            disableRuntimeUpdates = true;

            // Misc Settings
            renderCameraPathCB.Checked = Runtime.renderPath;
            drawUvCB.Checked = Runtime.drawUv;
            textParamDir.Text = Runtime.paramDir;
            BackgroundGradient1.BackColor = Runtime.backgroundGradientTop;
            BackgroundGradient2.BackColor = Runtime.backgroundGradientBottom;
            floorColorPictureBox.BackColor = Runtime.floorColor;
            modelscaleTB.Text = Runtime.modelScale + "";

            // Bone settings
            renderBonesCB.Checked = Runtime.renderBones;
            showSwagDataCB.Checked = Runtime.renderSwagZ;
            swagYCB.Checked = Runtime.renderSwagY;

            // Model Settings
            renderModelCB.Checked = Runtime.renderModel;
            boundingCB.Checked = Runtime.renderBoundingBox;
            wireframeCB.Checked = Runtime.renderModelWireframe;
            modelSelectCB.Checked = Runtime.renderModelSelection;
            wireframeCB.Enabled = renderModelCB.Checked;
            modelSelectCB.Enabled = renderModelCB.Checked;

            // Hitbox Settings
            renderHitboxesCB.Checked = Runtime.renderHitboxes;
            nudHitboxAlpha.Value = Runtime.hitboxAlpha;
            pbHitboxAnglesColor.BackColor = Runtime.hitboxAnglesColor;

            // Hurtbox Settings
            renderHurtboxesCB.Checked = Runtime.renderHurtboxes;
            renderHurtboxZonesCB.Checked = Runtime.renderHurtboxesZone;
            nudHurtboxAlpha.Value = Runtime.hurtboxAlpha;
            pbHurtboxColor.BackColor = Runtime.hurtboxColor;
            pbHurtboxColorHi.BackColor = Runtime.hurtboxColorHi;
            pbHurtboxColorMed.BackColor = Runtime.hurtboxColorMed;
            pbHurtboxColorLw.BackColor = Runtime.hurtboxColorLow;
            pbHurtboxColorSelected.BackColor = Runtime.hurtboxColorSelected;

            // Misc Hitbox/Hurtbox Settings
            renderEnvCollisionBoxCB.Checked = Runtime.renderECB;
            renderInterpHitboxCB.Checked = Runtime.renderInterpolatedHitboxes;
            renderSpecialBubblesCB.Checked = Runtime.renderSpecialBubbles;
            renderPriorityTopCB.Checked = Runtime.renderHitboxesNoOverlap;
            hitboxAnglesCB.Checked = Runtime.renderHitboxAngles;
            renderLedgeGrabCB.Checked = Runtime.renderLedgeGrabboxes;
            renderTetherLedgeCB.Checked = Runtime.renderTetherLedgeGrabboxes;
            renderReverseLedgeGrabCB.Checked = Runtime.renderReverseLedgeGrabboxes;
            pbWindboxColor.BackColor = Runtime.windboxColor;
            pbGrabboxColor.BackColor = Runtime.grabboxColor;
            pbSearchboxColor.BackColor = Runtime.searchboxColor;
            pbCounterColor.BackColor = Runtime.counterBubbleColor;
            pbReflectColor.BackColor = Runtime.reflectBubbleColor;
            pbAbsorbColor.BackColor = Runtime.absorbBubbleColor;
            pbShieldColor.BackColor = Runtime.shieldBubbleColor;

            // Discord Settings        
            if (DiscordSettings.imageKeyMode == DiscordSettings.ImageKeyMode.UserPicked)
            {
                customRadioButton.Checked = true;
                customComboBox.Enabled = true;
            }
            else if (DiscordSettings.imageKeyMode == DiscordSettings.ImageKeyMode.Default)
                defaultRadioButton.Checked = true;
            else if (DiscordSettings.imageKeyMode == DiscordSettings.ImageKeyMode.LastFileOpened)
                filenameRadioButton.Checked = true;

            customComboBox.SelectedIndex = customComboBox.Items.IndexOf(DiscordSettings.userPickedImageKey);
            modNameTextBox.Text = DiscordSettings.userNamedMod;
            timeElapsedCheckbox.Checked = DiscordSettings.showTimeElapsed;
            showActiveWindowCheckbox.Checked = DiscordSettings.showCurrentWindow;
            userModCheckbox.Checked = DiscordSettings.useUserModName;

            // LVD Settings
            renderLvdCB.Checked = Runtime.renderLVD;
            checkChanged();

            renderCollisionsCB.Checked = Runtime.renderCollisions;
            renderSpawnsCB.Checked = Runtime.renderSpawns;
            renderRespawnsCB.Checked = Runtime.renderRespawns;
            renderItemSpawnersCB.Checked = Runtime.renderItemSpawners;
            renderGeneralShapesCB.Checked = Runtime.renderGeneralPoints;
            renderPassthroughCB.Checked = Runtime.renderCollisionNormals;

            // Material Lighting Settings
            materialLightingCB.Checked = Runtime.renderMaterialLighting;
            useNormCB.Checked = Runtime.renderNormalMap;
            cameraLightCB.Checked = Runtime.cameraLight;
            diffuseCB.Checked = Runtime.renderDiffuse;
            specularCB.Checked = Runtime.renderSpecular;
            fresnelCB.Checked = Runtime.renderFresnel;
            reflectionCB.Checked = Runtime.renderReflection;
            renderFogCB.Checked = Runtime.renderFog;
            stageLightingCB.Checked = Runtime.renderStageLighting;

            ambTB.Text = Runtime.ambItensity + "";
            difTB.Text = Runtime.difIntensity + "";
            spcTB.Text = Runtime.spcIntentensity + "";
            frsTB.Text = Runtime.frsIntensity + "";
            refTB.Text = Runtime.refIntensity + "";
            depthTestCB.Checked = Runtime.useDepthTest;
            zScaleTB.Text = Runtime.zScale + "";
            renderAlphaCB.Checked = Runtime.renderAlpha;
            vertColorCB.Checked = Runtime.renderVertColor;
            renderModeComboBox.SelectedIndex = (int)Runtime.renderType;
            UpdateDebugButtonsFromRenderType();

            // Post Processing Settings
            postProcessingCB.Checked = Runtime.usePostProcessing;
            bloomCB.Checked = Runtime.renderBloom;
            bloomGroupBox.Enabled = postProcessingCB.Checked;
            bloomIntensityTB.Text = Runtime.bloomIntensity + "";
            bloomThresholdTB.Text = Runtime.bloomThreshold + "";

            // Floor Settings
            renderFloorCB.Checked = Runtime.renderFloor;
            floorComboBox.SelectedIndex = (int)Runtime.floorStyle;
            floorScaleTB.Text = Runtime.floorSize + "";
            SetFloorPictureBoxToolTip();

            // Background Settings
            renderBackgroundCB.Checked = Runtime.renderBackGround;
            backgroundComboBox.SelectedIndex = (int)Runtime.backgroundStyle;

            disableRuntimeUpdates = false;
        }

        private void SetFloorPictureBoxToolTip()
        {
            if (Runtime.floorStyle == Runtime.FloorStyle.UserTexture)
                toolTip1.SetToolTip(floorColorPictureBox, "Click to select an image.");
            else
                toolTip1.SetToolTip(floorColorPictureBox, "Click to select a color.");
        }

        private void renderLvdCB_CheckedChanged(object sender, EventArgs e)
        {
            checkChanged();
        }

        private void checkChanged()
        {
            if (!disableRuntimeUpdates)
            {
                Runtime.renderModel = renderModelCB.Checked;
                Runtime.renderBones = renderBonesCB.Checked;
                Runtime.renderHitboxes = renderHitboxesCB.Checked;
                Runtime.renderPath = renderCameraPathCB.Checked;
                Runtime.renderFloor = renderFloorCB.Checked;
                Runtime.renderLVD = renderLvdCB.Checked;
                Runtime.renderCollisions = renderCollisionsCB.Checked;
                Runtime.renderSpawns = renderSpawnsCB.Checked;
                Runtime.renderRespawns = renderRespawnsCB.Checked;
                Runtime.renderItemSpawners = renderItemSpawnersCB.Checked;
                Runtime.renderGeneralPoints = renderGeneralShapesCB.Checked;
                Runtime.renderCollisionNormals = renderPassthroughCB.Checked;
                Runtime.renderHurtboxes = renderHurtboxesCB.Checked;
                Runtime.renderHurtboxesZone = renderHurtboxZonesCB.Checked;
                Runtime.renderECB = renderEnvCollisionBoxCB.Checked;
                Runtime.renderInterpolatedHitboxes = renderInterpHitboxCB.Checked;
                Runtime.renderSpecialBubbles = renderSpecialBubblesCB.Checked;
                Runtime.renderHitboxesNoOverlap = renderPriorityTopCB.Checked;
                Runtime.renderHitboxAngles = hitboxAnglesCB.Checked;
                Runtime.renderLedgeGrabboxes = renderLedgeGrabCB.Checked;
                Runtime.renderTetherLedgeGrabboxes = renderTetherLedgeCB.Checked;
                Runtime.renderReverseLedgeGrabboxes = renderReverseLedgeGrabCB.Checked;
            }
            renderPassthroughCB.Enabled = renderLvdCB.Checked && renderCollisionsCB.Checked;
            wireframeCB.Enabled = renderModelCB.Checked;
            modelSelectCB.Enabled = renderModelCB.Checked;

            //Disable all the checkboxes for LVD
            renderCollisionsCB.Enabled = renderLvdCB.Checked;
            renderSpawnsCB.Enabled = renderLvdCB.Checked;
            renderRespawnsCB.Enabled = renderLvdCB.Checked;
            renderItemSpawnersCB.Enabled = renderLvdCB.Checked;
            renderGeneralShapesCB.Enabled = renderLvdCB.Checked;
            renderPassthroughCB.Enabled = renderLvdCB.Checked && renderCollisionsCB.Checked;
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
            ShowHideDebugButtonsIfUsingDebugMode();
            DisplayDebugButtonsFromDebugMode();
        }

        private void DisplayDebugButtonsFromDebugMode()
        {

            // Reuse the same buttons to control different settings for each render mode.
            switch (Runtime.renderType)
            {
                default:
                    debug1CB.Enabled = false;

                    radioButton1.Enabled = false;
                    radioButton2.Enabled = false;
                    radioButton3.Enabled = false;
                    break;
                case Runtime.RenderTypes.UVTestPattern:
                    debug1CB.Enabled = false;

                    radioButton1.Enabled = true;
                    radioButton2.Enabled = true;
                    radioButton3.Enabled = true;

                    radioButton1.Text = "UV1";
                    radioButton2.Text = "UV2";
                    radioButton3.Text = "UV3";
                    break;
                case Runtime.RenderTypes.UVCoords:
                    debug1CB.Enabled = false;

                    radioButton1.Enabled = true;
                    radioButton2.Enabled = true;
                    radioButton3.Enabled = true;

                    radioButton1.Text = "UV1";
                    radioButton2.Text = "UV2";
                    radioButton3.Text = "UV3";
                    break;
                case Runtime.RenderTypes.DiffuseMap:
                    debug1CB.Enabled = false;

                    radioButton1.Enabled = true;
                    radioButton2.Enabled = true;
                    radioButton3.Enabled = true;

                    radioButton1.Text = "UV1";
                    radioButton2.Text = "UV2";
                    radioButton3.Text = "UV3";
                    break;
                case Runtime.RenderTypes.AmbientOcclusion:
                    debug1CB.Text = "aoMinGain";
                    debug1CB.Enabled = true;

                    radioButton1.Enabled = false;
                    radioButton2.Enabled = false;
                    radioButton3.Enabled = false;
                    break;
                case Runtime.RenderTypes.SelectedBoneWeights:
                    debug1CB.Text = "Color Ramp";
                    debug1CB.Enabled = false;

                    radioButton1.Enabled = true;
                    radioButton2.Enabled = true;
                    radioButton3.Enabled = true;

                    radioButton1.Text = "BnW";
                    radioButton2.Text = "Color 1";
                    radioButton3.Text = "Color 2";
                    break;
                case Runtime.RenderTypes.Normals:
                    debug1CB.Enabled = false;

                    radioButton1.Enabled = false;
                    radioButton2.Enabled = false;
                    radioButton3.Enabled = false;
                    break;
                case Runtime.RenderTypes.VertColor:
                    debug1CB.Text = "Divide by 2";
                    debug1CB.Enabled = true;

                    radioButton1.Enabled = false;
                    radioButton2.Enabled = false;
                    radioButton3.Enabled = false;
                    break;
            }
        }

        private void ShowHideDebugButtonsIfUsingDebugMode()
        {
            renderChannelR.Enabled = true;
            renderChannelG.Enabled = true;
            renderChannelB.Enabled = true;
            renderChannelA.Enabled = true;
            debug1CB.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton1.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton2.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton3.Enabled = Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton1.Checked = Runtime.uvChannel == Runtime.UVChannel.Channel1 && Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton2.Checked = Runtime.uvChannel == Runtime.UVChannel.Channel2 && Runtime.renderType != Runtime.RenderTypes.Shaded;
            radioButton3.Checked = Runtime.uvChannel == Runtime.UVChannel.Channel3 && Runtime.renderType != Runtime.RenderTypes.Shaded;
        }

        private void swagViewing_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderSwagZ = showSwagDataCB.Checked;
        }

        private void materialLightingCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderMaterialLighting = materialLightingCB.Checked;
            
            diffuseCB.Enabled = materialLightingCB.Checked;
            fresnelCB.Enabled = materialLightingCB.Checked;
            specularCB.Enabled = materialLightingCB.Checked;
            reflectionCB.Enabled = materialLightingCB.Checked;
        }

        private void renderAlphaCB_CheckedChanged(object sender, EventArgs e)
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
            Runtime.renderBackGround = renderBackgroundCB.Checked;
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
            Runtime.difIntensity = GuiTools.TryParseTBFloat(difTB);
        }

        private void spcTB_TextChanged(object sender, EventArgs e)
        {
            Runtime.spcIntentensity = GuiTools.TryParseTBFloat(spcTB);
        }

        private void frsTB_TextChanged(object sender, EventArgs e)
        {
            Runtime.frsIntensity = GuiTools.TryParseTBFloat(frsTB);
        }

        private void ambTB_TextChanged(object sender, EventArgs e)
        {
            Runtime.ambItensity = GuiTools.TryParseTBFloat(ambTB);
        }

        private void refTB_TextChanged(object sender, EventArgs e)
        {
            Runtime.refIntensity = GuiTools.TryParseTBFloat(refTB);
        }

        private void modelscaleTB_TextChanged(object sender, EventArgs e)
        {
            if (disableRuntimeUpdates)
                return;

            Runtime.modelScale = GuiTools.TryParseTBFloat(modelscaleTB);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            Runtime.hitboxRenderMode = cb.SelectedIndex;
            populateColorsFromRuntime();
        }

        private void buttonAddColor_Click(object sender, EventArgs e)
        {
            ColorDialog hitboxColorDialog = new ColorDialog();
            if (hitboxColorDialog.ShowDialog() == DialogResult.OK)
            {
                hitboxColors.Add(hitboxColorDialog.Color);
                populateColorsFromRuntime();
            }
        }

        private void buttonRemoveColor_Click(object sender, EventArgs e)
        {
            if (listViewKbColors.Items.Count <= 1 || listViewKbColors.SelectedIndices.Count == 0)
                return;  // don't allow no colours for hitboxes
            int index = listViewKbColors.SelectedIndices[0];
            hitboxColors.RemoveAt(index);
            populateColorsFromRuntime();
            int newSelectionIndex = index - 1 <= 0 ? 0 : index - 1;
            listViewKbColors.Items[newSelectionIndex].Selected = true;
        }

        private void btnColorUp_Click(object sender, EventArgs e)
        {
            if (listViewKbColors.SelectedIndices.Count == 0)
                return;

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
            if (listViewKbColors.SelectedIndices.Count == 0)
                return;

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

        private void setParamDirButton_Click(object sender, EventArgs e)
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
            Runtime.zScale = GuiTools.TryParseTBFloat(zScaleTB);
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

        private void debug1CB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.debug1 = debug1CB.Checked;
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

        private void drawUvCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.drawUv = drawUvCB.Checked;
        }

        private void swagYCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderSwagY = swagYCB.Checked;
        }

        private void imageModeChanged(object sender, EventArgs e)
        {
            customComboBox.Enabled = (sender == customRadioButton);
            if (sender == defaultRadioButton)
                DiscordSettings.imageKeyMode = DiscordSettings.ImageKeyMode.Default;
            else if (sender == customRadioButton)
                DiscordSettings.imageKeyMode = DiscordSettings.ImageKeyMode.UserPicked;
            else if (sender == filenameRadioButton)
                DiscordSettings.imageKeyMode = DiscordSettings.ImageKeyMode.LastFileOpened;
            DiscordSettings.Update();
        }

        private void customImageKeyChange(object sender, EventArgs e)
        {
            DiscordSettings.userPickedImageKey = customComboBox.Text;
            DiscordSettings.Update();
        }

        private void userModCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            modNameTextBox.Enabled = userModCheckbox.Checked;
            DiscordSettings.useUserModName = userModCheckbox.Checked;
        }

        private void discordCheckChanged(object sender, EventArgs e)
        {
            if (sender == showActiveWindowCheckbox)
                DiscordSettings.showCurrentWindow = showActiveWindowCheckbox.Checked;
            if (sender == timeElapsedCheckbox)
                DiscordSettings.showTimeElapsed = timeElapsedCheckbox.Checked;
        }

        private void modNameTextBox_TextChanged(object sender, EventArgs e)
        {
            DiscordSettings.userNamedMod = modNameTextBox.Text;
        }

        private void BackgroundGradient1_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.backgroundGradientTop = Color.FromArgb(0xFF, colorDialog.Color);
                BackgroundGradient1.BackColor = Runtime.backgroundGradientTop;
            }
        }

        private void BackgroundGradient2_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.backgroundGradientBottom = Color.FromArgb(0xFF, colorDialog.Color);
                BackgroundGradient2.BackColor = Runtime.backgroundGradientBottom;
            }
        }

        private void postProcessingCB_CheckedChanged(object sender, EventArgs e)
        {
            bloomGroupBox.Enabled = postProcessingCB.Checked;
            Runtime.usePostProcessing = postProcessingCB.Checked;
        }

        private void bloomCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderBloom = bloomCB.Checked;
        }

        private void bloomIntensityTB_TextChanged(object sender, EventArgs e)
        {
            Runtime.bloomIntensity = GuiTools.TryParseTBFloat(bloomIntensityTB);
        }

        private void bloomThresholdTB_TextChanged(object sender, EventArgs e)
        {
            Runtime.bloomThreshold = GuiTools.TryParseTBFloat(bloomThresholdTB);
        }

        private void groupBox8_Enter(object sender, EventArgs e)
        {

        }

        private void floorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Runtime.floorStyle = (Runtime.FloorStyle)floorComboBox.SelectedIndex;

            if (Runtime.floorStyle == Runtime.FloorStyle.UserTexture)
            {
                floorColorPictureBox.BackColor = Color.FromArgb(0, Color.White);

                try
                {
                    floorColorPictureBox.Image = new Bitmap(Runtime.floorTexFilePath);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                floorColorPictureBox.BackColor = Runtime.floorColor;

                // Flashbacks to previous memory leaks involving bitmaps...
                if (floorColorPictureBox.Image != null)
                {
                    floorColorPictureBox.Image.Dispose();
                    floorColorPictureBox.Image = null;
                }
            }

            SetFloorPictureBoxToolTip();
            floorColorPictureBox.Refresh();
        }

        private void openBackgroundTexButton_Click(object sender, EventArgs e)
        {
            OpenBackgroundTexture();
        }

        private void OpenBackgroundTexture()
        {
            using (var ofd = new OpenFileDialog() { Filter = "Image (.png)|*.png|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Rendering.RenderTools.backgroundTexture = new Texture2D(new Bitmap(ofd.FileName));
                    Runtime.backgroundTexFilePath = ofd.FileName;
                    backgroundPictureBox.Image = new Bitmap(Runtime.backgroundTexFilePath);
                }
            }
        }

        private void OpenFloorTexture()
        {
            using (var ofd = new OpenFileDialog() { Filter = "Image (.png)|*.png|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Bitmap floorImg = new Bitmap(ofd.FileName);
                    Runtime.floorTexFilePath = ofd.FileName;
                    floorColorPictureBox.Image = floorImg;
                    floorColorPictureBox.Refresh();
                    Rendering.RenderTools.floorTexture = new Texture2D(floorImg);
                }
            }
        }

        private void backgroundComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Runtime.backgroundStyle = (Runtime.BackgroundStyle)backgroundComboBox.SelectedIndex;

            if (Runtime.backgroundStyle == Runtime.BackgroundStyle.UserTexture)
            {
                backgroundPictureBox.Visible = true;

                BackgroundGradient1.Visible = false;
                backgroundTopLabel.Visible = false;
                backgroundBottomLabel.Visible = false;
                BackgroundGradient2.Visible = false;
            }
            else if (Runtime.backgroundStyle == Runtime.BackgroundStyle.Solid)
            {
                // Single color and no picture box.
                BackgroundGradient1.Visible = true;
                backgroundTopLabel.Visible = true;
                backgroundBottomLabel.Visible = false;
                BackgroundGradient2.Visible = false;

                backgroundPictureBox.Visible = false;
            }
            else if (Runtime.backgroundStyle == Runtime.BackgroundStyle.Gradient)
            {
                BackgroundGradient1.Visible = true;
                backgroundTopLabel.Visible = true;
                backgroundBottomLabel.Visible = true;
                BackgroundGradient2.Visible = true;

                backgroundPictureBox.Visible = false;
            }
        }

        private void ClearBackgroundPictureBox()
        {
            if (backgroundPictureBox.Image != null)
            {
                backgroundPictureBox.Image.Dispose();
                backgroundPictureBox.Image = null;
                backgroundPictureBox.Refresh();
            }
        }

        private void floorScaleTB_TextChanged(object sender, EventArgs e)
        {
            Runtime.floorSize = GuiTools.TryParseTBFloat(floorScaleTB);
        }

        private void floorPictureBox_Click(object sender, EventArgs e)
        {
            if (Runtime.floorStyle == Runtime.FloorStyle.UserTexture)
                OpenFloorTexture();
            else 
                SelectFloorColor();
        }

        private void SelectFloorColor()
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Runtime.floorColor = Color.FromArgb(0xFF, colorDialog.Color);
                floorColorPictureBox.BackColor = Runtime.floorColor;
            }
        }

        private void backgroundPictureBox_Click(object sender, EventArgs e)
        {
            OpenBackgroundTexture();
        }
    }
}
