using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Constants;

using System;
using System.Collections.Generic;

namespace SujaySarma.Data.Tests.Core.Reflection
{
    /// <summary>
    /// A target record to perform our reflection-based method unit testing
    /// </summary>
    [Container("Table 3")]
    internal record ReflectionTestsRecordTarget([property: GuidPrimaryKeyMember("Id")] Guid PrimaryKey
    , [property: DateTimeAuditMember("Created", DateTimeKind = DateTimeKind.Utc, IncludeInDataModificationOperation = DataModificationInclusionBehaviour.Inserts, IsSearchKey = false)] DateTime Created
    , [property: DateTimeAuditMember("LastModified", DateTimeKind = DateTimeKind.Utc, IncludeInDataModificationOperation = DataModificationInclusionBehaviour.Inserts | DataModificationInclusionBehaviour.Updates, IsSearchKey = false)] DateTime LastModified
    )
    {
        [ContainerMember("Value 1")]
        public string Value1
        {
            get; set;

        } = string.Empty;

        [ContainerMember("Value 2", AllowSerializationAsJson = true)]
        public List<int> Value2
        {
            get; set;

        } = new List<int>();
    }
}