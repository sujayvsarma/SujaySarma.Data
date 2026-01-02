using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementatio of SET and UPDATE FROM.
public sealed partial class SqlUpdateBuilder
{

    #region UPDATE FROM

    /// <summary>
    /// Update values in the table mapped to <typeparamref name="TDestination"/> by INNER JOINing with the table pointed to by <typeparamref name="TSource"/>, 
    /// and (optionally) appended with <paramref name="additionalValues"/>.
    /// </summary>
    /// <typeparam name="TDestination">Type mapped to the SQL Server destination table. Must (a) match the type used with <see cref="Into{TEntity}()"/> during initialisation of this builder 
    /// or (b) The type 'TEntity' is annotated with a <see cref="Attributes.SqlTable"/> to the same table name as the initialised (<see cref="Into{TEntity}()"/>) type.</typeparam>
    /// <typeparam name="TSource">Type mapped to a SQL Server table that will be INNER JOINed with <typeparamref name="TDestination"/>. This may or may not be the same as <typeparamref name="TDestination"/> (i.e., self-joins are supported).</typeparam>
    /// <param name="innerJoinCondition"><typeparamref name="TSource"/> will be INNER JOINed with <typeparamref name="TDestination"/>. Provide the "ON" condition through this expression.</param>
    /// <param name="innerJoinHints">SQL hints for the <paramref name="innerJoinCondition"/> condition.</param>
    /// <param name="columnMappings">A dictionary providing mappings between the columns of <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>.The expressions may provide: 
    /// (a) direct constant value or (b) value from another variable in scope or (c) a function that can be resolved and executed (client side) to then send a constant 
    /// value to SQL Server for the update or (d) a function that will be resolved to a SQL Server function to be executed by SQL Server during the update.</param>
    /// <param name="additionalValues">[optional] Values that would be appended to the row updated. Key: name of the destination column (need not be quoted, will be quoted by the function if not), 
    /// Value: an expression providing (a) direct constant value or (b) value from another variable in scope or (c) a function that can be resolved and executed (client side) to then send a constant 
    /// value to SQL Server for the update or (d) a function that will be resolved to a SQL Server function to be executed by SQL Server during the update.</param>
    /// <returns>Instance of self.</returns>
    /// <example>
    ///     [C#]:
    ///         builder = SqlUpdateBuilder.Using{Order}();
    ///         builder.UpdateFrom{Order, OrderTracking}(
    ///             (o, ot) => o.Id = ot.OrderId,
    ///             SqlQueryHint.NoLock,
    ///             new Dictionary{string, Expression{Func{OrderTracking, object}}}() {
    ///                 { "Status", ot => ot.Status }
    ///             },
    ///             new Dictionary{string, Expression{Func{Order, object}}}() {
    ///                 { "LastModified", o => DateTime.UtcNow }
    ///             });
    ///             
    ///     [SQL]:
    ///         UPDATE O 
    ///             SET
    ///                 O.Status = OT.Status,
    ///                 O.LastModified = GETUTCDATE()
    ///         FROM Orders O 
    ///         INNER JOIN OrderTracking OT WITH (NOLOCK) ON (O.Id = OT.OrderId)
    ///         ...
    /// </example>
    public SqlUpdateBuilder UpdateFrom<TDestination, TSource>(
            Expression<Func<TDestination, TSource, bool>> innerJoinCondition,
            SqlHint innerJoinHints,
            Dictionary<string, Expression<Func<TSource, object>>> columnMappings,
            Dictionary<string, Expression<Func<TDestination, object>>>? additionalValues = null
        )
    {
        ThrowIfUpdateModeSet(UpdateMode.FromJoin);

        Type destinationType = typeof(TDestination);

        // check if we have already seen this type before.
        if (_updateFromTypes.Contains(destinationType))
        {
            throw new ArgumentException("The provided destination type has already been mapped.", destinationType.GetUsableTypeName());
        }

        PersistenceContainerInfo container;

        // Type of destination should match primary table OR both must be mapped to the same destination table.        
        if (destinationType == _primaryTable.EntityType)
        {
            container = _primaryTable;
        }
        else
        {
            container = base.ResolveType(destinationType);
            if (container.PersistenceInfo.CreateQualifiedName() != _primaryTable.PersistenceInfo.CreateQualifiedName())
            {
                throw new ArgumentException("The provided destination type does not map to the target table type.", destinationType.GetUsableTypeName());
            }
        }

        // We have already validated we have not enumerated TTarget's members before, so do that now.
        foreach (PersistenceContainerMemberInfo member in container.Members)
        {
            if (member.PersistenceInfo is SqlTablePrimaryKeyColumn)
            {
                continue;
            }

            if (columnMappings.ContainsKey(member.PersistenceInfo.TableFieldName))
            {
                string qualifiedDestinationColumn = $"{container.ReferenceAlias}.{member.PersistenceInfo.CreateQualifiedName()}";
                if (_updateFromColumnMappings.ContainsKey(qualifiedDestinationColumn))
                {
                    throw new InvalidOperationException($"The column '{member.PersistenceInfo.CreateQualifiedName()}' has already been mapped for update.");
                }

                _updateFromColumnMappings.Add(
                        qualifiedDestinationColumn,
                        SqlExpressionParser.Parse(columnMappings[member.PersistenceInfo.TableFieldName])
                    );
            }
        }

        // Evaluate the provided additionalValues and add them. Remember each "value" is actually an Expression.
        if ((additionalValues is not null) && (additionalValues.Count > 0))
        {
            foreach (string additionalColumnName in additionalValues.Keys)
            {
                string name = $"{container.ReferenceAlias}.{additionalColumnName.EnsureIdentifierIsQuoted()}";
                if (_updateFromColumnMappings.ContainsKey(name))
                {
                    throw new InvalidOperationException($"The column '{additionalColumnName}' has already been mapped for update.");
                }

                _updateFromColumnMappings.Add(
                        name,
                        SqlExpressionParser.Parse(additionalValues[additionalColumnName])
                    );
            }
        }

        // Add the implicit inner join.
        InnerJoin<TDestination, TSource>(innerJoinCondition, innerJoinHints);

        // Set the update mode.
        _mode = UpdateMode.FromJoin;

        // Add to Type tracker
        _updateFromTypes.Add(destinationType);

        return this;
    }


