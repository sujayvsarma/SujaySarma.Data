using System;
using System.IO;
using System.Text;

using SujaySarma.Data.TokenLimitedFiles.Constants;
using SujaySarma.Data.TokenLimitedFiles.Types;

namespace SujaySarma.Data.TokenLimitedFiles;

/// <summary>
/// Reads token (comma, semi-colon, etc) limited records from a flatfile as per RFC 4180. 
/// This reader implementation specifically performs its operations synchronously.
/// </summary>
public class TokenLimitedFileReader : IDisposable
{

    /// <summary>
    /// Reads the next available (complete) record/row from the current position in the initialised stream.
    /// This method complies strictly with RFC 4180 regarding the reading of token-delimited data.
    /// </summary>
    /// <param name="record">[out] The fields of the record/row read:
    /// Empty - Nothing was read, or a blank-line was encountered.
    /// Value - fields of the record that were read -- maybe fewer than actual if there was an error (see [return] value!)
    /// [SPECIAL CASE] Only one element - when [return] is 'Error', contains the error message.</param>
    /// <returns>A <see cref="ReaderExitReason"/>Provides the reason why the reader returned. When this value is 
    /// 'Error', caller must stop attempting to read further from the stream as the data WILL be corrupt!</returns>
    public ReaderExitReason TryReadRecord(out string[] record)
    {
        record = Array.Empty<string>();
        ReaderExitReason reason = ReaderExitReason.EndOfFileOrStream;
        string? field = null;

        if (!CanRead)
        {
            return reason;
        }

        _readRecordBuffer.Clear();
        bool hasPreviousFieldDelimiter = false;

        while (true)
        {
            ReaderExitReason fieldReaderExitReason = TryReadField(out field);
            switch (fieldReaderExitReason)
            {
                case ReaderExitReason.BlankLineEncountered:
                    // return the reason as-is. Nothing in 'record'.
                    return fieldReaderExitReason;

                case ReaderExitReason.EndOfFileOrStream:
                    if (field is not null)
                    {
                        _readRecordBuffer.Append(field);
                    }
                    else
                    {
                        if (hasPreviousFieldDelimiter)
                        {
                            // special case!
                            // record ended right after a field delimiter.
                            // Consider this a blank field.
                            _readRecordBuffer.Append(string.Empty);
                        }
                    }
                    reason = fieldReaderExitReason;
                    goto readerLoopExit;

                case ReaderExitReason.Error:
                    // add the error as the first and only field of the record.
                    record = new string[1] { field! };
                    return fieldReaderExitReason;

                case ReaderExitReason.InContentNullCharacter:
                    // if field itself wasnt NULL, add it to the record.
                    // and terminate reading.
                    if (!string.IsNullOrEmpty(field))
                    {
                        _readRecordBuffer.Append(field);
                    }
                    reason = fieldReaderExitReason;
                    goto readerLoopExit;

                case ReaderExitReason.FieldDelimiterEncountered:
                    // field delimiter, add to record, continue reading.
                    if (field is not null)
                    {
                        _readRecordBuffer.Append(field);
                    }
                    hasPreviousFieldDelimiter = true;
                    break;

                case ReaderExitReason.RecordDelimiterEncountered:
                    // record delimiter, add field read, stop reading.
                    if (field is not null)
                    {
                        _readRecordBuffer.Append(field);
                    }
                    reason = ReaderExitReason.RecordDelimiterEncountered;
                    goto readerLoopExit;
            }
        }

    readerLoopExit:
        record = _readRecordBuffer.ToStringArray();
        return reason;
    }

