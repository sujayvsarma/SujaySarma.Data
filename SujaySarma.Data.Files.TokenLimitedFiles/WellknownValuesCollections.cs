using System.Collections.Generic;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /// <summary>
    /// Collection of well-known values
    /// </summary>
    public static class WellknownValuesCollections
    {

        /// <summary>
        /// Values that are treated as "true"
        /// </summary>
        public static readonly List<string> TrueValues = new List<string>()
        {
            "true",
            "yes",
            "1"
        };

        /// <summary>
        /// Values that are treated as "false"
        /// </summary>
        public static readonly List<string> FalseValues = new List<string>()
        {
            "false",
            "no",
            "0",
            string.Empty
        };

    }
}
