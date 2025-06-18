namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Sorting order
    /// </summary>
    public enum SortOrderEnum
    {
        /// <summary>
        /// Ascending order
        /// </summary>
        ASC,

        /// <summary>
        /// Descending order
        /// </summary>
        DESC
    }

    /// <summary>
    /// Provides extension methods for the <see cref="SortOrderEnum"/> enumeration.
    /// </summary>
    /// <remarks>This class contains methods that extend the functionality of the <see cref="SortOrderEnum"/>
    /// type, enabling convenient operations such as converting enum values to their SQL-compatible string
    /// representations.</remarks>
    public static class SortOrderEnumExtensions
    {
        /// <summary>
        /// Converts the enum to a string representation for SQL queries
        /// </summary>
        /// <param name="sortOrder">The sort order</param>
        /// <returns>String representation of the sort order</returns>
        public static string ToSqlString(this SortOrderEnum sortOrder)
            => ((sortOrder == SortOrderEnum.ASC) ? "ASC" : "DESC");
    }
}
