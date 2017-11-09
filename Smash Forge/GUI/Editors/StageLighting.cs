using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using SALT.PARAMS;

namespace Smash_Forge.GUI.Editors
{
    public partial class StageLighting : Form
    {
        private DirectionalLight selectedStageLight;

        public StageLighting()
        {
            InitializeComponent();
            InitializeStageLightSetListBox();
            InitializeCharLightListBox();
            InitializeAreaLightListBox();
        }

        private void InitializeAreaLightListBox()
        {
            foreach (AreaLight light in Lights.areaLights)
            {
                areaLightListBox.Items.Add(light);
            }
        }

        private void InitializeStageLightSetListBox()
        {
            for (int i = 0; i < 16; i++)
            {
                lightSetGroupListBox.Items.Add(i.ToString());
            }
        }

        private void InitializeCharLightListBox()
        {
            charLightsListBox.Items.Add(Lights.diffuseLight);
            charLightsListBox.Items.Add(Lights.fresnelLight);
        }

        private void openLightSetButton_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Param Files (.bin)|*.bin|" +
                             "All files(*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith("light_set_param.bin"))
                    {
                        Runtime.lightSetParam = new ParamFile(ofd.FileName);
                        Lights.SetLightsFromLightSetParam(Runtime.lightSetParam);
                        lightSetDirTB.Text = ofd.FileName;
                    }
                }
            }
        }

        private void charLightsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                charColor1GroupBox.Text = "Fresnel Sky Color";
                charColor1GroupBox.Text = "Fresnel Ground Color";
            }
            else
            {
                charColor1GroupBox.Text = "Diffuse Color";
                charColor1GroupBox.Text = "Ambient Color";
            }
        }

        private void lightSetGroupListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            lightSetLightListBox.Items.Clear();
            for (int i = 0; i < 4; i++)
            {
                lightSetLightListBox.Items.Add(Lights.stageDiffuseLightSet[(lightSetGroupListBox.SelectedIndex * 4) + i]);
            }
        }

        private void UpdateCurrentStageLightValues()
        {
            selectedStageLight = Lights.stageDiffuseLightSet[(lightSetGroupListBox.SelectedIndex * 4) + lightSetLightListBox.SelectedIndex];
            stageDifHueTB.Text = selectedStageLight.hue + "";
            stageDifSatTB.Text = selectedStageLight.saturation + "";
            stageDifIntensityTB.Text = selectedStageLight.intensity + "";
            stageDifRotXTB.Text = selectedStageLight.rotX + "";
            stageDifRotYTB.Text = selectedStageLight.rotY + "";
            stageDifRotZTB.Text = selectedStageLight.rotZ + "";
        }

        private void lightSetLightListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCurrentStageLightValues();
        }

        private void stageDifHueTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stageDifHueTB.Text, out i))
            {
                stageDifHueTB.BackColor = Color.White;
                selectedStageLight.setHue(i);
            }
            else
                stageDifHueTB.BackColor = Color.Red;
        }

        private void stageDifSatTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stageDifSatTB.Text, out i))
            {
                stageDifSatTB.BackColor = Color.White;
                selectedStageLight.setSaturation(i);
            }
            else
                stageDifSatTB.BackColor = Color.Red;
        }

        private void stageDifIntensityTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stageDifIntensityTB.Text, out i))
            {
                stageDifIntensityTB.BackColor = Color.White;
                selectedStageLight.setIntensity(i);
            }
            else
                stageDifIntensityTB.BackColor = Color.Red;
        }

        private void stageDifRotXTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stageDifRotXTB.Text, out i))
            {
                stageDifRotXTB.BackColor = Color.White;
                selectedStageLight.setRotX(i);
            }
            else
                stageDifRotXTB.BackColor = Color.Red;
        }

        private void stageDifRotYTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stageDifRotYTB.Text, out i))
            {
                stageDifRotYTB.BackColor = Color.White;
                selectedStageLight.setRotY(i);
            }
            else
                stageDifRotYTB.BackColor = Color.Red;
        }

        private void stageDifRotZTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stageDifRotZTB.Text, out i))
            {
                stageDifRotZTB.BackColor = Color.White;
                selectedStageLight.setRotZ(i);
            }
            else
                stageDifRotZTB.BackColor = Color.Red;
        }

        private void charColor1HueTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void charColor1SatTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void charColor1IntenTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void charColor2HueTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void charColor2SatTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void charColor2IntenTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaLightListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
