using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Syroot.NintenTools.Yaz0;
using Syroot.NintenTools.Bfres;
using Syroot.NintenTools.Bfres.GX2;
using Syroot.NintenTools.Bfres.Helpers;
using ResNSW = Syroot.NintenTools.NSW.Bfres;
using GFXNSW = Syroot.NintenTools.NSW.Bfres.GFX;
using HelperNSW = Syroot.NintenTools.NSW.Bfres.Helpers;

namespace Smash_Forge
{
    public class BFRES : FileBase
    {
        public override Endianness Endian { get; set; }

        public List<string> stringContainer = new List<string>();
        public List<FMDL_Model> models = new List<FMDL_Model>();
        public List<Vector3> BoneFixTrans = new List<Vector3>();
        public List<Vector3> BoneFixRot = new List<Vector3>();
        public List<Vector3> BoneFixScale = new List<Vector3>();

        public static Shader shader = null;

        public Matrix4[] sb;

        public Dictionary<string, FTEX> textures = new Dictionary<string, FTEX>();
        public int FSKACount;
        public int FVISCount;
        public int FMAACount;

        public TreeNode TModels = new TreeNode() { Text = "Models", Checked = true };
        public TreeNode TMaterialAnim = new TreeNode() { Text = "Material Animations" };
        public TreeNode TSkeletalAnim = new TreeNode() { Text = "Skeletal Animations" };
        public TreeNode TVisualAnim = new TreeNode() { Text = "Visual Animations" };
        public TreeNode TShapeAnim = new TreeNode() { Text = "Shape Animations" };
        public TreeNode TSceneAnim = new TreeNode() { Text = "Scene Animations" };
        public TreeNode TEmbedded = new TreeNode() { Text = "Embedded Files" };

        public ResFile TargetWiiUBFRES;
        public ResNSW.ResFile TargetSwitchBFRES;

        public TreeNode TTextures = new TreeNode() { Text = "Textures" };
        public TreeNode TShaderparam = new TreeNode() { Text = "Shader Param Animations" };
        public TreeNode TColoranim = new TreeNode() { Text = "Color Animations" };
        public TreeNode TTextureSRT = new TreeNode() { Text = "Texture STR Animations" };
        public TreeNode TTexturePat = new TreeNode() { Text = "Texture Pattern Animations" };
        public TreeNode TBonevisabilty = new TreeNode() { Text = "Bone Visabilty" };


        //Kirby star allies makes it impossible to texture map without REing the shaders so i'll do them by texture list
        public List<string> HackyTextureList = new List<string>(new string[] {
            "Alb", "alb", "Base", "base", "bonbon.167300917","Eye.00","EyeIce.00", "FaceDummy", "Eye01.17", "Dee.00",
            "rainbow.758540574", "Mucus._1700670200", "Eye.11", "CapTail00","eye.0"

        });

        // gl buffer objects
        int vbo_position;
        int vbo_color;
        int vbo_nrm;
        int vbo_uv0;
        int vbo_uv1;
        int vbo_weight;
        int vbo_weight1;
        int vbo_bone;
        int vbo_bone1;
        int vbo_tan;
        int vbo_bitan;
        int ibo_elements;

        Vector2[] uvdata0, uvdata1;
        Vector3[] vertdata, nrmdata, tangentdata, bitangentdata;
        int[] facedata;
        Vector4[] bonedata, coldata, weightdata, weight1data, bone1data;
        public string path = "";
        public static Vector3 position, rotation, scale;

        #region Render BFRES

        public BFRES()
        {
            GL.GenBuffers(1, out vbo_position);
            GL.GenBuffers(1, out vbo_color);
            GL.GenBuffers(1, out vbo_nrm);
            GL.GenBuffers(1, out vbo_uv0);
            GL.GenBuffers(1, out vbo_uv1);
            GL.GenBuffers(1, out vbo_bone);
            GL.GenBuffers(1, out vbo_bone1);
            GL.GenBuffers(1, out vbo_weight);
            GL.GenBuffers(1, out vbo_weight1);
            GL.GenBuffers(1, out vbo_tan);
            GL.GenBuffers(1, out vbo_bitan);
            GL.GenBuffers(1, out ibo_elements);

            if (!Runtime.shaders.ContainsKey("BFRES"))
            {
                Rendering.ShaderTools.CreateShader("BFRES", "/lib/Shader/Legacy/", "/lib/Shader/");
            }
            if (!Runtime.shaders.ContainsKey("BFRES_PBR"))
            {
                Rendering.ShaderTools.CreateShader("BFRES_PBR", "/lib/Shader/Legacy/", "/lib/Shader/");
            }


            Runtime.shaders["BFRES_PBR"].DisplayCompilationWarning("BFRES_PBR");
            Runtime.shaders["BFRES"].DisplayCompilationWarning("BFRES");
        }

        public BFRES(string fname, Vector3 p, Vector3 s, Vector3 r) : this()
        {
            Text = Path.GetFileName(fname);

            //Set the position/rot/scale variable to what it's set to in the main form. This is so i can change it by byaml and other things
            position = p;
            scale = s;
            rotation = r;

            path = Text;
            Read(fname);
            UpdateTextureMaps();
            UpdateVertexData();
            SetupShader();
        }

        public void Destroy()
        {
            GL.DeleteBuffer(vbo_position);
            GL.DeleteBuffer(ibo_elements);
        }


        private void SetupShader()
        {
            int maxUniformBlockSize = GL.GetInteger(GetPName.MaxUniformBlockSize);
            if (shader == null)
            {
                shader = new Shader();
                shader = Runtime.shaders["BFRES"];
            }
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);

            RenderTest();
        }

        public Matrix4 BonePosExtra;
        public Matrix4 BonePosFix;

