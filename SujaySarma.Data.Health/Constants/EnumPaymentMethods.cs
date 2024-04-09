using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Constants
{
    /// <summary>
    /// Enumeration of the types of payment methods
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumPaymentMethods
    {
        /// <summary>
        /// Cash
        /// </summary>
        [Display(Name = "Cash", Description = "Cash")]
        Cash = 0,

        /// <summary>
        /// Cheque
        /// </summary>
        [Display(Name = "Cheque", Description = "Cheque")]
        Cheque,

        /// <summary>
        /// Wire transfer (NEFT, IMPS, etc)
        /// </summary>
        [Display(Name = "Wire or bank transfer (includes Debit cards)", Description = "Online wire transfer (NEFT, IMPS, etc), or using a debit card")]
        WireOrBankTransfer,

        /// <summary>
        /// Credit card
        /// </summary>
        [Display(Name = "Credit card", Description = "Credit card")]
        CreditCard,

        /// <summary>
        /// UPI
        /// </summary>
        [Display(Name = "UPI", Description = "Unified Payments Interface (UPI) using a supporting app or wallet")]
        UPI,

        /// <summary>
        /// Deferred payment instrument (insurance)
        /// </summary>
        [Display(Name = "Deferred (Insurance)", Description = "To be paid by insurer/underwriter at a later date")]
        Deferred,

        /// <summary>
        /// Payment waived
        /// </summary>
        [Display(Name = "Waived", Description = "Amount is cancelled or waived")]
        Waived,
    }
}
