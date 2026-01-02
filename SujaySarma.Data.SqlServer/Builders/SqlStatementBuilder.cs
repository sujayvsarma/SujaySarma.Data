using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Collections.Generic;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders;

/// <summary>
/// A base class that provides functionality for our other 
/// SQL statementType builder implementations.
/// </summary>
public abstract class SqlStatementBuilder
{
    /// <summary>
    /// Assemble all components of the builder into the statementType.
    /// </summary>
    /// <returns>Instance of a <see cref="StringBuilder"/> that can then be serialised to a string.</returns>
    public virtual StringBuilder Build()
        => throw new NotImplementedException("Ouch! Fluid-statementType builder forgot to implement the Build() function!");


    /// <summary>
    /// Resolve the provided type. Checks the builder's cache first and then retrieves from the type-discovery system if not found.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to resolve.</param>
    /// <returns>Instance of <see cref="PersistenceContainerInfo"/> for the provided <paramref name="type"/>.</returns>
    internal PersistenceContainerInfo ResolveType(Type type)
    {
        if (!_addedTypes.TryGetValue(type, out PersistenceContainerInfo? pci))
        {
            pci = type.RetrievePersistenceContainerInfoOrThrowException();
            _addedTypes[type] = pci;
        }

        return pci;
    }

    /// <summary>
    /// Returns if the provided <paramref name="type"/> has been added to our collection.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check for.</param>
    /// <returns>TRUE if the <paramref name="type"/> has been added.</returns>
    internal bool IsAdded(Type type)
    {
        return _addedTypes.ContainsKey(type);
    }


    /// <summary>
    /// Validates that both <paramref name="type1"/> and <paramref name="type2"/> target the same destination table. 
    /// </summary>
    /// <param name="type1">The <see cref="Type"/> of one entity to check.</param>
    /// <param name="type2">The <see cref="Type"/> of the other entity to check.</param>
    /// <returns>True if both types target the same destination table.</returns>
    internal bool IsSameTableTarget(Type type1, Type type2)
    {
        // When types are the same, their destinations will match.
        if (type1 == type2)
        {
            return true;
        }

        // Resolve, fetch PCI and compare the actual targets.
        if (ResolveType(type1).PersistenceInfo.CreateQualifiedName() == ResolveType(type2).PersistenceInfo.CreateQualifiedName())
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check for validity and conflicts and append all <see cref="SqlHint"/>s in the <paramref name="hints"/> 
    /// to the internal collection. Either all hints are added, or none are -- appending hints happens only after all validations are successful.
    /// </summary>
    /// <param name="hints">An OR'ed collection of <see cref="SqlHint"/> to append.</param>
    /// <param name="statementType">The type of statement we are building -- Each element of <paramref name="hints"/> will 
    /// be checked for validity (against this statement type) and inter-hint conflict.</param>
    internal void AppendHints(SqlHint hints, SqlStatementType statementType)
    {
        if (!_hints.TryAdd(hints, statementType, out string? errorMessage))
        {
            throw new InvalidOperationException(errorMessage);
        }
    }

    /// <summary>
    /// Initialise the statement builder using the primary entity's mapped table.
    /// </summary>
    /// <param name="primaryTableType">Type of entity mapped to the primary entity for this statement or query.</param>
    protected SqlStatementBuilder(Type primaryTableType)
    {
        _addedTypes[primaryTableType] = primaryTableType.RetrievePersistenceContainerInfoOrThrowException();
        _primaryTable = _addedTypes[primaryTableType];
    }

    /// <summary>
    /// Query hints for the table. Each element is an INDIVIDUAL flag (not OR'ed!)
    /// </summary>
    protected readonly List<SqlHint> _hints = new List<SqlHint>();

    /// <summary>
    /// A statement-builder level cache of types that have been added to this statement. 
    /// </summary>
    protected readonly Dictionary<Type, PersistenceContainerInfo> _addedTypes = new Dictionary<Type, PersistenceContainerInfo>();

    /// <summary>
    /// The PCI for the entity added via the constructor.
    /// </summary>
    internal readonly PersistenceContainerInfo _primaryTable;
}
