using SujaySarma.Data.SqlServer.Builders.Internal;

using System;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of column specifier functions for the query.
public sealed partial class SqlQueryBuilder
{
    /// <summary>
    /// Appends an ORDER BY...ASC.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of entity mapped to the table containing the column to order by.</typeparam>
    /// <param name="columnSelector">Lambda columnSelector to select the columns to add. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }".</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder OrderByASC<TTable>(Expression<Func<TTable, object>> columnSelector)
    {
        base.ResolveType(typeof(TTable));

        _orderBy.Asc<TTable>(columnSelector);
        return this;
    }

    /// <summary>
    /// Appends an ORDER BY...DESC.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of entity mapped to the table containing the column to order by.</typeparam>
    /// <param name="columnSelector">Lambda expression to select the columns to add. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }".</param>
    /// <returns>Self-instance</returns>
    public SqlQueryBuilder OrderByDESC<TTable>(Expression<Func<TTable, object>> columnSelector)
    {
        base.ResolveType(typeof(TTable));

        _orderBy.Desc<TTable>(columnSelector);
        return this;
    }

    private readonly SqlOrderBy _orderBy = new SqlOrderBy();
}
