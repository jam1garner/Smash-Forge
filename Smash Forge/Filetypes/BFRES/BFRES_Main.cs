using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using Syroot.NintenTools.Bfres;
using Syroot.NintenTools.Bfres.GX2;
using Syroot.NintenTools.Bfres.Helpers;
using ResNSW = Syroot.NintenTools.NSW.Bfres;
using Smash_Forge.Rendering;

namespace Smash_Forge
{
    public partial class BFRES : TreeNode
    {
        public List<string> stringContainer = new List<string>();
        public List<FMDL_Model> models = new List<FMDL_Model>();
        public List<Vector3> BoneFixTrans = new List<Vector3>();
        public List<Vector3> BoneFixRot = new List<Vector3>();
        public List<Vector3> BoneFixScale = new List<Vector3>();

        public FSKA SkeletonAnimation;

        public static Shader shader = null;

        public Matrix4[] sb;

        public byte[] BFRESFile;

        public static Dictionary<string, FTEX> FTEXtextures = new Dictionary<string, FTEX>();
        public int AnimationCountTotal;
        public int FSKACount; //Skeleton animations
        public int FVISCount; //Bone Visual animations
        public int FMAACount; //Material animations
        public int FSHACount; //Shape/Vertex animations
        public int FSCNCount; //Scene animations

        //Wii U seperates it's material anims into different sections
        public int FTXPCount; //Texture patterns
        public int FSHUCount; //Shader animations

        public ResNSW.ResFile TargetSwitchBFRES;
        public static bool IsSwitchBFRES;

        //Switch TreeNodes
        public TreeNode TModels = new TreeNode() { Text = "Models", Checked = true };
        public TreeNode TMaterialAnim = new TreeNode() { Text = "Material Animations" };
        public TreeNode TSkeletalAnim = new TreeNode() { Text = "Skeletal Animations" };
        public TreeNode TVisualAnim = new TreeNode() { Text = "Visual Animations" };
        public TreeNode TShapeAnim = new TreeNode() { Text = "Shape Animations" };
        public TreeNode TSceneAnim = new TreeNode() { Text = "Scene Animations" };
        public TreeNode TEmbedded = new TreeNode() { Text = "Embedded Files" };

        public ResFile TargetWiiUBFRES;

        //Wii U TreeNodes
        public TreeNode TTextures = new TreeNode() { Text = "Textures" };
        public TreeNode TShaderparam = new TreeNode() { Text = "Shader Param Animations" };
        public TreeNode TColoranim = new TreeNode() { Text = "Color Animations" };
        public TreeNode TTextureSRT = new TreeNode() { Text = "Texture STR Animations" };
        public TreeNode TTexturePat = new TreeNode() { Text = "Texture Pattern Animations" };
        public TreeNode TBonevisabilty = new TreeNode() { Text = "Bone Visabilty" };


        //Kirby star allies makes it impossible to texture map without decompiling the shaders so i'll do them by texture list
        public List<string> HackyTextureList = new List<string>(new string[] {
           "Alb", "alb", "Base", "base", "bonbon.167300917","Eye.00","EyeIce.00", "FaceDummy", "Eye01.17", "Dee.00",
            "rainbow.758540574", "Mucus._1700670200", "Eye.11", "CapTail00","eye.0","pallet_texture","Mark.930799313","InEye.1767598300","Face.00",
            "ThunderHair_Thunder_BaseColor.1751853236","FireHair_Thunder_BaseColor._162539711","IceHair_Thunder_BaseColor.674061150","BodyEnemy.1866226988",
            "Common_Scroll01._13827715"
        });

        // gl buffer objects
        int vbo_position;
        int ibo_elements;


        public string path = "";
        public static Vector3 position = new Vector3(0, 0, 0);
        public static Vector3 rotation = new Vector3(0, 0, 0);
        public static Vector3 scale = new Vector3(1, 1, 1);

     

        public static List<DefaultBonePos> HackyBoneList = new List<DefaultBonePos>();
        public class DefaultBonePos
        {
            public string Name;
            public Vector3 pos;
            public Vector3 rot;
            public Vector3 scale;
        }
        public static List<NewBonePos> HackyBoneDiffList = new List<NewBonePos>();
        public class NewBonePos
        {
            public string Name;
            public Vector3 pos;
            public Vector3 rot;
            public Vector3 scale;
        }

        #region Render BFRES

        public BFRES()
        {
            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out ibo_elements);

            if (!Runtime.shaders.ContainsKey("BFRES"))
            {
               ShaderTools.CreateShader("BFRES", "/lib/Shader/Legacy/", "/lib/Shader/");
            }
            if (!Runtime.shaders.ContainsKey("BFRES_PBR"))
            {
                ShaderTools.CreateShader("BFRES_PBR", "/lib/Shader/Legacy/", "/lib/Shader/");
            }


            Runtime.shaders["BFRES_PBR"].DisplayCompilationWarning("BFRES_PBR");
            Runtime.shaders["BFRES"].DisplayCompilationWarning("BFRES");
        }



        public BFRES(string fname, byte[] file_data) : this()
        {
            Text = Path.GetFileName(fname);

            SetMarioPosition(fname);


            FileData f = new FileData(file_data);

            BFRESFile = file_data;

            f.seek(4);

            int SwitchCheck = f.readInt();
            if (SwitchCheck == 0x20202020)
            {
                IsSwitchBFRES = true;

                TargetSwitchBFRES = new ResNSW.ResFile(new MemoryStream(file_data));
                path = Text;
                Read(TargetSwitchBFRES, f); //Temp add FileData for now till I parse BNTX with lib
                UpdateVertexData();
                RenderTest();

            }
            else
            {
                IsSwitchBFRES = false;

                TargetWiiUBFRES = new ResFile(new MemoryStream(file_data));
                path = Text;
                Read(TargetWiiUBFRES);
                UpdateVertexData();

            }
        }

