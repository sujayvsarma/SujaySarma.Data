using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SujaySarma.Data.TokenLimitedFiles;

/// <summary>
/// Writes token (comma, semi-colon, etc) limited records to a flatfile as per RFC 4180. 
/// This writer implementation specifically performs its operations synchronously.
/// </summary>
public class TokenLimitedFileWriter : IDisposable
{

    /// <summary>
    /// Writes the provided content as the next record in the sequence.
    /// </summary>
    /// <param name="record">Content/record to write to the destination. Array cannot be NULL, though 
    /// elements may be (they are converted to empty strings in the data)
    /// IMPORTANT: Do not quote string values because the function will quote it if required.</param>
    /// <returns>True - content was written successfully, False - if there were errors.</returns>
    public bool TryWriteRecord(IEnumerable<string?> record)
    {
        if ((!_writeEmptyRows) && record.All(s => string.IsNullOrEmpty(s)))
        {
            return true;
        }

        _fieldsWritten = false;
        foreach (string? field in record)
        {
            if (!TryWriteFieldImpl(field))
            {
                return false;
            }
        }

        _writer.Write(_recordTerminator);

        return true;
    }

    /// <summary>
    /// Writes the provided content as the next record in the sequence.
    /// </summary>
    /// <param name="record">>Content/record to write to the destination. Array cannot be NULL, though 
    /// elements may be (they are converted to empty strings in the data)
    /// IMPORTANT: Do not quote string values because the function will quote it if required.</param>
    /// <returns>True - content was written successfully, False - if there were errors.</returns>
    public bool TryWriteRecord(IEnumerable<object?> record)
    {
        List<string?> rec = new List<string?>();
        foreach(object? obj in record)
        {
            rec.Add(Serialiser.SerialiseValue(obj, _delimiter, _recordTerminator));
        }

        return TryWriteRecord(rec);
    }


    /// <summary>
    /// Writes the provided content as the next field in the sequence.
    /// </summary>
    /// <param name="field">Value/content to write to the destination. If this is NULL, writes an empty string. 
    /// IMPORTANT: The caller must quote the string appropriately, as this function has no mechanism to identify if it needs quoting!</param>
    /// <returns>True - content was written successfully, False - if there were errors.</returns>
    private bool TryWriteFieldImpl(string? field)
    {
        if (!CanWrite)
        {
            return false;
        }

        try
        {
            if (_fieldsWritten)
            {
                _writer.Write(_delimiter);
            }

            _writer.Write(field ?? string.Empty);
            _fieldsWritten = true;
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Writes the provided content as the next field in the sequence.
    /// </summary>
    /// <typeparam name="T">Type of <paramref name="field"/>.</typeparam>
    /// <param name="field">Value/content to write to the destination. If this is NULL, writes an empty string.
    /// IMPORTANT: String values should NOT be as this function will automatically quote strings.</param>
    /// <returns>True - content was written successfully, False - if there were errors.</returns>
    public bool TryWriteField<T>(T? field)
    {
        string s = Serialiser.SerialiseValue(field, _delimiter, _recordTerminator);
        return TryWriteFieldImpl(s);
    }


    #region Common stream functions

    /// <summary>
    /// Returns if this writer can still write the stream.
    /// </summary>
    public bool CanWrite
        => ((!_isDisposed) && _writer.BaseStream.CanWrite);

    #endregion

    #region -- Initialisers --

    /// <summary>
    /// Initialises the writer.
    /// </summary>
    /// <param name="stream">A stream (perhaps from a network or web source) already initialised and perhaps open.</param>
    /// <param name="delimiter">The character that delimits a field. Defaults to a comma.</param>
    /// <param name="recordDelimiter">The [string] sequence that terminates a record. Defaults to CRLF (Windows)</param>
    /// <param name="encoding">Encoding to use. If NULL, uses auto-detection.</param>
    /// <param name="leaveStreamOpen">Instructs the writer to leave the provided <paramref name="stream"/> open after the writer is done with it.</param>
    /// <param name="writeEmptyRows">When set, writes empty records to the file. Otherwise, skips them silently.</param>
    public TokenLimitedFileWriter(Stream stream, char delimiter = ',', string recordDelimiter = "\r\n", Encoding? encoding = null, bool leaveStreamOpen = false, bool writeEmptyRows = false)
    {
        if ((stream is null) || (!stream.CanWrite))
        {
            throw new IOException("Provided stream is not initialised or cannot be written to.");
        }

        // 64KB buffer for I/O
        _writer = new StreamWriter(stream, (encoding ?? Encoding.UTF8), bufferSize: 65536, leaveOpen: leaveStreamOpen);
        _delimiter = delimiter;
        _recordTerminator = recordDelimiter;
        _writeEmptyRows = writeEmptyRows;
    }

    /// <summary>
    /// Initialises the writer.
    /// </summary>
    /// <param name="path">Path to the disk or network file.</param>
    /// <param name="delimiter">The character that delimits a field. Defaults to a comma.</param>
    /// <param name="recordDelimiter">The [string] sequence that terminates a record. Defaults to CRLF (Windows)</param>
    /// <param name="encoding">Encoding to use. If NULL, uses auto-detection.</param>
    /// <param name="mode">The mode to open the file with.</param>
    /// <param name="writeEmptyRows">When set, writes empty records to the file. Otherwise, skips them silently.</param>
    public TokenLimitedFileWriter(string path, char delimiter = ',', string recordDelimiter = "\r\n", Encoding? encoding = null, FileMode mode = FileMode.CreateNew, bool writeEmptyRows = false)
    {
        if (!Enum.IsDefined<FileMode>(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), "Value must be from the 'FileMode' enumeration.");
        }

        if (!VALID_FILEMODES_FOR_WRITING.Contains(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), "Value is not valid for writing.");
        }

        FileStreamOptions options = new FileStreamOptions()
        {
            Access = FileAccess.Write,
            Mode = mode,
            Share = FileShare.None,
            BufferSize = 65536              // 64KB buffer for I/O
        };

        _writer = new StreamWriter(path, (encoding ?? Encoding.UTF8), options);
        _delimiter = delimiter;
        _recordTerminator = recordDelimiter;

        _writeEmptyRows = writeEmptyRows;
    }

    #endregion

    private bool _fieldsWritten = false;

    private readonly StreamWriter _writer;
    private readonly char _delimiter;
    private readonly string _recordTerminator;
    private readonly bool _writeEmptyRows;
    private readonly List<FileMode> VALID_FILEMODES_FOR_WRITING = new List<FileMode>
    {
        FileMode.Append,
        FileMode.CreateNew,
        FileMode.Create,
        FileMode.Open,
        FileMode.OpenOrCreate,
        FileMode.Truncate
    };

    private const char DOUBLE_QUOTE = '"';
    private const char CR = '\r';
    private const char LF = '\n';

    #region IDisposable Implementation

    /// <summary>
    /// Dispose the reader.
    /// </summary>
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _writer.Dispose();

            GC.SuppressFinalize(this);
        }
    }
    private bool _isDisposed = false;

    #endregion
}
