using System;
using System.Collections.Generic;

namespace Syroot.NintenTools.Byaml
{
    /// <summary>
    /// Represents the type of which a dynamic BYAML node can be.
    /// </summary>
    internal enum ByamlNodeType : byte
    {
        /// <summary>
        /// The node represents a <see cref="string"/> (internally referenced by index).
        /// </summary>
        StringIndex = 0xA0,

        /// <summary>
        /// The node represents a list of <see cref="ByamlPathPoint"/> instances (internally referenced by index).
        /// </summary>
        PathIndex = 0xA1,

        /// <summary>
        /// The node represents an array of dynamic child instances.
        /// </summary>
        Array = 0xC0,

        /// <summary>
        /// The node represents a dictionary of dynamic child instances referenced by a <see cref="string"/> key.
        /// </summary>
        Dictionary = 0xC1,

        /// <summary>
        /// The node represents an array of <see cref="string"/> instances.
        /// </summary>
        StringArray = 0xC2,

        /// <summary>
        /// The node represents an array of lists of <see cref="ByamlPathPoint"/> instances.
        /// </summary>
        PathArray = 0xC3,

        /// <summary>
        /// The node represents a <see cref="bool"/>.
        /// </summary>
        Boolean = 0xD0,

        /// <summary>
        /// The node represents an <see cref="int"/>.
        /// </summary>
        Integer = 0xD1,

        /// <summary>
        /// The node represents a <see cref="float"/>.
        /// </summary>
        Float = 0xD2,

        /// <summary>
        /// The node represents <c>null</c>.
        /// </summary>
        Null = 0xFF
    }

    /// <summary>
    /// Represents extension methods for <see cref="ByamlNodeType"/> instances.
    /// </summary>
    internal static class ByamlNodeTypeExtensions
    {
        /// <summary>
        /// Gets the corresponding, instantiatable <see cref="Type"/> for the given <paramref name="nodeType"/>.
        /// </summary>
        /// <param name="nodeType">The <see cref="ByamlNodeType"/> which should be instantiated.</param>
        /// <returns>The <see cref="Type"/> to instantiate for the node.</returns>
        internal static Type GetInstanceType(this ByamlNodeType nodeType)
        {
            switch (nodeType)
            {
                case ByamlNodeType.StringIndex:
                    return typeof(string);
                case ByamlNodeType.PathIndex:
                    return typeof(List<ByamlPathPoint>);
                case ByamlNodeType.Array:
                    // TODO: Check if this could be loaded as an object array.
                    throw new ByamlException("Cannot instantiate an array of unknown element type.");
                case ByamlNodeType.Dictionary:
                    // TODO: Check if this could be loaded as a string-object dictionary.
                    throw new ByamlException("Cannot instantiate an object of unknown type.");
                case ByamlNodeType.Boolean:
                    return typeof(bool);
                case ByamlNodeType.Integer:
                    return typeof(int);
                case ByamlNodeType.Float:
                    return typeof(float);
                case ByamlNodeType.Null:
                    return typeof(object);
                default:
                    throw new ByamlException($"Unknown node type {nodeType}.");
            }
        }
    }
}
