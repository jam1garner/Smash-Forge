using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.IO;
using Syroot.NintenTools.MarioKart8.Collisions;
using Smash_Forge.Rendering;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;
using SFGraphics.GLObjects.Shaders;

namespace Smash_Forge
{
    public class KCL : TreeNode
    {


        // gl buffer objects
        int vbo_position;
        int ibo_elements;

        //Set the game's material list
        public GameSet GameMaterialSet = GameSet.MarioKart8D;

        public enum GameSet : ushort
        {
            MarioOdyssey = 0x0,
            MarioKart8D = 0x1,
            Splatoon2 = 0x2,
        }
        public enum CollisionType_MarioOdssey : ushort
        {

        }
        public enum CollisionType_MK8D : ushort
        {
            Road_Default = 0,
            Road_Bumpy = 2,
            Road_Sand = 4,
            Offroad_Sand = 6,
            Road_HeavySand = 8,
            Road_IcyRoad = 9,
            OrangeBooster = 10,
            AntiGravityPanel = 11,
            Latiku = 16,
            Wall5 = 17,
            Wall4 = 19,
            Wall = 23,
            Latiku2 = 28,
            Glider = 31,
            SidewalkSlope = 32,
            Road_Dirt = 33,
            Unsolid = 56,
            Water = 60,
            Road_Stone = 64,
            Wall1 = 81,
            Wall2 = 84,
            FinishLine = 93,
            RedFlowerEffect = 95,
            Wall3 = 113,
            WhiteFlowerEffect = 127,
            Road_Metal = 128,
            Road_3DS_MP_Piano = 129,
            Road_RoyalR_Grass = 134,
            TopPillar = 135,
            YoshiCuiruit_Grass = 144,
            YellowFlowerEffect = 159,

            Road_MetalGating = 160,
            Road_3DS_MP_Xylophone = 161,
            Road_3DS_MP_Vibraphone = 193,
            SNES_RR_road = 227,
            Offroad_Mud = 230,
            Trick = 4096,
            BoosterStunt = 4106,
            TrickEndOfRamp = 4108,
            Trick3 = 4130,
            Trick6 = 4160,
            Trick4 = 4224,
            Trick5 = 8192,
            BoostTrick = 8202,
        }

        public KclFile kcl = null;
        public static Shader shader = null;
        public List<KCLModel> models = new List<KCLModel>();

        public KCL()
        {
            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out ibo_elements);
        }

        public KCL(byte[] file_data) : this()
        {
            Text = "KCLFile";

            Read(file_data);
            UpdateVertexData();
        }

        public void Destroy()
        {
            GL.DeleteBuffer(vbo_position);
            GL.DeleteBuffer(ibo_elements);
        }

        public class Vertex
        {
            public Vector3 pos = new Vector3();
            public Vector3 nrm = new Vector3();
            public Vector3 col = new Vector3();
        }
        public struct DisplayVertex
        {
            // Used for rendering.
            public Vector3 pos;
            public Vector3 nrm;
            public Vector3 col;

            public static int Size = 4 * (3 + 3 + 3);
        }

        public class KCLModel : TreeNode
        {
            public KCLModel()
            {
                Checked = true;
                ImageKey = "mesh";
                SelectedImageKey = "mesh";
            }

            public List<int> faces = new List<int>();
            public List<Vertex> vertices = new List<Vertex>();
            public int[] Faces;

            // for drawing
            public int[] display;
            public int Offset; // For Rendering

            public int strip = 0x40;
            public int displayFaceSize = 0;

            public class Face : TreeNode
            {
                public int MaterialFlag = 0;

            }

            public void CalulateOctree()
            {

            }

