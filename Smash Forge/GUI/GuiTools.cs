using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;



namespace Smash_Forge
{
    class GuiTools
    {
        public static float GetTrackBarValue(TrackBar trackBar, float maximum)
        {
            return ((float)trackBar.Value / trackBar.Maximum) * maximum;
        }

        public static void UpdateTrackBarFromValue(float value, TrackBar trackBar, float minValue, float maxValue)
        {
            // Values outside the displayable range of the trackbar are set to the
            // trackbar's min or max value. 
            // The trackbar's maximum only defines the precision. 
            int newSliderValue = (int)(value * trackBar.Maximum / maxValue);
            newSliderValue = Math.Min(newSliderValue, trackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, trackBar.Minimum);
            trackBar.Value = newSliderValue;
        }

        public static float TryParseTBFloat(TextBox textBox, bool changeTextBoxColor = true)
        {
            // Set the textbox backcolor to red for invalid values.
            float result = 0;
            if (float.TryParse(textBox.Text, out result))
                textBox.BackColor = Color.White;
            else if (changeTextBoxColor)
                textBox.BackColor = Color.Red;

            return result;
        }

        public static int TryParseTBInt(TextBox textBox, bool useHex = false, bool changeTextBoxColor = true)
        {
            // Returns -1 on failure. 
            int result = -1;
            if (useHex && int.TryParse(textBox.Text, NumberStyles.HexNumber, 
                CultureInfo.CurrentCulture, out result))
            {
                textBox.BackColor = Color.White;
            }
            else if (int.TryParse(textBox.Text, out result))
            {
                textBox.BackColor = Color.White;
            } else if (changeTextBoxColor)
            {
                textBox.BackColor = Color.Red;
            }

            return result;
        }
    }
}
