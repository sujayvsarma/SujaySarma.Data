using System;

using SujaySarma.Data.Health.Types;

namespace SujaySarma.Data.Health.BaseTypes
{
    /// <summary>
    /// Defines an object as a health-system object
    /// </summary>
    public interface IHealthObject
    {
        /// <summary>
        /// Guid of the owning organisation. 
        /// A value of Guid.Empty implies "public" or "global".
        /// </summary>
        Guid TenantId { get; set; }

        /// <summary>
        /// The unique identifier for this object
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Date/time of creation
        /// </summary>
        DateTimeUtc Created { get; init; }

        /// <summary>
        /// Must initialise all properties and fields with meaningful defaults, ready for serialisation.
        /// </summary>
        void EnsureMeaningfulValues();

        /// <summary>
        /// Validate the object, throw exceptions on problems
        /// </summary>
        void ValidateWithExceptions();
    }
}
