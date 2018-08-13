using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;

namespace Smash_Forge.Rendering
{
    static class NudMatSphereDrawing
    {
        // Nud Material Sphere Textures.
        public static Texture sphereDifTex;
        public static Texture sphereNrmMapTex;

        // Nud Material Sphere Vert Attribute Textures.
        private static Texture sphereNrmTex;
        private static Texture sphereUvTex;
        private static Texture sphereTanTex;
        private static Texture sphereBitanTex;

        private static ForgeCamera nudSphereCamera = new ForgeCamera();

        public static void DrawNudMaterialSphere(Shader shader, NUD.Material material, VertexArrayObject screenVao, Dictionary<NUD.DummyTextures, Texture> dummyTextures)
        {
            if (!shader.ProgramCreatedSuccessfully)
                return;

            shader.UseProgram();


            // Use the same uniforms as the NUD shader. 
            NUD.SetMaterialPropertyUniforms(shader, material);
            NUD.SetStageLightingUniforms(shader, 0);
            ModelContainer.SetRenderSettingsUniforms(shader);

            nudSphereCamera.UpdateMatrices();
            ModelContainer.SetLightingUniforms(shader, nudSphereCamera);
            ModelContainer.SetCameraMatrixUniforms(nudSphereCamera, shader);

            // Use default textures rather than textures from the NUT.
            NUD.SetTextureUniformsNudMatSphere(shader, material, dummyTextures);

            // These values aren't needed in the shader currently.
            shader.SetVector3("cameraPosition", 0, 0, 0);
            shader.SetFloat("zBufferOffset", 0);
            shader.SetFloat("bloomThreshold", Runtime.bloomThreshold);

            bool isTransparent = (material.srcFactor > 0) || (material.dstFactor > 0) || (material.alphaFunction > 0) || (material.alphaTest > 0);
            shader.SetBoolToInt("isTransparent", isTransparent);

            // Set texture uniforms for the mesh attributes. 
            shader.SetTexture("normalTex", sphereNrmTex.Id, TextureTarget.Texture2D, 15);
            shader.SetTexture("uvTex", sphereUvTex.Id, TextureTarget.Texture2D, 16);
            shader.SetTexture("tanTex", sphereTanTex.Id, TextureTarget.Texture2D, 17);
            shader.SetTexture("bitanTex", sphereBitanTex.Id, TextureTarget.Texture2D, 18);

            // Draw full screen "quad" (big triangle)
            ScreenDrawing.DrawScreenTriangle(shader, screenVao);
        }

        public static void LoadMaterialSphereTextures()
        {
            // Sphere Default Textures.
            sphereDifTex = new Texture2D(Properties.Resources.defaultDif);
            sphereNrmMapTex = new Texture2D(Properties.Resources.nrmMap);
            // Sphere Mesh Attributes.
            sphereNrmTex = new Texture2D(Properties.Resources.nrm);
            sphereUvTex = new Texture2D(Properties.Resources.uv);
            sphereTanTex = new Texture2D(Properties.Resources.tan);
            sphereBitanTex = new Texture2D(Properties.Resources.bitan);
        }
    }
}
