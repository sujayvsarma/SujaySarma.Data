using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using SujaySarma.Data.Core.Attributes;
using SujaySarma.Data.Core.ReflectionUtilities;

namespace SujaySarma.Data.Core.TypeDiscovery;

/// <summary>
/// The <see cref="TypeDiscoveryFactory"/> discovers metadata about business entity class/struct/record 
/// types and stores them as ContainerInfo (this class). This structure further provides information 
/// on the members (properties and fields) and various other "configuration" set on them as appropriate 
/// for ORM purposes.
/// </summary>
public sealed class PersistenceContainerInfo
{
    /// <summary>
    /// The type of the business entity (class/struct/record) type.
    /// </summary>
    public Type EntityType
    {
        get; init;
    }

    /// <summary>
    /// The <see cref="IPersistenceContainer"/>-derived attribute we found on the <see cref="EntityType"/>.
    /// </summary>
    public IPersistenceContainer PersistenceInfo
    {
        get; init;
    }

    /// <summary>
    /// The collection of attributes we found on the <see cref="EntityType"/>. 
    /// </summary>
    public IReadOnlyList<Attribute> Attributes
    {
        get; init;
    }

    /// <summary>
    /// The collection of member properties/fields we discovered in the <see cref="EntityType"/>. This 
    /// includes only those members that had a <see cref="IPersistenceContainerMember"/> attribute decorated 
    /// on them.
    /// </summary>
    public IReadOnlyList<PersistenceContainerMemberInfo> Members
    {
        get; init;
    }

    /// <summary>
    /// An alias name for the reference - Eg: table aliases in SQL statements.
    /// </summary>
    public string ReferenceAlias
    {
        get; private set;
    }

    /// <summary>
    /// Try to retrieve an attribute of the specified <paramref name="attributeType"/>.
    /// </summary>
    /// <param name="attributeType">The type of <see cref="Attribute"/> to look for.</param>
    /// <param name="attribute">Instance of the attribute if found, else NULL.</param>
    /// <returns>True if the attribute was found.</returns>
    public bool TryGetAttribute(Type attributeType, [NotNullWhen(true)] out Attribute? attribute)
    {
        attribute = Attributes.Where(a => a.GetType().IsOrIsDerivedFrom(attributeType)).FirstOrDefault();
        return ((attribute != null) ? true : false);
    }

    /// <summary>
    /// Retrieve a member by the member property or field's name.
    /// </summary>
    /// <param name="memberName">Name of the member property or field.</param>
    /// <param name="memberInfo">The <see cref="PersistenceContainerMemberInfo"/> if found, else NULL.</param>
    /// <returns>True if the member was found.</returns>
    public bool TryGetMember(string memberName, [NotNullWhen(true)] out PersistenceContainerMemberInfo? memberInfo)
    {
        memberInfo = Members.Where(m => (m.Member.Name == memberName)).FirstOrDefault();
        return ((memberInfo != null) ? true : false);
    }

    /// <summary>
    /// Retrieve a member by the backend storage system's field name (eg: table column/field name).
    /// </summary>
    /// <param name="persistenceColumnName">Name of the table column/field.</param>
    /// <param name="memberInfo">The <see cref="PersistenceContainerMemberInfo"/> if found, else NULL.</param>
    /// <returns>True if the member was found.</returns>
    public bool TryGetMemberByPersistenceColumnName(string persistenceColumnName, [NotNullWhen(true)] out PersistenceContainerMemberInfo? memberInfo)
    {
        memberInfo = Members.Where(m =>
                            ((m.PersistenceInfo.TableFieldName == persistenceColumnName) || (m.PersistenceInfo.CreateQualifiedName() == persistenceColumnName))
                        ).FirstOrDefault();

        return ((memberInfo != null) ? true : false);
    }

    /// <summary>
    /// Retrieve members by what attributes they have anotated on them.
    /// </summary>
    /// <param name="attributes">The collection of attributes to look for. Also see <paramref name="mustHaveAllAttributes"/>.</param>
    /// <param name="mustHaveAllAttributes">When TRUE, the member must be anotated with all of the <paramref name="attributes"/>. Otherwise, 
    /// any one attribute is sufficient for a match.</param>
    /// <param name="members">The collection of <see cref="PersistenceContainerMemberInfo"/> that were found, or an empty collection (never NULL).</param>
    /// <returns>True if at least one matching member was found.</returns>
    public bool TryGetMembers(Type[] attributes, bool mustHaveAllAttributes, [NotNull] out PersistenceContainerMemberInfo[] members)
    {
        List<PersistenceContainerMemberInfo> results = new List<PersistenceContainerMemberInfo>();
        foreach (PersistenceContainerMemberInfo member in Members)
        {
            int matches = 0;
            if (attributes.Any(a => member.PersistenceInfo.GetType().IsOrIsDerivedFrom(a)))
            {
                ++matches;
            }

            foreach (Attribute attribute in member.Attributes)
            {
                if (attributes.Any(a => attribute.GetType().IsOrIsDerivedFrom(a)))
                {
                    ++matches;

                    if (!mustHaveAllAttributes)
                    {
                        break;
                    }
                }
            }

            if ((mustHaveAllAttributes && (matches == attributes.Length)) || ((!mustHaveAllAttributes) && (matches > 0)))
            {
                results.Add(member);
            }
        }

        members = results.ToArray();
        return ((results.Count > 0) ? true : false);
    }


