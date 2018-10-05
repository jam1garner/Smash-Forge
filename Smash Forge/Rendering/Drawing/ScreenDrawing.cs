using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects.Textures;
using Smash_Forge.Rendering.Meshes;
using System.Collections.Generic;
using System.Drawing;

namespace Smash_Forge.Rendering
{
    static class ScreenDrawing
    {
        // A triangle that extends past the screen.
        // Avoids the need for a second triangle to fill a rectangular screen.
        private static List<Vector3> screenTriangleVertices = new List<Vector3>()
        {
            new Vector3(-1f, -1f, 0.0f),
            new Vector3( 3f, -1f, 0.0f),
            new Vector3(-1f,  3f, 0.0f)
        };

        public static Mesh3D CreateScreenTriangle()
        {
            Mesh3D screenTriangle = new Mesh3D(screenTriangleVertices);
            return screenTriangle;
        }

        public static void DrawTexturedQuad(Texture texture, int width, int height, Mesh3D screenTriangle,
            bool renderR = true, bool renderG = true, bool renderB = true, bool renderA = false, 
            bool keepAspectRatio = false, float intensity = 1, int currentMipLevel = 0)
        {
            // Draws RGB and alpha channels of texture to screen quad.
            Shader shader = OpenTKSharedResources.shaders["Texture"];
            shader.UseProgram();

            EnableAlphaBlendingWhiteBackground();

            // Single texture uniform.
            shader.SetTexture("image", texture, 0);

            // Channel toggle uniforms. 
            shader.SetBoolToInt("renderR", renderR);
            shader.SetBoolToInt("renderG", renderG);
            shader.SetBoolToInt("renderB", renderB);
            shader.SetBoolToInt("renderAlpha", renderA);

            shader.SetFloat("intensity", intensity);

            bool alphaOverride = renderA && !renderR && !renderG && !renderB;
            shader.SetBoolToInt("alphaOverride", alphaOverride);

            // Perform aspect ratio calculations in shader. 
            // This only displays correctly if the viewport is square.
            shader.SetBoolToInt("preserveAspectRatio", keepAspectRatio);
            shader.SetFloat("width", width);
            shader.SetFloat("height", height);

            // Display certain mip levels.
            shader.SetInt("currentMipLevel", currentMipLevel);

            // Draw full screen "quad" (big triangle)
            DrawScreenTriangle(shader, screenTriangle);
        }

        public static void DrawTexturedQuad(Texture texture, float intensity, Mesh3D screenTriangle)
        {
            DrawTexturedQuad(texture, 1, 1, screenTriangle, true, true, true, true, false, intensity, 0);
        }

        public static void DrawScreenQuadPostProcessing(Texture texture0, Texture texture1, Mesh3D screenTriangle)
        {
            // Draws RGB and alpha channels of texture to screen quad.
            Shader shader = OpenTKSharedResources.shaders["ScreenQuad"];
            shader.UseProgram();

            shader.SetTexture("image0", texture0, 0);
            shader.SetTexture("image1", texture1, 1);

            shader.SetBoolToInt("renderBloom", Runtime.renderBloom);
            shader.SetFloat("bloomIntensity", Runtime.bloomIntensity);

            ShaderTools.SystemColorVector3Uniform(shader, Runtime.backgroundGradientBottom, "backgroundBottomColor");
            ShaderTools.SystemColorVector3Uniform(shader, Runtime.backgroundGradientTop, "backgroundTopColor");

            // Draw full screen "quad" (big triangle)
            DrawScreenTriangle(shader, screenTriangle);
        }

        public static void DrawQuadGradient(Vector3 topColor, Vector3 bottomColor, Mesh3D screenTriangle)
        {
            // draw RGB and alpha channels of texture to screen quad
            Shader shader = OpenTKSharedResources.shaders["Gradient"];
            shader.UseProgram();

            EnableAlphaBlendingWhiteBackground();

            shader.SetVector3("topColor", topColor);
            shader.SetVector3("bottomColor", bottomColor);

            DrawScreenTriangle(shader, screenTriangle);
        }

        public static void EnableAlphaBlendingWhiteBackground()
        {
            // Set up OpenGL settings for basic 2D rendering.
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Allow for alpha blending.
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public static void DrawScreenTriangle(Shader shader, Mesh3D screenTriangle)
        {
            screenTriangle.Draw(shader, null);
        }
    }
}
