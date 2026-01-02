using SujaySarma.Data.SqlServer.Builders.Internal;

using System;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of WHERE.
public sealed partial class SqlQueryBuilder
{
    #region === AND ===

    /// <summary>
    /// Appends a WHERE condition using the provided expression, concatenating it with the previous one(s) using the AND operator.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of the table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder AndWhere<TTable>(Expression<Func<TTable, bool>> condition)
    {
        if (_where is null)
        {
            throw new InvalidOperationException("WHERE clause has not been initialized. Use Where() to add the first condition.");
        }

        _where.AndWhere<TTable>(condition);
        return this;
    }

    /// <summary>
    /// Appends a WHERE condition using the provided expression, concatenating it with the previous one(s) using the AND operator.
    /// </summary>
    /// <typeparam name="TTable1">The <see cref="Type"/> of one table-mapped entity participating in this condition.</typeparam>
    /// <typeparam name="TTable2">The <see cref="Type"/> of the other table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder AndWhere<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> condition)
    {
        if (_where is null)
        {
            throw new InvalidOperationException("WHERE clause has not been initialized. Use Where() to add the first condition.");
        }

        _where.AndWhere<TTable1, TTable2>(condition);
        return this;
    }

    #endregion

    #region === OR ===

    /// <summary>
    /// Appends a WHERE condition using the provided expression, concatenating it with the previous one(s) using the OR operator.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of the table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder OrWhere<TTable>(Expression<Func<TTable, bool>> condition)
    {
        if (_where is null)
        {
            throw new InvalidOperationException("WHERE clause has not been initialized. Use Where() to add the first condition.");
        }

        _where.OrWhere<TTable>(condition);
        return this;
    }

    /// <summary>
    /// Appends a WHERE condition using the provided expression, concatenating it with the previous one(s) using the OR operator.
    /// </summary>
    /// <typeparam name="TTable1">The <see cref="Type"/> of one table-mapped entity participating in this condition.</typeparam>
    /// <typeparam name="TTable2">The <see cref="Type"/> of the other table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder OrWhere<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> condition)
    {
        if (_where is null)
        {
            throw new InvalidOperationException("WHERE clause has not been initialized. Use Where() to add the first condition.");
        }

        _where.OrWhere<TTable1, TTable2>(condition);
        return this;
    }

    #endregion

    #region When it is the first condition being added

    /// <summary>
    /// Create a new WHERE condition using the provided expression.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of the table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder Where<TTable>(Expression<Func<TTable, bool>> condition)
    {
        if (_where is not null)
        {
            throw new InvalidOperationException("WHERE clause has already been initialized. Use AndWhere() or OrWhere() to add more conditions.");
        }

        _where = SqlWhere.Where<TTable>(condition);
        return this;
    }

    /// <summary>
    /// Create a new WHERE condition using the provided expression.
    /// </summary>
    /// <typeparam name="TTable1">The <see cref="Type"/> of one table-mapped entity participating in this condition.</typeparam>
    /// <typeparam name="TTable2">The <see cref="Type"/> of the other table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    /// <returns>Instance of self.</returns>
    public SqlQueryBuilder Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> condition)
    {
        if (_where is not null)
        {
            throw new InvalidOperationException("WHERE clause has already been initialized. Use AndWhere() or OrWhere() to add more conditions.");
        }

        _where = SqlWhere.Where<TTable1, TTable2>(condition);
        return this;
    }

    #endregion

    private SqlWhere? _where = null;
}
