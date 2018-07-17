using System;
using System.Collections.Generic;
using System.Reflection;

namespace Syroot.NintenTools.Byaml.Serialization
{
    /// <summary>
    /// Represents information about a type decorated with a <see cref="ByamlObjectAttribute"/>.
    /// </summary>
    internal class ByamlObjectInfo
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Creates a new instance of the <see cref="ByamlObjectInfo"/> class caching information on the given
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to cache information on.</param>
        internal ByamlObjectInfo(Type type)
        {
            Members = new Dictionary<string, ByamlMemberInfo>();

            TypeInfo typeInfo = type.GetTypeInfo();
            if (typeInfo.GetCustomAttribute<ByamlObjectAttribute>(true) == null)
            {
                throw new ByamlException(
                    $"Type {type.Name} requires to be decorated with a {nameof(ByamlObjectAttribute)} to be used for serialization.");
            }

            // Check if the type implements IByamlSerializable.
            ImplementsByamlSerializable = typeof(IByamlSerializable).GetTypeInfo().IsAssignableFrom(type);

            // Go through all fields decorated with the ByamlValueAttribute and cache them.
            foreach (FieldInfo fieldInfo in typeInfo.GetFields(BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Instance))
            {
                ByamlMemberAttribute attribute = fieldInfo.GetCustomAttribute<ByamlMemberAttribute>();
                if (attribute != null)
                {
                    string key = attribute.Key ?? fieldInfo.Name;
                    Members.Add(key, new ByamlMemberInfo(attribute, fieldInfo));
                }
            }
            // Go through all properties decorated with the ByamlValueAttribute and cache them.
            foreach (PropertyInfo propertyInfo in typeInfo.GetProperties(BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Instance))
            {
                ByamlMemberAttribute attribute = propertyInfo.GetCustomAttribute<ByamlMemberAttribute>();
                if (attribute != null)
                {
                    // Property requires to have a getter and setter.
                    if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
                    {
                        throw new ByamlException(
                            $"Property {type.Name}.{propertyInfo.Name} requires both a getter and setter to be used for serialization.");
                    }
                    string key = attribute.Key ?? propertyInfo.Name;
                    Members.Add(key, new ByamlMemberInfo(attribute, propertyInfo));
                }
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether the type implements the <see cref="IByamlSerializable"/> interface,
        /// providing code to serialize or deserialize complex data.
        /// </summary>
        internal bool ImplementsByamlSerializable { get; private set; }

        /// <summary>
        /// Gets the dictionary holding keys to the serializable values together with the <see cref="MemberInfo"/> to
        /// access them.
        /// </summary>
        internal Dictionary<string, ByamlMemberInfo> Members { get; private set; }
    }
}
