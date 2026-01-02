using System;

namespace SujaySarma.Data.SqlServer.Attributes;

/// <summary>
/// Maps an entity member property or field to a column in a SQL Server table, and marks the column as a PRIMARY KEY for the 
/// table. An SQL Server table may have a maximum of only one primary key, and hence the entity may have only one member property or field 
/// annotated with this attribute. The parent entity (that contains this member property/field) must be annotated with the <see cref="SqlTable"/> attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SqlTablePrimaryKeyColumn : SqlTableColumn
{
    /// <summary>
    /// Maps an entity member property or field to a column in a SQL Server table, and marks the column as a PRIMARY KEY for the 
    /// table. An SQL Server table may have a maximum of only one primary key, and hence the entity may have only one member property or field 
    /// annotated with this attribute. The parent entity (that contains this member property/field) must be annotated with the <see cref="SqlTable"/> attribute.
    /// </summary>
    /// <param name="columnName">Name of the column in the table.</param>
    public SqlTablePrimaryKeyColumn(string columnName)
        : base(columnName)
    {
    }
}