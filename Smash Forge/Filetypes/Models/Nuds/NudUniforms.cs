using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel.Materials;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects.Textures;
using SFGraphics.Tools;
using Smash_Forge.Rendering;
using System;
using System.Collections.Generic;

namespace Smash_Forge.Filetypes.Models.Nuds
{
    public static class NudUniforms
    {
        private static readonly Dictionary<string, Vector4> defaultValueByProperty = new Dictionary<string, Vector4>()
        {
            { "NU_colorSamplerUV",         new Vector4(1, 1, 0, 0) },
            { "NU_colorSampler2UV",        new Vector4(1, 1, 0, 0) },
            { "NU_colorSampler3UV",        new Vector4(1, 1, 0, 0) },
            { "NU_normalSamplerAUV",       new Vector4(1, 1, 0, 0) },
            { "NU_normalSamplerBUV",       new Vector4(1, 1, 0, 0) },
            { "NU_aoMinGain",              new Vector4(0, 0, 0, 0) },
            { "NU_colorGain",              new Vector4(1, 1, 1, 1) },
            { "NU_finalColorGain",         new Vector4(1, 1, 1, 1) },
            { "NU_finalColorGain2",        new Vector4(1, 1, 1, 1) },
            { "NU_finalColorGain3",        new Vector4(1, 1, 1, 1) },
            { "NU_colorOffset",            new Vector4(0, 0, 0, 0) },
            { "NU_diffuseColor",           new Vector4(1, 1, 1, 0.5f) },
            { "NU_characterColor",         new Vector4(1, 1, 1, 1) },
            { "NU_specularColor",          new Vector4(0, 0, 0, 0) },
            { "NU_specularColorGain",      new Vector4(1, 1, 1, 1) },
            { "NU_specularParams",         new Vector4(0, 0, 0, 0) },
            { "NU_fresnelColor",           new Vector4(0, 0, 0, 0) },
            { "NU_fresnelParams",          new Vector4(0, 0, 0, 0) },
            { "NU_reflectionColor",        new Vector4(0, 0, 0, 0) },
            { "NU_reflectionParams",       new Vector4(0, 0, 0, 0) },
            { "NU_fogColor",               new Vector4(0, 0, 0, 0) },
            { "NU_fogParams",              new Vector4(0, 1, 0, 0) },
            { "NU_softLightingParams",     new Vector4(0, 0, 0, 0) },
            { "NU_customSoftLightParams",  new Vector4(0, 0, 0, 0) },
            { "NU_normalParams",           new Vector4(1, 0, 0, 0) },
            { "NU_zOffset",                new Vector4(0, 0, 0, 0) },
            { "NU_angleFadeParams",        new Vector4(0, 0, 0, 0) },
            { "NU_dualNormalScrollParams", new Vector4(0, 0, 0, 0) },
            { "NU_alphaBlendParams",       new Vector4(0, 0, 0, 0) },
            { "NU_effCombinerColor0",      new Vector4(1, 1, 1, 1) },
            { "NU_effCombinerColor1",      new Vector4(1, 1, 1, 1) },
            { "NU_effColorGain",           new Vector4(1, 1, 1, 1) },
            { "NU_effScaleUV",             new Vector4(1, 1, 0, 0) },
            { "NU_effTransUV",             new Vector4(1, 1, 0, 0) },
            { "NU_effMaxUV",               new Vector4(1, 1, 0, 0) },
            { "NU_effUniverseParam",       new Vector4(1, 0, 0, 0) },
        };

        // Default bind location for NUT textures.
        private static readonly int nutTextureUnitOffset = 0;

        public static void SetMaterialPropertyUniforms(GenericMaterial genericMaterial, NUD.Material mat)
        {
            foreach (var property in defaultValueByProperty)
            {
                MatPropertyShaderUniform(genericMaterial, mat, property.Key, property.Value);
            }

            // Create some conditionals rather than using different shaders.
            HasMatPropertyShaderUniform(genericMaterial, mat, "NU_softLightingParams", "hasSoftLight");
            HasMatPropertyShaderUniform(genericMaterial, mat, "NU_customSoftLightParams", "hasCustomSoftLight");
            HasMatPropertyShaderUniform(genericMaterial, mat, "NU_specularParams", "hasSpecularParams");
            HasMatPropertyShaderUniform(genericMaterial, mat, "NU_dualNormalScrollParams", "hasDualNormal");
            HasMatPropertyShaderUniform(genericMaterial, mat, "NU_normalSamplerAUV", "hasNrmSamplerAUV");
            HasMatPropertyShaderUniform(genericMaterial, mat, "NU_normalSamplerBUV", "hasNrmSamplerBUV");
            HasMatPropertyShaderUniform(genericMaterial, mat, "NU_finalColorGain", "hasFinalColorGain");
            HasMatPropertyShaderUniform(genericMaterial, mat, "NU_effUniverseParam", "hasUniverseParam");
        }

