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
        // Default bind location for dummy textures.
        private static readonly TextureUnit dummyTextureUnit = TextureUnit.Texture20;
        private static readonly int dummyTextureUnitOffset = 20;

        // Default bind location for NUT textures.
        private static readonly int nutTextureUnitOffset = 3;
        private static readonly TextureUnit nutTextureUnit = TextureUnit.Texture3;

        public static void SetMaterialPropertyUniforms(GenericMaterial genericMaterial, NUD.Material mat)
        {
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

        public static int BindTexture(NUD.MatTexture matTexture, int hash, int loc, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
        {
            if (Enum.IsDefined(typeof(NudEnums.DummyTexture), hash))
            {
                return BindDummyTexture(loc, dummyTextures[(NudEnums.DummyTexture)hash]);
            }
            else
            {
                GL.ActiveTexture(nutTextureUnit + loc);
                GL.BindTexture(TextureTarget.Texture2D, RenderTools.defaultTex.Id);
            }

            // Look through all loaded textures and not just the current modelcontainer.
            foreach (NUT nut in Runtime.TextureContainers)
            {
                Texture texture;
                if (nut.glTexByHashId.TryGetValue(hash, out texture))
                {
                    BindNutTexture(matTexture, texture);
                    break;
                }
            }

            return nutTextureUnitOffset + loc;
        }

        public static int BindDummyTexture(int loc, Texture texture)
        {
            GL.ActiveTexture(dummyTextureUnit + loc);
            texture.Bind();
            return dummyTextureUnitOffset + loc;
        }

        private static void BindNutTexture(NUD.MatTexture matTexture, Texture texture)
        {
            // Set the texture's parameters based on the material settings.
            texture.Bind();
            texture.TextureWrapS = NudEnums.wrapModeByMatValue[matTexture.wrapModeS];
            texture.TextureWrapT = NudEnums.wrapModeByMatValue[matTexture.wrapModeT];
            texture.MinFilter = NudEnums.minFilterByMatValue[matTexture.minFilter];
            texture.MagFilter = NudEnums.magFilterByMatValue[matTexture.magFilter];

            if (OpenGLExtensions.IsAvailable("GL_EXT_texture_filter_anisotropic") && (texture is Texture2D))
            {
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

            // This is necessary to prevent some models from disappearing. 
            SetTextureUniformsToDefaultTexture(shader, RenderTools.defaultTex.Id);

            // The order of the textures in the following section is critical. 
            int textureUnitIndexOffset = 0;
            if (mat.hasDiffuse && textureUnitIndexOffset < mat.textures.Count)
            {
                int hash = mat.textures[textureUnitIndexOffset].hash;
                if (mat.displayTexId != -1) hash = mat.displayTexId;
                GL.Uniform1(shader.GetVertexAttributeUniformLocation("dif"), BindTexture(mat.textures[textureUnitIndexOffset], hash, textureUnitIndexOffset, RenderTools.dummyTextures));
                mat.diffuse1ID = mat.textures[textureUnitIndexOffset].hash;
                textureUnitIndexOffset++;
            }

            SetTextureUniformAndSetTexId(shader, mat, mat.hasSphereMap, "spheremap", ref textureUnitIndexOffset, ref mat.sphereMapID, dummyTextures);
            SetTextureUniformAndSetTexId(shader, mat, mat.hasDiffuse2, "dif2", ref textureUnitIndexOffset, ref mat.diffuse2ID, dummyTextures);
            SetTextureUniformAndSetTexId(shader, mat, mat.hasDiffuse3, "dif3", ref textureUnitIndexOffset, ref mat.diffuse3ID, dummyTextures);
            SetTextureUniformAndSetTexId(shader, mat, mat.hasStageMap, "stagecube", ref textureUnitIndexOffset, ref mat.stageMapID, dummyTextures);
            SetTextureUniformAndSetTexId(shader, mat, mat.hasCubeMap, "cube", ref textureUnitIndexOffset, ref mat.cubeMapID, dummyTextures);
            SetTextureUniformAndSetTexId(shader, mat, mat.hasAoMap, "ao", ref textureUnitIndexOffset, ref mat.aoMapID, dummyTextures);
            SetTextureUniformAndSetTexId(shader, mat, mat.hasNormalMap, "normalMap", ref textureUnitIndexOffset, ref mat.normalID, dummyTextures);
            SetTextureUniformAndSetTexId(shader, mat, mat.hasRamp, "ramp", ref textureUnitIndexOffset, ref mat.rampID, dummyTextures);
            SetTextureUniformAndSetTexId(shader, mat, mat.hasDummyRamp, "dummyRamp", ref textureUnitIndexOffset, ref mat.dummyRampID, dummyTextures);
        }


        public static void SetTextureUniformAndSetTexId(Shader shader, NUD.Material mat, bool hasTex, string name, ref int textureIndex, ref int texIdForCurrentTextureType, 
            Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
        {
            // Bind the texture and create the uniform if the material has the right textures and flags. 
            if (hasTex && textureIndex < mat.textures.Count)
            {
                // Find the index for the shader uniform.
                // Bind the texture to a texture unit and then find where it was bound.
                int uniformLocation = shader.GetVertexAttributeUniformLocation(name);
                int textureUnit = NudUniforms.BindTexture(mat.textures[textureIndex], mat.textures[textureIndex].hash, textureIndex, dummyTextures);
                GL.Uniform1(uniformLocation, textureUnit);

                // We won't know what type a texture is used for until we iterate through the textures.
                texIdForCurrentTextureType = mat.textures[textureIndex].hash;

                // Move on to the next texture.
                textureIndex++;
            }
        }

        public static void SetTextureUniformsNudMatSphere(Shader shader, NUD.Material mat, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
        {
            SetHasTextureUniforms(shader, mat);
            SetRenderModeTextureUniforms(shader);

            // This is necessary to prevent some models from disappearing. 
            SetTextureUniformsToDefaultTexture(shader, RenderTools.defaultTex.Id);

            // The material shader just uses predefined textures from the Resources folder.
            NUD.MatTexture diffuse = new NUD.MatTexture((int)NudEnums.DummyTexture.DummyRamp);
            NUD.MatTexture cubeMapHigh = new NUD.MatTexture((int)NudEnums.DummyTexture.StageMapHigh);

            // The order of the textures in the following section is critical. 
            int textureUnitIndexOffset = 0;
            if (mat.hasDiffuse && textureUnitIndexOffset < mat.textures.Count)
            {
                GL.ActiveTexture(nutTextureUnit + textureUnitIndexOffset);
                GL.BindTexture(TextureTarget.Texture2D, NudMatSphereDrawing.sphereDifTex.Id);
                GL.Uniform1(shader.GetVertexAttributeUniformLocation("dif"), nutTextureUnitOffset + textureUnitIndexOffset);
                textureUnitIndexOffset++;
            }

            // Jigglypuff has weird eyes.
            if ((mat.Flags & 0xFFFFFFFF) == 0x9AE11163)
            {
                if (mat.hasDiffuse2)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("dif2"), BindTexture(diffuse, diffuse.hash, textureUnitIndexOffset, dummyTextures));
                    textureUnitIndexOffset++;
                }

                if (mat.hasNormalMap)
                {
                    GL.ActiveTexture(nutTextureUnit + textureUnitIndexOffset);
                    GL.BindTexture(TextureTarget.Texture2D, NudMatSphereDrawing.sphereNrmMapTex.Id);
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("normalMap"), nutTextureUnitOffset + textureUnitIndexOffset);
                    textureUnitIndexOffset++;
                }
            }
            else if ((mat.Flags & 0xFFFFFFFF) == 0x92F01101)
            {
                // Final smash mats and Mega Man's eyes.
                if (mat.hasDiffuse2)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("dif2"), BindTexture(diffuse, diffuse.hash, textureUnitIndexOffset, dummyTextures));
                    textureUnitIndexOffset++;
                }

                if (mat.hasRamp)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("ramp"), BindTexture(diffuse, diffuse.hash, textureUnitIndexOffset, dummyTextures));
                    textureUnitIndexOffset++;
                }

                if (mat.hasDummyRamp)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("dummyRamp"), BindTexture(diffuse, diffuse.hash, textureUnitIndexOffset, dummyTextures));
                    textureUnitIndexOffset++;
                }
            }
            else
            {
                if (mat.hasSphereMap)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("spheremap"), BindTexture(diffuse, diffuse.hash, textureUnitIndexOffset, dummyTextures));
                    textureUnitIndexOffset++;
                }

                if (mat.hasDiffuse2)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("dif2"), BindTexture(diffuse, diffuse.hash, textureUnitIndexOffset, dummyTextures));
                    textureUnitIndexOffset++;
                }

                if (mat.hasDiffuse3)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("dif3"), BindTexture(diffuse, diffuse.hash, textureUnitIndexOffset, dummyTextures));
                    textureUnitIndexOffset++;
                }

                // The stage cube maps already use the appropriate dummy texture.
                if (mat.hasStageMap)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("stagecube"), BindTexture(mat.textures[textureUnitIndexOffset], mat.stageMapID, textureUnitIndexOffset, dummyTextures));
                    textureUnitIndexOffset++;
                }

                if (mat.hasCubeMap)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("cube"), BindTexture(cubeMapHigh, cubeMapHigh.hash, textureUnitIndexOffset, dummyTextures));
                    textureUnitIndexOffset++;
                }

                if (mat.hasAoMap)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("ao"), BindTexture(diffuse, diffuse.hash, textureUnitIndexOffset, dummyTextures));
                    textureUnitIndexOffset++;
                }

                if (mat.hasNormalMap)
                {
                    GL.ActiveTexture(nutTextureUnit + textureUnitIndexOffset);
                    GL.BindTexture(TextureTarget.Texture2D, NudMatSphereDrawing.sphereNrmMapTex.Id);
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("normalMap"), nutTextureUnitOffset + textureUnitIndexOffset);
                    textureUnitIndexOffset++;
                }

                if (mat.hasRamp)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("ramp"), BindTexture(diffuse, diffuse.hash, textureUnitIndexOffset, RenderTools.dummyTextures));
                    textureUnitIndexOffset++;
                }

                if (mat.hasDummyRamp)
                {
                    GL.Uniform1(shader.GetVertexAttributeUniformLocation("dummyRamp"), BindTexture(diffuse, diffuse.hash, textureUnitIndexOffset, RenderTools.dummyTextures));
                    textureUnitIndexOffset++;
                }
            }
        }

        public static void SetTextureUniformsToDefaultTexture(Shader shader, int texture)
        {
            shader.SetTexture("dif", texture, TextureTarget.Texture2D, 0);
            shader.SetTexture("dif2", texture, TextureTarget.Texture2D, 0);
            shader.SetTexture("normalMap", texture, TextureTarget.Texture2D, 0);
            shader.SetTexture("cube", texture, TextureTarget.Texture2D, 2);
            shader.SetTexture("stagecube", texture, TextureTarget.Texture2D, 2);
            shader.SetTexture("spheremap", texture, TextureTarget.Texture2D, 0);
            shader.SetTexture("ao", texture, TextureTarget.Texture2D, 0);
            shader.SetTexture("ramp", texture, TextureTarget.Texture2D, 0);
        }

        public static void SetRenderModeTextureUniforms(Shader shader)
        {
            shader.SetTexture("UVTestPattern", RenderTools.uvTestPattern.Id, TextureTarget.Texture2D, 10);
            shader.SetTexture("weightRamp1", RenderTools.boneWeightGradient.Id, TextureTarget.Texture2D, 11);
            shader.SetTexture("weightRamp2", RenderTools.boneWeightGradient2.Id, TextureTarget.Texture2D, 12);
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