    /// <summary>
    /// Reads the next available field from the current position in the initialised stream. 
    /// This method complies strictly with RFC 4180 regarding the use of quotes and special characters. 
    /// </summary>
    /// <param name="field">[out] Value of the field read: 
    /// NULL - reached EOF before reading anything, 
    /// Value - of field, 
    /// Error message - when [return] is 'Error'.</param>
    /// <returns>A <see cref="ReaderExitReason"/>Provides the reason why the reader returned. When this value is 
    /// 'Error', caller must stop attempting to read further from the stream as the data WILL be corrupt!</returns>
    public ReaderExitReason TryReadField(out string? field)
    {
        field = null;
        ReaderExitReason reason = ReaderExitReason.EndOfFileOrStream;

        if (!CanRead)
        {
            return reason;
        }

        const char DOUBLE_QUOTE = '"';
        const char CR = '\r';
        const char LF = '\n';

        ReaderScope currentScope = ReaderScope.NotReading;

        // VERY important!
        _readFieldBuffer.Clear();

        while (_reader.Peek() != -1)
        {
            char ch = (char)_reader.Read();

            // The DQ (") defines behaviour for multiple other things. Process it first.
            if (ch is DOUBLE_QUOTE)
            {
                char next = (char)_reader.Peek();
                bool isEscapedQuotes = (next is DOUBLE_QUOTE);
                switch (currentScope)
                {
                    case ReaderScope.NotReading when (!isEscapedQuotes):
                        /*
                         *  Began to read, encountered a single-".
                         *  RFC: Begin quoted scope.
                         */
                        currentScope = ReaderScope.Quoted;
                        break;

                    case ReaderScope.NotReading when isEscapedQuotes:
                        /*
                         *  Began to read, encountered a double-"".
                         *  Two possibles:
                         *  
                         *  1. It is a sequence of 3-" (eg: """This...) 
                         *     where an escape-quote block begins immediately within a quoted field.
                         *     
                         *     RFC: This is okay, begin quoted block, read the escape quotes.
                         *     
                         *  2. It is a regular escape-quoted block in an unquoted segment.
                         *  
                         *     RFC: This is invalid. Escape"" are valid only *within* a quoted block!
                         */
                        _reader.Read();
                        char nextAfterNext = (char)_reader.Peek();
                        if (nextAfterNext is DOUBLE_QUOTE)
                        {
                            // We have 3-"s.
                            // Enter quoted scope for 1st ".
                            currentScope = ReaderScope.Quoted;

                            // Consume the "" as a " and add it to the buffer.
                            _readFieldBuffer.Append(DOUBLE_QUOTE);
                            _reader.Read();
                        }
                        else if (nextAfterNext == _delimiter)
                        {
                            // "" within unquoted section.
                            // Is the next char our delimiter? If so, we have an empty quoted field.
                            _reader.Read();
                            field = string.Empty;
                            return ReaderExitReason.FieldDelimiterEncountered;
                        }
                        else if ((nextAfterNext is CR) || (nextAfterNext is LF) || (nextAfterNext is '\uffff'))
                        {
                            // "" as the final field in a record.
                            _reader.Read();
                            if (_reader.Peek() is LF)
                            {
                                // Eat the LF if present outside quotes.
                                _reader.Read();
                            }
                            field = string.Empty;
                            return ReaderExitReason.RecordDelimiterEncountered;
                        }
                        else
                        {
                            // RFC: Error! Terminate immediately.
                            field = "When field value is not quoted, cannot contain escaped double-quotes.";
                            return ReaderExitReason.Error;
                        }
                        break;

                    case ReaderScope.Unquoted:
                        // Begun to read, unquoted block, encountered " or "".
                        // RFC: Error! Terminate immediately.
                        field = "When field value is not quoted, cannot contain (escaped) double-quotes.";
                        return ReaderExitReason.Error;

                    case ReaderScope.Quoted when (!isEscapedQuotes):
                        // Quoted block, encountered a single-"
                        // RFC: End quote block. This automatically marks any further chars in field as invalid.
                        currentScope = ReaderScope.NonReadable;

                        // Check if it was indeed a quote-block ender or if we have more chars for this field.
                        if ((next != _delimiter) && (next is not CR) && (next is not LF))
                        {
                            field = "Encountered double-quote sequence within field value.";
                            return ReaderExitReason.Error;
                        }
                        break;

                    case ReaderScope.Quoted when isEscapedQuotes:
                        // Quoted block, encountered escaped quotes-"".
                        // RFC: Consume both ", add one of them to buffer, continue.
                        _reader.Read();
                        _readFieldBuffer.Append(DOUBLE_QUOTE);
                        break;
                }
            }
            else if (ch is CR or LF)
            {
                if (currentScope is ReaderScope.Quoted)
                {
                    _readFieldBuffer.Append(ch);
                }
                else
                {
                    if ((ch is CR) && ((char)_reader.Peek() is LF))
                    {
                        // Eat the CR's LF if present outside quotes.
                        _reader.Read();
                    }

                    reason = ReaderExitReason.RecordDelimiterEncountered;
                    break;  // while
                }
            }
            else if (ch == _delimiter)
            {
                if (currentScope is ReaderScope.Quoted)
                {
                    _readFieldBuffer.Append(ch);
                }
                else
                {
                    // If we encountered a quoted empty field, our read sequence would be:
                    // ["",] and we would now have ["] in the buffer. Discard it.
                    if ((_readFieldBuffer.Length == 1) && (_readFieldBuffer[0] is DOUBLE_QUOTE))
                    {
                        _readFieldBuffer.Clear();
                    }

                    reason = ReaderExitReason.FieldDelimiterEncountered;
                    break;  // while
                }
            }
            else if (ch == '\0')
            {
                // Null could be a part of data content -- though RARE!
                // Peek() will return 0, not -1 when it sees '\0'
                // However, it is still an "error" condition (malformed data), terminating the read.
                reason = ReaderExitReason.InContentNullCharacter;
                break;  // while
            }
            else
            {
                switch (currentScope)
                {
                    case ReaderScope.NonReadable:
                        throw new InvalidOperationException($"Invalid condition reached with record: Please file a bug with this record:\n\n{_readFieldBuffer}{ch}.");

                    case ReaderScope.NotReading:
                        currentScope = ReaderScope.Unquoted;
                        _readFieldBuffer.Append(ch);
                        break;

                    default:
                        _readFieldBuffer.Append(ch);
                        break;
                }
            }

        } // while

        field = _readFieldBuffer.ToString();
        return reason;
    }