            public List<DisplayVertex> CreateDisplayVertices()
            {
                // rearrange faces
                display = getDisplayFace().ToArray();

                List<DisplayVertex> displayVertList = new List<DisplayVertex>();

                if (faces.Count <= 3)
                    return displayVertList;

                foreach (Vertex v in vertices)
                {
                    DisplayVertex displayVert = new DisplayVertex()
                    {
                        pos = v.pos,
                        nrm = v.nrm,
                        col = v.col,
                    };

                    displayVertList.Add(displayVert);
                }

                return displayVertList;
            }
            public List<int> getDisplayFace()
            {
                if ((strip >> 4) == 4)
                {
                    displayFaceSize = faces.Count;
                    return faces;
                }
                else
                {
                    List<int> f = new List<int>();

                    int startDirection = 1;
                    int p = 0;
                    int f1 = faces[p++];
                    int f2 = faces[p++];
                    int faceDirection = startDirection;
                    int f3;
                    do
                    {
                        f3 = faces[p++];
                        if (f3 == 0xFFFF)
                        {
                            f1 = faces[p++];
                            f2 = faces[p++];
                            faceDirection = startDirection;
                        }
                        else
                        {
                            faceDirection *= -1;
                            if ((f1 != f2) && (f2 != f3) && (f3 != f1))
                            {
                                if (faceDirection > 0)
                                {
                                    f.Add(f3);
                                    f.Add(f2);
                                    f.Add(f1);
                                }
                                else
                                {
                                    f.Add(f2);
                                    f.Add(f3);
                                    f.Add(f1);
                                }
                            }
                            f1 = f2;
                            f2 = f3;
                        }
                    } while (p < faces.Count);

                    displayFaceSize = f.Count;
                    return f;
                }
            }
        }

        private void SetupShader()
        {
            int maxUniformBlockSize = GL.GetInteger(GetPName.MaxUniformBlockSize);
            if (shader == null)
            {
                shader = new Shader();
                shader = OpenTKSharedResources.shaders["KCL"];
            }
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

        }

