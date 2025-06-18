using System.Reflection;

namespace SujaySarma.Data.Core.Reflection
{
    /// <summary>
    /// Resolved information about a container-member object. Container type objects are (exclusively): properties and fields.
    /// </summary>
    /// <remarks>
    ///     This class cannot be instantiated outside this library as all its constructors are internal or private.
    /// </remarks>
    public sealed class MemberTypeInfo
    {
        /// <summary>
        /// Name of the property or field member
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Definition of the member-column in the container
        /// </summary>
        public IMemberTypeAttribute Column { get; init; }

        /// <summary>
        /// The field or property's information from reflection
        /// </summary>
        public MemberInfo FieldOrPropertyInfo { get; init; }

        /// <summary>
        /// Initialise for a property
        /// </summary>
        /// <param name="property">PropertyInfo</param>
        /// <param name="containerMemberAttribute">The instance of <see cref="IMemberTypeAttribute" /> decorated on this property</param>
        internal MemberTypeInfo(PropertyInfo property, IMemberTypeAttribute containerMemberAttribute)
        {
            Name = property.Name;
            Column = containerMemberAttribute;
            FieldOrPropertyInfo = property;
        }

        /// <summary>
        /// Initialise for a field
        /// </summary>
        /// <param name="field">FieldInfo</param>
        /// <param name="containerMemberAttribute">The instance of <see cref="IMemberTypeAttribute" /> decorated on this field</param>
        internal MemberTypeInfo(FieldInfo field, IMemberTypeAttribute containerMemberAttribute)
        {
            Name = field.Name;
            Column = containerMemberAttribute;
            FieldOrPropertyInfo = field;
        }
    }
}
