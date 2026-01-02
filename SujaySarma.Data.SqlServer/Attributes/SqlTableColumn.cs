using System;

using SujaySarma.Data.Core.Attributes;

namespace SujaySarma.Data.SqlServer.Attributes;

/// <summary>
/// Maps an entity member property or field to a column in a SQL Server table. The parent entity (that contains this 
/// member property/field) must be annotated with the <see cref="SqlTable"/> attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SqlTableColumn : PersistenceContainerMember
{

    /// <summary>
    /// Maps an entity member property or field to a column in a SQL Server table. The parent entity (that contains this 
    /// member property/field) must be annotated with the <see cref="SqlTable"/> attribute.
    /// </summary>
    /// <param name="columnName">Name of the column in the table.</param>
    public SqlTableColumn(string columnName) 
        : base(columnName)
    {
    }
}
