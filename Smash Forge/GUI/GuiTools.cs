using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;


namespace Smash_Forge
{
    class GuiTools
    {
        public static string GetTextValueFromTrackBar(TrackBar trackBar, float maximum)
        {
            float newValue = ((float)trackBar.Value / trackBar.Maximum) * maximum;
            return newValue.ToString();
        }

        public static void UpdateTrackBarFromValue(float value, TrackBar trackBar, float minValue, float maxValue)
        {
            // Values outside the displayable range of the trackbar are set to the
            // trackbar's min or max value. 
            int newSliderValue = (int)(value * trackBar.Maximum / maxValue);
            newSliderValue = Math.Min(newSliderValue, trackBar.Maximum);
            newSliderValue = Math.Max(newSliderValue, trackBar.Minimum);
            trackBar.Value = newSliderValue;
        }

        public static float TryParseTBFloat(TextBox textBox)
        {
            float result = 0;
            if (float.TryParse(textBox.Text, out result))
                textBox.BackColor = Color.White;
            else
                textBox.BackColor = Color.Red;

            return result;
        }
    }
}
