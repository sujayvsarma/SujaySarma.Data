using System.Threading.Tasks;

using SujaySarma.Data.Core;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /*
        Implementations of different Write methods
    */
    public sealed partial class TokenLimitedFileWriter
    {

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
                    if (element.Contains(' '))
                    {
                        if (element[0] != QUOTE)
                        {
                            _writer.Write(QUOTE);
                        }

                        _writer.Write(element);

                        if (element[^1] != QUOTE)
                        {
                            _writer.Write(QUOTE);
                        }
                    }
                    else
                    {
                        _writer.Write(element);
                    }
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
                    if (element.Contains(' '))
                    {
                        if (element[0] != QUOTE)
                        {
                            await _writer.WriteAsync(QUOTE);
                        }

                        await _writer.WriteAsync(element);

                        if (element[^1] != QUOTE)
                        {
                            await _writer.WriteAsync(QUOTE);
                        }
                    }
                    else
                    {
                        await _writer.WriteAsync(element);
                    }
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
        /// The quote character as an interned constant.
        /// </summary>
        private static char QUOTE = '\"';

    }
}