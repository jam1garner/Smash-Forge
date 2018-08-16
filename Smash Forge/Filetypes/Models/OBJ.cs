using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using OpenTK;

namespace Smash_Forge
{
    class OBJ
    {
        public List<OBJObject> objects;

        public OBJ()
        {
            objects = new List<OBJObject>();
        }

        public List<Vector3> v = new List<Vector3>();
        public List<Vector2> vt = new List<Vector2>();
        public List<Vector3> vn = new List<Vector3>();

        public class OBJObject
        {
            public string name = "None";
            public List<OBJGroup> groups = new List<OBJGroup>();
        }

        public class OBJGroup
        {
            public List<int> v = new List<int>();
            public List<int> vt = new List<int>();
            public List<int> vn = new List<int>();
        }

        public void Read(string fname)
        {
            string input = File.ReadAllText(fname);

            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            input = regex.Replace(input, " ");

            string[] lines = input.Split('\n');

            Vector3 v;
            OBJObject o = null;
            OBJGroup g = null;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] args = lines[i].Split(' ');
                switch (args[0])
                {
                    case "v":
                        if (o == null)
                        {
                            o = new OBJObject();
                            g = new OBJGroup();
                            o.groups.Add(g);
                            objects.Add(o);
                        }
                        v = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                        this.v.Add(v);
                        break;
                    case "vn":
                        v = new Vector3(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));
                        vn.Add(v);
                        break;
                    case "vt":
                        vt.Add(new Vector2(float.Parse(args[1]), float.Parse(args[2])));
                        break;
                    case "f":
                        g.v.Add(int.Parse(args[1].Split('/')[0]) - 1);
                        g.v.Add(int.Parse(args[2].Split('/')[0]) - 1);
                        g.v.Add(int.Parse(args[3].Split('/')[0]) - 1);
                        if (args[1].Split('/').Length > 1)
                        {
                            g.vt.Add(int.Parse(args[1].Split('/')[1]) - 1);
                            g.vt.Add(int.Parse(args[2].Split('/')[1]) - 1);
                            g.vt.Add(int.Parse(args[3].Split('/')[1]) - 1);
                        }
                        if (args[1].Split('/').Length > 2)
                        {
                            g.vn.Add(int.Parse(args[1].Split('/')[2]) - 1);
                            g.vn.Add(int.Parse(args[2].Split('/')[2]) - 1);
                            g.vn.Add(int.Parse(args[3].Split('/')[2]) - 1);
                        }
                        break;
                    case "o":
                        o = new OBJObject();
                        o.name = args[1];
                        objects.Add(o);
                        g = new OBJGroup();
                        o.groups.Add(g);
                        break;
                    case "g":
                        g = new OBJGroup();
                        if (o == null || args.Length > 1)
                        {
                            o = new OBJObject();
                            if (args.Length > 1)
                                o.name = args[1];
                            objects.Add(o);
                        }
                        o.groups.Add(g);
                        break;
                }
            }
        }
        public static void KCL2OBJ(string fname, KCL kcl)
        {
            int FaceShift = 1;
            using (StreamWriter f = new StreamWriter(fname))
            {
                foreach (KCL.KCLModel mdl in kcl.models)
                {
                    List<Vector3> VerticesN = new List<Vector3>(); //a lot of normals are often shared

                    f.WriteLine($"o {mdl.Text}");
                    foreach (KCL.Vertex vtx in mdl.vertices)
                    {
                        f.WriteLine($"v {vtx.pos.X} {vtx.pos.Y} {vtx.pos.Z}");
                    }
                    foreach (KCL.Vertex vtx in mdl.vertices)
                    {
                        f.WriteLine($"vn {vtx.nrm.X} {vtx.nrm.Y} {vtx.nrm.Z}");
                    }
                    f.WriteLine($"usemtl None");
                    f.WriteLine($"s off");

                    var vert = mdl.CreateDisplayVertices();
                    for (int i = 0; i < mdl.display.Length; i++)
                    {
                        int[] verts = new int[3] { (int)mdl.display[i++], (int)mdl.display[i++], (int)mdl.display[i] };
                        int[] normals = new int[3] {
                        VerticesN.IndexOf(mdl.vertices[verts[0]].nrm),
                        VerticesN.IndexOf(mdl.vertices[verts[1]].nrm),
                        VerticesN.IndexOf(mdl.vertices[verts[2]].nrm)
                        };

                        f.WriteLine($"f {verts[0] + FaceShift}/{verts[0] + FaceShift}/{normals[0] + FaceShift} {verts[1] + FaceShift}/{verts[1] + FaceShift}/{normals[1] + FaceShift} {verts[2] + FaceShift}/{verts[2] + FaceShift}/{normals[2] + FaceShift}");
                    }
                    FaceShift += vert.Count;
                }
            }
        }
        public static void BFRES2OBJ(string fname, BFRES bfres)
        {
            using (StreamWriter f = new StreamWriter(fname))
            {
                int FaceShift = 1;
                int CurMesh = 0;
                foreach (BFRES.FMDL_Model mdl in bfres.models)
                {
                    foreach (BFRES.Mesh m in mdl.poly)
                    {
                        List<Vector3> VerticesN = new List<Vector3>(); //a lot of normals are often shared
                        List<string> ExportTextures = new List<string>();



                        f.WriteLine($"o {m.Text}");

                        foreach (BFRES.Vertex vtx in m.vertices)
                        {
                            f.WriteLine($"v {vtx.pos.X} {vtx.pos.Y} {vtx.pos.Z}");
                            f.WriteLine($"vn {vtx.nrm.X} {vtx.nrm.Y} {vtx.nrm.Z}");
                            VerticesN.Add(vtx.nrm);

                        }
           
                        Vector2 Scale = new Vector2(1);
                        Vector2 Position = new Vector2(0);

                        if (m.material.matparam.ContainsKey("gsys_bake_st0"))
                        {
                            Vector4 uvShift = m.material.matparam["gsys_bake_st0"].Value_float4;
                            Scale = uvShift.Xy;
                            Position = uvShift.Zw;
                        }
                   
                        if (Scale != new Vector2(1) || Position != new Vector2(0))
                        {
                            foreach (BFRES.Vertex vtx in m.vertices)
                            {
                                Vector2 st = (vtx.uv1 * Scale) + Position;
                                st = new Vector2(st.X, 1 - st.Y);
                                f.WriteLine($"vt {st.X} {st.Y}");
                            }
                        }
                        else
                        {
                            foreach (BFRES.Vertex vtx in m.vertices)
                            {
                                f.WriteLine($"vt {vtx.uv0.X * Scale.X + Position.X} {vtx.uv0.Y * Scale.Y + Position.Y}");
                            }
                        }
                    

                  

                        f.WriteLine($"usemtl {m.material.Name}");
                        if (!ExportTextures.Contains(m.material.textures[0].Name)) ExportTextures.Add(m.material.textures[0].Name);
                        f.WriteLine($"s off");


                        var vert = m.CreateDisplayVertices();
                        for (int i = 0; i < m.display.Length; i++)
                        {
                            int[] verts = new int[3] { (int)m.display[i++], (int)m.display[i++], (int)m.display[i] };
                            int[] normals = new int[3] {
                        VerticesN.IndexOf(m.vertices[verts[0]].nrm),
                        VerticesN.IndexOf(m.vertices[verts[1]].nrm),
                        VerticesN.IndexOf(m.vertices[verts[2]].nrm)
                        };
                            f.WriteLine($"f {verts[0] + FaceShift}/{verts[0] + FaceShift}/{normals[0] + FaceShift} {verts[1] + FaceShift}/{verts[1] + FaceShift}/{normals[1] + FaceShift} {verts[2] + FaceShift}/{verts[2] + FaceShift}/{normals[2] + FaceShift}");                  
                        }

                        FaceShift += vert.Count;

                        Console.WriteLine(m.Text);
                        Console.WriteLine(m.vertices.Count);
                        Console.WriteLine(m.display.Length);

                        CurMesh++;
                    }
                }
            }
        }

        public static void Save(string fname, ModelContainer con)
        {
            if (con.Bfres != null)
            {
                BFRES2OBJ(fname, con.Bfres);
                return;
            }
            if (con.Kcl != null)
            {
                KCL2OBJ(fname, con.Kcl);
                return;
            }
        }
        public NUD toNUD()
        {
            NUD n = new NUD();

            n.hasBones = false;

            foreach (OBJObject o in objects)
            {
                NUD.Mesh m = new NUD.Mesh();
                m.Text = o.name;
                m.singlebind = -1;
                m.boneflag = 0x08;
                
                foreach (OBJGroup g in o.groups)
                {
                    if (g.v.Count == 0) continue;

                    NUD.Polygon p = new NUD.Polygon();
                    m.Nodes.Add(p);
                    m.Nodes.Add(p);
                    p.AddDefaultMaterial();
                    p.vertSize = 0x06;
                    p.UVSize = 0x10;
                    p.polflag = 0x00;

                    Dictionary<int, int> collected = new Dictionary<int, int>();

                    for (int i = 0; i < g.v.Count; i++)
                    {
                        p.vertexIndices.Add(p.vertices.Count);
                        NUD.Vertex v = new NUD.Vertex();
                        p.vertices.Add(v);
                        if (g.v.Count > i)
                            v.pos = this.v[g.v[i]] + Vector3.Zero;
                        if (g.vn.Count > i)
                            v.nrm = vn[g.vn[i]] + Vector3.Zero; 
                        if (g.vt.Count > i)
                            v.uv.Add(vt[g.vt[i]] + Vector2.Zero);
                    }
                }
                if(m.Nodes.Count > 0)
                    n.Nodes.Add(m);
            }

            n.OptimizeFileSize();
            n.UpdateRenderMeshes();

            return n;
        }

    }
}
