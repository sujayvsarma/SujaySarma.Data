using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
            get; init; 

        } = ',';

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
        /// Write a complete row
        /// </summary>
        /// <param name="row">A single row of data</param>
        public void Write(string?[]? row)
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileWriter));
            if (row == default)
            {
                WriteNewLine();
                return;
            }

            for (int h = 0; h < row.Length; h++)
            {
                string? element = row[h];
                if (element != null)
                {
                    _writer.Write(element);
                }

                if (h < (row.Length - 1))
                {
                    _writer.Write(Delimiter);
                }
            }
            WriteNewLine();

            ROWS_WRITTEN++;
        }

        /// <summary>
        /// Write a complete row
        /// </summary>
        /// <param name="row">A single row of data</param>
        public async Task WriteAsync(string?[]? row)
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileWriter));
            if (row == default)
            {
                await WriteNewLineAsync();
                return;
            }

            for (int h = 0; h < row.Length; h++)
            {
                string? element = row[h];
                if (element != null)
                {
                    await _writer.WriteAsync(element);
                }

                if (h < (row.Length - 1))
                {
                    await _writer.WriteAsync(Delimiter);
                }
            }
            await WriteNewLineAsync();

            ROWS_WRITTEN++;
        }

        /// <summary>
        /// Write a complete row
        /// </summary>
        /// <param name="row">A string of arbitrary information</param>
        public void Write(string? row)
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileWriter));
            if (row == default)
            {
                WriteNewLine();
                return;
            }

            _writer.Write(row);
            WriteNewLine();

            ROWS_WRITTEN++;
        }

        /// <summary>
        /// Write a complete row
        /// </summary>
        /// <param name="row">A string of arbitrary information</param>
        public async Task WriteAsync(string? row)
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileWriter));
            if (row == default)
            {
                await WriteNewLineAsync();
                return;
            }

            await _writer.WriteAsync(row);
            await WriteNewLineAsync();

            ROWS_WRITTEN++;
        }



        /// <summary>
        /// Write a newline to the stream
        /// </summary>
        public void WriteNewLine() 
            => _writer.WriteLine();

        /// <summary>
        /// Write a newline to the stream
        /// </summary>
        public async Task WriteNewLineAsync()
            => await _writer.WriteLineAsync();


        /// <summary>
        /// Initialize writer with a stream and other options
        /// </summary>
        /// <param name="stream">Stream to open the writer on</param>
        /// <param name="encoding">Specific encoding to use</param>
        /// <param name="bufferSize">Minimum stream buffer size</param>
        /// <param name="leaveStreamOpen">Set to prevent closing the stream when this writer is disposed</param>
        public TokenLimitedFileWriter(Stream stream, Encoding? encoding = default, int bufferSize = -1, bool leaveStreamOpen = false)
        {
            _writer = new StreamWriter(stream, encoding, bufferSize, leaveStreamOpen);
            _leaveStreamOpenOnDispose = leaveStreamOpen;
        }

        /// <summary>
        /// Initialize writer with path to file and other options
        /// </summary>
        /// <param name="path">Path to file (absolute preferred)</param>
        /// <param name="encoding">Specific encoding. NULL for autodetect</param>
        /// <param name="leaveStreamOpen">Set to prevent closing the stream when this writer is disposed</param>
        public TokenLimitedFileWriter(string path, Encoding? encoding = default, bool leaveStreamOpen = false)
        {
            if (encoding == default) { encoding = Encoding.UTF8; }
            FileStreamOptions options = new()
            {
                Access = FileAccess.Write,
                Mode = FileMode.Create,
                Options = FileOptions.None,
                Share = FileShare.Read,
                BufferSize = 4096
            };

            _writer = new StreamWriter(path, encoding, options);
            _leaveStreamOpenOnDispose = leaveStreamOpen;
        }

        /// <summary>
        /// Number of rows written to the flatfile
        /// </summary>
        private ulong ROWS_WRITTEN = 0;

        /// <summary>
        /// When set, we do not close the '_writer' stream when we are disposed. 
        /// This should be set to TRUE if the stream is shared by other "writers" (eg: in a Http Pipeline)
        /// </summary>
        private readonly bool _leaveStreamOpenOnDispose = false;

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

                if (!_leaveStreamOpenOnDispose)
                {
                    _writer.Close();
                }
            }

            GC.SuppressFinalize(this);
        }
        private bool isDisposed = false;

    }
}
