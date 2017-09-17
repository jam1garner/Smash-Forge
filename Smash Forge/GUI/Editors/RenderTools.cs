using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace Smash_Forge
{

    public class Camera
    {
        Vector3 pos = new Vector3(0, -10, -30);
        float rot = 0, lookup = 0;

        public float mouseSLast, mouseYLast, mouseXLast;

        public Camera()
        {
            mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
            mouseXLast = OpenTK.Input.Mouse.GetState().X;
            mouseYLast = OpenTK.Input.Mouse.GetState().Y;
        }

        public Matrix4 getViewMatrix()
        {
            return Matrix4.CreateRotationY(0.5f * rot) *
                Matrix4.CreateRotationX(0.2f * lookup) *
                Matrix4.CreateTranslation(pos);
        }

        public void Update()
        {
            float zoomscale = Runtime.zoomspeed;

            if ((OpenTK.Input.Mouse.GetState().RightButton == OpenTK.Input.ButtonState.Pressed))
            {
                pos.Y += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
                pos.X += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
            }
            if ((OpenTK.Input.Mouse.GetState().LeftButton == OpenTK.Input.ButtonState.Pressed))
            {
                rot += 0.025f * (OpenTK.Input.Mouse.GetState().X - mouseXLast);
                lookup += 0.025f * (OpenTK.Input.Mouse.GetState().Y - mouseYLast);
            }

            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftLeft) || OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.ShiftRight))
                zoomscale = 6;

            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Down))
                pos.Z -= 1 * zoomscale;
            if (OpenTK.Input.Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Up))
                pos.Z += 1 * zoomscale;

            pos.Z += (OpenTK.Input.Mouse.GetState().WheelPrecise - mouseSLast) * zoomscale;
            
        }

        public void TrackMouse()
        {
            mouseXLast = OpenTK.Input.Mouse.GetState().X;
            mouseYLast = OpenTK.Input.Mouse.GetState().Y;
            mouseSLast = OpenTK.Input.Mouse.GetState().WheelPrecise;
        }
    }


    public class Ray
    {
        public Vector3 p1, p2;

        public Ray(Vector3 p1, Vector3 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public bool TrySphereHit(Vector3 sphere, float rad, out Vector3 closest)
        {
            return RenderTools.CheckSphereHit(sphere, rad, p1, p2,  out closest);
        }
    }

    public class DirectionalLight
    {
        public float R = 1.0f;
        public float G = 1.0f;
        public float B = 1.0f;
        public float rotX = 0.0f; // in degrees (converted to radians)
        public float rotY = 0.0f; // in degrees (converted to radians)
        public float rotZ = 0.0f; // in degrees (converted to radians)
        public Vector3 direction = new Vector3(0f, 0f, 1f);

        public DirectionalLight(float H, float S, float V, float rotX, float rot, float rotZ)
        {
            // calculate light color
            RenderTools.HSV2RGB(H, S, V, out R, out G, out B);

            // calculate light vector
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, rotX * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitY, rotY * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitZ, rotZ * ((float)Math.PI / 180f));

            direction = Vector3.Transform(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
        }

        public DirectionalLight(float H, float S, float V, Vector3 lightDirection)
        {
            // calculate light color
            RenderTools.HSV2RGB(H, S, V, out R, out G, out B);

            // calculate light vector
            direction = lightDirection;
        }

        public DirectionalLight()
        {

        }

        public void setDirectionFromXYZAngles(float rotX, float rotY, float rotZ)
        {
            // calculate light vector from 3 rotation angles
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, rotX * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitY, rotY * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitZ, rotZ * ((float)Math.PI / 180f));

            direction = Vector3.Transform(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
        }

        public void setColorFromHSV(float H, float S, float V)
        {
            RenderTools.HSV2RGB(H, S, V, out R, out G, out B);
        }

    }

    public class AreaLight
    {
        public float groundR = 1.0f;
        public float groundG = 1.0f;
        public float groundB = 1.0f;

        public float skyR = 1.0f;
        public float skyG = 1.0f;
        public float skyB = 1.0f;

        public float scaleX = 1.0f;
        public float scaleY = 1.0f;
        public float scaleZ = 1.0f;

        public float rotX = 0.0f; // in degrees (converted to radians)
        public float rotY = 0.0f; // in degrees (converted to radians)
        public float rotZ = 0.0f; // in degrees (converted to radians)
        public Vector3 direction = new Vector3(0f, 0f, 1f);


        public AreaLight()
        {

        }

        public void setDirectionFromXYZAngles(float rotX, float rotY, float rotZ)
        {
            // calculate light vector from 3 rotation angles
            Matrix4 lightRotMatrix = Matrix4.CreateFromAxisAngle(Vector3.UnitX, rotX * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitY, rotY * ((float)Math.PI / 180f))
             * Matrix4.CreateFromAxisAngle(Vector3.UnitZ, rotZ * ((float)Math.PI / 180f));

            direction = Vector3.Transform(new Vector3(0f, 0f, 1f), lightRotMatrix).Normalized();
        }



    }


    public class RenderTools
    {
        public static int defaultTex = -1, userTex;
        public static int cubeTex, cubeTex2;
        public static int defaultRamp;
        public static int UVTestPattern;

        public static int cubeVAO, cubeVBO;

        public static void Setup()
        {
            cubeTex2 = LoadCubeMap(Smash_Forge.Properties.Resources._10101000);
            defaultRamp = NUT.loadImage(Smash_Forge.Properties.Resources._10080000);
            UVTestPattern = NUT.loadImage(Smash_Forge.Properties.Resources.UVPattern);

            if (defaultTex == -1)
            cubeTex = LoadCubeMap(Smash_Forge.Properties.Resources.cubemap);
            if(defaultTex == -1)
            defaultTex = NUT.loadImage(Smash_Forge.Resources.Resources.DefaultTexture);
         
            GL.GenBuffers(1, out cubeVAO);
            GL.GenBuffers(1, out cubeVBO);
        }


        public static void drawTranslator(Matrix4 view)
        {
            Vector3 center = new Vector3(5, 10, 5);

            // check if within range
            {
                Vector3 p1 = Vector3.Transform(center, view).Normalized();
                Vector3 p2 = Vector3.Transform(center + new Vector3(0, 5, 0), view).Normalized();

                // check if mouse is within range
                
            }

            GL.Color3(Color.Green);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(center);
            GL.Vertex3(center + new Vector3(0, 5, 0));
            GL.End();

            GL.Color3(Color.Red);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(center);
            GL.Vertex3(center + new Vector3(5, 0, 0));
            GL.End();

            GL.Color3(Color.Blue);
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(center);
            GL.Vertex3(center + new Vector3(0, 0, 5));
            GL.End();
        }


        #region Taken from Brawllib render TKContext.cs
        public static void drawSphere(Vector3 center, float radius, uint precision)
        {

            if (radius < 0.0f)
                radius = -radius;

            if (radius == 0.0f)
                throw new DivideByZeroException("DrawSphere: Radius cannot be zero.");

            if (precision == 0)
                throw new DivideByZeroException("DrawSphere: Precision of 8 or greater is required.");

            float halfPI = (float)(Math.PI * 0.5);
            float oneThroughPrecision = 1.0f / precision;
            float twoPIThroughPrecision = (float)(Math.PI * 2.0 * oneThroughPrecision);

            float theta1, theta2, theta3;
            Vector3 norm = new Vector3(), pos = new Vector3();

            for (uint j = 0; j < precision / 2; j++)
            {
                theta1 = (j * twoPIThroughPrecision) - halfPI;
                theta2 = ((j + 1) * twoPIThroughPrecision) - halfPI;

                GL.Begin(PrimitiveType.TriangleStrip);
                for (uint i = 0; i <= precision; i++)
                {
                    theta3 = i * twoPIThroughPrecision;

                    norm.X = (float)(Math.Cos(theta2) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta2);
                    norm.Z = (float)(Math.Cos(theta2) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * (j + 1) * oneThroughPrecision);
                    GL.Vertex3(pos.X, pos.Y, pos.Z);

                    norm.X = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta1);
                    norm.Z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * j * oneThroughPrecision);
                    GL.Vertex3(pos.X, pos.Y, pos.Z);
                }
                GL.End();
            }
        }

        public static void drawWireframeSphere(Vector3 center, float radius, uint precision)
        {

            if (radius < 0.0f)
                radius = -radius;

            if (radius == 0.0f)
                throw new DivideByZeroException("DrawSphere: Radius cannot be zero.");

            if (precision == 0)
                throw new DivideByZeroException("DrawSphere: Precision of 8 or greater is required.");

            float halfPI = (float)(Math.PI * 0.5);
            float oneThroughPrecision = 1.0f / precision;
            float twoPIThroughPrecision = (float)(Math.PI * 2.0 * oneThroughPrecision);

            float theta1, theta2, theta3;
            Vector3 norm = new Vector3(), pos = new Vector3();

            for (uint j = 0; j < precision / 2; j++)
            {
                theta1 = (j * twoPIThroughPrecision) - halfPI;
                theta2 = ((j + 1) * twoPIThroughPrecision) - halfPI;

                GL.Begin(PrimitiveType.LineStrip);
                for (uint i = 0; i <= precision; i++)
                {
                    theta3 = i * twoPIThroughPrecision;

                    norm.X = (float)(Math.Cos(theta2) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta2);
                    norm.Z = (float)(Math.Cos(theta2) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * (j + 1) * oneThroughPrecision);
                    GL.Vertex3(pos.X, pos.Y, pos.Z);

                    norm.X = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta1);
                    norm.Z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * j * oneThroughPrecision);
                    GL.Vertex3(pos.X, pos.Y, pos.Z);
                }
                GL.End();
            }
        }

        public static void beginTopLevelStencil()
        {
            GL.Enable(EnableCap.StencilTest);

            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.ColorMask(false, false, false, false);
        }

        // The same as beginTopLevelStencil but writes 0s instead of 1s
        // Also it does not clear the stencil buffer
        public static void beginTopLevelAntiStencil()
        {
            GL.Enable(EnableCap.StencilTest);

            GL.StencilFunc(StencilFunction.Always, 0, 0xFF);
            GL.StencilMask(0xFF);
            GL.Disable(EnableCap.DepthTest);
            GL.ColorMask(false, false, false, false);
        }

        public static void endTopLevelStencilAndDraw()
        {
            GL.ColorMask(true, true, true, true);
            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
            GL.StencilMask(0x00);
            //GL.Disable(EnableCap.CullFace);

            drawSphere(Vector3.Zero, 100, 10);

            GL.StencilMask(0xFF);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.CullFace);
        }

        public static void resetStencil()
        {
            GL.Enable(EnableCap.StencilTest);

            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.ColorMask(false, false, false, false);

            GL.ColorMask(true, true, true, true);
            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
            GL.StencilMask(0x00);
            //GL.Disable(EnableCap.CullFace);

            GL.StencilMask(0xFF);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
        }

        public static void drawSphereTransformedVisible(Vector3 center, float radius, uint precision, Matrix4 transform)
        {
            GL.Enable(EnableCap.StencilTest);

            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.ColorMask(false, false, false, false);

               drawSphereTransformed(center, radius, precision, transform);

            GL.ColorMask(true, true, true, true);
            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
            GL.StencilMask(0x00);
            GL.Disable(EnableCap.CullFace);

            drawSphere(Vector3.Zero, 100, 10);

            GL.StencilMask(0xFF);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        public static void drawWireframeSphereTransformedVisible(Vector3 center, float radius, uint precision, Matrix4 transform)
        {
            GL.Enable(EnableCap.StencilTest);

            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.ColorMask(false, false, false, false);

            drawWireframeSphereTransformed(center, radius, precision, transform);

            GL.ColorMask(true, true, true, true);
            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
            GL.StencilMask(0x00);
            GL.Disable(EnableCap.CullFace);

            drawSphere(Vector3.Zero, 100, 10);

            GL.StencilMask(0xFF);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        public static void drawSphereTransformed(Vector3 center, float radius, uint precision, Matrix4 transform)
        {
            if (radius < 0.0f)
                radius = -radius;

            if (radius == 0.0f)
                throw new DivideByZeroException("DrawSphere: Radius cannot be zero.");

            if (precision == 0)
                throw new DivideByZeroException("DrawSphere: Precision of 8 or greater is required.");

            float halfPI = (float)(Math.PI * 0.5);
            float oneThroughPrecision = 1.0f / precision;
            float twoPIThroughPrecision = (float)(Math.PI * 2.0 * oneThroughPrecision);

            float theta1, theta2, theta3;
            Vector3 norm = new Vector3(), pos = new Vector3();

            for (uint j = 0; j < precision / 2; j++)
            {
                theta1 = (j * twoPIThroughPrecision) - halfPI;
                theta2 = ((j + 1) * twoPIThroughPrecision) - halfPI;

                GL.Begin(PrimitiveType.TriangleStrip);
                for (uint i = 0; i <= precision; i++)
                {
                    theta3 = i * twoPIThroughPrecision;

                    norm.X = (float)(Math.Cos(theta2) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta2);
                    norm.Z = (float)(Math.Cos(theta2) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * (j + 1) * oneThroughPrecision);
                    GL.Vertex3(Vector3.Transform(new Vector3(pos.X, pos.Y, pos.Z), transform));

                    norm.X = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta1);
                    norm.Z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * j * oneThroughPrecision);
                    GL.Vertex3(Vector3.Transform(new Vector3(pos.X, pos.Y, pos.Z), transform));
                }
                GL.End();
            }
        }

        public static void drawWireframeSphereTransformed(Vector3 center, float radius, uint precision, Matrix4 transform)
        {
            if (radius < 0.0f)
                radius = -radius;

            if (radius == 0.0f)
                throw new DivideByZeroException("DrawSphere: Radius cannot be zero.");

            if (precision == 0)
                throw new DivideByZeroException("DrawSphere: Precision of 8 or greater is required.");

            float halfPI = (float)(Math.PI * 0.5);
            float oneThroughPrecision = 1.0f / precision;
            float twoPIThroughPrecision = (float)(Math.PI * 2.0 * oneThroughPrecision);

            float theta1, theta2, theta3;
            Vector3 norm = new Vector3(), pos = new Vector3();

            for (uint j = 0; j < precision / 2; j++)
            {
                theta1 = (j * twoPIThroughPrecision) - halfPI;
                theta2 = ((j + 1) * twoPIThroughPrecision) - halfPI;

                GL.Begin(PrimitiveType.LineStrip);
                for (uint i = 0; i <= precision; i++)
                {
                    theta3 = i * twoPIThroughPrecision;

                    norm.X = (float)(Math.Cos(theta2) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta2);
                    norm.Z = (float)(Math.Cos(theta2) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * (j + 1) * oneThroughPrecision);
                    GL.Vertex3(Vector3.Transform(new Vector3(pos.X, pos.Y, pos.Z), transform));

                    norm.X = (float)(Math.Cos(theta1) * Math.Cos(theta3));
                    norm.Y = (float)Math.Sin(theta1);
                    norm.Z = (float)(Math.Cos(theta1) * Math.Sin(theta3));
                    pos.X = center.X + radius * norm.X;
                    pos.Y = center.Y + radius * norm.Y;
                    pos.Z = center.Z + radius * norm.Z;

                    GL.Normal3(norm.X, norm.Y, norm.Z);
                    GL.TexCoord2(i * oneThroughPrecision, 2.0f * j * oneThroughPrecision);
                    GL.Vertex3(Vector3.Transform(new Vector3(pos.X, pos.Y, pos.Z), transform));
                }
                GL.End();
            }
        }

        public static Matrix4 def = Matrix4.CreateTranslation(0,0,0);

        public static void drawHitboxCylinder(Vector3 p1, Vector3 p2, float R)
        {
            Vector3 yAxis = new Vector3(0, 1, 0);
            Vector3 d = p2 - p1;
            float height = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z) / 2;

            Vector3 mid = (p1 + p2) / 2;  // midpoint

            Vector3 axis = Vector3.Cross(d, yAxis);
            float angle = (float)Math.Acos(Vector3.Dot(d.Normalized(), yAxis));
            

            GL.Enable(EnableCap.StencilTest);

            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.ColorMask(false, false, false, false);

            drawSphere(p1, R, 20);
            drawSphere(p2, R, 20);

            //  sides
            GL.PushMatrix();

            //GL.Scale(scale.X, scale.Y, scale.Z);
            //double[] f = new double[] {
            //    transform.M11, transform.M12, transform.M13, transform.M14,
            //    transform.M21, transform.M22, transform.M23, transform.M24,
            //    transform.M31, transform.M32, transform.M33, transform.M34,
            //    transform.M41, transform.M42, transform.M43, transform.M44,
            //};
            //GL.MultMatrix(f);
            //Vector3 scale = transform.ExtractScale();
            GL.Translate(mid);
            GL.Rotate(-(float)(angle * (180 / Math.PI)), axis);

            GL.Begin(PrimitiveType.QuadStrip);
            for (int j = 0; j <= 8 * 3; j += 1)
            {
                GL.Vertex3((float)Math.Cos(j) * R, +height, (float)Math.Sin(j) * R);
                GL.Vertex3((float)Math.Cos(j) * R, -height, (float)Math.Sin(j) * R);
            }
            GL.End();

            GL.PopMatrix();

            GL.ColorMask(true, true, true, true);
            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
            GL.StencilMask(0x00);
            GL.Disable(EnableCap.CullFace);

            drawSphere(Vector3.Zero, 100, 10);

            GL.StencilMask(0xFF);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        public static void drawReducedCylinderTransformed(Vector3 p1, Vector3 p2, float R, Matrix4 transform)
        {
            Vector3 yAxis = new Vector3(0, 1, 0);
            Vector3 d = p2 - p1;
            float height = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z) / 2;

            Vector3 mid = (p1 + p2) / 2;

            Vector3 axis = Vector3.Cross(d, yAxis);
            float angle = (float)Math.Acos(Vector3.Dot(d.Normalized(), yAxis));
            
            GL.Enable(EnableCap.StencilTest);

            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.ColorMask(false, false, false, false);
                
            drawSphereTransformed(p1, R, 20, transform);
            drawSphereTransformed(p2, R, 20, transform);

            //  sides
            GL.PushMatrix();

            //GL.Scale(scale.X, scale.Y, scale.Z);
            double[] f = new double[] {
                transform.M11, transform.M12, transform.M13, transform.M14,
                transform.M21, transform.M22, transform.M23, transform.M24,
                transform.M31, transform.M32, transform.M33, transform.M34,
                transform.M41, transform.M42, transform.M43, transform.M44,
            };
            GL.MultMatrix(f);
            //Vector3 scale = transform.ExtractScale();
            GL.Translate(mid);
            GL.Rotate(-(float)(angle * (180 / Math.PI)), axis);

            GL.Begin(PrimitiveType.QuadStrip);
            for (int j = 0; j <= 8 * 3; j += 1)
            {
                GL.Vertex3((float)Math.Cos(j) * R, +height, (float)Math.Sin(j) * R);
                GL.Vertex3((float)Math.Cos(j) * R, -height, (float)Math.Sin(j) * R);
            }
            GL.End();
        
            GL.PopMatrix();

            GL.ColorMask(true, true, true, true);
            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
            GL.StencilMask(0x00);
            GL.Disable(EnableCap.CullFace);

            drawSphere(Vector3.Zero, 100, 10);

            GL.StencilMask(0xFF);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        public static void drawWireframeCylinderTransformed(Vector3 p1, Vector3 p2, float R, Matrix4 transform)
        {
            Vector3 yAxis = new Vector3(0, 1, 0);
            Vector3 d = p2 - p1;
            float height = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z) / 2;

            Vector3 mid = (p1 + p2) / 2;

            Vector3 axis = Vector3.Cross(d, yAxis);
            float angle = (float)Math.Acos(Vector3.Dot(d.Normalized(), yAxis));

            GL.Enable(EnableCap.StencilTest);

            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilMask(0xFF);
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.ColorMask(false, false, false, false);

            drawWireframeSphereTransformed(p1, R, 10, transform);
            drawWireframeSphereTransformed(p2, R, 10, transform);

            //  sides
            GL.PushMatrix();

            //GL.Scale(scale.X, scale.Y, scale.Z);
            double[] f = new double[] {
                transform.M11, transform.M12, transform.M13, transform.M14,
                transform.M21, transform.M22, transform.M23, transform.M24,
                transform.M31, transform.M32, transform.M33, transform.M34,
                transform.M41, transform.M42, transform.M43, transform.M44,
            };
            GL.MultMatrix(f);
            //Vector3 scale = transform.ExtractScale();
            GL.Translate(mid);
            GL.Rotate(-(float)(angle * (180 / Math.PI)), axis);

            GL.Begin(PrimitiveType.LineStrip);
            for (int j = 0; j <= 8 * 3; j += 1)
            {
                GL.Vertex3((float)Math.Cos(j) * R, +height, (float)Math.Sin(j) * R);
                GL.Vertex3((float)Math.Cos(j) * R, -height, (float)Math.Sin(j) * R);
            }
            GL.End();

            GL.PopMatrix();

            GL.ColorMask(true, true, true, true);
            GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
            GL.StencilMask(0x00);
            GL.Disable(EnableCap.CullFace);

            drawSphere(Vector3.Zero, 100, 10);

            GL.StencilMask(0xFF);
            GL.Clear(ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
        }

        public static void drawCylinder(Vector3 p1, Vector3 p2, float R){
            int q = 8, p = 20;

            Vector3 yAxis = new Vector3 (0, 1, 0);
            Vector3 d = p2 - p1;
            float height = (float)Math.Sqrt (d.X*d.X + d.Y*d.Y + d.Z*d.Z) / 2;

            Vector3 mid = (p1 + p2) / 2;

            Vector3 axis = Vector3.Cross (d, yAxis);
            float angle = (float)Math.Acos (Vector3.Dot(d.Normalized(), yAxis));

            GL.PushMatrix ();
            GL.Translate(p1);
            GL.Rotate (-(float)((angle) * (180/Math.PI)), axis);
            for(int j = 0; j < q; j++)
            {
                GL.Begin(PrimitiveType.TriangleStrip);
                for(int i = 0; i <= p; i++)
                {
                    GL.Vertex3( R * Math.Cos( (float)(j+1)/q * Math.PI/2.0 ) * Math.Cos( 2.0 * (float)i/p * Math.PI ),
                        -R * Math.Sin( (float)(j+1)/q * Math.PI/2.0 ),
                        R * Math.Cos( (float)(j+1)/q * Math.PI/2.0 ) * Math.Sin( 2.0 * (float)i/p * Math.PI ) );
                    GL.Vertex3( R * Math.Cos( (float)j/q * Math.PI/2.0 ) * Math.Cos( 2.0 * (float)i/p * Math.PI ),
                        -R * Math.Sin( (float)j/q * Math.PI/2.0 ),
                        R * Math.Cos( (float)j/q * Math.PI/2.0 ) * Math.Sin( 2.0 * (float)i/p * Math.PI ) );         
                }
                GL.End();
            }
            GL.PopMatrix ();

            GL.PushMatrix ();
            GL.Translate(p2);
            GL.Rotate (-(float)(angle * (180/Math.PI)), axis);
            for(int j = 0; j < q; j++)
            {
                GL.Begin(PrimitiveType.TriangleStrip);
                for(int i = 0; i <= p; i++)
                {
                    GL.Vertex3( R * Math.Cos( (float)(j+1)/q * Math.PI/2.0 ) * Math.Cos( 2.0 * (float)i/p * Math.PI ),
                        R * Math.Sin( (float)(j+1)/q * Math.PI/2.0 ),
                        R * Math.Cos( (float)(j+1)/q * Math.PI/2.0 ) * Math.Sin( 2.0 * (float)i/p * Math.PI ) );
                    GL.Vertex3( R * Math.Cos( (float)j/q * Math.PI/2.0 ) * Math.Cos( 2.0 * (float)i/p * Math.PI ),
                        R * Math.Sin( (float)j/q * Math.PI/2.0 ),
                        R * Math.Cos( (float)j/q * Math.PI/2.0 ) * Math.Sin( 2.0 * (float)i/p * Math.PI ) );         
                }
                GL.End();
            }
            GL.PopMatrix ();


            /*  sides */
            GL.PushMatrix ();

            GL.Translate(mid);
            GL.Rotate (-(float)(angle * (180/Math.PI)), axis);

            GL.Begin(PrimitiveType.QuadStrip);
            for (int j=0;j<=360;j+=1) {
                GL.Vertex3((float)Math.Cos(j)*R,+height, (float)Math.Sin(j)*R);
                GL.Vertex3((float)Math.Cos(j)*R,-height, (float)Math.Sin(j)*R);
            }
            GL.End();

            GL.PopMatrix ();
        }

        public static void drawWireframeCylinder(Vector3 p1, Vector3 p2, float R)
        {
            int q = 8, p = 20;

            Vector3 yAxis = new Vector3(0, 1, 0);
            Vector3 d = p2 - p1;
            float height = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z) / 2;

            Vector3 mid = (p1 + p2) / 2;

            Vector3 axis = Vector3.Cross(d, yAxis);
            float angle = (float)Math.Acos(Vector3.Dot(d.Normalized(), yAxis));

            GL.PushMatrix();
            GL.Translate(p1);
            GL.Rotate(-(float)((angle) * (180 / Math.PI)), axis);
            for (int j = 0; j < q; j++)
            {
                GL.Begin(PrimitiveType.LineStrip);
                for (int i = 0; i <= p; i++)
                {
                    GL.Vertex3(R * Math.Cos((float)(j + 1) / q * Math.PI / 2.0) * Math.Cos(2.0 * (float)i / p * Math.PI),
                        -R * Math.Sin((float)(j + 1) / q * Math.PI / 2.0),
                        R * Math.Cos((float)(j + 1) / q * Math.PI / 2.0) * Math.Sin(2.0 * (float)i / p * Math.PI));
                    GL.Vertex3(R * Math.Cos((float)j / q * Math.PI / 2.0) * Math.Cos(2.0 * (float)i / p * Math.PI),
                        -R * Math.Sin((float)j / q * Math.PI / 2.0),
                        R * Math.Cos((float)j / q * Math.PI / 2.0) * Math.Sin(2.0 * (float)i / p * Math.PI));
                }
                GL.End();
            }
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Translate(p2);
            GL.Rotate(-(float)(angle * (180 / Math.PI)), axis);
            for (int j = 0; j < q; j++)
            {
                GL.Begin(PrimitiveType.LineStrip);
                for (int i = 0; i <= p; i++)
                {
                    GL.Vertex3(R * Math.Cos((float)(j + 1) / q * Math.PI / 2.0) * Math.Cos(2.0 * (float)i / p * Math.PI),
                        R * Math.Sin((float)(j + 1) / q * Math.PI / 2.0),
                        R * Math.Cos((float)(j + 1) / q * Math.PI / 2.0) * Math.Sin(2.0 * (float)i / p * Math.PI));
                    GL.Vertex3(R * Math.Cos((float)j / q * Math.PI / 2.0) * Math.Cos(2.0 * (float)i / p * Math.PI),
                        R * Math.Sin((float)j / q * Math.PI / 2.0),
                        R * Math.Cos((float)j / q * Math.PI / 2.0) * Math.Sin(2.0 * (float)i / p * Math.PI));
                }
                GL.End();
            }
            GL.PopMatrix();


            /*  sides */
            GL.PushMatrix();

            GL.Translate(mid);
            GL.Rotate(-(float)(angle * (180 / Math.PI)), axis);

            GL.Begin(PrimitiveType.LineStrip);
            for (int j = 0; j <= 45; j += 1)
            {
                GL.Vertex3((float)Math.Cos(j) * R, +height, (float)Math.Sin(j) * R);
                GL.Vertex3((float)Math.Cos(j) * R, -height, (float)Math.Sin(j) * R);
            }
            GL.End();

            GL.PopMatrix();
        }

        //Alternate drawCylinder method that tries to keep opacity uniform by reducing sides iterations, used for hurtboxes so model can still be visible
        public static void drawReducedSidesCylinder(Vector3 p1, Vector3 p2, float R)
        {
            int q = 8, p = 20;

            Vector3 yAxis = new Vector3(0, 1, 0);
            Vector3 d = p2 - p1;
            float height = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z) / 2;

            Vector3 mid = (p1 + p2) / 2;

            Vector3 axis = Vector3.Cross(d, yAxis);
            float angle = (float)Math.Acos(Vector3.Dot(d.Normalized(), yAxis));

            GL.PushMatrix();
            GL.Translate(p1);
            GL.Rotate(-(float)((angle) * (180 / Math.PI)), axis);
            for (int j = 0; j < q; j++)
            {
                GL.Begin(PrimitiveType.TriangleStrip);
                for (int i = 0; i <= p; i++)
                {
                    GL.Vertex3(R * Math.Cos((float)(j + 1) / q * Math.PI / 2.0) * Math.Cos(2.0 * (float)i / p * Math.PI),
                        -R * Math.Sin((float)(j + 1) / q * Math.PI / 2.0),
                        R * Math.Cos((float)(j + 1) / q * Math.PI / 2.0) * Math.Sin(2.0 * (float)i / p * Math.PI));
                    GL.Vertex3(R * Math.Cos((float)j / q * Math.PI / 2.0) * Math.Cos(2.0 * (float)i / p * Math.PI),
                        -R * Math.Sin((float)j / q * Math.PI / 2.0),
                        R * Math.Cos((float)j / q * Math.PI / 2.0) * Math.Sin(2.0 * (float)i / p * Math.PI));
                }
                GL.End();
            }
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Translate(p2);
            GL.Rotate(-(float)(angle * (180 / Math.PI)), axis);
            for (int j = 0; j < q; j++)
            {
                GL.Begin(PrimitiveType.TriangleStrip);
                for (int i = 0; i <= p; i++)
                {
                    GL.Vertex3(R * Math.Cos((float)(j + 1) / q * Math.PI / 2.0) * Math.Cos(2.0 * (float)i / p * Math.PI),
                        R * Math.Sin((float)(j + 1) / q * Math.PI / 2.0),
                        R * Math.Cos((float)(j + 1) / q * Math.PI / 2.0) * Math.Sin(2.0 * (float)i / p * Math.PI));
                    GL.Vertex3(R * Math.Cos((float)j / q * Math.PI / 2.0) * Math.Cos(2.0 * (float)i / p * Math.PI),
                        R * Math.Sin((float)j / q * Math.PI / 2.0),
                        R * Math.Cos((float)j / q * Math.PI / 2.0) * Math.Sin(2.0 * (float)i / p * Math.PI));
                }
                GL.End();
            }
            GL.PopMatrix();


            /*  sides */
            GL.PushMatrix();

            GL.Translate(mid);
            GL.Rotate(-(float)(angle * (180 / Math.PI)), axis);

            GL.Begin(PrimitiveType.QuadStrip);
            for (int j = 0; j <= q * 3; j += 1) //Reduced iterations to make quadstrips do a cylinder but keeping opacity low
            {
                GL.Vertex3(Math.Cos(j) * R, +height, Math.Sin(j) * R);
                GL.Vertex3(Math.Cos(j) * R, -height, Math.Sin(j) * R);
            }
            GL.End();

            GL.PopMatrix();
        }

        public static void draw2DCircle(float x, float y, float radius, Color color, int screenWidth, int screenHeight)
        {

            // No shaders
            GL.UseProgram(0);

            // Go to 2D
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0.0f, screenWidth, screenHeight, 0.0f, -1.0f, 10.0f);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            // Allow transparency
            GL.Enable(EnableCap.Blend);

            // Draw over everything
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // Draw here
            GL.Color4(color);
            uint precision = 30;  // force particular method overload
            drawCircle(new Vector3(x, y, -1f), radius, precision);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            // Back to 3D
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
        }

        public static void drawFloor()
        {
            bool solid = Runtime.floorStyle == Runtime.FloorStyle.Solid;
            float s = Runtime.floorSize;

            GL.UseProgram(0);

            GL.Color3(Runtime.floorColor);
            GL.LineWidth(1f);

            if (Runtime.floorStyle == Runtime.FloorStyle.Textured || Runtime.floorStyle == Runtime.FloorStyle.UserTexture)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.ActiveTexture(TextureUnit.Texture0);
                if (Runtime.floorStyle == Runtime.FloorStyle.UserTexture)
                    GL.BindTexture(TextureTarget.Texture2D, userTex);
                else
                    GL.BindTexture(TextureTarget.Texture2D, defaultTex);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)Runtime.floorWrap);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)Runtime.floorWrap);

                GL.Color3(Runtime.floorColor == Color.Gray ? Color.White : Runtime.floorColor);
                GL.Begin(PrimitiveType.Quads);

                GL.TexCoord2(0, 0);
                GL.Vertex3(new Vector3(-s, 0f, -s));
                GL.TexCoord2(0, 2);
                GL.Vertex3(new Vector3(-s, 0f, s));
                GL.TexCoord2(2, 2);
                GL.Vertex3(new Vector3(s, 0f, s));
                GL.TexCoord2(2, 0);
                GL.Vertex3(new Vector3(s, 0f, -s));

                GL.End();
                GL.Disable(EnableCap.Texture2D);
            }
            else
            if (solid)
            {
                GL.Begin(PrimitiveType.Quads);
                GL.Vertex3(-s, 0f, -s);
                GL.Vertex3(-s, 0f, s);
                GL.Vertex3(s, 0f, s);
                GL.Vertex3(s, 0f, -s);
                GL.End();
            }
            else
            {
                GL.Begin(PrimitiveType.Lines);
                for (var i = -s / 2; i <= s / 2; i++)
                {
                    if (i != 0)
                    {
                        GL.Vertex3(-s, 0f, i * 2);
                        GL.Vertex3(s, 0f, i * 2);
                        GL.Vertex3(i * 2, 0f, -s);
                        GL.Vertex3(i * 2, 0f, s);
                    }
                }
                GL.End();
            }

            if (Runtime.renderFloorLines)
            {
                GL.Disable(EnableCap.DepthTest);
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(Color.White);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(-s, 0f, 0);
                GL.Vertex3(s, 0f, 0);
                GL.Vertex3(0, 0f, -s);
                GL.Vertex3(0, 0f, s);
                GL.End();
                GL.Enable(EnableCap.DepthTest);

                GL.Disable(EnableCap.DepthTest);
                GL.Color3(Color.LightGray);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 5, 0);
                GL.Vertex3(0, 0, 0);

                GL.Color3(Color.OrangeRed);
                GL.Vertex3(0f, 0f, 0);
                GL.Color3(Color.OrangeRed);
                GL.Vertex3(5f, 0f, 0);

                GL.Color3(Color.Olive);
                GL.Vertex3(0, 0f, 0f);
                GL.Color3(Color.Olive);
                GL.Vertex3(0, 0f, 5f);

                GL.End();
            }

            GL.Enable(EnableCap.DepthTest);
        }

        public static void drawCircle(float x, float y, float z, float radius, uint precision)
        {
            drawCircle(new Vector3(x, y, z), radius, precision);
        }

        public static void drawCircle(Vector3 center, float radius, uint precision)
        {
            float theta = 2.0f * (float)Math.PI / precision;
            float cosine = (float)Math.Cos(theta);
            float sine = (float)Math.Sin(theta);

            float x = radius;
            float y = 0;

            GL.Begin(PrimitiveType.TriangleFan);
            for (int i = 0; i < precision; i++)
            {
                GL.Vertex2(x + center.X, y + center.Y);

                //apply the rotation matrix
                var temp = x;
                x = cosine * x - sine * y;
                y = sine * temp + cosine * y;
            }
            GL.End();
        }


        public static void drawHitboxCircle(Vector3 pos, float size, uint smooth, Matrix4 view)
        {
            float t = 2 * (float)Math.PI / smooth;
            float tf = (float)Math.Tan(t);

            float rf = (float)Math.Cos(t);

            float x = size;
            float y = 0;

            GL.Begin(PrimitiveType.LineLoop);

            for (int i = 0; i < smooth; i++)
            {
                GL.Vertex3(Vector3.Transform(new Vector3(x + pos.X, y + pos.Y, pos.Z),view));
                float tx = -y;
                float ty = x;
                x += tx * tf;
                y += ty * tf;
                x *= rf;
                y *= rf;
            }

            GL.End();
        }

        public static void drawCircleOutline(Vector3 center, float radius, uint precision)
        {
            float theta = 2.0f * (float)Math.PI / precision;
            float cosine = (float)Math.Cos(theta);
            float sine = (float)Math.Sin(theta);

            float x = radius;
            float y = 0;

            GL.Begin(PrimitiveType.LineStrip);
            for (int i = 0; i < precision; i++)
            {
                GL.Vertex3(x + center.X, y + center.Y, center.Z);

                //apply the rotation matrix
                var temp = x;
                x = cosine * x - sine * y;
                y = sine * temp + cosine * y;
            }
            GL.End();
        }

        public static void drawCircleOutline(Vector3 center, float radius, uint precision, Matrix4 transform)
        {
            float theta = 2.0f * (float)Math.PI / precision;
            float cosine = (float)Math.Cos(theta);
            float sine = (float)Math.Sin(theta);

            float x = radius;
            float y = 0;

            GL.Begin(PrimitiveType.LineStrip);
            for (int i = 0; i < precision; i++)
            {
                GL.Vertex3(Vector3.Transform(new Vector3(x, y, 0), transform) + center);

                //apply the rotation matrix
                var temp = x;
                x = cosine * x - sine * y;
                y = sine * temp + cosine * y;
            }
            GL.End();
        }

        public static void drawCube(Vector3 center, float size)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);

            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);

            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);

            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.End();
        }
        public static void drawCubeWireframe(Vector3 center, float size)
        {
            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);

            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(center.X - size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X - size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z - size);
            GL.Vertex3(center.X - size, center.Y - size, center.Z + size);
            GL.End();

            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex3(center.X + size, center.Y + size, center.Z - size);
            GL.Vertex3(center.X + size, center.Y + size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z + size);
            GL.Vertex3(center.X + size, center.Y - size, center.Z - size);
            GL.End();
        }

        #endregion

        
        public static void drawCircle(Vector3 pos, float r, int smooth)
        {
            float t = 2 * (float)Math.PI / smooth;
            float tf = (float)Math.Tan(t);

            float rf = (float)Math.Cos(t);

            float x = r;
            float y = 0;

            GL.Begin(PrimitiveType.LineLoop);

            for (int i = 0; i < smooth; i++)
            {
                GL.Vertex3(x + pos.X, y + pos.Y, pos.Z);
                float tx = -y;
                float ty = x;
                x += tx * tf;
                y += ty * tf;
                x *= rf;
                y *= rf;
            }

            GL.End();
        }

        public static bool intersectCircle(Vector3 pos, float r, int smooth, Vector3 vA, Vector3 vB)
        {
            float t = 2 * (float)Math.PI / smooth;
            float tf = (float)Math.Tan(t);

            float rf = (float)Math.Cos(t);

            float x = r;
            float y = 0;
            
            for (int i = 0; i < smooth; i++)
            {
                Vector3 c;
                Vector3 p = new Vector3(x + pos.X, y +pos.Y, pos.Z);
                if (CheckSphereHit(p, 0.3f, vA, vB, out c))
                    return true;
                float tx = -y;
                float ty = x;
                x += tx * tf;
                y += ty * tf;
                x *= rf;
                y *= rf;
            }

            return false;
        }

        public static bool CheckSphereHit(Vector3 sphere, float rad, Vector3 vA, Vector3 vB, out Vector3 closest)
        {
            Vector3 dirToSphere = sphere - vA;
            Vector3 vLineDir = (vB - vA).Normalized();
            float fLineLength = 100;
            /*(float)Math.Sqrt(Math.Pow(vA.X - vB.X, 2)
                + Math.Pow(vA.Y - vB.Y, 2)
                + Math.Pow(vA.Z - vB.Z, 2));*/
            float t = Vector3.Dot(dirToSphere, vLineDir);

            if (t <= 0.0f)
                closest = vA;
            else if (t >= fLineLength)
                closest = vB;
            else
                closest = vA + vLineDir * t;

            return (Math.Pow(sphere.X - closest.X, 2)
                + Math.Pow(sphere.Y - closest.Y, 2)
                + Math.Pow(sphere.Z - closest.Z, 2) <= rad * rad);
        }

        #region FileRendering


        public static void DrawModel(ModelContainer m, Matrix4 v)
        {
            if (m.dat_melee != null)
            {
                m.dat_melee.Render(v);
            }

            if (m.nud != null)
            {
                m.nud.Render(v, m.vbn);
                m.nud.DrawPoints(v, m.vbn);
            }
        }

        public static void DrawBones()
        {
            if (Runtime.ModelContainers.Count > 0)
            {
                foreach (ModelContainer m in Runtime.ModelContainers)
                {
                    DrawVBN(m.vbn);
                    if (m.bch != null)
                    {
                        DrawVBN(m.bch.models[0].skeleton);
                    }

                    if (m.dat_melee != null)
                    {
                        DrawVBN(m.dat_melee.bones);
                    }
                }
            }
        }

        public static void DrawVBN(VBN vbn)
        {
            if (vbn != null && Runtime.renderBones)
            {
                Bone selectedBone = null;
                foreach (Bone bone in vbn.bones)
                {
                    if (!bone.IsSelected)
                        bone.Draw();
                    else
                        selectedBone = bone;

                    // if swing bones then draw swing radius
                    /*if (vbn.swingBones.bones.Count > 0 && Runtime.renderSwag)
                    {
                        SB.SBEntry sb = null;
                        vbn.swingBones.TryGetEntry(bone.boneId, out sb);
                        if (sb != null)
                        {
                            // draw
                            if (bone.ParentBone != null)
                            {
                                int i = bone.parentIndex;
                                float degtorad = (float)(Math.PI / 180);
                                Vector3 pos_sb = Vector3.Transform(Vector3.Zero,
                                    Matrix4.CreateTranslation(new Vector3(3, 3, 3))
                                    * Matrix4.CreateScale(bone.sca)
                                    * Matrix4.CreateFromQuaternion(VBN.FromEulerAngles(sb.rx1 * degtorad, sb.ry1 * degtorad, sb.rz1 * degtorad))
                                    * Matrix4.CreateTranslation(bone.pos)
                                    * vbn.bones[i].transform);

                                Vector3 pos_sb2 = Vector3.Transform(Vector3.Zero,
                                    Matrix4.CreateTranslation(new Vector3(3, 3, 3))
                                    * Matrix4.CreateScale(bone.sca)
                                    * Matrix4.CreateFromQuaternion(VBN.FromEulerAngles(sb.rx2 * degtorad, sb.ry2 * degtorad, sb.rz2 * degtorad))
                                    * Matrix4.CreateTranslation(bone.pos)
                                    * vbn.bones[i].transform);

                                GL.Color3(Color.ForestGreen);
                                GL.Begin(PrimitiveType.LineLoop);
                                GL.Vertex3(pos_c);
                                GL.Vertex3(pos_sb);
                                GL.Vertex3(pos_sb2);
                                GL.End();
                            }
                        }
                    }*/
                }
                if (selectedBone != null)
                {
                    GL.Clear(ClearBufferMask.DepthBufferBit);
                    selectedBone.Draw();
                }
            }
        }

        public static void DrawTexturedQuad(int texture, bool renderR, bool renderG, bool renderB, bool renderAlpha, bool alphaOverride)             
        {
            // draw RGB and alpha channels of texture to screen quad

            Shader shader = Runtime.shaders["Texture"];
            GL.UseProgram(shader.programID);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.Uniform1(shader.getAttribute("texture"), 0);

            GL.Uniform1(shader.getAttribute("renderR"), renderR ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderG"), renderG ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderB"), renderB ? 1 : 0);
            GL.Uniform1(shader.getAttribute("renderAlpha"), renderAlpha ? 1 : 0);
            GL.Uniform1(shader.getAttribute("alphaOverride"), alphaOverride ? 1 : 0);



            GL.Disable(EnableCap.DepthTest);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3); // draw full screen "quad" (big triangle)
            GL.BindVertexArray(0);
        }

        public static void HSV2RGB(float h, float s, float v, out float R, out float G, out float B) 
        {

            R = 1.0f;
            G = 1.0f;
            B = 1.0f;

            // values have to be 0-360 to work properly
            while (h > 360) { h -= 360; }
            while (h < 0) { h += 360; }

            if (s > 1.0f)
                s = 1.0f;

            float hf = h / 60.0f;
            int i = (int)Math.Floor(hf);
            float f = hf - i;
            float pv = v * (1 - s);
            float qv = v * (1 - s * f);
            float tv = v * (1 - s * (1 - f));

            switch (i)
            {
                // Red is the dominant color

                case 0:
                    R = v;
                    G = tv;
                    B = pv;
                    break;

                // Green is the dominant color

                case 1:
                    R = qv;
                    G = v;
                    B = pv;
                    break;
                case 2:
                    R = pv;
                    G = v;
                    B = tv;
                    break;

                // Blue is the dominant color

                case 3:
                    R = pv;
                    G = qv;
                    B = v;
                    break;
                case 4:
                    R = tv;
                    G = pv;
                    B = v;
                    break;

                // Red is the dominant color

                case 5:
                    R = v;
                    G = pv;
                    B = qv;
                    break;

                case 6:
                    R = v;
                    G = tv;
                    B = pv;
                    break;
            
            }

        }

        public static void RGB2HSV(float R, float G, float B, out float h, out float s, out float v) // need to check weird values for errors
        {
            h = 360.0f;
            s = 1.0f;
            v = 1.0f;

            float cMax = Math.Max(Math.Max(R, G), B);
            float cMin = Math.Min(Math.Min(R, G), B);
            float delta = cMax - cMin;

            v = cMax;

            if (delta == 0)
                h = 0;

            if (v == 0)
                s = 0.0f;
            else
                s = delta / v;

            if (R == cMax)
                h = 60.0f * (((G - B) / delta));

            else if (G == cMax)
                h = 60.0f * (((B - R) / delta) + 2);

            else if (B == cMax)
                h = 60.0f * (((R - G) / delta) + 4);

            while (h < 0.0f)
                h += 360.0f;

        }

        public static void ColorTemp2RGB(float temp, out float R, out float G, out float B)
        {
            // adapted from a cool algorithm by Tanner Helland
            // http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/ 

            R = 1.0f;
            G = 1.0f;
            B = 1.0f;

            // use doubles for calculations and convert to float at the end
            // no need for double precision floating point colors on GPU
            double Red = 255.0;
            double Green = 255.0;
            double Blue = 255.0;

            temp = temp / 100.0f;

            // Red calculations
            if (temp <= 66.0f)
                Red = 255.0f;
            else
            {
                Red = temp - 60.0;
                Red = 329.698727446 * Math.Pow(Red, -0.1332047592);
                if (Red < 0.0)
                    Red = 0.0;
                if (Red > 255.0)
                    Red = 255.0;
            }

            // Green calculations
            if (temp <= 66.0)
            {
                Green = temp;
                Green = 99.4708025861 * Math.Log(Green) - 161.1195681661;
                if (Green < 0.0)
                    Green = 0.0;
                if (Green > 255.0)
                    Green = 255.0;
            }
            else
            {
                Green = temp - 60.0;
                Green = 288.1221695283 * Math.Pow(Green, -0.0755148492);
                if (Green < 0)
                    Green = 0;
                if (Green > 255)
                    Green = 255;
            }
                
            // Blue calculations
            if (temp >= 66.0)
                Blue = 255.0;
            else if (temp <= 19.0)
                Blue = 0.0;
            else
            {
                Blue = temp - 10;
                Blue = 138.5177312231 * Math.Log(Blue) - 305.0447927307;
                if (Blue < 0.0)
                    Blue = 0.0;
                if (Blue > 255)
                    Blue = 255;
            }

            Red = Red / 255.0;
            Green = Green / 255.0;
            Blue = Blue / 255.0;

            R = (float)Red;
            G = (float)Green;
            B = (float)Blue;
        }


        public static int ClampInt(int i) // Restricts RGB values to 0 to 255
        {
            if (i > 255)
                return 255;
            if (i < 0)
                return 0;
            else
                return i;
        }

        public static float ClampFloat(float i) // Restricts RGB values to 0.0 to 1.0
        {
            if (i > 1.0f)
                return 1.0f;
            if (i < 0.0f)
                return 0.0f;
            else
                return i;
        }

        public static int Float2RGBClamp(float i) 
        {
            // converts input color to int and restricts values to 0 to 255
            // useful for setting colors of form UI stuff

            i *= 255;
            i = (int)i;
            if (i > 255)
                return 255;
            if (i < 0)
                return 0;
            return (int)i;
        }


        #endregion

        #region Other
        public static int LoadCubeMap(Bitmap b)
        {
            int id;
            GL.GenBuffers(1, out id);

            GL.ActiveTexture(0);
            
            GL.BindTexture(TextureTarget.TextureCubeMap, id);

            Bitmap bmp = b;

            Rectangle[] srcRect = new Rectangle[] {
            new Rectangle(0, 0, 128, 128),
            new Rectangle(0, 128, 128, 128),
            new Rectangle(0, 256, 128, 128),
            new Rectangle(0, 384, 128, 128),
            new Rectangle(0, 512, 128, 128),
            new Rectangle(0, 640, 128, 128),
            }; 

            for(int i = 0; i < 6; i++)
            {
                Bitmap image = (Bitmap)bmp.Clone(srcRect[i], bmp.PixelFormat);
                BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                image.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            return id;
        }

        private static float[] cube_vertices = new float[]{
      -1.0f,  1.0f,  1.0f,
      -1.0f, -1.0f,  1.0f,
       1.0f, -1.0f,  1.0f,
       1.0f,  1.0f,  1.0f,
      -1.0f,  1.0f, -1.0f,
      -1.0f, -1.0f, -1.0f,
       1.0f, -1.0f, -1.0f,
       1.0f,  1.0f, -1.0f,
    };

        private static short[] cube_indices = new short[]{
	  0, 1, 2, 3,
	  3, 2, 6, 7,
	  7, 6, 5, 4,
	  4, 5, 1, 0,
	  0, 3, 7, 4,
	  1, 2, 6, 5,
	};

        public static string cubevs = @"#version 330
in vec3 position;
out vec3 uv;

uniform mat3 view;

void main()
{
    gl_Position =  vec4(position, 1.0);  
    uv = position;
}";

        public static string cubefs = @"#version 330
in vec3 uv;

uniform samplerCube skycube;

void main()
{    
    gl_FragColor = textureCube(skycube, uv);
}";
        

        public static void RenderCubeMap(Matrix4 view)
        {
            Shader shader = Runtime.shaders["SkyBox"];
            //------------------------------------------------------
            GL.UseProgram(shader.programID);
            
            shader.enableAttrib();
            //------------------------------------------------------
            Matrix3 v = new Matrix3(view);
            GL.UniformMatrix3(shader.getAttribute("view"), false, ref v);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, RenderTools.cubeTex);
            GL.Uniform1(shader.getAttribute("skycube"), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, cubeVBO);
            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(cube_vertices.Length * sizeof(float)), cube_vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shader.getAttribute("position"), 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, cubeVAO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(cube_indices.Length * sizeof(short)), cube_indices, BufferUsageHint.StaticDraw);

            GL.DrawElements(BeginMode.Quads, cube_indices.Length, DrawElementsType.UnsignedShort, 0);
            //Console.WriteLine(GL.GetError());

            //------------------------------------------------------
            shader.disableAttrib();

            GL.UseProgram(0);
            //------------------------------------------------------
        }

        #endregion

        #region Ray Trace Controls

        public static Ray createRay(Matrix4 v, Vector2 m)
        {
            Vector4 va = Vector4.Transform(new Vector4(m.X, m.Y, -1.0f, 1.0f), v.Inverted());
            Vector4 vb = Vector4.Transform(new Vector4(m.X, m.Y, 1.0f, 1.0f), v.Inverted());

            Vector3 p1 = va.Xyz;
            Vector3 p2 = p1 - (va - (va + vb)).Xyz * 100;
            Ray r = new Ray(p1, p2);

            return r;
        }

        #endregion

        #region Shaders

        #region NUD shader


        public static string nud_vs = @"#version 330

