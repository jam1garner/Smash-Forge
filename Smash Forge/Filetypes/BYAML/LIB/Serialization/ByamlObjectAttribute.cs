using System;

namespace Syroot.NintenTools.Byaml.Serialization
{
    /// <summary>
    /// Decorates a class which can be serialized with the <see cref="ByamlSerializer"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ByamlObjectAttribute : Attribute
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ByamlObjectAttribute"/> class.
        /// </summary>
        public ByamlObjectAttribute()
        {
        }
    }
}
