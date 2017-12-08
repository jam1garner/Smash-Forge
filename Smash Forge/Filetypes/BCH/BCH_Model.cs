using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

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

        public VBN skeleton = new VBN();
        public Matrix4 worldTransform;

        public byte[] vdata;
        public byte[] idata;

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

            if (!Runtime.shaders.ContainsKey("MBN"))
            {
                Shader mbn = new Shader();
                mbn.vertexShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/MBN_vs.txt"));
                mbn.fragmentShader(File.ReadAllText(MainForm.executableDir + "/lib/Shader/MBN_fs.txt"));
                Runtime.shaders.Add("MBN", mbn);
            }

            Runtime.shaders["MBN"].displayCompilationWarning("MBN");

            if (shader == null)
            {
                shader = new Shader();
                shader = Runtime.shaders["MBN"];
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
                Text = ((_3DSGPU.VertexAttribute)type).ToString();
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

            shader = Runtime.shaders["MBN"];
            GL.UseProgram(shader.programID);

            GL.Uniform1(shader.getAttribute("renderVertColor"), Runtime.renderVertColor ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderType"), 0);//(int)Runtime.renderType
            GL.Uniform1(shader.getAttribute("selectedBoneIndex"), Runtime.selectedBoneIndex);

            GL.UniformMatrix4(shader.getAttribute("modelview"), false, ref view);

            GL.Uniform3(shader.getAttribute("difLightColor"), Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB);
            GL.Uniform3(shader.getAttribute("ambLightColor"), Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB);

            GL.ActiveTexture(TextureUnit.Texture10);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.UVTestPattern);
            GL.Uniform1(shader.getAttribute("UVTestPattern"), 10);

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

            shader.enableAttrib();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_vert);
            GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.Stride * Vertices.Length), Vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("pos"), 3, VertexAttribPointerType.Float, false, Vertex.Stride, 0);
            GL.VertexAttribPointer(shader.getAttribute("nrm"), 3, VertexAttribPointerType.Float, false, Vertex.Stride, 12);
            GL.VertexAttribPointer(shader.getAttribute("col"), 4, VertexAttribPointerType.Float, false, Vertex.Stride, 24);
            GL.VertexAttribPointer(shader.getAttribute("tx0"), 2, VertexAttribPointerType.Float, false, Vertex.Stride, 40);
            GL.VertexAttribPointer(shader.getAttribute("bone"), 2, VertexAttribPointerType.Float, false, Vertex.Stride, 48);
            GL.VertexAttribPointer(shader.getAttribute("weight"), 2, VertexAttribPointerType.Float, false, Vertex.Stride, 56);

            GL.PointSize(4f);
            //GL.DrawArrays(PrimitiveType.Points, 0, Vertices.Length);

            foreach (BCH_Mesh m in Nodes)
            {
                GL.Uniform4(shader.getAttribute("colorSamplerUV"), new Vector4(1, 1, 0, 0));

                GL.ActiveTexture(TextureUnit.Texture0);
                BCH_Material material = (BCH_Material)((BCH)Parent.Parent).Materials.Nodes[m.MaterialIndex];
                BCH_Texture tex = ((BCH)Parent.Parent).GetTexture(material.Text);
                GL.BindTexture(TextureTarget.Texture2D, tex == null ? VBNViewport.defaulttex : tex.display);
                GL.Uniform1(shader.getAttribute("tex"), 0);
                if (!m.Checked) continue;

                foreach (BCH_PolyGroup pg in m.Nodes)
                {
                    GL.Uniform1(shader.getAttribute("boneList"), pg.BoneList.Length, pg.BoneList);

                    GL.Disable(EnableCap.CullFace);
                    GL.CullFace(CullFaceMode.Back);

                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_faces);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(pg.Faces.Length * sizeof(int)), pg.Faces, BufferUsageHint.StaticDraw);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    
                    GL.DrawElements(PrimitiveType.Triangles, pg.Faces.Length, DrawElementsType.UnsignedInt, 0);
                }
            }
            shader.disableAttrib();
        }
    }

    public class BCH_Mesh : TreeNode
    {
        public int renderPriority = 0;

        //Material
        public int MaterialIndex = 0;
    }

    public class BCH_PolyGroup : TreeNode
    {
        public int Count;
        public int[] Faces;
        public int[] BoneList;
    }

    public class BCH_Material : TreeNode
    {


    }
}
