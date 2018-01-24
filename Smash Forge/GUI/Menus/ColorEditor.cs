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
            hue = TryParseFloatFromTextBox(hueTB);
            UpdateValuesFromHsv();
        }

        private void useColorTempCB_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void colorTempTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void colorTempTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(colorTempTB.Text, out i))
            {
                colorTempTB.BackColor = Color.White;
            }
            else
                colorTempTB.BackColor = Color.Red;
        }

        private void redTB_TextChanged(object sender, EventArgs e) // fix type casting
        {
            R = TryParseFloatFromTextBox(redTB);
            UpdateValuesFromRgb();
        }

        private void greenTB_TextChanged(object sender, EventArgs e)
        {
            G = TryParseFloatFromTextBox(greenTB);
            UpdateValuesFromRgb();
        }

        private void blueTB_TextChanged(object sender, EventArgs e)
        {
            B = TryParseFloatFromTextBox(blueTB);
            UpdateValuesFromRgb();
        }

        private void redTrackBar_Scroll(object sender, EventArgs e)
        {
            redTB.Text = GetTextValueFromTrackBar(redTrackBar, maxRgb);
        }

        private void greenTrackBar_Scroll(object sender, EventArgs e)
        {
            greenTB.Text = GetTextValueFromTrackBar(greenTrackBar, maxRgb);
        }

        private void blueTrackBar_Scroll(object sender, EventArgs e)
        {
            blueTB.Text = GetTextValueFromTrackBar(blueTrackBar, maxRgb);
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
            UpdateTrackBarFromValue(R, redTrackBar, 0, maxRgb);
            UpdateTrackBarFromValue(G, greenTrackBar, 0, maxRgb);
            UpdateTrackBarFromValue(B, blueTrackBar, 0, maxRgb);
            UpdateTrackBarFromValue(hue, hueTrackBar, 0, maxHue);
            UpdateTrackBarFromValue(saturation, satTrackBar, 0, maxSat);
            UpdateTrackBarFromValue(value, valueTrackBar, 0, maxValue);
        }

        private void UpdateColorText()
        {
            redTB.Text = R.ToString();
            greenTB.Text = G.ToString();
            blueTB.Text = B.ToString();
            hueTB.Text = hue.ToString();
            satTB.Text = saturation.ToString();
            valueTB.Text = value.ToString();
            colorTempTB.Text = colorTemp.ToString();
        }

        private void hueTrackBar_Scroll(object sender, EventArgs e)
        {
            hueTB.Text = GetTextValueFromTrackBar(hueTrackBar, maxHue);
        }

        private void satTrackBar_Scroll(object sender, EventArgs e)
        {
            satTB.Text = GetTextValueFromTrackBar(satTrackBar, maxSat);
        }

        private void valueTrackBar_Scroll(object sender, EventArgs e)
        {
            valueTB.Text = GetTextValueFromTrackBar(valueTrackBar, maxValue);
        }

        private void satTB_TextChanged(object sender, EventArgs e)
        {
            saturation = TryParseFloatFromTextBox(satTB);
            UpdateValuesFromHsv();
        }

        private void valueTB_TextChanged(object sender, EventArgs e)
        {
            value = TryParseFloatFromTextBox(valueTB);
            UpdateValuesFromHsv();
        }

        private string GetTextValueFromTrackBar(TrackBar trackBar, float maximum)
        {
            float newValue = ((float)trackBar.Value / trackBar.Maximum) * maximum;
            return newValue.ToString();
        }

        private void UpdateTrackBarFromValue(float value, TrackBar trackBar, float minValue, float maxValue)
        {
            // Values outside the displayable range of the trackbar are set to the
            // trackbar's min or max value. 
            int newSliderValue = (int)(value * trackBar.Maximum / maxValue);
            newSliderValue = Math.Min(newSliderValue, trackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, trackBar.Minimum);
            trackBar.Value = newSliderValue;
        }

        private float TryParseFloatFromTextBox(TextBox textBox)
        {
            float result = 0;
            if (float.TryParse(textBox.Text, out result))
            {
                saturation = result;
                textBox.BackColor = Color.White;
            }
            else
                textBox.BackColor = Color.Red;

            return result;
        }
    }
}
