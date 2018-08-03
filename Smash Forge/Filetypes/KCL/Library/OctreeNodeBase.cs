using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Syroot.NintenTools.MarioKart8.Collisions
{
    /// <summary>
    /// Represents the base for an octree node.
    /// </summary>
    /// <typeparam name="T">The type of the octree node.</typeparam>
    [DebuggerDisplay(nameof(OctreeNodeBase<T>) + ", Key=0x{Key.ToString(\"X8\"),nq}")]
    public abstract class OctreeNodeBase<T> : IEnumerable<T>
        where T : OctreeNodeBase<T>
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// The number of children of an octree node.
        /// </summary>
        public const int ChildCount = 8;
        
        /// <summary>
        /// The bits storing the flags of this node.
        /// </summary>
        protected const uint _flagMask = 0b11000000_00000000_00000000_00000000;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="OctreeNodeBase{T}"/> class with the given octree node
        /// <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The octree node key with which the node can be referenced.</param>
        protected OctreeNodeBase(uint key)
        {
            Key = key;
            // Do not create child array here as it might not be required.
        }
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the octree key used to reference this node.
        /// </summary>
        public uint Key { get; internal set; }

        /// <summary>
        /// Gets the eight children of this node.
        /// </summary>
        public T[] Children { get; internal set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------
        
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return Children == null ? null : ((IEnumerable<T>)Children).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Children?.GetEnumerator();
        }
        
        // ---- ENUMERATIONS -------------------------------------------------------------------------------------------

        internal enum Flags : uint
        {
            Divide = 0b00000000_00000000_00000000_00000000,
            Values = 0b10000000_00000000_00000000_00000000,
            NoData = 0b11000000_00000000_00000000_00000000
        }
    }
}
