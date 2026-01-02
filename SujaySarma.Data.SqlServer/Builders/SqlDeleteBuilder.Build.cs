using SujaySarma.Data.SqlServer.Builders.Constants;

using System.Linq;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of the BUILD function.
public sealed partial class SqlDeleteBuilder
{
    /// <summary>
    /// Assembles the DELETE statement from provided clauses and returns it as a <see cref="StringBuilder" /> instance.
    /// </summary>
    /// <returns>Instance of <see cref="StringBuilder" /> containing the assembled DELETE statement.</returns>
    public override StringBuilder Build()
    {
        // Every segment will prefix spaces as required.

        // DELETE
        StringBuilder builder = new StringBuilder()
            .Append("DELETE");

        // TOP (X) [PERCENT]
        if (_topN.HasValue)
        {
            builder.Append($" TOP ({_topN.Value})");
            if (_topValueIsPercentage)
            {
                builder.Append(" PERCENT");
            }
        }

        // FROM <table> <alias>
        builder.Append(" FROM ")
            .Append(_primaryTable.PersistenceInfo.CreateQualifiedName())
            .Append(' ')
            .Append(_primaryTable.ReferenceAlias);

        // WITH (hints,...)
        if (_hints.Count > 0)
        {
            builder.Append(" WITH (")
                .AppendJoin(", ", _hints.Select(h => h.ToSQL()))
                .Append(')');
        }

        // OUTPUT clause
        if (_output is not null)
        {
            // ToString() prefixes the OUTPUT keyword.
            builder.Append(' ')
                .Append(_output.ToString());
        }

        // JOINs
        if (_joins.HasItems)
        {
            foreach (string join in _joins)
            {
                builder.Append(' ')
                    .Append(join);
            }
        }

        // WHERE
        if ((_where is not null) && _where.HasItems)
        {
            builder.Append(" WHERE");
            foreach (string where in _where)
            {
                builder.Append(' ')
                    .Append(where);
            }
        }

        builder.Append(';');

        return builder;
    }

}
