using System;

namespace SujaySarma.Data.SqlServer.Builders.Constants;

/// <summary>
/// SQL Operators
/// </summary>
public static class SqlOperators
{

    /// <summary>
    /// Operators that join two condition clauses together.
    /// </summary>
    public enum ConditionConcatenator
    {
        /// <summary>
        /// AND
        /// </summary>
        And = 0,

        /// <summary>
        /// OR
        /// </summary>
        Or = 1
    }

    /// <summary>
    /// The sorting order
    /// </summary>
    public enum SortOrder
    {
        /// <summary>
        /// ASC
        /// </summary>
        Asc = 0,

        /// <summary>
        /// DESC
        /// </summary>
        Desc = 1
    }

    /// <summary>
    /// GROUP BY
    /// </summary>
    public enum GroupBy
    {
        /// <summary>
        /// Standard
        /// </summary>
        Standard = 0,

        /// <summary>
        /// GROUP BY ROLL UP
        /// </summary>
        Rollup,

        /// <summary>
        /// GROUP BY CUBE
        /// </summary>
        Cube,

        /// <summary>
        /// GROUP BY GROUPING SETS
        /// </summary>
        GroupingSets,

        /// <summary>
        /// GROUP BY () -- produces a grand total
        /// </summary>
        EmptyGroup
    }

    /// <summary>
    /// JOIN
    /// </summary>
    public enum Join
    {
        /// <summary>
        /// INNER JOIN - Returns only matching rows from both tables
        /// </summary>
        Inner = 0,

        /// <summary>
        /// LEFT JOIN (LEFT OUTER JOIN) - Returns all rows from left table, matching rows from right
        /// </summary>
        Left,

        /// <summary>
        /// RIGHT JOIN (RIGHT OUTER JOIN) - Returns all rows from right table, matching rows from left
        /// </summary>
        Right,

        /// <summary>
        /// FULL JOIN (FULL OUTER JOIN) - Returns all rows from both tables
        /// </summary>
        Full,

        /// <summary>
        /// CROSS JOIN - Cartesian product of both tables
        /// </summary>
        Cross
    }

    /// <summary>
    /// Converts the provided <see cref="ConditionConcatenator"/> operator to its equivalent SQL string.
    /// </summary>
    /// <param name="value">The <see cref="ConditionConcatenator"/> operator to convert.</param>
    /// <returns>The string representation of the logical operator.</returns>
    public static string ToSQL(this ConditionConcatenator value)
        => value switch
        {
            SqlOperators.ConditionConcatenator.And => "AND",
            SqlOperators.ConditionConcatenator.Or => "OR",

            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };

    /// <summary>
    /// Converts the provided <see cref="SortOrder"/> operator to its equivalent SQL string.
    /// </summary>
    /// <param name="value">The <see cref="SortOrder"/> operator to convert.</param>
    /// <returns>The string representation of the sort order operator.</returns>
    public static string ToSQL(this SortOrder value)
        => value switch
        {
            SqlOperators.SortOrder.Asc => "ASC",
            SqlOperators.SortOrder.Desc => "DESC",

            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };

    /// <summary>
    /// Converts the provided <see cref="Join"/> operator to its equivalent SQL string.
    /// </summary>
    /// <param name="value">The <see cref="Join"/> operator to convert.</param>
    /// <returns>The string representation of the join operator.</returns>
    public static string ToSQL(this Join value)
        => value switch
        {
            SqlOperators.Join.Inner => "INNER JOIN",
            SqlOperators.Join.Left => "LEFT JOIN",
            SqlOperators.Join.Right => "RIGHT JOIN",
            SqlOperators.Join.Full => "FULL JOIN",
            SqlOperators.Join.Cross => "CROSS JOIN",

            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
}