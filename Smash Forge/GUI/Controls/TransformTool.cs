using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Smash_Forge
{
    public class TransformTool
    {
        public Bone b;
        public bool hit = false;

        public TransformTool()
        {
        }

        public void Render(Matrix4 view)
        {
            if (b == null) return;

            Matrix4 mat = b.transform;
            Vector3 center = Vector3.Transform(Vector3.Zero, mat);
            Vector3 camPoint = Vector3.Transform(Vector3.Zero, view);

            Matrix4 rot = mat;
            float Radius = CamDistance(center, camPoint);// / 1 * (Runtime.fov / 45.0f) * 1.0f;



            Vector3 lineStart = VBNViewport.p1;
            Vector3 lineEnd = VBNViewport.p2;
            Vector3 normal = new Vector3(0);
            Matrix4 invTrasnform = mat.Inverted();

            Vector3 point;
            Console.WriteLine(LineSphereIntersect(VBNViewport.p1, VBNViewport.p2, center, Radius, out point));
            float Distance = CamDistance(point, center);

            hit = false;
            bool _hiX = false;
            bool _hiY = false;
            bool _hiZ = false;
            Console.WriteLine(Distance + " " + Radius);
            if (Math.Abs(Distance - Radius) < (Radius * 3))
            {
                hit = true;

                
                Vector3 angle = Angles(Vector3.Transform(point, invTrasnform)) * new Vector3(180f / (float)Math.PI);
                angle.X = Math.Abs(angle.X);
                angle.Y = Math.Abs(angle.Y);
                angle.Z = Math.Abs(angle.Z);
                //Console.WriteLine(angle.ToString());

                float _axisSnapRange = 7f;
                if (Math.Abs(angle.Y - 90.0f) <= _axisSnapRange)
                    _hiX = true;
                else if (angle.X >= (180.0f - _axisSnapRange) || angle.X <= _axisSnapRange)
                    _hiY = true;
                else if (angle.Y >= (180.0f - _axisSnapRange) || angle.Y <= _axisSnapRange)
                    _hiZ = true;
            }
        
            GL.PushMatrix();
            GL.MultMatrix(ref mat);
            
            GL.Color3(_hiX ? Color.Yellow : Color.Green);
            GL.LineWidth(2);
            RenderTools.drawCircleOutline(Vector3.Zero, 2, 25);

            GL.Rotate(90.0f, 0.0f, 1.0f, 0.0f);

            GL.Color3(_hiY ? Color.Yellow : Color.Red);
            RenderTools.drawCircleOutline(Vector3.Zero, 2, 25);

            GL.Rotate(90.0f, 1.0f, 0.0f, 0.0f);

            GL.Color3(_hiZ ? Color.Yellow : Color.Blue);
            RenderTools.drawCircleOutline(Vector3.Zero, 2, 25);
            
            GL.PopMatrix();
        }

        public static bool LineSphereIntersect(Vector3 start, Vector3 end, Vector3 center, float radius, out Vector3 result)
        {
            Vector3 diff = end - start;
            float a = Vector3.Dot(diff, diff);
            //center.Normalize();

            if (a > 0.0f)
            {
                float b = 2 * Vector3.Dot(diff, start - center);
                float c = (Vector3.Dot(center, center) + Vector3.Dot(start, start)) - (2 * Vector3.Dot(center, start)) - (radius * radius);

                float magnitude = (b * b) - (4 * a * c);
                magnitude *= -1;

                if (magnitude >= 0.0f)
                {
                    magnitude = (float)Math.Sqrt(magnitude);
                    a *= 2;

                    float scale = (-b + magnitude) / a;
                    float dist2 = (-b - magnitude) / a;

                    if (dist2 < scale)
                        scale = dist2;

                    result = start + (diff * scale);
                    return true;
                }
            }

            result = new Vector3();
            return false;
        }

        public float CamDistance(Vector3 va, Vector3 vb)
        {
            return (float)Math.Sqrt(Vector3.Dot(vb - va, vb - va));
        }

        public float TrueDistance(Vector3 va, Vector3 vb)
        {
            return Vector3.Dot(vb, va);
        }


        public Vector3 Angles(Vector3 i)
        {
            i.X = (float)Math.Atan2(i.Y, -i.Z);
            i.Y = (float)Math.Atan2(-i.Z, i.X);
            i.Z = (float)Math.Atan2(i.Y, i.X);
            return i;
        }
    }
}
