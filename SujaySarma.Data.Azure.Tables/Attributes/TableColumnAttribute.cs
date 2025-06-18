using SujaySarma.Data.Core;

using System;

namespace SujaySarma.Data.Azure.Tables.Attributes
{
    /// <summary>
    /// Provide the data table column name the value for this property or field is stored in or retrieved from.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class TableColumnAttribute : ContainerMemberAttribute
    {
        /// <summary>
        /// Provide the data table column name the value for this property or field is stored in or retrieved from.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        public TableColumnAttribute(string columnName)
            : base(columnName)
        {
        }

        /// <summary>
        /// Default constructor.
        /// Made PRIVATE so nobody can use it.
        /// </summary>
        private TableColumnAttribute() 
            : base() 
        {
        }
    }
}
