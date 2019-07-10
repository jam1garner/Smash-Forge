using MeleeLib.DAT;
using MeleeLib.DAT.Helpers;
using MeleeLib.GCX;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel.Utils;
using SFGenericModel;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.Shaders;
using Smash_Forge.Filetypes.Melee.Rendering;
using Smash_Forge.GUI.Melee;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Smash_Forge.Filetypes.Melee.Utils;
using SFGenericModel.RenderState;

namespace Smash_Forge.Filetypes.Melee
{
    public class MeleeDataObjectNode : MeleeNode
    {
        public DatDOBJ DOBJ;

        // For Rendering Only
        private List<MeleeMesh> renderMeshes = new List<MeleeMesh>();
        private List<MeleeRenderTexture> renderTextures = new List<MeleeRenderTexture>();

        public int BoneIndex;

        // for importing
        public List<GXVertex[]> VertsToImport;

        public MeleeDataObjectNode(DatDOBJ DOBJ)
        {
            ImageKey = "mesh";
            SelectedImageKey = "mesh";
            this.DOBJ = DOBJ;
            Checked = true;

            ContextMenu = new ContextMenu();

            MenuItem Edit = new MenuItem("Edit");
            Edit.Click += OpenEditor;
            ContextMenu.MenuItems.Add(Edit);

            MenuItem Clear = new MenuItem("Clear Polygons");
            Clear.Click += ClearPolygons;
            ContextMenu.MenuItems.Add(Clear);

            MenuItem smd = new MenuItem("Import from File");
            smd.Click += ImportModel;
            ContextMenu.MenuItems.Add(smd);
        }

        public void OpenEditor(object sender, EventArgs args)
        {
            DOBJEditor editor = new DOBJEditor(DOBJ, this);
            editor.Show();
        }

        public void ClearPolygons(object sender, EventArgs args)
        {
            DOBJ.Polygons.Clear();
        }

        public void ImportModel(object sender, EventArgs args)
        {
            using (DOBJImportSettings import = new DOBJImportSettings(this))
            {
                import.ShowDialog();
                if (import.exitStatus == DOBJImportSettings.ExitStatus.Opened)
                {
                    GetDatFile().RecompileVertices();

                }
            }
        }

        public void GetVerticesAsTriangles(out int[] indices, out List<GXVertex> Verts)
        {
            Verts = new List<GXVertex>();
            List<int> ind = new List<int>();

            VBN Bones = GetRoot().RenderBones;
            GXVertexDecompressor decompressor = new GXVertexDecompressor(GetRoot().Root);

            int index = 0;
            foreach (DatPolygon p in DOBJ.Polygons)
            {
                foreach (GXDisplayList dl in p.DisplayLists)
                {
                    GXVertex[] verts = decompressor.GetFormattedVertices(dl, p);
                    for (int i = 0; i < verts.Length; i++)
                    {
                        if (verts[i].N != null && verts[i].N.Length == 1)
                        {
                            /*Vector3 ToTransform = Vector3.TransformPosition(new Vector3(verts[i].Pos.X, verts[i].Pos.Y, verts[i].Pos.Z), Bones.bones[verts[i].N[0]].transform);
                            verts[i].Pos.X = ToTransform.X;
                            verts[i].Pos.Y = ToTransform.Y;
                            verts[i].Pos.Z = ToTransform.Z;
                            Vector3 ToTransformN = Vector3.TransformNormal(new Vector3(verts[i].Nrm.X, verts[i].Nrm.Y, verts[i].Nrm.Z), Bones.bones[verts[i].N[0]].transform);
                            verts[i].Nrm.X = ToTransformN.X;
                            verts[i].Nrm.Y = ToTransformN.Y;
                            verts[i].Nrm.Z = ToTransformN.Z;*/
                        }
                        // TODO: Transform by attached jobj
                    }
                    Verts.AddRange(verts);

                    List<int> indi = new List<int>();
                    for (int i = 0; i < dl.Indices.Length; i++)
                    {
                        indi.Add(index + i);
                    }
                    switch (dl.PrimitiveType)
                    {
                        case GXPrimitiveType.TriangleStrip: ind.AddRange(TriangleTools.fromTriangleStrip(indi)); break;
                        case GXPrimitiveType.Quads: ind.AddRange(TriangleTools.fromQuad(indi)); break;
                        case GXPrimitiveType.Triangles: ind.AddRange(indi); break;
                        default:
                            Console.WriteLine("Warning: unsupported primitive type " + dl.PrimitiveType.ToString());
                            ind.AddRange(indi);
                            break;
                    }
                    index += indi.Count;
                }
            }

            indices = ind.ToArray();
        }

