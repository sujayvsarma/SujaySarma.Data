using System;
using System.IO;
using System.Text;

using SujaySarma.Data.TokenLimitedFiles.Attributes;
using SujaySarma.Data.TokenLimitedFiles.Constants;

namespace SujaySarma.Data.TokenLimitedFiles;

/// <summary>
/// The TokenLimitedFileContext reads and writes entities to token-delimited files/streams without needing to 
/// instantiate and manage multiple objects (Serialiser, Reader, Writer, etc). 
/// </summary>
public class TokenLimitedFileContext : IDisposable
{
    #region Writing operations

    /// <summary>
    /// Writes the entity record to the file/stream.
    /// </summary>
    /// <param name="instance">Entity to serialise and write.</param>
    public void Write(object? instance)
    {
        if ((_writer is null) || (!_writer.CanWrite))
        {
            throw new IOException("The writer is not initialised. Add a writer?");
        }

        _writer.TryWriteRecord(_serialiser.SerialiseEntity(instance));
    }


    /// <summary>
    /// Writes the preamble (header record) for the provided type.
    /// </summary>
    public void WritePreamble()
    {
        Flatfile flatfile = (Flatfile)_serialiser._container.PersistenceInfo;
        if (flatfile.FieldReferenceMode is Flatfile.FieldReferencesAre.Names)
        {
            if ((_writer is null) || (!_writer.CanWrite))
            {
                throw new IOException("The writer is not initialised. Add a writer?");
            }

            // Header index is 1-based.
            int index = 0;
            string[] emptyArray = Array.Empty<string>();
            while (++index < flatfile.HeaderLineNumber)
            {
                if (!_writer.TryWriteRecord(emptyArray))
                {
                    throw new IOException("Error writing preamble.");
                }
            }

            if ((index == flatfile.HeaderLineNumber) && _writer.CanWrite)
            {
                if (!_writer.TryWriteRecord(_serialiser.SerialiseHeaders()))
                {
                    throw new IOException("Error writing preamble.");
                }
            }
        }
    }

    #endregion

    #region Reading operations

    /// <summary>
    /// Reads the next available record from the added reader, deserialises it and 
    /// returns the rehydrated object entity instance.
    /// </summary>
    /// <returns>Instance of entity rehydrated from the record. NULL if there were no more rows to be read.</returns>
    public object? Read()
    {
        if ((_reader is null) || (!_reader.CanRead))
        {
            throw new IOException("The reader is not initialsed. Add a reader?");
        }

        ReaderExitReason exitReason = _reader.TryReadRecord(out string[] record);
        if (!exitReason.IsNormalRecordExit())
        {
            throw new IOException("Error retrieving record from stream or file.");
        }

        if (record.Length > 0)
        {
            return _serialiser.Deserialise(record);
        }

        return default;
    }

    /// <summary>
    /// Reads the preamble (header record) from the current position in the reader stream/file.
    /// </summary>
    public void ReadPreamble()
    {
        // Try to read the header row.
        Flatfile flatfile = (Flatfile)_serialiser._container.PersistenceInfo;
        if (flatfile.FieldReferenceMode is Flatfile.FieldReferencesAre.Names)
        {
            if ((_reader is null) || (!_reader.CanRead))
            {
                throw new IOException("The reader is not initialsed. Add a reader?");
            }

            // Header index is 1-based.
            int index = 0;
            ReaderExitReason exitReason = ReaderExitReason.EndOfFileOrStream;

            while (++index < flatfile.HeaderLineNumber)
            {
                exitReason = _reader.TryReadRecord(out _);
                if (!exitReason.IsNormalRecordExit())
                {
                    break;
                }
            }

            if ((index == flatfile.HeaderLineNumber) && _reader.CanRead)
            {
                exitReason = _reader.TryReadRecord(out _actualHeaderRowInFile);
                if (!exitReason.IsNormalRecordExit())
                {
                    throw new IOException("Error retrieving header row from stream or file.");
                }

                _serialiser.ReplacePreamble(_actualHeaderRowInFile);
            }
        }
    }

