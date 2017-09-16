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

namespace Smash_Forge.GUI
{
    public partial class RenderSettings : Form
    {
        private bool disableRuntimeUpdates;
        private List<Color> hitboxColors;

        float difR = 0.0f;
        float difG = 0.0f;
        float difB = 0.0f;

        float ambR = 0.0f;
        float ambG = 0.0f;
        float ambB = 0.0f;

        float fresSkyR = 0.0f;
        float fresSkyG = 0.0f;
        float fresSkyB = 0.0f;
        float fresGroundR = 0.0f;
        float fresGroundG = 0.0f;
        float fresGroundB = 0.0f;

        float specR = 0.0f;
        float specG = 0.0f;
        float specB = 0.0f;

        float refR = 0.0f;
        float refG = 0.0f;
        float refB = 0.0f;

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
            checkBox18.Checked = Runtime.renderHitboxesNoOverlap;
            swagViewing.Checked = Runtime.renderSwag;
            lightCheckBox.Checked = Runtime.renderLighting;
            FogCB.Checked = Runtime.renderFog;
            useNormCB.Checked = Runtime.useNormalMap;
            boundingCB.Checked = Runtime.renderBoundingBox;
            wireframeCB.Checked = Runtime.renderModelWireframe;
            modelSelectCB.Checked = Runtime.renderModelSelection;
            wireframeCB.Enabled = checkBox1.Checked;
            modelSelectCB.Enabled = checkBox1.Checked;

            depthSlider.Value = (int)Runtime.renderDepth;
            fovSlider.Value = (int)(Runtime.fov * 10);

            cameraLightCB.Checked = Runtime.CameraLight;
            diffuseCB.Checked = Runtime.renderDiffuse;
            specularCB.Checked = Runtime.renderSpecular;
            fresnelCB.Checked = Runtime.renderFresnel;
            reflectionCB.Checked = Runtime.renderReflection;
            ambTB.Text = Runtime.amb_inten + "";
            difTB.Text = Runtime.dif_inten + "";
            spcTB.Text = Runtime.spc_inten + "";
            frsTB.Text = Runtime.frs_inten + "";
            refTB.Text = Runtime.ref_inten + "";
            diffuseHue.Text = Runtime.dif_hue + "";
            diffuseSaturation.Text = Runtime.dif_saturation + "";
            diffuseIntensity.Text = Runtime.dif_intensity + "";
            difRotX.Text = Runtime.dif_rotX + "";
            difRotY.Text = Runtime.dif_rotY + "";
            difRotZ.Text = Runtime.dif_rotZ + "";
            ambientHue.Text = Runtime.amb_hue + "";
            ambientSaturation.Text = Runtime.amb_saturation + "";
            ambientIntensity.Text = Runtime.amb_intensity + "";
            modelscaleTB.Text = Runtime.model_scale + "";
            fresnelGroundHue.Text = Runtime.fres_ground_hue + "";
            fresnelGroundSaturation.Text = Runtime.fres_ground_saturation + "";
            fresnelGroundIntensity.Text = Runtime.fres_ground_intensity + "";
            fresnelSkyHue.Text = Runtime.fres_sky_hue + "";
            fresnelSkySaturation.Text = Runtime.fres_sky_saturation + "";
            fresnelSkyIntensity.Text = Runtime.fres_sky_intensity + "";
            fogHue.Text = Runtime.fog_hue + "";
            fogSaturation.Text = Runtime.fog_saturation + "";
            fogIntensity.Text = Runtime.fog_intensity + "";

            specularHue.Text = Runtime.specular_hue + "";
            specularSaturation.Text = Runtime.specular_saturation + "";
            specularIntensity.Text = Runtime.specular_intensity + "";
            specRotX.Text = Runtime.specular_rotX + "";
            specRotY.Text = Runtime.specular_rotY + "";
            specRotZ.Text = Runtime.specular_rotZ + "";

