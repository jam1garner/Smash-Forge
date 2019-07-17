using System;
using System.Collections.Generic;
using SmashForge.Filetypes.Models.Nuds;

namespace SmashForge
{
    public partial class Nud
    {
        public class Material
        {
            private readonly Dictionary<string, float[]> propertyValuesByName = new Dictionary<string, float[]>();

            private readonly Dictionary<string, float[]> animatedPropertyValuesByName = new Dictionary<string, float[]>();

            // HACK: Used for rendering optimizations.
            public bool ShouldUpdateRendering { get; set; } = true;

            public List<MatTexture> textures = new List<MatTexture>();

            public uint Flags
            {
                get
                {
                    // TODO: This should be called when updating the booleans.
                    byte new4thByte = RebuildFlag4thByte();
                    flags = (flags & 0xFFFFFF00) | new4thByte;

                    return flags;
                }
                set
                {
                    flags = value;
                    CheckFlags();
                    UpdateLabeledTextureIds();
                }
            }
            private uint flags;

            public int BlendMode { get; set; }
            public int DstFactor { get; set; }
            public int SrcFactor { get; set; }

            public int AlphaTest { get; set; }
            public int AlphaFunction { get; set; }
            public int RefAlpha { get; set; }

            public int CullMode { get; set; }

            public int Unk1 { get; set; }
            public int Unk2 { get; set; }

            public int ZBufferOffset { get; set; }

            public int displayTexId = -1;

            //flags
            public bool Glow { get; set; }

            public bool HasShadow { get; set; }

            public bool UseVertexColor { get; private set; }

            public bool UseReflectionMask { get; private set; }

            public bool UseColorGainOffset { get; private set; }

            public bool HasBayoHair { get; private set; }

            public bool UseDiffuseBlend { get; private set; }

            public bool SoftLightBrighten { get; private set; }

            // Texture flags
            public bool HasDiffuse { get; private set; }
            public bool HasDiffuse2 { get; private set; }
            public bool HasDiffuse3 { get; private set; }

            public bool HasNormalMap { get; private set; }

            public bool HasAoMap { get; private set; }

            public bool HasStageMap { get; private set; }

            public bool HasCubeMap { get; private set; }

            public bool HasRamp { get; private set; }

            public bool HasSphereMap { get; set; }

            public bool HasDummyRamp { get; private set; }

            // texture IDs for preserving existing textures
            public int Diffuse1Id => diffuse1Id;
            private int diffuse1Id;

            public int Diffuse2Id => diffuse2Id;
            private int diffuse2Id;

            public int Diffuse3Id => diffuse3Id;
            private int diffuse3Id;

            public int NormalId => normalId;
            private int normalId;

            public int RampId => rampId;
            private int rampId = (int)NudEnums.DummyTexture.DummyRamp;

            public int DummyRampId => dummyRampId;
            private int dummyRampId = (int)NudEnums.DummyTexture.DummyRamp;

            public int SphereMapId => sphereMapId;
            private int sphereMapId;

            public int AoMapId => aoMapId;
            private int aoMapId;

            public int StageMapId => stageMapId;
            private int stageMapId = (int)NudEnums.DummyTexture.StageMapHigh;

            public int CubeMapId => cubeMapId;
            private int cubeMapId;

            public bool HasProperty(string name) => propertyValuesByName.ContainsKey(name);

            public bool HasPropertyAnim(string name) => animatedPropertyValuesByName.ContainsKey(name);

            public int PropertyCount => propertyValuesByName.Count;

            public float[] GetPropertyValues(string name)
            {
                return propertyValuesByName[name];
            }

            public float[] GetPropertyValuesAnim(string name) => animatedPropertyValuesByName[name];

            public void UpdateProperty(string name, float[] values)
            {
                propertyValuesByName[name] = values;
                ShouldUpdateRendering = true;
            }

            public void UpdateProperty(string name, float value, int index)
            { 
                propertyValuesByName[name][index] = value;
                ShouldUpdateRendering = true;
            }

            public void UpdatePropertyAnim(string name, float[] values)
            {
                animatedPropertyValuesByName[name] = values;
                ShouldUpdateRendering = true;
            }

