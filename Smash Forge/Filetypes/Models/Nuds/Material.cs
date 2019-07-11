using System;
using System.Collections.Generic;
using Smash_Forge.Filetypes.Models.Nuds;

namespace Smash_Forge
{
    public partial class NUD
    {
        public class Material
        {
            public Dictionary<string, float[]> entries = new Dictionary<string, float[]>();
            public Dictionary<string, float[]> anims = new Dictionary<string, float[]>();
            public List<MatTexture> textures = new List<MatTexture>();

            public uint Flags
            {
                get
                {
                    return RebuildFlag4thByte();
                }
                set
                {
                    flag = value;
                    CheckFlags();
                    UpdateLabelledTextureIds();
                }
            }
            private uint flag;

            public int blendMode = 0;
            public int dstFactor = 0;
            public int srcFactor = 0;
            public int alphaTest = 0;
            public int alphaFunction = 0;
            public int RefAlpha = 0;
            public int cullMode = 0;
            public int displayTexId = -1;

            public int unknown1 = 0;
            public int unkownWater = 0;
            public int zBufferOffset = 0;

            //flags
            public bool Glow { get; set; } = false;
            public bool HasShadow { get; set; } = false;
            public bool UseVertexColor { get; private set; } = false;
            public bool UseReflectionMask { get; private set; } = false;
            public bool UseColorGainOffset { get; private set; } = false;
            public bool HasBayoHair { get; private set; } = false;
            public bool UseDiffuseBlend { get; private set; } = false;
            public bool SoftLightBrighten { get; private set; } = false;

            // Texture flags
            public bool HasDiffuse { get; private set; } = false;
            public bool HasNormalMap { get; private set; } = false;
            public bool HasDiffuse2 { get; private set; } = false;
            public bool HasDiffuse3 { get; private set; } = false;
            public bool HasAoMap { get; private set; } = false;
            public bool HasStageMap { get; private set; } = false;
            public bool HasCubeMap { get; private set; } = false;
            public bool HasRamp { get; private set; } = false;
            public bool HasSphereMap { get; set; } = false;
            public bool HasDummyRamp { get; private set; } = false;

            // texture IDs for preserving existing textures
            public int diffuse1ID = 0;
            public int diffuse2ID = 0;
            public int diffuse3ID = 0;
            public int normalID = 0;
            public int rampID = (int)NudEnums.DummyTexture.DummyRamp;
            public int dummyRampID = (int)NudEnums.DummyTexture.DummyRamp;
            public int sphereMapID = 0;
            public int aoMapID = 0;
            public int stageMapID = (int)NudEnums.DummyTexture.StageMapHigh;
            public int cubeMapID = 0;

            public Material()
            {

            }

            public Material Clone()
            {
                Material m = new Material();

                foreach (KeyValuePair<string, float[]> e in entries)
                    m.entries.Add(e.Key, e.Value);

                m.Flags = Flags;
                m.blendMode = blendMode;
                m.dstFactor = dstFactor;
                m.srcFactor = srcFactor;
                m.alphaTest = alphaTest;
                m.alphaFunction = alphaFunction;
                m.RefAlpha = RefAlpha;
                m.cullMode = cullMode;
                m.displayTexId = displayTexId;

                m.unknown1 = 0;
                m.unkownWater = 0;
                m.zBufferOffset = 0;

                foreach(MatTexture t in textures)
                {
                    m.textures.Add(t.Clone());
                }

                return m;
            }

