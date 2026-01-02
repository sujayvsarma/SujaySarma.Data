using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.SqlServer.Attributes;

using System;

namespace SujaySarma.Data.SqlServer.Builders;

/// <summary>
/// Helps build a SQL query (SELECT) statement. 
/// Supports: TOP, DISTINCT, INTO, JOINS, WHERE, GROUP BY, ORDER BY.
/// </summary>
public sealed partial class SqlQueryBuilder : SqlStatementBuilder
{

    /// <summary>
    /// When set, will include deleted rows in the result set. 
    /// This flag is used only if the table supports softdeletion (i.e., is annotated with a <see cref="SqlTableWithSoftDelete"/> attribute 
    /// instead of <see cref="SqlTable"/>).
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder IncludingDeletedRows()
    {
        _includeDeletedRows = true;
        return this;
    }

    /// <summary>
    /// Initialise an instance of SqlQueryBuilder, specifying the type of the entity mapped to the "FROM" table for this query.
    /// </summary>
    /// <typeparam name="TTable"><see cref="Type"/> of entity mapped to the "FROM" table for this query.</typeparam>
    /// <returns>Initialised instance of SqlQueryBuilder.</returns>
    public static SqlQueryBuilder From<TTable>()
        => new SqlQueryBuilder(typeof(TTable));

    /// <summary>
    /// Private constructor to prevent direct initialisation.
    /// </summary>
    /// <param name="type"><see cref="Type"/> of entity mapped to the "FROM" table for this query.</param>
    private SqlQueryBuilder(Type type)
        : base(type)
    {
    }


    /// <summary>
    /// Returns the primary table added above in From().
    /// </summary>
    internal PersistenceContainerInfo PrimaryTable 
        => _primaryTable;

    /// <summary>
    /// Flag when SET will INCLUDE deleted rows.
    /// </summary>
    private bool _includeDeletedRows = false;
}
