using OpenTK;
using OpenTK.Graphics.OpenGL;
using SALT.Graphics;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.BufferObjects;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects.Textures;
using SFGraphics.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SFGenericModel.Materials;
using SmashForge.Filetypes.Models.Nuds;
using SmashForge.Rendering;
using SmashForge.Rendering.Lights;

namespace SmashForge
{
    public partial class Nud : FileBase, IBoundableModel
    {
        // OpenGL Buffers
        private UniformBlock bonesUbo;
        private BufferObject selectVbo;

        //Smash and Pokkén both use version 0x0200 NUD
        //Smash uses big endian NUD (NDP3)
        //Pokkén uses little endian NUD (NDWD)
        public override Endianness Endian { get; set; }
        public ushort version = 0x0200;

        //The lowest and highest bone indexes referenced by the objects and vertices in this NUD
        public ushort boneIndexStart = 0;
        public ushort boneIndexEnd = 0;
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

        public Nud()
        {
            SetupTreeNode();
        }

        public Nud(string filename) : this()
        {
            Read(filename);
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
            bonesUbo = new UniformBlock(OpenTkSharedResources.shaders["Nud"], "BoneMatrices") { BlockBinding = 0 };
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
            // More distance objects will be rendered first.
            depthSortedMeshes = unsortedMeshes.OrderBy(m => -m.sortingDistance).ToList();
        }

        public void UpdateRenderMeshes()
        {
            foreach (Mesh mesh in Nodes)
            {
                foreach (Polygon p in mesh.Nodes)
                {
                    p.renderMesh = CreateRenderMesh(p);

                    // Ensure the newly created uniform buffer has its data set.
                    foreach (var material in p.materials)
                        material.ShouldUpdateRendering = true;
                }
            }
        }

        public NudRenderMesh CreateRenderMesh(Polygon p)
        {
            // Store all of the polygon vert data in one buffer.
            List<DisplayVertex> displayVerticesList;
            List<int> vertexIndicesList;
            p.GetDisplayVerticesAndIndices(out displayVerticesList, out vertexIndicesList);

            return new NudRenderMesh(displayVerticesList, vertexIndicesList);
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
                shader = OpenTkSharedResources.shaders["Shadow"];
            else if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                shader = OpenTkSharedResources.shaders["NudDebug"];
            else
                shader = OpenTkSharedResources.shaders["Nud"];

            // Set bone matrices.
            UpdateBonesBuffer(vbn, shader, bonesUbo);

            DrawAllPolygons(shader, camera, drawPolyIds);
        }

        private void UpdateBonesBuffer(VBN vbn, Shader shader, UniformBlock bonesUbo)
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
            bonesUbo.BindBlock(shader);
            bonesUbo.SetValues("transforms", boneMatrices);

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
                m.GenerateBoundingSphere();

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

            // Opaque meshes aren't depth sorted.
            foreach (Mesh m in Nodes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    if (!p.IsTransparent)
                        opaque.Add(p);
                }
            }

            foreach (Mesh m in depthSortedMeshes)
            {
                foreach (Polygon p in m.Nodes)
                {
                    if (p.IsTransparent)
                        transparent.Add(p);
                }
            }


            shader.UseProgram();
            SetGlobalShaderUniforms(shader, camera, RenderTools.dummyTextures);

            // Only draw polygons if the polygon and its parent are both checked.
            Material previousMaterial = null;
            foreach (Polygon p in opaque)
            {
                if (p.Parent != null && ((Mesh)p.Parent).Checked && p.Checked)
                {
                    DrawPolygonShaded(p, shader, camera, RenderTools.dummyTextures, previousMaterial, drawPolyIds);
                    previousMaterial = p.materials[0];
                }
            }

