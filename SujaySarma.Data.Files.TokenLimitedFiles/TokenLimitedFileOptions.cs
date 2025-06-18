using System.Text;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /// <summary>
    /// Options to control how a token-limited flatfile is dealt with either
    /// during reading or writing.
    /// </summary>
    public class TokenLimitedFileOptions
    {
        /// <summary>
        /// File encoding
        /// </summary>
        public Encoding TextEncoding { get; set; }

        /// <summary>
        /// When set, ignores the value set for <see cref="TextEncoding" /> and
        /// attempts to automatically detect the encoding being used.
        /// </summary>
        public bool AutoDetectEncoding { get; set; }

        /// <summary>
        /// If the flatfile has or must have a header row, containing
        /// the column names. If true, also set <see cref="HeaderRowIndex" />.
        /// </summary>
        public bool HasHeaderRow { get; set; }

        /// <summary>
        /// One-based index from the start of the file where the
        /// header-row containing column names may be found. The
        /// first line of the file is Line 1. Only valid if
        /// <see cref="HasHeaderRow" /> is true.
        /// </summary>
        public ulong HeaderRowIndex { get; set; }

        /// <summary>
        /// Buffer size to use. Set to -1 to have no preference.
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// When set, after our calls are complete, a call to Close the file
        /// or stream will not result in the underlying file/stream being closed.
        /// This is useful when multiple actors are using the same stream for example,
        /// in a Http Request or Response Pipeline.
        /// </summary>
        public bool LeaveFileOrStreamOpen { get; set; }

        /// <summary>Field-delimiting token. Default is a comma.</summary>
        public char Delimiter { get; set; }

        /// <summary>
        /// If set, all strings are quoted. Otherwise, only strings that
        /// have the <see cref="Delimiter" /> enclosed will be quoted.
        /// </summary>
        public bool ForceQuoteStrings { get; set; }

        /// <summary>
        /// Initialise
        /// </summary>
        public TokenLimitedFileOptions()
        {
            TextEncoding = Encoding.UTF8;
            AutoDetectEncoding = false;
            HasHeaderRow = true;
            HeaderRowIndex = 1UL;
            BufferSize = -1;
            LeaveFileOrStreamOpen = true;
            Delimiter = ',';
            ForceQuoteStrings = true;
        }
    }

}
