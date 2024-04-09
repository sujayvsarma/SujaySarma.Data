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
            get; init; 
        
        } = ',';

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
        /// <param name="encoding">Specific encoding. NULL to auto-detect</param>
        /// <param name="autoDetectEncoding">If set, auto-detects</param>
        /// <param name="bufferSize">Minimum stream buffer size</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        public TokenLimitedFileReader(Stream stream, Encoding? encoding = default, bool autoDetectEncoding = true, int bufferSize = -1, bool leaveStreamOpen = false)
        {
            _reader = new(stream, encoding, autoDetectEncoding, bufferSize, leaveStreamOpen);
            _leaveStreamOpenOnDispose = leaveStreamOpen;
            _state = new ReaderState();
        }

        /// <summary>
        /// Initialize reader with path to file and other options
        /// </summary>
        /// <param name="path">Path to file (absolute preferred)</param>
        /// <param name="encoding">Specific encoding. NULL for autodetect</param>
        /// <param name="autoDetectEncoding">If set, auto-detects</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        public TokenLimitedFileReader(string path, Encoding? encoding = default, bool autoDetectEncoding = true, bool leaveStreamOpen = false)
        {
            if (encoding == default) { encoding = Encoding.UTF8; }
            FileStreamOptions options = new()
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Options = FileOptions.SequentialScan,
                Share = FileShare.Read,
                BufferSize = 4096
            };

            _reader = new(path, encoding, autoDetectEncoding, options);
            _leaveStreamOpenOnDispose = leaveStreamOpen;
            _state = new ReaderState();
        }

        /// <summary>
        /// Total number of rows read by this instance of the reader
        /// </summary>
        private ulong ROWS_READ = 0;

        /// <summary>
        /// When set, we do not close the '_reader' stream when we are disposed. 
        /// This should be set to TRUE if the stream is shared by other "readers" (eg: in a Http Pipeline)
        /// </summary>
        private readonly bool _leaveStreamOpenOnDispose = false;

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

                if (!_leaveStreamOpenOnDispose)
                {
                    _reader.Close();
                }
            }

            GC.SuppressFinalize(this);
        }
        private bool isDisposed = false;
    }
}