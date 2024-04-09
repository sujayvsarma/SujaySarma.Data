using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Constants
{
    /// <summary>
    /// Enumeration of the type of visit a patient may be making
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumPatientVisitTypes
    {
        /// <summary>
        /// A first visit or an introduction
        /// </summary>
        [Display(Name = "First visit", Description = "First visit or introductory visit")]
        FirstVisitOrIntroduction = 0,

        /// <summary>
        /// Follow up visit
        /// </summary>
        [Display(Name = "Follow up", Description = "A follow up on a previous visit instance")]
        FollowUp,

        /// <summary>
        /// Regular health check
        /// </summary>
        [Display(Name = "Regular checkup", Description = "A regular health checkup")]
        RegularCheckup,

        /// <summary>
        /// Provide a sample or get a test done
        /// </summary>
        [Display(Name = "Test or sample provision", Description = "A visit to provide a sample or get a test done")]
        TestOrSampleProvision,

        /// <summary>
        /// Report collection
        /// </summary>
        [Display(Name = "Report collection", Description = "Collect reports from a previous visit or test")]
        ReportCollection,

    }
}