    #endregion

    #region --- Stream Functions ---

    /// <summary>
    /// Returns if the reader can be read from.
    /// </summary>
    public bool CanRead
        => ((_reader is not null) && _reader.CanRead);

    /// <summary>
    /// Returns if the writer can be written to.
    /// </summary>
    public bool CanWrite
        => ((_writer is not null) && _writer.CanWrite);

    #endregion

    #region --- Add writers ---

    /// <summary>
    /// Adds a <see cref="TokenLimitedFileWriter"/> to the context, to enable writing operations. A writer 
    /// must not have been previously added, or an <see cref="InvalidOperationException"/> will be thrown.
    /// </summary>
    /// <param name="path">Path to the disk or network file.</param>
    /// <param name="delimiter">The character that delimits a field. Defaults to a comma.</param>
    /// <param name="recordDelimiter">The [string] sequence that terminates a record. Defaults to CRLF (Windows)</param>
    /// <param name="encoding">Encoding to use. If NULL, uses auto-detection.</param>
    /// <param name="mode">The mode to open the file with.</param>
    /// <param name="writeEmptyRows">When set, writes empty records to the file. Otherwise, skips them silently.</param>
    /// <returns>An instance of self for method chaining.</returns>
    public TokenLimitedFileContext AddWriter(string path, char delimiter = ',', string recordDelimiter = "\r\n", Encoding? encoding = null, FileMode mode = FileMode.CreateNew, bool writeEmptyRows = false)
    {
        if (_writer != null)
        {
            throw new InvalidOperationException("A writer has already been added to this context.");
        }

        return AddWriter(new TokenLimitedFileWriter(path, delimiter, recordDelimiter, encoding, mode, writeEmptyRows));
    }

    /// <summary>
    /// Adds a <see cref="TokenLimitedFileWriter"/> to the context, to enable writing operations. A writer 
    /// must not have been previously added, or an <see cref="InvalidOperationException"/> will be thrown.
    /// </summary>
    /// <param name="stream">A stream (perhaps from a network or web source) already initialised and perhaps open.</param>
    /// <param name="delimiter">The character that delimits a field. Defaults to a comma.</param>
    /// <param name="recordDelimiter">The [string] sequence that terminates a record. Defaults to CRLF (Windows)</param>
    /// <param name="encoding">Encoding to use. If NULL, uses auto-detection.</param>
    /// <param name="leaveStreamOpen">Instructs the writer to leave the provided <paramref name="stream"/> open after the writer is done with it.</param>
    /// <param name="writeEmptyRows">When set, writes empty records to the file. Otherwise, skips them silently.</param>
    /// <returns>An instance of self for method chaining.</returns>
    public TokenLimitedFileContext AddWriter(Stream stream, char delimiter = ',', string recordDelimiter = "\r\n", Encoding? encoding = null, bool leaveStreamOpen = false, bool writeEmptyRows = false)
    {
        if (_writer != null)
        {
            throw new InvalidOperationException("A writer has already been added to this context.");
        }

        return AddWriter(new TokenLimitedFileWriter(stream, delimiter, recordDelimiter, encoding, leaveStreamOpen, writeEmptyRows));
    }

    /// <summary>
    /// Adds a <see cref="TokenLimitedFileWriter"/> to the context, to enable writing operations. A writer 
    /// must not have been previously added, or an <see cref="InvalidOperationException"/> will be thrown.
    /// </summary>
    /// <param name="writer">An instance of <see cref="TokenLimitedFileWriter"/> to add.</param>
    /// <returns>An instance of self for method chaining.</returns>
    public TokenLimitedFileContext AddWriter(TokenLimitedFileWriter writer)
    {
        if (_writer != null)
        {
            throw new InvalidOperationException("A writer has already been added to this context.");
        }

        _writer = writer;
        return this;
    }

    #endregion

