using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using static Smash_Forge.DAT.POBJ;
using System.IO;

namespace Smash_Forge
{
    public class DAT
    {
        public const int MaxWeightCount = 4;
        public const int headerSize = 0x20;
        Header header = new Header();
        public string filename;

        public List<TreeNode> tree = new List<TreeNode>();
        public List<TreeNode> displayList = new List<TreeNode>();
        public COLL_DATA collisions = null;
        public List<Point> spawns = null;
        public List<int> spawnOffs = new List<int>();
        public List<Point> respawns = null;
        public List<int> respawnOffs = new List<int>();
        public List<Point> itemSpawns = null;
        public List<int> itemSpawnOffs = new List<int>();
        public List<Point> targets = null;
        public List<int> targetOffs = new List<int>();
        public Bounds cameraBounds = null;
        public int[] cameraBoundOffs = new int[2];
        public Bounds blastzones = null;
        public int[] blastzoneOffs = new int[2];

        public Map_Head.Head_Node headNode = null;

        public float stageScale = 1; 

        public VBN bones = new VBN();

        // gl buffer objects
        int ubo_bones;
        int vbo_position;
        int vbo_color;
        int vbo_nrm;
        int vbo_uv;
        int vbo_weight;
        int vbo_bone;
        int ibo_elements;
        public int testtex;

        public static Shader shader = null;

        Dictionary<int, JOBJ> jobjOffsetLinker = new Dictionary<int, JOBJ>();
        public Dictionary<int, Bitmap> texturesLinker = new Dictionary<int, Bitmap>();
        public Dictionary<int, object[]> tobjLinker = new Dictionary<int, object[]>();

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

        public DAT()
        {
            GL.GenBuffers(1, out ubo_bones);
            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out vbo_color);
            GL.GenBuffers(1, out vbo_nrm);
            GL.GenBuffers(1, out vbo_uv);
            GL.GenBuffers(1, out vbo_bone);
            GL.GenBuffers(1, out vbo_weight);
            GL.GenBuffers(1, out ibo_elements);
        }

        ~DAT()
        {
            /*GL.DeleteBuffer(vbo_position);
            GL.DeleteBuffer(vbo_color);
            GL.DeleteBuffer(vbo_nrm);
            GL.DeleteBuffer(vbo_uv);
            GL.DeleteBuffer(vbo_weight);
            GL.DeleteBuffer(vbo_bone);*/
        }

        public void Read(FileData d)
        {
            d.Endian = System.IO.Endianness.Big;
            d.seek(0);

            header.Read(d);

            int dataBlockOffset = d.pos();
            d.skip(header.dataBlockSize); // skip to relocation table

            int relocationTableOffset = d.pos();

            // update relocation table and data offset
            for (int i = 0; i < header.relocationTableCount; ++i)
            {
                int relocationOffset = relocationTableOffset + i * 4;

                d.seek(relocationOffset);

                int dataOffset = d.readInt() + headerSize;

                d.writeInt(relocationOffset, dataOffset);

                d.seek(dataOffset);

                d.writeInt(dataOffset, d.readInt() + headerSize);
            }

            d.seek(relocationTableOffset + header.relocationTableCount * 4); // skip relocation table
            
            int strOffset = d.pos() + header.rootCount * 8 + header.referenceNodeCount * 8;
            int[] sectionOffset = new int[header.rootCount];
            string[] sectionNames = new string[header.rootCount];
            Console.WriteLine(d.pos().ToString("x") + " " + strOffset.ToString("x"));
            for (int i = 0; i < header.rootCount; i++)
            {
                // data then string
                int data = d.readInt() + headerSize;
                string s = d.readString(d.readInt() + strOffset, -1);
                sectionOffset[i] = data;
                sectionNames[i] = s;
                Console.WriteLine(s + " " + data.ToString("x"));

                TreeNode node = new TreeNode();
                node.Text = s;
                node.Tag = data;
                tree.Add(node);
            }
            Console.WriteLine(d.pos().ToString("x") + " " + strOffset.ToString("x"));

            foreach (TreeNode node in tree)
            {
                // then a file system is read... it works like a tree?
                d.seek((int)node.Tag);
                // now, the name determines what happens here
                // for now, it just assumes the _joint
                if (node.Text.EndsWith("_joint") && !node.Text.Contains("matanim") && !node.Text.Contains("anim_joint"))
                {
                    JOBJ j = new JOBJ();
                    j.Read(d, this, node);
                    //break;
                }
                else if (node.Text.EndsWith("grGroundParam"))
                {
                    stageScale = d.readFloat();
                    Console.WriteLine($"Stage scale - {stageScale}");
                }
            }

            foreach(TreeNode node in tree)
            {
                d.seek((int)node.Tag);
                if (node.Text.EndsWith("map_head"))
                {
                    Map_Head head = new Map_Head();
                    head.Read(d, this, node);
                }
            }

            Console.WriteLine("Done");
            //ExportTextures("",0);

            // now to fix single binds
            List<JOBJ> boneTrack = GetBoneOrder();
            Matrix4 mt = new Matrix4();
            int w = 0;
            foreach (Vertex v in vertBank)
            {
                w = 0;
                v.bones.Clear();
                mt = new Matrix4();

                foreach (object o in v.Tags)
                {
                    if(o is JOBJ)
                    {
                        v.bones.Add(boneTrack.IndexOf((JOBJ)o));
                        mt = Matrix4.CreateScale(1,1,1);
                        v.nrm = TransformNormal(((JOBJ)o).transform, v.nrm);
                    }
                    else
                    if(o is int)
                    {
                        v.bones.Add(boneTrack.IndexOf(jobjOffsetLinker[(int)o]));
                        mt += jobjOffsetLinker[(int)o].transform * v.weights[w++];
                    }
                }

                if (v.bones.Count == 1)
                {
                    v.pos = Vector3.Transform(v.pos, mt);
                    v.nrm = TransformNormal(mt, v.nrm);
                }
                // scale it
                v.pos = Vector3.Multiply(v.pos, stageScale);
            }

            foreach (TreeNode node in tree)
            {
                if (node.Text.EndsWith("coll_data"))
                {
                    d.seek((int)node.Tag);
                    collisions = new COLL_DATA();
                    collisions.Read(d);
                }
            }
        }

        Dictionary<int, PrimitiveType> primitiveTypes = new Dictionary<int, PrimitiveType>()
        {
            { 0xB8, PrimitiveType.Points},
            { 0xA8, PrimitiveType.Lines},
            { 0xB0, PrimitiveType.LineStrip},
            { 0x90, PrimitiveType.Triangles},
            { 0x98, PrimitiveType.TriangleStrip},
            { 0xA0, PrimitiveType.TriangleFan},
            { 0x80, PrimitiveType.Quads},
        };

        Dictionary<int, TextureWrapMode> GX_TEXTUREWRAP = new Dictionary<int, TextureWrapMode>()
        {
            { 0, TextureWrapMode.Clamp},
            { 1, TextureWrapMode.Repeat},
            { 2, TextureWrapMode.MirroredRepeat}
        };


        #region Rendering
        Vector3[] vertdata, nrmdata;
        Vector2[] uvdata;
        Vector4[] bonedata, coldata, weightdata;
        int[] facedata;

