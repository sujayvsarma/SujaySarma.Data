using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using SujaySarma.Data.Core.Attributes;
using SujaySarma.Data.Core.ReflectionUtilities;

namespace SujaySarma.Data.Core.TypeDiscovery;

/// <summary>
/// This structure stores metadata about a member property or field of a business class entity 
/// definitely anotated with an <see cref="IPersistenceContainer"/>-derived attribute.
/// </summary>
public sealed class PersistenceContainerMemberInfo
{
    /// <summary>
    /// The reflected <see cref="MemberInfo"/> of the business entity 
    /// property or field.
    /// </summary>
    public MemberInfo Member
    {
        get; init;
    }

    /// <summary>
    /// The <see cref="IPersistenceContainerMember"/>-derived attribute we found on the <see cref="Member"/>.
    /// </summary>
    public IPersistenceContainerMember PersistenceInfo
    {
        get; init;
    }

    /// <summary>
    /// The collection of attributes we found on the <see cref="Member"/>.
    /// </summary>
    public IReadOnlyList<Attribute> Attributes
    {
        get; init;
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
    /// Initialise metadata for the provided <paramref name="member"/>.
    /// </summary>
    /// <param name="member">The member to derive metadata for.</param>
    /// <param name="containerMemberAttribute">This was already discovered by the <see cref="PersistenceContainerInfo"/> that calls us, so why find it again!</param>
    public PersistenceContainerMemberInfo(MemberInfo member, IPersistenceContainerMember containerMemberAttribute)
    {
        // Collect all other member attributes
        List<Attribute> attributes = new List<Attribute>();
        foreach (Attribute attribute in member.GetCustomAttributes(inherit: true))
        {
            // because we already added that!
            if (!Type.Equals(attribute, containerMemberAttribute))
            {
                attributes.Add(attribute);
            }
        }

        Member = member;
        PersistenceInfo = containerMemberAttribute;
        Attributes = attributes.AsReadOnly();
    }
}