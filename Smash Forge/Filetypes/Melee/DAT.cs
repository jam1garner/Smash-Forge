using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using static Smash_Forge.DAT.POBJ;

namespace Smash_Forge
{
    public class DAT
    {
        public static int headerSize = 0x20;
        Header header = new Header();

        public List<TreeNode> tree = new List<TreeNode>();
        public List<TreeNode> displayList = new List<TreeNode>();

        public VBN bones = new VBN();

        // gl buffer objects
        int vbo_position;
        int vbo_color;
        int vbo_nrm;
        int vbo_uv;
        int vbo_weight;
        int vbo_bone;
        int ibo_elements;
        public int testtex;

        public static Shader shader = null;

        Dictionary<int, JOBJ> jobjOffsetLinker = new Dictionary<int, JOBJ>();

        static string vs = @"#version 330
 
in vec3 vPosition;
in vec4 vColor;
in vec3 vNormal;
in vec2 vUV;
in vec4 vBone;
in vec4 vWeight;

out vec2 f_texcoord;
out vec4 color;
out float normal;

uniform mat4 modelview;
uniform mat4 bones[150];
 
void
main()
{
    ivec4 index = ivec4(vBone); 
    vec4 objPos = vec4(vPosition.xyz, 1.0);

    /*if(vBone.x != -1){
        objPos = bones[index.x] * vec4(vPosition, 1.0) * vWeight.x;
        objPos += bones[index.y] * vec4(vPosition, 1.0) * vWeight.y;
        objPos += bones[index.z] * vec4(vPosition, 1.0) * vWeight.z;
        objPos += bones[index.w] * vec4(vPosition, 1.0) * vWeight.w;
    } */

    gl_Position = modelview * vec4(objPos.xyz, 1.0);

    f_texcoord = vUV;
    color = vColor;
    normal = dot(vec4(vNormal * mat3(modelview), 1.0), vec4(0.15,0.15,0.15,1.0)) ;
}";

        static string fs = @"#version 330

in vec2 f_texcoord;
in vec4 color;
in float normal;

uniform sampler2D tex;
uniform vec2 uvscale;

void
main()
{
    gl_FragColor = vec4 ((texture(tex, f_texcoord*uvscale) * color).xyz, 1);
}
";

        public DAT()
        {
            if (shader == null)
            {
                shader = new Shader();

                shader.vertexShader(vs);
                shader.fragmentShader(fs);

                shader.addAttribute("vPosition", false);
                shader.addAttribute("vColor", false);
                shader.addAttribute("vNormal", false);
                shader.addAttribute("vUV", false);
                shader.addAttribute("vBone", false);
                shader.addAttribute("vWeight", false);

                shader.addAttribute("tex", true);
                shader.addAttribute("modelview", true);
                shader.addAttribute("bones", true);
                shader.addAttribute("uvscale", true);
            }

            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out vbo_color);
            GL.GenBuffers(1, out vbo_nrm);
            GL.GenBuffers(1, out vbo_uv);
            GL.GenBuffers(1, out vbo_bone);
            GL.GenBuffers(1, out vbo_weight);
            GL.GenBuffers(1, out ibo_elements);
        }

        ~DAT()
        {
            /*GL.DeleteBuffer(vbo_position);
            GL.DeleteBuffer(vbo_color);
            GL.DeleteBuffer(vbo_nrm);
            GL.DeleteBuffer(vbo_uv);
            GL.DeleteBuffer(vbo_weight);
            GL.DeleteBuffer(vbo_bone);*/
        }

        public void Read(FileData d)
        {
            d.Endian = System.IO.Endianness.Big;
            d.seek(0);

            header.Read(d);
            d.skip(header.dataBlockSize); // skip to relocation table
            d.skip(header.relocationTableCount * 4); // skip relocation table

            int strOffset = d.pos() + header.rootCount * 8;
            int[] sectionOffset = new int[header.rootCount];
            string[] sectionNames = new string[header.rootCount];
            for (int i = 0; i < header.rootCount; i++)
            {
                // data then string
                int data = d.readInt() + 0x20;
                string s = d.readString(d.readInt() + strOffset, -1);
                sectionOffset[i] = data;
                sectionNames[i] = s;
                Console.WriteLine(s + " " + data.ToString("x"));

                TreeNode node = new TreeNode();
                node.Text = s;
                node.Tag = data;
                tree.Add(node);
            }

            foreach(TreeNode node in tree)
            {
                // then a file system is read... it works like a tree?
                d.seek((int)node.Tag);
                // now, the name determines what happens here
                // for now, it just assumes the _joint
                if (node.Text.EndsWith("_joint"))
                {
                    JOBJ j = new JOBJ();
                    j.Read(d, this, node);
                    break;
                }
                else
                if (node.Text.EndsWith("map_head"))
                {
                    int off = d.readInt() + headerSize;
                    int count = d.readInt();

                    d.seek(off);
                    for(int k = 0; k < 1; k++)
                    {
                        int temp = d.pos()+4;
                        d.seek(d.readInt() + headerSize);
                        JOBJ j = new JOBJ();
                        j.Read(d, this, node);
                        //jobjs.Add(j);
                        d.seek(temp);
                    }
                }
            }

        }