        public void RecompileVertices(GXVertexDecompressor decompressor, GXVertexCompressor compressor)
        {
            if (VertsToImport != null)
            {

                for (int p = 0; p < VertsToImport.Count; p++)
                {
                    if (p >= DOBJ.Polygons.Count)
                    {
                        MessageBox.Show("Error injecting vertices into DOBJ: Not enough polygons");
                        return;
                    }
                    DatPolygon poly = DOBJ.Polygons[p];
                    List<GXDisplayList> newDL = new List<GXDisplayList>();

                    // maximize vertex groups
                    int size = 3;

                    for (int i = 0; i < VertsToImport[p].Length; i += size)
                    {
                        List<GXVertex> VertList = new List<GXVertex>();
                        for (int j = 0; j < size; j += 3)
                        {
                            VertList.AddRange(new GXVertex[] { VertsToImport[p][i + j + 2], VertsToImport[p][i + j + 1], VertsToImport[p][i + j] });
                        }
                        newDL.Add(compressor.CompressDisplayList(
                            VertList.ToArray(),
                                GXPrimitiveType.Triangles,
                                poly.AttributeGroup));
                    }
                    poly.DisplayLists = newDL;
                }
                VertsToImport = null;
            }
            else
            {
                foreach (DatPolygon p in DOBJ.Polygons)
                {
                    List<GXDisplayList> newDL = new List<GXDisplayList>();
                    foreach (GXDisplayList dl in p.DisplayLists)
                    {
                        newDL.Add(compressor.CompressDisplayList(
                            decompressor.GetFormattedVertices(dl, p),
                            dl.PrimitiveType,
                            p.AttributeGroup));
                    }
                    p.DisplayLists = newDL;
                }
            }
        }

        public void Render(Camera c, Shader shader)
        {
            SetShaderUniforms(shader);

            if (Checked)
            {
                foreach (var m in renderMeshes)
                {
                    if (IsSelected)
                        DrawModelSelection(m, shader);
                    else
                        m.Draw(shader);
                }
            }
        }

        private void SetShaderUniforms(Shader shader)
        {
            shader.SetInt("BoneIndex", BoneIndex);

            SetTextureUniforms(shader);

            shader.SetInt("flags", DOBJ.Material.Flags);
            shader.SetBoolToInt("enableSpecular", IsSpecularBitSet());
            shader.SetBoolToInt("enableDiffuseLighting", IsDiffuseLightingBitSet());

            SetRgbaColor(shader, "ambientColor", DOBJ.Material.MaterialColor.AMB);
            SetRgbaColor(shader, "diffuseColor", DOBJ.Material.MaterialColor.DIF);
            SetRgbaColor(shader, "specularColor", DOBJ.Material.MaterialColor.SPC);

            shader.SetFloat("glossiness", DOBJ.Material.MaterialColor.Glossiness);
            shader.SetFloat("transparency", DOBJ.Material.MaterialColor.Transparency);
        }

        public RenderSettings GetRenderSettings()
        {
            RenderSettings renderSettings = new RenderSettings();
            if (DOBJ.Material == null)
                return renderSettings;

            SetAlphaTesting(DOBJ, renderSettings);
            SetAlphaBlending(DOBJ, renderSettings);

            return renderSettings;
        }

        private void SetAlphaBlending(DatDOBJ datDOBJ, RenderSettings renderSettings)
        {
            if (datDOBJ?.Material.PixelProcessing != null)
            {
                renderSettings.alphaBlendSettings.enabled = datDOBJ?.Material?.PixelProcessing.BlendMode == MeleeLib.GCX.GXBlendMode.Blend;
            }
        }

        private void SetAlphaTesting(DatDOBJ datDOBJ, RenderSettings renderSettings)
        {
            bool enabled = (datDOBJ.Material.Flags & (uint)MeleeDatEnums.MiscFlags.AlphaTest) > 0;
            float refAlpha = AlphaTestSettings.Default.referenceAlpha;
            AlphaFunction alphaFunction = AlphaTestSettings.Default.alphaFunction;
            if (datDOBJ?.Material.PixelProcessing != null)
            {
                refAlpha = datDOBJ.Material.PixelProcessing.AlphaRef0 / 255.0f;
                alphaFunction = MeleeDatToOpenGL.GetAlphaFunction(datDOBJ.Material.PixelProcessing.AlphaComp0);
            }

            renderSettings.alphaTestSettings = new AlphaTestSettings(enabled, alphaFunction, refAlpha);
        }

