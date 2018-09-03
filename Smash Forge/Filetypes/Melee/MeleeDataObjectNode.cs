using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using MeleeLib.DAT;
using MeleeLib.DAT.Helpers;
using MeleeLib.GCX;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.Shaders;
using System;
using Smash_Forge.GUI.Melee;

namespace Smash_Forge
{
    public class MeleeDataObjectNode : MeleeNode
    {
        enum TextureTypeFlag : uint
        {
            Diffuse = 0x10,
            Sphere = 0x1,
            Specular = 0x20,
            Unk2 = 0x00, // giga bowser ao?
            Unk3 = 0x30 // also diffuse?
        }

        public DatDOBJ DOBJ;

        // For Rendering Only
        public List<MeleeMesh> RenderMeshes = new List<MeleeMesh>();
        public List<MeleeRenderTexture> RenderTextures = new List<MeleeRenderTexture>();
        public Vector3 BonePosition;

        // for importing
        public GXVertex[] VertsToImport;
        
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
            DOBJEditor editor = new DOBJEditor(DOBJ);
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
            GXVertexDecompressor decompressor = new GXVertexDecompressor(GetDatFile().DatFile);

            int index = 0;
            foreach (DatPolygon p in DOBJ.Polygons)
            {
                foreach (GXDisplayList dl in p.DisplayLists)
                {
                    GXVertex[] verts = decompressor.GetFormattedVertices(dl, p);
                    for(int i = 0; i < verts.Length; i++)
                    {
                        if(verts[i].N != null && verts[i].N.Length == 1)
                        {
                            Vector3 ToTransform = Vector3.TransformPosition(new Vector3(verts[i].Pos.X, verts[i].Pos.Y, verts[i].Pos.Z), Bones.bones[verts[i].N[0]].transform);
                            verts[i].Pos.X = ToTransform.X;
                            verts[i].Pos.Y = ToTransform.Y;
                            verts[i].Pos.Z = ToTransform.Z;
                            Vector3 ToTransformN = Vector3.TransformNormal(new Vector3(verts[i].Nrm.X, verts[i].Nrm.Y, verts[i].Nrm.Z), Bones.bones[verts[i].N[0]].transform);
                            verts[i].Nrm.X = ToTransformN.X;
                            verts[i].Nrm.Y = ToTransformN.Y;
                            verts[i].Nrm.Z = ToTransformN.Z;
                        }
                        // TODO: Transform by attached jobj
                    }
                    Verts.AddRange(verts);

                    List<int> indi = new List<int>();
                    for (int i = 0; i < dl.Indices.Length; i ++)
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
            if(VertsToImport != null)
            {
                if(DOBJ.Polygons.Count == 0)
                {
                    MessageBox.Show("Error injecting vertices into DOBJ: Not enough polygons");
                    return;
                }
                DatPolygon p = DOBJ.Polygons[0];
                p.AttributeGroup = GetRoot().Root.Attributes[0];
                {
                    List<GXDisplayList> newDL = new List<GXDisplayList>();
                    for (int i = 0; i < VertsToImport.Length; i += 3)
                    {
                        newDL.Add(compressor.CompressDisplayList(
                            new GXVertex[] { VertsToImport[i + 2], VertsToImport[i + 1], VertsToImport[i] },
                            GXPrimitiveType.Triangles,
                            p.AttributeGroup));
                    }
                    p.DisplayLists = newDL;
                }
                VertsToImport = null;
            }
            else
            {
                foreach (DatPolygon p in DOBJ.Polygons)
                {
                    List<GXDisplayList> newDL = new List<GXDisplayList>();
                    if (VertsToImport == null)
                    {
                        foreach (GXDisplayList dl in p.DisplayLists)
                        {
                            newDL.Add(compressor.CompressDisplayList(
                                decompressor.GetFormattedVertices(dl, p),
                                dl.PrimitiveType,
                                p.AttributeGroup));
                        }
                    }
                    p.DisplayLists = newDL;
                }
            }
        }

        public void Render(Camera c, Shader shader)
        {
            shader.SetVector3("BonePosition", BonePosition);

            SetTextureUniforms(shader);

            shader.SetInt("flags", DOBJ.Material.Flags);
            shader.SetBoolToInt("enableSpecular", IsSpecularBitSet());
            shader.SetBoolToInt("enableDiffuseLighting", IsDiffuseLightingBitSet());

            SetRgbaColor(shader, "ambientColor", DOBJ.Material.MaterialColor.AMB);
            SetRgbaColor(shader, "diffuseColor", DOBJ.Material.MaterialColor.DIF);
            SetRgbaColor(shader, "specularColor", DOBJ.Material.MaterialColor.SPC);

            shader.SetFloat("glossiness", DOBJ.Material.MaterialColor.Glossiness);
            shader.SetFloat("transparency", DOBJ.Material.MaterialColor.Transparency);

            if (Checked)
                foreach (MeleeMesh m in RenderMeshes)
                {
                    if (IsSelected)
                        DrawModelSelection(m, shader, c);
                    else
                        m.Draw(shader, c);
                }
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
            shader.SetVector2("unk2Scale", new Vector2(1, 1));
            shader.SetVector2("specularScale", new Vector2(1, 1));

            shader.SetTexture("diffuseTex", Rendering.RenderTools.defaultTex.Id, TextureTarget.Texture2D, 0);
            shader.SetTexture("unk2Tex", Rendering.RenderTools.defaultTex.Id, TextureTarget.Texture2D, 2);
            shader.SetTexture("specularTex", Rendering.RenderTools.defaultTex.Id, TextureTarget.Texture2D, 3);

            bool hasDiffuse = false;
            bool hasUnk2 = false;
            bool hasSphere = false;
            bool hasSpecular = false;

            // TODO: Does each texture have its own scale?
            shader.SetVector2("UV0Scale", new Vector2(1));

            foreach (var renderTex in RenderTextures)
            {
                uint type = GetTextureType(renderTex);
                if (Enum.IsDefined(typeof(TextureTypeFlag), type))
                {
                    switch ((TextureTypeFlag)type)
                    {
                        default:
                            break;
                        case TextureTypeFlag.Specular:
                            hasSpecular = true;
                            SetSpecularTexUniforms(shader, renderTex);
                            break;
                        case TextureTypeFlag.Unk2:
                            hasUnk2 = true;
                            SetUnk2TexUniforms(shader, renderTex);
                            break;
                    }
                }

                if (IsSphereBitSet(renderTex))
                    hasSphere = true;

                if (IsDiffuseBitSet(renderTex))
                {
                    hasDiffuse = true;
                    SetDiffuseTexUniforms(shader, renderTex);
                }
            }

            shader.SetBoolToInt("hasDiffuse", hasDiffuse);
            shader.SetBoolToInt("hasUnk2", hasUnk2);
            shader.SetBoolToInt("hasSpecular", hasSpecular);
            shader.SetBoolToInt("hasSphere", hasSphere);
        }

        private static bool IsDiffuseBitSet(MeleeRenderTexture renderTex)
        {
            return (renderTex.Flag & (uint)TextureTypeFlag.Diffuse) > 0;
        }

        private static bool IsSphereBitSet(MeleeRenderTexture renderTex)
        {
            return (renderTex.Flag & (uint)TextureTypeFlag.Sphere) > 0;
        }

        private static void SetSphereTexUniforms(Shader shader, MeleeRenderTexture renderTex)
        {
            shader.SetVector2("sphereScale", new Vector2(renderTex.WScale, renderTex.HScale));
            shader.SetTexture("sphereTex", renderTex.texture.Id, TextureTarget.Texture2D, 1);
        }

        private static void SetUnk2TexUniforms(Shader shader, MeleeRenderTexture renderTex)
        {
            shader.SetVector2("unk2Scale", new Vector2(renderTex.WScale, renderTex.HScale));
            shader.SetTexture("unk2Tex", renderTex.texture.Id, TextureTarget.Texture2D, 2);
        }

        private static void SetDiffuseTexUniforms(Shader shader, MeleeRenderTexture renderTex)
        {
            shader.SetVector2("diffuseScale", new Vector2(renderTex.WScale, renderTex.HScale));
            shader.SetTexture("diffuseTex", renderTex.texture.Id, TextureTarget.Texture2D, 0);
        }

        private static void SetSpecularTexUniforms(Shader shader, MeleeRenderTexture renderTex)
        {
            shader.SetVector2("specularScale", new Vector2(renderTex.WScale, renderTex.HScale));
            shader.SetTexture("specularTex", renderTex.texture.Id, TextureTarget.Texture2D, 3);
        }

        private static uint GetTextureType(MeleeRenderTexture renderTex)
        {
            return renderTex.Flag & 0xFF;
        }

        private static void DrawModelSelection(MeleeMesh mesh, Shader shader, Camera camera)
        {
            //This part needs to be reworked for proper outline. Currently would make model disappear

            mesh.Draw(shader, camera);

            GL.Enable(EnableCap.StencilTest);
            // use vertex color for wireframe color
            shader.SetInt("colorOverride", 1);
            GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.5f);

            mesh.Draw(shader, camera);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            shader.SetInt("colorOverride", 0);

            GL.Enable(EnableCap.DepthTest);
        }
        
