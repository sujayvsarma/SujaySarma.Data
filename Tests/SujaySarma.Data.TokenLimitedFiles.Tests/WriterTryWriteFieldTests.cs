using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Files.TokenLimitedFiles.Constants;

namespace SujaySarma.Data.Files.TokenLimitedFiles.Tests
{
    /// <summary>
    /// Unit tests for TokenLimitedFileWriter
    /// </summary>
    [TestClass]
    public class WriterTryWriteFieldTests
    {

        [TestMethod(DisplayName = "TryWriteField: Single field")]
        [TestCategory("Functional")]
        public void WriteSingleField()
        {
            string data = "Simple field";
            (TokenLimitedFileWriter writer, string path) = CreateWriter();

            Assert.IsTrue(writer.TryWriteField(data));
            writer.Dispose();

            string result = GetTempContentAndDeleteIt(path);
            Assert.AreEqual($"\"{data}\"", result);
        }

        [TestMethod(DisplayName = "TryWriteField: Two string fields")]
        [TestCategory("Functional")]
        public void WriteTwoStringFields()
        {
            string[] data = new string[] { "Field1", "Field2" };
            (TokenLimitedFileWriter writer, string path) = CreateWriter();

            Assert.IsTrue(writer.TryWriteField(data[0]));
            Assert.IsTrue(writer.TryWriteField(data[1]));
            writer.Dispose();

            string result = GetTempContentAndDeleteIt(path);
            Assert.AreEqual($"\"{data[0]}\",\"{data[1]}\"", result);
        }

        [TestMethod(DisplayName = "TryWriteField: Mixed fields")]
        [TestCategory("Functional")]
        public void WriteMixedFields()
        {
            object[] data = new object[] { "Field1", 2, false, 14.22f, "hello world!\nThis is me..." };
            (TokenLimitedFileWriter writer, string path) = CreateWriter();

            foreach (object item in data)
            {
                Assert.IsTrue(writer.TryWriteField(item));
            }
            writer.Dispose();

            string result = GetTempContentAndDeleteIt(path);
            Assert.AreEqual($"\"{data[0]}\",{data[1]},{data[2]},{data[3]},\"{data[4]}\"", result);
        }

        [TestMethod(DisplayName = "TryWriteRecord: Write multiple records")]
        [TestCategory("Functional")]
        public void WriteMultipleRecords()
        {
            List<object[]> data = new List<object[]>
            {
                new object[] { "Field1", 2, false, 14.22f, "hello world!\nThis is me..." },
                new object[] { "Field2", 3, true, 0.22f, "Second row" },
                new object[] { "Field3", 4, true, 0f, "Third row" }
            };
            (TokenLimitedFileWriter writer, string path) = CreateWriter();

            foreach (object[] record in data)
            {
                Assert.IsTrue(writer.TryWriteRecord(record));
            }
            writer.Dispose();

            // Read it using our reader!
            TokenLimitedFileReader reader = new TokenLimitedFileReader(path, ',');
            int index = 0;
            while (true)
            {
                ReaderExitReason reason = reader.TryReadRecord(out string[] record);
                if (reason is ReaderExitReason.EndOfFileOrStream or ReaderExitReason.BlankLineEncountered)
                {
                    break;
                }
                ++index;

                Assert.HasCount(5, record, $"Record# {index}");
                switch (index)
                {
                    case 1: Assert.AreEqual("Field1", record[0]); break;
                    case 2: Assert.AreEqual("Field2", record[0]); break;
                    case 3: Assert.AreEqual("Field3", record[0]); break;
                }
            }

            Assert.AreEqual(3, index);

        }



        private string GetTempContentAndDeleteIt(string path)
        {
            string content;
            using (StreamReader sr = new StreamReader(path))
            {
                content = sr.ReadToEnd();
            }

            File.Delete(path);

            return content;
        }


        private (TokenLimitedFileWriter writer, string path) CreateWriter(char fieldDelimiter = ',', string recordDelimiter = "\r\n")
        {
            string path = Path.GetTempFileName();
            return (
                        writer: new TokenLimitedFileWriter(
                                    path,
                                    fieldDelimiter,
                                    recordDelimiter,
                                    Encoding.UTF8,
                                    FileMode.Append     // GetTempFileName will create the file.
                                    ),
                        path: path
                   );
        }

    }
}