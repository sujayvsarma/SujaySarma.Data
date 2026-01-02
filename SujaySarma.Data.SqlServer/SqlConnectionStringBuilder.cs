using SujaySarma.Data.Core.ReflectionUtilities;

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Helps build valid connection strings to SQL Server databases.
/// </summary>
public sealed class SqlConnectionStringBuilder
{
    /// <summary>
    /// Finalise and build the connection string.
    /// </summary>
    /// <returns>String containing the built connection string.</returns>
    public string Build()
    {
        StringBuilder hostBuilder = new StringBuilder();
        switch (_options.Protocol)
        {
            case Protocol.Tcp:
                {
                    hostBuilder.Append("Server=")
                        .Append(_options.HostNameOrIPAddress);

                    if (!_options.InstanceName.Equals("MSSQLSERVER", StringComparison.InvariantCultureIgnoreCase))
                    {
                        hostBuilder.Append('\\')
                            .Append(_options.InstanceName);
                    }

                    if (_options.PortNumber != 1433)
                    {
                        hostBuilder.Append(',')
                            .Append(_options.PortNumber);
                    }
                }
                break;

            case Protocol.NamedPipes:
                {
                    // Format: "np:\\\\{serverAddress}\\pipe\\MSSQL${instanceName}\\sql\\query";
                    hostBuilder.Append("Server=np:\\\\")
                        .Append(_options.HostNameOrIPAddress)
                        .Append("\\pipe");

                    if (!_options.InstanceName.Equals("MSSQLSERVER", StringComparison.InvariantCultureIgnoreCase))
                    {
                        hostBuilder.Append('\\').Append("MSSQL$")
                            .Append(_options.InstanceName);
                    }

                    hostBuilder.Append("\\sql\\query");
                }
                break;
        }

        StringBuilder builder = new StringBuilder();
        builder.Append(hostBuilder)
            .Append(';');

        builder.Append("Initial Catalog=\"")
            .Append(_options.DatabaseName)
            .Append("\";");

        builder.Append("Connect Timeout=")
            .Append(_options.ConnectionTimeout)
            .Append(';');

        switch (_options.Authentication)
        {
            case AuthenticationMode.IntegratedAuth:
                {
                    builder.Append("Integrated Security=true;");
                }
                break;

            // Our builders enforce that when set to Credentials, Credential will always have a value!
            // This check is to satisfy Roslyn's NULL check warning.
            case AuthenticationMode.Credentials when _options.AuthenticationCredential.HasValue:
                {
                    builder.Append("User ID=\"")
                        .Append(_options.AuthenticationCredential.Value.UserName)
                        .Append("\";Password=\"")
                        .Append(_options.AuthenticationCredential.Value.Password)
                        .Append("\";");
                }
                break;
        }

        foreach (ConnectionFlags flag in Enum.GetValues(typeof(ConnectionFlags)))
        {
            // Test for power of 2 flags (only valid ones) and do the loop 
            // only if we are power of 2.
            if (! flag.IsSingleBitFlag())
            {
                continue;
            }

            if (_options.ExplicitlyEnabledFlags.HasFlag(flag))
            {
                builder.Append(GetFlagName(flag)).Append('=').Append("True;");
            }
            else if (_options.ExplicitlyDisabledFlags.HasFlag(flag))
            {
                builder.Append(GetFlagName(flag)).Append('=').Append("False;");
            }
        }

        return builder.ToString();

        // Get the connection string name (string) corresponding to 
        // the provided ConnectionFlags value.
        static string GetFlagName(ConnectionFlags flag)
        {
            switch (flag)
            {
                case ConnectionFlags.PersistSecurityInfo:
                    return "Persist Security Info";

                case ConnectionFlags.ConnectionPooling:
                    return "Pooling";

                case ConnectionFlags.MultipleResultSets:
                    return "MultipleActiveResultSets";

                case ConnectionFlags.EncryptCommunications:
                    return "Encrypt";

                case ConnectionFlags.TrustServerCertificate:
                    return "TrustServerCertificate";

                default:
                    throw new ArgumentException($"The flag value '{(int)flag}' is not recognised.", nameof(flag));
            }
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Build();
    }


    #region Flags

    /// <summary>
    /// Disables all connection option flags.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder DisableAllFlags()
    {
        _options.ExplicitlyEnabledFlags = ConnectionFlags.None;
        _options.ExplicitlyDisabledFlags = ConnectionFlags.PersistSecurityInfo | ConnectionFlags.ConnectionPooling
            | ConnectionFlags.MultipleResultSets | ConnectionFlags.EncryptCommunications | ConnectionFlags.TrustServerCertificate;

        return this;
    }

    /// <summary>
    /// Enables all connection option flags.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder EnableAllFlags()
    {
        _options.ExplicitlyDisabledFlags = ConnectionFlags.None;
        _options.ExplicitlyEnabledFlags = ConnectionFlags.PersistSecurityInfo | ConnectionFlags.ConnectionPooling
            | ConnectionFlags.MultipleResultSets | ConnectionFlags.EncryptCommunications | ConnectionFlags.TrustServerCertificate;

        return this;
    }

    /// <summary>
    /// Enables the 'Persist Security Info' flag. This causes passwords to be persisted in connection string 
    /// after connection is made. Otherwise, sensitive information is removed after connection.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder EnablePersistSecurityInfo()
    {
        EnableFlag(ConnectionFlags.PersistSecurityInfo);
        return this;
    }

    /// <summary>
    /// Disables the 'Persist Security Info' flag. This causes passwords to be removed from the connection string 
    /// after connection is made. Otherwise, sensitive information remains after connection.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder DisablePersistSecurityInfo()
    {
        DisableFlag(ConnectionFlags.PersistSecurityInfo);
        return this;
    }

    /// <summary>
    /// Enables using Connection Pooling.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder EnableConnectionPooling()
    {
        EnableFlag(ConnectionFlags.ConnectionPooling);
        return this;
    }

    /// <summary>
    /// Disables use of Connection Pooling
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder DisableConnectionPooling()
    {
        DisableFlag(ConnectionFlags.ConnectionPooling);
        return this;
    }

    /// <summary>
    /// Enables Multiple Active Results. This allows query returns to have multiple table results. If not 
    /// set, only the first data table would be available to consuming applications.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder EnableMultipleActiveResults()
    {
        EnableFlag(ConnectionFlags.MultipleResultSets);
        return this;
    }

    /// <summary>
    /// Disables Multiple Active Results. This prevents query returns to have multiple table results. 
    /// Only the first data table would be available to consuming applications.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder DisableMultipleActiveResults()
    {
        DisableFlag(ConnectionFlags.MultipleResultSets);
        return this;
    }

    /// <summary>
    /// Allow communications between SQL Server and ourselves to be encrypted.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder EnableEncryption()
    {
        EnableFlag(ConnectionFlags.EncryptCommunications);
        return this;
    }

    /// <summary>
    /// Disallows communications between SQL Server and ourselves to be encrypted.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder DisableEncryption()
    {
        DisableFlag(ConnectionFlags.EncryptCommunications);
        return this;
    }

    /// <summary>
    /// Trust any/all certificates presented by the remote SQL Server 
    /// as valid and use it for encrypted communications.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder EnableTrustServerCertificate()
    {
        EnableFlag(ConnectionFlags.TrustServerCertificate);
        return this;
    }

    /// <summary>
    /// Do not trust any/all certificates presented by the remote SQL Server 
    /// as valid and use it for encrypted communications. They will be validated.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder DisableTrustServerCertificate()
    {
        DisableFlag(ConnectionFlags.TrustServerCertificate);
        return this;
    }


    /// <summary>
    /// Helper function to enable a flag (and remove it from the Disabled list).
    /// </summary>
    /// <param name="flag">The flag to enable.</param>
    private void EnableFlag(ConnectionFlags flag)
    {
        if (!_options.ExplicitlyEnabledFlags.HasFlag(flag))
        {
            _options.ExplicitlyEnabledFlags |= flag;
        }

        if (_options.ExplicitlyDisabledFlags.HasFlag(flag))
        {
            _options.ExplicitlyDisabledFlags &= ~flag;
        }
    }

    /// <summary>
    /// Helper function to disable a flag (and remove it from the Enabled list).
    /// </summary>
    /// <param name="flag">The flag to disable.</param>
    private void DisableFlag(ConnectionFlags flag)
    {
        if (!_options.ExplicitlyDisabledFlags.HasFlag(flag))
        {
            _options.ExplicitlyDisabledFlags |= flag;
        }

        if (_options.ExplicitlyEnabledFlags.HasFlag(flag))
        {
            _options.ExplicitlyEnabledFlags &= ~flag;
        }
    }

    #endregion

    #region All other configuration

    /// <summary>
    /// By default, the builder uses the "tempdb" database. Set the correct database using 
    /// this method.
    /// </summary>
    /// <param name="database">Name of the database to connect to.</param>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder UsingDatabase(string database)
    {
        if (string.IsNullOrWhiteSpace(database))
        {
            throw new ArgumentNullException("Name of the database must be a non-empty string.");
        }

        _options.DatabaseName = database;
        return this;
    }

    /// <summary>
    /// By default, the builder uses Integrated Authentication mode. Use this method to change it 
    /// to use username/password credentials instead.
    /// </summary>
    /// <param name="userName">Login username.</param>
    /// <param name="password">Login password.</param>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder UsingCredential(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException("Username and password must be non-empty strings.");
        }

