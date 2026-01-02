using System;

using SujaySarma.Data.Core.Attributes;

namespace SujaySarma.Data.SqlServer.Attributes;

/// <summary>
/// Maps a business class entity class, struct or record to a table in SQL Server that features the ability 
/// to soft-delete records. Soft-deletion where a "hidden" table column, typically a boolean value, indicates 
/// that the record is "deleted" instead of being actually deleted (called a "hard delete"). Soft-deleted records 
/// exist in the table and can be queried by either ignoring the soft-delete flag column or by specifying that 
/// you want to include these rows as well. This library will not return soft-deleted rows unless you use 
/// method variants that explicitly fetch it.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public class SqlTableWithSoftDelete : SqlTable, ISoftDeleteRecords
{
    /// <inheritdoc />
    public string SoftDeleteTableColumnName
    {
        get; init;
    }

    /// <summary>
    /// Maps a business class entity class, struct or record to a table in SQL Server that features the ability 
    /// to soft-delete records. Soft-deletion where a "hidden" table column, typically a boolean value, indicates 
    /// that the record is "deleted" instead of being actually deleted (called a "hard delete"). Soft-deleted records 
    /// exist in the table and can be queried by either ignoring the soft-delete flag column or by specifying that 
    /// you want to include these rows as well. This library will not return soft-deleted rows unless you use 
    /// method variants that explicitly fetch it.
    /// </summary>
    /// <param name="tableName">Name of table.</param>
    /// <param name="softDeleteColumnName">Name of the soft-delete column in the table.</param>
    public SqlTableWithSoftDelete(string tableName, string softDeleteColumnName)
        : base(tableName)
    {
        Schema = "dbo";
        SoftDeleteTableColumnName = softDeleteColumnName;
    }
}