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
    ///     Instead, callers must use the <see cref="TypeDiscoveryFactory" /> class.
    /// </remarks>
    public sealed class ContainerTypeInfo
    {
        /// <summary>
        /// Flags used to enumerate properties and fields of a given type
        /// </summary>
        private static readonly BindingFlags MEMBER_ENUMERATION_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// Name of the class
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Definition of the container system
        /// </summary>
        public IContainerAttribute Container { get; init; }

        /// <summary>
        /// Resolved information about this container type's properties and fields
        /// </summary>
        public Dictionary<string, MemberTypeInfo> Members { get; init; }

        /// <summary>
        /// Default constructor
        /// </summary>
        private ContainerTypeInfo()
        {
            Name = string.Empty;
            Container = default!;
            Members = new Dictionary<string, MemberTypeInfo>();
        }

        /// <summary>
        /// Initialise for a class, structure or record.
        /// </summary>
        /// <param name="classStructureOrRecordType">Type information of the class, structure or record</param>
        /// <exception cref="TypeLoadException">Thrown when the <paramref name="classStructureOrRecordType" /> is not decorated with an <see cref="IContainerAttribute" /> attribute.</exception>
        internal ContainerTypeInfo(Type classStructureOrRecordType)
          : this()
        {
            Name = classStructureOrRecordType.Name;

            Type containerAttributeType = typeof(IContainerAttribute);

            foreach (Attribute customAttribute in classStructureOrRecordType.GetCustomAttributes())
            {
                Type customAttributeType = customAttribute.GetType();
                if (containerAttributeType.IsAssignableFrom(customAttributeType) && !customAttributeType.IsInterface)
                {
                    Container = (IContainerAttribute)customAttribute;
                    break;
                }
            }

            if (Container == null)
            {
                throw new TypeLoadException($"The type '{Name}' is not decorated with a '{nameof(IContainerAttribute)}' attribute.");
            }
            
            foreach (MemberInfo member in classStructureOrRecordType.GetMembers(MEMBER_ENUMERATION_FLAGS))
            {
                Type memberAttributeType = typeof(IMemberTypeAttribute);
                IMemberTypeAttribute memberAttribute = default!;
                
                foreach (Attribute customAttribute in member.GetCustomAttributes())
                {
                    Type customAttributeType = customAttribute.GetType();
                    if (memberAttributeType.IsAssignableFrom(customAttributeType) && (!customAttributeType.IsInterface))
                    {
                        memberAttribute = (IMemberTypeAttribute)customAttribute;
                        break;
                    }
                }

                if (memberAttribute != null)
                {
                    switch (member)
                    {
                        case PropertyInfo property:
                            MemberTypeInfo mtiProperty = new MemberTypeInfo(property, memberAttribute);
                            Members.Add(mtiProperty.Name, mtiProperty);
                            break;

                        case FieldInfo field:
                            MemberTypeInfo mtiField = new MemberTypeInfo(field, memberAttribute);
                            Members.Add(mtiField.Name, mtiField);
                            break;
                    }
                }
            }
        }        
    }
}
