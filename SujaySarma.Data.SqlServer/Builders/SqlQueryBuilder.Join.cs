using SujaySarma.Data.SqlServer.Builders.Constants;
using SujaySarma.Data.SqlServer.Builders.Internal;

using System;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of JOIN
public sealed partial class SqlQueryBuilder
{
    /// <summary>
    /// Add an INNER JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">Type of entity of the LEFT side table.</typeparam>
    /// <typeparam name="TRight">Type of entity of the RIGHT side table.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">SQL Hints for the JOIN clause (only those that apply to SELECT are valid here!).</param>
    /// <param name="leftTableColumnSelector">[optional] Column selector expression for the LEFT side table to add columns to SELECT. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }" Provide column aliases by assigning them to the 
    /// alias [for example: (e) => "alias" = e.PropertyName].</param>
    /// <param name="rightTableColumnSelector">[optional] Column selector expression for the RIGHT side table to add columns to SELECT. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }" Provide column aliases by assigning them to the 
    /// alias [for example: (e) => "alias" = e.PropertyName].</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder InnerJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints, 
        Expression<Func<TLeft, object>>? leftTableColumnSelector = null, Expression<Func<TRight, object>>? rightTableColumnSelector = null)
    {
        base.ResolveType(typeof(TLeft));
        base.ResolveType(typeof(TRight));

        _joins.InnerJoin<TLeft, TRight>(joinCondition, joinHints);

        if (leftTableColumnSelector is not null)
        {
            Select<TLeft>(leftTableColumnSelector);
        }

        if (rightTableColumnSelector is not null)
        {
            Select<TRight>(rightTableColumnSelector);
        }

        return this;
    }

    /// <summary>
    /// Add a LEFT (OUTER) JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">Type of entity of the LEFT side table.</typeparam>
    /// <typeparam name="TRight">Type of entity of the RIGHT side table.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">SQL Hints for the JOIN clause (only those that apply to SELECT are valid here!).</param>
    /// <param name="leftTableColumnSelector">[optional] Column selector expression for the LEFT side table to add columns to SELECT. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }" Provide column aliases by assigning them to the 
    /// alias [for example: (e) => "alias" = e.PropertyName].</param>
    /// <param name="rightTableColumnSelector">[optional] Column selector expression for the RIGHT side table to add columns to SELECT. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }" Provide column aliases by assigning them to the 
    /// alias [for example: (e) => "alias" = e.PropertyName].</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder LeftJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints,
        Expression<Func<TLeft, object>>? leftTableColumnSelector = null, Expression<Func<TRight, object>>? rightTableColumnSelector = null)
    {
        base.ResolveType(typeof(TLeft));
        base.ResolveType(typeof(TRight));

        _joins.LeftJoin<TLeft, TRight>(joinCondition, joinHints);

        if (leftTableColumnSelector is not null)
        {
            Select<TLeft>(leftTableColumnSelector);
        }

        if (rightTableColumnSelector is not null)
        {
            Select<TRight>(rightTableColumnSelector);
        }

        return this;
    }

    /// <summary>
    /// Add a RIGHT (OUTER) JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">Type of entity of the LEFT side table.</typeparam>
    /// <typeparam name="TRight">Type of entity of the RIGHT side table.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">SQL Hints for the JOIN clause (only those that apply to SELECT are valid here!).</param>
    /// <param name="leftTableColumnSelector">[optional] Column selector expression for the LEFT side table to add columns to SELECT. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }" Provide column aliases by assigning them to the 
    /// alias [for example: (e) => "alias" = e.PropertyName].</param>
    /// <param name="rightTableColumnSelector">[optional] Column selector expression for the RIGHT side table to add columns to SELECT. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }" Provide column aliases by assigning them to the 
    /// alias [for example: (e) => "alias" = e.PropertyName].</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder RightJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints,
        Expression<Func<TLeft, object>>? leftTableColumnSelector = null, Expression<Func<TRight, object>>? rightTableColumnSelector = null)
    {
        base.ResolveType(typeof(TLeft));
        base.ResolveType(typeof(TRight));

        _joins.RightJoin<TLeft, TRight>(joinCondition, joinHints);

        if (leftTableColumnSelector is not null)
        {
            Select<TLeft>(leftTableColumnSelector);
        }

        if (rightTableColumnSelector is not null)
        {
            Select<TRight>(rightTableColumnSelector);
        }

        return this;
    }

    /// <summary>
    /// Add a FULL JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">Type of entity of the LEFT side table.</typeparam>
    /// <typeparam name="TRight">Type of entity of the RIGHT side table.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">SQL Hints for the JOIN clause (only those that apply to SELECT are valid here!).</param>
    /// <param name="leftTableColumnSelector">[optional] Column selector expression for the LEFT side table to add columns to SELECT. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }" Provide column aliases by assigning them to the 
    /// alias [for example: (e) => "alias" = e.PropertyName].</param>
    /// <param name="rightTableColumnSelector">[optional] Column selector expression for the RIGHT side table to add columns to SELECT. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }" Provide column aliases by assigning them to the 
    /// alias [for example: (e) => "alias" = e.PropertyName].</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder FullJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints,
        Expression<Func<TLeft, object>>? leftTableColumnSelector = null, Expression<Func<TRight, object>>? rightTableColumnSelector = null)
    {
        base.ResolveType(typeof(TLeft));
        base.ResolveType(typeof(TRight));

        _joins.FullJoin<TLeft, TRight>(joinCondition, joinHints);

        if (leftTableColumnSelector is not null)
        {
            Select<TLeft>(leftTableColumnSelector);
        }

        if (rightTableColumnSelector is not null)
        {
            Select<TRight>(rightTableColumnSelector);
        }

        return this;
    }

    /// <summary>
    /// Add a CROSS JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">Type of entity of the LEFT side table.</typeparam>
    /// <typeparam name="TRight">Type of entity of the RIGHT side table.</typeparam>
    /// <param name="joinHints">SQL Hints for the JOIN clause (only those that apply to SELECT are valid here!).</param>
    /// <param name="leftTableColumnSelector">[optional] Column selector expression for the LEFT side table to add columns to SELECT. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }" Provide column aliases by assigning them to the 
    /// alias [for example: (e) => "alias" = e.PropertyName].</param>
    /// <param name="rightTableColumnSelector">[optional] Column selector expression for the RIGHT side table to add columns to SELECT. A single column may be represented as "(e) => e.PropertyName", 
    /// multiple properties/fields maybe represented as "(e) => new { e.Property1, e.Field1, ... }" Provide column aliases by assigning them to the 
    /// alias [for example: (e) => "alias" = e.PropertyName].</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder CrossJoin<TLeft, TRight>(SqlHint joinHints,
        Expression<Func<TLeft, object>>? leftTableColumnSelector = null, Expression<Func<TRight, object>>? rightTableColumnSelector = null)
    {
        base.ResolveType(typeof(TLeft));
        base.ResolveType(typeof(TRight));

        _joins.CrossJoin<TLeft, TRight>(joinHints);

        if (leftTableColumnSelector is not null)
        {
            Select<TLeft>(leftTableColumnSelector);
        }

        if (rightTableColumnSelector is not null)
        {
            Select<TRight>(rightTableColumnSelector);
        }

        return this;
    }

    private readonly SqlJoin _joins = new SqlJoin();
}
