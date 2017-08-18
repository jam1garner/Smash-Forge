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
            reflectionHue.Text = Runtime.reflection_hue + "";
            reflectionSaturation.Text = Runtime.reflection_saturation + "";
            reflectionIntensity.Text = Runtime.reflection_intensity + "";

            cb_normals.Checked = Runtime.renderNormals;
            cb_vertcolor.Checked = Runtime.renderVertColor;
            renderMode.SelectedIndex = (int)Runtime.renderType;

            pbHurtboxColor.BackColor = Runtime.hurtboxColor;
            pbHurtboxColorHi.BackColor = Runtime.hurtboxColorHi;
            pbHurtboxColorMed.BackColor = Runtime.hurtboxColorMed;
            pbHurtboxColorLw.BackColor = Runtime.hurtboxColorLow;
            pbHurtboxColorSelected.BackColor = Runtime.hurtboxColorSelected;

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

        private void fogHue_TextChanged(object sender, EventArgs e)
        {

            float i = 0;
            if (float.TryParse(fogHue.Text, out i))
            {
                fogHue.BackColor = Color.White;
                Runtime.fog_hue = i ;
            }
            else
                fogHue.BackColor = Color.Red;
        }

        private void fogSaturation_TextChanged(object sender, EventArgs e)
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

        private void fogIntensity_TextChanged(object sender, EventArgs e)
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
        }
    }
}
