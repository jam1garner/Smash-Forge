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

    public class DirectionalLight
    {
        public float difR = 1.0f;
        public float difG = 1.0f;
        public float difB = 1.0f;
        public float difHue = 0.0f;
        public float difSaturation = 0.0f;
        public float difIntensity = 1.0f;

        public float ambR = 0.0f;
        public float ambG = 0.0f;
        public float ambB = 0.0f;
        public float ambHue = 0.0f;
        public float ambSaturation = 0.0f;
        public float ambIntensity = 1.0f;

        // in degrees (converted to radians for calcultions)
        public float rotX = 0.0f;
        public float rotY = 0.0f;
        public float rotZ = 0.0f;

        public Vector3 direction = new Vector3(0f, 0f, 1f);

        public string id = "";
        public bool enabled = true;

        public DirectionalLight(Vector3 diffuseHsv, Vector3 ambientHsv, float rotX, float rotY, float rotZ, string name)
        {
            // calculate light color
            difHue = diffuseHsv.X;
            difSaturation = diffuseHsv.Y;
            difIntensity = diffuseHsv.Z;
            ambHue = ambientHsv.X;
            ambSaturation = ambientHsv.Y;
            ambIntensity = ambientHsv.Z;
            ColorTools.HSV2RGB(difHue, difSaturation, difIntensity, out difR, out difG, out difB);
            ColorTools.HSV2RGB(ambHue, ambSaturation, ambIntensity, out ambR, out ambG, out ambB);

            // calculate light vector
            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
            UpdateDirection(rotX, rotY, rotZ);

            this.id = name;
        }

        public DirectionalLight(float H, float S, float V, Vector3 lightDirection, string name)
        {
            // calculate light color
            difHue = H;
            difSaturation = S;
            difIntensity = V;
            ColorTools.HSV2RGB(difHue, difSaturation, difIntensity, out difR, out difG, out difB);

            direction = lightDirection;

            this.id = name;
        }

        public DirectionalLight()
        {

        }

        public void setDifHue(float hue)
        {
            this.difHue = hue;
            ColorTools.HSV2RGB(hue, difSaturation, difIntensity, out difR, out difG, out difB);
        }

        public void setDifSaturation(float saturation)
        {
            this.difSaturation = saturation;
            ColorTools.HSV2RGB(difHue, saturation, difIntensity, out difR, out difG, out difB);
        }

        public void setDifIntensity(float intensity)
        {
            this.difIntensity = intensity;
            ColorTools.HSV2RGB(difHue, difSaturation, difIntensity, out difR, out difG, out difB);
        }

        public void setAmbHue(float hue)
        {
            this.ambHue = hue;
            ColorTools.HSV2RGB(ambHue, ambSaturation, ambIntensity, out ambR, out ambG, out ambB);
        }

        public void setAmbSaturation(float saturation)
        {
            this.ambSaturation = saturation;
            ColorTools.HSV2RGB(ambHue, ambSaturation, ambIntensity, out ambR, out ambG, out ambB);
        }

        public void setAmbIntensity(float intensity)
        {
            this.ambIntensity = intensity;
            ColorTools.HSV2RGB(ambHue, ambSaturation, ambIntensity, out ambR, out ambG, out ambB);
        }

        public void setRotX(float rotX)
        {
            this.rotX = rotX;
            UpdateDirection(rotX, rotY, rotZ);
        }

        public void setRotY(float rotY)
        {
            this.rotY = rotY;
            UpdateDirection(rotX, rotY, rotZ);
        }

        public void setRotZ(float rotZ)
        {
            this.rotZ = rotZ;
            UpdateDirection(rotX, rotY, rotZ);
        }

        public void setDirectionFromXYZAngles(float rotX, float rotY, float rotZ)
        {
            UpdateDirection(rotX, rotY, rotZ);
        }

        private void UpdateDirection(float rotX, float rotY, float rotZ)
        {
            // calculate light vector from 3 rotation angles
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, rotX * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitY, rotY * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitZ, rotZ * ((float)Math.PI / 180f));

            direction = Vector3.TransformVector(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
        }

        public void setColorFromHSV(float H, float S, float V)
        {
            // calculate light color
            difHue = H;
            difSaturation = S;
            difIntensity = V;
            ColorTools.HSV2RGB(difHue, difSaturation, difIntensity, out difR, out difG, out difB);
        }

        public override string ToString()
        {
            return id;
        }
    }

}