            foreach (Polygon p in transparent)
            {
                if (((Mesh)p.Parent).Checked && p.Checked)
                {
                    DrawPolygonShaded(p, shader, camera, RenderTools.dummyTextures, previousMaterial, drawPolyIds);
                    previousMaterial = p.materials[0];
                }
            }
            //System.Diagnostics.Debug.WriteLine("");
        }

        private void DrawPolygonShaded(Polygon p, Shader shader, Camera camera, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures, Material previousMaterial, bool drawId)
        {
            if (p.vertexIndices.Count < 3)
                return;

            Material material = p.materials[0];

            // Set Shader Values.
            shader.UseProgram();

            SetPolygonSpecificUniforms(p, shader, material);

            if (!material.EqualTextures(previousMaterial))
                NudUniforms.SetTextureUniforms(shader, material, dummyTextures);

            // Update render mesh settings.
            // TODO: Avoid redundant state changes.
            p.renderMesh.SetRenderSettings(material);
            p.renderMesh.SetMaterialValues(shader, material);

            p.renderMesh.Draw(shader);
        }

        private void SetPolygonSpecificUniforms(Polygon p, Shader shader, Material material, bool shouldDrawIdPass = false)
        {
            SetXMBUniforms(shader, p);
            SetNscUniform(p, shader);

            // The fragment alpha is set to 1 when alpha blending/testing aren't used.
            // This fixes the alpha output for PNG renders.
            shader.SetBoolToInt("isTransparent", p.IsTransparent);

            shader.SetVector3("colorId", ColorUtils.GetVector3(Color.FromArgb(p.DisplayId)));
            shader.SetBoolToInt("drawId", shouldDrawIdPass);

            shader.SetUint("flags", material.Flags);
            shader.SetFloat("zBufferOffset", material.ZBufferOffset);

            shader.SetBoolToInt("renderVertColor", Runtime.renderVertColor && material.UseVertexColor);
        }

        private void SetGlobalShaderUniforms(Shader shader, Camera camera, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
        {
            SetStageLightingUniforms(shader, lightSetNumber);

            // Misc Uniforms
            shader.SetInt("selectedBoneIndex", Runtime.selectedBoneIndex);
            shader.SetBoolToInt("drawWireFrame", Runtime.renderModelWireframe);
            shader.SetFloat("lineWidth", Runtime.wireframeLineWidth);
            shader.SetVector3("cameraPosition", camera.TransformedPosition);

            shader.SetFloat("bloomThreshold", Runtime.bloomThreshold);
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

        private void DrawModelSelection(Polygon p, Shader shader)
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

            p.renderMesh.Draw(shader);

            GL.ColorMask(cwm[0], cwm[1], cwm[2], cwm[3]);

            GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
            GL.StencilMask(0x00);

            // Override the model color with white in the shader.
            shader.SetInt("drawSelection", 1);

            GL.LineWidth(2.0f);
            p.renderMesh.SetWireFrame(true);
            p.renderMesh.Draw(shader);
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
            Shader shader = OpenTkSharedResources.shaders["Point"];
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
                        ma.ClearAnims();
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
                            float[] matHashFloat = material.GetPropertyValues("NU_materialHash");
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
                                            material.UpdatePropertyAnim(md.name, md.frames[frm].values);
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
            public short singlebind;
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
            public ushort faceCount;
            public ushort polyFlag;
            public int texprop1;
            public int texprop2;
            public int texprop3;
            public int texprop4;
        }

        public override void Read(string filename)
        {
            FileData fileData = new FileData(filename);
            fileData.endian = Endianness.Big;
            fileData.Seek(0);

            // read header
            string magic = fileData.ReadString(0, 4);
            fileData.Seek(4);
            if (magic.Equals("NDP3"))
                Endian = Endianness.Big;
            else if (magic.Equals("NDWD"))
                Endian = Endianness.Little;

            fileData.endian = Endian;
            fileData.ReadUInt(); //Filesize

            //Always read version in BE
            fileData.endian = Endianness.Big;
            version = fileData.ReadUShort();
            fileData.endian = Endian;

            int polysets = fileData.ReadUShort();
            boneIndexStart = fileData.ReadUShort();
            boneIndexEnd = fileData.ReadUShort();

            int polyClumpStart = fileData.ReadInt() + 0x30;
            int polyClumpSize = fileData.ReadInt();
            int vertClumpStart = polyClumpStart + polyClumpSize;
            int vertClumpSize = fileData.ReadInt();
            int vertaddClumpStart = vertClumpStart + vertClumpSize;
            int vertaddClumpSize = fileData.ReadInt();
            int nameStart = vertaddClumpStart + vertaddClumpSize;
            boundingSphere[0] = fileData.ReadFloat();
            boundingSphere[1] = fileData.ReadFloat();
            boundingSphere[2] = fileData.ReadFloat();
            boundingSphere[3] = fileData.ReadFloat();

            // object descriptors

            ObjectData[] obj = new ObjectData[polysets];
            List<float[]> boundingSpheres = new List<float[]>();
            int[] boneflags = new int[polysets];
            for (int i = 0; i < polysets; i++)
            {
                float[] boundingSphere = new float[8];
                boundingSphere[0] = fileData.ReadFloat();
                boundingSphere[1] = fileData.ReadFloat();
                boundingSphere[2] = fileData.ReadFloat();
                boundingSphere[3] = fileData.ReadFloat();
                boundingSphere[4] = fileData.ReadFloat();
                boundingSphere[5] = fileData.ReadFloat();
                boundingSphere[6] = fileData.ReadFloat();
                boundingSphere[7] = fileData.ReadFloat();
                boundingSpheres.Add(boundingSphere);
                int temp = fileData.Pos() + 4;
                fileData.Seek(nameStart + fileData.ReadInt());
                obj[i].name = (fileData.ReadString());
                // read name string
                fileData.Seek(temp);
                fileData.ReadUShort(); //Seems to be always 0
                boneflags[i] = fileData.ReadUShort(); //Controls whether it's single-bound, weighted, or unbound
                obj[i].singlebind = fileData.ReadShort(); //Bone index if it is single-bound, otherwise -1
                obj[i].polyCount = fileData.ReadUShort();
                obj[i].positionb = fileData.ReadInt();
            }

            // reading polygon data
            int meshIndex = 0;
            foreach (var o in obj)
            {
                Mesh m = new Mesh();
                m.Text = o.name;
                Nodes.Add(m);
                m.boneflag = boneflags[meshIndex];
                m.singlebind = o.singlebind;
                m.boundingSphere = boundingSpheres[meshIndex++];

                for (int i = 0; i < o.polyCount; i++)
                {
                    PolyData polyData = new PolyData();

                    polyData.polyStart = fileData.ReadInt() + polyClumpStart;
                    polyData.vertStart = fileData.ReadInt() + vertClumpStart;
                    polyData.verAddStart = fileData.ReadInt() + vertaddClumpStart;
                    polyData.vertCount = fileData.ReadUShort();
                    polyData.vertSize = fileData.ReadByte();
                    polyData.UVSize = fileData.ReadByte();
                    polyData.texprop1 = fileData.ReadInt();
                    polyData.texprop2 = fileData.ReadInt();
                    polyData.texprop3 = fileData.ReadInt();
                    polyData.texprop4 = fileData.ReadInt();
                    polyData.faceCount = fileData.ReadUShort();
                    polyData.polyFlag = fileData.ReadUShort();
                    fileData.Skip(0xC);

                    int temp = fileData.Pos();

                    // read vertex
                    Polygon poly = ReadVertex(fileData, polyData, o);
                    m.Nodes.Add(poly);

                    poly.materials = ReadMaterials(fileData, polyData, nameStart);

                    fileData.Seek(temp);
                }
            }
        }

        public static List<Material> ReadMaterials(FileData d, PolyData p, int nameOffset)
        {
            int propoff = p.texprop1;
            List<Material> mats = new List<Material>();

            while (propoff != 0)
            {
                d.Seek(propoff);

                Material m = new Material();
                mats.Add(m);

                m.Flags = d.ReadUInt();
                d.Skip(4);
                m.SrcFactor = d.ReadUShort();
                ushort texCount = d.ReadUShort();
                m.DstFactor = d.ReadUShort();
                m.AlphaFunc = d.ReadUShort();
                m.RefAlpha = d.ReadUShort();
                m.CullMode = d.ReadUShort();
                d.Skip(4); // unknown
                m.Unk2 = d.ReadInt();
                m.ZBufferOffset = d.ReadInt();

                for (ushort i = 0; i < texCount; i++)
                {
                    MatTexture tex = new MatTexture();
                    tex.hash = d.ReadInt();
                    d.Skip(6); // padding?
                    tex.mapMode = d.ReadUShort();
                    tex.wrapModeS = d.ReadByte();
                    tex.wrapModeT = d.ReadByte();
                    tex.minFilter = d.ReadByte();
                    tex.magFilter = d.ReadByte();
                    tex.mipDetail = d.ReadByte();
                    tex.unknown = d.ReadByte();
                    d.Skip(4); // padding?
                    tex.unknown2 = d.ReadShort();
                    m.textures.Add(tex);
                }

                int matAttSize;
                do
                {
                    int pos = d.Pos();

                    matAttSize = d.ReadInt();
                    int nameStart = d.ReadInt();
                    //valueCount has the same position regardless of endianness
                    //This could either mean that it's one byte long, or that it's always BE
                    //We assume the former, for now
                    d.Skip(3);
                    byte valueCount = d.ReadByte();
                    d.Skip(4); //Unknown, always 0, probably padding

                    //If it doesn't have any values, we want to skip over it
                    if (valueCount == 0)
                        goto Continue;

                    string name = d.ReadString(nameOffset + nameStart, -1);

                    // Material properties should always have 4 values. Use 0 for remaining values.
                    float[] values = new float[4];
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (i < valueCount)
                            values[i] = d.ReadFloat();
                        else
                            values[i] = 0;
                    }
                    m.UpdateProperty(name, values);

                Continue:
                    if (matAttSize == 0)
                        d.Seek(pos + 0x10 + (valueCount * 4));
                    else
                        d.Seek(pos + matAttSize);
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
            m.strip = (byte)(p.polyFlag >> 8);
            m.polflag = (byte)(p.polyFlag & 0xFF);

            Vertex[] vertices = new Vertex[p.vertCount];
            for (int x = 0; x < p.vertCount; x++)
                vertices[x] = new Vertex();

            d.Seek(p.vertStart);
            if (m.boneType > 0)
            {
                foreach (Vertex v in vertices)
                    ReadUV(d, m, v);
                d.Seek(p.verAddStart);
                foreach (Vertex v in vertices)
                    ReadVertex(d, m, v);
            }
            else
            {
                foreach (Vertex v in vertices)
                {
                    ReadVertex(d, m, v);
                    ReadUV(d, m, v);

                    v.boneIds.Add(o.singlebind);
                    v.boneWeights.Add(1);
                }
            }

            foreach (Vertex v in vertices)
                m.vertices.Add(v);

            // faces
            d.Seek(p.polyStart);
            for (int x = 0; x < p.faceCount; x++)
            {
                m.vertexIndices.Add(d.ReadUShort());
            }

            return m;
        }

        private static void ReadUV(FileData d, Polygon poly, Vertex v)
        {
            int uvCount = poly.uvCount;
            int colorType = poly.colorType;
            int uvType = poly.uvType;

            if (colorType == (int)Polygon.VertexColorTypes.None)
                {}
            else if (colorType == (int)Polygon.VertexColorTypes.Byte)
                v.color = new Vector4(d.ReadByte(), d.ReadByte(), d.ReadByte(), d.ReadByte());
            else if (colorType == (int)Polygon.VertexColorTypes.HalfFloat)
                v.color = new Vector4(d.ReadHalfFloat() * 0xFF, d.ReadHalfFloat() * 0xFF, d.ReadHalfFloat() * 0xFF, d.ReadHalfFloat() * 0xFF);
            else
                throw new NotImplementedException($"Unsupported vertex color type: {colorType}");

            for (int i = 0; i < uvCount; i++)
            {
                if (uvType == (int)Polygon.UVTypes.HalfFloat)
                    v.uv.Add(new Vector2(d.ReadHalfFloat(), d.ReadHalfFloat()));
                else if (uvType == (int)Polygon.UVTypes.Float)
                    v.uv.Add(new Vector2(d.ReadFloat(), d.ReadFloat()));
                else
                    throw new NotImplementedException($"Unsupported UV type: {uvType}");
            }
        }

        private static void ReadVertex(FileData d, Polygon poly, Vertex v)
        {
            int boneType = poly.boneType;
            int vertexType = poly.normalType;

            v.pos.X = d.ReadFloat();
            v.pos.Y = d.ReadFloat();
            v.pos.Z = d.ReadFloat();

            if (vertexType == (int)Polygon.VertexTypes.NoNormals)
            {
                d.ReadFloat();
            }
            else if (vertexType == (int)Polygon.VertexTypes.NormalsFloat)
            {
                d.ReadFloat(); // Always set to 100.0 in a Tekken model?
                v.nrm.X = d.ReadFloat();
                v.nrm.Y = d.ReadFloat();
                v.nrm.Z = d.ReadFloat();
                d.ReadFloat(); // n1?
            }
            else if (vertexType == 2) // This one needs verification
            {
                d.ReadFloat(); // What is this?
                v.nrm.X = d.ReadFloat();
                v.nrm.Y = d.ReadFloat();
                v.nrm.Z = d.ReadFloat();
                d.ReadFloat(); // n1?
                v.bitan.X = d.ReadFloat();
                v.bitan.Y = d.ReadFloat();
                v.bitan.Z = d.ReadFloat();
                v.bitan.W = d.ReadFloat();
                v.tan.X = d.ReadFloat();
                v.tan.Y = d.ReadFloat();
                v.tan.Z = d.ReadFloat();
                v.tan.W = d.ReadFloat();
            }
            else if (vertexType == (int)Polygon.VertexTypes.NormalsTanBiTanFloat)
            {
                d.ReadFloat(); // What is this?
                v.nrm.X = d.ReadFloat();
                v.nrm.Y = d.ReadFloat();
                v.nrm.Z = d.ReadFloat();
                d.ReadFloat(); // n1?
                v.bitan.X = d.ReadFloat();
                v.bitan.Y = d.ReadFloat();
                v.bitan.Z = d.ReadFloat();
                v.bitan.W = d.ReadFloat();
                v.tan.X = d.ReadFloat();
                v.tan.Y = d.ReadFloat();
                v.tan.Z = d.ReadFloat();
                v.tan.W = d.ReadFloat();
            }
            else if (vertexType == (int)Polygon.VertexTypes.NormalsHalfFloat)
            {
                v.nrm.X = d.ReadHalfFloat();
                v.nrm.Y = d.ReadHalfFloat();
                v.nrm.Z = d.ReadHalfFloat();
                d.ReadHalfFloat(); // n1?
            }
            else if (vertexType == (int)Polygon.VertexTypes.NormalsTanBiTanHalfFloat)
            {
                v.nrm.X = d.ReadHalfFloat();
                v.nrm.Y = d.ReadHalfFloat();
                v.nrm.Z = d.ReadHalfFloat();
                d.ReadHalfFloat(); // n1?
                v.bitan.X = d.ReadHalfFloat();
                v.bitan.Y = d.ReadHalfFloat();
                v.bitan.Z = d.ReadHalfFloat();
                v.bitan.W = d.ReadHalfFloat();
                v.tan.X = d.ReadHalfFloat();
                v.tan.Y = d.ReadHalfFloat();
                v.tan.Z = d.ReadHalfFloat();
                v.tan.W = d.ReadHalfFloat();
            }
            else
            {
                throw new Exception($"Unsupported vertex type: {vertexType}");
            }

            if (boneType == (int)Polygon.BoneTypes.NoBones)
            {
            }
            else if (boneType == (int)Polygon.BoneTypes.Float)
            {
                v.boneIds.Add(d.ReadInt());
                v.boneIds.Add(d.ReadInt());
                v.boneIds.Add(d.ReadInt());
                v.boneIds.Add(d.ReadInt());
                v.boneWeights.Add(d.ReadFloat());
                v.boneWeights.Add(d.ReadFloat());
                v.boneWeights.Add(d.ReadFloat());
                v.boneWeights.Add(d.ReadFloat());
            }
            else if (boneType == (int)Polygon.BoneTypes.HalfFloat)
            {
                v.boneIds.Add(d.ReadUShort());
                v.boneIds.Add(d.ReadUShort());
                v.boneIds.Add(d.ReadUShort());
                v.boneIds.Add(d.ReadUShort());
                v.boneWeights.Add(d.ReadHalfFloat());
                v.boneWeights.Add(d.ReadHalfFloat());
                v.boneWeights.Add(d.ReadHalfFloat());
                v.boneWeights.Add(d.ReadHalfFloat());
            }
            else if (boneType == (int)Polygon.BoneTypes.Byte)
            {
                v.boneIds.Add(d.ReadByte());
                v.boneIds.Add(d.ReadByte());
                v.boneIds.Add(d.ReadByte());
                v.boneIds.Add(d.ReadByte());
                v.boneWeights.Add((float)d.ReadByte() / 255);
                v.boneWeights.Add((float)d.ReadByte() / 255);
                v.boneWeights.Add((float)d.ReadByte() / 255);
                v.boneWeights.Add((float)d.ReadByte() / 255);
            }
            else
            {
                throw new Exception($"Unsupported bone type: {boneType}");
            }
        }

        public override byte[] Rebuild()
        {
            FileOutput d = new FileOutput(); // data
            d.endian = Endianness.Big;
            if (Endian == Endianness.Big)
                d.WriteString("NDP3");
            else if (Endian == Endianness.Little)
                d.WriteString("NDWD");

            d.endian = Endian;
            d.WriteInt(0); //Filesize

            //Always write version in BE
            d.endian = Endianness.Big;
            d.WriteUShort(version);
            d.endian = Endian;

            d.WriteShort(Nodes.Count); // polysets

            //TODO: Calculate these values properly
            int boneCount = ((ModelContainer)Parent).VBN == null ? 0 : ((ModelContainer)Parent).VBN.bones.Count;
            if (boneCount > 0) {
                boneIndexEnd = (ushort)(boneCount - 1);
                boneIndexStart = Math.Min(boneIndexEnd, boneIndexStart);
            }
            else {
                boneIndexStart = 0;
                boneIndexEnd = 0;
            }
            d.WriteUShort(boneIndexStart);
            d.WriteUShort(boneIndexEnd);

            d.WriteInt(0); // polyClump start
            d.WriteInt(0); // polyClump size
            d.WriteInt(0); // vertexClump size
            d.WriteInt(0); // vertexaddclump size

            d.WriteFloat(boundingSphere[0]);
            d.WriteFloat(boundingSphere[1]);
            d.WriteFloat(boundingSphere[2]);
            d.WriteFloat(boundingSphere[3]);

            // other sections....
            FileOutput obj = new FileOutput();
            obj.endian = Endian;
            FileOutput tex = new FileOutput();
            tex.endian = Endian;

            FileOutput poly = new FileOutput();
            poly.endian = Endian;
            FileOutput vert = new FileOutput();
            vert.endian = Endian;
            FileOutput vertadd = new FileOutput();
            vertadd.endian = Endian;

            FileOutput str = new FileOutput();
            str.endian = Endian;

            // obj descriptor
            FileOutput tempstring = new FileOutput();
            for (int i = 0; i < Nodes.Count; i++)
            {
                str.WriteString(Nodes[i].Text);
                str.WriteByte(0);
                str.Align(16);
            }

            int polyCount = 0; // counting number of poly
            foreach (Mesh m in Nodes)
                polyCount += m.Nodes.Count;

            foreach (Mesh m in Nodes)
            {
                foreach (float f in m.boundingSphere)
                    d.WriteFloat(f);

                d.WriteInt(tempstring.Size());

                tempstring.WriteString(m.Text);
                tempstring.WriteByte(0);
                tempstring.Align(16);

                d.WriteUShort(0);
                d.WriteUShort((ushort)m.boneflag); // Bind method
                d.WriteShort(m.singlebind); // Bone index
                d.WriteShort(m.Nodes.Count); // poly count
                d.WriteInt(obj.Size() + 0x30 + Nodes.Count * 0x30); // position start for obj

                // write obj info here...
                foreach (Polygon p in m.Nodes)
                {
                    obj.WriteInt(poly.Size());
                    obj.WriteInt(vert.Size());
                    obj.WriteInt(p.boneType > 0 ? vertadd.Size() : 0);
                    obj.WriteShort(p.vertices.Count);
                    obj.WriteByte(p.vertSize);

                    int maxUV = p.uvCount;
                    foreach (Vertex v in p.vertices) {
                        maxUV = v.uv.Count;
                        break;
                    }
                    p.uvCount = maxUV;
                    obj.WriteByte(p.UVSize);

                    // MATERIAL SECTION 
                    int[] texoff = WriteMaterial(tex, p.materials, str);

                    obj.WriteInt(texoff[0] + 0x30 + Nodes.Count * 0x30 + polyCount * 0x30);
                    obj.WriteInt(texoff[1] > 0 ? texoff[1] + 0x30 + Nodes.Count * 0x30 + polyCount * 0x30 : 0);
                    obj.WriteInt(texoff[2] > 0 ? texoff[2] + 0x30 + Nodes.Count * 0x30 + polyCount * 0x30 : 0);
                    obj.WriteInt(texoff[3] > 0 ? texoff[3] + 0x30 + Nodes.Count * 0x30 + polyCount * 0x30 : 0);

                    obj.WriteUShort((ushort)p.vertexIndices.Count); // polyamt
                    obj.WriteUShort((ushort)(p.strip << 8 | p.polflag));

                    obj.WriteInt(0); // idk, nothing padding??
                    obj.WriteInt(0);
                    obj.WriteInt(0);

                    // Write the poly...
                    foreach (int face in p.vertexIndices)
                        poly.WriteShort(face);

                    // Write the vertex....
                    if (p.boneType > 0)
                    {
                        foreach (Vertex v in p.vertices)
                        {
                            WriteUV(vert, p, v);
                            WriteVertex(vertadd, p, v);
                        }
                        vertadd.Align(4, 0x0);
                    }
                    else
                    {
                        foreach (Vertex v in p.vertices)
                        {
                            WriteVertex(vert, p, v);
                            WriteUV(vert, p, v);
                        }
                    }
                }
            }

            d.WriteOutput(obj);
            d.WriteOutput(tex);
            d.Align(16);

            d.WriteIntAt(d.Size() - 0x30, 0x10);
            d.WriteIntAt(poly.Size(), 0x14);
            d.WriteIntAt(vert.Size(), 0x18);
            d.WriteIntAt(vertadd.Size(), 0x1c);

            d.WriteOutput(poly);

            int s = d.Size();
            d.Align(16);
            s = d.Size() - s;
            d.WriteIntAt(poly.Size() + s, 0x14);

            d.WriteOutput(vert);

            s = d.Size();
            d.Align(16);
            s = d.Size() - s;
            d.WriteIntAt(vert.Size() + s, 0x18);

            d.WriteOutput(vertadd);

            s = d.Size();
            d.Align(16);
            s = d.Size() - s;
            d.WriteIntAt(vertadd.Size() + s, 0x1c);

            d.WriteOutput(str);

            d.WriteIntAt(d.Size(), 0x4);

            return d.GetBytes();
        }

        private static void WriteUV(FileOutput d, Polygon poly, Vertex v)
        {
            int uvCount = poly.uvCount;
            int colorType = poly.colorType;
            int uvType = poly.uvType;

            if (colorType == (int)Polygon.VertexColorTypes.None)
            {
            }
            else if (colorType == (int)Polygon.VertexColorTypes.Byte)
            {
                d.WriteByte((int)v.color.X);
                d.WriteByte((int)v.color.Y);
                d.WriteByte((int)v.color.Z);
                d.WriteByte((int)v.color.W);
            }
            else if (colorType == (int)Polygon.VertexColorTypes.HalfFloat)
            {
                d.WriteHalfFloat(v.color.X / 0xFF);
                d.WriteHalfFloat(v.color.Y / 0xFF);
                d.WriteHalfFloat(v.color.Z / 0xFF);
                d.WriteHalfFloat(v.color.W / 0xFF);
            }
            else
            {
                throw new NotImplementedException($"Unsupported vertex color type: {colorType}");
            }

            for (int i = 0; i < uvCount; i++)
            {
                if (uvType == (int)Polygon.UVTypes.HalfFloat)
                {
                    d.WriteHalfFloat(v.uv[i].X);
                    d.WriteHalfFloat(v.uv[i].Y);
                }
                else if (uvType == (int)Polygon.UVTypes.Float)
                {
                    d.WriteFloat(v.uv[i].X);
                    d.WriteFloat(v.uv[i].Y);
                }
                else
                {
                    throw new NotImplementedException($"Unsupported UV type: {uvType}");
                }
            }
        }

        private static void WriteVertex(FileOutput d, Polygon poly, Vertex v)
        {
            int boneType = poly.boneType;
            int vertexType = poly.normalType;

            d.WriteFloat(v.pos.X);
            d.WriteFloat(v.pos.Y);
            d.WriteFloat(v.pos.Z);

            if (vertexType == (int)Polygon.VertexTypes.NoNormals)
            {
                d.WriteFloat(1);
            }
            else if (vertexType == (int)Polygon.VertexTypes.NormalsFloat)
            {
                d.WriteFloat(100);
                d.WriteFloat(v.nrm.X);
                d.WriteFloat(v.nrm.Y);
                d.WriteFloat(v.nrm.Z);
                d.WriteFloat(1);
            }
            else if (vertexType == 2)
            {
                d.WriteFloat(1);
                d.WriteFloat(v.nrm.X);
                d.WriteFloat(v.nrm.Y);
                d.WriteFloat(v.nrm.Z);
                d.WriteFloat(1);
                d.WriteFloat(v.bitan.X);
                d.WriteFloat(v.bitan.Y);
                d.WriteFloat(v.bitan.Z);
                d.WriteFloat(1);
                d.WriteFloat(v.tan.X);
                d.WriteFloat(v.tan.Y);
                d.WriteFloat(v.tan.Z);
                d.WriteFloat(1);
            }
            else if (vertexType == (int)Polygon.VertexTypes.NormalsTanBiTanFloat)
            {
                d.WriteFloat(1);
                d.WriteFloat(v.nrm.X);
                d.WriteFloat(v.nrm.Y);
                d.WriteFloat(v.nrm.Z);
                d.WriteFloat(1);
                d.WriteFloat(v.bitan.X);
                d.WriteFloat(v.bitan.Y);
                d.WriteFloat(v.bitan.Z);
                d.WriteFloat(v.bitan.W);
                d.WriteFloat(v.tan.X);
                d.WriteFloat(v.tan.Y);
                d.WriteFloat(v.tan.Z);
                d.WriteFloat(v.tan.W);
            }
            else if (vertexType == (int)Polygon.VertexTypes.NormalsHalfFloat)
            {
                d.WriteHalfFloat(v.nrm.X);
                d.WriteHalfFloat(v.nrm.Y);
                d.WriteHalfFloat(v.nrm.Z);
                d.WriteHalfFloat(1);
            }
            else if (vertexType == (int)Polygon.VertexTypes.NormalsTanBiTanHalfFloat)
            {
                d.WriteHalfFloat(v.nrm.X);
                d.WriteHalfFloat(v.nrm.Y);
                d.WriteHalfFloat(v.nrm.Z);
                d.WriteHalfFloat(1);
                d.WriteHalfFloat(v.bitan.X);
                d.WriteHalfFloat(v.bitan.Y);
                d.WriteHalfFloat(v.bitan.Z);
                d.WriteHalfFloat(v.bitan.W);
                d.WriteHalfFloat(v.tan.X);
                d.WriteHalfFloat(v.tan.Y);
                d.WriteHalfFloat(v.tan.Z);
                d.WriteHalfFloat(v.tan.W);
            }
            else
            {
                throw new Exception($"Unsupported vertex type: {vertexType}");
            }

            if (boneType == (int)Polygon.BoneTypes.NoBones)
            {
            }
            else if (boneType == (int)Polygon.BoneTypes.Float)
            {
                d.WriteInt(v.boneIds.Count > 0 ? v.boneIds[0] : 0);
                d.WriteInt(v.boneIds.Count > 1 ? v.boneIds[1] : 0);
                d.WriteInt(v.boneIds.Count > 2 ? v.boneIds[2] : 0);
                d.WriteInt(v.boneIds.Count > 3 ? v.boneIds[3] : 0);
                d.WriteFloat(v.boneWeights.Count > 0 ? v.boneWeights[0] : 0);
                d.WriteFloat(v.boneWeights.Count > 1 ? v.boneWeights[1] : 0);
                d.WriteFloat(v.boneWeights.Count > 2 ? v.boneWeights[2] : 0);
                d.WriteFloat(v.boneWeights.Count > 3 ? v.boneWeights[3] : 0);
            }
            else if (boneType == (int)Polygon.BoneTypes.HalfFloat)
            {
                d.WriteShort(v.boneIds.Count > 0 ? v.boneIds[0] : 0);
                d.WriteShort(v.boneIds.Count > 1 ? v.boneIds[1] : 0);
                d.WriteShort(v.boneIds.Count > 2 ? v.boneIds[2] : 0);
                d.WriteShort(v.boneIds.Count > 3 ? v.boneIds[3] : 0);
                d.WriteHalfFloat(v.boneWeights.Count > 0 ? v.boneWeights[0] : 0);
                d.WriteHalfFloat(v.boneWeights.Count > 1 ? v.boneWeights[1] : 0);
                d.WriteHalfFloat(v.boneWeights.Count > 2 ? v.boneWeights[2] : 0);
                d.WriteHalfFloat(v.boneWeights.Count > 3 ? v.boneWeights[3] : 0);
            }
            else if (boneType == (int)Polygon.BoneTypes.Byte)
            {
                d.WriteByte(v.boneIds.Count > 0 ? v.boneIds[0] : 0);
                d.WriteByte(v.boneIds.Count > 1 ? v.boneIds[1] : 0);
                d.WriteByte(v.boneIds.Count > 2 ? v.boneIds[2] : 0);
                d.WriteByte(v.boneIds.Count > 3 ? v.boneIds[3] : 0);
                d.WriteByte((int)(v.boneWeights.Count > 0 ? Math.Round(v.boneWeights[0] * 0xFF) : 0));
                d.WriteByte((int)(v.boneWeights.Count > 1 ? Math.Round(v.boneWeights[1] * 0xFF) : 0));
                d.WriteByte((int)(v.boneWeights.Count > 2 ? Math.Round(v.boneWeights[2] * 0xFF) : 0));
                d.WriteByte((int)(v.boneWeights.Count > 3 ? Math.Round(v.boneWeights[3] * 0xFF) : 0));
            }
            else
            {
                throw new Exception($"Unsupported bone type: {boneType}");
            }
        }

        public static int[] WriteMaterial(FileOutput d, List<Material> materials, FileOutput str)
        {
            int[] offs = new int[4];
            int c = 0;
            foreach (Material mat in materials)
            {
                offs[c++] = d.Size();
                d.WriteInt((int)mat.Flags);
                d.WriteInt(0); // padding
                d.WriteShort(mat.SrcFactor);
                d.WriteShort(mat.textures.Count);
                d.WriteShort(mat.DstFactor);
                d.WriteShort(mat.AlphaFunc);
                d.WriteShort(mat.RefAlpha);
                d.WriteShort(mat.CullMode);
                d.WriteInt(0); // unknown
                d.WriteInt(mat.Unk2);
                d.WriteInt(mat.ZBufferOffset);

                foreach (MatTexture tex in mat.textures)
                {
                    d.WriteInt(tex.hash);
                    d.WriteInt(0);
                    d.WriteShort(0);
                    d.WriteShort(tex.mapMode);
                    d.WriteByte(tex.wrapModeS);
                    d.WriteByte(tex.wrapModeT);
                    d.WriteByte(tex.minFilter);
                    d.WriteByte(tex.magFilter);
                    d.WriteByte(tex.mipDetail);
                    d.WriteByte(tex.unknown);
                    d.WriteInt(0); // padding
                    d.WriteShort(tex.unknown2);
                }

                //If there are no material attributes, write a "blank" entry
                if (mat.PropertyCount == 0)
                {
                    d.WriteInt(0);
                    d.WriteInt(0);
                    d.WriteInt(0);
                    d.WriteInt(0);
                }

                for (int i = 0; i < mat.PropertyCount; i++)
                {
                    //It can be seen in Pokkén NDWD that the last material attribute name
                    // does not need to be aligned to 16. So, we do the alignment before writing
                    // the name rather than after.
                    str.Align(16);

                    float[] data = mat.GetPropertyValues(mat.PropertyNames.ElementAt(i));
                    d.WriteInt(i == mat.PropertyCount - 1 ? 0 : 16 + 4 * data.Length);
                    d.WriteInt(str.Size());

                    str.WriteString(mat.PropertyNames.ElementAt(i));
                    str.WriteByte(0);

                    d.WriteByte(0); d.WriteByte(0); d.WriteByte(0);
                    d.WriteByte((byte)data.Length);
                    d.WriteInt(0);
                    foreach (float f in data)
                        d.WriteFloat(f);
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
            MBN m = new MBN();

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

        private static void SingleBindMesh(Mesh m, int singleBindBone)
        {
            m.boneflag = (int)Mesh.BoneFlags.SingleBind;
            m.singlebind = (short)singleBindBone;
            foreach (Polygon p in m.Nodes)
            {
                p.boneType = 0;
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

