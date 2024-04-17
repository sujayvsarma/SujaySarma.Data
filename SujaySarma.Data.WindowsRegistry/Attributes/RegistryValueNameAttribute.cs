using SujaySarma.Data.Core;

namespace SujaySarma.Data.WindowsRegistry.Attributes
{
    /// <summary>
    /// Configure the registry value (value under a Key) contained a .NET property or field.
    /// </summary>
    public class RegistryValueNameAttribute : ContainerMemberAttribute
    {

        /// <summary>
        /// Configure the registry value (value under a Key) contained a .NET property or field.
        /// </summary>
        /// <param name="name">Name of the value entry</param>
        public RegistryValueNameAttribute(string name)
            : base(name)
        {
        }
    }
}
