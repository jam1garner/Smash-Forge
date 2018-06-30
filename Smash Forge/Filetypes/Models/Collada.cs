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
using System.Diagnostics;

namespace Smash_Forge
{
    class Collada
    {

        public Collada()
        {

        }

        public Collada(string fileName)
        {
            Read(fileName);
        }

        public static void DaetoNud(string fileName, ModelContainer container, bool importTexture = false)
        {
            Collada dae = new Collada(fileName);

            NUD nud;
            NUT nut; 
            // The filename is just used to get the file directory.
            CreateNudNutFromDae(fileName, container, importTexture, dae, out nud, out nut);

            // Modify model container.
            container.NUD = nud;
            container.NUT = nut;
            CreateBones(container, dae);
        }

        private static void CreateNudNutFromDae(string fileName, ModelContainer container, bool importTexture, Collada dae, out NUD nud, out NUT nut)
        {
            nud = new NUD();
            nut = new NUT();
            Runtime.TextureContainers.Add(nut);

            //grab all that material data so we can apply images later
            Dictionary<string, ColladaImages> images = new Dictionary<string, ColladaImages>();
            foreach (var image in dae.library_images)
                if (!images.ContainsKey(image.id))
                    images.Add(image.id, image);
            Dictionary<string, ColladaEffects> effects = new Dictionary<string, ColladaEffects>();
            foreach (var efc in dae.library_effects)
                if (!effects.ContainsKey(efc.id))
                    effects.Add(efc.id, efc);
            Dictionary<string, ColladaMaterials> materials = new Dictionary<string, ColladaMaterials>();
            foreach (var mat in dae.library_materials)
                if (!materials.ContainsKey(mat.id))
                    materials.Add(mat.id, mat);

            Dictionary<string, NutTexture> existingTextures = new Dictionary<string, NutTexture>();

            // Controllers used for vertex skinning.
            Dictionary<string, List<NUD.Vertex>> vertexListBySkinSource = new Dictionary<string, List<NUD.Vertex>>();
            Dictionary<string, Matrix4> bindMatrixBySkinSource = new Dictionary<string, Matrix4>();
            foreach (ColladaController control in dae.library_controllers)
            {
                ColladaSkin skin = control.skin;

                Dictionary<string, ColladaSource> sources = new Dictionary<string, ColladaSource>();
                foreach (ColladaSource s in skin.sources)
                {
                    sources.Add("#" + s.id, s);
                }

                List<NUD.Vertex> verts = new List<NUD.Vertex>();
                vertexListBySkinSource.Add(skin.source, verts);
                bindMatrixBySkinSource.Add(skin.source, skin.mat);

                // Sets the bones and weights for the vertex list.
                SkinVerts(container, skin, sources, verts);
            }

            Dictionary<string, NutTexture> texturemap = new Dictionary<string, NutTexture>();

            // Don't add duplicate meshes if they share a name.
            Dictionary<string, NUD.Mesh> nudMeshByGeometryName = new Dictionary<string, NUD.Mesh>();

            foreach (ColladaGeometry geom in dae.library_geometries)
            {
                ColladaMesh mesh = geom.mesh;
                ColladaPolygons colladaPoly = mesh.polygons[0];

                Dictionary<string, ColladaSource> sources = GetVertexDataSources(mesh);

                // Find the appropriate collada node and associated matrix.
                Matrix4 nodeTrans = Matrix4.CreateScale(1, 1, 1);
                ColladaNode colladaNode = null;
                foreach (ColladaNode node in dae.scene.nodes)
                {
                    if (node.geom_id.Equals(geom.id))
                    {
                        colladaNode = node;
                        nodeTrans = node.mat;
                        break;
                    }
                }

                // Check if the mesh already exists first.
                // Remove the initial numbers so that the names aren't unique.
                NUD.Mesh nudMesh;
                string nameWithoutNumber = RemoveInitialUnderscoreId(geom.name);

                if (nudMeshByGeometryName.ContainsKey(nameWithoutNumber))
                {
                    nudMesh = nudMeshByGeometryName[nameWithoutNumber];
                }
                else
                {
                    nudMesh = new NUD.Mesh();
                    nudMesh.Text = geom.name;
                    nud.Nodes.Add(nudMesh);
                    nudMeshByGeometryName.Add(nameWithoutNumber, nudMesh);
                }

                // Always add the polygon.
                NUD.Polygon nudPolygon = new NUD.Polygon();
                nudPolygon.AddDefaultMaterial();
                nudMesh.Nodes.Add(nudPolygon);

                // Many vertices share the same vertex data.
                // If there aren't any sources, we can't do anything meaningful anyway.
                int vertexDataLength = sources.First().Value.data.Length / sources.First().Value.stride;
                NUD.Vertex[] vertexDataSource = new NUD.Vertex[vertexDataLength];

                for (int pIndex = 0; pIndex < colladaPoly.p.Length; pIndex++)
                {
                    if (importTexture && colladaPoly.type == ColladaPrimitiveType.triangles)
                    {
                        // This may or may not still work.
                        TryReadTexture(fileName, dae, nut, images, effects, materials, existingTextures, texturemap, colladaPoly, nudPolygon);
                    }

                    int maxOffset = CalculateMaxOffset(colladaPoly);
                    if (pIndex * maxOffset >= colladaPoly.p.Length)
                        break;

                    // Initialize the vertex for the given index.
                    int vertexSourceIndex = colladaPoly.p[pIndex];
                    if (vertexDataSource[vertexSourceIndex] == null)
                    {
                        NUD.Vertex v = ReadVertexSemantics(dae, vertexListBySkinSource, geom, mesh, colladaPoly, sources, nudPolygon, pIndex, maxOffset);
                        vertexDataSource[vertexSourceIndex] = v;
                        TransformVertexNormalAndPosition(dae, bindMatrixBySkinSource, geom, nodeTrans, v);
                    }
                }

                // Add the vertices and indices to the polygon.
                nudPolygon.vertices = vertexDataSource.ToList();
                nudPolygon.vertexIndices = colladaPoly.p.ToList();

                AddMaterialsForEachUvChannel(nudPolygon);
            }
        }