            public bool RemoveProperty(string name)
            {
                ShouldUpdateRendering = true;
                return propertyValuesByName.Remove(name);
            }

            public IEnumerable<string> PropertyNames => propertyValuesByName.Keys;

            public void ClearAnims()
            {
                animatedPropertyValuesByName.Clear();
            }

            public bool EqualTextures(Material other)
            {
                if (other == null || textures.Count != other.textures.Count)
                    return false;

                for (int i = 0; i < textures.Count; i++)
                {
                    if (textures[i].hash != other.textures[i].hash)
                        return false;
                }

                return true;
            }

            public Material Clone()
            {
                Material m = new Material();

                foreach (KeyValuePair<string, float[]> e in propertyValuesByName)
                    m.propertyValuesByName.Add(e.Key, e.Value);

                m.Flags = Flags;
                m.BlendMode = BlendMode;
                m.DstFactor = DstFactor;
                m.SrcFactor = SrcFactor;
                m.AlphaTest = AlphaTest;
                m.AlphaFunction = AlphaFunction;
                m.RefAlpha = RefAlpha;
                m.CullMode = CullMode;

                m.displayTexId = displayTexId;

                m.Unk1 = 0;
                m.Unk2 = 0;
                m.ZBufferOffset = 0;

                foreach(MatTexture t in textures)
                {
                    m.textures.Add(t.Clone());
                }

                return m;
            }

            public static Material GetDefault()
            {
                Material material = new Material
                {
                    Flags = 0x94010161,
                    CullMode = 0x0405
                };
                material.propertyValuesByName.Add("NU_colorSamplerUV", new float[] { 1, 1, 0, 0 });
                material.propertyValuesByName.Add("NU_fresnelColor", new float[] { 1, 1, 1, 1 });
                material.propertyValuesByName.Add("NU_blinkColor", new float[] { 0, 0, 0, 0 });
                material.propertyValuesByName.Add("NU_aoMinGain", new float[] { 0, 0, 0, 0 });
                material.propertyValuesByName.Add("NU_lightMapColorOffset", new float[] { 0, 0, 0, 0 });
                material.propertyValuesByName.Add("NU_fresnelParams", new float[] { 1, 0, 0, 0 });
                material.propertyValuesByName.Add("NU_alphaBlendParams", new float[] { 0, 0, 0, 0 });
                material.propertyValuesByName.Add("NU_materialHash", new float[] { FileData.ToFloat(0x7E538F65), 0, 0, 0 });

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
                    CullMode = 1029
                };

                // Display a default texture rather than a dummy texture.
                material.textures.Add(new MatTexture(0));

                material.propertyValuesByName.Add("NU_colorSamplerUV", new float[] { 1, 1, 0, 0 });
                material.propertyValuesByName.Add("NU_diffuseColor", new float[] { 1, 1, 1, 0.5f });
                material.propertyValuesByName.Add("NU_materialHash", new float[] { BitConverter.ToSingle(new byte[] { 0x12, 0xEE, 0x2A, 0x1B }, 0), 0, 0, 0 });
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
                UpdateLabeledTextureIds();

                float materialHash = -1f;
                if (propertyValuesByName.ContainsKey("NU_materialHash"))
                    materialHash = propertyValuesByName["NU_materialHash"][0];
                animatedPropertyValuesByName.Clear();
                propertyValuesByName.Clear();

                // The texture ID used for diffuse later. 
                int difTexId = newDifTexId;
                if (preserveDiffuse)
                    difTexId = Diffuse1Id;

                // add all the textures
                textures.Clear();
                displayTexId = -1;

                MatTexture diffuse = new MatTexture(difTexId);
                MatTexture cube = new MatTexture(newCubeTexId);
                MatTexture normal = new MatTexture(normalId);
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
                propertyValuesByName.Add("NU_colorSamplerUV", new float[] { 1, 1, 0, 0 });
                propertyValuesByName.Add("NU_fresnelColor", fresColor);
                propertyValuesByName.Add("NU_blinkColor", new float[] { 0f, 0f, 0f, 0 });
                propertyValuesByName.Add("NU_reflectionColor", refColor);
                propertyValuesByName.Add("NU_aoMinGain", minGain);
                propertyValuesByName.Add("NU_lightMapColorOffset", new float[] { 0f, 0f, 0f, 0 });
                propertyValuesByName.Add("NU_fresnelParams", fresParams);
                propertyValuesByName.Add("NU_alphaBlendParams", new float[] { 0f, 0f, 0f, 0 });
                propertyValuesByName.Add("NU_materialHash", new float[] { materialHash, 0f, 0f, 0 });
            }

