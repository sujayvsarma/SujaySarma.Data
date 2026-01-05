using System;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.TokenLimitedFiles.Constants;

namespace SujaySarma.Data.TokenLimitedFiles.Tests
{
    /// <summary>
    /// Unit tests for TokenLimitedFileReader
    /// </summary>
    [TestClass]
    public class ReaderTryReadFieldTests
    {
        // Helper method
        private TokenLimitedFileReader CreateReaderFromString(string data, char delimiter = ',')
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var stream = new MemoryStream(bytes);
            return new TokenLimitedFileReader(stream, delimiter, Encoding.UTF8, leaveStreamOpen: false);
        }


        [TestMethod(DisplayName = "TryReadField: Simple unquoted field")]
        [TestCategory("Functional")]
        public void SimpleUnquotedField_ReadsCorrectly()
        {
            string data = "Hello,World";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");
            Assert.AreEqual("Hello", field);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result);
        }

        [TestMethod(DisplayName = "TryReadField: One quoted field with delimiter embedded")]
        [TestCategory("Functional")]
        public void QuotedField_ReadsCorrectly()
        {
            string data = "\"Hello, World\",Next";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual("Hello, World", field);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result);
        }

        [TestMethod(DisplayName = "TryReadField: Quoted field with escaped quotes")]
        [TestCategory("Functional")]
        public void EscapedQuotes_ReadsCorrectly()
        {
            string data = "\"He said \"\"Hello\"\"\",Next";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual("He said \"Hello\"", field);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result);
        }

        [TestMethod(DisplayName = "TryReadField: Empty field")]
        [TestCategory("Functional")]
        public void EmptyField_ReadsCorrectly()
        {
            string data = ",Next";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual(string.Empty, field);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result);
        }

        [TestMethod(DisplayName = "TryReadField: Quoted empty field")]
        [TestCategory("Functional")]
        public void QuotedEmptyField_ReadsCorrectly()
        {
            string data = "\"\",Next";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual(string.Empty, field);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result);
        }

        [TestMethod(DisplayName = "TryReadField: Quoted field with LF")]
        [TestCategory("Functional")]
        public void QuotedFieldWithNewline_ReadsCorrectly()
        {
            string data = "\"Line1\nLine2\",Next";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual("Line1\nLine2", field);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result);
        }

        [TestMethod(DisplayName = "TryReadField: Unquoted field single field ending with CRLF")]
        [TestCategory("Functional")]
        public void LastFieldInRecord_ReturnsRecordDelimiter()
        {
            string data = "LastField\r\n";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual("LastField", field);
            Assert.AreEqual(ReaderExitReason.RecordDelimiterEncountered, result);
        }

        [TestMethod(DisplayName = "TryReadField: Semi-colon delimiter")]
        [TestCategory("Functional")]
        public void SemicolonDelimiter_ReadsCorrectly()
        {
            string data = "Field1;Field2";
            using var reader = CreateReaderFromString(data, ';');

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual("Field1", field);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result);
        }

        [TestMethod(DisplayName = "TryReadField: Non-compliant double-quotes (quotes in the middle of value)")]
        [TestCategory("Functional")]
        public void NonCompliantQuotes_HandlesGracefully()
        {
            // Test: Value"s (quote in middle of unquoted field)
            string data = "Value\"s,Next";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual(ReaderExitReason.Error, result);
            Console.WriteLine($"Error message: {field}");
        }

        [TestMethod(DisplayName = "TryReadField: Within a quoted field, an escape quoted block begins immediately")]
        [TestCategory("Functional")]
        public void EscapeQuotedFieldBeginsQuotedScope()
        {
            string data = "\"\"\"This value\"\", must be read correctly\",\"no\"";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual("\"This value\", must be read correctly", field);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result);
        }

        [TestMethod(DisplayName = "TryReadField: Within a quoted field, an escape quoted block appears at the end")]
        [TestCategory("Functional")]
        public void EscapeQuotedFieldEndsQuotedScope()
        {
            string data = "\"He said: \"\"She said it!\"\"\",\"no\"";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual("He said: \"She said it!\"", field);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result);
        }

        [TestMethod(DisplayName = "TryReadField: An escape quote that is not pair-matched")]
        [TestCategory("Functional")]
        public void UnmatchedEscapeQuotes()
        {
            string data = "\"My monitor is 24\"\" in size.\",\"no\"";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual("My monitor is 24\" in size.", field);
            Assert.AreEqual(ReaderExitReason.FieldDelimiterEncountered, result);
        }

        [TestMethod(DisplayName = "TryReadField: Triple quotes in the middle of a string")]
        [TestCategory("Functional")]
        public void TripleQuotes()
        {
            string data = "\"In what world is \"\"\"this\"\"\" a valid value?\",\"no\"";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Console.WriteLine($"Error: {field}");
            Assert.AreEqual(ReaderExitReason.Error, result);
        }


        [TestMethod(DisplayName = "TryReadField: NULL character embedded in data")]
        [TestCategory("Functional")]
        public void InContentNull()
        {
            string data = "\"This value contains a \0 character.\",something else,123";
            using var reader = CreateReaderFromString(data);

            ReaderExitReason result = reader.TryReadField(out string? field);

            Console.WriteLine($"Actual: [{data}]");
            Console.WriteLine($"Read: [{field}]");

            Assert.AreEqual(ReaderExitReason.InContentNullCharacter, result);
        }
    }
}