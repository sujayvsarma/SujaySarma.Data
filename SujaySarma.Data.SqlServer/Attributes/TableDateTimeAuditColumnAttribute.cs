using System;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// Provides an auto-gen value enabled field that operates on DateTime values
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableDateTimeAuditColumnAttribute : TableColumnAttribute
    {

        /// <summary>
        /// Provides an auto-gen value enabled field that operates on DateTime values
        /// </summary>
        /// <param name="columnName">The name of the underlying table column</param>
        public TableDateTimeAuditColumnAttribute(string columnName)
            : base(columnName)
        {
        }

    }


}
