using System;

namespace Syroot.NintenTools.Byaml.Serialization
{
    /// <summary>
    /// Decorates fields or properties which are treated as values in BYAML dictionaries.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ByamlMemberAttribute : Attribute
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ByamlMemberAttribute"/> class. If the name of the member is
        /// different from the key in the BYAML data, it can be provided with the <paramref name="key"/> parameter.
        /// </summary>
        /// <param name="key">The name of the key in the BYAML data which maps to this member.</param>
        public ByamlMemberAttribute(string key = null)
        {
            Key = key;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the key under which this value is stored. If not specified, the field or property name is taken
        /// as it appears in code.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how this value will behave if it is <c>null</c>. When set to <c>true</c>,
        /// the value does not have to exist in the BYAML data and will not be stored if it is <c>null</c>. When set to
        /// <c>false</c>, the value must exist in the BYAML data and will be stored as a null node if it is <c>null</c>.
        /// It cannot be set to <c>true</c> on value types, which must be wrapped in a nullable type.
        /// </summary>
        public bool Optional { get; set; }
    }
}
