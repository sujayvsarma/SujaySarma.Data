using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.TokenLimitedFiles.Constants;

namespace SujaySarma.Data.TokenLimitedFiles.Tests;

[TestClass]
public class ReaderTryReadRecordTests
{

    [TestMethod(DisplayName = "TryReadRecord:  Read a complete record")]
    [TestCategory("Functional")]
    public void ReadCompleteRecord()
    {
        string data = "123,\"String value\",-455,2.0,\"B-1024,\"\"SSD\"\",\r\nSector-3\r\n,Platten 7, Local Hard Disk\",Simple unquoted value.\r\n";
        using var reader = CreateReaderFromString(data);

        ReaderExitReason reason = reader.TryReadRecord(out string[] record);

        Assert.HasCount(6, record);
        Assert.AreEqual("123", record[0]);
        Assert.AreEqual("String value", record[1]);
        Assert.AreEqual("-455", record[2]);
        Assert.AreEqual("2.0", record[3]);
        Assert.AreEqual("B-1024,\"SSD\",\r\nSector-3\r\n,Platten 7, Local Hard Disk", record[4]);
        Assert.AreEqual("Simple unquoted value.", record[5]);

        Assert.AreEqual(ReaderExitReason.RecordDelimiterEncountered, reason);
    }

    [TestMethod(DisplayName = "TryReadRecord:  Read a record that terminates in an EOF")]
    [TestCategory("Functional")]
    public void ReadFinalRecord()
    {
        string data = "123,\"String value\",-455,2.0,\"B-1024,\"\"SSD\"\",\r\nSector-3\r\n,Platten 7, Local Hard Disk\",Simple unquoted value.";
        using var reader = CreateReaderFromString(data);

        ReaderExitReason reason = reader.TryReadRecord(out string[] record);

        Assert.HasCount(6, record);
        Assert.AreEqual("123", record[0]);
        Assert.AreEqual("String value", record[1]);
        Assert.AreEqual("-455", record[2]);
        Assert.AreEqual("2.0", record[3]);
        Assert.AreEqual("B-1024,\"SSD\",\r\nSector-3\r\n,Platten 7, Local Hard Disk", record[4]);
        Assert.AreEqual("Simple unquoted value.", record[5]);

        Assert.AreEqual(ReaderExitReason.EndOfFileOrStream, reason);
    }

    [TestMethod(DisplayName = "TryReadRecord:  Read a record with an embedded NULL at the start of a field")]
    [TestCategory("Functional")]
    public void ReadRecordWithEmbeddedNullAtFieldStart()
    {
        string data = "123,\"String value\",\0-455,2.0,\"B-1024,\"\"SSD\"\",\r\nSector-3\r\n,Platten 7, Local Hard Disk\",Simple unquoted value.";
        using var reader = CreateReaderFromString(data);

        ReaderExitReason reason = reader.TryReadRecord(out string[] record);

        Assert.HasCount(2, record);
        Assert.AreEqual("123", record[0]);
        Assert.AreEqual("String value", record[1]);

        Assert.AreEqual(ReaderExitReason.InContentNullCharacter, reason);
    }

    [TestMethod(DisplayName = "TryReadRecord:  Read a record with an embedded NULL in a field")]
    [TestCategory("Functional")]
    public void ReadRecordWithEmbeddedNullInField()
    {
        string data = "123,\"String value\",-45\05,2.0,\"B-1024,\"\"SSD\"\",\r\nSector-3\r\n,Platten 7, Local Hard Disk\",Simple unquoted value.";
        using var reader = CreateReaderFromString(data);

        ReaderExitReason reason = reader.TryReadRecord(out string[] record);

        Assert.HasCount(3, record);
        Assert.AreEqual("123", record[0]);
        Assert.AreEqual("String value", record[1]);
        Assert.AreEqual("-45", record[2]);

        Assert.AreEqual(ReaderExitReason.InContentNullCharacter, reason);
    }

    [TestMethod(DisplayName = "TryReadRecord:  Reads multiple records correctly")]
    [TestCategory("Functional")]
    public void ReadMultipleRecordsCorrectly()
    {
        string data = "258036,6530,\"01ID\",2894,100,\"TURF-F\",0,0,\"14\",42.611000061035156,-112.03500366210938,,,,\"32\",42.60369873046875,-112.03099822998047,,,\r\n265052,6594,\"01IL\",416,216,\"TURF\",0,0,\"H1\",,,,,,,,,,,\r\n265053,6595,\"01IN\",45,45,\"ASPH\",0,0,\"H1\",,,,,,,,,,,\r\n252362,6596,\"01IS\",1300,100,\"TURF\",0,0,\"09\",,,,,,\"27\",,,,,\r\n254664,6597,\"01J\",3365,125,\"TURF-P\",1,0,\"18\",30.690900802612305,-81.90640258789062,59,,795,\"36\",30.681699752807617,-81.90499877929688,59,,370\r\n249830,6598,\"01K\",2460,110,\"TURF-G\",1,0,\"17\",,,1157,,,\"35\",,,,,\r\n255679,6599,\"01KS\",2400,100,\"TURF\",0,0,\"18\",,,,,,\"36\",,,,,\r\n265054,6600,\"01KY\",40,40,\"CONC\",0,0,\"H1\",,,,,,,,,,,\r\n";
        using var reader = CreateReaderFromString(data);
        int index = 0;
        while (true)
        {
            ReaderExitReason reason = reader.TryReadRecord(out string[] record);
            if (reason is ReaderExitReason.EndOfFileOrStream or ReaderExitReason.BlankLineEncountered)
            {
                break;
            }
            ++index;

            Assert.HasCount(20, record, $"Record# {index}: Expected: 20, Actual: {record.Length}");
            switch (index)
            {
                case 1: Assert.AreEqual("258036", record[0]); break;
                case 2: Assert.AreEqual("265052", record[0]); break;
                case 3: Assert.AreEqual("265053", record[0]); break;
                case 4: Assert.AreEqual("252362", record[0]); break;
                case 5: Assert.AreEqual("254664", record[0]); break;
                case 6: Assert.AreEqual("249830", record[0]); break;
                case 7: Assert.AreEqual("255679", record[0]); break;
                case 8: Assert.AreEqual("265054", record[0]); break;
            }
        }

        Assert.AreEqual(8, index);
    }



    // Helper method
    private TokenLimitedFileReader CreateReaderFromString(string data, char delimiter = ',')
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var stream = new MemoryStream(bytes);
        return new TokenLimitedFileReader(stream, delimiter, Encoding.UTF8, leaveStreamOpen: false);
    }
}
