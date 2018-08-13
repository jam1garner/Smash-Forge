using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;

namespace Smash_Forge.Rendering
{
    static class ScreenDrawing
    {
        // A triangle that extends past the screen.
        // Avoids the need for a second triangle to fill a rectangular screen.
        public static BufferObject screenTriangleVbo;
        public static VertexArrayObject screenTriangleVao;

        private static float[] screenTriangleVertices =
        {
            -1f, -1f, 0.0f,
             3f, -1f, 0.0f,
            -1f,  3f, 0.0f
        };

        public static BufferObject CreateScreenTriangleBuffer()
        {
            // Create buffer for vertex positions. The data won't change, so only initialize once.
            BufferObject screenBuffer = new BufferObject(BufferTarget.ArrayBuffer);
            screenBuffer.BufferData(screenTriangleVertices, sizeof(float), BufferUsageHint.StaticDraw);
            return screenBuffer;
        }

        public static void DrawTexturedQuad(int texture, int width, int height, 
            bool renderR = true, bool renderG = true, bool renderB = true, bool renderA = false, 
            bool keepAspectRatio = false, float intensity = 1, int currentMipLevel = 0)
        {
            // Draws RGB and alpha channels of texture to screen quad.
            Shader shader = OpenTKSharedResources.shaders["Texture"];
            shader.UseProgram();

            EnableAlphaBlendingWhiteBackground();

            // Single texture uniform.
            shader.SetTexture("image", texture, TextureTarget.Texture2D, 0);

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
            DrawScreenTriangle(shader, screenTriangleVao);
        }

        public static void DrawTexturedQuad(int texture, float intensity)
        {
            DrawTexturedQuad(texture, 1, 1, true, true, true, true, false, intensity, 0);
        }

        public static void DrawScreenQuadPostProcessing(int texture0, int texture1)
        {
            // Draws RGB and alpha channels of texture to screen quad.
            Shader shader = OpenTKSharedResources.shaders["ScreenQuad"];
            shader.UseProgram();

            shader.SetTexture("image0", texture0, TextureTarget.Texture2D, 0);
            shader.SetTexture("image1", texture1, TextureTarget.Texture2D, 1);

            shader.SetBoolToInt("renderBloom", Runtime.renderBloom);
            shader.SetFloat("bloomIntensity", Runtime.bloomIntensity);

            ShaderTools.SystemColorVector3Uniform(shader, Runtime.backgroundGradientBottom, "backgroundBottomColor");
            ShaderTools.SystemColorVector3Uniform(shader, Runtime.backgroundGradientTop, "backgroundTopColor");

            // Draw full screen "quad" (big triangle)
            DrawScreenTriangle(shader, screenTriangleVao);
        }

        public static void DrawQuadGradient(Vector3 topColor, Vector3 bottomColor, BufferObject screenVbo)
        {
            // draw RGB and alpha channels of texture to screen quad
            Shader shader = OpenTKSharedResources.shaders["Gradient"];
            shader.UseProgram();

            EnableAlphaBlendingWhiteBackground();

            shader.SetVector3("topColor", topColor);
            shader.SetVector3("bottomColor", bottomColor);

            DrawScreenTriangle(shader, screenTriangleVao);
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

        public static void DrawScreenTriangle(Shader shader, VertexArrayObject vao)
        {
            // TODO: Use an existing vao to avoid memory issues and improve performance.
            VertexArrayObject test = new VertexArrayObject();
            test.Bind();

            // Set vao state.
            screenTriangleVbo.Bind();
            shader.EnableVertexAttributes();
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("position"), 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            test.Unbind();
        }
    }
}
