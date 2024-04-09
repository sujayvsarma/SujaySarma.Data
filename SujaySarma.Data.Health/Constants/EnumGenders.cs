using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Constants
{
    /// <summary>
    /// Enumeration of possible genders
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumGenders
    {
        /// <summary>
        /// Male
        /// </summary>
        [Display(Name = "Male", Description = "Male or man")]
        Male,

        /// <summary>
        /// Female
        /// </summary>
        [Display(Name = "Female", Description = "Female or woman")]
        Female,

        /// <summary>
        /// Non-binary
        /// </summary>
        [Display(Name = "Non-binary", Description = "Identity falling outside the categorisation of 'male' and 'female'")]
        NonBinary,

        /// <summary>
        /// Gender-queer
        /// </summary>
        [Display(Name = "Gender-queer", Description = "Challenges gender-norms, does not fit 'male' or 'female'")]
        GenderQueer,

        /// <summary>
        /// Agender
        /// </summary>
        [Display(Name = "Agender", Description = "No gender identity")]
        Agender,

        /// <summary>
        /// Gender-fluid
        /// </summary>
        [Display(Name = "Gender-fluid", Description = "Moves across genders over time")]
        GenderFluid,

        /// <summary>
        /// Transgender
        /// </summary>
        [Display(Name = "Transgender", Description = "Gender identity differs from one assigned at birth")]
        Transgender,

        /// <summary>
        /// Prefer not to say
        /// </summary>
        [Display(Name = "Prefer not to say", Description = "Undisclosed")]
        PreferNotToSay
    }
}
