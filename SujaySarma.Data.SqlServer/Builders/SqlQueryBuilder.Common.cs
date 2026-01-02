using SujaySarma.Data.SqlServer.Builders.Constants;

using System;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of TOP, DISTINCT, WITH clauses.
public sealed partial class SqlQueryBuilder
{
    /// <summary>
    /// Set this statement to select only <paramref name="count"/> number of rows.
    /// </summary>
    /// <param name="count">Zero is a valid value.</param>
    /// <param name="isPercent">If true, indicates <paramref name="count"/> is a percent value.</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder Top(uint count, bool isPercent = false)
    {
        if (_topN.HasValue)
        {
            throw new InvalidOperationException("TOP has already been specified for this query.");
        }

        if (isPercent && (count > 100))
        {
            throw new ArgumentOutOfRangeException(nameof(count), $"Count cannot be greater than 100 when {nameof(isPercent)} is true.");
        }

        _topN = count;
        _topValueIsPercentage = isPercent;
        return this;
    }

    /// <summary>
    /// Set this query to return DISTINCT rows.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder Distinct()
    {
        _selectDistinctRows = true;
        return this;
    }

    /// <summary>
    /// Specify one or more table hints. You may call this method multiple times to add multiple table hints OR specify all the hints at once 
    /// using the OR pattern.
    /// </summary>
    /// <param name="hints">Table hints to specify: may be a single value, or OR'ed with other values.</param>
    /// <returns>Self-instance.</returns>
    public SqlQueryBuilder With(SqlHint hints)
    {
        base.AppendHints(hints, SqlStatementType.Query);
        return this;
    }

    private uint? _topN = null;
    private bool _topValueIsPercentage = false;
    private bool _selectDistinctRows = false;
}
