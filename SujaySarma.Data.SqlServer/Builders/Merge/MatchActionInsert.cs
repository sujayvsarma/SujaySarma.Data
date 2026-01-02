using System;

namespace SujaySarma.Data.SqlServer.Builders.Merge;

/// <summary>
/// Handles the WHEN NOT MATCHED BY TARGET actions -- can only INSERT.
/// </summary>
/// <typeparam name="TTarget">The <see cref="Type"/> of the target table/entity for the MERGE statement.</typeparam>
/// <typeparam name="TSource">The <see cref="Type"/> of the source table/entity being compared (INNER JOINed) with.</typeparam>
public sealed class MatchActionInsert<TTarget, TSource>
{

    /// <summary>
    /// Prepares an INSERT action for the matching WHEN clause.
    /// </summary>
    /// <returns></returns>
    public InsertAction<TTarget, TSource> Insert()
    {
        return new InsertAction<TTarget, TSource>(_mergeBuilder, _whenBuilder);
    }

    /// <summary>
    /// Initialise.
    /// </summary>
    /// <param name="mergeBuilder">Instance of the <see cref="SqlMergeBuilder{TTarget}"/> to return to.</param>
    /// <param name="whenBuilder">Instance of the parent <see cref="WhenBuilder{TTarget, TSource}"/> that instantiated this builder.</param>
    internal MatchActionInsert(SqlMergeBuilder<TTarget> mergeBuilder, WhenBuilder<TTarget, TSource> whenBuilder)
    {
        _mergeBuilder = mergeBuilder;
        _whenBuilder = whenBuilder;
    }

    /// <summary>
    /// The parent SqlMergeBuilder instance.
    /// </summary>
    private readonly SqlMergeBuilder<TTarget> _mergeBuilder;

    /// <summary>
    /// Instance of the parent WhenBuilder that instantiated this builder.
    /// </summary>
    private readonly WhenBuilder<TTarget, TSource> _whenBuilder;
}