using System;
using System.Text.Json.Serialization;

using SujaySarma.Data.Health.Types;
using SujaySarma.Data.Health.Validation;

namespace SujaySarma.Data.Health.BaseTypes
{
    /// <summary>
    /// Implements <see cref="IHealthObject"/>
    /// </summary>
    public class HealthObjectBase : IHealthObject
    {

        /// <summary>
        /// Guid of the owning organisation. 
        /// A value of Guid.Empty implies "public" or "global".
        /// </summary>
        [JsonPropertyName("tenantId")]
        public Guid TenantId { get; set; }


        /// <summary>
        /// The unique identifier for this object
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Date/time of creation
        /// </summary>
        [JsonPropertyName("audit.created")]
        public DateTimeUtc Created { get; init; }


        /// <summary>
        /// Must initialise all properties and fields with meaningful defaults, ready for serialisation.
        /// </summary>
        public virtual void EnsureMeaningfulValues()
        {
            if (Id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }
        }

        /// <summary>
        /// Validate the object, throw exceptions on problems
        /// </summary>
        public virtual void ValidateWithExceptions()
        {
            if (Id == Guid.Empty)
            {
                throw new HealthObjectValidationException(HealthObjectValidationProblem.DefaultValue, nameof(Id));
            }
        }

        /// <summary>
        /// Initialise
        /// </summary>
        protected HealthObjectBase()
        {
            Id = Guid.Empty;
            TenantId = Guid.Empty;
            Created = new DateTimeUtc();
        }
    }
}