        private bool IsDiffuseLightingBitSet()
        {
            return (DOBJ.Material.Flags & 0x4) > 0;
        }

        private bool IsSpecularBitSet()
        {
            return (DOBJ.Material.Flags & 0x8) > 0;
        }

        private void SetTextureUniforms(Shader shader)
        {
            // Set default values
            shader.SetVector2("diffuseScale", new Vector2(1, 1));
            shader.SetVector2("bumpMapScale", new Vector2(1, 1));
            shader.SetVector2("specularScale", new Vector2(1, 1));

            shader.SetTexture("diffuseTex0", Smash_Forge.Rendering.RenderTools.defaultTex, 0);
            shader.SetTexture("diffuseTex1", Smash_Forge.Rendering.RenderTools.defaultTex, 1);
            shader.SetTexture("bumpMapTex", Smash_Forge.Rendering.RenderTools.defaultTex, 2);
            shader.SetTexture("specularTex", Smash_Forge.Rendering.RenderTools.defaultTex, 3);

            bool hasBumpMap = false;
            bool hasSpecular = false;
            shader.SetBoolToInt("hasSphere0", false);
            shader.SetBoolToInt("hasSphere1", false);

            // TODO: Use a more general solution, so any texture can have sphere coords.
            int diffuseCount = 0;
            foreach (var renderTex in renderTextures)
            {
                if (IsFlagSet(renderTex.Flag, (uint)MeleeDatEnums.TextureFlag.BumpMap))
                {
                    hasBumpMap = true;
                    SetBumpMapTexUniforms(shader, renderTex);
                }

                if (IsDiffuseBitSet(renderTex))
                {
                    SetDiffuseTexUniforms(shader, renderTex, diffuseCount);
                    diffuseCount++;
                }

                if (IsFlagSet(renderTex.Flag, (uint)MeleeDatEnums.TextureFlag.Specular))
                {
                    hasSpecular = true;
                    SetSpecularTexUniforms(shader, renderTex);
                }
            }

            shader.SetBoolToInt("hasDiffuse0", diffuseCount > 0);
            shader.SetBoolToInt("hasDiffuse1", diffuseCount > 1);
            shader.SetBoolToInt("hasBumpMap", hasBumpMap);
            shader.SetBoolToInt("hasSpecular", hasSpecular);
        }

        private static bool IsFlagSet(uint value, uint flag)
        {
            return (value & flag) > 0;
        }

        private static bool IsDiffuseBitSet(MeleeRenderTexture renderTex)
        {
            return IsFlagSet(renderTex.Flag, (uint)MeleeDatEnums.TextureFlag.Diffuse) || IsFlagSet(renderTex.Flag, (uint)MeleeDatEnums.TextureFlag.Unk4);
        }

        private static void SetBumpMapTexUniforms(Shader shader, MeleeRenderTexture renderTex)
        {
            shader.SetVector2("bumpMapTexScale", new Vector2(renderTex.WScale, renderTex.HScale));
            shader.SetInt("bumpMapWidth", renderTex.texture.Width);
            shader.SetInt("bumpMapHeight", renderTex.texture.Height);
            shader.SetTexture("bumpMapTex", renderTex.texture, 2);
        }

        private static void SetDiffuseTexUniforms(Shader shader, MeleeRenderTexture renderTex, int index)
        {
            shader.SetBoolToInt($"hasSphere{index}", IsFlagSet(renderTex.Flag, (uint)MeleeDatEnums.TexCoordsFlag.SphereMap));
            shader.SetVector2($"diffuseScale{index}", new Vector2(renderTex.WScale, renderTex.HScale));
            shader.SetTexture($"diffuseTex{index}", renderTex.texture, index);
        }

        private static void SetSpecularTexUniforms(Shader shader, MeleeRenderTexture renderTex)
        {
            shader.SetVector2("specularScale", new Vector2(renderTex.WScale, renderTex.HScale));
            shader.SetTexture("specularTex", renderTex.texture, 3);
        }

        private static void DrawModelSelection(MeleeMesh mesh, Shader shader)
        {
            //This part needs to be reworked for proper outline. Currently would make model disappear

            mesh.Draw(shader);

            GL.Enable(EnableCap.StencilTest);
            // use vertex color for wireframe color
            shader.SetInt("colorOverride", 1);
            GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.5f);

