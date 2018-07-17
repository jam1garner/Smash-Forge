using Syroot.BinaryData;
using Syroot.Maths;

namespace Syroot.NintenTools.Byaml.IO
{
    /// <summary>
    /// Represents a set of extension methods for the <see cref="BinaryDataReader"/> class.
    /// </summary>
    internal static class BinaryDataReaderExtensions
    {
        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Reads a <see cref="Vector3F"/> instance from the current position in the base stream, and advances the
        /// position of the stream by 12 bytes.
        /// </summary>
        /// <param name="reader">The extended <see cref="BinaryDataReader"/>.</param>
        /// <returns>The read <see cref="Vector3F"/>.</returns>
        internal static Vector3F ReadVector3F(this BinaryDataReader reader)
        {
            return new Vector3F(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
}
