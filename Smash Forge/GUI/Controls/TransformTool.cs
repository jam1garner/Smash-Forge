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

        public float Distance(Vector3 v1, Vector3 v2)
        {
            return (float)Math.Sqrt(Math.Pow(v1.X-v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2)+ Math.Pow(v1.Z - v2.Z, 2));
        }

        public void Render(Matrix4 view)
        {
            if (b == null) return;

            Matrix4 mat = b.transform;
            Vector3 center = Vector3.Transform(Vector3.Zero, mat);
            Vector3 point;

            hit = false;
            bool _hiX = false;
            bool _hiY = false;
            bool _hiZ = false;
            if (RenderTools.CheckSphereHit(center, 2, VBNViewport.p1, VBNViewport.p2, out point))
            {
                hit = true;

                VBNViewport.LineSphereIntersect(VBNViewport.p1, VBNViewport.p2, center, 2, out point);

                Vector3 angle = Angles(Vector3.Transform(point, b.invert)) * new Vector3(180f / (float)Math.PI);
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

        public Vector3 Angles(Vector3 i)
        {
            i.X = (float)Math.Atan2(i.Y, -i.Z);
            i.Y = (float)Math.Atan2(-i.Z, i.X);
            i.Z = (float)Math.Atan2(i.Y, i.X);
            return i;
        }
    }
}
