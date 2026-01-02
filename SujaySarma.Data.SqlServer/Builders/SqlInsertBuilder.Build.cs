using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of hte BUILD function.
public sealed partial class SqlInsertBuilder
{

    /// <summary>
    /// Assembles the INSERT statement from provided clauses and returns it as a <see cref="StringBuilder" /> instance.
    /// </summary>
    /// <returns>Instance of <see cref="StringBuilder" /> containing the assembled INSERT statement.</returns>
    public override StringBuilder Build()
    {
        if ((!_insertDefaultValues) && (_values.Count is 0) && (_insertFromQuery is null))
        {
            throw new ArgumentException("No values to insert! NotSet of UsingDefaultValues or Value or From appear to have been called.");
        }

        // Each portion of the statement will prepend the required SPACE.

        // INSERT
        StringBuilder builder = new StringBuilder()
            .Append("INSERT");

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

        // INTO [tablename] [alias]
        builder.Append(" INTO ")
            .Append(_primaryTable.PersistenceInfo.CreateQualifiedName())
            .Append(' ')
            .Append(_primaryTable.ReferenceAlias);

        // WITH (hints)
        if (_hints.Count > 0)
        {
            builder.Append(" WITH (")
                .AppendJoin(", ", _hints.Select(h => h.ToSQL()))
                .Append(')');
        }

        // Column names (if not using DEFAULTS)
        // If are INSERT FROM, optionally provided column names through From() will be in _values[0].
        if ((!_insertDefaultValues) && (_values.Count > 0))
        {
            builder.Append(" (")
                    .AppendJoin(", ", _values[0].Keys)
                    .Append(')');
        }

        // OUTPUT
        if (_output is not null)
        {
            // ToString() generates the complete clause including "OUTPUT".
            builder.Append(' ')
                .Append(_output.ToString());
        }

        // VALUES!
        if (_insertDefaultValues)
        {
            // DEFAULTs
            builder.Append(" DEFAULT VALUES");
        }
        else if (_insertFromQuery is not null)
        {
            // FROM query
            builder.Append(' ')
                .Append(_insertFromQuery);
        }
        else if (_values.Count > 0)
        {
            // VALUES!
            builder.Append(" VALUES ");
            bool needsRowComma = false, needsColumnComma = false;
            foreach(Dictionary<string, string> row in _values)
            {
                if (needsRowComma)
                {
                    builder.Append(", ");
                }

                builder.Append('(');
                foreach(string columnName in row.Keys)
                {
                    if (needsColumnComma)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(row[columnName]);
                    needsColumnComma = true;
                }
                builder.Append(')');

                needsRowComma = true;
                needsColumnComma = false;
            }
        }

        builder.Append(';');
        return builder;
    }

}