    #endregion

    #region SET values -- serialise entity + additional values

    /// <summary>
    /// Provide data that would be updated into the destination table through serialisation of the provided <paramref name="entities"/> instance, 
    /// and (optionally) appended with <paramref name="additionalValues"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of <paramref name="entities"/>. Must: (a) match the type used with <see cref="Into{TEntity}()"/> during initialisation of this builder 
    /// or (b) The type 'TEntity' is annotated with a <see cref="Attributes.SqlTable"/> to the same table name as the initialised (<see cref="Into{TEntity}()"/>) type.</typeparam>
    /// <param name="additionalValues">[optional] Values that would be appended to the row updated. Key: name of the destination column (need not be quoted, will be quoted by the function if not), 
    /// Value: an expression providing (a) direct constant value or (b) value from another variable in scope or (c) a function that can be resolved and executed (client side) to then send a constant 
    /// value to SQL Server for the update or (d) a function that will be resolved to a SQL Server function to be executed by SQL Server during the update.</param>
    /// <param name="entities">Instances of entities of type <typeparamref name="TEntity"/> to serialise into the database table.</param>
    /// <returns>Instance of self.</returns>
    /// <example>
    ///     [C#]: 
    ///         builder = SqlUpdateBuilder.Using{Customer}(); 
    ///         builder.Set{Customer}(new Customer[] { cust1, cust2, cust3 }, new Dictionary{string, Expression{Func{Customer, object}}}() { "LastModified", c => c.LastModified = DateTime.UtcNow });
    ///         
    ///     [SQL]: 
    ///         for each (cust1, cust2...) generates: 
    ///         UPDATE [dbo].[Customers] SET ...(col/columnNames from custX)..., [LastModified] = GETUTCDATE() WHERE ...
    /// </example>
    public SqlUpdateBuilder Set<TEntity>(Dictionary<string, Expression<Func<TEntity, object>>>? additionalValues = null, params IEnumerable<TEntity> entities)
    {
        // Check if mode has been set.
        ThrowIfUpdateModeSet(UpdateMode.Serialised);

        // entity should not be NULL.
        if (entities is null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        // materialise the collection to a concrete List<T>, this helps with a bunch of things later on.
        List<TEntity> entitiesList = entities.Materialise<TEntity>(acceptNullElements: false, throwExceptionOnNull: true);
        if (entitiesList.Count is 0)
        {
            throw new ArgumentException("Sequence is empty.", nameof(entities));
        }

        // Type of entity should match primary table OR both must be mapped to the same destination table.
        Type entityType = entitiesList[0]!.GetType();
        PersistenceContainerInfo container = base.ResolveType(entityType);
        if ((entityType != _primaryTable.EntityType)
            && (container.PersistenceInfo.CreateQualifiedName() != _primaryTable.PersistenceInfo.CreateQualifiedName()))
        {
            throw new ArgumentException("The provided entity type does not map to the target table type.", entityType.GetUsableTypeName());
        }

        // Pre-evaluate the provided additionalValues. Remember each "value" is actually an Expression.
        Dictionary<string, string> evaluatedAdditionalValues = new Dictionary<string, string>();
        if ((additionalValues is not null) && (additionalValues.Count > 0))
        {
            foreach (string additionalColumnName in additionalValues.Keys)
            {
                evaluatedAdditionalValues.Add(
                        additionalColumnName.EnsureIdentifierIsQuoted(),
                        SqlExpressionParser.Parse(additionalValues[additionalColumnName])
                    );
            }
        }

        // process each entity in the list and serialise.
        foreach (TEntity entity in entitiesList)
        {
            Dictionary<string, string> row = SerializeEntityToRow<TEntity>(entity, container.ReferenceAlias);

            // copy over additional columnNames.
            if (evaluatedAdditionalValues.Count > 0)
            {
                foreach (string additionalColumnName in evaluatedAdditionalValues.Keys)
                {
                    // additionalColumnName was already quoted in the pre-evaluation loop!
                    if (row.ContainsKey(additionalColumnName))
                    {
                        throw new InvalidOperationException($"The column '{additionalColumnName}' has already been added from the entity.");
                    }

                    row.Add(
                            additionalColumnName,
                            evaluatedAdditionalValues[additionalColumnName]
                        );
                }
            }

            // append row to collection
            _values.Add(row);
        }

        // Set mode.
        _mode = UpdateMode.Serialised;

        return this;
    }


    /// <summary>
    /// Provide data that would be updated into the destination table through serialisation of the provided <paramref name="entity"/> instance, 
    /// and (optionally) appended with <paramref name="additionalValues"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of <paramref name="entity"/>. Must: (a) match the type used with <see cref="Into{TEntity}()"/> during initialisation of this builder 
    /// or (b) The type 'TEntity' is annotated with a <see cref="Attributes.SqlTable"/> to the same table name as the initialised (<see cref="Into{TEntity}()"/>) type.</typeparam>
    /// <param name="entity">An entity instance of type <typeparamref name="TEntity"/> to serialise into the database table.</param>
    /// <param name="additionalValues">[optional] Values that would be appended to the row updated. Key: name of the destination column (need not be quoted, will be quoted by the function if not), 
    /// Value: an expression providing (a) direct constant value or (b) value from another variable in scope or (c) a function that can be resolved and executed (client side) to then send a constant 
    /// value to SQL Server for the update or (d) a function that will be resolved to a SQL Server function to be executed by SQL Server during the update.</param>
    /// <returns>Instance of self.</returns>
    /// <example>
    ///     [C#]: 
    ///         builder = SqlUpdateBuilder.Using{Customer}(); 
    ///         builder.Set{Customer}(cust, new Dictionary{string, Expression{Func{Customer, object}}}() { "LastModified", c => c.LastModified = DateTime.UtcNow });
    ///         
    ///     [SQL]: 
    ///         UPDATE [dbo].[Customers] SET (...col/columnNames from cust...), [LastModified] = GETUTCDATE() WHERE ...
    /// </example>
    public SqlUpdateBuilder Set<TEntity>(TEntity entity, Dictionary<string, Expression<Func<TEntity, object>>>? additionalValues = null)
    {
        // Check if mode has been set.
        ThrowIfUpdateModeSet(UpdateMode.Serialised);

        // entity should not be NULL.
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        // Type of entity should match primary table OR both must be mapped to the same destination table.
        Type entityType = entity.GetType();
        PersistenceContainerInfo container = base.ResolveType(entityType);
        if ((entityType != _primaryTable.EntityType)
            && (container.PersistenceInfo.CreateQualifiedName() != _primaryTable.PersistenceInfo.CreateQualifiedName()))
        {
            throw new ArgumentException("The provided entity type does not map to the target table type.", entityType.GetUsableTypeName());
        }

        Dictionary<string, string> row = SerializeEntityToRow(entity, container.ReferenceAlias);

        // Add additional values, checking that columns don't already exist.
        if ((additionalValues is not null) && (additionalValues.Count > 0))
        {
            foreach (string additionalColumnName in additionalValues.Keys)
            {
                string quoteIt = $"{container.ReferenceAlias}.{additionalColumnName.EnsureIdentifierIsQuoted()}";
                if (row.ContainsKey(quoteIt))
                {
                    throw new InvalidOperationException($"The additional column '{additionalColumnName}' is already set from the entity.");
                }

                row.Add(
                        quoteIt,
                        SqlExpressionParser.Parse(additionalValues[additionalColumnName])
                    );
            }
        }

        // Set the global vars
        _mode = UpdateMode.Serialised;
        _values.Add(row);

        return this;
    }

    #endregion

    /// <summary>
    /// Check if the _mode has already been set to something other than NotSet, and throw an exception if it is.
    /// </summary>
    /// <param name="thisModeIsAlright">Value of UpdateMode that we are planning to set -- if it is already set, it is perfectly alright.</param>
    private void ThrowIfUpdateModeSet(UpdateMode thisModeIsAlright)
    {
        if ((_mode is not UpdateMode.NotSet) && (_mode != thisModeIsAlright))
        {
            throw new InvalidOperationException("A different update mode has already been chosen.");
        }
    }

    /// <summary>
    /// Serialise the provided <paramref name="entity"/> to a row dictionary.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of entity <paramref name="entity"/>.</typeparam>
    /// <param name="entity">The entity object to serialise.</param>
    /// <param name="referenceAlias">The table reference alias for <typeparamref name="TTable"/></param>
    /// <returns>A row-dictionary: column name/value pairs with the identifiers and values suitably quoted.</returns>
    private Dictionary<string, string> SerializeEntityToRow<TTable>(TTable entity, string referenceAlias)
    {
        Dictionary<string, string> row = new Dictionary<string, string>();
        foreach (PersistenceContainerMemberInfo member in _primaryTable.Members)
        {
            // since we are INSERTing, skip the auto-populated colums.
            if (member.PersistenceInfo is not SqlTablePrimaryKeyColumn)
            {
                object? value = entity.GetValue(member, useAutoPopulate: true);
                string stringifiedValue = value.GetSQLStringValue();

                row.Add(
                        $"{referenceAlias}.{member.PersistenceInfo.CreateQualifiedName()}",
                        stringifiedValue
                    );
            }
        }

        return row;
    }

    /// <summary>
    /// List of rows -- each element of list (dict[string,string]) is a row -- each element of dict is a column/value pair. 
    /// Column names are already quoted, values are stringified/quoted/escaped.
    /// Cannot set this when ANY other values option is set (i.e., DEFAULT values above, INSERT FROM query).
    /// </summary>
    private readonly List<Dictionary<string, string>> _values = new List<Dictionary<string, string>>();

    /// <summary>
    /// if _updateMode = UpdateFrom, Build() will use this dictionary to build the SET clause.
    /// </summary>
    private Dictionary<string, string> _updateFromColumnMappings = new Dictionary<string, string>();

    /// <summary>
    /// The types that we have already mapped columns for in UPDATE FROM mode.
    /// </summary>
    private HashSet<Type> _updateFromTypes = new HashSet<Type>();


    /// <summary>
    /// UpdateMany mode. Initial value is set to NotSet (this is an invalid state!)
    /// </summary>
    private UpdateMode _mode = UpdateMode.NotSet;




    private enum UpdateMode
    {
        /// <summary>
        /// Initial value, NOT SET!
        /// </summary>
        NotSet = -1,

        /// <summary>
        /// Serialised from provided entities.
        /// Translates to an "UPDATE TABLE SET..." statement.
        /// </summary>
        Serialised = 1,

        /// <summary>
        /// From a query or table join.
        /// Translates to an "UPDATE TABLE SET... FROM..." statement.
        /// </summary>
        FromJoin = 2
    }
}