            public byte RebuildFlag4thByte()
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

                return new4thByte;
            }

            private void UpdateLabeledTextureIds()
            {
                int textureIndex = 0;
                if ((flags & 0xFFFFFFFF) == 0x9AE11163)
                {
                    UpdateLabeledId(HasDiffuse, ref diffuse1Id, ref textureIndex);
                    UpdateLabeledId(HasDiffuse2, ref diffuse2Id, ref textureIndex);
                    UpdateLabeledId(HasNormalMap, ref normalId, ref textureIndex);
                }
                else
                {
                    // The order of the textures here is critical. 
                    UpdateLabeledId(HasDiffuse, ref diffuse1Id, ref textureIndex);
                    UpdateLabeledId(HasSphereMap, ref sphereMapId, ref textureIndex);
                    UpdateLabeledId(HasDiffuse2, ref diffuse2Id, ref textureIndex);
                    UpdateLabeledId(HasDiffuse3, ref diffuse3Id, ref textureIndex);
                    UpdateLabeledId(HasStageMap, ref stageMapId, ref textureIndex);
                    UpdateLabeledId(HasCubeMap, ref cubeMapId, ref textureIndex);
                    UpdateLabeledId(HasAoMap, ref aoMapId, ref textureIndex);
                    UpdateLabeledId(HasNormalMap, ref normalId, ref textureIndex);
                    UpdateLabeledId(HasRamp, ref rampId, ref textureIndex);
                    UpdateLabeledId(HasDummyRamp, ref dummyRampId, ref textureIndex);
                }
            }

            private void UpdateLabeledId(bool hasTexture, ref int textureId, ref int textureIndex)
            {
                if (hasTexture && textureIndex < textures.Count)
                {
                    textureId = textures[textureIndex].hash;
                    textureIndex += 1;
                }
            }

            private void CheckFlags()
            {
                int intFlags = ((int)flags);
                Glow = (intFlags & (int)NudEnums.TextureFlag.Glow) > 0;
                HasShadow = (intFlags & (int)NudEnums.TextureFlag.Shadow) > 0;
                CheckMisc(intFlags);
                CheckTextures(flags);
            }

            private void CheckMisc(int matFlags)
            {
                // Some hacky workarounds until I understand flags better.
                UseColorGainOffset = CheckColorGain(flags);
                UseDiffuseBlend = (matFlags & 0xD0090000) == 0xD0090000 || (matFlags & 0x90005000) == 0x90005000;
                UseVertexColor = CheckVertexColor(flags);
                UseReflectionMask = (matFlags & 0xFFFFFF00) == 0xF8820000;
                HasBayoHair = (matFlags & 0x00FF0000) == 0x00420000;
                SoftLightBrighten = ((matFlags & 0x00FF0000) == 0x00810000 || (matFlags & 0xFFFF0000) == 0xFA600000);
            }

            private static bool CheckVertexColor(uint matFlags)
            {
                // Characters and stages use different values for enabling vertex color.
                // Always use vertex color for effect materials for now.
                byte byte1 = (byte) ((matFlags & 0xFF000000) >> 24);
                bool vertexColor = (byte1 == 0x94) || (byte1 == 0x9A) || (byte1 == 0x9C) || (byte1 == 0xA2) 
                    || (byte1 == 0xA4) || (byte1 == 0xB0);

                return vertexColor;
            }

            private static bool CheckColorGain(uint matFlags)
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
                // Why figure out how these values work when you can just hard code all the important ones?
                // Effect materials use 4th byte 00 but often still have a diffuse texture.
                byte byte1 = (byte)(matFlags >> 24);
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

                // Jigglypuff has weird eyes, so just hard code it.
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