    #region --- Add readers ---

    /// <summary>
    /// Adds a <see cref="TokenLimitedFileReader"/> to the context, to enable reading operations. A reader 
    /// must not have been previously added, or an <see cref="InvalidOperationException"/> will be thrown.
    /// </summary>
    /// <param name="path">Path to the disk or network file.</param>
    /// <param name="delimiter">The character that delimits a field. Defaults to a comma.</param>
    /// <param name="encoding">Encoding to use. If NULL, uses auto-detection.</param>
    /// <returns>An instance of self for method chaining.</returns>
    public TokenLimitedFileContext AddReader(string path, char delimiter = ',', Encoding? encoding = null)
    {
        if (_reader != null)
        {
            throw new InvalidOperationException("A reader has already been added to this context.");
        }

        return AddReader(new TokenLimitedFileReader(path, delimiter, encoding));
    }

    /// <summary>
    /// Adds a <see cref="TokenLimitedFileReader"/> to the context, to enable reading operations. A reader 
    /// must not have been previously added, or an <see cref="InvalidOperationException"/> will be thrown.
    /// </summary>
    /// <param name="stream">A stream (perhaps from a network or web source) already initialised and perhaps open.</param>
    /// <param name="delimiter">The character that delimits a field. Defaults to a comma.</param>
    /// <param name="encoding">Encoding to use. If NULL, uses auto-detection.</param>
    /// <param name="leaveStreamOpen">Instructs the reader to leave the provided <paramref name="stream"/> open after the reader is done with it.</param>
    /// <returns>An instance of self for method chaining.</returns>
    public TokenLimitedFileContext AddReader(Stream stream, char delimiter = ',', Encoding? encoding = null, bool leaveStreamOpen = false)
    {
        if (_reader != null)
        {
            throw new InvalidOperationException("A reader has already been added to this context.");
        }

        return AddReader(new TokenLimitedFileReader(stream, delimiter, encoding, leaveStreamOpen));
    }

    /// <summary>
    /// Adds a <see cref="TokenLimitedFileReader"/> to the context, to enable reading operations. A reader 
    /// must not have been previously added, or an <see cref="InvalidOperationException"/> will be thrown.
    /// </summary>
    /// <param name="reader">An instance of <see cref="TokenLimitedFileReader"/> to add.</param>
    /// <returns>An instance of self for method chaining.</returns>
    public TokenLimitedFileContext AddReader(TokenLimitedFileReader reader)
    {
        if (_reader != null)
        {
            throw new InvalidOperationException("A reader has already been added to this context.");
        }

        _reader = reader;
        return this;
    }

    #endregion

    #region Constructors / Initialisers

    /// <summary>
    /// Create a TokenLimitedFileContext instance for the provided <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">Type to create the TokenLimitedFileContext for.</typeparam>
    /// <returns>Instantiated TokenLimitedFileContext.</returns>
    public static TokenLimitedFileContext For<T>()
        => For(typeof(T));

    /// <summary>
    /// Create a TokenLimitedFileContext instance for the provided <paramref name="type"/> type.
    /// </summary>
    /// <param name="type">Type to create the TokenLimitedFileContext for.</param>
    /// <returns>Instantiated TokenLimitedFileContext.</returns>
    public static TokenLimitedFileContext For(Type type)
        => new TokenLimitedFileContext(type);

    /// <summary>
    /// Instantiate a TokenLimitedFileContext for the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to bind the context to.</param>
    private TokenLimitedFileContext(Type type)
    {
        _serialiser = Serialiser.For(type);
    }

    #endregion

    private readonly Serialiser _serialiser;
    private TokenLimitedFileReader? _reader = null;
    private TokenLimitedFileWriter? _writer = null;
    private string[] _actualHeaderRowInFile = Array.Empty<string>();

    #region IDisposable

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _reader?.Dispose();
            _writer?.Dispose();
        }
    }
    private bool _isDisposed = false;

    #endregion
}
