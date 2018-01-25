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
using OpenTK;

namespace Smash_Forge.GUI.Menus
{
    public partial class ColorEditor : Form
    {
        float hue = 360.0f;
        float saturation = 1.0f;
        float value = 1.0f;
        float R = 1.0f;
        float G = 1.0f;
        float B = 1.0f;
        float colorTemp = 6500.0f;

        const float maxRgb = 2;
        const float maxHue = 360;
        const float maxValue = 2;
        const float maxSat = 1;
        const float maxTemp = 10000;

        Vector4 color;

        public ColorEditor(Vector4 color)
        {
            InitializeComponent();

            R = color.X;
            G = color.Y;
            B = color.Z;
            ColorTools.RGB2HSV(R, G, B, out hue, out saturation, out value);
            modeComboBox.SelectedIndex = 0;
            colorXTB.Text = R.ToString();
            colorYTB.Text = G.ToString();
            colorZTB.Text = G.ToString();
            this.color = color;
        }

        public Vector4 GetColor()
        {
            color.X = R;
            color.Y = G;
            color.Z = B;
            return color;
        }

        private void colorXTB_TextChanged(object sender, EventArgs e)
        {
            float newValue = GuiTools.TryParseTBFloat(colorXTB);
            switch (modeComboBox.SelectedItem.ToString())
            {
                default:
                    break;
                case "RGB":
                    R = newValue;
                    UpdateValuesFromRgb();
                    break;
                case "HSV":
                    hue = newValue;
                    UpdateValuesFromHsv();
                    break;
            }
        }

        private void colorYTB_TextChanged(object sender, EventArgs e)
        {
            float newValue = GuiTools.TryParseTBFloat(colorYTB);
            switch (modeComboBox.SelectedItem.ToString())
            {
                default:
                    break;
                case "RGB":
                    G = newValue;
                    UpdateValuesFromRgb();
                    break;
                case "HSV":
                    saturation = newValue;
                    UpdateValuesFromHsv();
                    break;
            }
        }

        private void colorZTB_TextChanged(object sender, EventArgs e)
        {
            float newValue = GuiTools.TryParseTBFloat(colorZTB);
            switch (modeComboBox.SelectedItem.ToString())
            {
                default:
                    break;
                case "RGB":
                    B = newValue;
                    UpdateValuesFromRgb();
                    break;
                case "HSV":
                    value = newValue;
                    UpdateValuesFromHsv();
                    break;
            }
        }


        private void useColorTempCB_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void colorTempTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void colorWTB_TextChanged(object sender, EventArgs e) 
        {
            R = GuiTools.TryParseTBFloat(colorWTB);
            UpdateValuesFromRgb();
        }

        private void redTrackBar_Scroll(object sender, EventArgs e)
        {
            colorWTB.Text = GuiTools.GetTextValueFromTrackBar(colorTrackBarW, maxRgb);
        }

        private void colorTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void UpdateValuesFromRgb()
        {
            ColorTools.RGB2HSV(R, G, B, out hue, out saturation, out value);
            UpdateColorTrackBars();
            UpdateButtonColor();
        }

        private void UpdateValuesFromHsv()
        {
            ColorTools.HSV2RGB(hue, saturation, value, out R, out G, out B);
            UpdateColorTrackBars();
            UpdateButtonColor();
        }

        private void UpdateValuesFromTemp()
        {
            ColorTools.ColorTemp2RGB(colorTemp, out R, out G, out B);
            UpdateValuesFromRgb();
            UpdateColorTrackBars();
            UpdateButtonColor();
        }

        private void UpdateButtonColor()
        {
            colorButton.BackColor = Color.FromArgb(255, ColorTools.Float2RGBClamp(R), ColorTools.Float2RGBClamp(G), ColorTools.Float2RGBClamp(B));
        }

