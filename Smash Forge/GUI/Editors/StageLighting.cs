using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SALT.PARAMS;
using System.Diagnostics;

namespace Smash_Forge.GUI.Editors
{
    public partial class StageLighting : Form
    {
        private DirectionalLight selectedStageLight = new DirectionalLight();
        private AreaLight selectedAreaLight = new AreaLight("");

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
            UpdateStageButtonColor();
        }

        private void UpdateStageButtonColor()
        {
            Color stageColor = Color.FromArgb(255, 255, 255, 255);
            stageDifColorButton.BackColor = stageColor;
        }


        private void RenderAreaLightColor()
        {
            areaColorGLControl.MakeCurrent();
            GL.Viewport(areaColorGLControl.ClientRectangle);
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            Debug.WriteLine(selectedAreaLight.skyR);
            RenderTools.DrawQuadGradient(selectedAreaLight.skyR, selectedAreaLight.skyG, selectedAreaLight.skyB, selectedAreaLight.groundR, selectedAreaLight.groundG, selectedAreaLight.groundB);

            areaColorGLControl.SwapBuffers();
        }

        #region stage color events
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
        #endregion

        #region stage rotation events
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
        #endregion

        #region character color events
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
        #endregion

        #region area light color events
        private void areaLightListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCurrentAreaLightValues();
            RenderAreaLightColor();
        }

        private void UpdateCurrentAreaLightValues()
        {
            selectedAreaLight = (AreaLight)areaLightListBox.SelectedItem;
            areaCeilRedTB.Text = selectedAreaLight.skyR + "";
            areaCeilBlueTB.Text = selectedAreaLight.skyB + "";
            areaCeilGreenTB.Text = selectedAreaLight.skyG + "";
            areaGroundRedTB.Text = selectedAreaLight.groundR + "";
            areaGroundGreenTB.Text = selectedAreaLight.groundG + "";
            areaGroundBlueTB.Text = selectedAreaLight.groundB + "";
            areaRotX.Text = selectedAreaLight.rotX + "";
            areaRotY.Text = selectedAreaLight.rotY + "";
            areaRotZ.Text = selectedAreaLight.rotZ + "";
            areaPosXTB.Text = selectedAreaLight.positionX + "";
            areaPosYTB.Text = selectedAreaLight.positionY + "";
            areaPosZTB.Text = selectedAreaLight.positionZ + "";
            areaScaleXTB.Text = selectedAreaLight.scaleX + "";
            areaScaleYTB.Text = selectedAreaLight.scaleY + "";
            areaScaleZTB.Text = selectedAreaLight.scaleZ + "";
        }

        private void areaCeilRedTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaCeilGreenTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaCeilBlueTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaGroundRedTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaGroundGreenTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaGroundBlueTB_TextChanged(object sender, EventArgs e)
        {

        }

        #endregion

        #region area light transformations events

        private void areaPosXTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaPosYTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaPosZTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaScaleXTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaScaleYTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaScaleZTB_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaRotX_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaRotY_TextChanged(object sender, EventArgs e)
        {

        }

        private void areaRotZ_TextChanged(object sender, EventArgs e)
        {

        }

        #endregion events
    }
}
