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
        float maxRgb = 5;
        float maxHue = 360;
        float maxValue = 5;

        Vector4 color;

        public ColorEditor(Vector4 color)
        {
            InitializeComponent();
            R = color.X;
            G = color.Y;
            B = color.Z;
            ColorTools.RGB2HSV(R, G, B, out hue, out saturation, out value);

            colorTempTB.Text = colorTemp + "";
            redTB.Text = R + "";
            greenTB.Text = G + "";
            blueTB.Text = B + "";
            hueTB.Text = hue + "";
            satTB.Text = saturation + "";
            valueTB.Text = value + "";

            redTrackBar.Value = (int)(R * redTrackBar.Maximum);
            greenTrackBar.Value = (int)(G * greenTrackBar.Maximum);
            blueTrackBar.Value = (int)(B * blueTrackBar.Maximum);
            hueTrackBar.Value = (int)(hue);
            colorTempTrackBar.Value = (int)colorTemp;

            this.color = color;
        }

        public Vector4 GetColor()
        {
            return color;
        }

        private void hueTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(hueTB.Text, out i))
            {
                hueTB.BackColor = Color.White;
                hue = i;
            }
            else
                hueTB.BackColor = Color.Red;

            UpdateColors();
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
            float i = 0;
            if (float.TryParse(redTB.Text, out i))
            {
                redTB.BackColor = Color.White;
            }
            else
                redTB.BackColor = Color.Red;
        }

        private void greenTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(greenTB.Text, out i))
            {
                greenTB.BackColor = Color.White;
            }
            else
                greenTB.BackColor = Color.Red;
        }

        private void blueTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(blueTB.Text, out i))
            {
                blueTB.BackColor = Color.White;
            }
            else
                blueTB.BackColor = Color.Red;

        }

        private void redTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void greenTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void blueTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void colorTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void updateColorFromRGB() // update HSV values and button color
        {
            ColorTools.RGB2HSV(R, G, B, out hue, out saturation, out value);
            redTB.Text = R.ToString();
            greenTB.Text = G.ToString();
            blueTB.Text = B.ToString();
            hueTB.Text = hue.ToString();
            satTB.Text = saturation.ToString();
            valueTB.Text = value.ToString();

            redTrackBar.Value = (int)(R * redTrackBar.Maximum);
            greenTrackBar.Value = (int)(G * greenTrackBar.Maximum);
            blueTrackBar.Value = (int)(B * blueTrackBar.Maximum);
            hueTrackBar.Value = (int)(hue);

            colorButton.BackColor = Color.FromArgb(255, ColorTools.Float2RGBClamp(R), ColorTools.Float2RGBClamp(G), ColorTools.Float2RGBClamp(B));
        }

        private void UpdateColors() // update values and button color
        {
            ColorTools.HSV2RGB(hue, saturation, value, out R, out G, out B);
            UpdateColorFromRGB();
            UpdateColorText();

            redTrackBar.Value = (int)(R * redTrackBar.Maximum);
            greenTrackBar.Value = (int)(G * greenTrackBar.Maximum);
            blueTrackBar.Value = (int)(B * blueTrackBar.Maximum);
            hueTrackBar.Value = (int)(hue);

            colorButton.BackColor = Color.FromArgb(255, ColorTools.Float2RGBClamp(R), ColorTools.Float2RGBClamp(G), ColorTools.Float2RGBClamp(B));
        }

        private void UpdateTextValues()
        {
            redTB.Text = R.ToString();
            greenTB.Text = G.ToString();
            blueTB.Text = B.ToString();
            hueTB.Text = hue.ToString();
            satTB.Text = saturation.ToString();
            valueTB.Text = value.ToString();
        }

        private void UpdateColorFromRGB()
        {
            color.X = R;
            color.Y = G;
            color.Z = B;
            color.W = 1.0f;
        }

        private void UpdateColorFromTemp() // update values and button color
        {
            ColorTools.ColorTemp2RGB(colorTemp, out R, out G, out B);
            UpdateColorFromRGB();
            UpdateColorText();
            UpdateTrackBarValues();

            colorButton.BackColor = Color.FromArgb(255, ColorTools.Float2RGBClamp(R), ColorTools.Float2RGBClamp(G), ColorTools.Float2RGBClamp(B));
        }

        private void UpdateTrackBarValues()
        {
            UpdateTrackBarFromValue(R, redTrackBar, maxRgb);
            UpdateTrackBarFromValue(G, greenTrackBar, maxRgb);
            UpdateTrackBarFromValue(B, blueTrackBar, maxRgb);
            UpdateTrackBarFromValue(hue, hueTrackBar, maxHue);
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

        }

        private void satTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void valueTrackBar_Scroll(object sender, EventArgs e)
        {

        }

        private void valueTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(valueTB.Text, out i))
            {
                valueTB.BackColor = Color.White;
            }
            else
                valueTB.BackColor = Color.Red;
        }

        private void UpdateTrackBarFromValue(float value, TrackBar trackBar, float maxValue)
        {
            int newSliderValue = (int)(value * trackBar.Maximum / maxValue);
            newSliderValue = Math.Min(newSliderValue, trackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, 0);
            trackBar.Value = newSliderValue;
        }
    }
}
