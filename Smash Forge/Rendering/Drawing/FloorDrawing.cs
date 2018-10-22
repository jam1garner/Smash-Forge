using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Smash_Forge.Rendering
{
    public static class FloorDrawing
    {
        public static void DrawFloor(Matrix4 mvpMatrix)
        {
            float scale = Runtime.floorSize;

            GL.UseProgram(0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref mvpMatrix);

            // objects shouldn't show through opaque parts of floor
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Color3(Runtime.floorColor);
            GL.LineWidth(1f);

            if (Runtime.floorStyle == Runtime.FloorStyle.UserTexture)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.ActiveTexture(TextureUnit.Texture0);

                if (RenderTools.floorTexture != null)
                    RenderTools.floorTexture.Bind();
                else
                    RenderTools.defaultTex.Bind();

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)Runtime.floorWrap);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)Runtime.floorWrap);

                GL.Color3(Runtime.floorColor == Color.Gray ? Color.White : Runtime.floorColor);

                GL.Begin(PrimitiveType.Quads);
                GL.TexCoord2(0, 0);
                GL.Vertex3(new Vector3(-scale, 0f, -scale));

                GL.TexCoord2(0, 2);
                GL.Vertex3(new Vector3(-scale, 0f, scale));

                GL.TexCoord2(2, 2);
                GL.Vertex3(new Vector3(scale, 0f, scale));

                GL.TexCoord2(2, 0);
                GL.Vertex3(new Vector3(scale, 0f, -scale));
                GL.End();

                GL.Disable(EnableCap.Texture2D);
            }
            else if (Runtime.floorStyle == Runtime.FloorStyle.Solid)
            {
                GL.Begin(PrimitiveType.Quads);
                GL.Vertex3(-scale, 0f, -scale);
                GL.Vertex3(-scale, 0f, scale);
                GL.Vertex3(scale, 0f, scale);
                GL.Vertex3(scale, 0f, -scale);
                GL.End();
            }
            else
            {
                GL.Begin(PrimitiveType.Lines);
                for (var i = -scale / 2; i <= scale / 2; i++)
                {
                    if (i != 0)
                    {
                        GL.Vertex3(-scale, 0f, i * 2);
                        GL.Vertex3(scale, 0f, i * 2);
                        GL.Vertex3(i * 2, 0f, -scale);
                        GL.Vertex3(i * 2, 0f, scale);
                    }
                }
                GL.End();
            }

            if (Runtime.renderFloorLines)
            {
                GL.Disable(EnableCap.DepthTest);
                GL.Color3(Color.White);

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(-scale, 0f, 0);
                GL.Vertex3(scale, 0f, 0);
                GL.Vertex3(0, 0f, -scale);
                GL.Vertex3(0, 0f, scale);
                GL.End();

                GL.Enable(EnableCap.DepthTest);

                GL.Disable(EnableCap.DepthTest);
                GL.Color3(Color.LightGray);

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 5, 0);
                GL.Vertex3(0, 0, 0);
                GL.End();

                GL.Color3(Color.OrangeRed);

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0f, 0f, 0);
                GL.Vertex3(5f, 0f, 0);
                GL.End();

                GL.Color3(Color.Olive);

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0f, 0f);
                GL.Vertex3(0, 0f, 5f);
                GL.End();
            }

            GL.Enable(EnableCap.DepthTest);
        }
    }
}