            mesh.Draw(shader);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            shader.SetInt("colorOverride", 0);

            GL.Enable(EnableCap.DepthTest);
        }

        public void SetRgbaColor(Shader shader, string name, Color color)
        {
            shader.SetVector4(name, SFGraphics.Utils.ColorUtils.GetVector4(color));
        }

        public void RefreshRendering()
        {
            RefreshRenderTextures();
            RefreshRenderMeshes();
        }

        private void RefreshRenderMeshes()
        {
            renderMeshes.Clear();
            GXVertexDecompressor decom = new GXVertexDecompressor(GetRoot().Root);

            var vertexContainers = new List<IndexedVertexData<MeleeVertex>>();

            // Each display list can have a different primitive type, so we need to generate a lot of containers.
            foreach (DatPolygon polygon in DOBJ.Polygons)
            {
                foreach (GXDisplayList displayList in polygon.DisplayLists)
                {
                    AddVertexContainer(decom, vertexContainers, polygon, displayList);
                }
            }

            // Combine vertex containers with the same primitive type.
            // The optimization doesn't work properly for all primitive types yet.
            GroupContainersCreateRenderMeshes(vertexContainers);
        }

        private static void AddVertexContainer(GXVertexDecompressor decom, List<IndexedVertexData<MeleeVertex>> vertexContainers, DatPolygon polygon, GXDisplayList displayList)
        {
            List<MeleeVertex> vertices = new List<MeleeVertex>();
            List<int> vertexIndices = new List<int>();

            for (int i = 0; i < displayList.Indices.Length; i++)
            {
                vertexIndices.Add(i);
            }

            vertices.AddRange(ConvertVerts(decom.GetFormattedVertices(displayList, polygon)));

            PrimitiveType primitiveType = MeleeDatToOpenGL.GetGLPrimitiveType(displayList.PrimitiveType);
            var vertexContainer = new IndexedVertexData<MeleeVertex>(vertices, vertexIndices, primitiveType);
            vertexContainers.Add(vertexContainer);
        }

        private void GroupContainersCreateRenderMeshes(List<IndexedVertexData<MeleeVertex>> vertexContainers)
        {
            var optimizedContainers = MeshBatchUtils.GroupContainersByPrimitiveType(vertexContainers);
            foreach (var container in optimizedContainers)
            {
                MeleeMesh meleeMesh = new MeleeMesh(container.Vertices, container.Indices, container.PrimitiveType);
                renderMeshes.Add(meleeMesh);
            }
        }

        public void RefreshRenderTextures()
        {
            renderTextures.Clear();

            foreach (DatTexture t in DOBJ.Material.Textures)
            {
                MeleeRenderTexture tex = new MeleeRenderTexture(t);
                tex.Flag = t.UnkFlags;
                renderTextures.Add(tex);
            }
        }

        public static List<MeleeVertex> ConvertVerts(GXVertex[] Verts)
        {
            List<MeleeVertex> o = new List<MeleeVertex>();
            foreach (GXVertex v in Verts)
            {
                MeleeVertex vert = new MeleeVertex()
                {
                    Pos = new Vector3(v.Pos.X, v.Pos.Y, v.Pos.Z),
                    Nrm = new Vector3(v.Nrm.X, v.Nrm.Y, v.Nrm.Z),
                    Bit = new Vector3(v.Bit.X, v.Bit.Y, v.Bit.Z),
                    Tan = new Vector3(v.Tan.X, v.Tan.Y, v.Tan.Z),
                    Clr = new Vector4(v.CLR0.X, v.CLR0.Y, v.CLR0.Z, v.CLR0.W),
                    UV0 = new Vector2(v.TX0.X, v.TX0.Y),
                };

                if (v.N != null)
                {
                    if (v.N.Length > 0)
                    {
                        vert.Bone.X = v.N[0];
                        vert.Weight.X = v.W[0];
                    }
                    if (v.N.Length > 1)
                    {
                        vert.Bone.Y = v.N[1];
                        vert.Weight.Y = v.W[1];
                    }
                    if (v.N.Length > 2)
                    {
                        vert.Bone.Z = v.N[2];
                        vert.Weight.Z = v.W[2];
                    }
                    if (v.N.Length > 3)
                    {
                        vert.Bone.W = v.N[3];
                        vert.Weight.W = v.W[3];
                    }
                }
                o.Add(vert);
            }
            return o;
        }
    }
}