using SujaySarma.Data.SqlServer.Builders.Internal;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of OUTPUT clause.
public sealed partial class SqlUpdateBuilder
{
    #region Output - column specifiers

    /// <summary>
    /// Creates or appends an OUTPUT clause that outputs columns from the specified <typeparamref name="TTable"/>. If a 
    /// column selector is specified, then only the columns returned by that selector are included.
    /// </summary>
    /// <typeparam name="TTable">Type of entity whose eligible columns are to be returned.</typeparam>
    /// <param name="columnSelector">The lambda expression that will return a list of column names (optionally with aliases) for the eligible member properties/fields of the provided type. 
    /// For a single member/column, use an expression such as "(e) => e.Property", or to alias it: "(e) => "alias" = e.Property". When selecting multiple columns, this becomes: 
    /// "(e) => new { e.Prop1, e.Prop2...}" or with aliases as: "(e) => new { "col1" = e.Prop1, "col2" = e.Prop2...}. Alias only the columns you need to!</param>
    /// <returns>Instance of self.</returns>
    public SqlUpdateBuilder Output<TTable>(Expression<Func<TTable, object>>? columnSelector = null)
    {
        if (_output is null)
        {
            _output = SqlOutput.WithColumns(columnSelector);
        }
        else
        {
            _output.AddColumns<TTable>(columnSelector);
        }

        return this;
    }

    /// <summary>
    /// Creates or appends an OUTPUT clause that outputs all columns from the INSERTED and DELETED tables.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlUpdateBuilder OutputUpdated()
    {
        if (_output is null)
        {
            _output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.UPDATED);
        }
        else
        {
            _output.AddAllColumns(SqlOutput.EphermeralTableName.UPDATED);
        }

        return this;
    }

    /// <summary>
    /// Creates or appends an OUTPUT clause that outputs the specified columns from the INSERTED and DELETED tables.
    /// </summary>
    /// <param name="columnNames">An array of column names to include in the output. Each name must correspond to a valid column in the specified
    /// table. Cannot be null or contain null or empty elements.</param>
    /// <returns>Instance of self.</returns>
    public SqlUpdateBuilder OutputUpdated(params IEnumerable<string> columnNames)
    {
        if (_output is null)
        {
            _output = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.UPDATED, columnNames);
        }
        else
        {
            _output.AddColumns(SqlOutput.EphermeralTableName.UPDATED, columnNames);
        }

        return this;
    }

    /// <summary>
    /// Creates or appends an OUTPUT clause that outputs the specified columns from the INSERTED and DELETED tables.
    /// </summary>
    /// <param name="columnNamesWithAliases">An array of column names to include in the output. Each name must correspond to a valid column in the specified
    /// table. Cannot be null or contain null or empty elements. KEYS: Actual column name (do NOT include "UPDATED."/"DELETED."), VALUES: Alias for the column name.</param>
    /// <returns>Instance of self.</returns>
    public SqlUpdateBuilder OutputUpdated(Dictionary<string, string> columnNamesWithAliases)
    {
        if (_output is null)
        {
            _output = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.UPDATED, columnNamesWithAliases);
        }
        else
        {
            _output.AddColumns(SqlOutput.EphermeralTableName.UPDATED, columnNamesWithAliases);
        }

        return this;
    }

    #endregion

    #region Output - table specifiers

    /// <summary>
    /// Specify that the output must be redirected to the table backing the provided <typeparamref name="TOutputTable"/>.
    /// </summary>
    /// <typeparam name="TOutputTable">The entity type mapped to a SQL Server table to write the output data into.</typeparam>
    /// <returns>Instance of self.</returns>
    public SqlUpdateBuilder OutputToTable<TOutputTable>()
    {
        if (_output is null)
        {
            throw new InvalidOperationException("Set the output columns before specifying an output table.");
        }

        _output.ToTable<TOutputTable>();
        return this;
    }

    /// <summary>
    /// Specify the name of the table to output the columns into.
    /// </summary>
    /// <param name="tableName">Name of the table. Cannot be a table variable, can be a temporary table.</param>
    /// <returns>Self-instance.</returns>
    public SqlUpdateBuilder OutputToTable(string tableName)
    {
        if (_output is null)
        {
            throw new InvalidOperationException("Set the output columns before specifying an output table.");
        }

        // The name string is validated by ToTable.
        _output.ToTable(tableName);
        return this;
    }


    #endregion

    /// <summary>
    /// When the OUTPUT clause is set, populated with the OUTPUT options (columns, optionally the destination table)
    /// </summary>
    private SqlOutput? _output = null;
}
