using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Constants;

using System;

namespace SujaySarma.Data.Azure.Tables.Attributes
{
    /// <summary>
    /// Marks the property or field as the Timestamp value for the table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class TimestampAttribute : DateTimeAuditMemberAttribute
    {
        /// <summary>Mark the property or field as the Timestamp value</summary>
        /// <remarks>
        ///     All date/time values in Azure Tables is stored in UTC format only!
        /// </remarks>
        public TimestampAttribute()
          : base(ReservedNames.Timestamp, DateTimeKind.Utc)
        {
            IncludeFor = ColumnInclusionStrategy.Inserts | ColumnInclusionStrategy.Deletes;
        }
    }
}
