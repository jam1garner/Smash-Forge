using OpenTK.Graphics.OpenGL;
using SFGenericModel;
using System.Collections.Generic;

namespace SmashForge
{
    public class BfresRenderMesh : GenericMesh<BFRES.DisplayVertex>
    {
        public BfresRenderMesh(List<BFRES.DisplayVertex> vertices, List<int> vertexIndices) 
            : base(vertices.ToArray(), vertexIndices.ToArray(), PrimitiveType.Triangles)
        {

        }
    }
}
