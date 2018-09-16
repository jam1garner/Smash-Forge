using OpenTK;
using OpenTK.Graphics.OpenGL;
using SALT.Graphics;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.Utils;
using Smash_Forge.Rendering;
using Smash_Forge.Rendering.Lights;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SFGraphics.GLObjects.Textures;

namespace Smash_Forge
{
    public class ModelContainer : TreeNode
    {
        public NUD NUD
        {
            get
            {
                return nud;
            }
            set
            {
                nud = value;
                if (nud != null && xmb != null)
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

                if (nud != null)
                    nud.CheckTexIdErrors(nut);
            }
        }
        private NUT nut;

        public BNTX BNTX
        {
            get
            {
                return bntx;
            }
            set
            {
                bntx = value;
                Refresh();

            }
        }
        private BNTX bntx;

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
        private MTA mta;

        public BFRES.MTA BFRES_MTA
        {
            get
            {
                return bfres_mta;
            }
            set
            {
                bfres_mta = value;
                Refresh();
            }
        }
        public BFRES.MTA bfres_mta;

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
                if (VBN != null)
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
        public BCH Bch
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
        private BCH bch;

        public BFRES Bfres
        {
            get
            {
                return bfres;
            }
            set
            {
                bfres = value;
                Refresh();
            }
        }
        private BFRES bfres;

        public KCL Kcl
        {
            get
            {
                return kcl;
            }
            set
            {
                kcl = value;
                Refresh();
            }
        }
        private KCL kcl;

        public DAT DatMelee
        {
            get
            {
                return datMelee;
            }
            set
            {
                datMelee = value;
                VBN = datMelee.bones;
                Refresh();
            }
        }
        private DAT datMelee;
        
        public MeleeDataNode MeleeData
        {
            get
            {
                return _meleeData;
            }
            set
            {
                _meleeData = value;
                //VBN = datMelee.bones;
                Refresh();
            }
        }
        private MeleeDataNode _meleeData;

        public static Dictionary<string, SkelAnimation> Animations { get; set; }
        public static MovesetManager Moveset { get; set; }