    #region Common stream functions

    /// <summary>
    /// Returns if this reader can still read the stream.
    /// </summary>
    public bool CanRead
        => ((!_isDisposed) && (!_reader.EndOfStream) && _reader.BaseStream.CanRead);

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Dispose the reader.
    /// </summary>
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _reader.Dispose();
            _readFieldBuffer.Dispose();
            _readRecordBuffer.Dispose();

            GC.SuppressFinalize(this);
        }
    }
    private bool _isDisposed = false;

    #endregion

    #region -- Initialisers --

    /// <summary>
    /// Initialises the reader.
    /// </summary>
    /// <param name="stream">A stream (perhaps from a network or web source) already initialised and perhaps open.</param>
    /// <param name="delimiter">The character that delimits a field. Defaults to a comma.</param>
    /// <param name="encoding">Encoding to use. If NULL, uses auto-detection.</param>
    /// <param name="leaveStreamOpen">Instructs the reader to leave the provided <paramref name="stream"/> open after the reader is done with it.</param>
    public TokenLimitedFileReader(Stream stream, char delimiter = ',', Encoding? encoding = null, bool leaveStreamOpen = false)
    {
        if ((stream is null) || (!stream.CanRead))
        {
            throw new IOException("Provided stream is not initialised or cannot be read from.");
        }

        bool autoDetect = (encoding == null);

        // 64KB buffer for I/O
        _reader = new StreamReader(stream, (encoding ?? Encoding.UTF8), autoDetect, bufferSize: 65536, leaveOpen: leaveStreamOpen);
        _delimiter = delimiter;

        _readFieldBuffer = new FastCharacterArrayBuffer();
        _readRecordBuffer = new FastStringArrayBuffer();
    }

    /// <summary>
    /// Initialises the reader.
    /// </summary>
    /// <param name="path">Path to the disk or network file.</param>
    /// <param name="delimiter">The character that delimits a field. Defaults to a comma.</param>
    /// <param name="encoding">Encoding to use. If NULL, uses auto-detection.</param>
    public TokenLimitedFileReader(string path, char delimiter = ',', Encoding? encoding = null)
    {
        bool autoDetect = (encoding is null);
        FileStreamOptions options = new FileStreamOptions()
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Options = FileOptions.SequentialScan,
            Share = FileShare.Read,
            BufferSize = 65536              // 64KB buffer for I/O
        };

        _reader = new StreamReader(path, (encoding ?? Encoding.UTF8), autoDetect, options);
        _delimiter = delimiter;

        _readFieldBuffer = new FastCharacterArrayBuffer();
        _readRecordBuffer = new FastStringArrayBuffer();
    }

    #endregion

    private readonly StreamReader _reader;
    private readonly char _delimiter;
    private readonly FastCharacterArrayBuffer _readFieldBuffer;
    private readonly FastStringArrayBuffer _readRecordBuffer;

    /// <summary>
    /// Types of reading scopes
    /// </summary>
    public enum ReaderScope
    {
        /// <summary>
        /// Have not started reading yet.
        /// </summary>
        NotReading,

        /// <summary>
        /// Unquoted value.
        /// eg: [Hello]
        /// </summary>
        Unquoted,

        /// <summary>
        /// A value within a set of double quotes. 
        /// eg: ["Hello"]
        /// </summary>
        Quoted,

        /// <summary>
        /// When scope is NonReadable, the reader will 
        /// pass through them, however only the field delimiter 
        /// and record delimiters are considered/processed. Any/all 
        /// other characters are read and discarded.
        /// </summary>
        NonReadable
    }
}
