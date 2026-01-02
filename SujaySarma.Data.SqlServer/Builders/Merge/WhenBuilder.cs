using System;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders.Merge;

/// <summary>
/// Helps chain the <see cref="UsingBuilder{TTarget}.MatchBuilder{TSource}"/> with the actual WHEN clauses.
/// </summary>
/// <typeparam name="TTarget">The <see cref="Type"/> of the target table/entity for the MERGE statement.</typeparam>
/// <typeparam name="TSource">The <see cref="Type"/> of the source table/entity being compared (INNER JOINed) with.</typeparam>
public sealed class WhenBuilder<TTarget, TSource>
{

    /// <summary>
    /// Create a new WHEN MATCHED sub-clause.
    /// </summary>
    /// <param name="condition">The condition to be used to match rows between <typeparamref name="TTarget"/> and <typeparamref name="TSource"/>.</param>
    /// <returns>An instance of a <see cref="MatchActionUpdateOrDelete{TTarget, TSource}"/> to provide the UPDATE or DELETE action to take if the match is TRUE.</returns>
    public MatchActionUpdateOrDelete<TTarget, TSource> WhenMatched(Expression<Func<TTarget, TSource, bool>>? condition = null)
    {
        if (condition is null)
        {
            if (_matchedWithoutConditionIsUsed)
            {
                throw new InvalidOperationException("A 'WHEN MATCHED' without a condition has already been defined. Only one unconditional 'WHEN MATCHED' is allowed per MERGE statement.");
            }

            _matchedWithoutConditionIsUsed = true;
        }

        _mergeBuilder.Write($"{Environment.NewLine}WHEN MATCHED ");
        if (condition is not null)
        {
            _mergeBuilder.Write("AND ");
            _mergeBuilder.Write(SqlExpressionParser.Parse(condition));
            _mergeBuilder.Write(" ");
        }
        _mergeBuilder.Write("THEN ");

        return new MatchActionUpdateOrDelete<TTarget, TSource>(_mergeBuilder, this);
    }
    private bool _matchedWithoutConditionIsUsed = false;

    /// <summary>
    /// Create a new WHEN NOT MATCHED BY SOURCE sub-clause.
    /// </summary>
    /// <param name="condition">The condition to be used to match rows between <typeparamref name="TTarget"/> and <typeparamref name="TSource"/>.</param>
    /// <returns>An instance of a <see cref="MatchActionUpdateOrDelete{TTarget, TSource}"/> to provide the UPDATE or DELETE action to take if the match is TRUE.</returns>
    public MatchActionUpdateOrDelete<TTarget, TSource> WhenNotMatchedBySource(Expression<Func<TTarget, TSource, bool>>? condition = null)
    {
        if (condition is null)
        {
            if (_notMatchedBySourceWithoutConditionIsUsed)
            {
                throw new InvalidOperationException("A 'WHEN NOT MATCHED BY SOURCE' without a condition has already been defined. Only one unconditional 'WHEN NOT MATCHED BY SOURCE' is allowed per MERGE statement.");
            }

            _notMatchedBySourceWithoutConditionIsUsed = true;
        }

        _mergeBuilder.Write($"{Environment.NewLine}WHEN NOT MATCHED BY SOURCE ");
        if (condition is not null)
        {
            _mergeBuilder.Write("AND ");
            _mergeBuilder.Write(SqlExpressionParser.Parse(condition));
            _mergeBuilder.Write(" ");
        }
        _mergeBuilder.Write("THEN ");

        return new MatchActionUpdateOrDelete<TTarget, TSource>(_mergeBuilder, this);
    }
    private bool _notMatchedBySourceWithoutConditionIsUsed = false;

    /// <summary>
    /// Create a new WHEN NOT MATCHED BY TARGET sub-clause.
    /// </summary>
    /// <param name="condition">The condition to be used to match rows between <typeparamref name="TTarget"/> and <typeparamref name="TSource"/>.</param>
    /// <returns>An instance of a <see cref="MatchActionInsert{TTarget, TSource}"/> to provide the INSERT action to take if the match is TRUE.</returns>
    public MatchActionInsert<TTarget, TSource> WhenNotMatchedByTarget(Expression<Func<TTarget, TSource, bool>>? condition = null)
    {
        if (condition is null)
        {
            if (_notMatchedByTargetWithoutConditionIsUsed)
            {
                throw new InvalidOperationException("A 'WHEN NOT MATCHED BY TARGET' without a condition has already been defined. Only one unconditional 'WHEN NOT MATCHED BY TARGET' is allowed per MERGE statement.");
            }

            _notMatchedByTargetWithoutConditionIsUsed = true;
        }

        _mergeBuilder.Write($"{Environment.NewLine}WHEN NOT MATCHED BY TARGET ");
        if (condition is not null)
        {
            _mergeBuilder.Write("AND ");
            _mergeBuilder.Write(SqlExpressionParser.Parse(condition));
            _mergeBuilder.Write(" ");
        }
        _mergeBuilder.Write("THEN ");

        return new MatchActionInsert<TTarget, TSource>(_mergeBuilder, this);
    }
    private bool _notMatchedByTargetWithoutConditionIsUsed = false;

    /// <summary>
    /// Mark that we have completed our WHEN (NOT) MATCHED (BY TARGET|SOURCE) sub-clauses.
    /// </summary>
    /// <returns>The parent <see cref="SqlMergeBuilder{TTarget}"/> instance.</returns>
    public SqlMergeBuilder<TTarget> EndMatches() 
        => _mergeBuilder;


    /// <summary>
    /// Initialise.
    /// </summary>
    /// <param name="mergeBuilder">Instance of the <see cref="SqlMergeBuilder{TTarget}"/> to return to.</param>
    internal WhenBuilder(SqlMergeBuilder<TTarget> mergeBuilder)
    {
        _mergeBuilder = mergeBuilder;
    }

    /// <summary>
    /// The parent SqlMergeBuilder instance.
    /// </summary>
    private readonly SqlMergeBuilder<TTarget> _mergeBuilder;
}
