using System;

using SujaySarma.Data.Core.Attributes;

namespace SujaySarma.Data.SqlServer.Attributes;

/// <summary>
/// Maps an entity member property or field to a column in a SQL Server table. The column is expected to be populated by 
/// SQL Server (using one of many mechanisms: IDENTITY, DEFAULT constraint, Triggers, etc). The parent entity (that contains this 
/// member property/field) must be annotated with the <see cref="SqlTable"/> attribute.
/// </summary>
/// <remarks>Properties/fields with this attribute applied are not included in INSERT/UPDATE operations.</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SqlTablePopulatedColumn : SqlTableColumn, IBackendSystemPopulatedField
{

    /// <summary>
    /// Maps an entity member property or field to a column in a SQL Server table. The column is expected to be populated by 
    /// SQL Server (using one of many mechanisms: IDENTITY, DEFAULT constraint, Triggers, etc). The parent entity (that contains this 
    /// member property/field) must be annotated with the <see cref="SqlTable"/> attribute.
    /// </summary>
    /// <param name="columnName">Name of the column in the table.</param>
    public SqlTablePopulatedColumn(string columnName)
        : base(columnName)
    {
    }
}