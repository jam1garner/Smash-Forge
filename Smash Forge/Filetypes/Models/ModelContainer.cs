using System;
using System.Collections.Generic;
using SALT.Graphics;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
using System.Diagnostics;
using Smash_Forge.Rendering.Lights;
using Smash_Forge.Rendering;


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

                nud.CheckTexIdErrors(nut);
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
                VBN = dat_melee.bones;
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
                if (vbn != null && vbn.Parent == null) Nodes.Add(vbn);
            }
            else
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
            if (NUD != null)
                NUD.Destroy();
            if (NUT != null)
                NUT.Destroy();
        }

        public VBN GetVBN()
        {
            if (BCH != null && BCH.Models.Nodes.Count > 0)
            {
                ((BCH_Model)BCH.Models.Nodes[0]).skeleton.JointTable = JTB;
                return ((BCH_Model)BCH.Models.Nodes[0]).skeleton;
            }
            else if (vbn != null)
            {
                return vbn;
            }
            return null;
        }

        public void Render(Camera camera, int depthmap, Matrix4 lightMatrix, Matrix4 modelMatrix, bool specialWireFrame = false)
        {
            if (!Checked)
                return;

            Shader shader;
            if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                shader = Runtime.shaders["NUD_Debug"];
            else
                shader = Runtime.shaders["NUD"];
            GL.UseProgram(shader.programID);

            int renderType = (int)Runtime.renderType;
            
            Matrix4 mvpMatrix = camera.mvpMatrix;
            GL.UniformMatrix4(shader.GetVertexAttributeLocation("mvpMatrix"), false, ref mvpMatrix);

            // Perform the calculations here to reduce render times in shader
            Matrix4 modelViewMatrix = camera.modelViewMatrix;
            Matrix4 sphereMapMatrix = modelViewMatrix;
            sphereMapMatrix.Invert();
            sphereMapMatrix.Transpose();
            GL.UniformMatrix4(shader.GetVertexAttributeLocation("modelViewMatrix"), false, ref modelViewMatrix);
            GL.UniformMatrix4(shader.GetVertexAttributeLocation("sphereMapMatrix"), false, ref sphereMapMatrix);

            Matrix4 rotationMatrix = camera.rotationMatrix;
            GL.UniformMatrix4(shader.GetVertexAttributeLocation("rotationMatrix"), false, ref rotationMatrix);

            shader = Runtime.shaders["MBN"];
            GL.UseProgram(shader.programID);

            if (Runtime.cameraLight)
            {
                GL.Uniform3(shader.GetVertexAttributeLocation("difLightDirection"), Vector3.TransformNormal(new Vector3(0f, 0f, -1f), camera.mvpMatrix.Inverted()).Normalized());
            }
            else
            {
                GL.Uniform3(shader.GetVertexAttributeLocation("difLightDirection"), Runtime.lightSetParam.characterDiffuse.direction);
            }

            shader = Runtime.shaders["DAT"];
            GL.UseProgram(shader.programID);

            LightColor diffuseColor = Runtime.lightSetParam.characterDiffuse.diffuseColor;
            LightColor ambientColor = Runtime.lightSetParam.characterDiffuse.ambientColor;
            GL.Uniform3(shader.GetVertexAttributeLocation("difLightColor"), diffuseColor.R, diffuseColor.G, diffuseColor.B);
            GL.Uniform3(shader.GetVertexAttributeLocation("ambLightColor"), ambientColor.R, ambientColor.G, ambientColor.B);
            
            
            if (BCH != null)
            {
                foreach (BCH_Model mo in BCH.Models.Nodes)
                {
                    mo.Render(camera.mvpMatrix);
                }
            }

            if (DAT_MELEE != null && Runtime.shaders["DAT"].CompiledSuccessfully())
            {
                DAT_MELEE.Render(camera.mvpMatrix);
            }

            if (NUD != null && Runtime.shaders["NUD"].CompiledSuccessfully() && Runtime.shaders["NUD_Debug"].CompiledSuccessfully())
            {
                if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                    shader = Runtime.shaders["NUD_Debug"];
                else
                    shader = Runtime.shaders["NUD"];

                GL.UseProgram(shader.programID);

                SetRenderSettingsUniforms(shader);
                SetLightingUniforms(shader, camera);

                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.TextureCubeMap, RenderTools.dummyTextures[NUD.DummyTextures.StageMapHigh]);
                GL.Uniform1(shader.GetVertexAttributeLocation("cmap"), 2);

                GL.Uniform1(shader.GetVertexAttributeLocation("renderType"), renderType);
                GL.Uniform1(shader.GetVertexAttributeLocation("debugOption"), (int)Runtime.uvChannel);

                SetElapsedDirectUvTime(shader);

                GL.UniformMatrix4(shader.GetVertexAttributeLocation("modelMatrix"), false, ref modelMatrix);

                if (specialWireFrame)
                {
                    Runtime.renderModelWireframe = true;
                    Runtime.renderModel = false;
                }

                NUD.Render(VBN, camera);
            }
        }

        private void SetElapsedDirectUvTime(Shader shader)
        {
            float elapsedSeconds = 0;
            if (NUD.useDirectUVTime)
            {
                elapsedSeconds = ModelViewport.directUVTimeStopWatch.ElapsedMilliseconds / 1000.0f;
                // Should be based on XMB eventualy.
                if (elapsedSeconds >= 100)
                    ModelViewport.directUVTimeStopWatch.Restart();
            }
            else
                ModelViewport.directUVTimeStopWatch.Stop();

            GL.Uniform1(shader.GetVertexAttributeLocation("elapsedTime"), elapsedSeconds);
        }

        public void RenderPoints(Camera camera)
        {
            if (NUD != null)
            {
                NUD.DrawPoints(camera, VBN, PrimitiveType.Triangles);
                NUD.DrawPoints(camera, VBN, PrimitiveType.Points);
            }
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
            GL.Uniform1(shader.GetVertexAttributeLocation("renderStageLighting"), Runtime.renderStageLighting ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("renderLighting"), Runtime.renderMaterialLighting ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("renderVertColor"), Runtime.renderVertColor ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("renderAlpha"), Runtime.renderAlpha ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("renderDiffuse"), Runtime.renderDiffuse ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("renderFresnel"), Runtime.renderFresnel ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("renderSpecular"), Runtime.renderSpecular ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("renderReflection"), Runtime.renderReflection ? 1 : 0);

            GL.Uniform1(shader.GetVertexAttributeLocation("useNormalMap"), Runtime.renderNormalMap ? 1 : 0);

            GL.Uniform1(shader.GetVertexAttributeLocation("ambientIntensity"), Runtime.ambItensity);
            GL.Uniform1(shader.GetVertexAttributeLocation("diffuseIntensity"), Runtime.difIntensity);
            GL.Uniform1(shader.GetVertexAttributeLocation("specularIntensity"), Runtime.spcIntentensity);
            GL.Uniform1(shader.GetVertexAttributeLocation("fresnelIntensity"), Runtime.frsIntensity);
            GL.Uniform1(shader.GetVertexAttributeLocation("reflectionIntensity"), Runtime.refIntensity);

            GL.Uniform1(shader.GetVertexAttributeLocation("zScale"), Runtime.zScale);

            GL.Uniform1(shader.GetVertexAttributeLocation("renderR"), Runtime.renderR ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("renderG"), Runtime.renderG ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("renderB"), Runtime.renderB ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("renderAlpha"), Runtime.renderAlpha ? 1 : 0);

            GL.Uniform1(shader.GetVertexAttributeLocation("uvChannel"), (int)Runtime.uvChannel);

            bool alphaOverride = Runtime.renderAlpha && !Runtime.renderR && !Runtime.renderG && !Runtime.renderB;
            GL.Uniform1(shader.GetVertexAttributeLocation("alphaOverride"), alphaOverride ? 1 : 0);

            GL.Uniform3(shader.GetVertexAttributeLocation("lightSetColor"), 0, 0, 0);

            GL.Uniform1(shader.GetVertexAttributeLocation("colorOverride"), 0);

            GL.Uniform1(shader.GetVertexAttributeLocation("debug1"), Runtime.debug1 ? 1 : 0);
            GL.Uniform1(shader.GetVertexAttributeLocation("debug2"), Runtime.debug2 ? 1 : 0);

        }

        private static void SetLightingUniforms(Shader shader, Camera camera)
        {
            // fresnel sky/ground color for characters & stages
            ShaderTools.LightColorVector3Uniform(shader, Runtime.lightSetParam.fresnelLight.groundColor, "fresGroundColor");
            ShaderTools.LightColorVector3Uniform(shader, Runtime.lightSetParam.fresnelLight.skyColor, "fresSkyColor");
            GL.Uniform3(shader.GetVertexAttributeLocation("fresSkyDirection"), Runtime.lightSetParam.fresnelLight.getSkyDirection());
            GL.Uniform3(shader.GetVertexAttributeLocation("fresGroundDirection"), Runtime.lightSetParam.fresnelLight.getGroundDirection());

            // reflection color for characters & stages
            float refR, refG, refB = 1.0f;
            ColorTools.HsvToRgb(Runtime.reflectionHue, Runtime.reflectionSaturation, Runtime.reflectionIntensity, out refR, out refG, out refB);
            GL.Uniform3(shader.GetVertexAttributeLocation("refLightColor"), refR, refG, refB);
            
            // character diffuse lights
            GL.Uniform3(shader.GetVertexAttributeLocation("difLightColor"), Runtime.lightSetParam.characterDiffuse.diffuseColor.R, Runtime.lightSetParam.characterDiffuse.diffuseColor.G, Runtime.lightSetParam.characterDiffuse.diffuseColor.B);
            GL.Uniform3(shader.GetVertexAttributeLocation("ambLightColor"), Runtime.lightSetParam.characterDiffuse.ambientColor.R, Runtime.lightSetParam.characterDiffuse.ambientColor.G, Runtime.lightSetParam.characterDiffuse.ambientColor.B);

            GL.Uniform3(shader.GetVertexAttributeLocation("difLightColor2"), Runtime.lightSetParam.characterDiffuse2.diffuseColor.R, Runtime.lightSetParam.characterDiffuse2.diffuseColor.G, Runtime.lightSetParam.characterDiffuse2.diffuseColor.B);
            GL.Uniform3(shader.GetVertexAttributeLocation("ambLightColor2"), Runtime.lightSetParam.characterDiffuse2.ambientColor.R, Runtime.lightSetParam.characterDiffuse2.ambientColor.G, Runtime.lightSetParam.characterDiffuse2.ambientColor.B);

            GL.Uniform3(shader.GetVertexAttributeLocation("difLightColor3"), Runtime.lightSetParam.characterDiffuse3.diffuseColor.R, Runtime.lightSetParam.characterDiffuse3.diffuseColor.G, Runtime.lightSetParam.characterDiffuse3.diffuseColor.B);
            GL.Uniform3(shader.GetVertexAttributeLocation("ambLightColor3"), Runtime.lightSetParam.characterDiffuse3.ambientColor.R, Runtime.lightSetParam.characterDiffuse3.ambientColor.G, Runtime.lightSetParam.characterDiffuse3.ambientColor.B);
            
            // character specular light
            GL.Uniform3(shader.GetVertexAttributeLocation("specLightColor"), LightTools.specularLight.diffuseColor.R, LightTools.specularLight.diffuseColor.G, LightTools.specularLight.diffuseColor.B);
            
            // stage fog
            GL.Uniform1(shader.GetVertexAttributeLocation("renderFog"), Runtime.renderFog ? 1 : 0);

            GL.Uniform3(shader.GetVertexAttributeLocation("difLight2Direction"), Runtime.lightSetParam.characterDiffuse2.direction);
            GL.Uniform3(shader.GetVertexAttributeLocation("difLight3Direction"), Runtime.lightSetParam.characterDiffuse2.direction);

            if (Runtime.cameraLight) 
            {
                // Camera light should only affect character lighting.
                Matrix4 invertedCamera = camera.mvpMatrix.Inverted();
                Vector3 lightDirection = new Vector3(0f, 0f, -1f);
                GL.Uniform3(shader.GetVertexAttributeLocation("lightDirection"), Vector3.TransformNormal(lightDirection, invertedCamera).Normalized());
                GL.Uniform3(shader.GetVertexAttributeLocation("specLightDirection"), Vector3.TransformNormal(lightDirection, invertedCamera).Normalized());
                GL.Uniform3(shader.GetVertexAttributeLocation("difLightDirection"), Vector3.TransformNormal(lightDirection, invertedCamera).Normalized());
            }
            else
            {
                GL.Uniform3(shader.GetVertexAttributeLocation("specLightDirection"), LightTools.specularLight.direction);
                GL.Uniform3(shader.GetVertexAttributeLocation("difLightDirection"), Runtime.lightSetParam.characterDiffuse.direction);
            }
        }

        public void DepthSortModels(Vector3 cameraPosition)
        {
            if (NUD != null)
                NUD.DepthSortMeshes(cameraPosition);
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

