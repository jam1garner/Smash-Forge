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
using Smash_Forge.Params;


namespace Smash_Forge.Rendering.Lights
{
    class LightTools
    {
        // still not sure what controls this yet
        public static DirectionalLight specularLight = new DirectionalLight(new Vector3(0), new Vector3(0), 0, 0, 0, "Specular");

        // area_light.xmb
        public static List<AreaLight> areaLights = new List<AreaLight>();

        // light_map.xmb
        public static List<LightMap> lightMaps = new List<LightMap>();

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
