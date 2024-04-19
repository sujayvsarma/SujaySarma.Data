using System;
using System.Collections.Generic;
using System.Reflection;

namespace SujaySarma.Data.Core.Reflection
{
    /// <summary>
    /// Resolved information about a container-type object. Container type objects are (exclusively): classes, structs and records.
    /// </summary>
    /// <remarks>
    ///     This class cannot be instantiated outside this library as all its constructors are internal or private. 
    ///     Instead, callers must use the <see cref="TypeDiscoveryFactory"/> class.
    /// </remarks>
    public sealed class ContainerTypeInformation
    {
        /// <summary>
        /// Name of the class
        /// </summary>
        public string Name 
        { 
            get; init; 
        }

        /// <summary>
        /// Definition of the container system
        /// </summary>
        public IContainerAttribute ContainerDefinition 
        { 
            get; init; 
        }

        /// <summary>
        /// Resolved information about this container type's properties and fields
        /// </summary>
        public Dictionary<string, ContainerMemberTypeInformation> Members 
        { 
            get; init; 
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        private ContainerTypeInformation()
        {
            Name = string.Empty;
            ContainerDefinition = default!;
            Members = new Dictionary<string, ContainerMemberTypeInformation>();
        }

        /// <summary>
        /// Initialise for a class, structure or record.
        /// </summary>
        /// <param name="classStructureOrRecordType">Type information of the class, structure or record</param>
        /// <exception cref="TypeLoadException">Thrown when the <paramref name="classStructureOrRecordType"/> is not decorated with an <see cref="IContainerAttribute"/> attribute.</exception>
        internal ContainerTypeInformation(Type classStructureOrRecordType)
            : this()
        {
            Name = classStructureOrRecordType.Name;
            ContainerDefinition = GetContainerAttribute(classStructureOrRecordType) ?? throw new TypeLoadException($"The type '{Name}' is not decorated with a '{nameof(IContainerAttribute)}' attribute.");

            foreach(MemberInfo member in classStructureOrRecordType.GetMembers(MEMBER_ENUMERATION_FLAGS))
            {
                Type IIContainerMemberAttribute = typeof(IContainerMemberAttribute);
                IContainerMemberAttribute? containerMemberAttribute = null;
                foreach (Attribute attribute in member.GetCustomAttributes())
                {
                    Type tAttribute = attribute.GetType();
                    if (IIContainerMemberAttribute.IsAssignableFrom(tAttribute) && (!tAttribute.IsInterface))
                    {
                        containerMemberAttribute = attribute as IContainerMemberAttribute;
                        break;
                    }
                }

                if (containerMemberAttribute != null)
                {
                    if (member is PropertyInfo pi)
                    {
                        ContainerMemberTypeInformation memberType = new ContainerMemberTypeInformation(pi, containerMemberAttribute);
                        Members.Add(memberType.Name, memberType);
                    }
                    else if (member is FieldInfo fi)
                    {
                        ContainerMemberTypeInformation memberType = new ContainerMemberTypeInformation(fi, containerMemberAttribute);
                        Members.Add(memberType.Name, memberType);
                    }
                }               
            }
        }


        /// <summary>
        /// Retrieve the IContainerAttribute on the provided type.
        /// </summary>
        /// <param name="classOrRecordType">Type of a class, structure or record</param>
        /// <returns>The IContainerAttribute or Null</returns>
        private static IContainerAttribute? GetContainerAttribute(Type classOrRecordType)
        {
            Type IIContainerAttribute = typeof(IContainerAttribute);
            foreach (Attribute attribute in classOrRecordType.GetCustomAttributes())
            {
                Type tAttribute = attribute.GetType();
                if (IIContainerAttribute.IsAssignableFrom(tAttribute) && (!tAttribute.IsInterface))
                {
                    return (IContainerAttribute)attribute;
                }
            }

            return null;
        }

        /// <summary>
        /// Flags used to enumerate properties and fields of a given type
        /// </summary>
        private static readonly BindingFlags MEMBER_ENUMERATION_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    }
}
