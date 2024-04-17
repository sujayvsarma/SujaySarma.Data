using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /*  Perform Write operations */
    public partial class TokenLimitedFileContext
    {
        /// <summary>
        /// Write records from the provided <paramref name="table"/>
        /// </summary>
        /// <param name="table">A <see cref="DataTable"/> containing records to write</param>
        public void Write(DataTable table)
        {
            EnsureHasWriter();

            TokenLimitedFileWriter.WriteTable(_writer!, table);
        }

        /// <summary>
        /// Write records from the provided <paramref name="table"/>
        /// </summary>
        /// <param name="table">A <see cref="DataTable"/> containing records to write</param>
        public async Task WriteAsync(DataTable table)
        {
            EnsureHasWriter();

            await TokenLimitedFileWriter.WriteTableAsync(_writer!, table);
        }

        /// <summary>
        /// Write data from the provided object <paramref name="instance"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">An instance of type <typeparamref name="TObject"/> containing data to write</param>
        public void Write<TObject>(TObject instance)
        {
            EnsureHasWriter();

            TokenLimitedFileWriter.WriteObject<TObject>(_writer!, instance);
        }

        /// <summary>
        /// Write data from the provided object <paramref name="instance"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">An instance of type <typeparamref name="TObject"/> containing data to write</param>
        public async Task WriteAsync<TObject>(TObject instance)
        {
            EnsureHasWriter();

            await TokenLimitedFileWriter.WriteObjectAsync<TObject>(_writer!, instance);
        }

        /// <summary>
        /// Write data from the provided objects <paramref name="instances"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instances">Collection of instances of type <typeparamref name="TObject"/> containing data to write</param>
        public void Write<TObject>(params TObject[] instances)
        {
            EnsureHasWriter();

            TokenLimitedFileWriter.WriteObjects<TObject>(_writer!, instances);
        }

        /// <summary>
        /// Write data from the provided objects <paramref name="instances"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instances">Collection of instances of type <typeparamref name="TObject"/> containing data to write</param>
        public async Task WriteAsync<TObject>(params TObject[] instances)
        {
            EnsureHasWriter();

            await TokenLimitedFileWriter.WriteObjectsAsync<TObject>(_writer!, instances);
        }

        /// <summary>
        /// Write data from the provided objects <paramref name="instances"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instances">Collection of instances of type <typeparamref name="TObject"/> containing data to write</param>
        public void Write<TObject>(IEnumerable<TObject> instances)
        {
            EnsureHasWriter();

            TokenLimitedFileWriter.WriteObjects<TObject>(_writer!, instances);
        }

        /// <summary>
        /// Write data from the provided objects <paramref name="instances"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instances">Collection of instances of type <typeparamref name="TObject"/> containing data to write</param>
        public async Task WriteAsync<TObject>(IEnumerable<TObject> instances)
        {
            EnsureHasWriter();

            await TokenLimitedFileWriter.WriteObjectsAsync<TObject>(_writer!, instances);
        }


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