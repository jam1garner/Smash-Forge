using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Smash_Forge
{
    public class MBN : FileBase
    {
        enum Format 
        {
            Character = 6,
            Stage = 4
        }

        int format = 6;
        ushort unkown = 0xFFFF;
        int flags = 0;
        int mode = 1;
        public List<Mesh> mesh = new List<Mesh>();
        public List<Vertex> vertices = new List<Vertex>();
        public List<string> nameTable = new List<string>();

        public static Shader shader = null;

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
uniform bones
{{
    mat4 transforms[{0}];
}} bones_;
 
void
main()
{{
    ivec4 index = ivec4(vBone); 
    vec4 objPos = vec4(vPosition.xyz, 1.0);

    if(vBone.x != -1)
    {{
        objPos = bones_.transforms[index.x] * vec4(vPosition, 1.0) * vWeight.x;
        objPos += bones_.transforms[index.y] * vec4(vPosition, 1.0) * vWeight.y;
        objPos += bones_.transforms[index.z] * vec4(vPosition, 1.0) * vWeight.z;
        objPos += bones_.transforms[index.w] * vec4(vPosition, 1.0) * vWeight.w;
    }}

    gl_Position = modelview * vec4(objPos.xyz, 1.0);

    f_texcoord = vUV;
    color = vColor;
    normal = dot(vec4(vNormal * mat3(modelview), 1.0), vec4(0.3,0.3,0.3,1.0)) ;
}}";

        static string fs = @"#version 330

in vec2 f_texcoord;
in vec4 color;
in float normal;

uniform sampler2D tex;
uniform vec2 uvscale;

void
main()
{{
    vec4 alpha = texture(tex, f_texcoord*uvscale).aaaa;
    gl_FragColor = vec4 ((color * alpha * texture(tex, f_texcoord*uvscale) * normal).xyz, alpha.a * color.w);
}}
";

        public List<Descriptor> descript = new List<Descriptor>(); // Descriptors are used to describe the vertex data...


        // Rendering
        int vbo_position;
        int vbo_color;
        int vbo_nrm;
        int vbo_uv;
        int vbo_weight;
        int vbo_bone;
        int ibo_elements;

        Vector2[] uvdata;
        Vector3[] vertdata, nrmdata;
        Vector4[] bonedata, coldata, weightdata;
        int[] facedata;

        public override Endianness Endian
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public MBN()
        {
            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out vbo_color);
            GL.GenBuffers(1, out vbo_nrm);
            GL.GenBuffers(1, out vbo_uv);
            GL.GenBuffers(1, out vbo_bone);
            GL.GenBuffers(1, out vbo_weight);
            GL.GenBuffers(1, out ibo_elements);
        }

        public void Destroy()
        {
            GL.DeleteBuffer(vbo_position);
            GL.DeleteBuffer(vbo_color);
            GL.DeleteBuffer(vbo_nrm);
            GL.DeleteBuffer(vbo_uv);
            GL.DeleteBuffer(vbo_weight);
            GL.DeleteBuffer(vbo_bone);
        }

        public void PreRender()
        {
            List<Vector3> vert = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector4> col = new List<Vector4>();
            List<Vector3> nrm = new List<Vector3>();
            List<Vector4> bone = new List<Vector4>();
            List<Vector4> weight = new List<Vector4>();
            List<int> face = new List<int>();

            int vi = 0;
            foreach (Vertex v in vertices)
            {
                vert.Add(v.pos);
                nrm.Add(v.nrm);
                col.Add(v.col / 0x7F);
                if (v.tx.Count > 0)
                    uv.Add(new Vector2(v.tx[0].X, 1 - v.tx[0].Y));
                else
                    uv.Add(new Vector2(0, 0));
                // TODO: Bones
                //Console.WriteLine(v.node.Count);
                if (v.node.Count > 1)
                {
                    foreach (Mesh m in mesh)
                        foreach (List<int> l in m.faces)
                            if (l.Contains(vi))
                            {
                                List<int> nl = m.nodeList[m.faces.IndexOf(l)];
                                bone.Add(new Vector4(nl[v.node[0]], nl[v.node[1]], 0, 0));
                                weight.Add(new Vector4(v.weight[0], v.weight[1], 0, 0));
                                goto loop;
                            }
                }
                else
                {
                    bone.Add(new Vector4(-1, 0, 0, 0));
                    weight.Add(new Vector4(0, 0, 0, 0));
                }

                loop:
                vi++;
            }

            foreach (Mesh m in mesh)
            {
                foreach (List<int> l in m.faces)
                    face.AddRange(l);
            }

            vertdata = vert.ToArray();
            coldata = col.ToArray();
            nrmdata = nrm.ToArray();
            uvdata = uv.ToArray();
            facedata = face.ToArray();
            bonedata = bone.ToArray();
            weightdata = weight.ToArray();

            SetupShader();
        }

        private void SetupShader()
        {
            int maxUniformBlockSize = GL.GetInteger(GetPName.MaxUniformBlockSize);

            if (shader == null)
            {
                shader = new Shader();

                shader.vertexShader(string.Format(vs, 0));
                shader.fragmentShader(string.Format(fs));
            }
        }

        public void Render(Matrix4 modelview)
        {
            if (null == shader)
                return;

            GL.UseProgram(shader.programID);

            GL.UniformMatrix4(shader.getAttribute("modelview"), false, ref modelview);

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
            foreach (Mesh m in mesh)
            {
                GL.Uniform4(shader.getAttribute("colorSamplerUV"), new Vector4(1, 1, 0, 0));

                GL.BindTexture(TextureTarget.Texture2D, m.texId);
                GL.Uniform1(shader.getAttribute("tex"), 0);

                foreach (List<int> l in m.faces)
                {
                    if (m.isVisible)
                        GL.DrawElements(PrimitiveType.Triangles, l.Count, DrawElementsType.UnsignedInt, indiceat * sizeof(int));

                    indiceat += l.Count;
                }
            }
            shader.disableAttrib();
        }

        /**
         * Reading and saving --------------------
        **/

        public override void Read(string filename)
        {
            FileData d = new FileData(filename);
            d.seek(0);
            d.Endian = Endianness.Little;

            format = d.readShort();
            unkown = (ushort)d.readShort();
            flags = d.readInt();
            mode = d.readInt();
            bool hasNameTable = (flags & 2) > 0;

            int polyCount = d.readInt();

            mesh = new List<Mesh>();
            descript = new List<Descriptor>();
            List<List<int>> prim = new List<List<int>>();
            for (int i = 0; i < polyCount; i++)
            {
                if (i == 0 && mode == 1)
                {
                    Descriptor des = new Descriptor();
                    des.ReadDescription(d);
                    descript.Add(des);
                }

                Mesh m = new Mesh();
                mesh.Add(m);

                int faceCount = d.readInt();
                List<int> prims = new List<int>();
                prim.Add(prims);
                for (int j = 0; j < faceCount; j++)
                {
                    int nodeCount = d.readInt();
                    List<int> nodeList = new List<int>();
                    m.nodeList.Add(nodeList);

                    for (int k = 0; k < nodeCount; k++)
                        nodeList.Add(d.readInt()); // for a node list?
                    

                    int primitiveCount = d.readInt();
                    prims.Add(primitiveCount);

                    if (hasNameTable)
                    {
                        int nameId = d.readInt();
                    }

                    if (mode == 0)
                    {
                        if (format == 4)
                        {
                            int[] buffer = new int[primitiveCount];
                            for (int k = 0; k < primitiveCount; k++)
                            {
                                buffer[k] = d.readShort();
                            }
                            d.align(4);
                            List<int> buf = new List<int>();
                            buf.AddRange(buffer);
                            m.faces.Add(buf);
                        }
                        else
                        {
                            Descriptor des = new Descriptor();
                            des.ReadDescription(d);
                            descript.Add(des);
                        }
                            
                    }
                }
            }

            if(mode == 0){
                //Console.WriteLine("Extra!");
            }

            // TODO: STRING TABLE
            if (hasNameTable)
            {
                for (int i = 0; i < mesh.Count; i++)
                {
                    int index = d.readByte();
                    nameTable.Add(d.readString());
                }
            }


            if (format != 4) d.align(32);

            // Vertex Bank
            int start = d.pos();
            for (int i = 0; i < 1; i++)
            {
                if (mode == 0 || i == 0)
                {
                    Descriptor des = descript[i];

                    if (format != 4)
                    {
                        while (d.pos() < start + des.length)
                        {
                            Vertex v = new Vertex();
                            vertices.Add(v);

                            for (int k = 0; k < des.type.Length; k++)
                            {
                                d.align(2);
                                switch(des.type[k]){
                                    case 0: //Position
                                        v.pos.X = readType(d, des.format[k], des.scale[k]);
                                        v.pos.Y = readType(d, des.format[k], des.scale[k]);
                                        v.pos.Z = readType(d, des.format[k], des.scale[k]);
                                        break;
                                    case 1: //Normal
                                        v.nrm.X = readType(d, des.format[k], des.scale[k]);
                                        v.nrm.Y = readType(d, des.format[k], des.scale[k]);
                                        v.nrm.Z = readType(d, des.format[k], des.scale[k]);
                                        break;
                                    case 2: //Color
                                        v.col.X = (int)(readType(d, des.format[k], des.scale[k]));
                                        v.col.Y = (int)(readType(d, des.format[k], des.scale[k]));
                                        v.col.Z = (int)(readType(d, des.format[k], des.scale[k]));
                                        v.col.W = (int)(readType(d, des.format[k], des.scale[k]));
                                        break;
                                    case 3: //Tex0
                                        v.tx.Add(new Vector2(readType(d, des.format[k], des.scale[k]), readType(d, des.format[k], des.scale[k])));
                                        break;
                                    case 4: //Tex1
                                        v.tx.Add(new Vector2(readType(d, des.format[k], des.scale[k]), readType(d, des.format[k], des.scale[k])));
                                        break;
                                    case 5: //Bone Index
                                        v.node.Add(d.readByte());
                                        v.node.Add(d.readByte());
                                        break;
                                    case 6: //Bone Weight
                                        v.weight.Add(readType(d, des.format[k], des.scale[k]));
                                        v.weight.Add(readType(d, des.format[k], des.scale[k]));
                                        break;
                                    //default:
                                    //    Console.WriteLine("WTF is this");
                                }
                            }
                        }
                        d.align(32);
                    }
                }

                for (int j = 0; j < mesh.Count; j++)
                {
                    foreach (int l in prim[j])
                    {
                        List<int> face = new List<int>();
                        mesh[j].faces.Add(face);
                        for (int k = 0; k < l; k++)
                            face.Add(d.readShort());
                        d.align(32);
                    }
                }

            }

            PreRender();
        }

        public override byte[] Rebuild()
        {
            FileOutput f = new FileOutput();
            f.Endian = Endianness.Little;
            FileOutput fv = new FileOutput();
            fv.Endian = Endianness.Little;

            f.writeShort(format);
            f.writeShort(unkown);
            f.writeInt(flags);
            f.writeInt(mode);
            bool hasNameTable = (flags & 2) > 0;

            f.writeInt(mesh.Count);

            int vertSize = 0;

            // Vertex Bank
            for (int i = 0; i < 1; i++)
            {
                if (mode == 0 || i == 0)
                {
                    Descriptor des = descript[i];

                    if (format != 4)
                    {
                        foreach(Vertex v in vertices)
                        {
                            for (int k = 0; k < des.type.Length; k++)
                            {
                                fv.align(2, 0x00);
                                switch(des.type[k]){
                                    case 0: //Position
                                        writeType(fv, v.pos.X, des.format[k], des.scale[k]);
                                        writeType(fv, v.pos.Y, des.format[k], des.scale[k]);
                                        writeType(fv, v.pos.Z, des.format[k], des.scale[k]);
                                        break;
                                    case 1: //Normal
                                        writeType(fv, v.nrm.X, des.format[k], des.scale[k]);
                                        writeType(fv, v.nrm.Y, des.format[k], des.scale[k]);
                                        writeType(fv, v.nrm.Z, des.format[k], des.scale[k]);
                                        break;
                                    case 2: //Color
                                        writeType(fv, v.col.X, des.format[k], des.scale[k]);
                                        writeType(fv, v.col.Y, des.format[k], des.scale[k]);
                                        writeType(fv, v.col.Z, des.format[k], des.scale[k]);
                                        writeType(fv, v.col.W, des.format[k], des.scale[k]);
                                        break;
                                    case 3: //Tex0
                                        writeType(fv, v.tx[0].X, des.format[k], des.scale[k]);
                                        writeType(fv, v.tx[0].Y, des.format[k], des.scale[k]);
                                        break;
                                    case 4: //Tex1
                                        writeType(fv, v.tx[1].X, des.format[k], des.scale[k]);
                                        writeType(fv, v.tx[1].Y, des.format[k], des.scale[k]);
                                        break;
                                    case 5: //Bone Index
                                        fv.writeByte(v.node[0]);
                                        fv.writeByte(v.node[1]);
                                        break;
                                    case 6: //Bone Weight
                                        writeType(fv, v.weight[0], des.format[k], des.scale[k]);
                                        writeType(fv, v.weight[1], des.format[k], des.scale[k]);
                                        break;
                                        //default:
                                        //    Console.WriteLine("WTF is this");
                                }
                            }
                        }
                        vertSize = fv.size();
                        fv.align(32, 0xFF);
                    }
                }


                for (int j = 0; j < mesh.Count; j++)
                {
                    foreach (List<int> l in mesh[j].faces)
                    {
                        foreach (int index in l)
                            fv.writeShort(index);
                        fv.align(32, 0xFF);
                    }
                }

            }
            

            for (int i = 0; i < mesh.Count; i++)
            {
                if (i == 0 && mode == 1)
                {
                    descript[0].WriteDescription(f);
                    f.writeInt(vertSize);
                }

                f.writeInt(mesh[i].nodeList.Count);
                //Console.WriteLine(mesh[i].faces.Count + " " + mesh[i].nodeList.Count);
                for (int j = 0; j < mesh[i].nodeList.Count; j++)
                {
                    f.writeInt(mesh[i].nodeList[j].Count);

                    for (int k = 0; k < mesh[i].nodeList[j].Count; k++)
                        f.writeInt(mesh[i].nodeList[j][k]);

                    f.writeInt(mesh[i].faces[j].Count);

                    // TODO: This stuff
                    if (hasNameTable)
                    {
                        //int nameId = d.readInt();
                    }

                    /*if (mode == 0)
                    {
                        if (format == 4)
                        {
                            int[] buffer = new int[primitiveCount];
                            for (int k = 0; k < primitiveCount; k++)
                            {
                                buffer[k] = d.readShort();
                            }
                            d.align(4);
                            List<int> buf = new List<int>();
                            buf.AddRange(buffer);
                            m.faces.Add(buf);
                        }
                        else
                        {
                            Descriptor des = new Descriptor();
                            des.ReadDescription(d);
                            descript.Add(des);
                        }

                    }*/
                }
            }

            // TODO: STRING TABLE
            /*if (hasNameTable)
            {
                for (int i = 0; i < mesh.Count; i++)
                {
                    int index = d.readByte();
                    nameTable.Add(d.readString());
                }
            }*/


            if (format != 4) f.align(32, 0xFF);

            f.writeOutput(fv);

            return f.getBytes();
        }

        public void Save(string filename)
        {
            var Data = Rebuild();
            if (Data.Length <= 0)
                throw new Exception("Warning: Data was empty!");

            File.WriteAllBytes(filename, Data);
        }

        private static float readType(FileData d, int format, float scale){
            switch(format){
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

        private static void writeType(FileOutput d, float value, int format, float scale){
            switch(format){
                case 0:
                    d.writeFloat(value / scale);
                    break;
                case 1:
                    d.writeByte((int)(value / scale));
                    break;
                case 2:
                    d.writeByte((byte)(value / scale));
                    break;
                case 3:
                    d.writeShort((short)(value / scale));
                    break;
            }
        }


        public NUD toNUD()
        {
            NUD nud = new NUD();
            int j = 0;
            foreach (Mesh m in mesh)
            {
                NUD.Mesh n_mesh = new NUD.Mesh();
                nud.mesh.Add(n_mesh);
                n_mesh.Text = "Mesh_" + j++;
                foreach (List<int> i in m.faces)
                {
                    NUD.Polygon poly = new NUD.Polygon();
                    n_mesh.Nodes.Add(poly);
                    poly.setDefaultMaterial();

                    List<Vertex> indexSim = new List<Vertex>();

                    foreach (int index in i)
                    {
                        Vertex v = vertices[index];

                        if (!indexSim.Contains(v))
                        {
                            indexSim.Add(v);
                            NUD.Vertex vert = new NUD.Vertex();
                            vert.pos = v.pos;
                            vert.nrm = v.nrm;
                            vert.col = v.col;
                            List<Vector2> uvs = new List<Vector2>();
                            uvs.Add(new Vector2(v.tx[0].X, 1 - v.tx[0].Y));
                            vert.tx = uvs;
                            if (vert.weight.Count < 4) {
                                v.weight.Add(0f);
                                v.weight.Add(0f);
                            }
                            vert.weight = v.weight;
                            List<int> nodez = new List<int>();
                            nodez.Add(m.nodeList[0][v.node[0]]);
                            nodez.Add(m.nodeList[0][v.node[1]]);
                            nodez.Add(0);
                            nodez.Add(0);
                            vert.node = nodez;
                            poly.AddVertex(vert);
                        }

                        poly.faces.Add(indexSim.IndexOf(v));
                    }
                }
            }
            return nud;
        }


        public class Vertex
        {
            public Vector3 pos = new Vector3(0, 0, 0), nrm = new Vector3(0, 0, 0);
            public Vector4 col = new Vector4(127, 127, 127, 127);
            public List<Vector2> tx = new List<Vector2>();
            public List<int> node = new List<int>();
            public List<float> weight = new List<float>();
        }

        public class Mesh
        {
            public string name = "";
            public List<List<int>> faces = new List<List<int>>();
            public List<List<int>> nodeList = new List<List<int>>();
            public int renderPriority = 0;

            // display
            public bool isVisible = true;
            public int texId = 0;
        }

        public void setDefaultDescriptor()
        {
            Descriptor d = new Descriptor();
            d.type = new int[5];
            d.format = new int[5];
            d.scale = new float[5];
            d.type[0] = 0;
            d.type[1] = 1;
            d.type[2] = 3;
            d.type[3] = 5;
            d.type[4] = 6;
            for (int i = 0; i < 5; i++)
            {
                d.format[i] = 0;
                d.scale[i] = 1f;
            }
            d.format[3] = 1;
            d.format[0] = 3;
            d.scale[0] = 0.000628672249f;
            d.format[1] = 2;
            d.scale[1] = 0.007874f;
            d.format[2] = 3;
            d.scale[2] = 0.000089f;
            d.format[4] = 1;
            d.scale[4] = 0.01f;
            descript.Add(d);
        }

        /*
         * The Descriptor contains information about the vertex stream
        */
        public class Descriptor
        {
            public int length;
            public int[] type;
            public int[] format;
            public float[] scale;

            public void ReadDescription(FileData d)
            {
                int count = d.readInt();

                type = new int[count];
                format = new int[count];
                scale = new float[count];

                for(int i = 0 ;i < count ; i++){
                    type[i] = d.readInt();
                    format[i] = d.readInt();
                    scale[i] = d.readFloat();
                }

                length = d.readInt(); 
            }

            public void WriteDescription(FileOutput f)
            {
                // TODO: Calculate Length
                f.writeInt(type.Length);

                for(int i = 0 ;i < type.Length ; i++){
                    f.writeInt(type[i]);
                    f.writeInt(format[i]);
                    f.writeFloat(scale[i]);
                }
            }
        }
    }
}

