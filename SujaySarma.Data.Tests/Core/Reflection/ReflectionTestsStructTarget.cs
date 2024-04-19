using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Constants;

using System;
using System.Collections.Generic;

namespace SujaySarma.Data.Tests.Core.Reflection
{
    /// <summary>
    /// A target struct to perform our reflection-based method unit testing
    /// </summary>
    [Container("Table 2")]
    internal struct ReflectionTestsStructTarget
    {
        [GuidPrimaryKeyMember("Id")]
        public Guid PrimaryKey;

        [DateTimeAuditMember("Created", DateTimeKind = DateTimeKind.Utc, IncludeInDataModificationOperation = DataModificationInclusionBehaviour.Inserts, IsSearchKey = false)]
        public DateTime Created;

        [DateTimeAuditMember("LastModified", DateTimeKind = DateTimeKind.Utc, IncludeInDataModificationOperation = DataModificationInclusionBehaviour.Inserts | DataModificationInclusionBehaviour.Updates, IsSearchKey = false)]
        public DateTime LastModified;

        [ContainerMember("Value 1")]
        public string Value1 = string.Empty;

        [ContainerMember("Value 2", AllowSerializationAsJson = true)]
        public List<int> Value2 = new List<int>();


        public ReflectionTestsStructTarget()
        {
            PrimaryKey = Guid.Empty;
            Created = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
        }
    }
}