        public void SetRgbaColor(Shader shader, string name, Color color)
        {
            shader.SetVector4(name, SFGraphics.Utils.ColorTools.Vector4FromColor(color));
        }

        public void RefreshRendering()
        {
            RefreshRenderTextures();
            RefreshRenderMeshes();
        }

        private void RefreshRenderMeshes()
        {
            RenderMeshes.Clear();
            GXVertexDecompressor decom = new GXVertexDecompressor(((MeleeDataNode)Parent.Parent.Parent).DatFile);
            foreach (DatPolygon p in DOBJ.Polygons)
            {
                foreach (GXDisplayList dl in p.DisplayLists)
                {
                    int size = 1;
                    PrimitiveType Type = PrimitiveType.Triangles;
                    switch (dl.PrimitiveType)
                    {
                        case GXPrimitiveType.Points: Type = PrimitiveType.Points; break;
                        case GXPrimitiveType.Lines: Type = PrimitiveType.Lines; break;
                        case GXPrimitiveType.LineStrip: Type = PrimitiveType.LineStrip; break;
                        case GXPrimitiveType.TriangleFan: Type = PrimitiveType.TriangleFan; break;
                        case GXPrimitiveType.TriangleStrip: Type = PrimitiveType.TriangleStrip; break;
                        case GXPrimitiveType.Triangles: Type = PrimitiveType.Triangles; break;
                        case GXPrimitiveType.Quads: Type = PrimitiveType.Quads; break;
                    }
                    List<int> indices = new List<int>();
                    for (int i = 0; i < dl.Indices.Length; i += size)
                    {
                        for (int j = size - 1; j >= 0; j--)
                        {
                            indices.Add(i + j);
                        }
                    }
                    MeleeMesh m = new MeleeMesh(ConvertVerts(decom.GetFormattedVertices(dl, p)), indices);
                    m.PrimitiveType = Type;
                    RenderMeshes.Add(m);
                }
            }
        }

        private void RefreshRenderTextures()
        {
            RenderTextures.Clear();

            foreach (DatTexture t in DOBJ.Material.Textures)
            {
                MeleeRenderTexture tex = new MeleeRenderTexture(t);
                tex.Flag = t.UnkFlags;
                RenderTextures.Add(tex);
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
                    UV0 = new Vector2(v.TX0.X, v.TX0.Y)
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
                }
                o.Add(vert);
            }
            return o;
        }
    }
}
