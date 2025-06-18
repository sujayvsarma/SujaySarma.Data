namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Types of GROUP BY clauses
    /// </summary>
    public enum TypesOfGroupByEnum
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
}
