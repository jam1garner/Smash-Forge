using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Smash_Forge
{
    class BFRES
    {
        public int readOffset(FileData f)
        { 
            return f.pos() + f.readInt();
        }
        public string readString(FileData f)
        {
            int returnOff = f.pos() + 4;
            int stringOff = readOffset(f);
            f.seek(stringOff);
            string str = f.readString();
            f.seek(returnOff);
            return str;
        }
        public BFRES()
        {
            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out vbo_color);
            GL.GenBuffers(1, out vbo_nrm);
            GL.GenBuffers(1, out vbo_uv);
            GL.GenBuffers(1, out vbo_bone);
            GL.GenBuffers(1, out vbo_weight);
            GL.GenBuffers(1, out ibo_elements);
        }
        public BFRES(string fname) : this()
        {
            Read(fname);
            PreRender();
        }
        // gl buffer objects
        int vbo_position;
        int vbo_color;
        int vbo_nrm;
        int vbo_uv;
        int vbo_weight;
        int vbo_bone;
        int ibo_elements;

        Vector2[] uvdata;
        Vector3[] vertdata, nrmdata;
        int[] facedata;
        Vector4[] bonedata, coldata, weightdata;

        public List<Mesh> mesh = new List<Mesh>();
        public  void Read(string filename)
        {
            FileData f = new FileData(filename);
            f.Endian = Endianness.Big;
            f.seek(0);

            f.seek(4);
            int highVersion = f.readByte();
            int lowVersion = f.readByte();
            int overallVerstion = f.readShort();
            short BOM = (short)f.readShort();
            f.skip(6);
            int fileAlignment = f.readInt();
            int nameOffset = readOffset(f);
            int strTblLenth = f.readInt();
            int strTblOffset = readOffset(f);

            int FMDLOffset = readOffset(f);
            int FTEXOffset = readOffset(f);
            int FSKAOffset = readOffset(f);
            int FSHU0Offset = readOffset(f);
            int FSHU1Offset = readOffset(f);
            int FSHU2Offset = readOffset(f);
            int FTXPOffset = readOffset(f);
            int FVIS0Offset = readOffset(f);
            int FVIS1Offset = readOffset(f);
            int FSHAOffset = readOffset(f);
            int FSCNOffset = readOffset(f);
            int EMBOffset = readOffset(f);

            int FMDLCount = f.readShort();
            int FTEXCount = f.readShort();
            int FSKACount = f.readShort();
            int FSHU0Count = f.readShort();
            int FSHU1Count = f.readShort();
            int FSHU2Count = f.readShort();
            int FTXPCount = f.readShort();
            int FVIS0Count = f.readShort();
            int FVIS1Count = f.readShort();
            int FSHACount = f.readShort();
            int FSCNCount = f.readShort();
            int EMBCount = f.readShort();






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

            int i = 0;

            foreach (Mesh m in mesh)
            {
                foreach (Polygon p in m.polygons)
                {
                    if (p.faces.Count <= 3)
                        continue;
                    foreach (Vertex v in p.vertices)
                    {
                        vert.Add(v.pos);
                        col.Add(v.col);
                        nrm.Add(v.nrm);

                        uv.Add(v.tx[0]);

                        while (v.node.Count < 4)
                        {
                            v.node.Add(0);
                            v.weight.Add(0);
                        }
                        bone.Add(new Vector4(v.node[0], v.node[1], v.node[2], v.node[3]));
                        weight.Add(new Vector4(v.weight[0], v.weight[1], v.weight[2], v.weight[3]));
                    }

                    // rearrange faces
                    int[] ia = p;
                    for (int j = 0; j < ia.Length; j++)
                    {
                        ia[j] += i;
                    }
                    face.AddRange(ia);
                    i += p.vertices.Count;
                }
            }

            vertdata = vert.ToArray();
            coldata = col.ToArray();
            nrmdata = nrm.ToArray();
            uvdata = uv.ToArray();
            facedata = face.ToArray();
            bonedata = bone.ToArray();
            weightdata = weight.ToArray();

        }
        public class Vertex
        {
            public Vector3 pos = new Vector3(0, 0, 0), nrm = new Vector3(0, 0, 0);
            public Vector4 bitan = new Vector4(0, 0, 0, 1), tan = new Vector4(0, 0, 0, 1);
            public Vector4 col = new Vector4(127, 127, 127, 127);
            public List<Vector2> tx = new List<Vector2>();
            public List<int> node = new List<int>();
            public List<float> weight = new List<float>();

            public Vertex()
            {
            }

            public Vertex(float x, float y, float z)
            {
                pos = new Vector3(x, y, z);
            }
        }
        public class fmdlh
        {
            string fmdl;
            string name;

        }
        public class Polygon
        {
            public List<Vertex> vertices = new List<Vertex>();
            public List<int> faces = new List<int>();
            public int displayFaceSize = 0;

            public bool isVisible = true;

            // for nud stuff
            public int vertSize = 0x46; // defaults to a basic bone weighted vertex format
            public int UVSize = 0x12;
            public int strip = 0x40;
            public int polflag = 0x04;

            public void AddVertex(Vertex v)
            {
                vertices.Add(v);
            }
        }
            public class Mesh
        {
            public string name;
            public List<Polygon> polygons = new List<Polygon>();
            public int boneflag = 4; // 0 not rigged 4 rigged 8 singlebind
            public short singlebind = -1;

            public bool isVisible = true;
            public float[] bbox = new float[8];

            public void addVertex(Vertex v)
            {
                if (polygons.Count == 0)
                    polygons.Add(new Polygon());

                polygons[0].AddVertex(v);
            }
        }
    }
}
