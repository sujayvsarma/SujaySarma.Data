using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Files.TokenLimitedFiles.Constants;

namespace SujaySarma.Data.Files.TokenLimitedFiles.Tests
{
    /// <summary>
    /// Unit tests for TokenLimitedFileReaderAsync
    /// </summary>
    [TestClass]
    public class TokenLimitedFileReaderAsyncTests
    {
        [TestMethod]
        public async Task TryReadFieldAsync_SimpleUnquotedField_ReadsCorrectly()
        {
            string data = "Hello,World";
            using var reader = CreateReaderFromString(data);
            
            var result = await reader.TryReadFieldAsync();
            
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual("Hello", result.Data);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result.Reason);
        }

        [TestMethod]
        public async Task TryReadFieldAsync_QuotedField_ReadsCorrectly()
        {
            string data = "\"Hello, World\",Next";
            using var reader = CreateReaderFromString(data);
            
            var result = await reader.TryReadFieldAsync();
            
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual("Hello, World", result.Data);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result.Reason);
        }

        [TestMethod]
        public async Task TryReadFieldAsync_EscapedQuotes_ReadsCorrectly()
        {
            string data = "\"He said \"\"Hello\"\"\",Next";
            using var reader = CreateReaderFromString(data);
            
            var result = await reader.TryReadFieldAsync();
            
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual("He said \"Hello\"", result.Data);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result.Reason);
        }

        [TestMethod]
        public async Task TryReadFieldAsync_EmptyField_ReadsCorrectly()
        {
            string data = ",Next";
            using var reader = CreateReaderFromString(data);
            
            var result = await reader.TryReadFieldAsync();
            
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(string.Empty, result.Data);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result.Reason);
        }

        [TestMethod]
        public async Task TryReadFieldAsync_QuotedFieldWithNewline_ReadsCorrectly()
        {
            string data = "\"Line1\nLine2\",Next";
            using var reader = CreateReaderFromString(data);
            
            var result = await reader.TryReadFieldAsync();
            
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual("Line1\nLine2", result.Data);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result.Reason);
        }

        [TestMethod]
        public async Task TryReadFieldAsync_LastFieldInRecord_ReturnsRecordDelimiter()
        {
            string data = "LastField\r\n";
            using var reader = CreateReaderFromString(data);
            
            var result = await reader.TryReadFieldAsync();
            
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual("LastField", result.Data);
            Assert.AreEqual(ReaderExitReason.RecordDelimiterEncountered, result.Reason);
        }

        [TestMethod]
        public async Task TryReadFieldAsync_SemicolonDelimiter_ReadsCorrectly()
        {
            string data = "Field1;Field2";
            using var reader = CreateReaderFromString(data, ';');
            
            var result = await reader.TryReadFieldAsync();
            
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual("Field1", result.Data);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result.Reason);
        }

        [TestMethod]
        public async Task TryReadRecordAsync_SimpleRecord_ReadsAllFields()
        {
            string data = "Field1,Field2,Field3\r\n";
            using var reader = CreateReaderFromString(data);
            
            var result = await reader.TryReadRecordAsync();
            
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(3, result.Data.Count);
            Assert.AreEqual("Field1", result.Data[0]);
            Assert.AreEqual("Field2", result.Data[1]);
            Assert.AreEqual("Field3", result.Data[2]);
        }

        [TestMethod]
        public async Task TryReadRecordAsync_MixedQuotedAndUnquoted_ReadsCorrectly()
        {
            string data = "Plain,\"Quoted\",\"Quoted, with comma\",123\r\n";
            using var reader = CreateReaderFromString(data);
            
            var result = await reader.TryReadRecordAsync();
            
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(4, result.Data.Count);
            Assert.AreEqual("Plain", result.Data[0]);
            Assert.AreEqual("Quoted", result.Data[1]);
            Assert.AreEqual("Quoted, with comma", result.Data[2]);
            Assert.AreEqual("123", result.Data[3]);
        }

        [TestMethod]
        public async Task TryReadRecordAsync_EmptyFields_ReadsCorrectly()
        {
            string data = "A,,C,\r\n";
            using var reader = CreateReaderFromString(data);
            
            var result = await reader.TryReadRecordAsync();
            
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual(4, result.Data.Count);
            Assert.AreEqual("A", result.Data[0]);
            Assert.AreEqual(string.Empty, result.Data[1]);
            Assert.AreEqual("C", result.Data[2]);
            Assert.AreEqual(string.Empty, result.Data[3]);
        }

        [TestMethod]
        public async Task TryReadRecordAsync_MultipleRecords_ReadsSequentially()
        {
            string data = "Row1Field1,Row1Field2\r\nRow2Field1,Row2Field2\r\n";
            using var reader = CreateReaderFromString(data);
            
            var result1 = await reader.TryReadRecordAsync();
            Assert.IsTrue(result1.IsSuccessful);
            Assert.AreEqual(2, result1.Data.Count);
            Assert.AreEqual("Row1Field1", result1.Data[0]);
            
            var result2 = await reader.TryReadRecordAsync();
            Assert.IsTrue(result2.IsSuccessful);
            Assert.AreEqual(2, result2.Data.Count);
            Assert.AreEqual("Row2Field1", result2.Data[0]);
        }

        [TestMethod]
        [DeploymentItem("TestData")]
        [TestCategory("Integration")]
        public async Task Performance_ReadEntireCSVFileAsync_MeetsPerformanceBenchmark()
        {
            var csvFiles = Directory.GetFiles(".", "*.csv", SearchOption.TopDirectoryOnly);
            
            if (csvFiles.Length == 0)
            {
                Assert.Inconclusive("No CSV test files found in TestData folder.");
                return;
            }

            foreach (var csvFile in csvFiles)
            {
                Console.WriteLine($"\n=== Testing file (Async): {Path.GetFileName(csvFile)} ===");
                
                var fileInfo = new FileInfo(csvFile);
                Console.WriteLine($"File size: {fileInfo.Length:N0} bytes ({fileInfo.Length / 1024.0:F2} KB)");
                
                var stopwatch = Stopwatch.StartNew();
                long initialMemory = GC.GetTotalMemory(true);
                
                int recordCount = 0;
                int totalFieldCount = 0;
                List<string> firstRecord = null;
                List<string> lastRecord = null;
                
                using (var reader = new TokenLimitedFileReaderAsync(csvFile))
                {
                    while (reader.CanRead)
                    {
                        var result = await reader.TryReadRecordAsync();
                        
                        if (!result.IsSuccessful || result.Data == null || result.Data.Count == 0)
                        {
                            continue;
                        }
                        
                        recordCount++;
                        totalFieldCount += result.Data.Count;
                        
                        if (firstRecord == null)
                        {
                            firstRecord = result.Data;
                        }
                        lastRecord = result.Data;
                    }
                }
                
                stopwatch.Stop();
                long finalMemory = GC.GetTotalMemory(false);
                long memoryUsed = finalMemory - initialMemory;
                
                // Output results
                Console.WriteLine($"Records read: {recordCount:N0}");
                Console.WriteLine($"Total fields: {totalFieldCount:N0}");
                Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds:N0} ms");
                Console.WriteLine($"Memory used: {memoryUsed:N0} bytes ({memoryUsed / (1024.0 * 1024.0):F2} MB)");
                
                if (recordCount > 0)
                {
                    Console.WriteLine($"Average per record: {(double)stopwatch.ElapsedMilliseconds / recordCount:F4} ms");
                    Console.WriteLine($"Records per second: {(recordCount * 1000.0 / stopwatch.ElapsedMilliseconds):N0}");
                }
                
                // Sample output
                if (firstRecord != null)
                {
                    Console.WriteLine($"\nFirst record ({firstRecord.Count} fields):");
                    Console.WriteLine($"  {string.Join(", ", firstRecord.Take(5))}...");
                }
                
                if (lastRecord != null)
                {
                    Console.WriteLine($"Last record ({lastRecord.Count} fields):");
                    Console.WriteLine($"  {string.Join(", ", lastRecord.Take(5))}...");
                }
                
                // Validate performance benchmarks
                if (recordCount >= 20000)
                {
                    Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000,
                        $"FAILED: Must parse {recordCount:N0} records in < 1 second. Actual: {stopwatch.ElapsedMilliseconds} ms");
                    
                    if (stopwatch.ElapsedMilliseconds > 300)
                    {
                        Console.WriteLine($"⚠️  WARNING: Exceeded typical performance target of 300ms for {recordCount:N0} records");
                    }
                    else
                    {
                        Console.WriteLine($"✓ Meets typical performance target (< 300ms) for {recordCount:N0} records");
                    }
                }
                
                Assert.IsTrue(recordCount > 0, "Should have read at least one record");
                Assert.IsNotNull(firstRecord, "First record should not be null");
            }
        }

        [TestMethod]
        [DeploymentItem("TestData")]
        [TestCategory("Integration")]
        public async Task Correctness_SampleRecords_ReadAccurately()
        {
            var csvFiles = Directory.GetFiles(".", "*.csv", SearchOption.TopDirectoryOnly);
            
            if (csvFiles.Length == 0)
            {
                Assert.Inconclusive("No CSV test files found in TestData folder.");
                return;
            }

            foreach (var csvFile in csvFiles)
            {
                Console.WriteLine($"\n=== Sampling file (Async): {Path.GetFileName(csvFile)} ===");
                
                using var reader = new TokenLimitedFileReaderAsync(csvFile);
                
                var records = new List<List<string>>();
                int recordIndex = 0;
                
                // Read first 10 records
                while (reader.CanRead && recordIndex < 10)
                {
                    var result = await reader.TryReadRecordAsync();
                    
                    if (result.IsSuccessful && result.Data != null && result.Data.Count > 0)
                    {
                        records.Add(result.Data);
                        recordIndex++;
                    }
                }
                
                Assert.IsTrue(records.Count > 0, "Should read at least one record");
                
                Console.WriteLine($"Read {records.Count} sample records:");
                for (int i = 0; i < Math.Min(3, records.Count); i++)
                {
                    Console.WriteLine($"  Record {i + 1}: {records[i].Count} fields");
                    Console.WriteLine($"    First 3 fields: {string.Join(" | ", records[i].Take(3))}");
                }
                
                foreach (var record in records)
                {
                    Assert.IsTrue(record.Count > 0, "Record should have at least one field");
                    Assert.IsTrue(record.All(f => f != null), "All fields should be non-null");
                }
                
                if (records.Count > 1)
                {
                    int expectedFieldCount = records[0].Count;
                    foreach (var record in records.Skip(1))
                    {
                        Assert.AreEqual(expectedFieldCount, record.Count,
                            $"All records should have the same number of fields ({expectedFieldCount})");
                    }
                }
            }
        }

        // Helper methods
        private TokenLimitedFileReaderAsync CreateReaderFromString(string data, char delimiter = ',')
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var stream = new MemoryStream(bytes);
            return new TokenLimitedFileReaderAsync(stream, delimiter, Encoding.UTF8, leaveStreamOpen: false);
        }
    }
}