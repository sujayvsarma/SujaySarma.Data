using SujaySarma.Data.Core.TypeDiscovery;

using System;

namespace SujaySarma.Data.SqlServer.Builders;

/// <summary>
/// Helps build a SQL UPDATE statement.
/// Supports: TOP, WITH, FROM, JOIN, OUTPUT
/// </summary>
public sealed partial class SqlUpdateBuilder : SqlStatementBuilder
{

    /// <summary>
    /// Initialise an instance of SqlUpdateBuilder, specifying the type of the entity mapped to the "INTO" table for this query.
    /// </summary>
    /// <typeparam name="TTable"><see cref="Type"/> of entity mapped to the "INTO" table for this query.</typeparam>
    /// <returns>Initialised instance of SqlUpdateBuilder.</returns>
    public static SqlUpdateBuilder Into<TTable>()
        => new SqlUpdateBuilder(typeof(TTable));


    /// <summary>
    /// Private constructor to prevent direct initialisation.
    /// </summary>
    /// <param name="type"><see cref="Type"/> of entity mapped to the "FROM" table for this query.</param>
    private SqlUpdateBuilder(Type type)
        : base(type)
    {
    }
}
