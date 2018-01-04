using System;
using System.Collections.Generic;
using SALT.Graphics;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Smash_Forge
{
    public class ModelContainer : TreeNode
    {
        public NUD NUD {
            get
            {
                return nud;
            }
            set
            {
                nud = value;
                if (xmb != null)
                    nud.SetPropertiesFromXMB(xmb);
                Refresh();
            }
        }
        private NUD nud;
        public NUT NUT
        {
            get
            {
                return nut;
            }
            set
            {
                nut = value;
                Refresh();
            }
        }
        private NUT nut;
        public VBN VBN
        {
            get
            {
                return vbn;
            }
            set
            {
                vbn = value;
                if (vbn == null)
                    vbn = new VBN();
                if (JTB != null)
                    vbn.JointTable = JTB;
                Refresh();
            }
        }
        private VBN vbn;
        public MTA MTA
        {
            get
            {
                return mta;
            }
            set
            {
                mta = value;
                Refresh();
            }
        }
        public MTA mta;
        public MOI MOI
        {
            get
            {
                return moi;
            }
            set
            {
                moi = value;
                Refresh();
            }
        }
        private MOI moi;
        public JTB JTB
        {
            get
            {
                return jtb;
            }
            set
            {
                jtb = value;
                if(VBN != null)
                    VBN.JointTable = jtb;
                Refresh();
            }
        }
        private JTB jtb;
        public XMBFile XMB
        {
            get
            {
                return xmb;
            }
            set
            {
                xmb = value;
                if (NUD != null)
                    NUD.SetPropertiesFromXMB(xmb);
                Refresh();
            }
        }
        private XMBFile xmb;

        // Other Model Formats
        public BCH bch;
        public BCH BCH
        {
            get
            {
                return bch;
            }
            set
            {
                bch = value;
                Refresh();
            }
        }
        public DAT dat_melee;

        public static Dictionary<string, SkelAnimation> Animations { get; set; }
        public static MovesetManager Moveset { get; set; }

        public ModelContainer()
        {
            ImageKey = "folder";
            SelectedImageKey = "folder";
            nud = new NUD();
            nut = new NUT();
            vbn = new VBN();
            mta = new MTA();
            MOI = new MOI();
            jtb = new JTB();
            XMB = new XMBFile();
            Checked = true;
            Refresh();
        }

        public void Refresh()
        {
            Nodes.Clear();

            if(bch != null)
            {
                Nodes.Add(bch);
            }else
            {
                if (nud != null) Nodes.Add(nud);
                if (nut != null) Nodes.Add(nut);
                if (vbn != null && vbn.Parent == null) Nodes.Add(vbn);
                if (mta != null) Nodes.Add(mta);
                if (moi != null) Nodes.Add(moi);
                if (jtb != null) Nodes.Add(jtb);
                if (xmb != null) Nodes.Add(
                    new TreeNode()
                    {
                        Tag = xmb,
                        Text = "model.xmb",
                        ImageKey = "info",
                        SelectedImageKey = "info"
                    });
            }
        }

        /*
         * This method is for clearing all the GL stuff
         * Don't want wasted buffers :>
         * */
        public void Destroy()
        {
            if(NUD != null)
                NUD.Destroy();
        }

        public VBN GetVBN()
        {
            if (BCH != null)
                if (BCH.Models.Nodes.Count > 0)
                {
                    ((BCH_Model)BCH.Models.Nodes[0]).skeleton.JointTable = JTB;
                    return ((BCH_Model)BCH.Models.Nodes[0]).skeleton;
                }
            if (VBN != null)
            {
                return VBN;
            }
            return null;
        }

        public void Render(Camera camera, int depthmap, Matrix4 lightMatrix, Matrix4 modelMatrix, bool specialWireFrame = false)
        {
            if (!Checked) return;
            Shader shader;
            if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                shader = Runtime.shaders["NUD_Debug"];
            else
                shader = Runtime.shaders["nud"];
            GL.UseProgram(shader.programID);

            int renderType = (int)Runtime.renderType;
            
            Matrix4 mvpMatrix = camera.getMVPMatrix();
            GL.UniformMatrix4(shader.getAttribute("mvpMatrix"), false, ref mvpMatrix);

            // Perform the calculations here to reduce render times in shader
            Matrix4 sphereMapMatrix = camera.getModelViewMatrix();
            sphereMapMatrix.Invert();
            sphereMapMatrix.Transpose();
            GL.UniformMatrix4(shader.getAttribute("sphereMapMatrix"), false, ref sphereMapMatrix);

            #region MBN Uniforms

            shader = Runtime.shaders["MBN"];
            GL.UseProgram(shader.programID);

            if (Runtime.cameraLight)
            {
                GL.Uniform3(shader.getAttribute("difLightDirection"), Vector3.TransformNormal(new Vector3(0f, 0f, -1f), camera.getMVPMatrix().Inverted()).Normalized());
            }
            else
            {
                GL.Uniform3(shader.getAttribute("difLightDirection"), Lights.diffuseLight.direction);
            }

            #endregion

            #region DAT uniforms
            shader = Runtime.shaders["DAT"];
            GL.UseProgram(shader.programID);

            GL.Uniform3(shader.getAttribute("difLightColor"), Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB);
            GL.Uniform3(shader.getAttribute("ambLightColor"), Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB);
            
            #endregion

            {
                if (BCH != null)
                {
                    foreach (BCH_Model mo in BCH.Models.Nodes)
                    {
                        mo.Render(camera.getMVPMatrix());
                    }
                }

                if (dat_melee != null && Runtime.shaders["DAT"].shadersCompiledSuccessfully())
                {
                    dat_melee.Render(camera.getMVPMatrix());
                }

                if (NUD != null && Runtime.shaders["nud"].shadersCompiledSuccessfully() && Runtime.shaders["NUD_Debug"].shadersCompiledSuccessfully())
                {
                    if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                        shader = Runtime.shaders["NUD_Debug"];
                    else
                        shader = Runtime.shaders["nud"];

                    GL.UseProgram(shader.programID);

                    SetRenderSettingsUniforms(shader);
                    SetLightingUniforms(shader, camera);

                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.TextureCubeMap, RenderTools.cubeMapHigh);
                    GL.Uniform1(shader.getAttribute("cmap"), 2);

                    GL.ActiveTexture(TextureUnit.Texture11);
                    GL.BindTexture(TextureTarget.Texture2D, depthmap);
                    GL.Uniform1(shader.getAttribute("shadowMap"), 11);

                    GL.Uniform1(shader.getAttribute("renderType"), renderType);
                    
                    GL.Uniform1(shader.getAttribute("elapsedTime"), (DateTime.Now.Second / 60f) * 100);

                    GL.UniformMatrix4(shader.getAttribute("modelMatrix"), false, ref modelMatrix);
                    GL.UniformMatrix4(shader.getAttribute("lightSpaceMatrix"), false, ref lightMatrix);

                    if (specialWireFrame)
                    {
                        Runtime.renderModelWireframe = true;
                        Runtime.renderModel = false;
                    }

                    NUD.Render(VBN, camera);                    
                }
            }
        }

        public void RenderPoints(Camera camera)
        {
            if (NUD != null)
            {
                NUD.DrawPoints(camera, VBN, PrimitiveType.Triangles);
                NUD.DrawPoints(camera, VBN, PrimitiveType.Points);
            }
        }

        public void RenderShadow(Camera camera, int depthmap, Matrix4 lightMatrix, Matrix4 modelMatrix)
        {
            // critical to clear depth buffer
            GL.Clear(ClearBufferMask.DepthBufferBit);

            if (NUD != null)
            {
                NUD.RenderShadow(lightMatrix, camera.getMVPMatrix(), modelMatrix);
            }

            Matrix4 matrix = camera.getMVPMatrix();
        }

        public void RenderBones()
        {
            if (VBN != null)
                RenderTools.DrawVBN(VBN);

            if (BCH != null)
            {
                foreach (BCH_Model mo in BCH.Models.Nodes)
                    RenderTools.DrawVBN(mo.skeleton);
            }

            if (dat_melee != null)
            {
                RenderTools.DrawVBN(dat_melee.bones);
            }
        }

        private static void SetRenderSettingsUniforms(Shader shader)
        {
            GL.Uniform1(shader.getAttribute("renderStageLighting"), Runtime.renderStageLighting ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderLighting"), Runtime.renderMaterialLighting ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderVertColor"), Runtime.renderVertColor ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderAlpha"), Runtime.renderAlpha ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderDiffuse"), Runtime.renderDiffuse ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderFresnel"), Runtime.renderFresnel ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderSpecular"), Runtime.renderSpecular ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderReflection"), Runtime.renderReflection ? 1 : 0);

            GL.Uniform1(shader.getAttribute("useNormalMap"), Runtime.renderNormalMap ? 1 : 0);

            GL.Uniform1(shader.getAttribute("ambientIntensity"), Runtime.amb_inten);
            GL.Uniform1(shader.getAttribute("diffuseIntensity"), Runtime.dif_inten);
            GL.Uniform1(shader.getAttribute("specularIntensity"), Runtime.spc_inten);
            GL.Uniform1(shader.getAttribute("fresnelIntensity"), Runtime.frs_inten);
            GL.Uniform1(shader.getAttribute("reflectionIntensity"), Runtime.ref_inten);

            GL.Uniform1(shader.getAttribute("zScale"), Runtime.zScale);

            GL.Uniform1(shader.getAttribute("renderR"), Runtime.renderR ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderG"), Runtime.renderG ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderB"), Runtime.renderB ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderAlpha"), Runtime.renderAlpha ? 1 : 0);

            GL.Uniform1(shader.getAttribute("uvChannel"), (int)Runtime.uvChannel);

            bool alphaOverride = Runtime.renderAlpha && !Runtime.renderR && !Runtime.renderG && !Runtime.renderB;
            GL.Uniform1(shader.getAttribute("alphaOverride"), alphaOverride ? 1 : 0);

            GL.Uniform3(shader.getAttribute("lightSetColor"), 0, 0, 0);

            GL.Uniform1(shader.getAttribute("colorOverride"), 0);

            GL.Uniform1(shader.getAttribute("debug1"), Runtime.debug1 ? 1 : 0);
            GL.Uniform1(shader.getAttribute("debug2"), Runtime.debug2 ? 1 : 0);

        }

        private static void SetLightingUniforms(Shader shader, Camera camera)
        {
            // fresnel sky/ground color for characters & stages
            GL.Uniform3(shader.getAttribute("fresGroundColor"), Lights.fresnelLight.groundR, Lights.fresnelLight.groundG, Lights.fresnelLight.groundB);
            GL.Uniform3(shader.getAttribute("fresSkyColor"), Lights.fresnelLight.skyR, Lights.fresnelLight.skyG, Lights.fresnelLight.skyB);
            GL.Uniform3(shader.getAttribute("fresSkyDirection"), Lights.fresnelLight.getSkyDirection());
            GL.Uniform3(shader.getAttribute("fresGroundDirection"), Lights.fresnelLight.getGroundDirection());

            // reflection color for characters & stages
            float refR, refG, refB = 1.0f;
            ColorTools.HSV2RGB(Runtime.reflection_hue, Runtime.reflection_saturation, Runtime.reflection_intensity, out refR, out refG, out refB);
            GL.Uniform3(shader.getAttribute("refLightColor"), refR, refG, refB);

            // character diffuse lights
            GL.Uniform3(shader.getAttribute("difLightColor"), Lights.diffuseLight.difR, Lights.diffuseLight.difG, Lights.diffuseLight.difB);
            GL.Uniform3(shader.getAttribute("ambLightColor"), Lights.diffuseLight.ambR, Lights.diffuseLight.ambG, Lights.diffuseLight.ambB);

            GL.Uniform3(shader.getAttribute("difLightColor2"), Lights.diffuseLight2.difR, Lights.diffuseLight2.difG, Lights.diffuseLight2.difB);
            GL.Uniform3(shader.getAttribute("ambLightColor2"), Lights.diffuseLight2.ambR, Lights.diffuseLight2.ambG, Lights.diffuseLight2.ambB);

            GL.Uniform3(shader.getAttribute("difLightColor3"), Lights.diffuseLight3.difR, Lights.diffuseLight3.difG, Lights.diffuseLight3.difB);
            GL.Uniform3(shader.getAttribute("ambLightColor3"), Lights.diffuseLight3.ambR, Lights.diffuseLight3.ambG, Lights.diffuseLight3.ambB);

            // character specular light
            Lights.specularLight.setColorFromHSV(Runtime.specular_hue, Runtime.specular_saturation, Runtime.specular_intensity);
            Lights.specularLight.setDirectionFromXYZAngles(Runtime.specular_rotX, Runtime.specular_rotY, Runtime.specular_rotZ);
            GL.Uniform3(shader.getAttribute("specLightColor"), Lights.specularLight.difR, Lights.specularLight.difG, Lights.specularLight.difB);

            // stage fog
            GL.Uniform1(shader.getAttribute("renderFog"), Runtime.renderFog ? 1 : 0);

            GL.Uniform3(shader.getAttribute("difLight2Direction"), Lights.diffuseLight2.direction);
            GL.Uniform3(shader.getAttribute("difLight3Direction"), Lights.diffuseLight2.direction);

            GL.Uniform3(shader.getAttribute("stageLight1Direction"), Lights.stageLight1.direction);
            GL.Uniform3(shader.getAttribute("stageLight2Direction"), Lights.stageLight2.direction);
            GL.Uniform3(shader.getAttribute("stageLight3Direction"), Lights.stageLight3.direction);
            GL.Uniform3(shader.getAttribute("stageLight4Direction"), Lights.stageLight4.direction);


            if (Runtime.cameraLight) // camera light should only affects character lighting
            {
                Matrix4 invertedCamera = camera.getMVPMatrix().Inverted();
                Vector3 lightDirection = new Vector3(0f, 0f, -1f);
                GL.Uniform3(shader.getAttribute("lightDirection"), Vector3.TransformNormal(lightDirection, invertedCamera).Normalized());
                GL.Uniform3(shader.getAttribute("specLightDirection"), Vector3.TransformNormal(lightDirection, invertedCamera).Normalized());
                GL.Uniform3(shader.getAttribute("difLightDirection"), Vector3.TransformNormal(lightDirection, invertedCamera).Normalized());
            }
            else
            {
                GL.Uniform3(shader.getAttribute("specLightDirection"), Lights.specularLight.direction);
                GL.Uniform3(shader.getAttribute("difLightDirection"), Lights.diffuseLight.direction);
            }
        }


        #region Editing Tools
        
        public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
        {
            public int Compare(TKey x, TKey y)
            {
                int result = x.CompareTo(y);

                if (result == 0)
                    return 1;
                else
                    return result;
            }
        }

        public SortedList<double, NUD.Mesh> GetMeshSelection(Ray ray)
        {
            SortedList<double, NUD.Mesh> selected = new SortedList<double, NUD.Mesh>(new DuplicateKeyComparer<double>());
            if (NUD != null)
            {
                Vector3 closest = Vector3.Zero;
                foreach (NUD.Mesh mesh in NUD.Nodes)
                {
                    if (ray.CheckSphereHit(new Vector3(mesh.boundingBox[0], mesh.boundingBox[1], mesh.boundingBox[2]), mesh.boundingBox[3], out closest))
                        selected.Add(ray.Distance(closest), mesh);
                }
            }
            return selected;
        }

        public SortedList<double, Bone> GetBoneSelection(Ray ray)
        {
            SortedList<double, Bone> selected = new SortedList<double, Bone>(new DuplicateKeyComparer<double>());
            if (VBN != null)
            {
                Vector3 closest = Vector3.Zero;
                foreach (Bone b in VBN.bones)
                {
                    if (ray.CheckSphereHit(Vector3.Transform(Vector3.Zero, b.transform), 2, out closest))
                        selected.Add(ray.Distance(closest), b);
                }
            }
            return selected;
        }

        #endregion

    }
}