    /// <summary>
    /// Retrieve the container name and the reference alias.
    /// </summary>
    /// <param name="name">Name of target container.</param>
    /// <param name="alias">Alias of the target container.</param>
    public void GetNameAndAlias(out string name, out string alias)
    {
        name = PersistenceInfo.CreateQualifiedName();
        alias = ReferenceAlias;
    }


    /// <summary>
    /// Initialise the metadata for the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type"><see cref="Type"/> of the business entity to get metadata about.</param>
    /// <param name="options">Options to use while discovering the <paramref name="type"/> and filter our results.</param>
    /// <param name="alias">The alias for this table. It is used directly.</param>
    internal PersistenceContainerInfo(Type type, TypeDiscoveryOptions options, string alias)
    {
        // If we came here through TDF, there is a very low chance of it being a generic TD, but check still!
        if (type.IsGenericTypeDefinition)
        {
            throw new TypeLoadException($"The type '{type.GetUsableTypeName()}' is an open-generic (type definition) and cannot be used for ORM.");
        }

        // Check if entity implements everything it is required to.
        if (options.EntityMustImplement.Count > 0)
        {
            int entityMustImplementMatchCount = 0;
            foreach (Type mustImplement in options.EntityMustImplement)
            {
                if (type.IsOrIsDerivedFrom(mustImplement))
                {
                    ++entityMustImplementMatchCount;
                }
            }

            if (entityMustImplementMatchCount != options.EntityMustImplement.Count)
            {
                throw new TypeLoadException($"The type '{type.GetUsableTypeName()}' does not implement one or more types specified in the 'EntityMustImplement' option.");
            }
        }

        // Check if it defines a persistence container.
        if (!type.TryGetAttribute(typeof(IPersistenceContainer), out Attribute? persistenceContainerAttribute))
        {
            throw new TypeLoadException($"The type '{type.GetUsableTypeName()}' is not annotated with a recognisable persistence container attribute (implementing '{nameof(IPersistenceContainer)}').");
        }

        // Check if the persistence container attribute is acceptable.
        if ((options.PersistenceContainerAttributeRestriction != null)
                && (!persistenceContainerAttribute.GetType().IsOrIsDerivedFrom(options.PersistenceContainerAttributeRestriction)))
        {
            throw new TypeLoadException($"The persistence container attribute of type '{persistenceContainerAttribute.GetType().GetUsableTypeName()}' does not implement '{options.PersistenceContainerAttributeRestriction.GetUsableTypeName()}' specified in the 'PersistenceAttributeMustBeOrMustImplement' option.");
        }

        // Retrieve other attributes on the type.
        // NOTE: As of .NET Core (5+ ??) this will return a few extra ones that .NET adds.
        List<Attribute> attributes = new List<Attribute>();
        foreach (Attribute attribute in type.GetCustomAttributes())
        {
            // Exclude the PersistenceContainer attribute, as we already pulled it just earlier!
            if (!Type.Equals(attribute, persistenceContainerAttribute))
            {
                attributes.Add(attribute);
            }
        }

        // Enumerate members.
        List<PersistenceContainerMemberInfo> members = new List<PersistenceContainerMemberInfo>();
        foreach (MemberInfo member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            // Only properties and fields & only if they are annoated with a PersistenceContainerMember attribute.
            if ((member.MemberType is MemberTypes.Property or MemberTypes.Field) && member.TryGetAttribute(typeof(IPersistenceContainerMember), out Attribute? persistenceMemberAtribute))
            {
                // And if that PCM is acceptable.
                if ((options.PersistenceContainerMemberAttributeRestriction != null)
                    && (!persistenceMemberAtribute.GetType().IsOrIsDerivedFrom(options.PersistenceContainerMemberAttributeRestriction)))
                {
                    continue;
                }

                members.Add(new PersistenceContainerMemberInfo(member, (IPersistenceContainerMember)persistenceMemberAtribute));
            }
        }

        // Finally we have everything required to complete the structure!
        EntityType = type;
        PersistenceInfo = (IPersistenceContainer)persistenceContainerAttribute;
        Attributes = attributes.AsReadOnly();
        Members = members.AsReadOnly();
        ReferenceAlias = alias;

        _originalDiscoveryOptions = options;
    }

