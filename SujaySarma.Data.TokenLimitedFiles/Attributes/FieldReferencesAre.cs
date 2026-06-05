namespace SujaySarma.Data.TokenLimitedFiles.Attributes;

/// <summary>
/// How fields are referenced in this flatfile.
/// </summary>
public enum FieldReferencesAre
{
    /// <summary>
    /// By a zero-based index. The field's position in the record 
    /// is its identifier.
    /// </summary>
    Indexes = 0,

    /// <summary>
    /// By names. We will expect a header row or a list of names to be 
    /// supplied that maps names to columns.
    /// </summary>
    Names
}
