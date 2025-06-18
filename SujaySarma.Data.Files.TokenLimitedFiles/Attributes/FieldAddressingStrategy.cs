namespace SujaySarma.Data.Files.TokenLimitedFiles.Attributes
{
    /// <summary>
    /// The strategy to use to address a flatfile field while reading or writing information.
    /// </summary>
    public enum FieldAddressingStrategy
    {
        /// <summary>
        /// Address fields by the names provided in the column header row.
        /// </summary>
        Name,

        /// <summary>
        /// Address fields by their zero-based index in the column header row.
        /// </summary>
        Indices
    }
}
