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
                    }
                }
            }
        }

        private void charLightsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                charColor1GroupBox.Text = "Fresnel Sky Color";
                charColor2GroupBox.Text = "Fresnel Ground Color";
                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
                UpdateCharFresnelValues();
            }
            else if(charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                charColor1GroupBox.Text = "Diffuse Color";
                charColor2GroupBox.Text = "Ambient Color";
                RenderCharacterLightColor(new Vector3(Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB),
                      new Vector3(Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB));
                UpdateCharDiffuseValues();
            }
        }

        private void UpdateCharFresnelValues()
        {
            charColor1XTB.Text = Lights.fresnelLight.skyR + "";
            charColor1YTB.Text = Lights.fresnelLight.skyG + "";
            charColor1ZTB.Text = Lights.fresnelLight.skyB + "";
        }

        private void UpdateCharDiffuseValues()
        {
            charColor1XTB.Text = Lights.diffuseLight.difR + "";
            charColor1YTB.Text = Lights.diffuseLight.difG + "";
            charColor1ZTB.Text = Lights.diffuseLight.difB + "";
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
            stageDifHueTB.Text = selectedStageLight.difHue + "";
            stageDifSatTB.Text = selectedStageLight.difSaturation + "";
            stageDifIntensityTB.Text = selectedStageLight.difIntensity + "";
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

        private void RenderCharacterLightColor(Vector3 topColor, Vector3 bottomColor)
        {
            charDifColorGLControl.MakeCurrent();
            GL.Viewport(charDifColorGLControl.ClientRectangle);
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            RenderTools.DrawQuadGradient(topColor.X, topColor.Y, topColor.Z, bottomColor.X, bottomColor.Y, bottomColor.Z);

            charDifColorGLControl.SwapBuffers();
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
            float i = 0;
            if (float.TryParse(areaCeilRedTB.Text, out i))
            {
                areaCeilRedTB.BackColor = Color.White;
                selectedAreaLight.skyR = i;
            }
            else
                areaCeilRedTB.BackColor = Color.Red;

            RenderAreaLightColor();
        }

        private void areaCeilGreenTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaCeilGreenTB.Text, out i))
            {
                areaCeilGreenTB.BackColor = Color.White;
                selectedAreaLight.skyG = i;
            }
            else
                areaCeilGreenTB.BackColor = Color.Red;

            RenderAreaLightColor();
        }

        private void areaCeilBlueTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaCeilBlueTB.Text, out i))
            {
                areaCeilBlueTB.BackColor = Color.White;
                selectedAreaLight.skyB = i;
            }
            else
                areaCeilBlueTB.BackColor = Color.Red;

            RenderAreaLightColor();
        }

        private void areaGroundRedTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaGroundRedTB.Text, out i))
            {
                areaGroundRedTB.BackColor = Color.White;
                selectedAreaLight.groundR = i;
            }
            else
                areaGroundRedTB.BackColor = Color.Red;

            RenderAreaLightColor();
        }

        private void areaGroundGreenTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaGroundGreenTB.Text, out i))
            {
                areaGroundGreenTB.BackColor = Color.White;
                selectedAreaLight.groundG = i;
            }
            else
                areaGroundGreenTB.BackColor = Color.Red;

            RenderAreaLightColor();
        }

        private void areaGroundBlueTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaGroundBlueTB.Text, out i))
            {
                areaGroundBlueTB.BackColor = Color.White;
                selectedAreaLight.groundB = i;
            }
            else
                areaGroundBlueTB.BackColor = Color.Red;

            RenderAreaLightColor();
        }

        private void areaCeilRedTrackBar_Scroll(object sender, EventArgs e)
        {
            areaCeilRedTB.Text = (float)(5 * (areaCeilRedTrackBar.Value / (float)areaCeilRedTrackBar.Maximum)) + "";
        }

        private void areaCeilGreenTrackBar_Scroll(object sender, EventArgs e)
        {
            areaCeilGreenTB.Text = (float)(5 * (areaCeilGreenTrackBar.Value / (float)areaCeilGreenTrackBar.Maximum)) + "";

        }

        private void areaCeilBlueTrackBar_Scroll(object sender, EventArgs e)
        {
            areaCeilBlueTB.Text = (float)(5 * (areaCeilBlueTrackBar.Value / (float)areaCeilBlueTrackBar.Maximum)) + "";

        }

        #endregion

        #region area light transformations events

        private void areaPosXTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaPosXTB.Text, out i))
            {
                areaPosXTB.BackColor = Color.White;
                selectedAreaLight.positionX = i;
            }
            else
                areaPosXTB.BackColor = Color.Red;
        }

        private void areaPosYTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaPosYTB.Text, out i))
            {
                areaPosYTB.BackColor = Color.White;
                selectedAreaLight.positionY = i;
            }
            else
                areaPosYTB.BackColor = Color.Red;
        }

        private void areaPosZTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaPosZTB.Text, out i))
            {
                areaPosZTB.BackColor = Color.White;
                selectedAreaLight.positionZ = i;
            }
            else
                areaPosZTB.BackColor = Color.Red;
        }

        private void areaScaleXTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaScaleXTB.Text, out i))
            {
                areaScaleXTB.BackColor = Color.White;
                selectedAreaLight.scaleX = i;
            }
            else
                areaScaleXTB.BackColor = Color.Red;
        }

        private void areaScaleYTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaScaleYTB.Text, out i))
            {
                areaScaleYTB.BackColor = Color.White;
                selectedAreaLight.scaleY = i;
            }
            else
                areaScaleYTB.BackColor = Color.Red;
        }

        private void areaScaleZTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaScaleZTB.Text, out i))
            {
                areaScaleZTB.BackColor = Color.White;
                selectedAreaLight.scaleZ = i;
            }
            else
                areaScaleZTB.BackColor = Color.Red;
        }

        private void areaRotX_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaRotX.Text, out i))
            {
                areaRotX.BackColor = Color.White;
                selectedAreaLight.rotX = i;
            }
            else
                areaRotX.BackColor = Color.Red;
        }

        private void areaRotY_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaRotY.Text, out i))
            {
                areaRotY.BackColor = Color.White;
                selectedAreaLight.rotY = i;
            }
            else
                areaRotY.BackColor = Color.Red;
        }

        private void areaRotZ_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(areaRotZ.Text, out i))
            {
                areaRotZ.BackColor = Color.White;
                selectedAreaLight.rotZ = i;
            }
            else
                areaRotZ.BackColor = Color.Red;
        }

        private void areaPosXTrackBar_Scroll(object sender, EventArgs e)
        {
            areaPosXTB.Text = (float)(500.0f * (areaPosXTrackBar.Value - ((float)areaPosXTrackBar.Maximum / 2.0f)) / (float)areaPosXTrackBar.Maximum) + "";
        }

        private void areaPosYTrackBar_Scroll(object sender, EventArgs e)
        {
            areaPosYTB.Text = (float)(500.0f * (areaPosYTrackBar.Value - ((float)areaPosYTrackBar.Maximum / 2.0f)) / (float)areaPosYTrackBar.Maximum) + "";

        }

        private void areaPosZTrackBar_Scroll(object sender, EventArgs e)
        {
            areaPosZTB.Text = (float)(500.0f * (areaPosZTrackBar.Value - ((float)areaPosZTrackBar.Maximum / 2.0f)) / (float)areaPosZTrackBar.Maximum) + "";

        }

        private void areaScaleXTrackBar_Scroll(object sender, EventArgs e)
        {
            areaScaleXTB.Text = (float)(250 * (areaScaleXTrackBar.Value / (float)areaScaleXTrackBar.Maximum)) + "";
        }

        private void areaScaleYTrackBar_Scroll(object sender, EventArgs e)
        {
            areaScaleYTB.Text = (float)(250 * (areaScaleYTrackBar.Value / (float)areaScaleYTrackBar.Maximum)) + "";

        }

        private void areaScaleZTrackBar_Scroll(object sender, EventArgs e)
        {
            areaScaleZTB.Text = (float)(250 * (areaScaleZTrackBar.Value / (float)areaScaleZTrackBar.Maximum)) + "";

        }


        #endregion events


    }
}
