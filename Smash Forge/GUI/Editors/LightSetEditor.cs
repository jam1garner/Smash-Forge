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
using Smash_Forge.Rendering.Lights;
using Smash_Forge.Rendering;
using SFGraphics.Tools;


namespace Smash_Forge.GUI.Editors
{
    public partial class LightSetEditor : Form
    {
        private DirectionalLight selectedStageLight = new DirectionalLight();
        private DirectionalLight selectedCharDiffuseLight = new DirectionalLight();
        private AreaLight selectedAreaLight = new AreaLight("");
        private LightColor selectedFogColor = new LightColor();

        public LightSetEditor()
        {
            InitializeComponent();

            InitCharLightListBox();
            InitAreaLightListBox();
            InitLightMapListBox();
            InitFogListBox();
            InitStageLightTreeView();

            // Make sure the panels all fit on the form initially.
            areaLightRotPanel.Visible = false;
            areaLightScalePanel.Visible = false;
            areaLightPosPanel.Visible = false;
            areaLightColGroundPanel.Visible = false;
        }

        private void InitStageLightTreeView()
        {
            // Use solid color thumbnails to avoid confusion between color names.
            stageLightSetTreeView.ImageList = new ImageList();
            stageLightSetTreeView.ImageList.ColorDepth = ColorDepth.Depth32Bit;

            // There are 16 groups of 4 stage lights stored in a single array.
            for (int groupIndex = 0; groupIndex < 16; groupIndex++)
            {
                // Add the group and the 4 child lights.
                TreeNode[] children = GetChildLightsForLightSet(groupIndex);
                string name = GetGroupAndColorName(groupIndex);
                TreeNode parent = new TreeNode(name, children);
                stageLightSetTreeView.Nodes.Add(parent);

                AddLightSetGroupThumbnail(groupIndex, parent);
            }
        }

        private void AddLightSetGroupThumbnail(int groupIndex, TreeNode parent)
        {
            // Add color thumbnail.
            parent.ImageIndex = groupIndex;
            parent.SelectedImageIndex = groupIndex;
            Bitmap colorImage = GetLightSetIndexColorThumbnail(groupIndex);
            stageLightSetTreeView.ImageList.Images.Add(colorImage);
        }

        private static Bitmap GetLightSetIndexColorThumbnail(int groupIndex)
        {
            Bitmap colorImage = new Bitmap(64, 64);
            using (Graphics graph = Graphics.FromImage(colorImage))
            {
                Rectangle ImageSize = new Rectangle(0, 0, 64, 64);
                graph.Clear(NUD.lightSetColorByIndex[groupIndex]);
            }

            return colorImage;
        }

        private static TreeNode[] GetChildLightsForLightSet(int groupIndex)
        {
            // The first 4 lights are used for characters.
            // We only need the stage lights. #ihatearrayindices
            int offset = 1;

            TreeNode[] children = new TreeNode[4];
            for (int lightIndex = 0; lightIndex < 4; lightIndex++)
            {
                // Create a node from the current stage light.
                DirectionalLight currentLight = Runtime.lightSetParam.stageDiffuseLights[((groupIndex + offset) * 4) + lightIndex];
                TreeNode childLight = new TreeNode(lightIndex.ToString()) { Tag = currentLight };
                childLight = new TreeNode(lightIndex.ToString()) { Tag = currentLight };
                childLight.Checked = currentLight.enabled;

                // Don't use an image. This will leave a blank space.
                childLight.ImageIndex = 99;
                childLight.SelectedImageIndex = 99;

                children[lightIndex] = childLight;
            }

            return children;
        }

        private string GetGroupAndColorName(int groupIndex)
        {
            string name = groupIndex + "";
            try
            {
                string colorName = NUD.lightSetColorByIndex[groupIndex].Name;
                name += String.Format(" {0}", colorName);
            }
            catch (KeyNotFoundException)
            {
            }
            return name;
        }

        private void InitLightMapListBox()
        {
            foreach (LightMap lightMap in LightTools.lightMaps)
            {
                lightmapListBox.Items.Add(lightMap);
            }
        }

        private void InitAreaLightListBox()
        {
            foreach (AreaLight light in LightTools.areaLights)
            {
                areaLightListBox.Items.Add(light);
            }
        }

