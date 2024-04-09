using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Constants
{
    /// <summary>
    /// Enumeration of salutations to people
    /// </summary>
    /// <remarks>
    ///     List does NOT include profession, regional and other specific salutations on purpose!
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumSalutations
    {
        /// <summary>
        /// Gender-neutral
        /// </summary>
        [Display(Name = "Mx", Description = "Gender-neutral (unidentified as either Mr/Ms)")]
        Mx = 0,

        /// <summary>
        /// Mister
        /// </summary>
        [Display(Name = "Mr", Description = "Adult male, regardless of marital status")]
        Mr,

        /// <summary>
        /// Master
        /// </summary>
        [Display(Name = "Mast", Description = "Junior unmarried male")]
        Mast,

        /// <summary>
        /// Mrs
        /// </summary>
        [Display(Name = "Mrs", Description = "Adult married female")]
        Mrs,

        /// <summary>
        /// Miss
        /// </summary>
        [Display(Name = "Miss", Description = "Junior unmarried female")]
        Miss,

        /// <summary>
        /// Ms
        /// </summary>
        [Display(Name = "Ms", Description = "Female regardless of age and marital status")]
        Ms
    }
}