in vec3 vPosition;
in vec4 vColor;
in vec3 vNormal;
in vec3 vTangent;
in vec3 vBiTangent;
in vec2 vUV;
in vec4 vBone;
in vec4 vWeight;

out vec3 viewPosition;
out vec3 objectPosition;
out vec3 tangent;
out vec3 bitangent;
out vec4 vertexColor;

out vec2 texcoord;
out vec2 texcoord2;
out vec2 texcoord3;
out vec2 normaltexcoord;

out vec3 normal;
out vec3 fragpos;
out vec4 vBoneOut;
out vec4 vWeightOut;
out vec4 viewNormals;

uniform vec4 colorSamplerUV;
uniform vec4 colorSampler2UV;
uniform vec4 colorSampler3UV;
uniform vec4 normalSamplerAUV;
uniform mat4 eyeview;
uniform uint flags;

uniform float zScale;
uniform vec4 zOffset;

uniform bones
{
    mat4 transforms[200];
} bones_;

uniform int renderType;


vec4 skin(vec3 po, ivec4 index);
vec3 skinNRM(vec3 po, ivec4 index);

vec4 skin(vec3 po, ivec4 index)
{
    vec4 oPos = vec4(po.xyz, 1.0);

    oPos = bones_.transforms[index.x] * vec4(po, 1.0) * vWeight.x;
    oPos += bones_.transforms[index.y] * vec4(po, 1.0) * vWeight.y;
    oPos += bones_.transforms[index.z] * vec4(po, 1.0) * vWeight.z;
    oPos += bones_.transforms[index.w] * vec4(po, 1.0) * vWeight.w;

    return oPos;
}

