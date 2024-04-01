using System;

using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Constants;

namespace SujaySarma.Data.Azure.Tables.Attributes
{
    /// <summary>
    /// Marks the property or field as the Partition Key for the table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PartitionKeyAttribute : ContainerMemberAttribute
    {
        /// <summary>
        /// Mark the property or field as a PartitionKey
        /// </summary>
        public PartitionKeyAttribute()
            : base(ReservedNames.PartitionKey)
        {
            base.IncludeInDataModificationOperation = DataModificationInclusionBehaviour.Inserts | DataModificationInclusionBehaviour.Deletes;
            base.EnumSerializationStrategy = EnumSerializationBehaviour.AsString;
            
            // Because complex-types cannot be a PK
            base.AllowSerializationAsJson = false;

            base.DefaultValueProviderFunction = () => "PK";
        }
    }
}
