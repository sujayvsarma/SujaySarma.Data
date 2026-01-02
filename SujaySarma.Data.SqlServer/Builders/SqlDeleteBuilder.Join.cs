using SujaySarma.Data.SqlServer.Builders.Constants;
using SujaySarma.Data.SqlServer.Builders.Internal;

using System;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of JOIN.
public sealed partial class SqlDeleteBuilder
{
    /// <summary>
    /// Add an INNER JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">Type of entity of the LEFT side table.</typeparam>
    /// <typeparam name="TRight">Type of entity of the RIGHT side table.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">SQL Hints for the JOIN clause (only those that apply to SELECT are valid here!).</param>
    /// <returns>Instance of self.</returns>
    public SqlDeleteBuilder InnerJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints)
    {
        base.ResolveType(typeof(TLeft));
        base.ResolveType(typeof(TRight));

        _joins.InnerJoin<TLeft, TRight>(joinCondition, joinHints);
        return this;
    }

    /// <summary>
    /// Add a LEFT (OUTER) JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">Type of entity of the LEFT side table.</typeparam>
    /// <typeparam name="TRight">Type of entity of the RIGHT side table.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">SQL Hints for the JOIN clause (only those that apply to SELECT are valid here!).</param>
    /// <returns>Instance of self.</returns>
    public SqlDeleteBuilder LeftJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints)
    {
        base.ResolveType(typeof(TLeft));
        base.ResolveType(typeof(TRight));

        _joins.LeftJoin<TLeft, TRight>(joinCondition, joinHints);
        return this;
    }

    /// <summary>
    /// Add a RIGHT (OUTER) JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">Type of entity of the LEFT side table.</typeparam>
    /// <typeparam name="TRight">Type of entity of the RIGHT side table.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">SQL Hints for the JOIN clause (only those that apply to SELECT are valid here!).</param>
    /// <returns>Instance of self.</returns>
    public SqlDeleteBuilder RightJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints)
    {
        base.ResolveType(typeof(TLeft));
        base.ResolveType(typeof(TRight));

        _joins.RightJoin<TLeft, TRight>(joinCondition, joinHints);
        return this;
    }

    /// <summary>
    /// Add a FULL JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">Type of entity of the LEFT side table.</typeparam>
    /// <typeparam name="TRight">Type of entity of the RIGHT side table.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">SQL Hints for the JOIN clause (only those that apply to SELECT are valid here!).</param>
    /// <returns>Instance of self.</returns>
    public SqlDeleteBuilder FullJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints)
    {
        base.ResolveType(typeof(TLeft));
        base.ResolveType(typeof(TRight));

        _joins.FullJoin<TLeft, TRight>(joinCondition, joinHints);
        return this;
    }

    /// <summary>
    /// Add a CROSS JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">Type of entity of the LEFT side table.</typeparam>
    /// <typeparam name="TRight">Type of entity of the RIGHT side table.</typeparam>
    /// <param name="joinHints">SQL Hints for the JOIN clause (only those that apply to SELECT are valid here!).</param>
    /// <returns>Instance of self.</returns>
    public SqlDeleteBuilder CrossJoin<TLeft, TRight>(SqlHint joinHints)
    {
        base.ResolveType(typeof(TLeft));
        base.ResolveType(typeof(TRight));

        _joins.CrossJoin<TLeft, TRight>(joinHints);
        return this;
    }

    private readonly SqlJoin _joins = new SqlJoin();
}