        public void Render(Matrix4 view)
        {
            shader = OpenTKSharedResources.shaders["KCL"];

            shader.UseProgram();
            shader.EnableVertexAttributes();
            SetRenderSettings(shader);

            shader.SetMatrix4x4("modelview", ref view);

            foreach (KCLModel mdl in models)
            {
                 DrawModel(mdl, shader);
            }

            shader.DisableVertexAttributes();
        }
        private void SetRenderSettings(Shader shader)
        {
            shader.SetBoolToInt("renderVertColor", Runtime.renderVertColor);
            shader.SetInt("renderType", (int)Runtime.renderType);
        }
        private void DrawModel(KCLModel m, Shader shader, bool drawSelection = false)
        {
            if (m.faces.Count <= 3)
                return;

            SetVertexAttributes(m, shader);


            if (m.Checked)
            {
                if ((m.IsSelected))
                {
                    DrawModelSelection(m, shader);
                }
                else
                {
                    if (Runtime.renderModelWireframe)
                    {
                        DrawModelWireframe(m, shader);
                    }

                    if (Runtime.renderModel)
                    {
                        GL.DrawElements(PrimitiveType.Triangles, m.displayFaceSize, DrawElementsType.UnsignedInt, m.Offset);
                    }
                }
            }    
        }
        public void UpdateVertexData()
        {
            DisplayVertex[] Vertices;
            int[] Faces;

            int poffset = 0;
            int voffset = 0;
            List<DisplayVertex> Vs = new List<DisplayVertex>();
            List<int> Ds = new List<int>();
            foreach (KCLModel m in models)
            {
                m.Offset = poffset * 4;
                List<DisplayVertex> pv = m.CreateDisplayVertices();
                Vs.AddRange(pv);

                Console.WriteLine(m.displayFaceSize);

                for (int i = 0; i < m.displayFaceSize; i++)
                {
                    Ds.Add(m.display[i] + voffset);
                }
                poffset += m.displayFaceSize;
                voffset += pv.Count;
            }

            // Binds
            Vertices = Vs.ToArray();
            Faces = Ds.ToArray();

            // Bind only once!
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<DisplayVertex>(BufferTarget.ArrayBuffer, (IntPtr)(Vertices.Length * DisplayVertex.Size), Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData<int>(BufferTarget.ElementArrayBuffer, (IntPtr)(Faces.Length * sizeof(int)), Faces, BufferUsageHint.StaticDraw);
        }

        private void SetVertexAttributes(KCLModel m, Shader shader)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vPosition"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 0);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vNormal"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 12);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vColor"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 24);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
        }

        private static void DrawModelWireframe(KCLModel p, Shader shader)
        {
            // use vertex color for wireframe color
            shader.SetInt("colorOverride", 1);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.5f);
            GL.DrawElements(PrimitiveType.Triangles, p.displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            shader.SetInt("colorOverride", 0);
        }
        private static void DrawModelSelection(KCLModel p, Shader shader)
        {
            //This part needs to be reworked for proper outline. Currently would make model disappear

            GL.DrawElements(PrimitiveType.Triangles, p.displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);

            GL.Enable(EnableCap.StencilTest);
            // use vertex color for wireframe color
            shader.SetInt("colorOverride", 1);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.5f);
            GL.DrawElements(PrimitiveType.Triangles, p.displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            shader.SetInt("colorOverride", 0);

            GL.Enable(EnableCap.DepthTest);
        }

        public List<int> AllFlags = new List<int>();

        public void Read(byte[] file_data)
        {
            try
            {
                kcl = new KclFile(new MemoryStream(file_data), true, false, Syroot.BinaryData.ByteOrder.LittleEndian);
            }
            catch
            {
                kcl = new KclFile(new MemoryStream(file_data), true, false, Syroot.BinaryData.ByteOrder.BigEndian);
            }

            AllFlags.Clear();

            int CurModelIndx = 0;
            foreach (KclModel mdl in kcl.Models)
            {
                KCLModel kclmodel = new KCLModel();

                kclmodel.Text = "Model " + CurModelIndx;


                KclFace[] indicesArray = mdl.Faces;

                int ft = 0;
                foreach (KclFace f in mdl.Faces)
                {
                    Vertex vtx = new Vertex();
                    Vertex vtx2 = new Vertex();
                    Vertex vtx3 = new Vertex();


                    Vector3 CrossA = Vector3.Cross(Vec3F_To_Vec3(mdl.Normals[f.Normal1Index]), Vec3F_To_Vec3(mdl.Normals[f.DirectionIndex]));
                    Vector3 CrossB = Vector3.Cross(Vec3F_To_Vec3(mdl.Normals[f.Normal2Index]), Vec3F_To_Vec3(mdl.Normals[f.DirectionIndex]));
                    Vector3 CrossC = Vector3.Cross(Vec3F_To_Vec3(mdl.Normals[f.Normal3Index]), Vec3F_To_Vec3(mdl.Normals[f.DirectionIndex]));
                    Vector3 normal_a = Vec3F_To_Vec3(mdl.Normals[f.Normal1Index]);
                    Vector3 normal_b = Vec3F_To_Vec3(mdl.Normals[f.Normal2Index]);
                    Vector3 normal_c = Vec3F_To_Vec3(mdl.Normals[f.Normal3Index]);


                    float result1 = Vector3.Dot(new Vector3(CrossB.X, CrossB.Y, CrossB.Z), (new Vector3(normal_c.X, normal_c.Y, normal_c.Z)));
                    float result2 = Vector3.Dot(new Vector3(CrossA.X, CrossA.Y, CrossA.Z), (new Vector3(normal_c.X, normal_c.Y, normal_c.Z)));

                    Vector3 pos = Vec3F_To_Vec3(mdl.Positions[f.PositionIndex]);
                    Vector3 nrm = Vec3F_To_Vec3(mdl.Normals[f.Normal1Index]);

                    Vector3 Vertex1 = pos;
                    Vector3 Vertex2 = pos + CrossB * (f.Length / result1);
                    Vector3 Vertex3 = pos + CrossA * (f.Length / result2);

                    vtx.pos = new Vector3(Vertex1.X, Vertex1.Y, Vertex1.Z);
                    vtx2.pos = new Vector3(Vertex2.X, Vertex2.Y, Vertex2.Z);
                    vtx3.pos = new Vector3(Vertex3.X, Vertex3.Y, Vertex3.Z);

                    var dir = Vector3.Cross(Vertex2 - Vertex1, Vertex3 - Vertex1);
                    var norm = Vector3.Normalize(dir);

                    vtx.nrm = norm;
                    vtx2.nrm = norm;
                    vtx3.nrm = norm;

                    KCLModel.Face face = new KCLModel.Face();

                    face.Text = f.CollisionFlags.ToString();

                    face.MaterialFlag = f.CollisionFlags;

                    Color color = SetMaterialColor(face);


                    AllFlags.Add(face.MaterialFlag);

                    Vector3 ColorSet = new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);

                    vtx.col = new Vector3(ColorSet);
                    vtx2.col = new Vector3(ColorSet);
                    vtx3.col = new Vector3(ColorSet);

                    kclmodel.faces.Add(ft);
                    kclmodel.faces.Add(ft + 1);
                    kclmodel.faces.Add(ft + 2);

                    ft += 3;

                    kclmodel.vertices.Add(vtx);
                    kclmodel.vertices.Add(vtx2);
                    kclmodel.vertices.Add(vtx3);


                }


                models.Add(kclmodel);


 
                Nodes.Add(kclmodel);

                CurModelIndx++;
            }

            List<int> noDupes = AllFlags.Distinct().ToList();
            Console.WriteLine("List of all material flags (Not duped)");
            foreach (int mat in noDupes)
            {
                Console.WriteLine("Mat flag " + (CollisionType_MK8D)mat);

            }

        }
        //Convert KCL lib vec3 to opentk one so i can use the cross and dot methods
        public static Vector3 Vec3F_To_Vec3(Syroot.Maths.Vector3F v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public void Save(string fileName)
        {
            kcl.Save(fileName);
        }
        private Color SetMaterialColor(KCLModel.Face f)
        {
            if (GameMaterialSet == GameSet.MarioKart8D)
            {
                switch (f.MaterialFlag)
                {
                    case (ushort)CollisionType_MK8D.Road_Default:
                        return Color.DarkGray;
                    case (ushort)CollisionType_MK8D.Glider:
                        return Color.Orange;
                    case (ushort)CollisionType_MK8D.Road_Sand:
                        return Color.LightYellow;
                    case (ushort)CollisionType_MK8D.Offroad_Sand:
                        return Color.SandyBrown; 
                    case (ushort)CollisionType_MK8D.Water:
                        return Color.Blue;
                    case (ushort)CollisionType_MK8D.Wall1:
                        return Color.LightSlateGray;
                    case (ushort)CollisionType_MK8D.Wall2:
                        return Color.OrangeRed;
                    case (ushort)CollisionType_MK8D.Wall3:
                        return Color.IndianRed;
                    case (ushort)CollisionType_MK8D.Unsolid:
                        return Color.Beige;
                    case (ushort)CollisionType_MK8D.Road_3DS_MP_Piano:
                        return Color.RosyBrown;
                    case (ushort)CollisionType_MK8D.Road_3DS_MP_Vibraphone:
                        return Color.BurlyWood;
                    case (ushort)CollisionType_MK8D.Road_3DS_MP_Xylophone:
                        return Color.DarkSalmon;
                    case (ushort)CollisionType_MK8D.Latiku:
                        return Color.GhostWhite;
                    case (ushort)CollisionType_MK8D.Road_Bumpy:
                        return Color.GreenYellow;
                    case (ushort)CollisionType_MK8D.Road_RoyalR_Grass:
                        return Color.Green;
                    case (ushort)CollisionType_MK8D.YoshiCuiruit_Grass:
                        return Color.Green;
                    case (ushort)CollisionType_MK8D.Wall:
                        return Color.LightCyan;
                    case (ushort)CollisionType_MK8D.Wall4:
                        return Color.LightSlateGray;
                    case (ushort)CollisionType_MK8D.Wall5:
                        return Color.DarkSlateGray;
                    case (ushort)CollisionType_MK8D.AntiGravityPanel:
                        return Color.Purple;
                    case (ushort)CollisionType_MK8D.SidewalkSlope:
                        return Color.FromArgb(153, 153, 102);
                    case (ushort)CollisionType_MK8D.BoostTrick:
                        return Color.DarkOrange;
                    case (ushort)CollisionType_MK8D.Offroad_Mud:
                        return Color.FromArgb(77, 26, 0);
                    case (ushort)CollisionType_MK8D.Road_Metal:
                        return Color.FromArgb(80, 80, 80);
                    case (ushort)CollisionType_MK8D.Road_MetalGating:
                        return Color.FromArgb(64, 64, 64);
                    case (ushort)CollisionType_MK8D.Road_Dirt:
                        return Color.Sienna;
                    case (ushort)CollisionType_MK8D.Road_Stone:
                        return Color.FromArgb(50, 50, 50);
                    case (ushort)CollisionType_MK8D.Latiku2:
                        return Color.WhiteSmoke;
                    case (ushort)CollisionType_MK8D.RedFlowerEffect:
                        return Color.MediumVioletRed;
                    case (ushort)CollisionType_MK8D.WhiteFlowerEffect:
                        return Color.FloralWhite;
                    case (ushort)CollisionType_MK8D.YellowFlowerEffect:
                        return Color.Yellow;
                    case (ushort)CollisionType_MK8D.TopPillar:
                        return Color.Gray;
                    default:
                        return Color.FromArgb(20, 20, 20);
                }
            }
            else if (GameMaterialSet == GameSet.MarioOdyssey)
            {
                return Color.Gray;
            }
            else if (GameMaterialSet == GameSet.Splatoon2)
            {
                return Color.Gray;
            }
            else
            {
                return Color.Gray;
            }
        }
    }
}
