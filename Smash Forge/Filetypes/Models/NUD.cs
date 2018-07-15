using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Windows.Forms;
using SALT.Graphics;
using System.Text;
using Smash_Forge.Rendering.Lights;
using Smash_Forge.Rendering;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;
using SFGraphics.Tools;
using SFGraphics.Cameras;


namespace Smash_Forge
{
    public class NUD : FileBase
    {
        // OpenGL Buffers
        private BufferObject positionVbo;
        private BufferObject elementsIbo;
        private BufferObject bonesUbo;
        private BufferObject selectVbo;

        // Default bind location for dummy textures.
        private static readonly TextureUnit dummyTextureUnit = TextureUnit.Texture20;
        private static readonly int dummyTextureUnitOffset = 20;

        // Default bind location for NUT textures.
        private static readonly int nutTextureUnitOffset = 3;
        private static readonly TextureUnit nutTextureUnit = TextureUnit.Texture3;

        public const int SMASH = 0;
        public const int POKKEN = 1;
        public int type = SMASH;
        public int boneCount = 0;
        public bool hasBones = false;
        public float[] boundingSphere = new float[4];

        // Just used for rendering.
        private List<Mesh> depthSortedMeshes = new List<Mesh>();

        // xmb stuff
        public int lightSetNumber = 0;
        public int directUVTime = 0;
        public int drawRange = 0;
        public int drawingOrder = 0;
        public bool useDirectUVTime = false;
        public string modelType = "";

        public override Endianness Endian { get; set; }

        private static readonly Dictionary<int, BlendingFactorSrc> srcFactorsByMatValue = new Dictionary<int, BlendingFactorSrc>()
        {
            { 0x00, BlendingFactorSrc.One },
            { 0x01, BlendingFactorSrc.SrcAlpha},
            { 0x02, BlendingFactorSrc.One},
            { 0x03, BlendingFactorSrc.SrcAlpha},
            { 0x04, BlendingFactorSrc.SrcAlpha},
        };

        private static readonly Dictionary<int, BlendingFactorDest> dstFactorsByMatValue = new Dictionary<int, BlendingFactorDest>()
        {
            { 0x00, BlendingFactorDest.Zero },
            { 0x01, BlendingFactorDest.OneMinusSrcAlpha},
            { 0x02, BlendingFactorDest.One},
            { 0x03, BlendingFactorDest.One},
        };

        private static readonly Dictionary<int, TextureWrapMode> wrapmode = new Dictionary<int, TextureWrapMode>()
        {
            { 0x01, TextureWrapMode.Repeat},
            { 0x02, TextureWrapMode.MirroredRepeat},
            { 0x03, TextureWrapMode.ClampToEdge}
        };

        private static readonly Dictionary<int, TextureMinFilter> minfilter = new Dictionary<int, TextureMinFilter>()
        {
            { 0x00, TextureMinFilter.LinearMipmapLinear},
            { 0x01, TextureMinFilter.Nearest},
            { 0x02, TextureMinFilter.Linear},
            { 0x03, TextureMinFilter.NearestMipmapLinear},
        };

        static readonly Dictionary<int, TextureMagFilter> magfilter = new Dictionary<int, TextureMagFilter>()
        {
            { 0x00, TextureMagFilter.Linear},
            { 0x01, TextureMagFilter.Nearest},
            { 0x02, TextureMagFilter.Linear}
        };

        public enum TextureFlags
        {
            Glow = 0x00000080,
            Shadow = 0x00000040,
            DummyRamp = 0x00000020,
            SphereMap = 0x00000010,
            StageAOMap = 0x00000008,
            RampCubeMap = 0x00000004,
            NormalMap = 0x00000002,
            DiffuseMap = 0x00000001
        }

        public enum DummyTextures
        {
            StageMapLow = 0x10101000,
            StageMapHigh = 0x10102000,
            PokemonStadium = 0x10040001,
            PunchOut = 0x10040000,
            DummyRamp = 0x10080000,
            ShadowMap = 0x10100000
        }

        public static readonly Dictionary<int, Color> lightSetColorByIndex = new Dictionary<int, Color>()
        {
            { 0, Color.Red },
            { 1, Color.Blue },
            { 2, Color.Green },
            { 3, Color.Black },
            { 4, Color.Orange },
            { 5, Color.Cyan },
            { 6, Color.Yellow },
            { 7, Color.Magenta },
            { 8, Color.Purple },
            { 9, Color.Gray },
            { 10, Color.White },
            { 11, Color.Navy },
            { 12, Color.Lavender },
            { 13, Color.Brown },
            { 14, Color.Olive },
            { 15, Color.Pink },
        };

        public enum SrcFactors
        {
            Nothing = 0x0,
            SourceAlpha = 0x1,
            One = 0x2,
            InverseSourceAlpha = 0x3,
            SourceColor = 0x4,
            Zero = 0xA
        }

        public NUD()
        {
            SetupTreeNode();
        }

        public NUD(string fname) : this()
        {
            Read(fname);
        }

        private void SetupTreeNode()
        {
            Text = "model.nud";
            ImageKey = "model";
            SelectedImageKey = "model";
        }

        private void GenerateBuffers()
        {
            positionVbo = new BufferObject(BufferTarget.ArrayBuffer);
            elementsIbo = new BufferObject(BufferTarget.ElementArrayBuffer);
            bonesUbo = new BufferObject(BufferTarget.UniformBuffer);
            selectVbo = new BufferObject(BufferTarget.ArrayBuffer);
        }

        public void CheckTexIdErrors(NUT nut)
        {
            if (!Runtime.checkNudTexIdOnOpen)
                return;

            string incorrectTextureIds = GetTextureIdsWithoutNutOrDummyTex(nut);
            if (incorrectTextureIds.Length > 0)
            {
                MessageBox.Show("The following texture IDs do not match a texture in the NUT or a valid dummy texture:\n" + incorrectTextureIds, "Incorrect Texture IDs");
            }
        }

        private string GetTextureIdsWithoutNutOrDummyTex(NUT nut)
        {
            StringBuilder incorrectTextureIds = new StringBuilder();
            int textureCount = 0;
            const int maxTextureCount = 10;
            foreach (Mesh m in Nodes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    foreach (Material mat in p.materials)
                    {
                        foreach (MatTexture matTex in mat.textures)
                        {
                            bool validTextureId = false;

                            // Checks to see if the texture is in the nut. 
                            foreach (NutTexture nutTex in nut.Nodes)
                            {
                                if (matTex.hash == nutTex.HASHID)
                                {
                                    validTextureId = true;
                                    break;
                                }
                            }

                            // Checks to see if the texture ID is a valid dummy texture.
                            foreach (DummyTextures dummyTex in Enum.GetValues(typeof(DummyTextures)))
                            {
                                if (matTex.hash == (int)dummyTex)
                                {
                                    validTextureId = true;
                                    break;
                                }
                            }

                            // If all the textures are incorrect, the message box will fill the entire screen and can't be resized.
                            if (!validTextureId && (textureCount != maxTextureCount))
                            {
                                incorrectTextureIds.AppendLine(m.Text + " [" + p.Index + "] ID: " + matTex.hash.ToString("X"));
                                textureCount++;
                            }
                        }
                    }
                }
            }

            if (textureCount == maxTextureCount)
                incorrectTextureIds.AppendLine("...");

            return incorrectTextureIds.ToString();
        }

        public void DepthSortMeshes(Vector3 cameraPosition)
        {
            // Meshes can be rendered in the order they appear in the NUD, by bounding spheres, and offsets. 
            List<Mesh> unsortedMeshes = new List<Mesh>();
            foreach (Mesh m in Nodes)
            {
                m.SetMeshAttributesFromName();
                m.sortingDistance = m.CalculateSortingDistance(cameraPosition);
                unsortedMeshes.Add(m);
            }

            // Order by the distance from the camera to the closest point on the bounding sphere. 
            // Positive values are usually closer to camera. Negative values are usually farther away. 
            depthSortedMeshes = unsortedMeshes.OrderBy(o => (o.sortingDistance)).ToList();
        }