        private void SetMarioPosition(string pathBfres)
        {
            string fname = Path.GetFileNameWithoutExtension(pathBfres);

            if (fname.Contains("Mario") && fname.Contains("Face"))
            {
                Console.WriteLine("Positioning Face Mesh.....");
                position = new Vector3(0, 97.0f, 0);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0, 0);
            }
            else if (fname.Contains("Mario") && fname.Contains("Head"))
            {
                Console.WriteLine("Positioning Head Mesh.....");
                position = new Vector3(0, 97.0f, 0);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0, 0);
            }
            else if(fname.Contains("Mario") && fname.Contains("HandL"))
            {
                Console.WriteLine("Positioning Face Mesh.....");
                position = new Vector3(48.877f, 82.551f, -3.3f);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 90f, 0);
            }
            else if(fname.Contains("Mario") && fname.Contains("HandR"))
            {
                Console.WriteLine("Positioning HandR Mesh.....");
                position = new Vector3(-48.877f, 82.551f, -3.3f);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, -90f, 0);
            }
            else if(fname.Contains("Mario") && fname.Contains("Eye"))
            {
                Console.WriteLine("Positioning Eye Mesh.....");
                position = new Vector3(0, 97.0f, 0);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0f, 0);
            }
            else if (fname.Contains("Mario") && fname.Contains("Hair"))
            {
                Console.WriteLine("Positioning Hair Mesh.....");
                position = new Vector3(0, 97.0f, 0);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0f, 0);
            }
            else if(fname.Contains("Mario") && fname.Contains("Skirt"))
            {
                Console.WriteLine("Positioning Skirt Mesh.....");
                position = new Vector3(0, 56.0f, 0);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0f, 0);
            }
            else if (fname.Contains("Mario") && fname.Contains("Tail"))
            {
                Console.WriteLine("Positioning Tail Mesh.....");
                position = new Vector3(0, 56.0f, 0);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0f, 0);
            }
            else if (fname.Contains("Mario") && fname.Contains("Shell"))
            {
                Console.WriteLine("Positioning Tail Mesh.....");
                position = new Vector3(0, 75.0f, 0);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0f, 0);
            }
            else if (fname.Contains("Mario") && fname.Contains("aHakama"))
            {
                Console.WriteLine("Positioning Hakama Mesh.....");
                position = new Vector3(0, 61.0f, -3.0f);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0f, 0);
            }
            else if (fname.Contains("Mario") && fname.Contains("Under"))
            {
                Console.WriteLine("Positioning Under Mesh.....");
                position = new Vector3(0, 56.0f, 0);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0f, 0);
            }
            else if (fname.Contains("Mario") && fname.Contains("PonchoPoncho"))
            {
                Console.WriteLine("Positioning Poncho Mesh.....");
                position = new Vector3(0, 60.5f, -4.0f);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0f, 0);
            }
            else if (fname.Contains("Mario") && fname.Contains("PonchoGuitar"))
            {
                Console.WriteLine("Positioning Guitar Mesh.....");
                position = new Vector3(48.877f, 0, -12.3f);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 90f, 0);
            }
            else
            {
                position = new Vector3(0, 0, 0);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0, 0);
            }
        }

        public void Destroy()
        {
            GL.DeleteBuffer(vbo_position);
            GL.DeleteBuffer(ibo_elements);
        }

        public Matrix4 BonePosExtra;
        public Matrix4 BonePosFix;

        //Transform function for single binded meshes
        //Thanks GDKchan for the function
        public static Vector3 transform_position(Vector3 input, Matrix4 matrix)
        {
            Vector3 output = new Vector3();
            output.X = input.X * matrix.M11 + input.Y * matrix.M21 + input.Z * matrix.M31 + matrix.M41;
            output.Y = input.X * matrix.M12 + input.Y * matrix.M22 + input.Z * matrix.M32 + matrix.M42;
            output.Z = input.X * matrix.M13 + input.Y * matrix.M23 + input.Z * matrix.M33 + matrix.M43;
            return output;
        }

        public void RenderTest()
        {

            //These are used to move models anywhere I want
            Matrix4 positionMat = Matrix4.CreateTranslation(position);
            Matrix4 rotXMat = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X));
            Matrix4 rotYMat = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y));
            Matrix4 rotZMat = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));
            Matrix4 scaleMat = Matrix4.CreateScale(scale);
            BonePosExtra = scaleMat * (rotXMat * rotYMat * rotZMat) * positionMat;



            foreach (FMDL_Model fmdl in models)
            {
                foreach (Bone b in fmdl.skeleton.bones)
                {

                    Matrix4 test2 = b.transform;
                    b.transform = b.transform * BonePosExtra;

/*

                    b.sca = test.ExtractScale();
                    b.rot = test.ExtractRotation();
                    b.pos = test.ExtractTranslation();

                    b.scale[0] = test.ExtractScale().X;
                    b.scale[1] = test.ExtractScale().Y;
                    b.scale[2] = test.ExtractScale().Z;
                    b.rotation[0] = test.ExtractRotation().X;
                    b.rotation[1] = test.ExtractRotation().Y;
                    b.rotation[2] = test.ExtractRotation().Z;
                    b.position[0] = test.ExtractTranslation().X;
                    b.position[1] = test.ExtractTranslation().Y;
                    b.position[2] = test.ExtractTranslation().Z;*/
                }
                

            }
        }

        public void DepthSortMeshes(Vector3 cameraPosition)
        {
            foreach (FMDL_Model fmdl in models)
            {
                List<Mesh> unsortedMeshes = new List<Mesh>();

                foreach (Mesh m in fmdl.Nodes)
                {
                    m.sortingDistance = m.CalculateSortingDistance(cameraPosition);
                    unsortedMeshes.Add(m);
                }

                fmdl.depthSortedMeshes = unsortedMeshes.OrderBy(o => (o.sortingDistance)).ToList();
            }


            // Order by the distance from the camera to the closest point on the bounding sphere. 
            // Positive values are usually closer to camera. Negative values are usually farther away. 
        }

        private void DrawBoundingRadius()
        {
            foreach (FMDL_Model mdl in models)
            {
                foreach (Mesh m in mdl.poly)
                {
                    if (m.IsSelected)
                        GL.Color4(Color.GhostWhite);
                    else
                        GL.Color4(Color.OrangeRed);
                }
            }
        }

        private void DrawBoundingBoxes()
        {
            foreach (FMDL_Model mdl in models)
            {
                foreach (Mesh m in mdl.poly)
                {
                    if (m.IsSelected)
                        GL.Color4(Color.GhostWhite);
                    else
                        GL.Color4(Color.OrangeRed);


                    foreach (Mesh.BoundingBox box in m.boundingBoxes)
                    {
                        if (m.Checked)
                        {
                            RenderTools.DrawRectangularPrism(box.Center, box.Extent.X, box.Extent.Y, box.Extent.Z, true);
                        }
                    }
                }
            }
        }
        public void Render(Matrix4 view)
        {
            if (Runtime.renderPhysicallyBasedRendering == true)
                shader = Runtime.shaders["BFRES_PBR"];
            else
                shader = Runtime.shaders["BFRES"];
            GL.UseProgram(shader.programID);

            shader.enableAttrib();

            if (Runtime.renderBoundingBox)
            {
                DrawBoundingBoxes();
            }

            SetRenderSettings(shader);

            GL.UniformMatrix4(shader.getAttribute("modelview"), false, ref view);

            foreach (FMDL_Model fmdl in models)
            {
                // For proper alpha blending, draw in reverse order and draw opaque objects first. 
                List<Mesh> opaque = new List<Mesh>();
                List<Mesh> transparent = new List<Mesh>();


                //Render Skeleton
                Matrix4[] f = fmdl.skeleton.getShaderMatrix();
                int[] bind = fmdl.Node_Array; //Now bind each bone
                GL.UniformMatrix4(shader.getAttribute("bones"), f.Length, false, ref f[0].Row0.X);
                if (bind.Length != 0)
                {
                    GL.Uniform1(shader.getAttribute("boneList"), bind.Length, ref bind[0]);
                }

                foreach (Mesh m in fmdl.depthSortedMeshes)
                {
                    if (m.isTransparent)
                    {
                        transparent.Add(m);
                    }
                    else
                        opaque.Add(m);
                }

                foreach (Mesh m in opaque)
                {
                          ApplyTransformFix(fmdl, m);

                    if (m.Parent != null && (m.Parent).Checked)
                        DrawMesh(m, shader, m.material);
                }

                foreach (Mesh m in transparent)
                {
                           ApplyTransformFix(fmdl, m);

                    if (((FMDL_Model)m.Parent).Checked)
                    {
                        DrawMesh(m, shader, m.material);
                    }
                }
            }

      
            shader.disableAttrib();
        }

        private void SetRenderSettings(Shader shader)
        {
            GL.Uniform1(shader.getAttribute("renderVertColor"), Runtime.renderVertColor ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderType"), (int)Runtime.renderType);
            GL.Uniform1(shader.getAttribute("uvChannel"), (int)Runtime.uvChannel);
            GL.Uniform1(shader.getAttribute("useNormalMap"), Runtime.renderNormalMap ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderR"), Runtime.renderR ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderG"), Runtime.renderG ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderB"), Runtime.renderB ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderAlpha"), Runtime.renderAlpha ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderFog"), Runtime.renderFog ? 1 : 0);
        }
        private void DrawMesh(Mesh m, Shader shader, MaterialData mat, bool drawSelection = false)
        {
            if (m.lodMeshes[m.DisplayLODIndex].faces.Count <= 3)
                return;

            SetVertexAttributes(m, shader);
            RenderUniformParams(mat, shader, m);
            SetTextureUniforms(mat, m);
            SetAlphaBlending(mat);
            SetAlphaTesting(mat);

            foreach (RenderInfoData r in mat.renderinfo)
            {
                SetFaceCulling(mat, r);
            }

            if (m.Checked)
            {
                if ((m.IsSelected || m.Parent.IsSelected))
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
                        GL.DrawElements(PrimitiveType.Triangles, m.lodMeshes[m.DisplayLODIndex].displayFaceSize, DrawElementsType.UnsignedInt, m.Offset);
                    }
                }

            }
        }

        private static void DrawModelWireframe(Mesh p, Shader shader)
        {
            // use vertex color for wireframe color
            GL.Uniform1(shader.getAttribute("colorOverride"), 1);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.5f);
            GL.DrawElements(PrimitiveType.Triangles, p.lodMeshes[p.DisplayLODIndex].displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Uniform1(shader.getAttribute("colorOverride"), 0);
        }
        private static void DrawModelSelection(Mesh p, Shader shader)
        {
            //This part needs to be reworked for proper outline. Currently would make model disappear

            GL.DrawElements(PrimitiveType.Triangles, p.lodMeshes[p.DisplayLODIndex].displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);

            GL.Enable(EnableCap.StencilTest);
            // use vertex color for wireframe color
            GL.Uniform1(shader.getAttribute("colorOverride"), 1);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.5f);
            GL.DrawElements(PrimitiveType.Triangles, p.lodMeshes[p.DisplayLODIndex].displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Uniform1(shader.getAttribute("colorOverride"), 0);

            GL.Enable(EnableCap.DepthTest);
        }
        private void SetVertexAttributes(Mesh m, Shader shader)
        {
            //Note on these buffers
            // - vBone and vWeight have 2 attributes since bfres has 4 weights/bones per vertice. Additional one can allow up to a max of 8
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.VertexAttribPointer(shader.getAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 0);
            GL.VertexAttribPointer(shader.getAttribute("vNormal"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 12);
            GL.VertexAttribPointer(shader.getAttribute("vTangent"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 24);
            GL.VertexAttribPointer(shader.getAttribute("vBitangent"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 36);
            GL.VertexAttribPointer(shader.getAttribute("vUV0"), 2, VertexAttribPointerType.Float, false, DisplayVertex.Size, 48);
            GL.VertexAttribPointer(shader.getAttribute("vColor"), 4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 56);
            GL.VertexAttribIPointer(shader.getAttribute("vBone"), 4, VertexAttribIntegerType.Int, DisplayVertex.Size, new IntPtr(72));
            GL.VertexAttribPointer(shader.getAttribute("vWeight"), 4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 88);
            GL.VertexAttribPointer(shader.getAttribute("vUV1"), 2, VertexAttribPointerType.Float, false, DisplayVertex.Size, 104);
            GL.VertexAttribPointer(shader.getAttribute("vUV2"), 2, VertexAttribPointerType.Float, false, DisplayVertex.Size, 112);
            GL.VertexAttribPointer(shader.getAttribute("vPosition2"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 124);
            GL.VertexAttribPointer(shader.getAttribute("vPosition3"), 3, VertexAttribPointerType.Float, false, DisplayVertex.Size, 136);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);

            // Disabled these untill I fix transform stuff manually without shaders
            //     GL.VertexAttribPointer(shader.getAttribute("vBone1"),     4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 84);
            //     GL.VertexAttribPointer(shader.getAttribute("vWeight1"),   4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 116);

        }

        //Here I'll use the same enums as NUDs does. BFRES for switch is inconsistent with the strings and data though multiple games and wii u uses flags so it's best to have them all in one
        Dictionary<int, BlendingFactorDest> dstFactor = new Dictionary<int, BlendingFactorDest>(){
                    { 0x01, BlendingFactorDest.OneMinusSrcAlpha},
                    { 0x02, BlendingFactorDest.One},
                    { 0x03, BlendingFactorDest.OneMinusSrcAlpha},
                    { 0x04, BlendingFactorDest.OneMinusConstantAlpha},
                    { 0x05, BlendingFactorDest.ConstantAlpha},
                    { 0x06, BlendingFactorDest.Zero},
        };

        static Dictionary<int, BlendingFactorSrc> srcFactor = new Dictionary<int, BlendingFactorSrc>(){
                    { 0x01, BlendingFactorSrc.SrcAlpha},
                    { 0x02, BlendingFactorSrc.Zero}
        };

        static Dictionary<int, TextureWrapMode> wrapmode = new Dictionary<int, TextureWrapMode>(){
                    { 0x00, TextureWrapMode.Repeat},
                    { 0x01, TextureWrapMode.MirroredRepeat},
                    { 0x02, TextureWrapMode.ClampToEdge},
                    { 0x03, TextureWrapMode.MirroredRepeat},
        };

        public void UpdateVertexData()
        {
            DisplayVertex[] Vertices;
            int[] Faces;

            Console.WriteLine("Updating Verts");
            int poffset = 0;
            int voffset = 0;
            List<DisplayVertex> Vs = new List<DisplayVertex>();
            List<int> Ds = new List<int>();
            foreach (FMDL_Model fmdl in models)
            {
                foreach (Mesh m in fmdl.poly)
                {
                    m.Offset = poffset * 4;
                    List<DisplayVertex> pv = m.CreateDisplayVertices();
                    Vs.AddRange(pv);

                    for (int i = 0; i < m.lodMeshes[m.DisplayLODIndex].displayFaceSize; i++)
                    {
                        Ds.Add(m.display[i] + voffset);
                    }
                    poffset += m.lodMeshes[m.DisplayLODIndex].displayFaceSize;
                    voffset += pv.Count;
                }
            }

            // Binds
            Vertices = Vs.ToArray();
            Faces = Ds.ToArray();

            Console.WriteLine(Faces.Length);

            // Bind only once!
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<DisplayVertex>(BufferTarget.ArrayBuffer, (IntPtr)(Vertices.Length * DisplayVertex.Size), Vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData<int>(BufferTarget.ElementArrayBuffer, (IntPtr)(Faces.Length * sizeof(int)), Faces, BufferUsageHint.StaticDraw);
        }

        private static void ApplyTransformFix(FMDL_Model fmdl, Mesh m)
        {
            GL.Uniform1(shader.getAttribute("NoSkinning"), 0);
            GL.Uniform1(shader.getAttribute("RigidSkinning"), 0);
            //Some objects will have no weights or indices. These will weigh to the bone index in the shape section.
            GL.Uniform1(shader.getAttribute("SingleBoneIndex"), m.boneIndx);

            if (m.VertexSkinCount == 1)
            {
                GL.Uniform1(shader.getAttribute("RigidSkinning"), 1);
            }
            if (m.VertexSkinCount == 0)
            {
                GL.Uniform1(shader.getAttribute("NoSkinning"), 1);

            }
        }

        private static void RenderUniformParams(MaterialData mat, Shader shader, Mesh m)
        {
            GL.Uniform4(shader.getAttribute("SamplerUV1"), new Vector4(1, 1, 0, 0));
            GL.Uniform4(shader.getAttribute("gsys_bake_st0"), new Vector4(1, 1, 0, 0));
            GL.Uniform4(shader.getAttribute("gsys_bake_st1"), new Vector4(1, 1, 0, 0));

            GL.Uniform1(shader.getAttribute("selectedBoneIndex"), Runtime.selectedBoneIndex);

            //This uniform is set so I can do SRT anims.
            SetUnifromData(mat, shader, "tex_mtx0");
        //    SetUnifromData(mat, shader, "tex_mtx1");
       //     SetUnifromData(mat, shader, "tex_mtx2");

            //This uniform variable shifts first bake map coords (MK8, Spatoon 1/2, ect)
            SetUnifromData(mat, shader, "gsys_bake_st0");
            //This uniform variable shifts second bake map coords (MK8, Spatoon 1/2, ect)
            SetUnifromData(mat, shader, "gsys_bake_st1");
            SetUnifromData(mat, shader, "normal_map_weight");
            SetUnifromData(mat, shader, "ao_density");
            SetUnifromData(mat, shader, "base_color_mul_color");
            SetUnifromData(mat, shader, "emission_color");
            

            //   Shader option data
            // These enable certain effects
            //They can enable texture maps. However due to these being varied between games, doing by samplers is more simple. 

            //This uniform sets normal maps for BOTW to use second UV channel
            SetUnifromData(mat, shader, "uking_texture2_texcoord");
            //Sets shadow type
            //0 = Ambient occusion bake map
            //1 = Shadow 
            //2 = Shadow + Ambient occusion map
            SetUnifromData(mat, shader, "bake_shadow_type");

            SetUnifromData(mat, shader, "enable_fresnel");
            SetUnifromData(mat, shader, "enable_emission");
            

            ShaderTools.BoolToIntShaderUniform(shader, m.isTransparent, "isTransparent");
        }

        private static void SetUnifromData(MaterialData mat, Shader shader, string propertyName)
        {
            //Note uniform data has so many types so it's messy atm

            if (mat.shaderassign.options.ContainsKey(propertyName))
            {
                float value = float.Parse(mat.shaderassign.options[propertyName]);

                GL.Uniform1(shader.getAttribute(propertyName), value);
            }

            if (mat.matparam.ContainsKey(propertyName))
            {
                if (mat.matparam[propertyName].Type == ShaderParamType.Float)
                {
                    if (mat.anims.ContainsKey(propertyName))
                        mat.matparam[propertyName].Value_float = mat.anims[propertyName][0];
                    GL.Uniform1(shader.getAttribute(propertyName), mat.matparam[propertyName].Value_float);
                }
                if (mat.matparam[propertyName].Type == ShaderParamType.Float2)
                {
                    if (mat.anims.ContainsKey(propertyName))
                    {
                           mat.matparam[propertyName].Value_float2 = new Vector2(
                                        mat.anims[propertyName][0], mat.anims[propertyName][1]);
                    }

                    GL.Uniform2(shader.getAttribute(propertyName), mat.matparam[propertyName].Value_float2);
                }
                if (mat.matparam[propertyName].Type == ShaderParamType.Float3)
                {
                    if (mat.anims.ContainsKey(propertyName))
                    {
                            mat.matparam[propertyName].Value_float3 = new Vector3(
                                         mat.anims[propertyName][0], mat.anims[propertyName][1],
                                         mat.anims[propertyName][2]);
                    }

                    GL.Uniform3(shader.getAttribute(propertyName), mat.matparam[propertyName].Value_float3);
                }
                if (mat.matparam[propertyName].Type == ShaderParamType.Float4)
                {
                    if (mat.anims.ContainsKey(propertyName))
                    {
                        mat.matparam[propertyName].Value_float4 = new Vector4(
                                     mat.anims[propertyName][0], mat.anims[propertyName][1],
                                     mat.anims[propertyName][2], mat.anims[propertyName][3]);
                    }

                    GL.Uniform4(shader.getAttribute(propertyName), mat.matparam[propertyName].Value_float4);
                }
                if (mat.matparam[propertyName].Type == ShaderParamType.TexSrt)
                {
                    // Vector 2 Scale
                    // 1 roation float
                    // Vector2 translate
                    ShaderParam.TextureSRT texSRT = mat.matparam[propertyName].Value_TexSrt;

                    GL.Uniform2(shader.getAttribute("SRT_Scale"), texSRT.scale);
                    GL.Uniform1(shader.getAttribute("SRT_Rotate"), texSRT.rotate);
                    GL.Uniform2(shader.getAttribute("SRT_Translate"), texSRT.translate);
                }
            }         
        }

        private void SetAlphaBlending(MaterialData material)
        {
            GL.Enable(EnableCap.Blend);
            BlendingFactorSrc blendSrc = srcFactor.Keys.Contains(material.srcFactor) ? srcFactor[material.srcFactor] : BlendingFactorSrc.SrcAlpha;
            BlendingFactorDest blendDst = dstFactor.Keys.Contains(material.dstFactor) ? dstFactor[material.dstFactor] : BlendingFactorDest.OneMinusSrcAlpha;
            GL.BlendFuncSeparate(blendSrc, blendDst, BlendingFactorSrc.One, BlendingFactorDest.One);
            GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
            if (material.srcFactor == 0 && material.dstFactor == 0)
            {
                GL.Disable(EnableCap.Blend);
            }
        }

        private static void SetAlphaTesting(MaterialData material)
        {
            GL.Enable(EnableCap.AlphaTest);
            if (material.alphaTest == 0)
                GL.Disable(EnableCap.AlphaTest);

            float refAlpha = material.RefAlpha / 255f;
            GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);
            switch (material.alphaFunction)
            {
                case 0x0:
                    GL.AlphaFunc(AlphaFunction.Never, refAlpha);
                    break;
                case 0x4:
                    GL.AlphaFunc(AlphaFunction.Gequal, refAlpha);
                    break;
                case 0x6:
                    GL.AlphaFunc(AlphaFunction.Gequal, refAlpha);
                    break;
            }
        }

        private static void SetFaceCulling(MaterialData mat, RenderInfoData r)
        {
            GL.Disable(EnableCap.CullFace); //Set as enabled by default unless specified otherwise.
            switch (r.Name)
            {
                case "gsys_render_state_display_face":
                    if (r.Value_String == "front")
                    {
                        GL.CullFace(CullFaceMode.Back);
                    }
                    else if (r.Value_String == "back")
                    {
                        GL.CullFace(CullFaceMode.Front);
                    }
                    else if (r.Value_String == "both")
                    {
                        GL.Disable(EnableCap.CullFace);
                    }
                    else if (r.Value_String == "none")
                    {
                        GL.CullFace(CullFaceMode.FrontAndBack);
                    }
                    break;
            }
        }
 
        private static void SetDefaultTextureAttributes(MaterialData mat)
        {            
            ShaderTools.BoolToIntShaderUniform(shader, mat.HasDiffuseMap, "HasDiffuseLayer");
            ShaderTools.BoolToIntShaderUniform(shader, mat.HasDiffuseLayer, "HasDiffuseLayer");
            ShaderTools.BoolToIntShaderUniform(shader, mat.HasNormalMap, "HasNormalMap");
            ShaderTools.BoolToIntShaderUniform(shader, mat.HasEmissionMap, "HasEmissionMap");
            ShaderTools.BoolToIntShaderUniform(shader, mat.HasLightMap, "HasLightMap");
            ShaderTools.BoolToIntShaderUniform(shader, mat.HasShadowMap, "HasShadowMap");
            ShaderTools.BoolToIntShaderUniform(shader, mat.HasSpecularMap, "HasSpecularMap");
            ShaderTools.BoolToIntShaderUniform(shader, mat.HasTeamColorMap, "HasTeamColorMap");
            ShaderTools.BoolToIntShaderUniform(shader, mat.HasTransparencyMap, "hasDummyRamp");

            //Unused atm untill I do PBR shader
            ShaderTools.BoolToIntShaderUniform(shader, mat.HasMetalnessMap, "HasMetalnessMap");
            ShaderTools.BoolToIntShaderUniform(shader, mat.HasRoughnessMap, "HasRoughnessMap");
        }

        private static void SetTextureUniforms(MaterialData mat, Mesh m)
        {
            SetDefaultTextureAttributes(mat);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.defaultTex);

            GL.ActiveTexture(TextureUnit.Texture10);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.uvTestPattern);
            GL.Uniform1(shader.getAttribute("UVTestPattern"), 10);

            GL.ActiveTexture(TextureUnit.Texture11);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.boneWeightGradient);
            GL.Uniform1(shader.getAttribute("weightRamp1"), 11);

            GL.ActiveTexture(TextureUnit.Texture12);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.boneWeightGradient2);
            GL.Uniform1(shader.getAttribute("weightRamp2"), 12);


            GL.Uniform1(shader.getAttribute("nrm"), 0);
            GL.Uniform1(shader.getAttribute("BakeShadowMap"), 0);

            //So this loops through each type and maps by tex hash. This is done because there is no particular order in the list

            foreach (MatTexture matex in mat.textures)
            {
                if (matex.Type == MatTexture.TextureType.Diffuse)    
                    TextureUniform(shader, mat, mat.HasDiffuseMap, "tex0", matex);              
                else if (matex.Type == MatTexture.TextureType.Normal)          
                    TextureUniform(shader, mat, mat.HasNormalMap, "nrm", matex);
                else if (matex.Type == MatTexture.TextureType.Emission)
                    TextureUniform(shader, mat, mat.HasEmissionMap, "EmissionMap", matex);
                else if (matex.Type == MatTexture.TextureType.Specular)
                    TextureUniform(shader, mat, mat.HasSpecularMap, "SpecularMap", matex);
                else if (matex.Type == MatTexture.TextureType.Shadow)
                    TextureUniform(shader, mat, mat.HasShadowMap, "BakeShadowMap", matex);
                else if (matex.Type == MatTexture.TextureType.Light)
                    TextureUniform(shader, mat, mat.HasLightMap, "BakeLightMap", matex);
                else if (matex.Type == MatTexture.TextureType.Metalness)
                    TextureUniform(shader, mat, mat.HasMetalnessMap, "MetalnessMap", matex);
                else if (matex.Type == MatTexture.TextureType.Roughness)
                    TextureUniform(shader, mat, mat.HasRoughnessMap, "RoughnessMap", matex);
                else if (matex.Type == MatTexture.TextureType.TeamColor)
                    TextureUniform(shader, mat, mat.HasTeamColorMap, "TeamColorMap", matex);
                else if (matex.Type == MatTexture.TextureType.Transparency)
                    TextureUniform(shader, mat, mat.HasTransparencyMap, "TransparencyMap", matex);
                else if (matex.Type == MatTexture.TextureType.DiffuseLayer2)
                    TextureUniform(shader, mat, mat.HasDiffuseLayer, "DiffuseLayer", matex);
            }
        }

        private static void TextureUniform(Shader shader, MaterialData mat, bool hasTex, string name, MatTexture mattex)
        {
            // Bind the texture and create the uniform if the material has the right textures. 
            if (hasTex)
            {
                GL.Uniform1(shader.getAttribute(name), BindTexture(mattex));
            }
        }

        public static int BindTexture(MatTexture tex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + tex.hash + 1);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.defaultTex);

            if (IsSwitchBFRES == true)
            {
                if (BNTX.textured.ContainsKey(tex.Name))
                {
                    BindBRTTexture(tex, BNTX.textured[tex.Name].texture.display);
                }
            }
            else
            {
                if (FTEXtextures.ContainsKey(tex.Name))
                {
                    BindBRTTexture(tex, FTEXtextures[tex.Name].texture.display);
                }
            }
         
            return tex.hash + 1;
        }

        private static void BindBRTTexture(MatTexture tex, int texid)
        {
            //   GL.ActiveTexture(TextureUnit.Texture0 + texid);
            GL.BindTexture(TextureTarget.Texture2D, texid);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapmode[tex.wrapModeS]);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapmode[tex.wrapModeT]);

            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, 0.0f);
        }


        //This mta function is pretty much for all animations in a bfres besides skeleton animations. 
        public void ApplyMta(MTA m, int frame)
        {
            foreach (MatAnimEntry matEntry in m.matEntries)
            {
                foreach (FMDL_Model mdl in models)
                {
                    foreach (Mesh mesh in mdl.poly)
                    {
                        MaterialData material = mesh.material;
                        if (material.Name == matEntry.Text)
                        {
                            //Loop through each curve
                            //Curves contain data such as
                            // - Single uniform data
                            // - Texture maps to switch
                            // - SRT constants
                            // - Vis mat anims

                         

                            int FrameRate = 60;
                            int frm = (int)((frame * 60 / FrameRate) % (m.FrameCount));

                            if (matEntry.matCurves != null)
                            {
                                foreach (MatAnimData md in matEntry.matCurves)
                                {
                                    if (frm == md.Frame)
                                    { 
                                        //If it's a texture pattern one set the texture by the frame
                                        if (md.Pat0Tex != null)
                                        {
                                            foreach (string Sampler in mesh.material.Samplers.Keys)
                                            {
                                                if (md.SamplerName == Sampler)
                                                {
                                                    mesh.material.textures[mesh.material.Samplers[md.SamplerName]].Name = md.Pat0Tex;
                                                }
                                            }
                                        }
                                        //Set key data used by uniform and SRT data
                                        if (md.keys != null)
                                        {

                                        }
                                    }
                                }
                            }
                        }

                        //For this do shape/morph animations
                        if (mesh.Text == matEntry.Text)
                        {

                            int FrameRate = 60;
                            int frm = (int)((frame * 60 / FrameRate) % (m.FrameCount));

                            if (matEntry.matCurves != null)
                            {
                                foreach (MatAnimData md in matEntry.matCurves)
                                {
                                    foreach (int f in md.Frames)
                                    {
                                        if (frm == f)
                                        {
                                            if (md.keys != null)
                                            {
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Find a matching bone if the matEntry uses bones instead of materials
                        //For visual anims simply get the mesh that is linked to the material and hide it
                        if (mdl.skeleton.bones[mesh.boneIndx].Text == matEntry.Text)
                        {
                            int FrameRate = 60;
                            int frm = (int)((frame * 60 / FrameRate) % (m.FrameCount));

                            if (matEntry.matCurves != null)
                            {
                                foreach (MatAnimData md in matEntry.matCurves)
                                {
                                    if (frm == md.Frame)
                                    {
                                        if (frm == md.Frame)
                                        {
                                            if (md.VIS_State == false)
                                            {
                                                mesh.Checked = false;
                                            }
                                            if (md.VIS_State == true)
                                            {
                                                mesh.Checked = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (string name in mesh.BoneIndexList.Keys)
                            {
                                if (name == matEntry.Text)
                                {
                                    int FrameRate = 60;
                                    int frm = (int)((frame * 60 / FrameRate) % (m.FrameCount));

                                    if (matEntry.matCurves != null)
                                    {
                                        foreach (MatAnimData md in matEntry.matCurves)
                                        {
                                            if (frm == md.Frame)
                                            {
                                                if (md.VIS_State == false)
                                                {
                                                    mesh.Checked = false;
                                                }
                                                if (md.VIS_State == true)
                                                {
                                                    mesh.Checked = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /*  foreach (VisEntry e in m.visEntries)
              {
                  int state = e.getState(frame);
                  foreach (Mesh me in Nodes)
                  {
                      if (me.Text.Equals(e.name))
                      {
                          if (state == 0)
                          {
                              me.Checked = false;
                          }
                          else
                          {
                              me.Checked = true;
                          }
                          break;
                      }
                  }
              }*/
        }

        #endregion

        #region BFRES Data (Wii U and Switch)

        public class Vertex
        {
            public Vector3 pos = new Vector3(0);
            public Vector3 nrm = new Vector3(0);
            public Vector4 col = new Vector4(1);
            public Vector2 uv0 = new Vector2(0);
            public Vector2 uv1 = new Vector2(0);
            public Vector2 uv2 = new Vector2(0);
            public Vector4 tan = new Vector4(0);
            public Vector4 bitan = new Vector4(0);

            public List<int> boneIds = new List<int>();
            public List<float> boneWeights = new List<float>();

            //For vertex morphing 
            public Vector3 pos1 = new Vector3();
            public Vector3 pos2 = new Vector3();
        }

        public struct DisplayVertex
        {
            // Used for rendering.
            public Vector3 pos;
            public Vector3 nrm;
            public Vector3 tan;
            public Vector3 bit;
            public Vector2 uv;
            public Vector4 col;
            public Vector4 node;
            public Vector4 weight;
            public Vector2 uv2;
            public Vector2 uv3;
            public Vector3 pos1;
            public Vector3 pos2;



            public static int Size = 4 * (3 + 3 + 3 + 3 + 2 + 4 + 4 + 4 + 2 + 2 + 3 + 3);
        }

        public class Mesh : TreeNode
        {
            public List<Vertex> vertices = new List<Vertex>();
            public List<int> texHashs = new List<int>();
            public string name;
            public int MaterialIndex;
            public List<string> TextureMapTypes = new List<string>();
            public int VertexSkinCount;
            public int[] BoneFixNode;
            public int boneIndx;
            public int fmdlIndx; //Just so we know what fmdl it's in
            public uint[] indicesArray;

            public bool isTransparent = false;

            public MaterialData material = new MaterialData();

            public int BoundingCount;
            public List<float> radius = new List<float>();
            public List<BoundingBox> boundingBoxes = new List<BoundingBox>();
            public Dictionary<string, int> BoneIndexList = new Dictionary<string, int>();

            public int DisplayLODIndex = 0;
            public List<LOD_Mesh> lodMeshes = new List<LOD_Mesh>();
            public class LOD_Mesh
            {
                public int index = 0;
                public int strip = 0x40;
                public int displayFaceSize = 0;

                private List<int> Faces = new List<int>();
                public List<int> faces
                {
                    get
                    { return Faces; }
                }
                public override string ToString()
                {
                    return "LOD Mesh " + index;
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

            public class BoundingBox
            {
                public Vector3 Center;
                public Vector3 Extent;
            }

            public float sortingDistance = 0;

   

            public Mesh()
            {
                Checked = true;
                ImageKey = "mesh";
                SelectedImageKey = "mesh";
            }
            // for drawing
            public int[] display;
            public int[] selectedVerts;
            public int Offset; // For Rendering

            //Store list of all existing vertex attribute. This is for saving
            public List<VertexAttribute> vertexAttributes = new List<VertexAttribute>();
            public class VertexAttribute
            {
                public string Name;
                public Syroot.NintenTools.NSW.Bfres.GFX.AttribFormat Format;

                public override string ToString()
                {
                    return Name;
                }
            }

            public float CalculateSortingDistance(Vector3 cameraPosition)
            {
                BoundingBox box = new BoundingBox();

                Vector3 distanceVector = new Vector3(cameraPosition - box.Center);
                return distanceVector.Length + radius[0];
            }     

            public List<DisplayVertex> CreateDisplayVertices()
            {
                // rearrange faces

                display = lodMeshes[DisplayLODIndex].getDisplayFace().ToArray();

                List<DisplayVertex> displayVertList = new List<DisplayVertex>();

                if (lodMeshes[DisplayLODIndex].faces.Count <= 3)
                    return displayVertList;

                foreach (Vertex v in vertices)
                {
                    DisplayVertex displayVert = new DisplayVertex()
                    {
                        pos = v.pos,
                        nrm = v.nrm,
                        tan = v.tan.Xyz,
                        bit = v.bitan.Xyz,
                        col = v.col / 127,
                        uv = v.uv0,
                        uv2 = v.uv1,
                        uv3 = v.uv2,
                        node = new Vector4(
                            v.boneIds.Count > 0 ? v.boneIds[0] : -1,
                            v.boneIds.Count > 1 ? v.boneIds[1] : -1,
                            v.boneIds.Count > 2 ? v.boneIds[2] : -1,
                            v.boneIds.Count > 3 ? v.boneIds[3] : -1),
                        weight = new Vector4(
                            v.boneWeights.Count > 0 ? v.boneWeights[0] : 0,
                            v.boneWeights.Count > 1 ? v.boneWeights[1] : 0,
                            v.boneWeights.Count > 2 ? v.boneWeights[2] : 0,
                            v.boneWeights.Count > 3 ? v.boneWeights[3] : 0),
                    };


                    /*   Console.WriteLine($"---------------------------------------------------------------------------------------");
                       Console.WriteLine($"Position   {displayVert.pos.X} {displayVert.pos.Y} {displayVert.pos.Z}");
                       Console.WriteLine($"Normal     {displayVert.nrm.X} {displayVert.nrm.Y} {displayVert.nrm.Z}");
                       Console.WriteLine($"Binormal   {displayVert.bit.X} {displayVert.bit.Y} {displayVert.bit.Z}");
                       Console.WriteLine($"Tanget     {displayVert.tan.X} {displayVert.tan.Y} {displayVert.tan.Z}");
                       Console.WriteLine($"Color      {displayVert.col.X} {displayVert.col.Y} {displayVert.col.Z} {displayVert.col.W}");
                       Console.WriteLine($"UV Layer 1 {displayVert.uv.X} {displayVert.uv.Y}");
                       Console.WriteLine($"UV Layer 2 {displayVert.uv2.X} {displayVert.uv2.Y}");
                       Console.WriteLine($"UV Layer 3 {displayVert.uv3.X} {displayVert.uv3.Y}");
                       Console.WriteLine($"Bone Index {displayVert.node.X} {displayVert.node.Y} {displayVert.node.Z} {displayVert.node.W}");
                       Console.WriteLine($"Weights    {displayVert.weight.X} {displayVert.weight.Y} {displayVert.weight.Z} {displayVert.weight.W}");
                       Console.WriteLine($"---------------------------------------------------------------------------------------");*/



                    displayVertList.Add(displayVert);
                }

                selectedVerts = new int[displayVertList.Count];
                return displayVertList;
            }

            public void SingleBindMesh()
            {
                MeshBoneList mshbl = new MeshBoneList();
                mshbl.SetMeshBoneList(((FMDL_Model)Parent), this, true);
                mshbl.Show();
            }
 
            public void ExportMaterials2XML()
            {
                Console.WriteLine("Wring XML");
                WriteFMATXML(material, this);
            }
            public void CopyUVChannel2()
            {
                foreach (Vertex v in vertices)
                {
                    v.uv1 = v.uv0;
                }

                //Reset bake coordinates. If you are using the first uv channel as the second these should be default values
                if (material.matparam.ContainsKey("gsys_bake_st0"))
                    material.matparam["gsys_bake_st0"].Value_float4 = new Vector4(1, 1, 0, 0);
                if (material.matparam.ContainsKey("gsys_bake_st1"))
                    material.matparam["gsys_bake_st1"].Value_float4 = new Vector4(1, 1, 0, 0);

            }

            public void CalculateTangentBitangent()
            {
                List<int> f = lodMeshes[DisplayLODIndex].getDisplayFace();
                Vector3[] tanArray = new Vector3[vertices.Count];
                Vector3[] bitanArray = new Vector3[vertices.Count];

                CalculateTanBitanArrays(f, tanArray, bitanArray);
                ApplyTanBitanArray(tanArray, bitanArray);
            }

            private void ApplyTanBitanArray(Vector3[] tanArray, Vector3[] bitanArray)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vertex v = vertices[i];
                    Vector3 newTan = tanArray[i];
                    Vector3 newBitan = bitanArray[i];

                    // The tangent and bitangent should be orthogonal to the normal. 
                    // Bitangents are not calculated with a cross product to prevent flipped shading  with mirrored normal maps.
                    v.tan = new Vector4(Vector3.Normalize(newTan - v.nrm * Vector3.Dot(v.nrm, newTan)), 1);
                    v.bitan = new Vector4(Vector3.Normalize(newBitan - v.nrm * Vector3.Dot(v.nrm, newBitan)), 1);
                    v.bitan *= -1;
                }
            }

            private void CalculateTanBitanArrays(List<int> faces, Vector3[] tanArray, Vector3[] bitanArray)
            {
                for (int i = 0; i < lodMeshes[DisplayLODIndex].displayFaceSize; i += 3)
                {
                    Vertex v1 = vertices[faces[i]];
                    Vertex v2 = vertices[faces[i + 1]];
                    Vertex v3 = vertices[faces[i + 2]];

                    bool UseUVLayer2 = false;

                    //for BOTW if it uses UV layer 2 for normal maps use second UV map
                    if (material.shaderassign.options.ContainsKey("uking_texture2_texcoord"))
                    {
                        float value = float.Parse(material.shaderassign.options["uking_texture2_texcoord"]);

                        if (value == 1)
                        {
                            UseUVLayer2 = true;
                        }
                    }

                    float x1 = v2.pos.X - v1.pos.X;
                    float x2 = v3.pos.X - v1.pos.X;
                    float y1 = v2.pos.Y - v1.pos.Y;
                    float y2 = v3.pos.Y - v1.pos.Y;
                    float z1 = v2.pos.Z - v1.pos.Z;
                    float z2 = v3.pos.Z - v1.pos.Z;

                    float s1, s2, t1, t2;
                    if (UseUVLayer2)
                    {
                        s1 = v2.uv1.X - v1.uv1.X;
                        s2 = v3.uv1.X - v1.uv1.X;
                        t1 = v2.uv1.Y - v1.uv1.Y;
                        t2 = v3.uv1.Y - v1.uv1.Y;
                    }
                    else
                    {
                     
                        s1 = v2.uv0.X - v1.uv0.X;
                        s2 = v3.uv0.X - v1.uv0.X;
                        t1 = v2.uv0.Y - v1.uv0.Y;
                        t2 = v3.uv0.Y - v1.uv0.Y;
                    }
           

                    float div = (s1 * t2 - s2 * t1);
                    float r = 1.0f / div;

                    // Fix +/- infinity from division by 0.
                    if (r == float.PositiveInfinity || r == float.NegativeInfinity)
                        r = 1.0f;

                    float sX = t2 * x1 - t1 * x2;
                    float sY = t2 * y1 - t1 * y2;
                    float sZ = t2 * z1 - t1 * z2;
                    Vector3 s = new Vector3(sX, sY, sZ) * r;

                    float tX = s1 * x2 - s2 * x1;
                    float tY = s1 * y2 - s2 * y1;
                    float tZ = s1 * z2 - s2 * z1;
                    Vector3 t = new Vector3(tX, tY, tZ) * r;

                    // Prevents black tangents or bitangents due to having vertices with the same UV coordinates. 
                    float delta = 0.00075f;
                    bool sameU, sameV;
                    if (UseUVLayer2)
                    {
                        sameU = (Math.Abs(v1.uv1.X - v2.uv1.X) < delta) && (Math.Abs(v2.uv1.X - v3.uv1.X) < delta);
                        sameV = (Math.Abs(v1.uv1.Y - v2.uv1.Y) < delta) && (Math.Abs(v2.uv1.Y - v3.uv1.Y) < delta);
                    }
                    else
                    {
                        sameU = (Math.Abs(v1.uv0.X - v2.uv0.X) < delta) && (Math.Abs(v2.uv0.X - v3.uv0.X) < delta);
                        sameV = (Math.Abs(v1.uv0.Y - v2.uv0.Y) < delta) && (Math.Abs(v2.uv0.Y - v3.uv0.Y) < delta);
                    }
                
                    if (sameU || sameV)
                    {
                        // Let's pick some arbitrary tangent vectors.
                        s = new Vector3(1, 0, 0);
                        t = new Vector3(0, 1, 0);
                    }

                    // Average tangents and bitangents.
                    tanArray[faces[i]] += s;
                    tanArray[faces[i + 1]] += s;
                    tanArray[faces[i + 2]] += s;

                    bitanArray[faces[i]] += t;
                    bitanArray[faces[i + 1]] += t;
                    bitanArray[faces[i + 2]] += t;
                }
            }

            public void GenerateBoundingBoxes()
            {
                //Set center and extent
                //Each sub mesh has their own bounding + for BOTW/switch has per LOD mesh too

                for (int i = 0; i < BoundingCount; i++)
                {

            



                    boundingBoxes[i].Center = new Vector3(0, 0, 0);
                    boundingBoxes[i].Extent = new Vector3(0, 0, 0);

                }
            }

            public void SmoothNormals()
            {
                Vector3[] normals = new Vector3[vertices.Count];

                List<int> f = lodMeshes[DisplayLODIndex].getDisplayFace();

                for (int i = 0; i < lodMeshes[DisplayLODIndex].displayFaceSize; i += 3)
                {
                    Vertex v1 = vertices[f[i]];
                    Vertex v2 = vertices[f[i + 1]];
                    Vertex v3 = vertices[f[i + 2]];
                    Vector3 nrm = CalculateNormal(v1, v2, v3);

                    normals[f[i + 0]] += nrm;
                    normals[f[i + 1]] += nrm;
                    normals[f[i + 2]] += nrm;
                }

                for (int i = 0; i < normals.Length; i++)
                    vertices[i].nrm = normals[i].Normalized();

                // Compare each vertex with all the remaining vertices. This might skip some.
                for (int i = 0; i < vertices.Count; i++)
                {
                    Vertex v = vertices[i];

                    for (int j = i + 1; j < vertices.Count; j++)
                    {
                        Vertex v2 = vertices[j];

                        if (v == v2)
                            continue;
                        float dis = (float)Math.Sqrt(Math.Pow(v.pos.X - v2.pos.X, 2) + Math.Pow(v.pos.Y - v2.pos.Y, 2) + Math.Pow(v.pos.Z - v2.pos.Z, 2));
                        if (dis <= 0f) // Extra smooth
                        {
                            Vector3 nn = ((v2.nrm + v.nrm) / 2).Normalized();
                            v.nrm = nn;
                            v2.nrm = nn;
                        }
                    }
                }
            }

            public void CalculateNormals()
            {
                Vector3[] normals = new Vector3[vertices.Count];

                for (int i = 0; i < normals.Length; i++)
                    normals[i] = new Vector3(0, 0, 0);

                List<int> f = lodMeshes[DisplayLODIndex].getDisplayFace();

                for (int i = 0; i < lodMeshes[DisplayLODIndex].displayFaceSize; i += 3)
                {
                    Vertex v1 = vertices[f[i]];
                    Vertex v2 = vertices[f[i + 1]];
                    Vertex v3 = vertices[f[i + 2]];
                    Vector3 nrm = CalculateNormal(v1, v2, v3);

                    normals[f[i + 0]] += nrm * (nrm.Length / 2);
                    normals[f[i + 1]] += nrm * (nrm.Length / 2);
                    normals[f[i + 2]] += nrm * (nrm.Length / 2);
                }

                for (int i = 0; i < normals.Length; i++)
                    vertices[i].nrm = normals[i].Normalized();
            }

            private Vector3 CalculateNormal(Vertex v1, Vertex v2, Vertex v3)
            {
                Vector3 U = v2.pos - v1.pos;
                Vector3 V = v3.pos - v1.pos;

                // Don't normalize here, so surface area can be calculated. 
                return Vector3.Cross(U, V);
            }

            public void SetVertexColor(Vector4 intColor)
            {
                // (127, 127, 127, 255) is white.
                foreach (Vertex v in vertices)
                {
                    v.col = intColor;
                }
            }

            public void UpdateTexIDs()
            {
                texHashs.Clear();
                foreach (var tex in material.textures)
                {
                    try
                    {
                        texHashs.Add(BNTX.textured[tex.Name].texture.display);
                    }
                    catch
                    {
                        texHashs.Add(0);
                    }
                }
            }
        }

        public class MaterialData
        {
            public string Name;

            public enum AlphaFunction
            {
                Never = 0x0,
                GequalRefAlpha1 = 0x4,
                GequalRefAlpha2 = 0x6
            }

            public Dictionary<string, float[]> anims = new Dictionary<string, float[]>();
            public Dictionary<string, int> Samplers = new Dictionary<string, int>();
            public List<MatTexture> textures = new List<MatTexture>();
            public List<RenderInfoData> renderinfo = new List<RenderInfoData>();
            public List<SamplerInfo> samplerinfo = new List<SamplerInfo>();
            public Dictionary<string, ShaderParam> matparam = new Dictionary<string, ShaderParam>();

            public ShaderAssign shaderassign;

            public class ShaderAssign
            {
                public string ShaderModel = "";
                public string ShaderArchive = "";
                

                public Dictionary<string, string> options = new Dictionary<string, string>();
                public Dictionary<string, string> samplers = new Dictionary<string, string>();
                public Dictionary<string, string> attributes = new Dictionary<string, string>();
            }


            public int blendMode = 0;
            public int dstFactor = 0;
            public int srcFactor = 0;
            public int alphaTest = 0;
            public int alphaFunction = 0;

            public int RefAlpha = 0;
            public int cullMode = 0;


            // Texture Maps
            public bool HasDiffuseMap = false;
            public bool HasNormalMap = false;
            public bool HasSpecularMap = false;
            public bool HasEmissionMap = false;
            public bool HasDiffuseLayer = false;
            public bool HasTeamColorMap = false; //Splatoon uses this (TLC)
            public bool HasTransparencyMap = false;
            public bool HasShadowMap = false;
            public bool HasAmbientOcclusionMap = false;
            public bool HasLightMap = false;

            //PBR (Switch) data
            public bool HasMetalnessMap = false;
            public bool HasRoughnessMap = false;

            public MaterialFlags IsVisable = MaterialFlags.Visible;

            public Material Clone()
            {
                Material m = new Material();

                m.Flags = IsVisable;
                m.Name = Name;
                m.TextureRefs = new List<TextureRef>();
                m.RenderInfos = new ResDict<RenderInfo>();
                m.Samplers = new ResDict<Sampler>();
          //      m.ShaderAssign = new ShaderAssign();
                m.ShaderParamData = new byte[0];
                m.ShaderParams = new ResDict<Syroot.NintenTools.Bfres.ShaderParam>();
                m.UserData = new ResDict<UserData>();
                m.VolatileFlags = new byte[0];


                foreach (MatTexture tex in textures)
                {
                    TextureRef texture = new TextureRef();
                    texture.Name = tex.Name;
                    texture.Texture = new Texture();

                    m.TextureRefs.Add(texture);
                }

                return m;
            }

        }

        public class MatTexture
        {
            public int hash;
            public int mapMode = 0;
            public int wrapModeS = 1;
            public int wrapModeT = 1;
            public int wrapModeW = 1; //Used for 3D textures
            public int minFilter = 3;
            public int magFilter = 2;
            public int mipDetail = 6;
            public string Name;
            public string SamplerName;
            //Note samplers will get converted to another sampler type sometimes in the shader assign section
            //Use this string if not empty for our bfres fragment shader to produce the accurate affects
            //An example of a conversion maybe be like a1 - t0 so texture gets used as a transparent map/alpha texture
            public string FragShaderSampler = ""; 


            public TextureType Type;

            //An enum for the assumed texture type by sampler
            //Many games have a consistant type of samplers and type. _a0 for diffuse, _n0 for normal, ect
            public enum TextureType
            {
                Unknown = 0,
                Diffuse = 1,
                Normal = 2,
                Specular = 3,
                Emission = 4,
                DiffuseLayer2 = 5,
                TeamColor = 6,
                Transparency = 7,
                Shadow = 8,
                AO = 9,
                Light = 10,
                Roughness = 11,
                Metalness = 12,
                MRA = 13, //Combined pbr texture HAL uses for KSA
            }

            public MatTexture()
            {

            }

            public MatTexture(int hash)
            {
                this.hash = hash;
            }

            public MatTexture Clone()
            {
                MatTexture t = new MatTexture();
                t.hash = hash;
                t.mapMode = mapMode;
                t.wrapModeS = wrapModeS;
                t.wrapModeT = wrapModeT;
                t.minFilter = minFilter;
                t.magFilter = magFilter;
                t.mipDetail = mipDetail;
                return t;
            }

            public static MatTexture GetDefault()
            {
                MatTexture defaultTex = new MatTexture((int)DummyTextures.DummyRamp);
                return defaultTex;
            }
        }

        public enum DummyTextures
        {
            DummyRamp = 0x10080000,
        }

        public class ShaderParam
        {
            public ShaderParamType Type;

            public Vector4 Value_float4;
            public Vector3 Value_float3;
            public Vector2 Value_float2;
            public float Value_float;
            public bool Value_bool;
            public TextureSRT Value_TexSrt;
            public string Name = ""; //Used for lists
            public float[] Value_float4x4 = new float[16];
            public float[] Value_float2x2 = new float[8];
            public float[] Value_float2x3 = new float[12];
            public uint Value_UInt;
            public bool Value_Bool;         

            public override string ToString()
            {
                return Name;
            }

            public class TextureSRT
            {
                public Vector2 translate;
                public Vector2 scale;
                public float rotate;
                public float Mode;
            }

        }

        public class RenderInfoData
        {
            public string Name;
            public long DataOffset;
            public RenderInfoType Type;
            public int ArrayLength;

            //Data Section by "Type"

            public int Value_Int;
            public string Value_String;
            public float Value_Float;

        }
        public class SamplerInfo
        {
            public int WrapModeU;
            public int WrapModeV;
            public int WrapModeW;

        }

        private static VBN vbn()
        {

            if (Runtime.TargetVBN == null) //Create VBN as target so we can export anims
                Runtime.TargetVBN = new VBN();

            return Runtime.TargetVBN;
        }
        public class FMDL_Model : TreeNode
        {
            public VBN skeleton
            {
                get
                {
                    return vbn;
                }
                set
                {
                    vbn = value;
                }
            }
            public FMDL_Model()
            {
                Checked = true;
                ImageKey = "model";
                SelectedImageKey = "model";
            }

            public void GenerateTansBitansEachMesh()
            {
                foreach (Mesh m in poly)
                {
                    m.CalculateTangentBitangent();
                }
            }
            public void SmoothNormalEachMesh()
            {
                foreach (Mesh m in poly)
                {
                    m.SmoothNormals();
                }
            }
            public void GenerateNormalEachMesh()
            {
                foreach (Mesh m in poly)
                {
                    m.CalculateNormals();
                }
            }
 

            private VBN vbn = new VBN();
            public List<Mesh> poly = new List<Mesh>();
            public bool isVisible = true;
            public int[] Node_Array;

            public List<Mesh> depthSortedMeshes = new List<Mesh>();
        }

        public class MTA : TreeNode
        {
            public uint FrameCount;

            public List<MatAnimEntry> matEntries = new List<MatAnimEntry>();

            public List<string> Pat0 = new List<string>();
          

            public List<TexPatInfo> TexPat0Info = new List<TexPatInfo>();

            public MTA()
            {
                ImageKey = "image";
                SelectedImageKey = "image";
            }

            public void ExpandNodes()
            {
                Nodes.Clear();
                TreeNode mat = new TreeNode();
                foreach (MatAnimEntry e in matEntries)
                {
                    mat.Text = e.Name;
                    mat.Nodes.Add(e);
                }
                Nodes.Add(mat);
            }
        }
        public class TexPatInfo
        {
            public string SamplerName;
            public int CurveIndex;
        }

        public class MatAnimEntry : TreeNode
        {
            public MatAnimEntry()
            {
                ImageKey = "image";
                SelectedImageKey = "image";
            }
            

            public List<MatAnimData> matCurves = new List<MatAnimData>();

            public void Interpolate(ResNSW.AnimCurve cr)
            {
                MatAnimData md = new MatAnimData();

                md.Frames = new int[cr.Frames.Length];

                for (int i = 0; i < (ushort)cr.Frames.Length; i++)
                {
                    md.Frames[i] = i;

                    if (cr.CurveType == ResNSW.AnimCurveType.Cubic)
                    {
                        md.keys.Add(new AnimKey()
                        {
                            frame = (int)cr.Frames[i],
                            unk1 = cr.Offset + ((cr.Keys[i, 0] * cr.Scale)),
                            unk2 = cr.Offset + ((cr.Keys[i, 1] * cr.Scale)),
                            unk3 = cr.Offset + ((cr.Keys[i, 2] * cr.Scale)),
                            unk4 = cr.Offset + ((cr.Keys[i, 3] * cr.Scale)),
                        });
                    }
                    if (cr.CurveType == ResNSW.AnimCurveType.Linear)
                    {
                        md.keys.Add(new AnimKey()
                        {
                            frame = (int)cr.Frames[i],
                            unk1 = cr.Offset + ((cr.Keys[i, 0] * cr.Scale)),
                            unk2 = cr.Offset + ((cr.Keys[i, 1] * cr.Scale)),
                        });
                    }
                    else if (cr.CurveType == ResNSW.AnimCurveType.StepInt)
                    {
                        md.keys.Add(new AnimKey()
                        {
                            frame = (int)cr.Frames[i],
                            unk1 = cr.Offset + ((cr.Keys[i, 0] * cr.Scale)),
                        });
                    }
                    else if (cr.CurveType == ResNSW.AnimCurveType.StepBool)
                    {

                    }
                }
                matCurves.Add(md);
            }
        }
        public class MatAnimData
        {
            public int Frame;
            public int CurveIndex;
            public int ConstIndex;
            public int[] Frames;

            //VIS data
            public bool VIS_State;
            public string MaterialName;

            //Pat0 data
            public string Pat0Tex;
            public string SamplerName;

            public float Value;

            public List<AnimKey> keys = new List<AnimKey>();

            //For interpolation. 
            public AnimKey GetLeft(int frame)
            {
                AnimKey prev = keys[0];

                for (int i = 0; i < keys.Count - 1; i++)
                {
                    AnimKey key = keys[i];
                    if (key.frame > frame && prev.frame <= frame)
                        break;
                    prev = key;
                }

                return prev;
            }
            public AnimKey GetRight(int frame)
            {
                AnimKey cur = keys[0];
                AnimKey prev = keys[0];

                for (int i = 1; i < keys.Count; i++)
                {
                    AnimKey key = keys[i];
                    cur = key;
                    if (key.frame > frame && prev.frame <= frame)
                        break;
                    prev = key;
                }

                return cur;
            }
        }
        public class AnimKey
        {
            public int frame;
            public float unk1, unk2, unk3, unk4;

            public int offset;
        }
#endregion
    }
}