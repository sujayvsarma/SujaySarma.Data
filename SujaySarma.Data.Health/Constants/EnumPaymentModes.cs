using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Constants
{
    /// <summary>
    /// Enumeration of payment modes
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumPaymentModes
    {
        /// <summary>
        /// Single payment
        /// </summary>
        [Display(Name = "Single", Description = "Full payment as a single transaction")]
        Single = 0,

        /// <summary>
        /// Split by method
        /// </summary>
        [Display(Name = "Split by payment methods", Description = "Payment split among multiple instruments or methods")]
        SplitByMethod,

        /// <summary>
        /// Installments
        /// </summary>
        [Display(Name = "Installments", Description = "Paid over multiple installments")]
        Installments,

    }
}