            reflectionHue.Text = Runtime.reflection_hue + "";
            reflectionSaturation.Text = Runtime.reflection_saturation + "";
            reflectionIntensity.Text = Runtime.reflection_intensity + "";

            stage1Hue.Text = Runtime.stagelight1_hue + "";
            stage1Saturation.Text = Runtime.stagelight1_saturation + "";
            stage1Intensity.Text = Runtime.stagelight1_intensity + "";
            stage1RotX.Text = Runtime.stagelight1_rotX + "";
            stage1RotY.Text = Runtime.stagelight1_rotY + "";
            stage1RotZ.Text = Runtime.stagelight1_rotZ + "";

            stage2Hue.Text = Runtime.stagelight2_hue + "";
            stage2Saturation.Text = Runtime.stagelight2_saturation + "";
            stage2Intensity.Text = Runtime.stagelight2_intensity + "";
            stage2RotX.Text = Runtime.stagelight2_rotX + "";
            stage2RotY.Text = Runtime.stagelight2_rotY + "";
            stage2RotZ.Text = Runtime.stagelight2_rotZ + "";

            stage3Hue.Text = Runtime.stagelight3_hue + "";
            stage3Saturation.Text = Runtime.stagelight3_saturation + "";
            stage3Intensity.Text = Runtime.stagelight3_intensity + "";
            stage3RotX.Text = Runtime.stagelight3_rotX + "";
            stage3RotY.Text = Runtime.stagelight3_rotY + "";
            stage3RotZ.Text = Runtime.stagelight3_rotZ + "";

            stage4Hue.Text = Runtime.stagelight4_hue + "";
            stage4Saturation.Text = Runtime.stagelight4_saturation + "";
            stage4Intensity.Text = Runtime.stagelight4_intensity + "";
            stage4RotX.Text = Runtime.stagelight4_rotX + "";
            stage4RotY.Text = Runtime.stagelight4_rotY + "";
            stage4RotZ.Text = Runtime.stagelight4_rotZ + "";

            RendererLabel.Text = "Renderer: " + Runtime.renderer;
            OpenGLVersionLabel.Text = "OpenGL Version: " + Runtime.GLSLVersion;

            depthTestCB.Checked = Runtime.useDepthTest;
            zScaleTB.Text = Runtime.zScale + "";

            cb_normals.Checked = Runtime.renderNormals;
            cb_vertcolor.Checked = Runtime.renderVertColor;
            renderMode.SelectedIndex = (int)Runtime.renderType;

            pbHurtboxColor.BackColor = Runtime.hurtboxColor;
            pbHurtboxColorHi.BackColor = Runtime.hurtboxColorHi;
            pbHurtboxColorMed.BackColor = Runtime.hurtboxColorMed;
            pbHurtboxColorLw.BackColor = Runtime.hurtboxColorLow;
            pbHurtboxColorSelected.BackColor = Runtime.hurtboxColorSelected;
            pbWindboxColor.BackColor = Runtime.windboxColor;
            pbGrabboxColor.BackColor = Runtime.grabboxColor;
            pbSearchboxColor.BackColor = Runtime.searchboxColor;

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
                Runtime.renderHitboxesNoOverlap = checkBox18.Checked;
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

        private void depthSlider_ValueChanged(object sender, EventArgs e)
        {
            Runtime.renderDepth = depthSlider.Value;
        }

        private void renderMode_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Runtime.renderType = (Runtime.RenderTypes)renderMode.SelectedIndex;
        }

