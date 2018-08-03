using Syroot.BinaryData;

namespace Syroot.NintenTools.MarioKart8.Collisions
{
    /// <summary>
    /// Represents a node in a course model octree.
    /// </summary>
    public class CourseOctreeNode : OctreeNodeBase<CourseOctreeNode>
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseOctreeNode"/> class with an empty key.
        /// </summary>
        internal CourseOctreeNode() : base(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseOctreeNode"/> class with the key and data read from the
        /// given <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryDataReader"/> to read the node data with.</param>
        internal CourseOctreeNode(BinaryDataReader reader) : base(reader.ReadUInt32())
        {
            switch ((Flags)(Key & _flagMask))
            {
                case Flags.Divide:
                    // Node is a branch subdivided into 8 children.
                    Children = new CourseOctreeNode[ChildCount];
                    for (int i = 0; i < ChildCount; i++)
                    {
                        Children[i] = new CourseOctreeNode(reader);
                    }
                    break;
                case Flags.Values:
                    // Node points to a model in the file's model array.
                    ModelIndex = Key & ~_flagMask;
                    break;
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the index to the model referenced by this node in the model array of the file this node belongs to.
        /// </summary>
        public uint? ModelIndex { get; internal set; }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        internal void Save(BinaryDataWriter writer)
        {
            if (Children == null)
            {
                if (ModelIndex.HasValue)
                {
                    // Node points to a model in the file's model array.
                    Key = (uint)Flags.Values | ModelIndex.Value;
                }
                else
                {
                    // Node is an empty cube.
                    Key = (uint)Flags.NoData;
                }
                writer.Write(Key);
            }
            else
            {
                // Node is a branch subdivided into 8 children.
                Key = 8;
                writer.Write(Key);
                foreach (CourseOctreeNode child in Children)
                {
                    child.Save(writer);
                }
            }
        }
    }
}
