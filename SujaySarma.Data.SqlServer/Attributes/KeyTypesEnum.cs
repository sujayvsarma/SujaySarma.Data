using System;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// Provides a list of types of supported "keys" against table columns.
    /// </summary>
    [Flags]
    public enum KeyTypesEnum
    {
        /// <summary>
        /// Not a key.
        /// </summary>
        None = 0,

        /// <summary>
        /// Primary key.
        /// </summary>
        PrimaryKey = 1,

        /// <summary>
        /// UNIQUE key.
        /// </summary>
        UniqueKey = 2,

        /// <summary>
        /// An IDENTITY value.
        /// </summary>
        Identity = 8
    }

}
