using SujaySarma.Data.Core.Constants;

using System;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// Provides an auto-gen value enabled primary key that operates on Guid values
    /// </summary>
    public class GuidPrimaryKeyMemberAttribute : ContainerMemberAttribute
    {
        /// <summary>
        /// Defines an auto-gen value enabled primary key that operates on Guid values
        /// </summary>
        /// <param name="name">The name of the underlying column (eg: name of the table column).</param>
        public GuidPrimaryKeyMemberAttribute(string name)
          : base(name)
        {
            DefaultValueProviderFunction =(() => Guid.NewGuid());

            base.SerialiseAsJson = false;
            base.IncludeFor = ColumnInclusionStrategy.Inserts;
            base.IsSearchKey = true;
        }
    }
}