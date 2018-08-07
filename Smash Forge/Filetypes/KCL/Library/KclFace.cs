namespace Syroot.NintenTools.MarioKart8.Collisions
{
    /// <summary>
    /// Represents a triangle as stored in a collision file.
    /// </summary>
    public struct KclFace
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The length of this triangle.
        /// </summary>
        public float Length;

        /// <summary>
        /// The 0-based index of the positional vector in the position array of the model this triangle belongs to.
        /// </summary>
        public ushort PositionIndex;

        /// <summary>
        /// The 0-based index of the direction normal in the normal array of the model this triangle belongs to.
        /// </summary>
        public ushort DirectionIndex;

        /// <summary>
        /// The first 0-based index of the normal in the normal array of the model this triangle belongs to.
        /// </summary>
        public ushort Normal1Index;

        /// <summary>
        /// The second 0-based index of the normal in the normal array of the model this triangle belongs to.
        /// </summary>
        public ushort Normal2Index;

        /// <summary>
        /// The third 0-based index of the normal in the normal array of the model this triangle belongs to.
        /// </summary>
        public ushort Normal3Index;
        
        /// <summary>
        /// The collision flags determining in-game behavior when colliding with this polygon.
        /// </summary>
        public ushort CollisionFlags;

        /// <summary>
        /// The 0-based index of the triangle in the KCL file this triangle belongs to.
        /// </summary>
        public uint GlobalIndex;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="KclFace"/> struct with the given values.
        /// </summary>
        /// <param name="length">The length of this triangle.</param>
        /// <param name="positionIndex">The 0-based index of the positional vector in the position array of the model
        /// this triangle belongs to.</param>
        /// <param name="directionIndex">The 0-based index of the direction normal in the normal array of the model this
        /// triangle belongs to.</param>
        /// <param name="normalIndex1">The first 0-based index of the normal in the normal array of the model this
        /// triangle belongs to.</param>
        /// <param name="normalIndex2">The second 0-based index of the normal in the normal array of the model this
        /// triangle belongs to.</param>
        /// <param name="normalIndex3">The third 0-based index of the normal in the normal array of the model this
        /// triangle belongs to.</param>
        /// <param name="collisionFlags">The collision flags determining in-game behavior when colliding with this
        /// polygon.</param>
        /// <param name="globalIndex">The 0-based index of the triangle in the triangle array of the model this
        /// triangle belongs to.</param>
        internal KclFace(float length, ushort positionIndex, ushort directionIndex, ushort normalIndex1,
            ushort normalIndex2, ushort normalIndex3, ushort collisionFlags, uint globalIndex)
        {
            Length = length;
            PositionIndex = positionIndex;
            DirectionIndex = directionIndex;
            Normal1Index = normalIndex1;
            Normal2Index = normalIndex2;
            Normal3Index = normalIndex3;
            CollisionFlags = collisionFlags;
            GlobalIndex = globalIndex;
        }
    }
}
