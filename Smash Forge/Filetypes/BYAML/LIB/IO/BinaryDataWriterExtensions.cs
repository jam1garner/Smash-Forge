using Syroot.BinaryData;
using Syroot.Maths;

namespace Syroot.NintenTools.Byaml.IO
{   
    /// <summary>
    /// Represents a set of extension methods for the <see cref="BinaryDataWriter"/> class.
    /// </summary>
    internal static class BinaryDataWriterExtensions
    {
        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Writes a <see cref="Vector3F"/> instance at the current position in the base stream, and advances the
        /// position of the stream by 12 bytes.
        /// </summary>
        /// <param name="writer">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector3F"/> to write.</param>
        internal static void Write(this BinaryDataWriter writer, Vector3F value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }
    }
}
