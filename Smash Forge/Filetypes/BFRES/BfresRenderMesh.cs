using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFGenericModel;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SFGraphics.Cameras;
using SFGraphics.GLObjects.Shaders;

namespace Smash_Forge
{
    public class BfresRenderMesh : GenericMesh<BFRES.DisplayVertex>
    {
        public BfresRenderMesh(List<BFRES.DisplayVertex> vertices, List<int> vertexIndices) 
            : base(vertices, vertexIndices)
        {

        }

        protected override void SetCameraUniforms(Shader shader, Camera camera)
        {
            // Do nothing for now.
        }

        protected override List<VertexAttributeInfo> GetVertexAttributes()
        {
            return new List<VertexAttributeInfo>()
            {
                new VertexAttributeInfo("vPosition",  ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vNormal",    ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vTangent",   ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vBitangent", ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vUV0",       ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexAttributeInfo("vColor",     ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexAttributeInfo("vBone",      ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexAttributeInfo("vWeight",    ValueCount.Four,  VertexAttribPointerType.Float),
                new VertexAttributeInfo("vUV1",       ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexAttributeInfo("vUV2",       ValueCount.Two,   VertexAttribPointerType.Float),
                new VertexAttributeInfo("vPosition2", ValueCount.Three, VertexAttribPointerType.Float),
                new VertexAttributeInfo("vPosition3", ValueCount.Three, VertexAttribPointerType.Float)
            };
        }
    }
}
