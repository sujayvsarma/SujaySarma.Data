using System.Text;
using System.Text.Json.Serialization;

using SujaySarma.Data.Health.Constants;

namespace SujaySarma.Data.Health.Types
{
    /// <summary>
    /// Address of a location
    /// </summary>
    public class PhysicalLocationAddress
    {

        /// <summary>
        /// Type of address
        /// </summary>
        [JsonPropertyName("type")]
        public EnumLocationTypes Type { get; set; }

        /// <summary>
        /// Is current address?
        /// </summary>
        [JsonPropertyName("current")]
        public bool IsCurrent { get; set; }


        /// <summary>
        /// Street address
        /// </summary>
        [JsonPropertyName("streetAddress")]
        public string? StreetAddress { get; set; }

        /// <summary>
        /// City, town, etc name
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; }

        /// <summary>
        /// District or province
        /// </summary>
        [JsonPropertyName("district")]
        public string? District { get; set; }

        /// <summary>
        /// State
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; }

        /// <summary>
        /// Postal or ZIP code
        /// </summary>
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }


        /// <summary>
        /// Initialise
        /// </summary>
        public PhysicalLocationAddress()
        {
            Type = EnumLocationTypes.Unknown;
            IsCurrent = false;

            StreetAddress = District = null;
            City = State = Country = PostalCode = string.Empty;
        }

        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="address">Instance to copy from</param>
        public PhysicalLocationAddress(PhysicalLocationAddress address)
        {
            this.Type = address.Type;
            this.IsCurrent = address.IsCurrent;
            this.StreetAddress = address.StreetAddress;
            this.District = address.District;
            this.City = address.City;
            this.State = address.State;
            this.Country = address.Country;
            this.PostalCode = address.PostalCode;
        }

        /// <summary>
        /// Concatenate the components of the address
        /// </summary>
        /// <param name="breakWith">Each component is seperated with this string sequence. Default is ", "</param>
        /// <returns>Concatenated address</returns>
        public string ToString(string breakWith = ", ")
        {
            StringBuilder ad = new StringBuilder();
            if (! string.IsNullOrWhiteSpace(StreetAddress))
            {
                ad.Append(StreetAddress).Append(breakWith);
            }
            if (!string.IsNullOrWhiteSpace(City))
            {
                ad.Append(City);
                if (! string.IsNullOrWhiteSpace(PostalCode))
                {
                    ad.Append(" - ").Append(PostalCode);
                }
                ad.Append(breakWith);
            }
            if (! string.IsNullOrWhiteSpace(District))
            {
                ad.Append(District).Append(breakWith);
            }
            if (!string.IsNullOrWhiteSpace(State))
            {
                ad.Append(State).Append(breakWith);
            }
            if (!string.IsNullOrWhiteSpace(Country))
            {
                ad.Append(Country).Append(breakWith);
            }

            if (string.IsNullOrWhiteSpace(City) && (! string.IsNullOrWhiteSpace(PostalCode)))
            {
                ad.Append("ZIP/PIN: ").Append(PostalCode).Append(breakWith);
            }

            return ad.ToString();
        }
    }
}
