using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Types
{
    /// <summary>
    /// Provides a range of dates with time
    /// </summary>
    public class DateTimeRange
    {
        /// <summary>
        /// Start of the range
        /// </summary>
        [JsonPropertyName("from")]
        public DateTimeUtc From { get; set; }

        /// <summary>
        /// End of the slot
        /// </summary>
        [JsonPropertyName("to")]
        public DateTimeUtc To { get; set; }

        /// <summary>
        /// Initialise
        /// </summary>
        public DateTimeRange()
        {
            From = new DateTimeUtc();
            To = From;
        }
    }
}
