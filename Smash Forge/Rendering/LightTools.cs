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
    class LightTools
    {
        // character diffuse from light_set_param.bin
        public static DirectionalLight diffuseLight = new DirectionalLight(0, 0, 1, 0, 0, 0.75f, 0, 0, 0, "Diffuse");
        public static DirectionalLight diffuseLight2 = new DirectionalLight(0, 0, 1, 0, 0, 0.75f, 0, 0, 0, "Diffuse2");
        public static DirectionalLight diffuseLight3 = new DirectionalLight(0, 0, 1, 0, 0, 0.75f, 0, 0, 0, "Diffuse3");

        // still not sure what controls this yet
        public static DirectionalLight specularLight = new DirectionalLight();

        // hemisphere fresnel from light_set_param.bin
        public static HemisphereFresnel fresnelLight = new HemisphereFresnel(0, 0, 0, 0, 0, 1, 0, 0, "Fresnel");

        // used for rendering
        public static DirectionalLight stageLight1 = new DirectionalLight();
        public static DirectionalLight stageLight2 = new DirectionalLight();
        public static DirectionalLight stageLight3 = new DirectionalLight();
        public static DirectionalLight stageLight4 = new DirectionalLight();

        // stage diffuse from light_set_param.bin
        public static DirectionalLight[] stageDiffuseLightSet = new DirectionalLight[64];
        public static Vector3[] stageFogSet = new Vector3[16];

        // area_light.xmb
        public static List<AreaLight> areaLights = new List<AreaLight>();

        // light_map.xmb
        public static List<LightMap> lightMaps = new List<LightMap>();

        public static void SetLightsFromLightSetParam(ParamFile lightSet)
        {
            if (lightSet != null)
            {
                // stage diffuse
                for (int i = 0; i < stageDiffuseLightSet.Length; i++)
                {
                    stageDiffuseLightSet[i] = CreateDirectionalLightFromLightSet(lightSet, 4 + i, "Stage " + i);
                }

                // stage fog
                for (int i = 0; i < stageFogSet.Length; i++)
                {
                    stageFogSet[i] = CreateFogColorFromFogSet(lightSet, i);
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

                // character diffuse 2
                {
                    diffuseLight2 = CreateDirectionalLightFromLightSet(lightSet, 0, "Diffuse2");
                }

                // character diffuse 3
                {
                    diffuseLight3 = CreateDirectionalLightFromLightSet(lightSet, 1, "Diffuse3");
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

        private static Vector3 CreateFogColorFromFogSet(ParamFile lightSet, int i)
        {
            float hue = (float)RenderTools.GetValueFromParamFile(lightSet, 2, 1 + i, 0);
            float saturation = (float)RenderTools.GetValueFromParamFile(lightSet, 2, 1 + i, 1);
            float value = (float)RenderTools.GetValueFromParamFile(lightSet, 2, 1 + i, 2);
            float fogR = 0.0f, fogB = 0.0f, fogG = 0.0f;
            ColorTools.HSV2RGB(hue, saturation, value, out fogR, out fogG, out fogB);
            Vector3 color = new Vector3(fogR, fogG, fogB);
            return color;
        }

        private static DirectionalLight CreateDirectionalLightFromLightSet(ParamFile lightSet, int lightNumber, string name)
        {
            bool enabled = (uint)RenderTools.GetValueFromParamFile(lightSet, 1, lightNumber, 1) == 1;
            float hue = (float)RenderTools.GetValueFromParamFile(lightSet, 1, lightNumber, 2);
            float saturation = (float)RenderTools.GetValueFromParamFile(lightSet, 1, lightNumber, 3);
            float value = (float)RenderTools.GetValueFromParamFile(lightSet, 1, lightNumber, 4);

            float rotX = (float)RenderTools.GetValueFromParamFile(lightSet, 1, lightNumber, 5);
            float rotY = (float)RenderTools.GetValueFromParamFile(lightSet, 1, lightNumber, 6);
            float rotZ = (float)RenderTools.GetValueFromParamFile(lightSet, 1, lightNumber, 7);

            DirectionalLight newLight = new DirectionalLight(hue, saturation, value, 0, 0, 0, rotX, rotY, rotZ, name);
            newLight.enabled = enabled; // doesn't render properly for some stages
            return newLight;
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
                        }
                    }
                }
            }
        }

        public static void CreateLightMapsFromXMB(XMBFile xmb)
        {
            if (xmb != null)
            {
                foreach (XMBEntry entry in xmb.Entries)
                {
                    if (entry.Children.Count > 0)
                    {
                        foreach (XMBEntry lightMapEntry in entry.Children)
                        {
                            LightMap newLightMap = CreateLightMapFromXMBEntry(lightMapEntry);
                            lightMaps.Add(newLightMap);
                        }
                    }
                }
            }
        }

        private static LightMap CreateLightMapFromXMBEntry(XMBEntry entry)
        {
            float scaleX = 1;
            float scaleY = 1;
            float scaleZ = 1;

            int texture_index = 0x10080000;
            int texture_addr = 0;

            float posX = 0;
            float posY = 0;
            float posZ = 0;

            float rotX = 0;
            float rotY = 0;
            float rotZ = 0;

            string id = "";

            for (int i = 0; i < entry.Expressions.Count; i++)
            {
                string expression = entry.Expressions[i];

                string[] sections = expression.Split('=');
                string name = sections[0];
                string[] values = sections[1].Split(',');

                if (name.Contains("id"))
                {
                    id = values[0];
                }
                if (name.Contains("texture_index"))
                {
                    // remove 0x from the beginning
                    string index = values[0].Trim();
                    if (index.StartsWith("0x"))
                        index = index.Substring(2);

                    int.TryParse(index, NumberStyles.HexNumber, null, out texture_index);
                }
                if (name.Contains("texture_addr"))
                {
                    int.TryParse(values[0], out texture_addr);
                }
                if (name.Contains("pos"))
                {
                    float.TryParse(values[0], out posX);
                    float.TryParse(values[1], out posY);
                    float.TryParse(values[2], out posZ);
                }
                if (name.Contains("scale"))
                {
                    float.TryParse(values[0], out scaleX);
                    float.TryParse(values[1], out scaleY);
                    float.TryParse(values[2], out scaleZ);
                }
                if (name.Contains("rot"))
                {
                    float.TryParse(values[0], out rotX);
                    float.TryParse(values[1], out rotY);
                    float.TryParse(values[2], out rotZ);
                }
            }

            Vector3 position = new Vector3(posX, posY, posZ);
            Vector3 scale = new Vector3(scaleX, scaleY, scaleZ);

            return new LightMap(new Vector3(scaleX, scaleY, scaleZ), texture_index, texture_addr, new Vector3(posX, posY, posZ), rotX, rotY, rotZ, id);
        }


        private static AreaLight CreateAreaLightFromXMBEntry(XMBEntry entry)
        {
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
            float rotX = 0;
            float rotY = 0;
            float rotZ = 0;

            for (int i = 0; i < entry.Expressions.Count; i++)
            {
                string expression = entry.Expressions[i];

                string[] sections = expression.Split('=');
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
                }
                if (name.Contains("scale"))
                {
                    float.TryParse(values[0], out scaleX);
                    float.TryParse(values[1], out scaleY);
                    float.TryParse(values[2], out scaleZ);
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
                if (name.Contains("rot"))
                {
                    float.TryParse(values[0], out rotX);
                    float.TryParse(values[1], out rotY);
                    float.TryParse(values[2], out rotZ);
                }
            }

            Vector3 groundColor = new Vector3(groundR, groundG, groundB);
            Vector3 skyColor = new Vector3(skyR, skyG, skyB);
            Vector3 position = new Vector3(posX, posY, posZ);
            Vector3 scale = new Vector3(scaleX, scaleY, scaleZ);

            return new AreaLight(id, groundColor, skyColor, position, scale, rotX, rotY, rotZ);
        }
    }
}