        private void swagViewing_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderSwag = swagViewing.Checked;
        }

        private void lightCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderLighting = lightCheckBox.Checked;
            
            diffuseCB.Enabled = lightCheckBox.Checked;
            fresnelCB.Enabled = lightCheckBox.Checked;
            specularCB.Enabled = lightCheckBox.Checked;
            reflectionCB.Enabled = lightCheckBox.Checked;
        }

        private void cb_normals_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderNormals= cb_normals.Checked;
        }

        private void cb_vertcolor_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderVertColor = cb_vertcolor.Checked;
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
            Runtime.useNormalMap = useNormCB.Checked;
        }

        private void fovSlider_Scroll(object sender, EventArgs e)
        {
            Runtime.fov = fovSlider.Value / 10f;
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
            Runtime.CameraLight = cameraLightCB.Checked;
        }

        private void difTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(difTB.Text, out i))
            {
                difTB.BackColor = Color.White;
                Runtime.dif_inten = i;
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
                Runtime.spc_inten = i;
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
                Runtime.frs_inten = i;
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
                Runtime.amb_inten = i;
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



        private void diffuseHue_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(diffuseHue.Text, out i))
            {
                diffuseHue.BackColor = Color.White;
                Runtime.dif_hue = i ;
            }
            else
                diffuseHue.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.dif_hue, Runtime.dif_saturation, Runtime.dif_intensity, out difR, out difG, out difB);

            difR = Clamp(difR);
            difG = Clamp(difG);
            difB = Clamp(difB);

            difColorButton.BackColor = Color.FromArgb(255, (int)(difR), (int)(difG), (int)(difB));

        }

        private void diffuseSaturation_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(diffuseSaturation.Text, out i))
            {
                diffuseSaturation.BackColor = Color.White;
                Runtime.dif_saturation = i;
            }
            else
                diffuseSaturation.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.dif_hue, Runtime.dif_saturation, Runtime.dif_intensity, out difR, out difG, out difB);

            difR = Clamp(difR);
            difG = Clamp(difG);
            difB = Clamp(difB);

            difColorButton.BackColor = Color.FromArgb(255, (int)(difR), (int)(difG), (int)(difB));
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void diffuseIntensity_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(diffuseIntensity.Text, out i))
            {
                diffuseIntensity.BackColor = Color.White;
                Runtime.dif_intensity = i;
            }
            else
                diffuseIntensity.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.dif_hue, Runtime.dif_saturation, Runtime.dif_intensity, out difR, out difG, out difB);

            difR = Clamp(difR);
            difG = Clamp(difG);
            difB = Clamp(difB);

            difColorButton.BackColor = Color.FromArgb(255, (int)(difR), (int)(difG), (int)(difB));
        }

        private void ambientHue_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(ambientHue.Text, out i))
            {
                ambientHue.BackColor = Color.White;
                Runtime.amb_hue = i ;
            }
            else
                ambientHue.BackColor = Color.Red;


            RenderTools.HSV2RGB(Runtime.amb_hue, Runtime.amb_saturation, Runtime.amb_intensity, out ambR, out ambG, out ambB);

            ambR = Clamp(ambR);
            ambG = Clamp(ambG);
            ambB = Clamp(ambB);

            ambColorButton.BackColor = Color.FromArgb(255, (int)(ambR), (int)(ambG), (int)(ambB));
        }

        private void ambientSaturation_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(ambientSaturation.Text, out i))
            {
                ambientSaturation.BackColor = Color.White;
                Runtime.amb_saturation = i;
            }
            else
                ambientSaturation.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.amb_hue, Runtime.amb_saturation, Runtime.amb_intensity, out ambR, out ambG, out ambB);

            ambR = Clamp(ambR);
            ambG = Clamp(ambG);
            ambB = Clamp(ambB);

            ambColorButton.BackColor = Color.FromArgb(255, (int)(ambR), (int)(ambG), (int)(ambB));
        }

        private void ambientIntensity_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(ambientIntensity.Text, out i))
            {
                ambientIntensity.BackColor = Color.White;
                Runtime.amb_intensity = i;
            }
            else
                ambientIntensity.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.amb_hue, Runtime.amb_saturation, Runtime.amb_intensity, out ambR, out ambG, out ambB);

            ambR = Clamp(ambR);
            ambG = Clamp(ambG);
            ambB = Clamp(ambB);

            ambColorButton.BackColor = Color.FromArgb(255, (int)(ambR), (int)(ambG), (int)(ambB));
        }

        private void fresnelGroundHue_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(fresnelGroundHue.Text, out i))
            {
                fresnelGroundHue.BackColor = Color.White;
                Runtime.fres_ground_hue = i ;
            }
            else
                fresnelGroundHue.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.fres_ground_hue, Runtime.fres_ground_saturation, Runtime.fres_ground_intensity, out fresGroundR, out fresGroundG, out fresGroundB);

            fresGroundR = Clamp(fresGroundR);
            fresGroundG = Clamp(fresGroundG);
            fresGroundB = Clamp(fresGroundB);

            fresGroundColorButton.BackColor = Color.FromArgb(255, (int)(fresGroundR), (int)(fresGroundG), (int)(fresGroundB));
        }

        private void fresnelGroundSaturation_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(fresnelGroundSaturation.Text, out i))
            {
                fresnelGroundSaturation.BackColor = Color.White;
                Runtime.fres_ground_saturation = i;
            }
            else
                fresnelGroundSaturation.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.fres_ground_hue, Runtime.fres_ground_saturation, Runtime.fres_ground_intensity, out fresGroundR, out fresGroundG, out fresGroundB);

            fresGroundR = Clamp(fresGroundR);
            fresGroundG = Clamp(fresGroundG);
            fresGroundB = Clamp(fresGroundB);

            fresGroundColorButton.BackColor = Color.FromArgb(255, (int)(fresGroundR), (int)(fresGroundG), (int)(fresGroundB));
        }

        private void fresnelGroundIntensity_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(fresnelGroundIntensity.Text, out i))
            {
                fresnelGroundIntensity.BackColor = Color.White;
                Runtime.fres_ground_intensity = i;
            }
            else
                fresnelGroundIntensity.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.fres_ground_hue, Runtime.fres_ground_saturation, Runtime.fres_ground_intensity, out fresGroundR, out fresGroundG, out fresGroundB);

            fresGroundR = Clamp(fresGroundR);
            fresGroundG = Clamp(fresGroundG);
            fresGroundB = Clamp(fresGroundB);

            fresGroundColorButton.BackColor = Color.FromArgb(255, (int)(fresGroundR), (int)(fresGroundG), (int)(fresGroundB));
        }

        private void FogCB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderFog = FogCB.Checked;
            label31.Enabled = FogCB.Checked;
            label32.Enabled = FogCB.Checked;
            label33.Enabled = FogCB.Checked;
            fogHue.Enabled = FogCB.Checked;
            fogSaturation.Enabled = FogCB.Checked;
            fogIntensity.Enabled = FogCB.Checked;

        }

        private void label35_Click(object sender, EventArgs e)
        {

        }

        private void fresnelSkyHue_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(fresnelSkyHue.Text, out i))
            {
                fresnelSkyHue.BackColor = Color.White;
                Runtime.fres_sky_hue = i ;
            }
            else
                fresnelSkyHue.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.fres_sky_hue, Runtime.fres_sky_saturation, Runtime.fres_sky_intensity, out fresSkyR, out fresSkyG, out fresSkyB);

            fresSkyR = Clamp(fresSkyR);
            fresSkyG = Clamp(fresSkyG);
            fresSkyB = Clamp(fresSkyB);

            fresSkyColorButton.BackColor = Color.FromArgb(255, (int)(fresSkyR), (int)(fresSkyG), (int)(fresSkyB));
        }

        private void fresnelSkySaturation_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(fresnelSkySaturation.Text, out i))
            {
                fresnelSkySaturation.BackColor = Color.White;
                Runtime.fres_sky_saturation = i;
            }
            else
                fresnelSkySaturation.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.fres_sky_hue, Runtime.fres_sky_saturation, Runtime.fres_sky_intensity, out fresSkyR, out fresSkyG, out fresSkyB);

            fresSkyR = Clamp(fresSkyR);
            fresSkyG = Clamp(fresSkyG);
            fresSkyB = Clamp(fresSkyB);

            fresSkyColorButton.BackColor = Color.FromArgb(255, (int)(fresSkyR), (int)(fresSkyG), (int)(fresSkyB));
        }

        private void fresnelSkyIntensity_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(fresnelSkyIntensity.Text, out i))
            {
                fresnelSkyIntensity.BackColor = Color.White;
                Runtime.fres_sky_intensity = i;
            }
            else
                fresnelSkyIntensity.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.fres_sky_hue, Runtime.fres_sky_saturation, Runtime.fres_sky_intensity, out fresSkyR, out fresSkyG, out fresSkyB);

            fresSkyR = Clamp(fresSkyR);
            fresSkyG = Clamp(fresSkyG);
            fresSkyB = Clamp(fresSkyB);

            fresSkyColorButton.BackColor = Color.FromArgb(255, (int)(fresSkyR), (int)(fresSkyG), (int)(fresSkyB));
        }

        private void label33_Click(object sender, EventArgs e)
        {

        }

        private void specularHue_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(specularHue.Text, out i))
            {
                specularHue.BackColor = Color.White;
                Runtime.specular_hue = i;
            }
            else
                specularHue.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.specular_hue, Runtime.specular_saturation, Runtime.specular_intensity, out specR, out specG, out specB);

            specR = Clamp(specR);
            specG = Clamp(specG);
            specB = Clamp(specB);

            specColorButton.BackColor = Color.FromArgb(255, (int)(specR), (int)(specG), (int)(specB));
        }

        private void specularSaturation_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(specularSaturation.Text, out i))
            {
                specularSaturation.BackColor = Color.White;
                Runtime.specular_saturation = i;
            }
            else
                specularSaturation.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.specular_hue, Runtime.specular_saturation, Runtime.specular_intensity, out specR, out specG, out specB);

            specR = Clamp(specR);
            specG = Clamp(specG);
            specB = Clamp(specB);

            specColorButton.BackColor = Color.FromArgb(255, (int)(specR), (int)(specG), (int)(specB));
        }

        private void specularIntensity_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(specularIntensity.Text, out i))
            {
                specularIntensity.BackColor = Color.White;
                Runtime.specular_intensity = i;
            }
            else
                specularIntensity.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.specular_hue, Runtime.specular_saturation, Runtime.specular_intensity, out specR, out specG, out specB);

            specR = Clamp(specR);
            specG = Clamp(specG);
            specB = Clamp(specB);

            specColorButton.BackColor = Color.FromArgb(255, (int)(specR), (int)(specG), (int)(specB));
        }

        private void reflectionHue_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(reflectionHue.Text, out i))
            {
                reflectionHue.BackColor = Color.White;
                Runtime.reflection_hue = i;
            }
            else
                reflectionHue.BackColor = Color.Red;


            RenderTools.HSV2RGB(Runtime.reflection_hue, Runtime.reflection_saturation, Runtime.reflection_intensity, out refR, out refG, out refB);

            refR = Clamp(refR);
            refG = Clamp(refG);
            refB = Clamp(refB);

            refColorButton.BackColor = Color.FromArgb(255, (int)(refR), (int)(refG), (int)(refB));

        }

        private void reflectionSaturation_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(reflectionSaturation.Text, out i))
            {
                reflectionSaturation.BackColor = Color.White;
                Runtime.reflection_saturation = i;
            }
            else
                reflectionSaturation.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.reflection_hue, Runtime.reflection_saturation, Runtime.reflection_intensity, out refR, out refG, out refB);

            refR = Clamp(refR);
            refG = Clamp(refG);
            refB = Clamp(refB);

            refColorButton.BackColor = Color.FromArgb(255, (int)(refR), (int)(refG), (int)(refB));
        }

        private void reflectionIntensity_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(reflectionIntensity.Text, out i))
            {
                reflectionIntensity.BackColor = Color.White;
                Runtime.reflection_intensity = i;
            }
            else
                reflectionIntensity.BackColor = Color.Red;

            RenderTools.HSV2RGB(Runtime.reflection_hue, Runtime.reflection_saturation, Runtime.reflection_intensity, out refR, out refG, out refB);

            refR = Clamp(refR);
            refG = Clamp(refG);
            refB = Clamp(refB);

            refColorButton.BackColor = Color.FromArgb(255, (int)(refR), (int)(refG), (int)(refB));
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


        private void specRotX_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(specRotX.Text, out i))
            {
                specRotX.BackColor = Color.White;
                Runtime.specular_rotX = i;
            }
            else
                specRotX.BackColor = Color.Red;
        }

        private void specRotY_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(specRotY.Text, out i))
            {
                specRotY.BackColor = Color.White;
                Runtime.specular_rotY = i;
            }
            else
                specRotY.BackColor = Color.Red;
        }

        private void difRotX_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(difRotX.Text, out i))
            {
                difRotX.BackColor = Color.White;
                Runtime.dif_rotX = i;
            }
            else
                difRotX.BackColor = Color.Red;
        }

        private void difRotY_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(difRotY.Text, out i))
            {
                difRotY.BackColor = Color.White;
                Runtime.dif_rotY = i;
            }
            else
                difRotY.BackColor = Color.Red;
        }

        private void difRotZ_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(difRotZ.Text, out i))
            {
                difRotZ.BackColor = Color.White;
                Runtime.dif_rotZ = i;
            }
            else
                difRotZ.BackColor = Color.Red;
        }

        private void stage1Hue_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage1Hue.Text, out i))
            {
                stage1Hue.BackColor = Color.White;
                Runtime.stagelight1_hue = i;
            }
            else
                stage1Hue.BackColor = Color.Red;
        }

        private void stage1Saturation_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage1Saturation.Text, out i))
            {
                stage1Saturation.BackColor = Color.White;
                Runtime.stagelight1_saturation = i;
            }
            else
                stage1Saturation.BackColor = Color.Red;
        }

        private void stage1Intensity_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage1Intensity.Text, out i))
            {
                stage1Intensity.BackColor = Color.White;
                Runtime.stagelight1_intensity = i;
            }
            else
                stage1Intensity.BackColor = Color.Red;
        }

        private void stage1RotX_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage1RotX.Text, out i))
            {
                stage1RotX.BackColor = Color.White;
                Runtime.stagelight1_rotX = i;
            }
            else
                stage1RotX.BackColor = Color.Red;
        }

        private void stage1RotY_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage1RotY.Text, out i))
            {
                stage1RotY.BackColor = Color.White;
                Runtime.stagelight1_rotY = i;
            }
            else
                stage1RotY.BackColor = Color.Red;
        }

        private void stage1RotZ_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage1RotZ.Text, out i))
            {
                stage1RotZ.BackColor = Color.White;
                Runtime.stagelight1_rotZ = i;
            }
            else
                stage1RotZ.BackColor = Color.Red;
        }

        private void stageLight1CB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderStageLight1 = stageLight1CB.Checked;
            stage1Hue.Enabled = stageLight1CB.Checked;
            stage1Saturation.Enabled = stageLight1CB.Checked;
            stage1Intensity.Enabled = stageLight1CB.Checked;
            stage1RotX.Enabled = stageLight1CB.Checked;
            stage1RotY.Enabled = stageLight1CB.Checked;
            stage1RotZ.Enabled = stageLight1CB.Checked;
            label52.Enabled = stageLight1CB.Checked;
            label53.Enabled = stageLight1CB.Checked;
            label54.Enabled = stageLight1CB.Checked;
            label56.Enabled = stageLight1CB.Checked;
            label57.Enabled = stageLight1CB.Checked;
            label58.Enabled = stageLight1CB.Checked;
        }

        private void stageLight2CB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderStageLight2 = stageLight2CB.Checked;
            stage2Hue.Enabled = stageLight2CB.Checked;
            stage2Saturation.Enabled = stageLight2CB.Checked;
            stage2Intensity.Enabled = stageLight2CB.Checked;
            stage2RotX.Enabled = stageLight2CB.Checked;
            stage2RotY.Enabled = stageLight2CB.Checked;
            stage2RotZ.Enabled = stageLight2CB.Checked;
            label59.Enabled = stageLight2CB.Checked;
            label65.Enabled = stageLight2CB.Checked;
            label64.Enabled = stageLight2CB.Checked;
            label63.Enabled = stageLight2CB.Checked;
            label60.Enabled = stageLight2CB.Checked;
            label61.Enabled = stageLight2CB.Checked;
        }

        private void stage2Hue_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage2Hue.Text, out i))
            {
                stage2Hue.BackColor = Color.White;
                Runtime.stagelight2_hue = i;
            }
            else
                stage2Hue.BackColor = Color.Red;
        }

        private void stage2Saturation_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage2Saturation.Text, out i))
            {
                stage2Saturation.BackColor = Color.White;
                Runtime.stagelight2_saturation = i;
            }
            else
                stage2Saturation.BackColor = Color.Red;
        }

        private void stage2Intensity_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage2Intensity.Text, out i))
            {
                stage2Intensity.BackColor = Color.White;
                Runtime.stagelight2_intensity = i;
            }
            else
                stage2Intensity.BackColor = Color.Red;
        }

        private void stage2RotX_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage2RotX.Text, out i))
            {
                stage2RotX.BackColor = Color.White;
                Runtime.stagelight2_rotX = i;
            }
            else
                stage2RotX.BackColor = Color.Red;
        }

        private void stage2RotY_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage2RotY.Text, out i))
            {
                stage2RotY.BackColor = Color.White;
                Runtime.stagelight2_rotY = i;
            }
            else
                stage2RotY.BackColor = Color.Red;
        }

        private void stage2RotZ_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage2RotZ.Text, out i))
            {
                stage2RotZ.BackColor = Color.White;
                Runtime.stagelight2_rotZ = i;
            }
            else
                stage2RotZ.BackColor = Color.Red;
        }

        private void stageLight3CB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderStageLight3 = stageLight3CB.Checked;
            stage3Hue.Enabled = stageLight3CB.Checked;
            stage3Saturation.Enabled = stageLight3CB.Checked;
            stage3Intensity.Enabled = stageLight3CB.Checked;
            stage3RotX.Enabled = stageLight3CB.Checked;
            stage3RotY.Enabled = stageLight3CB.Checked;
            stage3RotZ.Enabled = stageLight3CB.Checked;
            label73.Enabled = stageLight3CB.Checked;
            label74.Enabled = stageLight3CB.Checked;
            label75.Enabled = stageLight3CB.Checked;
            label77.Enabled = stageLight3CB.Checked;
            label78.Enabled = stageLight3CB.Checked;
            label79.Enabled = stageLight3CB.Checked;
        }

        private void stage3Hue_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage3Hue.Text, out i))
            {
                stage3Hue.BackColor = Color.White;
                Runtime.stagelight3_hue = i;
            }
            else
                stage3Hue.BackColor = Color.Red;
        }

        private void stage3Saturation_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage3Saturation.Text, out i))
            {
                stage3Saturation.BackColor = Color.White;
                Runtime.stagelight3_saturation = i;
            }
            else
                stage3Saturation.BackColor = Color.Red;
        }

        private void stage3Intensity_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage3Intensity.Text, out i))
            {
                stage3Intensity.BackColor = Color.White;
                Runtime.stagelight3_intensity = i;
            }
            else
                stage3Intensity.BackColor = Color.Red;
        }

        private void stage3RotX_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage3RotX.Text, out i))
            {
                stage3RotX.BackColor = Color.White;
                Runtime.stagelight3_rotX = i;
            }
            else
                stage3RotX.BackColor = Color.Red;
        }

        private void stage3RotY_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage3RotY.Text, out i))
            {
                stage3RotY.BackColor = Color.White;
                Runtime.stagelight3_rotY = i;
            }
            else
                stage3RotY.BackColor = Color.Red;
        }

        private void stage3RotZ_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage3RotZ.Text, out i))
            {
                stage3RotZ.BackColor = Color.White;
                Runtime.stagelight3_rotZ = i;
            }
            else
                stage3RotZ.BackColor = Color.Red;
        }

        private void fogHue_TextChanged_1(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(fogHue.Text, out i))
            {
                fogHue.BackColor = Color.White;
                Runtime.fog_hue = i;
            }
            else
                fogHue.BackColor = Color.Red;
        }

        private void fogSaturation_TextChanged_1(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(fogSaturation.Text, out i))
            {
                fogSaturation.BackColor = Color.White;
                Runtime.fog_saturation = i;
            }
            else
                fogSaturation.BackColor = Color.Red;
        }

        private void fogIntensity_TextChanged_1(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(fogIntensity.Text, out i))
            {
                fogIntensity.BackColor = Color.White;
                Runtime.fog_intensity = i;
            }
            else
                fogIntensity.BackColor = Color.Red;
        }

        private void FogCB_CheckedChanged_1(object sender, EventArgs e)
        {
            Runtime.renderFog = FogCB.Checked;
            label31.Enabled = FogCB.Checked;
            label32.Enabled = FogCB.Checked;
            label33.Enabled = FogCB.Checked;
            fogHue.Enabled = FogCB.Checked;
            fogSaturation.Enabled = FogCB.Checked;
            fogIntensity.Enabled = FogCB.Checked;
        }

        private void stageLight4CB_CheckedChanged(object sender, EventArgs e)
        {
            Runtime.renderStageLight4 = stageLight4CB.Checked;
            stage4Hue.Enabled = stageLight4CB.Checked;
            stage4Saturation.Enabled = stageLight4CB.Checked;
            stage4Intensity.Enabled = stageLight4CB.Checked;
            stage4RotX.Enabled = stageLight4CB.Checked;
            stage4RotY.Enabled = stageLight4CB.Checked;
            stage4RotZ.Enabled = stageLight4CB.Checked;

            label66.Enabled = stageLight4CB.Checked;
            label68.Enabled = stageLight4CB.Checked;
            label67.Enabled = stageLight4CB.Checked;
            label70.Enabled = stageLight4CB.Checked;
            label71.Enabled = stageLight4CB.Checked;
            label72.Enabled = stageLight4CB.Checked;
        }

        private void stage4Hue_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage4Hue.Text, out i))
            {
                stage4Hue.BackColor = Color.White;
                Runtime.stagelight4_hue = i;
            }
            else
                stage4Hue.BackColor = Color.Red;
        }

        private void stage4RotX_TextChanged_1(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage4RotX.Text, out i))
            {
                stage4RotX.BackColor = Color.White;
                Runtime.stagelight4_rotX = i;
            }
            else
                stage4RotX.BackColor = Color.Red;
        }

        private void stage4RotY_TextChanged_1(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage4RotY.Text, out i))
            {
                stage4RotY.BackColor = Color.White;
                Runtime.stagelight4_rotY = i;
            }
            else
                stage4RotY.BackColor = Color.Red;
        }

        private void stage4Intensity_TextChanged_1(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage4Intensity.Text, out i))
            {
                stage4Intensity.BackColor = Color.White;
                Runtime.stagelight4_intensity = i;
            }
            else
                stage4Intensity.BackColor = Color.Red;
        }

        private void stage4Saturation_TextChanged_1(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage4Saturation.Text, out i))
            {
                stage4Saturation.BackColor = Color.White;
                Runtime.stagelight4_saturation = i;
            }
            else
                stage4Saturation.BackColor = Color.Red;
        }

        private void stage4RotZ_TextChanged_1(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stage4RotZ.Text, out i))
            {
                stage4RotZ.BackColor = Color.White;
                Runtime.stagelight4_rotZ = i;
            }
            else
                stage4RotZ.BackColor = Color.Red;
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
    }
    
}