        public void PreRender()
        {
            List<Vector3> vert = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector4> col = new List<Vector4>();
            List<Vector3> nrm = new List<Vector3>();
            List<Vector4> bone = new List<Vector4>();
            List<Vector4> weight = new List<Vector4>();
            List<int> face = new List<int>();

            foreach (Vertex v in vertBank)
            {
                vert.Add(v.pos);
                uv.Add(v.tx0);
                col.Add(v.clr);
                nrm.Add(v.nrm);

                if(v.bones.Count == 0)
                {
                    v.bones.Add (-1);
                    v.weights.Add(0);
                }
                while (v.bones.Count < MaxWeightCount)
                {
                    v.bones.Add(0);
                    v.weights.Add(0);
                }

                bone.Add(new Vector4(v.bones[0], v.bones[1], v.bones[2], v.bones[3]));
                weight.Add(new Vector4(v.weights[0], v.weights[1], v.weights[2], v.weights[3]));
            }

            Stack<TreeNode> queue = new Stack<TreeNode>();
            //foreach (TreeNode node in tree)
            for(int i = tree.Count - 1; i >= 0; i--)
                queue.Push(tree[i]);

            displayList.Clear();
            List<JOBJ> boneTrack = new List<JOBJ>();
            while (queue.Any())
            {
                TreeNode node = queue.Pop();

                //foreach (TreeNode n in node.Nodes)
                //    queue.Enqueue(n);

                for (int i = node.Nodes.Count - 1; i >= 0; i--)
                    queue.Push(node.Nodes[i]);
                if (node.Tag is DOBJ)
                    displayList.Add(node);

                if (!(node.Tag is JOBJ))
                    continue;

                JOBJ j = (JOBJ)node.Tag;
                boneTrack.Add(j);

                Bone b = new Bone(bones);
                b.Text = ("Bone_" + boneTrack.IndexOf(j).ToString());
                if (node.Parent.Tag is JOBJ)
                    b.parentIndex = boneTrack.IndexOf((JOBJ)node.Parent.Tag);
                else
                    b.parentIndex = -1;
                //Console.WriteLine("Where is this? " + b.parentIndex);

                //b.children = new List<int>();
                b.scale = new float[3];
                b.rotation = new float[3];
                b.position = new float[3];
                b.position[0] = j.pos.X * stageScale;
                b.position[1] = j.pos.Y * stageScale;
                b.position[2] = j.pos.Z * stageScale;
                b.scale[0] = j.sca.X;
                b.scale[1] = j.sca.Y;
                b.scale[2] = j.sca.Z;
                b.rotation[0] = j.rot.X;
                b.rotation[1] = j.rot.Y;
                b.rotation[2] = j.rot.Z;
                //if (b.parentIndex != -1)
                //    bones.bones[b.parentIndex].children.Add(boneTrack.IndexOf(j));
                bones.bones.Add(b);
            }
            bones.reset();
            bones.update();

            foreach (var da in displayList)
            {
                DOBJ data = (DOBJ)da.Tag;
                foreach (POBJ poly in data.polygons)
                {
                    foreach (POBJ.DisplayObject d in poly.display)
                    {
                        // I wanna triangulate
                        /*if(d.type != 0x90)
                        {
                            Console.WriteLine("Triangulate " + primitiesTypes[d.type]);
                        }
                        if(d.type == 0x98)
                        {
                            d.faces = TriangleTools.fromTriangleStrip(d.faces);
                            d.type = 0x90;
                        }
                        if (d.type == 0x80)
                        {
                            d.faces = TriangleTools.fromQuad(d.faces);
                            d.type = 0x90;
                        }*/
                        face.AddRange(d.faces);
                    }
                }
            }

            vertdata = vert.ToArray();
            coldata = col.ToArray();
            nrmdata = nrm.ToArray();
            uvdata = uv.ToArray();
            bonedata = bone.ToArray();
            weightdata = weight.ToArray();

            facedata = face.ToArray();

            SetupShader();
        }

        private void SetupShader()
        {
            int maxUniformBlockSize = GL.GetInteger(GetPName.MaxUniformBlockSize);
            int boneCount = bones.bones.Count;
            int dataSize = boneCount * Vector4.SizeInBytes * 4;

            if (shader == null)
            {
                shader = new Shader();

                shader.vertexShader(string.Format(vs, boneCount));
                shader.fragmentShader(string.Format(fs));
            }

            GL.BindBuffer(BufferTarget.UniformBuffer, ubo_bones);
            GL.BufferData(BufferTarget.UniformBuffer, (IntPtr)(dataSize), IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            var blockIndex = GL.GetUniformBlockIndex(shader.programID, "bones");
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, blockIndex, ubo_bones);
        }

        public void Render(Matrix4 modelview)
        {
            if (null == shader)
                return;

            GL.UseProgram(shader.programID);

            GL.UniformMatrix4(shader.getAttribute("modelview"), false, ref modelview);

            if (bones != null)
            {
                Matrix4[] f = bones.getShaderMatrix();

                if(f.Length > 0)
                {
                    GL.BindBuffer(BufferTarget.UniformBuffer, ubo_bones);
                    GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, (IntPtr)(f.Length * Vector4.SizeInBytes * 4), f);
                }
            }

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

