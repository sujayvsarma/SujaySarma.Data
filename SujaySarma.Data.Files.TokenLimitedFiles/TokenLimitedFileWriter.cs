using System;
using System.IO;
using System.Text;

using SujaySarma.Data.Core;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /// <summary>
    /// Writes token-limited flat-files. Default token is the comma (',').
    /// </summary>
    public sealed partial class TokenLimitedFileWriter : IDisposable
    {
        /// <summary>
        /// Field delimiter. Default is comma (',').
        /// </summary>
        public char Delimiter 
        {
            get => _options.Delimiter;
        }

        /// <summary>
        /// Returns the text encoding being used
        /// </summary>
        public Encoding Encoding 
        { 
            get => _writer.Encoding; 
        }

        /// <summary>
        /// Get/set if the stream automatically flushes written data to the backing file
        /// </summary>
        public bool AutoFlush 
        { 
            get => _writer.AutoFlush; 
            init => _writer.AutoFlush = value; 
        }

        /// <summary>
        /// New line character used by the writer
        /// </summary>
        public string NewLine 
        { 
            get => _writer.NewLine; 
            init => _writer.NewLine = value; 
        }


        /// <summary>
        /// The number of rows actually written (so far). Since we stream the data, this is not the Total count!
        /// </summary>
        public ulong RowCount 
        { 
            get => ROWS_WRITTEN; 
        }

        /// <summary>
        /// Closes the stream, including the underlying stream. The stream is also disposed. 
        /// No further operation must be attempted on the stream after this call.
        /// </summary>
        public void Close()
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileWriter));
            Dispose();
        }

        /// <summary>
        /// Initialize writer with a stream and other options
        /// </summary>
        /// <param name="stream">Stream to open the writer on</param>
        /// <param name="options">Options for the writer</param>
        public TokenLimitedFileWriter(Stream stream, TokenLimitedFileOptions options)
        {
            _writer = new StreamWriter(stream, options.TextEncoding, options.BufferSize, options.LeaveFileOrStreamOpen);
            _options = options;
        }

        /// <summary>
        /// Initialize writer with path to file and other options
        /// </summary>
        /// <param name="path">Path to file (absolute preferred)</param>
        /// <param name="options">Options for the writer</param>
        public TokenLimitedFileWriter(string path, TokenLimitedFileOptions options)
        {
            FileStreamOptions streamOptions = new FileStreamOptions()
            {
                Access = FileAccess.Write,
                Mode = FileMode.Create,
                Options = FileOptions.None,
                Share = FileShare.Read,
                BufferSize = options.BufferSize
            };

            _writer = new StreamWriter(path, options.TextEncoding, streamOptions);
            _options = options;
        }

        /// <summary>
        /// Number of rows written to the flatfile
        /// </summary>
        private ulong ROWS_WRITTEN = 0;

        /// <summary>
        /// Options for this writer instance
        /// </summary>
        private readonly TokenLimitedFileOptions _options;

        /// <summary>
        /// The stream that we are writing the token-limited data into
        /// </summary>
        private readonly StreamWriter _writer = default!;

        // IDisposable
        /// <inheritdoc/>
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                if (! _options.LeaveFileOrStreamOpen)
                {
                    _writer.Close();
                }
            }

            GC.SuppressFinalize(this);
        }
        private bool isDisposed = false;

    }
}
