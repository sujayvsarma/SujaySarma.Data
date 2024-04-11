using System;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// Provide the data table column name and other flags used the value for this property or field is stored in or retrieved from.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableColumnAttribute : TableColumnPropertiesAttributeBase
    {

        /// <summary>
        /// Provides information about the table column used to contain the data for an object.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        public TableColumnAttribute(string columnName)
            : base(columnName)
        {
        }

        /// <inheritdoc />
        public override string CreateQualifiedName()
            => $"[{Name}]";
    }
}
