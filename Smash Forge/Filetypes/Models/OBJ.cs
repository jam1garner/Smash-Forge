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

        public class OBJObject
        {
            public string name = "None";
            public List<OBJGroup> groups = new List<OBJGroup>();
            public List<OBJVert> verts = new List<OBJVert>();
        }

        public class OBJVert
        {
            public Vector3 pos;
            public Vector3 nrm;
            public Vector2 tx;
        }

        public class OBJGroup
        {
            public List<int> faces = new List<int>();
        }

        public void Read(string fname)
        {
            string input = File.ReadAllText(fname);

            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            input = regex.Replace(input, " ");

            string[] lines = input.Split('\n');

            OBJVert v;
            OBJObject o = null;
            OBJGroup g = null;
            int vi = 0, vti = 0, vni = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] args = lines[i].Split(' ');
                switch (args[0])
                {
                    case "v":
                        if(o == null)
                        {
                            o = new OBJObject();
                            g = new OBJGroup();
                            o.groups.Add(g);
                            objects.Add(o);
                        }
                        v = new OBJVert();
                        o.verts.Add(v);
                        v.pos.X = float.Parse(args[1]);
                        v.pos.Y = float.Parse(args[2]);
                        v.pos.Z = float.Parse(args[3]);
                        break;
                    case "vn":
                        v = o.verts[vni++];
                        v.nrm.X = float.Parse(args[1]);
                        v.nrm.Y = float.Parse(args[2]);
                        v.nrm.Z = float.Parse(args[3]);
                        break;
                    case "vt":
                        v = o.verts[vti++];
                        v.tx = new Vector2(float.Parse(args[1]), float.Parse(args[2]));
                        break;
                    case "f":
                        g.faces.Add(int.Parse(args[1].Split('/')[0]) - 1);
                        g.faces.Add(int.Parse(args[2].Split('/')[0]) - 1);
                        g.faces.Add(int.Parse(args[3].Split('/')[0]) - 1);
                        break;
                    case "o":
                        o = new OBJObject();
                        o.name = args[1];
                        objects.Add(o);
                        break;
                    case "g":
                        g = new OBJGroup();
                        if (o == null)
                        {
                            o = new OBJObject();
                            if(args.Length > 1)
                                o.name = args[1];
                            objects.Add(o);
                        }
                        o.groups.Add(g);
                        break;
                }
            }
        }
        
        public NUD toNUD()
        {
            NUD n = new NUD();

            n.hasBones = false;

            foreach (OBJObject o in objects)
            {
                NUD.Mesh m = new NUD.Mesh();
                n.mesh.Add(m);
                m.Text = o.name;
                m.singlebind = -1;
                m.boneflag = 0x08;
                
                foreach (OBJGroup g in o.groups)
                {
                    if (g.faces.Count == 0) continue;

                    NUD.Polygon p = new NUD.Polygon();
                    m.polygons.Add(p);
                    p.setDefaultMaterial();
                    p.vertSize = 0x06;
                    p.UVSize = 0x12;
                    p.polflag = 0x00;

                    Dictionary<int, int> collected = new Dictionary<int, int>();

                    foreach (int f in g.faces)
                    {
                        if (collected.ContainsKey(f))
                        {
                            p.faces.Add(collected[f]);
                        }else
                        {
                            collected.Add(f, p.vertices.Count);
                            p.faces.Add(p.vertices.Count);
                            OBJVert ov = o.verts[f];
                            NUD.Vertex v = new NUD.Vertex();
                            p.vertices.Add(v);
                            v.pos.X = ov.pos.X;
                            v.pos.Y = ov.pos.Y;
                            v.pos.Z = ov.pos.Z;
                            v.nrm.X = ov.nrm.X;
                            v.nrm.Y = ov.nrm.Y;
                            v.nrm.Z = ov.nrm.Z;
                            v.tx.Add(new Vector2(ov.tx.X, ov.tx.Y));
                        }
                    }
                }
            }

            n.Optimize();
            n.PreRender();

            return n;
        }

    }
}
