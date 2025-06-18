using System;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Operators that join two condition clauses together.
    /// </summary>
    public enum OperatorsToJoinConditionsEnum
    {
        /// <summary>
        /// AND
        /// </summary>
        And = 0,

        /// <summary>
        /// Or
        /// </summary>
        Or = 1
    }

    /// <summary>
    /// Helper methods to work with <c>OperatorsToJoinConditionsEnum</c>.
    /// </summary>
    public static class OperatorsToJoinConditionsEnumExtensions
    {
        /// <summary>
        /// Converts the enum to a string representation.
        /// </summary>
        /// <param name="value">The enum value.</param>
        /// <returns>The string representation of the enum.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when enum value provided is outside the value range of the Enum.</exception>
        public static string ToSqlString(this OperatorsToJoinConditionsEnum value)
            => value switch
            {
                OperatorsToJoinConditionsEnum.And => "AND",
                OperatorsToJoinConditionsEnum.Or => "OR",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
    }
}
