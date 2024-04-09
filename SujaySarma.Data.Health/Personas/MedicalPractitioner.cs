using System.Collections.Generic;
using System.Text.Json.Serialization;

using SujaySarma.Data.Health.BaseTypes;
using SujaySarma.Data.Health.SubsidiaryObjects;
using SujaySarma.Data.Health.Types;

namespace SujaySarma.Data.Health.Personas
{
    /// <summary>
    /// Defines a medical practitioner
    /// </summary>
    public class MedicalPractitioner : Person, IHealthVersionableObject<MedicalPractitioner>
    {
        /// <summary>
        /// Capabilities of the practitioner
        /// </summary>
        [JsonPropertyName("capabilities")]
        public List<MedicalPractitionerCapabilities> Capabilities { get; }

        /// <summary>
        /// Availability information
        /// </summary>
        [JsonPropertyName("availability")]
        public List<CalendarAvailability> Availabilities { get; }


        /// <summary>
        /// Initialise
        /// </summary>
        public MedicalPractitioner()
            : base()
        {
            Capabilities = new List<MedicalPractitionerCapabilities>();
            Availabilities = new List<CalendarAvailability>();
        }

        /// <summary>
        /// Creates a clone of the provided medical practitioner instance as a new version
        /// </summary>
        /// <param name="instance">Instance to clone</param>
        private protected MedicalPractitioner(MedicalPractitioner instance)
            : base(instance)
        {
            Capabilities = new List<MedicalPractitionerCapabilities>();
            Capabilities.AddRange(instance.Capabilities);

            Availabilities = new List<CalendarAvailability>();
            Availabilities.AddRange(instance.Availabilities);
        }

        /// <summary>
        /// Create a new version of the current medical practitioner record
        /// </summary>
        /// <returns>New instance of medical practitioner with an incremented version number</returns>
        MedicalPractitioner IHealthVersionableObject<MedicalPractitioner>.CreateVersion()
            => new MedicalPractitioner(this);
    }



}