vec3 skinNRM(vec3 nr, ivec4 index)
{
    vec3 nrmPos = vec3(0);

    if(vWeight.x != 0.0) nrmPos = mat3(bones_.transforms[index.x]) * nr * vWeight.x;
    if(vWeight.y != 0.0) nrmPos += mat3(bones_.transforms[index.y]) * nr * vWeight.y;
    if(vWeight.z != 0.0) nrmPos += mat3(bones_.transforms[index.z]) * nr * vWeight.z;
    if(vWeight.w != 0.0) nrmPos += mat3(bones_.transforms[index.w]) * nr * vWeight.w;

    return nrmPos;
}

void main()
{
    vec4 objPos = vec4(vPosition.xyz, 1.0);

    ivec4 bi = ivec4(vBone);

    if(vBone.x != -1.0) objPos = skin(vPosition, bi);
    objectPosition = objPos.xyz;
    objPos.z *= zScale;
    objPos = eyeview * vec4(objPos.xyz, 1.0);

    gl_Position = objPos;



    texcoord = vec2((vUV * colorSamplerUV.xy) + colorSamplerUV.zw);
    normaltexcoord = vec2((vUV * normalSamplerAUV.xy) + normalSamplerAUV.zw);
    normal = vec3(0,0,0);
    tangent.xyz = vTangent.xyz;
    bitangent.xyz = vBiTangent.xyz;
    viewPosition = vec3(vPosition * mat3(eyeview));

    // calculate view space normals for sphere map rendering. animations don't change normals?
    viewNormals = vec4(vNormal.xyz, 0);
	mat4 matrixThing = transpose(inverse(eyeview));
	viewNormals = matrixThing * viewNormals;


	vBoneOut = vBone;
	vWeightOut = vWeight;

    vertexColor = vColor;


	fragpos = objPos.xyz;

	if(vBone.x != -1.0)
		normal = normalize((skinNRM(vNormal.xyz, bi)).xyz) ; //  * -1 * mat3(eyeview)
	else
		normal = vNormal ;


}";

        public static string nud_fs = @"#version 330

