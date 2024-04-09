using System;
using System.Text.Json.Serialization;
using SujaySarma.Data.Health.Types;

namespace SujaySarma.Data.Health.BaseTypes
{
    /// <summary>
    /// Defines a health-system object with versioning
    /// </summary>
    public class HealthVersionedObjectBase : HealthObjectBase
    {
        /// <summary>
        /// Version of this record
        /// </summary>
        [JsonPropertyName("version")]
        public HealthObjectVersionInfo Version { get; init; }


        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="previousVersion">The previous version of the object</param>
        protected HealthVersionedObjectBase(HealthObjectVersionInfo previousVersion)
            : base()
        {
            Id = Guid.NewGuid();
            Version = new HealthObjectVersionInfo(previousVersion);
        }
    }
}