        Dictionary<int, PrimitiveType> primitiesTypes = new Dictionary<int, PrimitiveType>()
        {
            { 0xB8, PrimitiveType.Points},
            { 0xA8, PrimitiveType.Lines},
            { 0xB0, PrimitiveType.LineStrip},
            { 0x90, PrimitiveType.Triangles},
            { 0x98, PrimitiveType.TriangleStrip},
            { 0xA0, PrimitiveType.TriangleFan},
            { 0x80, PrimitiveType.Quads},
        };

        Dictionary<int, TextureWrapMode> GX_TEXTUREWRAP = new Dictionary<int, TextureWrapMode>()
        {
            { 0, TextureWrapMode.Clamp},
            { 1, TextureWrapMode.Repeat},
            { 2, TextureWrapMode.MirroredRepeat}
        };


        Vector3[] vertdata, nrmdata;
        Vector2[] uvdata;
        Vector4[] bonedata, coldata, weightdata;
        int[] facedata;

        public void PreRender()
        {
            List<Vector3> vert = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector4> col = new List<Vector4>();
            List<Vector3> nrm = new List<Vector3>();
            List<Vector4> bone = new List<Vector4>();
            List<Vector4> weight = new List<Vector4>();
            List<int> face = new List<int>();

            foreach (Vertex v in vertBank)
            {
                vert.Add(v.pos);
                uv.Add(v.tx0);
                col.Add(new Vector4(1, 1, 1, 1));
                nrm.Add(v.nrm);

                while (v.bones.Count < 4)
                {
                    v.bones.Add(0);
                    v.weights.Add(0);
                }

                bone.Add(new Vector4(v.bones[0], v.bones[1], v.bones[2], v.bones[3]));
                weight.Add(new Vector4(v.weights[0], v.weights[1], v.weights[2], v.weights[3]));
            }

            Queue<TreeNode> queue = new Queue<TreeNode>();
            foreach (TreeNode node in tree)
                queue.Enqueue(node);

            displayList.Clear();
            List<JOBJ> boneTrack = new List<JOBJ>();
            while (queue.Any())
            {
                TreeNode node = queue.Dequeue();

                foreach (TreeNode n in node.Nodes)
                    queue.Enqueue(n);
                if(node.Tag is DOBJ)
                    displayList.Add(node);

                if (!(node.Tag is JOBJ))
                    continue;

                JOBJ j = (JOBJ)node.Tag;
                boneTrack.Add(j);

                Bone b = new Bone();
                b.boneName = new char[] { 'b' };
                if (node.Parent.Tag is JOBJ)
                    b.parentIndex = boneTrack.IndexOf((JOBJ)node.Parent.Tag);
                else
                    b.parentIndex = -1;
                //Console.WriteLine("Where is this? " + b.parentIndex);

                b.children = new List<int>();
                b.scale = new float[3];
                b.rotation = new float[3];
                b.position = new float[3];
                b.position[0] = j.pos.X;
                b.position[1] = j.pos.Y;
                b.position[2] = j.pos.Z;
                b.scale[0] = j.sca.X;
                b.scale[1] = j.sca.Y;
                b.scale[2] = j.sca.Z;
                b.rotation[0] = j.rot.X;
                b.rotation[1] = j.rot.Y;
                b.rotation[2] = j.rot.Z;
                if (b.parentIndex != -1)
                    bones.bones[b.parentIndex].children.Add(boneTrack.IndexOf(j));
                bones.bones.Add(b);
            }
            bones.reset();
            bones.update();

            foreach (var da in displayList)
            {
                DOBJ data = (DOBJ)da.Tag;
                foreach (POBJ poly in data.polygons)
                {
                    foreach (POBJ.DisplayObject d in poly.display)
                    {
                        face.AddRange(d.faces);
                    }
                }
            }

            vertdata = vert.ToArray();
            coldata = col.ToArray();
            nrmdata = nrm.ToArray();
            uvdata = uv.ToArray();
            bonedata = bone.ToArray();
            weightdata = weight.ToArray();

            facedata = face.ToArray();
        }

