using SujaySarma.Data.SqlServer.Attributes;

using System;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders.Merge;

/// <summary>
/// Handles the WHEN MATCHED and WHEN NOT MATCHED BY SOURCE actions -- either UPDATE or DELETE.
/// </summary>
/// <typeparam name="TTarget">The <see cref="Type"/> of the target table/entity for the MERGE statement.</typeparam>
/// <typeparam name="TSource">The <see cref="Type"/> of the source table/entity being compared (INNER JOINed) with.</typeparam>
public sealed class MatchActionUpdateOrDelete<TTarget, TSource>
{
    /// <summary>
    /// Prepares an UPDATE action for the matching WHEN clause.
    /// </summary>
    /// <returns>An <see cref="UpdateAction{TTarget, TSource}"/> that allows setting of column/values.</returns>
    public UpdateAction<TTarget, TSource> Update()
    {
        _mergeBuilder.Write("UPDATE SET ");
        return new UpdateAction<TTarget, TSource>(_mergeBuilder, _whenBuilder);
    }


    /// <summary>
    /// Prepares a DELETE action for the matching WHEN clause. This method intelligently handles 
    /// entities/tables marked as softdelete-aware.
    /// </summary>
    public WhenBuilder<TTarget, TSource> Delete()
    {
        if (_mergeBuilder._primaryTable.PersistenceInfo is SqlTableWithSoftDelete softDelete)
        {
            _mergeBuilder.Write(
                    (new StringBuilder()).Append("UPDATE SET ")
                        .Append(softDelete.SoftDeleteTableColumnName.EnsureIdentifierIsQuoted()).Append(" = 1 ")
                );
        }
        else
        {
            _mergeBuilder.Write("DELETE ");
        }

        return _whenBuilder;
    }

    /// <summary>
    /// Initialise.
    /// </summary>
    /// <param name="mergeBuilder">Instance of the <see cref="SqlMergeBuilder{TTarget}"/> to return to.</param>
    /// <param name="whenBuilder">Instance of the parent <see cref="WhenBuilder{TTarget, TSource}"/> that instantiated this builder.</param>
    internal MatchActionUpdateOrDelete(SqlMergeBuilder<TTarget> mergeBuilder, WhenBuilder<TTarget, TSource> whenBuilder)
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