        private void UpdateColorTrackBars()
        {
            /*
            GuiTools.UpdateTrackBarFromValue(hue, colorTrackBarX, 0, maxHue);
            GuiTools.UpdateTrackBarFromValue(saturation, colorTrackBarY, 0, maxSat);
            GuiTools.UpdateTrackBarFromValue(value, colorTrackBarZ, 0, maxValue);
            GuiTools.UpdateTrackBarFromValue(R, colorTrackBarW, 0, maxRgb);
            */
        }

        private void colorTrackBarX_Scroll(object sender, EventArgs e)
        {
            switch (modeComboBox.SelectedItem.ToString())
            {
                default:
                    break;
                case "RGB":
                    colorXTB.Text = GuiTools.GetTextValueFromTrackBar(colorTrackBarX, maxRgb);
                    break;
                case "HSV":
                    colorXTB.Text = GuiTools.GetTextValueFromTrackBar(colorTrackBarX, maxHue); 
                    break;
            }
        }

        private void colorTrackBarY_Scroll(object sender, EventArgs e)
        {
            switch (modeComboBox.SelectedItem.ToString())
            {
                default:
                    break;
                case "RGB":
                    colorYTB.Text = GuiTools.GetTextValueFromTrackBar(colorTrackBarY, maxRgb);
                    break;
                case "HSV":
                    colorYTB.Text = GuiTools.GetTextValueFromTrackBar(colorTrackBarY, maxSat);
                    break;
            }
        }

        private void colorTrackBarZ_Scroll(object sender, EventArgs e)
        {
            switch (modeComboBox.SelectedItem.ToString())
            {
                default:
                    break;
                case "RGB":
                    colorZTB.Text = GuiTools.GetTextValueFromTrackBar(colorTrackBarZ, maxRgb);
                    break;
                case "HSV":
                    colorZTB.Text = GuiTools.GetTextValueFromTrackBar(colorTrackBarZ, maxValue);
                    break;
            }
        }

        private void editModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (modeComboBox.SelectedItem.ToString())
            {
                default:
                    break;
                case "RGB":
                    colorXTB.Text = R.ToString();
                    colorYTB.Text = G.ToString();
                    colorZTB.Text = B.ToString();
                    colorLabelX.Text = "Red";
                    colorLabelY.Text = "Green";
                    colorLabelZ.Text = "Blue";
                    colorLabelX.Visible = true;
                    colorLabelY.Visible = true;
                    colorLabelZ.Visible = true;
                    colorLabelW.Visible = false;
                    colorTrackBarX.Visible = true;
                    colorTrackBarY.Visible = true;
                    colorTrackBarZ.Visible = true;
                    colorTrackBarW.Visible = false;
                    colorXTB.Visible = true;
                    colorYTB.Visible = true;
                    colorZTB.Visible = true;
                    colorWTB.Visible = false;
                    break;
                case "HSV":
                    colorXTB.Text = hue.ToString();
                    colorYTB.Text = saturation.ToString();
                    colorZTB.Text = value.ToString();
                    colorLabelX.Text = "Hue";
                    colorLabelY.Text = "Saturation";
                    colorLabelZ.Text = "Value";
                    colorLabelX.Visible = true;
                    colorLabelY.Visible = true;
                    colorLabelZ.Visible = true;
                    colorLabelW.Visible = false;
                    colorTrackBarX.Visible = true;
                    colorTrackBarY.Visible = true;
                    colorTrackBarZ.Visible = true;
                    colorTrackBarW.Visible = false;
                    colorXTB.Visible = true;
                    colorYTB.Visible = true;
                    colorZTB.Visible = true;
                    colorWTB.Visible = false;
                    break;
                case "Temperature (K)":
                    colorLabelX.Text = "Temp";
                    colorLabelX.Visible = true;
                    colorLabelY.Visible = false;
                    colorLabelZ.Visible = false;
                    colorLabelW.Visible = false;
                    colorTrackBarX.Visible = true;
                    colorTrackBarY.Visible = false;
                    colorTrackBarZ.Visible = false;
                    colorTrackBarW.Visible = false;
                    colorXTB.Visible = true;
                    colorYTB.Visible = false;
                    colorZTB.Visible = false;
                    colorWTB.Visible = false;
                    break;
            }
        }
    }
}
