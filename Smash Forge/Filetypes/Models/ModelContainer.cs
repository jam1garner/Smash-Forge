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
                Refresh();
            }
        }
        private VBN vbn;
        public MTA mta;
        public MOI moi;
        public XMBFile xmb;

        public BCH bch;

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
            Refresh();
        }

        public void Refresh()
        {
            Nodes.Clear();
            if (nud != null) Nodes.Add(nud);
            if (nut != null) Nodes.Add(nut);
            if (vbn != null) Nodes.Add(vbn);
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

        public void Render(Camera camera, int depthmap, Matrix4 lightMatrix, Matrix4 modelMatrix)
        {
            Shader shader;
            if (Runtime.renderType != Runtime.RenderTypes.Shaded)
                shader = Runtime.shaders["NUD_Debug"];
            else
                shader = Runtime.shaders["nud"];
            GL.UseProgram(shader.programID);

            int renderType = (int)Runtime.renderType;
            
            Matrix4 mvpMatrix = camera.getMVPMatrix();
            GL.UniformMatrix4(shader.getAttribute("mvpMatrix"), false, ref mvpMatrix);

            Matrix4 modelView = camera.getModelViewMatrix();
            GL.UniformMatrix4(shader.getAttribute("modelViewMatrix"), false, ref modelView);

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
                if (bch != null)
                {
                    foreach (BCH_Model mo in bch.Models.Nodes)
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

                    NUD.Render(VBN, camera);                    
                }
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

            if (bch != null)
            {
                foreach (BCH_Model mo in bch.Models.Nodes)
                    RenderTools.DrawVBN(mo.skeleton);
            }

            if (dat_melee != null)
            {
                RenderTools.DrawVBN(dat_melee.bones);
            }
        }
    }
}

