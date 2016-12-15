using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml;
using OpenTK;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
            Collada dae = new Collada();
            dae.Read(fname);

            NUD n = new Smash_Forge.NUD();
            con.nud = n;

            // next will be nodes then controllers
            // craft vbn :>
            // find joint node
            foreach (ColladaNode node in dae.scene.nodes)
            {
                if (node.type.Equals("JOINT") && con.vbn == null)
                {
                    // joint tree

                    VBN vbn = new VBN();
                    con.vbn = vbn;

                    List<ColladaNode> parenttrack = new List<ColladaNode>();
                    Queue<ColladaNode> nodes = new Queue<ColladaNode>();
                    nodes.Enqueue(node);

                    while (nodes.Count > 0)
                    {
                        ColladaNode bo = nodes.Dequeue();
                        parenttrack.Add(bo);
                        foreach (ColladaNode child in bo.children)
                            nodes.Enqueue(child);

                        Bone bone = new Smash_Forge.Bone();
                        vbn.bones.Add(bone);
                        bone.boneName = bo.name.ToCharArray();
                        bone.parentIndex = parenttrack.IndexOf(bo.parent);
                        bone.position = new float[3];
                        bone.rotation = new float[3];
                        bone.scale = new float[3];
                        bone.position[0] = bo.pos.X;
                        bone.position[1] = bo.pos.Y;
                        bone.position[2] = bo.pos.Z;
                        bone.rotation[0] = bo.rot.X;
                        bone.rotation[1] = bo.rot.Y;
                        bone.rotation[2] = bo.rot.Z;
                        bone.scale[0] = bo.sca.X;
                        bone.scale[1] = bo.sca.X;
                        bone.scale[2] = bo.sca.X;
                        bone.children = new List<int>();

                        if (bone.parentIndex != 0x0FFFFFFF && bone.parentIndex > -1)
                            vbn.bones[bone.parentIndex].children.Add(parenttrack.IndexOf(bo));
                    }

                    vbn.reset();
                    vbn.update();
                }
                if (node.type.Equals("NODE"))
                {
                    // this will link the controller to the geometry

                }
            }

            // controllers
            Dictionary<string, List<NUD.Vertex>> vertices = new Dictionary<string, List<NUD.Vertex>>();
            Dictionary<string, Matrix4> bindMatrix = new Dictionary<string, Matrix4>();
            foreach (ColladaController control in dae.library_controllers)
            {
                ColladaSkin skin = control.skin;

                Dictionary<string, ColladaSource> sources = new Dictionary<string, ColladaSource>();
                foreach (ColladaSource s in skin.sources)
                {
                    sources.Add("#" + s.id, s);
                }

                List<NUD.Vertex> verts = new List<NUD.Vertex>();
                vertices.Add(skin.source, verts);
                bindMatrix.Add(skin.source, skin.mat);

                int v = 0;
                for(int i = 0; i < skin.weights.count; i++)
                {
                    //basically, I need to find all verts that use this position and apply that.........

                    int count = skin.weights.vcount[i];
                    if (count > 4)
                    {
                        MessageBox.Show("Error: More than 4 weights detected!");
                    }

                    NUD.Vertex vert = new NUD.Vertex();
                    verts.Add(vert);

                    for (int j = 0; j < count; j++)
                    {
                        foreach (ColladaInput input in skin.weights.inputs)
                        {
                            switch (input.semantic)
                            {
                                case SemanticType.JOINT:
                                    string bname = sources[input.source].data[skin.weights.v[v]];
                                    int index = con.vbn.boneIndex(bname);
                                    vert.node.Add(index);
                                    break;
                                case SemanticType.WEIGHT:
                                    float weight = float.Parse(sources[input.source].data[skin.weights.v[v]]);
                                    vert.weight.Add(weight);
                                    break;
                            }
                            v++;
                        }
                    }
                }
            }
            

            Dictionary<string, NUD.Mesh> geometries = new Dictionary<string, NUD.Mesh>(); 
            foreach(ColladaGeometry geom in dae.library_geometries)
            {
                ColladaMesh mesh = geom.mesh;
                ColladaPolygons p = mesh.polygons[0];

                // first create vertices?

                Dictionary<string, ColladaSource> sources = new Dictionary<string, ColladaSource>();
                foreach (ColladaSource s in mesh.sources)
                {
                    sources.Add("#" + s.id, s);
                }

                NUD.Mesh nmesh = new NUD.Mesh();
                geometries.Add("#" + geom.id, nmesh);
                n.mesh.Add(nmesh);
                nmesh.name = geom.name;
                NUD.Polygon npoly = new NUD.Polygon();
                npoly.setDefaultMaterial();
                nmesh.polygons.Add(npoly);

                int i = 0;
                while(i < p.p.Length)
                {
                    NUD.Vertex v = new NUD.Vertex();
                    foreach (ColladaInput input in p.inputs)
                    {
                        if(input.semantic == SemanticType.VERTEX)
                        {
                            v = new NUD.Vertex();
                            v.node.AddRange(vertices["#" + geom.id][p.p[i]].node);
                            v.weight.AddRange(vertices["#" + geom.id][p.p[i]].weight);
                            npoly.vertices.Add(v);
                            npoly.faces.Add(npoly.vertices.IndexOf(v));
                            foreach (ColladaInput vinput in mesh.vertices.inputs)
                            {
                                ReadSemantic(vinput, v, p.p[i], sources);
                            }
                        }else
                            ReadSemantic(input, v, p.p[i], sources);
                        i++;
                    }

                    v.pos = Vector3.Transform(v.pos, bindMatrix["#" + geom.id]);
                    v.nrm = Vector3.Transform(v.nrm, bindMatrix["#" + geom.id]);
                }

                while(npoly.materials.Count < npoly.vertices[0].tx.Count)
                {
                    NUD.Polygon nd = new NUD.Polygon();
                    nd.setDefaultMaterial();
                    npoly.materials.Add(nd.materials[0]);
                }
            }


            // then image materials and effects

            
            n.MergePoly();
            n.PreRender();
        }

        private static void ReadSemantic(ColladaInput input, NUD.Vertex v, int p, Dictionary<string, ColladaSource> sources)
        {
            switch(input.semantic)
            {
                case SemanticType.POSITION:
                    v.pos.X = float.Parse(sources[input.source].data[p * 3 + 0]);
                    v.pos.Y = float.Parse(sources[input.source].data[p * 3 + 1]);
                    v.pos.Z = float.Parse(sources[input.source].data[p * 3 + 2]);
                    break;
                case SemanticType.NORMAL:
                    v.nrm.X = float.Parse(sources[input.source].data[p * 3 + 0]);
                    v.nrm.Y = float.Parse(sources[input.source].data[p * 3 + 1]);
                    v.nrm.Z = float.Parse(sources[input.source].data[p * 3 + 2]);
                    break;
                case SemanticType.TEXCOORD:
                    Vector2 tx = new Vector2();
                    tx.X = float.Parse(sources[input.source].data[p * 2 + 0]);
                    tx.Y = float.Parse(sources[input.source].data[p * 2 + 1]);
                    v.tx.Add(tx);
                    break;
                case SemanticType.COLOR:
                    v.col.X = float.Parse(sources[input.source].data[p * 4 + 0]) * 255;
                    v.col.Y = float.Parse(sources[input.source].data[p * 4 + 1]) * 255;
                    v.col.Z = float.Parse(sources[input.source].data[p * 4 + 2]) * 255;
                    v.col.W = float.Parse(sources[input.source].data[p * 4 + 3]) * 127;
                    break;
            }
        }

        public static void DAEtoNUDOld(string fname, ModelContainer con)
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


        // collada containers

        Dictionary<string, object> sourceLinks = new Dictionary<string, object>();

        List<ColladaGeometry> library_geometries = new List<ColladaGeometry>();
        List<ColladaController> library_controllers = new List<ColladaController>();
        ColladaVisualScene scene = new ColladaVisualScene();
        public int v1, v2, v3;

        public void Read(string fname)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fname);
            XmlNode colnode = doc.ChildNodes[1];

            string v = (string)colnode.Attributes["version"].Value;
            string[] s = v.Split('.');
            int.TryParse(s[0], NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out v1);
            int.TryParse(s[1], NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out v2);
            int.TryParse(s[2], NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out v3);

            foreach (XmlNode node in colnode.ChildNodes)
            {
                /*if (f.Name.Equals("asset", true))
                    ParseAsset();
                else if (f.Name.Equals("library_images", true))
                    ParseLibImages();
                else if (f.Name.Equals("library_materials", true))
                    ParseLibMaterials();
                else if (f.Name.Equals("library_effects", true))
                    ParseLibEffects();
                else*/
                Console.WriteLine(node.Name);
                if (node.Name.Equals("library_geometries"))
                    ParseGeometry(node);
                if (node.Name.Equals("library_visual_scenes"))
                    scene.Read(node);
                if (node.Name.Equals("library_controllers"))
                    ParseControllers(node);
            }
        }

        // I want geometry firsts

        #region ENUMS
        public enum ColladaPrimitiveType
        {
            None,
            polygons,
            polylist,
            triangles,
            trifans,
            tristrips,
            lines,
            linestrips
        }
        public enum SemanticType
        {
            None,
            POSITION,
            VERTEX,
            NORMAL,
            TEXCOORD,
            COLOR,
            WEIGHT,
            JOINT,
            INV_BIND_MATRIX,
            TEXTANGENT,
            TEXBINORMAL
        }

        #endregion

        #region Assets

        public void ParseAssets(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name.Equals("unit"))
                {

                }
                if (node.Name.Equals("up_axis"))
                {

                }
            }
        }

        #endregion

        #region Geometry

        public void ParseGeometry(XmlNode root)
        {
            foreach(XmlNode node in root.ChildNodes)
            {
                ColladaGeometry g = new ColladaGeometry();
                g.Read(node);
                library_geometries.Add(g);
            }
        }

        public class ColladaGeometry
        {
            public string id;
            public string name;
            public ColladaMesh mesh;

            public void Read(XmlNode root)
            {
                id = (string)root.Attributes["id"].Value;
                name = (string)root.Attributes["name"].Value;
                mesh = new ColladaMesh();
                foreach(XmlNode node in root.ChildNodes)
                {
                    if(node.Name.Equals("mesh"))
                        mesh.Read(node);
                }
            }

            public XmlNode Write()
            {
                return null;
            }
        }

        public class ColladaMesh
        {
            public List<ColladaSource> sources = new List<ColladaSource>();
            public ColladaVertices vertices = new ColladaVertices();
            public List<ColladaPolygons> polygons = new List<ColladaPolygons>();

            public void Read(XmlNode root)
            {
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("source"))
                    {
                        ColladaSource source = new ColladaSource();
                        source.Read(node);
                        sources.Add(source);
                    }
                    if (node.Name.Equals("vertices"))
                    {
                        vertices.Read(node);
                    }
                    if (node.Name.Equals("triangles"))
                    {
                        ColladaPolygons source = new ColladaPolygons();
                        source.type = ColladaPrimitiveType.triangles;
                        source.Read(node);
                        polygons.Add(source);
                    }
                }
            }
        }

        public class ColladaSource
        {
            public string id;

            public string[] data;
            public int count;

            public void Read(XmlNode root)
            {
                id = (string)root.Attributes["id"].Value;
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("float_array"))
                    {
                        count = int.Parse((string)node.Attributes["count"].Value);
                        data = node.InnerText.Trim().Replace("\n", " ").Split(' ');
                    }
                    if (node.Name.Equals("Name_array"))
                    {
                        count = int.Parse((string)node.Attributes["count"].Value);
                        data = node.InnerText.Trim().Replace("\n", " ").Split(' ');
                    }
                }
            }
        }

        public class ColladaVertices
        {
            public string id;
            public List<ColladaInput> inputs = new List<ColladaInput>();
            
            public void Read(XmlNode root)
            {
                id = (string)root.Attributes["id"].Value;

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("input"))
                    {
                        ColladaInput input = new ColladaInput();
                        input.Read(node);
                        inputs.Add(input);
                    }
                }
            }
        }

        public class ColladaPolygons
        {
            public ColladaPrimitiveType type = ColladaPrimitiveType.triangles;
            public List<ColladaInput> inputs = new List<ColladaInput>();
            public int[] p;
            public int count;
            public string material;

            public void Read(XmlNode root)
            {
                material = (string)root.Attributes["material"].Value;
                int.TryParse((string)root.Attributes["count"].Value, out count);

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("input"))
                    {
                        ColladaInput input = new ColladaInput();
                        input.Read(node);
                        inputs.Add(input);
                    }
                    if (node.Name.Equals("p"))
                    {
                        string[] ps = node.InnerText.Trim().Split(' ');
                        p = new int[ps.Length];
                        for(int i = 0; i < ps.Length; i++)
                            p[i] = int.Parse(ps[i]);
                    }
                }
            }

        }

        public class ColladaInput
        {
            public SemanticType semantic;
            public string source;
            public int set, offset;

            public void Read(XmlNode root)
            {
                semantic = (SemanticType)Enum.Parse(typeof(SemanticType), (string)root.Attributes["semantic"].Value);
                source = (string)root.Attributes["source"].Value;
                if(root.Attributes["set"]!=null)
                int.TryParse((string)root.Attributes["set"].Value, out set);
                if (root.Attributes["offset"] != null)
                    int.TryParse((string)root.Attributes["offset"].Value, out offset);
            }
        }
        #endregion

        #region Materials
        // Images and Materials
        public class ColladaMaterial
        {

        }
        #endregion

        #region Controllers

        public void ParseControllers(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                ColladaController g = new ColladaController();
                g.Read(node);
                library_controllers.Add(g);
            }
        }

        public class ColladaController
        {
            public string id;
            public ColladaSkin skin = new ColladaSkin();

            public void Read(XmlNode root)
            {
                id = (string)root.Attributes["id"].Value;

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("skin"))
                    {
                        skin.Read(node);
                    }
                }
            }
        }

        public class ColladaSkin
        {
            public string source;
            public Matrix4 mat = new Matrix4();
            public List<ColladaSource> sources = new List<ColladaSource>();
            public ColladaJoints joints = new ColladaJoints();
            public ColladaVertexWeights weights = new ColladaVertexWeights();

            public void Read(XmlNode root)
            {
                source = (string)root.Attributes["source"].Value;

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("bind_shape_matrix"))
                    {
                        string[] data = node.InnerText.Trim().Replace("\n", " ").Split(' ');
                        mat.M11 = float.Parse(data[0]); mat.M12 = float.Parse(data[1]); mat.M13 = float.Parse(data[2]); mat.M14 = float.Parse(data[3]);
                        mat.M21 = float.Parse(data[4]); mat.M22 = float.Parse(data[5]); mat.M23 = float.Parse(data[6]); mat.M24 = float.Parse(data[7]);
                        mat.M31 = float.Parse(data[8]); mat.M32 = float.Parse(data[9]); mat.M33 = float.Parse(data[10]); mat.M34 = float.Parse(data[11]);
                        mat.M41 = float.Parse(data[12]); mat.M42 = float.Parse(data[13]); mat.M43 = float.Parse(data[14]); mat.M44 = float.Parse(data[15]);
                    }
                    if (node.Name.Equals("source"))
                    {
                        ColladaSource source = new ColladaSource();
                        source.Read(node);
                        sources.Add(source);
                    }
                    if (node.Name.Equals("joints"))
                    {
                        joints.Read(node);
                    }
                    if (node.Name.Equals("vertex_weights"))
                    {
                        weights.Read(node);
                    }
                }
            }

        }

        public class ColladaJoints
        {
            public List<ColladaInput> inputs = new List<ColladaInput>();

            public void Read(XmlNode root)
            {
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("input"))
                    {
                        ColladaInput input = new ColladaInput();
                        input.Read(node);
                        inputs.Add(input);
                    }
                }
            }
        }

        public class ColladaVertexWeights
        {
            public List<ColladaInput> inputs = new List<ColladaInput>();
            public int[] v, vcount;
            public int count;

            public void Read(XmlNode root)
            {
                count = int.Parse((string)root.Attributes["count"].Value);

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("input"))
                    {
                        ColladaInput input = new ColladaInput();
                        input.Read(node);
                        inputs.Add(input);
                    }
                    if (node.Name.Equals("vcount"))
                    {
                        string[] ps = node.InnerText.Trim().Split(' ');
                        vcount = new int[ps.Length];
                        for (int i = 0; i < ps.Length; i++)
                            vcount[i] = int.Parse(ps[i]);
                    }
                    if (node.Name.Equals("v"))
                    {
                        string[] ps = node.InnerText.Trim().Split(' ');
                        v = new int[ps.Length];
                        for (int i = 0; i < ps.Length; i++)
                            v[i] = int.Parse(ps[i]);
                    }
                }
            }
        }

        #endregion
        
        #region Visual Nodes

        public class ColladaVisualScene
        {
            public List<ColladaNode> nodes = new List<ColladaNode>();
            public string id, name;

            public void Read(XmlNode root)
            {
                root = root.ChildNodes[0];
                id = (string)root.Attributes["id"].Value;
                name = (string)root.Attributes["name"].Value;

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("node"))
                    {
                        ColladaNode n = new ColladaNode();
                        n.Read(node, null);
                        nodes.Add(n);
                    }
                }
            }
        }

        public class ColladaNode
        {
            public ColladaNode parent;
            public string id, name, type = "NODE";
            public List<ColladaNode> children = new List<ColladaNode>();
            
            public Vector3 pos = new Vector3(), sca = new Vector3(), rot = new Vector3();

            public void Read(XmlNode root, ColladaNode parent)
            {
                this.parent = parent;
                id = (string)root.Attributes["id"].Value;
                name = (string)root.Attributes["name"].Value;
                if(root.Attributes["type"] != null)
                    type = (string)root.Attributes["type"].Value;
                
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("node"))
                    {
                        ColladaNode n = new ColladaNode();
                        n.Read(node, this);
                        children.Add(n);
                    }else
                    if (node.Name.Equals("matrix"))
                    {
                        string[] data = node.InnerText.Trim().Replace("\n", " ").Split(' ');
                        Matrix4 mat = new Matrix4();
                        mat.M11 = float.Parse(data[0]); mat.M12 = float.Parse(data[1]); mat.M13 = float.Parse(data[2]); mat.M14 = float.Parse(data[3]);
                        mat.M21 = float.Parse(data[4]); mat.M22 = float.Parse(data[5]); mat.M23 = float.Parse(data[6]); mat.M24 = float.Parse(data[7]);
                        mat.M31 = float.Parse(data[8]); mat.M32 = float.Parse(data[9]); mat.M33 = float.Parse(data[10]); mat.M34 = float.Parse(data[11]);
                        mat.M41 = float.Parse(data[12]); mat.M42 = float.Parse(data[13]); mat.M43 = float.Parse(data[14]); mat.M44 = float.Parse(data[15]);
                        
                        pos = new Vector3(mat.M14, mat.M24, mat.M34);
                        sca = mat.ExtractScale();

                        mat.ClearScale();
                        mat.ClearTranslation();
                        mat.Invert();
                        rot = ANIM.quattoeul(mat.ExtractRotation()); // TODO: We need a better conversion code for this
                    }else
                    if (node.Name.Equals("extra")) { }
                    else
                    {
                        Console.WriteLine(node.Name);
                    }
                }
            }
        }

        #endregion
    }
}
