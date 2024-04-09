using System.Text.Json.Serialization;

using SujaySarma.Data.Health.Constants;

namespace SujaySarma.Data.Health.Types
{
    /// <summary>
    /// A phone number
    /// </summary>
    public class PhoneNumber
    {
        /// <summary>
        /// Type of phone number
        /// </summary>
        [JsonPropertyName("type")]
        public EnumLocationTypes Type { get; set; }

        /// <summary>
        /// Is current phone number?
        /// </summary>
        [JsonPropertyName("current")]
        public bool IsCurrent { get; set; }

        /// <summary>
        /// International dialing prefix code
        /// </summary>
        [JsonPropertyName("isd")]
        public int InternationalPrefix 
        {
            get => _isd; 
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value), "International prefix cannot be less than zero!");
                }
                _isd = value;
            }
        }
        private int _isd = 0;

        /// <summary>
        /// The number
        /// </summary>
        [JsonPropertyName("number")]
        public int Number
        {
            get => _number;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value), "Phone number cannot be less than zero!");
                }
                _number = value;
            }
        }
        private int _number = 0;

        /// <summary>
        /// Initialise
        /// </summary>
        public PhoneNumber()
        {
            Type = EnumLocationTypes.Unknown;
            IsCurrent = false;
            InternationalPrefix = Number = 0;
        }

        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="number">Instance to copy from</param>
        public PhoneNumber(PhoneNumber number)
        {
            this.Type = number.Type;
            this.IsCurrent = number.IsCurrent;
            this.InternationalPrefix = number.InternationalPrefix;
            this.Number = number.Number;
        }

        /// <summary>
        /// Returns a formatted phone number string
        /// </summary>
        public override string ToString()
            => $"{((InternationalPrefix <= 0) ? "" : $"+{InternationalPrefix}")} {Number}";
    }
}
