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
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.Textures;
using SFGraphics.Tools;

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

        public FTEXContainer FTEXContainer;

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

        public string path = "";
        public static Vector3 position = new Vector3(0, 0, 0);
        public static Vector3 rotation = new Vector3(0, 0, 0);
        public static Vector3 scale = new Vector3(1, 1, 1);

        private bool hasCreatedRenderMeshes = false;

        public BNTX Bntx = null;

        #region Render BFRES

        public BFRES()
        {

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
                ModelTransform();
            }
            else
            {
                IsSwitchBFRES = false;

                TargetWiiUBFRES = new ResFile(new MemoryStream(file_data));
                path = Text;
                Read(TargetWiiUBFRES);
            }

          //  SetMaterialToXML();
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
            else if (fname.Contains("Mario") && fname.Contains("HandL"))
            {
                Console.WriteLine("Positioning Face Mesh.....");
                position = new Vector3(48.877f, 82.551f, -3.3f);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 90f, 0);
            }
            else if (fname.Contains("Mario") && fname.Contains("HandR"))
            {
                Console.WriteLine("Positioning HandR Mesh.....");
                position = new Vector3(-48.877f, 82.551f, -3.3f);
                scale = new Vector3(1, 1, 1);
                rotation = new Vector3(0, -90f, 0);
            }
            else if (fname.Contains("Mario") && fname.Contains("Eye"))
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
            else if (fname.Contains("Mario") && fname.Contains("Skirt"))
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

        public BNTX getBntx()
        {
            BNTX NewBntx = null;
            if (TargetSwitchBFRES != null)
            {
                foreach (ResNSW.ExternalFile ext in TargetSwitchBFRES.ExternalFiles)
                {
                    FileData f = new FileData(ext.Data);
                    if (ext.Data.Length > 4) //BOTW has some external files that are smaller than 4 which i need to read for magic
                    {
                        int EmMagic = f.readInt();
                        if (EmMagic == 0x424E5458) //Textures
                        {
                            f.Endian = Endianness.Little;
                            f.skip(-4);
                            int temp = f.pos();
                            NewBntx = new BNTX();
                            NewBntx.Read(f);
                            TEmbedded.Nodes.Add(NewBntx);
                        }

                    }
                }
            }
            return NewBntx;
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

        public void ModelTransform()
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
                }
            }
        }

        public void DepthSortMeshes(Vector3 cameraPosition)
        {
            foreach (FMDL_Model fmdl in models)
            {
                List<Mesh> unsortedMeshes = new List<Mesh>();

                foreach (Mesh m in fmdl.poly)
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

        public void Render(Camera camera, bool drawPolyIds)
        {
            if (Runtime.renderBoundingSphere)
                DrawBoundingBoxes();

            foreach (FMDL_Model fmdl in models)
            {
                // For proper alpha blending, draw in reverse order and draw opaque objects first. 
                List<Mesh> opaque = new List<Mesh>();
                List<Mesh> transparent = new List<Mesh>();

                if (!hasCreatedRenderMeshes)
                {
                    UpdateRenderMeshes();
                    hasCreatedRenderMeshes = true;
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
                    if (m.Parent != null && (m.Parent).Checked)
                        DrawMesh(m, fmdl, camera, drawPolyIds);
                }

                foreach (Mesh m in transparent)
                {
                    if (((FMDL_Model)m.Parent).Checked)
                        DrawMesh(m, fmdl, camera, drawPolyIds);
                }
            }
        }

        private static void SetBoneUniforms(Shader shader, FMDL_Model fmdl)
        {
            for (int i = 0; i < fmdl.Node_Array.Length; i++)
            {
                Matrix4 transform = fmdl.skeleton.bones[fmdl.Node_Array[i]].invert * fmdl.skeleton.bones[fmdl.Node_Array[i]].transform;
                GL.UniformMatrix4(GL.GetUniformLocation(shader.Id, String.Format("bones[{0}]", i)), false, ref transform);
            }
        }

        private void CheckChildren(TreeNode rootNode, bool isChecked)
        {
            foreach (TreeNode node in rootNode.Nodes)
            {
                CheckChildren(node, isChecked);
                node.Checked = isChecked;
            }
        }

        private void SetRenderSettings(Shader shader)
        {
            shader.SetBoolToInt("renderVertColor", Runtime.renderVertColor);
            shader.SetInt("renderType", (int)Runtime.renderType);
            shader.SetInt("uvChannel", (int)Runtime.uvChannel);
            shader.SetBoolToInt("useNormalMap", Runtime.renderNormalMap);
            shader.SetBoolToInt("renderR", Runtime.renderR);
            shader.SetBoolToInt("renderG", Runtime.renderG);
            shader.SetBoolToInt("renderB", Runtime.renderB);
            shader.SetBoolToInt("renderAlpha", Runtime.renderAlpha);
            shader.SetBoolToInt("renderFog", Runtime.renderFog);
            shader.SetBoolToInt("useImageBasedLighting", true);
        }

        private static void SetMiscUniforms(Camera camera, Shader shader)
        {
            Rendering.Lights.LightColor diffuseColor = Runtime.lightSetParam.characterDiffuse.diffuseColor;
            Rendering.Lights.LightColor ambientColor = Runtime.lightSetParam.characterDiffuse.ambientColor;
            shader.SetVector3("difLightColor", diffuseColor.R, diffuseColor.G, diffuseColor.B);
            shader.SetVector3("ambLightColor", ambientColor.R, ambientColor.G, ambientColor.B);

            Matrix4 invertedCamera = camera.MvpMatrix.Inverted();
            Vector3 lightDirection = new Vector3(0f, 0f, -1f);

            //Todo. Maybe change direction via AAMP file (configs shader data)
            shader.SetVector3("specLightDirection", Vector3.TransformNormal(lightDirection, invertedCamera).Normalized());
            shader.SetVector3("difLightDirection", Vector3.TransformNormal(lightDirection, invertedCamera).Normalized());

            // PBR IBL
            shader.SetTexture("irradianceMap", RenderTools.diffusePbr.Id, TextureTarget.TextureCubeMap, 18);
            shader.SetTexture("specularIbl", RenderTools.specularPbr.Id, TextureTarget.TextureCubeMap, 19);
        }

        private void DrawMesh(Mesh mesh, FMDL_Model fmdl, Camera camera, bool drawPolyIds = false, bool drawSelection = false)
        {
            if (mesh.lodMeshes[mesh.DisplayLODIndex].faces.Count <= 3)
                return;

            Shader shader;
            if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                shader = OpenTKSharedResources.shaders["BFRES_Debug"];
            else if (mesh.material.shaderassign.ShaderModel == "uking_mat")
                shader = OpenTKSharedResources.shaders["BFRES_Botw"];
            else if (Runtime.renderBfresPbr)
                shader = OpenTKSharedResources.shaders["BFRES_PBR"];
            else
                shader = OpenTKSharedResources.shaders["BFRES"];

            shader.UseProgram();

            // Shader Uniforms
            ApplyTransformFix(fmdl, mesh, shader);

            SetMiscUniforms(camera, shader);
            SetRenderSettings(shader);

            Matrix4 mvpMatrix = camera.MvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref mvpMatrix);

            Matrix4 sphereMatrix = camera.ModelViewMatrix;
            sphereMatrix.Invert();
            sphereMatrix.Transpose();
            shader.SetMatrix4x4("sphereMatrix", ref sphereMatrix);

            SetUniforms(mesh.material, shader, mesh, mesh.DisplayId, drawPolyIds);
            SetTextureUniforms(mesh.material, mesh, shader);
            SetBoneUniforms(shader, fmdl);

            SetAlphaBlending(mesh.material);
            SetAlphaTesting(mesh.material);

            foreach (RenderInfoData renderInfo in mesh.material.renderinfo)
            {
                SetFaceCulling(mesh.material, renderInfo);
            }

            DrawGeometry(mesh, shader, camera);
        }

        private static void DrawGeometry(Mesh mesh, Shader shader, Camera camera)
        {
            if (mesh.Checked)
            {
                if ((mesh.IsSelected || mesh.Parent.IsSelected))
                {
                    DrawModelSelection(mesh, shader, camera);
                }
                else
                {
                    if (Runtime.renderModelWireframe)
                    {
                        DrawModelWireframe(mesh, shader, camera);
                    }

                    if (Runtime.renderModel)
                    {
                        mesh.renderMesh.Draw(shader, camera);
                    }
                }
            }
        }

        private static void DrawModelWireframe(Mesh mesh, Shader shader, Camera camera)
        {
            // use vertex color for wireframe color
            shader.SetInt("colorOverride", 1);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.5f);

            mesh.renderMesh.Draw(shader, camera);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            shader.SetInt("colorOverride", 0);
        }
        private static void DrawModelSelection(Mesh mesh, Shader shader, Camera camera)
        {
            //This part needs to be reworked for proper outline. Currently would make model disappear

            mesh.renderMesh.Draw(shader, camera);

            GL.Enable(EnableCap.StencilTest);
            // use vertex color for wireframe color
            shader.SetInt("colorOverride", 1);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.5f);

            mesh.renderMesh.Draw(shader, camera);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            shader.SetInt("colorOverride", 0);

            GL.Enable(EnableCap.DepthTest);
        }

        // Here I'll use the same enums as NUDs does. 
        // BFRES for switch is inconsistent with the strings and data.
        // multiple games and wii u uses flags, so it's best to have them all in one.
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

        private static readonly Dictionary<int, TextureMinFilter> minfilter = new Dictionary<int, TextureMinFilter>()
        {
            { 0x00, TextureMinFilter.LinearMipmapLinear},
            { 0x01, TextureMinFilter.Nearest},
            { 0x02, TextureMinFilter.Linear},
            { 0x03, TextureMinFilter.NearestMipmapLinear},
        };

        static readonly Dictionary<int, TextureMagFilter> magfilter = new Dictionary<int, TextureMagFilter>()
        {
            { 0x00, TextureMagFilter.Linear},
            { 0x01, TextureMagFilter.Nearest},
            { 0x02, TextureMagFilter.Linear}
        };

        static Dictionary<int, TextureWrapMode> wrapmode = new Dictionary<int, TextureWrapMode>(){
                    { 0x00, TextureWrapMode.Repeat},
                    { 0x01, TextureWrapMode.MirroredRepeat},
                    { 0x02, TextureWrapMode.ClampToEdge},
                    { 0x03, TextureWrapMode.MirroredRepeat},
        };

        public void UpdateRenderMeshes()
        {
            foreach (FMDL_Model fmdl in models)
            {
                foreach (Mesh m in fmdl.poly)
                {
                    m.renderMesh = m.CreateRenderMesh();
                }
            }
        }

        private static void ApplyTransformFix(FMDL_Model fmdl, Mesh m, Shader shader)
        {
            shader.SetInt("NoSkinning", 0);
            shader.SetInt("RigidSkinning", 0);
            //Some objects will have no weights or indices. These will weigh to the bone index in the shape section.
            shader.SetInt("SingleBoneIndex", m.boneIndx);

            if (m.VertexSkinCount == 1)
            {
                shader.SetInt("RigidSkinning", 1);
            }
            if (m.VertexSkinCount == 0)
            {
                Matrix4 transform = fmdl.skeleton.bones[m.boneIndx].invert * fmdl.skeleton.bones[m.boneIndx].transform;
                shader.SetMatrix4x4("singleBoneBindTransform", ref transform);

                shader.SetInt("NoSkinning", 1);
            }
        }

        private static void SetUniforms(MaterialData mat, Shader shader, Mesh m, int id, bool drawId)
        {
            shader.SetVector4("gsys_bake_st0", new Vector4(1, 1, 0, 0));
            shader.SetVector4("gsys_bake_st1", new Vector4(1, 1, 0, 0));
            shader.SetInt("enableCellShading", 0);

            shader.SetVector3("colorId", ColorTools.Vector4FromColor(Color.FromArgb(id)).Xyz);
            shader.SetBoolToInt("drawId", drawId);

            //BOTW uses this shader so lets add in cell shading
            if (mat.shaderassign.ShaderModel == "uking_mat")
                shader.SetInt("enableCellShading", 1);

            shader.SetInt("selectedBoneIndex", Runtime.selectedBoneIndex);

            //This uniform is set so I can do SRT anims.
            SetUniformData(mat, shader, "tex_mtx0");
            //    SetUnifromData(mat, shader, "tex_mtx1");
            //     SetUnifromData(mat, shader, "tex_mtx2");

            //This uniform variable shifts first bake map coords (MK8, Spatoon 1/2, ect)
            SetUniformData(mat, shader, "gsys_bake_st0");
            //This uniform variable shifts second bake map coords (MK8, Spatoon 1/2, ect)
            SetUniformData(mat, shader, "gsys_bake_st1");
            SetUniformData(mat, shader, "normal_map_weight");
            SetUniformData(mat, shader, "ao_density");
            SetUniformData(mat, shader, "base_color_mul_color");
            SetUniformData(mat, shader, "emission_color");
            SetUniformData(mat, shader, "specular_color");

            //   Shader option data
            // These enable certain effects
            //They can enable texture maps. However due to these being varied between games, doing by samplers is more simple. 

            //This uniform sets normal maps for BOTW to use second UV channel
            SetUniformData(mat, shader, "uking_texture2_texcoord");
            //Sets shadow type
            //0 = Ambient occusion bake map
            //1 = Shadow 
            //2 = Shadow + Ambient occusion map
            SetUniformData(mat, shader, "bake_shadow_type");

            SetUniformData(mat, shader, "enable_fresnel");
            SetUniformData(mat, shader, "enable_emission");

            shader.SetBoolToInt("isTransparent", m.isTransparent);
        }

        private static void SetUniformData(MaterialData mat, Shader shader, string propertyName)
        {
            //Note uniform data has so many types so it's messy atm

            if (mat.shaderassign.options.ContainsKey(propertyName))
            {
                float value = float.Parse(mat.shaderassign.options[propertyName]);

                shader.SetFloat(propertyName, value);
            }

            if (mat.matparam.ContainsKey(propertyName))
            {
                if (mat.matparam[propertyName].Type == ShaderParamType.Float)
                {
                    if (mat.anims.ContainsKey(propertyName))
                        mat.matparam[propertyName].Value_float = mat.anims[propertyName][0];
                    shader.SetFloat(propertyName, mat.matparam[propertyName].Value_float);
                }

                if (mat.matparam[propertyName].Type == ShaderParamType.Float2)
                {
                    if (mat.anims.ContainsKey(propertyName))
                    {
                        mat.matparam[propertyName].Value_float2 = new Vector2(
                                     mat.anims[propertyName][0], mat.anims[propertyName][1]);
                    }

                    shader.SetVector2(propertyName, mat.matparam[propertyName].Value_float2);
                }

                if (mat.matparam[propertyName].Type == ShaderParamType.Float3)
                {
                    if (mat.anims.ContainsKey(propertyName))
                    {
                        mat.matparam[propertyName].Value_float3 = new Vector3(
                                     mat.anims[propertyName][0], mat.anims[propertyName][1],
                                     mat.anims[propertyName][2]);
                    }

                    shader.SetVector3(propertyName, mat.matparam[propertyName].Value_float3);
                }
                if (mat.matparam[propertyName].Type == ShaderParamType.Float4)
                {
                    if (mat.anims.ContainsKey(propertyName))
                    {
                        mat.matparam[propertyName].Value_float4 = new Vector4(
                                     mat.anims[propertyName][0], mat.anims[propertyName][1],
                                     mat.anims[propertyName][2], mat.anims[propertyName][3]);
                    }

                    shader.SetVector4(propertyName, mat.matparam[propertyName].Value_float4);
                }
                if (mat.matparam[propertyName].Type == ShaderParamType.TexSrt)
                {
                    // Vector 2 Scale
                    // 1 roation float
                    // Vector2 translate
                    ShaderParam.TextureSRT texSRT = mat.matparam[propertyName].Value_TexSrt;

                    shader.SetVector2("SRT_Scale", texSRT.scale);
                    shader.SetFloat("SRT_Rotate", texSRT.rotate);
                    shader.SetVector2("SRT_Translate", texSRT.translate);
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

        private static void SetDefaultTextureAttributes(MaterialData materialData, Shader shader)
        {
            shader.SetBoolToInt("HasDiffuse", materialData.HasDiffuseMap);
            shader.SetBoolToInt("HasDiffuseLayer", materialData.HasDiffuseLayer);
            shader.SetBoolToInt("HasNormalMap", materialData.HasNormalMap);
            shader.SetBoolToInt("HasEmissionMap", materialData.HasEmissionMap);
            shader.SetBoolToInt("HasLightMap", materialData.HasLightMap);
            shader.SetBoolToInt("HasShadowMap", materialData.HasShadowMap);
            shader.SetBoolToInt("HasSpecularMap", materialData.HasSpecularMap);
            shader.SetBoolToInt("HasTeamColorMap", materialData.HasTeamColorMap);
            shader.SetBoolToInt("HasSphereMap", materialData.HasSphereMap);

            //Unused atm untill I do PBR shader
            shader.SetBoolToInt("HasMetalnessMap", materialData.HasMetalnessMap);
            shader.SetBoolToInt("HasRoughnessMap", materialData.HasRoughnessMap);
            shader.SetBoolToInt("HasMRA", materialData.HasMRA);
        }

        private static void SetTextureUniforms(MaterialData materialData, Mesh mesh, Shader shader)
        {
            SetDefaultTextureAttributes(materialData, shader);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.defaultTex.Id);

            shader.SetTexture("UVTestPattern", RenderTools.uvTestPattern.Id, TextureTarget.Texture2D, 10);
            shader.SetTexture("weightRamp1", RenderTools.boneWeightGradient.Id, TextureTarget.Texture2D, 11);
            shader.SetTexture("weightRamp2", RenderTools.boneWeightGradient2.Id, TextureTarget.Texture2D, 12);
            GL.Uniform1(shader.GetVertexAttributeUniformLocation("normalMap"), 0);
            GL.Uniform1(shader.GetVertexAttributeUniformLocation("BakeShadowMap"), 0);

            // There is no particular order in the list.
            foreach (MatTexture matex in materialData.textures)
            {
                if (matex.Type == MatTexture.TextureType.Diffuse)
                    TextureUniform(shader, materialData, materialData.HasDiffuseMap, "tex0", matex);
                else if (matex.Type == MatTexture.TextureType.Normal)
                    TextureUniform(shader, materialData, materialData.HasNormalMap, "normalMap", matex);
                else if (matex.Type == MatTexture.TextureType.Emission)
                    TextureUniform(shader, materialData, materialData.HasEmissionMap, "EmissionMap", matex);
                else if (matex.Type == MatTexture.TextureType.Specular)
                    TextureUniform(shader, materialData, materialData.HasSpecularMap, "SpecularMap", matex);
                else if (matex.Type == MatTexture.TextureType.Shadow)
                    TextureUniform(shader, materialData, materialData.HasShadowMap, "BakeShadowMap", matex);
                else if (matex.Type == MatTexture.TextureType.Light)
                    TextureUniform(shader, materialData, materialData.HasLightMap, "BakeLightMap", matex);
                else if (matex.Type == MatTexture.TextureType.Metalness)
                    TextureUniform(shader, materialData, materialData.HasMetalnessMap, "MetalnessMap", matex);
                else if (matex.Type == MatTexture.TextureType.Roughness)
                    TextureUniform(shader, materialData, materialData.HasRoughnessMap, "RoughnessMap", matex);
                else if (matex.Type == MatTexture.TextureType.TeamColor)
                    TextureUniform(shader, materialData, materialData.HasTeamColorMap, "TeamColorMap", matex);
                else if (matex.Type == MatTexture.TextureType.Transparency)
                    TextureUniform(shader, materialData, materialData.HasTransparencyMap, "TransparencyMap", matex);
                else if (matex.Type == MatTexture.TextureType.DiffuseLayer2)
                    TextureUniform(shader, materialData, materialData.HasDiffuseLayer, "DiffuseLayer", matex);
                else if (matex.Type == MatTexture.TextureType.SphereMap)
                    TextureUniform(shader, materialData, materialData.HasSphereMap, "SphereMap", matex);
                else if (matex.Type == MatTexture.TextureType.MRA)
                    TextureUniform(shader, materialData, materialData.HasMRA, "MRA", matex);
            }
        }

        private static void TextureUniform(Shader shader, MaterialData mat, bool hasTex, string name, MatTexture matTexture)
        {
            // Bind the texture and create the uniform if the material has the right textures. 
            if (hasTex)
            {
                GL.Uniform1(shader.GetVertexAttributeUniformLocation(name), BindTexture(matTexture));
            }
        }

        public static int BindTexture(MatTexture tex)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + tex.hash + 1);
            GL.BindTexture(TextureTarget.Texture2D, RenderTools.defaultTex.Id);

            SFGraphics.GLObjects.Textures.Texture texture;

            if (IsSwitchBFRES == true)
            {
                foreach (BNTX bntx in Runtime.BNTXList)
                {
                    if (bntx.glTexByName.TryGetValue(tex.Name, out texture))
                    {
                        BindGLTexture(tex, texture);
                    }
                }
            }
            else
            {
                foreach (FTEXContainer ftexC in Runtime.FTEXContainerList)
                {
                    if (ftexC.glTexByName.TryGetValue(tex.Name, out texture))
                    {
                        BindGLTexture(tex, texture);
                    }
                }
            }

            return tex.hash + 1;
        }

        private static void BindGLTexture(MatTexture matTexture, SFGraphics.GLObjects.Textures.Texture texture)
        {
            // Set the texture's parameters based on the material settings.
            texture.Bind();
            texture.TextureWrapS = wrapmode[matTexture.wrapModeS];
            texture.TextureWrapT = wrapmode[matTexture.wrapModeT];
            texture.MinFilter = minfilter[matTexture.minFilter];
            texture.MagFilter = magfilter[matTexture.magFilter];

            // TODO: These aren't controlled by the Texture class yet.
            GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, 0.0f);
            if (matTexture.mipDetail == 0x4 || matTexture.mipDetail == 0x6)
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, 4.0f);
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
                                            Console.WriteLine(md.Value);
                                            Console.WriteLine(md.AnimColorType);

                                            //Do shader param animations. (Color, SRT, ect)
                                            if (mesh.material.matparam.ContainsKey(md.shaderParamName))
                                            {
                                                ShaderParam prm = mesh.material.matparam[md.shaderParamName];




                                                switch (prm.Type)
                                                {
                                                    case ShaderParamType.TexSrt:

                                                        break;
                                                    case ShaderParamType.Float:

                                                        break;
                                                    case ShaderParamType.Float2:

                                                        break;
                                                    case ShaderParamType.Float3:

                                                        break;
                                                    case ShaderParamType.Float4:


                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
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

            public BfresRenderMesh renderMesh;

            // Used to generate a unique color for viewport selection.
            private static List<int> previousDisplayIds = new List<int>();
            public int DisplayId { get { return displayId; } }
            private int displayId = 0;

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
                GenerateDisplayId();
            }
            // for drawing
            public int[] display;
            public int[] selectedVerts;
            public int Offset; // For Rendering

            private void GenerateDisplayId()
            {
                // Find last used ID. Next ID will be last ID + 1.
                // A color is generated from the integer as hexadecimal, but alpha is ignored.
                // Incrementing will affect RGB before it affects Alpha (ARGB color).
                int index = 0;
                if (previousDisplayIds.Count > 0)
                    index = previousDisplayIds.Last();
                index++;
                previousDisplayIds.Add(index);
                displayId = index;
            }

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

            public BfresRenderMesh CreateRenderMesh()
            {
                List<DisplayVertex> displayVertices = new List<DisplayVertex>();
                List<int> displayVertexIndices = new List<int>();

                List<DisplayVertex> pv = CreateDisplayVertices();
                displayVertices.AddRange(pv);

                for (int i = 0; i < lodMeshes[DisplayLODIndex].displayFaceSize; i++)
                {
                    displayVertexIndices.Add(display[i]);
                }

                BfresRenderMesh bfresRenderMesh = new BfresRenderMesh(displayVertices, displayVertexIndices);
                return bfresRenderMesh;
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
                        col = v.col,
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
                WriteMaterialXML(material, this);
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

                List<Vector3> vertexPositions = new List<Vector3>();
                foreach (var vertex in vertices)
                {
                    vertexPositions.Add(vertex.pos);
                }

                Vector4 boundingSphere = SFGraphics.Tools.BoundingSphereGenerator.GenerateBoundingSphere(vertexPositions);

                for (int i = 0; i < BoundingCount; i++)
                {
                    boundingBoxes[i].Center = boundingSphere.Xyz;
                    boundingBoxes[i].Extent = boundingSphere.Xyz + new Vector3(boundingSphere.W);
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
            public bool HasLightMap = false;
            public bool HasSphereMap = false;

            //PBR (Switch) data
            public bool HasMetalnessMap = false;
            public bool HasRoughnessMap = false;
            public bool HasMRA = false;

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
                    texture.Texture = new Syroot.NintenTools.Bfres.Texture();

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
                SphereMap = 14,
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


            public void InterpolateWU(AnimCurve cr)
            {
                MatAnimData md = new MatAnimData();

                md.Frames = new int[cr.Frames.Length];

                for (int i = 0; i < (ushort)cr.Frames.Length; i++)
                {
                    md.Frames[i] = i;

                    if (cr.CurveType == AnimCurveType.Cubic)
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
                    if (cr.CurveType == AnimCurveType.Linear)
                    {
                        md.keys.Add(new AnimKey()
                        {
                            frame = (int)cr.Frames[i],
                            unk1 = cr.Offset + ((cr.Keys[i, 0] * cr.Scale)),
                            unk2 = cr.Offset + ((cr.Keys[i, 1] * cr.Scale)),
                        });
                    }
                    else if (cr.CurveType == AnimCurveType.StepInt)
                    {
                        md.keys.Add(new AnimKey()
                        {
                            frame = (int)cr.Frames[i],
                            unk1 = cr.Offset + ((cr.Keys[i, 0] * cr.Scale)),
                        });
                    }
                    else if (cr.CurveType == AnimCurveType.StepBool)
                    {

                    }
                }
                matCurves.Add(md);
            }
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
            public string Pat0Tex = "";
            public string SamplerName = "";
            public string shaderParamName = "";

            public ColorType AnimColorType;
            public enum ColorType
            {
                Red = 0,
                Blue = 1,
                Green = 2,
                Alpha = 3,
            }

            public SRTType AnimSRTType;
            public enum SRTType
            {
                ScaleX = 0,
                ScaleY = 1,
                Rotate = 2,
                TranslateX = 3,
                TranslateY = 4,
            }

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
