using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using SFGenericModel;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using MeleeLib.DAT;
using MeleeLib.DAT.Helpers;
using MeleeLib.IO;
using MeleeLib.GCX;
using SFGraphics.Cameras;
using Smash_Forge.Rendering;
using SFGraphics.GLObjects.Shaders;

namespace Smash_Forge
{
    public class MeleeDataNode : TreeNode
    {
        public DATFile DatFile;

        private bool hasCreatedRenderMeshes = false;

        public MeleeDataNode(string fname)
        {
            DatFile = Decompiler.Decompile(File.ReadAllBytes(fname));
        }

        public void RefreshDisplay()
        {
            Nodes.Clear();
            foreach(DATRoot root in DatFile.Roots)
            {
                MeleeRootNode ro = new MeleeRootNode(root);
                Nodes.Add(ro);
                ro.RefreshDisplay();
            }
        }

        public void Render(Camera c)
        {
            if (!hasCreatedRenderMeshes)
            {
                RefreshDisplay();
                hasCreatedRenderMeshes = true;
            }

            foreach (MeleeRootNode n in Nodes)
            {
                n.Render(c);
            }
        }
    }

    public class MeleeRootNode : TreeNode
    {
        DATRoot Root;

        TreeNode Skeleton = new TreeNode("Joints") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode DataObjects = new TreeNode("Data Objects") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode MatAnims = new TreeNode("Material Animations") { SelectedImageKey = "folder", ImageKey = "folder" };
        TreeNode JointAnims = new TreeNode("Joint Animations") { SelectedImageKey = "folder", ImageKey = "folder" };

        public MeleeRootNode(DATRoot Root)
        {
            this.Root = Root;
            Text = Root.Text;

            SelectedImageKey = "folder";
            ImageKey = "folder";
        }

        public void RefreshDisplay()
        {
            Nodes.Clear();
            Skeleton.Nodes.Clear();
            DataObjects.Nodes.Clear();
            MatAnims.Nodes.Clear();
            JointAnims.Nodes.Clear();


            if(Root.GetJOBJinOrder().Length > 0)
                Nodes.Add(Skeleton);

            int i = 0;
            foreach (DatJOBJ j in Root.GetJOBJinOrder())
            {
                Skeleton.Nodes.Add(new MeleeJointNode(j) { Text = "Bone_" + i++ });
            }
            
            if (Root.GetDataObjects().Length > 0)
            {
                Nodes.Add(DataObjects);
                SelectedImageKey = "model";
                ImageKey = "model";
            }

            i = 0;
            foreach (DatDOBJ d in Root.GetDataObjects())
            {
                MeleeDataObjectNode n = new MeleeDataObjectNode(d) { Text = "DataObject" + i++ };
                DataObjects.Nodes.Add(n);
                n.RefreshRenderMeshes();
            }
            
            if (Root.MatAnims.Count > 0)
            {
                Nodes.Add(MatAnims);
            }

            if (Root.Animations.Count > 0)
            {
                Nodes.Add(JointAnims);
            }
        }

        public void Render(Camera c)
        {
            foreach(MeleeDataObjectNode n in DataObjects.Nodes)
            {
                n.Render(c);
            }
        }
    }

    public class MeleeJointNode : TreeNode
    {
        private DatJOBJ JOBJ;

        public MeleeJointNode(DatJOBJ jobj)
        {
            ImageKey = "bone";
            SelectedImageKey = "bone";
            this.JOBJ = jobj;
        }
    }

    public class MeleeDataObjectNode : TreeNode
    {
        private DatDOBJ DOBJ;

        public List<MeleeMesh> RenderMeshes = new List<MeleeMesh>();

        public MeleeDataObjectNode(DatDOBJ DOBJ)
        {
            ImageKey = "mesh";
            SelectedImageKey = "mesh";
            this.DOBJ = DOBJ;
        }
        public void Render(Camera c)
        {
            Shader shader = OpenTKSharedResources.shaders["DAT"];
            shader.UseProgram();

            Matrix4 mvpMatrix = c.MvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref mvpMatrix);

            foreach (MeleeMesh m in RenderMeshes)
            {
                m.Draw(shader, c);
            }
        }

        public void RefreshRenderMeshes()
        {
            GXVertexDecompressor decom = new GXVertexDecompressor();
            decom.SetRoot(((MeleeDataNode)Parent.Parent.Parent).DatFile);

            foreach(DatPolygon p in DOBJ.Polygons)
            {
                foreach(GXDisplayList dl in p.DisplayLists)
                {
                    List<int> indices = new List<int>();
                    for(int i =0; i < dl.Indices.Length; i++)
                    {
                        indices.Add(i);
                    }
                    MeleeMesh m = new MeleeMesh(ConvertVerts(decom.GetFormattedVertices(dl, p)), indices);
                    switch (dl.PrimitiveType)
                    {
                        case GXPrimitiveType.Points: m.PrimitiveType = PrimitiveType.Points; break;
                        case GXPrimitiveType.Lines: m.PrimitiveType = PrimitiveType.Lines; break;
                        case GXPrimitiveType.LineStrip: m.PrimitiveType = PrimitiveType.LineStrip; break;
                        case GXPrimitiveType.TriangleFan: m.PrimitiveType = PrimitiveType.TriangleFan; break;
                        case GXPrimitiveType.TriangleStrip: m.PrimitiveType = PrimitiveType.TriangleStrip; break;
                        case GXPrimitiveType.Triangles: m.PrimitiveType = PrimitiveType.Triangles; break;
                        case GXPrimitiveType.Quads: m.PrimitiveType = PrimitiveType.Quads; break;
                    }
                    RenderMeshes.Add(m);
                }
            }
            Console.WriteLine(RenderMeshes.Count);
        }

        public static List<MeleeVertex> ConvertVerts(GXVertex[] Verts)
        {
            List<MeleeVertex> o = new List<MeleeVertex>();
            foreach(GXVertex v in Verts)
            {
                o.Add(new MeleeVertex()
                {
                    Pos = new Vector3(v.Pos.X, v.Pos.Y, v.Pos.Z)
                });
            }
            return o;
        }
    }

    public struct MeleeVertex
    {
        public Vector3 Pos;
    }

    public class MeleeMesh : GenericMesh<MeleeVertex>
    {

        public MeleeMesh(List<MeleeVertex> vertices, List<int> vertexIndices) 
            : base(vertices, vertexIndices)
        {

        }
        protected override List<VertexAttributeInfo> GetVertexAttributes()
        {
            return new List<VertexAttributeInfo>()
            {
                new VertexAttributeInfo("vPosition",  3, VertexAttribPointerType.Float, Vector3.SizeInBytes)
            };
        }
    }
}
