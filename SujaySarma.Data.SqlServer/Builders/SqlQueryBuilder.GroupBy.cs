using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Linq.Expressions;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of GROUP BY.
public sealed partial class SqlQueryBuilder
{
    /// <summary>
    /// Add one or more GROUP BY ROLLUP column(s) to the collection. This function can be used only once as a SQL query may contain only one GROUP BY clause!
    /// </summary>
    /// <typeparam name="TTable">Type of .NET object</typeparam>
    /// <param name="selector">Linq expression to select the column(s) for the grouping.</param>
    /// <param name="having">Linq expression to select the conditions for the HAVING clause. NULL to exclude the HAVING.</param>
    public SqlQueryBuilder GroupByRollup<TTable>(Expression selector, Expression? having = null)
    {
        return GroupByImpl<TTable>(selector, having, SqlOperators.GroupBy.Rollup);
    }

    /// <summary>
    /// Add one or more GROUP BY CUBE column(s) to the collection. This function can be used only once as a SQL query may contain only one GROUP BY clause!
    /// </summary>
    /// <typeparam name="TTable">Type of .NET object</typeparam>
    /// <param name="selector">Linq expression to select the column(s) for the grouping.</param>
    /// <param name="having">Linq expression to select the conditions for the HAVING clause. NULL to exclude the HAVING.</param>
    public SqlQueryBuilder GroupByCube<TTable>(Expression selector, Expression? having = null)
    {
        return GroupByImpl<TTable>(selector, having, SqlOperators.GroupBy.Cube);
    }

    /// <summary>
    /// Add one or more GROUP BY GROUPING SETS column(s) to the collection. This function can be used only once as a SQL query may contain only one GROUP BY clause!
    /// </summary>
    /// <typeparam name="TTable">Type of .NET object</typeparam>
    /// <param name="selector">Linq expression to select the column(s) for the grouping.</param>
    /// <param name="having">Linq expression to select the conditions for the HAVING clause. NULL to exclude the HAVING.</param>
    public SqlQueryBuilder GroupByGroupingSets<TTable>(Expression selector, Expression? having = null)
    {
        return GroupByImpl<TTable>(selector, having, SqlOperators.GroupBy.GroupingSets);
    }

    /// <summary>
    /// Add one or more GROUP BY () column(s) to the collection. This function can be used only once as a SQL query may contain only one GROUP BY clause!
    /// </summary>
    /// <typeparam name="TTable">Type of .NET object</typeparam>
    public SqlQueryBuilder GroupByEmpty<TTable>()
    {
        return GroupByImpl<TTable>(default!, null, SqlOperators.GroupBy.EmptyGroup);
    }

    /// <summary>
    /// Add one or more GROUP BY column(s) to the collection. This function can be used only once as a SQL query may contain only one GROUP BY clause!
    /// </summary>
    /// <typeparam name="TTable">Type of .NET object</typeparam>
    /// <param name="selector">Linq expression to select the column(s) for the grouping.</param>
    /// <param name="having">Linq expression to select the conditions for the HAVING clause. NULL to exclude the HAVING.</param>
    /// <param name="type">Type of GROUP BY to generate</param>
    private SqlQueryBuilder GroupByImpl<TTable>(Expression selector, Expression? having = null, SqlOperators.GroupBy type = SqlOperators.GroupBy.Standard)
    {
        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(_groupBy))
        {
            throw new InvalidOperationException("GROUP BY may be set only once.");
        }

        if (type is SqlOperators.GroupBy.EmptyGroup)
        {
            sb.Append("GROUP BY ()");
        }
        else
        {
            string groupByColumns = SqlExpressionParser.Parse(selector);
            string? havingCondition = ((having is not null) ? SqlExpressionParser.Parse(having) : null);

            switch (type)
            {
                case SqlOperators.GroupBy.Standard:
                    sb.Append($"GROUP BY {groupByColumns}");
                    if (havingCondition is not null)
                    {
                        sb.Append($" HAVING {havingCondition}");
                    }
                    break;

                case SqlOperators.GroupBy.Rollup:
                    sb.Append($"GROUP BY ROLLUP({groupByColumns})");
                    if (havingCondition is not null)
                    {
                        sb.Append($" HAVING {havingCondition}");
                    }
                    break;

                case SqlOperators.GroupBy.Cube:
                    sb.Append($"GROUP BY CUBE({groupByColumns})");
                    if (havingCondition is not null)
                    {
                        sb.Append($" HAVING {havingCondition}");
                    }
                    break;

                case SqlOperators.GroupBy.GroupingSets:
                    sb.Append($"GROUP BY GROUPING SETS ({groupByColumns})");
                    if (havingCondition is not null)
                    {
                        sb.Append($" HAVING {havingCondition}");
                    }
                    break;
            }
        }

        _groupBy = sb.ToString();

        return this;
    }

    private string? _groupBy = null;
}
