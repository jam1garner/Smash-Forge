using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;

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

out vec3 pos;
out vec3 tan;
out vec3 bit;
out vec4 color;
out vec2 texcoord;
out vec3 normal;
out vec3 fragpos;
out vec4 vBoneOut;
out vec4 vWeightOut;
out vec4 viewNormals;


uniform vec4 colorSamplerUV;
uniform mat4 eyeview;
uniform uint flags;


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

    objPos = eyeview * vec4(objPos.xyz, 1.0);

    gl_Position = objPos;

    texcoord = vec2((vUV * colorSamplerUV.xy) + colorSamplerUV.zw);
    normal = vec3(0,0,0);
    tan = vTangent;
    bit = vBiTangent;
    color = vec4(vNormal, 1);
    pos = vec3(vPosition * mat3(eyeview));

    // calculate view space normals for sphere map rendering. animations don't change normals?
    viewNormals = vec4(vNormal.xyz, 0);
	mat4 matrixThing = transpose(inverse(eyeview));
	viewNormals = matrixThing * viewNormals;


	vBoneOut = vBone;
	vWeightOut = vWeight;

    if(renderType != 1){

	if(renderType == 2){
        	float normal = dot(vec4(vNormal * mat3(eyeview), 1.0), vec4(0.15,0.15,0.15,1.0)) ;
        	color = vec4(normal, normal, normal, 1);
	}
        else
            color = vColor;

	fragpos = objPos.xyz;

	if(vBone.x != -1.0)
		normal = normalize((skinNRM(vNormal.xyz, bi)).xyz) ; //  * -1 * mat3(eyeview)
	else
		normal = vNormal ;
    }
}";

        public static string nud_fs = @"#version 330

in vec3 pos;
in vec2 texcoord;
in vec4 color;
in vec3 normal;
in vec3 tan;
in vec3 bit;
in vec4 vBoneOut;
in vec4 vWeightOut;
in vec4 viewNormals;

out vec4 fincol;

//uniform boneIndex;

// Textures
uniform sampler2D dif;
uniform sampler2D dif2;
uniform sampler2D ramp;
uniform sampler2D dummyRamp;
uniform sampler2D nrm;
uniform sampler2D ao;
uniform samplerCube cube;
uniform samplerCube stagecube;
uniform sampler2D spheremap;
uniform samplerCube cmap;
uniform sampler2D UVTestPattern;

// flags tests
uniform int hasDif;
uniform int hasDif2;
uniform int hasStage;
uniform int hasCube;
uniform int hasOo;
uniform int hasNrm;
uniform int hasRamp;
uniform int hasDummyRamp;
uniform int hasColorGainOffset;
uniform int hasSpecularParams;

// Da Flags
uniform uint flags;
uniform int isTransparent;

// Properties
uniform vec4 colorSamplerUV;
uniform vec4 colorOffset;
uniform vec4 minGain;

uniform vec4 fresnelColor;
uniform vec4 specularColor;
uniform vec4 specularColorGain;
uniform vec4 diffuseColor;
uniform vec4 colorGain;
uniform vec4 finalColorGain;
uniform vec4 reflectionColor;

// params
uniform vec4 fresnelParams;
uniform vec4 specularParams;
uniform vec4 reflectionParams;

uniform int renderType;
uniform int renderLighting;
uniform int renderVertColor;
uniform int renderNormal;

// render settings
uniform int renderDiffuse;
uniform int renderSpecular;
uniform int renderFresnel;
uniform int renderReflection;
uniform float diffuse_intensity;
uniform float ambient;
uniform float specular_intensity;
uniform float fresnel_intensity;
uniform float reflection_intensity;

uniform int useNormalMap;

uniform mat4 eyeview;

uniform vec3 lightPosition;
uniform vec3 lightDirection;
uniform vec3 freslightDirection;

uniform sampler2D shadowMap;


