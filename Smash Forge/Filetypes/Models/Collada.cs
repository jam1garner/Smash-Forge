using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml;
using System.Drawing;
using System.IO;
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

        public static void DAEtoNUD(string fname, ModelContainer con, bool importTexture = false)
        {
            Collada dae = new Collada();
            dae.Read(fname);

            NUD n = new NUD();
            con.nud = n;

            NUT thisNut = new NUT();
            Runtime.TextureContainers.Add(thisNut);

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

                        Bone bone = new Smash_Forge.Bone(vbn);
                        vbn.bones.Add(bone);
                        bone.Text = bo.name;
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
                        //bone.children = new List<int>();

                        //if (bone.parentIndex != 0x0FFFFFFF && bone.parentIndex > -1)
                        //    vbn.bones[bone.parentIndex].children.Add(parenttrack.IndexOf(bo));
                    }

                    vbn.reset();
                    vbn.update();
                }
                if (node.type.Equals("NODE"))
                {
                    // this will link the controller to the geometry

                }
            }

            //grab all that material data so we can apply images later
            Dictionary<string, ColladaImages> images = new Dictionary<string, ColladaImages>();
            foreach (var image in dae.library_images)
                images.Add(image.id, image);
            Dictionary<string, ColladaEffects> effects = new Dictionary<string, ColladaEffects>();
            foreach (var efc in dae.library_effects)
                effects.Add(efc.id, efc);
            Dictionary<string, ColladaMaterials> materials = new Dictionary<string, ColladaMaterials>();
            foreach (var mat in dae.library_materials)
                materials.Add(mat.id, mat);

            Dictionary<string, NUT.NUD_Texture> existingTextures = new Dictionary<string, NUT.NUD_Texture>();

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
                for (int i = 0; i < skin.weights.count; i++)
                {
                    //basically, I need to find all verts that use this position and apply that.........

                    int count = skin.weights.vcount[i];
                    if (count > 4)
                    {
                        MessageBox.Show("Error: More than 4 weights detected!");
                        return;
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
                                    if (bname.StartsWith("_"))
                                        bname = bname.Substring(6, bname.Length - 6);
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
            foreach (ColladaGeometry geom in dae.library_geometries)
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
                Matrix4 nodeTrans = Matrix4.CreateScale(1,1,1);

                foreach(ColladaNode node in dae.scene.nodes)
                {
                    if (node.geom_id.Equals(geom.id))
                    {
                        nodeTrans = node.mat;
                        break;
                    }
                }

                geometries.Add("#" + geom.id, nmesh);
                n.mesh.Add(nmesh);
                nmesh.Text = geom.name;
                NUD.Polygon npoly = new NUD.Polygon();
                npoly.setDefaultMaterial();
                nmesh.Nodes.Add(npoly);

                int i = 0;
                while (i < p.p.Length)
                {
                    if (importTexture)
                        if (p.type == ColladaPrimitiveType.triangles)
                        {
                            NUT.NUD_Texture tempTex = null;
                            ColladaMaterials mat = null;
                            ColladaEffects eff = null;
                            ColladaImages img = null;
                            string matId = null;
                            dae.scene.MaterialIds.TryGetValue(p.materialid, out matId);
                            if (matId != null && matId[0] == '#')
                                materials.TryGetValue(matId.Substring(1, matId.Length - 1), out mat);
                            if (mat != null && mat.effecturl[0] == '#')
                                effects.TryGetValue(mat.effecturl.Substring(1, mat.effecturl.Length - 1), out eff);
                            if (eff != null && eff.source[0] == '#')
                                images.TryGetValue(eff.source.Substring(1, eff.source.Length - 1), out img);
                            if (img != null)
                                existingTextures.TryGetValue(img.initref, out tempTex);
                            //Console.WriteLine($"mat {mat != null} | matid = {p.materialid}");
                            if (tempTex == null && img != null && File.Exists(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fname), img.initref))))
                            {
                                //Console.WriteLine($"Importing texture {img.initref}");
                                NUT.NUD_Texture tex = null;
                                if (fname.ToLower().EndsWith(".dds"))
                                {
                                    DDS dds = new DDS(new FileData(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fname), img.initref))));
                                    tex = dds.toNUT_Texture();
                                }
                                if (fname.ToLower().EndsWith(".png"))
                                {
                                    tex = NUTEditor.fromPNG(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fname), img.initref)), 1);
                                }
                                if (tex == null) continue;
                                tex.id = 0x40FFFF00;
                                while (NUT.texIdUsed(tex.id))
                                    tex.id++;
                                thisNut.textures.Add(tex);
                                thisNut.draw.Add(tex.id, NUT.loadImage(tex));
                                existingTextures.Add(img.initref, tex);
                                tempTex = tex;
                            }
                            if (tempTex != null)
                            {
                                npoly.materials[0].textures[0].hash = tempTex.id;
                            }
                        }

                    NUD.Vertex v = new NUD.Vertex();
                    foreach (ColladaInput input in p.inputs)
                    {
                        if (input.semantic == SemanticType.VERTEX)
                        {
                            v = new NUD.Vertex();
                            if (dae.library_controllers.Count > 0)
                            {
                                v.node.AddRange(vertices["#" + geom.id][p.p[i]].node);
                                v.weight.AddRange(vertices["#" + geom.id][p.p[i]].weight);
                            }
                            npoly.vertices.Add(v);
                            npoly.faces.Add(npoly.vertices.IndexOf(v));
                            foreach (ColladaInput vinput in mesh.vertices.inputs)
                            {
                                ReadSemantic(vinput, v, p.p[i], sources);
                            }
                        }
                        else
                            ReadSemantic(input, v, p.p[i], sources);
                        i++;
                    }

                    v.pos = Vector3.Transform(v.pos, nodeTrans);
                    if (v.nrm != null)
                        v.nrm = Vector3.TransformNormal(v.nrm, nodeTrans);

                    if (dae.library_controllers.Count > 0)
                    {
                        v.pos = Vector3.Transform(v.pos, bindMatrix["#" + geom.id]);
                        if (v.nrm != null)
                            v.nrm = Vector3.TransformNormal(v.nrm, bindMatrix["#" + geom.id]);
                    }
                }

                while (npoly.materials.Count < npoly.vertices[0].tx.Count)
                {
                    NUD.Polygon nd = new NUD.Polygon();
                    nd.setDefaultMaterial();
                    npoly.materials.Add(nd.materials[0]);
                }
            }

            // then image materials and effects

            n.Optimize();
            n.PreRender();
        }

        private static void ReadSemantic(ColladaInput input, NUD.Vertex v, int p, Dictionary<string, ColladaSource> sources)
        {
            switch (input.semantic)
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
                        n_mesh.Text = geom.name;

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
                                n_mesh.Nodes.Add(poly);
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
                                                // correct them
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
                        List<NUD.Vertex> v = ((NUD.Polygon)m.Nodes[0]).vertices;
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
                foreach (NUD.Polygon poly in mesh.Nodes)
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
        
        public static void SaveBoneNodes(Collada dae, Bone b, VBN vbn, ColladaNode parent)
        {
            ColladaNode node = new ColladaNode();
            if (parent != null)
                parent.children.Add(node);
            else
                dae.scene.nodes.Add(node);
            node.name = b.Text;
            node.id = node.name + "_id";
            node.type = "JOINT";
            node.mat = Matrix4.CreateScale(b.sca) * Matrix4.CreateFromQuaternion(b.rot) * Matrix4.CreateTranslation(b.pos);
            node.pos = b.pos;
            node.sca = b.sca;
            node.rot = ANIM.quattoeul(b.rot);
            foreach (var bone in b.GetChildren())
                SaveBoneNodes(dae, bone, vbn, node);
        }


        public static void Save(string fname, DAT dat)
        {
            Collada dae = new Collada();
            // bones

            SaveBoneNodes(dae, dat.bones.bones[0], dat.bones, null);

            // images
            Dictionary<Bitmap, string> texbank = new Dictionary<Bitmap, string>();
            int tid = 0;
            foreach(int tex in dat.texturesLinker.Keys)
            {
                ColladaImages image = new ColladaImages();
                dae.library_images.Add(image);
                image.id = "Tex" + tid;
                image.name = image.id;
                image.initref = image.id + ".png";
                dat.texturesLinker[tex].Save(fname.Substring(0, fname.LastIndexOf("\\")+1) + image.initref);
                texbank.Add(dat.texturesLinker[tex], "#" + image.id);
                tid++;
            }

            // geometry

            int num = 0;
            foreach (var da in dat.displayList)
            {
                DAT.DOBJ data = (DAT.DOBJ)da.Tag;
                ColladaGeometry geom = new ColladaGeometry();
                dae.library_geometries.Add(geom);
                geom.name = "Mesh_" + dat.displayList.IndexOf(da);
                geom.id = "Mesh_" + dat.displayList.IndexOf(da);
                geom.mesh = new ColladaMesh();

                // create a node for this
                ColladaNode colnode = new ColladaNode();
                dae.scene.nodes.Add(colnode);
                colnode.id = "VisualScene" + num;
                colnode.name = geom.name;
                colnode.geomid = "#" + geom.id;
                colnode.type = "NODE";
                colnode.instance = "instance_controller";

                // create material
                ColladaMaterials mat = new ColladaMaterials();
                mat.id = "VisualMaterial" + num;
                mat.effecturl = "#Effect" + num;
                dae.library_materials.Add(mat);
                colnode.materialSymbol = "Material" + num;
                colnode.materialTarget = "#" + mat.id;

                ColladaEffects eff = new ColladaEffects();
                eff.id = "Effect" + num;
                eff.name = geom.name + "-effect";
                if (data.material.texture.image != null && texbank.ContainsKey(data.material.texture.image))
                    eff.source = texbank[data.material.texture.image];
                else
                    eff.source = texbank[texbank.Keys.First()];
                dae.library_effects.Add(eff);

                ColladaSampler2D samp = new ColladaSampler2D();
                if(data.material.texture.image != null && texbank.ContainsKey(data.material.texture.image))
                    samp.url = texbank[data.material.texture.image];
                else
                    samp.url = texbank[texbank.Keys.First()];
                eff.sampler = samp;
                Dictionary<int, COLLADA_WRAPMODE> wraptranslate = new Dictionary<int, COLLADA_WRAPMODE>
                {
                    {0, COLLADA_WRAPMODE.CLAMP },
                    {1, COLLADA_WRAPMODE.REPEAT },
                    {2, COLLADA_WRAPMODE.MIRROR }
                };
                samp.wrap_t = wraptranslate[data.material.texture.wrap_t];
                samp.wrap_s = wraptranslate[data.material.texture.wrap_t];

                // create vertex object
                ColladaVertices vertex = new ColladaVertices();
                vertex.id = geom.name + "_verts";
                geom.mesh.vertices = vertex;

                // create polygon objects (NUD uses basically 1)
                ColladaPolygons p = new ColladaPolygons();
                ColladaInput inv = new ColladaInput();
                inv.offset = 0;
                inv.semantic = SemanticType.VERTEX;
                inv.source = "#" + "Mesh_" + dat.displayList.IndexOf(da) + "_verts";
                List<DAT.Vertex> usedVertices = new List<DAT.Vertex>();
                List<int> faces = new List<int>();
                p.materialid = "Material" + num;
                foreach (DAT.POBJ poly in data.polygons)
                {
                    foreach (DAT.POBJ.DisplayObject di in poly.display)
                    {
                        List<int> f = di.faces;
                        if (di.type == 0x98)
                            f = TriangleTools.fromTriangleStrip(di.faces);
                        else
                        if (di.type == 0x80)
                            f = TriangleTools.fromQuad(di.faces);

                        foreach (int index in f)
                        {
                            if (!usedVertices.Contains(dat.vertBank[index]))
                                usedVertices.Add(dat.vertBank[index]);
                            faces.Add(usedVertices.IndexOf(dat.vertBank[index]));
                        }
                    }
                }
                p.inputs.Add(inv);
                p.count = faces.Count;
                p.p = faces.ToArray();
                geom.mesh.polygons.Add(p);

                // create sources... this may take a minute
                // POSITION
                {
                    ColladaSource src = new ColladaSource();
                    geom.mesh.sources.Add(src);
                    src.id = geom.name + "_pos";
                    //src.name = mesh.name + "src1";
                    vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.POSITION });
                    List<string> d = new List<string>();
                    foreach (DAT.Vertex v in usedVertices)
                    {
                        d.AddRange(new string[] { v.pos.X.ToString(), v.pos.Y.ToString(), v.pos.Z.ToString() });
                    }
                    src.accessor.Add("X");
                    src.accessor.Add("Y");
                    src.accessor.Add("Z");
                    src.data = d.ToArray();
                    src.count = d.Count * 3;
                }
                // NORMAL
                {
                    ColladaSource src = new ColladaSource();
                    geom.mesh.sources.Add(src);
                    src.id = geom.name + "_nrm";
                    //src.name = mesh.name + "src1";
                    vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.NORMAL });
                    List<string> d = new List<string>();
                    foreach (DAT.Vertex v in usedVertices)
                    {
                        d.AddRange(new string[] { v.nrm.X.ToString(), v.nrm.Y.ToString(), v.nrm.Z.ToString() });
                    }
                    src.accessor.Add("X");
                    src.accessor.Add("Y");
                    src.accessor.Add("Z");
                    src.data = d.ToArray();
                    src.count = d.Count * 3;
                }
                // TEXTURE
                {
                    ColladaSource src = new ColladaSource();
                    geom.mesh.sources.Add(src);
                    src.id = geom.name + "_tx0";
                    //src.name = mesh.name + "src1";
                    vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.TEXCOORD });
                    List<string> d = new List<string>();
                    foreach (DAT.Vertex v in usedVertices)
                    {
                        d.AddRange(new string[] {( v.tx0.X * data.material.texture.scale_w).ToString(), (1-(v.tx0.Y * data.material.texture.scale_h)).ToString() });
                    }
                    src.accessor.Add("S");
                    src.accessor.Add("T");
                    src.data = d.ToArray();
                    src.count = d.Count * 2;
                }
                // COLOR
                {
                    ColladaSource src = new ColladaSource();
                    geom.mesh.sources.Add(src);
                    src.id = geom.name + "_clr";
                    //src.name = mesh.name + "src1";
                    vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.COLOR });
                    List<string> d = new List<string>();
                    foreach (DAT.Vertex v in usedVertices)
                    {
                        d.AddRange(new string[] { (v.clr.X).ToString(), (v.clr.Y).ToString(), (v.clr.Z).ToString(), (v.clr.W).ToString() });
                    }
                    src.accessor.Add("R");
                    src.accessor.Add("G");
                    src.accessor.Add("B");
                    src.accessor.Add("A");
                    src.data = d.ToArray();
                    src.count = d.Count * 4;
                }

                // create controllers too
                ColladaController control = new ColladaController();
                control.id = "Controller" + num;
                colnode.geomid = "#" + control.id;
                dae.library_controllers.Add(control);
                ColladaSkin skin = new ColladaSkin();
                control.skin = skin;
                skin.source = "#" + geom.id;
                skin.mat = Matrix4.CreateScale(1, 1, 1);
                skin.joints = new ColladaJoints();

                ColladaVertexWeights weights = new ColladaVertexWeights();
                skin.weights = weights;

                // JOINT
                {
                    ColladaSource src = new ColladaSource();
                    skin.sources.Add(src);
                    src.id = control.id + "_joints";
                    src.type = ArrayType.Name_array;
                    //src.name = mesh.name + "src1";
                    skin.joints.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.JOINT });
                    weights.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.JOINT, offset = 0 });
                    List<string> d = new List<string>();
                    foreach (Bone b in dat.bones.bones)
                        d.Add(b.Text);
                    src.accessor.Add("JOINT");
                    src.data = d.ToArray();
                    src.count = d.Count;
                }
                // INVTRANSFORM
                {
                    ColladaSource src = new ColladaSource();
                    skin.sources.Add(src);
                    src.id = control.id + "_trans";
                    skin.joints.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.INV_BIND_MATRIX });
                    List<string> d = new List<string>();
                    foreach (Bone b in dat.bones.bones)
                    {
                        d.Add(b.invert.M11 + " " + b.invert.M21 + " " + b.invert.M31 + " " + b.invert.M41 + " "
                            + b.invert.M12 + " " + b.invert.M22 + " " + b.invert.M32 + " " + b.invert.M42 + " "
                            + b.invert.M13 + " " + b.invert.M23 + " " + b.invert.M33 + " " + b.invert.M43 + " "
                            + b.invert.M14 + " " + b.invert.M24 + " " + b.invert.M34 + " " + b.invert.M44);
                    }
                    src.accessor.Add("TRANSFORM");
                    src.data = d.ToArray();
                    src.count = d.Count * 16;
                }
                // WEIGHT
                {
                    ColladaSource src = new ColladaSource();
                    skin.sources.Add(src);
                    src.id = control.id + "_weights";
                    weights.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.WEIGHT, offset = 1 });
                    List<string> d = new List<string>();
                    List<int> vcount = new List<int>();
                    List<int> vert = new List<int>();
                    foreach (DAT.Vertex v in usedVertices)
                    {
                        int vc = 0;

                        for (int i = 0; i < v.bones.Count; i++)
                        {
                            string w = v.weights[i].ToString();
                            if (w.Equals("0")) continue;
                            vc++;
                            if (!d.Contains(w))
                                d.Add(w);
                            vert.Add(v.bones[i]);
                            vert.Add(d.IndexOf(w));
                        };
                        vcount.Add(vc);
                    }
                    
                    weights.vcount = vcount.ToArray();
                    weights.v = vert.ToArray();
                    src.accessor.Add("WEIGHT");
                    src.data = d.ToArray();
                    src.count = d.Count;
                }

                num++;
            }

            dae.Write(fname);
        }

        public static void Save(string fname, ModelContainer con)
        {
            Collada dae = new Collada();

            if (con.dat_melee != null)
            {
                Save(fname, con.dat_melee);
                return;
            }
            NUD nud = con.nud;


            // bones

            if(con.vbn != null)
                SaveBoneNodes(dae, con.vbn.bones[0], con.vbn, null);

            // images
            /*int defaultTexture = -1;
            foreach (int tex in nud.GetTexIds())
            {
                ColladaImages image = new ColladaImages();
                dae.library_images.Add(image);
                image.id = "Tex_0x" + tex.ToString("X");
                image.name = image.id;
                image.initref = image.id + ".dds";

                //dat.texturesLinker[tex].Save(fname.Substring(0, fname.LastIndexOf("\\") + 1) + image.initref);
                if (defaultTexture == -1)
                    defaultTexture = tex;
            }*/

            // geometry

            int num = 0;
            foreach (NUD.Mesh mesh in nud.mesh)
            {
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    ColladaGeometry geom = new ColladaGeometry();
                    dae.library_geometries.Add(geom);
                    geom.name = mesh.Text;
                    geom.id = mesh.Text + mesh.Nodes.IndexOf(poly); ;
                    geom.mesh = new ColladaMesh();

                    // create a node for this
                    ColladaNode colnode = new ColladaNode();
                    dae.scene.nodes.Add(colnode);
                    colnode.id = "VisualScene" + num;
                    colnode.name = geom.name;
                    colnode.geomid = "#" + geom.id;
                    colnode.type = "NODE";
                    colnode.instance = "instance_controller";

                    // create material
                    ColladaMaterials mat = new ColladaMaterials();
                    mat.id = "VisualMaterial" + num;
                    mat.effecturl = "#Effect" + num;
                    dae.library_materials.Add(mat);
                    colnode.materialSymbol = "Material" + num;
                    colnode.materialTarget = "#" + mat.id;
                    
                    ColladaEffects eff = new ColladaEffects();
                    eff.id = "Effect" + num;
                    eff.name = geom.name + "-effect";
                    eff.source = eff.name + "tex";
                    dae.library_effects.Add(eff);

                    ColladaImages img = new ColladaImages();
                    img.id = eff.source;
                    img.initref = "./" + poly.materials[0].textures[0].hash.ToString("x") + ".png";
                    dae.library_images.Add(img);

                    ColladaSampler2D samp = new ColladaSampler2D();
                    if (poly.materials[0] != null)
                        samp.url = $"Tex_0x{poly.materials[0].displayTexId}";
                    else
                        samp.url = $"Tex_0x{0}";
                    eff.sampler = samp;
                    Dictionary<int, COLLADA_WRAPMODE> wraptranslate = new Dictionary<int, COLLADA_WRAPMODE>
                {
                    {0, COLLADA_WRAPMODE.CLAMP },
                    {1, COLLADA_WRAPMODE.REPEAT },
                    {2, COLLADA_WRAPMODE.MIRROR }
                };
                    //samp.wrap_t = wraptranslate[poly.materials[0].textures[0].WrapMode1];
                    //samp.wrap_s = wraptranslate[poly.materials[0].textures[0].WrapMode2];

                    // create vertex object
                    ColladaVertices vertex = new ColladaVertices();
                    vertex.id = mesh.Text + mesh.Nodes.IndexOf(poly) + "_verts";
                    geom.mesh.vertices = vertex;

                    // create sources... this may take a minute
                    // POSITION
                    {
                        ColladaSource src = new ColladaSource();
                        geom.mesh.sources.Add(src);
                        src.id = mesh.Text + mesh.Nodes.IndexOf(poly) + "_pos";
                        //src.name = mesh.name + "src1";
                        vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.POSITION });
                        List<string> d = new List<string>();
                        foreach (NUD.Vertex v in poly.vertices)
                        {
                            d.AddRange(new string[] { v.pos.X.ToString(), v.pos.Y.ToString(), v.pos.Z.ToString() });
                        }
                        src.accessor.Add("X");
                        src.accessor.Add("Y");
                        src.accessor.Add("Z");
                        src.data = d.ToArray();
                        src.count = d.Count * 3;
                    }
                    // NORMAL
                    {
                        ColladaSource src = new ColladaSource();
                        geom.mesh.sources.Add(src);
                        src.id = mesh.Text + mesh.Nodes.IndexOf(poly) + "_nrm";
                        //src.name = mesh.name + "src1";
                        vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.NORMAL });
                        List<string> d = new List<string>();
                        foreach (NUD.Vertex v in poly.vertices)
                        {
                            d.AddRange(new string[] { v.nrm.X.ToString(), v.nrm.Y.ToString(), v.nrm.Z.ToString() });
                        }
                        src.accessor.Add("X");
                        src.accessor.Add("Y");
                        src.accessor.Add("Z");
                        src.data = d.ToArray();
                        src.count = d.Count * 3;
                    }
                    // TEXTURE
                    {
                        ColladaSource src = new ColladaSource();
                        geom.mesh.sources.Add(src);
                        src.id = mesh.Text + mesh.Nodes.IndexOf(poly) + "_tx0";
                        //src.name = mesh.name + "src1";
                        vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.TEXCOORD });
                        List<string> d = new List<string>();
                        foreach (NUD.Vertex v in poly.vertices)
                        {
                            d.AddRange(new string[] { v.tx[0].X.ToString(), v.tx[0].Y.ToString()});
                        }
                        src.accessor.Add("S");
                        src.accessor.Add("T");
                        src.data = d.ToArray();
                        src.count = d.Count * 2;
                    }
                    // COLOR
                    {
                        ColladaSource src = new ColladaSource();
                        geom.mesh.sources.Add(src);
                        src.id = mesh.Text + mesh.Nodes.IndexOf(poly) + "_clr";
                        //src.name = mesh.name + "src1";
                        vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.COLOR });
                        List<string> d = new List<string>();
                        foreach (NUD.Vertex v in poly.vertices)
                        {
                            d.AddRange(new string[] { (v.col.X/128).ToString(), (v.col.Y / 128).ToString(), (v.col.Z / 128).ToString(), (v.col.W / 128).ToString() });
                        }
                        src.accessor.Add("R");
                        src.accessor.Add("G");
                        src.accessor.Add("B");
                        src.accessor.Add("A");
                        src.data = d.ToArray();
                        src.count = d.Count * 4;
                    }

                    // create polygon objects (NUD uses basically 1)
                    ColladaPolygons p = new ColladaPolygons();
                    ColladaInput inv = new ColladaInput();
                    inv.offset = 0;
                    inv.semantic = SemanticType.VERTEX;
                    inv.source = "#" + mesh.Text + mesh.Nodes.IndexOf(poly) + "_verts";
                    p.inputs.Add(inv);
                    p.count = poly.displayFaceSize;
                    p.p = poly.getDisplayFace().ToArray();
                    geom.mesh.polygons.Add(p);

                    // create controllers too
                    ColladaController control = new ColladaController();
                    control.id = "Controller" + num;
                    colnode.geomid = "#" + control.id;
                    dae.library_controllers.Add(control);
                    ColladaSkin skin = new ColladaSkin();
                    control.skin = skin;
                    skin.source = "#" + geom.id;
                    skin.mat = Matrix4.CreateScale(1,1,1);
                    skin.joints = new ColladaJoints();
                    
                    ColladaVertexWeights weights = new ColladaVertexWeights();
                    skin.weights = weights;

                    // JOINT
                    {
                        ColladaSource src = new ColladaSource();
                        skin.sources.Add(src);
                        src.id = control.id + "_joints";
                        src.type = ArrayType.Name_array;
                        //src.name = mesh.name + "src1";
                        skin.joints.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.JOINT });
                        weights.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.JOINT, offset = 0 });
                        List<string> d = new List<string>();
                        foreach (Bone b in con.vbn.bones)
                            d.Add(b.Text);
                        src.accessor.Add("JOINT");
                        src.data = d.ToArray();
                        src.count = d.Count;
                    }
                    // INVTRANSFORM
                    {
                        ColladaSource src = new ColladaSource();
                        skin.sources.Add(src);
                        src.id = control.id + "_trans";
                        skin.joints.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.INV_BIND_MATRIX });
                        List<string> d = new List<string>();
                        foreach (Bone b in con.vbn.bones)
                        {
                            d.Add(b.invert.M11 + " " + b.invert.M21 + " " + b.invert.M31 + " " + b.invert.M41 + " "
                                + b.invert.M12 + " " + b.invert.M22 + " " + b.invert.M32 + " " + b.invert.M42 + " "
                                + b.invert.M13 + " " + b.invert.M23 + " " + b.invert.M33 + " " + b.invert.M43 + " "
                                + b.invert.M14 + " " + b.invert.M24 + " " + b.invert.M34 + " " + b.invert.M44);
                        }
                        src.accessor.Add("TRANSFORM");
                        src.data = d.ToArray();
                        src.count = d.Count * 16;
                    }
                    // WEIGHT
                    {
                        ColladaSource src = new ColladaSource();
                        skin.sources.Add(src);
                        src.id = control.id + "_weights";
                        weights.inputs.Add(new ColladaInput() { source = "#" + src.id, semantic = SemanticType.WEIGHT, offset=1 });
                        List<string> d = new List<string>();
                        List<int> vcount = new List<int>();
                        List<int> vert = new List<int>();
                        foreach (NUD.Vertex v in poly.vertices)
                        {
                            int vc = 0;
                            for (int i = 0; i < v.node.Count; i++)
                            {
                                string w = v.weight[i].ToString();
                                if (w.Equals("0")) continue;
                                vc++;
                                if (!d.Contains(w))
                                    d.Add(w);
                                vert.Add(v.node[i]);
                                vert.Add(d.IndexOf(w));
                            };
                            vcount.Add(vc);
                        }
                        weights.vcount = vcount.ToArray();
                        weights.v = vert.ToArray();
                        src.accessor.Add("WEIGHT");
                        src.data = d.ToArray();
                        src.count = d.Count;
                    }

                    num++;
                }
            }

            dae.Write(fname);
        }

        private void Write(string fname)
        {
            XmlDocument doc = new XmlDocument();
            
            XmlNode colladaNode = doc.CreateElement("COLLADA");
            colladaNode.Attributes.Append(createAttribute(doc, "xmlns", "http://www.collada.org/2005/11/COLLADASchema"));
            colladaNode.Attributes.Append(createAttribute(doc, "version", "1.4.1"));

            // asset

            XmlNode asset = doc.CreateElement("asset");
            XmlNode created = doc.CreateElement("created");
            XmlNode modified = doc.CreateElement("modified");
            asset.AppendChild(created);
            asset.AppendChild(modified);
            colladaNode.AppendChild(asset);

            // library images
            XmlNode li = doc.CreateElement("library_images");
            foreach (ColladaImages geom in library_images)
            {
                geom.Write(doc, li);
            }
            colladaNode.AppendChild(li);
            
            // library materials
            XmlNode lm = doc.CreateElement("library_materials");
            foreach (ColladaMaterials geom in library_materials)
            {
                geom.Write(doc, lm);
            }
            colladaNode.AppendChild(lm);

            // library effects
            XmlNode le = doc.CreateElement("library_effects");
            foreach (ColladaEffects geom in library_effects)
            {
                geom.Write(doc, le);
            }
            colladaNode.AppendChild(le);


            // library geometries
            XmlNode lg = doc.CreateElement("library_geometries");
            foreach (ColladaGeometry geom in library_geometries)
            {
                geom.Write(doc, lg);
            }
            colladaNode.AppendChild(lg);
            
            // library controllers
            XmlNode lc = doc.CreateElement("library_controllers");
            foreach (ColladaController geom in library_controllers)
            {
                geom.Write(doc, lc);
            }
            colladaNode.AppendChild(lc);

            // library_visual_scenes
            this.scene.Write(doc, colladaNode);

            // scene
            XmlNode scene = doc.CreateElement("scene");
            XmlNode vs = doc.CreateElement("instance_visual_scene");
            vs.Attributes.Append(createAttribute(doc, "url", "#VisualSceneNode"));
            scene.AppendChild(vs);
            colladaNode.AppendChild(scene);
            
            doc.AppendChild(colladaNode);
            doc.Save(fname);
        }

        public static XmlAttribute createAttribute(XmlDocument doc, string att, string value)
        {
            XmlAttribute at = doc.CreateAttribute(att);
            at.Value = value;
            return at;
        }

        // collada containers

        Dictionary<string, object> sourceLinks = new Dictionary<string, object>();

        List<ColladaImages> library_images = new List<ColladaImages>();
        List<ColladaMaterials> library_materials = new List<ColladaMaterials>();
        List<ColladaEffects> library_effects = new List<ColladaEffects>();
        List<ColladaGeometry> library_geometries = new List<ColladaGeometry>();
        List<ColladaController> library_controllers = new List<ColladaController>();
        ColladaVisualScene scene = new ColladaVisualScene();
        public int v1, v2, v3;

        public void Read(string fname)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fname);
            XmlNode colnode = doc.ChildNodes[1];
            if(colnode == null)
                colnode = doc.ChildNodes[0];

            string v = (string)colnode.Attributes["version"].Value;
            string[] s = v.Split('.');
            int.TryParse(s[0], NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out v1);
            int.TryParse(s[1], NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out v2);
            int.TryParse(s[2], NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out v3);

            foreach (XmlNode node in colnode.ChildNodes)
            {
                /*if (f.Name.Equals("asset", true))
                    ParseAsset();*/
                Console.WriteLine(node.Name);
                if (node.Name.Equals("library_images"))
                    ParseImages(node);
                else if (node.Name.Equals("library_materials"))
                    ParseMaterials(node);
                else if (node.Name.Equals("library_effects"))
                    ParseEffects(node);
                else if (node.Name.Equals("library_geometries"))
                    ParseGeometry(node);
                else if (node.Name.Equals("library_visual_scenes"))
                    scene.Read(node);
                else if (node.Name.Equals("library_controllers"))
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
            foreach (XmlNode node in root.ChildNodes)
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
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("mesh"))
                        mesh.Read(node);
                }
            }

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("geometry");
                node.Attributes.Append(createAttribute(doc, "id", id));
                node.Attributes.Append(createAttribute(doc, "name", name));

                // write mesh
                mesh.Write(doc, node);

                parent.AppendChild(node);
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

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("mesh");

                foreach (ColladaSource src in sources)
                {
                    src.Write(doc, node);
                }
                vertices.Write(doc, node);
                foreach (ColladaPolygons p in polygons)
                {
                    p.Write(doc, node);
                }

                parent.AppendChild(node);
            }
        }

        public enum ArrayType
        {
            float_array,
            Name_array
        }

        public class ColladaSource
        {
            public string id;

            public string[] data;
            public int count;
            public ArrayType type;
            public List<string> accessor = new List<string>();

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

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("source");

                node.Attributes.Append(createAttribute(doc, "id", id));
                //node.Attributes.Append(createAttribute(doc, "count", count));

                XmlNode arr = doc.CreateElement(type + "");
                node.AppendChild(arr);
                arr.Attributes.Append(createAttribute(doc, "id", id + "-array"));
                arr.Attributes.Append(createAttribute(doc, "count", count + ""));
                string aa = "";
                foreach (string s in data) aa += s + " ";
                arr.InnerText = aa;

                XmlNode tc = doc.CreateElement("technique_common");
                node.AppendChild(tc);
                XmlNode accessor = doc.CreateElement("accessor");
                accessor.Attributes.Append(createAttribute(doc, "source", "#" + id + "-array"));
                accessor.Attributes.Append(createAttribute(doc, "count", (count / this.accessor.Count) + ""));
                if(this.accessor.Count > 0 && this.accessor[0].Equals("TRANSFORM"))
                    accessor.Attributes.Append(createAttribute(doc, "stride", 16 + ""));
                else
                    accessor.Attributes.Append(createAttribute(doc, "stride", this.accessor.Count + ""));
                tc.AppendChild(accessor);

                foreach (string param in this.accessor)
                {
                    XmlNode pa = doc.CreateElement("param");
                    accessor.AppendChild(pa);
                    pa.Attributes.Append(createAttribute(doc, "name", param));
                    if(param.Equals("TRANSFORM"))
                        pa.Attributes.Append(createAttribute(doc, "type", "float4x4"));
                    else
                        pa.Attributes.Append(createAttribute(doc, "type", type.ToString().Replace("_array", "")));
                }

                parent.AppendChild(node);
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

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("vertices");

                node.Attributes.Append(createAttribute(doc, "id", id));

                foreach (ColladaInput input in inputs)
                {
                    input.Write(doc, node);
                }

                parent.AppendChild(node);
            }
        }

        public class ColladaPolygons
        {
            public ColladaPrimitiveType type = ColladaPrimitiveType.triangles;
            public List<ColladaInput> inputs = new List<ColladaInput>();
            public int[] p;
            public int count;
            public string materialid;

            public void Read(XmlNode root)
            {
                foreach(XmlAttribute att in root.Attributes)
                {
                    if (att.Name.Equals("material")) materialid = (string)att.Value;
                    if (att.Name.Equals("count")) int.TryParse((string)att.Value, out count);
                }

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
                        for (int i = 0; i < ps.Length; i++)
                            p[i] = int.Parse(ps[i]);
                    }
                }
            }

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement(type.ToString());

                node.Attributes.Append(createAttribute(doc, "material", materialid));
                node.Attributes.Append(createAttribute(doc, "count", count+""));

                foreach (ColladaInput input in inputs)
                {
                    input.Write(doc, node);
                }
                string p = "";
                foreach(int i in this.p)
                {
                    p += i + " ";
                }
                XmlNode pi = doc.CreateElement("p");
                pi.InnerText = p;
                node.AppendChild(pi);

                parent.AppendChild(node);
            }
        }

        public class ColladaInput
        {
            public SemanticType semantic;
            public string source;
            public int set = -99, offset =-99;

            public void Read(XmlNode root)
            {
                semantic = (SemanticType)Enum.Parse(typeof(SemanticType), (string)root.Attributes["semantic"].Value);
                source = (string)root.Attributes["source"].Value;
                if (root.Attributes["set"] != null)
                    int.TryParse((string)root.Attributes["set"].Value, out set);
                if (root.Attributes["offset"] != null)
                    int.TryParse((string)root.Attributes["offset"].Value, out offset);
            }

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("input");

                node.Attributes.Append(createAttribute(doc, "semantic", semantic.ToString()));
                node.Attributes.Append(createAttribute(doc, "source", source));
                if (set != -99)
                    node.Attributes.Append(createAttribute(doc, "set", set + ""));
                if (offset != -99)
                    node.Attributes.Append(createAttribute(doc, "offset", offset+""));

                parent.AppendChild(node);

            }
        }
        #endregion

        #region Materials
        // Images and Materials

        public void ParseMaterials(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                ColladaMaterials m = new ColladaMaterials();
                m.Read(node);
                library_materials.Add(m);
            }
        }

        public void ParseImages(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                ColladaImages m = new ColladaImages();
                m.Read(node);
                library_images.Add(m);
            }
        }

        public void ParseEffects(XmlNode root)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                ColladaEffects m = new ColladaEffects();
                m.Read(node);
                library_effects.Add(m);
            }
        }

        public class ColladaImages
        {
            public string id, name, initref;

            public void Read(XmlNode root)
            {
                id = root.Attributes["id"].Value;
                //name = root.Attributes["name"].Value;
                foreach (XmlNode child in root.ChildNodes)
                {
                    if (child.Name.Equals("init_from"))
                    {
                        initref = child.InnerText;
                        if (initref.StartsWith("file://"))
                            initref = initref.Substring(7, initref.Length - 7);
                    }
                }
            }

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("image");
                node.Attributes.Append(createAttribute(doc, "id", id));
                node.Attributes.Append(createAttribute(doc, "name", name));

                XmlNode init = doc.CreateElement("init_from");
                init.InnerText=initref;
                node.AppendChild(init);

                parent.AppendChild(node);
            }
        }
        public class ColladaMaterials
        {
            public string id, effecturl;

            public void Read(XmlNode root)
            {
                id = root.Attributes["id"].Value;
                foreach (XmlNode node in root.ChildNodes)
                    if (node.Name.Equals("instance_effect") && node.Attributes["url"] != null)
                        effecturl = node.Attributes["url"].Value;
            }

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("material");
                node.Attributes.Append(createAttribute(doc, "id", id));
                
                XmlNode init = doc.CreateElement("instance_effect");
                init.Attributes.Append(createAttribute(doc, "url", effecturl));
                node.AppendChild(init);

                parent.AppendChild(node);
            }
        }
        public class ColladaEffects
        {
            public string id, name;
            public string source = "#";
            public ColladaSampler2D sampler;

            public void Read(XmlNode root)
            {
                foreach(XmlAttribute att in root.Attributes)
                {
                    if (att.Name.Equals("id")) id = att.Value;
                    if (att.Name.Equals("name")) name = root.Attributes["name"].Value;
                }

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("profile_COMMON"))
                    {
                        readEffectTechnique(node);
                    }
                }
            }

            private void readEffectTechnique(XmlNode root)
            {
                Dictionary<string, XmlNode> surfaces = new Dictionary<string, XmlNode>();
                Dictionary<string, XmlNode> samplers = new Dictionary<string, XmlNode>();
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("newparam") && node.ChildNodes[0].Name.Equals("surface"))
                        surfaces.Add(node.Attributes["sid"].Value, node.ChildNodes[0]);
                    if (node.Name.Equals("newparam") && node.ChildNodes[0].Name.Equals("sampler2D"))
                        samplers.Add(node.Attributes["sid"].Value, node.ChildNodes[0]);
                }
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.Equals("technique") && node.ChildNodes[0].Name.Equals("phong"))
                    {
                        foreach (XmlNode node1 in node.ChildNodes[0].ChildNodes)
                        {
                            if (node1.Name.Equals("diffuse"))
                            {
                                foreach (XmlNode node2 in node1.ChildNodes)
                                {
                                    if (node2.Name.Equals("texture"))
                                    {
                                        string texture = node2.Attributes["texture"].Value;
                                        XmlNode temp = null;
                                        samplers.TryGetValue(texture, out temp);
                                        if (temp != null)
                                        {
                                            foreach (XmlNode node3 in temp.ChildNodes)
                                            {
                                                if (node3.Name.Equals("source"))
                                                {
                                                    //if you are reading this I am sorry
                                                    XmlNode temp2 = null;
                                                    surfaces.TryGetValue(node3.InnerText, out temp2);
                                                    if (temp2 != null)
                                                    {
                                                        texture = temp2.ChildNodes[0].InnerText;
                                                    }
                                                }
                                            }
                                        }
                                        source = "#" + texture;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // for writing
            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("effect");
                node.Attributes.Append(createAttribute(doc, "id", id));
                node.Attributes.Append(createAttribute(doc, "name", name));

                XmlNode prof = doc.CreateElement("profile_COMMON");
                node.AppendChild(prof);
                {
                    XmlNode np = doc.CreateElement("newparam");
                    prof.AppendChild(np);
                    np.Attributes.Append(createAttribute(doc, "sid", id + "-surface"));
                    
                    XmlNode sur = doc.CreateElement("surface");
                    np.AppendChild(sur);
                    XmlNode init = doc.CreateElement("init_from");
                    sur.AppendChild(init);
                    init.InnerText = source.Replace("#", "");
                    sur.Attributes.Append(createAttribute(doc, "type", "2D"));
                }
                {
                    XmlNode np = doc.CreateElement("newparam");
                    prof.AppendChild(np);
                    np.Attributes.Append(createAttribute(doc, "sid", id + "-sampler"));
                    sampler.source = id + "-surface";
                    sampler.Write(doc, np);
                }
                {
                    XmlNode tech = doc.CreateElement("technique");
                    prof.AppendChild(tech);
                    tech.Attributes.Append(createAttribute(doc, "sid", "COMMON"));

                    XmlNode sur = doc.CreateElement("phong");
                    tech.AppendChild(sur);
                    XmlNode init = doc.CreateElement("diffuse");
                    sur.AppendChild(init);
                    XmlNode reff = doc.CreateElement("texture");
                    init.AppendChild(reff);
                    reff.Attributes.Append(createAttribute(doc, "texture", id + "-sampler"));
                    reff.Attributes.Append(createAttribute(doc, "texcoord", ""));
                }

                parent.AppendChild(node);
            }
        }

        public enum COLLADA_WRAPMODE
        {
            WRAP,
            REPEAT,
            CLAMP,
            CLAMP_TO_EDGE,
            MIRROR
        }
        public enum COLLADA_FILTER
        {
            NONE,
            NEAREST,
            LINEAR
        }

        public class ColladaSampler2D
        {
            public string source, url;
            public COLLADA_WRAPMODE wrap_s, wrap_t;
            public COLLADA_FILTER minfilter, magfilter;
            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("sampler2D");
                {
                    XmlNode src = doc.CreateElement("source");
                    src.InnerText = source;
                    node.AppendChild(src);
                }
                {
                    XmlNode src = doc.CreateElement("wrap_s");
                    src.InnerText = wrap_s.ToString();
                    node.AppendChild(src);
                }
                {
                    XmlNode src = doc.CreateElement("wrap_t");
                    src.InnerText = wrap_t.ToString();
                    node.AppendChild(src);
                }

                /*XmlNode inst = doc.CreateElement("instance_image");
                inst.Attributes.Append(createAttribute(doc, "url", url));
                node.AppendChild(inst);*/


                parent.AppendChild(node);
            }
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

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("controller");
                node.Attributes.Append(createAttribute(doc, "id", id));

                skin.Write(doc, node);

                parent.AppendChild(node);
            }
        }

        public class ColladaSkin
        {
            public string source;
            public Matrix4 mat = Matrix4.CreateScale(1,1,1);
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

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("skin");
                node.Attributes.Append(createAttribute(doc, "source", source));

                XmlNode matrix = doc.CreateElement("matrix");
                node.AppendChild(matrix);
                matrix.InnerText = mat.M11 + " " + mat.M21 + " " + mat.M31 + " " + mat.M41
                    + " " + mat.M12 + " " + mat.M22 + " " + mat.M32 + " " + mat.M42
                    + " " + mat.M13 + " " + mat.M23 + " " + mat.M33 + " " + mat.M43
                    + " " + mat.M14 + " " + mat.M24 + " " + mat.M34 + " " + mat.M44;
                foreach (ColladaSource src in sources)
                {
                    src.Write(doc, node);
                }
                joints.Write(doc, node);
                weights.Write(doc, node);

                parent.AppendChild(node);
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

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("joints");

                foreach (ColladaInput input in inputs)
                {
                    input.Write(doc, node);
                }

                parent.AppendChild(node);
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

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("vertex_weights");
                node.Attributes.Append(createAttribute(doc, "count", vcount.Length.ToString()));

                foreach (ColladaInput input in inputs)
                {
                    input.Write(doc, node);
                }

                XmlNode vc = doc.CreateElement("vcount");
                XmlNode p = doc.CreateElement("v");
                node.AppendChild(vc);
                node.AppendChild(p);

                string ar = "";
                foreach (int i in vcount)
                    ar += i + " ";
                vc.InnerText = ar;

                ar = "";
                foreach (int i in v)
                    ar += i + " ";
                p.InnerText = ar;

                parent.AppendChild(node);
            }
        }

        #endregion
        
        #region Visual Nodes

        public class ColladaVisualScene
        {
            public List<ColladaNode> nodes = new List<ColladaNode>();
            public string id, name;
            public Dictionary<string, string> MaterialIds = new Dictionary<string, string>();

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
                        foreach (var v in n.materialIds)
                        {
                            if(!MaterialIds.ContainsKey(v.Key))
                                MaterialIds.Add(v.Key, v.Value);
                        }
                    }
                }
            }

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("library_visual_scenes");
                XmlNode vs = doc.CreateElement("visual_scene");
                vs.Attributes.Append(createAttribute(doc, "id", "VisualSceneNode"));
                vs.Attributes.Append(createAttribute(doc, "name", "rdmscene"));
                node.AppendChild(vs);

                foreach(ColladaNode no in nodes)
                {
                    no.Write(doc, vs);
                }
                

                parent.AppendChild(node);
            }
        }

        public class ColladaNode
        {
            public ColladaNode parent;
            public string id, name, type = "NODE", geomid, instance = "";
            public List<ColladaNode> children = new List<ColladaNode>();

            public Matrix4 mat = Matrix4.CreateScale(1,1,1);
            public Vector3 pos = new Vector3(), sca = new Vector3(), rot = new Vector3();

            // material
            public string materialSymbol, materialTarget;
            public Dictionary<string,string> materialIds = new Dictionary<string, string>();

            // instance geometry
            public string geom_id = "";

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
                    }
                    else if (node.Name.Equals("matrix"))
                    {
                        string[] data = node.InnerText.Trim().Replace("\n", " ").Split(' ');
                        mat = new Matrix4();
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

                        mat.M11 = float.Parse(data[0]); mat.M12 = float.Parse(data[1]); mat.M13 = float.Parse(data[2]); mat.M14 = float.Parse(data[3]);
                        mat.M21 = float.Parse(data[4]); mat.M22 = float.Parse(data[5]); mat.M23 = float.Parse(data[6]); mat.M24 = float.Parse(data[7]);
                        mat.M31 = float.Parse(data[8]); mat.M32 = float.Parse(data[9]); mat.M33 = float.Parse(data[10]); mat.M34 = float.Parse(data[11]);
                        mat.M41 = float.Parse(data[12]); mat.M42 = float.Parse(data[13]); mat.M43 = float.Parse(data[14]); mat.M44 = float.Parse(data[15]);
                    }
                    else if (node.Name.Equals("extra"))
                    {

                    }
                    else if (node.Name.Equals("instance_geometry"))
                    {
                        geom_id = node.Attributes["url"].Value.Replace("#", "");
                    }
                    else if (node.Name.Equals("instance_controller"))
                    {
                        foreach (XmlNode node1 in node.ChildNodes)
                        {
                            if (node1.Name.Equals("bind_material"))
                            {
                                foreach (XmlNode node2 in node1.ChildNodes)
                                {
                                    if (node2.Name.Equals("technique_common"))
                                    {
                                        if(node2.ChildNodes[0].Attributes["symbol"] != null && !materialIds.ContainsKey(node2.ChildNodes[0].Attributes["symbol"].Value))
                                            materialIds.Add(node2.ChildNodes[0].Attributes["symbol"].Value, node2.ChildNodes[0].Attributes["target"].Value);
                                    }
                                }
                            }
                        }
                    }
                    else if (node.Name.Equals("translate"))
                    {
                        string[] data = node.InnerText.Trim().Replace("\n", " ").Split(' ');
                        pos = new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
                    }
                    else if (node.Name.Equals("scale"))
                    {
                        string[] data = node.InnerText.Trim().Replace("\n", " ").Split(' ');
                        sca = new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
                    }
                    else if (node.Name.Equals("rotate"))
                    {
                        string[] data = node.InnerText.Trim().Replace("\n", " ").Split(' ');
                        rot = new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
                    }
                    else
                    {
                        Console.WriteLine(node.Name);
                    }
                }
            }
            
            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("node");
                node.Attributes.Append(createAttribute(doc, "id", id));
                node.Attributes.Append(createAttribute(doc, "name", name));
                node.Attributes.Append(createAttribute(doc, "type", type));
                if(type.Equals("JOINT"))
                    node.Attributes.Append(createAttribute(doc, "sid", name));

                // transform matrix

                XmlNode matrix = doc.CreateElement("matrix");
                node.AppendChild(matrix);
                matrix.InnerText = mat.M11 + " " + mat.M21 + " " + mat.M31 + " " + mat.M41
                    + " " + mat.M12 + " " + mat.M22 + " " + mat.M32 + " " + mat.M42
                    + " " + mat.M13 + " " + mat.M23 + " " + mat.M33 + " " + mat.M43
                    + " " + mat.M14 + " " + mat.M24 + " " + mat.M34 + " " + mat.M44;

                // instance geometry (no rigging) instance controller for rigging
                if (!instance.Equals(""))
                {
                    XmlNode inst = doc.CreateElement(instance);
                    inst.Attributes.Append(createAttribute(doc, "url", geomid));
                    node.AppendChild(inst);
                    if (instance.Equals("instance_controller"))
                    {
                        XmlNode skel = doc.CreateElement("skeleton");
                        inst.AppendChild(skel);
                        skel.InnerText = "#Bone_0_id";
                    }
                    if (materialSymbol != null && materialTarget != null)
                    {
                        XmlNode bn = doc.CreateElement("bind_material");
                        inst.AppendChild(bn);
                        XmlNode tc = doc.CreateElement("technique_common");
                        bn.AppendChild(tc);
                        XmlNode im = doc.CreateElement("instance_material");
                        tc.AppendChild(im);
                        im.Attributes.Append(createAttribute(doc, "symbol", materialSymbol));
                        im.Attributes.Append(createAttribute(doc, "target", materialTarget));

                    }
                }


                foreach (ColladaNode no in children)
                {
                    no.Write(doc, node);
                }

                parent.AppendChild(node);
            }
        }

        #endregion
    }
}