in vec3 viewPosition;
in vec3 objectPosition;
in vec2 texcoord;
in vec2 texcoord2;
in vec2 texcoord3;
in vec2 normaltexcoord;
in vec4 vertexColor;
in vec3 normal;
in vec3 tangent;
in vec3 bitangent;
in vec4 vBoneOut;
in vec4 vWeightOut;
in vec4 viewNormals;

out vec4 FragColor;

// Textures
uniform sampler2D dif;
uniform sampler2D dif2;
uniform sampler2D dif3;
uniform sampler2D ramp;
uniform sampler2D dummyRamp;
uniform sampler2D normalMap;
uniform sampler2D ao;
uniform samplerCube cube;
uniform samplerCube stagecube;
uniform sampler2D spheremap;
uniform samplerCube cmap;
uniform sampler2D UVTestPattern;

// flags tests
uniform int hasDif;
uniform int hasDif2;
uniform int hasDif3;
uniform int hasStage;
uniform int hasCube;
uniform int hasNrm;
uniform int hasRamp;
uniform int hasAo;
uniform int hasDummyRamp;
uniform int hasColorGainOffset;
uniform int hasSpecularParams;
uniform int useDiffuseBlend;
uniform int hasDualNormal;
uniform int hasSoftLight;
uniform int hasCustomSoftLight;

