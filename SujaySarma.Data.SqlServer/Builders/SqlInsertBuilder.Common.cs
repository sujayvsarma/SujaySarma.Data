using SujaySarma.Data.SqlServer.Builders.Constants;

using System;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of TOP and WITH clauses.
public sealed partial class SqlInsertBuilder
{
    /// <summary>
    /// Set this statement to insert only <paramref name="count"/> number of rows.
    /// </summary>
    /// <param name="count">Zero is a valid value.</param>
    /// <param name="isPercent">If true, indicates <paramref name="count"/> is a percent value.</param>
    /// <returns>Instance of self.</returns>
    public SqlInsertBuilder Top(uint count, bool isPercent = false)
    {
        if (_topN.HasValue)
        {
            throw new InvalidOperationException("TOP has already been specified for this statement.");
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
    /// Specify one or more table hints. You may call this method multiple times to add multiple table hints OR specify all the hints at once 
    /// using the OR pattern.
    /// </summary>
    /// <param name="hints">Table hints to specify: may be a single value, or OR'ed with other values.</param>
    /// <returns>Self-instance.</returns>
    public SqlInsertBuilder With(SqlHint hints)
    {
        base.AppendHints(hints, SqlStatementType.Insert);
        return this;
    }

    /// <summary>
    /// The TOP N value specified for this statement, if any. Else NULL.
    /// </summary>
    private uint? _topN = null;

    /// <summary>
    /// When true, indicates that _topN is a percent value. Makes sense only when _topN is non-NULL!
    /// </summary>
    private bool _topValueIsPercentage = false;

}
