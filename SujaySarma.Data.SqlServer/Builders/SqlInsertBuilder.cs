using SujaySarma.Data.Core.TypeDiscovery;

using System;

namespace SujaySarma.Data.SqlServer.Builders;

/// <summary>
/// Helps build a SQL INSERT statement.
/// Supports: INSERT INTO VALUES, INSERT INTO FROM, TOP, WITH, injected values and OUTPUT.
/// </summary>
public sealed partial class SqlInsertBuilder : SqlStatementBuilder
{

    /// <summary>
    /// Initialise an instance of SqlInsertBuilder, specifying the type of the entity mapped to the "INTO" table for this query.
    /// </summary>
    /// <typeparam name="TTable"><see cref="Type"/> of entity mapped to the "INTO" table for this query.</typeparam>
    /// <returns>Initialised instance of SqlInsertBuilder.</returns>
    public static SqlInsertBuilder Into<TTable>()
        => new SqlInsertBuilder(typeof(TTable));


    /// <summary>
    /// Private constructor to prevent direct initialisation.
    /// </summary>
    /// <param name="type"><see cref="Type"/> of entity mapped to the "FROM" table for this query.</param>
    private SqlInsertBuilder(Type type)
        : base(type)
    {
    }
}
