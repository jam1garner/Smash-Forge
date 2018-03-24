using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge.Rendering.Lights
{
    public class LightColor
    {
        public LightColor()
        {

        }

        public LightColor(float h, float s, float v)
        {
            H = h;
            S = s;
            V = v;
        }

        private float h = 0;
        public float H
        {
            get { return h; }
            set
            {
                h = value;
                ColorTools.HsvToRgb(h, s, v, out r, out g, out b);
            }
        }

        private float s = 0;
        public float S
        {
            get { return s; }
            set
            {
                s = value;
                ColorTools.HsvToRgb(h, s, v, out r, out g, out b);
            }
        }

        private float v = 0;
        public float V
        {
            get { return v; }
            set
            {
                v = value;
                ColorTools.HsvToRgb(h, s, v, out r, out g, out b);
            }
        }

        private float r = 0;
        public float R
        {
            get { return r; }
            set
            {
                r = value;
                ColorTools.RgbToHsv(r, g, b, out h, out s, out v);
            }
        }

        private float g = 0;
        public float G
        {
            get { return g; }
            set
            {
                g = value;
                ColorTools.RgbToHsv(r, g, b, out h, out s, out v);
            }
        }

        private float b = 0;
        public float B
        {
            get { return b; }
            set
            {
                b = value;
                ColorTools.RgbToHsv(r, g, b, out h, out s, out v);
            }
        }
    }
}
