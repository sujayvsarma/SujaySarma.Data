using SujaySarma.Data.Core.Constants;

using System;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// Provides an auto-gen value enabled field that operates on DateTime values
    /// </summary>
    public class DateTimeAuditMemberAttribute : ContainerMemberAttribute
    {
        /// <summary>
        /// Toggle whether the date/time provided will be a local timestamp (server time if this is a server application)
        /// or the Utc time.
        /// </summary>
        public DateTimeKind DateTimeKind { get; init; }

        /// <summary>
        /// Defines an auto-gen value enabled field that operates on Utc DateTime values
        /// </summary>
        /// <param name="name">The name of the underlying column (eg: name of the table column).</param>
        public DateTimeAuditMemberAttribute(string name)
          : base(name)
        {
            DateTimeKind = DateTimeKind.Utc;
            DefaultValueProviderFunction = (() => DateTime.UtcNow);
        }

        /// <summary>
        /// Defines an auto-gen value enabled field that operates on the specified DateTime values
        /// </summary>
        /// <param name="name">The name of the underlying column (eg: name of the table column).</param>
        /// <param name="kind">Specify whether the DateTime values shall be local or Utc</param>
        public DateTimeAuditMemberAttribute(string name, DateTimeKind kind)
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
