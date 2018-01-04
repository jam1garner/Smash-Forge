using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Windows.Forms;

namespace Smash_Forge
{
    public class Ray
    {
        public Vector3 p1, p2;

        public Ray(Vector3 p1, Vector3 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public Ray(Camera Camera, GLControl Viewport)
        {
            float mouse_x = Viewport.PointToClient(Cursor.Position).X;
            float mouse_y = Viewport.PointToClient(Cursor.Position).Y;

            float x = (2.0f * mouse_x) / Viewport.Width - 1.0f;
            float y = 1.0f - (2.0f * mouse_y) / Viewport.Height;
            Vector4 va = Vector4.Transform(new Vector4(x, y, -1.0f, 1.0f), Camera.getMVPMatrix().Inverted());
            Vector4 vb = Vector4.Transform(new Vector4(x, y, 1.0f, 1.0f), Camera.getMVPMatrix().Inverted());

            p1 = va.Xyz;
            p2 = p1 - (va - (va + vb)).Xyz * 100;
        }

        public bool TrySphereHit(Vector3 sphere, float rad, out Vector3 closest)
        {
            return CheckSphereHit(sphere, rad, out closest);
        }

        public double Distance(Vector3 closest)
        {
            return Math.Pow(closest.X - p1.X, 2) + Math.Pow(closest.Y - p1.Y, 2) + Math.Pow(closest.Z - p1.Z, 2);
        }

        public bool intersectCircle(Vector3 pos, float r, int smooth)
        {
            float t = 2 * (float)Math.PI / smooth;
            float tf = (float)Math.Tan(t);

            float rf = (float)Math.Cos(t);

            float x = r;
            float y = 0;

            for (int i = 0; i < smooth; i++)
            {
                Vector3 c;
                Vector3 p = new Vector3(x + pos.X, y + pos.Y, pos.Z);
                if (CheckSphereHit(p, 0.3f, out c))
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

        public bool CheckSphereHit(Vector3 sphere, float rad, out Vector3 closest)
        {
            Vector3 dirToSphere = sphere - p1;
            Vector3 vLineDir = (p2 - p1).Normalized();
            float fLineLength = 100;

            float t = Vector3.Dot(dirToSphere, vLineDir);

            if (t <= 0.0f)
                closest = p1;
            else if (t >= fLineLength)
                closest = p2;
            else
                closest = p1 + vLineDir * t;

            return (Math.Pow(sphere.X - closest.X, 2)
                + Math.Pow(sphere.Y - closest.Y, 2)
                + Math.Pow(sphere.Z - closest.Z, 2) <= rad * rad);
        }
    }
}
