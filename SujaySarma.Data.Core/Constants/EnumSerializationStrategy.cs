

namespace SujaySarma.Data.Core.Constants
{
    /// <summary>
    /// When the value of the property or field is an 'Enum' type, this value controls
    /// how that column is serialised.
    /// </summary>
    public enum EnumSerializationStrategy : byte
    {
        /// <summary>
        /// As its integer-equivalent value
        /// </summary>
        AsInt = 0,
        
        /// <summary>
        /// As its string value, calling the ToString() method on the property or field.
        /// </summary>
        AsString = 1,
        
        /// <summary>
        /// When used with type-defined backend systems, will attempt to store the value
        /// according to the underlying field's data type. This may result in costlier I/O as it
        /// involves an extra call for type-retrieval and is not supported on type-free systems
        /// (eg: flat files).
        /// </summary>
        MatchUnderlyingMember = 2
    }
}
