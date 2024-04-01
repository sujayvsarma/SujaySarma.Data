using System;

using SujaySarma.Data.Core;

namespace SujaySarma.Data.Azure.Tables.Attributes
{
    /// <summary>
    /// Provide the data table column name the value for this property or field is stored in or retrieved from.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class TableColumnAttribute : ContainerMemberAttribute
    {
        /// <summary>
        /// Provides information about the table column used to contain the data for an object.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        public TableColumnAttribute(string columnName)
            : base(columnName)
        {      
        }
    }
}
