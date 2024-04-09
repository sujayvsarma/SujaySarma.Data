using System;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Validation
{
    /// <summary>
    /// Exception thrown by health objects on validation failures
    /// </summary>
    public class HealthObjectValidationException : Exception
    {

        /// <summary>
        /// Type of problem
        /// </summary>
        [JsonPropertyName("problem"), JsonConverter(typeof(JsonStringEnumConverter))]
        public HealthObjectValidationProblem Problem { get; init; }

        /// <summary>
        /// Name of property or field with the problem
        /// </summary>
        [JsonPropertyName("member")]
        public string MemberName { get; init; }

        /// <summary>
        /// Exception thrown by health objects on validation failures
        /// </summary>
        /// <param name="problem">Type of problem</param>
        /// <param name="memberName">Name of property or field with the problem</param>
        public HealthObjectValidationException(HealthObjectValidationProblem problem, string memberName)
            : base($"Member '{memberName}' does not contain a valid value.")
        {
            Problem = problem;
            MemberName = memberName;
        }
    }
}
