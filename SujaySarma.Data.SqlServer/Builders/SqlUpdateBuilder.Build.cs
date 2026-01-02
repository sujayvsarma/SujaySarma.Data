using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of the BUILD() function.
public sealed partial class SqlUpdateBuilder
{
    /// <summary>
    /// Assembles the UPDATE statement from provided clauses and returns it as a StringBuilder instance.
    /// </summary>
    /// <returns>Instance of StringBuilder containing the assembled UPDATE statement.</returns>
    public override StringBuilder Build()
    {
        if (_mode is UpdateMode.NotSet)
        {
            throw new InvalidOperationException("No values have been provided for the UPDATE either via entity(ies) or a FROM clause.");
        }

        StringBuilder builder = new StringBuilder();

        switch (_mode)
        {
            case UpdateMode.Serialised:
                BuildStatementModeSerialised(builder);
                break;

            case UpdateMode.FromJoin:
                BuildStatementModeFromJoin(builder);
                break;
        }

        return builder;
    }


    /// <summary>
    /// Builds SQL for the serialised (entity-based) mode.
    /// </summary>
    /// <param name="builder">Instance of the <see cref="StringBuilder"/> to compose the statement into.</param>
    private void BuildStatementModeSerialised(StringBuilder builder)
    {
        if (_values.Count == 0)
        {
            throw new InvalidOperationException("No values to update! Entity data was not provided.");
        }

        foreach (Dictionary<string, string> row in _values)
        {
            if (row.Count is 0)
            {
                // We should never get here in practice!
                throw new InvalidOperationException("There are no columns to update for one of the provided entities.");
            }

            //////////////////////////////////////////////////
            // Each clause prefixes the space BEFORE it.

            // UPDATE
            builder.Append("UPDATE");

            // TOP (X) PERCENT
            if (_topN.HasValue)
            {
                builder.Append(" TOP (")
                    .Append(_topN.Value)
                    .Append(')');

                if (_topValueIsPercentage)
                {
                    builder.Append(" PERCENT");
                }
            }

            // <tablename> <alias>
            builder.Append(' ').Append(_primaryTable.PersistenceInfo.CreateQualifiedName())
                .Append(' ')
                .Append(_primaryTable.ReferenceAlias);

            // WITH (hints)
            if (_hints.Count > 0)
            {
                builder.Append(" WITH (")
                    .AppendJoin(", ", _hints.Select(h => h.ToSQL()))
                    .Append(')');
            }

            // SET column = value, ...
            builder.Append(" SET ")
                .AppendJoin(", ", row.Select(kv => $"{kv.Key} = {kv.Value}"));

            // OUTPUT
            if (_output is not null)
            {
                // ToString() generates the complete clause including "OUTPUT".
                builder.Append(' ')
                    .Append(_output.ToString());
            }

            // WHERE clause (required for serialised mode to target specific rows)
            if ((_where is not null) && (_where.Count > 0))
            {
                builder.Append(" WHERE");
                foreach (string where in _where)
                {
                    builder.Append(' ').Append(where);
                }
            }

            builder.AppendLine(";");
        }
    }

    /// <summary>
    /// Builds SQL for the JOIN (UPDATE FROM) mode.
    /// </summary>
    /// <param name="builder">Instance of the <see cref="StringBuilder"/> to compose the statement into.</param>
    private void BuildStatementModeFromJoin(StringBuilder builder)
    {
        if (_updateFromColumnMappings.Count is 0)
        {
            throw new InvalidOperationException("No columns have been provided.");
        }

        if (!_joins.HasItems)
        {
            throw new InvalidOperationException("No joins have been added for an UPDATE-FROM syntax!");
        }

        //////////////////////////////////////////////////
        // Each clause prefixes the space BEFORE it.

        // UPDATE
        builder.Append("UPDATE");

        // TOP (X) PERCENT
        if (_topN.HasValue)
        {
            builder.Append(" TOP (")
                .Append(_topN.Value)
                .Append(')');

            if (_topValueIsPercentage)
            {
                builder.Append(" PERCENT");
            }
        }

        // <alias>
        builder.Append(' ').Append(_primaryTable.ReferenceAlias);

        // WITH (hints)
        if (_hints.Count > 0)
        {
            builder.Append(" WITH (")
                .AppendJoin(", ", _hints.Select(h => h.ToSQL()))
                .Append(')');
        }

        // SET column = value, ...
        builder.Append(" SET ")
            .AppendJoin(", ", _updateFromColumnMappings.Select(kv => $"{kv.Key} = {kv.Value}"));

        // OUTPUT
        if (_output is not null)
        {
            // ToString() generates the complete clause including "OUTPUT".
            builder.Append(' ')
                .Append(_output.ToString());
        }

        // FROM clause (explicitly added though T-SQL permits this to be absent when JOINs are present)
        builder.Append(" FROM ")
            .Append(_primaryTable.PersistenceInfo.CreateQualifiedName())
            .Append(' ')
            .Append(_primaryTable.ReferenceAlias);

        // JOINs
        foreach(string join in _joins)
        {
            builder.Append(' ')
                .Append(join);
        }

        // WHERE clause (required for serialised mode to target specific rows)
        if ((_where is not null) && (_where.Count > 0))
        {
            builder.Append(" WHERE");
            foreach (string where in _where)
            {
                builder.Append(' ').Append(where);
            }
        }

        builder.AppendLine(";");
    }
}