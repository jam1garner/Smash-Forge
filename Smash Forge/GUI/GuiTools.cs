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
        public static float GetTrackBarValue(TrackBar trackBar, float minimum, float maximum)
        {
            // Remaps [trackBar.min, trackBar.max] to [minimum, maximum].
            return RemapValues(trackBar.Minimum, trackBar.Maximum, minimum, maximum, trackBar.Value);
        }

        private static float RemapValues(float low1, float high1, float low2, float high2, float value)
        {
            return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
        }

        public static void UpdateTrackBarFromValue(float value, TrackBar trackBar, float minValue, float maxValue)
        {
            int newSliderValue = (int)(RemapValues(minValue, maxValue, trackBar.Minimum, trackBar.Maximum, value));
            // Values outside the displayable range of the trackbar are set to the trackbar's min or max value. 
            // The trackbar's maximum only defines the precision. 
            newSliderValue = Math.Min(newSliderValue, trackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, trackBar.Minimum);
            trackBar.Value = newSliderValue;
        }

        public static float TryParseTBFloat(TextBox textBox, bool changeTextBoxColor = true)
        {
            // Sets the textbox backcolor to red for invalid values.
            float result = 0;
            if (float.TryParse(textBox.Text, out result))
                textBox.BackColor = Color.White;
            else if (changeTextBoxColor)
                textBox.BackColor = Color.Red;

            return result;
        }

        public static int TryParseTBInt(TextBox textBox, bool useHex = false, bool changeTextBoxColor = true)
        {
            // Returns -1 on failure. Sets the textbox backcolor to red for invalid values.
            int result = -1;
            if (useHex && int.TryParse(textBox.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out result))
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
