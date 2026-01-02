using SujaySarma.Data.Core.TypeDiscovery;

using System;

namespace SujaySarma.Data.SqlServer.Builders;

/// <summary>
/// Helps build a SQL DELETE statement.
/// Supports: TOP, WITH, FROM, JOIN, OUTPUT
/// </summary>
public sealed partial class SqlDeleteBuilder : SqlStatementBuilder
{

    /// <summary>
    /// Initialise an instance of SqlDeleteBuilder, specifying the type of the entity mapped to the "FROM" table for this query.
    /// </summary>
    /// <typeparam name="TTable"><see cref="Type"/> of entity mapped to the "FROM" table for this query.</typeparam>
    /// <returns>Initialised instance of SqlDeleteBuilder.</returns>
    public static SqlDeleteBuilder From<TTable>()
        => new SqlDeleteBuilder(typeof(TTable));

    /// <summary>
    /// Private constructor to prevent direct initialisation.
    /// </summary>
    /// <param name="deleteFromTable"><see cref="Type"/> of the table to delete records from.</param>
    private SqlDeleteBuilder(Type deleteFromTable)
        : base(deleteFromTable)
    {
    }
}
