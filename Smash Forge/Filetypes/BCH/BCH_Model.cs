using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Smash_Forge.Rendering;

namespace Smash_Forge
{
    public class BCH_Model : TreeNode
    {
        public static Shader shader = null;

        int vbo_vert;
        int vbo_bone;
        int ibo_faces;

        public int flags;
        public int skeletonScaleType;
        public int silhouetteMaterialEntries;

        public VBN skeleton
        {
            get
            {
                return vbn;
            }
            set
            {
                vbn = value;
            }
        }
        private VBN vbn = new VBN();
        public Matrix4 worldTransform;

        List<VertexAttribute> Attributes = new List<VertexAttribute>();
        public Vertex[] Vertices;
        
        public struct Vertex
        {
            public Vector3 pos;
            public Vector3 nrm;
            public Vector4 col;
            public Vector2 tx;
            public Vector2 bone;
            public Vector2 weight;
            public static int Stride = (3+3+2+2+2+4) * 4;
        }

        public BCH_Model()
        {
            GL.GenBuffers(1, out vbo_vert);
            GL.GenBuffers(1, out vbo_bone);
            GL.GenBuffers(1, out ibo_faces);
            
            ImageKey = "model";
            SelectedImageKey = "model";

            ContextMenu cm = new ContextMenu();
            MenuItem im = new MenuItem("Import DAE");
            im.Click += Import;
            cm.MenuItems.Add(im);
            MenuItem save = new MenuItem("Save as MBN");
            save.Click += Click;
            cm.MenuItems.Add(save);
            ContextMenu = cm;

            if (!Runtime.shaders.ContainsKey("MBN"))
            {
                ShaderTools.CreateShader("MBN", "/lib/Shader/Legacy/", "/lib/Shader/");
            }

            Runtime.shaders["MBN"].DisplayCompilationWarning("MBN");

            if (shader == null)
            {
                shader = new Shader();
                shader = Runtime.shaders["MBN"];
            }
        }

