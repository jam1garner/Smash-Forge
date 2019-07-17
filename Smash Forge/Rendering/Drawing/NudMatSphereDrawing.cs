using System.Collections.Generic;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects.Shaders;
using SFGenericModel.Materials;
using SmashForge.Filetypes.Models.Nuds;
using SmashForge.Rendering.Meshes;

namespace SmashForge.Rendering
{
    static class NudMatSphereDrawing
    {
        // Nud Material Sphere Textures.
        public static Texture2D sphereDifTex;
        public static Texture2D sphereNrmMapTex;

        // Nud Material Sphere Vert Attribute Textures.
        private static Texture2D sphereNrmTex;
        private static Texture2D sphereUvTex;
        private static Texture2D sphereTanTex;
        private static Texture2D sphereBitanTex;

        private static ForgeCamera nudSphereCamera = new ForgePerspCamera();

        public static void DrawNudMaterialSphere(Shader shader, Nud.Material material, Mesh3D screenTriangle, Dictionary<NudEnums.DummyTexture, Texture> dummyTextures)
        {
            if (!shader.LinkStatusIsOk)
                return;

            shader.UseProgram();

            // Use the same uniforms as the NUD shader. 
            var uniformBlock = new UniformBlock(shader, "MaterialProperties") {BlockBinding = 1};
            NudUniforms.SetMaterialPropertyUniforms(uniformBlock, shader, material);

            Nud.SetStageLightingUniforms(shader, 0);
            ModelContainer.SetRenderSettingsUniforms(shader);

            ModelContainer.SetLightingUniforms(shader, nudSphereCamera);
            ModelContainer.SetCameraMatrixUniforms(nudSphereCamera, shader);

            // Use default textures rather than textures from the NUT.
            NudUniforms.SetTextureUniformsNudMatSphere(shader, material, dummyTextures);

            // These values aren't needed in the shader currently.
            shader.SetVector3("cameraPosition", 0, 0, 0);
            shader.SetFloat("zBufferOffset", 0);
            shader.SetFloat("bloomThreshold", Runtime.bloomThreshold);

            bool isTransparent = (material.SrcFactor > 0) || (material.DstFactor > 0) || (material.AlphaFunction > 0) || (material.AlphaTest > 0);
            shader.SetBoolToInt("isTransparent", isTransparent);

            // Set texture uniforms for the mesh attributes. 
            shader.SetTexture("normalTex", sphereNrmTex, 15);
            shader.SetTexture("uvTex", sphereUvTex, 16);
            shader.SetTexture("tanTex", sphereTanTex, 17);
            shader.SetTexture("bitanTex", sphereBitanTex, 18);

            // Draw full screen "quad" (big triangle)
            ScreenDrawing.DrawScreenTriangle(shader, screenTriangle);
        }

        public static void LoadMaterialSphereTextures()
        {
            // Sphere Default Textures.
            sphereDifTex = new Texture2D();
            sphereDifTex.LoadImageData(Properties.Resources.defaultDif);

            sphereNrmMapTex = new Texture2D();
            sphereNrmMapTex.LoadImageData(Properties.Resources.nrmMap);

            // Sphere Mesh Attributes.
            sphereNrmTex = new Texture2D();
            sphereNrmTex.LoadImageData(Properties.Resources.nrm);

            sphereUvTex = new Texture2D();
            sphereUvTex.LoadImageData(Properties.Resources.uv);

            sphereTanTex = new Texture2D();
            sphereTanTex.LoadImageData(Properties.Resources.tan);

            sphereBitanTex = new Texture2D();
            sphereBitanTex.LoadImageData(Properties.Resources.bitan);
        }
    }
}
