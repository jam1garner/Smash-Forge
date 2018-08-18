using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects.Shaders;
using Smash_Forge.Filetypes.Models.Nuds;
using Smash_Forge.Rendering.Meshes;
using SFGenericModel.Materials;

namespace Smash_Forge.Rendering
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

        private static ForgeCamera nudSphereCamera = new ForgeCamera();

        public static void DrawNudMaterialSphere(Shader shader, NUD.Material material, Mesh3D screenTriangle, Dictionary<NUD.DummyTextures, Texture> dummyTextures)
        {
            if (!shader.ProgramCreatedSuccessfully)
                return;

            shader.UseProgram();

            // Use the same uniforms as the NUD shader. 
            GenericMaterial genericMaterial = new GenericMaterial();
            NudUniforms.SetMaterialPropertyUniforms(genericMaterial, material);
            genericMaterial.SetShaderUniforms(shader);

            NUD.SetStageLightingUniforms(shader, 0);
            ModelContainer.SetRenderSettingsUniforms(shader);

            nudSphereCamera.UpdateMatrices();
            ModelContainer.SetLightingUniforms(shader, nudSphereCamera);
            ModelContainer.SetCameraMatrixUniforms(nudSphereCamera, shader);

            // Use default textures rather than textures from the NUT.
            NudUniforms.SetTextureUniformsNudMatSphere(shader, material, dummyTextures);

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
