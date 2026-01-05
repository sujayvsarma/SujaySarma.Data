using System;

namespace SujaySarma.Data.TokenLimitedFiles.Attributes;

/// <summary>
/// Marks the member property or field as a field in a flatfile that is mapped by a 
/// field/column name in the file's header -- consequently, named fields can only be used 
/// to serialise/deserialise from a flatfile that has header rows defining the 
/// fields of data in that file.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class FlatfileNamedField : FlatfileField
{

    /// <summary>
    /// Marks the member property or field as a field in a flatfile that is mapped by a 
    /// field/column name in the file's header -- consequently, named fields can only be used 
    /// to serialise/deserialise from a flatfile that has header rows defining the 
    /// fields of data in that file.
    /// </summary>
    /// <param name="name">Name of the field</param>
    /// <param name="position">A 1-based position of the field in the data file.</param>
    public FlatfileNamedField(string name, uint position)
        : base(name, position)
    {
    }

    /// <summary>
    /// Returns the name enclosed in double quotes.
    /// </summary>
    /// <param name="quoteNames">(Not used)</param>
    /// <returns>Name enclosed in double quotes.</returns>
    public override string CreateQualifiedName(bool quoteNames = true)
        => $"\"{TableFieldName}\"";
}
