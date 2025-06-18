using System;
using System.Text;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    // The ReaderState class
    public sealed partial class TokenLimitedFileReader
    {

        /// <summary>
        /// A portable state of the system during ReadRow operations
        /// </summary>
        private struct ReaderState
        {
            /// <summary>
            /// Number of fields read in the current row
            /// </summary>
            public int ThisRowFieldsRead { get; set; }

            /// <summary>
            /// Maximum number of fields we have read on any row
            /// </summary>
            public int MaximumFieldsRead { get; set; }

            /// <summary>
            /// The current character (already read)
            /// </summary>
            public char CurrentCharacter { get; set; }

            /// <summary>
            /// Next character in the buffer (Peeked)
            /// </summary>
            public char NextCharacter { get; set; }

            /// <summary>
            /// Number of quote marks we have crossed in the current field
            /// </summary>
            public int ThisFieldQuoteCount { get; set; }

            /// <summary>
            /// Returns if the <see cref="ThisFieldQuoteCount" /> is an even number
            /// </summary>
            public readonly bool HasMatchedQuotes => (((ThisFieldQuoteCount % 2) == 0) ? true : false);

            /// <summary>
            /// Current field buffer
            /// </summary>
            public StringBuilder FieldBuffer { get; set; }

            /// <summary>
            /// Current row buffer
            /// </summary>
            public string?[] RowBuffer { get; set; }

            /// <summary>
            /// The read-loops set this value when the begin filling
            /// the field buffer. This should be cleared when the
            /// field buffer has been flushed to the row.
            /// </summary>
            public bool FieldBufferIsMeaningful { get; set; }

            /// <summary>Initialise</summary>
            public ReaderState()
            {
                ThisRowFieldsRead = 0;
                MaximumFieldsRead = 0;
                ThisFieldQuoteCount = 0;
                CurrentCharacter = char.MinValue;
                NextCharacter = char.MinValue;
                FieldBuffer = new StringBuilder();
                RowBuffer = new string[24];
                FieldBufferIsMeaningful = false;
            }

            /// <summary>
            /// Reset the <see cref="FieldBuffer" /> field.
            /// </summary>
            public void ResetFieldsForNewField()
            {
                FieldBuffer.Clear();
                FieldBufferIsMeaningful = false;
                ThisFieldQuoteCount = 0;
                CurrentCharacter = char.MinValue;
                NextCharacter = char.MinValue;
            }

            /// <summary>
            /// Reset the <see cref="RowBuffer" /> field. All elements are initialised to NULL.
            /// This will automatically size the array to the value of <see cref="MaximumFieldsRead" />.
            /// </summary>
            public void ResetFieldsForNewRow()
            {
                RowBuffer = new string[MaximumFieldsRead];
                ThisRowFieldsRead = 0;
                for (int index = 0; index < MaximumFieldsRead; ++index)
                {
                    RowBuffer[index] = null;
                }

                ResetFieldsForNewField();
            }

            /// <summary>
            /// Size the <see cref="RowBuffer" /> to the length of the <see cref="MaximumFieldsRead" />
            /// </summary>
            public void SizeRowBufferForMaximumFieldsRead()
            {
                if (RowBuffer.Length != MaximumFieldsRead)
                {
                    string?[] rowBuffer = RowBuffer;
                    Array.Resize<string?>(ref rowBuffer, MaximumFieldsRead);
                    RowBuffer = rowBuffer;
                }
                if (RowBuffer.Length <= ThisRowFieldsRead)
                {
                    return;
                }

                for (int thisRowFieldsRead = ThisRowFieldsRead; thisRowFieldsRead < RowBuffer.Length; ++thisRowFieldsRead)
                {
                    RowBuffer[thisRowFieldsRead] = null;
                }
            }
        }

    }
}
