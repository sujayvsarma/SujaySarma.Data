using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Validation
{
    /// <summary>
    /// The problem detected on validation of health object data
    /// </summary>
    [Flags]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HealthObjectValidationProblem : uint
    {
        /// <summary>
        /// Property or field has default values.
        /// </summary>
        [Display(Name = "Default value", Description = "Property or field has default values.")]
        DefaultValue = 0,

        /// <summary>
        /// When values are to be set in a combination, another property or field that is a 
        /// part of this combination is not set (or not set correctly).
        /// </summary>
        [Display(Name = "Dependency value not set", Description = "Another property or field that is a part of this combination is not set (or not set correctly).")]
        DepdendencyValueNotSet = 1,

        /// <summary>
        /// For enum values, the value set is not a member of the enum type.
        /// </summary>
        [Display(Name = "Invalid flag", Description = "Value is not a member of the enum type.")]
        InvalidFlag = 2,
    }
}
