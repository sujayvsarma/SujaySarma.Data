using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using SujaySarma.Data.Health.Types;

namespace SujaySarma.Data.Health.SubsidiaryObjects
{
    /// <summary>
    /// Specifies the availability of someone or something on a given day, date or time
    /// </summary>
    public class CalendarAvailability
    {
        /// <summary>
        /// Overall days available
        /// </summary>
        [JsonPropertyName("availableDays")]
        public List<DayOfWeekTimeslot> AvailableDays { get; set; }

        /// <summary>
        /// Days on leaves of absence (time off)
        /// </summary>
        [JsonPropertyName("leavesOfAbsence")]
        public List<DateTimeRange> TimeOff { get; set; }

        /// <summary>
        /// Initialise
        /// </summary>
        public CalendarAvailability()
        {
            AvailableDays = new List<DayOfWeekTimeslot>();
            TimeOff = new List<DateTimeRange>();
        }

        /// <summary>
        /// Check if availability is indicated (regardless of appointments) on the 
        /// given date and time
        /// </summary>
        /// <param name="dateTime">Date/time in UTC to check for availability.</param>
        /// <returns>True if available</returns>
        public bool IsAvailableAt(DateTimeUtc dateTime)
        {
            bool dayCheckResult = false, holidayCheckResult = true;
            TimeOnly checkTime = dateTime;

            foreach (DayOfWeekTimeslot slot in AvailableDays)
            {
                if (slot.Weekday == dateTime.DayOfWeek)
                {
                    if (slot.From <= checkTime && slot.To >= checkTime)
                    {
                        dayCheckResult = true;
                        break;
                    }
                }
            }

            foreach (DateTimeRange dtr in TimeOff)
            {
                if (dtr.From <= dateTime && dtr.To >= dateTime)
                {
                    holidayCheckResult = false;
                    break;
                }
            }

            return dayCheckResult && holidayCheckResult;
        }

    }
}
