using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.SqlServer.Builders.Internal;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of: OUTPUT
public sealed partial class SqlMergeBuilder<TTarget>
{

    /// <summary>
    /// Begins a builder to define the MERGE statement's OUTPUT clause.
    /// </summary>
    /// <returns>An instance of the <see cref="OutputBuilder"/>.</returns>
    public OutputBuilder WithOutput()
    {
        return new OutputBuilder(this);
    }


    /// <summary>
    /// 
    /// </summary>
    public sealed class OutputBuilder
    {
        /// <summary>
        /// Adds the OUTPUT action column that indicates if the row was INSERTed, UPDATEd or DELETEd.
        /// </summary>
        /// <param name="alias">[optional] Alias for the action column -- default name is "$action".</param>
        /// <returns>Instance of the Output builder to specify more configuration.</returns>
        public OutputBuilder AddActionColumn(string? alias = null)
        {
            if (_output is null)
            {
                _output = SqlOutput.WithActionColumn(alias);
            }
            else
            {
                _output.AddActionColumn(alias);
            }

            return this;
        }

        /// <summary>
        /// Adds columns from the specified table, as selected by the selection expression.
        /// </summary>
        /// <typeparam name="TTable">The <see cref="Type"/> mapped to the table. Note that this table needs to be already a part of the MERGE!</typeparam>
        /// <param name="columnSelector">The lambda expression that will return a list of column names (optionally with aliases) for the eligible member properties/fields of the provided type. 
        /// For a single member/column, use an expression such as "(e) => e.Property", or to alias it: "(e) => "alias" = e.Property". When selecting multiple columns, this becomes: 
        /// "(e) => new { e.Prop1, e.Prop2...}" or with aliases as: "(e) => new { "col1" = e.Prop1, "col2" = e.Prop2...}. Alias only the columns you need to!</param>
        /// <returns>Instance of the Output builder to specify more configuration.</returns>
        public OutputBuilder AddTable<TTable>(Expression<Func<TTable, object>>? columnSelector = null)
        {
            if (! _mergeBuilder.IsAdded(typeof(TTable)))
            {
                throw new ArgumentException($"The specified table type '{typeof(TTable).GetUsableTypeName()}' has not been added to the MERGE statement. Cannot add output columns from it.", nameof(TTable));
            }

            if (_output is null)
            {
                _output = SqlOutput.WithColumns<TTable>(columnSelector);
            }
            else
            {
                _output.AddColumns<TTable>(columnSelector);
            }
            return this;
        }

        #region INSERTED

        /// <summary>
        /// Adds the specified columns from the INSERTED table to the output.
        /// </summary>
        /// <param name="columnNames">Names of columns from the INSERTED table to select.</param>
        /// <returns>Instance of the Output builder to specify more configuration.</returns>
        public OutputBuilder AddInserted(params IEnumerable<string> columnNames)
        {
            Dictionary<string, string> names = new Dictionary<string, string>();
            foreach(string cn in columnNames)
            {
                names.Add(cn, string.Empty);
            }

            if (names.Count == 0)
            {
                names.Add("*", string.Empty);
            }

            return AddInserted(names);
        }

        /// <summary>
        /// Adds the specified columns with aliases from the INSERTED table to the output.
        /// </summary>
        /// <param name="columnNamesWithAliases">An dictionary of column names with aliases to include in the output. Each name must correspond to a valid column in the specified 
        /// table. Cannot be null or contain null or empty elements. KEYS: Actual column name (do NOT include "INSERTED."), VALUES: Alias for the column name (specify an empty 
        /// string "" if an alias is not required).</param>
        /// <returns>Instance of the Output builder to specify more configuration.</returns>
        public OutputBuilder AddInserted(Dictionary<string, string> columnNamesWithAliases)
        {
            if (_output is null)
            {
                _output = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.INSERTED, columnNamesWithAliases);
            }
            else
            {
                _output.AddColumns(SqlOutput.EphermeralTableName.INSERTED, columnNamesWithAliases);
            }

            return this;
        }

        #endregion

        #region DELETED

        /// <summary>
        /// Adds the specified columns from the DELETED table to the output.
        /// </summary>
        /// <param name="columnNames">Names of columns from the DELETED table to select.</param>
        /// <returns>Instance of the Output builder to specify more configuration.</returns>
        public OutputBuilder AddDeleted(params IEnumerable<string> columnNames)
        {
            Dictionary<string, string> names = new Dictionary<string, string>();
            foreach (string cn in columnNames)
            {
                names.Add(cn, string.Empty);
            }

            if (names.Count == 0)
            {
                names.Add("*", string.Empty);
            }

            return AddDeleted(names);
        }

        /// <summary>
        /// Adds the specified columns with aliases from the DELETED table to the output.
        /// </summary>
        /// <param name="columnNamesWithAliases">An dictionary of column names with aliases to include in the output. Each name must correspond to a valid column in the specified 
        /// table. Cannot be null or contain null or empty elements. KEYS: Actual column name (do NOT include "DELETED."), VALUES: Alias for the column name (specify an empty 
        /// string "" if an alias is not required).</param>
        /// <returns>Instance of the Output builder to specify more configuration.</returns>
        public OutputBuilder AddDeleted(Dictionary<string, string> columnNamesWithAliases)
        {
            if (_output is null)
            {
                _output = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.DELETED, columnNamesWithAliases);
            }
            else
            {
                _output.AddColumns(SqlOutput.EphermeralTableName.DELETED, columnNamesWithAliases);
            }

            return this;
        }

        #endregion

        #region Table specifiers

        /// <summary>
        /// Specify that the output must be redirected to the table backing the provided <typeparamref name="TOutput"/>.
        /// </summary>
        /// <typeparam name="TOutput">The entity type mapped to a SQL Server table to write the output data into.</typeparam>
        /// <returns>Instance of self.</returns>
        public OutputBuilder ToTable<TOutput>()
        {
            if (_output is null)
            {
                throw new InvalidOperationException("Set the output columns before specifying an output table.");
            }

            _output.ToTable<TOutput>();
            return this;
        }

        /// <summary>
        /// Specify the name of the table to output the columns into.
        /// </summary>
        /// <param name="tableName">Name of the table. Cannot be a table variable, can be a temporary table.</param>
        /// <returns>Self-instance.</returns>
        public OutputBuilder ToTable(string tableName)
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
        /// Mark that we have completed our WHEN (NOT) MATCHED (BY TARGET|SOURCE) sub-clauses.
        /// </summary>
        /// <returns>The parent <see cref="SqlMergeBuilder{TTarget}"/> instance.</returns>
        public SqlMergeBuilder<TTarget> EndOutput()
        {
            if (_output is null)
            {
                throw new InvalidOperationException("Cannot generate OUTPUT clause when one has not been configured.");
            }

            _mergeBuilder.Write(Environment.NewLine);
            _mergeBuilder.Write(_output.ToString());

            return _mergeBuilder;
        }

        /// <summary>
        /// Initialise.
        /// </summary>
        /// <param name="mergeBuilder">Instance of the <see cref="SqlMergeBuilder{TTarget}"/> to return to.</param>
        internal OutputBuilder(SqlMergeBuilder<TTarget> mergeBuilder)
        {
            _mergeBuilder = mergeBuilder;
        }

        /// <summary>
        /// The parent SqlMergeBuilder instance.
        /// </summary>
        private readonly SqlMergeBuilder<TTarget> _mergeBuilder;

        /// <summary>
        /// When the OUTPUT clause is set, populated with the OUTPUT options (columns, optionally the destination table)
        /// </summary>
        private SqlOutput? _output = null;
    }   
}
