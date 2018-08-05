using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Syroot.BinaryData;
using Syroot.Maths;

namespace Syroot.NintenTools.MarioKart8.Collisions
{
    /// <summary>
    /// Represents a node in a model triangle octree.
    /// </summary>
    [DebuggerDisplay(nameof(ModelOctreeNode) + ", {TriangleIndices?.Count} Triangles")]
    public class ModelOctreeNode : OctreeNodeBase<ModelOctreeNode>
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelOctreeNode"/> class with the key and data read from the
        /// given <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryDataReader"/> to read the node data with.</param>
        /// <param name="parentOffset">The required offset of the start of the parent node.</param>
        internal ModelOctreeNode(BinaryDataReader reader, long parentOffset) : base(reader.ReadUInt32())
        {
            // Get and seek to the data offset in bytes relative to the parent node's start.
            long offset = parentOffset + Key & ~_flagMask;
            if ((Flags)(Key & _flagMask) == Flags.Values)
            {
                // Node is a leaf and key points to triangle list starting 2 bytes later.
                using (reader.TemporarySeek(offset + sizeof(ushort), SeekOrigin.Begin))
                {
                    TriangleIndices = new List<ushort>();
                    ushort index;
                    while ((index = reader.ReadUInt16()) != 0xFFFF)
                    {
                        TriangleIndices.Add(index);
                    }
                }
            }
            else
            {
                // Node is a branch and points to 8 child nodes.
                using (reader.TemporarySeek(offset, SeekOrigin.Begin))
                {
                    ModelOctreeNode[] children = new ModelOctreeNode[ChildCount];
                    for (int i = 0; i < ChildCount; i++)
                    {
                        children[i] = new ModelOctreeNode(reader, offset);
                    }
                    Children = children;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelOctreeNode"/> class, initializing children and
        /// subdivisions from the given list of faces sorted into a cube of the given <paramref name="cubeSize"/>.
        /// </summary>
        /// <param name="triangles">The dictionary of <see cref="Triangle"/> instances which have to be sorted into this
        /// octree, with the key being their original index in the model.</param>
        /// <param name="cubePosition">The offset of the cube.</param>
        /// <param name="cubeSize">The size of the cube.</param>
        /// <param name="maxTrianglesInCube">The maximum number of triangles to sort into this node.</param>
        /// <param name="minCubeSize">The minimum size a cube can be subdivided to.</param>
        internal ModelOctreeNode(Dictionary<ushort, Triangle> triangles, Vector3F cubePosition, int cubeSize,
            int maxTrianglesInCube, int minCubeSize) : base(0)
        {
            int cubeHalfSize = cubeSize / 2;
            Vector3F cubeCenter = cubePosition + new Vector3F(cubeHalfSize, cubeHalfSize, cubeHalfSize);
            
            // Go through all triangles and remember them if they overlap with the region of this cube.
            Dictionary<ushort, Triangle> containedTriangles = new Dictionary<ushort, Triangle>();
            foreach (KeyValuePair<ushort, Triangle> triangle in triangles)
            {
                if (Maths.TriangleCubeOverlap(triangle.Value, cubeCenter, cubeHalfSize))
                {
                    containedTriangles.Add(triangle.Key, triangle.Value);
                }
            }
            
            if (cubeSize > minCubeSize && containedTriangles.Count > maxTrianglesInCube)
            {
                // Too many triangles are in this cube, and it can still be subdivided into smaller cubes.
                int childCubeSize = cubeSize / 2;
                Children = new ModelOctreeNode[ChildCount];
                int i = 0;
                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            Vector3F childCubePosition = new Vector3F(x, y, z) * childCubeSize + cubePosition;
                            Children[i++] = new ModelOctreeNode(triangles, childCubePosition, childCubeSize,
                                maxTrianglesInCube, minCubeSize);
                        }
                    }
                }
            }
            else
            {
                // Either the amount of triangles in this cube is okay or it cannot be subdivided any further.
                TriangleIndices = new List<ushort>(containedTriangles.Keys);
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the indices to triangles of the model appearing in this cube.
        /// </summary>
        public List<ushort> TriangleIndices { get; }
    }
}
