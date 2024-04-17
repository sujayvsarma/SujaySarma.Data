using Microsoft.Win32;

using SujaySarma.Data.Core;

namespace SujaySarma.Data.WindowsRegistry.Attributes
{
    /// <summary>
    /// Configure the registry key the information contained in the .NET class, structure or record is stored into 
    /// or retrieved from.
    /// </summary>
    public class RegistryKeyNameAttribute : ContainerAttribute
    {

        /// <summary>
        /// The registry hive to use
        /// </summary>
        public RegistryHive Hive
        {
            get; init;
        }

        /// <summary>
        /// The root path of the key (between the <see cref="Hive"/> and the value of 'Name').
        /// </summary>
        public string ParentKeyPath
        {
            get; init;
        }

        /// <summary>
        /// Full path to the key that includes the <see cref="ParentKeyPath"/> and the Name but not the <see cref="Hive"/>.
        /// </summary>
        public string KeyPath
        {
            get => CreateQualifiedName();
        }

        /// <summary>
        /// Returns the reference to the registry key described by the properties. If it does not exist, 
        /// it is created. The key is opened in Writable mode.
        /// </summary>
        public RegistryKey Key
        {
            get
            {
                if (_regKey == null)
                {
                    string keypath = KeyPath;
                    RegistryKey baseKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Default);
                    _regKey = baseKey.OpenSubKey(keypath, writable: true)
                                ?? baseKey.CreateSubKey(keypath, writable: true);
                }

                return _regKey;
            }
        }
        private RegistryKey? _regKey = null;

        /// <summary>
        /// Returns the reference to the Parent Key of the key described by the properties. If it does not exist, 
        /// it is created. The key is opened in Writable mode.
        /// </summary>
        public RegistryKey ParentKey
        {
            get
            {
                if (_parentKey == null)
                {
                    RegistryKey baseKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Default);
                    _parentKey = baseKey.OpenSubKey(ParentKeyPath, writable: true)
                                ?? baseKey.CreateSubKey(ParentKeyPath, writable: true);
                }

                return _parentKey;
            }
        }
        private RegistryKey? _parentKey = null;

        /// <summary>
        /// Configure the registry key the information contained in the .NET class, structure or record is stored into 
        /// or retrieved from.
        /// </summary>
        /// <param name="name">Name of the key</param>
        public RegistryKeyNameAttribute(string name)
            : base(name)
        {
            Hive = RegistryHive.LocalMachine;
            ParentKeyPath = string.Empty;
        }

        /// <summary>
        /// Configure the registry key the information contained in the .NET class, structure or record is stored into 
        /// or retrieved from.
        /// </summary>
        /// <param name="name">Name of the key</param>
        /// <param name="hive">The registry hive to use</param>
        public RegistryKeyNameAttribute(string name, RegistryHive hive)
            : base(name)
        {
            Hive = hive;
            ParentKeyPath = string.Empty;
        }

        /// <summary>
        /// Configure the registry key the information contained in the .NET class, structure or record is stored into 
        /// or retrieved from.
        /// </summary>
        /// <param name="name">Name of the key</param>
        /// <param name="hive">The registry hive to use</param>
        /// <param name="parentKey">The root path of the key (between the <paramref name="hive"/> and <paramref name="name"/>).</param>
        public RegistryKeyNameAttribute(string name, RegistryHive hive, string parentKey)
            : base(name)
        {
            Hive = hive;
            ParentKeyPath = parentKey;
        }


        /// <inheritdoc />
        public override string CreateQualifiedName()
        {
            return $"{ParentKeyPath}\\{Name}";
        }
    }
}
