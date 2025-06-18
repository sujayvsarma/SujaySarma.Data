using SujaySarma.Data.Core;

using System.Data;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    // Private helper methods
    public sealed partial class TokenLimitedFileReader
    {

        /// <summary>
        /// Flush the contents of the field buffer to the row buffer
        /// </summary>
        private void FlushFieldBufferToRow()
        {
            if (_state.FieldBufferIsMeaningful)
            {
                // There is nothing useful to flush from the buffer.
                return;
            }

            string str = _state.FieldBuffer.ToString();
            _state.ResetFieldsForNewField();

            bool flag = string.IsNullOrWhiteSpace(str);
            if ((_state.ThisRowFieldsRead <= 0) && flag)
            {
                return;
            }

            ++_state.ThisRowFieldsRead;
            _state.SizeRowBufferForMaximumFieldsRead();

            if (!flag)
            {
                if (str == "\"")
                {
                    _state.RowBuffer[_state.ThisRowFieldsRead] = string.Empty;
                }
                else
                {
                    _state.RowBuffer[_state.ThisRowFieldsRead] = str;
                }
            }
            else
            {
                _state.RowBuffer[_state.ThisRowFieldsRead] = null;
            }

            if (_state.ThisRowFieldsRead <= _state.MaximumFieldsRead)
            {
                return;
            }

            _state.MaximumFieldsRead = _state.ThisRowFieldsRead;
        }


        /// <summary>
        /// Returns the char-casted value equivalent to the provided <paramref name="readResultValue" />.
        /// </summary>
        /// <param name="readResultValue">The value returned by a <see cref="System.IO.StreamReader" />'s <see cref="System.IO.StreamReader.Read()" /></param>
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
        /// <returns>A result from <see cref="ProcessCharacterResult" /> indicating how the caller should proceed after this call</returns>
        private ProcessCharacterResult ProcessCharacter()
        {
            ProcessCharacterResult processCharacterResult = ProcessCharacterResult.ContinueNormalRead;
            switch (_state.CurrentCharacter)
            {
                case LINE_FEED:
                case CARRIAGE_RETURN:
                    // Linux: LF (0x0A, "\n"), MSDOS/Windows: CR+LF ("\r\n"), Legacy Mac (pre-X): CR (0x0D, "\r")
                    // We need to look at Current+Next only for Windows. The other two OSes use only a single character to push to next line.
                    if ((_state.CurrentCharacter == CARRIAGE_RETURN) && (_state.NextCharacter == LINE_FEED))
                    {
                        // If we have a CR+LF, we need to skip the next character (which is LF)
                        processCharacterResult = ProcessCharacterResult.SkipNextCharacter;
                    }

                    if (_state.HasMatchedQuotes)
                    {
                        FlushFieldBufferToRow();
                        if (_state.ThisRowFieldsRead > -1)
                        {
                            processCharacterResult = ProcessCharacterResult.FinishRow;
                        }
                    }
                    break;

                case DOUBLE_QUOTE:
                    if (_state.NextCharacter == DOUBLE_QUOTE)
                    {
                        _state.FieldBuffer.Append(DOUBLE_QUOTE);
                        ++_state.ThisFieldQuoteCount;
                        processCharacterResult = ProcessCharacterResult.SkipNextCharacter;
                    }

                    ++_state.ThisFieldQuoteCount;
                    break;

                default:
                    if (((int)_state.CurrentCharacter == (int)Delimiter) && (!_state.HasMatchedQuotes))
                    {
                        FlushFieldBufferToRow();
                    }
                    else
                    {
                        _state.FieldBuffer.Append(_state.CurrentCharacter);
                    }

                    break;
            }

            return processCharacterResult;
        }

        /// <summary>
        /// Process the row that was read and populate the table with its data
        /// </summary>
        /// <param name="fileRow">String array read from file</param>
        /// <param name="table">DataTable instance to populate with the row or header information</param>
        /// <param name="options">Copy of the <see cref="TokenLimitedFileOptions" /> as being used by the active <see cref="TokenLimitedFileReader" /></param>
        /// <param name="currentRowCount">Number of rows ALREADY processed</param>
        private static void ProcessReadRow(string?[]? fileRow, DataTable table, TokenLimitedFileOptions options, ulong currentRowCount)
        {
            if (fileRow == null)
            {
                return;
            }

            if (options.HasHeaderRow && (currentRowCount == options.HeaderRowIndex))
            {
                for (int index = 0; index < fileRow.Length; ++index)
                {
                    DataColumnCollection columns = table.Columns;
                    string columnName;

                    if (!string.IsNullOrWhiteSpace(fileRow[index]))
                    {
                        columnName = fileRow[index]!;
                    }
                    else
                    {
                        columnName = $"Column {index}";
                    }

                    columns.Add(columnName, typeof(string));
                }
            }
            else
            {
                DataRow row = table.NewRow();
                for (int columnIndex = 0; columnIndex < fileRow.Length; ++columnIndex)
                {
                    try
                    {
                        row[columnIndex] = ReflectionUtils.ConvertValueIfRequired(fileRow[0], typeof(string));
                    }
                    catch
                    {
                        row[columnIndex] = fileRow[columnIndex]?.ToString();
                    }
                }
                table.Rows.Add(row);
            }
        }


    }
}