        private static void HasMatPropertyShaderUniform(GenericMaterial genericMaterial, NUD.Material mat, string propertyName, string uniformName)
        {
            bool hasValue = mat.entries.ContainsKey(propertyName) || mat.anims.ContainsKey(propertyName);
            if (hasValue)
                genericMaterial.AddInt(uniformName, 1);
            else
                genericMaterial.AddInt(uniformName, 0);
        }

        private static void MatPropertyShaderUniform(GenericMaterial genericMaterial, NUD.Material mat, string propertyName, Vector4 defaultValue)
        {
            // Attempt to get the values from the material. 
            float[] values = null;
            mat.entries.TryGetValue(propertyName, out values);
            if (mat.anims.ContainsKey(propertyName))
                values = mat.anims[propertyName];

            if (values == null || values.Length != 4)
                values = new float[] { defaultValue.X, defaultValue.Y, defaultValue.Z, defaultValue.W };

            string uniformName = propertyName.Replace("NU_", "");
            genericMaterial.AddVector4(uniformName, new Vector4(values[0], values[1], values[2], values[3]));
        }

        public static Texture GetTexture(int hash, NUD.MatTexture matTexture, int loc, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
        {
            if (Enum.IsDefined(typeof(NudEnums.DummyTexture), hash))
            {
                return dummyTextures[(NudEnums.DummyTexture)hash];
            }

            // Look through all loaded textures and not just the current modelcontainer.
            foreach (NUT nut in Runtime.TextureContainers)
            {
                Texture texture;
                if (nut.glTexByHashId.TryGetValue(hash, out texture))
                {
                    SetTextureParameters(texture, matTexture);
                    return texture;
                }
            }

            return RenderTools.defaultTex;
        }

        private static void SetTextureParameters(Texture target, NUD.MatTexture matTexture)
        {
            // TODO: Use a sampler object.
            // Set the texture's parameters based on the material settings.
            target.TextureWrapS = NudEnums.wrapModeByMatValue[matTexture.wrapModeS];
            target.TextureWrapT = NudEnums.wrapModeByMatValue[matTexture.wrapModeT];
            target.MinFilter = NudEnums.minFilterByMatValue[matTexture.minFilter];
            target.MagFilter = NudEnums.magFilterByMatValue[matTexture.magFilter];

            if (OpenGLExtensions.IsAvailable("GL_EXT_texture_filter_anisotropic") && (target is Texture2D))
            {
                target.Bind();
                TextureParameterName anisotropy = (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt;
                GL.TexParameter(TextureTarget.Texture2D, anisotropy, 0.0f);
                if (matTexture.mipDetail == 0x4 || matTexture.mipDetail == 0x6)
                    GL.TexParameter(TextureTarget.Texture2D, anisotropy, 4.0f);
            }
        }

        public static void SetTextureUniforms(Shader shader, NUD.Material mat, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
        {
            SetHasTextureUniforms(shader, mat);
            SetRenderModeTextureUniforms(shader);

            int textureIndex = 0;

            // The type of texture can be partially determined by texture order.
            GenericMaterial textures = new GenericMaterial(nutTextureUnitOffset);
            textures.AddTexture("dif", GetTextureAndSetTexId(mat, mat.hasDiffuse, "dif", ref textureIndex, ref mat.diffuse1ID, dummyTextures));
            textures.AddTexture("spheremap", GetTextureAndSetTexId(mat, mat.hasSphereMap, "spheremap", ref textureIndex, ref mat.sphereMapID, dummyTextures));
            textures.AddTexture("dif2", GetTextureAndSetTexId(mat, mat.hasDiffuse2, "dif2", ref textureIndex, ref mat.diffuse2ID, dummyTextures));
            textures.AddTexture("dif3", GetTextureAndSetTexId(mat, mat.hasDiffuse3, "dif3", ref textureIndex, ref mat.diffuse3ID, dummyTextures));
            textures.AddTexture("stagecube", GetTextureAndSetTexId(mat, mat.hasStageMap, "stagecube", ref textureIndex, ref mat.stageMapID, dummyTextures));
            textures.AddTexture("cube", GetTextureAndSetTexId(mat, mat.hasCubeMap, "cube", ref textureIndex, ref mat.cubeMapID, dummyTextures));
            textures.AddTexture("ao", GetTextureAndSetTexId(mat, mat.hasAoMap, "ao", ref textureIndex, ref mat.aoMapID, dummyTextures));
            textures.AddTexture("normalMap", GetTextureAndSetTexId(mat, mat.hasNormalMap, "normalMap", ref textureIndex, ref mat.normalID, dummyTextures));
            textures.AddTexture("ramp", GetTextureAndSetTexId(mat, mat.hasRamp, "ramp", ref textureIndex, ref mat.rampID, dummyTextures));
            textures.AddTexture("dummyRamp", GetTextureAndSetTexId(mat, mat.hasDummyRamp, "dummyRamp", ref textureIndex, ref mat.dummyRampID, dummyTextures));

            textures.SetShaderUniforms(shader);
        }

        public static Texture GetTextureAndSetTexId(NUD.Material mat, bool hasTex, string name, ref int textureIndex, ref int texIdForCurrentTextureType, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
        {
            Texture texture;
            if (hasTex && textureIndex < mat.textures.Count)
            {
                // We won't know what type a texture is used for until we iterate through the textures.
                texIdForCurrentTextureType = mat.textures[textureIndex].hash;
                texture = GetTexture(mat.textures[textureIndex].hash, mat.textures[textureIndex], textureIndex, dummyTextures);
                textureIndex++;
            }
            else
            {
                texture = RenderTools.defaultTex;
            }

            return texture;
        }

        public static void SetTextureUniformsNudMatSphere(Shader shader, NUD.Material mat, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
        {
            SetHasTextureUniforms(shader, mat);
            SetRenderModeTextureUniforms(shader);

            // The material shader just uses predefined textures from the Resources folder.
            NUD.MatTexture diffuse = new NUD.MatTexture((int)NudEnums.DummyTexture.DummyRamp);
            NUD.MatTexture cubeMapHigh = new NUD.MatTexture((int)NudEnums.DummyTexture.StageMapHigh);

            SetHasTextureUniforms(shader, mat);
            SetRenderModeTextureUniforms(shader);

            // The type of texture can be partially determined by texture order.
            GenericMaterial textures = new GenericMaterial(nutTextureUnitOffset);
            textures.AddTexture("dif", GetSphereTexture("dif", dummyTextures));
            textures.AddTexture("spheremap", GetSphereTexture("spheremap", dummyTextures));
            textures.AddTexture("dif2", GetSphereTexture("dif2", dummyTextures));
            textures.AddTexture("dif3", GetSphereTexture("dif3", dummyTextures));
            textures.AddTexture("stagecube", GetSphereTexture("stagecube", dummyTextures));
            textures.AddTexture("cube", GetSphereTexture("cube", dummyTextures));
            textures.AddTexture("ao", GetSphereTexture("ao", dummyTextures));
            textures.AddTexture("normalMap", GetSphereTexture("normalMap", dummyTextures));
            textures.AddTexture("ramp", GetSphereTexture("ramp", dummyTextures));
            textures.AddTexture("dummyRamp", GetSphereTexture("dummyRamp", dummyTextures));

            textures.SetShaderUniforms(shader);
        }

        private static Texture GetSphereTexture(string name, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
        {
            if (name.Contains("dif"))
                return NudMatSphereDrawing.sphereDifTex;
            else if (name.Contains("normal"))
                return NudMatSphereDrawing.sphereNrmMapTex;
            else if (name.Contains("cube"))
                return dummyTextures[NudEnums.DummyTexture.StageMapHigh];
            else
                return NudMatSphereDrawing.sphereDifTex;
        }

        public static void SetRenderModeTextureUniforms(Shader shader)
        {
            shader.SetTexture("UVTestPattern", RenderTools.uvTestPattern, 10);
            shader.SetTexture("weightRamp1", RenderTools.boneWeightGradient, 11);
            shader.SetTexture("weightRamp2", RenderTools.boneWeightGradient2, 12);
        }

        public static void SetHasTextureUniforms(Shader shader, NUD.Material mat)
        {
            shader.SetBoolToInt("hasDif", mat.hasDiffuse);
            shader.SetBoolToInt("hasDif2", mat.hasDiffuse2);
            shader.SetBoolToInt("hasDif3", mat.hasDiffuse3);
            shader.SetBoolToInt("hasStage", mat.hasStageMap);
            shader.SetBoolToInt("hasCube", mat.hasCubeMap);
            shader.SetBoolToInt("hasAo", mat.hasAoMap);
            shader.SetBoolToInt("hasNrm", mat.hasNormalMap);
            shader.SetBoolToInt("hasRamp", mat.hasRamp);
            shader.SetBoolToInt("hasDummyRamp", mat.hasDummyRamp);
            shader.SetBoolToInt("hasColorGainOffset", mat.useColorGainOffset);
            shader.SetBoolToInt("useDiffuseBlend", mat.useDiffuseBlend);
            shader.SetBoolToInt("hasSphereMap", mat.hasSphereMap);
            shader.SetBoolToInt("hasBayoHair", mat.hasBayoHair);
            shader.SetBoolToInt("useDifRefMask", mat.useReflectionMask);
            shader.SetBoolToInt("softLightBrighten", mat.softLightBrighten);
        }
    }
}
