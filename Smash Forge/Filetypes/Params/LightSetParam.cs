using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALT.PARAMS;
using OpenTK;
using Smash_Forge.Rendering.Lights;

namespace Smash_Forge.Params
{
    public class LightSetParam
    {
        private ParamFile paramFile;

        public DirectionalLight characterDiffuse;
        public DirectionalLight characterDiffuse2;
        public DirectionalLight characterDiffuse3;

        public DirectionalLight[] stageDiffuseLights = new DirectionalLight[64];
        public Vector3[] stageFogSet = new Vector3[16];
        public HemisphereFresnel fresnelLight;

        public LightSetParam(string fileName)
        {
            paramFile = new ParamFile(fileName);
            // TODO: Update all the lights using the param values. 
            // stage diffuse
            for (int i = 0; i < stageDiffuseLights.Length; i++)
            {
                stageDiffuseLights[i] = CreateDirectionalLightFromLightSet(paramFile, 4 + i, "Stage " + i);
            }

            // stage fog
            for (int i = 0; i < stageFogSet.Length; i++)
            {
                stageFogSet[i] = CreateFogColorFromFogSet(paramFile, i);
            }

            characterDiffuse = CreateCharDiffuseLightFromLightSet(paramFile);
            characterDiffuse2 = CreateDirectionalLightFromLightSet(paramFile, 0, "Diffuse2");
            characterDiffuse3 = CreateDirectionalLightFromLightSet(paramFile, 1, "Diffuse3");
            fresnelLight = CreateFresnelLightFromLightSet(paramFile);
        }

        public void Save(string fileName)
        {
            // TODO: Update all the param values.
            paramFile.Export(fileName);
        }

        public static HemisphereFresnel CreateFresnelLightFromLightSet(ParamFile lightSet)
        {
            Vector3 hsvGround = new Vector3(0);
            hsvGround.X = (float)ParamTools.GetParamValue(lightSet, 0, 0, 8);
            hsvGround.Y = (float)ParamTools.GetParamValue(lightSet, 0, 0, 9);
            hsvGround.Z = (float)ParamTools.GetParamValue(lightSet, 0, 0, 10);

            Vector3 hsvSky = new Vector3(0);
            hsvSky.X = (float)ParamTools.GetParamValue(lightSet, 0, 0, 11);
            hsvSky.Y = (float)ParamTools.GetParamValue(lightSet, 0, 0, 12);
            hsvSky.Z = (float)ParamTools.GetParamValue(lightSet, 0, 0, 13);

            float skyAngle = (float)ParamTools.GetParamValue(lightSet, 0, 0, 14);
            float groundAngle = (float)ParamTools.GetParamValue(lightSet, 0, 0, 15);

            return new HemisphereFresnel(hsvGround, hsvSky, skyAngle, groundAngle, "Fresnel");
        }

        public static DirectionalLight CreateCharDiffuseLightFromLightSet(ParamFile lightSet)
        {
            Vector3 diffuseHsv = new Vector3(0);
            diffuseHsv.X = (float)ParamTools.GetParamValue(lightSet, 0, 0, 29);
            diffuseHsv.Y = (float)ParamTools.GetParamValue(lightSet, 0, 0, 30);
            diffuseHsv.Z = (float)ParamTools.GetParamValue(lightSet, 0, 0, 31);

            Vector3 ambientHsv = new Vector3(0);
            ambientHsv.X = (float)ParamTools.GetParamValue(lightSet, 0, 0, 33);
            ambientHsv.Y = (float)ParamTools.GetParamValue(lightSet, 0, 0, 34);
            ambientHsv.Z = (float)ParamTools.GetParamValue(lightSet, 0, 0, 35);

            float rotX = (float)ParamTools.GetParamValue(lightSet, 1, 4, 5);
            float rotY = (float)ParamTools.GetParamValue(lightSet, 1, 4, 6);
            float rotZ = (float)ParamTools.GetParamValue(lightSet, 1, 4, 7);

            return new DirectionalLight(diffuseHsv, ambientHsv, 0, 0, 0, "Diffuse");
        }

        public static Vector3 CreateFogColorFromFogSet(ParamFile lightSet, int i)
        {
            float hue = (float)ParamTools.GetParamValue(lightSet, 2, 1 + i, 0);
            float saturation = (float)ParamTools.GetParamValue(lightSet, 2, 1 + i, 1);
            float value = (float)ParamTools.GetParamValue(lightSet, 2, 1 + i, 2);
            float fogR = 0.0f, fogB = 0.0f, fogG = 0.0f;
            ColorTools.HsvToRgb(hue, saturation, value, out fogR, out fogG, out fogB);
            Vector3 color = new Vector3(fogR, fogG, fogB);
            return color;
        }

        public static DirectionalLight CreateDirectionalLightFromLightSet(ParamFile lightSet, int lightNumber, string name)
        {
            bool enabled = (uint)ParamTools.GetParamValue(lightSet, 1, lightNumber, 1) == 1;
            Vector3 hsv = new Vector3(0);
            hsv.X = (float)ParamTools.GetParamValue(lightSet, 1, lightNumber, 2);
            hsv.Y = (float)ParamTools.GetParamValue(lightSet, 1, lightNumber, 3);
            hsv.Z = (float)ParamTools.GetParamValue(lightSet, 1, lightNumber, 4);

            float rotX = (float)ParamTools.GetParamValue(lightSet, 1, lightNumber, 5);
            float rotY = (float)ParamTools.GetParamValue(lightSet, 1, lightNumber, 6);
            float rotZ = (float)ParamTools.GetParamValue(lightSet, 1, lightNumber, 7);

            DirectionalLight newLight = new DirectionalLight(hsv, new Vector3(0), rotX, rotY, rotZ, name);
            newLight.enabled = enabled; // doesn't render properly for some stages
            return newLight;
        }
    }
}
