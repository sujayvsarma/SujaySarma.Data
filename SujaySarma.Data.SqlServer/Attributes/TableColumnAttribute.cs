using SujaySarma.Data.Core;

using System;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// Provide the data table column name and other flags used the value for this property or field is stored in or retrieved from.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableColumnAttribute : ContainerMemberAttribute
    {

        /// <summary>
        /// Provide the data table column name and other flags used the value for this property or field is stored in or retrieved from.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        public TableColumnAttribute(string columnName)
          : base(columnName)
        {
            Name = ((!string.IsNullOrWhiteSpace(columnName)) ? columnName : throw new ArgumentNullException(nameof(columnName)));
        }

        /// <inheritdoc />
        public override string CreateQualifiedName() 
            => $"[{Name}]";
    }

}
