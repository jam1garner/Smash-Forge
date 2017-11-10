using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SALT.PARAMS;
using SALT.Graphics;
using System.Diagnostics;

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

        public HemisphereFresnel(float groundH, float groundS, float groundV, float skyH, float skyS, float skyV, 
            float skyAngle, float groundAngle, string name)
        {
            this.groundHue = groundH;
            this.groundSaturation = groundS;
            this.groundIntensity = groundV;
            this.skyHue = skyH;
            this.skySaturation = skyS;
            this.skyIntensity = skyV;
            RenderTools.HSV2RGB(skyHue, skySaturation, skyIntensity, out skyR, out skyG, out skyB);

            this.skyAngle = skyAngle;
            this.groundAngle = groundAngle;

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
        public string id = "";

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
            id = areaLightID;
        }

        public AreaLight(string areaLightID, Vector3 groundColor, Vector3 skyColor, Vector3 position, Vector3 scale, Vector3 direction)
        {
            id = areaLightID;
            groundR = groundColor.X;
            groundG = groundColor.Y;
            groundB = groundColor.Z;

            skyR = skyColor.X;
            skyG = skyColor.Y;
            skyB = skyColor.Z;
        }

        public AreaLight(string areaLightID, Vector3 groundColor, Vector3 skyColor, Vector3 position, Vector3 scale, float rotX, float rotY, float rotZ)
        {
            id = areaLightID;

            positionX = position.X;
            positionY = position.Y;
            positionZ = position.Z;

            scaleX = scale.X;
            scaleY = scale.Y;
            scaleZ = scale.Z;

            skyR = skyColor.X;
            skyG = skyColor.Y;
            skyB = skyColor.Z;

            groundR = groundColor.X;
            groundB = groundColor.Y;
            groundG = groundColor.Z;

        }

        public void setDirectionFromXYZAngles(float rotX, float rotY, float rotZ)
        {
            // calculate light vector from 3 rotation angles
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, rotX * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitY, rotY * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitZ, rotZ * ((float)Math.PI / 180f));

            direction = Vector3.Transform(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
        }

        public override string ToString()
        {
            return id;
        }
    }

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

        public string name = "";

        public DirectionalLight(float difH, float difS, float difV, float ambH, float ambS, float ambV, float rotX, float rotY, float rotZ, string name)
        {
            // calculate light color
            difHue = difH;
            difSaturation = difS;
            difIntensity = difV;
            ambHue = ambH;
            ambSaturation = ambS;
            ambIntensity = ambV;
            RenderTools.HSV2RGB(difHue, difSaturation, difIntensity, out difR, out difG, out difB);
            RenderTools.HSV2RGB(ambHue, ambSaturation, ambIntensity, out ambR, out ambG, out ambB);

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
            difHue = H;
            difSaturation = S;
            difIntensity = V;
            RenderTools.HSV2RGB(difHue, difSaturation, difIntensity, out difR, out difG, out difB);

            direction = lightDirection;

            this.name = name;
        }

        public DirectionalLight()
        {

        }

        public void setHue(float hue)
        {
            this.difHue = hue;
            RenderTools.HSV2RGB(hue, difSaturation, difIntensity, out difR, out difG, out difB);
        }

        public void setSaturation(float saturation)
        {
            this.difSaturation = saturation;
            RenderTools.HSV2RGB(difHue, saturation, difIntensity, out difR, out difG, out difB);
        }

        public void setIntensity(float intensity)
        {
            this.difIntensity = intensity;
            RenderTools.HSV2RGB(difHue, difSaturation, this.difIntensity, out difR, out difG, out difB);
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
            difHue = H;
            difSaturation = S;
            difIntensity = V;
            RenderTools.HSV2RGB(difHue, difSaturation, difIntensity, out difR, out difG, out difB);
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

        public static List<AreaLight> areaLights = new List<AreaLight>();

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

                    DirectionalLight light = new DirectionalLight(hue, saturation, value, 0, 0, 0, rotX, rotY, rotZ, "Stage " + i.ToString());
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
                    float difHue = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 29);
                    float difSaturation = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 30);
                    float difIntensity = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 31);

                    float ambHue = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 33);
                    float ambSaturation = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 34);
                    float ambIntensity = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 35);

                    float rotX = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4, 5);
                    float rotY = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4, 6);
                    float rotZ = (float)RenderTools.GetValueFromParamFile(lightSet, 1, 4, 7);

                    diffuseLight = new DirectionalLight(difHue, difSaturation, difIntensity, ambHue, ambSaturation, ambIntensity, 0, 0, 0, "Diffuse");
                }

                // fresnel lighting
                {
                    float hueSky = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 8);
                    float satSky = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 9);
                    float intensitySky = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 10);

                    float hueGround = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 11);
                    float satGround = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 12);
                    float intensityGround = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 13);

                    float skyAngle = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 14);
                    float groundAngle = (float)RenderTools.GetValueFromParamFile(lightSet, 0, 0, 15);

                    fresnelLight = new HemisphereFresnel(hueGround, satGround, intensityGround, hueSky, satSky, intensitySky, 
                        skyAngle, groundAngle, "Fresnel");
                }
            }
        }

        public static void CreateAreaLightsFromXMB(XMBFile xmb)
        {
            if (xmb != null)
            {
                foreach (XMBEntry entry in xmb.Entries)
                {
                    if (entry.Children.Count > 0)
                    {
                        foreach (XMBEntry lightEntry in entry.Children)
                        {
                            AreaLight newAreaLight = CreateAreaLightFromXMBEntry(lightEntry);
                            areaLights.Add(newAreaLight);
                            Debug.WriteLine("\n");
                        }
                    }
                }
            }
        }

        private static AreaLight CreateAreaLightFromXMBEntry(XMBEntry entry)
        {
            // really need to clean this up
            string id = "";
            float posX = 0;
            float posY = 0;
            float posZ = 0;
            float scaleX = 0;
            float scaleY = 0;
            float scaleZ = 0;
            float groundR = 0;
            float groundG = 0;
            float groundB = 0;
            float skyR = 0;
            float skyG = 0;
            float skyB = 0;

            for (int i = 0; i < entry.Expressions.Count; i++)
            {
                string expression = entry.Expressions[i];

                string[] sections = expression.Split('=');
                Debug.WriteLine(sections[0]);
                string name = sections[0];
                string[] values = sections[1].Split(',');

                if (name.Contains("id"))
                {
                    id = values[0];
                }
                if (name.Contains("pos"))
                {
                    float.TryParse(values[0], out posX);
                    float.TryParse(values[1], out posY);
                    float.TryParse(values[2], out posZ);
                    Debug.WriteLine("position:" + posX + "," + posY + "," + posZ);

                }
                if (name.Contains("scale"))
                {
                    float.TryParse(values[0], out scaleX);
                    float.TryParse(values[1], out scaleY);
                    float.TryParse(values[2], out scaleZ);
                    Debug.WriteLine("scale:" + scaleX + "," + scaleY + "," + scaleZ);
                }
                if (name.Contains("col_ground"))
                {
                    float.TryParse(values[0], out groundR);
                    float.TryParse(values[1], out groundG);
                    float.TryParse(values[2], out groundB);
                }
                if (name.Contains("col_ceiling"))
                {
                    float.TryParse(values[0], out skyR);
                    float.TryParse(values[1], out skyG);
                    float.TryParse(values[2], out skyB);
                }
            }

            AreaLight newAreaLight = new AreaLight(id, new Vector3(groundR, groundG, groundB), new Vector3(skyR, skyG, skyB), 
                new Vector3(posX, posY, posZ), new Vector3(scaleX, scaleY, scaleZ), 0, 0, 0);
            return newAreaLight;
        }

    }
}
