using System;
using System.Diagnostics;
using System.Reflection;

namespace Syroot.NintenTools.Byaml.Serialization
{
    /// <summary>
    /// Represents information about a type decorated with a <see cref="ByamlMemberAttribute"/>.
    /// </summary>
    [DebuggerDisplay("MemberInfo={MemberInfo} Optional={Optional}")]
    internal class ByamlMemberInfo
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Creates a new instance of the <see cref="ByamlMemberInfo"/> class caching information on the given
        /// <paramref name="fieldInfo"/> decorated with the given <paramref name="attribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="ByamlMemberAttribute"/> to cache information on.</param>
        /// <param name="fieldInfo">The <see cref="FieldInfo"/> decorated with the attribute.</param>
        internal ByamlMemberInfo(ByamlMemberAttribute attribute, FieldInfo fieldInfo)
        {
            Optional = attribute.Optional;
            MemberInfo = fieldInfo;
            Type = fieldInfo.FieldType;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ByamlMemberInfo"/> class caching information on the given
        /// <paramref name="propertyInfo"/> decorated with the given <paramref name="attribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="ByamlMemberAttribute"/> to cache information on.</param>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> decorated with the attribute.</param>
        internal ByamlMemberInfo(ByamlMemberAttribute attribute, PropertyInfo propertyInfo)
        {
            Optional = attribute.Optional;
            MemberInfo = propertyInfo;
            Type = propertyInfo.PropertyType;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether the field is not required to be set to a non-<c>null</c> value.
        /// </summary>
        internal bool Optional { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Reflection.MemberInfo"/> instance for this value.
        /// </summary>
        internal MemberInfo MemberInfo { get; private set; }

        /// <summary>
        /// Gets the type of the member which has to be instantiated.
        /// </summary>
        internal Type Type { get; private set; }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the value of the member from the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The instance of which member's value will be returned.</param>
        internal object GetValue(object instance)
        {
            switch (MemberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)MemberInfo).GetValue(instance);
                case MemberTypes.Property:
                    return ((PropertyInfo)MemberInfo).GetValue(instance);
                default:
                    throw new ByamlException($"Invalid {nameof(ByamlMemberInfo)} type.");
            }
        }

        /// <summary>
        /// Sets the member to the given value for the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">The instance of which member will be set.</param>
        /// <param name="value">The value to set.</param>
        internal void SetValue(object instance, object value)
        {
            // If the member type is a nullable enum type, we need to wrap the value in a nullable.
            Type nullableType = Nullable.GetUnderlyingType(Type);
            if (nullableType != null && nullableType.GetTypeInfo().IsEnum)
            {
                value = Enum.ToObject(nullableType, value);
            }

            switch (MemberInfo.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)MemberInfo).SetValue(instance, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)MemberInfo).SetValue(instance, value);
                    break;
                default:
                    throw new ByamlException($"Invalid {nameof(ByamlMemberInfo)} type.");
            }
        }
    }
}
