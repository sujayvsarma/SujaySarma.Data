using System;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders.Internal;

/// <summary>
/// Represents a collection of table ORDER BY clauses. Enumeration will yield a fully parsed ORDER BY clause as a string.
/// </summary>
internal sealed class SqlOrderBy : SqlClauseCollection
{

    /// <summary>
    /// Appends an ORDER BY...ASC.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of entity mapped to the table containing the column to order by.</typeparam>
    /// <param name="columnSelector">Lambda expression to select the columns to add. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }".</param>
    public void Asc<TTable>(Expression<Func<TTable, object>> columnSelector)
    {
        string columns = SqlExpressionParser.Parse(columnSelector);
        base.Add($"{columns} ASC");
    }

    /// <summary>
    /// Appends an ORDER BY...DESC.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of entity mapped to the table containing the column to order by.</typeparam>
    /// <param name="columnSelector">Lambda expression to select the columns to add. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }".</param>
    public void Desc<TTable>(Expression<Func<TTable, object>> columnSelector)
    {
        string columns = SqlExpressionParser.Parse(columnSelector);
        base.Add($"{columns} DESC");
    }


    /// <summary>
    /// Initialise the colletion.
    /// </summary>
    public SqlOrderBy()
        : base()
    {
    }

}
