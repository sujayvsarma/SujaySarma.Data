using System;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Types of joins
    /// </summary>
    public enum TypesOfJoinsEnum
    {
        /// <summary>
        /// INNER JOIN
        /// </summary>
        Inner,

        /// <summary>
        /// LEFT JOIN
        /// </summary>
        Left,

        /// <summary>
        /// RIGHT JOIN
        /// </summary>
        Right,

        /// <summary>
        /// FULL JOIN
        /// </summary>
        Full
    }

    /// <summary>
    /// Helper methods to work with <c>TypesOfJoinsEnum</c>.
    /// </summary>
    public static class TypesOfJoinsEnumExtensions
    {
        /// <summary>
        /// Converts the enum to a string representation.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The string representation of the enum.</returns>
        public static string ToSqlString(this TypesOfJoinsEnum value)
            => value switch
            {
                TypesOfJoinsEnum.Inner => "INNER JOIN",
                TypesOfJoinsEnum.Left => "LEFT JOIN",
                TypesOfJoinsEnum.Right => "RIGHT JOIN",
                TypesOfJoinsEnum.Full => "FULL JOIN",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
    }

}
