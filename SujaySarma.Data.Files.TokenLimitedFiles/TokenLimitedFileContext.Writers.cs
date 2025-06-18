using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    // Writers-specific methods
    public partial class TokenLimitedFileContext
    {


        /// <summary>
        /// Write records from the provided <paramref name="table" />
        /// </summary>
        /// <param name="table">A <see cref="DataTable" /> containing records to write</param>
        public void Write(DataTable table)
        {
            EnsureHasWriter();
            TokenLimitedFileWriter.WriteTable(_writer!, table);
        }

        /// <summary>
        /// Write records from the provided <paramref name="table" />
        /// </summary>
        /// <param name="table">A <see cref="DataTable" /> containing records to write</param>
        public async Task WriteAsync(DataTable table)
        {
            EnsureHasWriter();
            await TokenLimitedFileWriter.WriteTableAsync(_writer!, table);
        }

        /// <summary>
        /// Write data from the provided object <paramref name="instance" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">An instance of type <typeparamref name="TObject" /> containing data to write</param>
        public void Write<TObject>(TObject instance)
        {
            EnsureHasWriter();
            TokenLimitedFileWriter.WriteObject<TObject>(_writer!, instance);
        }

        /// <summary>
        /// Write data from the provided object <paramref name="instance" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">An instance of type <typeparamref name="TObject" /> containing data to write</param>
        public async Task WriteAsync<TObject>(TObject instance)
        {
            EnsureHasWriter();
            await TokenLimitedFileWriter.WriteObjectAsync<TObject>(_writer!, instance);
        }

        /// <summary>
        /// Write data from the provided objects <paramref name="instances" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instances">Collection of instances of type <typeparamref name="TObject" /> containing data to write</param>
        public void Write<TObject>(params TObject[] instances)
        {
            EnsureHasWriter();
            TokenLimitedFileWriter.WriteObjects<TObject>(_writer!, instances);
        }

        /// <summary>
        /// Write data from the provided objects <paramref name="instances" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instances">Collection of instances of type <typeparamref name="TObject" /> containing data to write</param>
        public async Task WriteAsync<TObject>(params TObject[] instances)
        {
            EnsureHasWriter();
            await TokenLimitedFileWriter.WriteObjectsAsync<TObject>(_writer!, instances);
        }

        /// <summary>
        /// Write data from the provided objects <paramref name="instances" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instances">Collection of instances of type <typeparamref name="TObject" /> containing data to write</param>
        public void Write<TObject>(IEnumerable<TObject> instances)
        {
            EnsureHasWriter();
            TokenLimitedFileWriter.WriteObjects<TObject>(_writer!, instances);
        }

        /// <summary>
        /// Write data from the provided objects <paramref name="instances" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instances">Collection of instances of type <typeparamref name="TObject" /> containing data to write</param>
        public async Task WriteAsync<TObject>(IEnumerable<TObject> instances)
        {
            EnsureHasWriter();
            await TokenLimitedFileWriter.WriteObjectsAsync<TObject>(_writer!, instances);
        }


        /// <summary>
        /// Adds a flatfile writer to the context
        /// </summary>
        /// <param name="path">Path to the flatfile</param>
        /// <param name="options">Options for the writer</param>
        /// <returns>Reference to the FileContext</returns>
        public TokenLimitedFileContext AddWriter(string path, TokenLimitedFileOptions options)
        {
            _writer = new TokenLimitedFileWriter(path, options);
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
            _writer = new TokenLimitedFileWriter(stream, options);
            return this;
        }

        /// <summary>
        /// Return a context that can be used for writing flatfiles
        /// </summary>
        /// <param name="path">Path to the flatfile</param>
        /// <param name="options">Options for the writer</param>
        /// <returns>Reference to the FileContext</returns>
        public static TokenLimitedFileContext ForWriting(string path, TokenLimitedFileOptions options)
            => new TokenLimitedFileContext()
            {
                _writer = new TokenLimitedFileWriter(path, options)
            };

        /// <summary>
        /// Return a context that can be used for writing flatfiles
        /// </summary>
        /// <param name="stream">A writable stream opened to the path to the flatfile</param>
        /// <param name="options">Options for the writer</param>
        /// <returns>Reference to the FileContext</returns>
        public static TokenLimitedFileContext ForWriting(Stream stream, TokenLimitedFileOptions options)
            => new TokenLimitedFileContext()
            {
                _writer = new TokenLimitedFileWriter(stream, options)
            };

        /// <summary>
        /// Ensure we have a valid _writer, else throws an exception
        /// </summary>
        /// <exception cref="IOException">Thrown when the internal writer has not be initialised. Initialise with a call to ForWriting (ctor) or AddWriter (method)</exception>
        private void EnsureHasWriter()
        {
            if (_writer == null)
            {
                throw new IOException("Cannot execute Write, please call ForWriting or AddWriter methods.");
            }
        }
    }
}
