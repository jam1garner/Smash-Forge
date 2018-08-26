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
                new VertexAttributeInfo("vPosition",  3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vNormal",    3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vTangent",   3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vBitangent", 3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vUV0",       2, VertexAttribPointerType.Float, Vector2.SizeInBytes),
                new VertexAttributeInfo("vColor",     4, VertexAttribPointerType.Float, Vector4.SizeInBytes),
                new VertexAttributeInfo("vBone",      4, VertexAttribPointerType.Float,  Vector4.SizeInBytes),
                new VertexAttributeInfo("vWeight",    4, VertexAttribPointerType.Float, Vector4.SizeInBytes),
                new VertexAttributeInfo("vUV1",       2, VertexAttribPointerType.Float, Vector2.SizeInBytes),
                new VertexAttributeInfo("vUV2",       2, VertexAttribPointerType.Float, Vector2.SizeInBytes),
                new VertexAttributeInfo("vPosition2", 3, VertexAttribPointerType.Float, Vector3.SizeInBytes),
                new VertexAttributeInfo("vPosition3", 3, VertexAttribPointerType.Float, Vector3.SizeInBytes)
            };
        }
    }
}
