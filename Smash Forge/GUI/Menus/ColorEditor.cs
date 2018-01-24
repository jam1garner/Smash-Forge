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
        float colorTemp = 6500.0f; // color temperature in Kelvin

        const float maxRgb = 5;
        const float maxHue = 360;
        const float maxValue = 5;
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

            UpdateColorText();
            this.color = color;
        }

        public Vector4 GetColor()
        {
            return color;
        }

        private void hueTB_TextChanged(object sender, EventArgs e)
        {
            hue = GuiTools.TryParseTBFloat(colorXTB);
            UpdateValuesFromHsv();
        }

        private void useColorTempCB_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void colorTempTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void redTB_TextChanged(object sender, EventArgs e) // fix type casting
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
            GuiTools.UpdateTrackBarFromValue(R, colorTrackBarW, 0, maxRgb);
            //UpdateTrackBarFromValue(G, greenTrackBar, 0, maxRgb);
            //UpdateTrackBarFromValue(B, blueTrackBar, 0, maxRgb);
            GuiTools.UpdateTrackBarFromValue(hue, colorTrackBarX, 0, maxHue);
            GuiTools.UpdateTrackBarFromValue(saturation, colorTrackBarY, 0, maxSat);
            GuiTools.UpdateTrackBarFromValue(value, colorTrackBarZ, 0, maxValue);
        }

        private void UpdateColorText()
        {
            colorWTB.Text = R.ToString();
            //greenTB.Text = G.ToString();
            //blueTB.Text = B.ToString();
            colorXTB.Text = hue.ToString();
            colorYTB.Text = saturation.ToString();
            colorZTB.Text = value.ToString();
            //colorTempTB.Text = colorTemp.ToString();
        }

        private void hueTrackBar_Scroll(object sender, EventArgs e)
        {
            colorXTB.Text = GuiTools.GetTextValueFromTrackBar(colorTrackBarX, maxHue);
        }

        private void satTrackBar_Scroll(object sender, EventArgs e)
        {
            colorYTB.Text = GuiTools.GetTextValueFromTrackBar(colorTrackBarY, maxSat);
        }

        private void valueTrackBar_Scroll(object sender, EventArgs e)
        {
            colorZTB.Text = GuiTools.GetTextValueFromTrackBar(colorTrackBarZ, maxValue);
        }

        private void satTB_TextChanged(object sender, EventArgs e)
        {
            saturation = GuiTools.TryParseTBFloat(colorYTB);
            UpdateValuesFromHsv();
        }

        private void valueTB_TextChanged(object sender, EventArgs e)
        {
            value = GuiTools.TryParseTBFloat(colorZTB);
            UpdateValuesFromHsv();
        }

        private void editModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (editModeComboBox.SelectedItem.ToString())
            {
                default:
                    break;
                case "RGB":
                    break;
                case "HSV":
                    break;
                case "Temperature (K)":
                    break;
            }
        }
    }
}