        public void Render(Matrix4 modelview)
        {
            GL.UseProgram(shader.programID);

           GL.UniformMatrix4(shader.getAttribute("modelview"), false, ref modelview);

           if (bones != null)
           {
               Matrix4[] f = bones.getShaderMatrix();
               int shad = shader.getAttribute("bones");
               GL.UniformMatrix4(shad, f.Length, false, ref f[0].Row0.X);
           }

           shader.enableAttrib();

           GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
           GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
           GL.VertexAttribPointer(shader.getAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

           GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
           GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector4.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
           GL.VertexAttribPointer(shader.getAttribute("vColor"), 4, VertexAttribPointerType.Float, false, 0, 0);

           GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_nrm);
           GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(nrmdata.Length * Vector3.SizeInBytes), nrmdata, BufferUsageHint.StaticDraw);
           GL.VertexAttribPointer(shader.getAttribute("vNormal"), 3, VertexAttribPointerType.Float, false, 0, 0);

           GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_uv);
           GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(uvdata.Length * Vector2.SizeInBytes), uvdata, BufferUsageHint.StaticDraw);
           GL.VertexAttribPointer(shader.getAttribute("vUV"), 2, VertexAttribPointerType.Float, false, 0, 0);

           GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_bone);
           GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(bonedata.Length * Vector4.SizeInBytes), bonedata, BufferUsageHint.StaticDraw);
           GL.VertexAttribPointer(shader.getAttribute("vBone"), 4, VertexAttribPointerType.Float, false, 0, 0);

           GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_weight);
           GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(weightdata.Length * Vector4.SizeInBytes), weightdata, BufferUsageHint.StaticDraw);
           GL.VertexAttribPointer(shader.getAttribute("vWeight"), 4, VertexAttribPointerType.Float, false, 0, 0);

           GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
           GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(facedata.Length * sizeof(int)), facedata, BufferUsageHint.StaticDraw);

           int indiceat = 0;

            foreach (var da in displayList)
            {
                DOBJ data = da.Tag as DOBJ;
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, data.material.texture.texid);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GX_TEXTUREWRAP[data.material.texture.wrap_s]);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GX_TEXTUREWRAP[data.material.texture.wrap_t]);
                GL.Uniform1(shader.getAttribute("tex"), 0);

                GL.Uniform2(shader.getAttribute("uvscale"), new Vector2(data.material.texture.scale_w, data.material.texture.scale_h));

                foreach (POBJ poly in data.polygons)
                {
                    foreach (POBJ.DisplayObject d in poly.display)
                    {
                        if(da.Checked)
                            GL.DrawElements(primitiesTypes[d.type], d.faces.Count, DrawElementsType.UnsignedInt, indiceat * sizeof(int));
                        indiceat += d.faces.Count;
                    }
                }
            }

            shader.disableAttrib();
            
        }

        /*
            Header
            Data Block
            Relocation Table
            Root Nodes (2)
            String Table
         */

        private class Header
        {
            public int fileSize = 0;
            public int dataBlockSize = 0;
            public int relocationTableCount = 0;
            public int rootCount = 0;
            public int referenceNodeCount = 0;
            public int unk1 = 0;
            public int unk2 = 0;
            public int unk3 = 0;

            public void Read(FileData d)
            {
                fileSize = d.readInt();
                dataBlockSize = d.readInt();
                relocationTableCount = d.readInt();
                rootCount = d.readInt();
                referenceNodeCount = d.readInt();
                unk1 = d.readInt();
                unk2 = d.readInt();
                unk3 = d.readInt();
            }
        }

        public class JOBJ
        {
            TreeNode node = new TreeNode();

            public int unk1 = 0; // padding?
            public int flags = 0x1004008E;
            public int childOffset = 0;
            public int nextOffset = 0;

            public int dobjOffset = 0;
            public Vector3 rot = new Vector3(), sca = new Vector3(), pos = new Vector3();
            public Matrix4 inverseTransform;
            public int unk2; //?? padding again??
            
            public Matrix4 transform = new Matrix4();

            public JOBJ()
            {

            }

            public void Read(FileData d, DAT dat, TreeNode parentNode)
            {
                dat.jobjOffsetLinker.Add(d.pos(), this);
                unk1 = d.readInt();
                flags = d.readInt();
                childOffset = d.readInt() + headerSize;
                nextOffset = d.readInt() + headerSize;
                dobjOffset = d.readInt() + headerSize;
                rot.X = d.readFloat();
                rot.Y = d.readFloat();
                rot.Z = d.readFloat();
                sca.X = d.readFloat();
                sca.Y = d.readFloat();
                sca.Z = d.readFloat();
                pos.X = d.readFloat();
                pos.Y = d.readFloat();
                pos.Z = d.readFloat();
                d.skip(8); // matrix offset?
                inverseTransform.M11 = d.readFloat();
                inverseTransform.M12 = d.readFloat();
                inverseTransform.M13 = d.readFloat();
                inverseTransform.M14 = d.readFloat();
                inverseTransform.M21 = d.readFloat();
                inverseTransform.M22 = d.readFloat();
                inverseTransform.M23 = d.readFloat();
                inverseTransform.M24 = d.readFloat();
                inverseTransform.M31 = d.readFloat();
                inverseTransform.M32 = d.readFloat();
                inverseTransform.M33 = d.readFloat();
                inverseTransform.M34 = d.readFloat();
                inverseTransform.M44 = 1;
                unk2 = d.readInt();

                transform = Matrix4.CreateScale(sca)
                                    * Matrix4.CreateFromQuaternion(VBN.FromEulerAngles(rot.Z, rot.Y, rot.X))
                                    * Matrix4.CreateTranslation(pos) * (parentNode.Tag is JOBJ ? ((JOBJ)parentNode.Tag).transform : Matrix4.CreateScale(1, 1, 1));
                inverseTransform = transform.Inverted(); // screw the given transform

                //dat.jobjs.Add(this);
                node.Text = "Bone_" + parentNode.Nodes.Count;
                node.Tag = this;
                parentNode.Nodes.Add(node);

                if (nextOffset != headerSize)
                {
                    d.seek(nextOffset);
                    JOBJ j = new JOBJ();
                    j.Read(d, dat, parentNode);
                }
                if (childOffset != headerSize)
                {
                    d.seek(childOffset);
                    JOBJ j = new JOBJ();
                    j.Read(d, dat, node);
                }

                if (dobjOffset != headerSize)
                {
                    Console.WriteLine("DOBJ" + dobjOffset.ToString("X"));
                    d.seek(dobjOffset);
                    DOBJ dobj = new DOBJ();
                    dobj.Read(d, dat, node);
                }
            }
        }

        // DATA (Contains mesh and material)
        public class DOBJ
        {
            TreeNode node = new TreeNode();
            public int unk1 = 0; // padding?
            public int nextOffset = 0;
            public int mobjOffset = 0;
            public int pobjOffset = 0;

            public List<POBJ> polygons = new List<POBJ>();
            public MOBJ material = new MOBJ();

            public void Read(FileData d, DAT dat, TreeNode parent)
            {
                //if (dat.dobjs.Count > 0) return;
                //dat.dobjs.Add(this);
                unk1 = d.readInt();
                nextOffset = d.readInt() + headerSize;
                mobjOffset = d.readInt() + headerSize;
                pobjOffset = d.readInt() + headerSize;
                
                d.seek(pobjOffset);
                POBJ p = new POBJ();
                p.Read(d, dat, this, node);

                d.seek(mobjOffset);
                material.Read(d, dat);

                node.Text = "Mesh";
                node.Tag = this;
                node.Checked = true;
                parent.Nodes.Add(node);

                if (nextOffset != headerSize)
                {
                    d.seek(nextOffset);
                    DOBJ de = new Smash_Forge.DAT.DOBJ();
                    de.Read(d, dat, parent);
                }
            }

            public class MOBJ
            {
                public int unk1 = 0, unk2 = 0;
                public int tobjOffset, materialOffset;
                public int unk3 = 0, unk4 = 0;

                public TOBJ texture = new TOBJ();

                public void Read(FileData d, DAT dat)
                {
                    unk1 = d.readInt();
                    unk2 = d.readInt();
                    tobjOffset = d.readInt() + headerSize;
                    materialOffset = d.readInt() + headerSize;
                    unk3 = d.readInt();
                    unk4 = d.readInt();

                    d.seek(tobjOffset);
                    if(d.pos() > headerSize)
                    texture.Read(d, dat);
                }
            }

            public class TOBJ
            {
                public int unk1 = 0;
                public int wrap_s = 0, wrap_t = 0;
                public int scale_w = 0, scale_h = 0;
                public int imageOffset, paletteOffset;
                public int unk2, unkOffset;

                public Bitmap image;
                public int texid;

                public int imageDataOffset;
                public int width, height, format;

                public int paletteDataOffset;
                public int paletteFormat;
                public int unk;
                public int count;
                public int unkown;

                public void Read(FileData d, DAT dat)
                {
                    for (int i = 0; i < 13; i++)
                        d.skip(4); // ?? TODO:

                    wrap_s = d.readInt();
                    wrap_t = d.readInt();
                    scale_w = d.readByte();
                    scale_h = d.readByte();
                    d.skip(2);
                    d.skip(12);
                    imageOffset = d.readInt() + headerSize;
                    paletteOffset = d.readInt() + headerSize;
                    unkOffset = d.readInt() + headerSize;

                    d.seek(imageOffset);
                    imageDataOffset = d.readInt() + headerSize;
                    width = d.readShort();
                    height = d.readShort();
                    format = d.readInt();

                    d.seek(paletteOffset);
                    paletteDataOffset = d.readInt() + headerSize;
                    paletteFormat = d.readInt();
                    unk = d.readInt();
                    count = d.readShort();
                    unkown = d.readShort();

                    texid = NUT.loadImage(
                        TPL.ConvertFromTextureMelee(d.getSection(imageDataOffset, TPL.textureByteSize((TPL_TextureFormat)format, width, height)), 
                        width, height, format,
                        paletteOffset == headerSize ? null : d.getSection(paletteDataOffset, 4*count), count, paletteFormat)
                        );
                }
            }
        }

        public enum GXAttrType
        {
            GX_NONE = 0,
            GX_DIRECT = 1,
            GX_INDEX8 = 2,
            GX_INDEX16 = 3
        }
        public Dictionary<GXAttrType, int> GXAttrSize = new Dictionary<GXAttrType, int>()
        {
            { GXAttrType.GX_NONE, 0},
            { GXAttrType.GX_DIRECT, 1},
            { GXAttrType.GX_INDEX8, 1},
            { GXAttrType.GX_INDEX16, 2}
        };

        public static Dictionary<int, int> GX_COMPSIZE = new Dictionary<int, int>()
            {
                { 0, 1},
                { 1, 1},
                { 2, 2},
                { 3, 2},
                { 4, 4}
            };

        public class VertexAttr
        {
            public int vtxAttr;
            public GXAttrType vtxAttrType;
            public int compCnt;
            public int compType;

            public byte scale = 0xFF;
            public byte unknown = 0x00;
            public short vtxStride;
            public int dataOffset = 0;

            public VertexAttr Read(FileData d)
            {
                vtxAttr = d.readInt();
                if (vtxAttr == 0xFF)
                    return null;
                vtxAttrType = (GXAttrType)d.readInt();
                compCnt = d.readInt();
                compType = d.readInt();
                scale = (byte)d.readByte();
                unknown = (byte)d.readByte();
                vtxStride = (short)d.readShort();
                dataOffset = d.readInt() + headerSize;

                Console.WriteLine((GXAttr)vtxAttr + " " + vtxAttrType + " comp type " + compType + " Data Offset: " + dataOffset.ToString("x") + " Scale: " + scale);

                return this;
            }
        }

        public class Vertex
        {
            public Vector3 pos = new Vector3();
            public Vector3 nrm = new Vector3();
            public Vector2 tx0 = new Vector2();
            public List<int> bones = new List<int>();
            public List<float> weights = new List<float>();
        }

        public List<Vertex> vertBank = new List<Vertex>();


        public enum GXAttr
        {
            GX_VA_PNMTXIDX = 0,    // position/normal matrix index
            GX_VA_TEX0MTXIDX,      // texture 0 matrix index
            GX_VA_TEX1MTXIDX,      // texture 1 matrix index
            GX_VA_TEX2MTXIDX,      // texture 2 matrix index
            GX_VA_TEX3MTXIDX,      // texture 3 matrix index
            GX_VA_TEX4MTXIDX,      // texture 4 matrix index
            GX_VA_TEX5MTXIDX,      // texture 5 matrix index
            GX_VA_TEX6MTXIDX,      // texture 6 matrix index
            GX_VA_TEX7MTXIDX,      // texture 7 matrix index
            GX_VA_POS = 9,    // position
            GX_VA_NRM,             // normal
            GX_VA_CLR0,            // color 0
            GX_VA_CLR1,            // color 1
            GX_VA_TEX0,            // input texture coordinate 0
            GX_VA_TEX1,            // input texture coordinate 1
            GX_VA_TEX2,            // input texture coordinate 2
            GX_VA_TEX3,            // input texture coordinate 3
            GX_VA_TEX4,            // input texture coordinate 4
            GX_VA_TEX5,            // input texture coordinate 5
            GX_VA_TEX6,            // input texture coordinate 6
            GX_VA_TEX7,            // input texture coordinate 7
            GX_POS_MTX_ARRAY,      // position matrix array pointer
            GX_NRM_MTX_ARRAY,      // normal matrix array pointer
            GX_TEX_MTX_ARRAY,      // texture matrix array pointer
            GX_LIGHT_ARRAY,        // light parameter array pointer
            GX_VA_NBT,             // normal, bi-normal, tangent 
            GX_VA_MAX_ATTR,        // maximum number of vertex attributes
            GX_VA_NULL = 0xff  // NULL attribute (to mark end of lists)
        }

        // POLYGON
        public class POBJ
        {
            TreeNode node = new TreeNode();

            public int unk1 = 0; // padding?
            public int nextOffset = 0;
            public int vertexAttrArray = 0;
            public short flags = 0;
            public short displayListSize = 0;

            public int displayListOffset = 0;
            public int weightListOffset = 0;

            // format
            List<VertexAttr> vertexAttributes = new List<VertexAttr>();

            public class DisplayObject
            {
                public int type;
                public Vertex[] verts;
                public List<int> faces = new List<int>();
            }
            public List<DisplayObject> display = new List<DisplayObject>();


            public void Read(FileData d, DAT dat, DOBJ dobj, TreeNode parent)
            {
                dobj.polygons.Add(this);

                node.Text = "Polygon";
                node.Tag = this;
                parent.Nodes.Add(node);

                unk1 = d.readInt();
                nextOffset = d.readInt() + headerSize;
                vertexAttrArray = d.readInt() + headerSize;
                flags = (short)d.readShort();
                displayListSize = (short)d.readShort();
                displayListOffset = d.readInt() + headerSize;
                weightListOffset = d.readInt() + headerSize;

                // vertex attributes
                d.seek(vertexAttrArray);
                VertexAttr a = new VertexAttr();
                a.Read(d);
                while (a.vtxAttr != 0xFF)
                {
                    vertexAttributes.Add(a);
                    a = new VertexAttr();
                    a.Read(d);
                }

                //Console.WriteLine(weightListOffset.ToString("x"));

                // display list
                d.seek(displayListOffset);
                //Console.WriteLine("Display 0x" + displayListOffset);
                int bid = 0;
                List<Vertex> used = new List<Vertex>();
                if (displayListOffset != headerSize)
                    for (int i = 0; i < displayListSize - 1; i++)
                    {
                        byte prim = (byte)d.readByte();
                        int count = d.readShort();

                        if (prim == 0)
                            break;

                        DisplayObject ob = new DisplayObject();
                        ob.type = prim;

                        //List<Vertex> vcol = new List<Vertex>();
                        for (int j = 0; j < count; j++)
                        {
                            bid = -1;
                            float[] f;
                            int temp;
                            Vertex v = new Vertex();
                            dat.vertBank.Add(v);
                            ob.faces.Add(dat.vertBank.IndexOf(v));
                            foreach (VertexAttr att in vertexAttributes)
                            {
                                int value = 0;
                                switch (att.vtxAttrType)
                                {
                                    case GXAttrType.GX_DIRECT:
                                        if(att.vtxAttr != (int)GXAttr.GX_VA_CLR0)
                                            value = d.readByte();
                                        else
                                        {
                                            switch (att.compType)
                                            {
                                                case 0: d.skip(2); break;
                                                case 1: d.skip(3); break;
                                                case 2: d.skip(3); break;
                                                case 3: d.skip(2); break;
                                                case 4: d.skip(2); break;
                                                case 5: d.skip(4); break;
                                            }
                                        }
                                        break;
                                    case GXAttrType.GX_INDEX8:
                                        value = d.readByte();
                                        break;
                                    case GXAttrType.GX_INDEX16:
                                        value = d.readShort();
                                        break;
                                }

                                switch ((GXAttr)att.vtxAttr)
                                {
                                    case GXAttr.GX_VA_PNMTXIDX:
                                        bid = value;
                                        break;
                                    case GXAttr.GX_VA_POS:
                                        // here add face and vertex for rendering......
                                        int vr = value;

                                        int te = d.pos();
                                        d.seek(att.dataOffset + att.vtxStride * vr);
                                        f = read3(d, att.compType, att.vtxStride);
                                        v.pos.X = f[0];
                                        v.pos.Y = f[1];
                                        v.pos.Z = f[2];
                                        v.pos = Vector3.Divide(v.pos, (float)Math.Pow(2, att.scale));
                                        d.seek(te);

                                        // sometimes we need to transform the default vertex by
                                        // the normal transform; I plan on storing it in a normal way

                                        if (weightListOffset > headerSize && bid > -1)
                                        {
                                            temp = d.pos();
                                            d.seek(weightListOffset + (bid / 3) * 4);
                                            d.seek(d.readInt() + headerSize);
                                            if (d.pos() > headerSize)
                                            {
                                                int off1 = d.readInt() + headerSize;
                                                float wei1 = d.readFloat();
                                                Vector3 newp = Vector3.Zero;

                                                while (off1 > headerSize)
                                                {
                                                    newp += Vector3.Transform(v.pos, dat.jobjOffsetLinker[off1].transform) * wei1;
                                                    //v.bones.Add(dat.jobjs.IndexOf(dat.jobjOffsetLinker[off1]));
                                                    v.weights.Add(wei1);
                                                    off1 = d.readInt() + headerSize;
                                                    wei1 = d.readFloat();
                                                }

                                                if (v.weights.Count == 1)
                                                    v.pos = newp;
                                            }

                                            d.seek(temp);
                                        }
                                        break;

                                    case GXAttr.GX_VA_NRM:
                                        int ten = d.pos();
                                        d.seek(att.dataOffset + att.vtxStride * value);
                                        f = read3(d, att.compType, att.vtxStride);
                                        v.nrm.X = f[0];
                                        v.nrm.Y = f[1];
                                        v.nrm.Z = f[2];
                                        v.nrm = Vector3.Divide(v.nrm, (float)Math.Pow(2, att.scale));
                                        d.seek(ten);

                                        /*if (weightListOffset > headerSize && bid > -1)
                                        {
                                            temp = d.pos();
                                            d.seek(weightListOffset + (bid / 3) * 4);
                                            d.seek(d.readInt() + headerSize);
                                            if (d.pos() > headerSize)
                                            {
                                                int off1 = d.readInt() + headerSize;
                                                float wei1 = d.readFloat();
                                                Vector3 newp = Vector3.Zero;

                                                while (off1 > headerSize)
                                                {
                                                    newp += Vector3.Transform(v.nrm, dat.jobjOffsetLinker[off1].transform) * wei1;
                                                    off1 = d.readInt() + headerSize;
                                                    wei1 = d.readFloat();
                                                }

                                                if (v.weights.Count == 1)
                                                    v.nrm = newp;
                                            }

                                            d.seek(temp);
                                        }*/
                                        break;

                                    case GXAttr.GX_VA_TEX0:
                                        temp = d.pos();
                                        d.seek(att.dataOffset + att.vtxStride * value);
                                        f = read3(d, att.compType, att.vtxStride / GX_COMPSIZE[att.compType]);
                                        v.tx0.X = f[0];
                                        v.tx0.Y = f[1];
                                        v.tx0 = Vector2.Divide(v.tx0, (float)Math.Pow(2, att.scale));
                                        d.seek(temp);
                                        break;

                                    case GXAttr.GX_VA_TEX1:
                                        temp = d.pos();
                                        d.seek(att.dataOffset + att.vtxStride * value);
                                        f = read3(d, att.compType, att.vtxStride / GX_COMPSIZE[att.compType]);
                                        v.tx0.X = f[0];
                                        v.tx0.Y = f[1];
                                        v.tx0 = Vector2.Divide(v.tx0, (float)Math.Pow(2, att.scale));
                                        d.seek(temp);
                                        break;
                                }
                            }
                        }

                        display.Add(ob);
                    }

                if (nextOffset != headerSize)
                {
                    d.seek(nextOffset);
                    POBJ pol = new POBJ();
                    pol.Read(d, dat, dobj, parent);
                }
            }

            public static float[] read3(FileData d, int type, int size)
            {
                float[] a = new float[size];

                switch (type)
                {
                    case 0:
                        for(int i = 0; i < size; i++)
                            a[i] = d.readByte();
                        break;
                    case 1:
                        for (int i = 0; i < size; i++)
                            a[i] = (byte)(d.readByte());
                        break;
                    case 2:
                        for (int i = 0; i < size; i++)
                            a[i] = (d.readShort());
                        break;
                    case 3:
                        for (int i = 0; i < size; i++)
                            a[i] = (short)(d.readShort());
                        break;
                    case 4:
                        for (int i = 0; i < size; i++)
                            a[i] = d.readFloat();
                        break;
                }

                return a;
            }




            #region Textures

            public static byte[] fromCMP(byte[] tpl, int width, int height)
            {
                uint[] output = new uint[width * height];
                ushort[] c = new ushort[4];
                int[] pix = new int[4];
                int inp = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int ww = width; //Shared.AddPadding(width, 8);

                        if (ww % 8 != 0)
                        {
                            ww = ww + (8 - (ww % 8));
                        }

                        int x0 = x & 0x03;
                        int x1 = (x >> 2) & 0x01;
                        int x2 = x >> 3;

                        int y0 = y & 0x03;
                        int y1 = (y >> 2) & 0x01;
                        int y2 = y >> 3;

                        int off = (8 * x1) + (16 * y1) + (32 * x2) + (4 * ww * y2);

                        c[0] = Swap(BitConverter.ToUInt16(tpl, off));
                        c[1] = Swap(BitConverter.ToUInt16(tpl, off + 2));

                        if (c[0] > c[1])
                        {
                            c[2] = (ushort)avg(2, 1, c[0], c[1]);
                            c[3] = (ushort)avg(1, 2, c[0], c[1]);
                        }
                        else
                        {
                            c[2] = (ushort)avg(1, 1, c[0], c[1]);
                            c[3] = 0;
                        }

                        uint pixel = Swap(BitConverter.ToUInt32(tpl, off + 4));

                        int ix = x0 + (4 * y0);
                        int raw = c[(pixel >> (30 - (2 * ix))) & 0x03];

                        pix[0] = (raw >> 8) & 0xf8;
                        pix[1] = (raw >> 3) & 0xf8;
                        pix[2] = (raw << 3) & 0xf8;
                        pix[3] = 0xff;
                        if (((pixel >> (30 - (2 * ix))) & 0x03) == 3 && c[0] <= c[1]) pix[3] = 0x00;

                        output[inp] = (uint)((pix[0] << 16) | (pix[1] << 8) | (pix[2] << 0) | (pix[3] << 24));
                        inp++;
                    }
                }

                return UIntArrayToByteArray(output);
            }

            public static Bitmap rgbaToImage(byte[] data, int width, int height)
            {
                if (width == 0) width = 1;
                if (height == 0) height = 1;

                Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                try
                {
                    System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                                         new Rectangle(0, 0, bmp.Width, bmp.Height),
                                         System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

                    System.Runtime.InteropServices.Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
                    bmp.UnlockBits(bmpData);
                }
                catch { bmp.Dispose(); throw; }

                return bmp;
            }

            public static byte[] UIntArrayToByteArray(uint[] array)
            {
                List<byte> results = new List<byte>();
                foreach (uint value in array)
                {
                    byte[] converted = BitConverter.GetBytes(value);
                    results.AddRange(converted);
                }
                return results.ToArray();
            }

            public static ushort Swap(ushort value)
            {
                return (ushort)IPAddress.HostToNetworkOrder((short)value);
            }

            public static uint Swap(uint value)
            {
                return (uint)IPAddress.HostToNetworkOrder((int)value);
            }

            public static int avg(int w0, int w1, int c0, int c1)
            {
                int a0 = c0 >> 11;
                int a1 = c1 >> 11;
                int a = (w0 * a0 + w1 * a1) / (w0 + w1);
                int c = (a << 11) & 0xffff;

                a0 = (c0 >> 5) & 63;
                a1 = (c1 >> 5) & 63;
                a = (w0 * a0 + w1 * a1) / (w0 + w1);
                c = c | ((a << 5) & 0xffff);

                a0 = c0 & 31;
                a1 = c1 & 31;
                a = (w0 * a0 + w1 * a1) / (w0 + w1);
                c = c | a;

                return c;
            }

            #endregion
        }
    }
}