        public ModelContainer()
        {
            ImageKey = "folder";
            SelectedImageKey = "folder";
            nud = new NUD();
            nut = new NUT();
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

            if (MeleeData != null)
            {
                Text = "Melee Data";
                Nodes.Add(MeleeData);
            }
            else if(DatMelee != null)
            {
                Text = "Melee DAT";
                Nodes.AddRange(DatMelee.tree.ToArray());
                if (vbn != null && vbn.Parent == null) Nodes.Add(vbn);
            }
            else if (bch != null)
            {
                Nodes.Add(bch);
            }
            else if (bfres != null)
            {
                Nodes.Add(bfres);
            }
            else if (kcl != null)
            {
                Nodes.Add(kcl);
            }
            else
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

        public VBN GetVBN()
        {
            if (Bch != null && Bch.Models.Nodes.Count > 0)
            {
                ((BCH_Model)Bch.Models.Nodes[0]).skeleton.JointTable = JTB;
                return ((BCH_Model)Bch.Models.Nodes[0]).skeleton;
            }
            if (Bfres != null && Bfres.models.Count > 0)
            {
                Bfres.models[0].skeleton.JointTable = JTB;
                return Bfres.models[0].skeleton;
            }
            else if (vbn != null)
            {
                return vbn;
            }
            return null;
        }

        public void Render(Camera camera, DepthTexture depthMap, Matrix4 lightMatrix, Vector2 screenDimensions, bool drawShadow = false)
        {
            if (!Checked)
                return;

            Shader shader;

            // 3DS MBN
            shader = OpenTKSharedResources.shaders["Mbn"];
            shader.UseProgram();
            SetMbnUniforms(camera, shader);

            if (MeleeData != null)
            {
                MeleeData.Render(camera);
            }

            if (Bch != null)
            {
                foreach (BCH_Model mo in Bch.Models.Nodes)
                {
                    mo.Render(camera.MvpMatrix);
                }
            }

            if (DatMelee != null && OpenTKSharedResources.shaders["Dat"].LinkStatusIsOk)
            {
                DatMelee.Render(camera.MvpMatrix);
            }

            LightColor diffuseColor = Runtime.lightSetParam.characterDiffuse.diffuseColor;
            LightColor ambientColor = Runtime.lightSetParam.characterDiffuse.ambientColor;

            if (Kcl != null)
            {
                shader = OpenTKSharedResources.shaders["KCL"];
                if (!shader.LinkStatusIsOk)
                    return;

                shader.UseProgram();

                shader.SetVector3("difLightColor", diffuseColor.R, diffuseColor.G, diffuseColor.B);
                shader.SetVector3("ambLightColor", ambientColor.R, ambientColor.G, ambientColor.B);

                Kcl.Render(camera.MvpMatrix);
            }

            if (Bfres != null)
            {
                Bfres.Render(camera, Runtime.drawNudColorIdPass);
            }

            if (NUD != null && OpenTKSharedResources.shaders["Nud"].LinkStatusIsOk && OpenTKSharedResources.shaders["NudDebug"].LinkStatusIsOk)
            {
                // Choose the appropriate shader.
                if (drawShadow)
                    shader = OpenTKSharedResources.shaders["Shadow"];
                else if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                    shader = OpenTKSharedResources.shaders["NudDebug"];
                else
                    shader = OpenTKSharedResources.shaders["Nud"];

                shader.UseProgram();

                // Matrices.
                Matrix4 lightMatrixRef = lightMatrix;
                shader.SetMatrix4x4("lightMatrix", ref lightMatrixRef);
                SetCameraMatrixUniforms(camera, shader);

                SetRenderSettingsUniforms(shader);
                SetLightingUniforms(shader, camera);

                shader.SetInt("renderType", (int)Runtime.renderType);
                shader.SetInt("debugOption", (int)Runtime.uvChannel);
                shader.SetBoolToInt("drawShadow", Runtime.drawModelShadow);

                shader.SetTexture("depthMap", depthMap, 14);

                SetElapsedDirectUvTime(shader);

                NUD.Render(VBN, camera, drawShadow, Runtime.drawNudColorIdPass);
            }
        }

        private static void SetDatUniforms(Shader shader)
        {
            LightColor diffuseColor = Runtime.lightSetParam.characterDiffuse.diffuseColor;
            LightColor ambientColor = Runtime.lightSetParam.characterDiffuse.ambientColor;
            shader.SetVector3("difLightColor", diffuseColor.R, diffuseColor.G, diffuseColor.B);
            shader.SetVector3("ambLightColor", ambientColor.R, ambientColor.G, ambientColor.B);
        }

        private static void SetMbnUniforms(Camera camera, Shader shader)
        {
            if (Runtime.cameraLight)
            {
                shader.SetVector3("difLightDirection", Vector3.TransformNormal(new Vector3(0f, 0f, -1f), camera.MvpMatrix.Inverted()).Normalized());
            }
            else
            {
                shader.SetVector3("difLightDirection", Runtime.lightSetParam.characterDiffuse.direction);
            }
        }

        public static void SetCameraMatrixUniforms(Camera camera, Shader shader)
        {
            Matrix4 mvpMatrix = camera.MvpMatrix;
            shader.SetMatrix4x4("mvpMatrix", ref mvpMatrix);

            // Perform the calculations here to reduce render times in shader
            Matrix4 modelViewMatrix = camera.ModelViewMatrix;
            Matrix4 sphereMapMatrix = modelViewMatrix;
            sphereMapMatrix.Invert();
            sphereMapMatrix.Transpose();
            shader.SetMatrix4x4("modelViewMatrix", ref modelViewMatrix);
            shader.SetMatrix4x4("sphereMapMatrix", ref sphereMapMatrix);

            Matrix4 rotationMatrix = camera.RotationMatrix;
            shader.SetMatrix4x4("rotationMatrix", ref rotationMatrix);
        }

        private void SetElapsedDirectUvTime(Shader shader)
        {
            float elapsedSeconds = 0;
            if (NUD.useDirectUVTime)
            {
                elapsedSeconds = ModelViewport.directUvTimeStopWatch.ElapsedMilliseconds / 1000.0f;
                // Should be based on XMB eventualy.
                if (elapsedSeconds >= 100)
                    ModelViewport.directUvTimeStopWatch.Restart();
            }
            else
                ModelViewport.directUvTimeStopWatch.Stop();

            shader.SetFloat("elapsedTime", elapsedSeconds);
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

            if (Bch != null)
            {
                foreach (BCH_Model mo in Bch.Models.Nodes)
                    RenderTools.DrawVBN(mo.skeleton);
            }

            if (Bfres != null)
            {
                foreach (var mo in Bfres.models)
                {
                    RenderTools.DrawVBN(mo.skeleton);
                }
            }

            if (DatMelee != null)
            {
                RenderTools.DrawVBN(DatMelee.bones);
            }
        }

        public static void SetRenderSettingsUniforms(Shader shader)
        {
            shader.SetBoolToInt("renderStageLighting", Runtime.renderStageLighting);
            shader.SetBoolToInt("renderLighting", Runtime.renderMaterialLighting);
            shader.SetBoolToInt("renderVertColor", Runtime.renderVertColor);
            shader.SetBoolToInt("renderAlpha", Runtime.renderAlpha);
            shader.SetBoolToInt("renderDiffuse", Runtime.renderDiffuse);
            shader.SetBoolToInt("renderFresnel", Runtime.renderFresnel);
            shader.SetBoolToInt("renderSpecular", Runtime.renderSpecular);
            shader.SetBoolToInt("renderReflection", Runtime.renderReflection);

            shader.SetBoolToInt("useNormalMap", Runtime.renderNormalMap);

            shader.SetFloat("ambientIntensity", Runtime.ambItensity);
            shader.SetFloat("diffuseIntensity", Runtime.difIntensity);
            shader.SetFloat("specularIntensity", Runtime.spcIntentensity);
            shader.SetFloat("fresnelIntensity", Runtime.frsIntensity);
            shader.SetFloat("reflectionIntensity", Runtime.refIntensity);

            shader.SetFloat("zScale", Runtime.zScale);

            shader.SetBoolToInt("renderR", Runtime.renderR);
            shader.SetBoolToInt("renderG", Runtime.renderG);
            shader.SetBoolToInt("renderB", Runtime.renderB);
            shader.SetBoolToInt("renderAlpha", Runtime.renderAlpha);

            shader.SetInt("uvChannel", (int)Runtime.uvChannel);

            bool alphaOverride = Runtime.renderAlpha && !Runtime.renderR && !Runtime.renderG && !Runtime.renderB;
            shader.SetBoolToInt("alphaOverride", alphaOverride);

            shader.SetVector3("lightSetColor", 0, 0, 0);

            shader.SetInt("colorOverride", 0);

            shader.SetBoolToInt("debug1", Runtime.debug1);
            shader.SetBoolToInt("debug2", Runtime.debug2);

        }

        public static void SetLightingUniforms(Shader shader, Camera camera)
        {
            // fresnel sky/ground color for characters & stages
            ShaderTools.LightColorVector3Uniform(shader, Runtime.lightSetParam.fresnelLight.groundColor, "fresGroundColor");
            ShaderTools.LightColorVector3Uniform(shader, Runtime.lightSetParam.fresnelLight.skyColor, "fresSkyColor");
            shader.SetVector3("fresSkyDirection", Runtime.lightSetParam.fresnelLight.getSkyDirection());
            shader.SetVector3("fresGroundDirection", Runtime.lightSetParam.fresnelLight.getGroundDirection());

            // reflection color for characters & stages
            float refR, refG, refB = 1.0f;
            ColorUtils.HsvToRgb(Runtime.reflectionHue, Runtime.reflectionSaturation, Runtime.reflectionIntensity, out refR, out refG, out refB);
            shader.SetVector3("refLightColor", refR, refG, refB);

            // character diffuse lights
            shader.SetVector3("difLightColor", Runtime.lightSetParam.characterDiffuse.diffuseColor.R, Runtime.lightSetParam.characterDiffuse.diffuseColor.G, Runtime.lightSetParam.characterDiffuse.diffuseColor.B);
            shader.SetVector3("ambLightColor", Runtime.lightSetParam.characterDiffuse.ambientColor.R, Runtime.lightSetParam.characterDiffuse.ambientColor.G, Runtime.lightSetParam.characterDiffuse.ambientColor.B);

            shader.SetVector3("difLightColor2", Runtime.lightSetParam.characterDiffuse2.diffuseColor.R, Runtime.lightSetParam.characterDiffuse2.diffuseColor.G, Runtime.lightSetParam.characterDiffuse2.diffuseColor.B);
            shader.SetVector3("ambLightColor2", Runtime.lightSetParam.characterDiffuse2.ambientColor.R, Runtime.lightSetParam.characterDiffuse2.ambientColor.G, Runtime.lightSetParam.characterDiffuse2.ambientColor.B);

            shader.SetVector3("difLightColor3", Runtime.lightSetParam.characterDiffuse3.diffuseColor.R, Runtime.lightSetParam.characterDiffuse3.diffuseColor.G, Runtime.lightSetParam.characterDiffuse3.diffuseColor.B);
            shader.SetVector3("ambLightColor3", Runtime.lightSetParam.characterDiffuse3.ambientColor.R, Runtime.lightSetParam.characterDiffuse3.ambientColor.G, Runtime.lightSetParam.characterDiffuse3.ambientColor.B);

            // character specular light
            shader.SetVector3("specLightColor", LightTools.specularLight.diffuseColor.R, LightTools.specularLight.diffuseColor.G, LightTools.specularLight.diffuseColor.B);

            // stage fog
            shader.SetBoolToInt("renderFog", Runtime.renderFog);

            shader.SetVector3("difLight2Direction", Runtime.lightSetParam.characterDiffuse2.direction);
            shader.SetVector3("difLight3Direction", Runtime.lightSetParam.characterDiffuse2.direction);

            if (Runtime.cameraLight)
            {
                // Camera light should only affect character lighting.
                Matrix4 invertedCamera = camera.MvpMatrix.Inverted();
                Vector3 lightDirection = new Vector3(0f, 0f, -1f);
                shader.SetVector3("lightDirection", Vector3.TransformNormal(lightDirection, invertedCamera).Normalized());
                shader.SetVector3("specLightDirection", Vector3.TransformNormal(lightDirection, invertedCamera).Normalized());
                shader.SetVector3("difLightDirection", Vector3.TransformNormal(lightDirection, invertedCamera).Normalized());
            }
            else
            {
                shader.SetVector3("specLightDirection", LightTools.specularLight.direction);
                shader.SetVector3("difLightDirection", Runtime.lightSetParam.characterDiffuse.direction);
            }
        }

        public void DepthSortModels(Vector3 cameraPosition)
        {
            if (NUD != null)
                NUD.DepthSortMeshes(cameraPosition);
            if (Bfres != null)
                Bfres.DepthSortMeshes(cameraPosition);
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
