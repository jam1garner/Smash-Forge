using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smash_Forge
{
    class Collada
    {

        public Collada()
        {

        }

        public void CreateMesh(string name)
        {

        }

        public static void DAEtoNUD(string fname, ModelContainer con)
        {
            COLLADA model = COLLADA.Load(fname);

            NUD n = new NUD();
            //if (con.vbn == null)
            //    return;
            VBN vbn = con.vbn;
            con.nud = n;

            // Iterate on libraries
            foreach (var item in model.Items)
            {
                var geometries = item as library_geometries;
                if (geometries != null)
                {
                    // Iterate on geomerty in library_geometries 
                    foreach (var geom in geometries.geometry)
                    {
                        var mesh = geom.Item as mesh;
                        if (mesh == null)
                            continue;

                        NUD.Mesh n_mesh = new NUD.Mesh();
                        n.mesh.Add(n_mesh);
                        n_mesh.name = geom.name;

                        Dictionary<string, double[]> sources = new Dictionary<string, double[]>();
                        Dictionary<string, string> vertex = new Dictionary<string, string>();
                        Dictionary<string, string> semantic = new Dictionary<string, string>();

                        // Dump source[] for geom
                        foreach (var source in mesh.source)
                        {
                            var float_array = source.Item as float_array;
                            if (float_array == null)
                                continue;
                            sources.Add(source.id, float_array.Values);
                        }
                        {
                            var inputs = mesh.vertices.input;
                            foreach (var input in inputs)
                            {
                                vertex.Add(input.semantic, input.source);
                            }
                        }
                        // Dump Items[] for geom
                        foreach (var meshItem in mesh.Items)
                        {
                            if (meshItem is vertices)
                            {
                                var vertices = meshItem as vertices;
                                var inputs = vertices.input;
                                foreach (var input in inputs)
                                    vertex.Add(input.semantic, input.source);
                            }
                            else if (meshItem is triangles)
                            {
                                var triangles = meshItem as triangles;
                                var inputs = triangles.input;

                                foreach (var input in inputs)
                                    semantic.Add(input.semantic, input.source);

                                NUD.Polygon poly = new NUD.Polygon();
                                poly.setDefaultMaterial();
                                n_mesh.polygons.Add(poly);
                                string[] ps = triangles.p.StartsWith(" ") ? triangles.p.Substring(1).Split(' ') : triangles.p.Split(' ');
                                for (int i = 0; i < ps.Length;)
                                {
                                    //poly.faces.Add(int.Parse(ps[i]));
                                    int p = int.Parse(ps[i]);
                                    /*}
                                    poly.faces.Add(int.Parse(ps[i]));
                                        if (int.Parse(ps[i]) > vCount)
                                            vCount = int.Parse(ps[i]);
                                    for (int i = 0; i < vCount + 1; i++)
                                    {*/
                                    NUD.Vertex v = new NUD.Vertex();

                                    // iterate semantics
                                    foreach (string s in semantic.Keys)
                                    {
                                        string src;
                                        double[] bank;
                                        semantic.TryGetValue(s, out src);
                                        src = src.Replace("#", "");
                                        sources.TryGetValue(src, out bank);
                                        switch (s)
                                        {
                                            case "VERTEX":
                                                {
                                                    poly.faces.Add(p);
                                                    //poly.AddVertex(v);
                                                    while (poly.vertices.Count <= p)
                                                        poly.AddVertex(new NUD.Vertex());
                                                    poly.vertices[p] = v;
                                                    foreach (string s2 in vertex.Keys)
                                                    {
                                                        string vsrc;
                                                        vertex.TryGetValue(s2, out vsrc);
                                                        vsrc = vsrc.Replace("#", "");
                                                        //Console.WriteLine(vsrc);
                                                        sources.TryGetValue(vsrc, out bank);
                                                        switch (s2)
                                                        {
                                                            case "POSITION":
                                                                v.pos.X = (float)bank[p * 3 + 0];
                                                                v.pos.Y = (float)bank[p * 3 + 1];
                                                                v.pos.Z = (float)bank[p * 3 + 2];
                                                                break;
                                                            case "NORMAL":
                                                                v.nrm.X = (float)bank[p * 3 + 0];
                                                                v.nrm.Y = (float)bank[p * 3 + 1];
                                                                v.nrm.Z = (float)bank[p * 3 + 2];
                                                                break;
                                                            case "COLOR":
                                                                v.col.X = (float)bank[p * 4 + 0] * 255;
                                                                v.col.Y = (float)bank[p * 4 + 1] * 255;
                                                                v.col.Z = (float)bank[p * 4 + 2] * 255;
                                                                v.col.W = (float)bank[p * 4 + 3] * 127;
                                                                break;
                                                            case "TEXCOORD":
                                                                v.tx.Add(new OpenTK.Vector2((float)bank[p * 2 + 0], (float)bank[p * 2 + 1]));
                                                                break;
                                                        }
                                                    }
                                                    break;
                                                }
                                            case "POSITION":
                                                v.pos.X = (float)bank[p * 3 + 0];
                                                v.pos.Y = (float)bank[p * 3 + 1];
                                                v.pos.Z = (float)bank[p * 3 + 2];
                                                break;
                                            case "NORMAL":
                                                v.nrm.X = (float)bank[p * 3 + 0];
                                                v.nrm.Y = (float)bank[p * 3 + 1];
                                                v.nrm.Z = (float)bank[p * 3 + 2];
                                                break;
                                            case "COLOR":
                                                v.col.X = (float)bank[p * 4 + 0] * 255;
                                                v.col.Y = (float)bank[p * 4 + 1] * 255;
                                                v.col.Z = (float)bank[p * 4 + 2] * 255;
                                                v.col.W = (float)bank[p * 4 + 3] * 127;
                                                break;
                                            case "TEXCOORD":
                                                v.tx.Add(new OpenTK.Vector2((float)bank[p * 2 + 0], (float)bank[p * 2 + 1]));
                                                break;
                                        }
                                        i++;
                                        if (i >= ps.Length) break;
                                        p = int.Parse(ps[i]);
                                    }
                                }
                            }
                        }
                    }
                }

                var controllers = item as library_controllers;
                if (controllers != null && vbn != null)
                {
                    int cid = 0;
                    // Iterate on controllers in library_controllers 
                    foreach (var cont in controllers.controller)
                    {
                        var control = cont as controller;
                        //if (control == null)
                        //    continue;

                        var skin = control.Item as skin;

                        string[] boneNames = null;
                        Dictionary<string, double[]> sources = new Dictionary<string, double[]>();
                        Dictionary<string, string> semantic = new Dictionary<string, string>();

                        // Dump source[] for geom
                        foreach (var source in skin.source)
                        {
                            var float_array = source.Item as float_array;
                            if (float_array != null)
                            {
                                sources.Add(source.id, float_array.Values);
                            }
                            var name_array = source.Item as Name_array;
                            if (name_array != null)
                            {
                                boneNames = name_array._Text_.Split(' ');
                            }
                        }
                        {
                            var inputs = skin.joints.input;
                            foreach (var input in inputs)
                            {
                                if (input.semantic.Equals(""))
                                    semantic.Add(input.semantic, input.source);
                            }
                        }
                        // Dump Items[] for geom
                        NUD.Mesh m = n.mesh[cid];
                        List<NUD.Vertex> v = m.polygons[0].vertices;
                        string[] vcount = skin.vertex_weights.vcount.Split(' ');
                        string[] vi = skin.vertex_weights.v.Split(' ');
                        int pos = 0;

                        List<string> bname = new List<string>();
                        for (int i = 0; i < (int)skin.vertex_weights.count; i++)
                        {
                            NUD.Vertex vert = v[i];

                            for (int j = 0; j < int.Parse(vcount[i]); j++)
                                foreach (var sem in skin.vertex_weights.input)
                                {
                                    switch (sem.semantic)
                                    {
                                        case "JOINT":
                                            // find joint name in vbn
                                            int ind = int.Parse(vi[pos]);
                                            int index = vbn.boneIndex(boneNames[ind]);
                                            vert.node.Add(index == -1 ? 0 : index);
                                            if (index == -1)
                                                if (!bname.Contains(boneNames[ind]))
                                                    bname.Add(boneNames[ind]);
                                            break;
                                        case "WEIGHT":
                                            // find weight int weight list
                                            double[] weight;
                                            sources.TryGetValue(sem.source.Replace("#", ""), out weight);
                                            float w = (int)Math.Round(weight[int.Parse(vi[pos])] * 0xFF);
                                            w /= 0xFF;
                                            //Console.WriteLine(w + " " + weight[int.Parse(vi[pos])]);
                                            vert.weight.Add((float)weight[int.Parse(vi[pos])]);
                                            if (vert.weight.Count > 4)
                                                Console.WriteLine("Weight Error");
                                            break;
                                    }
                                    pos++;
                                }

                        }
                        cid++;
                        foreach (string nam in bname)
                        {
                            Console.WriteLine("No match " + nam);
                        }
                    }
                }
            }

            foreach (NUD.Mesh mesh in n.mesh)
            {
                foreach (NUD.Polygon poly in mesh.polygons)
                {
                    poly.vertSize = 0x16;

                    if (vbn == null)
                        poly.vertSize = 0x06;

                    foreach (NUD.Vertex v in poly.vertices)
                    {
                        v.node.Add(-1);
                        v.weight.Add(0);
                    }
                }
            }
            n.MergePoly();
            n.PreRender();
        }

        public void Save(string fname, DAT mod)
        {
            COLLADA dae = new Smash_Forge.COLLADA();

            List<object> Items = new List<object>();

            library_geometries mesh = new library_geometries();
            Items.Add(mesh);
            List<geometry> Mesh = new List<geometry>();
            /*foreach (DAT.DOBJ d in mod.dobjs)
            {
                geometry geom = new geometry();
                Mesh.Add(geom);
                geom.id = "Mesh_" + mod.dobjs.IndexOf(d);
                geom.name = "Mesh";

                mesh finmesh = new mesh();
                
                List<DAT.Vertex> verts = new List<DAT.Vertex>();
                List<int> faces = new List<int>();
                List<source> msources = new List<source>();
                //sources
                {
                    source pos = new source();
                    msources.Add(pos);
                    float_array posa = new float_array();
                    sourceTechnique_common post = new sourceTechnique_common();
                    pos.technique_common = post;
                    post.accessor = new accessor();
                    post.accessor.stride = 3;
                    post.accessor.param = new param[3];
                    post.accessor.source = "#" + "Mesh_" + mod.dobjs.IndexOf(d) + "_POSARR";

                    {
                        param p1 = new Smash_Forge.param();
                        p1.name = "X"; p1.type = "float";
                        post.accessor.param[0] = p1;
                        p1 = new Smash_Forge.param();
                        p1.name = "Y"; p1.type = "float";
                        post.accessor.param[1] = p1;
                        p1 = new Smash_Forge.param();
                        p1.name = "Z"; p1.type = "float";
                        post.accessor.param[2] = p1;
                    }

                    pos.Item = posa;

                    pos.id = "Mesh_" + mod.dobjs.IndexOf(d) + "_POS";
                    posa.id = "Mesh_" + mod.dobjs.IndexOf(d) + "_POSARR";
                    string positions = "";

                    foreach (DAT.POBJ poly in d.polygons)
                    {
                        foreach(DAT.POBJ.DisplayObject dispoly in poly.display)
                        {
                            foreach(int i in dispoly.faces)
                            {
                                DAT.Vertex v = mod.vertBank[i];
                                if (!verts.Contains(v))
                                    verts.Add(v);
                                faces.Add(verts.IndexOf(v));
                            }
                        }
                    }

                    foreach(DAT.Vertex v in verts)
                    {
                        positions += v.pos.X + " ";
                        positions += v.pos.Y + " ";
                        positions += v.pos.Z + " ";
                    }

                    posa._Text_ = positions;
                    post.accessor.count = (ulong)verts.Count;
                    posa.count = (ulong)verts.Count*4;
                }

                finmesh.source = msources.ToArray();

                //vertex

                vertices vertices = new vertices();

                finmesh.vertices = vertices;

                vertices.id = "Mesh_" + mod.dobjs.IndexOf(d) + "_Vertices";
                vertices.input = new InputLocal[1];
                vertices.input[0] = new InputLocal();
                vertices.input[0].semantic = "POSITION";
                vertices.input[0].source = " #" + "Mesh_" + mod.dobjs.IndexOf(d) + "_POS";


                // triangle
                triangles triangles = new triangles();
                finmesh.Items = new object[1];
                finmesh.Items[0] = triangles;
                triangles.count = (ulong)faces.Count;

                triangles.input = new InputLocalOffset[1];
                triangles.input[0] = new InputLocalOffset();
                triangles.input[0].semantic = "VERTEX";
                triangles.input[0].offset = 0;
                triangles.input[0].source = "#" + "Mesh_" + mod.dobjs.IndexOf(d) + "_Vertices";

                string ps = "";
                foreach(int i in faces)
                    ps += i + " ";

                triangles.p = ps;

                geom.Item = finmesh;
            }
            mesh.geometry = Mesh.ToArray();



            /*COLLADAScene scene = new COLLADAScene();
            scene.instance_visual_scene = new InstanceWithExtra();
            scene.instance_visual_scene.url = "#RootNode";
            Items.Add(scene);*/

            dae.Items = Items.ToArray();

            dae.Save(fname);
        }

    }
}
