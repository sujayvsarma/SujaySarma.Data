using System;

using SujaySarma.Data.Core.Constants;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// Provides an auto-gen value enabled primary key that operates on Guid values
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableGuidPrimaryKeyColumnAttribute : TableColumnAttribute
    {

        /// <summary>
        /// Provides an auto-gen value enabled primary key that operates on Guid values
        /// </summary>
        /// <param name="columnName">The name of the underlying table column</param>
        public TableGuidPrimaryKeyColumnAttribute(string columnName)
            : base(columnName)
        {
            DefaultValueProviderFunction = (() => Guid.NewGuid());

            base.SerialiseAsJson = false;
            base.IncludeFor = ColumnInclusionStrategy.Inserts;
            base.IsSearchKey = true;
        }

    }


}
