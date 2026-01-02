using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.SqlServer.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders.Merge;

/// <summary>
/// Causes an UPDATE action to be written to the parent WHEN clause of the MERGE statement.
/// </summary>
/// <typeparam name="TTarget">The <see cref="Type"/> of the target table/entity for the MERGE statement.</typeparam>
/// <typeparam name="TSource">The <see cref="Type"/> of the source table/entity being compared (INNER JOINed) with.</typeparam>
public sealed class UpdateAction<TTarget, TSource>
{

    /// <summary>
    /// Update values in the table mapped to <typeparamref name="TTarget"/> by from mapped values from the table pointed to by <typeparamref name="TSource"/>, 
    /// and (optionally) appended with <paramref name="additionalValues"/>.
    /// </summary>
    /// <param name="columnMappings">A dictionary providing mappings between the columns of <typeparamref name="TSource"/> and <typeparamref name="TTarget"/>.The expressions may provide: 
    /// (a) direct constant value or (b) value from another variable in scope or (c) a function that can be resolved and executed (client side) to then send a constant 
    /// value to SQL Server for the update or (d) a function that will be resolved to a SQL Server function to be executed by SQL Server during the update.</param>
    /// <param name="additionalValues">[optional] Values that would be appended to the row updated. Key: name of the destination column (need not be quoted, will be quoted by the function if not), 
    /// Value: an expression providing (a) direct constant value or (b) value from another variable in scope or (c) a function that can be resolved and executed (client side) to then send a constant 
    /// value to SQL Server for the update or (d) a function that will be resolved to a SQL Server function to be executed by SQL Server during the update.</param>
    /// <returns>The parent action choice to continue the builder.</returns>
    /// <example>
    ///     [C#]:
    ///         builder = SqlMergeBuilder.Merge{Order}()
    ///                 .Using{OrderTracking}()
    ///                     .Update()
    ///                         .Set{Order, OrderTracking}(
    ///                             new Dictionary{string, Expression{Func{OrderTracking, object}}}() {
    ///                                 { "Status", ot => ot.Status }
    ///                             },
    ///                             new Dictionary{string, Expression{Func{Order, object}}}() {
    ///                                 { "LastModified", o => DateTime.UtcNow }
    ///                             });
    ///             
    ///     [SQL]:
    ///         UPDATE  
    ///             SET
    ///                 O.Status = OT.Status,
    ///                 O.LastModified = GETUTCDATE()
    /// </example>
    public WhenBuilder<TTarget, TSource> Set(Dictionary<string, Expression<Func<TSource, object>>> columnMappings, 
        Dictionary<string, Expression<Func<TTarget, object>>>? additionalValues = null)
    {
        Type destinationType = typeof(TTarget);
        PersistenceContainerInfo container;
        Dictionary<string, string> values = new Dictionary<string, string>();

        // Type of destination should match primary table OR both must be mapped to the same destination table.        
        if (destinationType == _mergeBuilder._primaryTable.EntityType)
        {
            container = _mergeBuilder._primaryTable;
        }
        else
        {
            container = _mergeBuilder.ResolveType(destinationType);
            if (container.PersistenceInfo.CreateQualifiedName() != _mergeBuilder._primaryTable.PersistenceInfo.CreateQualifiedName())
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
                if (values.ContainsKey(qualifiedDestinationColumn))
                {
                    throw new InvalidOperationException($"The column '{member.PersistenceInfo.CreateQualifiedName()}' has already been mapped for update.");
                }

                values.Add(
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
                if (values.ContainsKey(name))
                {
                    throw new InvalidOperationException($"The column '{additionalColumnName}' has already been mapped for update.");
                }

                values.Add(
                        name,
                        SqlExpressionParser.Parse(additionalValues[additionalColumnName])
                    );
            }
        }

        _mergeBuilder.Write((new StringBuilder())
                    .AppendJoin(", ", values.Select(kv => $"{kv.Key} = {kv.Value}")).Append(' '));

        return _whenBuilder;
    }


    /// <summary>
    /// Initialise.
    /// </summary>
    /// <param name="mergeBuilder">Instance of the <see cref="SqlMergeBuilder{TTarget}"/> to return to.</param>
    /// <param name="whenBuilder">Instance of the parent <see cref="WhenBuilder{TTarget, TSource}"/> that instantiated this builder.</param>
    internal UpdateAction(SqlMergeBuilder<TTarget> mergeBuilder, WhenBuilder<TTarget, TSource> whenBuilder)
    {
        _mergeBuilder = mergeBuilder;
        _whenBuilder = whenBuilder;
    }

    /// <summary>
    /// The parent SqlMergeBuilder instance.
    /// </summary>
    private readonly SqlMergeBuilder<TTarget> _mergeBuilder;

    /// <summary>
    /// Instance of the parent WhenBuilder that instantiated this builder.
    /// </summary>
    private readonly WhenBuilder<TTarget, TSource> _whenBuilder;
}
