using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Files.TokenLimitedFiles.Tests.Objects;

namespace SujaySarma.Data.Files.TokenLimitedFiles.Tests;


[TestClass]
public class ContextTests
{

    [TestMethod(DisplayName = "TokenLimitedFileContext: Test functionality")]
    [TestCategory("Functional")]
    [DeploymentItem("TestData")]
    public void TestContext()
    {
        string input = Path.Combine(".", "perfbenchmark.csv");
        string output = Path.GetTempFileName();

        Console.WriteLine($"Input: '{input}'");
        Console.WriteLine($"Output: '{output}'");

        TokenLimitedFileContext context = TokenLimitedFileContext.For<Airport>()
                                                .AddReader(input)
                                                    .AddWriter(output, mode: FileMode.OpenOrCreate);

        context.ReadPreamble();
        context.WritePreamble();

        while (context.CanRead)
        {
            Airport? airport = (Airport?)context.Read();
            if (airport != default)
            {
                context.Write(airport);
            }
        }

        context.Dispose();

        Console.WriteLine("Output written successfully. Use WinMerge or a tool to diff that the two files are the same or equivalent.");
    }

}
