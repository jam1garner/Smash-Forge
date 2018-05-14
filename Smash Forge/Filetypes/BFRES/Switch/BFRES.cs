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

namespace Smash_Forge
{
    public class BFRES : FileBase
    {
        public override Endianness Endian { get; set; }

        public List<string> stringContainer = new List<string>();
        public List<FMDL_Model> models = new List<FMDL_Model>();
        public List<WU.FMDL_Model> WiiU_models = new List<WU.FMDL_Model>();
        public static Shader shader = null;
        public List<Vector3> BoneFixTrans = new List<Vector3>();
        public List<Vector3> BoneFixRot = new List<Vector3>();
        public List<Vector3> BoneFixScale = new List<Vector3>();
        public Matrix4[] sb;
        public bool IsWiiU = false;

        public Dictionary<string, FTEX> textures = new Dictionary<string, FTEX>();
        public int FSKACount;


        public TreeNode Models = new TreeNode() { Text = "Models", Checked = true };
        public TreeNode MaterialAnim = new TreeNode() { Text = "Material Animations" };
        public TreeNode SkeletalAnim = new TreeNode() { Text = "Skeletal Animations" };
        public TreeNode VisualAnim = new TreeNode() { Text = "Visual Animations" };
        public TreeNode ShapeAnim = new TreeNode() { Text = "Shape Animations" };
        public TreeNode SceneAnim = new TreeNode() { Text = "Scene Animations" };
        public TreeNode Embedded = new TreeNode() { Text = "Embedded Files" };

        public ResFile TargetWiiUBFRES;

        public TreeNode Textures = new TreeNode() { Text = "Textures" };
        public TreeNode Shaderparam = new TreeNode() { Text = "Shader Param Animations" };
        public TreeNode Coloranim = new TreeNode() { Text = "Color Animations" };
        public TreeNode TextureSRT = new TreeNode() { Text = "Texture STR Animations" };
        public TreeNode TexturePat = new TreeNode() { Text = "Texture Pattern Animations" };
        public TreeNode Bonevisabilty = new TreeNode() { Text = "Bone Visabilty" };


        //Kirby star allies makes it impossible to texture map without REing the shaders so i'll do them by texture list
        public List<string> HackyTextureList = new List<string>(new string[] {
            "Alb", "alb", "Base", "base", "bonbon.167300917","Eye.00","EyeIce.00", "FaceDummy", "Eye01.17", "Dee.00",
            "rainbow.758540574", "Mucus._1700670200", "Eye.11", "CapTail00"

        });

        public static int sign10Bit(int i)
        {
            if (((i >> 9) & 0x1) == 1)
            {
                i = ~i;
                i = i & 0x3FF;
                i += 1;
                i *= -1;
            }

            return i;
        }

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
        }

