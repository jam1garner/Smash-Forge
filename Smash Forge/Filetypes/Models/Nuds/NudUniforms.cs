using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Smash_Forge.Rendering;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.Tools;
using SFGenericModel.Materials;

namespace Smash_Forge.Filetypes.Models.Nuds
{
    public static class NudUniforms
    {
        // Default bind location for NUT textures.
        private static readonly int nutTextureUnitOffset = 0;

        public static void SetMaterialPropertyUniforms(GenericMaterial genericMaterial, NUD.Material mat)
        {
            // TODO: Use a dictionary for this.

            // UV samplers
            MatPropertyShaderUniform(genericMaterial, mat, "NU_colorSamplerUV", 1, 1, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_colorSampler2UV", 1, 1, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_colorSampler3UV", 1, 1, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_normalSamplerAUV", 1, 1, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_normalSamplerBUV", 1, 1, 0, 0);

            // Diffuse Color
            MatPropertyShaderUniform(genericMaterial, mat, "NU_aoMinGain", 0, 0, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_colorGain", 1, 1, 1, 1);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_finalColorGain", 1, 1, 1, 1);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_finalColorGain2", 1, 1, 1, 1);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_finalColorGain3", 1, 1, 1, 1);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_colorOffset", 0, 0, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_diffuseColor", 1, 1, 1, 0.5f);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_characterColor", 1, 1, 1, 1);

            // Specular
            MatPropertyShaderUniform(genericMaterial, mat, "NU_specularColor", 0, 0, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_specularColorGain", 1, 1, 1, 1);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_specularParams", 0, 0, 0, 0);

            // Fresnel
            MatPropertyShaderUniform(genericMaterial, mat, "NU_fresnelColor", 0, 0, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_fresnelParams", 0, 0, 0, 0);

            // Reflections
            MatPropertyShaderUniform(genericMaterial, mat, "NU_reflectionColor", 0, 0, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_reflectionParams", 0, 0, 0, 0);

            // Fog
            MatPropertyShaderUniform(genericMaterial, mat, "NU_fogColor", 0, 0, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_fogParams", 0, 1, 0, 0);

            // Soft Lighting
            MatPropertyShaderUniform(genericMaterial, mat, "NU_softLightingParams", 0, 0, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_customSoftLightParams", 0, 0, 0, 0);

            // Misc Properties
            MatPropertyShaderUniform(genericMaterial, mat, "NU_normalParams", 1, 0, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_zOffset", 0, 0, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_angleFadeParams", 0, 0, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_dualNormalScrollParams", 0, 0, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_alphaBlendParams", 0, 0, 0, 0);

            // Effect Materials
            MatPropertyShaderUniform(genericMaterial, mat, "NU_effCombinerColor0", 1, 1, 1, 1);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_effCombinerColor1", 1, 1, 1, 1);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_effColorGain", 1, 1, 1, 1);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_effScaleUV", 1, 1, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_effTransUV", 1, 1, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_effMaxUV", 1, 1, 0, 0);
            MatPropertyShaderUniform(genericMaterial, mat, "NU_effUniverseParam", 1, 0, 0, 0);

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
            float[] values;
            mat.entries.TryGetValue(propertyName, out values);
            if (mat.anims.ContainsKey(propertyName))
                values = mat.anims[propertyName];

            int hasParam = 1;
            if (values == null)
                hasParam = 0;

            genericMaterial.AddInt(uniformName, hasParam);
        }

        private static void MatPropertyShaderUniform(GenericMaterial genericMaterial, NUD.Material mat, string propertyName, float default1,
            float default2, float default3, float default4)
        {
            // Attempt to get the values from the material's properties. 
            // Otherwise, use the specified default values.
            float[] values;
            mat.entries.TryGetValue(propertyName, out values);
            if (mat.anims.ContainsKey(propertyName))
            {
                values = mat.anims[propertyName];
            }
            if (values == null)
                values = new float[] { default1, default2, default3, default4 };

            string uniformName = propertyName.Substring(3); // remove the NU_ from name

            if (values.Length == 4)
                genericMaterial.AddVector4(uniformName, new Vector4(values[0], values[1], values[2], values[3]));
            else
                Debug.WriteLine(uniformName + " invalid parameter count: " + values.Length);
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
