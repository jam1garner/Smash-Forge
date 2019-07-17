using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SALT.PARAMS;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using SFGraphics.GLObjects.Textures;
using SFGraphics.Cameras;
using SmashForge.Filetypes.Models.Nuds;

namespace SmashForge.Rendering
{
    static class RenderTools
    {
        public static Texture2D defaultTex;
        public static Texture2D floorTexture;
        public static Texture2D backgroundTexture;

        public static Dictionary<NudEnums.DummyTexture, Texture> dummyTextures = new Dictionary<NudEnums.DummyTexture, Texture>();

        public static Texture2D uvTestPattern;
        public static Texture2D boneWeightGradient;
        public static Texture2D boneWeightGradient2;

        public static TextureCubeMap diffusePbr;
        public static TextureCubeMap specularPbr;

        public static void LoadTextures()
        {
            dummyTextures = CreateNudDummyTextures();

            NudMatSphereDrawing.LoadMaterialSphereTextures();

            // Helpful textures. 
            uvTestPattern = new Texture2D();
            uvTestPattern.LoadImageData(Properties.Resources.UVPattern);
            uvTestPattern.TextureWrapS = TextureWrapMode.Repeat;
            uvTestPattern.TextureWrapT = TextureWrapMode.Repeat;

            // TODO: Simplify this conversion.
            Dds specularSdr = new Dds(new FileData(Properties.Resources.specularSDR));      
            specularPbr = NUT.CreateTextureCubeMap(specularSdr.ToNutTexture());

            Dds diffuseSdr = new Dds(new FileData(Properties.Resources.diffuseSDR));
            diffusePbr = NUT.CreateTextureCubeMap(diffuseSdr.ToNutTexture());
            // Don't use mipmaps.
            diffusePbr.MinFilter = TextureMinFilter.Linear;
            diffusePbr.MagFilter = TextureMagFilter.Linear;

            boneWeightGradient = new Texture2D();
            boneWeightGradient.LoadImageData(Properties.Resources.boneWeightGradient);

            boneWeightGradient2 = new Texture2D();
            boneWeightGradient2.LoadImageData(Properties.Resources.boneWeightGradient2);

            defaultTex = new Texture2D();
            defaultTex.LoadImageData(Resources.Resources.DefaultTexture);

            try
            {
                floorTexture = new Texture2D();
                floorTexture.LoadImageData(new Bitmap(Runtime.floorTexFilePath));

                backgroundTexture = new Texture2D();
                backgroundTexture.LoadImageData(new Bitmap(Runtime.backgroundTexFilePath));
            }
            catch (Exception)
            {
                // File paths are incorrect or never set. 
            }
        }

        public static Dictionary<NudEnums.DummyTexture, Texture> CreateNudDummyTextures()
        {
            Dictionary<NudEnums.DummyTexture, Texture> dummyTextures = new Dictionary<NudEnums.DummyTexture, Texture>();

            // Dummy textures. 
            TextureCubeMap stageMapHigh = new TextureCubeMap();
            stageMapHigh.LoadImageData(Properties.Resources._10102000, 128);
            dummyTextures.Add(NudEnums.DummyTexture.StageMapHigh, stageMapHigh);

            TextureCubeMap stageMapLow = new TextureCubeMap();
            stageMapLow.LoadImageData(Properties.Resources._10101000, 128);
            dummyTextures.Add(NudEnums.DummyTexture.StageMapLow, stageMapLow);

            Texture2D dummyRamp = new Texture2D();
            dummyRamp.LoadImageData(Properties.Resources._10080000);
            dummyTextures.Add(NudEnums.DummyTexture.DummyRamp, dummyRamp);

            Texture2D pokemonStadiumDummyTex = new Texture2D();
            pokemonStadiumDummyTex.LoadImageData(Properties.Resources._10040001);
            dummyTextures.Add(NudEnums.DummyTexture.PokemonStadium, pokemonStadiumDummyTex);

            Texture2D punchOutDummyTex = new Texture2D();
            punchOutDummyTex.LoadImageData(Properties.Resources._10040000);
            dummyTextures.Add(NudEnums.DummyTexture.PunchOut, punchOutDummyTex);

            Texture2D shadowMapDummyTex = new Texture2D();
            shadowMapDummyTex.LoadImageData(Properties.Resources._10100000);
            dummyTextures.Add(NudEnums.DummyTexture.ShadowMap, shadowMapDummyTex);

            return dummyTextures;
        }

        public static void SetUp3DFixedFunctionRendering(Matrix4 mvpMatrix)
        {
            GL.UseProgram(0);

            // Manually set up the matrix for immediate mode.
            Matrix4 matrix = mvpMatrix;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref matrix);

            GL.Enable(EnableCap.LineSmooth); // This is Optional 
            GL.Enable(EnableCap.Normalize);  // This is critical to have
            GL.Enable(EnableCap.RescaleNormal);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BlendEquation(BlendEquationMode.FuncAdd);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Gequal, 0.1f);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

