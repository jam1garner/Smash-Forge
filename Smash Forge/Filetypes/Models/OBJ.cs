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
                        if(o == null)
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
                        if(args[1].Split('/').Length > 1)
                        {
                            g.vt.Add(int.Parse(args[1].Split('/')[2]) - 1);
                            g.vt.Add(int.Parse(args[2].Split('/')[2]) - 1);
                            g.vt.Add(int.Parse(args[3].Split('/')[2]) - 1);
                        }
                        if (args[1].Split('/').Length > 2)
                        {
                            g.vn.Add(int.Parse(args[1].Split('/')[1]) - 1);
                            g.vn.Add(int.Parse(args[2].Split('/')[1]) - 1);
                            g.vn.Add(int.Parse(args[3].Split('/')[1]) - 1);
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
                m.Text = o.name;
                m.singlebind = -1;
                m.boneflag = 0x08;
                
                foreach (OBJGroup g in o.groups)
                {
                    if (g.v.Count == 0) continue;

                    NUD.Polygon p = new NUD.Polygon();
                    m.Nodes.Add(p);
                    m.Nodes.Add(p);
                    p.setDefaultMaterial();
                    p.vertSize = 0x06;
                    p.UVSize = 0x10;
                    p.polflag = 0x00;

                    Dictionary<int, int> collected = new Dictionary<int, int>();

                    for (int i = 0; i < g.v.Count; i++)
                    {
                        p.faces.Add(p.vertices.Count);
                        NUD.Vertex v = new NUD.Vertex();
                        p.vertices.Add(v);
                        if (g.v.Count > i)
                            v.pos = this.v[g.v[i]] + Vector3.Zero;
                        if (g.vn.Count > i)
                            v.nrm = vn[g.vn[i]] + Vector3.Zero; ;
                        if (g.vt.Count > i)
                            v.tx.Add(vt[g.vt[i]] + Vector2.Zero);
                    }
                }
                if(m.Nodes.Count > 0)
                    n.mesh.Add(m);
            }

            n.Optimize();
            n.PreRender();

            return n;
        }

    }
}
