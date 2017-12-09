using System;
using System.Drawing;

namespace Smash_Forge
{
    class ColorTools
    {
        public static void HSV2RGB(float h, float s, float v, out float R, out float G, out float B)
        {

            R = 1.0f;
            G = 1.0f;
            B = 1.0f;

            // values have to be 0-360 to work properly
            while (h > 360) { h -= 360; }
            while (h < 0) { h += 360; }

            if (s > 1.0f)
                s = 1.0f;

            float hf = h / 60.0f;
            int i = (int)Math.Floor(hf);
            float f = hf - i;
            float pv = v * (1 - s);
            float qv = v * (1 - s * f);
            float tv = v * (1 - s * (1 - f));

            switch (i)
            {
                // Red is the dominant color

                case 0:
                    R = v;
                    G = tv;
                    B = pv;
                    break;

                // Green is the dominant color

                case 1:
                    R = qv;
                    G = v;
                    B = pv;
                    break;
                case 2:
                    R = pv;
                    G = v;
                    B = tv;
                    break;

                // Blue is the dominant color

                case 3:
                    R = pv;
                    G = qv;
                    B = v;
                    break;
                case 4:
                    R = tv;
                    G = pv;
                    B = v;
                    break;

                // Red is the dominant color

                case 5:
                    R = v;
                    G = pv;
                    B = qv;
                    break;

                case 6:
                    R = v;
                    G = tv;
                    B = pv;
                    break;

            }

        }

        public static void RGB2HSV(float R, float G, float B, out float h, out float s, out float v)
        {
            h = 360.0f;
            s = 1.0f;
            v = 1.0f;

            float cMax = Math.Max(Math.Max(R, G), B);
            float cMin = Math.Min(Math.Min(R, G), B);
            float delta = cMax - cMin;

            v = cMax;

            if (delta == 0)
                h = 0;

            if (v == 0)
                s = 0.0f;
            else
                s = delta / v;

            if (R == cMax)
                h = 60.0f * (((G - B) / delta));

            else if (G == cMax)
                h = 60.0f * (((B - R) / delta) + 2);

            else if (B == cMax)
                h = 60.0f * (((R - G) / delta) + 4);

            while (h < 0.0f)
                h += 360.0f;

        }

        public static void ColorTemp2RGB(float temp, out float R, out float G, out float B)
        {
            // adapted from a cool algorithm by Tanner Helland
            // http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/ 

            R = 1.0f;
            G = 1.0f;
            B = 1.0f;

            // use doubles for calculations and convert to float at the end
            // no need for double precision floating point colors on GPU
            double Red = 255.0;
            double Green = 255.0;
            double Blue = 255.0;

            temp = temp / 100.0f;

            // Red calculations
            if (temp <= 66.0f)
                Red = 255.0f;
            else
            {
                Red = temp - 60.0;
                Red = 329.698727446 * Math.Pow(Red, -0.1332047592);
                if (Red < 0.0)
                    Red = 0.0;
                if (Red > 255.0)
                    Red = 255.0;
            }

            // Green calculations
            if (temp <= 66.0)
            {
                Green = temp;
                Green = 99.4708025861 * Math.Log(Green) - 161.1195681661;
                if (Green < 0.0)
                    Green = 0.0;
                if (Green > 255.0)
                    Green = 255.0;
            }
            else
            {
                Green = temp - 60.0;
                Green = 288.1221695283 * Math.Pow(Green, -0.0755148492);
                if (Green < 0)
                    Green = 0;
                if (Green > 255)
                    Green = 255;
            }

            // Blue calculations
            if (temp >= 66.0)
                Blue = 255.0;
            else if (temp <= 19.0)
                Blue = 0.0;
            else
            {
                Blue = temp - 10;
                Blue = 138.5177312231 * Math.Log(Blue) - 305.0447927307;
                if (Blue < 0.0)
                    Blue = 0.0;
                if (Blue > 255)
                    Blue = 255;
            }

            Red = Red / 255.0;
            Green = Green / 255.0;
            Blue = Blue / 255.0;

            R = (float)Red;
            G = (float)Green;
            B = (float)Blue;
        }


        public static int ClampInt(int i) // Restricts RGB values to 0 to 255
        {
            if (i > 255)
                return 255;
            if (i < 0)
                return 0;
            else
                return i;
        }

        public static float ClampFloat(float i) // Restricts RGB values to 0.0 to 1.0
        {
            if (i > 1.0f)
                return 1.0f;
            if (i < 0.0f)
                return 0.0f;
            else
                return i;
        }

        public static Color invertColor(Color color)
        {
            return Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
        }

        public static int Float2RGBClamp(float i)
        {
            // converts input color to int and restricts values to 0 to 255
            // useful for setting colors of form UI stuff

            i *= 255;
            i = (int)i;
            if (i > 255)
                return 255;
            if (i < 0)
                return 0;
            return (int)i;
        }
    }
}
