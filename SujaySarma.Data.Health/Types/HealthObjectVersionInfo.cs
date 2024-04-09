using System;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Types
{
    /// <summary>
    /// Versioning information encapsulated by a health-system object
    /// </summary>
    public class HealthObjectVersionInfo
    {

        /// <summary>
        /// Id of this version record
        /// </summary>
        [JsonPropertyName("versionId")]
        public Guid VersionId { get; init; }

        /// <summary>
        /// Numerical version number
        /// </summary>
        [JsonPropertyName("versionNumber")]
        public ulong VersionNumber { get; init; }


        /// <summary>
        /// Date/time this version was created at
        /// </summary>
        [JsonPropertyName("created")]
        public DateTimeUtc Created { get; init; }

        /// <summary>
        /// Id of the previous version record. 
        /// NULL if none.
        /// </summary>
        [JsonPropertyName("previousVersionId")]
        public Guid? PreviousVersionId { get; init; }


        /// <summary>
        /// Initialise
        /// </summary>
        public HealthObjectVersionInfo()
        {
            VersionId = Guid.Empty;
            VersionNumber = 1;
            Created = new DateTimeUtc();
            PreviousVersionId = null;
        }

        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="previousVersion">Reference to the previous version</param>
        public HealthObjectVersionInfo(HealthObjectVersionInfo previousVersion)
        {
            VersionId = Guid.NewGuid();
            VersionNumber = previousVersion.VersionNumber + 1;
            Created = new DateTimeUtc();
            PreviousVersionId = previousVersion.VersionId;
        }


        /// <summary>
        /// The default initial version of any versionable object
        /// </summary>
        public static HealthObjectVersionInfo INITIAL_VERSION
        {
            get
            {
                if (_initial_version_cached == null)
                {
                    _initial_version_cached = new HealthObjectVersionInfo();
                }

                return _initial_version_cached;
            }
        }
        private static HealthObjectVersionInfo? _initial_version_cached = null;
    }
}
