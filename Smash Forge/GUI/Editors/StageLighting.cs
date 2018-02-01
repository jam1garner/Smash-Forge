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
        private DirectionalLight selectedCharDiffuseLight = new DirectionalLight();
        private AreaLight selectedAreaLight = new AreaLight("");

        public StageLighting()
        {
            InitializeComponent();

            InitCharLightListBox();
            InitAreaLightListBox();
            InitLightMapListBox();

            for (int groupIndex = 0; groupIndex < 16; groupIndex++)
            {
                string name = groupIndex.ToString();
                switch (groupIndex)
                {
                    case 0:
                        name += " black";
                        break;
                    case 1:
                        name += " cyan";
                        break;
                    case 2:
                        name += " blue";
                        break;
                    case 3:
                        name += " yellow";
                        break;
                    case 4:
                        name += " magenta";
                        break;
                    case 5:
                        name += " green";
                        break;
                    default:
                        name += "";
                        break;
                }

                TreeNode[] children = new TreeNode[4];
                for (int lightIndex = 0; lightIndex < 4; lightIndex++)
                {
                    DirectionalLight currentLight = Lights.stageDiffuseLightSet[(groupIndex * 4) + lightIndex];
                    string number = lightIndex.ToString();
                    children[lightIndex] = new TreeNode(number) { Tag = currentLight };
                    children[lightIndex].Checked = currentLight.enabled;
                }
                TreeNode parent = new TreeNode(name, children);

                stageLightSetTreeView.Nodes.Add(parent);
            }


        }

        private void InitLightMapListBox()
        {
            foreach (LightMap lightMap in Lights.lightMaps)
            {
                lightmapListBox.Items.Add(lightMap);
            }
        }

        private void InitAreaLightListBox()
        {
            foreach (AreaLight light in Lights.areaLights)
            {
                areaLightListBox.Items.Add(light);
            }
        }

        private void InitCharLightListBox()
        {
            charLightsListBox.Items.Add(Lights.diffuseLight);
            charLightsListBox.Items.Add(Lights.diffuseLight2);
            charLightsListBox.Items.Add(Lights.diffuseLight3);
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
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
                selectedCharDiffuseLight = Lights.diffuseLight;
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse2")
                selectedCharDiffuseLight = Lights.diffuseLight2;
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse3")
                selectedCharDiffuseLight = Lights.diffuseLight3;

            RenderCharacterLightColors();
        }

        private void RenderCharacterLightColors()
        {
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                charColor1GroupBox.Text = "Fresnel Sky Color";
                charColor2GroupBox.Text = "Fresnel Ground Color";
                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
                UpdateCharFresnelValues();
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString().Contains("Diffuse"))
            {
                charColor1GroupBox.Text = "Diffuse Color";
                charColor2GroupBox.Text = "Ambient Color";

                RenderCharacterLightColor(new Vector3(selectedCharDiffuseLight.difR, selectedCharDiffuseLight.difG, selectedCharDiffuseLight.difB),
                      new Vector3(selectedCharDiffuseLight.ambR, selectedCharDiffuseLight.ambG, selectedCharDiffuseLight.ambB));
                UpdateCharDiffuseValues();
            }
        }

        private void UpdateCharFresnelValues()
        {
            charColor1XTB.Text = Lights.fresnelLight.skyHue + "";
            charColor1YTB.Text = Lights.fresnelLight.skySaturation + "";
            charColor1ZTB.Text = Lights.fresnelLight.skyIntensity + "";
            charColor2XTB.Text = Lights.fresnelLight.groundHue + "";
            charColor2YTB.Text = Lights.fresnelLight.groundSaturation + "";
            charColor2ZTB.Text = Lights.fresnelLight.groundIntensity + "";
        }

        private void UpdateCharDiffuseValues()
        {
            charColor1XTB.Text = selectedCharDiffuseLight.difHue + "";
            charColor1YTB.Text = selectedCharDiffuseLight.difSaturation + "";
            charColor1ZTB.Text = selectedCharDiffuseLight.difIntensity + "";
            charColor2XTB.Text = selectedCharDiffuseLight.ambHue + "";
            charColor2YTB.Text = selectedCharDiffuseLight.ambSaturation + "";
            charColor2ZTB.Text = selectedCharDiffuseLight.ambIntensity + "";
        }

        private void UpdateCurrentStageLightValues()
        {
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
            int red = ColorTools.ClampInt((int)(selectedStageLight.difR * 255));
            int green = ColorTools.ClampInt((int)(selectedStageLight.difG * 255));
            int blue = ColorTools.ClampInt((int)(selectedStageLight.difB * 255));
            Color stageColor = Color.FromArgb(255, red, green, blue);
            stageDifColorButton.BackColor = stageColor;
        }

        private void RenderCharacterLightColor(Vector3 topColor, Vector3 bottomColor)
        {
            charDifColorGLControl.MakeCurrent();
            GL.Viewport(charDifColorGLControl.ClientRectangle);
            SetOpenGLSettings();

            RenderTools.DrawQuadGradient(topColor.X, topColor.Y, topColor.Z, bottomColor.X, bottomColor.Y, bottomColor.Z);

            charDifColorGLControl.SwapBuffers();
        }

        private void RenderAreaLightColor()
        {
            areaColorGLControl.MakeCurrent();
            GL.Viewport(areaColorGLControl.ClientRectangle);
            SetOpenGLSettings();

            RenderTools.DrawQuadGradient(selectedAreaLight.skyR, selectedAreaLight.skyG, selectedAreaLight.skyB, selectedAreaLight.groundR, selectedAreaLight.groundG, selectedAreaLight.groundB);

            areaColorGLControl.SwapBuffers();
        }

        private static void SetOpenGLSettings()
        {
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        private void renderStageLightCB_CheckedChanged(object sender, EventArgs e)
        {
            //selectedStageLight.enabled = renderStageLightCB.Checked;
        }

        private void stageDifHueTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(stageDifHueTB);
            selectedStageLight.setDifHue(value);
            UpdateSliderFromValue(value, stageDifHueTrackBar, 360.0f);
            UpdateStageButtonColor();
        }

        private void stageDifSatTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(stageDifSatTB);
            selectedStageLight.setDifSaturation(value);
            GuiTools.UpdateTrackBarFromValue(value, stageDifSatTrackBar, 0, 1);
            UpdateStageButtonColor();
        }

        private void stageDifIntensityTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(stageDifIntensityTB);
            selectedStageLight.setDifIntensity(value);
            UpdateSliderFromValue(value, stageDifIntensityTrackBar, 5.0f);
            UpdateStageButtonColor();
        }

        private void stageDifHueTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifHueTB.Text = GuiTools.GetTrackBarValue(stageDifHueTrackBar, 360).ToString();
        }

        private void stageDifSatTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifSatTB.Text = GuiTools.GetTrackBarValue(stageDifSatTrackBar, 1).ToString();
        }

        private void stageDifIntensityTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifIntensityTB.Text = GuiTools.GetTrackBarValue(stageDifIntensityTrackBar, 5).ToString();
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

            stageRotXTrackBar.Value = (int)(i + 180.0f);
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

            stageRotYTrackBar.Value = (int)(i + 180.0f);
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

            stageRotZTrackBar.Value = (int)(i + 180.0f);
        }

        private void stageRotXTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifRotXTB.Text = (float)((stageRotXTrackBar.Value - 180.0f)) + "";
        }

        private void stageRotYTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifRotYTB.Text = (float)((stageRotYTrackBar.Value - 180.0f)) + "";
        }

        private void stageRotZTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifRotZTB.Text = (float)((stageRotZTrackBar.Value - 180.0f)) + "";
        }

        private void charColor1XTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor1XTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Lights.fresnelLight.setSkyHue(value);
                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString().Contains("Diffuse"))
            {
                selectedCharDiffuseLight.setDifHue(value);
                RenderCharacterLightColor(new Vector3(selectedCharDiffuseLight.difR, selectedCharDiffuseLight.difG, selectedCharDiffuseLight.difB),
                    new Vector3(selectedCharDiffuseLight.ambR, selectedCharDiffuseLight.ambG, selectedCharDiffuseLight.ambB));
            }

            UpdateSliderFromValue(value, charColor1XTrackBar, 360.0f);
        }

        private void charColor1YTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor1YTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Lights.fresnelLight.setSkySaturation(value);
                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString().Contains("Diffuse"))
            {
                selectedCharDiffuseLight.setDifSaturation(value);
                RenderCharacterLightColor(new Vector3(selectedCharDiffuseLight.difR, selectedCharDiffuseLight.difG, selectedCharDiffuseLight.difB),
                    new Vector3(selectedCharDiffuseLight.ambR, selectedCharDiffuseLight.ambG, selectedCharDiffuseLight.ambB));
            }
            GuiTools.UpdateTrackBarFromValue(value, charColor1YTrackBar, 0, 1);
        }

        private void charColor1ZTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor1ZTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Lights.fresnelLight.setSkyIntensity(value);
                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString().Contains("Diffuse"))
            {
                selectedCharDiffuseLight.setDifIntensity(value);
                RenderCharacterLightColor(new Vector3(selectedCharDiffuseLight.difR, selectedCharDiffuseLight.difG, selectedCharDiffuseLight.difB),
                    new Vector3(selectedCharDiffuseLight.ambR, selectedCharDiffuseLight.ambG, selectedCharDiffuseLight.ambB));
            }

            GuiTools.UpdateTrackBarFromValue(value, charColor1ZTrackBar, 0, 5);
        }

        private void charColor2XTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor2XTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Lights.fresnelLight.setGroundHue(value);
                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                Lights.diffuseLight.setAmbHue(value);
                RenderCharacterLightColor(new Vector3(Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB),
                    new Vector3(Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB));
            }

            UpdateSliderFromValue(value, charColor2XTrackBar, 360.0f);
        }

        private void charColor2YTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor2YTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Lights.fresnelLight.setGroundSaturation(value);
                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                Lights.diffuseLight.setAmbSaturation(value);
                RenderCharacterLightColor(new Vector3(Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB),
                    new Vector3(Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB));
            }

            UpdateSliderFromValue(value, charColor2YTrackBar, 1.0f);
        }

        private void charColor2ZTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor2ZTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Lights.fresnelLight.setGroundIntensity(value);
                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                Lights.diffuseLight.setAmbIntensity(value);
                RenderCharacterLightColor(new Vector3(Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB),
                    new Vector3(Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB));
            }

            UpdateSliderFromValue(value, charColor2ZTrackBar, 5.0f);
        }

        private void charColor1XTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor1XTB.Text = GuiTools.GetTrackBarValue(charColor1XTrackBar, 360).ToString();
        }

        private void charColor1YTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor1YTB.Text = GuiTools.GetTrackBarValue(charColor1YTrackBar, 1).ToString();
        }

        private void charColor1ZTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor1ZTB.Text = GuiTools.GetTrackBarValue(charColor1ZTrackBar, 5).ToString();
        }

        private void charColor2XTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor2XTB.Text = GuiTools.GetTrackBarValue(charColor2XTrackBar, 360).ToString();
        }

        private void charColor2YTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor2YTB.Text = GuiTools.GetTrackBarValue(charColor2YTrackBar, 1).ToString();
        }

        private void charColor2ZTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor2ZTB.Text = GuiTools.GetTrackBarValue(charColor2ZTrackBar, 5).ToString();
        }

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
            float value = GuiTools.TryParseTBFloat(areaCeilRedTB);
            selectedAreaLight.skyR = value;
            RenderAreaLightColor();
            UpdateSliderFromValue(selectedAreaLight.skyR, areaCeilRedTrackBar, 2.0f);
        }

        private void areaCeilGreenTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaCeilGreenTB);
            selectedAreaLight.skyG = value;
            RenderAreaLightColor();
            UpdateSliderFromValue(selectedAreaLight.skyG, areaCeilGreenTrackBar, 2.0f);
        }

        private void areaCeilBlueTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaCeilBlueTB);
            selectedAreaLight.skyB = value;
            RenderAreaLightColor();
            UpdateSliderFromValue(selectedAreaLight.skyB, areaCeilBlueTrackBar, 2.0f);
        }

        private void areaGroundRedTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaGroundRedTB); 
            selectedAreaLight.groundR = value;
            RenderAreaLightColor();
            UpdateSliderFromValue(selectedAreaLight.groundR, areaGroundRedTrackBar, 2.0f);
        }

        private void areaGroundGreenTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaGroundGreenTB);
            selectedAreaLight.groundG = value;
            RenderAreaLightColor();
            UpdateSliderFromValue(selectedAreaLight.groundG, areaGroundGreenTrackBar, 2.0f);
        }

        private void areaGroundBlueTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaGroundBlueTB);
            selectedAreaLight.groundB = value;
            RenderAreaLightColor();
            UpdateSliderFromValue(selectedAreaLight.groundB, areaGroundBlueTrackBar, 2.0f);
        }

        private void areaCeilRedTrackBar_Scroll(object sender, EventArgs e)
        {
            areaCeilRedTB.Text = (float)(2 * (areaCeilRedTrackBar.Value / (float)areaCeilRedTrackBar.Maximum)) + "";
        }

        private void areaCeilGreenTrackBar_Scroll(object sender, EventArgs e)
        {
            areaCeilGreenTB.Text = (float)(2 * (areaCeilGreenTrackBar.Value / (float)areaCeilGreenTrackBar.Maximum)) + "";

        }

        private void areaCeilBlueTrackBar_Scroll(object sender, EventArgs e)
        {
            areaCeilBlueTB.Text = (float)(2 * (areaCeilBlueTrackBar.Value / (float)areaCeilBlueTrackBar.Maximum)) + "";
        }

        private void areaGroundRedTrackBar_Scroll(object sender, EventArgs e)
        {
            areaGroundRedTB.Text = (float)(2 * (areaGroundRedTrackBar.Value / (float)areaGroundRedTrackBar.Maximum)) + "";
        }

        private void areaGroundGreenTrackBar_Scroll(object sender, EventArgs e)
        {
            areaGroundGreenTB.Text = (float)(2 * (areaGroundGreenTrackBar.Value / (float)areaGroundGreenTrackBar.Maximum)) + "";
        }

        private void areaGroundBlueTrackBar_Scroll(object sender, EventArgs e)
        {
            areaGroundBlueTB.Text = (float)(2 * (areaGroundBlueTrackBar.Value / (float)areaGroundBlueTrackBar.Maximum)) + "";
        }

        private void areaPosXTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaPosXTB);
            selectedAreaLight.positionX = value;
        }

        private void areaPosYTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaPosYTB);
            selectedAreaLight.positionY = value;
        }

        private void areaPosZTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaPosZTB);
            selectedAreaLight.positionZ = value;
        }

        private void areaScaleXTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaScaleXTB);
            selectedAreaLight.scaleX = value;
        }

        private void areaScaleYTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaScaleYTB);
            selectedAreaLight.scaleY = value;
        }

        private void areaScaleZTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaScaleZTB);
            selectedAreaLight.scaleZ = value;
        }

        private void areaRotX_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaRotX);
            selectedAreaLight.rotX = value;
        }

        private void areaRotY_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaRotY);
            selectedAreaLight.rotY = value;
        }

        private void areaRotZ_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaRotZ);
            selectedAreaLight.rotZ = value;
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

        private void stageLightingTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // render solid white to avoid displaying the stage light text
            RenderCharacterLightColor(new Vector3(1), new Vector3(1));
        }

        private void lightmapListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lightSetLightListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            //lightSetLightListBox.SetItemChecked()
            selectedStageLight.enabled = e.NewValue == CheckState.Checked;
        }

        private void stageLightSetTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (stageLightSetTreeView.SelectedNode.Tag is DirectionalLight)
            {
                selectedStageLight = (DirectionalLight)stageLightSetTreeView.SelectedNode.Tag;
                UpdateCurrentStageLightValues();
                UpdateStageButtonColor();
            }
        }

        private void stageLightSetTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            /*if (e.Node.Tag is DirectionalLight)
            {
                foreach (TreeNode node in stageLightSetTreeView.SelectedNode.Nodes)
                {
                    node.Checked = stageLightSetTreeView.SelectedNode.Checked;
                }
            }*/
        }

        private void UpdateSliderFromValue(float value, TrackBar trackBar, float maxValue)
        {
            int newSliderValue = (int)(value * trackBar.Maximum / maxValue);
            newSliderValue = Math.Min(newSliderValue, trackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            trackBar.Value = newSliderValue;
        }
    }
}
