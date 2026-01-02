using System;
using System.Collections.Generic;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of INSERT FROM.
public sealed partial class SqlInsertBuilder
{
    /// <summary>
    /// Set the datasource of the INSERT statement to the provided <paramref name="query"/>.
    /// </summary>
    /// <param name="query">Instance of a <see cref="SqlQueryBuilder"/> populated with a SELECT statement that will provide the data to be inserted by the current statement.</param>
    /// <returns>Instance of self.</returns>
    public SqlInsertBuilder From(SqlQueryBuilder query)
        => From(query.Build(), query.ListQueryColumns);

    /// <summary>
    /// Set the datasource of the INSERT statement to the provided <paramref name="query"/>.
    /// </summary>
    /// <param name="query">Instance of a <see cref="StringBuilder"/> populated with a SELECT statement that will provide the data to be inserted by the current statement.</param>
    /// <param name="columnNames">[optional] Names of the columns that will be inserted.</param>
    /// <returns>Instance of self.</returns>
    public SqlInsertBuilder From(StringBuilder query, IEnumerable<string>? columnNames = null)
    {
        // If From() has already been called and column names were provided, _values[0] would have the column names.
        // therefore, the below exception will be confusing! THUS, we need to check _values AFTER we check _insertFromQuery.
        if (_insertFromQuery != null)
        {
            throw new InvalidOperationException("INSERT FROM query has already been set for this statement.");
        }

        if (_insertDefaultValues)
        {
            throw new InvalidOperationException("Cannot set INSERT FROM query when DEFAULT VALUES option has already been set for this statement.");
        }

        // If From() has already been called and column names were provided, _values[0] would have the column names.
        // therefore, the below exception will be confusing! THUS, we need to check _values AFTER we check _insertFromQuery.
        if (_values.Count > 0)
        {
            throw new InvalidOperationException("Cannot set INSERT FROM query when actual column values have already been set for this statement.");
        }

        // The minimum SELECT "SELECT * FROM [T]" has 17 characters.
        if (query.Length < 17)
        {
            throw new ArgumentException("The provided query is too short to be a valid SELECT statement.", nameof(query));
        }

        // add the column names to the _values collection so they can be used by the builder.
        if (columnNames is not null)
        {
            Dictionary<string, string>  columnRow = new Dictionary<string, string>();
            foreach (string col in columnNames)
            {
                if (string.IsNullOrWhiteSpace(col))
                {
                    throw new ArgumentException("Column names cannot be null or whitespace.", nameof(columnNames));
                }
                columnRow.Add(col.EnsureIdentifierIsQuoted(), default!);
            }

            _values.Add(columnRow);
        }

        _insertFromQuery = query;
        return this;
    }

    /// <summary>
    /// The INSERT FROM query.
    /// Cannot set this when ANY other values option is set (i.e., DEFAULT values, actual column values).
    /// </summary>
    private StringBuilder? _insertFromQuery = null;
}
