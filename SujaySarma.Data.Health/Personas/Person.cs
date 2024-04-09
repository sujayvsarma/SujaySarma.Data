using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using SujaySarma.Data.Health.BaseTypes;
using SujaySarma.Data.Health.Constants;
using SujaySarma.Data.Health.Types;

namespace SujaySarma.Data.Health.Personas
{
    /// <summary>
    /// A general person. 
    /// Also serves as the base class for other personas in this namespace.
    /// </summary>
    public class Person : HealthVersionedObjectBase, IHealthVersionableObject<Person>
    {

        /// <summary>
        /// Name of the person
        /// </summary>
        [JsonPropertyName("name")]
        public PersonName Name { get; set; }

        /// <summary>
        /// Date of birth
        /// </summary>
        [JsonPropertyName("dateOfBirth")]
        public DateTimeUtc DateOfBirth { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        [JsonPropertyName("gender")]
        public EnumGenders Gender { get; set; }

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
        /// Social security number
        /// </summary>
        [JsonPropertyName("ssn")]
        public SecuredMetadata SocialSecurityNumber { get; set; }


        /// <summary>
        /// Age in years
        /// </summary>
        public int Age => (int)((DateTime.UtcNow - DateOfBirth).TotalDays / 365);


        /// <summary>
        /// Initialise
        /// </summary>
        public Person()
            : base(HealthObjectVersionInfo.INITIAL_VERSION)
        {
            Name = new PersonName();
            DateOfBirth = new DateTimeUtc();
            Gender = EnumGenders.PreferNotToSay;

            Addresses = new List<PhysicalLocationAddress>();
            PhoneNumbers = new List<PhoneNumber>();

            SocialSecurityNumber = new SecuredMetadata();
        }

        /// <summary>
        /// Creates a clone of the provided person instance as a new version
        /// </summary>
        /// <param name="instance">Instance to clone</param>
        private protected Person(Person instance)
            : base(instance.Version)
        {
            Name = new PersonName(instance.Name);
            DateOfBirth = instance.DateOfBirth;
            Gender = instance.Gender;

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

            SocialSecurityNumber = new SecuredMetadata(instance.SocialSecurityNumber);
        }

        /// <summary>
        /// Create a new version of the current person record
        /// </summary>
        /// <returns>New instance of person with an incremented version number</returns>
        public Person CreateVersion()
            => new Person(this);
    }
}