        public void RenderTest()
        {

            shader.enableAttrib();
            GL.UseProgram(shader.programID);

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
                    b.transform = b.transform * BonePosExtra;
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

            GL.Uniform1(shader.getAttribute("renderVertColor"), Runtime.renderVertColor ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderType"), (int)Runtime.renderType);
            GL.Uniform1(shader.getAttribute("uvChannel"), (int)Runtime.uvChannel);
            GL.Uniform1(shader.getAttribute("UVTestPattern"), 10);
            GL.Uniform1(shader.getAttribute("useNormalMap"), Runtime.renderNormalMap ? 1 : 0);


            GL.UniformMatrix4(shader.getAttribute("modelview"), false, ref view);




            GL.ActiveTexture(TextureUnit.Texture10);
            GL.BindTexture(TextureTarget.Texture2D, Rendering.RenderTools.uvTestPattern);

            GL.Uniform1(shader.getAttribute("renderR"), Runtime.renderR ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderG"), Runtime.renderG ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderB"), Runtime.renderB ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderAlpha"), Runtime.renderAlpha ? 1 : 0);


            foreach (FMDL_Model fmdl in models)
            {

                //Render Skeleton
                Matrix4[] f = fmdl.skeleton.getShaderMatrix();
                int[] bind = fmdl.Node_Array; //Now bind each bone
                GL.UniformMatrix4(shader.getAttribute("bones"), f.Length, false, ref f[0].Row0.X);
                if (bind.Length != 0)
                {
                    GL.Uniform1(shader.getAttribute("boneList"), bind.Length, ref bind[0]);
                }
                sb = fmdl.skeleton.getShaderMatrixSingleBinded();
                GL.UniformMatrix4(shader.getAttribute("bonesfixed"), sb.Length, false, ref sb[0].Row0.X);

                //Render meshes
                foreach (Mesh m in fmdl.poly)
                {
                    if (fmdl.Parent != null && (m.Parent).Checked)
                    {
                        ApplyTransformFix(fmdl, m);
                        DrawMesh(m, shader, m.material);
                    }
                }
            }
            shader.disableAttrib();
        }

        private void DrawMesh(Mesh m, Shader shader, MaterialData mat, bool drawSelection = false)
        {
            if (m.faces.Count <= 3)
                return;

            SetVertexAttributes(m, shader);

            RenderUniformParams(mat);
            MapTextures(mat, m);

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
                        GL.DrawElements(PrimitiveType.Triangles, m.displayFaceSize, DrawElementsType.UnsignedInt, m.Offset);
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
            GL.DrawElements(PrimitiveType.Triangles, p.displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Uniform1(shader.getAttribute("colorOverride"), 0);
        }
        private static void DrawModelSelection(Mesh p, Shader shader)
        {
            GL.DrawElements(PrimitiveType.Triangles, p.displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);

            GL.Enable(EnableCap.StencilTest);
            // use vertex color for wireframe color
            GL.Uniform1(shader.getAttribute("colorOverride"), 1);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.5f);
            GL.DrawElements(PrimitiveType.Triangles, p.displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);
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
            //     GL.VertexAttribPointer(shader.getAttribute("vBone1"),     4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 84);
            //     GL.VertexAttribPointer(shader.getAttribute("vWeight1"),   4, VertexAttribPointerType.Float, false, DisplayVertex.Size, 116);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);

        }

        public void UpdateTextureMaps()
        {
         
        }

        public void UpdateVertexData()
        {
            DisplayVertex[] Vertices;
            int[] Faces;

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

                    for (int i = 0; i < m.displayFaceSize; i++)
                    {
                        Ds.Add(m.display[i] + voffset);
                    }
                    poffset += m.displayFaceSize;
                    voffset += pv.Count;
                }
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

        private static void ApplyTransformFix(FMDL_Model fmdl, Mesh m)
        {
            GL.Uniform1(shader.getAttribute("RigidSkinning"), 0);
            GL.Uniform1(shader.getAttribute("NoSkinning"), 0);

            if (m.matrFlag == 1)
            {
                GL.Uniform1(shader.getAttribute("RigidSkinning"), 1);
            }
            if (m.matrFlag == 0)
            {
                Matrix4 NoBindFix = fmdl.skeleton.bones[m.fsklindx].transform;

                GL.UniformMatrix4(shader.getAttribute("TransformNoRig"), false, ref NoBindFix);
                GL.Uniform1(shader.getAttribute("NoSkinning"), 1);
            }
        }

        private static void RenderUniformParams(MaterialData mat)
        {
            GL.Uniform4(shader.getAttribute("gsys_bake_st0"), new Vector4(1, 1, 0, 0));
            GL.Uniform4(shader.getAttribute("gsys_bake_st1"), new Vector4(1, 1, 0, 0));

            //This uniform variable shifts first bake map coords (MK8, Spatoon 1/2, ect)
            if (mat.matparam.ContainsKey("gsys_bake_st0"))
                GL.Uniform4(shader.getAttribute("gsys_bake_st0"), new Vector4(mat.matparam["gsys_bake_st0"].Value_float4));

            //This uniform variable shifts first bake map coords (MK8, Spatoon 1/2, ect)
            if (mat.matparam.ContainsKey("gsys_bake_st1"))
                GL.Uniform4(shader.getAttribute("gsys_bake_st1"), new Vector4(mat.matparam["gsys_bake_st1"].Value_float4));

            //This uniform variable shifts first bake map coords (MK8, Spatoon 1/2, ect)
            if (mat.matparam.ContainsKey("normal_map_weight"))
                GL.Uniform1(shader.getAttribute("normal_map_weight"), (mat.matparam["normal_map_weight"].Value_float));

            //This uniform variable shifts first bake map coords (MK8, Spatoon 1/2, ect)
            if (mat.matparam.ContainsKey("ao_density"))
                GL.Uniform1(shader.getAttribute("ao_density"), (mat.matparam["ao_density"].Value_float));
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
        private void SetAlphaBlending(MaterialData mat, RenderInfoData r)
        {

            switch (r.Name)
            {
                case "gsys_color_blend_rgb_src_func":
                    if (r.Value_String == "src_alpha")
                    {
                    }
                    break;
                case "gsys_color_blend_rgb_dst_func":
                    if (r.Value_String == "one_minus_src_alpha")
                    {
                    }
                    break;
                case "gsys_color_blend_rgb_op":
                    if (r.Value_String == "add")
                    {

                    }
                    break;
                case "gsys_color_blend_alpha_op":
                    if (r.Value_String == "add")
                    {
                    }
                    break;
                case "gsys_color_blend_alpha_src_func":
                    if (r.Value_String == "one")
                    {

                    }
                    break;
                case "gsys_color_blend_alpha_dst_func":
                    if (r.Value_String == "zero")
                    {
                    }
                    break;
                case "gsys_alpha_test_func":
                    break;
                case "gsys_alpha_test_value":
                    break;
                default:
                    break;
            }
        }
        private static void SetAlphaTesting(MaterialData mat, RenderInfoData r)
        {
            switch (r.Name)
            {
                case "gsys_alpha_test_enable":
                    if (r.Value_String == "true")
                    {

                    }
                    else
                    {

                    }
                    break;
                case "gsys_alpha_test_func":
                    if (r.Value_String == "lequal"){
                        
                    }
                    else if (r.Value_String == "gequal")
                    {

                    }
                    break;
                case "gsys_alpha_test_value":
                    break;
                default:
                    break;
            }
        }
        private static void SetDepthTesting(MaterialData mat, RenderInfoData r)
        {
            switch (r.Name)
            {
                case "gsys_depth_test_enable":
                    if (r.Value_String == "true")
                    {

                    }
                    else
                    {

                    }
                    break;
                case "gsys_depth_test_func":
                    if (r.Value_String == "lequal")
                    {
                        
                    }
                    break;
                case "gsys_depth_test_write":
                    break;
                default:
                    break;
            }
        }
        private static void SetDefaultTextureAttributes()
        {
            GL.Uniform1(shader.getAttribute("HasNormalMap"), 0);
            GL.Uniform1(shader.getAttribute("HasTransparencyMap"), 0);
            GL.Uniform1(shader.getAttribute("HasEmissionMap"), 0);
            GL.Uniform1(shader.getAttribute("HasShadowMap"), 0);
            GL.Uniform1(shader.getAttribute("HasAOMap"), 0);
            GL.Uniform1(shader.getAttribute("HasLightMap"), 0);
            GL.Uniform1(shader.getAttribute("HasSpecularMap"), 0);

            //Unused atm untill I do PBR shader
            GL.Uniform1(shader.getAttribute("HasMetalnessMap"), 0);
            GL.Uniform1(shader.getAttribute("HasRoughnessMap"), 0);
        }

        private static void MapTextures(MaterialData mat, Mesh m)
        {
            //Todo. Redo this part to have changable textures and updatable when a new mesh is loaded
            SetDefaultTextureAttributes();

            int id = 0;
            foreach (string tex in m.TextureMapTypes)
            {


                SamplerInfo smp = mat.samplerinfo[id];

                if (tex == "Diffuse")
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                    GL.Uniform1(shader.getAttribute("tex0"), 0);
                }
                else if (tex == "Diffuse_Layer")
                {
                    GL.ActiveTexture(TextureUnit.Texture6);
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                    GL.Uniform1(shader.getAttribute("DiffuseLayer"), 6);
                    GL.Uniform1(shader.getAttribute("HasDiffuseLayer"), 1);
                }
                else if (tex == "Normal")
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                    GL.Uniform1(shader.getAttribute("nrm"), 1);
                    GL.Uniform1(shader.getAttribute("HasNormalMap"), 1);
                }
                else if (tex == "Bake1")
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                    GL.Uniform1(shader.getAttribute("BakeShadowMap"), 2);
                    GL.Uniform1(shader.getAttribute("HasShadowMap"), 1);
                }
                else if (tex == "Bake2")
                {
                    GL.ActiveTexture(TextureUnit.Texture3);
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                    GL.Uniform1(shader.getAttribute("BakeLightMap"), 3);
                }
                else if (tex == "Metalness")
                {
                    GL.ActiveTexture(TextureUnit.Texture4);
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                    GL.Uniform1(shader.getAttribute("metallicMap"), 4);
                }
                else if (tex == "Roughness")
                {
                    GL.ActiveTexture(TextureUnit.Texture5);
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                    GL.Uniform1(shader.getAttribute("roughnessMap"), 5);
                }
                else if (tex == "MRA")
                {
              //      GL.ActiveTexture(TextureUnit.Texture6);
              //      GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
             //       GL.Uniform1(shader.getAttribute("MRA"), 6);
                }
                else if (tex == "TransparencyMap")
                {
                    GL.ActiveTexture(TextureUnit.Texture7);
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                    GL.Uniform1(shader.getAttribute("TransparencyMap"), 7);
                    GL.Uniform1(shader.getAttribute("HasTransparencyMap"), 1);
                }
                else if (tex == "EmissionMap")
                {
                    GL.ActiveTexture(TextureUnit.Texture8);
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                    GL.Uniform1(shader.getAttribute("EmissionMap"), 8);
                    GL.Uniform1(shader.getAttribute("HasEmissionMap"), 1);
                }
                else if (tex == "SpecularMap")
                {
                    GL.ActiveTexture(TextureUnit.Texture9);
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                    GL.Uniform1(shader.getAttribute("SpecularMap"), 9);
                    GL.Uniform1(shader.getAttribute("HasSpecularMap"), 1);
                }
                else
                {
                    GL.ActiveTexture(TextureUnit.Texture10);
                    GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                }
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapmode[smp.WrapModeU]);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapmode[smp.WrapModeV]);

                id++;
            }
        }

        public void ApplyMta(FMAA m, int frame)
        {
            foreach (FMAA.MatAnimEntry matEntry in m.matanims)
            {
                foreach (Mesh mesh in Nodes)
                {
                    MaterialData material = mesh.material;

                    if (material.Name == matEntry.Name)
                    {

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
        public static int verNumA, verNumB, verNumC, verNumD, SwitchCheck;

        private const string TEMP_FILE = "temp.bfres";

        public override void Read(string filename)
        {
            FileData f = new FileData(filename);
            f.Endian = Endianness.Big;

            string path = filename;

            int Magic = f.readInt();

            if (Magic == 0x59617A30) //YAZO compressed
            {
                using (FileStream input = new FileStream(path, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Yaz0Compression.Decompress(path, TEMP_FILE);

                    path = TEMP_FILE;
                }
            }
            f = new FileData(path);

            f.seek(0);

            f.seek(4); // magic check
            SwitchCheck = f.readInt(); //Switch version only has padded magic
            verNumD = f.readByte();
            verNumC = f.readByte();
            verNumB = f.readByte();
            verNumA = f.readByte();
            if (SwitchCheck == 0x20202020)
            {
                Nodes.Add(TModels);
                Nodes.Add(TMaterialAnim);
                Nodes.Add(TVisualAnim);
                Nodes.Add(TShapeAnim);
                Nodes.Add(TSceneAnim);
                Nodes.Add(TEmbedded);
                ImageKey = "bfres";
                SelectedImageKey = "bfres";

                TargetSwitchBFRES = new ResNSW.ResFile(path);

                FSKACount = TargetSwitchBFRES.SkeletalAnims.Count;
                FVISCount = TargetSwitchBFRES.BoneVisibilityAnims.Count;
                FMAACount = TargetSwitchBFRES.MaterialAnims.Count;

                Console.WriteLine("Name = " + TargetSwitchBFRES.Name);

                foreach (ResNSW.ExternalFile ext in TargetSwitchBFRES.ExternalFiles)
                {
                    f = new FileData(ext.Data);

                    f.Endian = Endianness.Little;

                    string EmMagic = f.readString(f.pos(), 4);

                    if (EmMagic.Equals("BNTX")) //Textures
                    {
                        int temp = f.pos();
                        BNTX t = new BNTX();
                        t.ReadBNTX(f);
                        TEmbedded.Nodes.Add(t);

                    }
                }

                int ModelCur = 0;
                //FMDLs -Models-
                foreach (ResNSW.Model mdl in TargetSwitchBFRES.Models)
                {
                    FMDL_Model model = new FMDL_Model(); //This will store VBN data and stuff
                    model.Text = mdl.Name;

                    TModels.Nodes.Add(model);

                    model.Node_Array = new int[mdl.Skeleton.MatrixToBoneList.Count];
                    int nodes = 0;
                    foreach (ushort node in mdl.Skeleton.MatrixToBoneList)
                    {
                        model.Node_Array[nodes] = node;
                        nodes++;
                    }

                    foreach (ResNSW.Bone bn in mdl.Skeleton.Bones)
                    {

                        Bone bone = new Bone(model.skeleton);
                        bone.Text = bn.Name;
                        bone.boneId = bn.BillboardIndex;
                        bone.parentIndex = bn.ParentIndex;
                        bone.scale = new float[3];
                        bone.rotation = new float[4];
                        bone.position = new float[3];

                        if (bn.FlagsRotation == ResNSW.BoneFlagsRotation.Quaternion)
                            bone.boneRotationType = 1;
                        else
                            bone.boneRotationType = 0;
                        bone.scale[0] = bn.Scale.X;
                        bone.scale[1] = bn.Scale.Y;
                        bone.scale[2] = bn.Scale.Z;
                        bone.rotation[0] = bn.Rotation.X;
                        bone.rotation[1] = bn.Rotation.Y;
                        bone.rotation[2] = bn.Rotation.Z;
                        bone.rotation[3] = bn.Rotation.W;
                        bone.position[0] = bn.Position.X;
                        bone.position[1] = bn.Position.Y;
                        bone.position[2] = bn.Position.Z;

                        model.skeleton.bones.Add(bone);

                    }
                    model.skeleton.reset();
                    model.skeleton.update();

                    //MeshTime!!
                    foreach (ResNSW.Shape shp in mdl.Shapes)
                    {
                        Mesh poly = new Mesh();
                        poly.Text = shp.Name;
                        poly.MaterialIndex = shp.MaterialIndex;
                        poly.matrFlag = shp.VertexSkinCount;
                        poly.fsklindx = shp.BoneIndex;

                        TModels.Nodes[ModelCur].Nodes.Add(poly);



                        //Create a buffer instance which stores all the buffer data
                        HelperNSW.VertexBufferHelper helper = new HelperNSW.VertexBufferHelper(mdl.VertexBuffers[shp.VertexBufferIndex], TargetSwitchBFRES.ByteOrder);

                        Vertex v = new Vertex();
                        foreach (ResNSW.VertexAttrib att in mdl.VertexBuffers[shp.VertexBufferIndex].Attributes)
                        {
                            if (att.Name == "_p0")
                            {
                                HelperNSW.VertexBufferHelperAttrib position = helper["_p0"];
                                Syroot.Maths.Vector4F[] vec4Positions = position.Data;

                                foreach (Syroot.Maths.Vector4F p in vec4Positions)
                                {
                                    switch (position.Format)
                                    {
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_32_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_16_16_16_16_Single:
                                            v.pos.Add(new Vector3 { X = p.X, Y = p.Y, Z = p.Z });
                                            break;
                                        default:
                                            MessageBox.Show("Something went wrong :(");
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_n0")
                            {
                                HelperNSW.VertexBufferHelperAttrib normal = helper["_n0"];
                                Syroot.Maths.Vector4F[] vec4Normals = normal.Data;

                                foreach (Syroot.Maths.Vector4F n in vec4Normals)
                                {
                                    switch (normal.Format)
                                    {
                                        case GFXNSW.AttribFormat.Format_10_10_10_2_SNorm:
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_16_16_16_16_Single:
                                        case GFXNSW.AttribFormat.Format_8_8_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_16_16_SNorm:
                                        case GFXNSW.AttribFormat.Format_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_32_32_32_Single:
                                            v.nrm.Add(new Vector3 { X = n.X, Y = n.Y, Z = n.Z });
                                            break;
                                        default:
                                            MessageBox.Show("Unsupported Format " + normal.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_u0")
                            {
                                HelperNSW.VertexBufferHelperAttrib uv0 = helper["_u0"];
                                Syroot.Maths.Vector4F[] vec4uv0 = uv0.Data;

                                foreach (Syroot.Maths.Vector4F u in vec4uv0)
                                {
                                    switch (uv0.Format)
                                    {
                                        case GFXNSW.AttribFormat.Format_10_10_10_2_SNorm:
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_UInt:
                                        case GFXNSW.AttribFormat.Format_16_16_UNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_UNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_SNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_Single:
                                        case GFXNSW.AttribFormat.Format_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_32_Single:
                                            v.uv0.Add(new Vector2 { X = u.X, Y = u.Y });
                                            break;
                                        default:
                                            MessageBox.Show("Unsupported Format " + uv0.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_u1")
                            {

                                HelperNSW.VertexBufferHelperAttrib uv1 = helper["_u1"];
                                Syroot.Maths.Vector4F[] vec4uv1 = uv1.Data;

                                foreach (Syroot.Maths.Vector4F u in vec4uv1)
                                {
                                    switch (uv1.Format)
                                    {
                                        case GFXNSW.AttribFormat.Format_10_10_10_2_SNorm:
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_UInt:
                                        case GFXNSW.AttribFormat.Format_16_16_UNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_UNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_SNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_Single:
                                        case GFXNSW.AttribFormat.Format_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_32_Single:
                                            v.uv1.Add(new Vector2 { X = u.X, Y = u.Y });
                                            break;
                                        default:
                                            MessageBox.Show("Unsupported Format for uv1 " + uv1.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_u2")
                            {
                                Console.WriteLine(att.Name);
                                HelperNSW.VertexBufferHelperAttrib uv2 = helper["_u2"];
                                Syroot.Maths.Vector4F[] vec4uv2 = uv2.Data;

                                foreach (Syroot.Maths.Vector4F u in vec4uv2)
                                {
                                    switch (uv2.Format)
                                    {
                                        case GFXNSW.AttribFormat.Format_10_10_10_2_SNorm:
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_UInt:
                                        case GFXNSW.AttribFormat.Format_16_16_UNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_UNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_SNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_Single:
                                        case GFXNSW.AttribFormat.Format_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_32_Single:
                                            v.uv2.Add(new Vector2 { X = u.X, Y = u.Y });
                                            break;
                                        default:
                                            MessageBox.Show("UV 2 has unsupported Format " + uv2.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_c0")
                            {
                                HelperNSW.VertexBufferHelperAttrib c0 = helper["_c0"];
                                Syroot.Maths.Vector4F[] vec4c0 = c0.Data;

                                foreach (Syroot.Maths.Vector4F c in vec4c0)
                                {
                                    switch (c0.Format)
                                    {
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_10_10_10_2_SNorm:
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_UInt:
                                        case GFXNSW.AttribFormat.Format_16_16_16_16_Single:
                                        case GFXNSW.AttribFormat.Format_16_16_UNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_UNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_SNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_Single:
                                        case GFXNSW.AttribFormat.Format_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_32_Single:
                                        case GFXNSW.AttribFormat.Format_8_8_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_8_8_UNorm:
                                            v.col.Add(new Vector4 { X = c.X, Y = c.Y, Z = c.Z, W = c.W });
                                            break;
                                        default:
                                            MessageBox.Show("Unsupported Format for c0 " + c0.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_t0")
                            {
                                HelperNSW.VertexBufferHelperAttrib t0 = helper["_t0"];
                                Syroot.Maths.Vector4F[] vec4t0 = t0.Data;

                                foreach (Syroot.Maths.Vector4F u in vec4t0)
                                {
                                    switch (t0.Format)
                                    {
                                        case GFXNSW.AttribFormat.Format_10_10_10_2_SNorm:
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_UInt:
                                        case GFXNSW.AttribFormat.Format_16_16_UNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_UNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_SNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_Single:
                                        case GFXNSW.AttribFormat.Format_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_32_Single:
                                        case GFXNSW.AttribFormat.Format_8_8_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_8_8_UNorm:
                                            v.tans.Add(new Vector4 { X = u.X, Y = u.Y, Z = u.Z, W = u.W });
                                            break;
                                        default:
                                            MessageBox.Show("Unsupported Format for t0 " + t0.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_b0")
                            {

                                HelperNSW.VertexBufferHelperAttrib b0 = helper["_b0"];
                                Syroot.Maths.Vector4F[] vec4b0 = b0.Data;

                                foreach (Syroot.Maths.Vector4F u in vec4b0)
                                {
                                    switch (b0.Format)
                                    {
                                        case GFXNSW.AttribFormat.Format_10_10_10_2_SNorm:
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_UInt:
                                        case GFXNSW.AttribFormat.Format_16_16_UNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_UNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_SNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_Single:
                                        case GFXNSW.AttribFormat.Format_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_32_Single:
                                        case GFXNSW.AttribFormat.Format_8_8_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_8_8_UNorm:
                                            v.bitans.Add(new Vector4 { X = u.X, Y = u.Y, Z = u.Z, W = u.W });
                                            break;
                                        default:
                                            MessageBox.Show("Unsupported Format " + b0.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_w0")
                            {

                                HelperNSW.VertexBufferHelperAttrib w0 = helper["_w0"];
                                Syroot.Maths.Vector4F[] vec4w0 = w0.Data;

                                foreach (Syroot.Maths.Vector4F w in vec4w0)
                                {
                                    switch (w0.Format)
                                    {
                                        case GFXNSW.AttribFormat.Format_10_10_10_2_SNorm:
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_UInt:
                                        case GFXNSW.AttribFormat.Format_16_16_UNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_UNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_SNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_Single:
                                        case GFXNSW.AttribFormat.Format_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_32_Single:
                                        case GFXNSW.AttribFormat.Format_8_8_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_8_8_UNorm:
                                        case GFXNSW.AttribFormat.Format_32_32_32_UInt:
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_32_32_32_Single:
                                            v.weights.Add(new Vector4 { X = w.X, Y = w.Y, Z = w.Z, W = w.W });
                                            break;
                                        default:
                                            MessageBox.Show("Unsupported Format " + w0.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_i0")
                            {

                                HelperNSW.VertexBufferHelperAttrib i0 = helper["_i0"];
                                Syroot.Maths.Vector4F[] vec4i0 = i0.Data;

                                foreach (Syroot.Maths.Vector4F i in vec4i0)
                                {
                                    switch (i0.Format)
                                    {
                                        case GFXNSW.AttribFormat.Format_10_10_10_2_SNorm:
                                        case GFXNSW.AttribFormat.Format_32_32_32_32_UInt:
                                        case GFXNSW.AttribFormat.Format_16_16_UNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_SNorm:
                                        case GFXNSW.AttribFormat.Format_8_8_UNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_SNorm:
                                        case GFXNSW.AttribFormat.Format_16_16_Single:
                                        case GFXNSW.AttribFormat.Format_32_32_Single:
                                        case GFXNSW.AttribFormat.Format_32_Single:
                                        case GFXNSW.AttribFormat.Format_8_8_8_8_UInt:
                                        case GFXNSW.AttribFormat.Format_8_8_8_8_UNorm:
                                        case GFXNSW.AttribFormat.Format_8_UInt:
                                        case GFXNSW.AttribFormat.Format_8_8_UInt:
                                        case GFXNSW.AttribFormat.Format_32_32_UInt:
                                        case GFXNSW.AttribFormat.Format_32_32_32_UInt:
                                        case GFXNSW.AttribFormat.Format_32_UInt:
                                            v.nodes.Add(new Vector4 { X = i.X, Y = i.Y, Z = i.Z, W = i.W });
                                            break;
                                        default:
                                            MessageBox.Show("Unsupported Format " + i0.Format);
                                            break;
                                    }
                                }
                            }
                        }
                        poly.vertices = v;

                      //  int LODCount = shp.Meshes.Count - 1; //For going to the lowest poly LOD mesh

                        int LODCount = 0;

                        uint FaceCount = FaceCount = shp.Meshes[LODCount].IndexCount;

                        uint[] indicesArray = shp.Meshes[LODCount].GetIndices().ToArray();

                        for (int face = 0; face < FaceCount; face++)
                        {
                            poly.faces.Add((int)indicesArray[face] + (int)shp.Meshes[LODCount].FirstVertex);
                        }

                        int AlbedoCount = 0;

                        string TextureName = "";
                        int id = 0;



                        foreach (ResNSW.TextureRef tex in mdl.Materials[shp.MaterialIndex].TextureRefs)
                        {
                            TextureName = mdl.Materials[shp.MaterialIndex].TextureRefs[id].Name;
                            //Need to have default texture load instead of random texture if texture doesn't exist within the tex index

                            //    model.mat[shp.MaterialIndex].TextureMaps.Add(TextureName);

                            MatTexture texture = new MatTexture();

                            bool IsAlbedo = HackyTextureList.Any(TextureName.Contains);

                            //This works decently for now. I tried samplers but Kirby Star Allies doesn't map with samplers properly? 
                            if (IsAlbedo)
                            {
                                if (AlbedoCount == 0)
                                {
                                    try
                                    {
                                        poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                        AlbedoCount++;
                                    }
                                    catch
                                    {
                                        poly.texHashs.Add(0);
                                    }
                                    poly.TextureMapTypes.Add("Diffuse");
                                }
                            }
                            else if (TextureName.Contains("Nrm") || TextureName.Contains("Norm") || TextureName.Contains("norm") || TextureName.Contains("nrm"))
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(1);
                                }
                                poly.material.HasNormalMap = true;
                                poly.TextureMapTypes.Add("Normal");
                            }
                            else if (TextureName.Contains("Emm"))
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(2);
                                }
                                poly.TextureMapTypes.Add("EmissionMap");
                            }
                            else if (TextureName.Contains("Spm"))
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(2);
                                }
                                poly.TextureMapTypes.Add("SpecularMap");
                            }
                            else if (TextureName.Contains("b00"))
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(2);
                                }
                                poly.TextureMapTypes.Add("Bake1");
                            }
                            else if (TextureName.Contains("b01") || TextureName.Contains("Moc"))
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(3);
                                }
                                poly.TextureMapTypes.Add("Bake2");
                            }
                            else if (TextureName.Contains("MRA")) //Metalness, Roughness, and Cavity Map in one
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(7);
                                }
                                poly.TextureMapTypes.Add("MRA");
                            }
                            else if (TextureName.Contains("mtl"))
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(8);
                                }
                                poly.TextureMapTypes.Add("Metalness");
                            }
                            else if (TextureName.Contains("rgh"))
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(9);
                                }
                                poly.TextureMapTypes.Add("Roughness");
                            }
                            texture.Name = TextureName;
                            id++;
                        }

                        poly.material.Name = mdl.Materials[shp.MaterialIndex].Name;

                        foreach (ResNSW.Sampler smp in mdl.Materials[shp.MaterialIndex].Samplers)
                        {
                            SamplerInfo s = new SamplerInfo();
                            s.WrapModeU = (int)smp.WrapModeU;
                            s.WrapModeV = (int)smp.WrapModeV;
                            s.WrapModeW = (int)smp.WrapModeW;
                            poly.material.samplerinfo.Add(s);
                        }


                        foreach (ResNSW.RenderInfo rnd in mdl.Materials[shp.MaterialIndex].RenderInfos)
                        {
                            RenderInfoData r = new RenderInfoData();

                            r.Name = rnd.Name;

                            switch (rnd.Type)
                            {
                                case ResNSW.RenderInfoType.Int32:
                                    foreach (int rn in rnd.GetValueInt32s())
                                    {
                                        r.Value_Int = rn;
                                    }
                                    break;
                                case ResNSW.RenderInfoType.Single:
                                    foreach (float rn in rnd.GetValueSingles())
                                    {
                                        r.Value_Float = rn;
                                    }
                                    break;
                                case ResNSW.RenderInfoType.String:
                                    foreach (string rn in rnd.GetValueStrings())
                                    {
                                        r.Value_String = rn;
                                    }
                                    break;
                            }

                            poly.material.renderinfo.Add(r);
                        }

                        using (Syroot.BinaryData.BinaryDataReader reader = new Syroot.BinaryData.BinaryDataReader(new MemoryStream(mdl.Materials[shp.MaterialIndex].ShaderParamData)))
                        {

                            reader.ByteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
                            foreach (Syroot.NintenTools.NSW.Bfres.ShaderParam param in mdl.Materials[shp.MaterialIndex].ShaderParams)
                            {
                                ShaderParam prm = new ShaderParam();

                                switch (param.Type)
                                {
                                    case ResNSW.ShaderParamType.Float:
                                        reader.Seek(param.DataOffset, SeekOrigin.Begin);
                                        prm.Value_float = reader.ReadSingle();
                                        break;
                                    case ResNSW.ShaderParamType.Float2:
                                        reader.Seek(param.DataOffset, SeekOrigin.Begin);
                                        prm.Value_float2 = new Vector2(
                                            reader.ReadSingle(),
                                            reader.ReadSingle());
                                        break;
                                    case ResNSW.ShaderParamType.Float3:
                                        reader.Seek(param.DataOffset, SeekOrigin.Begin);
                                        prm.Value_float3 = new Vector3(
                                                reader.ReadSingle(),
                                                reader.ReadSingle(),
                                                reader.ReadSingle()); break;
                                    case ResNSW.ShaderParamType.Float4:
                                        reader.Seek(param.DataOffset, SeekOrigin.Begin);
                                        prm.Value_float4 = new Vector4(
                                                reader.ReadSingle(),
                                                reader.ReadSingle(),
                                                reader.ReadSingle(),
                                                reader.ReadSingle()); break;
                                }
                                poly.material.matparam.Add(param.Name, prm);
                            }
                            reader.Close();
                        }
                        model.poly.Add(poly);
                    }
                    models.Add(model);
                    ModelCur++;
                }
            }
            else
            {
                f.Endian = Endianness.Big;
                f.eof();

                Nodes.Add(TModels);
                //    Nodes.Add(SkeletalAnim);  Gets added to anim panel on left so this not neeeded
                Nodes.Add(TTextures);
                Nodes.Add(TShaderparam);
                Nodes.Add(TColoranim);
                Nodes.Add(TTextureSRT);
                Nodes.Add(TTexturePat);
                Nodes.Add(TBonevisabilty);
                Nodes.Add(TVisualAnim);
                Nodes.Add(TShapeAnim);
                Nodes.Add(TSceneAnim);
                Nodes.Add(TEmbedded);
                ImageKey = "bfres";
                SelectedImageKey = "bfres";



                TargetWiiUBFRES = new ResFile(path);

                FSKACount = TargetWiiUBFRES.SkeletalAnims.Count;

                textures.Clear();

                foreach (Texture tex in TargetWiiUBFRES.Textures.Values)
                {
                    string TextureName = tex.Name;
                    FTEX texture = new FTEX();
                    texture.ReadFTEX(tex);
                    textures.Add(TextureName, texture);
                    TTextures.Nodes.Add(texture);
                }

                int ModelCur = 0;
                //FMDLs -Models-
                foreach (Model mdl in TargetWiiUBFRES.Models.Values)
                {
                    FMDL_Model model = new FMDL_Model(); //This will store VBN data and stuff
                    model.Text = mdl.Name;

                    TModels.Nodes.Add(model);

                    model.Node_Array = new int[mdl.Skeleton.MatrixToBoneList.Count];
                    int nodes = 0;
                    foreach (ushort node in mdl.Skeleton.MatrixToBoneList)
                    {
                        model.Node_Array[nodes] = node;
                        nodes++;
                    }

                    foreach (Syroot.NintenTools.Bfres.Bone bn in mdl.Skeleton.Bones.Values)
                    {

                        Bone bone = new Bone(model.skeleton);
                        bone.Text = bn.Name;
                        bone.boneId = bn.BillboardIndex;
                        bone.parentIndex = bn.ParentIndex;
                        bone.scale = new float[3];
                        bone.rotation = new float[4];
                        bone.position = new float[3];

                        if (bn.FlagsRotation == BoneFlagsRotation.Quaternion)
                            bone.boneRotationType = 1;
                        else
                            bone.boneRotationType = 0;
                        
                        bone.scale[0] = bn.Scale.X;
                        bone.scale[1] = bn.Scale.Y;
                        bone.scale[2] = bn.Scale.Z;
                        bone.rotation[0] = bn.Rotation.X;
                        bone.rotation[1] = bn.Rotation.Y;
                        bone.rotation[2] = bn.Rotation.Z;
                        bone.rotation[3] = bn.Rotation.W;
                        bone.position[0] = bn.Position.X;
                        bone.position[1] = bn.Position.Y;
                        bone.position[2] = bn.Position.Z;

                        model.skeleton.bones.Add(bone);

                    }
                    model.skeleton.reset();
                    model.skeleton.update();

                    //MeshTime!!
                    int ShapeCur = 0;
                    foreach (Shape shp in mdl.Shapes.Values)
                    {
                        Mesh poly = new Mesh();
                        poly.Text = shp.Name;
                        poly.MaterialIndex = shp.MaterialIndex;
                        poly.matrFlag = shp.VertexSkinCount;
                        poly.fsklindx = shp.BoneIndex;

                        TModels.Nodes[ModelCur].Nodes.Add(poly);

                        //Create a buffer instance which stores all the buffer data
                        VertexBufferHelper helper = new VertexBufferHelper(mdl.VertexBuffers[shp.VertexBufferIndex], TargetWiiUBFRES.ByteOrder);

                        // VertexBufferHelperAttrib uv1 = helper["_u1"];


                        Vertex v = new Vertex();
                        foreach (VertexAttrib att in mdl.VertexBuffers[shp.VertexBufferIndex].Attributes.Values)
                        {
                            if (att.Name == "_p0")
                            {
                                Console.WriteLine(att.Name);
                                VertexBufferHelperAttrib position = helper["_p0"];
                                Syroot.Maths.Vector4F[] vec4Positions = position.Data;

                                foreach (Syroot.Maths.Vector4F p in vec4Positions)
                                {
                                    switch (position.Format)
                                    {
                                        case GX2AttribFormat.Format_32_32_32_32_Single:
                                        case GX2AttribFormat.Format_32_32_32_Single:
                                        case GX2AttribFormat.Format_16_16_16_16_Single:
                                            v.pos.Add(new Vector3 { X = p.X, Y = p.Y, Z = p.Z });
                                            break;
                                        default:
                                            MessageBox.Show("Position has unsupported Format " + position.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_n0")
                            {
                                Console.WriteLine(att.Name);
                                VertexBufferHelperAttrib normal = helper["_n0"];
                                Syroot.Maths.Vector4F[] vec4Normals = normal.Data;

                                foreach (Syroot.Maths.Vector4F n in vec4Normals)
                                {
                                    switch (normal.Format)
                                    {
                                        case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                        case GX2AttribFormat.Format_32_32_32_32_Single:
                                        case GX2AttribFormat.Format_16_16_16_16_Single:
                                        case GX2AttribFormat.Format_8_8_8_8_SNorm:
                                        case GX2AttribFormat.Format_16_16_16_16_SNorm:
                                        case GX2AttribFormat.Format_32_32_Single:
                                        case GX2AttribFormat.Format_32_32_32_Single:
                                            v.nrm.Add(new Vector3 { X = n.X, Y = n.Y, Z = n.Z });
                                            break;
                                        default:
                                            MessageBox.Show("Normals has unsupported Format " + normal.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_u0")
                            {
                                Console.WriteLine(att.Name);
                                VertexBufferHelperAttrib uv0 = helper["_u0"];
                                Syroot.Maths.Vector4F[] vec4uv0 = uv0.Data;

                                foreach (Syroot.Maths.Vector4F u in vec4uv0)
                                {
                                    switch (uv0.Format)
                                    {
                                        case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                        case GX2AttribFormat.Format_32_32_32_32_UInt:
                                        case GX2AttribFormat.Format_16_16_UNorm:
                                        case GX2AttribFormat.Format_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_UNorm:
                                        case GX2AttribFormat.Format_16_16_SNorm:
                                        case GX2AttribFormat.Format_16_16_Single:
                                        case GX2AttribFormat.Format_32_32_Single:
                                        case GX2AttribFormat.Format_32_Single:
                                            v.uv0.Add(new Vector2 { X = u.X, Y = u.Y });
                                            break;
                                        default:
                                            MessageBox.Show("UV 0 has unsupported Format " + uv0.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_u1")
                            {
                                Console.WriteLine(att.Name);
                                VertexBufferHelperAttrib uv1 = helper["_u1"];
                                Syroot.Maths.Vector4F[] vec4uv1 = uv1.Data;

                                foreach (Syroot.Maths.Vector4F u in vec4uv1)
                                {
                                    switch (uv1.Format)
                                    {
                                        case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                        case GX2AttribFormat.Format_32_32_32_32_UInt:
                                        case GX2AttribFormat.Format_16_16_UNorm:
                                        case GX2AttribFormat.Format_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_UNorm:
                                        case GX2AttribFormat.Format_16_16_SNorm:
                                        case GX2AttribFormat.Format_16_16_Single:
                                        case GX2AttribFormat.Format_32_32_Single:
                                        case GX2AttribFormat.Format_32_Single:
                                            v.uv1.Add(new Vector2 { X = u.X, Y = u.Y });
                                            break;
                                        default:
                                            MessageBox.Show("UV 1 has unsupported Format " + uv1.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_u2")
                            {
                                Console.WriteLine(att.Name);
                                VertexBufferHelperAttrib uv2 = helper["_u2"];
                                Syroot.Maths.Vector4F[] vec4uv2 = uv2.Data;

                                foreach (Syroot.Maths.Vector4F u in vec4uv2)
                                {
                                    switch (uv2.Format)
                                    {
                                        case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                        case GX2AttribFormat.Format_32_32_32_32_UInt:
                                        case GX2AttribFormat.Format_16_16_UNorm:
                                        case GX2AttribFormat.Format_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_UNorm:
                                        case GX2AttribFormat.Format_16_16_SNorm:
                                        case GX2AttribFormat.Format_16_16_Single:
                                        case GX2AttribFormat.Format_32_32_Single:
                                        case GX2AttribFormat.Format_32_Single:
                                            v.uv2.Add(new Vector2 { X = u.X, Y = u.Y });
                                            break;
                                        default:
                                            MessageBox.Show("UV 2 has unsupported Format " + uv2.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_c0")
                            {
                                Console.WriteLine(att.Name);
                                VertexBufferHelperAttrib c0 = helper["_c0"];
                                Syroot.Maths.Vector4F[] vec4c0 = c0.Data;

                                foreach (Syroot.Maths.Vector4F c in vec4c0)
                                {
                                    switch (c0.Format)
                                    {
                                        case GX2AttribFormat.Format_32_32_32_32_Single:
                                        case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                        case GX2AttribFormat.Format_32_32_32_32_UInt:
                                        case GX2AttribFormat.Format_16_16_16_16_Single:
                                        case GX2AttribFormat.Format_16_16_UNorm:
                                        case GX2AttribFormat.Format_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_UNorm:
                                        case GX2AttribFormat.Format_16_16_SNorm:
                                        case GX2AttribFormat.Format_16_16_Single:
                                        case GX2AttribFormat.Format_32_32_Single:
                                        case GX2AttribFormat.Format_32_Single:
                                        case GX2AttribFormat.Format_8_8_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_8_8_UNorm:
                                            v.col.Add(new Vector4 { X = c.X, Y = c.Y, Z = c.Z, W = c.W });
                                            break;
                                        default:
                                            MessageBox.Show("Color has unsupported Format " + c0.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_t0")
                            {
                                Console.WriteLine(att.Name);
                                VertexBufferHelperAttrib t0 = helper["_t0"];
                                Syroot.Maths.Vector4F[] vec4t0 = t0.Data;

                                foreach (Syroot.Maths.Vector4F u in vec4t0)
                                {
                                    switch (t0.Format)
                                    {
                                        case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                        case GX2AttribFormat.Format_32_32_32_32_UInt:
                                        case GX2AttribFormat.Format_16_16_UNorm:
                                        case GX2AttribFormat.Format_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_UNorm:
                                        case GX2AttribFormat.Format_16_16_SNorm:
                                        case GX2AttribFormat.Format_16_16_Single:
                                        case GX2AttribFormat.Format_32_32_Single:
                                        case GX2AttribFormat.Format_32_Single:
                                        case GX2AttribFormat.Format_8_8_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_8_8_UNorm:
                                            v.tans.Add(new Vector4 { X = u.X, Y = u.Y, Z = u.Z, W = u.W });
                                            break;
                                        default:
                                            MessageBox.Show("tangents have unsupported Format " + t0.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_b0")
                            {
                                Console.WriteLine(att.Name);
                                VertexBufferHelperAttrib b0 = helper["_b0"];
                                Syroot.Maths.Vector4F[] vec4b0 = b0.Data;

                                foreach (Syroot.Maths.Vector4F u in vec4b0)
                                {
                                    switch (b0.Format)
                                    {
                                        case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                        case GX2AttribFormat.Format_32_32_32_32_UInt:
                                        case GX2AttribFormat.Format_16_16_UNorm:
                                        case GX2AttribFormat.Format_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_UNorm:
                                        case GX2AttribFormat.Format_16_16_SNorm:
                                        case GX2AttribFormat.Format_16_16_Single:
                                        case GX2AttribFormat.Format_32_32_Single:
                                        case GX2AttribFormat.Format_32_Single:
                                        case GX2AttribFormat.Format_8_8_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_8_8_UNorm:
                                            v.bitans.Add(new Vector4 { X = u.X, Y = u.Y, Z = u.Z, W = u.W });
                                            break;
                                        default:
                                            MessageBox.Show("binorms has unsupported Format " + b0.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_w0")
                            {
                                Console.WriteLine(att.Name);
                                VertexBufferHelperAttrib w0 = helper["_w0"];
                                Syroot.Maths.Vector4F[] vec4w0 = w0.Data;

                                foreach (Syroot.Maths.Vector4F w in vec4w0)
                                {
                                    switch (w0.Format)
                                    {
                                        case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                        case GX2AttribFormat.Format_32_32_32_32_UInt:
                                        case GX2AttribFormat.Format_16_16_UNorm:
                                        case GX2AttribFormat.Format_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_UNorm:
                                        case GX2AttribFormat.Format_16_16_SNorm:
                                        case GX2AttribFormat.Format_16_16_Single:
                                        case GX2AttribFormat.Format_32_32_Single:
                                        case GX2AttribFormat.Format_32_Single:
                                        case GX2AttribFormat.Format_8_8_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_8_8_UNorm:
                                        case GX2AttribFormat.Format_32_32_32_UInt:
                                        case GX2AttribFormat.Format_32_32_32_32_Single:
                                        case GX2AttribFormat.Format_32_32_32_Single:
                                            v.weights.Add(new Vector4 { X = w.X, Y = w.Y, Z = w.Z, W = w.W });
                                            break;
                                        default:
                                            MessageBox.Show("weights has unsupported Format " + w0.Format);
                                            break;
                                    }
                                }
                            }
                            if (att.Name == "_i0")
                            {
                                Console.WriteLine(att.Name);
                                VertexBufferHelperAttrib i0 = helper["_i0"];
                                Syroot.Maths.Vector4F[] vec4i0 = i0.Data;

                                foreach (Syroot.Maths.Vector4F i in vec4i0)
                                {
                                    switch (i0.Format)
                                    {
                                        case GX2AttribFormat.Format_10_10_10_2_SNorm:
                                        case GX2AttribFormat.Format_32_32_32_32_UInt:
                                        case GX2AttribFormat.Format_16_16_UNorm:
                                        case GX2AttribFormat.Format_8_8_SNorm:
                                        case GX2AttribFormat.Format_8_8_UNorm:
                                        case GX2AttribFormat.Format_16_16_SNorm:
                                        case GX2AttribFormat.Format_16_16_Single:
                                        case GX2AttribFormat.Format_32_32_Single:
                                        case GX2AttribFormat.Format_32_Single:
                                        case GX2AttribFormat.Format_8_8_8_8_UInt:
                                        case GX2AttribFormat.Format_8_8_8_8_UNorm:
                                        case GX2AttribFormat.Format_8_UInt:
                                        case GX2AttribFormat.Format_8_8_UInt:
                                        case GX2AttribFormat.Format_32_32_UInt:
                                        case GX2AttribFormat.Format_32_32_32_UInt:
                                        case GX2AttribFormat.Format_32_UInt:
                                        case GX2AttribFormat.Format_16_16_UInt:
                                            v.nodes.Add(new Vector4 { X = i.X, Y = i.Y, Z = i.Z, W = i.W });
                                            break;
                                        default:
                                            MessageBox.Show("indices has unsupported Format " + i0.Format);
                                            break;
                                    }
                                }
                            }
                        }
                        poly.vertices = v;

                        //shp.Meshes.Count - 1 //For going to the lowest poly LOD mesh

                        int LODCount = 0;

                        uint FaceCount = FaceCount = shp.Meshes[LODCount].IndexCount;


                        uint[] indicesArray = shp.Meshes[LODCount].GetIndices().ToArray();

                        for (int face = 0; face < FaceCount; face++)
                        {
                            poly.faces.Add((int)indicesArray[face] + (int)shp.Meshes[LODCount].FirstVertex);
                        }

                        int AlbedoCount = 0;

                        string TextureName = "";
                        int id = 0;
                        foreach (TextureRef tex in mdl.Materials[shp.MaterialIndex].TextureRefs)
                        {
                            TextureName = tex.Name;

                            MatTexture texture = new MatTexture();

                            poly.Nodes.Add(new TreeNode { Text = TextureName });

                            if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_a0")
                            {
                                if (AlbedoCount == 0) 
                                {
                                    try
                                    {
                                        poly.texHashs.Add(textures[TextureName].texture.display);
                                        AlbedoCount++;
                                    }
                                    catch
                                    {
                                        poly.texHashs.Add(0);
                                    }
                                    poly.TextureMapTypes.Add("Diffuse");
                                }
                            }
                            if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_a1")
                            {
                                try
                                {
                                    poly.texHashs.Add(textures[TextureName].texture.display);
                                    AlbedoCount++;
                                }
                                catch
                                {
                                    poly.texHashs.Add(0);
                                }
                                poly.TextureMapTypes.Add("Diffuse_Layer");
                            }
                            if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_n0")
                            {
                                try
                                {
                                    poly.texHashs.Add(textures[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(1);
                                }
                                poly.material.HasNormalMap = true;
                                poly.TextureMapTypes.Add("Normal");
                            }
                            if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_b0")
                            {
                                try
                                {
                                    poly.texHashs.Add(textures[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(2);
                                }
                                poly.TextureMapTypes.Add("Bake1");
                            }
                            if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_b1")
                            {
                                try
                                {
                                    poly.texHashs.Add(textures[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(3);
                                }
                                poly.TextureMapTypes.Add("Bake2");
                            }
                            id++;
                            texture.Name = TextureName;
                        }

                        poly.material.Name = mdl.Materials[shp.MaterialIndex].Name;

                        foreach (Sampler smp in mdl.Materials[shp.MaterialIndex].Samplers.Values)
                        {
                            SamplerInfo s = new SamplerInfo();
                            s.WrapModeU = (int)smp.TexSampler.ClampX;
                            s.WrapModeV = (int)smp.TexSampler.ClampY;
                            s.WrapModeW = (int)smp.TexSampler.ClampZ;
                            poly.material.samplerinfo.Add(s);
                        }

                        using (Syroot.BinaryData.BinaryDataReader reader = new Syroot.BinaryData.BinaryDataReader(new MemoryStream(mdl.Materials[shp.MaterialIndex].ShaderParamData)))
                        {
                            reader.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;
                            foreach (Syroot.NintenTools.Bfres.ShaderParam param in mdl.Materials[shp.MaterialIndex].ShaderParams.Values)
                            {
                                ShaderParam prm = new ShaderParam();

                                switch (param.Type)
                                {
                                    case ShaderParamType.Float:
                                        reader.Seek(param.DataOffset, SeekOrigin.Begin);
                                        prm.Value_float = reader.ReadSingle();
                                        break;
                                    case ShaderParamType.Float2:
                                        reader.Seek(param.DataOffset, SeekOrigin.Begin);
                                        prm.Value_float2 = new Vector2(
                                            reader.ReadSingle(),
                                            reader.ReadSingle());
                                        break;
                                    case ShaderParamType.Float3:
                                        reader.Seek(param.DataOffset, SeekOrigin.Begin);
                                        prm.Value_float3 = new Vector3(
                                                reader.ReadSingle(),
                                                reader.ReadSingle(),
                                                reader.ReadSingle()); break;
                                    case ShaderParamType.Float4:
                                        reader.Seek(param.DataOffset, SeekOrigin.Begin);
                                        prm.Value_float4 = new Vector4(
                                                reader.ReadSingle(),
                                                reader.ReadSingle(),
                                                reader.ReadSingle(),
                                                reader.ReadSingle()); break;
                                }
                                poly.material.matparam.Add(param.Name, prm);
                            }
                            reader.Close();
                        }
                        model.poly.Add(poly);
                        ShapeCur++;
                    }
                    models.Add(model);
                    ModelCur++;
                }
            }
        }

        public void SaveFile(string FileName)
        {

            TargetWiiUBFRES.Save(FileName);
        }

        public override byte[] Rebuild()
        {
            throw new Exception("Unsupported atm :(");
        }

        public class Vertex
        {
            public List<Vector3> pos = new List<Vector3>();
            public List<Vector3> nrm = new List<Vector3>();
            public List<Vector4> col = new List<Vector4>();
            public List<Vector2> uv0 = new List<Vector2>();
            public List<Vector2> uv1 = new List<Vector2>();
            public List<Vector2> uv2 = new List<Vector2>();
            public List<Vector4> tans = new List<Vector4>();
            public List<Vector4> bitans = new List<Vector4>();
            public List<Vector4> nodes = new List<Vector4>();
            public List<Vector4> weights = new List<Vector4>();
            public List<Vector4> nodes1 = new List<Vector4>();
            public List<Vector4> weights1 = new List<Vector4>();
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

            public static int Size = 4 * (3 + 3 + 3 + 3 + 2 + 4 + 4 + 4 + 2 + 2);
        }

        public class Mesh : TreeNode
        {
            public List<int> faces = new List<int>();
            public Vertex vertices = new Vertex();
            public List<int> texHashs = new List<int>();
            public string name;
            public int MaterialIndex;
            public List<string> TextureMapTypes = new List<string>();
            public int[] SkinIndexList;
            public int matrFlag;
            public int[] BoneFixNode;
            public int fsklindx;
            public int[] Faces;
            public MaterialData material = new MaterialData();

            public int strip = 0x40;

            public int displayFaceSize = 0;

            public Mesh()
            {
                Checked = true;
                ImageKey = "mesh";
                SelectedImageKey = "mesh";
            }
            // for drawing
            public bool isTransparent = false;
            public int[] display;
            public int[] selectedVerts;
            public int Offset; // For Rendering

            public List<DisplayVertex> CreateDisplayVertices()
            {
                // rearrange faces
                display = getDisplayFace().ToArray();

                List<DisplayVertex> displayVertList = new List<DisplayVertex>();

                if (faces.Count <= 3)
                    return displayVertList;
                Vertex v = vertices;
                for (int p = 0; p < v.pos.Count; p++)
                {
                    DisplayVertex displayVert = new DisplayVertex()
                    {
                        pos = v.pos.Count > 0 ? v.pos[p] : new Vector3(0, 0, 0),
                        nrm = v.nrm.Count > 0 ? v.nrm[p] : new Vector3(0, 0, 0),
                        tan = v.tans.Count > 0 ? v.tans[p].Xyz : new Vector3(0, 0, 0),
                        bit = v.bitans.Count > 0 ? v.bitans[p].Xyz : new Vector3(0, 0, 0),
                        col = v.col.Count > 0 ? v.col[p] : new Vector4(0.9f, 0.9f, 0.9f, 0.9f),
                        uv = v.uv0.Count > 0 ? v.uv0[p] : new Vector2(0, 0),
                        uv2 = v.uv1.Count > 0 ? v.uv1[p] : new Vector2(0, 0),
                        uv3 = v.uv2.Count > 0 ? v.uv2[p] : new Vector2(0, 0),
                        node = v.nodes.Count > 0 ? v.nodes[p] : new Vector4(-1, 0, 0, 0),
                        weight = v.weights.Count > 0 ? v.weights[p] : new Vector4(0, 0, 0, 0),
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

        public class MaterialData
        {
            public string Name;

            public List<MatTexture> textures = new List<MatTexture>();
            public List<RenderInfoData> renderinfo = new List<RenderInfoData>();
            public List<SamplerInfo> samplerinfo = new List<SamplerInfo>();
            public Dictionary<string, string[]> shaderassign = new Dictionary<string, string[]>();
            public Dictionary<string, ShaderParam> matparam = new Dictionary<string, ShaderParam>();

            // Texture Maps
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
                m.ShaderAssign = new ShaderAssign();
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
            public int minFilter = 3;
            public int magFilter = 2;
            public int mipDetail = 6;
            public int unknown = 0;
            public string Name;
            public string Type;

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
                t.hash       = hash;
                t.mapMode    = mapMode;
                t.wrapModeS  = wrapModeS;
                t.wrapModeT  = wrapModeT;
                t.minFilter  = minFilter;
                t.magFilter  = magFilter;
                t.mipDetail  = mipDetail;
                t.unknown    = unknown;
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
            public Vector4 Value_float4;
            public Vector3 Value_float3;
            public Vector2 Value_float2;
            public float Value_float;
            public bool Value_bool;
        }

        public class RenderInfoData
        {
            public string Name;
            public long DataOffset;
            public int Type;
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
        static Dictionary<int, TextureWrapMode> wrapmode = new Dictionary<int, TextureWrapMode>(){
                    { 0x00, TextureWrapMode.Repeat},
                    { 0x01, TextureWrapMode.MirroredRepeat},
                    { 0x02, TextureWrapMode.ClampToEdge}
        };
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
            private VBN vbn = new VBN();
            public List<Mesh> poly = new List<Mesh>();
            public bool isVisible = true;
            public int[] Node_Array;

        }

        public void readFSKA()
        {

        }


    }
}