// Da Flags
uniform uint flags;

// NU_ Material Properties
uniform vec4 colorSamplerUV;
uniform vec4 normalSamplerAUV;
uniform vec4 colorOffset;
uniform vec4 minGain;
uniform vec4 fresnelColor;
uniform vec4 specularColor;
uniform vec4 specularColorGain;
uniform vec4 diffuseColor;
uniform vec4 colorGain;
uniform vec4 finalColorGain;
uniform vec4 reflectionColor;
uniform vec4 fogColor;
uniform vec4 effColorGain;
uniform vec4 zOffset;

// params
uniform vec4 fresnelParams;
uniform vec4 specularParams;
uniform vec4 reflectionParams;
uniform vec4 fogParams;
uniform vec4 normalParams;
uniform vec4 angleFadeParams;
uniform vec4 dualNormalScrollParams;
uniform vec4 alphaBlendParams;
uniform vec4 softLightingParams;
uniform vec4 customSoftLightParams;

// render settings
uniform int renderDiffuse;
uniform int renderSpecular;
uniform int renderFresnel;
uniform int renderReflection;
uniform int renderType;
uniform int renderLighting;
uniform int renderVertColor;
uniform int renderNormal;
uniform int useNormalMap;

uniform float diffuseIntensity;
uniform float ambientIntensity;
uniform float specularIntensity;
uniform float fresnelIntensity;
uniform float reflectionIntensity;

