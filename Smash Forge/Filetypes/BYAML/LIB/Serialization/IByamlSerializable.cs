using System.Collections.Generic;

namespace Syroot.NintenTools.Byaml.Serialization
{
    /// <summary>
    /// Represents a class or structure which provides additional code to read or store complex BYAML data.
    /// </summary>
    public interface IByamlSerializable
    {
        // ---- METHODS ------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reads data from the given <paramref name="dictionary"/> to satisfy members.
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{String, Object}"/> to read the data from.</param>
        void DeserializeByaml(IDictionary<string, object> dictionary);

        /// <summary>
        /// Writes instance members into the given <paramref name="dictionary"/> to store them as BYAML data.
        /// </summary>
        /// <param name="dictionary">The <see cref="Dictionary{String, Object}"/> to store the data in.</param>
        void SerializeByaml(IDictionary<string, object> dictionary);
    }
}
