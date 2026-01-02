using SujaySarma.Data.Core.Attributes;

namespace SujaySarma.Data.Files.TokenLimitedFiles.Attributes;

/// <summary>
/// Marks the member property or field as a field in a flatfile.
/// </summary>
public class FlatfileField : PersistenceContainerMember
{
    /// <summary>
    /// The record position within the record-row. 
    /// IMPORTANT: This is a ONE-based value (1, 2, 3...)
    /// </summary>
    public uint Position
    {
        get; init;
    }

    /// <summary>
    /// Marks the member property or field as a named field in the flatfile.
    /// </summary>
    /// <param name="name">Name of the field</param>
    /// <param name="position">A 1-based position of the field in the data file.</param>
    protected FlatfileField(string name, uint position)
        : base(name)
    {
        Position = position;
    }
}