        public void Click(object o, EventArgs a)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.ShowDialog();
                if(sfd.FileNames.Length > 0)
                {
                    SaveAsMBN(sfd.FileName);
                }
            }
        }

        public void Import(object o, EventArgs a)
        {
            using (OpenFileDialog sfd = new OpenFileDialog())
            {
                sfd.ShowDialog();
                foreach(string f in sfd.FileNames)
                {
                    if (f.ToLower().EndsWith(".dae"))
                    {
                        DAEImportSettings daeImport = new DAEImportSettings();
                        daeImport.ShowDialog();
                        if (daeImport.exitStatus == DAEImportSettings.ExitStatus.Opened)
                        {
                            ModelContainer con = new ModelContainer();

                            // load vbn
                            con.VBN = skeleton;

                            Collada.DaetoNud(f, con, daeImport.importTexCB.Checked);
                            
                            // apply settings
                            daeImport.Apply(con.NUD);
                            con.NUD.MergePoly();
                            CreateFromNUD(con.NUD);
                        }
                    }
                }
            }
        }

        public enum Formats
        {
            Data = 4
        }

        public class VertexAttribute : TreeNode
        {
            public int type;
            public int format;
            public float scale;

            public void Read(FileData f)
            {
                type = f.readInt();
                format = f.readInt();
                scale = f.readFloat();
                Text = ((_3DSGPU.VertexAttribute)type).ToString() + "_" + format;
            }

            public void ReadVertex(FileData f, ref Vertex v)
            {
                switch (type)
                {
                    case (int)_3DSGPU.VertexAttribute.pos:
                        v.pos = new Vector3(ReadType(f), ReadType(f), ReadType(f));
                        break;
                    case (int)_3DSGPU.VertexAttribute.nrm:
                        v.nrm = new Vector3(ReadType(f), ReadType(f), ReadType(f));
                        ReadType(f);
                        break;
                    case (int)_3DSGPU.VertexAttribute.tx0:
                        v.tx = new Vector2(ReadType(f), ReadType(f));
                        break;
                    case (int)_3DSGPU.VertexAttribute.col:
                        v.col = new Vector4(ReadType(f), ReadType(f), ReadType(f), ReadType(f));
                        break;
                    case (int)_3DSGPU.VertexAttribute.bone:
                        v.bone = new Vector2(f.readByte(), f.readByte());
                        break;
                    case (int)_3DSGPU.VertexAttribute.weight:
                        v.weight = new Vector2(ReadType(f), ReadType(f));
                        break;
                }
            }

            public void WriteVertex(FileOutput f, ref Vertex v)
            {
                switch (type)
                {
                    case (int)_3DSGPU.VertexAttribute.pos:
                        WriteType(f, v.pos.X);
                        WriteType(f, v.pos.Y);
                        WriteType(f, v.pos.Z);
                        break;
                    case (int)_3DSGPU.VertexAttribute.nrm:
                        WriteType(f, v.nrm.X);
                        WriteType(f, v.nrm.Y);
                        WriteType(f, v.nrm.Z);
                        WriteType(f, 1f);
                        break;
                    case (int)_3DSGPU.VertexAttribute.tx0:
                        WriteType(f, v.tx.X);
                        WriteType(f, v.tx.Y);
                        break;
                    case (int)_3DSGPU.VertexAttribute.col:
                        WriteType(f, v.col.X);
                        WriteType(f, v.col.Y);
                        WriteType(f, v.col.Z);
                        WriteType(f, v.col.W);
                        break;
                    case (int)_3DSGPU.VertexAttribute.bone:
                        f.writeByte((int)v.bone.X);
                        f.writeByte((int)v.bone.Y);
                        break;
                    case (int)_3DSGPU.VertexAttribute.weight:
                        WriteType(f, v.weight.X);
                        WriteType(f, v.weight.Y);
                        break;
                    default:
                        Console.WriteLine("Error");
                        break;
                }
            }

            private float ReadType(FileData d)
            {
                switch (format)
                {
                    case 0:
                        return d.readFloat() * scale;
                    case 1:
                        return d.readByte() * scale;
                    case 2:
                        return (sbyte)d.readByte() * scale;
                    case 3:
                        return (short)d.readShort() * scale;
                }
                return 0;
            }

            private void WriteType(FileOutput d, float data)
            {
                switch (format)
                {
                    case 0:
                        d.writeFloat(data / scale);
                        break;
                    case 1:
                        d.writeByte((byte)(data / scale));
                        break;
                    case 2:
                        d.writeByte((sbyte)(data / scale));
                        break;
                    case 3:
                        d.writeShort((int)(data / scale));
                        break;
                }
            }
        }

        public void SaveAsMBN(string fname)
        {
            int format = 6;
            FileOutput o = new FileOutput();
            o.Endian = Endianness.Little;
            
            o.writeShort(format);
            o.writeShort(0xFFFF);
            o.writeInt(0); //flags
            o.writeInt(1); //mode
            o.writeInt(Nodes.Count);

            // Write Vertex Attributes
            {
                o.writeInt(Attributes.Count);
                foreach(VertexAttribute va in Attributes)
                {
                    o.writeInt(va.type);
                    o.writeInt(va.format);
                    o.writeFloat(va.scale);
                }
            }


            //Vertex Buffer
            FileOutput vertexBuffer = new FileOutput();
            vertexBuffer.Endian = Endianness.Little;

            for(int i = 0; i < Vertices.Length; i++)
            {
                foreach (VertexAttribute va in Attributes)
                {
                    //Write Data
                    va.WriteVertex(vertexBuffer, ref Vertices[i]);
                }
            }

            o.writeInt(vertexBuffer.size()); // Vertex Buffer Size

            //Mesh Information
            FileOutput indexBuffer = new FileOutput();
            indexBuffer.Endian = Endianness.Little;
            foreach (BCH_Mesh mesh in Nodes)
            {
                o.writeInt(mesh.Nodes.Count);
                foreach(BCH_PolyGroup pg in mesh.Nodes)
                {
                    // Node List
                    o.writeInt(pg.BoneList.Length);
                    foreach (int b in pg.BoneList)
                        o.writeInt(b);

                    // Triangle Count
                    o.writeInt(pg.Faces.Length);
                    // o.writeInt(0); something if format == 4

                    // Index Buffer
                    foreach (int i in pg.Faces)
                        indexBuffer.writeShort(i);
                    indexBuffer.align(0x20, 0xFF);
                }
            }

            if (format != 4) o.align(0x20, 0xFF);

            o.writeOutput(vertexBuffer);
            o.align(0x20, 0xFF);
            o.writeOutput(indexBuffer);
            o.save(fname);
        }

        public void OpenMBN(FileData f)
        {
            f.Endian = Endianness.Little;
            f.seek(0);

            int format = f.readShort();
            f.skip(2);//0xFFFF
            int flags = f.readInt();
            int mode = f.readInt();
            int meshCount = f.readInt();
            
            int length = 0;
            if (mode == 1)
            {
                //One Attribute
                int count = f.readInt();
                for(int i = 0; i < count; i++)
                {
                    VertexAttribute a = new VertexAttribute();
                    a.Read(f);
                    Attributes.Add(a);
                }
                length = f.readInt();
            }

            // Get Mesh Nodes
            /*List<BCH_Mesh> meshes = new List<BCH_Mesh>();
            foreach(BCH_Mesh m in Nodes)
            {
                meshes.Add(m);
                foreach (BCH_Mesh m2 in m.Nodes)
                    meshes.Add(m);
            }*/

            for (int i = 0; i < meshCount; i++)
            {
                BCH_Mesh m = (BCH_Mesh)Nodes[i];

                int polyCount = f.readInt();
                for (int j = 0; j < polyCount; j++)
                {
                    BCH_PolyGroup pg = new BCH_PolyGroup();
                    m.Nodes.Add(pg);
                    int nodeCount = f.readInt();
                    int[] nodeList = new int[nodeCount];
                    pg.BoneList=(nodeList);
                    for (int k = 0; k < nodeCount; k++)
                    {
                        nodeList[k] = f.readInt();
                    }
                    pg.Count=(f.readInt());
                    if ((flags & 2) > 0) f.readInt();

                }
            }


            if (format != 4) f.align(32);

            int stride = 0;
            foreach (VertexAttribute a in Attributes)
                stride += _3DSGPU.getTypeSize(a.format) * _3DSGPU.getFormatSize(a.type);

            // Vertex Bank
            Vertices = new Vertex[length / (stride+stride%2)];
            for (int vi = 0; vi < Vertices.Length; vi++)
            {
                Vertex v = new Vertex();
                foreach (VertexAttribute a in Attributes)
                {
                    //f.align(2);
                    a.ReadVertex(f, ref v);

                }
                Vertices[vi] = v;
            }
            f.align(32);


            for (int i = 0; i < meshCount; i++)
            {
                BCH_Mesh m = (BCH_Mesh)Nodes[i];

                int pi = 0;
                foreach (BCH_PolyGroup pg in m.Nodes)
                {
                    pg.Text = "Polygroup_"+pi++;
                    pg.Faces = new int[pg.Count];
                    for(int k = 0; k < pg.Count; k++)
                        pg.Faces[k] = f.readShort();
                    f.align(32);
                }
            }
        }

        public void Render(Matrix4 view)
        {
            if (Vertices == null)
                return;

            shader = Runtime.shaders["MBN"];
            GL.UseProgram(shader.programID);

            GL.Uniform1(shader.GetVertexAttributeLocation("renderVertColor"), Runtime.renderVertColor ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("renderType"), (int)Runtime.renderType);
            GL.Uniform1(shader.GetVertexAttributeLocation("selectedBoneIndex"), Runtime.selectedBoneIndex);

            GL.UniformMatrix4(shader.GetVertexAttributeLocation("modelview"), false, ref view);

            GL.Uniform3(shader.GetVertexAttributeLocation("difLightColor"), Runtime.lightSetParam.characterDiffuse.diffuseColor.R, Runtime.lightSetParam.characterDiffuse.diffuseColor.G, Runtime.lightSetParam.characterDiffuse.diffuseColor.B);
            GL.Uniform3(shader.GetVertexAttributeLocation("ambLightColor"), Runtime.lightSetParam.characterDiffuse.ambientColor.R, Runtime.lightSetParam.characterDiffuse.ambientColor.G, Runtime.lightSetParam.characterDiffuse.ambientColor.B);

            GL.ActiveTexture(TextureUnit.Texture10);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.uvTestPattern);
            GL.Uniform1(shader.GetVertexAttributeLocation("UVTestPattern"), 10);

            Matrix4[] f = skeleton.getShaderMatrix();

            int maxUniformBlockSize = GL.GetInteger(GetPName.MaxUniformBlockSize);
            int boneCount = skeleton.bones.Count;
            int dataSize = boneCount * Vector4.SizeInBytes * 4;

            GL.BindBuffer(BufferTarget.UniformBuffer, vbo_bone);
            GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)(dataSize), IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            var blockIndex = GL.GetUniformBlockIndex(shader.programID, "bones");
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, blockIndex, vbo_bone);

            if (f.Length > 0)
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, vbo_bone);
                GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, (IntPtr)(f.Length * Vector4.SizeInBytes * 4), f);
            }

            shader.EnableVertexAttributes();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_vert);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.Stride * Vertices.Length), Vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.GetVertexAttributeLocation("pos"), 3, VertexAttribPointerType.Float, false, Vertex.Stride, 0);
            GL.VertexAttribPointer(shader.GetVertexAttributeLocation("nrm"), 3, VertexAttribPointerType.Float, false, Vertex.Stride, 12);
            GL.VertexAttribPointer(shader.GetVertexAttributeLocation("col"), 4, VertexAttribPointerType.Float, false, Vertex.Stride, 24);
            GL.VertexAttribPointer(shader.GetVertexAttributeLocation("tx0"), 2, VertexAttribPointerType.Float, false, Vertex.Stride, 40);
            GL.VertexAttribPointer(shader.GetVertexAttributeLocation("bone"), 2, VertexAttribPointerType.Float, false, Vertex.Stride, 48);
            GL.VertexAttribPointer(shader.GetVertexAttributeLocation("weight"), 2, VertexAttribPointerType.Float, false, Vertex.Stride, 56);

            GL.PointSize(4f);
            //GL.DrawArrays(PrimitiveType.Points, 0, Vertices.Length);

            foreach (BCH_Mesh m in Nodes)
            {
                GL.Uniform4(shader.GetVertexAttributeLocation("colorSamplerUV"), new Vector4(1, 1, 0, 0));

                GL.ActiveTexture(TextureUnit.Texture0);
                BCH_Material material = (BCH_Material)((BCH)Parent.Parent).Materials.Nodes[m.MaterialIndex];
                BCH_Texture tex = ((BCH)Parent.Parent).GetTexture(material.Text);
                GL.BindTexture(TextureTarget.Texture2D, tex == null ? RenderTools.defaultTex : tex.display);
                GL.Uniform1(shader.GetVertexAttributeLocation("tex"), 0);
                if (!m.Checked) continue;

                foreach (BCH_PolyGroup pg in m.Nodes)
                {
                    GL.Uniform1(shader.GetVertexAttributeLocation("boneList"), pg.BoneList.Length, pg.BoneList);

                    GL.Disable(EnableCap.CullFace);
                    GL.CullFace(CullFaceMode.Back);

                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_faces);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(pg.Faces.Length * sizeof(int)), pg.Faces, BufferUsageHint.StaticDraw);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    
                    GL.DrawElements(PrimitiveType.Triangles, pg.Faces.Length, DrawElementsType.UnsignedInt, 0);
                }
            }
            shader.DisableVertexAttributes();
        }


        // Create from nud
        public void CreateFromNUD(NUD n)
        {
            //Alrighty.............................
            /*int meshcount = Nodes.Count;

            // First transfer over the mesh polygons?
            int i = 0;
            List<BCH_Mesh> Meshes = new List<BCH_Mesh>();
            List<Vertex> Verts = new List<Vertex>();
            Console.WriteLine(n.Nodes.Count + " " + n.Nodes.Count);
            foreach (NUD.Mesh nudmesh in n.Nodes)
            {
                BCH_Mesh mesh = new BCH_Mesh();
                mesh.Text = Nodes[i].Text; //nudmesh.Text;//
                Console.WriteLine(nudmesh.Text);
                mesh.MaterialIndex = ((BCH_Mesh)Nodes[i]).MaterialIndex;
                i++;
                Meshes.Add(mesh);
                foreach(NUD.Polygon nudpoly in nudmesh.Nodes)
                {
                    BCH_PolyGroup pg = new BCH_PolyGroup();
                    pg.Text = "Polygroup";
                    mesh.Nodes.Add(pg);

                    pg.Faces = new int[nudpoly.display.Length];
                    for(int k = 0; k < nudpoly.display.Length; k++)
                    {
                        pg.Faces[k] = nudpoly.display[k] + Verts.Count;
                    }

                    List<int> boneList = new List<int>();
                    foreach(NUD.dVertex v in nudpoly.vertdata)
                    {
                        Vertex vn = new Vertex();
                        vn.pos = v.pos;
                        vn.nrm = v.nrm;
                        vn.tx = v.uv;
                        vn.col = v.col;
                        vn.weight = v.weight.Xy;
                        if (!boneList.Contains((int)v.node.X)) boneList.Add((int)v.node.X);
                        if (!boneList.Contains((int)v.node.Y)) boneList.Add((int)v.node.Y);

                        vn.bone = new Vector2(boneList.IndexOf((int)v.node.X), boneList.IndexOf((int)v.node.Y));
                        vn.bone = v.node.Xy;
                        Verts.Add(vn);
                    }

                    pg.BoneList = boneList.ToArray();
                }
            }

            //Fill out blank meshes
            while(Meshes.Count < meshcount)
            {
                BCH_Mesh mesh = new BCH_Mesh();
                mesh.Text = Nodes[i].Text;
                mesh.MaterialIndex = ((BCH_Mesh)Nodes[i]).MaterialIndex;
                mesh.Nodes.Add(new BCH_PolyGroup()
                {
                    Faces = new int[] { 0, 0, 0 },
                    BoneList = new int[] { 0}
                });
                Verts.Add(new Vertex());
                Meshes.Add(mesh);
                i++;
            }

            Nodes.Clear();
            Nodes.AddRange(Meshes.ToArray());
            Vertices = Verts.ToArray();*/
        }
    }

    public class BCH_Mesh : TreeNode
    {
        public int renderPriority = 0;

        //Material
        public int MaterialIndex = 0;

        public BCH_Mesh()
        {
            ImageKey = "mesh";
            SelectedImageKey = "mesh";
        }
    }

    public class BCH_PolyGroup : TreeNode
    {
        public int Count;
        public int[] Faces;
        public int[] BoneList;

        public BCH_PolyGroup()
        {
            ImageKey = "polygon";
            SelectedImageKey = "polygon";
        }
    }

    public class BCH_Material : TreeNode
    {


    }
}
