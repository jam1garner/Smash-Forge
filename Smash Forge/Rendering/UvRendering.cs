using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using SFGraphics.GLObjects.Textures;
using SFGraphics.GLObjects.Shaders;
using SFGraphics.GLObjects;

namespace Smash_Forge.Rendering
{
    static class UvRendering
    {
        // Used for UV drawing.
        private static BufferObject uvPositionVbo;
        private static BufferObject uvElementsIbo;
        private static int uvCount = 0;

        public static void DrawUv()
        {
            if (uvPositionVbo == null)
                return;

            Shader shader = Runtime.shaders["UV"];
            GL.UseProgram(shader.Id);
            shader.EnableVertexAttributes();
            uvPositionVbo.Bind();

            // Draw over everything
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // Scale to 0 to 1 UV space and flip vertically.
            Matrix4 matrix = Matrix4.CreateOrthographicOffCenter(0, 1, 1, 0, -1, 1);
            shader.SetMatrix4x4("mvpMatrix", ref matrix);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("position"), 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);

            // Draw the uvs.
            GL.LineWidth(1.5f);
            GL.Enable(EnableCap.LineSmooth);
            GL.DrawArrays(PrimitiveType.LineStrip, 0, uvCount);
            shader.DisableVertexAttributes();
        }

        public static void InitializeUVBufferData(List<NUD.Polygon> polygons)
        {
            // The previous object's data will be cleaned up by GLOBjectManager.
            uvPositionVbo = new BufferObject(BufferTarget.ArrayBuffer);
            List<Vector2> uvs = GenerateUVList(polygons);
            InitializeUVBufferData(uvPositionVbo, uvs);
            uvCount = uvs.Count;
        }

        private static void InitializeUVBufferData(BufferObject uvPositionVBO, List<Vector2> uvs)
        {
            // Set up the buffer data.
            uvPositionVBO.Bind();
            Vector2[] uvArray = uvs.ToArray();
            GL.BufferData(uvPositionVBO.BufferTarget, (IntPtr)(sizeof(float) * 2 * uvArray.Length), uvArray, BufferUsageHint.StaticDraw);
        }

        private static List<Vector2> GenerateUVList(List<NUD.Polygon> polygons)
        {
            List<Vector2> uvs = new List<Vector2>();
            int uvIndex = 0;


            foreach (NUD.Polygon p in polygons)
            {
                foreach (int vertIndex in p.GetRenderingVertexIndices())
                {
                    // TODO: Not sure why some of the indices are incorrect.
                    if (p.vertices.Count > vertIndex)
                        uvs.Add(p.vertices[vertIndex].uv[uvIndex]);
                }
            }

            return uvs;
        }

    }
}