            public static Material GetDefault()
            {
                Material material = new Material();
                material.Flags = 0x94010161;
                material.cullMode = 0x0405;
                material.entries.Add("NU_colorSamplerUV", new float[] { 1, 1, 0, 0 });
                material.entries.Add("NU_fresnelColor", new float[] { 1, 1, 1, 1 });
                material.entries.Add("NU_blinkColor", new float[] { 0, 0, 0, 0 });
                material.entries.Add("NU_aoMinGain", new float[] { 0, 0, 0, 0 });
                material.entries.Add("NU_lightMapColorOffset", new float[] { 0, 0, 0, 0 });
                material.entries.Add("NU_fresnelParams", new float[] { 1, 0, 0, 0 });
                material.entries.Add("NU_alphaBlendParams", new float[] { 0, 0, 0, 0 });
                material.entries.Add("NU_materialHash", new float[] { FileData.toFloat(0x7E538F65), 0, 0, 0 });

                material.textures.Add(new MatTexture(0x10000000));
                material.textures.Add(MatTexture.GetDefault());
                return material;
            }

            public static Material GetStageDefault()
            {
                Material material = new Material
                {
                    Flags = 0xA2001001,
                    RefAlpha = 128,
                    cullMode = 1029
                };

                // Display a default texture rather than a dummy texture.
                material.textures.Add(new MatTexture(0));

                material.entries.Add("NU_colorSamplerUV", new float[] { 1, 1, 0, 0 });
                material.entries.Add("NU_diffuseColor", new float[] { 1, 1, 1, 0.5f });
                material.entries.Add("NU_materialHash", new float[] { BitConverter.ToSingle(new byte[] { 0x12, 0xEE, 0x2A, 0x1B }, 0), 0, 0, 0 });
                return material;
            }

            public void CopyTextureIds(Material other)
            {
                // Copies all the texture IDs from the source material to the current material. 
                // This is useful for preserving Tex IDs when using a preset or changing flags. 

                for (int i = 0; i < Math.Min(textures.Count, other.textures.Count); i++)
                {
                    if (HasDiffuse)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (HasDiffuse2)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (HasDiffuse3)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (HasStageMap)
                    {
                        // Don't preserve stageMap ID.
                        continue;
                    }
                    if (HasCubeMap)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (HasSphereMap)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (HasAoMap)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (HasNormalMap)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (HasRamp)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (HasDummyRamp)
                    {
                        // Dummy ramp should almost always be 0x10080000.
                        continue;
                    }
                }
            }

            public void MakeMetal(int newDifTexId, int newCubeTexId, float[] minGain, float[] refColor, float[] fresParams, float[] fresColor, bool preserveDiffuse = false, bool preserveNrmMap = true)
            {
                UpdateLabelledTextureIds();

                float materialHash = -1f;
                if (entries.ContainsKey("NU_materialHash"))
                    materialHash = entries["NU_materialHash"][0];
                anims.Clear();
                entries.Clear();

                // The texture ID used for diffuse later. 
                int difTexID = newDifTexId;
                if (preserveDiffuse)
                    difTexID = diffuse1ID;

                // add all the textures
                textures.Clear();
                displayTexId = -1;

                MatTexture diffuse = new MatTexture(difTexID);
                MatTexture cube = new MatTexture(newCubeTexId);
                MatTexture normal = new MatTexture(normalID);
                MatTexture dummyRamp = MatTexture.GetDefault();
                dummyRamp.hash = 0x10080000;

                if (HasNormalMap && preserveNrmMap)
                {
                    Flags = 0x9601106B;
                    textures.Add(diffuse);
                    textures.Add(cube);
                    textures.Add(normal);
                    textures.Add(dummyRamp);
                }
                else
                {
                    Flags = 0x96011069;
                    textures.Add(diffuse);
                    textures.Add(cube);
                    textures.Add(dummyRamp);
                }

                // add material properties
                entries.Add("NU_colorSamplerUV", new float[] { 1, 1, 0, 0 });
                entries.Add("NU_fresnelColor", fresColor);
                entries.Add("NU_blinkColor", new float[] { 0f, 0f, 0f, 0 });
                entries.Add("NU_reflectionColor", refColor);
                entries.Add("NU_aoMinGain", minGain);
                entries.Add("NU_lightMapColorOffset", new float[] { 0f, 0f, 0f, 0 });
                entries.Add("NU_fresnelParams", fresParams);
                entries.Add("NU_alphaBlendParams", new float[] { 0f, 0f, 0f, 0 });
                entries.Add("NU_materialHash", new float[] { materialHash, 0f, 0f, 0 });
            }

