namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /// <summary>
    /// Perform contextual read/write operations against flatfiles
    /// </summary>
    public partial class TokenLimitedFileContext
    {

        /// <summary>
        /// Initialise with defaults
        /// </summary>
        private TokenLimitedFileContext()
        {
        }

        /// <summary>
        /// Reference to the flatfile writer
        /// </summary>
        private TokenLimitedFileWriter? _writer;

        /// <summary>
        /// Reference to the flatfile reader
        /// </summary>
        private TokenLimitedFileReader? _reader;
    }
}
