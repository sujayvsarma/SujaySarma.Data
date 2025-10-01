using System;

using SujaySarma.Data.Core.Constants;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// Provides an auto-gen value enabled field that operates on DateTime values
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableDateTimeAuditColumnAttribute : TableColumnAttribute
    {
        /// <summary>
        /// Toggle whether the date/time provided will be a local timestamp (server time if this is a server application)
        /// or the Utc time.
        /// </summary>
        public DateTimeKind DateTimeKind { get; init; }

        /// <summary>
        /// Provides an auto-gen value enabled field that operates on DateTime values
        /// </summary>
        /// <param name="columnName">The name of the underlying table column</param>
        public TableDateTimeAuditColumnAttribute(string columnName)
            : base(columnName)
        {
            DateTimeKind = DateTimeKind.Utc;

            base.DefaultValueProviderFunction = (() => DateTime.UtcNow);
        }

        /// <summary>
        /// Defines an auto-gen value enabled field that operates on the specified DateTime values
        /// </summary>
        /// <param name="name">The name of the underlying column (eg: name of the table column).</param>
        /// <param name="kind">Specify whether the DateTime values shall be local or Utc</param>
        public TableDateTimeAuditColumnAttribute(string name, DateTimeKind kind)
          : base(name)
        {
            DateTimeKind = kind;
            DefaultValueProviderFunction = (() => ((kind == DateTimeKind.Local) ? DateTime.Now : DateTime.UtcNow));

            base.SerialiseAsJson = false;

            // Consumer needs to set this, as this should be INSERTS only for Created, InsertsAndUpdates for Modified
            base.IncludeFor = ColumnInclusionStrategy.InsertsAndUpdates;
        }

    }


}