            public uint RebuildFlag4thByte()
            {
                byte new4thByte = 0;
                if (HasDiffuse)
                    new4thByte |= (byte)NudEnums.TextureFlag.DiffuseMap;
                if (HasNormalMap)
                    new4thByte |= (byte)NudEnums.TextureFlag.NormalMap;
                if (HasCubeMap || HasRamp)
                    new4thByte |= (byte)NudEnums.TextureFlag.RampCubeMap;
                if (HasStageMap || HasAoMap)
                    new4thByte |= (byte)NudEnums.TextureFlag.StageAOMap;
                if (HasSphereMap)
                    new4thByte |= (byte)NudEnums.TextureFlag.SphereMap;
                if (Glow)
                    new4thByte |= (byte)NudEnums.TextureFlag.Glow;
                if (HasShadow)
                    new4thByte |= (byte)NudEnums.TextureFlag.Shadow;
                if (HasDummyRamp)
                    new4thByte |= (byte)NudEnums.TextureFlag.DummyRamp; 
                flag = (flag & 0xFFFFFF00) | new4thByte;

                return flag;
            }

            private void UpdateLabelledTextureIds()
            {
                int textureIndex = 0;
                if ((flag & 0xFFFFFFFF) == 0x9AE11163)
                {
                    UpdateLabelledId(HasDiffuse, ref diffuse1ID, ref textureIndex);
                    UpdateLabelledId(HasDiffuse2, ref diffuse2ID, ref textureIndex);
                    UpdateLabelledId(HasNormalMap, ref normalID, ref textureIndex);
                }
                else
                {
                    // The order of the textures here is critical. 
                    UpdateLabelledId(HasDiffuse, ref diffuse1ID, ref textureIndex);
                    UpdateLabelledId(HasSphereMap, ref sphereMapID, ref textureIndex);
                    UpdateLabelledId(HasDiffuse2, ref diffuse2ID, ref textureIndex);
                    UpdateLabelledId(HasDiffuse3, ref diffuse3ID, ref textureIndex);
                    UpdateLabelledId(HasStageMap, ref stageMapID, ref textureIndex);
                    UpdateLabelledId(HasCubeMap, ref cubeMapID, ref textureIndex);
                    UpdateLabelledId(HasAoMap, ref aoMapID, ref textureIndex);
                    UpdateLabelledId(HasNormalMap, ref normalID, ref textureIndex);
                    UpdateLabelledId(HasRamp, ref rampID, ref textureIndex);
                    UpdateLabelledId(HasDummyRamp, ref dummyRampID, ref textureIndex);
                }
            }

            private void UpdateLabelledId(bool hasTexture, ref int textureId, ref int textureIndex)
            {
                if (hasTexture && textureIndex < textures.Count)
                {
                    textureId = textures[textureIndex].hash;
                    textureIndex += 1;
                }
            }

            private void CheckFlags()
            {
                int intFlags = ((int)flag);
                Glow = (intFlags & (int)NudEnums.TextureFlag.Glow) > 0;
                HasShadow = (intFlags & (int)NudEnums.TextureFlag.Shadow) > 0;
                CheckMisc(intFlags);
                CheckTextures(flag);
            }

            private void CheckMisc(int matFlags)
            {
                // Some hacky workarounds until I understand flags better.
                UseColorGainOffset = CheckColorGain(flag);
                UseDiffuseBlend = (matFlags & 0xD0090000) == 0xD0090000 || (matFlags & 0x90005000) == 0x90005000;
                UseVertexColor = CheckVertexColor(flag);
                UseReflectionMask = (matFlags & 0xFFFFFF00) == 0xF8820000;
                HasBayoHair = (matFlags & 0x00FF0000) == 0x00420000;
                SoftLightBrighten = ((matFlags & 0x00FF0000) == 0x00810000 || (matFlags & 0xFFFF0000) == 0xFA600000);
            }

