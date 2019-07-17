using OpenTK;

namespace SmashForge
{
    public partial class Nud
    {
        public struct Vector4I
        {
            public int X { get; }
            public int Y { get; }
            public int Z { get; }
            public int W { get; }

            public Vector4I(int x, int y, int z, int w)
            {
                X = x;
                Y = y;
                Z = z;
                W = w;
            }
        }

        public struct DisplayVertex
        {
            // Used for rendering.
            public Vector3 pos;
            public Vector3 nrm;
            public Vector3 tan;
            public Vector3 bit;
            public Vector2 uv;
            public Vector4 col;
            public Vector4I boneIds;
            public Vector4 weight;
            public Vector2 uv2;
            public Vector2 uv3;

            public static int Size = 4 * (3 + 3 + 3 + 3 + 2 + 4 + 4 + 4 + 2 + 2);
        }
    }
}