        private void SortMeshesByObjHeirarchy()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                Mesh mesh = (Mesh)Nodes[i];
                if (mesh.sortByObjHeirarchy)
                {
                    // Just use the mesh list order.
                    depthSortedMeshes.Remove(mesh);
                    depthSortedMeshes.Insert(i, mesh);
                }
            }
        }

        public void UpdateVertexBuffers(BufferObject positionVbo, BufferObject elementsIbo)
        {
            DisplayVertex[] displayVerticesArray;
            int[] vertexIndicesArray;

            int polygonOffset = 0;
            int vertexOffset = 0;

            // Store all of the polygon vert data in one buffer.
            List<DisplayVertex> displayVerticesList = new List<DisplayVertex>();
            List<int> vertexIndicesList = new List<int>();

            // Loop backwards?
            for (int meshIndex = Nodes.Count - 1; meshIndex >= 0; meshIndex--)
            {
                Mesh m = (Mesh)Nodes[meshIndex];

                for (int polyIndex = m.Nodes.Count - 1; polyIndex >= 0; polyIndex--)
                {
                    Polygon p = (Polygon)m.Nodes[polyIndex];
                    p.Offset = polygonOffset * sizeof(float);

                    List<DisplayVertex> polygonDisplayVertices = p.CreateDisplayVertices();
                    displayVerticesList.AddRange(polygonDisplayVertices);

                    for (int i = 0; i < p.displayFaceSize; i++)
                    {
                        vertexIndicesList.Add(p.display[i] + vertexOffset);
                    }

                    polygonOffset += p.displayFaceSize;
                    vertexOffset += polygonDisplayVertices.Count;
                }
            }

            // Initialize the buffers.
            displayVerticesArray = displayVerticesList.ToArray();
            vertexIndicesArray = vertexIndicesList.ToArray();

            positionVbo.Bind();
            GL.BufferData<DisplayVertex>(positionVbo.BufferTarget, (IntPtr)(displayVerticesArray.Length * DisplayVertex.Size), displayVerticesArray, BufferUsageHint.StaticDraw);

            elementsIbo.Bind();
            GL.BufferData<int>(elementsIbo.BufferTarget, (IntPtr)(vertexIndicesArray.Length * sizeof(int)), vertexIndicesArray, BufferUsageHint.StaticDraw);
        }

        public void UpdateVertexBuffers()
        {
            UpdateVertexBuffers(positionVbo, elementsIbo);
        }

        public void Render(VBN vbn, Camera camera, bool drawShadow = false, bool drawPolyIds = false)
        {
            // Binding 0 to a buffer target will crash. This also means the NUD buffers weren't generated yet.
            bool buffersWereInitialized = elementsIbo != null && positionVbo != null && bonesUbo != null && selectVbo != null;
            if (!buffersWereInitialized)
            {
                GenerateBuffers();
                UpdateVertexBuffers(positionVbo, elementsIbo);
            }

            // Main function for NUD rendering.
            if (Runtime.renderBoundingSphere)
                DrawBoundingSpheres();

            // Choose the correct shader.
            Shader shader;
            if (drawShadow)
                shader = Runtime.shaders["Shadow"];
            else if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                shader = Runtime.shaders["NudDebug"];
            else
                shader = Runtime.shaders["Nud"];

            // Render using the selected shader.
            GL.UseProgram(shader.Id);
            shader.EnableVertexAttributes();
            UpdateBonesBuffer(vbn, shader, bonesUbo);

            DrawAllPolygons(shader, camera, drawPolyIds);

            shader.DisableVertexAttributes();
        }

        private void UpdateBonesBuffer(VBN vbn, Shader shader, BufferObject bonesUbo)
        {
            if (vbn != null)
            {
                Matrix4[] f = vbn.getShaderMatrix();

                int maxUniformBlockSize = GL.GetInteger(GetPName.MaxUniformBlockSize);
                int boneCount = vbn.bones.Count;
                int dataSize = boneCount * Vector4.SizeInBytes * sizeof(float);

                bonesUbo.Bind();
                GL.BufferData(bonesUbo.BufferTarget, (IntPtr)(dataSize), IntPtr.Zero, BufferUsageHint.DynamicDraw);
                GL.BindBuffer(BufferTarget.UniformBuffer, 0);

                var blockIndex = GL.GetUniformBlockIndex(shader.Id, "bones");
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, blockIndex, bonesUbo.Id);

                if (f.Length > 0)
                {
                    bonesUbo.Bind();
                    GL.BufferSubData(bonesUbo.BufferTarget, IntPtr.Zero, (IntPtr)(f.Length * Vector4.SizeInBytes * sizeof(float)), f);
                }

                shader.SetBoolToInt("useBones", true);
            }
            else
            {
                shader.SetBoolToInt("useBones", false);
            }
        }

        private void DrawBoundingSpheres()
        {
            GL.UseProgram(0);

            // Draw NUD bounding box. 
            GL.Color4(Color.GhostWhite);
            RenderTools.DrawCube(new Vector3(boundingSphere[0], boundingSphere[1], boundingSphere[2]), boundingSphere[3], true);

            // Draw all the mesh bounding boxes. Selected: White. Deselected: Orange.
            foreach (Mesh mesh in Nodes)
            {
                if (mesh.IsSelected)
                    GL.Color4(Color.GhostWhite);
                else
                    GL.Color4(Color.OrangeRed);

                if (mesh.Checked)
                {
                    if (mesh.useNsc && mesh.singlebind != -1)
                    {
                        // Use the center of the bone as the bounding box center for NSC meshes. 
                        Vector3 center = ((ModelContainer)Parent).VBN.bones[mesh.singlebind].pos;
                        RenderTools.DrawCube(center, mesh.boundingSphere[3], true);
                    }
                    else
                    {
                        RenderTools.DrawCube(new Vector3(mesh.boundingSphere[0], mesh.boundingSphere[1], mesh.boundingSphere[2]), mesh.boundingSphere[3], true);
                    }
                }
            }
        }

        public void GenerateBoundingSpheres()
        {
            foreach (Mesh m in Nodes)
                m.generateBoundingSphere();

            Vector3 cen1 = new Vector3(0,0,0), cen2 = new Vector3(0,0,0);
            double rad1 = 0, rad2 = 0;

            //Get first vert
            int vertCount = 0;
            Vector3 vert0 = new Vector3();
            foreach (Mesh m in Nodes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    foreach (Vertex v in p.vertices)
                    {
                        vert0 = v.pos;
                        vertCount++;
                        break;
                    }
                    break;
                }
                break;
            }

            if (vertCount == 0) //No vertices
                return;

            //Calculate average and min/max
            Vector3 min = new Vector3(vert0);
            Vector3 max = new Vector3(vert0);

            vertCount = 0;
            foreach (Mesh m in Nodes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    foreach (Vertex v in p.vertices)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            min[i] = Math.Min(min[i], v.pos[i]);
                            max[i] = Math.Max(max[i], v.pos[i]);
                        }

                        cen1 += v.pos;
                        vertCount++;
                    }
                }
            }

            cen1 /= vertCount;
            for (int i = 0; i < 3; i++)
                cen2[i] = (min[i]+max[i])/2;

            //Calculate the radius of each
            double dist1, dist2;
            foreach (Mesh m in Nodes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    foreach (Vertex v in p.vertices)
                    {
                        dist1 = ((Vector3)(v.pos - cen1)).Length;
                        if (dist1 > rad1)
                            rad1 = dist1;

                        dist2 = ((Vector3)(v.pos - cen2)).Length;
                        if (dist2 > rad2)
                            rad2 = dist2;
                    }
                }
            }

            //Use the one with the lowest radius
            Vector3 temp;
            double radius;
            if (rad1 < rad2)
            {
                temp = cen1;
                radius = rad1;
            }
            else
            {
                temp = cen2;
                radius = rad2;
            }

            //Set
            for (int i = 0; i < 3; i++)
            {
                boundingSphere[i] = temp[i];
            }
            boundingSphere[3] = (float)radius;
        }

        public void SetPropertiesFromXMB(XMBFile xmb)
        {
            if (xmb != null)
            {
                foreach (XMBEntry entry in xmb.Entries)
                {
                    if (entry.Name == "model")
                        modelType = xmb.Values[entry.FirstPropertyIndex];

                    if (entry.Children.Count > 0)
                    {
                        foreach (XMBEntry value in entry.Children)
                        {
                            if (xmb.Values.Count >= value.FirstPropertyIndex)
                            {
                                switch (value.Name)
                                {
                                    default:
                                        break;
                                    case "lightset":
                                        int.TryParse(xmb.Values[value.FirstPropertyIndex], out lightSetNumber);
                                        break;
                                    case "directuvtime":
                                        useDirectUVTime = true;
                                        int.TryParse(xmb.Values[value.FirstPropertyIndex], out directUVTime);
                                        break;
                                    case "draw_range":
                                        int.TryParse(xmb.Values[value.FirstPropertyIndex], out drawRange);
                                        break;
                                    case "drawing_order":
                                        int.TryParse(xmb.Values[value.FirstPropertyIndex], out drawingOrder);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawAllPolygons(Shader shader, Camera camera, bool drawPolyIds)
        {
            DrawShadedPolygons(shader, camera, drawPolyIds);
            DrawSelectionOutlines(shader);
        }

        private void DrawSelectionOutlines(Shader shader)
        {
            foreach (Mesh m in Nodes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    if (((Mesh)p.Parent).Checked && p.Checked)
                    {
                        if ((p.IsSelected || p.Parent.IsSelected))
                        {
                            DrawModelSelection(p, shader);
                        }
                    }
                }
            }
        }

        private void DrawShadedPolygons(Shader shader, Camera camera, bool drawPolyIds = false)
        {
            // For proper alpha blending, draw in reverse order and draw opaque objects first. 
            List<Polygon> opaque = new List<Polygon>();
            List<Polygon> transparent = new List<Polygon>();

            foreach (Mesh m in depthSortedMeshes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    if (p.materials.Count > 0 && p.materials[0].srcFactor != 0 || p.materials[0].dstFactor != 0)
                        transparent.Add(p);
                    else
                        opaque.Add(p);
                }
            }

            // Only draw polgons if the polygon and its parent are both checked.
            foreach (Polygon p in opaque)
            {
                if (p.Parent != null && ((Mesh)p.Parent).Checked && p.Checked)
                {
                    DrawPolygonShaded(p, shader, camera, RenderTools.dummyTextures, drawPolyIds);
                }
            }

            foreach (Polygon p in transparent)
            {
                if (((Mesh)p.Parent).Checked && p.Checked)
                {
                    DrawPolygonShaded(p, shader, camera, RenderTools.dummyTextures, drawPolyIds);
                }
            }
        }

        private void DrawPolygonShaded(Polygon p, Shader shader, Camera camera, Dictionary<DummyTextures, Texture> dummyTextures, bool drawId = false)
        {
            if (p.vertexIndices.Count <= 3)
                return;

            Material material = p.materials[0];

            // Set Shader Values.
            SetShaderUniforms(p, shader, camera, material, dummyTextures, p.DisplayId, drawId);
            SetVertexAttributes(shader);

            // Set OpenTK Render Options.
            SetAlphaBlending(material);
            SetAlphaTesting(material);
            SetFaceCulling(material);

            // Draw the model normally.
            elementsIbo.Bind();
            GL.DrawElements(PrimitiveType.Triangles, p.displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);
        }

        private void SetShaderUniforms(Polygon p, Shader shader, Camera camera, Material material, Dictionary<DummyTextures, Texture> dummyTextures, int id = 0, bool drawId = false)
        {
            // Shader Uniforms
            shader.SetUint("flags", material.Flags);
            shader.SetBoolToInt("renderVertColor", Runtime.renderVertColor && material.useVertexColor);
            SetTextureUniforms(shader, material, dummyTextures);
            SetMaterialPropertyUniforms(shader, material);
            SetStageLightingUniforms(shader, lightSetNumber);
            SetXMBUniforms(shader, p);
            SetNscUniform(p, shader);

            // Misc Uniforms
            shader.SetInt("selectedBoneIndex", Runtime.selectedBoneIndex);
            shader.SetBoolToInt("drawWireFrame", Runtime.renderModelWireframe);
            shader.SetFloat("lineWidth", Runtime.wireframeLineWidth);
            shader.SetVector3("cameraPosition", camera.Position);
            shader.SetFloat("zBufferOffset", material.zBufferOffset);
            shader.SetFloat("bloomThreshold", Runtime.bloomThreshold);
            shader.SetVector3("colorId", ColorTools.Vector4FromColor(Color.FromArgb(id)).Xyz);
            shader.SetBoolToInt("drawId", drawId);

            // The fragment alpha is set to 1 when alpha blending/testing aren't used.
            // This fixes the alpha output for PNG renders.
            p.isTransparent = (material.srcFactor > 0) || (material.dstFactor > 0) || (material.alphaFunction > 0) || (material.alphaTest > 0);
            shader.SetBoolToInt("isTransparent", p.isTransparent);
        }

        private void SetAlphaBlending(Material material)
        {
            // Disable alpha blending for debug shading.
            if (Runtime.renderType != Runtime.RenderTypes.Shaded)
            {
                GL.Disable(EnableCap.Blend);
                return;
            }

            //// Src and dst of 0 don't use alpha blending.
            if (material.srcFactor == 0 && material.dstFactor == 0)
            {
                GL.Disable(EnableCap.Blend);
                return;
            }

            // Set the alpha blending based on the material.
            // If the values are not researched, use a default blending mode.
            // Hacks for Game & Watch.
            if (material.srcFactor == 4)
                GL.DepthMask(false);
            else
                GL.DepthMask(true);

            GL.Enable(EnableCap.Blend);

            BlendingFactorSrc blendSrc = BlendingFactorSrc.SrcAlpha;
            if (srcFactorsByMatValue.ContainsKey(material.srcFactor))
                blendSrc = srcFactorsByMatValue[material.srcFactor];
            blendSrc = (BlendingFactorSrc)material.srcFactor;

            BlendingFactorDest blendDst = BlendingFactorDest.OneMinusSrcAlpha;
            if (dstFactorsByMatValue.ContainsKey(material.dstFactor))
                blendDst = dstFactorsByMatValue[material.dstFactor];
            blendDst = (BlendingFactorDest)material.dstFactor;

            // The dstFactor can also set the blending equation.
            BlendEquationMode blendEquation = BlendEquationMode.FuncAdd;
            //if (material.dstFactor == 3)
            //    blendEquation = BlendEquationMode.FuncReverseSubtract;

            GL.BlendFuncSeparate(blendSrc, blendDst, BlendingFactorSrc.One, BlendingFactorDest.One);
            GL.BlendEquationSeparate(blendEquation, BlendEquationMode.FuncAdd);

        }

        private static void SetAlphaTesting(Material material)
        {
            if (Runtime.renderType != Runtime.RenderTypes.Shaded)
            {
                // Disable alpha testing for debug shading.
                GL.Disable(EnableCap.AlphaTest);
                return;
            }

            GL.Enable(EnableCap.AlphaTest);
            if (material.alphaTest == 0)
                GL.Disable(EnableCap.AlphaTest);

            float refAlpha = material.RefAlpha / 255f;
            GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);
            switch (material.alphaFunction)
            {
                case 0x0:
                    GL.AlphaFunc(AlphaFunction.Never, refAlpha);
                    break;
                case 0x4:
                    GL.AlphaFunc(AlphaFunction.Gequal, refAlpha);
                    break;
                case 0x6:
                    GL.AlphaFunc(AlphaFunction.Gequal, refAlpha);
                    break;
            }
        }

        private static void SetFaceCulling(Material material)
        {
            if (Runtime.renderType != Runtime.RenderTypes.Shaded)
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
                return;
            }

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            switch (material.cullMode)
            {
                case 0x0000:
                    GL.Disable(EnableCap.CullFace);
                    break;
                case 0x0404:
                    GL.CullFace(CullFaceMode.Front);
                    break;
                case 0x0405:
                    GL.CullFace(CullFaceMode.Back);
                    break;
                default:
                    GL.Disable(EnableCap.CullFace);
                    break;
            }
        }

        public static void SetStageLightingUniforms(Shader shader, int lightSetNumber)
        {
            for (int i = 0; i < 4; i++)
            {
                SetStageLightUniform(shader, i, lightSetNumber);
            }

            ShaderTools.LightColorVector3Uniform(shader, Runtime.lightSetParam.stageFogSet[lightSetNumber], "stageFogColor");
        }

        private static void SetStageLightUniform(Shader shader, int lightIndex, int lightSetNumber)
        {
            int index = lightIndex + (4 * lightSetNumber);
            DirectionalLight stageLight = Runtime.lightSetParam.stageDiffuseLights[index];

            string uniformBoolName = "renderStageLight" + (lightIndex + 1);
            shader.SetBoolToInt(uniformBoolName, Runtime.lightSetParam.stageDiffuseLights[index].enabled);

            string uniformColorName = "stageLight" + (lightIndex + 1) + "Color";
            ShaderTools.LightColorVector3Uniform(shader, stageLight.diffuseColor, uniformColorName);

            string uniformDirectionName = "stageLight" + (lightIndex + 1) + "Direction";
            shader.SetVector3(uniformDirectionName, stageLight.direction);
        }

        private static void SetNscUniform(Polygon p, Shader shader)
        {
            Matrix4 nscMatrix = Matrix4.Identity;

            // transform object using the bone's transforms
            if (p.Parent != null && p.Parent.Text.Contains("_NSC"))
            {
                int index = ((Mesh)p.Parent).singlebind;
                if (index != -1)
                {
                    // Very hacky
                    nscMatrix = ((ModelContainer)p.Parent.Parent.Parent).VBN.bones[index].transform;
                }
            }

            shader.SetMatrix4x4("nscMatrix", ref nscMatrix);
        }
        
        private void SetXMBUniforms(Shader shader, Polygon p)
        {
            shader.SetBoolToInt("isStage", modelType.Equals("stage"));

            bool directUVTimeFlags = (p.materials[0].Flags & 0x00001900) == 0x00001900; // should probably move elsewhere
            shader.SetBoolToInt("useDirectUVTime", useDirectUVTime && directUVTimeFlags);

            SetLightSetColorUniform(shader);
        }

        private void SetLightSetColorUniform(Shader shader)
        {
            int maxLightSet = 15;
            if (lightSetNumber >= 0 && lightSetNumber <= maxLightSet)
            {
                Color color = lightSetColorByIndex[lightSetNumber];
                shader.SetVector3("lightSetColor", ColorTools.Vector4FromColor(color).Xyz);
            }
        }

        private void SetVertexAttributes(Shader shader)
        {
            positionVbo.Bind();
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vPosition"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 0);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vNormal"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 12);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vTangent"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 24);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vBiTangent"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 36);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vUV"), 2, VertexAttribPointerType.Float, false, DisplayVertex.Size, 48);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vColor"), 4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 56);
            GL.VertexAttribIPointer(shader.GetVertexAttributeUniformLocation("vBone"), 4, VertexAttribIntegerType.Int, DisplayVertex.Size, new IntPtr(72));
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vWeight"), 4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 88);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vUV2"), 2, VertexAttribPointerType.Float, false, DisplayVertex.Size, 104);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vUV3"), 2, VertexAttribPointerType.Float, false, DisplayVertex.Size, 112);
        }

        private static void DrawModelSelection(Polygon p, Shader shader)
        {
            GL.Enable(EnableCap.StencilTest);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            GL.Disable(EnableCap.DepthTest);

            bool[] cwm = new bool[4];
            GL.GetBoolean(GetIndexedPName.ColorWritemask, 4, cwm);
            GL.ColorMask(false, false, false, false);

            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);

            GL.DrawElements(PrimitiveType.Triangles, p.displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);

            GL.ColorMask(cwm[0], cwm[1], cwm[2], cwm[3]);

            GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
            GL.StencilMask(0x00);

            // Override the model color with white in the shader.
            shader.SetInt("drawSelection", 1);

            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.LineWidth(2.0f);
            GL.DrawElements(PrimitiveType.Triangles, p.displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            shader.SetInt("drawSelection", 0);

            GL.StencilMask(0xFF);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
        }

        public static void SetMaterialPropertyUniforms(Shader shader, Material mat)
        {
            // UV samplers
            MatPropertyShaderUniform(shader, mat, "NU_colorSamplerUV",   1, 1, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_colorSampler2UV",  1, 1, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_colorSampler3UV",  1, 1, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_normalSamplerAUV", 1, 1, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_normalSamplerBUV", 1, 1, 0, 0);

            // Diffuse Color
            MatPropertyShaderUniform(shader, mat, "NU_aoMinGain",       0, 0, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_colorGain",       1, 1, 1, 1);
            MatPropertyShaderUniform(shader, mat, "NU_finalColorGain",  1, 1, 1, 1);
            MatPropertyShaderUniform(shader, mat, "NU_finalColorGain2", 1, 1, 1, 1);
            MatPropertyShaderUniform(shader, mat, "NU_finalColorGain3", 1, 1, 1, 1);
            MatPropertyShaderUniform(shader, mat, "NU_colorOffset",     0, 0, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_diffuseColor",    1, 1, 1, 0.5f);
            MatPropertyShaderUniform(shader, mat, "NU_characterColor",  1, 1, 1, 1);

            // Specular
            MatPropertyShaderUniform(shader, mat, "NU_specularColor",     0, 0, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_specularColorGain", 1, 1, 1, 1);
            MatPropertyShaderUniform(shader, mat, "NU_specularParams",    0, 0, 0, 0);

            // Fresnel
            MatPropertyShaderUniform(shader, mat, "NU_fresnelColor",  0, 0, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_fresnelParams", 0, 0, 0, 0);

            // Reflections
            MatPropertyShaderUniform(shader, mat, "NU_reflectionColor",  0, 0, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_reflectionParams", 0, 0, 0, 0);

            // Fog
            MatPropertyShaderUniform(shader, mat, "NU_fogColor",  0, 0, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_fogParams", 0, 1, 0, 0);

            // Soft Lighting
            MatPropertyShaderUniform(shader, mat, "NU_softLightingParams",    0, 0, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_customSoftLightParams", 0, 0, 0, 0);

            // Misc Properties
            MatPropertyShaderUniform(shader, mat, "NU_normalParams",           1, 0, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_zOffset",                0, 0, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_angleFadeParams",        0, 0, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_dualNormalScrollParams", 0, 0, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_alphaBlendParams",       0, 0, 0, 0);

            // Effect Materials
            MatPropertyShaderUniform(shader, mat, "NU_effCombinerColor0", 1, 1, 1, 1);
            MatPropertyShaderUniform(shader, mat, "NU_effCombinerColor1", 1, 1, 1, 1);
            MatPropertyShaderUniform(shader, mat, "NU_effColorGain",      1, 1, 1, 1);
            MatPropertyShaderUniform(shader, mat, "NU_effScaleUV",        1, 1, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_effTransUV",        1, 1, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_effMaxUV",          1, 1, 0, 0);
            MatPropertyShaderUniform(shader, mat, "NU_effUniverseParam",  1, 0, 0, 0);

            // Create some conditionals rather than using different shaders.
            HasMatPropertyShaderUniform(shader, mat, "NU_softLightingParams",     "hasSoftLight");
            HasMatPropertyShaderUniform(shader, mat, "NU_customSoftLightParams",  "hasCustomSoftLight");
            HasMatPropertyShaderUniform(shader, mat, "NU_specularParams",         "hasSpecularParams");
            HasMatPropertyShaderUniform(shader, mat, "NU_dualNormalScrollParams", "hasDualNormal");
            HasMatPropertyShaderUniform(shader, mat, "NU_normalSamplerAUV",       "hasNrmSamplerAUV");
            HasMatPropertyShaderUniform(shader, mat, "NU_normalSamplerBUV",       "hasNrmSamplerBUV");
            HasMatPropertyShaderUniform(shader, mat, "NU_finalColorGain",         "hasFinalColorGain");
            HasMatPropertyShaderUniform(shader, mat, "NU_effUniverseParam",       "hasUniverseParam");
        }

        public static void SetTextureUniforms(Shader shader, Material mat, Dictionary<DummyTextures, Texture> dummyTextures)
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

        public static void SetTextureUniformsNudMatSphere(Shader shader, Material mat, Dictionary<DummyTextures, Texture> dummyTextures)
        {
            SetHasTextureUniforms(shader, mat);
            SetRenderModeTextureUniforms(shader);

            // This is necessary to prevent some models from disappearing. 
            SetTextureUniformsToDefaultTexture(shader, RenderTools.defaultTex.Id);

            // The material shader just uses predefined textures from the Resources folder.
            MatTexture diffuse = new MatTexture((int)DummyTextures.DummyRamp);
            MatTexture cubeMapHigh = new MatTexture((int)DummyTextures.StageMapHigh);

            // The order of the textures in the following section is critical. 
            int textureUnitIndexOffset = 0;
            if (mat.hasDiffuse && textureUnitIndexOffset < mat.textures.Count)
            {
                GL.ActiveTexture(nutTextureUnit + textureUnitIndexOffset);
                GL.BindTexture(TextureTarget.Texture2D, RenderTools.sphereDifTex.Id);
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
                    GL.BindTexture(TextureTarget.Texture2D, RenderTools.sphereNrmMapTex.Id);
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
                    GL.BindTexture(TextureTarget.Texture2D, RenderTools.sphereNrmMapTex.Id);
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

        private static void SetTextureUniformsToDefaultTexture(Shader shader, int texture)
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

        private static void SetRenderModeTextureUniforms(Shader shader)
        {
            shader.SetTexture("UVTestPattern", RenderTools.uvTestPattern.Id, TextureTarget.Texture2D, 10);
            shader.SetTexture("weightRamp1", RenderTools.boneWeightGradient.Id, TextureTarget.Texture2D, 11);
            shader.SetTexture("weightRamp2", RenderTools.boneWeightGradient2.Id, TextureTarget.Texture2D, 12);
        }

        private static void SetHasTextureUniforms(Shader shader, Material mat)
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

        private static void MatPropertyShaderUniform(Shader shader, Material mat, string propertyName, float default1,
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
                shader.SetVector4(uniformName, values[0], values[1], values[2], values[3]);
            else
                Debug.WriteLine(uniformName + " invalid parameter count: " + values.Length);
        }

        private static void SetTextureUniformAndSetTexId(Shader shader, Material mat, bool hasTex, string name, ref int textureIndex, ref int texIdForCurrentTextureType, Dictionary<DummyTextures, Texture> dummyTextures)
        {
            // Bind the texture and create the uniform if the material has the right textures and flags. 
            if (hasTex && textureIndex < mat.textures.Count)
            {
                // Find the index for the shader uniform.
                // Bind the texture to a texture unit and then find where it was bound.
                int uniformLocation = shader.GetVertexAttributeUniformLocation(name);
                int textureUnit = BindTexture(mat.textures[textureIndex], mat.textures[textureIndex].hash, textureIndex, dummyTextures);
                GL.Uniform1(uniformLocation, textureUnit);

                // We won't know what type a texture is used for until we iterate through the textures.
                texIdForCurrentTextureType = mat.textures[textureIndex].hash;

                // Move on to the next texture.
                textureIndex++;
            }
        }

        public void MakeMetal(int newDifTexId, int newCubeTexId, float[] minGain, float[] refColor, float[] fresParams, float[] fresColor, bool preserveDiffuse = false, bool preserveNrmMap = true)
        {
            foreach (Mesh mesh in Nodes)
            {
                foreach (Polygon poly in mesh.Nodes)
                {
                    foreach (Material mat in poly.materials)
                    {
                        mat.MakeMetal(newDifTexId, newCubeTexId, minGain, refColor, fresParams, fresColor, preserveDiffuse, preserveNrmMap);
                    }
                }
            }
        }

        private static void HasMatPropertyShaderUniform(Shader shader, Material mat, string propertyName, string uniformName)
        {
            float[] values;
            mat.entries.TryGetValue(propertyName, out values);
            if (mat.anims.ContainsKey(propertyName))
                values = mat.anims[propertyName];

            int hasParam = 1;
            if (values == null)
                hasParam = 0;

            shader.SetInt(uniformName, hasParam);
        }

        public void DrawPoints(Camera camera, VBN vbn, PrimitiveType type)
        {
            Shader shader = Runtime.shaders["Point"];
            GL.UseProgram(shader.Id);
            Matrix4 mat = camera.MvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref mat);

            if (type == PrimitiveType.Points)
            {
                shader.SetVector3("col1", 0, 0, 1);
                shader.SetVector3("col2", 1, 1, 0);
            }
            if (type == PrimitiveType.Triangles)
            {
                shader.SetVector3("col1", 0.5f, 0.5f, 0.5f);
                shader.SetVector3("col2", 1, 0, 0);
            }

            shader.EnableVertexAttributes();
            foreach (Mesh m in Nodes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    positionVbo.Bind();
                    GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vPosition"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 0);
                    GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vBone"), 4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 72);
                    GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vWeight"), 4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 88);

                    selectVbo.Bind();
                    if (p.selectedVerts == null) return;
                    GL.BufferData<int>(selectVbo.BufferTarget, (IntPtr)(p.selectedVerts.Length * sizeof(int)), p.selectedVerts, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vSelected"), 1, VertexAttribPointerType.Int, false, sizeof(int), 0);

                    elementsIbo.Bind();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    
                    GL.PointSize(6f);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    GL.DrawElements(type, p.displayFaceSize, DrawElementsType.UnsignedInt, 0);
                }
            }
            shader.DisableVertexAttributes();
        }

        public static int BindTexture(MatTexture matTexture, int hash, int loc, Dictionary<DummyTextures, Texture> dummyTextures)
        {
            if (Enum.IsDefined(typeof(DummyTextures), hash))
            {
                return BindDummyTexture(loc, dummyTextures[(DummyTextures)hash]);
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

        private static int BindDummyTexture(int loc, Texture texture)
        {
            GL.ActiveTexture(dummyTextureUnit + loc);
            texture.Bind();
            return dummyTextureUnitOffset + loc;
        }

        private static void BindNutTexture(MatTexture matTexture, Texture texture)
        {
            // Set the texture's parameters based on the material settings.
            texture.Bind();
            texture.TextureWrapS = wrapmode[matTexture.wrapModeS];
            texture.TextureWrapT = wrapmode[matTexture.wrapModeT];
            texture.MinFilter = minfilter[matTexture.minFilter];
            texture.MagFilter = magfilter[matTexture.magFilter];

            // TODO: These aren't controlled by the Texture class yet.
            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, 0.0f);
            if (matTexture.mipDetail == 0x4 || matTexture.mipDetail == 0x6)
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, 4.0f);
        }

        public void ClearMta()
        {
            foreach (Mesh me in Nodes)
            {
                foreach (Polygon p in me.Nodes)
                {
                    foreach (Material ma in p.materials)
                    {
                        ma.anims.Clear();
                    }
                }
            }
        }

        public void ApplyMta(MTA m, int frame)
        {
            foreach (MatEntry matEntry in m.matEntries)
            {
                foreach (Mesh mesh in Nodes)
                {
                    foreach(Polygon polygon in mesh.Nodes)
                    {
                        foreach (Material material in polygon.materials)
                        {
                            float[] matHashFloat;
                            material.entries.TryGetValue("NU_materialHash", out matHashFloat);
                            if (matHashFloat != null)
                            {
                                byte[] bytes = new byte[4];
                                Buffer.BlockCopy(matHashFloat, 0, bytes, 0, 4);
                                int matHash = BitConverter.ToInt32(bytes, 0);

                                int frm = (int)((frame * 60 / m.frameRate) % (m.frameCount));

                                if (matHash == matEntry.matHash || matHash == matEntry.matHash2)
                                {
                                    if (matEntry.hasPat)
                                    {
                                        material.displayTexId = matEntry.pat0.getTexId(frm);
                                    }
                                    foreach (MatData md in matEntry.properties)
                                    {
                                        if (md.frames.Count > 0 && md.frames.Count > frm)
                                        {
                                            if (material.anims.ContainsKey(md.name))
                                                material.anims[md.name] = md.frames[frm].values;
                                            else
                                                material.anims.Add(md.name, md.frames[frm].values);
                                        }                                          
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (VisEntry e in m.visEntries)
            {
                int state = e.getState(frame);
                foreach (Mesh me in Nodes)
                {
                    if (me.Text.Equals(e.name))
                    {
                        if (state == 0)
                        {
                            me.Checked = false;
                        }
                        else
                        {
                            me.Checked = true;
                        }
                        break;
                    }
                }
            }
        }

        // Helpers for reading
        private struct ObjectData
        {
            public int singlebind;
            public int polyCount;
            public int positionb;
            public string name;
        }

        public struct PolyData
        {
            public int polyStart;
            public int vertStart;
            public int verAddStart;
            public int vertCount;
            public int vertSize;
            public int UVSize;
            public int polyCount;
            public int polySize;
            public int polyFlag;
            public int texprop1;
            public int texprop2;
            public int texprop3;
            public int texprop4;
        }

        public override void Read(string filename)
        {
            FileData fileData = new FileData(filename);
            fileData.Endian = Endianness.Big;
            fileData.seek(0);

            // read header
            string magic = fileData.readString(0, 4);

            if (magic.Equals("NDWD"))
                fileData.Endian = Endianness.Little;

            Endian = fileData.Endian;

            fileData.seek(0xA);
            int polysets = fileData.readUShort();
            boneCount = fileData.readUShort();
            fileData.skip(2);  // somethingsets
            int polyClumpStart = fileData.readInt() + 0x30;
            int polyClumpSize = fileData.readInt();
            int vertClumpStart = polyClumpStart + polyClumpSize;
            int vertClumpSize = fileData.readInt();
            int vertaddClumpStart = vertClumpStart + vertClumpSize;
            int vertaddClumpSize = fileData.readInt();
            int nameStart = vertaddClumpStart + vertaddClumpSize;
            boundingSphere[0] = fileData.readFloat();
            boundingSphere[1] = fileData.readFloat();
            boundingSphere[2] = fileData.readFloat();
            boundingSphere[3] = fileData.readFloat();

            // object descriptors

            ObjectData[] obj = new ObjectData[polysets];
            List<float[]> boundingSpheres = new List<float[]>();
            int[] boneflags = new int[polysets];
            for (int i = 0; i < polysets; i++)
            {
                float[] boundingSphere = new float[8];
                boundingSphere[0] = fileData.readFloat();
                boundingSphere[1] = fileData.readFloat();
                boundingSphere[2] = fileData.readFloat();
                boundingSphere[3] = fileData.readFloat();
                boundingSphere[4] = fileData.readFloat();
                boundingSphere[5] = fileData.readFloat();
                boundingSphere[6] = fileData.readFloat();
                boundingSphere[7] = fileData.readFloat();
                boundingSpheres.Add(boundingSphere);
                int temp = fileData.pos() + 4;
                fileData.seek(nameStart + fileData.readInt());
                obj[i].name = (fileData.readString());
                // read name string
                fileData.seek(temp);
                boneflags[i] = fileData.readInt();
                obj[i].singlebind = fileData.readShort();
                obj[i].polyCount = fileData.readUShort();
                obj[i].positionb = fileData.readInt();
            }

            // reading polygon data
            int meshIndex = 0;
            foreach (var o in obj)
            {
                Mesh m = new Mesh();
                m.Text = o.name;
                Nodes.Add(m);
                m.boneflag = boneflags[meshIndex];
                m.singlebind = (short)o.singlebind;
                m.boundingSphere = boundingSpheres[meshIndex++];

                for (int i = 0; i < o.polyCount; i++)
                {
                    PolyData polyData = new PolyData();

                    polyData.polyStart = fileData.readInt() + polyClumpStart;
                    polyData.vertStart = fileData.readInt() + vertClumpStart;
                    polyData.verAddStart = fileData.readInt() + vertaddClumpStart;
                    polyData.vertCount = fileData.readUShort();
                    polyData.vertSize = fileData.readByte();
                    polyData.UVSize = fileData.readByte();
                    polyData.texprop1 = fileData.readInt();
                    polyData.texprop2 = fileData.readInt();
                    polyData.texprop3 = fileData.readInt();
                    polyData.texprop4 = fileData.readInt();
                    polyData.polyCount = fileData.readUShort();
                    polyData.polySize = fileData.readByte();
                    polyData.polyFlag = fileData.readByte();
                    fileData.skip(0xC);

                    int temp = fileData.pos();

                    // read vertex
                    Polygon poly = ReadVertex(fileData, polyData, o);
                    m.Nodes.Add(poly);

                    poly.materials = ReadMaterials(fileData, polyData, nameStart);

                    fileData.seek(temp);
                }
            }
        }

        public static List<Material> ReadMaterials(FileData d, PolyData p, int nameOffset)
        {
            int propoff = p.texprop1;
            List<Material> mats = new List<Material>();

            while (propoff != 0)
            {
                d.seek(propoff);

                Material m = new Material();
                mats.Add(m);

                m.Flags = (uint)d.readInt();
                d.skip(4);
                m.srcFactor = d.readUShort();
                ushort texCount = d.readUShort();
                m.dstFactor = d.readUShort();
                m.alphaTest = d.readByte();
                m.alphaFunction = d.readByte();

                d.skip(1); // unknown
                m.RefAlpha = d.readByte();
                m.cullMode = d.readUShort();
                d.skip(4); // padding
                m.unkownWater = d.readInt();
                m.zBufferOffset = d.readInt();

                for (ushort i = 0; i < texCount; i++)
                {
                    MatTexture tex = new MatTexture();
                    tex.hash = d.readInt();
                    d.skip(6); // padding?
                    tex.mapMode = d.readUShort();
                    tex.wrapModeS = d.readByte();
                    tex.wrapModeT = d.readByte();
                    tex.minFilter = d.readByte();
                    tex.magFilter = d.readByte();
                    tex.mipDetail = d.readByte();
                    tex.unknown = d.readByte();
                    d.skip(4); // padding?
                    tex.unknown2 = d.readShort();
                    m.textures.Add(tex);
                }

                int head = 0x20;

                if(d.Endian != Endianness.Little)
                while (head != 0)
                {
                    head = d.readInt();
                    int nameStart = d.readInt();

                    string name = d.readString(nameOffset + nameStart, -1);

                    int pos = d.pos();
                    int valueCount = d.readInt();
                    d.skip(4);

                    // Material properties should always have 4 values. Use 0 for remaining values.
                    float[] values = new float[4];
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (i < valueCount)
                            values[i] = d.readFloat();
                        else
                            values[i] = 0;
                    }
                    m.entries.Add(name, values);

                    d.seek(pos);

                    if (head == 0)
                        d.skip(0x20 - 8);
                    else
                        d.skip(head - 8);
                }

                if (propoff == p.texprop1)
                    propoff = p.texprop2;
                else if (propoff == p.texprop2)
                        propoff = p.texprop3;
                else if (propoff == p.texprop3)
                    propoff = p.texprop4;
            }

            return mats;
        }

        private static Polygon ReadVertex(FileData d, PolyData p, ObjectData o)
        {
            Polygon m = new Polygon();
            m.vertSize = p.vertSize;
            m.UVSize = p.UVSize;
            m.polflag = p.polyFlag;
            m.strip = p.polySize;

            ReadVertex(d, p, o, m);

            // faces
            d.seek(p.polyStart);

            for (int x = 0; x < p.polyCount; x++)
            {
                m.vertexIndices.Add(d.readUShort());
            }

            return m;
        }

        private static void ReadUV(FileData d, PolyData p, ObjectData o, Polygon m, Vertex[] v)
        {
            int uvCount = (p.UVSize >> 4);
            int uvType = (p.UVSize) & 0xF;

            for (int i = 0; i < p.vertCount; i++)
            {
                v[i] = new Vertex();
                if (uvType == 0x0)
                {
                    for (int j = 0; j < uvCount; j++)
                        v[i].uv.Add(new Vector2(d.readHalfFloat(), d.readHalfFloat()));
                }
                else if (uvType == 0x2)
                {
                        v[i].color = new Vector4(d.readByte(), d.readByte(), d.readByte(), d.readByte());
                        for (int j = 0; j < uvCount; j++)
                            v[i].uv.Add(new Vector2(d.readHalfFloat(), d.readHalfFloat()));
                }
                else if (uvType == 0x4)
                {
                    v[i].color = new Vector4(d.readHalfFloat() * 0xFF, d.readHalfFloat() * 0xFF, d.readHalfFloat() * 0xFF, d.readHalfFloat() * 0xFF);
                    for (int j = 0; j < uvCount; j++)
                        v[i].uv.Add(new Vector2(d.readHalfFloat(), d.readHalfFloat()));
                }
                else
                    throw new NotImplementedException("UV type not supported " + uvType);
            }
        }

        private static void ReadVertex(FileData d, PolyData p, ObjectData o, Polygon m)
        {
            int boneType = p.vertSize & 0xF0;
            int vertexType = p.vertSize & 0xF;

            Vertex[] vertices = new Vertex[p.vertCount];

            d.seek(p.vertStart);

            if (boneType > 0)
            {
                ReadUV(d, p, o, m, vertices);
                d.seek(p.verAddStart);
            }
            else
            {
                for (int i = 0; i < p.vertCount; i++)
                {
                    vertices[i] = new Vertex();
                }
            }

            foreach (Vertex v in vertices)
            {
                v.pos.X = d.readFloat();
                v.pos.Y = d.readFloat();
                v.pos.Z = d.readFloat();

                if (vertexType == (int)Polygon.VertexTypes.NoNormals)
                {
                    d.readFloat();
                }
                else if (vertexType == (int)Polygon.VertexTypes.NormalsFloat)
                {
                    v.nrm.X = d.readFloat();
                    v.nrm.Y = d.readFloat();
                    v.nrm.Z = d.readFloat();
                    d.skip(4); // n1?
                    d.skip(4); // r1?
                }
                else if (vertexType == 2)
                {
                    v.nrm.X = d.readFloat();
                    v.nrm.Y = d.readFloat();
                    v.nrm.Z = d.readFloat();
                    d.skip(4); // n1?
                    d.skip(12); // r1?
                    d.skip(12); // r1?
                    d.skip(12); // r1?
                }
                else if (vertexType == (int)Polygon.VertexTypes.NormalsTanBiTanFloat)
                {
                    d.skip(4);
                    v.nrm.X = d.readFloat();
                    v.nrm.Y = d.readFloat();
                    v.nrm.Z = d.readFloat();
                    d.skip(4);
                    v.bitan.X = d.readFloat();
                    v.bitan.Y = d.readFloat();
                    v.bitan.Z = d.readFloat();
                    v.bitan.W = d.readFloat();
                    v.tan.X = d.readFloat();
                    v.tan.Y = d.readFloat();
                    v.tan.Z = d.readFloat();
                    v.tan.W = d.readFloat();
                }
                else if (vertexType == (int)Polygon.VertexTypes.NormalsHalfFloat)
                {
                    v.nrm.X = d.readHalfFloat();
                    v.nrm.Y = d.readHalfFloat();
                    v.nrm.Z = d.readHalfFloat();
                    d.skip(2); // n1?
                }
                else if (vertexType == (int)Polygon.VertexTypes.NormalsTanBiTanHalfFloat)
                {
                    v.nrm.X = d.readHalfFloat();
                    v.nrm.Y = d.readHalfFloat();
                    v.nrm.Z = d.readHalfFloat();
                    d.skip(2); // n1?
                    v.bitan.X = d.readHalfFloat();
                    v.bitan.Y = d.readHalfFloat();
                    v.bitan.Z = d.readHalfFloat();
                    v.bitan.W = d.readHalfFloat();
                    v.tan.X = d.readHalfFloat();
                    v.tan.Y = d.readHalfFloat();
                    v.tan.Z = d.readHalfFloat();
                    v.tan.W = d.readHalfFloat();
                }
                else
                {
                    throw new Exception($"Unsupported vertex type: {vertexType}");
                }

                if (boneType == (int)Polygon.BoneTypes.NoBones)
                {
                    if (p.UVSize >= 18)
                    {
                        v.color.X = d.readByte();
                        v.color.Y = d.readByte();
                        v.color.Z = d.readByte();
                        v.color.W = d.readByte();
                    }

                    int uvChannelCount = p.UVSize >> 4;
                    for (int i = 0; i < uvChannelCount; i++)
                        v.uv.Add(new Vector2(d.readHalfFloat(), d.readHalfFloat()));
                }

                if (boneType == (int)Polygon.BoneTypes.Float)
                {
                    v.boneIds.Add(d.readInt());
                    v.boneIds.Add(d.readInt());
                    v.boneIds.Add(d.readInt());
                    v.boneIds.Add(d.readInt());
                    v.boneWeights.Add(d.readFloat());
                    v.boneWeights.Add(d.readFloat());
                    v.boneWeights.Add(d.readFloat());
                    v.boneWeights.Add(d.readFloat());
                }
                else if (boneType == (int)Polygon.BoneTypes.HalfFloat)
                {
                    v.boneIds.Add(d.readUShort());
                    v.boneIds.Add(d.readUShort());
                    v.boneIds.Add(d.readUShort());
                    v.boneIds.Add(d.readUShort());
                    v.boneWeights.Add(d.readHalfFloat());
                    v.boneWeights.Add(d.readHalfFloat());
                    v.boneWeights.Add(d.readHalfFloat());
                    v.boneWeights.Add(d.readHalfFloat());
                }
                else if (boneType == (int)Polygon.BoneTypes.Byte)
                {
                    v.boneIds.Add(d.readByte());
                    v.boneIds.Add(d.readByte());
                    v.boneIds.Add(d.readByte());
                    v.boneIds.Add(d.readByte());
                    v.boneWeights.Add((float)d.readByte() / 255);
                    v.boneWeights.Add((float)d.readByte() / 255);
                    v.boneWeights.Add((float)d.readByte() / 255);
                    v.boneWeights.Add((float)d.readByte() / 255);
                }
                else if (boneType == (int)Polygon.BoneTypes.NoBones)
                {
                    v.boneIds.Add((short)o.singlebind);
                    v.boneWeights.Add(1);
                }
                else
                {
                    throw new Exception($"Unsupported bone type: {boneType}");
                }
            }

            foreach (Vertex vi in vertices)
                m.vertices.Add(vi);
        }

        public override byte[] Rebuild()
        {
            FileOutput d = new FileOutput(); // data
            d.Endian = Endianness.Big;

            d.writeString("NDP3");
            d.writeInt(0); //FileSize
            d.writeShort(0x200); //  version num
            d.writeShort(Nodes.Count); // polysets

            boneCount = ((ModelContainer)Parent).VBN == null ? 0 : ((ModelContainer)Parent).VBN.bones.Count;

            d.writeShort(boneCount == 0 ? 0 : 2); // type
            d.writeShort(boneCount == 0 ? boneCount : boneCount - 1); // Number of bones

            d.writeInt(0); // polyClump start
            d.writeInt(0); // polyClump size
            d.writeInt(0); // vertexClumpsize
            d.writeInt(0); // vertexaddclump size

            d.writeFloat(boundingSphere[0]);
            d.writeFloat(boundingSphere[1]);
            d.writeFloat(boundingSphere[2]);
            d.writeFloat(boundingSphere[3]);

            // other sections....
            FileOutput obj = new FileOutput();
            obj.Endian = Endianness.Big;
            FileOutput tex = new FileOutput();
            tex.Endian = Endianness.Big;

            FileOutput poly = new FileOutput();
            poly.Endian = Endianness.Big;
            FileOutput vert = new FileOutput();
            vert.Endian = Endianness.Big;
            FileOutput vertadd = new FileOutput();
            vertadd.Endian = Endianness.Big;

            FileOutput str = new FileOutput();
            str.Endian = Endianness.Big;

            // obj descriptor
            FileOutput tempstring = new FileOutput();
            for (int i = 0; i < Nodes.Count; i++)
            {
                str.writeString(Nodes[i].Text);
                str.writeByte(0);
                str.align(16);
            }

            int polyCount = 0; // counting number of poly
            foreach (Mesh m in Nodes)
                polyCount += m.Nodes.Count;

            foreach (Mesh m in Nodes)
            {
                foreach (float f in m.boundingSphere)
                    d.writeFloat(f);

                d.writeInt(tempstring.size());

                tempstring.writeString(m.Text);
                tempstring.writeByte(0);
                tempstring.align(16);

                d.writeInt(m.boneflag); // ID
                d.writeShort(m.singlebind); // Single Bind 
                d.writeShort(m.Nodes.Count); // poly count
                d.writeInt(obj.size() + 0x30 + Nodes.Count * 0x30); // position start for obj

                // write obj info here...
                foreach (Polygon p in m.Nodes)
                {
                    obj.writeInt(poly.size());
                    obj.writeInt(vert.size());
                    obj.writeInt(p.vertSize >> 4 > 0 ? vertadd.size() : 0);
                    obj.writeShort(p.vertices.Count);
                    obj.writeByte(p.vertSize);

                    int maxUV = p.vertices[0].uv.Count;

                    obj.writeByte((maxUV << 4) | (p.UVSize & 0xF));

                    // MATERIAL SECTION 
                    int[] texoff = WriteMaterial(tex, p.materials, str);

                    obj.writeInt(texoff[0] + 0x30 + Nodes.Count * 0x30 + polyCount * 0x30);
                    obj.writeInt(texoff[1] > 0 ? texoff[1] + 0x30 + Nodes.Count * 0x30 + polyCount * 0x30 : 0);
                    obj.writeInt(texoff[2] > 0 ? texoff[2] + 0x30 + Nodes.Count * 0x30 + polyCount * 0x30 : 0);
                    obj.writeInt(texoff[3] > 0 ? texoff[3] + 0x30 + Nodes.Count * 0x30 + polyCount * 0x30 : 0);

                    obj.writeShort(p.vertexIndices.Count); // polyamt
                    obj.writeByte(p.strip); // polysize 0x04 is strips and 0x40 is easy
                    // :D
                    obj.writeByte(p.polflag); // polyflag

                    obj.writeInt(0); // idk, nothing padding??
                    obj.writeInt(0);
                    obj.writeInt(0);

                    // Write the poly...
                    foreach (int face in p.vertexIndices)
                        poly.writeShort(face);

                    // Write the vertex....
                    WriteVertex(vert, vertadd, p);
                    vertadd.align(4, 0x0);
                }
            }

            d.writeOutput(obj);
            d.writeOutput(tex);
            d.align(16);

            d.writeIntAt(d.size() - 0x30, 0x10);
            d.writeIntAt(poly.size(), 0x14);
            d.writeIntAt(vert.size(), 0x18);
            d.writeIntAt(vertadd.size(), 0x1c);

            d.writeOutput(poly);

            int s = d.size();
            d.align(16);
            s = d.size() - s;
            d.writeIntAt(poly.size() + s, 0x14);

            d.writeOutput(vert);

            s = d.size();
            d.align(16);
            s = d.size() - s;
            d.writeIntAt(vert.size() + s, 0x18);

            d.writeOutput(vertadd);

            s = d.size();
            d.align(16);
            s = d.size() - s;
            d.writeIntAt(vertadd.size() + s, 0x1c);

            d.writeOutput(str);

            d.writeIntAt(d.size(), 0x4);

            return d.getBytes();
        }

        private static void WriteUV(FileOutput d, Polygon poly)
        {
            int uvType = (poly.UVSize) & 0xF;

            for (int i = 0; i < poly.vertices.Count; i++)
            {

                if (uvType == 0x0)
                {
                    for (int j = 0; j < poly.vertices[i].uv.Count; j++)
                    {
                        d.writeHalfFloat(poly.vertices[i].uv[j].X);
                        d.writeHalfFloat(poly.vertices[i].uv[j].Y);
                    }
                }
                else if (uvType == 0x2)
                {
                    d.writeByte((int)poly.vertices[i].color.X);
                    d.writeByte((int)poly.vertices[i].color.Y);
                    d.writeByte((int)poly.vertices[i].color.Z);
                    d.writeByte((int)poly.vertices[i].color.W);
                    for (int j = 0; j < poly.vertices[i].uv.Count; j++)
                    {
                        d.writeHalfFloat(poly.vertices[i].uv[j].X);
                        d.writeHalfFloat(poly.vertices[i].uv[j].Y);
                    }
                }
                else if (uvType == 0x4)
                {
                    d.writeHalfFloat(poly.vertices[i].color.X / 0xFF);
                    d.writeHalfFloat(poly.vertices[i].color.Y / 0xFF);
                    d.writeHalfFloat(poly.vertices[i].color.Z / 0xFF);
                    d.writeHalfFloat(poly.vertices[i].color.W / 0xFF);
                    for (int j = 0; j < poly.vertices[i].uv.Count; j++)
                    {
                        d.writeHalfFloat(poly.vertices[i].uv[j].X);
                        d.writeHalfFloat(poly.vertices[i].uv[j].Y);
                    }
                }
                else
                    throw new NotImplementedException("Unsupported UV format");
            }
        }

        private static void WriteVertex(FileOutput d, FileOutput add, Polygon poly)
        {
            int boneType = poly.vertSize & 0xF0;
            int vertexType = poly.vertSize & 0xF;

            if (boneType > 0)
            {
                WriteUV(d, poly);
                d = add;
            }

            foreach (Vertex v in poly.vertices)
            {
                d.writeFloat(v.pos.X);
                d.writeFloat(v.pos.Y);
                d.writeFloat(v.pos.Z);

                if (vertexType == (int)Polygon.VertexTypes.NoNormals)
                {
                    d.writeFloat(1);
                }
                else if (vertexType == (int)Polygon.VertexTypes.NormalsFloat)
                {
                    d.writeFloat(v.nrm.X);
                    d.writeFloat(v.nrm.Y);
                    d.writeFloat(v.nrm.Z);
                    d.writeFloat(1);
                    d.writeFloat(1);
                }
                else if (vertexType == 2)
                {
                    d.writeFloat(v.nrm.X);
                    d.writeFloat(v.nrm.Y);
                    d.writeFloat(v.nrm.Z);
                    d.writeFloat(1);
                    d.writeFloat(v.bitan.X);
                    d.writeFloat(v.bitan.Y);
                    d.writeFloat(v.bitan.Z);
                    d.writeFloat(1);
                    d.writeFloat(v.tan.X);
                    d.writeFloat(v.tan.Y);
                    d.writeFloat(v.tan.Z);
                    d.writeFloat(1);
                    d.writeFloat(1);
                }
                else if (vertexType == (int)Polygon.VertexTypes.NormalsTanBiTanFloat)
                {
                    d.writeFloat(1);
                    d.writeFloat(v.nrm.X);
                    d.writeFloat(v.nrm.Y);
                    d.writeFloat(v.nrm.Z);
                    d.writeFloat(1);
                    d.writeFloat(v.bitan.X);
                    d.writeFloat(v.bitan.Y);
                    d.writeFloat(v.bitan.Z);
                    d.writeFloat(v.bitan.W);
                    d.writeFloat(v.tan.X);
                    d.writeFloat(v.tan.Y);
                    d.writeFloat(v.tan.Z);
                    d.writeFloat(v.tan.W);
                }
                else if (vertexType == (int)Polygon.VertexTypes.NormalsHalfFloat)
                {
                    d.writeHalfFloat(v.nrm.X);
                    d.writeHalfFloat(v.nrm.Y);
                    d.writeHalfFloat(v.nrm.Z);
                    d.writeHalfFloat(1);
                }
                else if (vertexType == (int)Polygon.VertexTypes.NormalsTanBiTanHalfFloat)
                {
                    d.writeHalfFloat(v.nrm.X);
                    d.writeHalfFloat(v.nrm.Y);
                    d.writeHalfFloat(v.nrm.Z);
                    d.writeHalfFloat(1);
                    d.writeHalfFloat(v.bitan.X);
                    d.writeHalfFloat(v.bitan.Y);
                    d.writeHalfFloat(v.bitan.Z);
                    d.writeHalfFloat(v.bitan.W);
                    d.writeHalfFloat(v.tan.X);
                    d.writeHalfFloat(v.tan.Y);
                    d.writeHalfFloat(v.tan.Z);
                    d.writeHalfFloat(v.tan.W);
                }
                else
                {
                    throw new Exception($"Unsupported vertex type: {vertexType}");
                }

                if (boneType == (int)Polygon.BoneTypes.NoBones)
                {
                    if (poly.UVSize >= 18)
                    {
                        d.writeByte((int)v.color.X);
                        d.writeByte((int)v.color.Y);
                        d.writeByte((int)v.color.Z);
                        d.writeByte((int)v.color.W);
                    }

                    for (int j = 0; j < v.uv.Count; j++)
                    {
                        d.writeHalfFloat(v.uv[j].X);
                        d.writeHalfFloat(v.uv[j].Y);
                    }
                }

                if (boneType == (int)Polygon.BoneTypes.Float)
                {
                    d.writeInt(v.boneIds.Count > 0 ? v.boneIds[0] : 0);
                    d.writeInt(v.boneIds.Count > 1 ? v.boneIds[1] : 0);
                    d.writeInt(v.boneIds.Count > 2 ? v.boneIds[2] : 0);
                    d.writeInt(v.boneIds.Count > 3 ? v.boneIds[3] : 0);
                    d.writeFloat(v.boneWeights.Count > 0 ? v.boneWeights[0] : 0);
                    d.writeFloat(v.boneWeights.Count > 1 ? v.boneWeights[1] : 0);
                    d.writeFloat(v.boneWeights.Count > 2 ? v.boneWeights[2] : 0);
                    d.writeFloat(v.boneWeights.Count > 3 ? v.boneWeights[3] : 0);
                }
                else if (boneType == (int)Polygon.BoneTypes.HalfFloat)
                {
                    d.writeShort(v.boneIds.Count > 0 ? v.boneIds[0] : 0);
                    d.writeShort(v.boneIds.Count > 1 ? v.boneIds[1] : 0);
                    d.writeShort(v.boneIds.Count > 2 ? v.boneIds[2] : 0);
                    d.writeShort(v.boneIds.Count > 3 ? v.boneIds[3] : 0);
                    d.writeHalfFloat(v.boneWeights.Count > 0 ? v.boneWeights[0] : 0);
                    d.writeHalfFloat(v.boneWeights.Count > 1 ? v.boneWeights[1] : 0);
                    d.writeHalfFloat(v.boneWeights.Count > 2 ? v.boneWeights[2] : 0);
                    d.writeHalfFloat(v.boneWeights.Count > 3 ? v.boneWeights[3] : 0);
                }
                else if (boneType == (int)Polygon.BoneTypes.Byte)
                {
                    d.writeByte(v.boneIds.Count > 0 ? v.boneIds[0] : 0);
                    d.writeByte(v.boneIds.Count > 1 ? v.boneIds[1] : 0);
                    d.writeByte(v.boneIds.Count > 2 ? v.boneIds[2] : 0);
                    d.writeByte(v.boneIds.Count > 3 ? v.boneIds[3] : 0);
                    d.writeByte((int)(v.boneWeights.Count > 0 ? Math.Round(v.boneWeights[0] * 0xFF) : 0));
                    d.writeByte((int)(v.boneWeights.Count > 1 ? Math.Round(v.boneWeights[1] * 0xFF) : 0));
                    d.writeByte((int)(v.boneWeights.Count > 2 ? Math.Round(v.boneWeights[2] * 0xFF) : 0));
                    d.writeByte((int)(v.boneWeights.Count > 3 ? Math.Round(v.boneWeights[3] * 0xFF) : 0));
                }
                else if (boneType == (int)Polygon.BoneTypes.NoBones)
                {
                }
                else
                {
                    throw new Exception($"Unsupported bone type: {boneType}");
                }
            }
        }

        public static int[] WriteMaterial(FileOutput d, List<Material> materials, FileOutput str)
        {
            int[] offs = new int[4];
            int c = 0;
            foreach (Material mat in materials)
            {
                offs[c++] = d.size();
                d.writeInt((int)mat.Flags);
                d.writeInt(0); // padding
                d.writeShort(mat.srcFactor);
                d.writeShort(mat.textures.Count);
                d.writeShort(mat.dstFactor);
                d.writeByte(mat.alphaTest);
                d.writeByte(mat.alphaFunction);
                d.writeByte(0); // unknown padding?
                d.writeByte(mat.RefAlpha);
                d.writeShort(mat.cullMode);
                d.writeInt(0); // padding
                d.writeInt(mat.unkownWater);
                d.writeInt(mat.zBufferOffset);

                foreach (MatTexture tex in mat.textures)
                {
                    d.writeInt(tex.hash);
                    d.writeInt(0);
                    d.writeShort(0);
                    d.writeShort(tex.mapMode);
                    d.writeByte(tex.wrapModeS);
                    d.writeByte(tex.wrapModeT);
                    d.writeByte(tex.minFilter);
                    d.writeByte(tex.magFilter);
                    d.writeByte(tex.mipDetail);
                    d.writeByte(tex.unknown);
                    d.writeInt(0); // padding
                    d.writeShort(tex.unknown2);
                }

                for (int i = 0; i < mat.entries.Count; i++)
                {
                    float[] data;
                    mat.entries.TryGetValue(mat.entries.ElementAt(i).Key, out data);
                    d.writeInt(i == mat.entries.Count - 1 ? 0 : 16 + 4 * data.Length);
                    d.writeInt(str.size());

                    str.writeString(mat.entries.ElementAt(i).Key);
                    str.writeByte(0);
                    str.align(16);

                    d.writeInt(data.Length);
                    d.writeInt(0);
                    foreach (float f in data)
                        d.writeFloat(f);
                }
            }
            return offs;
        }

        public void MergePoly()
        {
            Dictionary<string, Mesh> nmesh = new Dictionary<string, Mesh>();
            foreach(Mesh m in Nodes)
            {
                if (nmesh.ContainsKey(m.Text))
                {
                    // merge poly
                    List<Polygon> torem = new List<Polygon>();
                    foreach(Polygon p in m.Nodes)
                        torem.Add(p);

                    foreach (Polygon p in torem)
                    {
                        m.Nodes.Remove(p);
                        nmesh[m.Text].Nodes.Add(p);
                    }
                }
                else
                {
                    nmesh.Add(m.Text, m);
                }
            }
            // consolidate
            Nodes.Clear();
            foreach (string n in nmesh.Keys)
            {
                Nodes.Add(nmesh[n]);
            }
        }

        public void ChangeTextureIds(int newTexId)
        {
            foreach (Mesh mesh in Nodes)
            {
                foreach (Polygon polygon in mesh.Nodes)
                {
                    foreach (Material material in polygon.materials)
                    {
                        foreach (MatTexture matTexture in material.textures)
                        {
                            // Don't change dummy texture IDs.
                            if (Enum.IsDefined(typeof(DummyTextures), matTexture.hash))
                                continue;

                            // Only change the first 3 bytes.
                            matTexture.hash = matTexture.hash & 0xFF;
                            int first3Bytes = (int)(newTexId & 0xFFFFFF00);
                            matTexture.hash = matTexture.hash | first3Bytes;
                        }
                    }
                }
            }
        }

        public int GetFirstTexId()
        {
            foreach (Mesh m in Nodes)
            {
                foreach (Polygon poly in m.Nodes)
                {
                    foreach (Material mat in poly.materials)
                    {
                        foreach (MatTexture matTex in mat.textures)
                        {
                            return matTex.hash;
                        }
                    }
                }
            }

            return 0;
        }

        #region ClassStructure

        public struct DisplayVertex
        {
            // Used for rendering.
            public Vector3 pos;
            public Vector3 nrm;
            public Vector3 tan;
            public Vector3 bit;
            public Vector2 uv;
            public Vector4 col;
            public Vector4 boneIds;
            public Vector4 weight;
            public Vector2 uv2;
            public Vector2 uv3;

            public static int Size = 4 * (3 + 3 + 3 + 3 + 2 + 4 + 4 + 4 + 2 + 2);
        }

        public class Vertex
        {
            public Vector3 pos = new Vector3(0, 0, 0), nrm = new Vector3(0, 0, 0);
            public Vector4 bitan = new Vector4(0, 0, 0, 1);
            public Vector4 tan = new Vector4(0, 0, 0, 1);
            public Vector4 color = new Vector4(127, 127, 127, 127);
            public List<Vector2> uv = new List<Vector2>();
            public List<int> boneIds = new List<int>();
            public List<float> boneWeights = new List<float>();

            public Vertex()
            {
            }

            public Vertex(float x, float y, float z)
            {
                pos = new Vector3(x, y, z);
            }

            public bool Equals(Vertex p)
            {
                return pos.Equals(p.pos) && nrm.Equals(p.nrm) && new HashSet<Vector2>(uv).SetEquals(p.uv) && color.Equals(p.color)
                    && new HashSet<int>(boneIds).SetEquals(p.boneIds) && new HashSet<float>(boneWeights).SetEquals(p.boneWeights);
            }

            public override string ToString()
            {
                return pos.ToString();
            }
        }

        public class MatTexture
        {
            public int hash;
            public int mapMode = 0;
            public int wrapModeS = 1;
            public int wrapModeT = 1;
            public int minFilter = 3;
            public int magFilter = 2;
            public int mipDetail = 6;
            public int unknown = 0;
            public short unknown2 = 0;

            public MatTexture()
            {

            }

            public MatTexture(int hash)
            {
                this.hash = hash;
            }

            public MatTexture Clone()
            {
                MatTexture t = new MatTexture();
                t.hash = hash;
                t.mapMode = mapMode;
                t.wrapModeS = wrapModeS;
                t.wrapModeT = wrapModeT;
                t.minFilter = minFilter;
                t.magFilter = magFilter;
                t.mipDetail = mipDetail;
                t.unknown = unknown;
                t.unknown2 = unknown2;
                return t;
            }

            public static MatTexture GetDefault()
            {
                MatTexture defaultTex = new MatTexture((int)DummyTextures.DummyRamp);
                return defaultTex;
            }
        }

        public class Material
        {
            public enum AlphaFunction
            {
                Never = 0x0,
                GequalRefAlpha1 = 0x4,
                GequalRefAlpha2 = 0x6
            }

            public Dictionary<string, float[]> entries = new Dictionary<string, float[]>();
            public Dictionary<string, float[]> anims = new Dictionary<string, float[]>();
            public List<MatTexture> textures = new List<MatTexture>();

            private uint flag;
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

            private void UpdateLabelledTextureIds()
            {
                int textureIndex = 0;
                if ((flag & 0xFFFFFFFF) == 0x9AE11163)
                {
                    UpdateLabelledId(hasDiffuse,   ref diffuse1ID, ref textureIndex);
                    UpdateLabelledId(hasDiffuse2,  ref diffuse2ID, ref textureIndex);
                    UpdateLabelledId(hasNormalMap, ref normalID,   ref textureIndex);
                }
                else
                {
                    // The order of the textures here is critical. 
                    UpdateLabelledId(hasDiffuse,   ref diffuse1ID,  ref textureIndex);
                    UpdateLabelledId(hasSphereMap, ref sphereMapID, ref textureIndex);
                    UpdateLabelledId(hasDiffuse2,  ref diffuse2ID,  ref textureIndex);
                    UpdateLabelledId(hasDiffuse3,  ref diffuse3ID,  ref textureIndex);
                    UpdateLabelledId(hasStageMap,  ref stageMapID,  ref textureIndex);
                    UpdateLabelledId(hasCubeMap ,  ref cubeMapID,   ref textureIndex);
                    UpdateLabelledId(hasAoMap,     ref aoMapID,     ref textureIndex);
                    UpdateLabelledId(hasNormalMap, ref normalID,    ref textureIndex);
                    UpdateLabelledId(hasRamp,      ref rampID,      ref textureIndex);
                    UpdateLabelledId(hasDummyRamp, ref dummyRampID, ref textureIndex);
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
            public bool glow = false;
            public bool hasShadow = false;
            public bool useVertexColor = false;
            public bool useReflectionMask = false;
            public bool useColorGainOffset = false;
            public bool hasBayoHair = false;
            public bool useDiffuseBlend = false;
            public bool softLightBrighten = false;

            // Texture flags
            public bool hasDiffuse = false;
            public bool hasNormalMap = false;
            public bool hasDiffuse2 = false;
            public bool hasDiffuse3 = false;
            public bool hasAoMap = false;
            public bool hasStageMap = false;
            public bool hasCubeMap = false;
            public bool hasRamp = false;
            public bool hasSphereMap = false;
            public bool hasDummyRamp = false;

            // texture IDs for preserving existing textures
            public int diffuse1ID = 0;
            public int diffuse2ID = 0;
            public int diffuse3ID = 0;
            public int normalID = 0;
            public int rampID = (int)DummyTextures.DummyRamp;
            public int dummyRampID = (int)DummyTextures.DummyRamp;
            public int sphereMapID = 0;
            public int aoMapID = 0;
            public int stageMapID = (int)DummyTextures.StageMapHigh;
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
                Material material = new Material();
                material.Flags = 0xA2001001;
                material.RefAlpha = 128;
                material.cullMode = 1029;

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
                    if (hasDiffuse)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (hasDiffuse2)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (hasDiffuse3)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (hasStageMap)
                    {
                        // Don't preserve stageMap ID.
                        continue;
                    }
                    if (hasCubeMap)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (hasSphereMap)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (hasAoMap)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (hasNormalMap)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (hasRamp)
                    {
                        textures[i].hash = other.textures[i].hash;
                        continue;
                    }
                    if (hasDummyRamp)
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

                if (hasNormalMap && preserveNrmMap)
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
                if (hasDiffuse)
                    new4thByte |= (byte)TextureFlags.DiffuseMap;
                if (hasNormalMap)
                    new4thByte |= (byte)TextureFlags.NormalMap;
                if (hasCubeMap || hasRamp)
                    new4thByte |= (byte)TextureFlags.RampCubeMap;
                if (hasStageMap || hasAoMap)
                    new4thByte |= (byte)TextureFlags.StageAOMap;
                if (hasSphereMap)
                    new4thByte |= (byte)TextureFlags.SphereMap;
                if (glow)
                    new4thByte |= (byte) TextureFlags.Glow;
                if (hasShadow)
                    new4thByte |= (byte) TextureFlags.Shadow;
                if (hasDummyRamp)
                    new4thByte |= (byte) TextureFlags.DummyRamp; 
                flag = (flag & 0xFFFFFF00) | new4thByte;

                return flag;
            }

            private void CheckFlags()
            {
                int intFlags = ((int)flag);
                glow = (intFlags & (int)TextureFlags.Glow) > 0;
                hasShadow = (intFlags & (int)TextureFlags.Shadow) > 0;
                CheckMisc(intFlags);
                CheckTextures(flag);
            }

            private void CheckMisc(int matFlags)
            {
                // Some hacky workarounds until I understand flags better.
                useColorGainOffset = CheckColorGain(flag);
                useDiffuseBlend = (matFlags & 0xD0090000) == 0xD0090000 || (matFlags & 0x90005000) == 0x90005000;
                useVertexColor = CheckVertexColor(flag);
                useReflectionMask = (matFlags & 0xFFFFFF00) == 0xF8820000;
                hasBayoHair = (matFlags & 0x00FF0000) == 0x00420000;
                softLightBrighten = ((matFlags & 0x00FF0000) == 0x00810000 || (matFlags & 0xFFFF0000) == 0xFA600000);
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
                hasDiffuse = (matFlags & (byte)TextureFlags.DiffuseMap) > 0 || isEffectMaterial;

                hasSphereMap = (byte4 & (byte)TextureFlags.SphereMap) > 0;

                hasNormalMap = (byte4 & (byte)TextureFlags.NormalMap) > 0;

                hasDummyRamp = (byte4 & (byte)TextureFlags.DummyRamp) > 0;

                hasAoMap = (byte4 & (byte)TextureFlags.StageAOMap) > 0 && !hasDummyRamp;

                hasStageMap = (byte4 & (byte)TextureFlags.StageAOMap) > 0 && hasDummyRamp;

                bool hasRampCubeMap = (matFlags & (int)TextureFlags.RampCubeMap) > 0;
                hasCubeMap = (matFlags & (int)TextureFlags.RampCubeMap) > 0 && (!hasDummyRamp) && (!hasSphereMap);
                hasRamp = (matFlags & (int)TextureFlags.RampCubeMap) > 0 && hasDummyRamp;

                hasDiffuse3 = (byte3 & 0x91) == 0x91 || (byte3 & 0x96) == 0x96 || (byte3 & 0x99) == 0x99;

                hasDiffuse2 = hasRampCubeMap && ((matFlags & (int)TextureFlags.NormalMap) == 0)
                    && (hasDummyRamp || hasDiffuse3);

                // Jigglypuff has weird eyes, so just hardcode it.
                if ((matFlags & 0xFFFFFFFF) == 0x9AE11163)
                {
                    hasDiffuse2 = true;
                    hasNormalMap = true;
                }

                // Mega Man also has strange eyes.
                if ((matFlags & 0xFFFFFFFF) == 0x92F01101)
                {
                    hasDiffuse2 = true;
                    hasRamp = true;
                    hasDummyRamp = true;
                }
            }
        }

        public class Polygon : TreeNode
        {
            // Bone types and vertex types control two bytes of the vertsize.
            public enum BoneTypes
            {
                NoBones =  0x00,
                Float = 0x10,
                HalfFloat = 0x20,
                Byte = 0x40
            }

            public enum VertexTypes
            {
                NoNormals = 0x0,
                NormalsFloat = 0x1,
                NormalsTanBiTanFloat = 0x3,
                NormalsHalfFloat = 0x6,
                NormalsTanBiTanHalfFloat = 0x7
            }

            // Used to generate a unique color for viewport selection.
            private static List<int> previousDisplayIds = new List<int>();
            public int DisplayId { get { return displayId; } }
            private int displayId = 0;

            // The number of vertices is vertexIndices.Count because many vertices are shared.
            public List<Vertex> vertices = new List<Vertex>();
            public List<int> vertexIndices = new List<int>();
            public int displayFaceSize = 0;

            public List<Material> materials = new List<Material>();

            // defaults to a basic bone weighted vertex format
            public int vertSize = (int)BoneTypes.Byte | (int)VertexTypes.NormalsHalfFloat;

            public int UVSize = 0x12;
            public int strip = 0x40;
            public int polflag = 0x04;

            // for drawing
            public bool isTransparent = false;
            public int[] display;
            public int[] selectedVerts;
            public int Offset; // For Rendering


            public Polygon()
            {
                Checked = true;
                Text = "Polygon";
                ImageKey = "polygon";
                SelectedImageKey = "polygon";
                GenerateDisplayId();
            }

            public void AddVertex(Vertex v)
            {
                vertices.Add(v);
            }

            private void GenerateDisplayId()
            {
                // Find last used ID. Next ID will be last ID + 1.
                // A color is generated from the integer as hexadecimal, but alpha is ignored.
                // Incrementing will affect RGB before it affects Alpha (ARGB color).
                int index = 0;
                if (previousDisplayIds.Count > 0)
                    index = previousDisplayIds.Last();
                index++;
                previousDisplayIds.Add(index);
                displayId = index;
            }

            public void AOSpecRefBlend()
            {
                // change aomingain to only affect specular and reflection. ignore 2nd material
                if (materials[0].entries.ContainsKey("NU_aoMinGain"))
                {
                    materials[0].entries["NU_aoMinGain"][0] = 15.0f;
                    materials[0].entries["NU_aoMinGain"][1] = 15.0f;
                    materials[0].entries["NU_aoMinGain"][2] = 15.0f;
                    materials[0].entries["NU_aoMinGain"][3] = 0.0f;
                }
            }

            public List<DisplayVertex> CreateDisplayVertices()
            {
                // rearrange faces
                display = GetRenderingVertexIndices().ToArray();

                List<DisplayVertex> displayVertList = new List<DisplayVertex>();

                if (vertexIndices.Count <= 3)
                    return displayVertList;
                foreach (Vertex v in vertices)
                {
                    DisplayVertex displayVert = new DisplayVertex()
                    {
                        pos = v.pos,
                        nrm = v.nrm,
                        tan = v.tan.Xyz,
                        bit = v.bitan.Xyz,
                        col = v.color / 127,
                        uv = v.uv.Count > 0 ? v.uv[0] : new Vector2(0, 0),
                        uv2 = v.uv.Count > 1 ? v.uv[1] : new Vector2(0, 0),
                        uv3 = v.uv.Count > 2 ? v.uv[2] : new Vector2(0, 0),
                        boneIds = new Vector4(
                            v.boneIds.Count > 0 ? v.boneIds[0] : -1,
                            v.boneIds.Count > 1 ? v.boneIds[1] : -1,
                            v.boneIds.Count > 2 ? v.boneIds[2] : -1,
                            v.boneIds.Count > 3 ? v.boneIds[3] : -1),
                        weight = new Vector4(
                            v.boneWeights.Count > 0 ? v.boneWeights[0] : 0,
                            v.boneWeights.Count > 1 ? v.boneWeights[1] : 0,
                            v.boneWeights.Count > 2 ? v.boneWeights[2] : 0,
                            v.boneWeights.Count > 3 ? v.boneWeights[3] : 0),
                    };
                    displayVertList.Add(displayVert);
                }

                selectedVerts = new int[displayVertList.Count];
                return displayVertList;
            }

            public void CalculateTangentBitangent()
            {
                // Don't generate tangents and bitangents if the vertex format doesn't support them. 
                int vertType = vertSize & 0xF;
                if (!(vertType == 3 || vertType == 7))
                    return;

                List<int> f = GetRenderingVertexIndices();
                Vector3[] tanArray = new Vector3[vertices.Count];
                Vector3[] bitanArray = new Vector3[vertices.Count];

                CalculateTanBitanArrays(f, tanArray, bitanArray);
                ApplyTanBitanArray(tanArray, bitanArray);
            }

            public void SetVertexColor(Vector4 intColor)
            {
                // (127, 127, 127, 127) is white.
                foreach (Vertex v in vertices)
                {
                    v.color = intColor;
                }
            }


            private void ApplyTanBitanArray(Vector3[] tanArray, Vector3[] bitanArray)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vertex v = vertices[i];
                    Vector3 newTan = tanArray[i];
                    Vector3 newBitan = bitanArray[i];

                    // The tangent and bitangent should be orthogonal to the normal but not each other. 
                    // Bitangents are not calculated with a cross product to prevent flipped shading with mirrored normal maps.
                    v.tan = new Vector4(VectorTools.Orthogonalize(newTan, v.nrm), 1);
                    v.bitan = new Vector4(VectorTools.Orthogonalize(newBitan, v.nrm), 1);
                    v.bitan *= -1;
                }
            }

            private void CalculateTanBitanArrays(List<int> faces, Vector3[] tanArray, Vector3[] bitanArray)
            {
                // Three verts per face.
                for (int i = 0; i < displayFaceSize; i += 3)
                {
                    Vertex v1 = vertices[faces[i]];
                    Vertex v2 = vertices[faces[i + 1]];
                    Vertex v3 = vertices[faces[i + 2]];

                    // Check for index out of range errors and just skip this face.
                    if (v1.uv.Count < 1 || v2.uv.Count < 1 || v3.uv.Count < 1)
                        continue;

                    Vector3 s = new Vector3();
                    Vector3 t = new Vector3();
                    VectorTools.GenerateTangentBitangent(v1.pos, v2.pos, v3.pos, v1.uv[0], v2.uv[0], v3.uv[0], out s, out t);

                    // Average tangents and bitangents.
                    tanArray[faces[i]] += s;
                    tanArray[faces[i + 1]] += s;
                    tanArray[faces[i + 2]] += s;

                    bitanArray[faces[i]] += t;
                    bitanArray[faces[i + 1]] += t;
                    bitanArray[faces[i + 2]] += t;
                }
            }

            public void SmoothNormals()
            {
                Vector3[] normals = new Vector3[vertices.Count];

                List<int> f = GetRenderingVertexIndices();

                for (int i = 0; i < displayFaceSize; i += 3)
                {
                    Vertex v1 = vertices[f[i]];
                    Vertex v2 = vertices[f[i+1]];
                    Vertex v3 = vertices[f[i+2]];
                    Vector3 nrm = VectorTools.CalculateNormal(v1.pos, v2.pos, v3.pos);

                    normals[f[i + 0]] += nrm;
                    normals[f[i + 1]] += nrm;
                    normals[f[i + 2]] += nrm;
                }
                
                for (int i = 0; i < normals.Length; i++)
                    vertices[i].nrm = normals[i].Normalized();

                // Compare each vertex with all the remaining vertices. This might skip some.
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vertex v = vertices[i];

                    for (int j = i + 1; j < vertices.Count; j++)
                    {
                        Vertex v2 = vertices[j];

                        if (v == v2)
                            continue;
                        float dis = (float)Math.Sqrt(Math.Pow(v.pos.X - v2.pos.X, 2) + Math.Pow(v.pos.Y - v2.pos.Y, 2) + Math.Pow(v.pos.Z - v2.pos.Z, 2));
                        if (dis <= 0f) // Extra smooth
                        {
                            Vector3 nn = ((v2.nrm + v.nrm) / 2).Normalized();
                            v.nrm = nn;
                            v2.nrm = nn;
                        }
                    }
                }
            }

            public void CalculateNormals()
            {
                Vector3[] normals = new Vector3[vertices.Count];

                for (int i = 0; i < normals.Length; i++)
                    normals[i] = new Vector3(0, 0, 0);

                List<int> f = GetRenderingVertexIndices();

                for (int i = 0; i < displayFaceSize; i += 3)
                {
                    Vertex v1 = vertices[f[i]];
                    Vertex v2 = vertices[f[i + 1]];
                    Vertex v3 = vertices[f[i + 2]];
                    Vector3 nrm = VectorTools.CalculateNormal(v1.pos, v2.pos, v3.pos);

                    normals[f[i + 0]] += nrm * (nrm.Length / 2);
                    normals[f[i + 1]] += nrm * (nrm.Length / 2);
                    normals[f[i + 2]] += nrm * (nrm.Length / 2);
                }

                for (int i = 0; i < normals.Length; i++)
                    vertices[i].nrm = normals[i].Normalized();
            }

            public void AddDefaultMaterial()
            {
                Material mat = Material.GetDefault();
                materials.Add(mat);
                mat.textures.Add(new MatTexture(0x10000000));
                mat.textures.Add(MatTexture.GetDefault());
            }

            public List<int> GetRenderingVertexIndices()
            {
                if ((strip >> 4) == 4)
                {
                    displayFaceSize = vertexIndices.Count;
                    return vertexIndices;
                }
                else
                {
                    List<int> vertexIndices = new List<int>();

                    int startDirection = 1;
                    int p = 0;
                    int f1 = this.vertexIndices[p++];
                    int f2 = this.vertexIndices[p++];
                    int faceDirection = startDirection;
                    int f3;
                    do
                    {
                        f3 = this.vertexIndices[p++];
                        if (f3 == 0xFFFF)
                        {
                            f1 = this.vertexIndices[p++];
                            f2 = this.vertexIndices[p++];
                            faceDirection = startDirection;
                        }
                        else
                        {
                            faceDirection *= -1;
                            if ((f1 != f2) && (f2 != f3) && (f3 != f1))
                            {
                                if (faceDirection > 0)
                                {
                                    vertexIndices.Add(f3);
                                    vertexIndices.Add(f2);
                                    vertexIndices.Add(f1);
                                }
                                else
                                {
                                    vertexIndices.Add(f2);
                                    vertexIndices.Add(f3);
                                    vertexIndices.Add(f1);
                                }
                            }
                            f1 = f2;
                            f2 = f3;
                        }
                    } while (p < this.vertexIndices.Count);

                    displayFaceSize = vertexIndices.Count;
                    return vertexIndices;
                }
            }
        }

        // typically a mesh will just have 1 polygon
        // but you can just use the mesh class without polygons
        public class Mesh : TreeNode
        {
            public enum BoneFlags
            {
                NotRigged = 0,
                Rigged = 4,
                SingleBind = 8
            }

            // Used to generate a unique color for mesh viewport selection.
            private static List<int> previousDisplayIds = new List<int>();
            private int displayId = 0;
            public int DisplayId { get { return displayId; } }

            public int boneflag = (int)BoneFlags.Rigged;
            public short singlebind = -1;
            public int sortBias = 0;
            public bool billboardY = false;
            public bool billboard = false;
            public bool useNsc = false;

            public bool sortByObjHeirarchy = true;
            public float[] boundingSphere = new float[8];
            public float sortingDistance = 0;

            public Mesh()
            {
                Checked = true;
                ImageKey = "mesh";
                SelectedImageKey = "mesh";
                GenerateDisplayId();
            }

            private void GenerateDisplayId()
            {
                // Find last used ID. Next ID will be last ID + 1.
                // A color is generated from the integer as hexadecimal, but alpha is ignored.
                // Incrementing will affect RGB before it affects Alpha (ARGB color).
                int index = 0;
                if (previousDisplayIds.Count > 0)
                    index = previousDisplayIds.Last();
                index++;
                previousDisplayIds.Add(index);
                displayId = index;
            }

            public void addVertex(Vertex v)
            {
                if (Nodes.Count == 0)
                    Nodes.Add(new Polygon());

                ((Polygon)Nodes[0]).AddVertex(v);
            }

            public void generateBoundingSphere()
            {
                Vector3 cen1 = new Vector3(0,0,0), cen2 = new Vector3(0,0,0);
                double rad1 = 0, rad2 = 0;

                //Get first vert
                int vertCount = 0;
                Vector3 vert0 = new Vector3();
                foreach (Polygon p in Nodes)
                {
                    foreach (Vertex v in p.vertices)
                    {
                        vert0 = v.pos;
                        vertCount++;
                        break;
                    }
                    break;
                }

                if (vertCount == 0)
                    return;

                //Calculate average and min/max
                Vector3 min = new Vector3(vert0);
                Vector3 max = new Vector3(vert0);

                vertCount = 0;
                foreach (Polygon p in Nodes)
                {
                    foreach(Vertex v in p.vertices)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            min[i] = Math.Min(min[i], v.pos[i]);
                            max[i] = Math.Max(max[i], v.pos[i]);
                        }

                        cen1 += v.pos;
                        vertCount++;
                    }
                }

                cen1 /= vertCount;
                for (int i = 0; i < 3; i++)
                    cen2[i] = (min[i]+max[i])/2;

                //Calculate the radius of each
                double dist1, dist2;
                foreach (Polygon p in Nodes)
                {
                    foreach (Vertex v in p.vertices)
                    {
                        dist1 = ((Vector3)(v.pos - cen1)).Length;
                        if (dist1 > rad1)
                            rad1 = dist1;

                        dist2 = ((Vector3)(v.pos - cen2)).Length;
                        if (dist2 > rad2)
                            rad2 = dist2;
                    }
                }

                // Use the one with the lowest radius.
                Vector3 temp;
                double radius;
                if (rad1 < rad2)
                {
                    temp = cen1;
                    radius = rad1;
                }
                else
                {
                    temp = cen2;
                    radius = rad2;
                }

                // Set
                for (int i = 0; i < 3; i++)
                {
                    boundingSphere[i] = temp[i];
                    boundingSphere[i+4] = temp[i];
                }
                boundingSphere[3] = (float)radius;
                boundingSphere[7] = 0;
            }

            public float CalculateSortingDistance(Vector3 cameraPosition)
            {
                Vector3 meshCenter = new Vector3(boundingSphere[0], boundingSphere[1], boundingSphere[2]);
                if (useNsc && singlebind != -1)
                {
                    // Use the bone position as the bounding box center
                    ModelContainer modelContainer = (ModelContainer)Parent.Parent;
                    meshCenter = modelContainer.VBN.bones[singlebind].pos;
                }

                Vector3 distanceVector = new Vector3(cameraPosition - meshCenter);
                return distanceVector.Length + boundingSphere[3] + sortBias;
            }

            private int CalculateSortBias()
            {
                if (!(Text.Contains("SORTBIAS")))
                    return 0;

                // Isolate the integer value from the mesh name.
                string sortBiasKeyWord = "SORTBIAS";
                string sortBiasText = GetSortBiasNumbers(sortBiasKeyWord);

                int sortBiasValue = 0;
                int.TryParse(sortBiasText, out sortBiasValue);

                // TODO: What does "m" do? Ex: SORTBIASm50_
                int firstSortBiasCharIndex = Text.IndexOf(sortBiasKeyWord) + sortBiasKeyWord.Length;
                if (Text[firstSortBiasCharIndex] == 'm')
                    sortBiasValue *= -1;

                return sortBiasValue;
            }

            private string GetSortBiasNumbers(string sortBiasKeyWord)
            {
                string sortBiasText = "";
                for (int i = Text.IndexOf(sortBiasKeyWord) + sortBiasKeyWord.Length; i < Text.Length; i++)
                {
                    if (Text[i] != '_')
                        sortBiasText += Text[i];
                    else
                        break;
                }

                return sortBiasText;
            }

            public void SetMeshAttributesFromName()
            {
                sortBias = CalculateSortBias();
                billboard = Text.Contains("BILLBOARD");
                billboardY = Text.Contains("BILLBOARDYAXIS");
                useNsc = Text.Contains("NSC");
                sortByObjHeirarchy = Text.Contains("HIR");
            }
        }

        #endregion

        #region Converters

        public MBN toMBN()
        {
            MBN m = new Smash_Forge.MBN();

            m.setDefaultDescriptor();
            List<MBN.Vertex> vertBank = new List<MBN.Vertex>();

            foreach (Mesh mesh in Nodes)
            {
                MBN.Mesh nmesh = new MBN.Mesh();
                
                int pi = 0;
                int fadd = vertBank.Count;
                nmesh.nodeList = new List<List<int>>();
                nmesh.faces = new List<List<int>>();
                foreach (Polygon p in mesh.Nodes)
                {
                    List<int> nodeList = new List<int>();
                    // vertices
                    foreach(Vertex v in p.vertices)
                    {
                        MBN.Vertex mv = new MBN.Vertex();
                        mv.pos = v.pos;
                        mv.nrm = v.nrm;
                        List<Vector2> uvs = new List<Vector2>();
                        uvs.Add(new Vector2(v.uv[0].X, 1 - v.uv[0].Y));
                        mv.tx = uvs;
                        mv.col = v.color;
                        int n1 = v.boneIds[0];
                        int n2 = v.boneIds.Count > 1 ? v.boneIds[1] : 0;
                        if (!nodeList.Contains(n1)) nodeList.Add(n1);
                        if (!nodeList.Contains(n2)) nodeList.Add(n2);
                        mv.node.Add(nodeList.IndexOf(n1));
                        mv.node.Add(nodeList.IndexOf(n2));
                        mv.weight.Add(v.boneWeights[0]);
                        mv.weight.Add(v.boneWeights.Count > 1 ? v.boneWeights[1] : 0);
                        vertBank.Add(mv);
                    }
                    // Node list 
                    nmesh.nodeList.Add(nodeList);
                    // polygons
                    List<int> fac = new List<int>();
                    nmesh.faces.Add(fac);
                    foreach (int i in p.vertexIndices)
                        fac.Add(i + fadd);
                    pi++;
                }

                m.mesh.Add(nmesh);
            }
            m.vertices = vertBank;

            return m;
        }

        public void OptimizeFileSize(bool singleBind = false)
        {
            // Generate proper indices.
            MergeDuplicateVertices();

            // This is pretty broken right now.
            //OptimizeSingleBind(false);
        }

        private void MergeDuplicateVertices()
        {
            // Massive reductions in file size but very slow.

            int MAX_BANK = 30000; //  for speeding this up a little with some loss...

            foreach (Mesh m in Nodes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    List<Vertex> newVertices = new List<Vertex>();
                    List<int> newFaces = new List<int>();

                    List<Vertex> vbank = new List<Vertex>(); // only check last 50 verts - may miss far apart ones but is faster
                    foreach (int f in p.vertexIndices)
                    {
                        int newFaceIndex = -1; 
                        int i = 0;

                        // Has to loop through all the new vertices each time, which is very slow.
                        foreach (Vertex v in vbank)
                        {
                            if (v.Equals(p.vertices[f]))
                            {
                                newFaceIndex = i;
                                break;
                            }
                            else
                                i++;
                        }

                        bool verticesAreEqual = newFaceIndex != -1;
                        if (verticesAreEqual)
                        {
                            newFaces.Add(newVertices.Count + newFaceIndex);
                        }
                        else
                        {
                            vbank.Add(p.vertices[f]);
                            newFaces.Add(newVertices.Count + vbank.Count - 1);
                        }
                        if(vbank.Count > MAX_BANK)
                        {
                            newVertices.AddRange(vbank);
                            vbank.Clear();
                        }
                    }
                    newVertices.AddRange(vbank);

                    p.vertices = newVertices;
                    p.vertexIndices = newFaces;
                    p.displayFaceSize = 0;
                }
            }
        }

        private void OptimizeSingleBind(bool useSingleBind)
        {
            // Use single bind to avoid saving weights.
            // The space savings are significant but not as much as merging duplicate vertices. 

            foreach (Mesh m in Nodes)
            {
                bool isSingleBound = true;
                int singleBindBone = -1;

                foreach (Polygon p in m.Nodes)
                {
                    foreach (Vertex v in p.vertices)
                    {
                        if (v.boneIds.Count > 0 && isSingleBound)
                        {
                            // Can't use single bind if some vertices aren't weighted to the same bone. 
                            if (singleBindBone == -1)
                            {
                                singleBindBone = v.boneIds[0];
                            }
                            else if(singleBindBone != v.boneIds[0])
                            {
                                isSingleBound = false;
                                break;
                            }

                            // Vertices bound to a single bone will have a node.Count of 1.
                            if (v.boneIds.Count > 1)
                            {
                                isSingleBound = false;
                                break;
                            }
                     
                        }
                    }
                }

                if (isSingleBound && useSingleBind)
                {
                    SingleBindMesh(m, singleBindBone);
                }
            }
        }

        private static void SingleBindMesh(Mesh m, int singleBindBone)
        {
            m.boneflag = (int)Mesh.BoneFlags.SingleBind;
            m.singlebind = (short)singleBindBone;
            foreach (Polygon p in m.Nodes)
            {
                p.polflag = 0;
                p.vertSize = p.vertSize & 0x0F;
            }
        }

        public void ComputeTangentBitangent()
        {
            foreach (Mesh m in Nodes)
            {
                foreach (Polygon p in m.Nodes)
                    p.CalculateTangentBitangent();
            }
        }

        #endregion

        public List<int> GetTexIds()
        {
            List<int> texIds = new List<int>();
            foreach (Mesh m in Nodes)
                foreach (Polygon poly in m.Nodes)
                    foreach (var mat in poly.materials)
                        if(!texIds.Contains(mat.displayTexId))
                            texIds.Add(mat.displayTexId);
            return texIds;
        }
    }
}

