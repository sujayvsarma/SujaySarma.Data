using System;

using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Constants;

namespace SujaySarma.Data.Azure.Tables.Attributes
{
    /// <summary>
    /// Marks the property or field as the Row Key for the table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class RowKeyAttribute : ContainerMemberAttribute
    {
        /// <summary>
        /// Mark the property or field as a RowKey
        /// </summary>
        public RowKeyAttribute()
            : base(ReservedNames.RowKey)
        {
            base.IncludeInDataModificationOperation = DataModificationInclusionBehaviour.Inserts | DataModificationInclusionBehaviour.Deletes;
            base.EnumSerializationStrategy = EnumSerializationBehaviour.AsString;

            // Because complex-types cannot be a PK
            base.AllowSerializationAsJson = false;

            base.DefaultValueProviderFunction = () => Guid.NewGuid().ToString("d");
        }
    }
}
