using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Constants;

using System;
using System.Collections.Generic;

namespace SujaySarma.Data.Tests.Core.Reflection
{
    /// <summary>
    /// A target class to perform our reflection-based method unit testing
    /// </summary>
    [Container("Table 1")]
    internal class ReflectionTestsClassTarget
    {
        [GuidPrimaryKeyMember("Id")]
        public Guid PrimaryKey
        {
            get; set;
        }

        [DateTimeAuditMember("Created", DateTimeKind = DateTimeKind.Utc, IncludeInDataModificationOperation = DataModificationInclusionBehaviour.Inserts, IsSearchKey = false)]
        public DateTime Created
        {
            get; set;
        }

        [DateTimeAuditMember("LastModified", DateTimeKind = DateTimeKind.Utc, IncludeInDataModificationOperation = DataModificationInclusionBehaviour.Inserts | DataModificationInclusionBehaviour.Updates, IsSearchKey = false)]
        public DateTime LastModified
        {
            get; set;
        }

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