        _options.Authentication = AuthenticationMode.Credentials;
        _options.AuthenticationCredential = (UserName: userName, Password: password);
        return this;
    }

    /// <summary>
    /// By default we use the default instance of SQL Server (named: "MSSQLSERVER"). If the target 
    /// SQL Server uses a different non-default instance name, set it here.
    /// </summary>
    /// <param name="instanceName">Name of the instance.</param>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder UsingNamedInstance(string instanceName)
    {
        if (string.IsNullOrWhiteSpace(instanceName) || (instanceName.Length < 1) || (instanceName.Length > 16))
        {
            throw new ArgumentException("Name of an instance cannot be NULL or empty. Length must be 1-16 characters.");
        }

        instanceName = instanceName.ToUpperInvariant();
        if (instanceName.Contains("DEFAULT") || instanceName.Equals("MSSQLSERVER"))
        {
            return this;
        }

        // Regex to validate the character set AND starting character:
        // ^[\p{L}]               -> Must start with a Unicode letter (\p{L}).
        // ([\p{L}\p{N}\$]|_)*$   -> Subsequent characters can be Unicode letter, Unicode number (\p{N}), dollar sign ($), or underscore (_).
        // This implicitly excludes spaces, hyphens, and other special characters like \ / , : ; ' & @.
        Regex allowedCharsRegex = new Regex(@"^[\p{L}]([\p{L}\p{N}\$]|_)*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        if (!allowedCharsRegex.IsMatch(instanceName))
        {
            throw new ArgumentException("Name of an instance must follow the rules outlined at: https://learn.microsoft.com/en-us/sql/sql-server/install/instance-configuration");
        }

        _options.InstanceName = instanceName;
        return this;
    }

    /// <summary>
    /// Set the protocol as Named Pipes. Default protocol is TCP/IP, use this method to change the 
    /// protocol to Named Pipes if required.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder UsingNamedPipes()
    {
        _options.Protocol = Protocol.NamedPipes;
        return this;
    }

    /// <summary>
    /// Set the port the remote SQL Server is listening on. Default set at initialisation is 1433, 
    /// configure a new port number if SQL Server is listening on a custom port number.
    /// </summary>
    /// <param name="port">The port number.</param>
    /// <returns>Instance of self.</returns>
    public SqlConnectionStringBuilder UsingPort(ushort port)
    {
        _options.PortNumber = port;
        return this;
    }

    #endregion

    /// <summary>
    /// Initiate the builder sequence using the server's IP address or hostname.
    /// </summary>
    /// <param name="serverNameOrAddress">SQL Server's hostname or IP address. Does not need to be findable immediately, but needs to be 
    /// in the appropriate format (for names/IP address).</param>
    /// <returns>Instance of an initialised builder.</returns>
    public static SqlConnectionStringBuilder UsingServerAddress(string serverNameOrAddress)
    {
        Regex HostnameRegex = new Regex(@"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9-]*[A-Za-z0-9])$",
                            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        List<string> LOCALHOST = new List<string>() { "(local)", "localhost", "." };

        if (IPAddress.TryParse(serverNameOrAddress, out IPAddress? ip))
        {
            if (ip.AddressFamily.HasFlag(System.Net.Sockets.AddressFamily.InterNetworkV6))
            {
                serverNameOrAddress = $"[{ip}]";
            }
        }
        else
        {
            if ((!LOCALHOST.Contains(serverNameOrAddress)) && ((serverNameOrAddress.Length > 253) || (!HostnameRegex.IsMatch(serverNameOrAddress))))
            {
                throw new ArgumentException("Server address must be a valid IP address or hostname.", nameof(serverNameOrAddress));
            }
        }

        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        builder._options.HostNameOrIPAddress = serverNameOrAddress;
        return builder;
    }


    /// <summary>
    /// Private initialiser. Consumers must use the builder pattern functions 
    /// to set up the options.
    /// </summary>
    private SqlConnectionStringBuilder()
    {
        _options = new ConnectionOptions();
    }

    private ConnectionOptions _options;

    /// <summary>
    /// Configuration options for the connection string. 
    /// Each builder method sets one or more properties of this struct.
    /// </summary>
    struct ConnectionOptions
    {
        /// <summary>
        /// The server's hostname or IP address.
        /// </summary>
        public string HostNameOrIPAddress;

        /// <summary>
        /// Port number.
        /// </summary>
        public ushort PortNumber;

        /// <summary>
        /// The protocol to use.
        /// </summary>
        public Protocol Protocol;

        /// <summary>
        /// Mode of authentication.
        /// </summary>
        public AuthenticationMode Authentication;

        /// <summary>
        /// Username/password pair for authentication.
        /// </summary>
        public (string UserName, string Password)? AuthenticationCredential;

        /// <summary>
        /// Name of the SQL Server instance.
        /// </summary>
        public string InstanceName;

        /// <summary>
        /// Name of the database to connect to.
        /// </summary>
        public string DatabaseName;

        /// <summary>
        /// Connection timeout in seconds.
        /// </summary>
        public ushort ConnectionTimeout;

        /// <summary>
        /// Explicitly enabled/configured flags.
        /// </summary>
        public ConnectionFlags ExplicitlyEnabledFlags;

        /// <summary>
        /// Explicitly disabled/de-configured flags.
        /// </summary>
        public ConnectionFlags ExplicitlyDisabledFlags;

        /// <summary>
        /// Initialise with defaults.
        /// </summary>
        public ConnectionOptions()
        {
            HostNameOrIPAddress = "(local)";
            PortNumber = 1433;
            Protocol = Protocol.Tcp;
            Authentication = AuthenticationMode.IntegratedAuth;
            AuthenticationCredential = null;
            InstanceName = "MSSQLSERVER";
            DatabaseName = "tempdb";        // Don't use "master" as the default.
            ConnectionTimeout = 30;         // seconds

            ExplicitlyEnabledFlags = ConnectionFlags.ConnectionPooling | ConnectionFlags.MultipleResultSets
                | ConnectionFlags.EncryptCommunications | ConnectionFlags.TrustServerCertificate;

            ExplicitlyDisabledFlags = ConnectionFlags.None;
        }
    }

    /// <summary>
    /// The protocol to use.
    /// </summary>
    public enum Protocol
    {
        /// <summary>
        /// TCP/IP connection.
        /// </summary>
        Tcp = 0,

        /// <summary>
        /// Named pipes connection.
        /// </summary>
        NamedPipes
    }

    /// <summary>
    /// Authentication mode.
    /// </summary>
    public enum AuthenticationMode
    {
        /// <summary>
        /// Windows integrated authentication or SSPI.
        /// </summary>
        IntegratedAuth = 0,

        /// <summary>
        /// Username/password credentials.
        /// </summary>
        Credentials
    }

    /// <summary>
    /// Various connection properties that can be stored/used 
    /// as flag values.
    /// </summary>
    [Flags]
    public enum ConnectionFlags
    {
        /// <summary>
        /// No flags are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Persist passwords in connection string after connection is made.
        /// Otherwise, sensitive information is removed after connection.
        /// </summary>
        PersistSecurityInfo = 1,

        /// <summary>
        /// Use connection pooling.
        /// </summary>
        ConnectionPooling = 2,

        /// <summary>
        /// Allow query returns to have multiple table results. If not 
        /// set, only the first data table would be available to consuming applications.
        /// </summary>
        MultipleResultSets = 4,

        /// <summary>
        /// Allow communications between SQL Server and ourselves to be 
        /// encrypted.
        /// </summary>
        EncryptCommunications = 8,

        /// <summary>
        /// Trust any/all certificates presented by the remote SQL Server 
        /// as valid and use it for encrypted communications.
        /// </summary>
        TrustServerCertificate = 16
    }
}
