using System.Text.Json.Serialization;

using SujaySarma.Data.Health.Constants;
using SujaySarma.Data.Health.Personas;

namespace SujaySarma.Data.Health.Types
{
    /// <summary>
    /// Capabilities or qualifications of a medical practitioner
    /// </summary>
    public class MedicalPractitionerCapabilities
    {
        /// <summary>
        /// Authority validating the capability claim
        /// </summary>
        [JsonPropertyName("authority")]
        public Establishment ClaimAuthority { get; set; }

        /// <summary>
        /// Type of capability
        /// </summary>
        [JsonPropertyName("type")]
        public EnumCapabilityClaimTypes Type { get; set; }

        /// <summary>
        /// The capability
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Initialise
        /// </summary>
        public MedicalPractitionerCapabilities()
        {
            ClaimAuthority = new Establishment();
            Type = EnumCapabilityClaimTypes.Skill;
            Name = string.Empty;
        }
    }
}
