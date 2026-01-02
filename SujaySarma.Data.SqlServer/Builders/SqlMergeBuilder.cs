using SujaySarma.Data.SqlServer.Builders.Constants;
using SujaySarma.Data.SqlServer.Builders.Merge;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SujaySarma.Data.SqlServer.Builders;

/// <summary>
/// Helps assemble the MERGE statement, with all of its complexities, in a fluid-style.
/// Supports: USING, WHEN MATCHED, WHEN NOT MATCHED BY TARGET, WHEN NOT MATCHED BY SOURCE, OUTPUT, WITH clauses.
/// </summary>
/// <typeparam name="TTarget">The <see cref="Type"/> of the target table/entity for the MERGE statement.</typeparam>
public sealed partial class SqlMergeBuilder<TTarget> : SqlStatementBuilder
{

    /// <summary>
    /// Assembles the complete MERGE statement.
    /// </summary>
    /// <returns>An instance of <see cref="StringBuilder"/> containing the completed statement.</returns>
    public override StringBuilder Build()
    {
        // If there is a trailing space, remove it.
        if (_mergeBuilder[_mergeBuilder.Length - 1] is ' ')
        {
            _mergeBuilder.Remove(_mergeBuilder.Length - 1, 1);
        }

        if (_mergeBuilder[_mergeBuilder.Length - 1] is not ';')
        {
            _mergeBuilder.Append(';');
        }

        return _mergeBuilder;
    }

    /// <summary>
    /// Create a new instance of the SqlMergeBuilder, and return the UsingBuilder to continue 
    /// building the MERGE statement.
    /// </summary>
    /// <param name="lockingHint">SQL LOCK hints for the <typeparamref name="TTarget"/> table. HOLDLOCK/SERIALIZABLE is recommended, though 
    /// TABLOCK, UPDLOCK, XLOCK can also be taken. Be mindful that the MERGE statement has potential to cause really ugly concurrency issues.</param>
    /// <param name="top">If specified, limits the number of rows affected by the MERGE statement to the provided count.</param>
    /// <param name="topIsPercent">If true, indicates <paramref name="top"/> is a percent value.</param>
    /// <returns>An instance of a <see cref="UsingBuilder{TTarget}"/> to continue building the MERGE statement.</returns>
    public static UsingBuilder<TTarget> Create(SqlHint lockingHint = SqlHint.HoldLock, uint? top = null, bool topIsPercent = false)
    {
        List<SqlHint> lockHints = new List<SqlHint>();
        if (!lockHints.TryAdd(lockingHint, SqlStatementType.Merge, out string? errorMessage))
        {
            throw new ArgumentException($"One or more hints are not valid for statement type 'MERGE': {errorMessage}", nameof(lockingHint));
        }

        if (topIsPercent && (top > 100))
        {
            throw new ArgumentOutOfRangeException(nameof(top), $"Count cannot be greater than 100 when {nameof(topIsPercent)} is true.");
        }

        SqlMergeBuilder<TTarget> builder = new SqlMergeBuilder<TTarget>(lockHints, top, topIsPercent);
        return builder._usingBuilder;
    }

    /// <summary>
    /// Private constructor to prevent accidental initialisation.
    /// </summary>
    /// <param name="lockHints">SQL LOCK hints for the <typeparamref name="TTarget"/> table. HOLDLOCK/SERIALIZABLE is recommended, though 
    /// TABLOCK, UPDLOCK, XLOCK can also be taken. Be mindful that the MERGE statement has potential to cause really ugly concurrency issues.</param>
    /// <param name="top">If specified, limits the number of rows affected by the MERGE statement to the provided count.</param>
    /// <param name="topIsPercent">If true, indicates <paramref name="top"/> is a percent value.</param>
    private SqlMergeBuilder(List<SqlHint> lockHints, uint? top = null, bool topIsPercent = false)
        : base(typeof(TTarget))
    {
        _mergeBuilder = new StringBuilder()
            .Append("MERGE ");

        if (top is not null)
        {
            _mergeBuilder.Append("TOP (").Append(top.Value).Append(") ");
            if (topIsPercent)
            {
                _mergeBuilder.Append("PERCENT ");
            }
        }

        _mergeBuilder.Append(_primaryTable.PersistenceInfo.CreateQualifiedName()).Append(' ');

        if (lockHints.Count > 0)
        {
            _mergeBuilder.Append("WITH (").AppendJoin(", ", lockHints.Select(h => h.ToSQL())).Append(") ");
        }

        _mergeBuilder.Append("AS ").Append(_primaryTable.ReferenceAlias).Append(' ').AppendLine();

        _usingBuilder = new UsingBuilder<TTarget>(this);
    }

    /// <summary>
    /// Write the provided <paramref name="clause"/> to the _mergeBuilder instance.
    /// </summary>
    /// <param name="clause">Portion of the clause to write to the builder instance.</param>
    internal void Write(StringBuilder clause)
    {
        _mergeBuilder.Append(clause);
    }

    /// <summary>
    /// Write the provided <paramref name="clause"/> to the _mergeBuilder instance.
    /// </summary>
    /// <param name="clause">Portion of the clause to write to the builder instance.</param>
    internal void Write(string clause)
    {
        _mergeBuilder.Append(clause);
    }


    /// <summary>
    /// Reference to the USING builder.
    /// </summary>
    private readonly UsingBuilder<TTarget> _usingBuilder;

    /// <summary>
    /// Instance of a StringBuilder that all clause-builders write back to.
    /// </summary>
    private readonly StringBuilder _mergeBuilder;
}
