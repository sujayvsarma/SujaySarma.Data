using System;
using System.Diagnostics;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.TokenLimitedFiles.Constants;

namespace SujaySarma.Data.TokenLimitedFiles.Tests;

[TestClass]
public class ReaderTryReadRecordPerformanceTests
{
    [TestMethod(DisplayName = "TryReadRecord: Performance reading airports.csv")]
    [TestCategory("Performance (RUN ISOLATED)")]
    [DeploymentItem("TestData")]
    public void TryReadRecord_Performance()
    {
        string filePath = Path.Combine(".", "perfbenchmark.csv");
        if (!File.Exists(filePath))
        {
            Assert.Fail($"Cannot find file '{filePath}' -- Must be present in 'TestData' folder and set to 'Copy always' or 'Copy if newer'.");
        }

        Console.WriteLine("=== perfbenchmark.csv ===");

        const int FIELD_COUNT = 19;
        const int RECORD_COUNT = 83799; // includes header, that we don't differentiate with for this test.

        Stopwatch sw = Stopwatch.StartNew();
        int recordIndex = 0;
        using (TokenLimitedFileReader reader = new TokenLimitedFileReader(filePath))
        {
            while (reader.CanRead)
            {
                ReaderExitReason reason = reader.TryReadRecord(out string[] record);
                if (reason is ReaderExitReason.EndOfFileOrStream or ReaderExitReason.BlankLineEncountered)
                {
                    break;
                }
                ++recordIndex;

                switch (reason)
                {
                    case ReaderExitReason.InContentNullCharacter:
                        Console.WriteLine($"Record# {record}: in-content NULL encountered.");
                        break;

                    case ReaderExitReason.Error:
                        Console.WriteLine($"Record# {record}: Error: {record[0]}");
                        break;

                    default:
                        Assert.HasCount(FIELD_COUNT, record, $"Record# {record}: Expected: {FIELD_COUNT}, Actual: {record.Length}");
                        break;
                }
            }
        }

        sw.Stop();

        Console.WriteLine($"Records read: {recordIndex}/{RECORD_COUNT}");
        Console.WriteLine($"Time taken: {sw.ElapsedMilliseconds:N0} ms");
        try
        {
            Assert.AreEqual(RECORD_COUNT, recordIndex);
            Assert.IsLessThanOrEqualTo<long>(300, sw.ElapsedMilliseconds, $"❎ Took {(sw.ElapsedMilliseconds - 300)} ms too long! (Expected: < 300 ms)");
        }
        finally
        {
            //// Write out the records for a diff.
            //string outputFileName = $"D:\\Sujay\\src\\SujaySarma.Data\\TestResults\\perfbenchmark-output ({DateTime.Now:yyyyMMddHHmmss}).csv";
            //using (StreamWriter writer = new StreamWriter(outputFileName))
            //{
            //    foreach (string record in records)
            //    {
            //        writer.WriteLine(record);
            //    }

            //    writer.Flush();
            //    writer.Close();
            //}

            //Console.WriteLine($"Records dump file: \"{outputFileName}\"");
        }
    }
}
