using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using SujaySarma.Data.Health.BaseTypes;

namespace SujaySarma.Data.Health.SubsidiaryObjects
{
    /// <summary>
    /// Notes from a scheduled appointment/meeting
    /// </summary>
    public class ScheduledAppointmentNotes : HealthObjectBase
    {
        /// <summary>
        /// The appointment this is for
        /// </summary>
        [JsonPropertyName("appointment")]
        public Guid Appointment { get; set; }

        /// <summary>
        /// Owner of this string
        /// </summary>
        [JsonPropertyName("owner")]
        public Guid Owner { get; set; }

        /// <summary>
        /// Lines of string
        /// </summary>
        [JsonPropertyName("strings")]
        public List<string> LinesOfString { get; set; }

        /// <summary>
        /// Initialise
        /// </summary>
        public ScheduledAppointmentNotes()
            : base()
        {
            Appointment = Guid.Empty;
            Owner = Guid.Empty;
            LinesOfString = new List<string>();
        }
    }
}
