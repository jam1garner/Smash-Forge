using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smash_Forge.GUI.Menus
{
    public partial class LightColorEditor : Form
    {
        float hue = 360.0f;
        float saturation = 1.0f;
        float value = 1.0f;
        float R = 1.0f;
        float G = 1.0f;
        float B = 1.0f;
        float colorTemp = 6500.0f; // color temperature in Kelvin

        DirectionalLight light;

        public LightColorEditor(DirectionalLight light)
        {
            InitializeComponent();
            R = light.R;
            G = light.G;
            B = light.B;

            colorTempTB.Text = colorTemp + "";
            redTB.Text = R + "";
            greenTB.Text = G + "";
            blueTB.Text = B + "";

            redTrackBar.Value = (int)(R * redTrackBar.Maximum);
            greenTrackBar.Value = (int)(G * greenTrackBar.Maximum);
            blueTrackBar.Value = (int)(B * blueTrackBar.Maximum);


            this.light = light;
        }

        private void hueTB_TextChanged(object sender, EventArgs e)
        {
 
        }

        private void useColorTempCB_CheckedChanged(object sender, EventArgs e)
        {

            tempLabel.Enabled = useColorTempCB.Checked;
            colorTempTB.Enabled = useColorTempCB.Checked;
            colorTempTrackBar.Enabled = useColorTempCB.Checked;

            if (useColorTempCB.Checked)
            {
                // override the color with RGB calculated from color temperature and disable other color controls
                RenderTools.ColorTemp2RGB(colorTemp, out R, out G, out B);



            }

        }

        private void colorTempTrackBar_Scroll(object sender, EventArgs e)
        {
            colorTemp = (float)colorTempTrackBar.Value;

            updateColorFromTemp();
        }

        private void colorTempTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(colorTempTB.Text, out i))
            {
                colorTempTB.BackColor = Color.White;
                colorTemp = i;
            }
            else
                colorTempTB.BackColor = Color.Red;

            updateColorFromTemp();
        }

        private void redTB_TextChanged(object sender, EventArgs e) // fix type casting
        {
            float i = 0;
            if (float.TryParse(redTB.Text, out i))
            {
                redTB.BackColor = Color.White;
                R = i;
            }
            else
                redTB.BackColor = Color.Red;

            updateColorFromRGB();
        }

        private void greenTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(greenTB.Text, out i))
            {
                greenTB.BackColor = Color.White;
                G = i;
            }
            else
                greenTB.BackColor = Color.Red;

            updateColorFromRGB();
        }

        private void blueTB_TextChanged(object sender, EventArgs e)
        {
            float i = 0;
            if (float.TryParse(blueTB.Text, out i))
            {
                blueTB.BackColor = Color.White;
                B = i;
            }
            else
                blueTB.BackColor = Color.Red;

            updateColorFromRGB();
        }

        private void redTrackBar_Scroll(object sender, EventArgs e)
        {
            R = (float)redTrackBar.Value / (float)redTrackBar.Maximum;
            redTB.Text = R.ToString();

            updateColorFromRGB();
        }

        private void greenTrackBar_Scroll(object sender, EventArgs e)
        {
            G = (float)greenTrackBar.Value / (float)greenTrackBar.Maximum;
            greenTB.Text = G.ToString();

            updateColorFromRGB();
        }

        private void blueTrackBar_Scroll(object sender, EventArgs e)
        {
            B = (float)blueTrackBar.Value / (float)blueTrackBar.Maximum;
            blueTB.Text = B.ToString();

            updateColorFromRGB();
        }

        private void colorTrackBar_Scroll(object sender, EventArgs e)
        {
            float intensity = (float)colorTrackBar.Value / (float)colorTrackBar.Maximum;
            R *= intensity;
            G *= intensity;
            B *= intensity;

            updateColorFromRGB();
        }

        public void updateColorFromRGB() // update HSV values and button color
        {
            RenderTools.RGB2HSV(R, G, B, out hue, out saturation, out value);
            redTB.Text = R.ToString();
            greenTB.Text = G.ToString();
            blueTB.Text = B.ToString();
            hueTB.Text = hue.ToString();
            satTB.Text = saturation.ToString();
            valueTB.Text = value.ToString();


            colorButton.BackColor = Color.FromArgb(255, RenderTools.Float2RGBClamp(R), RenderTools.Float2RGBClamp(G), RenderTools.Float2RGBClamp(B));
        }

        public void updateColorFromHSV() // update values and button color
        {
            RenderTools.HSV2RGB(hue, saturation, value, out R, out G, out B);
            redTB.Text = R.ToString();
            greenTB.Text = G.ToString();
            blueTB.Text = B.ToString();
            hueTB.Text = hue.ToString();
            satTB.Text = saturation.ToString();
            valueTB.Text = value.ToString();


            colorButton.BackColor = Color.FromArgb(255, RenderTools.Float2RGBClamp(R), RenderTools.Float2RGBClamp(G), RenderTools.Float2RGBClamp(B));
        }


        public void updateColorFromTemp() // update values and button color
        {
            RenderTools.ColorTemp2RGB(colorTemp, out R, out G, out B);
            redTB.Text = R.ToString();
            greenTB.Text = G.ToString();
            blueTB.Text = B.ToString();
            hueTB.Text = hue.ToString();
            satTB.Text = saturation.ToString();
            valueTB.Text = value.ToString();
            colorTempTB.Text = colorTemp.ToString();

            colorButton.BackColor = Color.FromArgb(255, RenderTools.Float2RGBClamp(R), RenderTools.Float2RGBClamp(G), RenderTools.Float2RGBClamp(B));
        }

        private void hueTrackBar_Scroll(object sender, EventArgs e)
        {
            hue = (float)hueTrackBar.Value;// / (float)hueTrackBar.Maximum;

            updateColorFromHSV();
        }

        private void satTrackBar_Scroll(object sender, EventArgs e)
        {
            saturation = (float)satTrackBar.Value / (float)satTrackBar.Maximum;

            updateColorFromHSV();
        }

        private void valueTrackBar_Scroll(object sender, EventArgs e)
        {
            value = (float)valueTrackBar.Value / (float)valueTrackBar.Maximum;

            updateColorFromHSV();
        }
    }
}
