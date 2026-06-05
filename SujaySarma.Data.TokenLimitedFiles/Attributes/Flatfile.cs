using System;

using SujaySarma.Data.Core.Attributes;

namespace SujaySarma.Data.TokenLimitedFiles.Attributes;

/// <summary>
/// Marks the class, struct or record as being persisted to a flatfile.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public partial class Flatfile : PersistenceContainer
{
    /// <summary>
    /// How fields are referenced in a record. Only one of the supporting 
    /// mechanisms may be used in a file!
    /// Defaults to 'Indexes'.
    /// </summary>
    public FieldReferencesAre FieldReferenceMode
    {
        get; private set;

    } = FieldReferencesAre.Indexes;

    /// <summary>
    /// The 1-based indexed LINE NUMBER in the flatfile that contains the column name information. 
    /// IMPORTANT: Any lines before this line are skipped!
    /// </summary>
    public uint HeaderLineNumber
    {
        get; private set;
    }

    /// <summary>
    /// Marks the class, struct or record as being persisted to a flatfile. 
    /// Flatfiles connected using this constructor must be 'Index' based (not named).
    /// </summary>
    public Flatfile()
        : base(Guid.NewGuid().ToString("n"))
    {
        // The base constructor requires a non-empty string (a table name), 
        // however flatfiles don't use table names, meaning we cannot force the caller 
        // to specify a junk value just to satisfy our idiosyncracies! 
        // Guid.NewGuid:n is the safest way to have a unique/collision free (because of how 
        // table names are used in TypeDiscoveryFactory) string.


        FieldReferenceMode = FieldReferencesAre.Indexes;
        
        // A value of zero is invalid, because files are 1-based.
        HeaderLineNumber = 0;
    }

    /// <summary>
    /// Marks the class, struct or record as being persisted to a flatfile.
    /// Flatfiles connected using this constructor must be 'Name' based (not indexed).
    /// </summary>
    public Flatfile(uint headerRowIndex)
        : base(Guid.NewGuid().ToString("n"))
    {
        // The base constructor requires a non-empty string (a table name), 
        // however flatfiles don't use table names, meaning we cannot force the caller 
        // to specify a junk value just to satisfy our idiosyncracies! 
        // Guid.NewGuid:n is the safest way to have a unique/collision free (because of how 
        // table names are used in TypeDiscoveryFactory) string.

        if (headerRowIndex == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(headerRowIndex), "Indexes in flatfiles are 1-based sequences (1, 2, 3, ...).");
        }

        FieldReferenceMode = FieldReferencesAre.Names;
        HeaderLineNumber = headerRowIndex;
    }
}
