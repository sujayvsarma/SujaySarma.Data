using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;

namespace SujaySarma.Data.SqlServer.Attributes;

/// <summary>
/// Maps an entity member property or field to a column in a SQL Server table and marks it as a FOREIGN KEY. A table may have 
/// any number of foreign keys (and hence the entity may have any number of member properties/fields annotated with this attribute). 
/// The parent entity (that contains this member property/field) must be annotated with the <see cref="SqlTable"/> attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SqlTableForeignKeyColumn : SqlTableColumn
{
    /// <summary>
    /// Gets the direction of the foreign key relationship represented by this column.
    /// </summary>
    /// <remarks>The direction indicates whether the column refers from the child table to the parent table or
    /// vice versa. This property is immutable and is set during object initialization.</remarks>
    public ReferenceDirection Direction
    {
        get; init;

    } = ReferenceDirection.ChildToParent;


    /// <summary>
    /// Creates the stub for the JOIN clause ("JOIN table t on (t.col = a.col)").
    /// </summary>
    /// <param name="primaryTableAlias">Name or alias of the table for the entity this column has been declared in.</param>
    /// <param name="referencedTableAlias">Name or alias of the foreign (referenced) table.</param>
    /// <returns>The JOIN clause stub  ("JOIN table t on (t.col = a.col)").</returns>
    public string CreateJoinClause(string primaryTableAlias, string referencedTableAlias)
        => $"JOIN [{ReferencedTableName}] [{referencedTableAlias}] ON ([{referencedTableAlias}].[{ReferencedColumnName}] = [{primaryTableAlias}].[{TableFieldName}])";

    /// <summary>
    /// Generates a list of the names of all columns in the referenced table. 
    /// NOTE: This function runs only if the typed constructor was used and we have a referenced table type (not just the name!).
    /// </summary>
    /// <returns>List of string (names), or empty list.</returns>
    public List<string> GetAllColumnsForReferencedTable()
    {
        List<string> results = new List<string>();
        if (IsTypedReference && (ReferencedTable is not null))
        {
            foreach (PersistenceContainerMemberInfo member in ReferencedTable.Members)
            {
                results.Add($"[{ReferencedTableAlias}].[{member.PersistenceInfo.CreateQualifiedName()}]");
            }
        }
        else
        {
            // we don't know the columns, so return wildcard
            results.Add($"[{ReferencedTableAlias}].*"); 
        }

        return results;
    }


    /// <summary>
    /// Maps an entity member property or field to a column in a SQL Server table and marks it as a FOREIGN KEY. A table may have 
    /// any number of foreign keys (and hence the entity may have any number of member properties/fields annotated with this attribute). 
    /// The parent entity (that contains this member property/field) must be annotated with the <see cref="SqlTable"/> attribute.
    /// </summary>
    /// <param name="columnName">Name of the column in this table.</param>
    /// <param name="referencedTableName">Name of the referenced (other) table in the relationship. Ideally, this should include the schema name as well. This value is NOT validated!</param>
    /// <param name="referencedColumnName">Name of the referenced column in the relationship. This value is NOT validated!</param>
    /// <param name="referencedTableAlias">Alias for the referenced table. This should NOT be in the format "T[number]" (eg: "T4") to avoid collisions with auto-generated aliases!</param>
    public SqlTableForeignKeyColumn(string columnName, string referencedTableName, string referencedColumnName, string referencedTableAlias)
        : base(columnName)
    {
        if (string.IsNullOrWhiteSpace(referencedTableName))
        {
            throw new ArgumentNullException(nameof(referencedTableName), "Cannot be NULL or empty");
        }

        if (string.IsNullOrWhiteSpace(referencedColumnName))
        {
            throw new ArgumentNullException(nameof(referencedColumnName), "Cannot be NULL or empty");
        }

        if (string.IsNullOrWhiteSpace(referencedTableAlias) || Regex.IsMatch(referencedTableAlias, @"^T\d+$"))
        {
            throw new ArgumentException("Referenced table alias cannot be NULL, empty, or match the auto-generated pattern 'T{number}' (e.g., T1, T10).", nameof(referencedTableAlias));
        }

        IsTypedReference = false;
        ReferencedTableName = referencedTableName;
        ReferencedColumnName = referencedColumnName;
        ReferencedTableAlias = referencedTableAlias;
    }


    /// <summary>
    /// Maps an entity member property or field to a column in a SQL Server table and marks it as a FOREIGN KEY. A table may have 
    /// any number of foreign keys (and hence the entity may have any number of member properties/fields annotated with this attribute). 
    /// The parent entity (that contains this member property/field) must be annotated with the <see cref="SqlTable"/> attribute.
    /// </summary>
    /// <param name="columnName">Name of the column in the table.</param>
    /// <param name="referencedTableEntity">The <see cref="Type"/> of the entity class/struct/record that is the referenced (other) table in the relationship.</param>
    /// <param name="referencedPropertyOrField">The name of the member/instance property or field in <paramref name="referencedTableEntity"/> that is the referenced column in the relationship.</param>
    public SqlTableForeignKeyColumn(string columnName, Type referencedTableEntity, string referencedPropertyOrField)
        : base(columnName)
    {
        if (string.IsNullOrWhiteSpace(referencedPropertyOrField))
        {
            throw new ArgumentNullException(nameof(referencedPropertyOrField), "Cannot be NULL or empty");
        }

        PersistenceContainerInfo container = referencedTableEntity.RetrievePersistenceContainerInfoOrThrowException();

        IsTypedReference = true;
        ReferencedTable = container;

        container.GetNameAndAlias(out ReferencedTableName, out ReferencedTableAlias);
        foreach (PersistenceContainerMemberInfo member in container.Members)
        {
            if (member.Member.Name == referencedPropertyOrField)
            {
                ReferencedColumn = member;
                break;
            }
        }

        if (ReferencedColumn is null)
        {
            throw new InvalidOperationException($"The type '{referencedTableEntity.GetUsableTypeName()}' does not contain an instance property or field named '{referencedPropertyOrField}' and annotated with a '{nameof(SqlTableColumn)}' attribute.");
        }

        ReferencedColumnName = ReferencedColumn.PersistenceInfo.TableFieldName;
    }

    // The below fields are populated by the constructors above, and accessible only 
    // internally by the ORM code. It would be too complicated to expose them as public, 
    // or rewrite them as properties!
    internal readonly bool IsTypedReference = false;
    internal readonly PersistenceContainerInfo? ReferencedTable = null;
    internal readonly PersistenceContainerMemberInfo? ReferencedColumn = null;
    internal readonly string ReferencedTableName, ReferencedColumnName, ReferencedTableAlias;


    /// <summary>
    /// The direction of the relationship.
    /// </summary>
    public enum ReferenceDirection
    {
        /// <summary>
        /// Child-Parent (a normal Foreign Key)
        /// </summary>
        ChildToParent = 0,

        /// <summary>
        /// Parent-Child (reverse FK, Master/Detail relationship)
        /// </summary>
        ParentToChild = 1
    }

}


