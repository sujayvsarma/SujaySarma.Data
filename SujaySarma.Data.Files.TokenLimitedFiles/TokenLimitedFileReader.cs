using System;
using System.IO;
using System.Text;

using SujaySarma.Data.Core;

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
        public char Delimiter 
        {
            get => _options.Delimiter;
        
        }

        /// <summary>
        /// Returns the current text encoding being used
        /// </summary>
        public Encoding CurrentEncoding 
        { 
            get => _reader.CurrentEncoding; 
        }

        /// <summary>
        /// Returns if the current position is the EOF
        /// </summary>
        public bool EndOfStream 
        { 
            get => _reader.EndOfStream;
        }

        /// <summary>
        /// The number of rows read (so far). Since we stream the data, this is not the Total count!
        /// </summary>
        public ulong RowCount 
        { 
            get => ROWS_READ; 
        }

        /// <summary>
        /// Closes the stream, including the underlying stream. The stream is also disposed. 
        /// No further operation must be attempted on the stream after this call.
        /// </summary>
        public void Close()
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileReader));
            _state = new ReaderState();
            _reader.Close();
            Dispose();
        }


        /// <summary>
        /// Initialize reader with a stream and other options
        /// </summary>
        /// <param name="stream">Stream to open the reader on</param>
        /// <param name="options">Options for the reader</param>
        public TokenLimitedFileReader(Stream stream, TokenLimitedFileOptions options)
        {
            _reader = new StreamReader(stream, options.TextEncoding, options.AutoDetectEncoding, options.BufferSize, options.LeaveFileOrStreamOpen);

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
            if (options.TextEncoding == default) { options.TextEncoding = Encoding.UTF8; }
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
        /// Total number of rows read by this instance of the reader
        /// </summary>
        private ulong ROWS_READ = 0;

        /// <summary>
        /// Options for this reader instance
        /// </summary>
        private readonly TokenLimitedFileOptions _options;

        /// <summary>
        /// The stream that we are writing the token-limited data into
        /// </summary>
        private readonly StreamReader _reader = default!;

        // IDisposable
        /// <inheritdoc/>
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                if (! _options.LeaveFileOrStreamOpen)
                {
                    _reader.Close();
                }
            }

            GC.SuppressFinalize(this);
        }
        private bool isDisposed = false;
    }
}