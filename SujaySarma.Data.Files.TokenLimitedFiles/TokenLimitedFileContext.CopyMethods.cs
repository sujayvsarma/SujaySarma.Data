using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    // Read/write (Copy) specific methods
    public partial class TokenLimitedFileContext
    {

        /// <summary>
        /// Copy all information verbatim from the <see cref="TokenLimitedFileReader" /> to the <see cref="TokenLimitedFileWriter" />.
        /// </summary>
        public void CopyVerbatim()
        {
            EnsureHasBothReaderAndWriter();
            while (!_reader!.EndOfStream)
            {
                _writer!.Write(_reader.ReadRow());
            }
        }

        /// <summary>
        /// Copy all information verbatim from the <see cref="TokenLimitedFileReader" /> to the <see cref="TokenLimitedFileWriter" />.
        /// </summary>
        public async Task CopyVerbatimAsync()
        {
            EnsureHasBothReaderAndWriter();
            while (!_reader!.EndOfStream)
            {
                await _writer!.WriteAsync(await _reader.ReadRowAsync());
            }
        }

        /// <summary>
        /// From the <see cref="TokenLimitedFileReader" />, copy only that data which successfully deserialise into
        /// an instance of <typeparamref name="TObject" /> and write them to <see cref="TokenLimitedFileWriter" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public void Copy<TObject>()
        {
            EnsureHasBothReaderAndWriter();
            DataTable table = TokenLimitedFileReader.ReadToTable(_reader!);

            if (table.Columns.Count > 0)
            {
                TokenLimitedFileWriter.WriteHeader(_writer!, table);
                foreach (DataRow row in table.Rows)
                {
                    TokenLimitedFileWriter.WriteRow(_writer!, row);
                }
            }
        }

        /// <summary>
        /// From the <see cref="TokenLimitedFileReader" />, copy only that data which successfully deserialise into
        /// an instance of <typeparamref name="TObject" /> and write them to <see cref="TokenLimitedFileWriter" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public async Task CopyAsync<TObject>()
        {
            EnsureHasBothReaderAndWriter();
            DataTable table = await TokenLimitedFileReader.ReadToTableAsync(_reader!);

            if (table.Columns.Count > 0)
            {
                await TokenLimitedFileWriter.WriteHeaderAsync(_writer!, table);
                foreach (DataRow row in table.Rows)
                {
                    await TokenLimitedFileWriter.WriteRowAsync(_writer!, row);
                }
            }
        }

        /// <summary>
        /// Ensure we have a valid _reader and _writer, else throws an exception
        /// </summary>
        /// <exception cref="T:System.IO.IOException">Thrown when the internal writer has not be initialised. Initialise with a call to ForReading/ForWriting (ctors) or AddReader/AddWriter (methods)</exception>
        private void EnsureHasBothReaderAndWriter()
        {
            if ((_reader == null) || (_writer == null))
            {
                throw new IOException("Cannot execute Read/Write, please call ForReading/ForWriting or AddReader/AddWriter methods.");
            }
        }
    }
}
