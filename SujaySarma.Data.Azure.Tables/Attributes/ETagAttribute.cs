using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Constants;

using System;

namespace SujaySarma.Data.Azure.Tables.Attributes
{
    /// <summary>
    /// Marks the property or field as the ETag key for the table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ETagAttribute : ContainerMemberAttribute
    {
        /// <summary>Mark the property or field as an ETag key</summary>
        public ETagAttribute() 
            : base(ReservedNames.ETag)
        {
            IncludeFor = ColumnInclusionStrategy.Updates | ColumnInclusionStrategy.Deletes;
            DefaultValueProviderFunction = (() => "*");
        }
    }
}
