using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SujaySarma.Data.SqlServer.Tests;

[TestClass]
public class SqlConnectionStringBuilderTests
{
    #region Valid Connection String Tests

    [TestMethod]
    public void Build_DefaultSettings_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=localhost");
        StringAssert.Contains(connectionString, "Initial Catalog=\"tempdb\"");
        StringAssert.Contains(connectionString, "Connect Timeout=30");
        StringAssert.Contains(connectionString, "Integrated Security=true");
        StringAssert.Contains(connectionString, "Pooling=True");
        StringAssert.Contains(connectionString, "MultipleActiveResultSets=True");
        StringAssert.Contains(connectionString, "Encrypt=True");
        StringAssert.Contains(connectionString, "TrustServerCertificate=True");
    }

    [TestMethod]
    public void Build_WithIPv4Address_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("192.168.1.100")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=192.168.1.100");
    }

    [TestMethod]
    public void Build_WithIPv6Address_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("::1")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=[::1]");
    }

    [TestMethod]
    public void Build_WithFullIPv6Address_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("2001:0db8:85a3:0000:0000:8a2e:0370:7334")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=[2001:db8:85a3::8a2e:370:7334]");
    }

    [TestMethod]
    public void Build_WithCustomPort_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingPort(1435)
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=localhost,1435");
    }

    [TestMethod]
    public void Build_WithDefaultPort_DoesNotIncludePort()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingPort(1433)
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=localhost");
        Assert.DoesNotContain(",1433", connectionString);
    }

    [TestMethod]
    public void Build_WithNamedInstance_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingNamedInstance("SQLEXPRESS")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=localhost\\SQLEXPRESS");
    }

    [TestMethod]
    public void Build_WithDefaultInstanceName_DoesNotIncludeInstance()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingNamedInstance("MSSQLSERVER")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=localhost");
        Assert.DoesNotContain("\\MSSQLSERVER", connectionString);
    }

    [TestMethod]
    public void Build_WithDefaultKeyword_DoesNotIncludeInstance()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingNamedInstance("DEFAULT")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=localhost");
        Assert.DoesNotContain("\\DEFAULT", connectionString);
    }

    [TestMethod]
    public void Build_WithCustomDatabase_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingDatabase("MyDatabase")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Initial Catalog=\"MyDatabase\"");
    }

    [TestMethod]
    public void Build_WithCredentials_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingCredential("sa", "P@ssw0rd!")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "User ID=\"sa\"");
        StringAssert.Contains(connectionString, "Password=\"P@ssw0rd!\"");
        Assert.DoesNotContain("Integrated Security", connectionString);
    }

    [TestMethod]
    public void Build_WithNamedPipes_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingNamedPipes()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=np:\\\\localhost\\pipe");
        StringAssert.Contains(connectionString, "\\sql\\query");
    }

    [TestMethod]
    public void Build_WithNamedPipesAndDefaultInstance_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingNamedPipes()
            .UsingNamedInstance("MSSQLSERVER")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=np:\\\\localhost\\pipe\\sql\\query");
        Assert.DoesNotContain("MSSQL$", connectionString);
    }

    [TestMethod]
    public void Build_WithNamedPipesAndCustomInstance_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingNamedPipes()
            .UsingNamedInstance("SQLEXPRESS")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=np:\\\\localhost\\pipe\\MSSQL$SQLEXPRESS\\sql\\query");
    }

    [TestMethod]
    public void Build_WithAllFlagsEnabled_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .EnableAllFlags()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Persist Security Info=True");
        StringAssert.Contains(connectionString, "Pooling=True");
        StringAssert.Contains(connectionString, "MultipleActiveResultSets=True");
        StringAssert.Contains(connectionString, "Encrypt=True");
        StringAssert.Contains(connectionString, "TrustServerCertificate=True");
    }

    [TestMethod]
    public void Build_WithAllFlagsDisabled_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .DisableAllFlags()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Persist Security Info=False");
        StringAssert.Contains(connectionString, "Pooling=False");
        StringAssert.Contains(connectionString, "MultipleActiveResultSets=False");
        StringAssert.Contains(connectionString, "Encrypt=False");
        StringAssert.Contains(connectionString, "TrustServerCertificate=False");
    }

    [TestMethod]
    public void Build_WithIndividualFlagsEnabled_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .DisableAllFlags()
            .EnablePersistSecurityInfo()
            .EnableConnectionPooling()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Persist Security Info=True");
        StringAssert.Contains(connectionString, "Pooling=True");
        StringAssert.Contains(connectionString, "MultipleActiveResultSets=False");
        StringAssert.Contains(connectionString, "Encrypt=False");
        StringAssert.Contains(connectionString, "TrustServerCertificate=False");
    }

    [TestMethod]
    public void Build_WithIndividualFlagsDisabled_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .EnableAllFlags()
            .DisableEncryption()
            .DisableTrustServerCertificate()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Persist Security Info=True");
        StringAssert.Contains(connectionString, "Pooling=True");
        StringAssert.Contains(connectionString, "MultipleActiveResultSets=True");
        StringAssert.Contains(connectionString, "Encrypt=False");
        StringAssert.Contains(connectionString, "TrustServerCertificate=False");
    }

    [TestMethod]
    public void EnablePersistSecurityInfo_ThenDisable_ProducesCorrectConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .EnablePersistSecurityInfo()
            .DisablePersistSecurityInfo()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Persist Security Info=False");
    }

    [TestMethod]
    public void EnableConnectionPooling_ThenDisable_ProducesCorrectConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .EnableConnectionPooling()
            .DisableConnectionPooling()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Pooling=False");
    }

    [TestMethod]
    public void EnableMultipleActiveResults_ThenDisable_ProducesCorrectConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .EnableMultipleActiveResults()
            .DisableMultipleActiveResults()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "MultipleActiveResultSets=False");
    }

    [TestMethod]
    public void EnableEncryption_ThenDisable_ProducesCorrectConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .EnableEncryption()
            .DisableEncryption()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Encrypt=False");
    }

    [TestMethod]
    public void EnableTrustServerCertificate_ThenDisable_ProducesCorrectConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .EnableTrustServerCertificate()
            .DisableTrustServerCertificate()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "TrustServerCertificate=False");
    }

    [TestMethod]
    public void Build_ComplexScenarioWithTcp_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("sql.example.com")
            .UsingPort(14330)
            .UsingNamedInstance("PROD01")
            .UsingDatabase("ProductionDB")
            .UsingCredential("dbuser", "SecurePass123!")
            .EnableMultipleActiveResults()
            .EnableEncryption()
            .DisableTrustServerCertificate()
            .DisableConnectionPooling()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=sql.example.com\\PROD01,14330");
        StringAssert.Contains(connectionString, "Initial Catalog=\"ProductionDB\"");
        StringAssert.Contains(connectionString, "User ID=\"dbuser\"");
        StringAssert.Contains(connectionString, "Password=\"SecurePass123!\"");
        StringAssert.Contains(connectionString, "MultipleActiveResultSets=True");
        StringAssert.Contains(connectionString, "Encrypt=True");
        StringAssert.Contains(connectionString, "TrustServerCertificate=False");
        StringAssert.Contains(connectionString, "Pooling=False");
    }

    [TestMethod]
    public void Build_ComplexScenarioWithNamedPipes_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("192.168.1.50")
            .UsingNamedPipes()
            .UsingNamedInstance("INSTANCE2")
            .UsingDatabase("TestDB")
            .UsingCredential("testuser", "Test@123")
            .DisableAllFlags()
            .EnableMultipleActiveResults()
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=np:\\\\192.168.1.50\\pipe\\MSSQL$INSTANCE2\\sql\\query");
        StringAssert.Contains(connectionString, "Initial Catalog=\"TestDB\"");
        StringAssert.Contains(connectionString, "User ID=\"testuser\"");
        StringAssert.Contains(connectionString, "Password=\"Test@123\"");
        StringAssert.Contains(connectionString, "MultipleActiveResultSets=True");
        StringAssert.Contains(connectionString, "Pooling=False");
    }

    [TestMethod]
    public void ToString_CallsBuild()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingDatabase("TestDB");

        // Act
        string connectionString = builder.ToString();

        // Assert
        StringAssert.Contains(connectionString, "Server=localhost");
        StringAssert.Contains(connectionString, "Initial Catalog=\"TestDB\"");
    }

    [TestMethod]
    public void Build_WithValidInstanceNames_ProducesValidConnectionStrings()
    {
        // Test various valid instance name patterns
        string[] validNames = { "A", "SQL2022", "INST_01", "My$Instance", "INST123ABC" };

        foreach (string instanceName in validNames)
        {
            // Arrange & Act
            string connectionString = SqlConnectionStringBuilder
                .UsingServerAddress("localhost")
                .UsingNamedInstance(instanceName)
                .Build();

            // Assert
            StringAssert.Contains(connectionString, $"Server=localhost\\{instanceName.ToUpperInvariant()}");
        }
    }

    #endregion

    #region Invalid Input Tests

    [TestMethod]
    public void UsingServerAddress_WithNull_ThrowsNullReferenceException()
    {
        // Act & Assert
        Assert.ThrowsExactly<NullReferenceException>(() => 
            SqlConnectionStringBuilder.UsingServerAddress(null!));
    }

    [TestMethod]
    public void UsingServerAddress_WithEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            SqlConnectionStringBuilder.UsingServerAddress(string.Empty));
    }

    [TestMethod]
    public void UsingServerAddress_WithWhitespace_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            SqlConnectionStringBuilder.UsingServerAddress("   "));
    }

    [TestMethod]
    public void UsingServerAddress_WithTooLongHostname_ThrowsArgumentException()
    {
        // Arrange - hostname > 253 characters
        string invalidHostname = new string('a', 254);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            SqlConnectionStringBuilder.UsingServerAddress(invalidHostname));
    }

    [TestMethod]
    public void UsingServerAddress_WithLeadingHyphen_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            SqlConnectionStringBuilder.UsingServerAddress("-invalid"));
    }

    [TestMethod]
    public void UsingServerAddress_WithTrailingHyphen_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            SqlConnectionStringBuilder.UsingServerAddress("invalid-"));
    }

    [TestMethod]
    public void UsingServerAddress_WithConsecutiveDots_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            SqlConnectionStringBuilder.UsingServerAddress("invalid..hostname"));
    }

    [TestMethod]
    public void UsingDatabase_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            builder.UsingDatabase(null!));
    }

    [TestMethod]
    public void UsingDatabase_WithEmpty_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            builder.UsingDatabase(string.Empty));
    }

    [TestMethod]
    public void UsingDatabase_WithWhitespace_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            builder.UsingDatabase("   "));
    }

    [TestMethod]
    public void UsingCredential_WithNullUsername_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            builder.UsingCredential(null!, "password"));
    }

    [TestMethod]
    public void UsingCredential_WithEmptyUsername_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            builder.UsingCredential(string.Empty, "password"));
    }

    [TestMethod]
    public void UsingCredential_WithWhitespaceUsername_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            builder.UsingCredential("   ", "password"));
    }

    [TestMethod]
    public void UsingCredential_WithNullPassword_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            builder.UsingCredential("username", null!));
    }

    [TestMethod]
    public void UsingCredential_WithEmptyPassword_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            builder.UsingCredential("username", string.Empty));
    }

    [TestMethod]
    public void UsingCredential_WithWhitespacePassword_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => 
            builder.UsingCredential("username", "   "));
    }

    [TestMethod]
    public void UsingNamedInstance_WithNull_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance(null!));
    }

    [TestMethod]
    public void UsingNamedInstance_WithEmpty_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance(string.Empty));
    }

    [TestMethod]
    public void UsingNamedInstance_WithWhitespace_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("   "));
    }

    [TestMethod]
    public void UsingNamedInstance_WithTooLongName_ThrowsArgumentException()
    {
        // Arrange - instance name > 16 characters
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");
        string tooLongInstance = "VERYLONGINSTANCENAME";

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance(tooLongInstance));
    }

    [TestMethod]
    public void UsingNamedInstance_StartingWithNumber_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("1InvalidStart"));
    }

    [TestMethod]
    public void UsingNamedInstance_WithSpace_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("Invalid Name"));
    }

    [TestMethod]
    public void UsingNamedInstance_WithHyphen_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("Invalid-Name"));
    }

    [TestMethod]
    public void UsingNamedInstance_WithColon_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("Invalid:Name"));
    }

    [TestMethod]
    public void UsingNamedInstance_WithSemicolon_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("Invalid;Name"));
    }

    [TestMethod]
    public void UsingNamedInstance_WithApostrophe_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("Invalid'Name"));
    }

    [TestMethod]
    public void UsingNamedInstance_WithBackslash_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("Invalid\\Name"));
    }

    [TestMethod]
    public void UsingNamedInstance_WithForwardSlash_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("Invalid/Name"));
    }

    [TestMethod]
    public void UsingNamedInstance_WithComma_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("Invalid,Name"));
    }

    [TestMethod]
    public void UsingNamedInstance_WithAmpersand_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("Invalid&Name"));
    }

    [TestMethod]
    public void UsingNamedInstance_WithAtSymbol_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlConnectionStringBuilder.UsingServerAddress("localhost");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => 
            builder.UsingNamedInstance("Invalid@Name"));
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [TestMethod]
    public void Build_WithMinimumPortNumber_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingPort(0)
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=localhost,0");
    }

    [TestMethod]
    public void Build_WithMaximumPortNumber_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingPort(65535)
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=localhost,65535");
    }

    [TestMethod]
    public void Build_WithOneCharacterInstanceName_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingNamedInstance("A")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=localhost\\A");
    }

    [TestMethod]
    public void Build_WithSixteenCharacterInstanceName_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingNamedInstance("MAXLENGTH123456")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=localhost\\MAXLENGTH123456");
    }

    [TestMethod]
    public void Build_MethodChaining_ProducesValidConnectionString()
    {
        // Arrange & Act - Test fluent interface
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingDatabase("DB1")
            .UsingDatabase("DB2") // Override previous
            .UsingPort(1434)
            .UsingPort(1435) // Override previous
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Initial Catalog=\"DB2\"");
        StringAssert.Contains(connectionString, "Server=localhost,1435");
    }

    [TestMethod]
    public void Build_WithSpecialCharactersInCredentials_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("localhost")
            .UsingCredential("user@domain", "P@$$w0rd!#%")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "User ID=\"user@domain\"");
        StringAssert.Contains(connectionString, "Password=\"P@$$w0rd!#%\"");
    }

    [TestMethod]
    public void Build_WithLocalAddress_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("(local)")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=(local)");
    }

    [TestMethod]
    public void Build_WithDotAddress_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress(".")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=.");
    }

    [TestMethod]
    public void Build_WithFQDN_ProducesValidConnectionString()
    {
        // Arrange & Act
        string connectionString = SqlConnectionStringBuilder
            .UsingServerAddress("sql-server.corporate.example.com")
            .Build();

        // Assert
        StringAssert.Contains(connectionString, "Server=sql-server.corporate.example.com");
    }

    #endregion
}