using SujaySarma.Data.Core;

using System;
using System.IO;
using System.Text;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /// <summary>
    /// Reads token-limited flat-files. Default token is the comma (',').
    /// </summary>
    public sealed partial class TokenLimitedFileReader : IDisposable
    {


        /// <summary>
        /// Field delimiter. Default is comma (',').
        /// </summary>
        public char Delimiter => _options.Delimiter;

        /// <summary>
        /// Returns the current text encoding being used
        /// </summary>
        public Encoding CurrentEncoding => _reader.CurrentEncoding;

        /// <summary>
        /// Returns if the current position is the EOF
        /// </summary>
        public bool EndOfStream => _reader.EndOfStream;

        /// <summary>
        /// The number of rows read (so far). Since we stream the data, this is not the Total count!
        /// </summary>
        public ulong RowCount => ROWS_READ;


        /// <summary>
        /// Initialize reader with a stream and other options
        /// </summary>
        /// <param name="stream">Stream to open the reader on</param>
        /// <param name="options">Options for the reader</param>
        public TokenLimitedFileReader(Stream stream, TokenLimitedFileOptions options)
        {
            _reader = new StreamReader(stream, options.AutoDetectEncoding ? null : options.TextEncoding, options.AutoDetectEncoding, options.BufferSize, options.LeaveFileOrStreamOpen);
            _state = new ReaderState();
            _options = options;
        }

        /// <summary>
        /// Initialize reader with path to file and other options
        /// </summary>
        /// <param name="path">Path to file (absolute preferred)</param>
        /// <param name="options">Options for the reader</param>
        public TokenLimitedFileReader(string path, TokenLimitedFileOptions options)
        {
            options.TextEncoding ??= Encoding.UTF8;

            FileStreamOptions streamOptions = new FileStreamOptions()
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.Read,
                BufferSize = options.BufferSize
            };

            _reader = new StreamReader(path, options.TextEncoding, options.AutoDetectEncoding, streamOptions);
            _state = new ReaderState();
            _options = options;
        }

        /// <summary>
        /// Closes the stream, including the underlying stream. The stream is also disposed.
        /// No further operation must be attempted on the stream after this call.
        /// </summary>
        public void Close()
        {
            this.ThrowIfDisposed(this.isDisposed, nameof(TokenLimitedFileReader));

            _state = new ReaderState();
            _reader.Close();
            Dispose();
        }

        /// <summary>
        /// Sets the maximum number of fields expected for row.
        /// </summary>
        /// <param name="numberOfFields">Number of expected fields</param>
        /// <remarks>
        ///     This only controls the length of the array returned by FIRST call to <see cref="M:SujaySarma.Data.Files.TokenLimitedFiles.TokenLimitedFileReader.ReadRow" /> and <see cref="M:SujaySarma.Data.Files.TokenLimitedFiles.TokenLimitedFileReader.ReadRowAsync" /> methods.
        ///     All fields on a row will be returned regardless of this value. Subsequent reads of longer rows will cause the internal
        ///     maximum field counter to be incremented accordingly.
        /// </remarks>
        public void SetExpectedRowSize(int numberOfFields)
        {
            if (numberOfFields <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfFields), "parameter must be a positive integer.");
            }

            if (numberOfFields < _state.MaximumFieldsRead)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfFields), "Must be called before the first call to ReadRow or ReadRowAsync methods.");
            }

            _state.MaximumFieldsRead = numberOfFields;
        }


        /// <summary>
        /// The double-quote (") character
        /// </summary>
        private const char DOUBLE_QUOTE = '"';

        /// <summary>
        /// The carriage return character (\r)
        /// </summary>
        private const char CARRIAGE_RETURN = '\r';

        /// <summary>
        /// The linefeed return character (\n)
        /// </summary>
        private const char LINE_FEED = '\n';

        /// <summary>
        /// Total number of rows read by this instance of the reader
        /// </summary>
        private ulong ROWS_READ;

        /// <summary>
        /// Options for this reader instance
        /// </summary>
        private readonly TokenLimitedFileOptions _options;

        /// <summary>
        /// The stream that we are writing the token-limited data into
        /// </summary>
        private readonly StreamReader _reader;

        /// <summary>
        /// Current state of this reader instance. Initialised in the constructor and reset in <see cref="M:SujaySarma.Data.Files.TokenLimitedFiles.TokenLimitedFileReader.Close" />.
        /// </summary>
        private TokenLimitedFileReader.ReaderState _state;

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                if (!_options.LeaveFileOrStreamOpen)
                {
                    _reader.Close();
                }
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The IDisposable flag
        /// </summary>
        private bool isDisposed;

        #endregion

    }
}