        public void Destroy()
        {
            GL.DeleteBuffer(vbo_position);
            GL.DeleteBuffer(vbo_color);
            GL.DeleteBuffer(vbo_nrm);
            GL.DeleteBuffer(vbo_uv0);
            GL.DeleteBuffer(vbo_uv1);
            GL.DeleteBuffer(vbo_weight);
            GL.DeleteBuffer(vbo_weight1);
            GL.DeleteBuffer(vbo_bone);
            GL.DeleteBuffer(vbo_bone1);
            GL.DeleteBuffer(vbo_tan);
            GL.DeleteBuffer(vbo_bitan);
        }
        public void PreRender()
        {
            List<Vector3> vert = new List<Vector3>();
            List<Vector2> u0 = new List<Vector2>();
            List<Vector2> u1 = new List<Vector2>();
            List<Vector4> col = new List<Vector4>();
            List<Vector3> nrm = new List<Vector3>();
            List<Vector4> bone = new List<Vector4>();
            List<Vector4> bone1 = new List<Vector4>();
            List<Vector4> weight = new List<Vector4>();
            List<Vector4> weight1 = new List<Vector4>();
            List<Vector3> tangent = new List<Vector3>();
            List<Vector3> bitangent = new List<Vector3>();
            List<int> face = new List<int>();

            int i = 0;
            foreach (FMDL_Model fmdl in models)
            {
                foreach (Mesh m in fmdl.poly)
                {
                    // Console.WriteLine(m.vertices.Count);
                    // Console.WriteLine(m.faces.Count);
                    if (m.faces.Count <= 3)
                        continue;
                    foreach (Vertex v in m.vertices)
                    {
                        vert.Add(v.pos);
                        col.Add(new Vector4(v.col.X * v.col.W, v.col.Y * v.col.W, v.col.Z * v.col.W, 1f));
                        // col.Add(new Vector4(0.9f, 0.9f, 0.9f, 1f)); //Vertex colors disabled atm due to some being used for other effects making the model black
                        nrm.Add(v.nrm);

                        //First UV Map
                        if (v.tx.Count > 0)
                        {
                            u0.Add(v.tx[0]);
                        }
                        else
                        {
                            u0.Add(new Vector2(0, 0));
                        }
                        //Second UV Map
                        if (v.tx.Count > 1)
                        {
                            u1.Add(v.tx[1]);
                        }
                        else
                        {
                            u1.Add(new Vector2(0, 0));
                        }
                        //Tangents
                        if (v.tx.Count > 0)
                        {
                            tangent.Add(new Vector3(v.tan.X, v.tan.Y, v.tan.Z)); //Only vec3, idk what 4th value does
                                                                                 //      Console.WriteLine(v.tan.X + " " + v.tan.Y + " " + v.tan.Z);
                        }
                        else
                        {
                            tangent.Add(new Vector3(0, 0, 0));
                        }
                        //Bitangents
                        if (v.tx.Count > 0)
                        {
                            bitangent.Add(new Vector3(v.bitan.X, v.bitan.Y, v.bitan.Z)); //Only vec3, idk what 4th value does
                        }
                        else
                        {
                            bitangent.Add(new Vector3(0, 0, 0));
                        }
                        //Bone Indicies
                        while (v.node.Count < 4)
                        {
                            v.node.Add(0);
                            v.weight.Add(0);
                        }

                        if (v.weight[0] + v.weight[1] + v.weight[2] + v.weight[3] != 0)
                        {
                            weight.Add(new Vector4(v.weight[0], v.weight[1], v.weight[2], v.weight[3]));
                        }
                        else
                        {
                            weight.Add(new Vector4(1, 1, 1, 1));
                        }

                        if (v.node[0] + v.node[1] + v.node[2] + v.node[3] != -1)
                        {
                            bone.Add(new Vector4(v.node[0], v.node[1], v.node[2], v.node[3]));
                        }
                        else
                        {
                            bone.Add(new Vector4(0, 0, 0, 0));
                        }

                        //For more than 4 bones weighed, they get stored in 2 more buffers, w1, and i1 giving the possibilty of 5-8 weights per vertice

                        //Very few models use this. I've only seen one (Zelda in BOTW) use this

                        while (v.node1.Count < 4)
                        {
                            v.node1.Add(0);
                            v.weight1.Add(0);
                        }
                        if (v.weight1[0] + v.weight1[1] + v.weight1[2] + v.weight1[3] != 0)
                        {
                            weight1.Add(new Vector4(v.weight1[0], v.weight1[1], v.weight1[2], v.weight1[3]));
                        }
                        else
                        {
                            weight1.Add(new Vector4(0, 0, 0, 0));
                        }

                        if (v.node1[0] + v.node1[1] + v.node1[2] + v.node1[3] != 0)
                        {
                            bone1.Add(new Vector4(v.node1[0], v.node1[1], v.node1[2], v.node1[3]));
                        }
                        else
                        {
                            bone1.Add(new Vector4(-1, 0, 0, 0));
                        }
                    }

                    if (m.verticesWiiU != null)
                    {
                        WU.VertexWiiU v = m.verticesWiiU;

                        for (int p = 0; p < v.pos.Count; p++)
                        {
                            if (v.pos.Count != 0)
                                vert.Add(v.pos[p]);
                            if (v.nrm.Count != 0)
                                nrm.Add(v.nrm[p]);
                            //UV Layer 1
                            if (v.uv0.Count != 0)
                                u0.Add(v.uv0[p]);
                            else
                                u0.Add(new Vector2(0, 0));


                            //UV Layer 2
                            if (v.uv1.Count != 0)
                                u1.Add(v.uv1[p]);
                            else
                                u1.Add(new Vector2(0, 0));

                            //Vertex Color
                            if (v.col.Count != 0)
                                col.Add(v.col[p]);
                            else
                                col.Add(new Vector4(0.9f, 0.9f, 0.9f, 1));

                            //Bone Indicies
                            if (v.nodes.Count != 0)
                                bone.Add(v.nodes[p]);
                            else
                                bone.Add(new Vector4(-1, 0, 0, 0));

                            //Weights
                            if (v.weights.Count != 0)
                                weight.Add(v.weights[p]);
                            else
                                weight.Add(new Vector4(0, 0, 0, 0));

                            //Tangents
                            if (v.tans.Count != 0)
                                tangent.Add(new Vector3(v.tans[p].X, v.tans[p].Y, v.tans[p].Z)); //Only vec3, idk what 4th value does
                            else
                                tangent.Add(new Vector3(0, 0, 0));

                            //Bitangents
                            if (v.bitans.Count != 0)
                                bitangent.Add(new Vector3(v.bitans[p].X, v.bitans[p].Y, v.bitans[p].Z)); //Only vec3, idk what 4th value does
                            else
                                bitangent.Add(new Vector3(0, 0, 0));

                            //Bone Indicies (More than 4)
                            if (v.nodes1.Count != 0)
                                bone1.Add(v.nodes1[p]);
                            else
                                bone1.Add(new Vector4(-1, 0, 0, 0));

                            //Weights (More than 4)
                            if (v.weights1.Count != 0)
                                weight1.Add(v.weights1[p]);
                            else
                                weight1.Add(new Vector4(0, 0, 0, 0));
                        }
                    }

                    foreach (List<int> l in m.faces)
                    {
                        //face.AddRange(l);
                        // rearrange faces
                        int[] ia = l.ToArray();
                        for (int j = 0; j < ia.Length; j++)
                        {
                            ia[j] += i;
                        }
                        face.AddRange(ia);
                    }
                    if (m.vertices.Count != 0)
                        i += m.vertices.Count;
                    else
                        i += m.verticesWiiU.pos.Count;
                }
            }
            vertdata = vert.ToArray();
            coldata = col.ToArray();
            nrmdata = nrm.ToArray();
            uvdata0 = u0.ToArray();
            uvdata1 = u1.ToArray();
            facedata = face.ToArray();
            bonedata = bone.ToArray();
            bone1data = bone1.ToArray();
            weightdata = weight.ToArray();
            weight1data = weight1.ToArray();
            tangentdata = tangent.ToArray();
            bitangentdata = bitangent.ToArray();

            if (Runtime.shaders["BFRES"].CompiledSuccessfully())
                SetupShader();

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



            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
            GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector4.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vColor"), 4, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_nrm);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(nrmdata.Length * Vector3.SizeInBytes), nrmdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vNormal"), 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_uv0);
            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(uvdata0.Length * Vector2.SizeInBytes), uvdata0, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vUV0"), 2, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_uv1);
            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(uvdata1.Length * Vector2.SizeInBytes), uvdata1, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vUV1"), 2, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_bone);
            GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(bonedata.Length * Vector4.SizeInBytes), bonedata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vBone"), 4, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_weight);
            GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(weightdata.Length * Vector4.SizeInBytes), weightdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vWeight"), 4, VertexAttribPointerType.Float, false, 0, 0);


            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_bone1);
            GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(bone1data.Length * Vector4.SizeInBytes), bone1data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vBone1"), 4, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_weight1);
            GL.BufferData<Vector4>(BufferTarget.ArrayBuffer, (IntPtr)(weight1data.Length * Vector4.SizeInBytes), weight1data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vWeight1"), 4, VertexAttribPointerType.Float, false, 0, 0);


            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_tan);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(tangentdata.Length * Vector3.SizeInBytes), tangentdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vTangent"), 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_bitan);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(bitangentdata.Length * Vector3.SizeInBytes), bitangentdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("vBitangent"), 3, VertexAttribPointerType.Float, false, 0, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(facedata.Length * sizeof(int)), facedata, BufferUsageHint.StaticDraw);

            GL.ActiveTexture(TextureUnit.Texture10);
            GL.BindTexture(TextureTarget.Texture2D, Rendering.RenderTools.uvTestPattern);

            GL.Uniform1(shader.getAttribute("renderR"), Runtime.renderR ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderG"), Runtime.renderG ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderB"), Runtime.renderB ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderAlpha"), Runtime.renderAlpha ? 1 : 0);


            if (Models.Checked == false)
            {
                return;
            }
            int indiceat = 0;



            foreach (FMDL_Model fmdl in models)
            {
                if (fmdl.Checked == false)
                {
                    /*   foreach (Mesh m in fmdl.poly) //Uncheck each mesh checkbox
                       {
                           m.Checked = false;
                       }*/
                    return;
                }




                Matrix4[] f = fmdl.skeleton.getShaderMatrix();

                int i = 0;

                int[] bind = fmdl.Node_Array; //Now bind each bone

                GL.UniformMatrix4(shader.getAttribute("bones"), f.Length, false, ref f[0].Row0.X);
                if (bind.Length != 0)
                {
                    GL.Uniform1(shader.getAttribute("boneList"), bind.Length, ref bind[0]);
                }


                sb = fmdl.skeleton.getShaderMatrixSingleBinded();

                GL.UniformMatrix4(shader.getAttribute("bonesfixed"), sb.Length, false, ref sb[0].Row0.X);

                foreach (Mesh m in fmdl.poly)
                {
                    if (m.Checked)
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


                        GL.Uniform4(shader.getAttribute("colorSamplerUV"), new Vector4(1, 1, 0, 0));
                        GL.Uniform4(shader.getAttribute("gsys_bake_st0"), new Vector4(1, 1, 0, 0));

                        foreach (ShaderParam p in fmdl.mat[m.MaterialIndex].matparam)
                        {
                            if (p.Name == "gsys_bake_st0" || p.Name == "bake0_st") //This uniform variable sets first bake map coords (MK8, Spatoon 1/2, ect)
                            {
                                GL.Uniform4(shader.getAttribute("gsys_bake_st0"), new Vector4(p.Vec4Data));
                            }
                            if (p.Name == "gsys_bake_st1" || p.Name == "bake1_st") //This uniform variable sets second bake map coords (MK8, Spatoon 1/2, ect)
                            {
                                GL.Uniform4(shader.getAttribute("gsys_bake_st1"), new Vector4(p.Vec4Data));
                            }
                            if (p.Name == "normal_map_weight") //This uniform variable sets the normal map intensity
                            {
                                GL.Uniform1(shader.getAttribute("normal_map_weight"), p.Data_Float);
                            }
                            if (p.Name == "ao_density") //This uniform variable sets the ao map intensity
                            {
                                GL.Uniform1(shader.getAttribute("ao_density"), p.Data_Float);
                            }

                        }
                        GL.Disable(EnableCap.CullFace); //Set as enabled by default unless specified otherwise.
                        bool seal = false;
                        foreach (RenderInfoData r in fmdl.mat[m.MaterialIndex].renderinfo)
                        {
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
                                case "gsys_color_blend_rgb_src_func":
                                    break;
                                case "gsys_color_blend_rgb_dst_func":
                                    break;
                                case "gsys_color_blend_rgb_op":
                                    break;
                                case "gsys_color_blend_alpha_op":
                                    break;
                                case "gsys_color_blend_alpha_src_func":
                                    break;
                                case "gsys_color_blend_alpha_dst_func":
                                    break;
                                case "gsys_color_blend_const_color":
                                    break;
                                case "gsys_depth_test_enable":
                                    break;
                                case "gsys_depth_test_func":
                                    break;
                                case "gsys_depth_test_write":
                                    break;
                                case "gsys_alpha_test_enable":
                                    break;
                                case "gsys_alpha_test_func":
                                    break;
                                case "gsys_alpha_test_value":
                                    break;
                                case "gsys_render_state_mode":
                                    if (r.Value_String == "opaque")
                                    {
                                    }
                                    else if (r.Value_String == "translucent")
                                    {
                                        //    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                                        //   GL.Enable(EnableCap.Blend);
                                    }
                                    else if (r.Value_String == "mask")
                                    {

                                    }
                                    break;
                                case "gsys_pass":
                                    if (r.Value_String == "seal") //Renders model over another and takes priortiy
                                    {
                                        seal = true;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        GL.Uniform1(shader.getAttribute("HasNormalMap"), 0);

                        if (fmdl.mat[m.MaterialIndex].HasNormalMap == true)
                        {
                            GL.Uniform1(shader.getAttribute("HasNormalMap"), 1);
                        }

                        int id = 0;
                        foreach (string tex in m.TextureMapTypes)
                        {
                            SamplerInfo smp = fmdl.mat[m.MaterialIndex].samplerinfo[id];

                            if (tex == "Diffuse")
                            {
                                GL.ActiveTexture(TextureUnit.Texture0);
                                GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                                GL.Uniform1(shader.getAttribute("tex0"), 0);
                            }
                            else if (tex == "Normal")
                            {
                                GL.ActiveTexture(TextureUnit.Texture1);
                                GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                                GL.Uniform1(shader.getAttribute("nrm"), 1);
                            }
                            else if (tex == "Bake1")
                            {
                                GL.ActiveTexture(TextureUnit.Texture2);
                                GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                                GL.Uniform1(shader.getAttribute("BakeShadowMap"), 2);
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
                                GL.ActiveTexture(TextureUnit.Texture6);
                                GL.BindTexture(TextureTarget.Texture2D, m.texHashs[id]);
                                GL.Uniform1(shader.getAttribute("MRA"), 6);
                            }
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapmode[smp.WrapModeU]);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapmode[smp.WrapModeV]);

                            id++;
                        }

                        //GL.BindTexture(TextureTarget.Texture2D, m.texId);
                        //GL.Uniform1(shader.getAttribute("tex"), 0);
                        foreach (List<int> l in m.faces)
                        {
                            GL.DrawElements(PrimitiveType.Triangles, l.Count, DrawElementsType.UnsignedInt, indiceat * sizeof(int));
                            indiceat += l.Count;

                            if (seal == true)
                            {
                                //Todo. Render priortiy over other faces
                            }
                        }
                    }
                }
            }
            shader.disableAttrib();
        }
        private static void DrawModelSelection(Mesh p, Shader shader, int Length, int Offset)
        {
            GL.Enable(EnableCap.StencilTest);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
            GL.Disable(EnableCap.DepthTest);

            bool[] cwm = new bool[4];
            GL.GetBoolean(GetIndexedPName.ColorWritemask, 4, cwm);
            GL.ColorMask(false, false, false, false);

            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);

            GL.DrawElements(PrimitiveType.Triangles, Length, DrawElementsType.UnsignedInt, Offset);

            GL.ColorMask(cwm[0], cwm[1], cwm[2], cwm[3]);

            GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
            GL.StencilMask(0x00);

            // use vertex color for model selection color
            GL.Uniform1(shader.getAttribute("colorOverride"), 1);

            GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
            GL.LineWidth(2.0f);
            GL.DrawElements(PrimitiveType.Triangles, Length, DrawElementsType.UnsignedInt, Offset);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            GL.Uniform1(shader.getAttribute("colorOverride"), 0);

            GL.StencilMask(0xFF);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
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
                Nodes.Add(Models);
                //    Nodes.Add(SkeletalAnim);  Gets added to anim panel on left so this not neeeded
                Nodes.Add(MaterialAnim);
                Nodes.Add(VisualAnim);
                Nodes.Add(ShapeAnim);
                Nodes.Add(SceneAnim);
                Nodes.Add(Embedded);
                ImageKey = "bfres";
                SelectedImageKey = "bfres";

                Console.WriteLine("Version = " + verNumA + "." + verNumB + "." + verNumC + "." + verNumD);
                if (f.readShort() == 0xFEFF)
                    f.Endian = Endianness.Big;
                else f.Endian = Endianness.Little;

                f.skip(2);  //Size Headeer
                f.skip(4); //File Name Direct
                int fileAlignment = f.readInt();

                int RelocationTableOffset = f.readInt();
                int BfresSize = f.readInt();

                string name = f.readString(f.readInt() + 2, -1);

                Console.WriteLine(BfresSize);

                f.skip(4); // Padding
                long FMDLOffset = f.readInt64();
                long FMDLDict = f.readInt64();
                long FSKAOffset = f.readInt64();
                long FSKADict = f.readInt64();
                long FMAAOffset = f.readInt64();
                long FMAADict = f.readInt64();
                long FVISOffset = f.readInt64();
                long FVISDict = f.readInt64();
                long FSHUOffset = f.readInt64();
                long FSHUDict = f.readInt64();
                long FSCNOffset = f.readInt64();
                long FSCNDict = f.readInt64();
                long BuffMemPool = f.readInt64();
                long BuffMemPoolInfo = f.readInt64();
                long EMBOffset = f.readInt64();
                long EMBDict = f.readInt64();
                f.skip(8); // Padding
                long StringTableOffset = f.readInt64();
                int unk11 = f.readInt();
                int FMDLCount = f.readShort();
                FSKACount = f.readShort();
                int FMAACount = f.readShort();
                int FVISCount = f.readShort();
                int FSHUCount = f.readShort();
                int FSCNCount = f.readShort();
                int EMBCount = f.readShort();
                f.skip(12); // Padding
                            // Console.WriteLine($"FMDLOffset {FMDLOffset} FMDLCount {FMDLCount} FMDLDict {FMDLDict} FSKAOffset {FSKAOffset} FSKADict {FSKADict}");
                            //  Console.WriteLine($"FMAAOffset {FMAAOffset} FMAADict {FMAADict} FVISOffset {FVISOffset} FSHUOffset {FSKAOffset} FSKADict {FSHUDict}");

                //Material animations (SRT, texture pattern, ect)
                for (int i = 0; i < FMAACount; i++)
                {
                    f.seek((int)FMAAOffset + (i * 120));
                    //    MaterialAnim.Nodes.Add(MatAnimName);
                    MaterialAnimation fma = new MaterialAnimation();
                    fma.readFMAA(f);

                }
                //Visual animations 
                for (int i = 0; i < FVISCount; i++)
                {
                    f.seek((int)FVISOffset + (i * 104));
                    f.skip(12);
                    string VisAnimName = f.readString((int)f.readInt64() + 2, -1);
                    VisualAnim.Nodes.Add(VisAnimName);
                }
                //Shape animations
                for (int i = 0; i < FSHUCount; i++)
                {
                    f.seek((int)FSHUOffset + (i * 120)); //I haven't seen a bfres with this yet so idk the size of header
                    f.skip(12);
                    string ShpAnimName = f.readString((int)f.readInt64() + 2, -1);
                    ShapeAnim.Nodes.Add(ShpAnimName);
                }
                //Scene animations (like lighting or cameras???)
                for (int i = 0; i < FSCNCount; i++)
                {
                    f.seek((int)FSCNOffset + (i * 104));
                    f.skip(12);
                    string ScnAnimName = f.readString((int)f.readInt64() + 2, -1);
                    SceneAnim.Nodes.Add(ScnAnimName);
                }

                //Textures and shaders are embedded into the bfres
                for (int i = 0; i < EMBCount; i++)
                {
                    f.seek((int)EMBOffset + (i * 16));
                    int DataOffset = f.readInt();
                    f.seek(DataOffset);
                    string EmMagic = f.readString(f.pos(), 4);


                    if (EmMagic.Equals("BNTX")) //Textures
                    {
                        int temp = f.pos();
                        BNTX t = new BNTX();
                        t.ReadBNTX(f);
                        Embedded.Nodes.Add(t);

                    }
                }
                //FMDLs -Models-
                f.seek((int)FMDLOffset);
                for (int i = 0; i < FMDLCount; i++)
                {
                    //   Console.WriteLine("Reading FMDL....");

                    FMDL_Model model = new FMDL_Model();



                    //FMDL modelTest = new FMDL();
                    //modelTest.Read(f);
                    f.skip(16);

                    FMDLheader fmdl_info = new FMDLheader
                    {

                        name = f.readString(f.readInt() + 2, -1),
                        padding = f.readInt(),
                        eofString = f.readInt64(),
                        fsklOff = f.readInt64(),
                        fvtxArrOff = f.readInt64(),
                        fshpOffset = f.readInt64(),
                        fshpIndx = f.readInt64(),
                        fmatOffset = f.readInt64(),
                        fmatIndx = f.readInt64(),
                        UserDataOffset = f.readInt64(),
                        padding1 = f.readInt64(),
                        padding2 = f.readInt64(),
                        fvtxCount = f.readShort(),
                        fshpCount = f.readShort(),
                        fmatCount = f.readShort(),
                        paramCount = f.readShort(),
                        VertCount = f.readInt(),
                        unk2 = f.readInt(),
                    };
                    int NextFMDL = f.pos();

                    model.Text = fmdl_info.name;

                    Models.Nodes.Add(model);

                    //   Console.WriteLine($" Name {fmdl_info.name} eofString {fmdl_info.eofString} fsklOff {fmdl_info.fsklOff}");
                    //  Console.WriteLine(fmdl_info.fvtxCount);

                    List<FVTXH> FVTXArr = new List<FVTXH>();
                    f.seek((int)fmdl_info.fvtxArrOff);
                    for (int vtx = 0; vtx < fmdl_info.fvtxCount; vtx++)
                    {
                        //   Console.WriteLine("Reading FVTX....");
                        f.skip(16);
                        FVTXArr.Add(new FVTXH
                        {
                            attArrOff = f.readInt64(),
                            attIndxOff = f.readInt64(),
                            unk1 = f.readInt64(),
                            unk2 = f.readInt64(),
                            unk3 = f.readInt64(),
                            buffSizeOff = f.readInt64(),
                            buffStrideSizeOff = f.readInt64(),
                            buffArrOff = f.readInt64(),
                            buffOff = f.readInt(),
                            attCount = f.readByte(),
                            buffCount = f.readByte(),
                            sectIndx = f.readShort(),
                            vertCount = f.readInt(),
                            SkinWeightInfluence = f.readInt()
                        });
                        //  Console.WriteLine($"attCount {FVTXArr[vtx].attCount}");
                    }


                    f.seek((int)fmdl_info.fmatOffset);
                    List<FMATH> FMATheaders = new List<FMATH>();
                    for (int mat = 0; mat < fmdl_info.fmatCount; mat++)
                    {
                        f.skip(16);

                        MaterialData matr = new MaterialData(); //This class stores multiple classes for mat data

                        FMATH fmat_info = new FMATH
                        {
                            name = f.readString((int)f.readInt64() + 2, -1),
                            renderInfoOff = f.readInt64(),
                            renderInfoIndx = f.readInt64(),
                            shaderAssignOff = f.readInt64(),
                            u1 = f.readInt64(),
                            texSelOff = f.readInt64(),
                            u2 = f.readInt64(),
                            texAttSelOff = f.readInt64(),
                            texAttIndxOff = f.readInt64(),
                            matParamArrOff = f.readInt64(),
                            matParamIndxOff = f.readInt64(),
                            matParamOff = f.readInt64(),
                            userDataOff = f.readInt64(),
                            userDataIndxOff = f.readInt64(),
                            volatileFlagOffset = f.readInt64(),
                            u3 = f.readInt64(),
                            samplerSlotOff = f.readInt64(),
                            textureSlotOff = f.readInt64(),
                            flags = f.readInt(), //This toggles material visabilty
                            sectIndx = f.readShort(),
                            rendParamCount = f.readShort(),
                            texSelCount = f.readByte(),
                            texAttSelCount = f.readByte(),
                            matParamCount = f.readShort(),
                            u4 = f.readShort(),
                            matParamSize = f.readShort(),
                            rawParamDataSize = f.readShort(),
                            userDataCount = f.readShort(),
                            padding = f.readInt(),

                        };
                        string FMATNameOffset = fmat_info.name;
                        // Console.WriteLine($"{FMATNameOffset} {fmat_info.texSelCount} ");
                        FMATheaders.Add(fmat_info);

                        matr.name = fmat_info.name;

                        model.mat.Add(matr);
                    }

                    f.seek((int)fmdl_info.fsklOff + 16);
                    // Console.WriteLine("Reading FSKL....");
                    FSKLH fskl_info = new FSKLH
                    {
                        boneIndxOff = f.readInt64(),
                        boneArrOff = f.readInt64(),
                        invIndxArrOff = f.readInt64(),
                        invMatrArrOff = f.readInt64(),
                        padding1 = f.readInt64(),
                        fsklType = f.readInt(), //flags
                        boneArrCount = f.readShort(),
                        invIndxArrCount = f.readShort(),
                        exIndxCount = f.readShort(),
                        u1 = f.readInt(),
                    };

                    f.seek((int)fmdl_info.fsklOff + 16);
                    FSKLH fskl_infov8 = new FSKLH
                    {
                        boneIndxOff = f.readInt64(),
                        boneArrOff = f.readInt64(),
                        invIndxArrOff = f.readInt64(),
                        invMatrArrOff = f.readInt64(),
                        padding1 = f.readInt64(),
                        padding2 = f.readInt64(),
                        padding3 = f.readInt64(),
                        fsklType = f.readInt(), //flags
                        boneArrCount = f.readShort(),
                        invIndxArrCount = f.readShort(),
                        exIndxCount = f.readShort(),
                        u1 = f.readInt(),
                    };
                    //  Console.WriteLine($"Bone Count {fskl_info.boneArrCount}");

                    //FSKL and many other sections will be revised and cleaner later

                    if (verNumB == 8)
                    {
                        model.Node_Array = new int[fskl_infov8.invIndxArrCount + fskl_infov8.exIndxCount];
                        f.seek((int)fskl_infov8.invIndxArrOff);
                        for (int nodes = 0; nodes < fskl_infov8.invIndxArrCount + fskl_infov8.exIndxCount; nodes++)
                        {
                            model.Node_Array[nodes] = (f.readShort());
                        }
                    }
                    else
                    {
                        model.Node_Array = new int[fskl_info.invIndxArrCount + fskl_info.exIndxCount];
                        f.seek((int)fskl_info.invIndxArrOff);
                        for (int nodes = 0; nodes < fskl_info.invIndxArrCount + fskl_info.exIndxCount; nodes++)
                        {
                            model.Node_Array[nodes] = (f.readShort());
                        }
                    }



                    List<FSHPH> FSHPArr = new List<FSHPH>();
                    // Console.WriteLine("Reading FSHP....");
                    f.seek((int)fmdl_info.fshpOffset);
                    for (int shp = 0; shp < fmdl_info.fshpCount; shp++)
                    {
                        f.skip(16);
                        FSHPArr.Add(new FSHPH
                        {
                            polyNameOff = f.readInt(),
                            u1 = f.readInt(),
                            fvtxOff = f.readInt64(),
                            lodMdlOff = f.readInt64(),
                            fsklIndxArrOff = f.readInt64(),
                            u3 = f.readInt64(),
                            u4 = f.readInt64(),
                            boundingBoxOff = f.readInt64(),
                            radiusOff = f.readInt64(),
                            padding = f.readInt64(),
                            flags = f.readInt(),
                            sectIndx = f.readShort(),
                            fmatIndx = f.readShort(),
                            fsklIndx = f.readShort(),
                            fvtxIndx = f.readShort(),
                            fsklIndxArrCount = f.readShort(),
                            matrFlag = f.readByte(),
                            lodMdlCount = f.readByte(),
                            visGrpCount = f.readInt(),
                            visGrpIndxOff = f.readShort(),
                            visGrpNodeOff = f.readShort(),
                        });
                    }


                    int BoneCount;
                    // Console.WriteLine("Reading Bones....");
                    if (verNumB == 8)
                    { f.seek((int)fskl_infov8.boneArrOff); BoneCount = fskl_infov8.boneArrCount; }
                    else
                    { f.seek((int)fskl_info.boneArrOff); BoneCount = fskl_info.boneArrCount; }
                    for (int bn = 0; bn < BoneCount; bn++)
                    {
                        Bone bone = new Bone(model.skeleton);
                        bone.Text = f.readString(f.readInt() + 2, -1);
                        if (verNumB == 8)
                        {
                            f.skip(36);
                        }
                        else
                        {
                            f.skip(20);
                        }
                        bone.boneId = (uint)f.readShort();
                        int parIndx1 = (short)f.readShort();
                        int parIndx2 = f.readShort();
                        int parIndx3 = f.readShort();
                        int parIndx4 = f.readShort();
                        bone.parentIndex = parIndx1;
                        int smootMatrix = f.readShort();
                        int RigidMatrix = f.readShort();
                        f.skip(2);
                        bone.scale = new float[3];
                        bone.rotation = new float[4];
                        bone.position = new float[3];
                        bone.scale[0] = f.readFloat();
                        bone.scale[1] = f.readFloat();
                        bone.scale[2] = f.readFloat();
                        bone.rotation[0] = f.readFloat();
                        bone.rotation[1] = f.readFloat();
                        bone.rotation[2] = f.readFloat();
                        bone.rotation[3] = f.readFloat();
                        bone.position[0] = f.readFloat();
                        bone.position[1] = f.readFloat();
                        bone.position[2] = f.readFloat();

                        model.skeleton.bones.Add(bone);

                    }

                    model.skeleton.reset();
                    model.skeleton.update();

                    // Console.WriteLine("Reading FSHP Array....");

                    //MeshTime!!

                    for (int m = 0; m < FSHPArr.Count; m++)
                    {

                        Mesh poly = new Mesh();

                        poly.MaterialIndex = FSHPArr[m].fmatIndx; //This index is important for dae exporting
                        poly.name = f.readString(FSHPArr[m].polyNameOff + 2, -1);
                        poly.Text = poly.name;
                        Models.Nodes[i].Nodes.Add(poly);
                        poly.matrFlag = FSHPArr[m].matrFlag;
                        poly.fsklindx = FSHPArr[m].fsklIndx;

                        //    Console.WriteLine("Polygon = " + poly.name);

                        poly.BoneFixNode = new int[FSHPArr[m].fsklIndxArrCount];
                        f.seek((int)FSHPArr[m].fsklIndxArrOff);
                        for (int nodes = 0; nodes < FSHPArr[m].fsklIndxArrCount; nodes++)
                        {
                            poly.BoneFixNode[nodes] = (f.readShort());
                        }



                        List<attdata> AttrArr = new List<attdata>();
                        f.seek((int)FVTXArr[FSHPArr[m].fvtxIndx].attArrOff);
                        for (int att = 0; att < FVTXArr[FSHPArr[m].fvtxIndx].attCount; att++)
                        {
                            string AttType = f.readString(f.readInt() + 2, -1);

                            f.skip(4); //padding  
                            f.Endian = Endianness.Big;
                            int vertType = f.readShort();
                            f.skip(2);
                            f.Endian = Endianness.Little;
                            int buffOff = f.readShort();
                            int buffIndx = f.readShort();
                            //   Console.WriteLine($"{poly.name} {AttType} Type = {vertType} Offset = {buffOff} Index = {buffIndx} ");
                            AttrArr.Add(new attdata { attName = AttType, buffIndx = buffIndx, buffOff = buffOff, vertType = vertType });
                        }


                        //Get RLT real quick for buffer offset
                        f.seek(0x18);
                        int RTLOffset = f.readInt();

                        f.seek(RTLOffset);
                        f.skip(0x030);
                        int DataStart = f.readInt();

                        // Console.WriteLine($"RLT {DataStart}");


                        List<buffData> BuffArr = new List<buffData>();
                        f.seek((int)FVTXArr[FSHPArr[m].fvtxIndx].buffArrOff);
                        for (int buff = 0; buff < FVTXArr[FSHPArr[m].fvtxIndx].buffCount; buff++)
                        {
                            buffData data = new buffData();
                            f.seek((int)FVTXArr[FSHPArr[m].fvtxIndx].buffSizeOff + ((buff) * 0x10));
                            data.buffSize = f.readInt();
                            f.seek((int)FVTXArr[FSHPArr[m].fvtxIndx].buffStrideSizeOff + ((buff) * 0x10));
                            data.strideSize = f.readInt();

                            //So these work by grabbing the RLT offset first and then adding the buffer offset. Then they keep adding each other by their buffer sizes
                            if (buff == 0) data.DataOffset = (DataStart + FVTXArr[FSHPArr[m].fvtxIndx].buffOff);
                            if (buff > 0) data.DataOffset = BuffArr[buff - 1].DataOffset + BuffArr[buff - 1].buffSize;
                            if (data.DataOffset % 8 != 0) data.DataOffset = data.DataOffset + (8 - (data.DataOffset % 8));

                            BuffArr.Add(data);
                            //   Console.WriteLine("Data Offset = " + data.DataOffset + " Vertex Buffer Size =" + data.buffSize + " Index = " + buff + " vertexStrideSize size =" + data.strideSize);
                        }

                        for (int v = 0; v < FVTXArr[FSHPArr[m].fvtxIndx].vertCount; v++)
                        {
                            Vertex vert = new Vertex();
                            for (int attr = 0; attr < AttrArr.Count; attr++)
                            {
                                f.seek(((BuffArr[AttrArr[attr].buffIndx].DataOffset) + (AttrArr[attr].buffOff) + (BuffArr[AttrArr[attr].buffIndx].strideSize * v)));
                                switch (AttrArr[attr].attName)
                                {
                                    case "_p0":
                                        if (AttrArr[attr].vertType == 1301)
                                            vert.pos = new Vector3 { X = f.readHalfFloat(), Y = f.readHalfFloat(), Z = f.readHalfFloat() };
                                        if (AttrArr[attr].vertType == 1304)
                                            vert.pos = new Vector3 { X = f.readFloat(), Y = f.readFloat(), Z = f.readFloat() };
                                        break;
                                    case "_c0":
                                        if (AttrArr[attr].vertType == 1301)
                                            vert.col = new Vector4(f.readHalfFloat(), f.readHalfFloat(), f.readHalfFloat(), f.readHalfFloat());
                                        if (AttrArr[attr].vertType == 2067)
                                            vert.col = new Vector4 { X = f.readFloat(), Y = f.readFloat(), Z = f.readFloat(), W = f.readFloat() };
                                        if (AttrArr[attr].vertType == 267)
                                            vert.col = new Vector4(f.readByte() / 255f, f.readByte() / 255f, f.readByte() / 255f, f.readByte() / 255f);
                                        break;
                                    case "_n0":
                                        if (AttrArr[attr].vertType == 526)
                                        {
                                            int normVal = (int)f.readInt();
                                            //Thanks RayKoopa!
                                            vert.nrm = new Vector3 { X = sign10Bit((normVal) & 0x3FF) / (float)511, Y = sign10Bit((normVal >> 10) & 0x3FF) / (float)511, Z = sign10Bit((normVal >> 20) & 0x3FF) / (float)511 };
                                        }
                                        break;
                                    case "_u0":
                                    case "_u1":
                                    case "_u2":
                                    case "_u3":
                                    case "color":
                                        if (AttrArr[attr].vertType == 265 || AttrArr[attr].vertType == 521)
                                            vert.tx.Add(new Vector2 { X = ((float)f.readByte()) / 255, Y = ((float)f.readByte()) / 255 });
                                        if (AttrArr[attr].vertType == 274)
                                            vert.tx.Add(new Vector2 { X = ((float)f.readShort()) / 65535, Y = ((float)f.readShort()) / 65535 });
                                        if (AttrArr[attr].vertType == 530)
                                            vert.tx.Add(new Vector2 { X = ((float)f.readShort()) / 32767, Y = ((float)f.readShort()) / 32767 });
                                        if (AttrArr[attr].vertType == 1298)
                                            vert.tx.Add(new Vector2 { X = f.readHalfFloat(), Y = f.readHalfFloat() });
                                        if (AttrArr[attr].vertType == 1303)
                                            vert.tx.Add(new Vector2 { X = f.readFloat(), Y = f.readFloat() });
                                        break;
                                    case "_t0":
                                        if (AttrArr[attr].vertType == 523)
                                            vert.tan = new Vector4((sbyte)f.readByte() / 255f, (sbyte)f.readByte() / 255f, (sbyte)f.readByte() / 255f, (sbyte)f.readByte() / 255f);
                                        break;
                                    case "_b0":
                                        if (AttrArr[attr].vertType == 523)
                                            vert.bitan = new Vector4((sbyte)f.readByte() * 127f, (sbyte)f.readByte() * 127f, (sbyte)f.readByte() * 127f, (sbyte)f.readByte() * 127f);
                                        break;
                                    case "_i0":
                                        if (AttrArr[attr].vertType == 770)
                                        {
                                            vert.node.Add(f.readByte());
                                            vert.weight.Add((float)1.0);
                                        }
                                        if (AttrArr[attr].vertType == 777)
                                        {
                                            vert.node.Add(f.readByte());
                                            vert.node.Add(f.readByte());
                                        }
                                        if (AttrArr[attr].vertType == 779)
                                        {
                                            vert.node.Add(f.readByte());
                                            vert.node.Add(f.readByte());
                                            vert.node.Add(f.readByte());
                                            vert.node.Add(f.readByte());
                                        }
                                        if (AttrArr[attr].vertType == 523)
                                        {
                                            vert.node.Add(f.readByte());
                                            vert.node.Add(f.readByte());
                                            vert.node.Add(f.readByte());
                                            vert.node.Add(f.readByte());
                                        }
                                        break;
                                    case "_i1":
                                        if (AttrArr[attr].vertType == 770)
                                        {
                                            vert.node1.Add(f.readByte());
                                            vert.weight1.Add((float)1.0);
                                        }
                                        if (AttrArr[attr].vertType == 777)
                                        {
                                            vert.node1.Add(f.readByte());
                                            vert.node1.Add(f.readByte());
                                        }
                                        if (AttrArr[attr].vertType == 779)
                                        {
                                            vert.node1.Add(f.readByte());
                                            vert.node1.Add(f.readByte());
                                            vert.node1.Add(f.readByte());
                                            vert.node1.Add(f.readByte());
                                        }
                                        if (AttrArr[attr].vertType == 523)
                                        {
                                            vert.node1.Add(f.readByte());
                                            vert.node1.Add(f.readByte());
                                            vert.node1.Add(f.readByte());
                                            vert.node1.Add(f.readByte());
                                        }
                                        break;
                                    case "_w0":
                                        if (AttrArr[attr].vertType == 258)
                                        {
                                            vert.weight.Add((f.readByte()) / (float)255);
                                        }
                                        if (AttrArr[attr].vertType == 265)
                                        {
                                            vert.weight.Add((f.readByte()) / (float)255);
                                            vert.weight.Add((f.readByte()) / (float)255);
                                        }
                                        if (AttrArr[attr].vertType == 267)
                                        {
                                            vert.weight.Add((f.readByte()) / (float)255);
                                            vert.weight.Add((f.readByte()) / (float)255);
                                            vert.weight.Add((f.readByte()) / (float)255);
                                            vert.weight.Add((f.readByte()) / (float)255);
                                        }
                                        if (AttrArr[attr].vertType == 274)
                                        {
                                            vert.weight.Add((f.readShort()) / (float)255);
                                            vert.weight.Add((f.readShort()) / (float)255);
                                        }
                                        break;
                                    case "_w1":
                                        if (AttrArr[attr].vertType == 258)
                                        {
                                            vert.weight1.Add((f.readByte()) / (float)255);
                                        }
                                        if (AttrArr[attr].vertType == 265)
                                        {
                                            vert.weight1.Add((f.readByte()) / (float)255);
                                            vert.weight1.Add((f.readByte()) / (float)255);
                                        }
                                        if (AttrArr[attr].vertType == 267)
                                        {
                                            vert.weight1.Add((f.readByte()) / (float)255);
                                            vert.weight1.Add((f.readByte()) / (float)255);
                                            vert.weight1.Add((f.readByte()) / (float)255);
                                            vert.weight1.Add((f.readByte()) / (float)255);
                                        }
                                        if (AttrArr[attr].vertType == 274)
                                        {
                                            vert.weight1.Add((f.readShort()) / (float)255);
                                            vert.weight1.Add((f.readShort()) / (float)255);
                                        }
                                        break;
                                    default:
                                        //     Console.WriteLine(AttrArr[attr].attName + " Unknown type " + AttrArr[attr].vertType.ToString("x") + " 0x");
                                        break;
                                }
                            }
                            poly.vertices.Add(vert);
                        }
                        int LoadLOD = 0;

                        f.seek((int)FSHPArr[m].lodMdlOff);
                        for (int lod = 0; lod < FSHPArr[m].lodMdlCount; lod++)
                        {
                            long SubMeshOff = f.readInt64();
                            long unk1 = f.readInt64();
                            long unk2 = f.readInt64();
                            long indxBuffOff = f.readInt64();
                            int FaceBuffer = f.readInt();
                            int PrimativefaceType = f.readInt();
                            int faceType = f.readInt();
                            int FaceCount = f.readInt();
                            int elmSkip = f.readInt();
                            int subMeshCount = f.readInt();

                            int temp = f.pos();



                            f.seek(FaceBuffer + DataStart);
                            if (faceType == 1)
                                FaceCount = FaceCount / 3;
                            if (faceType == 2)
                                FaceCount = FaceCount / 6;


                            if (lod == LoadLOD)
                            {
                                for (int face = 0; face < FaceCount; face++)
                                {
                                    if (faceType == 1)
                                        poly.faces.Add(new List<int> { elmSkip + f.readShort(), elmSkip + f.readShort(), elmSkip + f.readShort() });
                                    else if (faceType == 2)
                                        poly.faces.Add(new List<int> { elmSkip + f.readInt(), elmSkip + f.readInt(), elmSkip + f.readInt() });
                                    else
                                        Console.Write("UnkFaceFormat");
                                }
                            }

                            f.seek(temp);
                        }



                        /*   Old method which doesn't work on Kirby Star Allies atm
                        f.seek((int)FMATheaders[FSHPArr[m].fmatIndx].texSelOff);
                        for (int tex = 0; FMATheaders[FSHPArr[m].fmatIndx].texAttSelCount > tex; tex++)
                        {
                            string TextureName = f.readString((int)f.readInt64() + 2, -1);
                            int NextTexSel = f.pos();
                            f.seek((int)FMATheaders[FSHPArr[m].fmatIndx].texAttIndxOff + 24 + (tex * 16));
                            f.skip(8);
                            string SamplerName = f.readString((int)f.readInt64() + 2, -1);

                            Console.WriteLine(TextureName + " " + SamplerName);

                            if (SamplerName == "_a0")
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(0);
                                }
                            }
                            else if(SamplerName == "_n0")
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(1);
                                }
                            }
                            else if(SamplerName == "_b0")
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(2);
                                }
                            }
                            else if (SamplerName == "_b1")
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(3);
                                }
                            }
                            else
                            {
                                try
                                {
                                    poly.texHashs.Add(BNTX.textured[TextureName].texture.display);
                                }
                                catch
                                {
                                    poly.texHashs.Add(tex * 4);
                                }
                            }



                        }*/

                        Console.WriteLine(poly.Text + " is mapped to " + FMATheaders[FSHPArr[m].fmatIndx].name);

                        int AlbedoCount = 0;
                        f.seek((int)FMATheaders[FSHPArr[m].fmatIndx].texSelOff);
                        for (int tex = 0; FMATheaders[FSHPArr[m].fmatIndx].texAttSelCount > tex; tex++)
                        {
                            //Need to have default texture load instead of random texture if texture doesn't exist within the tex index

                            string TextureName = f.readString((int)f.readInt64() + 2, -1);

                            model.mat[FSHPArr[m].fmatIndx].TextureMaps.Add(TextureName);

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
                                model.mat[poly.MaterialIndex].HasNormalMap = true;
                                poly.TextureMapTypes.Add("Normal");
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
                        }

                        f.seek((int)FMATheaders[FSHPArr[m].fmatIndx].texAttSelOff);
                        for (int tex = 0; FMATheaders[FSHPArr[m].fmatIndx].texAttSelCount > tex; tex++)
                        {
                            int curpos = f.pos();

                            SamplerInfo s = new SamplerInfo();
                            s.WrapModeU = f.readByte();
                            s.WrapModeV = f.readByte();
                            s.WrapModeW = f.readByte();

                            f.seek(curpos + 32);

                            model.mat[FSHPArr[m].fmatIndx].samplerinfo.Add(s);

                        }

                        //This section controls rendering data like alpha, cull mode, ect

                        f.seek((int)FMATheaders[FSHPArr[m].fmatIndx].renderInfoOff);
                        for (int tex = 0; FMATheaders[FSHPArr[m].fmatIndx].rendParamCount > tex; tex++)
                        {
                            RenderInfoData r = new RenderInfoData();

                            int curpos = f.pos();

                            r.Name = f.readString((int)f.readInt64() + 2, -1);
                            r.DataOffset = f.readInt64();
                            r.ArrayLength = f.readShort();
                            r.Type = f.readByte();
                            f.skip(6); //Padding


                            if (r.DataOffset != 0)
                            {
                                f.seek((int)r.DataOffset);
                                switch (r.Type)
                                {
                                    case 0:
                                        r.Value_Int = f.readInt();
                                        break;
                                    case 1:
                                        r.Value_Float = f.readFloat();
                                        break;
                                    case 2:
                                        r.Value_String = f.readString((int)f.readInt64() + 2, -1);
                                        break;
                                }
                            }
                            model.mat[FSHPArr[m].fmatIndx].renderinfo.Add(r);
                            f.seek(curpos + 24);
                        }

                        //Parse Shader Params. These store shader uniforms and data to them

                        f.seek((int)FMATheaders[FSHPArr[m].fmatIndx].matParamArrOff);
                        for (int tex = 0; FMATheaders[FSHPArr[m].fmatIndx].matParamCount > tex; tex++)
                        {
                            ShaderParam prm = new ShaderParam();

                            int curpos = f.pos();

                            f.skip(8);
                            prm.Name = f.readString((int)f.readInt64() + 2, -1);
                            prm.Type = f.readByte();
                            prm.Size = f.readByte();
                            prm.Offset = f.readShort();
                            prm.UniformVar = f.readInt();
                            prm.Index = f.readInt();
                            f.skip(4);

                            if (prm.Type == 15)
                            {
                                f.seek((int)FMATheaders[FSHPArr[m].fmatIndx].matParamOff + prm.Offset);
                                float dataX = f.readFloat();
                                float dataY = f.readFloat();
                                float dataZ = f.readFloat();
                                float dataW = f.readFloat();
                                prm.Vec4Data = new Vector4(dataX, dataY, dataZ, dataW);
                            }
                            if (prm.Type == 12)
                            {
                                f.seek((int)FMATheaders[FSHPArr[m].fmatIndx].matParamOff + prm.Offset);
                                float value = f.readFloat();
                                prm.Data_Float = value;
                            }

                            model.mat[FSHPArr[m].fmatIndx].matparam.Add(prm);

                            f.seek(curpos + 32);
                        }
                        model.poly.Add(poly);
                    }
                    models.Add(model);
                    f.seek(NextFMDL);
                }
                PreRender();
            }
            else
            {
                f.Endian = Endianness.Big;
                f.eof();

                Nodes.Add(Models);
                //    Nodes.Add(SkeletalAnim);  Gets added to anim panel on left so this not neeeded
                Nodes.Add(Textures);
                Nodes.Add(Shaderparam);
                Nodes.Add(Coloranim);
                Nodes.Add(TextureSRT);
                Nodes.Add(TexturePat);
                Nodes.Add(Bonevisabilty);
                Nodes.Add(VisualAnim);
                Nodes.Add(ShapeAnim);
                Nodes.Add(SceneAnim);
                Nodes.Add(Embedded);
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
                    Textures.Nodes.Add(texture);
                }

                int ModelCur = 0;
                //FMDLs -Models-
                foreach (Model mdl in TargetWiiUBFRES.Models.Values)
                {
                    FMDL_Model model = new FMDL_Model(); //This will store VBN data and stuff
                    model.Text = mdl.Name;

                    Models.Nodes.Add(model);

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

                    foreach (Material mt in mdl.Materials.Values)
                    {
                        MaterialData matr = new MaterialData(); //This class stores multiple classes for mat data
                        model.mat.Add(matr);
                    }

                    //MeshTime!!
                    foreach (Shape shp in mdl.Shapes.Values)
                    {
                        Mesh poly = new Mesh();
                        poly.Text = shp.Name;
                        poly.MaterialIndex = shp.MaterialIndex;
                        poly.matrFlag = shp.VertexSkinCount;
                        poly.fsklindx = shp.BoneIndex;

                        Models.Nodes[ModelCur].Nodes.Add(poly);

                        //Create a buffer instance which stores all the buffer data
                        VertexBufferHelper helper = new VertexBufferHelper(mdl.VertexBuffers[shp.VertexBufferIndex], TargetWiiUBFRES.ByteOrder);

                        // VertexBufferHelperAttrib uv1 = helper["_u1"];


                        WU.VertexWiiU v = new WU.VertexWiiU();
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
                                            MessageBox.Show("Something went wrong :(");
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
                                            MessageBox.Show("Unsupported Format " + normal.Format);
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
                                            MessageBox.Show("Unsupported Format " + uv0.Format);
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
                                            MessageBox.Show("Unsupported Format " + uv1.Format);
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
                                            MessageBox.Show("Unsupported Format " + c0.Format);
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
                                            MessageBox.Show("Unsupported Format " + t0.Format);
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
                                            MessageBox.Show("Unsupported Format " + b0.Format);
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
                                            MessageBox.Show("Unsupported Format " + w0.Format);
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
                                            v.nodes.Add(new Vector4 { X = i.X, Y = i.Y, Z = i.Z, W = i.W });
                                            break;
                                        default:
                                            MessageBox.Show("Unsupported Format " + i0.Format);
                                            break;
                                    }
                                }
                            }
                        }
                        poly.verticesWiiU = v;

                        uint FaceCount = FaceCount = shp.Meshes[0].IndexCount;

                        uint[] indicesArray = shp.Meshes[0].GetIndices().ToArray();

                        for (int face = 0; face < FaceCount; face++)
                        {
                            poly.faces.Add(new List<int> { (int)indicesArray[face++], (int)indicesArray[face++], (int)indicesArray[face] });
                        }

                        int AlbedoCount = 0;

                        string TextureName = "";
                        int id = 0;
                        foreach (TextureRef tex in mdl.Materials[shp.MaterialIndex].TextureRefs)
                        {
                            TextureName = tex.Name;

                            if (mdl.Materials[shp.MaterialIndex].Samplers[id].Name == "_a0")
                            {
                                if (AlbedoCount == 0) //Only map one albedo for now
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
                                model.mat[shp.MaterialIndex].HasNormalMap = true;
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
                        }

                        foreach (Sampler smp in mdl.Materials[shp.MaterialIndex].Samplers.Values)
                        {
                            SamplerInfo s = new SamplerInfo();
                            s.WrapModeU = (int)smp.TexSampler.ClampX;
                            s.WrapModeV = (int)smp.TexSampler.ClampY;
                            s.WrapModeW = (int)smp.TexSampler.ClampZ;
                            model.mat[shp.MaterialIndex].samplerinfo.Add(s);
                        }

                        using (Syroot.BinaryData.BinaryDataReader reader = new Syroot.BinaryData.BinaryDataReader(new MemoryStream(mdl.Materials[shp.MaterialIndex].ShaderParamData)))
                        {
                            reader.ByteOrder = Syroot.BinaryData.ByteOrder.BigEndian;
                            foreach (Syroot.NintenTools.Bfres.ShaderParam param in mdl.Materials[shp.MaterialIndex].ShaderParams.Values)
                            {
                                ShaderParam prm = new ShaderParam();

                                prm.Name = param.Name;

                                switch (param.Type)
                                {
                                    case ShaderParamType.Float:
                                        reader.Seek(param.DataOffset, SeekOrigin.Begin);
                                        float Vec1 = reader.ReadSingle();
                                        break;
                                    case ShaderParamType.Float3:
                                        reader.Seek(param.DataOffset, SeekOrigin.Begin);
                                        float[] Vec3 = reader.ReadSingles(3);

                                        break;
                                    case ShaderParamType.Float4:
                                        reader.Seek(param.DataOffset, SeekOrigin.Begin);
                                        float dataX = reader.ReadSingle();
                                        float dataY = reader.ReadSingle();
                                        float dataZ = reader.ReadSingle();
                                        float dataW = reader.ReadSingle();
                                        prm.Vec4Data = new Vector4(dataX, dataY, dataZ, dataW);
                                        break;
                                }
                                model.mat[shp.MaterialIndex].matparam.Add(prm);
                            }
                            reader.Close();
                        }
                        model.poly.Add(poly);
                    }
                    models.Add(model);
                    ModelCur++;
                }
                PreRender();
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


        public class FMDLheader
        {
            public string magic;
            public int headerLength1;
            public long headerLength2;
            public string name;
            public long eofString;
            public long fsklOff;
            public long fvtxArrOff;
            public long fvtxOff;
            public long matrOff;
            public long fshpOffset;
            public long fshpIndx;
            public long fmatOffset;
            public long fmatIndx;
            public long UserDataOffset;
            public int fvtxCount;
            public int fshpCount;
            public int fmatCount;
            public int paramCount;
            public int VertCount;
            public int un;
            public int unk2;
            public int padding;
            public long padding1;
            public long padding2;
            public int padding3;
        }
        public class FVTXH
        {
            public int magic = 0x46565458;//FVTX
            public int attCount;
            public int buffCount;
            public int sectIndx;
            public int vertCount;
            public int u1;
            public long attArrOff;
            public long attIndxOff;
            public long buffArrOff;
            public long buffSizeOff;
            public long buffStrideSizeOff;
            public int buffOff;
            public long unk1;
            public long unk2;
            public long unk3;
            public int SkinWeightInfluence;
        }
        public class FMATH
        {
            public string name;
            public long renderInfoOff;
            public long renderInfoIndx;
            public long shaderAssignOff;
            public long u1;
            public long texSelOff;
            public long u2;
            public long texAttSelOff;
            public long texAttIndxOff;
            public long matParamArrOff;
            public long matParamIndxOff;
            public long matParamOff;
            public long userDataOff;
            public long userDataIndxOff;
            public int padding;
            public long volatileFlagOffset;
            public long u3;
            public long samplerSlotOff;
            public long textureSlotOff;
            public int flags;
            public int sectIndx;
            public int rendParamCount;
            public int texSelCount;
            public int texAttSelCount;
            public int matParamCount;
            public int matParamSize;
            public int rawParamDataSize;
            public int userDataCount;
            public int u4;

        }
        public class ShaderParam
        {
            public string Name;
            public int Type;
            public int Size;
            public int Offset;
            public int UniformVar;
            public int Index;
            public Vector4 Vec4Data;
            public float Data_Float;
        }
        public class FSKLH
        {
            public string magic;
            public int HeaderLength1;
            public long HeaderLenght2;
            public long boneIndxOff;
            public long boneArrOff;
            public long invMatrArrOff;
            public long invIndxArrOff;
            public int fsklType; //Flags
            public int boneArrCount;
            public int invIndxArrCount;
            public int exIndxCount;
            public long padding1;
            public long padding2;
            public long padding3;
            public long padding4;
            public long padding5;
            public int u1;
        }
        public class FSHPH
        {
            public string Magic;
            public int polyNameOff;
            public long fvtxOff;
            public long lodMdlOff;
            public long fsklIndxArrOff; //Skin bone index list
            public long boundingBoxOff;
            public long radiusOff;
            public int flags;
            public int sectIndx;
            public int matrFlag;
            public int fmatIndx;
            public int fsklIndx;
            public int fvtxIndx;
            public int u1;
            public int fsklIndxArrCount;
            public int lodMdlCount;
            public int visGrpCount;
            public long u3;
            public long u4;
            public int visGrpNodeOff;
            public int visGrpRangeOff;
            public int visGrpIndxOff;
            public long padding;
            public int VertexSkinCount;
            public int padding2;
            public int[] Node_Array;
        }
        public class attdata
        {
            public string attName;
            public int buffIndx;
            public int buffOff;
            public int vertType;
        }
        public class buffData
        {
            public int buffSize;
            public int strideSize;
            public int DataOffset;
        }
        public class lodmdl
        {
            public int u1;
            public int faceType;
            public int dCount;
            public int visGrpCount;
            public int u3;
            public int visGrpOff;
            public int indxBuffOff;
            public int elmSkip;
        }
        public class Vertex
        {
            public Vector3 pos = new Vector3(0, 0, 0), nrm = new Vector3(0, 0, 0);
            public Vector4 col = new Vector4(2, 2, 2, 2);
            public List<Vector2> tx = new List<Vector2>();
            public List<int> node = new List<int>();
            public List<float> weight = new List<float>();
            public Vector4 tan = new Vector4(0, 0, 0, 0);
            public Vector4 bitan = new Vector4(0, 0, 0, 0);
            public List<int> node1 = new List<int>();
            public List<float> weight1 = new List<float>();
        }
        public class Mesh : TreeNode
        {
            public List<List<int>> faces = new List<List<int>>();
            public List<Vertex> vertices = new List<Vertex>();
            public List<int> texHashs = new List<int>();
            public string name;
            public int MaterialIndex;
            public List<string> TextureMapTypes = new List<string>();
            public WU.VertexWiiU verticesWiiU = new WU.VertexWiiU();
            public int[] SkinIndexList;
            public int matrFlag;
            public int[] BoneFixNode;
            public int fsklindx;

            public Mesh()
            {
                Checked = true;
                ImageKey = "mesh";
                SelectedImageKey = "mesh";
            }
        }
        public class MaterialData
        {
            public List<ShaderParam> matparam = new List<ShaderParam>();
            public string name;
            public List<RenderInfoData> renderinfo = new List<RenderInfoData>();
            public List<SamplerInfo> samplerinfo = new List<SamplerInfo>();
            public bool HasNormalMap = false;
            public List<string> TextureMaps = new List<string>();
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
            public List<MaterialData> mat = new List<MaterialData>();

        }

        public class WU //Wii U BFRES Parse. Temp till i mess with Syroots lib
        {
            public class FMDLheader
            {
                public string FMDL;
                public string name;
                public int eofString;
                public int fsklOff;
                public int fvtxArrOff;
                public int fshpIndx;
                public int fmatIndx;
                public int paramOff;
                public int fvtxCount;
                public int fshpCount;
                public int fmatCount;
                public int paramCount;
            }
            public class FVTXH
            {
                public int attCount;
                public int buffCount;
                public int sectIndx;
                public int vertCount;
                public int u1;
                public int u2;
                public int attArrOff;
                public int attIndxOff;
                public int buffArrOff;
            }
            public class FMATH
            {
                public string name;
                public int matOff;
                public int u1;
                public int sectIndx;
                public int rendParamCount;
                public int texSelCount;
                public int texAttSelCount;
                public int matParamCount;
                public int matParamSize;
                public int u2;
                public int rendParamIndx;
                public int unkMatOff;
                public int shadeOff;
                public int texSelOff;
                public int texAttSelOff;
                public int texAttIndxOff;
                public int matParamArrOff;
                public int matParamIndxOff;
                public int matParamOff;
                public int shadParamIndxOff;
            }
            public class FSKLH
            {
                public int fsklType;
                public int boneArrCount;
                public int invIndxArrCount;
                public int exIndxCount;
                public int u1;
                public int boneIndxOff;
                public int boneArrOff;
                public int invIndxArrOff;
                public int invMatrArrOff;
            }
            public class FSHPH
            {
                public int polyNameOff;
                public int u1;
                public int fvtxIndx;
                public int fmatIndx;
                public int fsklIndx;
                public int sectIndx;
                public int fsklIndxArrCount;
                public int matrFlag;
                public int lodMdlCount;
                public int visGrpCount;
                public int u3;
                public int fvtxOff;
                public int lodMdlOff;
                public int fsklIndxArrOff;
                public int u4;
                public int visGrpNodeOff;
                public int visGrpRangeOff;
                public int visGrpIndxOff;
            }
            public class attdata
            {
                public string attName;
                public int buffIndx;
                public int buffOff;
                public int vertType;
            }
            public class buffData
            {
                public int buffSize;
                public int strideSize;
                public int dataOffset;
            }
            public class lodmdl
            {
                public int u1;
                public int faceType;
                public int dCount;
                public int visGrpCount;
                public int u3;
                public int visGrpOff;
                public int indxBuffOff;
                public int elmSkip;
            }
            public class VertexWiiU
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
            public class Mesh
            {
                public List<List<int>> faces = new List<List<int>>();
                public List<Vertex> vertices = new List<Vertex>();
                public List<int> texHashs = new List<int>();
                public string name;
            }

            public class FMDL_Model
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

                private VBN vbn = new VBN();
                public List<Mesh> poly = new List<Mesh>();
                public bool isVisible = true;
            }
        }

        public void readFSKA()
        {

        }


    }
}
