using System.Text;
using System.Text.Json.Serialization;

using SujaySarma.Data.Health.Constants;

namespace SujaySarma.Data.Health.Types
{
    /// <summary>
    /// The name of a person
    /// </summary>
    public class PersonName
    {

        /// <summary>
        /// Salutation
        /// </summary>
        [JsonPropertyName("salutation")]
        public EnumSalutations Salutation { get; set; }

        /// <summary>
        /// Prefix
        /// </summary>
        [JsonPropertyName("prefix")]
        public string? Prefix { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Middle name
        /// </summary>
        [JsonPropertyName("middleName")]
        public string? MiddleName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        /// <summary>
        /// Suffix
        /// </summary>
        [JsonPropertyName("suffix")]
        public string? Suffix { get; set; }

        /// <summary>
        /// Full name by combining all the components into one string. Never NULL.
        /// </summary>
        public override string ToString()
        {
            StringBuilder fn = new StringBuilder();
            fn.Append(Salutation.ToString()).Append(' ');
            if (!string.IsNullOrWhiteSpace(Prefix))
            {
                fn.Append(Prefix).Append(' ');
            }
            if (!string.IsNullOrWhiteSpace(FirstName))
            {
                fn.Append(FirstName).Append(' ');
            }
            if (!string.IsNullOrWhiteSpace(MiddleName))
            {
                fn.Append(MiddleName).Append(' ');
            }
            if (!string.IsNullOrWhiteSpace(LastName))
            {
                fn.Append(LastName).Append(' ');
            }
            if (!string.IsNullOrWhiteSpace(Suffix))
            {
                fn.Append(Suffix);
            }

            return fn.ToString().Trim();
        }

        /// <summary>
        /// Initialise
        /// </summary>
        public PersonName()
        {
            Salutation = EnumSalutations.Mx;
            Prefix = FirstName = MiddleName = LastName = Suffix = null;
        }

        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="name">Instance to copy from</param>
        public PersonName(PersonName name)
        {
            this.Salutation = name.Salutation;
            this.Prefix = name.Prefix;
            this.FirstName = name.FirstName;
            this.MiddleName = name.MiddleName;
            this.LastName = name.LastName;
            this.Suffix = name.Suffix;
        }
    }
}
