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
        /// If set, the column value is changed only during INSERTs. During other operations, it will appear in the WHERE clause only!
        /// </summary>
        PrimaryKey = 1,

        /// <summary>
        /// Foreign key. 
        /// When set, causes child-reference objects to be loaded using implicit joins. Also populate TableColumn attribute's ReferencedTable and ReferencedColumns.
        /// </summary>
        Foreign = 2,

        /// <summary>
        /// An IDENTITY value.
        /// If set, the column value is changed only during INSERTs. During other operations, it will appear in the WHERE clause only!
        /// </summary>
        Identity = 8
    }

}