        private void InitFogListBox()
        {
            for (int i = 0; i < Runtime.lightSetParam.stageFogSet.Length; i++)
            {
                string name = GetGroupAndColorName(i);
                stageFogListBox.Items.Add(name);
            }
        }

        private void InitCharLightListBox()
        {
            charLightsListBox.Items.Add(Runtime.lightSetParam.characterDiffuse);
            charLightsListBox.Items.Add(Runtime.lightSetParam.characterDiffuse2);
            charLightsListBox.Items.Add(Runtime.lightSetParam.characterDiffuse3);
            charLightsListBox.Items.Add(Runtime.lightSetParam.fresnelLight);
        }

        private void charLightsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (charLightsListBox.SelectedItems.Count == 0)
                return;

            switch (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString())
            {
                default:
                    break;
                case "Diffuse":
                    selectedCharDiffuseLight = Runtime.lightSetParam.characterDiffuse;
                    SetUpCharDiffuseLight();
                    // The additional character diffuse lights don't have ambient color.
                    charColor2Button.Visible = true;
                    charColor2Panel.Visible = true;
                    break;
                case "Diffuse2":
                    selectedCharDiffuseLight = Runtime.lightSetParam.characterDiffuse2;
                    SetUpCharDiffuseLight();
                    break;
                case "Diffuse3":
                    selectedCharDiffuseLight = Runtime.lightSetParam.characterDiffuse3;
                    SetUpCharDiffuseLight();
                    break;
                case "Fresnel":
                    charColor2Panel.Enabled = true;
                    SetUpCharFresnelLight();
                    break;
            }

            charDifColorGLControl.Invalidate();
        }

        private void SetUpCharFresnelLight()
        {
            charColor1Button.Text = "Fresnel Sky Color";
            charColor2Button.Text = "Fresnel Ground Color";

            charColor2Panel.Visible = true;
            charColor2Button.Visible = true;

            UpdateCharFresnelValues();
        }

        private void SetUpCharDiffuseLight()
        {
            charColor1Button.Text = "Diffuse Color";
            charColor2Button.Text = "Ambient Color";

            charColor2Panel.Visible = false;
            charColor2Button.Visible = false;

            UpdateCharDiffuseValues();
        }

        private void RenderCharacterLightColors()
        {
            if (charLightsListBox.SelectedItems.Count == 0)
                return;

            string name = charLightsListBox.SelectedItems[0].ToString();
            if (name.Contains("Fresnel"))
            {
                RenderCharacterLightGradient(Runtime.lightSetParam.fresnelLight.skyColor, Runtime.lightSetParam.fresnelLight.groundColor);
            }
            else if (name.Contains("Diffuse"))
            {
                RenderCharacterLightGradient(selectedCharDiffuseLight.diffuseColor, selectedCharDiffuseLight.ambientColor);
            }
        }

        private void UpdateCharFresnelValues()
        {
            charColor1XTB.Text = Runtime.lightSetParam.fresnelLight.skyColor.H + "";
            charColor1YTB.Text = Runtime.lightSetParam.fresnelLight.skyColor.S + "";
            charColor1ZTB.Text = Runtime.lightSetParam.fresnelLight.skyColor.V + "";
            charColor2XTB.Text = Runtime.lightSetParam.fresnelLight.groundColor.H + "";
            charColor2YTB.Text = Runtime.lightSetParam.fresnelLight.groundColor.S + "";
            charColor2ZTB.Text = Runtime.lightSetParam.fresnelLight.groundColor.V + "";
        }

        private void UpdateCharDiffuseValues()
        {
            charColor1XTB.Text = selectedCharDiffuseLight.diffuseColor.H + "";
            charColor1YTB.Text = selectedCharDiffuseLight.diffuseColor.S + "";
            charColor1ZTB.Text = selectedCharDiffuseLight.diffuseColor.V + "";
            charColor2XTB.Text = selectedCharDiffuseLight.ambientColor.H + "";
            charColor2YTB.Text = selectedCharDiffuseLight.ambientColor.S + "";
            charColor2ZTB.Text = selectedCharDiffuseLight.ambientColor.V + "";
        }

        private void UpdateCurrentStageLightValues()
        {
            stageDifHueTB.Text = selectedStageLight.diffuseColor.H + "";
            stageDifSatTB.Text = selectedStageLight.diffuseColor.S + "";
            stageDifIntensityTB.Text = selectedStageLight.diffuseColor.V + "";
            stageDifRotXTB.Text = selectedStageLight.RotationXDegrees + "";
            stageDifRotYTB.Text = selectedStageLight.RotationYDegrees + "";
            stageDifRotZTB.Text = selectedStageLight.RotationZDegrees + "";
        }

