using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Constants
{
    /// <summary>
    /// Enumeration of the types of billing
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumBillingMethods
    {
        /// <summary>
        /// Directly billed to the patient
        /// </summary>
        [Display(Name = "Direct", Description = "Directly, to the patient")]
        Direct = 0,

        /// <summary>
        /// Billed to patient's personal insurance
        /// </summary>
        [Display(Name = "Insurance", Description = "Personal insurance of patient")]
        Insurance,

        /// <summary>
        /// Billed to a group insurance policy (corporate)
        /// </summary>
        [Display(Name = "Corporate insurance", Description = "Group or corporate insurance")]
        GroupCorporate,

    }
}
