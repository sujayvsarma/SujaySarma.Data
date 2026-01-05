using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.TokenLimitedFiles.Constants;

namespace SujaySarma.Data.TokenLimitedFiles.Tests;

/// <summary>
/// Performance tests for TokenLimitedFileReaderSync
/// </summary>
[TestClass]
public class ReaderTryReadFieldPerformanceTests
{

    [TestMethod(DisplayName = "TryReadField: Performance reading airports.csv")]
    [TestCategory("Performance (RUN ISOLATED)")]
    [DeploymentItem("TestData")]
    public void TryReadField_Performance()
    {
        string filePath = Path.Combine(".", "perfbenchmark.csv");
        if (!File.Exists(filePath))
        {
            Assert.Fail($"Cannot find file '{filePath}' -- Must be present in 'TestData' folder and set to 'Copy always' or 'Copy if newer'.");
        }

        Console.WriteLine("=== perfbenchmark.csv ===");

        List<string> records = new List<string>();
        const int FIELD_COUNT = 19;
        const int RECORD_COUNT = 83799; // includes header, that we don't differentiate with for this test.

        Stopwatch sw = Stopwatch.StartNew();
        int recordIndex = 0;
        using (TokenLimitedFileReader reader = new TokenLimitedFileReader(filePath))
        {
            List<string> recordBuilder = new List<string>();
            while (reader.CanRead)
            {
                ReaderExitReason reason = reader.TryReadField(out string? field);
                switch (reason)
                {
                    case ReaderExitReason.InContentNullCharacter:
                    case ReaderExitReason.EndOfFileOrStream:
                        goto exitReaderLoop;

                    case ReaderExitReason.BlankLineEncountered:
                        records.Add(string.Empty);
                        recordBuilder.Clear();

                        ++recordIndex;
                        Console.WriteLine($"Blank record encountered at: {recordIndex}");
                        break;

                    case ReaderExitReason.FieldDelimiterEncountered:
                        if (field is not null)
                        {
                            recordBuilder.Add(field);
                        }
                        break;

                    case ReaderExitReason.RecordDelimiterEncountered:
                        ++recordIndex;
                        if (field is not null)
                        {
                            recordBuilder.Add(field);
                        }

                        if (recordBuilder.Count != FIELD_COUNT)
                        {
                            Console.WriteLine($"Record# {recordIndex}: Fields mismatch: expected: [{FIELD_COUNT}], actual [{recordBuilder.Count}].");
                            Console.WriteLine($"> {recordIndex}: [{string.Join("], [", recordBuilder)}]");
                        }
                        records.Add("[" + string.Join("], [", recordBuilder) + "]");
                        recordBuilder.Clear();
                        break;

                    case ReaderExitReason.Error:
                        Console.WriteLine($"Record# {recordIndex}: Error: {field}");
                        Console.WriteLine($"> {recordIndex}: [{string.Join("], [", recordBuilder)}]");
                        goto exitReaderLoop;
                }
            }
        }

    exitReaderLoop:
        sw.Stop();

        Console.WriteLine($"Records read: {records.Count}/{RECORD_COUNT}");
        Console.WriteLine($"Time taken: {sw.ElapsedMilliseconds:N0} ms");
        try
        {
            Assert.IsLessThanOrEqualTo<long>(500, sw.ElapsedMilliseconds, $"❎ Took {(sw.ElapsedMilliseconds - 500)} ms too long! (Expected: < 500 ms)");
        }
        finally
        {
            // Write out the records for a diff.
            string outputFileName = $"D:\\Sujay\\src\\SujaySarma.Data\\TestResults\\perfbenchmark-output ({DateTime.Now:yyyyMMddHHmmss}).csv";
            using (StreamWriter writer = new StreamWriter(outputFileName))
            {
                foreach (string record in records)
                {
                    writer.WriteLine(record);
                }

                writer.Flush();
                writer.Close();
            }

            Console.WriteLine($"Records dump file: \"{outputFileName}\"");
        }
    }

}
