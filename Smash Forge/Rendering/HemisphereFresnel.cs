using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SALT.PARAMS;
using SALT.Graphics;
using System.Diagnostics;
using System.Globalization;

namespace Smash_Forge.Rendering.Lights
{
    public class HemisphereFresnel
    {
        // ground color
        public float groundR = 1.0f;
        public float groundG = 1.0f;
        public float groundB = 1.0f;
        public float groundHue = 0.0f;
        public float groundSaturation = 0.0f;
        public float groundIntensity = 1.0f;

        // sky color
        public float skyR = 1.0f;
        public float skyG = 1.0f;
        public float skyB = 1.0f;
        public float skyHue = 0.0f;
        public float skySaturation = 0.0f;
        public float skyIntensity = 1.0f;

        // direction
        public float skyAngle = 0;
        public float groundAngle = 0;

        public string name = "";

        public HemisphereFresnel()
        {

        }

        public HemisphereFresnel(Vector3 groundHsv, Vector3 skyHsv, float skyAngle, float groundAngle, string name)
        {
            groundHue = groundHsv.X;
            groundSaturation = groundHsv.Y;
            groundIntensity = groundHsv.Z;
            skyHue = skyHsv.X;
            skySaturation = skyHsv.Y;
            skyIntensity = skyHsv.Z;
            ColorTools.HsvToRgb(skyHue, skySaturation, skyIntensity, out skyR, out skyG, out skyB);

            this.skyAngle = skyAngle;
            this.groundAngle = groundAngle;
            this.name = name;
        }

        public void setSkyHue(float skyHue)
        {
            this.skyHue = skyHue;
            ColorTools.HsvToRgb(skyHue, skySaturation, skyIntensity, out skyR, out skyG, out skyB);
        }

        public void setSkySaturation(float skySaturation)
        {
            this.skySaturation = skySaturation;
            ColorTools.HsvToRgb(skyHue, skySaturation, skyIntensity, out skyR, out skyG, out skyB);
        }

        public void setSkyIntensity(float skyIntensity)
        {
            this.skyIntensity = skyIntensity;
            ColorTools.HsvToRgb(skyHue, skySaturation, skyIntensity, out skyR, out skyG, out skyB);
        }

        public void setGroundHue(float groundHue)
        {
            this.groundHue = groundHue;
            ColorTools.HsvToRgb(groundHue, groundSaturation, groundIntensity, out groundR, out groundG, out groundB);
        }

        public void setGroundSaturation(float groundSaturation)
        {
            this.groundSaturation = groundSaturation;
            ColorTools.HsvToRgb(groundHue, groundSaturation, groundIntensity, out groundR, out groundG, out groundB);
        }

        public void setGroundIntensity(float groundIntensity)
        {
            this.groundIntensity = groundIntensity;
            ColorTools.HsvToRgb(groundHue, groundSaturation, groundIntensity, out groundR, out groundG, out groundB);
        }

        public Vector3 getSkyDirection()
        {
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, skyAngle * ((float)Math.PI / 180f));
            Vector3 direction = Vector3.TransformVector(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
            return direction;
        }

        public Vector3 getGroundDirection()
        {
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, groundAngle * ((float)Math.PI / 180f));
            Vector3 direction = Vector3.TransformVector(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
            return direction;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