// Tools
vec3 rgb2hsv(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c)
{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float luminance(vec3 rgb)
{
    const vec3 W = vec3(0.2125, 0.7154, 0.0721);
    return dot(rgb, W);
}

vec3 calculate_tint_color(vec3 inputColor, float color_alpha, float blendAmount) // needs some work
{
    float intensity = color_alpha*blendAmount;
    vec3 inputHSV = rgb2hsv(inputColor);
    float outSaturation = min((inputHSV.y * intensity),1); // can't have color with saturation > 1
    vec3 outColorTint = hsv2rgb(vec3(inputHSV.x,outSaturation,1));
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

vec3 CalcBumpedNormal()
{
    // if no normal map, then return just the normal
    if(hasNrm == 0 || useNormalMap == 0)
	return normal;

    vec3 Normal = normalize(normal);

    vec3 Tangent = tan;
    vec3 Bitangent = bit;
    vec3 BumpMapNormal = texture2D(nrm, texcoord).xyz;
    BumpMapNormal = 2.0 * BumpMapNormal - vec3(1.0, 1.0, 1.0);
    vec3 NewNormal;
    mat3 TBN = mat3(Tangent, Bitangent, Normal);
    NewNormal = TBN * BumpMapNormal;
    NewNormal = normalize(NewNormal);
    return NewNormal;
}


vec3 ScreenBlend(vec3 base, vec3 top)
{
    return vec3(1) - (vec3(1) - base) * (vec3(1) - top);
}



vec3 RampColor(vec3 col){
	if(hasRamp == 1)
	{
		float rampInputLuminance = luminance(col);
		rampInputLuminance = clamp((rampInputLuminance), 0.01, 0.99);
		return texture2D(ramp, vec2(1-rampInputLuminance, 0.50)).rgb;
	}
	else
		return vec3(0);
}

vec3 DummyRampColor(vec3 col){
	if(hasDummyRamp == 1)
	{
		float rampInputLuminance = luminance(col);
		rampInputLuminance = clamp((rampInputLuminance), 0.01, 0.99);
		return texture2D(dummyRamp, vec2(1-rampInputLuminance, 0.50)).rgb;
	}
	else
		return vec3(1);
}

vec3 SpecRampColor(vec3 col){ // currently just for bayo spec ramp
	float rampInputLuminance = luminance(col);
	rampInputLuminance = clamp((rampInputLuminance), 0.01, 0.99);
	return texture2D(dummyRamp, vec2(1-rampInputLuminance, 0.5)).rgb;
}


vec3 sm4sh_shader(vec4 diffuse_map, vec4 ao_map, vec3 N){
    vec3 I = vec3(0,0,-1) * mat3(eyeview);
    float blendAmount = 1;
	float fresnelBlendAmount = 0.5;
    float specularBlendAmount = 0.5;
    float reflectionBlendAmount = 0.5;

    // light direction seems to be (z,x,y) from light_set_param
    vec3 newDiffuseDirection = lightDirection; //normalize(I + diffuseLightDirection);
    vec3 newFresnelDirection = freslightDirection; //normalize(I + fresnelLightDirection);
    vec3 newSpecularDirection = lightDirection; //normalize(I + specularLightDirection);

	//---------------------------------------------------------------------------------------------
		// diffuse pass
		vec3 diffuse_pass = vec3(0);

			// lambertian shading
			float halfLambert = clamp((dot(newDiffuseDirection,N)),0,1);
            halfLambert = dot(newDiffuseDirection,N);
            halfLambert = (halfLambert + 1) / 2; // remap [-1,1] to [0,1]. not clamped because of ramp functions
			vec3 diffuse_color = vec3(0);
			vec3 diffuse_shading = vec3(0);
			vec3 ao_blend = vec3(1);
			float diffuse_luminance = luminance(diffuse_map.rgb);

            //ramp_contribution *= 0.25;
            //ramp_contribution = vec3(halfLambert);
			//ao_blend = total ambient occlusion term. desaturated to look correct
			ao_blend = min((ao_map.aaa + (minGain.rgb)),1.5);
			vec3 c = rgb2hsv(ao_blend.rgb);
			ao_blend.rgb = hsv2rgb(vec3(c.x, c.y*0.75, c.z));

            vec3 ambientLightColor = vec3(1) * ambient;
            vec3 diffuseLightColor = vec3(1) * diffuse_intensity;
            //diffuseLightColor *= RampColor(vec3(halfLambert));



			//---------------------------------------------------------------------------------------------
				// flags based corrections for diffuse

    			if (hasColorGainOffset == 1) // probably a more elegant solution...
    			{
    				diffuse_color = colorOffset.rgb + (vec3(luminance(diffuse_map.rgb)) * (colorGain.rgb));
    				ao_map.rgb = vec3(diffuse_luminance);
    				//diffuse_shading = diffuse_color * RampColor(vec3(halfLambert));
    			//	diffuse_pass = (diffuse_color * ambient) + (diffuse_shading * diffuse_intensity);

                    if ((flags & 0x00420000u) == 0x00420000u) // bayo hair 'diffuse' is weird
                    {
                    	diffuse_color = colorOffset.rgb + (diffuse_map.rrr * colorGain.rgb);
                        ao_map.rgb = vec3(1); // don't know bayo ao yet (if it exists)
                        //diffuse_pass = diffuse_color;// * ao_blend;
                    }
    			}

    			else // regular characters
    			{
    				diffuse_color = diffuse_map.rgb * ao_blend * diffuseColor.rgb;
    			//	diffuse_shading = diffuse_map.rgb * ao_blend * ramp_contribution;
    				//diffuse_pass = (diffuse_color * ambient) + (diffuse_shading * diffuse_intensity);
    			}
                diffuse_pass = mix(ambientLightColor, diffuseLightColor, halfLambert) * diffuse_color; // gradient based lighting

                vec3 ramp_contribution = 0.5 * pow((RampColor(vec3(halfLambert)) * DummyRampColor(vec3(halfLambert)) * diffuse_color), vec3(2.2));
                diffuse_pass = ScreenBlend(diffuse_pass, ramp_contribution);

		diffuse_pass = pow(diffuse_pass, vec3(2.2));


	//---------------------------------------------------------------------------------------------

	//---------------------------------------------------------------------------------------------
		// Calculate the color tint. input colors are r,g,b,alpha. alpha is color blend amount
		vec3 spec_tint = calculate_tint_color(diffuse_color, specularColor.a, specularBlendAmount);
		vec3 fresnel_tint = calculate_tint_color(diffuse_color, fresnelColor.a, fresnelBlendAmount);
		vec3 reflection_tint = calculate_tint_color(diffuse_color, reflectionColor.a, reflectionBlendAmount);
	//---------------------------------------------------------------------------------------------

	//---------------------------------------------------------------------------------------------
		// specular pass
		vec3 specular_pass = vec3(0);
            //---------------------------------------------------------------------------------------------
		        // blinn phong with anisotropy
    			vec3 half_angle = normalize(newSpecularDirection+I);
                // tangent plane vectors
                vec3 X = normalize(tan.xyz);
                vec3 Y = normalize(bit.xyz);
                // ax: anisotropic width. ay: anisotropic height
                float ax = reflectionParams.z;
                float ay = reflectionParams.w;
                float xComponent = max(pow((dot(half_angle, X)/ax),2),0);
                float yComponent = max(pow((dot(half_angle, Y)/ay),2),0);
                // only use anisotropy for mats without specularparams
                float exponent = xComponent + yComponent;
                if (hasSpecularParams == 1)
                    exponent = specularParams.y;

            float blinnPhongSpec = pow(clamp((dot(half_angle, N)), 0, 1), exponent);
            //---------------------------------------------------------------------------------------------


			//---------------------------------------------------------------------------------------------
				// flags based corrections for specular
			if ((flags & 0x00420000u) == 0x00420000u) // bayo hair mats
			{
			vec3 specularContribution = blinnPhongSpec * spec_tint;// * specular_intensity;
			specularContribution = SpecRampColor(specularContribution); // spec ramp
			specularContribution *= diffuse_map.bbb*1.35;
			specular_pass += specularContribution;
			}

			else if (hasColorGainOffset == 1) // Color Gain/Offset
            {
			    //specular_pass += specularColor.rgb * blinnPhongSpec * spec_tint * specular_intensity * (specularColorGain.rgb);
                specular_pass += specularColor.rgb * blinnPhongSpec * (specularColorGain.rgb) * specular_intensity;
            }
            else if ((flags & 0x00E10000u) == 0x00E10000u) // not sure how this works. specular works differently for eye mats
            {
                specular_pass += specularColor.rgb * blinnPhongSpec * spec_tint * specular_intensity * 0;
            }
			else // default
            {
                specular_pass += specularColor.rgb * blinnPhongSpec * spec_tint * specular_intensity;
                //if (hasRamp == 1) // do ramps affect specular?
                //    specular_pass = RampColor(specular_pass);
            }

			//---------------------------------------------------------------------------------------------
            specular_pass *= mix(ao_map.rgb, vec3(1), minGain.a);

		specular_pass = pow(specular_pass, vec3(2.2));
	//---------------------------------------------------------------------------------------------

	//---------------------------------------------------------------------------------------------
		// fresnel pass
		vec3 fresnel_pass = vec3(0);

			// hemisphere fresnel with fresnelParams control
			vec3 groundColor = vec3(0);
			vec3 skyColor = vec3(1);
			float hemiBlend = dot(N, vec3(0,1,0));
			hemiBlend = (hemiBlend * 0.5) + 0.5;
            hemiBlend = pow(hemiBlend, 2); // use more ground color
			vec3 hemiColor = mix(groundColor, skyColor, hemiBlend);
			float cosTheta = dot(I, N);
			vec3 fresnel = clamp((hemiColor * pow(1.0 - cosTheta, 2.75 + fresnelParams.x)), 0, 1);
			fresnel_pass += fresnelColor.rgb * fresnel * fresnel_intensity * fresnel_tint;

		fresnel_pass = pow(fresnel_pass, vec3(1));
        //fresnel_pass = hemiColor;
	//---------------------------------------------------------------------------------------------

	//---------------------------------------------------------------------------------------------
		// reflection pass
		vec3 reflection_pass = vec3(0);

			// cubemap reflection
			vec3 R = reflect(I, N);
			R.y *= -1.0;
			vec3 refColor = texture(stagecube, R).rgb;

			//---------------------------------------------------------------------------------------------
				// flags based corrections for reflection
			if (hasCube == 1) // cubemaps from model.nut. currently just uses miiverse cubemap
				reflection_pass += diffuse_map.aaa* refColor * reflection_tint* reflection_intensity;

			if ((flags & 0x00000010u) == 0x00000010u) // view-based sphere mapping (rosa's stars, sonic eyes, etc)
			{
				// calculate UVs based on view space normals (UV scale is currently not right)
				float PI = 3.14159;
				float uCoord = asin(viewNormals.x)/ PI + 0.5;
				float vCoord = asin(viewNormals.y)/PI + 0.5;

                // transform UVs to fix scaling. values are abitrary
                uCoord = uCoord * 2.4 - 0.7;
                vCoord = vCoord * 2.4 - 0.7;

				vec2 newTexCoord = vec2((uCoord)+0.05,(1-vCoord)+0.05);
				vec3 weirdReflection = texture2D(spheremap, newTexCoord).xyz;
				reflection_pass += weirdReflection*reflectionColor.xyz*reflection_tint*reflection_intensity;
			}
			else // stage cubemaps
				reflection_pass += reflectionColor.rgb* refColor * reflection_tint* reflection_intensity* diffuse_map.aaa;
			//---------------------------------------------------------------------------------------------

            reflection_pass *= mix(ao_map.rgb, vec3(1), minGain.a);

		reflection_pass = pow(reflection_pass, vec3(2.2));
	//---------------------------------------------------------------------------------------------

	vec3 resulting_color = vec3(0);

	if(renderLighting == 1)
	{
		if (renderDiffuse == 1)
			resulting_color += diffuse_pass;
		if (renderFresnel == 1)
			resulting_color += fresnel_pass;
		if (renderSpecular == 1)
			resulting_color += specular_pass;
		if (renderReflection == 1)
			resulting_color += reflection_pass;
	}
	else
		resulting_color = diffuse_pass;


    resulting_color = pow(resulting_color, vec3(1/2.2)); // gamma correction. gamma correction also done within render passes


    return resulting_color;
}

void
main()
{
    // if the renderer wants to show something other than textures
    if (renderType == 3) // normal map
    {
        fincol = vec4(pow(texture2D(nrm, texcoord).rgb, vec3(1)), 1);
    }
    else if (renderType == 5) // ambient occlusion
    {
        fincol = vec4(pow(texture2D(nrm, texcoord).aaa, vec3(1/2.2)), 1);
    }
    else if (renderType == 1 || renderType == 2 || renderType == 4 || renderType == 5) // other stuff
    {
        fincol = color;
    }
    else if (renderType == 6) // uv coords
	{
		fincol = vec4(texcoord.x, texcoord.y, 1, 1);
	}
    else if (renderType == 7)
    {
        fincol = vec4(texture2D(UVTestPattern, texcoord).rgb, 1);
    }

	else
    {
        fincol = vec4(0);
        vec4 fc = vec4(0, 0, 0, 0);

        if (hasDif == 1) // 1st diffuse texture
        {
            fc = texture2D(dif, texcoord);
            fincol = fc;
            if (hasDif2 == 1) // 2nd diffuse texture
            {
                fincol = texture2D(dif2, texcoord);
                fincol = mix(fincol, fc, fc.a);
                fincol.a = 1.0;
            }
        }

        // calcuate final color by mixing with vertex color
        if (renderVertColor == 1) fincol *= color;
        vec3 norm = CalcBumpedNormal();

        // Material lighting done in sm4sh shader
        if (renderNormal == 1)
          fincol.rgb = sm4sh_shader(fincol, texture2D(nrm, texcoord).aaaa, norm);
        fincol = fincol * (finalColorGain);

        // correct alpha
        float a = fincol.a;
        fincol.a = a;
        fincol.a *= finalColorGain.a;


		//if ((flags & 0x00420000u) == 0x00420000u)
			//fincol.a *= diffuse_map.w;

       // fincol.xyz *= vec3(ShadowCalculation(lightPos));
        //gl_FragColor = fincol; // final output color
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

        #endregion

    }
}

