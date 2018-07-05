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
    static class NudUvRendering
    {
        // Used for UV drawing.
        private static BufferObject uvPositionVbo;
        private static BufferObject uvElementsIbo;

        public static void DrawUv(NUD nud)
        {
            if (uvPositionVbo == null)
                return;

            foreach (NUD.Mesh mesh in nud.Nodes)
            {
                foreach (NUD.Polygon poly in mesh.Nodes)
                {
                    DrawPolygonUv(poly);
                }
            }
        }

        private static void DrawPolygonUv(NUD.Polygon p)
        {
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

            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vPosition"), 3, VertexAttribPointerType.Float, false, NUD.DisplayVertex.Size, 0);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vNormal"), 3, VertexAttribPointerType.Float, false, NUD.DisplayVertex.Size, 12);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vTangent"), 3, VertexAttribPointerType.Float, false, NUD.DisplayVertex.Size, 24);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vBiTangent"), 3, VertexAttribPointerType.Float, false, NUD.DisplayVertex.Size, 36);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vUV"), 2, VertexAttribPointerType.Float, false, NUD.DisplayVertex.Size, 48);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vColor"), 4, VertexAttribPointerType.Float, false, NUD.DisplayVertex.Size, 56);
            GL.VertexAttribIPointer(shader.GetVertexAttributeUniformLocation("vBone"), 4, VertexAttribIntegerType.Int, NUD.DisplayVertex.Size, new IntPtr(72));
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vWeight"), 4, VertexAttribPointerType.Float, false, NUD.DisplayVertex.Size, 88);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vUV2"), 2, VertexAttribPointerType.Float, false, NUD.DisplayVertex.Size, 104);
            GL.VertexAttribPointer(shader.GetVertexAttributeUniformLocation("vUV3"), 2, VertexAttribPointerType.Float, false, NUD.DisplayVertex.Size, 112);

            // Draw the uvs.
            GL.LineWidth(1.5f);
            GL.Enable(EnableCap.LineSmooth);

            uvElementsIbo.Bind();
            GL.DrawElements(PrimitiveType.Triangles, p.displayFaceSize, DrawElementsType.UnsignedInt, p.Offset);

            shader.DisableVertexAttributes();
        }

        public static void InitializeUVBufferData(NUD nud)
        {
            // The previous object's data will be cleaned up by GLOBjectManager.
            uvPositionVbo = new BufferObject(BufferTarget.ArrayBuffer);
            uvElementsIbo = new BufferObject(BufferTarget.ElementArrayBuffer);

            nud.UpdateVertexBuffers(uvPositionVbo, uvElementsIbo);
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