    /// <summary>
    /// Performs all the tests that the constructor does to validate if the provided <paramref name="type"/> is 
    /// valid for ORM purposes, optionally satisfying the provided <paramref name="options"/>.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    /// <param name="options">Options for validation.</param>
    /// <returns>True if the type matches all relevant criteria.</returns>
    internal static bool ValidateForOrm(Type type, TypeDiscoveryOptions options)
    {
        // If we came here through TDF, there is a very low chance of it being a generic TD, but check still!
        if (type.IsGenericTypeDefinition)
        {
            return false;
        }

        // Check if entity implements everything it is required to.
        if (options.EntityMustImplement.Count > 0)
        {
            int entityMustImplementMatchCount = 0;
            foreach (Type mustImplement in options.EntityMustImplement)
            {
                if (type.IsOrIsDerivedFrom(mustImplement))
                {
                    ++entityMustImplementMatchCount;
                }
            }

            if (entityMustImplementMatchCount != options.EntityMustImplement.Count)
            {
                return false;
            }
        }

        // Check if it defines a persistence container.
        if (!type.TryGetAttribute(typeof(IPersistenceContainer), out Attribute? persistenceContainerAttribute))
        {
            return false;
        }

        // Check if the persistence container attribute is acceptable.
        if ((options.PersistenceContainerAttributeRestriction != null)
                && (!persistenceContainerAttribute.GetType().IsOrIsDerivedFrom(options.PersistenceContainerAttributeRestriction)))
        {
            return false;
        }

        // Enumerate members.
        int validMemberCount = 0;
        foreach (MemberInfo member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            // Only properties and fields & only if they are annoated with a PersistenceContainerMember attribute.
            if ((member.MemberType is MemberTypes.Property or MemberTypes.Field) && member.TryGetAttribute(typeof(IPersistenceContainerMember), out Attribute? persistenceMemberAtribute))
            {
                // And if that PCM is acceptable.
                if ((options.PersistenceContainerMemberAttributeRestriction != null)
                    && (!persistenceMemberAtribute.GetType().IsOrIsDerivedFrom(options.PersistenceContainerMemberAttributeRestriction)))
                {
                    continue;
                }

                ++validMemberCount;
            }
        }

        return ((validMemberCount > 0) ? true : false);
    }


    /// <summary>
    /// Check the discovered metadata satisfies the provided <paramref name="options"/>.
    /// </summary>
    /// <param name="options">Type discovery options and configuration.</param>
    /// <returns>True if original discovery satisfies new options.</returns>
    internal bool Satisifes(TypeDiscoveryOptions options)
    {
        // Fast path
        if (_originalDiscoveryOptions.IsEquivalentTo(options))
        {
            return true;
        }

        if (options.EntityMustImplement.Count > 0)
        {
            int entityMustImplementMatchCount = 0;
            foreach (Type mustImplement in options.EntityMustImplement)
            {
                if (EntityType.IsOrIsDerivedFrom(mustImplement))
                {
                    ++entityMustImplementMatchCount;
                }
            }

            if (entityMustImplementMatchCount != options.EntityMustImplement.Count)
            {
                return false;
            }
        }

        if ((options.PersistenceContainerAttributeRestriction != null)
                && (!PersistenceInfo.GetType().IsOrIsDerivedFrom(options.PersistenceContainerAttributeRestriction)))
        {
            return false;
        }

        if (options.PersistenceContainerMemberAttributeRestriction != null)
        {
            foreach (PersistenceContainerMemberInfo member in Members)
            {
                if (member.PersistenceInfo.GetType().IsOrIsDerivedFrom(options.PersistenceContainerMemberAttributeRestriction))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Check if the original discovery (current instance) was made with the same options provided 
    /// as the <paramref name="options"/>.
    /// </summary>
    /// <param name="options">Type discovery options and configuration.</param>
    /// <returns>True if original discovery was made with identical options.</returns>
    internal bool IsDiscoveredWithEquivalentOptions(TypeDiscoveryOptions? options)
        => _originalDiscoveryOptions.IsEquivalentTo((options ?? TypeDiscoveryOptions.Default));

    private TypeDiscoveryOptions _originalDiscoveryOptions;
}
