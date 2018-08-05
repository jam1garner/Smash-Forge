using System;
using Syroot.Maths;

namespace Syroot.NintenTools.Byaml
{
    /// <summary>
    /// Represents a point in a BYAML path.
    /// </summary>
    public class ByamlPathPoint : IEquatable<ByamlPathPoint>
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// The size of a single point in bytes when serialized as BYAML data.
        /// </summary>
        internal const int SizeInBytes = 28;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ByamlPathPoint"/> class.
        /// </summary>
        public ByamlPathPoint()
        {
            Normal = new Vector3F(0, 1, 0);
        }
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public Vector3F Position { get; set; }

        /// <summary>
        /// Gets or sets the normal.
        /// </summary>
        public Vector3F Normal { get; set; }

        /// <summary>
        /// Gets or sets an unknown value.
        /// </summary>
        public uint Unknown { get; set; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(ByamlPathPoint other)
        {
            return Position == other.Position && Normal == other.Normal && Unknown == other.Unknown;
        }
    }
}
