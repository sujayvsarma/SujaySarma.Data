using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using SujaySarma.Data.Core;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /*
        Implementation of the row-reader and its helper functions
    */
    public sealed partial class TokenLimitedFileReader
    {

        /// <summary>
        /// Read and return a complete row.
        /// </summary>
        /// <returns>The row that was read. If no rows were read, it will be NULL.</returns>
        /// <remarks>
        ///     Each row of a token-limited flat-file may contain an irregular number of elements. Thus, each call to 
        ///     this function may return a different length of fields. HOWEVER, we will always return at least the 
        ///     number of elements as the previous read. 
        ///     
        ///     An "empty" field (or a field that has no value with just two consecutive delimiters (eg: ",,") is returned 
        ///     as a string.Empty and not as a NULL. A NULL is only returned if that field does not exist on the row.
        /// </remarks>
        public string?[]? ReadRow()
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileReader));

            if (this.EndOfStream)
            {
                return null;
            }

            _state.ResetFieldsForNewRow();
            while (! _reader.EndOfStream)
            {
                _state.CurrentCharacter = GetValidCharacterFromResult(_reader.Read());
                if (_state.CurrentCharacter == '\0')
                {
                    break;
                }

                _state.NextCharacter = GetValidCharacterFromResult(_reader.Peek());
                ProcessCharacterResult processCharacterResult = ProcessCharacter();
                if (processCharacterResult == ProcessCharacterResult.FinishRow)
                {
                    break;
                }

                if (processCharacterResult == ProcessCharacterResult.SkipNextCharacter)
                {
                    _reader.Read();
                }
            }

            // Take care of any data that is leftover
            FlushFieldBufferToRow();

            if (_state.ThisRowFieldsRead <= 0)
            {
                return null;
            }

            _state.SizeRowBufferForMaximumFieldsRead();

            ROWS_READ++;
            return _state.RowBuffer;
        }

        /// <summary>
        /// Read and return a complete row.
        /// </summary>
        /// <returns>The row that was read. If no rows were read, it will be NULL.</returns>
        /// <remarks>
        ///     Each row of a token-limited flat-file may contain an irregular number of elements. Thus, each call to 
        ///     this function may return a different length of fields. HOWEVER, we will always return at least the 
        ///     number of elements as the previous read. 
        ///     
        ///     An "empty" field (or a field that has no value with just two consecutive delimiters (eg: ",,") is returned 
        ///     as a string.Empty and not as a NULL. A NULL is only returned if that field does not exist on the row.
        /// </remarks>
        public async Task<string?[]?> ReadRowAsync()
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileReader));

            if (this.EndOfStream)
            {
                return null;
            }

            _state.ResetFieldsForNewRow();
            char[] buffer = new char[1];

            while (!_reader.EndOfStream)
            {
                buffer[0] = '\0';

                int count = await _reader.ReadAsync(buffer, 0, 1);
                if ((count == 0) || (buffer[0] == '\0'))
                {
                    break;
                }

                _state.CurrentCharacter = buffer[0];

                count = await _reader.ReadAsync(buffer, 0, 1);
                if ((count == 0) || (buffer[0] == '\0'))
                {
                    break;
                }

                _state.NextCharacter = buffer[0];

                ProcessCharacterResult processCharacterResult = ProcessCharacter();
                if (processCharacterResult == ProcessCharacterResult.FinishRow)
                {
                    break;
                }

                if (processCharacterResult == ProcessCharacterResult.SkipNextCharacter)
                {
                    await _reader.ReadAsync(buffer, 0, 1);
                }
            }

            // Take care of any data that is leftover
            FlushFieldBufferToRow();

            if (_state.ThisRowFieldsRead <= 0)
            {
                return null;
            }

            _state.SizeRowBufferForMaximumFieldsRead();

            ROWS_READ++;
            return _state.RowBuffer;
        }

        /// <summary>
        /// Sets the maximum number of fields expected for row.
        /// </summary>
        /// <param name="numberOfFields">Number of expected fields</param>
        /// <remarks>
        ///     This only controls the length of the array returned by FIRST call to <see cref="ReadRow"/> and <see cref="ReadRowAsync"/> methods. 
        ///     All fields on a row will be returned regardless of this value. Subsequent reads of longer rows will cause the internal 
        ///     maximum field counter to be incremented accordingly.
        /// </remarks>
        public void SetExpectedRowSize(int numberOfFields)
        {
            if (numberOfFields <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfFields), "parameter must be a positive integer.");
            }

            // If a positive count already exists, it means we have already gone through one or more Read loops.
            if (numberOfFields < _state.MaximumFieldsRead)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfFields), "Must be called before the first call to ReadRow or ReadRowAsync methods.");
            }

            _state.MaximumFieldsRead = numberOfFields;
        }

        /// <summary>
        /// Returns the char-casted value equivalent to the provided <paramref name="readResultValue"/>.
        /// </summary>
        /// <param name="readResultValue">The value returned by a <see cref="StreamReader"/>'s <see cref="StreamReader.Read()"/></param>
        /// <returns>Character equivalent</returns>
        private static char GetValidCharacterFromResult(int readResultValue)
        {
            if (readResultValue < 0)
            {
                readResultValue = 0;
            }

            return (char)readResultValue;
        }


        /// <summary>
        /// Process the current character (in _state.CurrentCharacter)
        /// </summary>
        /// <returns>A result from <see cref="ProcessCharacterResult"/> indicating how the caller should proceed after this call</returns>
        private ProcessCharacterResult ProcessCharacter()
        {
            ProcessCharacterResult result = ProcessCharacterResult.ContinueNormalRead;

            switch (_state.CurrentCharacter)
            {
                case DOUBLE_QUOTE:
                    if (_state.NextCharacter == DOUBLE_QUOTE)
                    {
                        // Escaped quote.
                        // Add to field buffer and skip the peeked character.
                        _state.FieldBuffer.Append('"');
                        _state.ThisFieldQuoteCount++;

                        result = ProcessCharacterResult.SkipNextCharacter;
                    }

                    _state.ThisFieldQuoteCount++;
                    break;

                case CARRIAGE_RETURN or LINE_FEED:
                    if ((_state.CurrentCharacter == CARRIAGE_RETURN) && (_state.NextCharacter == LINE_FEED))
                    {
                        // Don't want the next char, we don't read CRLF into the buffer!
                        result = ProcessCharacterResult.SkipNextCharacter;
                    }

                    if (_state.HasMatchedQuotes)
                    {
                        // End of line with matched quotes. We have a complete field and "row"!
                        FlushFieldBufferToRow();

                        // Only breakout of read-loop if we have a non-empty row
                        if (_state.ThisRowFieldsRead > -1)
                        {
                            result = ProcessCharacterResult.FinishRow;
                        }
                    }
                    break;

                default:
                    if ((_state.CurrentCharacter == Delimiter) && (!_state.HasMatchedQuotes))
                    {
                        // Delimiter + Matching quotes in buffer, field is complete!
                        FlushFieldBufferToRow();
                    }
                    else
                    {
                        // Everything else gets appended
                        _state.FieldBuffer.Append(_state.CurrentCharacter);
                    }
                    break;
            }

            return result;
        }



        /// <summary>
        /// Flush the contents of the field buffer to the row buffer
        /// </summary>
        private void FlushFieldBufferToRow()
        {
            if (! _state.FieldBufferIsMeaningful)
            {
                return;
            }

            string fieldContent = _state.FieldBuffer.ToString();
            _state.ResetFieldsForNewField();

            bool isFieldContentEmpty = string.IsNullOrWhiteSpace(fieldContent);
            if ((_state.ThisRowFieldsRead <= 0) && isFieldContentEmpty)
            {
                return;
            }

            _state.ThisRowFieldsRead++;
            _state.SizeRowBufferForMaximumFieldsRead();

            if (isFieldContentEmpty)
            {
                _state.RowBuffer[_state.ThisRowFieldsRead] = null;
            }
            else if (fieldContent == "\"")
            {
                // case when the TSV has something like a [,"",] where a quoted string field is empty
                // we would end up adding an unmatched quote mark to our builder
                _state.RowBuffer[_state.ThisRowFieldsRead] = string.Empty;
            }
            else
            {
                _state.RowBuffer[_state.ThisRowFieldsRead] = fieldContent;
            }

            if (_state.ThisRowFieldsRead > _state.MaximumFieldsRead)
            {
                _state.MaximumFieldsRead = _state.ThisRowFieldsRead;
            }
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
        /// Current state of this reader instance. Initialised in the constructor and reset in <see cref="Close">.
        /// </summary>
        private ReaderState _state;

        /// <summary>
        /// A portable state of the system during ReadRow operations
        /// </summary>
        private struct ReaderState
        {
            /// <summary>
            /// Number of fields read in the current row
            /// </summary>
            public int ThisRowFieldsRead 
            { 
                get; set; 
            }

            /// <summary>
            /// Maximum number of fields we have read on any row
            /// </summary>
            public int MaximumFieldsRead
            {
                get; set;
            }

            /// <summary>
            /// The current character (already read)
            /// </summary>
            public char CurrentCharacter
            {
                get; set;
            }

            /// <summary>
            /// Next character in the buffer (Peeked)
            /// </summary>
            public char NextCharacter
            {
                get; set;
            }

            /// <summary>
            /// Number of quote marks we have crossed in the current field
            /// </summary>
            public int ThisFieldQuoteCount
            {
                get; set;
            }

            /// <summary>
            /// Returns if the <see cref="ThisFieldQuoteCount"/> is an even number
            /// </summary>
            public readonly bool HasMatchedQuotes
                => (((ThisFieldQuoteCount % 2) == 0) ? true : false);

            /// <summary>
            /// Current field buffer
            /// </summary>
            public StringBuilder FieldBuffer
            {
                get; set;
            }

            /// <summary>
            /// Current row buffer
            /// </summary>
            public string?[] RowBuffer
            {
                get; set;
            }

            /// <summary>
            /// The read-loops set this value when the begin filling 
            /// the field buffer. This should be cleared when the 
            /// field buffer has been flushed to the row.
            /// </summary>
            public bool FieldBufferIsMeaningful
            {
                get; set;
            }


            /// <summary>
            /// Initialise
            /// </summary>
            public ReaderState()
            {
                ThisRowFieldsRead = 0;
                MaximumFieldsRead = 0;
                ThisFieldQuoteCount = 0;
                CurrentCharacter = '\0';
                NextCharacter = '\0';

                FieldBuffer = new StringBuilder();

                // "24" is an arbitrary number. Most TSFF will have lesser than this. 
                // If it is more, we always adjust
                RowBuffer = new string[24];
                FieldBufferIsMeaningful = false;
            }

            /// <summary>
            /// Reset the <see cref="FieldBuffer"/> field.
            /// </summary>
            public void ResetFieldsForNewField()
            {
                FieldBuffer.Clear();
                FieldBufferIsMeaningful = false;
                ThisFieldQuoteCount = 0;
                CurrentCharacter = '\0';
                NextCharacter = '\0';
            }

            /// <summary>
            /// Reset the <see cref="RowBuffer"/> field. All elements are initialised to NULL.
            /// This will automatically size the array to the value of <see cref="MaximumFieldsRead"/>.
            /// </summary>
            public void ResetFieldsForNewRow()
            {
                RowBuffer = new string[MaximumFieldsRead];
                ThisRowFieldsRead = 0;

                for (int i = 0; i < MaximumFieldsRead; i++)
                {
                    RowBuffer[i] = null;
                }

                ResetFieldsForNewField();
            }


            /// <summary>
            /// Size the <see cref="RowBuffer"/> to the length of the <see cref="MaximumFieldsRead"/>
            /// </summary>
            public void SizeRowBufferForMaximumFieldsRead()
            {
                if (RowBuffer.Length != MaximumFieldsRead)
                {
                    string?[] temp = RowBuffer;
                    Array.Resize(ref temp, MaximumFieldsRead);
                    RowBuffer = temp;
                }

                if (RowBuffer.Length > ThisRowFieldsRead)
                {
                    for (int i = ThisRowFieldsRead; i < RowBuffer.Length; i++)
                    {
                        RowBuffer[i] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Result returned by <see cref="ProcessCharacter"/>
        /// </summary>
        private enum ProcessCharacterResult
        {
            /// <summary>
            /// Continue and read the next character as normal
            /// </summary>
            ContinueNormalRead = 0,

            /// <summary>
            /// Skip the next character 
            /// (it is something we don't want to, or have already processed)
            /// </summary>
            SkipNextCharacter,

            /// <summary>
            /// The row is "complete", no fields left to be read.
            /// </summary>
            FinishRow
        }
    }
}