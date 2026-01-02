using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Builders.Constants;

using System.Linq;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of the BUILD function.
public sealed partial class SqlQueryBuilder
{

    /// <summary>
    /// Assembles the SELECT query.
    /// </summary>
    /// <returns>A <see cref="StringBuilder"/> populated with the constructed SELECT query.</returns>
    public override StringBuilder Build()
    {
        // Each clause will add a space before whatever it is appending!

        // SELECT
        StringBuilder builder = new StringBuilder()
            .Append("SELECT");

        // TOP
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

        // DISTINCT
        if (_selectDistinctRows)
        {
            builder.Append(" DISTINCT");
        }

        // column list
        if (_columns.Count is 0)
        {
            builder.Append(" *");
        }
        else
        {
            builder.Append(' ')
                .AppendJoin(", ", _columns);
        }

        // INTO
        if (!string.IsNullOrWhiteSpace(_intoTableName))
        {
            builder.Append(" INTO ")
                .Append(_intoTableName);
        }

        // FROM
        builder.Append(" FROM ")
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

        // JOINs
        if (_joins.HasItems)
        {
            foreach(string join in _joins)
            {
                builder.Append(' ')
                    .Append(join);
            }
        }

        // WHERE
        bool hasKeywordWHERE = false;
        if ((_where is not null) && _where.HasItems)
        {
            hasKeywordWHERE = true;
            builder.Append(" WHERE");
            foreach(string where in _where)
            {
                builder.Append(' ')
                    .Append(where);
            }
        }

        if (!_includeDeletedRows)
        {
            if (_primaryTable.PersistenceInfo is SqlTableWithSoftDelete softDeleteTable)
            {
                string softDeleteCondition = $"{_primaryTable.ReferenceAlias}.{softDeleteTable.SoftDeleteTableColumnName.EnsureIdentifierIsQuoted()} = 0";
                if (!hasKeywordWHERE)
                {
                    builder.Append(" WHERE");
                }
                else
                {
                    builder.Append(" AND");
                }

                builder.Append(' ')
                    .Append(softDeleteCondition);
            }
        }

        // GROUP BY
        if (_groupBy is not null)
        {
            builder.Append(' ')
                .Append(_groupBy);
        }

        // ORDER BY
        if (_orderBy.HasItems)
        {
            builder.Append(" ORDER BY ")
                .AppendJoin(", ", _orderBy);
        }

        builder.Append(';');

        return builder;
    }


}
