using System;
using System.Reflection;

namespace SujaySarma.Data.Core.Reflection
{
    /// <summary>
    /// Resolved information about a container-member object. Container type objects are (exclusively): properties and fields.
    /// </summary>
    /// <remarks>
    ///     This class cannot be instantiated outside this library as all its constructors are internal or private. 
    /// </remarks>
    public sealed class ContainerMemberTypeInformation
    {
        /// <summary>
        /// Name of the property or field member
        /// </summary>
        public string Name 
        { 
            get; 
            init; 
        }

        /// <summary>
        /// Definition of the container system
        /// </summary>
        public IContainerMemberAttribute ContainerMemberDefinition 
        { 
            get; 
            init; 
        }

        /// <summary>
        /// The field or property's information from reflection
        /// </summary>
        public MemberInfo FieldOrPropertyInfo 
        { 
            get; 
            init; 
        }

        /// <summary>
        /// Initialise for a property
        /// </summary>
        /// <param name="property">PropertyInfo</param>
        /// <exception cref="TypeLoadException">Thrown when the <paramref name="property"/> is not decorated with an <see cref="IContainerMemberAttribute"/> attribute.</exception>
        internal ContainerMemberTypeInformation(PropertyInfo property)
        {
            Name = property.Name;
            ContainerMemberDefinition = GetContainerMemberAttribute(property) ?? throw new TypeLoadException($"The property '{Name}' is not decorated with a '{nameof(IContainerMemberAttribute)}' attribute.");
            FieldOrPropertyInfo = property;
        }

        /// <summary>
        /// Initialise for a field
        /// </summary>
        /// <param name="field">PropertyInfo</param>
        /// <exception cref="TypeLoadException">Thrown when the <paramref name="field"/> is not decorated with an <see cref="IContainerMemberAttribute"/> attribute.</exception>
        internal ContainerMemberTypeInformation(FieldInfo field)
        {
            Name = field.Name;
            ContainerMemberDefinition = GetContainerMemberAttribute(field) ?? throw new TypeLoadException($"The field '{Name}' is not decorated with a '{nameof(IContainerMemberAttribute)}' attribute.");
            FieldOrPropertyInfo = field;
        }


        /// <summary>
        /// Retrieve the IContainerMemberAttribute on the provided type.
        /// </summary>
        /// <param name="propertyOrField">Type of a property or field</param>
        /// <returns>The IContainerMemberAttribute or Null</returns>
        private static IContainerMemberAttribute? GetContainerMemberAttribute(MemberInfo propertyOrField)
        {
            Type IIContainerMemberAttribute = typeof(IContainerMemberAttribute);
            foreach (Attribute attribute in propertyOrField.GetCustomAttributes())
            {
                Type tAttribute = attribute.GetType();
                if (IIContainerMemberAttribute.IsAssignableFrom(tAttribute) && (!tAttribute.IsInterface))
                {
                    return (IContainerMemberAttribute)attribute;
                }
            }

            return null;
        }
    }
}
