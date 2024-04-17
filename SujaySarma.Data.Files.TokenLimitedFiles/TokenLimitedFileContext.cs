using System.IO;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /// <summary>
    /// Perform contextual read/write operations against flatfiles
    /// </summary>
    public partial class TokenLimitedFileContext
    {

        /// <summary>
        /// Adds a flatfile writer to the context
        /// </summary>
        /// <param name="path">Path to the flatfile</param>
        /// <param name="options">Options for the writer</param>
        /// <returns>Reference to the FileContext</returns>
        public TokenLimitedFileContext AddWriter(string path, TokenLimitedFileOptions options)
        {
            this._writer = new TokenLimitedFileWriter(path, options);

            return this;
        }

        /// <summary>
        /// Adds a flatfile writer to the context
        /// </summary>
        /// <param name="stream">A writable stream opened to the path to the flatfile</param>
        /// <param name="options">Options for the writer</param>
        /// <returns>Reference to the FileContext</returns>
        public TokenLimitedFileContext AddWriter(Stream stream, TokenLimitedFileOptions options)
        {
            this._writer = new TokenLimitedFileWriter(stream, options);

            return this;
        }


        /// <summary>
        /// Adds a flatfile reader to the context
        /// </summary>
        /// <param name="path">Path to the flatfile</param>
        /// <param name="options">Options for the reader</param>
        /// <returns>Reference to the FileContext</returns>
        public TokenLimitedFileContext AddReader(string path, TokenLimitedFileOptions options)
        {
            this._reader = new TokenLimitedFileReader(path, options);

            return this;
        }

        /// <summary>
        /// Adds a flatfile reader to the context
        /// </summary>
        /// <param name="stream">A readable stream opened to the path to the flatfile</param>
        /// <param name="options">Options for the reader</param>
        /// <returns>Reference to the FileContext</returns>
        public TokenLimitedFileContext AddReader(Stream stream, TokenLimitedFileOptions options)
        {
            this._reader = new TokenLimitedFileReader(stream, options);

            return this;
        }


        /// <summary>
        /// Return a context that can be used for writing flatfiles
        /// </summary>
        /// <param name="path">Path to the flatfile</param>
        /// <param name="options">Options for the writer</param>
        /// <returns>Reference to the FileContext</returns>
        public static TokenLimitedFileContext ForWriting(string path, TokenLimitedFileOptions options)
        {
            TokenLimitedFileContext context = new TokenLimitedFileContext();
            context._writer = new TokenLimitedFileWriter(path, options);

            return context;
        }

        /// <summary>
        /// Return a context that can be used for writing flatfiles
        /// </summary>
        /// <param name="stream">A writable stream opened to the path to the flatfile</param>
        /// <param name="options">Options for the writer</param>
        /// <returns>Reference to the FileContext</returns>
        public static TokenLimitedFileContext ForWriting(Stream stream, TokenLimitedFileOptions options)
        {
            TokenLimitedFileContext context = new TokenLimitedFileContext();
            context._writer = new TokenLimitedFileWriter(stream, options);

            return context;
        }


        /// <summary>
        /// Return a context that can be used for reading flatfiles
        /// </summary>
        /// <param name="path">Path to the flatfile</param>
        /// <param name="options">Options for the reader</param>
        /// <returns>Reference to the FileContext</returns>
        public static TokenLimitedFileContext ForReading(string path, TokenLimitedFileOptions options)
        {
            TokenLimitedFileContext context = new TokenLimitedFileContext();
            context._reader = new TokenLimitedFileReader(path, options);

            return context;
        }

        /// <summary>
        /// Return a context that can be used for reading flatfiles
        /// </summary>
        /// <param name="stream">A readable stream opened to the path to the flatfile</param>
        /// <param name="options">Options for the reader</param>
        /// <returns>Reference to the FileContext</returns>
        public static TokenLimitedFileContext ForReading(Stream stream, TokenLimitedFileOptions options)
        {
            TokenLimitedFileContext context = new TokenLimitedFileContext();
            context._reader = new TokenLimitedFileReader(stream, options);

            return context;
        }


        /// <summary>
        /// Initialise with defaults
        /// </summary>
        private TokenLimitedFileContext()
        {
        }

        /// <summary>
        /// Reference to the flatfile writer
        /// </summary>
        private TokenLimitedFileWriter? _writer = null;

        /// <summary>
        /// Reference to the flatfile reader
        /// </summary>
        private TokenLimitedFileReader? _reader = null;
    }
}
