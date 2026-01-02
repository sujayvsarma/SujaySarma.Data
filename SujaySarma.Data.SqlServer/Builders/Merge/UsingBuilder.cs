using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;

using System;
using System.Linq.Expressions;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders.Merge;

/// <summary>
/// Helps build the USING clause of the mergeBuilder MERGE statement.
/// </summary>
/// <typeparam name="TTarget">The <see cref="Type"/> of the target table/entity for the MERGE statement.</typeparam>
public sealed class UsingBuilder<TTarget>
{
    /// <summary>
    /// Provide the source table (<typeparamref name="TSource"/>) and the join <paramref name="condition"/> for the 
    /// MERGE operation's USING clause.
    /// </summary>
    /// <typeparam name="TSource">The <see cref="Type"/> of the table to compare with -- this can be the same as <typeparamref name="TTarget"/> or another table.</typeparam>
    /// <param name="condition">The condition to be used to INNER JOIN the <typeparamref name="TTarget"/> and <typeparamref name="TSource"/>.</param>
    /// <returns>An instance of the <see cref="MatchBuilder{TSource}"/> that continues the statement building.</returns>
    public MatchBuilder<TSource> UsingTable<TSource>(Expression<Func<TTarget, TSource, bool>> condition)
    {
        PersistenceContainerInfo source = typeof(TSource).RetrievePersistenceContainerInfoOrThrowException();
        if (! SqlExpressionParser.IsValidCondition(condition, out string? errorMessage))
        {
            throw new ArgumentException($"The provided condition is not valid: {errorMessage}", nameof(condition));
        }

        StringBuilder clauseLet = new StringBuilder()
            .Append("USING ").Append(source.PersistenceInfo.CreateQualifiedName()).Append(" AS ").AppendLine(source.ReferenceAlias)
            .Append(" ON ").Append(SqlExpressionParser.Parse(condition));

        _parent.Write(clauseLet);
        _parent.Write(" ");

        MatchBuilder<TSource> builder = new MatchBuilder<TSource>(_parent);
        return builder;
    }

    /// <summary>
    /// Provide the source dataset as the result of a SELECT <paramref name="query"/> that map to the MERGE target table <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">The <see cref="Type"/> of the table to compare with -- this can be the same as <typeparamref name="TTarget"/> 
    /// or another table. This same type must be the <see cref="Type"/> of used with <see cref="SqlQueryBuilder.From{TTable}"/>.</typeparam>
    /// <param name="query">The SELECT query that will return the dataset to be used to compare for the merge.</param>
    /// <param name="condition">The condition to be used to INNER JOIN the <typeparamref name="TTarget"/> and <typeparamref name="TSource"/>.</param>
    /// <returns>An instance of the <see cref="MatchBuilder{TSource}"/> that continues the statement building.</returns>
    public MatchBuilder<TSource> UsingQuery<TSource>(SqlQueryBuilder query, Expression<Func<TTarget, TSource, bool>> condition)
    {
        if (!_parent.IsSameTableTarget(typeof(TSource), query._primaryTable.EntityType))
        {
            throw new ArgumentException("The provided query's table does not map to the source table type.", query._primaryTable.EntityType.GetUsableTypeName());
        }

        PersistenceContainerInfo source = typeof(TSource).RetrievePersistenceContainerInfoOrThrowException();
        if (!SqlExpressionParser.IsValidCondition(condition, out string? errorMessage))
        {
            throw new ArgumentException($"The provided condition is not valid: {errorMessage}", nameof(condition));
        }

        // We need to adjust a few things in the query.
        string queryString = query.Build().ToString();

        // Remove the aliasing of the primary table unless it has JOINs
        if (!queryString.Contains(" JOIN ", StringComparison.OrdinalIgnoreCase))
        {
            queryString = queryString.Replace($" AS {query._primaryTable.ReferenceAlias}", string.Empty);
            queryString = queryString.Replace($"{query._primaryTable.ReferenceAlias}.", string.Empty);
            queryString = queryString.Replace($" {query._primaryTable.ReferenceAlias}", string.Empty);
        }

        // Remove the trailing semicolon.
        queryString = queryString.Remove(queryString.Length - 1, 1);


        StringBuilder clauseLet = new StringBuilder()
            .Append("USING (").Append(queryString).Append(") AS ").AppendLine(source.ReferenceAlias)
            .Append(" ON ").Append(SqlExpressionParser.Parse(condition)).Append(' ');

        _parent.Write(clauseLet);

        MatchBuilder<TSource> builder = new MatchBuilder<TSource>(_parent);
        return builder;
    }


    /// <summary>
    /// Initialise.
    /// </summary>
    /// <param name="parent">Instance of the <see cref="SqlMergeBuilder{TTarget}"/> to return to.</param>
    internal UsingBuilder(SqlMergeBuilder<TTarget> parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// The parent MERGE builder.
    /// </summary>
    private SqlMergeBuilder<TTarget> _parent;


    /// <summary>
    /// Helps chain the <see cref="UsingBuilder{TTarget}"/> with the <see cref="WhenBuilder{TTarget, TSource}" />.
    /// </summary>
    /// <typeparam name="TSource">The <see cref="Type"/> of the source table/entity being compared (INNER JOINed) with.</typeparam>
    public sealed class MatchBuilder<TSource>
    {
        /// <summary>
        /// Begin defining the WHEN MATCHED, WHEN NOT MATCHED sub-clauses.
        /// </summary>
        /// <returns>An instance of the WhenBuilder to continue building the statement.</returns>
        public WhenBuilder<TTarget, TSource> BeginMatches()
        {
            if (_matchesBegun)
            {
                throw new InvalidOperationException("BeginMatches() been called already!");
            }

            _matchesBegun = true;
            return new WhenBuilder<TTarget, TSource>(_mergeBuilder);
        }

        /// <summary>
        /// Initialise.
        /// </summary>
        /// <param name="mergeBuilder">Instance of the <see cref="SqlMergeBuilder{TTarget}"/> to return to.</param>
        internal MatchBuilder(SqlMergeBuilder<TTarget> mergeBuilder)
        {
            _mergeBuilder = mergeBuilder;
        }

        /// <summary>
        /// The parent MERGE builder.
        /// </summary>
        private readonly SqlMergeBuilder<TTarget> _mergeBuilder;

        /// <summary>
        /// A flag to keep track of whether matches have begun.
        /// </summary>
        private bool _matchesBegun = false;
    }
}