            foreach (var da in displayList)
            {
                DOBJ data = da.Tag as DOBJ;
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, data.material.texture.texid);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GX_TEXTUREWRAP[data.material.texture.wrap_s]);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GX_TEXTUREWRAP[data.material.texture.wrap_t]);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.Uniform1(shader.getAttribute("tex"), 0);

                GL.Uniform2(shader.getAttribute("uvscale"), new Vector2(data.material.texture.scale_w, data.material.texture.scale_h));

                foreach (POBJ poly in data.polygons)
                {
                    foreach (POBJ.DisplayObject d in poly.display)
                    {
                        if (da.Checked)
                            GL.DrawElements(primitiveTypes[d.type], d.faces.Count, DrawElementsType.UnsignedInt, indiceat * sizeof(int));
                        indiceat += d.faces.Count;
                    }
                }
            }

            shader.disableAttrib();

        }

        #endregion


        public List<JOBJ> GetBoneOrder()
        {
            Stack<TreeNode> queue = new Stack<TreeNode>();
            for (int k = tree.Count - 1; k >= 0; k--)
                queue.Push(tree[k]);

            List<JOBJ> boneTrack = new List<JOBJ>();
            while (queue.Any())
            {
                TreeNode node = queue.Pop();

                for (int k = node.Nodes.Count - 1; k >= 0; k--)
                    queue.Push(node.Nodes[k]);

                if (!(node.Tag is JOBJ))
                    continue;

                boneTrack.Add((JOBJ)node.Tag);
            }
            return boneTrack;
        }
        
        public static Vector3 TransformNormal(Matrix4 M, Vector3 N)
        {
            return new Vector3((M.M11 * N.X) + (M.M12 * N.Y) + (M.M13 * N.Z),
            (M.M21 * N.X) + (M.M22 * N.Y) + (M.M23 * N.Z),
            (M.M31 * N.X) + (M.M32 * N.Y) + (M.M33 * N.Z));
        }

        #region Converting
        public static byte[] ConvertBitmapToByteArray(Bitmap bitmap)
        {
            byte[] result = null;
            if (bitmap != null)
            {
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                result = new FileData(stream.ToArray()).getSection(0x36, (int)stream.Length - 0x36);
            }
            return result;
        }

        public void ExportTextures(string path, int key)
        {
            int index = 0;
            foreach (int k in texturesLinker.Keys)
            {
                texturesLinker[k].Save(path + (key+index++).ToString("x") + ".png");
            }
        }

        private static bool inRange(int[] range, int val)
        {
            return (range[0] <= val && range[1] >= val);
        }

        private static bool vertExists(Collision c, Vector2D v)
        {
            foreach (Vector2D v2 in c.verts)
                if (v2.x == v.x && v2.y == v.y)
                    return true;
            return false;
        }

        private void addLink(COLL_DATA.Link link, Collision c)
        {
            int l1 = link.vertexIndices[0], l2 = link.vertexIndices[1];
            if (!vertExists(c, collisions.vertices[l1]))
                c.verts.Add(collisions.vertices[l1]);
            if (!vertExists(c, collisions.vertices[l2]))
                c.verts.Add(collisions.vertices[l2]);

            Vector2D currentNormal = new Vector2D();
            if ((link.collisionAngle & 1) != 0)
                currentNormal.y = 1;
            if ((link.collisionAngle & 2) != 0)
                currentNormal.y = -1;
            if ((link.collisionAngle & 4) != 0)
                currentNormal.x = 1;
            if ((link.collisionAngle & 8) != 0)
                currentNormal.x = -1;
            c.normals.Add(currentNormal);

            if ((link.flags & 1) != 0)
                c.flag4 = true;
            CollisionMat currentMat = new CollisionMat();
            if ((link.flags & 2) != 0)
            {
                currentMat.setFlag(6, true);
                currentMat.setFlag(7, true);
            }
            currentMat.setPhysics(link.material);
            c.materials.Add(currentMat);
        }

        private void recursivelyGrabPoints(int linkIndex, Collision c)
        {
            COLL_DATA.Link link = collisions.links[linkIndex];
            addLink(link, c);
            if (link.connectors[1] != 0xFFFF)
                recursivelyGrabPoints(link.connectors[1], c);
        }

        public LVD toLVD(bool safemode)
        {
            foreach (Vector2D v in collisions.vertices)
            {
                v.x *= stageScale;
                v.y *= stageScale;
            }
            LVD lvd = new LVD();
            int j = 0;
            if (safemode)
            {
                foreach (COLL_DATA.Link link in collisions.links)
                {
                    Collision c = new Collision() { name = $"COL_MELEE_{j}", subname = $"MELEE_{j++}" };
                    addLink(link, c);
                    lvd.collisions.Add(c);
                }
            }
            else
            {
                foreach (int[] poly in collisions.polyRanges)
                {
                    Collision c = new Collision() { name = $"COL_MELEE_{j}", subname = $"MELEE_{j++}" };
                    int k = 0;
                    foreach (COLL_DATA.Link link in collisions.links)
                    {
                        if (inRange(poly, link.vertexIndices[0]) && link.connectors[0] == 0xFFFF)
                        {
                            recursivelyGrabPoints(k, c);
                        }
                        k++;
                    }
                    lvd.collisions.Add(c);
                }
            }

            lvd.blastzones.Add(blastzones);
            lvd.cameraBounds.Add(cameraBounds);
            j = 0;
            foreach (Point s in spawns)
                lvd.spawns.Add(new Point() { x = s.x, y = s.y, name = $"Spawn_{j}", subname = $"{j++}" });
            j = 0;
            foreach (Point s in respawns)
                lvd.respawns.Add(new Point() { x = s.x, y = s.y, name = $"Respawn_{j}", subname = $"{j++}" });
            j = 0;
            foreach (Point p in itemSpawns)
                lvd.generalPoints.Add(new Point() { x = p.x, y = p.y, name = $"Item_{j}", subname = $"{j++}" });

            /*//This is basically for DSX8 for him quickly converting items in the "dumb" way
            foreach (Vector3 p in itemSpawns)
            {
                ItemSpawner item = new ItemSpawner() { name = $"Item_{j}", subname = $"{j++}" };
                item.sections.Add(new Section());
                Vector2D[] f = { new Vector2D() { x = p.X, y = p.Y }, new Vector2D() { x = p.X, y = p.Y } };
                item.sections[0].points = new List<Vector2D>(f);
                lvd.items.Add(item);
            }
            */

            return lvd;
        }

        public ModelContainer wrapToNUD()
        {
            ModelContainer con = new ModelContainer();
            con.vbn = bones;

            NUD nud = new NUD();
            con.nud = nud;

            // create a nut?
            NUT nut = new NUT();
            Runtime.TextureContainers.Add(nut);
            int texid = 0;
            foreach(int key in texturesLinker.Keys)
            {
                NUT.NUD_Texture tex = new NUT.NUD_Texture();
                tex.width = texturesLinker[key].Width;
                tex.height = texturesLinker[key].Height;
                tex.id = 0x401B1000 + texid;
                tex.mipmaps = new List<byte[]>();
                byte[] mip1 = ConvertBitmapToByteArray(texturesLinker[key]);
                Console.WriteLine(mip1.Length);
                tex.mipmaps.Add(mip1);
                tex.type = PixelInternalFormat.Rgba;
                tex.utype = PixelFormat.Bgra;
                nut.textures.Add(tex);
                nut.draw.Add(0x40545400 + texid, NUT.loadImage(tex));
                texid++;
            }

            foreach (var da in displayList)
            {
                DOBJ data = (DOBJ)da.Tag;
                NUD.Mesh mesh = new NUD.Mesh();
                mesh.Text = "Mesh_" + displayList.IndexOf(da);
                NUD.Polygon polygon = new NUD.Polygon();
                polygon.setDefaultMaterial();


                texid = 0;
                foreach (int key in texturesLinker.Keys)
                {
                    if (key == data.material.texture.imageDataOffset)
                        break;

                    texid++;
                }
                polygon.materials[0].textures[0].hash = 0x401B1000 + texid;
                switch (data.material.texture.wrap_s)
                {
                    case 0: polygon.materials[0].textures[0].WrapMode1 = 3; break;
                    case 1: polygon.materials[0].textures[0].WrapMode1 = 1; break;
                    case 2: polygon.materials[0].textures[0].WrapMode1 = 2; break;
                }
                switch (data.material.texture.wrap_t)
                {
                    case 0: polygon.materials[0].textures[0].WrapMode2 = 3; break;
                    case 1: polygon.materials[0].textures[0].WrapMode2 = 1; break;
                    case 2: polygon.materials[0].textures[0].WrapMode2 = 2; break;
                }


                List<Vertex> usedVertices = new List<Vertex>();
                foreach (POBJ poly in data.polygons)
                {
                    foreach (POBJ.DisplayObject d in poly.display)
                    {
                        Console.WriteLine("Mesh Type" + primitiveTypes[d.type] + " " + mesh.Text);
                        List<int> faces = new List<int>();// d.faces;
                        if (d.type == 0x98)faces = TriangleTools.fromTriangleStrip(d.faces);else
                        if (d.type == 0x80)
                            faces = TriangleTools.fromQuad(d.faces);

                        foreach (int index in faces)
                        {
                            if (!usedVertices.Contains(vertBank[index]))
                                usedVertices.Add(vertBank[index]);
                            polygon.faces.Add(usedVertices.IndexOf(vertBank[index]));
                        }
                    }
                }

                if (usedVertices.Count == 0)
                    continue;

                nud.mesh.Add(mesh);

                foreach (Vertex vert in usedVertices)
                {
                    // convert to nud vert
                    NUD.Vertex nv = new NUD.Vertex();
                    nv.pos = vert.pos;
                    nv.tx.Add(new Vector2(vert.tx0.X * data.material.texture.scale_w, (vert.tx0.Y * data.material.texture.scale_h)));
                    nv.nrm = vert.nrm;
                    nv.col = (vert.clr*0xFF)/2;
                    nv.node.AddRange(vert.bones);
                    nv.weight.AddRange(vert.weights);
                    polygon.AddVertex(nv);
                }

                mesh.Nodes.Add(polygon);
            }

            nud.PreRender();


            return con;
        }
        #endregion

        /*
            Header
            Data Block
            Relocation Table
            Root Nodes (2)
            String Table
         */

        private class Header
        {
            public int fileSize = 0;
            public int dataBlockSize = 0;
            public int relocationTableCount = 0;
            public int rootCount = 0;
            public int referenceNodeCount = 0;
            public int unk1 = 0;
            public int unk2 = 0;
            public int unk3 = 0;

            public void Read(FileData d)
            {
                fileSize = d.readInt();
                dataBlockSize = d.readInt();
                relocationTableCount = d.readInt();
                rootCount = d.readInt();
                referenceNodeCount = d.readInt();
                unk1 = d.readInt();
                unk2 = d.readInt();
                unk3 = d.readInt();
            }
        }

        #region Collisions

        public class COLL_DATA : LVDEntry
        {
            public class Link
            {
                public int[] vertexIndices;
                public int[] connectors;
                public int collisionAngle;
                public int idxVertFromLink = 0xFFFF;
                public int idxVertToLink = 0xFFFF;
                public byte flags;
                public byte material;
            }

            public class AreaTableEntry
            {
                public ushort idxFirstTopLink;
                public ushort nbTopLinks;
                public ushort idxFirstBotLink;
                public ushort nbBotLinks;
                public ushort idxFirstRightLink;
                public ushort nbRightLinks;
                public ushort idxFirstLeftLink;
                public ushort nbLeftLinks;
                //0 (spacing)
                public float xBotLeftCorner;
                public float yBotLeftCorner;
                public float xTopRightCorner;
                public float yTopRightCorner;
                public ushort idxLowestSpot;
                public ushort nbLinks;
            }

            public List<Vector2D> vertices = new List<Vector2D>();
            public List<List<Vector2D>> polys = new List<List<Vector2D>>();
            public List<AreaTableEntry> areaTable = new List<AreaTableEntry>();
            public List<Link> links = new List<Link>();
            public List<int[]> polyRanges = new List<int[]>();

            public int vertOffOff;
            public int linkOffOff;
            public int polyOffOff; 

            public COLL_DATA()
            {
                name = "Collisions";
                subname = "00";
            }

            public void Read(FileData f)
            {
                vertOffOff = f.pos();
                int vertOff = f.readInt();
                int vertCount = f.readInt();
                linkOffOff = f.pos();
                int linkOff = f.readInt();
                int linkCount = f.readInt();
                f.skip(0x14);
                polyOffOff = f.pos();
                int polyOff = f.readInt();
                int polyCount = f.readInt();
                int returnOffset = f.pos();
                f.seek(vertOff);
                for (int i = 0; i < vertCount; i++)
                {
                    Vector2D point = new Vector2D();
                    point.x = f.readFloat();
                    point.y = f.readFloat();
                    vertices.Add(point);
                }
                f.seek(polyOff);
                for (int i = 0; i < polyCount; i++)
                {
                    AreaTableEntry entry = new AreaTableEntry();
                    entry.idxFirstTopLink = (ushort)f.readShort();
                    entry.nbTopLinks = (ushort)f.readShort();
                    entry.idxFirstBotLink = (ushort)f.readShort();
                    entry.nbBotLinks = (ushort)f.readShort();
                    entry.idxFirstRightLink = (ushort)f.readShort();
                    entry.nbRightLinks = (ushort)f.readShort();
                    entry.idxFirstLeftLink = (ushort)f.readShort();
                    entry.nbLeftLinks = (ushort)f.readShort();
                    f.skip(4);
                    entry.xBotLeftCorner = f.readFloat();
                    entry.yBotLeftCorner = f.readFloat();
                    entry.xTopRightCorner = f.readFloat();
                    entry.yTopRightCorner = f.readFloat();
                    entry.idxLowestSpot = (ushort)f.readShort();
                    entry.nbLinks = (ushort)f.readShort();
                    areaTable.Add(entry);
                    int[] range = { entry.idxLowestSpot, entry.idxLowestSpot + entry.nbLinks };
                    polyRanges.Add(range);
                    polys.Add(vertices.GetRange(entry.idxLowestSpot, entry.nbLinks));
                }
                f.seek(linkOff);
                for (int i = 0; i < linkCount; i++)
                {
                    Link link = new Link();
                    link.vertexIndices = new int[] { f.readShort(), f.readShort() };
                    link.connectors = new int[] { f.readShort(), f.readShort() };
                    link.idxVertFromLink = f.readShort();
                    link.idxVertToLink = f.readShort();
                    link.collisionAngle = f.readShort();
                    link.flags = (byte)f.readByte();
                    link.material = (byte)f.readByte();
                    links.Add(link);
                }
                /*foreach (List<Vector2D> poly in polys)
                {
                    inc++;
                    Console.WriteLine("\nPoly" + inc);
                    foreach (Vector2D point in poly)
                        Console.WriteLine("(" + point.x + ", " + point.y + ")");
                }*/
            }
        }

        #endregion

        public class Map_Head
        {
            public List<Head_Node> nodes;

            public class Head_Node
            {
                public TreeNode node = new TreeNode();

                public int id, type;

                public void Read(FileData d, DAT dat, TreeNode parentNode)
                {
                    parentNode.Nodes.Add(node);

                    node.Text = "Object_0x" + type.ToString("x");

                    for (int i = 0; i < id; i++)
                    {
                        Head_Node_Object hno = new Head_Node_Object();
                        hno.Read(d, dat, node);
                    }
                }

                public class Head_Node_Object
                {
                    TreeNode node = new TreeNode();

                    public void Read(FileData d, DAT dat, TreeNode parentNode)
                    {
                        parentNode.Nodes.Add(node);
                        node.Tag = this;
                        node.Text = "NodeObject";
                        
                        // WTF ARE THESE OFFSETS OMG
                        int jPointer = d.readInt();
                        Console.WriteLine((d.readInt()).ToString("x") + " " + 
                            (d.readInt()).ToString("x") + " " +
                            (d.readInt()).ToString("x") + " " +
                            (d.readInt()).ToString("x") + " " +
                            (d.readInt()).ToString("x") + " " +
                            (d.readInt()).ToString("x") + " " +
                            (d.readInt()).ToString("x") + " " +

                            (d.readInt()).ToString("x") + " " +
                            (d.readInt()).ToString("x") + " " +
                            (d.readInt()).ToString("x") + " " +
                            (d.readInt()).ToString("x") + " " +
                            (d.readInt()).ToString("x") + " ");

                        if (jPointer == 0 || jPointer == -1)
                            return;

                        int temp = d.pos();
                        if (!dat.jobjOffsetLinker.ContainsKey(jPointer))
                        {
							d.seek(jPointer);
                            JOBJ j = new JOBJ();
                            j.Read(d, dat, node);
                        }

                        d.seek(temp);
                    }
                }
            }

            public void Read(FileData d, DAT dat, TreeNode parentNode)
            {
                int start = d.pos();
                Console.WriteLine($"Map_Head Start {start}");
                int spawnyOffset = d.readInt();
                int spawnyCount = d.readInt(); // ?? jammy-senpai assist ploaj-chan pls

                int mappymodelOffset = d.readInt();
                int mappymodelCount = d.readInt();

                // and some other nonsense
                Console.WriteLine($"spawnyOffset = {spawnyOffset}");
                d.seek(spawnyOffset);
                int stageBonesRoot = d.readInt();
                int boneIdTableOffset = d.readInt();
                int idEntryCount = d.readInt();

                Console.WriteLine($"boneIdTableOffset = {boneIdTableOffset}");
                d.seek(boneIdTableOffset);
                Dictionary<short, short> boneIds = new Dictionary<short, short>();
                for (int i = 0; i < idEntryCount; i++)
                    boneIds.Add((short)(d.readShort() - 1), (short)d.readShort());

                Console.WriteLine($"stageBonesRoot = {stageBonesRoot}");
                d.seek(stageBonesRoot);
                //PLOAJ WORK YOUR MAGIC HERE <3
                // you got it fam
                JOBJ j = new JOBJ();
                j.Read(d, dat, parentNode);

                // models I guess gosh
                d.seek(mappymodelOffset);
                Head_Node node = new Head_Node();
                node.id = mappymodelCount;
                node.Read(d, dat, parentNode);
                dat.headNode = node;

                short index = 0;
                dat.cameraBounds = new Bounds() { name = "Camera Bounds", subname = "00" };
                dat.blastzones = new Bounds() { name = "Blastzones", subname = "00" };
                dat.itemSpawns = new List<Point>();
                dat.spawns = new List<Point>();
                dat.respawns = new List<Point>();
                dat.targets = new List<Point>();

                List<JOBJ> testJobjs = new List<JOBJ>();
                foreach (TreeNode t in j.node.Nodes)
                    if (t.Tag is JOBJ)
                        testJobjs.Add((JOBJ)t.Tag);

                foreach (TreeNode t in j.node.Nodes)
                {
                    if (!(t.Tag is JOBJ))
                        continue;
                    Vector3 point = Vector3.Multiply(((JOBJ)t.Tag).pos, dat.stageScale);
                    Point pos = new Point() { name = "", subname = "", x = point.X, y = point.Y };
                    int type = -1;
                    try
                    {
                        type = boneIds[index];

                        if (type >= 0 && type <= 3)
                        {
                            dat.spawns.Add(pos);
                            dat.spawnOffs.Add(((JOBJ)t.Tag).posOff);
                        }
                        else if (type >= 4 && type <= 7)
                        {
                            dat.respawns.Add(pos);
                            dat.respawnOffs.Add(((JOBJ)t.Tag).posOff);
                        }
                        else if (0x7F <= type && type <= 0x93)
                        {
                            dat.itemSpawns.Add(pos);
                            dat.itemSpawnOffs.Add(((JOBJ)t.Tag).posOff);
                        }
                        else if (type == 0x95)
                        {
                            dat.cameraBounds.left += pos.x;
                            dat.cameraBounds.top += pos.y;
                            dat.cameraBoundOffs[0] = ((JOBJ)t.Tag).posOff;
                            //Console.WriteLine($"Cam0 - {pos}");
                        }
                        else if (type == 0x96)
                        {
                            dat.cameraBounds.right += pos.x;
                            dat.cameraBounds.bottom += pos.y;
                            dat.cameraBoundOffs[1] = ((JOBJ)t.Tag).posOff;
                            //Console.WriteLine($"Cam1 - {pos}");
                        }
                        else if (type == 0x97)
                        {
                            dat.blastzones.left += pos.x;
                            dat.blastzones.top += pos.y;
                            //Console.WriteLine($"Death0 - {pos}");
                            dat.blastzoneOffs[0] = ((JOBJ)t.Tag).posOff;
                        }
                        else if (type == 0x98)
                        {
                            dat.blastzones.right += pos.x;
                            dat.blastzones.bottom += pos.y;
                            //Console.WriteLine($"Death1 - {pos}");
                            dat.blastzoneOffs[1] = ((JOBJ)t.Tag).posOff;
                        }
                        else if (0xC7 <= type && type <= 0xD0)
                        {
                            Console.WriteLine($"Target at {pos.x},{pos.y}");
                            dat.targets.Add(new Point() { x = pos.x, y = pos.y });
                            dat.targetOffs.Add(((JOBJ)t.Tag).posOff);
                        }
                        else
                        {
                            Console.WriteLine($"Type {type}");
                        }

                    }
                    catch (KeyNotFoundException)
                    {
                        Console.WriteLine("No key for node found");
                    }
                    index++;
                }
            }
        }

        public class JOBJ
        {
            public TreeNode node = new TreeNode();

            public int unk1 = 0; // padding?
            public int flags = 0x1004008E;
            public int childOffset = 0;
            public int nextOffset = 0;

            public int dobjOffset = 0;
            public int inverseTransformOffset = 0;
            public Vector3 rot = new Vector3(), sca = new Vector3(), pos = new Vector3();
            public Matrix4 inverseTransform;
            public int unk2; //?? padding again??
            public int posOff;

            public Matrix4 transform = new Matrix4();

            public JOBJ()
            {

            }

            public void Read(FileData d, DAT dat, TreeNode parentNode)
            {
                if (dat.jobjOffsetLinker.ContainsKey(d.pos()))
                    return;
                dat.jobjOffsetLinker.Add(d.pos(), this);
                unk1 = d.readInt();
                flags = d.readInt();
                childOffset = d.readInt();
                nextOffset = d.readInt();
                dobjOffset = d.readInt();
                rot.X = d.readFloat();
                rot.Y = d.readFloat();
                rot.Z = d.readFloat();
                sca.X = d.readFloat();
                sca.Y = d.readFloat();
                sca.Z = d.readFloat();
                posOff = d.pos();
                pos.X = d.readFloat();
                pos.Y = d.readFloat();
                pos.Z = d.readFloat();
                inverseTransformOffset = d.readInt();
                d.skip(4); // offset?

                transform = Matrix4.CreateScale(sca)
                                    * Matrix4.CreateFromQuaternion(VBN.FromEulerAngles(rot.Z, rot.Y, rot.X))
                                    * Matrix4.CreateTranslation(pos) * (parentNode.Tag is JOBJ ? ((JOBJ)parentNode.Tag).transform : Matrix4.CreateScale(1, 1, 1));

                if (inverseTransformOffset != 0)
                {
                    d.seek(inverseTransformOffset);

                    inverseTransform.M11 = d.readFloat();
                    inverseTransform.M12 = d.readFloat();
                    inverseTransform.M13 = d.readFloat();
                    inverseTransform.M14 = d.readFloat();
                    inverseTransform.M21 = d.readFloat();
                    inverseTransform.M22 = d.readFloat();
                    inverseTransform.M23 = d.readFloat();
                    inverseTransform.M24 = d.readFloat();
                    inverseTransform.M31 = d.readFloat();
                    inverseTransform.M32 = d.readFloat();
                    inverseTransform.M33 = d.readFloat();
                    inverseTransform.M34 = d.readFloat();
                    inverseTransform.M44 = 1;

                    inverseTransform.Transpose();
                }
                else
                {
                    inverseTransform = transform.Inverted();
                }

                //dat.jobjs.Add(this);
                node.Text = "Bone_" + dat.jobjOffsetLinker.Count;
                node.Tag = this;
                parentNode.Nodes.Add(node);

                if (nextOffset != 0)
                {
                    d.seek(nextOffset);
                    JOBJ j = new JOBJ();
                    j.Read(d, dat, parentNode);
                }

                if (childOffset != 0)
                {
                    if ((flags & 0x1000) == 0)
                    {
                        d.seek(childOffset);
                        JOBJ j = new JOBJ();
                        j.Read(d, dat, node);
                    }
                }

                if (dobjOffset != 0)
                {
                    if ((flags & 0x4000) != 0)
                    {

                    }
                    else if ((flags & 0x20) != 0)
                    {

                    }
                    else
                    {
                        //Console.WriteLine("DOBJ" + dobjOffset.ToString("X"));
                        d.seek(dobjOffset);
                        DOBJ dobj = new DOBJ();
                        dobj.Read(d, dat, node);
                    }
                }
            }
        }

        // DATA (Contains mesh and material)
        public class DOBJ
        {
            TreeNode node = new TreeNode();
            public int unk1 = 0; // padding?
            public int nextOffset = 0;
            public int mobjOffset = 0;
            public int pobjOffset = 0;

            public List<POBJ> polygons = new List<POBJ>();
            public MOBJ material = new MOBJ();

            public void Read(FileData d, DAT dat, TreeNode parent)
            {
                //if (dat.dobjs.Count > 0) return;
                //dat.dobjs.Add(this);

                node.Text = "Mesh_" + d.pos().ToString("X8");
                node.Tag = this;
                node.Checked = true;
                parent.Nodes.Add(node);

                unk1 = d.readInt();
                nextOffset = d.readInt();
                mobjOffset = d.readInt();
                pobjOffset = d.readInt();

                d.seek(pobjOffset);
                //Console.WriteLine("POBJ" + pobjOffset.ToString("x"));
                POBJ p = new POBJ();
                p.Read(d, dat, this, node);

                d.seek(mobjOffset);
                material.Read(d, dat);

                if (nextOffset != 0)
                {
                    d.seek(nextOffset);
                    DOBJ de = new DOBJ();
                    de.Read(d, dat, parent);
                }
            }

            public class MOBJ
            {
                public int unk1 = 0, unk2 = 0;
                public int tobjOffset, materialOffset;
                public int unk3 = 0, unk4 = 0;

                public TOBJ texture = new TOBJ();

                public void Read(FileData d, DAT dat)
                {
                    unk1 = d.readInt();
                    unk2 = d.readInt();
                    tobjOffset = d.readInt();
                    materialOffset = d.readInt();
                    unk3 = d.readInt();
                    unk4 = d.readInt();
                    
                    if (tobjOffset != 0)
                    {
                        d.seek(tobjOffset);
                        texture.Read(d, dat);
                    }
                }
            }

            public class TOBJ
            {
                public int unk1 = 0;
                public int wrap_s = 0, wrap_t = 0;
                public int scale_w = 0, scale_h = 0;
                public int imageOffset, paletteOffset;
                public int unk2, unkOffset;

                public Bitmap image;
                public int texid;

                public int imageDataOffset;
                public int width, height, format;

                public int paletteDataOffset;
                public int paletteFormat;
                public int unk;
                public int count;
                public int unkown;

                public void Read(FileData d, DAT dat)
                {
                    for (int i = 0; i < 13; i++)
                        d.skip(4); // ?? TODO:

                    int testOffset = d.pos();
                    wrap_s = d.readInt();
                    wrap_t = d.readInt();
                    scale_w = d.readByte();
                    scale_h = d.readByte();
                    d.skip(2);
                    d.skip(12);
                    imageOffset = d.readInt();
                    paletteOffset = d.readInt();
                    unkOffset = d.readInt();

                    d.seek(imageOffset);
                    imageDataOffset = d.readInt();
                    width = d.readShort();
                    height = d.readShort();
                    format = d.readInt();

                    Console.WriteLine($"TOBJ offset - {testOffset.ToString("X")} | image offset - {imageOffset.ToString("X")} | image data offset - {imageDataOffset.ToString("X")}");

                    d.seek(paletteOffset);
                    paletteDataOffset = d.readInt();
                    paletteFormat = d.readInt();
                    unk = d.readInt();
                    count = d.readShort();
                    unkown = d.readShort();


                    if (dat.texturesLinker.Keys.Contains(imageDataOffset))
                        image = dat.texturesLinker[imageDataOffset];
                    else
                    {
                        image = TPL.ConvertFromTextureMelee(d.getSection(imageDataOffset, TPL.textureByteSize((TPL_TextureFormat)format, width, height)),
                            width, height, format,
                            paletteOffset == 0 ? null : d.getSection(paletteDataOffset, 4 * count), count, paletteFormat);

                        dat.texturesLinker.Add(imageDataOffset, image);
                        dat.tobjLinker.Add(imageDataOffset, new object[]{ testOffset, image, imageOffset, imageDataOffset });
                    }


                    texid = NUT.loadImage(image);
                }
            }
        }

        public enum GXAttrType
        {
            GX_NONE = 0,
            GX_DIRECT = 1,
            GX_INDEX8 = 2,
            GX_INDEX16 = 3
        }
        public Dictionary<GXAttrType, int> GXAttrSize = new Dictionary<GXAttrType, int>()
        {
            { GXAttrType.GX_NONE, 0},
            { GXAttrType.GX_DIRECT, 1},
            { GXAttrType.GX_INDEX8, 1},
            { GXAttrType.GX_INDEX16, 2}
        };

        public static Dictionary<int, int> GX_COMPSIZE = new Dictionary<int, int>()
            {
                { 0, 1},
                { 1, 1},
                { 2, 2},
                { 3, 2},
                { 4, 4}
            };

        public class VertexAttr
        {
            public int vtxAttr;
            public GXAttrType vtxAttrType;
            public int compCnt;
            public int compType;

            public byte scale = 0xFF;
            public byte unknown = 0x00;
            public short vtxStride;
            public int dataOffset = 0;

            public VertexAttr Read(FileData d)
            {
                vtxAttr = d.readInt();
                if (vtxAttr == 0xFF)
                    return null;
                vtxAttrType = (GXAttrType)d.readInt();
                compCnt = d.readInt();
                compType = d.readInt();
                scale = (byte)d.readByte();
                unknown = (byte)d.readByte();
                vtxStride = (short)d.readShort();
                dataOffset = d.readInt();

                //Console.WriteLine((GXAttr)vtxAttr + " " + vtxAttrType + " comp type " + compType + " Data Offset: " + dataOffset.ToString("x") + " Scale: " + scale);

                return this;
            }
        }

        public class Vertex
        {
            public Vector3 pos = new Vector3();
            public Vector3 nrm = new Vector3();
            public Vector2 tx0 = new Vector2();
            public Vector2 tx1 = new Vector2();
            public Vector4 clr = new Vector4(1, 1, 1, 1);
            public List<int> bones = new List<int>();
            public List<float> weights = new List<float>();
            
            public List<object> Tags = new List<object>(); // this is for post processing of vertices
        }

        public List<Vertex> vertBank = new List<Vertex>();


        public enum GXAttr
        {
            GX_VA_PNMTXIDX = 0,    // position/normal matrix index
            GX_VA_TEX0MTXIDX,      // texture 0 matrix index
            GX_VA_TEX1MTXIDX,      // texture 1 matrix index
            GX_VA_TEX2MTXIDX,      // texture 2 matrix index
            GX_VA_TEX3MTXIDX,      // texture 3 matrix index
            GX_VA_TEX4MTXIDX,      // texture 4 matrix index
            GX_VA_TEX5MTXIDX,      // texture 5 matrix index
            GX_VA_TEX6MTXIDX,      // texture 6 matrix index
            GX_VA_TEX7MTXIDX,      // texture 7 matrix index
            GX_VA_POS = 9,    // position
            GX_VA_NRM,             // normal
            GX_VA_CLR0,            // color 0
            GX_VA_CLR1,            // color 1
            GX_VA_TEX0,            // input texture coordinate 0
            GX_VA_TEX1,            // input texture coordinate 1
            GX_VA_TEX2,            // input texture coordinate 2
            GX_VA_TEX3,            // input texture coordinate 3
            GX_VA_TEX4,            // input texture coordinate 4
            GX_VA_TEX5,            // input texture coordinate 5
            GX_VA_TEX6,            // input texture coordinate 6
            GX_VA_TEX7,            // input texture coordinate 7
            GX_POS_MTX_ARRAY,      // position matrix array pointer
            GX_NRM_MTX_ARRAY,      // normal matrix array pointer
            GX_TEX_MTX_ARRAY,      // texture matrix array pointer
            GX_LIGHT_ARRAY,        // light parameter array pointer
            GX_VA_NBT,             // normal, bi-normal, tangent 
            GX_VA_MAX_ATTR,        // maximum number of vertex attributes
            GX_VA_NULL = 0xff  // NULL attribute (to mark end of lists)
        }

        // POLYGON
        public class POBJ
        {
            TreeNode node = new TreeNode();

            public int unk1 = 0; // padding?
            public int nextOffset = 0;
            public int vertexAttrArray = 0;
            public short flags = 0;
            public short displayListSize = 0;

            public int displayListOffset = 0;
            public int weightListOffset = 0;

            // format
            List<VertexAttr> vertexAttributes = new List<VertexAttr>();

            public class DisplayObject
            {
                public int type;
                public Vertex[] verts;
                public List<int> faces = new List<int>();
            }
            public List<DisplayObject> display = new List<DisplayObject>();


            public void Read(FileData d, DAT dat, DOBJ dobj, TreeNode parent)
            {
                dobj.polygons.Add(this);

                node.Tag = this;
                parent.Nodes.Add(node);

                unk1 = d.readInt();
                nextOffset = d.readInt();
                vertexAttrArray = d.readInt();
                flags = (short)d.readShort();
                displayListSize = (short)d.readShort();
                displayListOffset = d.readInt();
                weightListOffset = d.readInt();
                node.Text = "Polygon_" + flags.ToString("X4");

                // vertex attributes
                d.seek(vertexAttrArray);
                VertexAttr a = new VertexAttr();
                a.Read(d);
                while (a.vtxAttr != 0xFF)
                {
                    vertexAttributes.Add(a);
                    a = new VertexAttr();
                    a.Read(d);
                }

                //Console.WriteLine(weightListOffset.ToString("x"));

                var boneList = new List<List<object>>();
                var weightList = new List<List<float>>();

                if (weightListOffset != 0)
                {
                    switch (flags & 0x3000)
                    {
                        case 0x0000:
                            {
                                int offset = weightListOffset;
                                var jobjs = new List<object>();
                                while (offset != 0)
                                {
                                    jobjs.Add(offset);

                                    var jobj = dat.jobjOffsetLinker[offset];
                                    offset = jobj.childOffset;
                                }

                                boneList.Add(jobjs);
                            }
                            break;
                        case 0x1000:
                            {

                            }
                            break;
                        case 0x2000:
                            {
                                d.seek(weightListOffset);
                                int offset = 0;
                                while ((offset = d.readInt()) != 0)
                                {
                                    var temp = d.pos();
                                    var jobjs = new List<object>();
                                    var weights = new List<float>();

                                    d.seek(offset);

                                    int off1 = d.readInt();
                                    float wei1 = d.readFloat();
                                    while (off1 != 0)
                                    {
                                        jobjs.Add(off1);
                                        weights.Add(wei1);

                                        off1 = d.readInt();
                                        wei1 = d.readFloat();
                                    }

                                    boneList.Add(jobjs);
                                    weightList.Add(weights);

                                    if (weights.Count > MaxWeightCount)
                                    {
                                        Console.WriteLine(string.Format("Error: Weight count ({0}) > {1}.", weights.Count, MaxWeightCount));
                                    }

                                    d.seek(temp);
                                }
                            }
                            break;
                    }
                }

                // display list
                d.seek(displayListOffset);
                //Console.WriteLine("Display 0x" + displayListOffset + " " + displayListSize);
                int bid = 0;
                List<Vertex> used = new List<Vertex>();
                if (displayListOffset != 0)
                    for (int i = 0; i < displayListSize; i++)
                    {
                        byte prim = (byte)d.readByte();
                        int count = d.readShort();

                        if (prim == 0)
                            break;

                        DisplayObject ob = new DisplayObject();
                        ob.type = prim;

                        //List<Vertex> vcol = new List<Vertex>();
                        for (int j = 0; j < count; j++)
                        {
                            bid = -1;
                            float[] f;
                            int temp;
                            Vertex v = new Vertex();
                            ob.faces.Add(dat.vertBank.Count);
                            dat.vertBank.Add(v);

                            foreach (VertexAttr att in vertexAttributes)
                            {
                                int value = 0;
                                switch (att.vtxAttrType)
                                {
                                    case GXAttrType.GX_DIRECT:
                                        if (att.vtxAttr != (int)GXAttr.GX_VA_CLR0)
                                            value = d.readByte();
                                        else
                                        {
                                            v.clr = readGXClr(d, att.compType);
                                        }
                                        break;
                                    case GXAttrType.GX_INDEX8:
                                        value = d.readByte();
                                        break;
                                    case GXAttrType.GX_INDEX16:
                                        value = (short)d.readShort();
                                        break;
                                    default:
                                        break;
                                }

                                switch ((GXAttr)att.vtxAttr)
                                {
                                    case GXAttr.GX_VA_PNMTXIDX:
                                        bid = value;
                                        break;
                                    case GXAttr.GX_VA_POS:
                                        // here add face and vertex for rendering......
                                        int vr = value;

                                        int te = d.pos();
                                        d.seek(att.dataOffset + att.vtxStride * vr);
                                        f = read3(d, att.compType, att.vtxStride);
                                        v.pos.X = f[0];
                                        v.pos.Y = f[1];
                                        v.pos.Z = f[2];
                                        v.pos = Vector3.Divide(v.pos, (float)Math.Pow(2, att.scale));
                                        d.seek(te);

                                        // sometimes we need to transform the default vertex by
                                        // the normal transform; I plan on storing it in a normal way

                                        if (weightListOffset != 0 && bid > -1)
                                        {
                                            switch (flags & 0x3000)
                                            {
                                                case 0x0000:
                                                    {
                                                        // TODO:  Still need more testing and verification on this,
                                                        // but this fixed the flowers in GrOp.dat.  Probably actually needs
                                                        // to be implements as a transormation by the inverse transform
                                                        // but the current bone structure would not allow specifying this
                                                        int offset = (int)boneList[0][bid / 3];
                                                        JOBJ jobj = dat.jobjOffsetLinker[offset];

                                                        var transform = Matrix4.CreateScale(jobj.sca)
                                                                            * Matrix4.CreateFromQuaternion(VBN.FromEulerAngles(jobj.rot.Z, jobj.rot.Y, jobj.rot.X))
                                                                            * Matrix4.CreateTranslation(jobj.pos);
                                                        Matrix4 mt = jobj.inverseTransform * transform.Inverted() * ((JOBJ)parent.Parent.Tag).transform;
                                                        v.pos = Vector3.Transform(v.pos, mt);

                                                        v.Tags.Add(offset);
                                                        v.weights.Add(1);
                                                    }
                                                    break;
                                                case 0x1000:
                                                    {

                                                    }
                                                    break;
                                                case 0x2000:
                                                    {
                                                        v.Tags.AddRange(boneList[bid / 3]);
                                                        v.weights.AddRange(weightList[bid / 3]);
                                                    }
                                                    break;
                                            }
                                        }

                                        if (parent.Parent.Tag is JOBJ)
                                        {
                                            if (v.weights.Count == 0)
                                            {
                                                v.Tags.Add(parent.Parent.Tag);
                                                v.weights.Add(1);
                                                v.pos = Vector3.Transform(v.pos, ((JOBJ)parent.Parent.Tag).transform);
                                            }
                                        }
                                        break;

                                    case GXAttr.GX_VA_NRM:
                                        int ten = d.pos();
                                        d.seek(att.dataOffset + att.vtxStride * value);
                                        f = read3(d, att.compType, att.vtxStride);
                                        v.nrm.X = f[0];
                                        v.nrm.Y = f[1];
                                        v.nrm.Z = f[2];
                                        v.nrm = Vector3.Divide(v.nrm, (float)Math.Pow(2, att.scale));
                                        d.seek(ten);
                                        break;

                                    case GXAttr.GX_VA_TEX0:
                                        temp = d.pos();
                                        d.seek(att.dataOffset + att.vtxStride * value);
                                        f = read3(d, att.compType, att.vtxStride / GX_COMPSIZE[att.compType]);
                                        v.tx0.X = f[0];
                                        v.tx0.Y = f[1];
                                        v.tx0 = Vector2.Divide(v.tx0, (float)Math.Pow(2, att.scale));
                                        d.seek(temp);
                                        break;

                                    case GXAttr.GX_VA_TEX1:
                                        temp = d.pos();
                                        d.seek(att.dataOffset + att.vtxStride * value);
                                        f = read3(d, att.compType, att.vtxStride / GX_COMPSIZE[att.compType]);
                                        v.tx1.X = f[0];
                                        v.tx1.Y = f[1];
                                        v.tx1 = Vector2.Divide(v.tx0, (float)Math.Pow(2, att.scale));
                                        d.seek(temp);
                                        break;
                                }
                            }
                        }

                        display.Add(ob);
                    }
                

                if (nextOffset != 0)
                {
                    d.seek(nextOffset);
                    POBJ pol = new POBJ();
                    pol.Read(d, dat, dobj, parent);
                }
            }

            public static Vector4 readGXClr(FileData d, int type)
            {
                Vector4 clr = new Vector4(1, 1, 1, 1);
                int b;

                switch (type)
                {
                    case 0: // GX_RGB565
                        b = d.readShort();
                        clr.X = ((((b >> 11) & 0x1F) << 3) | (((b >> 11) & 0x1F) >> 2)) / (float)0xFF;
                        clr.Y = ((((b >> 5) & 0x3F) << 2) | (((b >> 5) & 0x3F) >> 4)) / (float)0xFF;
                        clr.Z = ((((b) & 0x1F) << 3) | (((b) & 0x1F) >> 2)) / (float)0xFF;
                        break;
                    case 1: // GX_RGB888
                        clr.X = d.readByte() / (float)0xFF;
                        clr.Y = d.readByte() / (float)0xFF;
                        clr.Z = d.readByte() / (float)0xFF;
                        break;
                    case 2: // GX_RGBX888
                        clr.X = d.readByte() / (float)0xFF;
                        clr.Y = d.readByte() / (float)0xFF;
                        clr.Z = d.readByte() / (float)0xFF;
                        d.skip(8);
                        break;
                    case 3: // GX_RGBA4
                        b = d.readShort();
                        clr.X = ((((b >> 12) & 0xF) << 4) | ((b >> 12) & 0xF)) / (float)0xFF;
                        clr.Y = ((((b >> 8) & 0xF) << 4) | ((b >> 8) & 0xF)) / (float)0xFF;
                        clr.Z = ((((b >> 4) & 0xF) << 4) | ((b >> 4) & 0xF)) / (float)0xFF;
                        clr.W = ((((b) & 0xF) << 4) | ((b) & 0xF)) / (float)0xFF;
                        break;
                    case 4: // GX_RGBA6
                        b = d.readThree();
                        clr.X = ((((b >> 18) & 0x3F) << 2) | (((b >> 18) & 0x3F) >> 4)) / (float)0xFF;
                        clr.Y = ((((b >> 12) & 0x3F) << 2) | (((b >> 12) & 0x3F) >> 4)) / (float)0xFF;
                        clr.Z = ((((b >> 6) & 0x3F) << 2) | (((b >> 6) & 0x3F) >> 4)) / (float)0xFF;
                        clr.W = ((((b) & 0x3F) << 2) | (((b) & 0x3F) >> 4)) / (float)0xFF;
                        break;
                    case 5: // GX_RGBX888
                        clr.X = d.readByte() / (float)0xFF;
                        clr.Y = d.readByte() / (float)0xFF;
                        clr.Z = d.readByte() / (float)0xFF;
                        clr.W = d.readByte() / (float)0xFF;
                        break;
                }

                return clr;
            }

            public static float[] read3(FileData d, int type, int size)
            {
                float[] a = new float[size];

                switch (type)
                {
                    case 0:
                        for (int i = 0; i < size; i++)
                            a[i] = d.readByte();
                        break;
                    case 1:
                        for (int i = 0; i < size; i++)
                            a[i] = unchecked((sbyte)(d.readByte()));
                        break;
                    case 2:
                        for (int i = 0; i < size; i++)
                            a[i] = (d.readShort());
                        break;
                    case 3:
                        for (int i = 0; i < size; i++)
                            a[i] = (short)(d.readShort());
                        break;
                    case 4:
                        for (int i = 0; i < size; i++)
                            a[i] = d.readFloat();
                        break;
                }

                return a;
            }




            #region Textures

            public static byte[] fromCMP(byte[] tpl, int width, int height)
            {
                uint[] output = new uint[width * height];
                ushort[] c = new ushort[4];
                int[] pix = new int[4];
                int inp = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int ww = width; //Shared.AddPadding(width, 8);

                        if (ww % 8 != 0)
                        {
                            ww = ww + (8 - (ww % 8));
                        }

                        int x0 = x & 0x03;
                        int x1 = (x >> 2) & 0x01;
                        int x2 = x >> 3;

                        int y0 = y & 0x03;
                        int y1 = (y >> 2) & 0x01;
                        int y2 = y >> 3;

                        int off = (8 * x1) + (16 * y1) + (32 * x2) + (4 * ww * y2);

                        c[0] = Swap(BitConverter.ToUInt16(tpl, off));
                        c[1] = Swap(BitConverter.ToUInt16(tpl, off + 2));

                        if (c[0] > c[1])
                        {
                            c[2] = (ushort)avg(2, 1, c[0], c[1]);
                            c[3] = (ushort)avg(1, 2, c[0], c[1]);
                        }
                        else
                        {
                            c[2] = (ushort)avg(1, 1, c[0], c[1]);
                            c[3] = 0;
                        }

                        uint pixel = Swap(BitConverter.ToUInt32(tpl, off + 4));

                        int ix = x0 + (4 * y0);
                        int raw = c[(pixel >> (30 - (2 * ix))) & 0x03];

                        pix[0] = (raw >> 8) & 0xf8;
                        pix[1] = (raw >> 3) & 0xf8;
                        pix[2] = (raw << 3) & 0xf8;
                        pix[3] = 0xff;
                        if (((pixel >> (30 - (2 * ix))) & 0x03) == 3 && c[0] <= c[1]) pix[3] = 0x00;

                        output[inp] = (uint)((pix[0] << 16) | (pix[1] << 8) | (pix[2] << 0) | (pix[3] << 24));
                        inp++;
                    }
                }

                return UIntArrayToByteArray(output);
            }

            public static Bitmap rgbaToImage(byte[] data, int width, int height)
            {
                if (width == 0) width = 1;
                if (height == 0) height = 1;

                Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                try
                {
                    System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                                         new Rectangle(0, 0, bmp.Width, bmp.Height),
                                         System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

                    System.Runtime.InteropServices.Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
                    bmp.UnlockBits(bmpData);
                }
                catch { bmp.Dispose(); throw; }

                return bmp;
            }

            public static byte[] UIntArrayToByteArray(uint[] array)
            {
                List<byte> results = new List<byte>();
                foreach (uint value in array)
                {
                    byte[] converted = BitConverter.GetBytes(value);
                    results.AddRange(converted);
                }
                return results.ToArray();
            }

            public static ushort Swap(ushort value)
            {
                return (ushort)IPAddress.HostToNetworkOrder((short)value);
            }

            public static uint Swap(uint value)
            {
                return (uint)IPAddress.HostToNetworkOrder((int)value);
            }

            public static int avg(int w0, int w1, int c0, int c1)
            {
                int a0 = c0 >> 11;
                int a1 = c1 >> 11;
                int a = (w0 * a0 + w1 * a1) / (w0 + w1);
                int c = (a << 11) & 0xffff;

                a0 = (c0 >> 5) & 63;
                a1 = (c1 >> 5) & 63;
                a = (w0 * a0 + w1 * a1) / (w0 + w1);
                c = c | ((a << 5) & 0xffff);

                a0 = c0 & 31;
                a1 = c1 & 31;
                a = (w0 * a0 + w1 * a1) / (w0 + w1);
                c = c | a;

                return c;
            }

            #endregion
        }
    }
}
