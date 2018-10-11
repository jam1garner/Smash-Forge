using OpenTK;
using OpenTK.Graphics.OpenGL;
using SALT.Graphics;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.BufferObjects;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects.Textures;
using SFGraphics.Utils;
using Smash_Forge.Filetypes.Models.Nuds;
using Smash_Forge.Rendering;
using Smash_Forge.Rendering.Lights;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Smash_Forge
{
    public partial class NUD : FileBase, IBoundableModel
    {
        // OpenGL Buffers
        private BufferObject bonesUbo;
        private BufferObject selectVbo;

        //Smash and Pokkén both use version 0x0200 NUD
        //Smash uses big endian NUD (NDP3)
        //Pokkén uses little endian NUD (NDWD)
        public override Endianness Endian { get; set; }
        public ushort version = 0x0200;

        //If the ModelContainer of the NUD has no bones, we will write the type as 0 regardless of this value
        //If it does have bones, this value will be written as normal. 2 is common but it can also validly be 0 and other values
        public ushort type = 2;
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

        public Vector4 BoundingSphere
        {
            get { return new Vector4(boundingSphere[0], boundingSphere[1], boundingSphere[2], boundingSphere[3]); }
        }

        private void SetupTreeNode()
        {
            Text = "model.nud";
            ImageKey = "model";
            SelectedImageKey = "model";
        }

        private void GenerateBuffers()
        {
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
                                if (matTex.hash == nutTex.HashId)
                                {
                                    validTextureId = true;
                                    break;
                                }
                            }

                            // Checks to see if the texture ID is a valid dummy texture.
                            foreach (NudEnums.DummyTexture dummyTex in Enum.GetValues(typeof(NudEnums.DummyTexture)))
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
            depthSortedMeshes = unsortedMeshes.OrderBy(m => m.sortingDistance).ToList();
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

        public void UpdateRenderMeshes()
        {
            foreach (Mesh mesh in Nodes)
            {
                foreach (Polygon p in mesh.Nodes)
                {
                    p.renderMesh = CreateRenderMesh(p);
                }
            }
        }

        public NudRenderMesh CreateRenderMesh(Polygon p)
        {
            // Store all of the polygon vert data in one buffer.
            List<DisplayVertex> displayVerticesList;
            List<int> vertexIndicesList;
            p.GetDisplayVerticesAndIndices(out displayVerticesList, out vertexIndicesList);

            NudRenderMesh nudRenderMesh = new NudRenderMesh(displayVerticesList, vertexIndicesList);
            // Only use the first material for now.
            if (p.materials.Count > 0)
                nudRenderMesh.SetRenderSettings(p.materials[0]);
            return nudRenderMesh;
        }

        public void Render(VBN vbn, Camera camera, bool drawShadow = false, bool drawPolyIds = false)
        {
            if (bonesUbo == null || selectVbo == null)
            {
                UpdateRenderMeshes();
                GenerateBuffers();
            }

            // Main function for NUD rendering.
            if (Runtime.renderBoundingSphere)
                DrawBoundingSpheres();

            // Choose the correct shader.
            Shader shader;
            if (drawShadow)
                shader = OpenTKSharedResources.shaders["Shadow"];
            else if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                shader = OpenTKSharedResources.shaders["NudDebug"];
            else
                shader = OpenTKSharedResources.shaders["Nud"];

            // Set bone matrices.
            UpdateBonesBuffer(vbn, shader, bonesUbo);

            DrawAllPolygons(shader, camera, drawPolyIds);
        }

        private void UpdateBonesBuffer(VBN vbn, Shader shader, BufferObject bonesUbo)
        {
            shader.UseProgram();

            if (vbn == null)
            {
                shader.SetBoolToInt("useBones", false);
                return;
            }

            Matrix4[] boneMatrices = vbn.GetShaderMatrices();
            if (boneMatrices.Length == 0)
            {
                shader.SetBoolToInt("useBones", false);
                return;
            }

            // Update bone matrices for the shader.
            int blockIndex = GL.GetUniformBlockIndex(shader.Id, "bones");
            bonesUbo.BindBase(BufferRangeTarget.UniformBuffer, blockIndex);

            bonesUbo.SetData(boneMatrices, BufferUsageHint.DynamicDraw);

            shader.SetBoolToInt("useBones", true);
        }

        private void DrawBoundingSpheres()
        {
            GL.UseProgram(0);

            // Draw NUD bounding box. 
            GL.Color4(Color.GhostWhite);
            ShapeDrawing.DrawCube(new Vector3(boundingSphere[0], boundingSphere[1], boundingSphere[2]), boundingSphere[3], true);

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
                        ShapeDrawing.DrawCube(center, mesh.boundingSphere[3], true);
                    }
                    else
                    {
                        ShapeDrawing.DrawCube(new Vector3(mesh.boundingSphere[0], mesh.boundingSphere[1], mesh.boundingSphere[2]), mesh.boundingSphere[3], true);
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
            DrawSelectionOutlines(shader, camera);
        }

        private void DrawSelectionOutlines(Shader shader, Camera camera)
        {
            foreach (Mesh m in Nodes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    if (((Mesh)p.Parent).Checked && p.Checked)
                    {
                        if ((p.IsSelected || p.Parent.IsSelected))
                        {
                            DrawModelSelection(p, shader, camera);
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

        private void DrawPolygonShaded(Polygon p, Shader shader, Camera camera, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures, bool drawId = false)
        {
            if (p.vertexIndices.Count < 3)
                return;

            Material material = p.materials[0];

            // Set Shader Values.
            shader.UseProgram();
            SetShaderUniforms(p, shader, camera, material, dummyTextures, p.DisplayId, drawId);

            // Update render mesh settings.
            // This is slow, but performance isn't an issue for NUDs.
            p.renderMesh.SetRenderSettings(material);
            p.renderMesh.SetMaterialValues(material);

            p.renderMesh.Draw(shader, camera);
        }

        private void SetShaderUniforms(Polygon p, Shader shader, Camera camera, Material material, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures, int id = 0, bool drawId = false)
        {
            // Shader Uniforms
            shader.SetUint("flags", material.Flags);
            shader.SetBoolToInt("renderVertColor", Runtime.renderVertColor && material.useVertexColor);
            NudUniforms.SetTextureUniforms(shader, material, dummyTextures);

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
            shader.SetVector3("colorId", ColorUtils.GetVector3(Color.FromArgb(id)));
            shader.SetBoolToInt("drawId", drawId);

            // The fragment alpha is set to 1 when alpha blending/testing aren't used.
            // This fixes the alpha output for PNG renders.
            p.isTransparent = (material.srcFactor > 0) || (material.dstFactor > 0) || (material.alphaFunction > 0) || (material.alphaTest > 0);
            shader.SetBoolToInt("isTransparent", p.isTransparent);
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

            // Transform object using the bone's transforms
            Mesh mesh = (Mesh)p.Parent;
            if (mesh != null && mesh.Text.Contains("_NSC"))
            {
                int index = mesh.singlebind;
                if (index != -1)
                {
                    // HACK
                    ModelContainer modelContainer = (ModelContainer)mesh.Parent.Parent;
                    if (modelContainer.VBN != null)
                        nscMatrix = modelContainer.VBN.bones[index].transform;
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
                shader.SetVector3("lightSetColor", ColorUtils.GetVector3(color));
            }
        }

        private void DrawModelSelection(Polygon p, Shader shader, Camera camera)
        {
            // This might have been changed to reverse subtract.
            GL.BlendEquation(BlendEquationMode.FuncAdd);

            GL.Enable(EnableCap.StencilTest);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            GL.Disable(EnableCap.DepthTest);

            bool[] cwm = new bool[4];
            GL.GetBoolean(GetIndexedPName.ColorWritemask, 4, cwm);
            GL.ColorMask(false, false, false, false);

            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);

            p.renderMesh.Draw(shader, camera);

            GL.ColorMask(cwm[0], cwm[1], cwm[2], cwm[3]);

            GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
            GL.StencilMask(0x00);

            // Override the model color with white in the shader.
            shader.SetInt("drawSelection", 1);

            GL.LineWidth(2.0f);
            p.renderMesh.SetWireFrame(true);
            p.renderMesh.Draw(shader, camera);
            p.renderMesh.SetWireFrame(false);

            shader.SetInt("drawSelection", 0);

            GL.StencilMask(0xFF);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
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

        public void DrawPoints(Camera camera, VBN vbn, PrimitiveType type)
        {
            // I doubt this even still works.
            Shader shader = OpenTKSharedResources.shaders["Point"];
            shader.UseProgram();
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

            //shader.EnableVertexAttributes();
            foreach (Mesh m in Nodes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    //vertexDataVbo.Bind();
                    GL.VertexAttribPointer(shader.GetAttribLocation("vPosition"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 0);
                    GL.VertexAttribPointer(shader.GetAttribLocation("vBone"), 4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 72);
                    GL.VertexAttribPointer(shader.GetAttribLocation("vWeight"), 4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 88);

                    selectVbo.Bind();
                    if (p.selectedVerts == null)
                        return;
                    selectVbo.SetData(p.selectedVerts, BufferUsageHint.StaticDraw);
                    GL.VertexAttribPointer(shader.GetAttribLocation("vSelected"), 1, VertexAttribPointerType.Int, false, sizeof(int), 0);

                    //vertexIndexEbo.Bind();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    
                    GL.PointSize(6f);
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    GL.DrawElements(type, p.displayFaceSize, DrawElementsType.UnsignedInt, 0);
                }
            }
            //shader.DisableVertexAttributes();
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
                                System.Buffer.BlockCopy(matHashFloat, 0, bytes, 0, 4);
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
            fileData.seek(4);
            if (magic.Equals("NDP3"))
                Endian = Endianness.Big;
            else if (magic.Equals("NDWD"))
                Endian = Endianness.Little;

            fileData.Endian = Endian;
            fileData.readUInt(); //Filesize

            //Always read version in BE
            fileData.Endian = Endianness.Big;
            version = fileData.readUShort();
            fileData.Endian = Endian;

            int polysets = fileData.readUShort();
            type = fileData.readUShort();
            boneCount = fileData.readUShort();

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
                fileData.readUShort(); //Seems to be always 0
                boneflags[i] = fileData.readUShort(); //Controls whether it's single-bound, weighted, or unbound
                obj[i].singlebind = fileData.readShort(); //Bone index if it is single-bound, otherwise -1
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

                m.Flags = d.readUInt();
                d.skip(4);
                m.srcFactor = d.readUShort();
                ushort texCount = d.readUShort();
                m.dstFactor = d.readUShort();
                m.alphaTest = d.readByte();
                m.alphaFunction = d.readByte();

                m.RefAlpha = d.readUShort();
                m.cullMode = d.readUShort();
                d.skip(4); // unknown
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

                int matAttSize;
                do
                {
                    int pos = d.pos();

                    matAttSize = d.readInt();
                    int nameStart = d.readInt();
                    //valueCount has the same position regardless of endianness
                    //This could either mean that it's one byte long, or that it's always BE
                    //We assume the former, for now
                    d.skip(3);
                    byte valueCount = d.readByte();
                    d.skip(4); //Unknown, always 0, probably padding

                    //If it doesn't have any values, we want to skip over it
                    if (valueCount == 0)
                        goto Continue;

                    string name = d.readString(nameOffset + nameStart, -1);

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

                Continue:
                    if (matAttSize == 0)
                        d.seek(pos + 0x10 + (valueCount * 4));
                    else
                        d.seek(pos + matAttSize);
                } while (matAttSize != 0);

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
                    d.skip(4); //Don't know what this is but it's not nothing
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
            if (Endian == Endianness.Big)
                d.writeString("NDP3");
            else if (Endian == Endianness.Little)
                d.writeString("NDWD");

            d.Endian = Endian;
            d.writeInt(0); //Filesize

            //Always write version in BE
            d.Endian = Endianness.Big;
            d.writeUShort(version);
            d.Endian = Endian;

            d.writeShort(Nodes.Count); // polysets

            boneCount = ((ModelContainer)Parent).VBN == null ? 0 : ((ModelContainer)Parent).VBN.bones.Count;

            d.writeShort(boneCount == 0 ? 0 : type); // type
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
            obj.Endian = Endian;
            FileOutput tex = new FileOutput();
            tex.Endian = Endian;

            FileOutput poly = new FileOutput();
            poly.Endian = Endian;
            FileOutput vert = new FileOutput();
            vert.Endian = Endian;
            FileOutput vertadd = new FileOutput();
            vertadd.Endian = Endian;

            FileOutput str = new FileOutput();
            str.Endian = Endian;

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

                d.writeUShort(0);
                d.writeUShort((ushort)m.boneflag); // Bind method
                d.writeShort(m.singlebind); // Bone index
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

                d.writeShort(mat.RefAlpha);
                d.writeShort(mat.cullMode);
                d.writeInt(0); // unknown
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

                //If there are no material attributes, write a "blank" entry
                if (mat.entries.Count == 0)
                {
                    d.writeInt(0);
                    d.writeInt(0);
                    d.writeInt(0);
                    d.writeInt(0);
                }

                for (int i = 0; i < mat.entries.Count; i++)
                {
                    //It can be seen in Pokkén NDWD that the last material attribute name
                    // does not need to be aligned to 16. So, we do the alignment before writing
                    // the name rather than after.
                    str.align(16);

                    float[] data;
                    mat.entries.TryGetValue(mat.entries.ElementAt(i).Key, out data);
                    d.writeInt(i == mat.entries.Count - 1 ? 0 : 16 + 4 * data.Length);
                    d.writeInt(str.size());

                    str.writeString(mat.entries.ElementAt(i).Key);
                    str.writeByte(0);

                    d.writeByte(0); d.writeByte(0); d.writeByte(0);
                    d.writeByte((byte)data.Length);
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
                            if (Enum.IsDefined(typeof(NudEnums.DummyTexture), matTexture.hash))
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

