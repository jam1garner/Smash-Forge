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

namespace Smash_Forge
{
    public class MeleeDataObjectNode : TreeNode
    {
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

            MenuItem Clear = new MenuItem("Clear Polygons");
            Clear.Click += ClearPolygons;
            ContextMenu.MenuItems.Add(Clear);
        }

        public void ClearPolygons(object sender, EventArgs args)
        {
            DOBJ.Polygons.Clear();
        }

        public void RecompileVertices(GXVertexDecompressor decompressor, GXVertexCompressor compressor)
        {
            foreach(DatPolygon p in DOBJ.Polygons)
            {
                List<GXDisplayList> newDL = new List<GXDisplayList>();
                foreach(GXDisplayList dl in p.DisplayLists)
                {
                    newDL.Add(compressor.CompressDisplayList(
                        VertsToImport == null ? decompressor.GetFormattedVertices(dl, p) : VertsToImport, 
                        dl.PrimitiveType, 
                        p.AttributeGroup));
                }
                p.DisplayLists = newDL;
            }
        }

        public void Render(Camera c, Shader shader)
        {
            shader.SetVector3("BonePosition", BonePosition);

            if (RenderTextures.Count > 0)
            {
                shader.SetVector2("UV0Scale", new Vector2(RenderTextures[0].WScale, RenderTextures[0].HScale));

                shader.SetTexture("TEX1", RenderTextures[0].texture.Id, TextureTarget.Texture2D, 0);
            }
            else
                shader.SetVector2("UV0Scale", new Vector2(1, 1));

            shader.SetVector3("Eye", Vector3.TransformPosition(Vector3.Zero, c.MvpMatrix));

            shader.SetInt("Flags", DOBJ.Material.Flags);
            SetColor(shader, "AMB", DOBJ.Material.MaterialColor.AMB);
            SetColor(shader, "DIF", DOBJ.Material.MaterialColor.DIF);
            SetColor(shader, "SPC", DOBJ.Material.MaterialColor.SPC);
            shader.SetFloat("glossiness", DOBJ.Material.MaterialColor.Glossiness);
            shader.SetFloat("transparency", DOBJ.Material.MaterialColor.Transparency);

            if(Checked)
            foreach (MeleeMesh m in RenderMeshes)
            {
                m.Draw(shader, c);
            }
        }

        public void SetColor(Shader s, string name, Color c)
        {
            s.SetVector4(name, new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f));
        }

        public void RefreshRenderMeshes()
        {
            RenderTextures.Clear();

            foreach (DatTexture t in DOBJ.Material.Textures)
            {
                MeleeRenderTexture tex = new MeleeRenderTexture(t);
                tex.Flag = t.UnkFlags;
                RenderTextures.Add(tex);
            }
            
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
                    try
                    {

                        MeleeMesh m = new MeleeMesh(ConvertVerts(decom.GetFormattedVertices(dl, p)), indices);
                        m.PrimitiveType = Type;
                        RenderMeshes.Add(m);
                    } catch (System.Exception e)
                    {

                    }
                }
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
