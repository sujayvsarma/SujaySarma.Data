using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using SujaySarma.Data.Health.BaseTypes;
using SujaySarma.Data.Health.Types;

namespace SujaySarma.Data.Health.SubsidiaryObjects
{
    /// <summary>
    /// A scheduled appointment
    /// </summary>
    public class ScheduledAppointment : HealthObjectBase
    {

        /// <summary>
        /// The range of time this appointment covers
        /// </summary>
        [JsonPropertyName("timeSlot")]
        public DateTimeRange Timeslot { get; set; }

        /// <summary>
        /// Organiser of the appointment
        /// </summary>
        [JsonPropertyName("organiser")]
        public Guid Organiser { get; set; }

        /// <summary>
        /// List of people invited to the appointment
        /// </summary>
        [JsonPropertyName("inviteList")]
        public List<Guid> InviteList { get; set; }

        /// <summary>
        /// Meeting location
        /// </summary>
        [JsonPropertyName("location")]
        public Guid Location { get; set; }

        /// <summary>
        /// Appointment name or title - What is this appointment about?
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Body content text/html/xml of the invitation to the appointment
        /// </summary>
        [JsonPropertyName("inviteBody")]
        public string InviteContent { get; set; }


        /// <summary>
        /// Initialise
        /// </summary>
        public ScheduledAppointment()
            : base()
        {
            Timeslot = new DateTimeRange();
            Timeslot.To = Timeslot.From.AddHours(1);

            Organiser = Location = Guid.Empty;
            InviteList = new List<Guid>();

            Title = InviteContent = string.Empty;
        }

    }
}
