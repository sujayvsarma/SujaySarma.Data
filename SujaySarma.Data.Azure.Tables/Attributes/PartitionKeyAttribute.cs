using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Constants;

using System;

namespace SujaySarma.Data.Azure.Tables.Attributes
{
    /// <summary>
    /// Marks the property or field as the Partition Key for the table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class PartitionKeyAttribute : ContainerMemberAttribute
    {
        /// <summary>
        /// Marks the property or field as the Partition Key for the table.
        /// </summary>
        public PartitionKeyAttribute()
            : base(ReservedNames.PartitionKey)
        {
            IncludeFor = ColumnInclusionStrategy.Inserts | ColumnInclusionStrategy.Deletes;
            IfEnumSerialiseAs = EnumSerializationStrategy.AsString;
            SerialiseAsJson = false;
            DefaultValueProviderFunction = (() => "PK");
        }
    }
}
