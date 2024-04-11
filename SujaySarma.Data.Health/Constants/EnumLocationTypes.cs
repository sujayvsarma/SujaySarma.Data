using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Constants
{
    /// <summary>
    /// Enumeration of types of locations
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumLocationTypes
    {
        /// <summary>
        /// The value is not known or set at this time
        /// </summary>
        [Display(Name = "Unknown", Description = "Not known at this time")]
        Unknown,

        /// <summary>
        /// Someone's private office space
        /// </summary>
        [Display(Name = "Office", Description = "Someone's private office space")]
        PrivateOffice,

        /// <summary>
        /// Conference room
        /// </summary>
        [Display(Name = "Conference room", Description = "A conference or meeting room")]
        ConferenceRoom,

        /// <summary>
        /// Public location such as a cafe or restaurant or the park
        /// </summary>
        [Display(Name = "Public location", Description = "Such as a cafe, restaurant or park")]
        PublicLocation,

        /// <summary>
        /// Telephone call
        /// </summary>
        [Display(Name = "Telephone call", Description = "Meeting will happen over a regular telephone or mobile call")]
        Telephone,

        /// <summary>
        /// Online meeting
        /// </summary>
        [Display(Name = "Online meeting", Description = "An online meeting using a pre-agreed app, website or service")]
        OnlineMeetingPlace
    }
}
