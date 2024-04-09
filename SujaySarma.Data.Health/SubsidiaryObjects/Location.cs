using System.Text.Json.Serialization;
using SujaySarma.Data.Health.BaseTypes;
using SujaySarma.Data.Health.Constants;

namespace SujaySarma.Data.Health.SubsidiaryObjects
{
    /// <summary>
    /// An addressable location
    /// </summary>
    public class Location : HealthObjectBase
    {

        /// <summary>
        /// Type of location
        /// </summary>
        [JsonPropertyName("type")]
        public EnumLocationTypes Type { get; set; }

        /// <summary>
        /// Details of the location
        /// </summary>
        [JsonPropertyName("detail")]
        public string Detail { get; set; }


        /// <summary>
        /// Initialise
        /// </summary>
        public Location()
            : base()
        {
            Type = EnumLocationTypes.PrivateOffice;
            Detail = string.Empty;
        }
    }
}
