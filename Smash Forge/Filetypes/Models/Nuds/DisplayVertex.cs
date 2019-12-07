using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel.VertexAttributes;

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
            [VertexFloatAttribute("vPosition", ValueCount.Three, VertexAttribPointerType.Float, false)]
            public Vector3 pos;

            [VertexFloatAttribute("vNormal", ValueCount.Three, VertexAttribPointerType.Float, false)]
            public Vector3 nrm;

            [VertexFloatAttribute("vTangent", ValueCount.Three, VertexAttribPointerType.Float, false)]
            public Vector3 tan;

            [VertexFloatAttribute("vBiTangent", ValueCount.Three, VertexAttribPointerType.Float, false)]
            public Vector3 bit;

            [VertexFloatAttribute("vUV", ValueCount.Two, VertexAttribPointerType.Float, false)]
            public Vector2 uv;

            [VertexFloatAttribute("vColor", ValueCount.Four, VertexAttribPointerType.Float, false)]
            public Vector4 col;

            [VertexIntAttribute("vBone", ValueCount.Four, VertexAttribIntegerType.Int)]
            public Vector4I boneIds;

            [VertexFloatAttribute("vWeight", ValueCount.Four, VertexAttribPointerType.Float, false)]
            public Vector4 weight;

            [VertexFloatAttribute("vUV2", ValueCount.Two, VertexAttribPointerType.Float, false)]
            public Vector2 uv2;

            [VertexFloatAttribute("vUV3", ValueCount.Two, VertexAttribPointerType.Float, false)]
            public Vector2 uv3;

            public static int Size = 4 * (3 + 3 + 3 + 3 + 2 + 4 + 4 + 4 + 2 + 2);
        }
    }
}

