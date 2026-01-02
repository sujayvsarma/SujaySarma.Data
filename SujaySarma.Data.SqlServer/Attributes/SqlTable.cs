using System;

using SujaySarma.Data.Core.Attributes;

namespace SujaySarma.Data.SqlServer.Attributes;

/// <summary>
/// Maps a business class entity class, struct or record to a table in SQL Server.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public class SqlTable : PersistenceContainer
{
    /// <summary>
    /// The table's parent schema. 
    /// Defaults to 'dbo'.
    /// </summary>
    public string Schema
    {
        get;
        init;
    }

    /// <summary>
    /// Maps a business class entity class, struct or record to a table in SQL Server.
    /// </summary>
    /// <param name="tableName">Name of table.</param>
    public SqlTable(string tableName) 
        : base(tableName)
    {
        Schema = "dbo";
    }


    /// <inheritdoc />
    public override string CreateQualifiedName(bool quoteNames = true)
        => (quoteNames ? $"[{Schema}].[{TableName}]" : $"{Schema}.{TableName}");
}
