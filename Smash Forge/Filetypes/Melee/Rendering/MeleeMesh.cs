using MeleeLib.DAT;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGenericModel;
using System.Collections.Generic;
using SFGenericModel.VertexAttributes;
using Smash_Forge.Filetypes.Melee.Utils;
using SFGenericModel.RenderState;

namespace Smash_Forge.Filetypes.Melee.Rendering
{
    public struct MeleeVertex
    {
        public Vector3 Pos;
        public Vector3 Nrm;
        public Vector3 Bit;
        public Vector3 Tan;
        public Vector2 UV0;
        public Vector4 Clr;
        public Vector4 Bone;
        public Vector4 Weight;
    }

    public class MeleeMesh : GenericMesh<MeleeVertex>
    {
        public MeleeMesh(IList<MeleeVertex> vertices, IList<int> vertexIndices, PrimitiveType primitiveType)
            : base(vertices, vertexIndices, primitiveType)
        {

        }

        public override List<VertexAttribute> GetVertexAttributes()
        {
            return new List<VertexAttribute>()
            {
                new VertexFloatAttribute("vPosition", ValueCount.Three, VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vNormal",   ValueCount.Three, VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vBitan",    ValueCount.Three, VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vTan",      ValueCount.Three, VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vUV0",      ValueCount.Two,   VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vColor",    ValueCount.Four,  VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vBone",     ValueCount.Four,  VertexAttribPointerType.Float, false),
                new VertexFloatAttribute("vWeight",   ValueCount.Four,  VertexAttribPointerType.Float, false)
            };
        }
    }
}
