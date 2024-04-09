using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SujaySarma.Data.Health.Types
{
    /// <summary>
    /// The name of an establishment
    /// </summary>
    public class EstablishmentName
    {
        /// <summary>
        /// Name registered under, at the appropriate authorities
        /// </summary>
        [JsonPropertyName("registeredName")]
        public string? RegisteredName { get; set; }

        /// <summary>
        /// Name doing business under
        /// </summary>
        [JsonPropertyName("businessName")]
        public string? BusinessName { get; set; }

        /// <summary>
        /// Collection of avaialble abbreviations for the names
        /// </summary>
        [JsonPropertyName("abbreviations")]
        public List<string> AbbreviatedNames { get; }

        /// <summary>
        /// Returns a name that can be used for display purposes
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (! string.IsNullOrWhiteSpace(BusinessName))
                {
                    return BusinessName;
                }

                if (! string.IsNullOrWhiteSpace(RegisteredName))
                {
                    return RegisteredName;
                }

                foreach(string abbr in AbbreviatedNames)
                {
                    if (! string.IsNullOrWhiteSpace(abbr))
                    {
                        return abbr;
                    }
                }

                return string.Empty;
            }
        }


        /// <summary>
        /// Initialise
        /// </summary>
        public EstablishmentName()
        {
            RegisteredName = BusinessName = null;
            AbbreviatedNames = new List<string>();
        }

        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="name">Instance of establishment name to copy from</param>
        public EstablishmentName(EstablishmentName name)
        {
            this.RegisteredName = name.RegisteredName;
            this.BusinessName = name.BusinessName;

            this.AbbreviatedNames = new List<string>();
            this.AbbreviatedNames.AddRange(name.AbbreviatedNames);
        }
    }
}
