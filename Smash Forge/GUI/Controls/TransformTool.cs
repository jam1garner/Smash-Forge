using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Diagnostics;

namespace Smash_Forge
{
    public class TransformTool
    {
        public Bone b;
        public bool hit = false;
        public float Size = 1.5f;

        Vector2 PrevPoint = new Vector2();
        public int state = 0;

        bool _hiX = false;
        bool _hiY = false;
        bool _hiZ = false;

        public ToolTypes Type = ToolTypes.ROTATION;

        public enum ToolTypes
        {
            POSITION,
            ROTATION,
            SCALE
        }

        public TransformTool()
        {
        }

        public void Render(Camera Camera, Ray Ray)
        {
            if (b == null) return;

            Matrix4 mat = b.transform;
            Vector3 center = Vector3.TransformPosition(Vector3.Zero, mat);
            Matrix4 invTrasnform = b.transform.ClearScale().Inverted();
            Vector3 point;
            if (state == 0)
            {
                hit = Ray.LineSphereIntersect(center, Size, out point);
                if (hit)
                {
                    Vector3 angle = Angles(Vector3.TransformPosition(point, invTrasnform)) * new Vector3(180f / (float)Math.PI);
                    angle.X = Math.Abs(angle.X);
                    angle.Y = Math.Abs(angle.Y);
                    angle.Z = Math.Abs(angle.Z);

                    _hiX = false;
                    _hiY = false;
                    _hiZ = false;
                    float _axisSnapRange = 14f;
                    if (Math.Abs(angle.Y - 90.0f) <= _axisSnapRange)
                        _hiX = true;
                    else if (angle.X >= (180.0f - _axisSnapRange) || angle.X <= _axisSnapRange)
                        _hiY = true;
                    else if (angle.Y >= (180.0f - _axisSnapRange) || angle.Y <= _axisSnapRange)
                        _hiZ = true;
                }
                if (!_hiX && !_hiZ && !_hiY)
                    hit = false;

                if (hit && OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Left))
                {
                    PrevPoint = new Vector2(Ray.mouse_x, Ray.mouse_y);
                    state = 1;
                }
            }

            if (state == 1)
            {
                float sx = (Ray.mouse_x - PrevPoint.X) / 100;
                float sy = (Ray.mouse_y - PrevPoint.Y) / 100;
                float s = sx+sy;
                if (_hiX)
                    b.rot = b.rot * Quaternion.FromAxisAngle(Vector3.UnitX, s);
                if (_hiY)
                    b.rot = b.rot * Quaternion.FromAxisAngle(Vector3.UnitY, s);
                if (_hiZ)
                    b.rot = b.rot * Quaternion.FromAxisAngle(Vector3.UnitZ, s);
                b.vbnParent.update();
                PrevPoint = new Vector2(Ray.mouse_x, Ray.mouse_y);


                if (!OpenTK.Input.Mouse.GetState().IsButtonDown(OpenTK.Input.MouseButton.Left))
                    state = 0;
            }

            GL.PushMatrix();
            GL.MultMatrix(ref mat);

            GL.Color4(0.25f, 0.25f, 0.25f, 0.2f);
            RenderTools.drawSphere(Vector3.Zero, Size, 25);

            GL.Color3(_hiZ ? Color.Yellow : Color.Green);
            GL.LineWidth(3);
            RenderTools.drawCircleOutline(Vector3.Zero, Size, 25);

            GL.Rotate(90.0f, 0.0f, 1.0f, 0.0f);

            GL.Color3(_hiX ? Color.Yellow : Color.Red);
            RenderTools.drawCircleOutline(Vector3.Zero, Size, 25);

            GL.Rotate(90.0f, 1.0f, 0.0f, 0.0f);

            GL.Color3(_hiY ? Color.Yellow : Color.Blue);
            RenderTools.drawCircleOutline(Vector3.Zero, Size, 25);
            
            GL.PopMatrix();
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
            Vector3 ni = new Vector3();
            ni.X = (float)Math.Atan2(i.Y, -i.Z);
            ni.Y = (float)Math.Atan2(-i.Z, i.X);
            ni.Z = (float)Math.Atan2(i.Y, i.X);
            return ni;
        }
    }
}
