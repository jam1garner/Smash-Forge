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
                string name = i.ToString();
                switch (i)
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
        
                lightSetGroupListBox.Items.Add(name);
            }

        }

        private void InitializeCharLightListBox()
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
            charColor1XTB.Text = Lights.fresnelLight.skyHue + "";
            charColor1YTB.Text = Lights.fresnelLight.skySaturation + "";
            charColor1ZTB.Text = Lights.fresnelLight.skyIntensity + "";
            charColor2XTB.Text = Lights.fresnelLight.groundHue + "";
            charColor2YTB.Text = Lights.fresnelLight.groundSaturation + "";
            charColor2ZTB.Text = Lights.fresnelLight.groundIntensity + "";
        }

        private void UpdateCharDiffuseValues()
        {
            charColor1XTB.Text = Lights.diffuseLight.difHue + "";
            charColor1YTB.Text = Lights.diffuseLight.difSaturation + "";
            charColor1ZTB.Text = Lights.diffuseLight.difIntensity + "";
            charColor2XTB.Text = Lights.diffuseLight.ambHue + "";
            charColor2YTB.Text = Lights.diffuseLight.ambSaturation + "";
            charColor2ZTB.Text = Lights.diffuseLight.ambIntensity + "";
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
            renderStageLightCB.Checked = selectedStageLight.enabled;
        }

        private void lightSetLightListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCurrentStageLightValues();
            UpdateStageButtonColor();
        }

        private void UpdateStageButtonColor()
        {
            int red = RenderTools.ClampInt((int)(selectedStageLight.difR * 255));
            int green = RenderTools.ClampInt((int)(selectedStageLight.difG * 255));
            int blue = RenderTools.ClampInt((int)(selectedStageLight.difB * 255));
            Color stageColor = Color.FromArgb(255, red, green, blue);
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

        private void renderStageLightCB_CheckedChanged(object sender, EventArgs e)
        {
            selectedStageLight.enabled = renderStageLightCB.Checked;
        }

        private void stageDifHueTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stageDifHueTB.Text, out i))
            {
                stageDifHueTB.BackColor = Color.White;
                selectedStageLight.setDifHue(i);
            }
            else
                stageDifHueTB.BackColor = Color.Red;

            UpdateStageButtonColor();

            int newSliderValue = (int) (selectedStageLight.difHue * (float)stageDifHueTrackBar.Maximum / 360.0f);
            newSliderValue = Math.Min(newSliderValue, stageDifHueTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            stageDifHueTrackBar.Value = newSliderValue;
        }

        private void stageDifSatTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stageDifSatTB.Text, out i))
            {
                stageDifSatTB.BackColor = Color.White;
                selectedStageLight.setDifSaturation(i);
            }
            else
                stageDifSatTB.BackColor = Color.Red;

            UpdateStageButtonColor();

            int newSliderValue = (int)(selectedStageLight.difSaturation * (float)stageDifSatTrackBar.Maximum / 1.0f);
            newSliderValue = Math.Min(newSliderValue, stageDifSatTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            stageDifSatTrackBar.Value = newSliderValue;
        }

        private void stageDifIntensityTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(stageDifIntensityTB.Text, out i))
            {
                stageDifIntensityTB.BackColor = Color.White;
                selectedStageLight.setDifIntensity(i);
            }
            else
                stageDifIntensityTB.BackColor = Color.Red;

            UpdateStageButtonColor();

            int newSliderValue = (int)(selectedStageLight.difIntensity * (float)stageDifIntensityTrackBar.Maximum / 5.0f);
            newSliderValue = Math.Min(newSliderValue, stageDifIntensityTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            stageDifIntensityTrackBar.Value = newSliderValue;
        }

        private void stageDifHueTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifHueTB.Text = (float)(360.0f * (stageDifHueTrackBar.Value / (float)stageDifHueTrackBar.Maximum)) + "";
        }

        private void stageDifSatTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifSatTB.Text = (float)(1.0f * (stageDifSatTrackBar.Value / (float)stageDifSatTrackBar.Maximum)) + "";
        }

        private void stageDifIntensityTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifIntensityTB.Text = (float)(5.0f * (stageDifIntensityTrackBar.Value / (float)stageDifIntensityTrackBar.Maximum)) + "";
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

        #endregion

        #region character color events
        private void charColor1XTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                i = 0;
                if (float.TryParse(charColor1XTB.Text, out i))
                {
                    charColor1XTB.BackColor = Color.White;
                    Lights.fresnelLight.setSkyHue(i);
                }
                else
                    charColor1XTB.BackColor = Color.Red;

                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                i = 0;
                if (float.TryParse(charColor1XTB.Text, out i))
                {
                    charColor1XTB.BackColor = Color.White;
                    Lights.diffuseLight.setDifHue(i);
                }
                else
                    charColor1XTB.BackColor = Color.Red;

                RenderCharacterLightColor(new Vector3(Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB),
                    new Vector3(Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB));
            }

            // update trackbar
            int newSliderValue = (int)(i * (float)charColor1XTrackBar.Maximum / 360.0f);
            newSliderValue = Math.Min(newSliderValue, charColor1XTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            charColor1XTrackBar.Value = newSliderValue;
        }

        private void charColor1YTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                i = 0;
                if (float.TryParse(charColor1YTB.Text, out i))
                {
                    charColor1YTB.BackColor = Color.White;
                    Lights.fresnelLight.setSkySaturation(i);
                }
                else
                    charColor1YTB.BackColor = Color.Red;

                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                i = 0;
                if (float.TryParse(charColor1YTB.Text, out i))
                {
                    charColor1YTB.BackColor = Color.White;
                    Lights.diffuseLight.setDifSaturation(i);
                }
                else
                    charColor1YTB.BackColor = Color.Red;


                RenderCharacterLightColor(new Vector3(Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB),
                    new Vector3(Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB));
            }

            // update trackbar
            int newSliderValue = (int)(i * (float)charColor1YTrackBar.Maximum / 1.0f);
            newSliderValue = Math.Min(newSliderValue, charColor1YTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            charColor1YTrackBar.Value = newSliderValue;
        }

        private void charColor1ZTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                i = 0;
                if (float.TryParse(charColor1ZTB.Text, out i))
                {
                    charColor1ZTB.BackColor = Color.White;
                    Lights.fresnelLight.setSkyIntensity(i);
                }
                else
                    charColor1ZTB.BackColor = Color.Red;

                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                i = 0;
                if (float.TryParse(charColor1ZTB.Text, out i))
                {
                    charColor1ZTB.BackColor = Color.White;
                    Lights.diffuseLight.setDifIntensity(i);
                }
                else
                    charColor1ZTB.BackColor = Color.Red;


                RenderCharacterLightColor(new Vector3(Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB),
                    new Vector3(Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB));
            }

            // update trackbar
            int newSliderValue = (int)(i * (float)charColor1ZTrackBar.Maximum / 5.0f);
            newSliderValue = Math.Min(newSliderValue, charColor1ZTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            charColor1ZTrackBar.Value = newSliderValue;
        }

        private void charColor2XTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                i = 0;
                if (float.TryParse(charColor2XTB.Text, out i))
                {
                    charColor2XTB.BackColor = Color.White;
                    Lights.fresnelLight.setGroundHue(i);
                }
                else
                    charColor2XTB.BackColor = Color.Red;

                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                i = 0;
                if (float.TryParse(charColor2XTB.Text, out i))
                {
                    charColor2XTB.BackColor = Color.White;
                    Lights.diffuseLight.setAmbHue(i);
                }
                else
                    charColor2XTB.BackColor = Color.Red;


                RenderCharacterLightColor(new Vector3(Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB),
                    new Vector3(Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB));
            }

            // update trackbar
            int newSliderValue = (int)(i * (float)charColor2XTrackBar.Maximum / 360.0f);
            newSliderValue = Math.Min(newSliderValue, charColor2XTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            charColor2XTrackBar.Value = newSliderValue;
        }

        private void charColor2YTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                i = 0;
                if (float.TryParse(charColor2YTB.Text, out i))
                {
                    charColor2YTB.BackColor = Color.White;
                    Lights.fresnelLight.setGroundSaturation(i);
                }
                else
                    charColor2YTB.BackColor = Color.Red;

                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                i = 0;
                if (float.TryParse(charColor2YTB.Text, out i))
                {
                    charColor2YTB.BackColor = Color.White;
                    Lights.diffuseLight.setAmbSaturation(i);
                }
                else
                    charColor2YTB.BackColor = Color.Red;

                RenderCharacterLightColor(new Vector3(Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB),
                    new Vector3(Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB));
            }

            // update trackbar
            int newSliderValue = (int)(i * (float)charColor2YTrackBar.Maximum / 1.0f);
            newSliderValue = Math.Min(newSliderValue, charColor2YTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            charColor2YTrackBar.Value = newSliderValue;
        }

        private void charColor2ZTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                i = 0;
                if (float.TryParse(charColor2ZTB.Text, out i))
                {
                    charColor2ZTB.BackColor = Color.White;
                    Lights.fresnelLight.setGroundIntensity(i);
                }
                else
                    charColor2ZTB.BackColor = Color.Red;

                RenderCharacterLightColor(new Vector3(Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB),
                    new Vector3(Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB));
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                i = 0;
                if (float.TryParse(charColor2ZTB.Text, out i))
                {
                    charColor2ZTB.BackColor = Color.White;
                    Lights.diffuseLight.setAmbIntensity(i);
                }
                else
                    charColor2ZTB.BackColor = Color.Red;

                RenderCharacterLightColor(new Vector3(Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB),
                    new Vector3(Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB));
            }

            // update trackbar
            int newSliderValue = (int)(i * (float)charColor2ZTrackBar.Maximum / 5.0f);
            newSliderValue = Math.Min(newSliderValue, charColor2ZTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            charColor2ZTrackBar.Value = newSliderValue;
        }

        private void charColor1XTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor1XTB.Text = (float)(360 * (charColor1XTrackBar.Value / (float)charColor1XTrackBar.Maximum)) + "";
        }

        private void charColor1YTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor1YTB.Text = (float)(1 * (charColor1YTrackBar.Value / (float)charColor1YTrackBar.Maximum)) + "";
        }

        private void charColor1ZTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor1ZTB.Text = (float)(5 * (charColor1ZTrackBar.Value / (float)charColor1ZTrackBar.Maximum)) + "";
        }

        private void charColor2XTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor2XTB.Text = (float)(360 * (charColor2XTrackBar.Value / (float)charColor2XTrackBar.Maximum)) + "";
        }

        private void charColor2YTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor2YTB.Text = (float)(1 * (charColor2YTrackBar.Value / (float)charColor2YTrackBar.Maximum)) + "";
        }

        private void charColor2ZTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor2ZTB.Text = (float)(5 * (charColor2ZTrackBar.Value / (float)charColor2ZTrackBar.Maximum)) + "";
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

            int newSliderValue = (int)(selectedAreaLight.skyR * areaCeilRedTrackBar.Maximum / 2.0f);
            newSliderValue = Math.Min(newSliderValue, areaCeilRedTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            areaCeilRedTrackBar.Value = newSliderValue;
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

            int newSliderValue = (int)(selectedAreaLight.skyG * areaCeilGreenTrackBar.Maximum / 2.0f);
            newSliderValue = Math.Min(newSliderValue, areaCeilGreenTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            areaCeilGreenTrackBar.Value = newSliderValue;
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

            int newSliderValue = (int)(selectedAreaLight.skyB * areaCeilBlueTrackBar.Maximum / 2.0f);
            newSliderValue = Math.Min(newSliderValue, areaCeilBlueTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            areaCeilBlueTrackBar.Value = newSliderValue;
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

            int newSliderValue = (int)(selectedAreaLight.groundR * areaGroundRedTrackBar.Maximum / 2.0f);
            newSliderValue = Math.Min(newSliderValue, areaGroundRedTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            areaGroundRedTrackBar.Value = newSliderValue;
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

            int newSliderValue = (int)(selectedAreaLight.groundG * areaGroundGreenTrackBar.Maximum / 2.0f);
            newSliderValue = Math.Min(newSliderValue, areaGroundGreenTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            areaGroundGreenTrackBar.Value = newSliderValue;
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

            int newSliderValue = (int)(selectedAreaLight.groundB * areaGroundBlueTrackBar.Maximum / 2.0f);
            newSliderValue = Math.Min(newSliderValue, areaGroundBlueTrackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            areaGroundBlueTrackBar.Value = newSliderValue;
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



        #endregion
    }
}
