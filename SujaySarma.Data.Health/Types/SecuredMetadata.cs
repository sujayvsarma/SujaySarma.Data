using System.Text.Json.Serialization;

using SujaySarma.Data.Health.BaseTypes;

namespace SujaySarma.Data.Health.Types
{
    /// <summary>
    /// An instance of a secured piece of metadata
    /// </summary>
    public class SecuredMetadata : HealthObjectSecurityBase
    {

        /// <summary>
        /// Name of the metadata
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Secured value
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }


        /// <summary>
        /// Initialise
        /// </summary>
        public SecuredMetadata()
            : base()
        {
            Name = Value = string.Empty;
        }

        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="metadata">Instance to copy from</param>
        public SecuredMetadata(SecuredMetadata metadata)
        {
            this.Name = metadata.Name;
            this.Value = metadata.Value;

            this.PermitAnonymousRead = metadata.PermitAnonymousRead;
            this.PermitPermanentDelete = metadata.PermitPermanentDelete;
            this.AuditAccess = metadata.AuditAccess;

            this.DenyPrincipals.AddRange(metadata.DenyPrincipals);
            this.ReadPrincipals.AddRange(metadata.ReadPrincipals);
            this.WritePrincipals.AddRange(metadata.WritePrincipals);
        }
    }
}