        private void lightSetLightListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCurrentStageLightValues();
            UpdateStageButtonColor();
        }

        private void UpdateStageButtonColor()
        {
            int red = ColorTools.FloatToIntClamp(selectedStageLight.diffuseColor.R);
            int green = ColorTools.FloatToIntClamp(selectedStageLight.diffuseColor.G);
            int blue = ColorTools.FloatToIntClamp(selectedStageLight.diffuseColor.B);
            Color stageColor = Color.FromArgb(255, red, green, blue);
            stageDifColorButton.BackColor = stageColor;
        }

        private void UpdateFogButtonColor()
        {
            int red = ColorTools.FloatToIntClamp(selectedFogColor.R);
            int green = ColorTools.FloatToIntClamp(selectedFogColor.G);
            int blue = ColorTools.FloatToIntClamp(selectedFogColor.B);
            Color fogColor = Color.FromArgb(255, red, green, blue);
            fogColorButton.BackColor = fogColor;
        }

        private void RenderCharacterLightGradient(LightColor topColor, LightColor bottomColor)
        {
            ScreenDrawing.DrawQuadGradient(new Vector3(topColor.R, topColor.G, topColor.B), new Vector3(bottomColor.R, bottomColor.G, bottomColor.B), ScreenDrawing.screenTriangleVbo);
        }

        private void RenderAreaLightColor()
        {
            Vector3 topColor = new Vector3(selectedAreaLight.skyR, selectedAreaLight.skyG, selectedAreaLight.skyB);
            Vector3 bottomColor = new Vector3(selectedAreaLight.groundR, selectedAreaLight.groundG, selectedAreaLight.groundB);
            ScreenDrawing.DrawQuadGradient(topColor, bottomColor, ScreenDrawing.screenTriangleVbo);
        }

        private void RenderLightMapColor()
        {
            lightMapGLControl.MakeCurrent();
            GL.Viewport(lightMapGLControl.ClientRectangle);

            Vector3 topColor = new Vector3(1);
            Vector3 bottomColor = new Vector3(1);
            ScreenDrawing.DrawQuadGradient(topColor, bottomColor, ScreenDrawing.screenTriangleVbo);

            lightMapGLControl.SwapBuffers();
        }

        private void stageDifHueTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(stageDifHueTB);
            selectedStageLight.diffuseColor.H = value;
            GuiTools.UpdateTrackBarFromValue(value, stageDifHueTrackBar, 0, 360);
            UpdateStageButtonColor();
        }

        private void stageDifSatTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(stageDifSatTB);
            selectedStageLight.diffuseColor.S = value;
            GuiTools.UpdateTrackBarFromValue(value, stageDifSatTrackBar, 0, 1);
            UpdateStageButtonColor();
        }

        private void stageDifIntensityTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(stageDifIntensityTB);
            selectedStageLight.diffuseColor.V = value;
            GuiTools.UpdateTrackBarFromValue(value, stageDifIntensityTrackBar, 0, 1);
            UpdateStageButtonColor();
        }

        private void stageDifHueTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifHueTB.Text = GuiTools.GetTrackBarValue(stageDifHueTrackBar, 0, 360).ToString();
        }

        private void stageDifSatTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifSatTB.Text = GuiTools.GetTrackBarValue(stageDifSatTrackBar, 0, 1).ToString();
        }

        private void stageDifIntensityTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifIntensityTB.Text = GuiTools.GetTrackBarValue(stageDifIntensityTrackBar, 0, 1).ToString();
        }

        private void stageDifRotXTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(stageDifRotXTB);
            selectedStageLight.RotationXDegrees = value;
            stageRotXTrackBar.Value = (int)(value + 180.0f);
        }

        private void stageDifRotYTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(stageDifRotYTB);
            selectedStageLight.RotationYDegrees = value;
            stageRotYTrackBar.Value = (int)(value + 180.0f);
        }

        private void stageDifRotZTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(stageDifRotZTB);
            selectedStageLight.RotationZDegrees = value;
            stageRotZTrackBar.Value = (int)(value + 180.0f);
        }

        private void stageRotXTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifRotXTB.Text = (stageRotXTrackBar.Value - 180.0f) + "";
        }

        private void stageRotYTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifRotYTB.Text = (stageRotYTrackBar.Value - 180.0f) + "";
        }

        private void stageRotZTrackBar_Scroll(object sender, EventArgs e)
        {
            stageDifRotZTB.Text = (stageRotZTrackBar.Value - 180.0f) + "";
        }

        private void charColor1XTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor1XTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Runtime.lightSetParam.fresnelLight.skyColor.H = value;
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString().Contains("Diffuse"))
            {
                selectedCharDiffuseLight.diffuseColor.H = value;
            }

            charDifColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, charColor1XTrackBar, 0, 360.0f);
        }

        private void charColor1YTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor1YTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Runtime.lightSetParam.fresnelLight.skyColor.S = value;
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString().Contains("Diffuse"))
            {
                selectedCharDiffuseLight.diffuseColor.S = value;
            }

            charDifColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, charColor1YTrackBar, 0, 1);
        }

        private void charColor1ZTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor1ZTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Runtime.lightSetParam.fresnelLight.skyColor.V = value;
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString().Contains("Diffuse"))
            {
                selectedCharDiffuseLight.diffuseColor.V = value;
            }

            charDifColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, charColor1ZTrackBar, 0, 1);
        }

        private void charColor2XTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor2XTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Runtime.lightSetParam.fresnelLight.groundColor.H = value;
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                Runtime.lightSetParam.characterDiffuse.ambientColor.H = value;
            }

            charDifColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, charColor2XTrackBar, 0, 360);
        }

        private void charColor2YTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor2YTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Runtime.lightSetParam.fresnelLight.groundColor.S = value;
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                Runtime.lightSetParam.characterDiffuse.ambientColor.S = value;
            }

            charDifColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, charColor2YTrackBar, 0, 1);
        }

        private void charColor2ZTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(charColor2ZTB);
            if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Fresnel")
            {
                Runtime.lightSetParam.fresnelLight.groundColor.V = value;
            }
            else if (charLightsListBox.Items[charLightsListBox.SelectedIndex].ToString() == "Diffuse")
            {
                Runtime.lightSetParam.characterDiffuse.ambientColor.V = value;
            }

            charDifColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, charColor2ZTrackBar, 0, 1);
        }

        private void charColor1XTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor1XTB.Text = GuiTools.GetTrackBarValue(charColor1XTrackBar, 0, 360).ToString();
        }

        private void charColor1YTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor1YTB.Text = GuiTools.GetTrackBarValue(charColor1YTrackBar, 0, 1).ToString();
        }

        private void charColor1ZTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor1ZTB.Text = GuiTools.GetTrackBarValue(charColor1ZTrackBar, 0, 1).ToString();
        }

        private void charColor2XTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor2XTB.Text = GuiTools.GetTrackBarValue(charColor2XTrackBar, 0, 360).ToString();
        }

        private void charColor2YTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor2YTB.Text = GuiTools.GetTrackBarValue(charColor2YTrackBar, 0, 1).ToString();
        }

        private void charColor2ZTrackBar_Scroll(object sender, EventArgs e)
        {
            charColor2ZTB.Text = GuiTools.GetTrackBarValue(charColor2ZTrackBar, 0, 1).ToString();
        }

        private void areaLightListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCurrentAreaLightValues();
            areaColorGLControl.Invalidate();
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

            areaRotX.Text = selectedAreaLight.RotationXDegrees + "";
            areaRotY.Text = selectedAreaLight.RotationYDegrees + "";
            areaRotZ.Text = selectedAreaLight.RotationZDegrees + "";

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
            areaColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, areaCeilRedTrackBar, 0, 2);
        }

        private void areaCeilGreenTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaCeilGreenTB);
            selectedAreaLight.skyG = value;
            areaColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, areaCeilGreenTrackBar, 0, 2);
        }

        private void areaCeilBlueTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaCeilBlueTB);
            selectedAreaLight.skyB = value;
            areaColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, areaCeilBlueTrackBar, 0, 2);
        }

        private void areaGroundRedTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaGroundRedTB); 
            selectedAreaLight.groundR = value;
            areaColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, areaGroundRedTrackBar, 0, 2);
        }

        private void areaGroundGreenTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaGroundGreenTB);
            selectedAreaLight.groundG = value;
            areaColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, areaGroundGreenTrackBar, 0, 2);
        }

        private void areaGroundBlueTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaGroundBlueTB);
            selectedAreaLight.groundB = value;
            areaColorGLControl.Invalidate();
            GuiTools.UpdateTrackBarFromValue(value, areaGroundBlueTrackBar, 0, 2);
        }

        private void areaCeilRedTrackBar_Scroll(object sender, EventArgs e)
        {
            areaCeilRedTB.Text = GuiTools.GetTrackBarValue(areaCeilRedTrackBar, 0, 2) + "";
        }

        private void areaCeilGreenTrackBar_Scroll(object sender, EventArgs e)
        {
            areaCeilGreenTB.Text = GuiTools.GetTrackBarValue(areaCeilGreenTrackBar, 0, 2) + "";
        }

        private void areaCeilBlueTrackBar_Scroll(object sender, EventArgs e)
        {
            areaCeilBlueTB.Text = GuiTools.GetTrackBarValue(areaCeilBlueTrackBar, 0, 2) + "";
        }

        private void areaGroundRedTrackBar_Scroll(object sender, EventArgs e)
        {
            areaGroundRedTB.Text = GuiTools.GetTrackBarValue(areaGroundRedTrackBar, 0, 2) + "";
        }

        private void areaGroundGreenTrackBar_Scroll(object sender, EventArgs e)
        {
            areaGroundGreenTB.Text = GuiTools.GetTrackBarValue(areaGroundGreenTrackBar, 0, 2) + "";
        }

        private void areaGroundBlueTrackBar_Scroll(object sender, EventArgs e)
        {
            areaGroundBlueTB.Text = GuiTools.GetTrackBarValue(areaGroundBlueTrackBar, 0, 2) + "";
        }

        private void areaPosXTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaPosXTB);
            selectedAreaLight.positionX = value;
            GuiTools.UpdateTrackBarFromValue(value, areaPosXTrackBar, -250, 250);
        }

        private void areaPosYTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaPosYTB);
            selectedAreaLight.positionY = value;
            GuiTools.UpdateTrackBarFromValue(value, areaPosYTrackBar, -250, 250);
        }

        private void areaPosZTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaPosZTB);
            selectedAreaLight.positionZ = value;
            GuiTools.UpdateTrackBarFromValue(value, areaPosZTrackBar, -250, 250);
        }

        private void areaScaleXTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaScaleXTB);
            selectedAreaLight.scaleX = value;
            GuiTools.UpdateTrackBarFromValue(value, areaScaleXTrackBar, 0, 250);
        }

        private void areaScaleYTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaScaleYTB);
            selectedAreaLight.scaleY = value;
            GuiTools.UpdateTrackBarFromValue(value, areaScaleYTrackBar, 0, 250);
        }

        private void areaScaleZTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaScaleZTB);
            selectedAreaLight.scaleZ = value;
            GuiTools.UpdateTrackBarFromValue(value, areaScaleZTrackBar, 0, 250);
        }

        private void areaRotX_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaRotX);
            selectedAreaLight.RotationXDegrees = value;
            GuiTools.UpdateTrackBarFromValue(value, areaRotXTrackBar, -180, 180);
        }

        private void areaRotY_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaRotY);
            selectedAreaLight.RotationYDegrees = value;
            GuiTools.UpdateTrackBarFromValue(value, areaRotYTrackBar, -180, 180);
        }

        private void areaRotZ_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(areaRotZ);
            selectedAreaLight.RotationZDegrees = value;
            GuiTools.UpdateTrackBarFromValue(value, areaRotZTrackBar, -180, 180);
        }

        private void areaPosXTrackBar_Scroll(object sender, EventArgs e)
        {
            areaPosXTB.Text = GuiTools.GetTrackBarValue(areaPosXTrackBar, -250, 250) + "";
        }

        private void areaPosYTrackBar_Scroll(object sender, EventArgs e)
        {
            areaPosYTB.Text = GuiTools.GetTrackBarValue(areaPosYTrackBar, -250, 250) + "";
        }

        private void areaPosZTrackBar_Scroll(object sender, EventArgs e)
        {
            areaPosZTB.Text = GuiTools.GetTrackBarValue(areaPosZTrackBar, -250, 250) + "";
        }

        private void areaScaleXTrackBar_Scroll(object sender, EventArgs e)
        {
            areaScaleXTB.Text = GuiTools.GetTrackBarValue(areaScaleXTrackBar, 0, 250) + "";
        }

        private void areaScaleYTrackBar_Scroll(object sender, EventArgs e)
        {
            areaScaleXTB.Text = GuiTools.GetTrackBarValue(areaScaleYTrackBar, 0, 250) + "";
        }

        private void areaScaleZTrackBar_Scroll(object sender, EventArgs e)
        {
            areaScaleXTB.Text = GuiTools.GetTrackBarValue(areaScaleZTrackBar, 0, 250) + "";
        }

        private void areaRotXTrackBar_Scroll(object sender, EventArgs e)
        {
            areaRotX.Text = GuiTools.GetTrackBarValue(areaRotXTrackBar, -180, 180) + "";
        }

        private void areaRotYTrackBar_Scroll(object sender, EventArgs e)
        {
            areaRotY.Text = GuiTools.GetTrackBarValue(areaRotYTrackBar, -180, 180) + "";
        }

        private void areaRotZTrackBar_Scroll(object sender, EventArgs e)
        {
            areaRotZ.Text = GuiTools.GetTrackBarValue(areaRotZTrackBar, -180, 180) + "";
        }

        private void stageLightingTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: Use tab name instead of index.
            switch (stageLightingTabControl.SelectedIndex)
            {
                case 0:
                    RenderCharacterLightGradient(new LightColor(0, 0, 1), new LightColor(0, 0, 1));                    
                    break;
                case 3:
                    areaColorGLControl.Invalidate();
                    break;
                case 4:
                    //RenderLightMapColor();
                    break;
            }
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
        }

        public static bool OpenLightSet()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Param Files (.bin)|*.bin|All files(*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (ofd.FileName.EndsWith("light_set_param.bin"))
                    {
                        Runtime.lightSetParam = new Params.LightSetParam(ofd.FileName);
                        Runtime.lightSetDirectory = ofd.FileName;
                        return true;
                    }
                }
            }

            return false;
        }

        private static void SaveLightSet()
        {
            string fileName = "";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Param Files (.bin)|*.bin|All files(*.*)|*.*";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialog.FileName;
                Runtime.lightSetParam.Save(fileName);
            }
        }

        private void fogHueTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(fogHueTB);
            selectedFogColor.H = value;
            GuiTools.UpdateTrackBarFromValue(value, fogHueTrackBar, 0, 360);
            UpdateFogButtonColor();
        }

        private void fogSaturationTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(fogSaturationTB);
            selectedFogColor.S = value;
            GuiTools.UpdateTrackBarFromValue(value, fogSaturationTrackBar, 0, 1);
            UpdateFogButtonColor();
        }

        private void fogIntensityTB_TextChanged(object sender, EventArgs e)
        {
            float value = GuiTools.TryParseTBFloat(fogIntensityTB);
            selectedFogColor.V = value;
            GuiTools.UpdateTrackBarFromValue(value, fogIntensityTrackBar, 0, 2);
            UpdateFogButtonColor();
        }

        private void fogHueTrackBar_Scroll(object sender, EventArgs e)
        {
            fogHueTB.Text = GuiTools.GetTrackBarValue(fogHueTrackBar, 0, 360) + "";
        }

        private void fogSaturationTrackBar_Scroll(object sender, EventArgs e)
        {
            fogSaturationTB.Text = GuiTools.GetTrackBarValue(fogSaturationTrackBar, 0, 1) + "";
        }

        private void fogIntensityTrackBar_Scroll(object sender, EventArgs e)
        {
            fogIntensityTB.Text = GuiTools.GetTrackBarValue(fogIntensityTrackBar, 0, 2) + "";
        }

        private void stageFogListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedFogColor = Runtime.lightSetParam.stageFogSet[stageFogListBox.SelectedIndex];
            fogHueTB.Text = selectedFogColor.H + "";
            fogSaturationTB.Text = selectedFogColor.S + "";
            fogIntensityTB.Text = selectedFogColor.V + "";
            UpdateFogButtonColor();
        }

        private void openLightsetparamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLightSet();
        }

        private void saveLightsetparamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveLightSet();
        }

        private void charDifColorGLControl_Load(object sender, EventArgs e)
        {
            OpenTKSharedResources.InitializeSharedResources();
        }

        private void charLightsTab_Click(object sender, EventArgs e)
        {

        }

        private void charLightPanelExpander_Click(object sender, EventArgs e)
        {
            charLightGlControlPanel.Visible = !charLightGlControlPanel.Visible;
            // TODO: Redraw the glControl.
        }

        private void charColor1Button_Click(object sender, EventArgs e)
        {
            charColor1Panel.Visible = !charColor1Panel.Visible;
        }

        private void charColor2Button_Click(object sender, EventArgs e)
        {
            charColor2Panel.Visible = !charColor2Panel.Visible;
        }

        private void stageDifColorPanelButton_Click(object sender, EventArgs e)
        {
            stageLightColorPanel.Visible = !stageLightColorPanel.Visible;
        }

        private void stageRotButton_Click(object sender, EventArgs e)
        {
            stageLightRotPanel.Visible = !stageLightRotPanel.Visible;
        }

        private void stageFogPanelButton_Click(object sender, EventArgs e)
        {
            stageFogColorPanel.Visible = !stageFogColorPanel.Visible;
        }

        private void charLightsFlowLayout_Resize(object sender, EventArgs e)
        {
            GuiTools.ScaleControlsHorizontallyToLayoutWidth(charLightsFlowLayout);
        }

        private void lightSetFlowLayout_Resize(object sender, EventArgs e)
        {
            GuiTools.ScaleControlsHorizontallyToLayoutWidth(lightSetFlowLayout);
        }

        private void fogFlowLayout_Resize(object sender, EventArgs e)
        {
            GuiTools.ScaleControlsHorizontallyToLayoutWidth(fogFlowLayout);
        }

        private void areaLightColorPreviewButton_Click(object sender, EventArgs e)
        {
            areaLightPreviewPanel.Visible = !areaLightPreviewPanel.Visible;
        }

        private void colCeilButton_Click(object sender, EventArgs e)
        {
            areaLightColCeilPanel.Visible = !areaLightColCeilPanel.Visible;
        }

        private void colGroundButton_Click(object sender, EventArgs e)
        {
            areaLightColGroundPanel.Visible = !areaLightColGroundPanel.Visible;
        }

        private void areaLightPosButton_Click(object sender, EventArgs e)
        {
            areaLightPosPanel.Visible = !areaLightPosPanel.Visible;
        }

        private void areaLightScaleButton_Click(object sender, EventArgs e)
        {
            areaLightScalePanel.Visible = !areaLightScalePanel.Visible;
        }

        private void areaLightRotButton_Click(object sender, EventArgs e)
        {
            areaLightRotPanel.Visible = !areaLightRotPanel.Visible;
        }

        private void charDifColorGLControl_Paint(object sender, PaintEventArgs e)
        {
            if (OpenTKSharedResources.SetupStatus != OpenTKSharedResources.SharedResourceStatus.Initialized)
                return;

            charDifColorGLControl.MakeCurrent();
            GL.Viewport(charDifColorGLControl.ClientRectangle);

            RenderCharacterLightColors();

            charDifColorGLControl.SwapBuffers();
        }

        private void areaLightFlowLayout_Resize(object sender, EventArgs e)
        {
            GuiTools.ScaleControlsHorizontallyToLayoutWidth(areaLightFlowLayout);
        }

        private void areaColorGLControl_Paint(object sender, PaintEventArgs e)
        {
            if (OpenTKSharedResources.SetupStatus != OpenTKSharedResources.SharedResourceStatus.Initialized)
                return;

            areaColorGLControl.MakeCurrent();
            GL.Viewport(areaColorGLControl.ClientRectangle);

            RenderAreaLightColor();

            areaColorGLControl.SwapBuffers();
        }

        private void charAngle1TB_TextChanged(object sender, EventArgs e)
        {

        }

        private void charAngle2TB_TextChanged(object sender, EventArgs e)
        {

        }

        private void charAngle3TB_TextChanged(object sender, EventArgs e)
        {

        }

        private void charAngleButton_Click(object sender, EventArgs e)
        {
            charAngleTableLayout.Visible = !charAngleTableLayout.Visible;
        }

        private void charAngle1TrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void charAngle2TrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void charAngle3TrackBar_Scroll(object sender, EventArgs e)
        {

        }
    }
}
