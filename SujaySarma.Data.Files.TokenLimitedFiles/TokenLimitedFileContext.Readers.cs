using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

using SujaySarma.Data.Files.TokenLimitedFiles.Serialisation;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /*  Perform Read operations */
    public partial class TokenLimitedFileContext
    {

        /// <summary>
        /// Read content from the flatfile and return it as a <see cref="DataTable" />.
        /// </summary>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <returns>A DataTable containing zero or more rows of data, potentially with column definitions</returns>
        public DataTable ReadToTable(string tableName = "Table 1")
        {
            EnsureHasReader();

            return TokenLimitedFileReader.ReadToTable(_reader!, tableName);
        }

        /// <summary>
        /// Read content from the flatfile and return it as a <see cref="DataTable" />.
        /// </summary>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <returns>A DataTable containing zero or more rows of data, potentially with column definitions</returns>
        public async Task<DataTable> ReadToTableAsync(string tableName = "Table 1")
        {
            EnsureHasReader();

            return await TokenLimitedFileReader.ReadToTableAsync(_reader!, tableName);
        }

        /// <summary>
        /// Get the data from the flatfile as an IEnumerable[T]
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="action">An action to perform on the DataTable before conversion to list</param>
        /// <returns>Data from the stream as an IEnumerable[T]</returns>
        public IEnumerable<TObject> ReadToEnumerable<TObject>(string tableName = "Table 1", Action<DataTable>? action = null)
        {
            EnsureHasReader();

            DataTable table = ReadToTable(tableName);
            action?.Invoke(table);

            return TokenLimitedFileSerialiser.Transform<TObject>(table);
        }

        /// <summary>
        /// Get the data from the flatfile as an IEnumerable[T]
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="action">An action to perform on the DataTable before conversion to list</param>
        /// <returns>Data from the stream as an IEnumerable[T]</returns>
        public async Task<IEnumerable<TObject>> ReadToEnumerableAsync<TObject>(string tableName = "Table 1", Action<DataTable>? action = null)
        {
            EnsureHasReader();

            DataTable table = await ReadToTableAsync(tableName);
            action?.Invoke(table);

            return TokenLimitedFileSerialiser.Transform<TObject>(table);
        }


        /// <summary>
        /// Ensure we have a valid _reader, else throws an exception
        /// </summary>
        /// <exception cref="IOException">Thrown when the internal reader has not be initialised. Initialise with a call to ForReading (ctor) or AddReader (method)</exception>
        private void EnsureHasReader()
        {
            if (_reader == null)
            {
                throw new IOException("Cannot execute read, please call ForReading or AddReader methods.");
            }
        }
    }
}