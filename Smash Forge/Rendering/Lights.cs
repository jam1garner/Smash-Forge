using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SALT.PARAMS;

namespace Smash_Forge
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

        public HemisphereFresnel(float groundH, float groundS, float groundV, float skyH, float skyS, float skyV, string name)
        {
            this.groundHue = groundH;
            this.groundSaturation = groundS;
            this.groundIntensity = groundV;
            this.skyHue = skyH;
            this.skySaturation = skyS;
            this.skyIntensity = skyV;
            RenderTools.HSV2RGB(skyHue, skySaturation, skyIntensity, out skyR, out skyG, out skyB);

            this.name = name;
        }

        public void setSkyHue(float skyHue)
        {
            this.skyHue = skyHue;
            RenderTools.HSV2RGB(skyHue, skySaturation, skyIntensity, out skyR, out skyG, out skyB);
        }

        public void setSkySaturation(float skySaturation)
        {
            this.skySaturation = skySaturation;
            RenderTools.HSV2RGB(skyHue, skySaturation, skyIntensity, out skyR, out skyG, out skyB);
        }

        public void setSkyIntensity(float skyIntensity)
        {
            this.skyIntensity = skyIntensity;
            RenderTools.HSV2RGB(skyHue, skySaturation, skyIntensity, out skyR, out skyG, out skyB);
        }

        public override string ToString()
        {
            return name;
        }
    }

    public class AreaLight
    {
        public string ID = "";

        // ambient color
        public float groundR = 1.0f;
        public float groundG = 1.0f;
        public float groundB = 1.0f;

        // diffuse color
        public float skyR = 1.0f;
        public float skyG = 1.0f;
        public float skyB = 1.0f;

        // size
        public float scaleX = 1.0f;
        public float scaleY = 1.0f;
        public float scaleZ = 1.0f;

        // position of the center of the region
        public float positionX = 0.0f;
        public float positionY = 0.0f;
        public float positionZ = 0.0f;

        // XYZ angles
        // How should "non directional" area lights work?
        public float rotX = 0.0f; // in degrees (converted to radians)
        public float rotY = 0.0f; // in degrees (converted to radians)
        public float rotZ = 0.0f; // in degrees (converted to radians)
        public Vector3 direction = new Vector3(0f, 0f, 1f);

        public bool noDirectional = false;
        public bool renderBoundingBox = true;

        public AreaLight(string areaLightID)
        {
            ID = areaLightID;
        }

        public AreaLight(string areaLightID, Vector3 groundColor, Vector3 skyColor, Vector3 position, Vector3 scale, Vector3 direction)
        {
            ID = areaLightID;
            groundR = groundColor.X;
            groundG = groundColor.Y;
            groundB = groundColor.Z;

            skyR = skyColor.X;
            skyG = skyColor.Y;
            skyB = skyColor.Z;
        }

        public AreaLight(string areaLightID, Vector3 groundColor, Vector3 skyColor, Vector3 position, Vector3 scale, float rotX, float rotY, float rotZ)
        {
            ID = areaLightID;
        }

        public void setDirectionFromXYZAngles(float rotX, float rotY, float rotZ)
        {
            // calculate light vector from 3 rotation angles
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, rotX * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitY, rotY * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitZ, rotZ * ((float)Math.PI / 180f));

            direction = Vector3.Transform(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
        }
    }

    public class DirectionalLight
    {
        public float R = 1.0f;
        public float G = 1.0f;
        public float B = 1.0f;
        public float hue = 0.0f;
        public float saturation = 0.0f;
        public float intensity = 1.0f;

        // in degrees (converted to radians for calcultions)
        public float rotX = 0.0f; 
        public float rotY = 0.0f; 
        public float rotZ = 0.0f; 

        public Vector3 direction = new Vector3(0f, 0f, 1f);

        public string name = "";

        public DirectionalLight(float H, float S, float V, float rotX, float rotY, float rotZ, string name)
        {
            // calculate light color
            hue = H;
            saturation = S;
            intensity = V;
            RenderTools.HSV2RGB(hue, saturation, intensity, out R, out G, out B);

            // calculate light vector
            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, rotX * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitY, rotY * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitZ, rotZ * ((float)Math.PI / 180f));

            direction = Vector3.Transform(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();

            this.name = name;
        }

        public DirectionalLight(float H, float S, float V, Vector3 lightDirection, string name)
        {
            // calculate light color
            hue = H;
            saturation = S;
            intensity = V;
            RenderTools.HSV2RGB(hue, saturation, intensity, out R, out G, out B);

            direction = lightDirection;

            this.name = name;
        }

        public DirectionalLight()
        {

        }

        public void setHue(float hue)
        {
            this.hue = hue;
            RenderTools.HSV2RGB(hue, saturation, intensity, out R, out G, out B);
        }

        public void setSaturation(float saturation)
        {
            this.saturation = saturation;
            RenderTools.HSV2RGB(hue, saturation, intensity, out R, out G, out B);
        }

        public void setIntensity(float intensity)
        {
            this.intensity = intensity;
            RenderTools.HSV2RGB(hue, saturation, this.intensity, out R, out G, out B);
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

            direction = Vector3.Transform(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
        }

        public void setColorFromHSV(float H, float S, float V)
        {
            // calculate light color
            hue = H;
            saturation = S;
            intensity = V;
            RenderTools.HSV2RGB(hue, saturation, intensity, out R, out G, out B);
        }

        public override string ToString()
        {
            return name;
        }

    }

    class Lights
    {
        public static DirectionalLight diffuseLight = new DirectionalLight();
        public static DirectionalLight specularLight = new DirectionalLight();
        public static HemisphereFresnel fresnelLight = new HemisphereFresnel();
        public static DirectionalLight stageLight1 = new DirectionalLight();
        public static DirectionalLight stageLight2 = new DirectionalLight();
        public static DirectionalLight stageLight3 = new DirectionalLight();
        public static DirectionalLight stageLight4 = new DirectionalLight();

        public static DirectionalLight[] stageDiffuseLightSet = new DirectionalLight[64];
        public static Vector3[] stageFogSet = new Vector3[16];

        public static void SetLightsFromLightSetParam(ParamFile lightSet)
        {
            if (lightSet != null)
            {
                // stage diffuse
                for (int i = 0; i < stageDiffuseLightSet.Length; i++)
                {
                    float hue = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4 + i, 2);
                    float saturation = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4 + i, 3);
                    float value = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4 + i, 4);

                    //Debug.WriteLine(hue + ", " + saturation + ", " + value);
                    float rotX = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4 + i, 5);
                    float rotY = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4 + i, 6);
                    float rotZ = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4 + i, 7);

                    DirectionalLight light = new DirectionalLight(hue, saturation, value, rotX, rotY, rotZ, "Stage" + i.ToString());
                    stageDiffuseLightSet[i] = light;
                }

                // stage fog
                for (int i = 0; i < stageFogSet.Length; i++)
                {
                    float hue = (float)RenderTools.GetValueFromParamFile(lightSet, 2, 1 + i, 0);
                    float saturation = (float)RenderTools.GetValueFromParamFile(lightSet, 2, 1 + i, 1);
                    float value = (float)RenderTools.GetValueFromParamFile(lightSet, 2, 1 + i, 2);
                    float fogR = 0.0f, fogB = 0.0f, fogG = 0.0f;
                    RenderTools.HSV2RGB(hue, saturation, value, out fogR, out fogG, out fogB);
                    Vector3 color = new Vector3(fogR, fogG, fogB);
                    stageFogSet[i] = color;
                }

                // character diffuse
                {
                    float hue = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 29);
                    float saturation = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 30);
                    float value = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 31);

                    float rotX = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4, 5);
                    float rotY = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4, 6);
                    float rotZ = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4, 7);

                    diffuseLight = new DirectionalLight(hue, saturation, value, 0, 0, 0, "Diffuse");
                }

                // fresnel lighting
                {
                    float hueSky = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 8);
                    float satSky = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 9);
                    float intensitySky = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 10);

                    float hueGround = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 11);
                    float satGround = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 12);
                    float intensityGround = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 13);

                    fresnelLight = new HemisphereFresnel(hueGround, satGround, intensityGround, hueSky, satSky, intensitySky, "Fresnel");
                }

                // ambient color
                {
                    Runtime.amb_hue = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 33);
                    Runtime.amb_saturation = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 34);
                    Runtime.amb_intensity = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 35);
                }
            }
        }

    }
}
