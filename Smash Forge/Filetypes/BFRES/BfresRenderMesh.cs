using OpenTK.Graphics.OpenGL;
using SFGenericModel;
using SFGenericModel.VertexAttributes;
using System.Collections.Generic;

namespace Smash_Forge
{
    public class BfresRenderMesh : GenericMesh<BFRES.DisplayVertex>
    {
        public BfresRenderMesh(List<BFRES.DisplayVertex> vertices, List<int> vertexIndices) 
            : base(vertices, vertexIndices, PrimitiveType.Triangles)
        {

        }

        public override List<VertexAttribute> GetVertexAttributes()
        {
            return new List<VertexAttribute>()
            {
                new VertexFloatAttribute("vPosition",  ValueCount.Three, VertexAttribPointerType.Float),
                new VertexFloatAttribute("vNormal",    ValueCount.Three, VertexAttribPointerType.Float),
                new VertexFloatAttribute("vTangent",   ValueCount.Three, VertexAttribPointerType.Float),
                new VertexFloatAttribute("vBitangent", ValueCount.Three, VertexAttribPointerType.Float),
                new VertexFloatAttribute("vUV0",       ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexFloatAttribute("vColor",     ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexFloatAttribute("vBone",      ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexFloatAttribute("vWeight",    ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexFloatAttribute("vUV1",       ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexFloatAttribute("vUV2",       ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexFloatAttribute("vPosition2", ValueCount.Three, VertexAttribPointerType.Float),
                new VertexFloatAttribute("vPosition3", ValueCount.Three, VertexAttribPointerType.Float)
            };
        }
    }
}
