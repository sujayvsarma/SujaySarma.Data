using System.Collections.Generic;

namespace SujaySarma.Data.Azure.Tables
{
    /// <summary>
    /// Contains names reserved for use in the Azure.Tables namespace
    /// </summary>
    internal static class ReservedNames
    {

        /// <summary>
        /// The ETag header value
        /// </summary>
        public static string ETag = "ETag";

        /// <summary>
        /// The PartitionKey column name
        /// </summary>
        public static string PartitionKey = "PartitionKey";

        /// <summary>
        /// The RowKey column name
        /// </summary>
        public static string RowKey = "RowKey";

        /// <summary>
        /// The Timestamp column name
        /// </summary>
        public static string Timestamp = "Timestamp";

        /// <summary>
        /// All the reserved names. Order is NOT important
        /// </summary>
        public static List<string> All = new List<string>()
        {
            ETag,
            PartitionKey,
            RowKey,
            Timestamp
        };

    }
}
