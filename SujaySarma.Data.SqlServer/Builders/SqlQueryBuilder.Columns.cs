using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of column specifier functions for the query.
public sealed partial class SqlQueryBuilder
{
    /// <summary>
    /// Select all columns from the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of entity to import columns from.</typeparam>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder Select<TEntity>()
    {
        Type type = typeof(TEntity);
        if (!TypeDiscoveryFactory.TryResolve(type, out PersistenceContainerInfo? container, SqlExtensions.GetSqlServerTypeDiscoveryOptions()))
        {
            throw new ArgumentException($"The type '{type.GetUsableTypeName()}' is not valid to be added to an SQL query or JOIN.");
        }

        foreach (PersistenceContainerMemberInfo member in container.Members)
        {
            // It is a Query, so all columns can be added.
            string qualifiedName = $"{container.ReferenceAlias}.{member.PersistenceInfo.CreateQualifiedName()}";
            if (!_columns.Contains(qualifiedName))
            {
                _columns.Add($"{container.ReferenceAlias}.{member.PersistenceInfo.CreateQualifiedName()}");
            }
        }

        return this;
    }

    /// <summary>
    /// Selects columns from <typeparamref name="TEntity"/> as specified by the <paramref name="columnSelector"/> lambda expression.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of entity to import columns from.</typeparam>
    /// <param name="columnSelector">A lambda expression that selects the member properties and fields of <typeparamref name="TEntity"/>, to then 
    /// select those columns from the table mapped to <typeparamref name="TEntity"/>. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }" Provide column aliases by assigning them to the 
    /// alias [for example: (e) => "alias" = e.PropertyName].</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder Select<TEntity>(Expression<Func<TEntity, object>> columnSelector)
    {
        string columnNames = SqlExpressionParser.Parse(columnSelector, SqlExpressionParser.AssignmentTreatment.AsAlias);
        foreach(string columnName in columnNames.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (!_columns.Contains(columnName))
            {
                _columns.Add(columnName);
            }
        }

        return this;
    }


    /// <summary>
    /// Returns a list of columns added to this query. 
    /// *** IMPORTANT! THIS SHOULD BE CALLED ONLY AFTER THE QUERY HAS BEEN FULLY POPULATED! ***
    /// </summary>
    internal List<string> ListQueryColumns
        => _columns;


    // The collection of [resolved] names of columns already added to the query.
    private readonly List<string> _columns = new List<string>();
}
