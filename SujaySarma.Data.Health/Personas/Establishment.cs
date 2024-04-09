using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using SujaySarma.Data.Health.BaseTypes;
using SujaySarma.Data.Health.Constants;
using SujaySarma.Data.Health.Types;

namespace SujaySarma.Data.Health.Personas
{
    /// <summary>
    /// Defines an establishment of some nature
    /// </summary>
    public class Establishment : HealthVersionedObjectBase
    {

        /// <summary>
        /// Name of the establishment
        /// </summary>
        [JsonPropertyName("name")]
        public EstablishmentName Name { get; set; }

        /// <summary>
        /// Type of establishment
        /// </summary>
        [JsonPropertyName("type")]
        public EnumEstablishmentTypes Type { get; set; }

        /// <summary>
        /// Addresses
        /// </summary>
        [JsonPropertyName("addresses")]
        public List<PhysicalLocationAddress> Addresses { get; }


        /// <summary>
        /// Phone numbers
        /// </summary>
        [JsonPropertyName("phones")]
        public List<PhoneNumber> PhoneNumbers { get; }


        /// <summary>
        /// Initialise
        /// </summary>
        public Establishment()
            : base(HealthObjectVersionInfo.INITIAL_VERSION)
        {
            Name = new EstablishmentName();
            Type = EnumEstablishmentTypes.SingleDoctorPractice;
            Addresses = new List<PhysicalLocationAddress>();
            PhoneNumbers = new List<PhoneNumber>();
        }

        /// <summary>
        /// Creates a clone of the provided establishment instance as a new version
        /// </summary>
        /// <param name="instance">Instance to clone</param>
        private protected Establishment(Establishment instance)
            : base(instance.Version)
        {
            Name = new EstablishmentName(instance.Name);
            Type = instance.Type;

            Addresses = new List<PhysicalLocationAddress>();
            foreach (PhysicalLocationAddress address in instance.Addresses)
            {
                Addresses.Add(new PhysicalLocationAddress(address));
            }

            PhoneNumbers = new List<PhoneNumber>();
            foreach (PhoneNumber number in instance.PhoneNumbers)
            {
                PhoneNumbers.Add(new PhoneNumber(number));
            }
        }

        /// <summary>
        /// Create a new version of the current establishment record
        /// </summary>
        /// <returns>New instance of establishment with an incremented version number</returns>
        public Establishment CreateVersion()
            => new Establishment(this);

    }
}
