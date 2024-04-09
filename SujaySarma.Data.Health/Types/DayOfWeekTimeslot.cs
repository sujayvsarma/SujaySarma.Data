using System;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Types
{
    /// <summary>
    /// Maps together a <see cref="DayOfWeek"/> and a range of times.
    /// </summary>
    public class DayOfWeekTimeslot
    {
        /// <summary>
        /// Day of week
        /// </summary>
        [JsonPropertyName("weekday")]
        public DayOfWeek Weekday { get; set; }

        /// <summary>
        /// Start time of the slot
        /// </summary>
        [JsonPropertyName("startTime")]
        public TimeOnly From { get; set; }

        /// <summary>
        /// End time of the slot
        /// </summary>
        [JsonPropertyName("to")]
        public TimeOnly To { get; set; }

        /// <summary>
        /// Initialise
        /// </summary>
        public DayOfWeekTimeslot()
        {
            Weekday = DayOfWeek.Sunday;
            From = new TimeOnly(0, 0, 0);
            To = new TimeOnly(23, 59, 59);
        }
    }
}