            GL.Enable(EnableCap.LineSmooth);

            GL.Enable(EnableCap.StencilTest);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
        }

        public static void DrawVBN(VBN vbn)
        {
            // Used for NUD, BFRES, BCH.

            float ToRad = (float)Math.PI / 180;
            int swinganim = 0;

            swinganim++;
            if (swinganim > 100) swinganim = 0;
            if (vbn != null)
            {
                Bone selectedBone = null;
                foreach (Bone bone in vbn.bones)
                {
                    if (!bone.IsSelected)
                        bone.Draw();
                    else
                        selectedBone = bone;

                    if (vbn.SwingBones != null && (Runtime.renderSwagY || Runtime.renderSwagZ))
                    {
                        SB.SBEntry sb = null;
                        vbn.SwingBones.TryGetEntry(bone.boneId, out sb);
                        if (sb != null)
                        {
                            float sf = Math.Abs(((swinganim - 50) / 50f));
                            float sz = (sb.rz1 + (sb.rz2 - sb.rz1) * sf) * ToRad;
                            float sy = (sb.ry1 + (sb.ry2 - sb.ry1) * sf) * ToRad;
                            if (!Runtime.renderSwagY)
                                sy = 0;
                            if (!Runtime.renderSwagZ)
                                sz = 0;
                            bone.rot = VBN.FromEulerAngles(bone.rotation[2], bone.rotation[1], bone.rotation[0]) *
                                VBN.FromEulerAngles(sz, sy, 0);
                        }

                    }
                }

                if (selectedBone != null)
                {
                    GL.Clear(ClearBufferMask.DepthBufferBit);
                    selectedBone.Draw();
                }

                if (vbn.SwingBones != null && (Runtime.renderSwagY || Runtime.renderSwagZ))
                    vbn.update();
            }
        }

        public static byte[] DXT5ScreenShot(GLControl gc, int x, int y, int width, int height)
        {
            int newtex;
            //x = gc.Width - x - width;
            y = gc.Height - y - height;
            GL.GenTextures(1, out newtex);
            GL.BindTexture(TextureTarget.Texture2D, newtex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.CompressedRgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            GL.CopyTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.CompressedRgba, x, y, width, height, 0);

            int size;
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureCompressedImageSize, out size);

            byte[] data = new byte[size];
            GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            GL.GetCompressedTexImage(TextureTarget.Texture2D, 0, pointer);
            pinnedArray.Free();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.DeleteTexture(newtex);

            return data;
        }

        public static void SetCameraValuesFromParam(Camera camera, ParamFile stprm)
        {
            if (stprm == null)
                return;

            camera.FovDegrees = (float)Params.ParamTools.GetParamValue(stprm, 0, 0, 6);
            camera.FarClipPlane = (float)Params.ParamTools.GetParamValue(stprm, 0, 0, 77);
        }

        public static void DrawPhotoshoot(GLControl glControl1, float shootX, float shootY, float shootWidth, float shootHeight)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, glControl1.Width, glControl1.Height, 0, -1, 1);

            GL.Disable(EnableCap.DepthTest);

            GL.Color4(1f, 1f, 1f, 0.5f);
            GL.Begin(PrimitiveType.Quads);

            // top
            GL.Vertex2(0, 0);
            GL.Vertex2(glControl1.Width, 0);
            GL.Vertex2(glControl1.Width, shootY);
            GL.Vertex2(0, shootY);

            //bottom
            GL.Vertex2(0, shootY + shootHeight);
            GL.Vertex2(glControl1.Width, shootY + shootHeight);
            GL.Vertex2(glControl1.Width, glControl1.Height);
            GL.Vertex2(0, glControl1.Height);

            // left
            GL.Vertex2(0, 0);
            GL.Vertex2(shootX, 0);
            GL.Vertex2(shootX, glControl1.Height);
            GL.Vertex2(0, glControl1.Height);

            // right
            GL.Vertex2(shootX + shootWidth, 0);
            GL.Vertex2(glControl1.Width, 0);
            GL.Vertex2(glControl1.Width, glControl1.Height);
            GL.Vertex2(shootX + shootWidth, glControl1.Height);

            GL.End();
        }

        public static Ray CreateRay(Matrix4 v, Vector2 m)
        {
            Vector4 va = Vector4.Transform(new Vector4(m.X, m.Y, -1.0f, 1.0f), v.Inverted());
            Vector4 vb = Vector4.Transform(new Vector4(m.X, m.Y, 1.0f, 1.0f), v.Inverted());

            Vector3 p1 = va.Xyz;
            Vector3 p2 = p1 - (va - (va + vb)).Xyz * 100;
            Ray r = new Ray(p1, p2);

            return r;
        }
    }
}
