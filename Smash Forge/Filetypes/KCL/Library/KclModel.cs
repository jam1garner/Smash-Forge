using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Syroot.BinaryData;
using Syroot.Maths;
using Syroot.NintenTools.MarioKart8.Common;
using Syroot.NintenTools.MarioKart8.IO;

namespace Syroot.NintenTools.MarioKart8.Collisions
{
    /// <summary>
    /// Represents a model referenced in a <see cref="KclFile"/> which can hold up to 65535 triangles.
    /// </summary>
    public class KclModel : ILoadableFile, ISaveableFile
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private static readonly Vector3F _minCoordinatePadding = new Vector3F(-50, -80, -50);
        private static readonly Vector3F _maxCoordinatePadding = new Vector3F(50, 50, 50);
        private const int _maxTrianglesInCube = 128;
        private const int _minCubeSize = 32;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="KclModel"/> class from the file with the given name.
        /// </summary>
        /// <param name="fileName">The name of the file from which the instance will be loaded.</param>
        /// <param name="loadOctree"><c>true</c> to also load the octree referencing triangles.</param>
        public KclModel(string fileName, bool loadOctree = true)
        {
            Load(fileName, loadOctree);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KclModel"/> class from the given stream.
        /// </summary>
        /// <param name="stream">The stream from which the instance will be loaded.</param>
        /// <param name="loadOctree"><c>true</c> to also load the octree referencing triangles.</param>
        /// <param name="leaveOpen"><c>true</c> to leave <paramref name="stream"/> open after creating the instance.
        /// </param>
        public KclModel(Stream stream, bool loadOctree = true, bool leaveOpen = false, ByteOrder Endianess = ByteOrder.LittleEndian)
        {
            Load(stream, loadOctree, leaveOpen, Endianess);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KclModel"/> class, created from the given
        /// <paramref name="objModel"/>.
        /// </summary>
        /// <param name="objModel">The <see cref="ObjModel"/> to create the collision data from.</param>
        internal KclModel(ObjModel objModel)
        {
            if (objModel.Faces.Count > UInt16.MaxValue)
                throw new InvalidOperationException("KCL models must not have more than 65535 triangles.");
            
            // Transfer the faces to collision faces and find the smallest and biggest coordinates.
            Positions = new List<Vector3F>(objModel.Positions);
            Normals = new List<Vector3F>(objModel.Normals);

            Vector3F minCoordinate = new Vector3F(Single.MaxValue, Single.MaxValue, Single.MaxValue);
            Vector3F maxCoordinate = new Vector3F(Single.MinValue, Single.MinValue, Single.MinValue);

            KclFace[] faces = new KclFace[objModel.Faces.Count];
            Triangle triangle = new Triangle();
            Dictionary<ushort, Triangle> triangles = new Dictionary<ushort, Triangle>(objModel.Faces.Count);
            ushort i = 0;
            foreach (ObjFace objFace in objModel.Faces)
            {
                KclFace face = new KclFace()
                {
                    PositionIndex = (ushort)objFace.Vertices[0].PositionIndex,
                    Normal1Index = (ushort)objFace.Vertices[0].NormalIndex,
                    Normal2Index = (ushort)objFace.Vertices[1].NormalIndex,
                    Normal3Index = (ushort)objFace.Vertices[2].NormalIndex
                };

                // Get the position vectors and find the smallest and biggest coordinates.
                for (int j = 0; j < 3; j++)
                {
                    Vector3F position = Positions[objFace.Vertices[j].PositionIndex];
                    minCoordinate.X = Math.Min(position.X, minCoordinate.X);
                    minCoordinate.Y = Math.Min(position.Y, minCoordinate.Y);
                    minCoordinate.Z = Math.Min(position.Z, minCoordinate.Z);
                    maxCoordinate.X = Math.Max(position.X, minCoordinate.X);
                    maxCoordinate.Y = Math.Max(position.X, minCoordinate.Y);
                    maxCoordinate.Z = Math.Max(position.X, minCoordinate.Z);
                    triangle.Vertices[j] = position;
                }

                // Compute the face direction (normal) and add it to the normal list.
                face.DirectionIndex = (ushort)(Normals.Count);
                Normals.Add(triangle.Normal);

                triangles.Add(i, triangle);
                faces[i++] = face;
            }
            minCoordinate += _minCoordinatePadding;
            maxCoordinate += _maxCoordinatePadding;
            MinCoordinate = minCoordinate;
            Faces = faces;

            // Compute the octree.
            Vector3F size = maxCoordinate - minCoordinate;
            Vector3 exponents = new Vector3(
                Maths.GetNext2Exponent(size.X),
                Maths.GetNext2Exponent(size.Y),
                Maths.GetNext2Exponent(size.Z));
            int cubeSizePower = Maths.GetNext2Exponent(Math.Min(Math.Min(size.X, size.Y), size.Z));
            int cubeSize = 1 << cubeSizePower;
            CoordinateShift = new Vector3(
                cubeSizePower,
                exponents.X - cubeSizePower,
                exponents.X - cubeSizePower + exponents.Y - cubeSizePower);
            CoordinateMask = new Vector3(
                (int)(0xFFFFFFFF << exponents.X),
                (int)(0xFFFFFFFF << exponents.Y),
                (int)(0xFFFFFFFF << exponents.Z));
            Vector3 cubeCounts = new Vector3(
                Math.Max(1, (1 << exponents.X) / cubeSize),
                Math.Max(1, (1 << exponents.Y) / cubeSize),
                Math.Max(1, (1 << exponents.Z) / cubeSize));
            // Generate the root nodes, which are square cubes required to cover all of the model.
            ModelOctreeRoots = new ModelOctreeNode[cubeCounts.X * cubeCounts.Y * cubeCounts.Z];
            i = 0;
            for (int z = 0; z < cubeCounts.Z; z++)
            {
                for (int y = 0; y < cubeCounts.Y; y++)
                {
                    for (int x = 0; x < cubeCounts.X; x++)
                    {
                        Vector3F cubePosition = new Vector3F(x, y, z) * cubeSize + minCoordinate;
                        ModelOctreeRoots[i++] = new ModelOctreeNode(triangles, cubePosition, cubeSize,
                            _maxTrianglesInCube, _minCubeSize);
                    }
                }
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the smallest coordinate of the cube spanned by the model.
        /// </summary>
        public Vector3F MinCoordinate { get; private set; }

        /// <summary>
        /// Gets the coordinate mask required to compute indices into the octree.
        /// </summary>
        public Vector3 CoordinateMask { get; private set; }

        /// <summary>
        /// Gets the coordinate shift required to compute indices into the octree.
        /// </summary>
        public Vector3 CoordinateShift { get; private set; }

        /// <summary>
        /// Gets the array of vertex positions.
        /// </summary>
        public List<Vector3F> Positions { get; private set; }

        /// <summary>
        /// Gets the array of vertex normals.
        /// </summary>
        public List<Vector3F> Normals { get; private set; }

        /// <summary>
        /// Gets the array of triangles.
        /// </summary>
        public KclFace[] Faces { get; private set; }

        /// <summary>
        /// Gets the root nodes of the model triangle octree. Can be <c>null</c> if no octree was loaded or created yet.
        /// </summary>
        public ModelOctreeNode[] ModelOctreeRoots { get; private set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Loads the data from the given file, including the octree.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public void Load(string fileName)
        {
            Load(fileName, true);
        }

        /// <summary>
        /// Loads the data from the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        /// <param name="loadOctree"><c>true</c> to also load the octree referencing triangles.</param>
        public void Load(string fileName, bool loadOctree)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Load(stream, loadOctree);
            }
        }

        /// <summary>
        /// Loads the data from the given <paramref name="stream"/>, including the octree.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave <paramref name="stream"/> open after loading the instance.
        /// </param>
        public void Load(Stream stream, bool leaveOpen = false)
        {
            Load(stream, true, leaveOpen);
        }

        /// <summary>
        /// Loads the data from the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="loadOctree"><c>true</c> to also load the octree referencing triangles.</param>
        /// <param name="leaveOpen"><c>true</c> to leave <paramref name="stream"/> open after loading the instance.
        /// </param>
        public void Load(Stream stream, bool loadOctree = true, bool leaveOpen = false, ByteOrder Endianness = ByteOrder.LittleEndian)
        {
            using (BinaryDataReader reader = new BinaryDataReader(stream, leaveOpen))
            {
                reader.ByteOrder = Endianness;

                long modelPosition = reader.Position;

                // Read the header.
                int positionArrayOffset = reader.ReadInt32();
                int normalArrayOffset = reader.ReadInt32();
                int triangleArrayOffset = reader.ReadInt32();
                int octreeOffset = reader.ReadInt32();
                reader.Seek(sizeof(float)); // Unknown value always being 30.0.
                MinCoordinate = reader.ReadVector3F();
                CoordinateMask = reader.ReadVector3();
                CoordinateShift = reader.ReadVector3();
                reader.Seek(sizeof(float)); // Unknown value always being 25.0.

                // Read the positions.
                reader.Position = modelPosition + positionArrayOffset; // Mostly unrequired, data is successive.
                int positionCount = (normalArrayOffset - positionArrayOffset) / Vector3F.SizeInBytes;
                Positions = new List<Vector3F>(reader.ReadVector3Fs(positionCount));

                // Read the normals.
                reader.Position = modelPosition + normalArrayOffset; // Mostly unrequired, data is successive.
                int normalCount = (triangleArrayOffset - normalArrayOffset) / Vector3F.SizeInBytes;
                Normals = new List<Vector3F>(reader.ReadVector3Fs(normalCount));

                // Read the triangles.
                reader.Position = modelPosition + triangleArrayOffset; // Mostly unrequired, data is successive.
                int triangleCount = (octreeOffset - triangleArrayOffset) / Marshal.SizeOf<KclFace>();
                Faces = reader.ReadTriangles(triangleCount);

                // Read the octree.
                if (loadOctree)
                {
                    reader.Position = modelPosition + octreeOffset; // Mostly unrequired, data is successive.
                    int nodeCount
                         = ((~CoordinateMask.X >> CoordinateShift.X) + 1)
                         * ((~CoordinateMask.Y >> CoordinateShift.X) + 1)
                         * ((~CoordinateMask.Z >> CoordinateShift.X) + 1);
                    ModelOctreeRoots = new ModelOctreeNode[nodeCount];
                    for (int i = 0; i < nodeCount; i++)
                    {
                        ModelOctreeRoots[i] = new ModelOctreeNode(reader, modelPosition + octreeOffset);
                    }
                    // Reader is now behind the last octree key, not at end of the model / behind the last separator.
                }
            }
        }

        /// <summary>
        /// Saves the data in the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to save the data in.</param>
        public void Save(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                Save(stream);
            }
        }

        /// <summary>
        /// Saves the data into the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save the data to.</param>
        /// <param name="leaveOpen"><c>true</c> to leave <paramref name="stream"/> open after saving the instance.
        /// </param>
        public void Save(Stream stream, bool leaveOpen = false, ByteOrder Endianness = ByteOrder.LittleEndian)
        {
            using (BinaryDataWriter writer = new BinaryDataWriter(stream, leaveOpen))
            {
                writer.ByteOrder = Endianness;

                long modelPosition = writer.Position;

                // Write the header.
                Offset positionArrayOffset = writer.ReserveOffset();
                Offset normalArrayOffset = writer.ReserveOffset();
                Offset triangleArrayOffset = writer.ReserveOffset();
                Offset octreeOffset = writer.ReserveOffset();
                writer.Write(30f);
                writer.Write(MinCoordinate);
                writer.Write(CoordinateMask);
                writer.Write(CoordinateShift);
                writer.Write(25f);

                // Write the positions.
                positionArrayOffset.Satisfy((int)(writer.Position - modelPosition));
                writer.Write(Positions.ToArray());

                // Write the normals.
                normalArrayOffset.Satisfy((int)(writer.Position - modelPosition));
                writer.Write(Normals.ToArray());

                // Write the triangles.
                triangleArrayOffset.Satisfy((int)(writer.Position - modelPosition));
                writer.Write(Faces);

                // Write the octree.
                int octreeOffsetValue = (int)(writer.Position - modelPosition);
                octreeOffset.Satisfy(octreeOffsetValue);
                // Write the node keys, and compute the correct offsets into the triangle lists or to child nodes.
                // Nintendo writes child nodes behind the current node, so the children need to be queued.
                // In this implementation, empty triangle lists point to the same terminator behind the last node.
                // This could be further optimized by reusing equal parts of lists as Nintendo apparently did it.
                int emptyListPos = GetNodeCount(ModelOctreeRoots) * sizeof(uint);
                int triangleListPos = emptyListPos + sizeof(ushort);
                Queue<ModelOctreeNode[]> queuedNodes = new Queue<ModelOctreeNode[]>();
                queuedNodes.Enqueue(ModelOctreeRoots);
                while (queuedNodes.Count > 0)
                {
                    ModelOctreeNode[] nodes = queuedNodes.Dequeue();
                    long offset = writer.Position - modelPosition - octreeOffsetValue;
                    foreach (ModelOctreeNode node in nodes)
                    {
                        if (node.Children == null)
                        {
                            // Node is a leaf and points to triangle index list.
                            int listPos;
                            if (node.TriangleIndices.Count == 0)
                            {
                                listPos = emptyListPos;
                            }
                            else
                            {
                                listPos = triangleListPos;
                                triangleListPos += (node.TriangleIndices.Count + 1) * sizeof(ushort);
                            }
                            node.Key = (uint)ModelOctreeNode.Flags.Values | (uint)(listPos - offset - sizeof(ushort));
                        }
                        else
                        {
                            // Node is a branch and points to 8 children.
                            node.Key = (uint)(nodes.Length + queuedNodes.Count * 8) * sizeof(uint);
                            queuedNodes.Enqueue(node.Children);
                        }
                        writer.Write(node.Key);
                    }
                }
                // Iterate through the nodes again and write their triangle lists now.
                writer.Write((ushort)0xFFFF); // Terminator for all empty lists.
                queuedNodes.Enqueue(ModelOctreeRoots);
                while (queuedNodes.Count > 0)
                {
                    ModelOctreeNode[] nodes = queuedNodes.Dequeue();
                    foreach (ModelOctreeNode node in nodes)
                    {
                        if (node.Children == null)
                        {
                            if (node.TriangleIndices.Count > 0)
                            {
                                // Node is a leaf and points to triangle index list.
                                writer.Write(node.TriangleIndices);
                                writer.Write((ushort)0xFFFF);
                            }
                        }
                        else
                        {
                            // Node is a branch and points to 8 children.
                            queuedNodes.Enqueue(node.Children);
                        }
                    }
                }
            }
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private int GetNodeCount(ModelOctreeNode[] nodes)
        {
            int count = nodes.Length;
            foreach (ModelOctreeNode node in nodes)
            {
                if (node.Children != null)
                {
                    count += GetNodeCount(node.Children);
                }
            }
            return count;
        }
    }
}
