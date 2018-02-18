using System;
using System.Collections.Generic;
using SALT.Graphics;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
using System.Diagnostics;
using Smash_Forge.Rendering.Lights;


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
        public DAT DAT_MELEE
        {
            get
            {
                return dat_melee;
            }
            set
            {
                dat_melee = value;
                Refresh();
            }
        }

        private DAT dat_melee;

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

            if(DAT_MELEE != null)
            {
                Text = "Melee DAT";
                Nodes.AddRange(DAT_MELEE.tree.ToArray());
            }else
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
                shader = Runtime.shaders["NUD"];
            GL.UseProgram(shader.programID);

            int renderType = (int)Runtime.renderType;
            
            Matrix4 mvpMatrix = camera.getMVPMatrix();
            GL.UniformMatrix4(shader.getAttribute("mvpMatrix"), false, ref mvpMatrix);

            // Perform the calculations here to reduce render times in shader
            Matrix4 modelViewMatrix = camera.getModelViewMatrix();
            Matrix4 sphereMapMatrix = modelViewMatrix;
            sphereMapMatrix.Invert();
            sphereMapMatrix.Transpose();
            GL.UniformMatrix4(shader.getAttribute("modelViewMatrix"), false, ref modelViewMatrix);
            GL.UniformMatrix4(shader.getAttribute("sphereMapMatrix"), false, ref sphereMapMatrix);

            Matrix4 rotationMatrix = camera.getRotationMatrix();
            GL.UniformMatrix4(shader.getAttribute("rotationMatrix"), false, ref rotationMatrix);


            #region MBN Uniforms

            shader = Runtime.shaders["MBN"];
            GL.UseProgram(shader.programID);

            if (Runtime.cameraLight)
            {
                GL.Uniform3(shader.getAttribute("difLightDirection"), Vector3.TransformNormal(new Vector3(0f, 0f, -1f), camera.getMVPMatrix().Inverted()).Normalized());
            }
            else
            {
                GL.Uniform3(shader.getAttribute("difLightDirection"), LightTools.diffuseLight.direction);
            }

            #endregion

            #region DAT uniforms
            shader = Runtime.shaders["DAT"];
            GL.UseProgram(shader.programID);

            GL.Uniform3(shader.getAttribute("difLightColor"), LightTools.diffuseLight.difR, LightTools.diffuseLight.difG, LightTools.diffuseLight.difB);
            GL.Uniform3(shader.getAttribute("ambLightColor"), LightTools.diffuseLight.ambR, LightTools.diffuseLight.ambG, LightTools.diffuseLight.ambB);
            
            #endregion

            {
                if (BCH != null)
                {
                    foreach (BCH_Model mo in BCH.Models.Nodes)
                    {
                        mo.Render(camera.getMVPMatrix());
                    }
                }

                if (DAT_MELEE != null && Runtime.shaders["DAT"].shadersCompiledSuccessfully())
                {
                    DAT_MELEE.Render(camera.getMVPMatrix());
                }

                if (NUD != null && Runtime.shaders["NUD"].shadersCompiledSuccessfully() && Runtime.shaders["NUD_Debug"].shadersCompiledSuccessfully())
                {
                    if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                        shader = Runtime.shaders["NUD_Debug"];
                    else
                        shader = Runtime.shaders["NUD"];

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
                    GL.Uniform1(shader.getAttribute("debugOption"), (int)Runtime.uvChannel);

                    float elapsedSeconds = 0;
                    if (NUD.useDirectUVTime)
                    {
                        elapsedSeconds = ModelViewport.directUVTimeStopWatch.ElapsedMilliseconds / 1000.0f;
                        // Should be based on XMB eventualy.
                        if (elapsedSeconds >= 100)
                        {
                            ModelViewport.directUVTimeStopWatch.Restart();
                        }
                    }
                    else
                        ModelViewport.directUVTimeStopWatch.Stop();

                    GL.Uniform1(shader.getAttribute("elapsedTime"), elapsedSeconds);

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

            if (DAT_MELEE != null)
            {
                RenderTools.DrawVBN(DAT_MELEE.bones);
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

            GL.Uniform1(shader.getAttribute("ambientIntensity"), Runtime.ambItensity);
            GL.Uniform1(shader.getAttribute("diffuseIntensity"), Runtime.difIntensity);
            GL.Uniform1(shader.getAttribute("specularIntensity"), Runtime.spcIntentensity);
            GL.Uniform1(shader.getAttribute("fresnelIntensity"), Runtime.frsIntensity);
            GL.Uniform1(shader.getAttribute("reflectionIntensity"), Runtime.refIntensity);

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
            GL.Uniform3(shader.getAttribute("fresGroundColor"), LightTools.fresnelLight.groundR, LightTools.fresnelLight.groundG, LightTools.fresnelLight.groundB);
            GL.Uniform3(shader.getAttribute("fresSkyColor"), LightTools.fresnelLight.skyR, LightTools.fresnelLight.skyG, LightTools.fresnelLight.skyB);
            GL.Uniform3(shader.getAttribute("fresSkyDirection"), LightTools.fresnelLight.getSkyDirection());
            GL.Uniform3(shader.getAttribute("fresGroundDirection"), LightTools.fresnelLight.getGroundDirection());

            // reflection color for characters & stages
            float refR, refG, refB = 1.0f;
            ColorTools.HSV2RGB(Runtime.reflectionHue, Runtime.reflectionSaturation, Runtime.reflectionIntensity, out refR, out refG, out refB);
            GL.Uniform3(shader.getAttribute("refLightColor"), refR, refG, refB);

            // character diffuse lights
            GL.Uniform3(shader.getAttribute("difLightColor"), LightTools.diffuseLight.difR, LightTools.diffuseLight.difG, LightTools.diffuseLight.difB);
            GL.Uniform3(shader.getAttribute("ambLightColor"), LightTools.diffuseLight.ambR, LightTools.diffuseLight.ambG, LightTools.diffuseLight.ambB);

            GL.Uniform3(shader.getAttribute("difLightColor2"), LightTools.diffuseLight2.difR, LightTools.diffuseLight2.difG, LightTools.diffuseLight2.difB);
            GL.Uniform3(shader.getAttribute("ambLightColor2"), LightTools.diffuseLight2.ambR, LightTools.diffuseLight2.ambG, LightTools.diffuseLight2.ambB);

            GL.Uniform3(shader.getAttribute("difLightColor3"), LightTools.diffuseLight3.difR, LightTools.diffuseLight3.difG, LightTools.diffuseLight3.difB);
            GL.Uniform3(shader.getAttribute("ambLightColor3"), LightTools.diffuseLight3.ambR, LightTools.diffuseLight3.ambG, LightTools.diffuseLight3.ambB);

            // character specular light
            LightTools.specularLight.setColorFromHSV(Runtime.specularHue, Runtime.specularSaturation, Runtime.specularIntensity);
            LightTools.specularLight.setDirectionFromXYZAngles(Runtime.specularRotX, Runtime.specularRotY, Runtime.specularRotZ);
            GL.Uniform3(shader.getAttribute("specLightColor"), LightTools.specularLight.difR, LightTools.specularLight.difG, LightTools.specularLight.difB);
            
            // stage fog
            GL.Uniform1(shader.getAttribute("renderFog"), Runtime.renderFog ? 1 : 0);

            GL.Uniform3(shader.getAttribute("difLight2Direction"), LightTools.diffuseLight2.direction);
            GL.Uniform3(shader.getAttribute("difLight3Direction"), LightTools.diffuseLight2.direction);

            GL.Uniform3(shader.getAttribute("stageLight1Direction"), LightTools.stageLight1.direction);
            GL.Uniform3(shader.getAttribute("stageLight2Direction"), LightTools.stageLight2.direction);
            GL.Uniform3(shader.getAttribute("stageLight3Direction"), LightTools.stageLight3.direction);
            GL.Uniform3(shader.getAttribute("stageLight4Direction"), LightTools.stageLight4.direction);


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
                GL.Uniform3(shader.getAttribute("specLightDirection"), LightTools.specularLight.direction);
                GL.Uniform3(shader.getAttribute("difLightDirection"), LightTools.diffuseLight.direction);
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
                    if (ray.CheckSphereHit(Vector3.TransformPosition(Vector3.Zero, b.transform), 1, out closest))
                        selected.Add(ray.Distance(closest), b);
                }
            }
            return selected;
        }

        #endregion

    }
}