        public static string RemoveInitialUnderscoreId(string geometryName)
        {
            bool hasId = HasInitialUnderscoreId(geometryName);
            if (hasId)
                geometryName = geometryName.Substring(5);

            return geometryName;
        }

        public static bool HasInitialUnderscoreId(string geometryName)
        {
            // Some geometry names start with a number "_XXX_" to avoid name clashes.
            bool hasId = false;
            if (geometryName.Length > 5)
            {
                string sub = geometryName.Substring(0, 5);
                int a = 0;
                if (sub.StartsWith("_") && sub.EndsWith("_") && int.TryParse(sub.Substring(1, 3), out a))
                    hasId = true;
            }

            return hasId;
        }

        private static Dictionary<string, ColladaSource> GetVertexDataSources(ColladaMesh mesh)
        {
            Dictionary<string, ColladaSource> sources = new Dictionary<string, ColladaSource>();
            foreach (ColladaSource s in mesh.sources)
            {
                sources.Add("#" + s.id, s);
            }

            return sources;
        }

        private static void TryReadTexture(string fileName, Collada dae, NUT nut, Dictionary<string, ColladaImages> images, Dictionary<string, ColladaEffects> effects, Dictionary<string, ColladaMaterials> materials, Dictionary<string, NutTexture> existingTextures, Dictionary<string, NutTexture> texturemap, ColladaPolygons colladaPoly, NUD.Polygon npoly)
        {
            NutTexture tempTex = null;
            ColladaMaterials mat = null;
            ColladaEffects eff = null;
            ColladaImages img = null;
            string matId = null;

            dae.scene.MaterialIds.TryGetValue(colladaPoly.materialid, out matId);

            if (matId != null && matId[0] == '#')
                materials.TryGetValue(matId.Substring(1, matId.Length - 1), out mat);
            if (mat != null && mat.effecturl[0] == '#')
                effects.TryGetValue(mat.effecturl.Substring(1, mat.effecturl.Length - 1), out eff);
            if (eff != null && eff.source[0] == '#')
                images.TryGetValue(eff.source.Substring(1, eff.source.Length - 1), out img);
            if (img != null)
                existingTextures.TryGetValue(img.initref, out tempTex);

            if (texturemap.ContainsKey(img.initref))
            {
                tempTex = texturemap[img.initref];
            }
            else if (tempTex == null && img != null && File.Exists(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fileName), img.initref))))
            {
                NutTexture tex = null;
                if (img.initref.ToLower().EndsWith(".dds"))
                {
                    DDS dds = new DDS(new FileData(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fileName), img.initref))));
                    tex = dds.ToNutTexture();
                }
                if (img.initref.ToLower().EndsWith(".png"))
                {
                    tex = NUTEditor.fromPNG(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(fileName), img.initref)), 1);
                }
                if (tex == null)
                    //continue;
                    texturemap.Add(img.initref, tex);
                tex.HASHID = 0x40FFFF00;
                while (NUT.texIdUsed(tex.HASHID))
                    tex.HASHID++;
                nut.Nodes.Add(tex);
                nut.glTexByHashId.Add(tex.HASHID, NUT.CreateTexture2D(tex));
                existingTextures.Add(img.initref, tex);
                tempTex = tex;
            }
            if (tempTex != null)
            {
                npoly.materials[0].textures[0].hash = tempTex.HASHID;
            }
        }

        private static int CalculateMaxOffset(ColladaPolygons colladaPoly)
        {
            // It calculates something...
            int maxOffset = 0;
            foreach (ColladaInput input in colladaPoly.inputs)
            {
                if (input.offset > maxOffset)
                    maxOffset = input.offset;
            }
            maxOffset += 1;
            return maxOffset;
        }

        private static NUD.Vertex ReadVertexSemantics(Collada dae, Dictionary<string, List<NUD.Vertex>> vertexListBySkinSource, ColladaGeometry geom, ColladaMesh mesh, ColladaPolygons colladaPoly, Dictionary<string, ColladaSource> sources, NUD.Polygon npoly, int pIndex, int maxoffset)
        {
            NUD.Vertex v = new NUD.Vertex();
            foreach (ColladaInput input in colladaPoly.inputs)
            {
                // The SemanticType for input is always SemanticType.Vertex.
                if (input.semanticType == SemanticType.POSITION)
                {
                    // Probably won't be reached.
                    AddBoneIdsAndWeights(dae, vertexListBySkinSource, geom, colladaPoly, pIndex, maxoffset, v);
                }
                if (input.semanticType == SemanticType.VERTEX)
                {
                    v = new NUD.Vertex();

                    // Read the position, normals, texcoord, and vert color semantics.
                    foreach (ColladaInput vinput in mesh.vertices.inputs)
                    {
                        // If there is a position semantic, try and find the bone weights and IDs.
                        if (vinput.semanticType == SemanticType.POSITION)
                        {
                            AddBoneIdsAndWeights(dae, vertexListBySkinSource, geom, colladaPoly, pIndex, maxoffset, v);
                        }
                        // Read position, normals, texcoord, or vert color semantic.
                        ReadSemantic(v, vinput, colladaPoly.p[maxoffset * pIndex], sources);
                    }
                }
                else
                {
                    // Read position, normals, texcoord, or vert color semantic.
                    ReadSemantic(v, input, colladaPoly.p[(maxoffset * pIndex) + input.offset], sources);
                }
            }

            return v;
        }

        private static void TransformVertexNormalAndPosition(Collada dae, Dictionary<string, Matrix4> bindMatrixBySkinSource, ColladaGeometry geom, Matrix4 nodeTrans, NUD.Vertex v)
        {
            // Transform some vectors.
            v.pos = Vector3.TransformPosition(v.pos, nodeTrans);
            if (v.nrm != null)
                v.nrm = Vector3.TransformNormal(v.nrm, nodeTrans);

            if (dae.library_controllers.Count > 0)
            {
                if (bindMatrixBySkinSource.ContainsKey("#" + geom.id))
                {
                    v.pos = Vector3.TransformPosition(v.pos, bindMatrixBySkinSource["#" + geom.id]);
                    if (v.nrm != null)
                        v.nrm = Vector3.TransformNormal(v.nrm, bindMatrixBySkinSource["#" + geom.id]);
                }
            }
        }

        private static void AddBoneIdsAndWeights(Collada dae, Dictionary<string, List<NUD.Vertex>> vertexListBySkinSource, ColladaGeometry geom, ColladaPolygons colladaPoly, int i, int maxoffset, NUD.Vertex v)
        {
            if (dae.library_controllers.Count > 0)
            {
                if (vertexListBySkinSource.ContainsKey("#" + geom.id))
                {
                    v.boneIds.AddRange(vertexListBySkinSource["#" + geom.id][colladaPoly.p[maxoffset * i]].boneIds);
                    v.boneWeights.AddRange(vertexListBySkinSource["#" + geom.id][colladaPoly.p[maxoffset * i]].boneWeights);
                }
            }
            else
            {
                v.boneIds.Add(-1);
                v.boneWeights.Add(1);
            }
        }

        private static void AddMaterialsForEachUvChannel(NUD.Polygon npoly)
        {
            // Don't add more than 2 materials to a polygon.
            while (npoly.materials.Count < npoly.vertices[0].uv.Count && npoly.materials.Count < 2)
            {
                NUD.Material material = NUD.Material.GetDefault();
                npoly.materials.Add(material);
            }
        }

        private static void SkinVerts(ModelContainer con, ColladaSkin skin, Dictionary<string, ColladaSource> sources, List<NUD.Vertex> verts)
        {
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

                NUD.Vertex newVertex = new NUD.Vertex();

                for (int j = 0; j < count; j++)
                {
                    foreach (ColladaInput input in skin.weights.inputs)
                    {
                        switch (input.semanticType)
                        {
                            case SemanticType.JOINT:
                                string bname = sources[input.source].data[skin.weights.v[v]];
                                if (bname.StartsWith("_"))
                                    bname = bname.Substring(6, bname.Length - 6);
                                int index = con.VBN.boneIndex(bname);
                                newVertex.boneIds.Add(index);
                                break;
                            case SemanticType.WEIGHT:
                                float weight = float.Parse(sources[input.source].data[skin.weights.v[v]]);
                                newVertex.boneWeights.Add(weight);
                                break;
                        }
                        v++;
                    }
                }

                verts.Add(newVertex);
            }
        }

        private static void CreateBones(ModelContainer con, Collada dae)
        {
            // next will be nodes then controllers
            // craft vbn :>
            // find joint node
            
            foreach (ColladaNode node in dae.scene.nodes)
            {
                if (node.type.Equals("JOINT") && con.VBN == null)
                {
                    // joint tree
                    con.VBN = new VBN();
                    VBN vbn = con.VBN;

                    List<ColladaNode> parenttrack = new List<ColladaNode>();
                    Queue<ColladaNode> nodes = new Queue<ColladaNode>();
                    nodes.Enqueue(node);

                    while (nodes.Count > 0)
                    {
                        ColladaNode bo = nodes.Dequeue();
                        parenttrack.Add(bo);
                        foreach (ColladaNode child in bo.children)
                            nodes.Enqueue(child);

                        Bone bone = new Bone(vbn);
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
                        bone.scale[0] = bo.scale.X;
                        bone.scale[1] = bo.scale.X;
                        bone.scale[2] = bo.scale.X;
                    }

                    vbn.reset();
                    vbn.update();
                }
            }
        }

        private static void ReadSemantic(NUD.Vertex targetVert, ColladaInput input, int pIndex, Dictionary<string, ColladaSource> sources)
        {
            ColladaSource colladaSource = sources[input.source];

            switch (input.semanticType)
            {
                case SemanticType.POSITION:
                    targetVert.pos.X = float.Parse(colladaSource.data[pIndex * 3 + 0]);
                    targetVert.pos.Y = float.Parse(colladaSource.data[pIndex * 3 + 1]);
                    targetVert.pos.Z = float.Parse(colladaSource.data[pIndex * 3 + 2]);
                    break;
                case SemanticType.NORMAL:
                    targetVert.nrm.X = float.Parse(colladaSource.data[pIndex * 3 + 0]);
                    targetVert.nrm.Y = float.Parse(colladaSource.data[pIndex * 3 + 1]);
                    targetVert.nrm.Z = float.Parse(colladaSource.data[pIndex * 3 + 2]);
                    break;
                case SemanticType.TEXCOORD:
                    Vector2 tx = new Vector2();
                    tx.X = float.Parse(colladaSource.data[pIndex * 2 + 0]);
                    tx.Y = float.Parse(colladaSource.data[pIndex * 2 + 1]);
                    targetVert.uv.Add(tx);
                    break;
                case SemanticType.COLOR:
                    // Vertex colors are stored as integers [0,255]. (127,127,127) is white.
                    targetVert.color.X = float.Parse(colladaSource.data[pIndex * colladaSource.stride + 0]) * 255;
                    targetVert.color.Y = float.Parse(colladaSource.data[pIndex * colladaSource.stride + 1]) * 255;
                    targetVert.color.Z = float.Parse(colladaSource.data[pIndex * colladaSource.stride + 2]) * 255;
                    if(colladaSource.stride > 3)
                        targetVert.color.W = float.Parse(colladaSource.data[pIndex * colladaSource.stride + 3]) * 127;
                    break;
            }
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
            node.pos = new Vector3(b.position[0], b.position[1], b.position[2]);
            node.scale = new Vector3(b.scale[0], b.scale[1], b.scale[2]);
            node.rot = new Vector3(b.rotation[0], b.rotation[1], b.rotation[2]);
            node.mat = Matrix4.CreateScale(node.scale) * Matrix4.CreateFromQuaternion(VBN.FromEulerAngles(node.rot.X, node.rot.Y, node.rot.Z)) * Matrix4.CreateTranslation(node.pos);
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

                // create polygon objects (nud uses basically 1)
                ColladaPolygons p = new ColladaPolygons();
                ColladaInput inv = new ColladaInput();
                inv.offset = 0;
                inv.semanticType = SemanticType.VERTEX;
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
                    vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.POSITION });
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
                    vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.NORMAL });
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
                    vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.TEXCOORD });
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
                    vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.COLOR });
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
                    skin.joints.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.JOINT });
                    weights.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.JOINT, offset = 0 });
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
                    skin.joints.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.INV_BIND_MATRIX });
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
                    weights.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.WEIGHT, offset = 1 });
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

            if (con.DatMelee != null)
            {
                Save(fname, con.DatMelee);
                return;
            }
            NUD nud = con.NUD;


            // bones

            if(con.VBN != null)
                SaveBoneNodes(dae, con.VBN.bones[0], con.VBN, null);

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
            foreach (NUD.Mesh mesh in nud.Nodes)
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
                        vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.POSITION });
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
                        vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.NORMAL });
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
                        vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.TEXCOORD });
                        List<string> d = new List<string>();
                        foreach (NUD.Vertex v in poly.vertices)
                        {
                            d.AddRange(new string[] { v.uv[0].X.ToString(), v.uv[0].Y.ToString()});
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
                        vertex.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.COLOR });
                        List<string> d = new List<string>();
                        foreach (NUD.Vertex v in poly.vertices)
                        {
                            d.AddRange(new string[] { (v.color.X/128).ToString(), (v.color.Y / 128).ToString(), (v.color.Z / 128).ToString(), (v.color.W / 128).ToString() });
                        }
                        src.accessor.Add("R");
                        src.accessor.Add("G");
                        src.accessor.Add("B");
                        src.accessor.Add("A");
                        src.data = d.ToArray();
                        src.count = d.Count * 4;
                    }

                    // create polygon objects (nud uses basically 1)
                    ColladaPolygons p = new ColladaPolygons();
                    ColladaInput inv = new ColladaInput();
                    inv.offset = 0;
                    inv.semanticType = SemanticType.VERTEX;
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
                        skin.joints.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.JOINT });
                        weights.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.JOINT, offset = 0 });
                        List<string> d = new List<string>();
                        if (con.VBN != null) { 
                            foreach (Bone b in con.VBN.bones)
                            d.Add(b.Text);
                        }
                        else
                        {
                            d.Add("ROOT");
                        }
                        src.accessor.Add("JOINT");
                        src.data = d.ToArray();
                        src.count = d.Count;
                    }
                    // INVTRANSFORM
                    
                    {
                        ColladaSource src = new ColladaSource();
                        skin.sources.Add(src);
                        src.id = control.id + "_trans";
                        skin.joints.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.INV_BIND_MATRIX });
                        List<string> d = new List<string>();
                        if (con.VBN != null)
                        {
                            foreach (Bone b in con.VBN.bones)
                            {
                                d.Add(b.invert.M11 + " " + b.invert.M21 + " " + b.invert.M31 + " " + b.invert.M41 + " "
                                    + b.invert.M12 + " " + b.invert.M22 + " " + b.invert.M32 + " " + b.invert.M42 + " "
                                    + b.invert.M13 + " " + b.invert.M23 + " " + b.invert.M33 + " " + b.invert.M43 + " "
                                    + b.invert.M14 + " " + b.invert.M24 + " " + b.invert.M34 + " " + b.invert.M44);
                            }
                        }
                        else
                        {
                            Bone b = new Bone(new VBN());
                            d.Add(    b.invert.M11 + " " + b.invert.M21 + " " + b.invert.M31 + " " + b.invert.M41 + " "
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
                        weights.inputs.Add(new ColladaInput() { source = "#" + src.id, semanticType = SemanticType.WEIGHT, offset=1 });
                        List<string> d = new List<string>();
                        List<int> vcount = new List<int>();
                        List<int> vert = new List<int>();
                        foreach (NUD.Vertex v in poly.vertices)
                        {
                            int vc = 0;
                            for (int i = 0; i < v.boneIds.Count; i++)
                            {
                                string w = v.boneWeights[i].ToString();
                                if (w.Equals("0")) continue;
                                vc++;
                                if (!d.Contains(w))
                                    d.Add(w);
                                vert.Add(v.boneIds[i]);
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

            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));

            XmlNode colladaNode = doc.CreateElement("COLLADA");
            colladaNode.Attributes.Append(CreateAttribute(doc, "xmlns", "http://www.collada.org/2005/11/COLLADASchema"));
            colladaNode.Attributes.Append(CreateAttribute(doc, "version", "1.4.1"));

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
            vs.Attributes.Append(CreateAttribute(doc, "url", "#VisualSceneNode"));
            scene.AppendChild(vs);
            colladaNode.AppendChild(scene);
            
            doc.AppendChild(colladaNode);
            doc.Save(fname);
        }

        public static XmlAttribute CreateAttribute(XmlDocument doc, string attributeText, string value)
        {
            XmlAttribute attribute = doc.CreateAttribute(attributeText);
            attribute.Value = value;
            return attribute;
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

        public void Read(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
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
                node.Attributes.Append(CreateAttribute(doc, "id", id));
                node.Attributes.Append(CreateAttribute(doc, "name", name));

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
                    if (node.Name.Equals("polylist"))
                    {
                        ColladaPolygons source = new ColladaPolygons();
                        source.type = ColladaPrimitiveType.polylist;
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
            public int stride;
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
                    if (node.Name.Equals("technique_common") && node.ChildNodes.Count > 0 && node.ChildNodes[0].Attributes["stride"] != null)
                    {
                        stride = int.Parse((string)node.ChildNodes[0].Attributes["stride"].Value);
                    }
                }
            }

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("source");

                node.Attributes.Append(CreateAttribute(doc, "id", id));
                //node.Attributes.Append(createAttribute(doc, "count", count));

                XmlNode arr = doc.CreateElement(type + "");
                node.AppendChild(arr);
                arr.Attributes.Append(CreateAttribute(doc, "id", id + "-array"));
                arr.Attributes.Append(CreateAttribute(doc, "count", count + ""));
                string aa = "";
                foreach (string s in data) aa += s + " ";
                arr.InnerText = aa;

                XmlNode tc = doc.CreateElement("technique_common");
                node.AppendChild(tc);
                XmlNode accessor = doc.CreateElement("accessor");
                accessor.Attributes.Append(CreateAttribute(doc, "source", "#" + id + "-array"));
                accessor.Attributes.Append(CreateAttribute(doc, "count", (count / this.accessor.Count) + ""));
                if(this.accessor.Count > 0 && this.accessor[0].Equals("TRANSFORM"))
                    accessor.Attributes.Append(CreateAttribute(doc, "stride", 16 + ""));
                else
                    accessor.Attributes.Append(CreateAttribute(doc, "stride", this.accessor.Count + ""));
                tc.AppendChild(accessor);

                foreach (string param in this.accessor)
                {
                    XmlNode pa = doc.CreateElement("param");
                    accessor.AppendChild(pa);
                    pa.Attributes.Append(CreateAttribute(doc, "name", param));
                    if(param.Equals("TRANSFORM"))
                        pa.Attributes.Append(CreateAttribute(doc, "type", "float4x4"));
                    else
                        pa.Attributes.Append(CreateAttribute(doc, "type", type.ToString().Replace("_array", "")));
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

                node.Attributes.Append(CreateAttribute(doc, "id", id));

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

                node.Attributes.Append(CreateAttribute(doc, "material", materialid));
                node.Attributes.Append(CreateAttribute(doc, "count", count+""));

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
            public SemanticType semanticType;
            public string source;
            public int set = -99;
            public int offset = 0;

            public void Read(XmlNode root)
            {
                semanticType = (SemanticType)Enum.Parse(typeof(SemanticType), (string)root.Attributes["semantic"].Value);
                source = (string)root.Attributes["source"].Value;
                if (root.Attributes["set"] != null)
                    int.TryParse((string)root.Attributes["set"].Value, out set);
                if (root.Attributes["offset"] != null)
                    int.TryParse((string)root.Attributes["offset"].Value, out offset);
            }

            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("input");

                node.Attributes.Append(CreateAttribute(doc, "semantic", semanticType.ToString()));
                node.Attributes.Append(CreateAttribute(doc, "source", source));
                if (set != -99)
                    node.Attributes.Append(CreateAttribute(doc, "set", set + ""));
                if (offset != -99)
                    node.Attributes.Append(CreateAttribute(doc, "offset", offset+""));

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
                node.Attributes.Append(CreateAttribute(doc, "id", id));
                node.Attributes.Append(CreateAttribute(doc, "name", name));

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
                node.Attributes.Append(CreateAttribute(doc, "id", id));
                
                XmlNode init = doc.CreateElement("instance_effect");
                init.Attributes.Append(CreateAttribute(doc, "url", effecturl));
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
                node.Attributes.Append(CreateAttribute(doc, "id", id));
                node.Attributes.Append(CreateAttribute(doc, "name", name));

                XmlNode prof = doc.CreateElement("profile_COMMON");
                node.AppendChild(prof);
                {
                    XmlNode np = doc.CreateElement("newparam");
                    prof.AppendChild(np);
                    np.Attributes.Append(CreateAttribute(doc, "sid", id + "-surface"));
                    
                    XmlNode sur = doc.CreateElement("surface");
                    np.AppendChild(sur);
                    XmlNode init = doc.CreateElement("init_from");
                    sur.AppendChild(init);
                    init.InnerText = source.Replace("#", "");
                    sur.Attributes.Append(CreateAttribute(doc, "type", "2D"));
                }
                {
                    XmlNode np = doc.CreateElement("newparam");
                    prof.AppendChild(np);
                    np.Attributes.Append(CreateAttribute(doc, "sid", id + "-sampler"));
                    sampler.source = id + "-surface";
                    sampler.Write(doc, np);
                }
                {
                    XmlNode tech = doc.CreateElement("technique");
                    prof.AppendChild(tech);
                    tech.Attributes.Append(CreateAttribute(doc, "sid", "COMMON"));

                    XmlNode sur = doc.CreateElement("phong");
                    tech.AppendChild(sur);
                    XmlNode init = doc.CreateElement("diffuse");
                    sur.AppendChild(init);
                    XmlNode reff = doc.CreateElement("texture");
                    init.AppendChild(reff);
                    reff.Attributes.Append(CreateAttribute(doc, "texture", id + "-sampler"));
                    reff.Attributes.Append(CreateAttribute(doc, "texcoord", ""));
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
                node.Attributes.Append(CreateAttribute(doc, "id", id));

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
                node.Attributes.Append(CreateAttribute(doc, "source", source));

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
                node.Attributes.Append(CreateAttribute(doc, "count", vcount.Length.ToString()));

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
                vs.Attributes.Append(CreateAttribute(doc, "id", "VisualSceneNode"));
                vs.Attributes.Append(CreateAttribute(doc, "name", "rdmscene"));
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
            public Vector3 pos = new Vector3();
            public Vector3 scale = new Vector3();
            public Vector3 rot = new Vector3();

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
                        scale = mat.ExtractScale();

                        mat.ClearScale();
                        mat.ClearTranslation();
                        mat.Invert();
                        rot = ANIM.quattoeul(mat.ExtractRotation()); // TODO: We need a better conversion code for this
                        if (float.IsNaN(rot.X)) rot.X = 0;
                        if (float.IsNaN(rot.Y)) rot.Y = 0;
                        if (float.IsNaN(rot.Z)) rot.Z = 0;

                        mat.M11 = float.Parse(data[0]); mat.M12 = float.Parse(data[1]); mat.M13 = float.Parse(data[2]); mat.M14 = float.Parse(data[3]);
                        mat.M21 = float.Parse(data[4]); mat.M22 = float.Parse(data[5]); mat.M23 = float.Parse(data[6]); mat.M24 = float.Parse(data[7]);
                        mat.M31 = float.Parse(data[8]); mat.M32 = float.Parse(data[9]); mat.M33 = float.Parse(data[10]); mat.M34 = float.Parse(data[11]);
                        mat.M41 = float.Parse(data[12]); mat.M42 = float.Parse(data[13]); mat.M43 = float.Parse(data[14]); mat.M44 = float.Parse(data[15]);
                    }
                    else if (node.Name.Equals("extra"))
                    {

                    }
                    else if (node.Name.Equals("instance_controller") || node.Name.Equals("instance_geometry"))
                    {
                        if (node.Name.Equals("instance_geometry"))
                        {
                            geom_id = node.Attributes["url"].Value.Replace("#", "");
                        }
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
                        scale = new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
                    }
                    else if (node.Name.Equals("rotate"))
                    {
                        string[] data = node.InnerText.Trim().Replace("\n", " ").Split(' ');
                        rot = new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
                    }
                }
            }
            
            public void Write(XmlDocument doc, XmlNode parent)
            {
                XmlNode node = doc.CreateElement("node");
                node.Attributes.Append(CreateAttribute(doc, "id", id));
                node.Attributes.Append(CreateAttribute(doc, "name", name));
                node.Attributes.Append(CreateAttribute(doc, "type", type));
                if(type.Equals("JOINT"))
                    node.Attributes.Append(CreateAttribute(doc, "sid", name));

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
                    inst.Attributes.Append(CreateAttribute(doc, "url", geomid));
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
                        im.Attributes.Append(CreateAttribute(doc, "symbol", materialSymbol));
                        im.Attributes.Append(CreateAttribute(doc, "target", materialTarget));

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