            private bool CheckVertexColor(uint matFlags)
            {
                // Characters and stages use different values for enabling vertex color.
                // Always use vertex color for effect materials for now.
                byte byte1 = (byte) ((matFlags & 0xFF000000) >> 24);
                bool vertexColor = (byte1 == 0x94) || (byte1 == 0x9A) || (byte1 == 0x9C) || (byte1 == 0xA2) 
                    || (byte1 == 0xA4) || (byte1 == 0xB0);

                return vertexColor;
            }

            private bool CheckColorGain(uint matFlags)
            {
                byte byte1 = (byte)(matFlags >> 24);
                byte byte2 = (byte)(matFlags >> 16);
                byte byte4 = (byte)(matFlags & 0xFF);

                bool hasLightingChannel = (byte1 & 0x0C) == 0x0C;
                bool hasByte2 = (byte2 == 0x61) || (byte2== 0x42) || (byte2 == 0x44);
                bool hasByte4 = (byte4 == 0x61);

                return hasLightingChannel && hasByte2 && hasByte4;
            }

            private void CheckTextures(uint matFlags)
            {
                // Why figure out how these values work when you can just hardcode all the important ones?
                // Effect materials use 4th byte 00 but often still have a diffuse texture.
                byte byte1 = (byte)(matFlags >> 24);
                byte byte2 = (byte)(matFlags >> 16);
                byte byte3 = (byte)(matFlags >> 8);
                byte byte4 = (byte)(matFlags & 0xFF);

                bool isEffectMaterial = (byte1 & 0xF0) == 0xB0;
                HasDiffuse = (matFlags & (byte)NudEnums.TextureFlag.DiffuseMap) > 0 || isEffectMaterial;

                HasSphereMap = (byte4 & (byte)NudEnums.TextureFlag.SphereMap) > 0;

                HasNormalMap = (byte4 & (byte)NudEnums.TextureFlag.NormalMap) > 0;

                HasDummyRamp = (byte4 & (byte)NudEnums.TextureFlag.DummyRamp) > 0;

                HasAoMap = (byte4 & (byte)NudEnums.TextureFlag.StageAOMap) > 0 && !HasDummyRamp;

                HasStageMap = (byte4 & (byte)NudEnums.TextureFlag.StageAOMap) > 0 && HasDummyRamp;

                bool hasRampCubeMap = (matFlags & (int)NudEnums.TextureFlag.RampCubeMap) > 0;
                HasCubeMap = (matFlags & (int)NudEnums.TextureFlag.RampCubeMap) > 0 && (!HasDummyRamp) && (!HasSphereMap);
                HasRamp = (matFlags & (int)NudEnums.TextureFlag.RampCubeMap) > 0 && HasDummyRamp;

                HasDiffuse3 = (byte3 & 0x91) == 0x91 || (byte3 & 0x96) == 0x96 || (byte3 & 0x99) == 0x99;

                HasDiffuse2 = hasRampCubeMap && ((matFlags & (int)NudEnums.TextureFlag.NormalMap) == 0)
                    && (HasDummyRamp || HasDiffuse3);

                // Jigglypuff has weird eyes, so just hardcode it.
                if ((matFlags & 0xFFFFFFFF) == 0x9AE11163)
                {
                    HasDiffuse2 = true;
                    HasNormalMap = true;
                }

                // Mega Man also has strange eyes.
                if ((matFlags & 0xFFFFFFFF) == 0x92F01101)
                {
                    HasDiffuse2 = true;
                    HasRamp = true;
                    HasDummyRamp = true;
                }
            }
        }
    }
}

