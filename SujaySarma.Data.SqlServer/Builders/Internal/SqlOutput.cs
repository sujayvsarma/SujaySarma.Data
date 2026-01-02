using SujaySarma.Data.Core.TypeDiscovery;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace SujaySarma.Data.SqlServer.Builders.Internal;

/// <summary>
/// Implements the OUTPUT clause for SQL statements. Note that because of the scenario this library operates in 
/// (as an ORM to an application), output of affected rows to table variables is NOT supported. Callers may only 
/// cause the affected rows to be returned to them, or insert them into an on-server table.
/// </summary>
internal sealed class SqlOutput
{

    #region Output Table

    /// <summary>
    /// Specify the name of the table to output the columns into.
    /// </summary>
    /// <param name="tableName">Name of the table. Cannot be a table variable, can be a temporary table.</param>
    /// <returns>Self-instance.</returns>
    public SqlOutput ToTable(string tableName)
    {
        const string pattern = @"^[a-zA-Z_#][a-zA-Z0-9_$#@.]*$";

        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentNullException(nameof(tableName), "Destination table name cannot be NULL or empty.");
        }

        // Regex pattern already requires that name does NOT start with a '@' (table var)
        if ((tableName.Length > 128) || (!Regex.IsMatch(tableName, pattern)))
        {
            throw new ArgumentException("Destination table name is invalid.", nameof(tableName));
        }

        if (!string.IsNullOrWhiteSpace(_targetTableName))
        {
            throw new InvalidOperationException($"Name of the target table can be set only once! It has already been set to '{_targetTableName}'.");
        }