// character lighting
uniform vec3 difLightColor;
uniform vec3 ambLightColor;
uniform vec3 difLightDirection;
uniform vec3 fresGroundColor;
uniform vec3 fresSkyColor;
uniform vec3 specLightColor;
uniform vec3 specLightDirection;
uniform vec3 refLightColor;

// stage light 1
uniform int renderStageLight1;
uniform vec3 stageLight1Color;
uniform vec3 stageLight1Direction;

// stage light 2
uniform int renderStageLight2;
uniform vec3 stageLight2Color;
uniform vec3 stageLight2Direction;

// stage light 3
uniform int renderStageLight3;
uniform vec3 stageLight3Color;
uniform vec3 stageLight3Direction;

// stage light 4
uniform int renderStageLight4;
uniform vec3 stageLight4Color;
uniform vec3 stageLight4Direction;

// light_set fog
uniform int renderFog;
uniform vec3 stageFogColor;

uniform mat4 eyeview;
uniform vec3 lightPosition;
uniform vec3 lightDirection;
uniform sampler2D shadowMap;

// Constants
#define gamma 2.2
#define PI 3.14159

// Tools
vec3 RGB2HSV(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 HSV2RGB(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float Luminance(vec3 rgb)
{
    const vec3 W = vec3(0.2125, 0.7154, 0.0721);
    return dot(rgb, W);
}

vec3 CalculateTintColor(vec3 inputColor, float colorAlpha)
{
    float intensity = colorAlpha * 0.4;
    vec3 inputHSV = RGB2HSV(inputColor);
    float outSaturation = min((inputHSV.y * intensity),1); // can't have color with saturation > 1
    vec3 outColorTint = HSV2RGB(vec3(inputHSV.x,outSaturation,1));
    return outColorTint;
}

float ShadowCalculation(vec4 fragPosLightSpace)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords * 0.5 + 0.5;
    float closestDepth = texture(shadowMap, projCoords.xy).r;
    float currentDepth = projCoords.z;
    float shadow = currentDepth > closestDepth  ? 1.0 : 0.0;

    return shadow;
}

vec3 CalcBumpedNormal(vec3 inputNormal)
{
    // if no normal map, then return just the normal
    if(hasNrm == 0 || useNormalMap == 0)
	   return inputNormal;

    float normalIntensity = normalParams.x;
    vec3 BumpMapNormal = texture2D(normalMap, normaltexcoord).xyz;
    vec3 BumpMapNormal2 = texture2D(normalMap, vec2(normaltexcoord.x + dualNormalScrollParams.x, normaltexcoord.y + dualNormalScrollParams.y)).xyz;
    if(hasDualNormal == 1)
        BumpMapNormal = normalize(BumpMapNormal + BumpMapNormal2);
    BumpMapNormal = mix(vec3(0.5, 0.5, 1), BumpMapNormal, normalIntensity); // probably a better way to do this
    BumpMapNormal = 2.0 * BumpMapNormal - vec3(1);

    vec3 NewNormal;
    vec3 Normal = normalize(normal);
    mat3 TBN = mat3(tangent, bitangent, Normal);
    NewNormal = TBN * BumpMapNormal;
    NewNormal = normalize(NewNormal);

    return NewNormal;
}

vec3 ScreenBlend(vec3 base, vec3 top)
{
    return vec3(1) - (vec3(1) - base) * (vec3(1) - top);
}

vec3 RampColor(vec3 color){
	if(hasRamp == 1)
	{
		float rampInputLuminance = Luminance(color);
		rampInputLuminance = clamp((rampInputLuminance), 0.01, 0.99);
		return texture2D(ramp, vec2(1-rampInputLuminance, 0.50)).rgb;
	}
	else
		return vec3(0);
}

vec3 DummyRampColor(vec3 color){
	if(hasDummyRamp == 1)
	{
		float rampInputLuminance = Luminance(color);
		rampInputLuminance = clamp((rampInputLuminance), 0.01, 0.99);
		return texture2D(dummyRamp, vec2(1-rampInputLuminance, 0.50)).rgb;
	}
	else
		return vec3(1);
}

vec3 SpecRampColor(vec3 color){ // currently just for bayo spec ramp
	float rampInputLuminance = Luminance(color);
	rampInputLuminance = clamp((rampInputLuminance), 0.01, 0.99);

    vec3 rampContribution = texture2D(dummyRamp, vec2(1-rampInputLuminance, 0.5)).rgb;
	return rampContribution;
}

vec3 ShiftTangent(vec3 tangent, vec3 normal, float shift) // probably not needed
{
    vec3 shiftedT = tangent + shift * normal;
    return normalize(shiftedT);
}

float StrandSpecular(vec3 tangent, vec3 V, vec3 L, float exponent) // also not needed
{
    vec3 H = normalize(L + V);
    float dotTH = dot(tangent, H);
    float sinTH = sqrt(1.0 - dotTH * dotTH);
    float dirAtten = smoothstep(-1.0, 0.0, dot(tangent, H));
    return dirAtten * pow(sinTH, exponent);
}

vec3 BayoHairSpecular(vec3 diffuseMap, vec3 I, float xComponent, float yComponent)
{
    float shiftTex = diffuseMap.g; // vertical component of ramp?
    shiftTex = 0;

    float primaryShift = 0;
    float secondaryShift = 0;

    vec3 t1 = ShiftTangent(tangent.xyz, normal.xyz, primaryShift + shiftTex);
    //vec3 t2 = ShiftTangent(tangent.xyz, normal.xyz, secondaryShift + shiftTex);
    vec3 t2 = ShiftTangent(bitangent.xyz, normal.xyz, secondaryShift + shiftTex);

    float specExp1 = reflectionParams.z;
    float specExp2 = reflectionParams.w;

    vec3 hairSpecular =  vec3(1);// * StrandSpecular(t1, I, lightDirection, specExp1);
    float specMask = diffuseMap.b; // what channel should this be?
    hairSpecular += vec3(.75,.75,1) * specMask * StrandSpecular(t2, I, specLightDirection, specExp2);

    vec3 halfAngle = normalize(I + specLightDirection);
    float test = dot(t2, halfAngle)/reflectionParams.w;
    test = (test + 1) / 2;
    float test2 = diffuseMap.g;

    hairSpecular = texture2D(dummyRamp, vec2(test, test2)).rgb * diffuseMap.b * alphaBlendParams.z * 0.1; // find proper constants
    return (hairSpecular) * diffuseMap.r * 20; // find proper constants
}

vec3 SoftLighting(vec3 diffuseColorFinal, vec3 ambientIntensityLightColor, vec3 diffuseLightColor, float smoothAmount, float darkenAmount, float saturationAmount, float darkenMultiplier, float saturationMultiplier, float lambert)
{
    // blend between a certain distance from dot(L,N) = 0.5
    float edgeL = 0.5 - (smoothAmount / 2);
    float edgeR = 0.5 + (smoothAmount / 2);
    float softLight = smoothstep(edgeL, edgeR, lambert);

    // darken ambientIntensity color to counteract flat stage lighting
    float softLightDarken = max(((-darkenMultiplier * darkenAmount) + 1), 0);
    vec3 softLightAmbient = diffuseColorFinal * softLightDarken * ambientIntensityLightColor;

    // more saturated ambientIntensity color
    softLightAmbient = RGB2HSV(softLightAmbient);
    softLightAmbient = HSV2RGB(vec3(softLightAmbient.x, (softLightAmbient.y + (saturationMultiplier * saturationAmount)), softLightAmbient.z));

    vec3 softLightDiffuse = diffuseColorFinal * diffuseLightColor;

    return mix(softLightAmbient, softLightDiffuse, softLight); // custom diffuse gradient
}


vec3 sm4shShader(vec4 diffuseMap, float aoMap, vec3 N){
    vec3 I = vec3(0,0,-1) * mat3(eyeview);

    // light directions
    vec3 newFresnelDirection = lightDirection;

	//---------------------------------------------------------------------------------------------
		// diffuse pass
		vec3 diffusePass = vec3(0);

			// lambertian shading
            float lambert = clamp((dot(difLightDirection,N)), 0, 1);
			float halfLambert = clamp((dot(difLightDirection,N)), 0, 1);
            halfLambert = dot(difLightDirection, N);
            halfLambert = (halfLambert * 0.5) + 0.5; // remap [-1,1] to [0,1]. not clamped because of ramp functions
			vec3 diffuseColorFinal = vec3(0); // result of diffuse map, aoBlend, and some NU_values
			float diffuseLuminance = Luminance(diffuseMap.rgb); // used for colorGain and colorOffset

			//aoBlend = total ambientIntensity occlusion term. brightness is just addition. hue/saturation is ???
            //aoMap = pow(aoMap, gamma);
            vec3 aoBlend = vec3(1);
            aoMap = texture2D(normalMap, texcoord).a;
            float maxAOBlendValue = 1.2;
            aoBlend = min((vec3(aoMap) + minGain.rgb), vec3(maxAOBlendValue));
            vec3 aoGain = min((aoMap * (1 + minGain.rgb)), vec3(1.0));
			vec3 c1 = RGB2HSV(aoBlend);
            vec3 c2 = RGB2HSV(aoGain);
			aoBlend.rgb = HSV2RGB(vec3(c1.x, c2.y, c1.z));
            float aoMixIntensity = minGain.a;


            if (useDiffuseBlend == 1) // aomingain but no ao map (mainly for trophies)
            {
                aoMap = Luminance(diffuseMap.rgb); // should this use color? party ball item is not saturated enough
                aoMixIntensity = 0;
            }

			//---------------------------------------------------------------------------------------------
				// flags based corrections for diffuse color

    			if (hasColorGainOffset == 1) // probably a more elegant solution...
    			{
    				diffuseColorFinal = colorOffset.rgb + (vec3(Luminance(diffuseMap.rgb)) * (colorGain.rgb));
    				aoMap = diffuseLuminance;

                    if ((flags & 0x00420000u) == 0x00420000u) // bayo hair 'diffuse' is weird
                    {
                    	diffuseColorFinal = colorOffset.rgb + (diffuseMap.rrr * colorGain.rgb);
                        aoMap = 1; // don't know bayo ao yet (if it exists)

                    }
    			}

    			else // regular characters
    			{
    				diffuseColorFinal = diffuseMap.rgb * aoBlend * diffuseColor.rgb;
    			}


            //---------------------------------------------------------------------------------------------
                // stage lighting
                vec3 lighting = vec3(1);
                if ((flags & 0xF0000000u) == 0xA0000000u) // stage lighting. should actually be based on xmb in future.
                {
                    // should this be half lambert?
                    vec3 stageLight1 = stageLight1Color * max((dot(N, stageLight1Direction)), 0);
                    vec3 stageLight2 = stageLight2Color * max((dot(N, stageLight2Direction)), 0);
                    vec3 stageLight3 = stageLight3Color * max((dot(N, stageLight3Direction)), 0);
                    vec3 stageLight4 = stageLight4Color * max((dot(N, stageLight4Direction)), 0);
                    lighting = (stageLight1 * renderStageLight1) + (stageLight2 * renderStageLight2) + (stageLight3 * renderStageLight3) + (stageLight4 * renderStageLight4);

                    // area lights experiment
                    float distanceMixFactor = distance(objectPosition.xyz, vec3(-50, 50, 50));
                    distanceMixFactor *= 0.01;
                    distanceMixFactor = pow(distanceMixFactor, 2);
                    lighting = mix(vec3(0), vec3(0.5), distanceMixFactor);
                }
                else // character lighting
                    lighting = mix(ambLightColor * ambientIntensity, difLightColor * diffuseIntensity, halfLambert); // gradient based lighting

                diffusePass = diffuseColorFinal * lighting;
            //---------------------------------------------------------------------------------------------

            if (hasSoftLight == 1)
                diffusePass = SoftLighting(diffuseColorFinal, ambLightColor, difLightColor, softLightingParams.z, softLightingParams.y, softLightingParams.x, 0.3, 0.0561, lambert);
            else if (hasCustomSoftLight == 1)
                diffusePass = SoftLighting(diffuseColorFinal, ambLightColor, difLightColor, customSoftLightParams.z, customSoftLightParams.y, customSoftLightParams.x, 0.3, 0.114, lambert);

            if ((flags & 0x00FF0000u) == 0x00810000u || (flags & 0xFFFF0000u) == 0xFA600000u) // used with softlightingparams and customSoftLightParams, respectively
                diffusePass *= 1.5;

            // result of ramp calculations isn't saturated enough
            vec3 rampContribution = 0.5 * pow((RampColor(vec3(halfLambert)) * DummyRampColor(vec3(halfLambert)) * diffuseColorFinal), vec3(gamma));
            diffusePass = ScreenBlend(diffusePass, rampContribution) * diffuseIntensity;

		diffusePass = pow(diffusePass, vec3(gamma));
	//---------------------------------------------------------------------------------------------

	//---------------------------------------------------------------------------------------------
		// Calculate the color tint. input colors are r,g,b,alpha. alpha is color blend amount
		vec3 spec_tint = CalculateTintColor(diffuseColorFinal, specularColor.a);
		vec3 fresnel_tint = CalculateTintColor(diffuseColorFinal, fresnelColor.a);
		vec3 reflection_tint = CalculateTintColor(diffuseColorFinal, reflectionColor.a);
	//---------------------------------------------------------------------------------------------

	//---------------------------------------------------------------------------------------------
		// specular pass
		vec3 specularPass = vec3(0);
            //---------------------------------------------------------------------------------------------
		        // blinn phong with anisotropy
    			vec3 halfAngle = normalize(specLightDirection + I);
                // tangent plane vectors
                vec3 X = normalize(tangent);
                vec3 Y = normalize(bitangent);
                // ax: anisotropic width. ay: anisotropic height
                float ax = reflectionParams.z;
                float ay = reflectionParams.w;
                float xComponent = max(pow((dot(halfAngle, X) / ax), 2), 0);
                float yComponent = max(pow((dot(halfAngle, Y) / ay), 2), 0);

                // only use anisotropy for mats without specularparams
                float exponent = xComponent + yComponent;
                if (hasSpecularParams == 1)
                    exponent = specularParams.y;

            float blinnPhongSpec = pow(clamp((dot(halfAngle, N)), 0, 1), exponent);
            //---------------------------------------------------------------------------------------------

			//---------------------------------------------------------------------------------------------
				// flags based corrections for specular
			if ((flags & 0x00FF0000u) == 0x00420000u) // bayo hair mats. rework this section
			{
                specularPass = BayoHairSpecular(diffuseMap.rgb, I, xComponent, yComponent);

			}

			else if (hasColorGainOffset == 1) // how does specularColorGain work?
            {
                specularPass += specularColor.rgb * blinnPhongSpec * (specularColorGain.rgb);
            }
            else if ((flags & 0x00E10000u) == 0x00E10000u) // not sure how this works. specular works differently for eye mats
            {
                specularPass += specularColor.rgb * blinnPhongSpec * spec_tint * 0;
            }
			else // default
            {
                specularPass += specularColor.rgb * blinnPhongSpec * spec_tint;
                // if (hasRamp == 1) // do ramps affect specular?
                    // specularPass = RampColor(specularPass);
            }

			//---------------------------------------------------------------------------------------------
            specularPass *= mix(aoMap, 1, aoMixIntensity);
            specularPass *= specLightColor;
            specularPass *= specularIntensity;

		specularPass = pow(specularPass, vec3(gamma));
	//---------------------------------------------------------------------------------------------

	//---------------------------------------------------------------------------------------------
		// fresnel pass
		vec3 fresnelPass = vec3(0);

			// hemisphere fresnel with fresnelParams control
            vec3 upVector = vec3(0, 1, 0);
			float hemiBlend = dot(N, upVector);
			hemiBlend = (hemiBlend * 0.5) + 0.5; // remap [-1, 1] to [0, 1]
            hemiBlend = pow(hemiBlend, 2); // use more ground color
			vec3 hemiColor = mix(fresGroundColor, fresSkyColor, hemiBlend);
			float cosTheta = dot(I, N);
            float exponentOffset = 2.75;
			vec3 fresnel = clamp((hemiColor * pow(1.0 - cosTheta, exponentOffset + fresnelParams.x)), 0, 1);
			fresnelPass += fresnelColor.rgb * fresnel * fresnelIntensity * fresnel_tint;

            //if((flags & 0x00120000u) == 0x00120000u)
                //fresnelPass *= mix(vec3(1),aoMap.rgb,fresnelParams.w);
            if ((flags & 0x0000FF00u) == 0x00003000u)
                fresnelPass *= diffuseMap.rgb;
		fresnelPass = pow(fresnelPass, vec3(1));
	//---------------------------------------------------------------------------------------------

	//---------------------------------------------------------------------------------------------
		// reflection pass
		vec3 reflectionPass = vec3(0);

			// cubemap reflection
			vec3 R = reflect(I, N);
			R.y *= -1.0;
			vec3 stageCubeColor = texture(stagecube, R).rgb;

			//---------------------------------------------------------------------------------------------
				// flags based corrections for reflection
			if (hasCube == 1) // cubemaps from model.nut. currently just uses miiverse cubemap
				reflectionPass += diffuseMap.aaa * stageCubeColor * reflection_tint * reflectionParams.x;

			if ((flags & 0x00000010u) == 0x00000010u) // view-based sphere mapping (rosa's stars, sonic eyes, etc)
			{
				// calculate UVs based on view space normals (UV scale is currently not right)
				//float PI = 3.14159;
				float uCoord = asin(viewNormals.x) / PI + 0.5;
				float vCoord = asin(viewNormals.y) / PI + 0.5;

                // transform UVs to fix scaling. values were chosen just to look correct
                float sphereTexcoordOffset = -0.7;
                float sphereTexcoordScale = 2.4;
                uCoord = uCoord * sphereTexcoordScale + sphereTexcoordOffset;
                vCoord = vCoord * sphereTexcoordScale + sphereTexcoordOffset;

				vec2 sphereTexcoord = vec2(uCoord + 0.05, (1 - vCoord) + 0.05); // 0.05?
				vec3 sphereMapColor = texture2D(spheremap, sphereTexcoord).xyz;
				reflectionPass += sphereMapColor * reflectionColor.xyz * reflection_tint;
			}

			else // stage cubemaps
				reflectionPass += reflectionColor.rgb * stageCubeColor * reflection_tint * diffuseMap.aaa;
			//---------------------------------------------------------------------------------------------

            reflectionPass *= mix(aoMap, 1, aoMixIntensity);
            reflectionPass *= refLightColor;
            reflectionPass *= reflectionIntensity;

		reflectionPass = pow(reflectionPass, vec3(gamma));
	//---------------------------------------------------------------------------------------------

	vec3 resultingColor = vec3(0);

	if(renderLighting == 1)
	{
		resultingColor += diffusePass * renderDiffuse;
		resultingColor += fresnelPass * renderFresnel;
		resultingColor += specularPass * renderSpecular;
		resultingColor += reflectionPass * renderReflection;
	}
	else
		resultingColor = diffusePass;

    resultingColor = pow(resultingColor, vec3(1 / gamma)); // gamma correction. gamma correction also done within render passes

    // light_set fog calculations
    float depth = viewPosition.z;
    depth = min((depth / fogParams.y),1);
    float fogIntensity = mix(fogParams.z, fogParams.w, depth);
    if(renderFog == 1)
        resultingColor = mix(resultingColor, stageFogColor, fogIntensity);


    return resultingColor;
}

void main()
{
    FragColor = vec4(0,0,0,1);
    vec3 bumpMapNormal = CalcBumpedNormal(normal);
    vec3 displayNormal = (bumpMapNormal * 0.5) + 0.5; // remap [-1,1] to [0,1]
    vec3 displayTangent = (tangent * 0.5) + 0.5; // remap [-1,1] to [0,1]
    vec3 displayBitangent = (bitangent * 0.5) + 0.5; // remap [-1,1] to [0,1]

    // zOffset correction
    gl_FragDepth = gl_FragCoord.z - (zOffset.x / 1500); // divide by far plane?

    // if the renderer wants to show something other than textures
    if (renderType == 1) // normals color
        FragColor = vec4(displayNormal,1);
    else if (renderType == 2) // normals black & white
    {
        float normal = dot(vec4(bumpMapNormal * mat3(eyeview), 1.0), vec4(vec3(0.15), 1.0));
        FragColor.rgb = vec3(normal);
    }
    else if (renderType == 3) // normal map
        FragColor = vec4(pow(texture2D(normalMap, texcoord).rgb, vec3(1)), 1);
    else if (renderType == 4) // vertexColor
        FragColor = vertexColor;
    else if (renderType == 5) // ambientIntensity occlusion
        FragColor = vec4(pow(texture2D(normalMap, texcoord).aaa, vec3(1)), 1);
    else if (renderType == 6) // uv coords
		FragColor = vec4(texcoord.x, texcoord.y, 1, 1);
    else if (renderType == 7) // uv test pattern
        FragColor = vec4(texture2D(UVTestPattern, texcoord).rgb, 1);
    else if (renderType == 8) // tangents
        FragColor = vec4(displayTangent, 1);
    else if (renderType == 9) // bitangents
        FragColor = vec4(displayBitangent, 1);
	else
    {

        // similar to du dv but uses just the normal map
        float offsetIntensity = 0;
        if(useNormalMap == 1)
            offsetIntensity = normalParams.z;
        vec2 textureOffset = 1-texture2D(normalMap, normaltexcoord).xy;
        textureOffset = (textureOffset * 2) -1; // remap to -1 to 1?
        vec2 offsetTexCoord = texcoord + (textureOffset * offsetIntensity);

        vec4 diffuse1 = vec4(0);
        vec4 diffuse2 = vec4(0);
        vec4 diffuse3 = texture2D(dif3, texcoord);
        if (hasDif == 1) // 1st diffuse texture
        {
            diffuse1 = texture2D(dif, offsetTexCoord);
            FragColor = diffuse1;

            if (hasDif2 == 1 && hasDif3 != 1) // 2nd diffuse texture. doesn't work properly with stages
            {
                diffuse2 = texture2D(dif2, offsetTexCoord);
                FragColor = mix(diffuse2, diffuse1, diffuse1.a);
                FragColor.a = 1.0;
            }

        }
        else
            FragColor = vec4(diffuseColor);

        vec3 diffuse2Texture = texture2D(ao, offsetTexCoord).rgb;

        if (hasAo == 1) // should be named differently. isn't always two diffuse
            FragColor.rgb = mix(diffuse2Texture, FragColor.rgb, ((normal.y + 1) / 2));

        // calcuate final color by mixing with vertex color
        if (renderVertColor == 1) FragColor *= vertexColor;

        // Material lighting done in sm4sh shader
        if (renderNormal == 1)
            FragColor.rgb = sm4shShader(FragColor, texture2D(normalMap, texcoord).a, bumpMapNormal);
        FragColor.rgb *= finalColorGain.rgb;
        FragColor.rgb *= effColorGain.rgb;


        //FragColor.rgb = mix(diffuse2.rgb, diffuse1.rgb, vertexColor.r); // umbraf stage vertex color blending
        //FragColor.rgb *= mix(diffuse3.rgb, vec3(1), 0.5);

        //---------------------------------------------------------------------------------------------
            // correct alpha

        FragColor.a *= finalColorGain.a;
        FragColor.a *= effColorGain.a;

        // angleFadeParams alpha correction
        float normalFadeAmount = angleFadeParams.x;
        float edgeFadeAmount = angleFadeParams.y;
        vec3 I = vec3(0,0,-1) * mat3(eyeview);
        float fresnelBlend = 1-dot(I, bumpMapNormal);
        float angleFadeAmount = mix(normalFadeAmount, edgeFadeAmount, fresnelBlend);
        angleFadeAmount = max((1 - angleFadeAmount), 0);
        FragColor.a *= angleFadeAmount;

        // if ((flags & 0xF0FF0000u) != 0xF0640000u) // ryu works differently. need to research this more
        FragColor.a += alphaBlendParams.x;
        //---------------------------------------------------------------------------------------------

	}

}";

        #endregion

        #region point shader
        public static string vs_Point = @"#version 330

in vec3 vPosition;
in vec4 vBone;
in vec4 vWeight;
in int vSelected;

flat out int selected;

uniform mat4 eyeview;
uniform bones
{
    mat4 transforms[200];
} bones_;

vec4 skin(vec3 po, ivec4 index)
{
    vec4 oPos = vec4(po.xyz, 1.0);

    oPos = bones_.transforms[index.x] * vec4(po, 1.0) * vWeight.x;
    oPos += bones_.transforms[index.y] * vec4(po, 1.0) * vWeight.y;
    oPos += bones_.transforms[index.z] * vec4(po, 1.0) * vWeight.z;
    oPos += bones_.transforms[index.w] * vec4(po, 1.0) * vWeight.w;
    
    selected = vSelected;

    return oPos;
}

void main()
{
    vec4 objPos = vec4(vPosition.xyz, 1.0);
    ivec4 bi = ivec4(vBone); 

    if(vBone.x != -1.0) objPos = skin(vPosition, bi);

    selected = vSelected;

    gl_Position = eyeview * vec4(objPos.xyz, 1.0f);
}
";
        public static string fs_Point = @"#version 330

flat in int selected;

void main()
{
    if(selected == 0)
        gl_FragColor = vec4(1,1,1,1);
    else if(selected > 0)
        gl_FragColor = vec4(1,1,0,1);
    else
        gl_FragColor = vec4(1,0,1,1);
}
";

        #endregion

        #region Shadow Shader

        public static string vs_Shadow = @"#version 330
in vec3 vPosition;

uniform mat4 lightSpaceMatrix;

void main()
{
    gl_Position = lightSpaceMatrix * vec4(vPosition, 1.0f);
}";
        public static string fs_Shadow = @"#version 330

void main()
{             
    gl_FragDepth = gl_FragCoord.z;
gl_FragColor = vec4(1);
}  ";


        #endregion


        #region texture shader
        public static string texture_vs = @"#version 330

out vec2 texCoord;
 
void main()
{
    float x = -1.0 + float((gl_VertexID & 1) << 2);
    float y = -1.0 + float((gl_VertexID & 2) << 1);
    texCoord.x = (x+1.0)*0.5;
    texCoord.y = (y+1.0)*0.5;
    gl_Position = vec4(x, y, 0, 1);
}";

        public static string texture_fs = @"#version 330
in vec2 texCoord;

uniform sampler2D texture;

uniform int renderR;
uniform int renderG;
uniform int renderB;
uniform int renderAlpha;
uniform int alphaOverride;

out vec4 outColor;

void main()
{   outColor = vec4(0,0,0,1);    
    vec4 textureColor = texture2D(texture, vec2(texCoord.x, 1-texCoord.y)).rgba;
    if (renderR == 1)
        outColor.r = textureColor.r;
    if (renderG == 1)
        outColor.g = textureColor.g;
    if (renderB == 1)
        outColor.b = textureColor.b;
    if (renderAlpha == 1)
        outColor.a = textureColor.a;
    if (alphaOverride == 1)
        outColor = vec4(textureColor.aaa, 1);
        
}";

        #endregion

        #endregion

    }
}

