using System;
using System.Text.Json.Serialization;
using SujaySarma.Data.Health.BaseTypes;

namespace SujaySarma.Data.Health.Personas
{
    /// <summary>
    /// A patient
    /// </summary>
    public class Patient : Person, IHealthVersionableObject<Patient>
    {

        /// <summary>
        /// Guid of the patient
        /// </summary>
        [JsonPropertyName("patientId")]
        public Guid PatientId { get; set; }

        /// <summary>
        /// Guid of the establishment creating this (version) record
        /// </summary>
        [JsonPropertyName("establishment.id")]
        public Guid EstablishmentId { get; set; }

        /// <summary>
        /// Id of the patient as provided by the recording establishment
        /// </summary>
        [JsonPropertyName("establishment.patientId")]
        public string EstablishmentProvidedId { get; set; }


        /// <summary>
        /// Initialise
        /// </summary>
        public Patient()
            : base()
        {
            PatientId = Guid.NewGuid();

            EstablishmentId = Guid.Empty;
            EstablishmentProvidedId = string.Empty;
        }

        /// <summary>
        /// Creates a clone of the provided patient instance as a new version
        /// </summary>
        /// <param name="instance">Instance to clone</param>
        private Patient(Patient instance)
            : base(instance)
        {
            PatientId = instance.PatientId;
            EstablishmentId = instance.EstablishmentId;
            EstablishmentProvidedId = instance.EstablishmentProvidedId;
        }

        /// <summary>
        /// Create a new version of the current patient record
        /// </summary>
        /// <returns>New instance of patient with an incremented version number</returns>
        Patient IHealthVersionableObject<Patient>.CreateVersion()
            => new Patient(this);
    }
}
