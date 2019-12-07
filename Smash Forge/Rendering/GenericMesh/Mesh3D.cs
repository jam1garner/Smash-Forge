using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Linq;

namespace SmashForge.Rendering.Meshes
{
    class Mesh3D : SFShapes.Mesh3D
    {
        public Mesh3D(List<Vector3> vertices) : base(vertices.Select(v => new SFShapes.Vertex3d(v.X, v.Y, v.Z)).ToArray(), PrimitiveType.Triangles)
        {

        }
    }
}
