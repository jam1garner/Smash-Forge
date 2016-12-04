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

        public void Save(string fname, DAT mod)
        {
            COLLADA dae = new Smash_Forge.COLLADA();

            List<object> Items = new List<object>();

            library_geometries mesh = new library_geometries();
            Items.Add(mesh);
            List<geometry> Mesh = new List<geometry>();
            foreach (DAT.DOBJ d in mod.dobjs)
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
