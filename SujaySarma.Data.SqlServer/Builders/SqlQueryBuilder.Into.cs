using System;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of INTO.
public sealed partial class SqlQueryBuilder
{

    /// <summary>
    /// Specify the name of the dynamically created table that the rows are to be inserted into.
    /// </summary>
    /// <typeparam name="TTable">Type of entity of dynamically created table to insert into.</typeparam>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder Into<TTable>()
    {
        return Into(
            base.ResolveType(typeof(TTable))
                .PersistenceInfo
                    .CreateQualifiedName());
    }

    /// <summary>
    /// Specify the name of the dynamically created table that the rows are to be inserted into.
    /// </summary>
    /// <param name="tableName">Name of the dynamically created table to insert into.</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder Into(string tableName)
    {
        if (!string.IsNullOrWhiteSpace(_intoTableName))
        {
            throw new InvalidOperationException("The table name for the INTO clause has already been set.");
        }

        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentNullException(nameof(tableName), "Name of the target table cannot be NULL or empty.");
        }

        _intoTableName = tableName.EnsureIdentifierIsQuoted();
        return this;

    }

    private string? _intoTableName = null;
}
