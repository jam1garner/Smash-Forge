using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge.GUI
{
    public partial class RenderSettings : Form
    {
        public RenderSettings()
        {
            InitializeComponent();

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
            swagViewing.Checked = Runtime.renderSwag;
            lightCheckBox.Checked = Runtime.renderLighting;
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
            modelscaleTB.Text = Runtime.model_scale + "";

            cb_normals.Checked = Runtime.renderNormals;
            cb_vertcolor.Checked = Runtime.renderVertColor;
            renderMode.SelectedIndex = (int)Runtime.renderType;
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
            Runtime.renderModel = checkBox1.Checked;
            Runtime.renderBones = checkBox2.Checked;
            Runtime.renderHitboxes = checkBox3.Checked;
            Runtime.renderPath = checkBox4.Checked;
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
            checkBox12.Enabled = checkBox6.Checked && checkBox7.Checked;
            wireframeCB.Enabled = checkBox1.Checked;
            modelSelectCB.Enabled = checkBox1.Checked;
        }

        private void checkChanged(object sender, EventArgs e)
        {
            checkChanged();
        }

        private void RenderSettings_Load(object sender, EventArgs e)
        {

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
    }
}