        _targetTableName = tableName.EnsureIdentifierIsQuoted();
        return this;
    }

    /// <summary>
    /// Specify that the output must be redirected to the table backing the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The entity type mapped to a SQL Server table to write the output data into.</param>
    /// <returns>Self-instance.</returns>
    public SqlOutput ToTable(Type type)
    {
        _targetTableName = type.RetrievePersistenceContainerInfoOrThrowException().PersistenceInfo.CreateQualifiedName();
        return this;
    }

    /// <summary>
    /// Specify that the output must be redirected to the table backing the provided <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type mapped to a SQL Server table to write the output data into.</typeparam>
    /// <returns>Self-instance.</returns>
    public SqlOutput ToTable<TEntity>()
    {
        return ToTable(typeof(TEntity));
    }


    #endregion

    #region Post-initialisation column specifiers (to add columns)

    /// <summary>
    /// Adds columns from the specified <typeparamref name="TEntity"/>. If a column selector is specified, then only the columns returned by that selector are included.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity whose eligible columns are to be returned.</typeparam>
    /// <param name="columnSelector">The lambda expression that will return a list of column names (optionally with aliases) for the eligible member properties/fields of the provided type. 
    /// For a single member/column, use an expression such as "(e) => e.Property", or to alias it: "(e) => "alias" = e.Property". When selecting multiple columns, this becomes: 
    /// "(e) => new { e.Prop1, e.Prop2...}" or with aliases as: "(e) => new { "col1" = e.Prop1, "col2" = e.Prop2...}. Alias only the columns you need to!</param>
    /// <returns></returns>
    public SqlOutput AddColumns<TEntity>(Expression<Func<TEntity, object>>? columnSelector = null)
    {
        if (columnSelector is null)
        {
            // Select all eligible columns from TTarget.
            PersistenceContainerInfo container = typeof(TEntity).RetrievePersistenceContainerInfoOrThrowException();
            foreach (PersistenceContainerMemberInfo member in container.Members)
            {
                string qcol = member.PersistenceInfo.CreateQualifiedName();
                if (!_outputColumnNames.Contains(qcol))
                {
                    _outputColumnNames.Add(qcol);
                }
            }
        }
        else
        {
            if (columnSelector is not LambdaExpression selector)
            {
                throw new ArgumentException("The selector specified is not a valid lambda expression.");
            }

            if (selector.Parameters.Count != 1)
            {
                throw new ArgumentException("Selector expression must have exactly one parameter.");
            }

            string columnNames = SqlExpressionParser.Parse(selector, assignmentTreatment: SqlExpressionParser.AssignmentTreatment.AsAlias);
            foreach (string col in columnNames.Split(',').Select(c => c.Trim()))
            {
                if (!col.Contains(" AS "))
                {
                    string qcol = col.EnsureIdentifierIsQuoted();
                    if (!_outputColumnNames.Contains(qcol))
                    {
                        _outputColumnNames.Add(qcol);
                    }
                }
            }
        }

        if (_outputColumnNames.Count == 0)
        {
            throw new InvalidOperationException("No columns were specified for output.");
        }

        return this;
    }

    /// <summary>
    /// Adds all columns from the relevant ephermeral tables 
    /// for the given statement <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Statement type to include columns from.</param>
    /// <returns>Self-instance.</returns>
    public SqlOutput AddAllColumns(EphermeralTableName name)
    {
        if (name.HasFlag(EphermeralTableName.INSERTED))
        {
            AddColumns(EphermeralTableName.INSERTED, "*");
        }

        if (name.HasFlag(EphermeralTableName.DELETED))
        {
            AddColumns(EphermeralTableName.DELETED, "*");
        }

        if (_outputColumnNames.Count == 0)
        {
            throw new InvalidOperationException($"Table name value '{name}' is not supported.");
        }

        return this;
    }

    /// <summary>
    /// Adds specific <paramref name="columnNames"/>.
    /// </summary>
    /// <param name="ephermeralTable">The ephemeral table from which to select columns.</param>
    /// <param name="columnNames">An array of column names to include in the output. Each name must correspond to a valid column in the specified
    /// table. Cannot be null or contain null or empty elements.</param>
    /// <returns>Self-instance.</returns>
    public SqlOutput AddColumns(EphermeralTableName ephermeralTable, params IEnumerable<string> columnNames)
    {
        if (!columnNames.Any())
        {
            throw new ArgumentNullException(nameof(columnNames), "No column names were specified for output.");
        }

        if (ephermeralTable is EphermeralTableName.INSERTED)
        {
            if (_outputColumnNames.Contains("INSERTED.*"))
            {
                throw new ArgumentException("The OUTPUT clause already contains an 'INSERTED.*' specification. Cannot add more columns from the INSERTED table.");
            }
        }
        else if (ephermeralTable is EphermeralTableName.DELETED)
        {
            if (_outputColumnNames.Contains("DELETED.*"))
            {
                throw new ArgumentException("The OUTPUT clause already contains an 'DELETED.*' specification. Cannot add more columns from the DELETED table.");
            }
        }
        else
        {
            if (_outputColumnNames.Contains("INSERTED.*"))
            {
                throw new ArgumentException("The OUTPUT clause already contains an 'INSERTED.*' specification. Cannot add more columns from the INSERTED table.");
            }

            if (_outputColumnNames.Contains("DELETED.*"))
            {
                throw new ArgumentException("The OUTPUT clause already contains an 'DELETED.*' specification. Cannot add more columns from the DELETED table.");
            }
        }

        foreach (string columnName in columnNames)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Name of a column cannot be NULL or empty.");
            }

            string qcol = ((columnName == "*") ? columnName : columnName.EnsureIdentifierIsQuoted());

            switch (ephermeralTable)
            {
                case EphermeralTableName.INSERTED:
                    qcol = $"INSERTED.{qcol}";
                    if (!_outputColumnNames.Contains(qcol))
                    {
                        _outputColumnNames.Add(qcol);
                    }
                    break;

                case EphermeralTableName.DELETED:
                    qcol = $"DELETED.{qcol}";
                    if (!_outputColumnNames.Contains(qcol))
                    {
                        _outputColumnNames.Add(qcol);
                    }
                    break;

                case EphermeralTableName.UPDATED:
                    string ins = $"INSERTED.{qcol}";
                    string del = $"DELETED.{qcol}";

                    if (!_outputColumnNames.Contains(ins))
                    {
                        _outputColumnNames.Add(ins);
                    }

                    if (!_outputColumnNames.Contains(del))
                    {
                        _outputColumnNames.Add(del);
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Table name value '{ephermeralTable}' is not supported.");
            }
        }

        if (_outputColumnNames.Count == 0)
        {
            throw new InvalidOperationException("No columns were specified for output.");
        }

        return this;
    }

    /// <summary>
    /// Adds specific <paramref name="columnNamesWithAliases"/> (column names with aliases).
    /// </summary>
    /// <param name="ephermeralTable">The ephemeral table from which to select columns.</param>
    /// <param name="columnNamesWithAliases">An array of column names to include in the output. Each name must correspond to a valid column in the specified
    /// table. Cannot be null or contain null or empty elements. KEYS: Actual column name (do NOT include "INSERTED."/"DELETED."), VALUES: Alias for the column name.</param>
    /// <returns>Self-instance.</returns>
    public SqlOutput AddColumns(EphermeralTableName ephermeralTable, Dictionary<string, string> columnNamesWithAliases)
    {
        if (columnNamesWithAliases.Count == 0)
        {
            throw new ArgumentNullException(nameof(columnNamesWithAliases), "No column names were specified for output.");
        }

        if (ephermeralTable is EphermeralTableName.INSERTED)
        {
            if (_outputColumnNames.Contains("INSERTED.*"))
            {
                throw new ArgumentException("The OUTPUT clause already contains an 'INSERTED.*' specification. Cannot add more columns from the INSERTED table.");
            }
        }
        else if (ephermeralTable is EphermeralTableName.DELETED)
        {
            if (_outputColumnNames.Contains("DELETED.*"))
            {
                throw new ArgumentException("The OUTPUT clause already contains an 'DELETED.*' specification. Cannot add more columns from the DELETED table.");
            }
        }
        else
        {
            if (_outputColumnNames.Contains("INSERTED.*"))
            {
                throw new ArgumentException("The OUTPUT clause already contains an 'INSERTED.*' specification. Cannot add more columns from the INSERTED table.");
            }

            if (_outputColumnNames.Contains("DELETED.*"))
            {
                throw new ArgumentException("The OUTPUT clause already contains an 'DELETED.*' specification. Cannot add more columns from the DELETED table.");
            }
        }

        foreach (string columnName in columnNamesWithAliases.Keys)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Name of a column cannot be NULL or empty.");
            }

            string qcol = columnName;
            string qalias = columnNamesWithAliases[columnName];

            if (columnName == "*")
            {
                qcol = columnName;
                qalias = string.Empty;
            }
            else
            {
                qcol = columnName.EnsureIdentifierIsQuoted();
                if (string.IsNullOrWhiteSpace(columnNamesWithAliases[columnName]))
                {
                    qalias = string.Empty;
                }
                else
                {
                    qalias = columnNamesWithAliases[columnName].EnsureIdentifierIsQuoted();
                }
            }

            if (qalias != string.Empty)
            {
                qalias = $" AS {qalias}";
            }

            switch (ephermeralTable)
            {
                case EphermeralTableName.INSERTED:
                    qcol = $"INSERTED.{qcol}{qalias}";
                    if (!_outputColumnNames.Contains(qcol))
                    {
                        _outputColumnNames.Add(qcol);
                    }
                    break;

                case EphermeralTableName.DELETED:
                    qcol = $"DELETED.{qcol}{qalias}";
                    if (!_outputColumnNames.Contains(qcol))
                    {
                        _outputColumnNames.Add($"{qcol}");
                    }
                    break;

                case EphermeralTableName.UPDATED:
                    string ins = $"INSERTED.{qcol}{qalias}";
                    string del = $"DELETED.{qcol}{qalias}";

                    if (!_outputColumnNames.Contains(ins))
                    {
                        _outputColumnNames.Add(ins);
                    }

                    if (!_outputColumnNames.Contains(del))
                    {
                        _outputColumnNames.Add(del);
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Table name value '{ephermeralTable}' is not supported.");
            }
        }

        if (_outputColumnNames.Count == 0)
        {
            throw new InvalidOperationException("No columns were specified for output.");
        }

        return this;
    }

    /// <summary>
    /// Adds the special action column ($action) aliased to the provided <paramref name="alias"/>.
    /// NOTE: The $action column is only valid in MERGE statements.
    /// </summary>
    /// <param name="alias">[optional] Alias for the $action column.</param>
    /// <returns>Self-instance.</returns>
    public SqlOutput AddActionColumn(string? alias = null)
    {
        if (_outputColumnNames.Any(c => (c.Equals("$action") || c.StartsWith("$action AS ["))))
        {
            throw new InvalidOperationException("The $action column has already been added to the output columns.");
        }

        if (alias is null)
        {
            _outputColumnNames.Add("$action");
        }
        else
        {
            _outputColumnNames.Add($"$action AS {alias.EnsureIdentifierIsQuoted()}");
        }

        return this;
    }

    #endregion

    #region Initialisers: Column specifiers

    /// <summary>
    /// Creates an OUTPUT clause that outputs columns from the specified <typeparamref name="TEntity"/>. If a 
    /// column selector is specified, then only the columns returned by that selector are included.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity whose eligible columns are to be returned.</typeparam>
    /// <param name="columnSelector">The lambda expression that will return a list of column names (optionally with aliases) for the eligible member properties/fields of the provided type. 
    /// For a single member/column, use an expression such as "(e) => e.Property", or to alias it: "(e) => "alias" = e.Property". When selecting multiple columns, this becomes: 
    /// "(e) => new { e.Prop1, e.Prop2...}" or with aliases as: "(e) => new { "col1" = e.Prop1, "col2" = e.Prop2...}. Alias only the columns you need to!</param>
    /// <returns>Instantiated copy of SqlOutput.</returns>
    public static SqlOutput WithColumns<TEntity>(Expression<Func<TEntity, object>>? columnSelector = null)
    {
        SqlOutput output = new SqlOutput();

        if (columnSelector is null)
        {
            // Select all eligible columns from TTarget.
            PersistenceContainerInfo container = typeof(TEntity).RetrievePersistenceContainerInfoOrThrowException();
            foreach (PersistenceContainerMemberInfo member in container.Members)
            {
                string qcol = member.PersistenceInfo.CreateQualifiedName();
                if (!output._outputColumnNames.Contains(qcol))
                {
                    output._outputColumnNames.Add(qcol);
                }
            }
        }
        else
        {
            if (columnSelector is not LambdaExpression selector)
            {
                throw new ArgumentException("The selector specified is not a valid lambda expression.");
            }

            if (selector.Parameters.Count != 1)
            {
                throw new ArgumentException("Selector expression must have exactly one parameter.");
            }

            string columnNames = SqlExpressionParser.Parse(selector, assignmentTreatment: SqlExpressionParser.AssignmentTreatment.AsAlias);
            foreach (string col in columnNames.Split(',').Select(c => c.Trim()))
            {
                if (!col.Contains(" AS "))
                {
                    string qcol = col.EnsureIdentifierIsQuoted();
                    if (!output._outputColumnNames.Contains(qcol))
                    {
                        output._outputColumnNames.Add(qcol);
                    }
                }
            }
        }

        if (output._outputColumnNames.Count == 0)
        {
            throw new InvalidOperationException("No columns were specified for output.");
        }

        return output;
    }

    /// <summary>
    /// Creates an OUTPUT clause that outputs all columns from the relevant ephermeral tables 
    /// for the given statement <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Statement type to include columns from.</param>
    /// <returns>Instantiated copy of SqlOutput.</returns>
    public static SqlOutput WithAllColumns(EphermeralTableName name)
    {
        SqlOutput output = new SqlOutput();

        if (name.HasFlag(EphermeralTableName.INSERTED))
        {
            output.AddAllColumns(EphermeralTableName.INSERTED);
        }

        if (name.HasFlag(EphermeralTableName.DELETED))
        {
            output.AddAllColumns(EphermeralTableName.DELETED);
        }

        if (output._outputColumnNames.Count == 0)
        {
            throw new InvalidOperationException($"Table name value '{name}' is not supported.");
        }

        return output;
    }

    /// <summary>
    /// Creates an OUTPUT clause with specific <paramref name="columnNames"/>.
    /// </summary>
    /// <param name="ephermeralTable">The ephemeral table from which to select columns.</param>
    /// <param name="columnNames">An array of column names to include in the output. Each name must correspond to a valid column in the specified
    /// table. Cannot be null or contain null or empty elements.</param>
    /// <returns>Instantiated copy of SqlOutput.</returns>
    public static SqlOutput WithColumns(EphermeralTableName ephermeralTable, params IEnumerable<string> columnNames)
    {
        if (!columnNames.Any())
        {
            throw new ArgumentNullException(nameof(columnNames), "No column names were specified for output.");
        }

        SqlOutput output = new SqlOutput();

        foreach (string columnName in columnNames)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Name of a column cannot be NULL or empty.");
            }

            string qcol = columnName.EnsureIdentifierIsQuoted();
            switch (ephermeralTable)
            {
                case EphermeralTableName.INSERTED:
                    qcol = $"INSERTED.{qcol}";
                    if (!output._outputColumnNames.Contains(qcol))
                    {
                        output._outputColumnNames.Add(qcol);
                    }
                    break;

                case EphermeralTableName.DELETED:
                    qcol = $"DELETED.{qcol}";
                    if (!output._outputColumnNames.Contains(qcol))
                    {
                        output._outputColumnNames.Add(qcol);
                    }
                    break;

                case EphermeralTableName.UPDATED:
                    string ins = $"INSERTED.{qcol}";
                    string del = $"DELETED.{qcol}";

                    if (!output._outputColumnNames.Contains(ins))
                    {
                        output._outputColumnNames.Add(ins);
                    }

                    if (!output._outputColumnNames.Contains(del))
                    {
                        output._outputColumnNames.Add(del);
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Table name value '{ephermeralTable}' is not supported.");
            }
        }

        if (output._outputColumnNames.Count == 0)
        {
            throw new InvalidOperationException("No columns were specified for output.");
        }

        return output;
    }

    /// <summary>
    /// Creates an OUTPUT clause with specific <paramref name="columnNamesWithAliases"/> (column names with aliases).
    /// </summary>
    /// <param name="ephermeralTable">The ephemeral table from which to select columns.</param>
    /// <param name="columnNamesWithAliases">An array of column names to include in the output. Each name must correspond to a valid column in the specified
    /// table. Cannot be null or contain null or empty elements. KEYS: Actual column name (do NOT include "INSERTED."/"DELETED."), VALUES: Alias for the column name.</param>
    /// <returns>Instantiated copy of SqlOutput.</returns>
    public static SqlOutput WithColumns(EphermeralTableName ephermeralTable, Dictionary<string, string> columnNamesWithAliases)
    {
        if ((columnNamesWithAliases is null) || (columnNamesWithAliases.Count == 0))
        {
            throw new ArgumentNullException(nameof(columnNamesWithAliases), "No column names were specified for output.");
        }

        SqlOutput output = new SqlOutput();

        foreach (string columnName in columnNamesWithAliases.Keys)
        {
            if (string.IsNullOrWhiteSpace(columnName) || string.IsNullOrWhiteSpace(columnNamesWithAliases[columnName]))
            {
                throw new ArgumentException("Name of a column or its alias cannot be NULL or empty.");
            }

            string qcol = columnName;
            string qalias = columnNamesWithAliases[columnName];

            if (columnName == "*")
            {
                qcol = columnName;
                qalias = string.Empty;
            }
            else
            {
                qcol = columnName.EnsureIdentifierIsQuoted();
                if (string.IsNullOrWhiteSpace(columnNamesWithAliases[columnName]))
                {
                    qalias = string.Empty;
                }
                else
                {
                    qalias = columnNamesWithAliases[columnName].EnsureIdentifierIsQuoted();
                }
            }

            if (qalias != string.Empty)
            {
                qalias = $" AS {qalias}";
            }

            switch (ephermeralTable)
            {
                case EphermeralTableName.INSERTED:
                    qcol = $"INSERTED.{qcol}{qalias}";
                    if (!output._outputColumnNames.Contains(qcol))
                    {
                        output._outputColumnNames.Add(qcol);
                    }
                    break;

                case EphermeralTableName.DELETED:
                    qcol = $"DELETED.{qcol}{qalias}";
                    if (!output._outputColumnNames.Contains(qcol))
                    {
                        output._outputColumnNames.Add($"{qcol}");
                    }
                    break;

                case EphermeralTableName.UPDATED:
                    string ins = $"INSERTED.{qcol}{qalias}";
                    string del = $"DELETED.{qcol}{qalias}";

                    if (!output._outputColumnNames.Contains(ins))
                    {
                        output._outputColumnNames.Add(ins);
                    }

                    if (!output._outputColumnNames.Contains(del))
                    {
                        output._outputColumnNames.Add(del);
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Table name value '{ephermeralTable}' is not supported.");
            }
        }

        if (output._outputColumnNames.Count == 0)
        {
            throw new InvalidOperationException("No columns were specified for output.");
        }

        return output;
    }

    /// <summary>
    /// Creates an OUTPUT clause with the special action column ($action) aliased to the provided <paramref name="alias"/>.
    /// NOTE: The $action column is only valid in MERGE statements.
    /// </summary>
    /// <param name="alias">[optional] Alias for the $action column.</param>
    /// <returns>Instantiated copy of SqlOutput.</returns>
    public static SqlOutput WithActionColumn(string? alias = null)
    {
        SqlOutput output = new SqlOutput();

        if (alias is null)
        {
            output._outputColumnNames.Add("$action");
        }
        else
        {
            output._outputColumnNames.Add($"$action AS {alias.EnsureIdentifierIsQuoted()}");
        }

        return output;
    }

    #endregion

    #region Initialisers

    /// <summary>
    /// Private constructor to prevent accidental initialisation.
    /// </summary>
    private SqlOutput()
    {
    }

    #endregion

    /// <summary>
    /// Generates the complete OUTPUT clause, including the OUTPUT keyword at the start!
    /// </summary>
    /// <returns>String containing the output clause.</returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder()
            .Append("OUTPUT ");

        for (int i = 0; i < _outputColumnNames.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(_outputColumnNames[i]);
        }

        if (_targetTableName is not null)
        {
            builder.Append($" INTO {_targetTableName}");
        }

        return builder.ToString();
    }


    // if we are outputting to a table, this is the name of the table.
    private string? _targetTableName = null;

    // list of output column names -- each name should be added PROPERLY QUOTED!
    private readonly List<string> _outputColumnNames = new List<string>();


    /// <summary>
    /// Type of statement
    /// </summary>
    [Flags]
    public enum EphermeralTableName
    {
        /// <summary>
        /// INSERTED.
        /// </summary>
        INSERTED = 1,

        /// <summary>
        /// DELETED.
        /// </summary>
        DELETED = 2,

        /// <summary>
        /// (for updates): INSERTED + DELETED.
        /// </summary>
        UPDATED = INSERTED | DELETED
    }
}
