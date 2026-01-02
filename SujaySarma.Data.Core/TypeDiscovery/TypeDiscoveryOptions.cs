using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using SujaySarma.Data.Core.Attributes;

namespace SujaySarma.Data.Core.TypeDiscovery;

/// <summary>
/// Options to control how type discovery functions. These options may vary for each 
/// call to retrieve a type from the type system.
/// </summary>
public struct TypeDiscoveryOptions
{

    /// <summary>
    /// Collection of types (classes, interfaces) that the business 
    /// entity must implement (derive from). If a potential entity does 
    /// not implement one of these, it will be ignored from both 
    /// type discovery and ORM.
    /// </summary>
    public IReadOnlyList<Type> EntityMustImplement
    {
        get; init;

    } = (new List<Type>()).AsReadOnly();

    /// <summary>
    /// When set, the business class entity (class/struct/record) must be annotated with 
    /// an attribute that is or derives from the type provided here. This is used to ensure 
    /// that the entity type serialises correctly for a specific persistence mechanism (a 
    /// database system, flatfile, etc). Value defaults to 'IPersistenceContainer'.
    /// </summary>
    public Type PersistenceContainerAttributeRestriction
    {
        get; init;

    } = typeof(IPersistenceContainer);

    /// <summary>
    /// When set, the business class entity (class/struct/record)'s member properties/fields 
    /// must be annotated with an attribute that is or derives from the type provided here. 
    /// This is used to ensure that the entity type serialises correctly for a specific 
    /// persistence mechanism (a database system, flatfile, etc). 
    /// Value defaults to 'IPersistenceContainerMember'.
    /// </summary>
    public Type PersistenceContainerMemberAttributeRestriction
    {
        get; init;

    } = typeof(IPersistenceContainerMember);

    /// <summary>
    /// When set, ensures that discovered/used containers have at least one 
    /// member property or field -- throws an exception in the absence of this.
    /// </summary>
    public bool MustHaveAtLeastOneMember
    {
        get; init;

    } = true;

    /// <summary>
    /// Default constructor (does nothing!)
    /// </summary>
    public TypeDiscoveryOptions() { }


    /// <summary>
    /// Default options.
    /// </summary>
    public static TypeDiscoveryOptions Default
        => _defaultOptions;

    private static TypeDiscoveryOptions _defaultOptions = new TypeDiscoveryOptions();

    #region --- Equality and Equivalence Checks ---

    /// <summary>
    /// Performs an element/element value comparison of the current instance with the provided <paramref name="tdo"/>.
    /// </summary>
    /// <param name="tdo">The target structure to compare with.</param>
    /// <returns>True if the current instance and the provided <paramref name="tdo"/> are **essentially** identical. This means that 
    /// when used for type discovery, both options would produce identical results.</returns>
    public bool IsEquivalentTo(TypeDiscoveryOptions tdo)
    {
        // Compare each value (as promised in the docs!)
        if ((!Type.Equals(PersistenceContainerAttributeRestriction, tdo.PersistenceContainerAttributeRestriction))
            || (!Type.Equals(PersistenceContainerMemberAttributeRestriction, tdo.PersistenceContainerMemberAttributeRestriction))
            || (MustHaveAtLeastOneMember != tdo.MustHaveAtLeastOneMember)
            || (EntityMustImplement.Count != tdo.EntityMustImplement.Count))
        {
            return false;
        }

        // For the purposes of this function, we don't care for the order -- only that everything is there.
        if (EntityMustImplement.Except(tdo.EntityMustImplement).Any()
            || tdo.EntityMustImplement.Except(EntityMustImplement).Any())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Performs an element/element value comparison of the current instance with the provided <paramref name="obj"/> (this should be non-NULL instance of <see cref="TypeDiscoveryOptions"/>).
    /// </summary>
    /// <param name="obj">The target structure to compare with.</param>
    /// <returns>True if the current instance and the provided <paramref name="obj"/> are value-identical.</returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if ((obj is null) || (obj is not TypeDiscoveryOptions tdo) || (GetHashCode() != tdo.GetHashCode()))
        {
            return false;
        }

        // Compare each value (as promised in the docs!)
        if ((!Type.Equals(PersistenceContainerAttributeRestriction, tdo.PersistenceContainerAttributeRestriction)) 
            || (!Type.Equals(PersistenceContainerMemberAttributeRestriction, tdo.PersistenceContainerMemberAttributeRestriction))
            || (MustHaveAtLeastOneMember != tdo.MustHaveAtLeastOneMember) 
            || (EntityMustImplement.Count != tdo.EntityMustImplement.Count))
        {
            return false;
        }

        for (int i = 0; i < EntityMustImplement.Count; i++)
        {
            if (!Type.Equals(EntityMustImplement[i], tdo.EntityMustImplement[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Performs an element/element value comparison of the two for equality.
    /// </summary>
    /// <param name="left">The left-side operand.</param>
    /// <param name="right">The right-side operand.</param>
    /// <returns>True if <paramref name="left"/> == <paramref name="right"/>.</returns>
    public static bool operator ==(TypeDiscoveryOptions left, TypeDiscoveryOptions right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Performs an element/element value comparison of the two for inequality.
    /// </summary>
    /// <param name="left">The left-side operand.</param>
    /// <param name="right">The right-side operand.</param>
    /// <returns>True if <paramref name="left"/> != <paramref name="right"/>.</returns>
    public static bool operator !=(TypeDiscoveryOptions left, TypeDiscoveryOptions right)
    {
        return (!left.Equals(right));
    }

    /// <summary>
    /// Create the hashcode for the instance.
    /// </summary>
    /// <returns>The computed hash value.</returns>
    public override int GetHashCode()
    {
        // Except for TableIndex (that is not useful for hashing and we don't care about its value 
        // for the purposes of type discovery, nothing else can be changed beyond the 
        // structure's init/ctor. So we can create the HashCode once and store it.

        if (_preComputedHashCode == 0)
        {
            HashCode hash = new HashCode();

            hash.Add(EntityMustImplement.Count);
            hash.Add((MustHaveAtLeastOneMember ? 1 : 0));
            hash.Add(PersistenceContainerAttributeRestriction);
            hash.Add(PersistenceContainerMemberAttributeRestriction);

            if (EntityMustImplement.Count > 0)
            {
                for (int i = 0; i < EntityMustImplement.Count; i++)
                {
                    hash.Add(EntityMustImplement[i]);
                }
            }

            _preComputedHashCode = hash.ToHashCode();
        }

        return _preComputedHashCode;
    }

    private int _preComputedHashCode = 0;

    #endregion
}