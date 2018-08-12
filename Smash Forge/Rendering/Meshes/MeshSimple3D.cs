using System;
using System.Collections.Generic;
using OpenTK;


namespace Smash_Forge.Rendering.Meshes
{
    class MeshSimple3D : Mesh<Vector3>
    {
        public MeshSimple3D(List<Vector3> vertices) : base(vertices, Vector3.SizeInBytes)
        {

        }
    }
}
