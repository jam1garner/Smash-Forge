using System;
using System.Reflection;

namespace Syroot.NintenTools.Byaml.Serialization
{
    /// <summary>
    /// Represents extension methods for the <see cref="TypeInfo"/> class.
    /// </summary>
    internal static class TypeInfoExtensions
    {
        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Retrieves a custom attribute of a specified type <typeparamref name="T"/> that is applied to a specified
        /// member.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to return.</typeparam>
        /// <param name="typeInfo">The extended <see cref="TypeInfo"/>.</param>
        /// <returns>A custom attribute that matches the given type, or <c>null</c> if no such attribute is found.
        /// </returns>
        internal static T GetCustomAttribute<T>(this TypeInfo typeInfo)
            where T : Attribute
        {
            return (T)typeInfo.GetCustomAttribute(typeof(T));
        }

        /// <summary>
        /// Retrieves a custom attribute of a specified type <typeparamref name="T"/> that is applied to a specified
        /// member, and optionally inspects the ancestors of that member.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to return.</typeparam>
        /// <param name="typeInfo">The extended <see cref="TypeInfo"/>.</param>
        /// <param name="inherit"><c>true</c> to inspect the ancestors of element; otherwise, <c>false</c>.</param>
        /// <returns>A custom attribute that matches the given type, or <c>null</c> if no such attribute is found.
        /// </returns>
        internal static T GetCustomAttribute<T>(this TypeInfo typeInfo, bool inherit)
            where T : Attribute
        {
            return (T)typeInfo.GetCustomAttribute(typeof(T), inherit);
        }

        /// <summary>
        /// Gets the type of the elements if this type is enumerable.
        /// </summary>
        /// <param name="typeInfo">The enumerable type.</param>
        /// <param name="inherit"><c>true</c> to inspect the ancestors of element; otherwise, <c>false</c>.</param>
        /// <returns>The type of the elements or <c>null</c> if there are no elements.</returns>
        internal static Type GetElementType(this TypeInfo typeInfo, bool inherit)
        {
            return typeInfo.GetProperty("Item")?.PropertyType;
        }
    }
}
