using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel.VertexAttributes;

namespace SmashForge
{
    public partial class BFRES
    {
        public struct DisplayVertex
        {
            [VertexFloatAttribute("vPosition", ValueCount.Three, VertexAttribPointerType.Float, false)]
            public Vector3 pos;

            [VertexFloatAttribute("vNormal", ValueCount.Three, VertexAttribPointerType.Float, false)]
            public Vector3 nrm;

            [VertexFloatAttribute("vTangent", ValueCount.Three, VertexAttribPointerType.Float, false)]
            public Vector3 tan;

            [VertexFloatAttribute("vBitangent", ValueCount.Three, VertexAttribPointerType.Float, false)]
            public Vector3 bit;

            [VertexFloatAttribute("vUV0", ValueCount.Two, VertexAttribPointerType.Float, false)]
            public Vector2 uv;

            [VertexFloatAttribute("vColor", ValueCount.Four, VertexAttribPointerType.Float, false)]
            public Vector4 col;

            [VertexFloatAttribute("vBone", ValueCount.Four, VertexAttribPointerType.Float, false)]
            public Vector4 node;

            [VertexFloatAttribute("vWeight", ValueCount.Four, VertexAttribPointerType.Float, false)]
            public Vector4 weight;

            [VertexFloatAttribute("vUV1", ValueCount.Two, VertexAttribPointerType.Float, false)]
            public Vector2 uv2;

            [VertexFloatAttribute("vUV2", ValueCount.Two, VertexAttribPointerType.Float, false)]
            public Vector2 uv3;

            [VertexFloatAttribute("vPosition2", ValueCount.Three, VertexAttribPointerType.Float, false)]
            public Vector3 pos1;

            [VertexFloatAttribute("vPosition3", ValueCount.Three, VertexAttribPointerType.Float, false)]
            public Vector3 pos2;

            public static int Size = 4 * (3 + 3 + 3 + 3 + 2 + 4 + 4 + 4 + 2 + 2 + 3 + 3);
        }
    }
}
