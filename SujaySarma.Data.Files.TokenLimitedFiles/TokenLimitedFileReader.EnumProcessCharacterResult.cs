namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    // The enum ProcessCharacterResult
    public sealed partial class TokenLimitedFileReader
    {
        /// <summary>
        /// Result returned by <see cref="M:SujaySarma.Data.Files.TokenLimitedFiles.TokenLimitedFileReader.ProcessCharacter" />
        /// </summary>
        private enum ProcessCharacterResult
        {
            /// <summary>
            /// Continue and read the next character as normal
            /// </summary>
            ContinueNormalRead,

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
