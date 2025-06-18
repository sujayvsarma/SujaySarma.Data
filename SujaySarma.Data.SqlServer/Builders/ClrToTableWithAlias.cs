using SujaySarma.Data.Core.Reflection;

using System;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// A helper class that allows quick translation between a .NET object and its mapped SQL table.
    /// </summary>
    public class ClrToTableWithAlias
    {

        /// <summary>
        /// Marks the object as a primary table in the given query or statement.
        /// </summary>
        public bool IsPrimaryTable 
        { 
            get; private set;

        }

        /// <summary>
        /// Type discovery information
        /// </summary>
        public ContainerTypeInfo TypeInfo 
        { 
            get; init; 
        }

        /// <summary>
        /// User provided alias for the SQL table, if present
        /// </summary>
        public string? Alias 
        { 
            get; init;         
        }

        /// <summary>
        /// The fully qualified table name, including the schema, as it would appear in a SQL statement.
        /// (eg: "[dbo].[Salaries]")
        /// </summary>
        public string QualifiedTableName
        {
            get; init;
        }

        /// <summary>
        /// The fully qualified table name with the alias, as it would appear in a SQL statement.
        /// (eg: "[dbo].[Salaries] [SAL]")
        /// </summary>
        public string QualifiedTableNameWithAlias
        {
            get; init;
        }

        /// <summary>
        /// Clears the flag that marks this object as a primary table in the query or statement.
        /// </summary>
        public void ClearIsPrimary()
            => IsPrimaryTable = false;

        /// <summary>
        /// Sets the flag that marks this object as a primary table in the query or statement.
        /// </summary>
        public void SetIsPrimary()
            => IsPrimaryTable = true;

        /// <summary>
        /// Initialise a new instance.
        /// </summary>
        /// <param name="clrObjectType">Type of the .NET object to map in.</param>
        /// <param name="isPrimaryTable">Flag to set or indicate this object type as a primary table in the statement sequence.</param>
        /// <param name="alias">The table's alias. NULL if we do not have one (yet).</param>
        public ClrToTableWithAlias(Type clrObjectType, bool isPrimaryTable, string? alias = null)
        {
            TypeInfo = TypeDiscoveryFactory.Resolve(clrObjectType);
            IsPrimaryTable = isPrimaryTable;
            Alias = alias;

            QualifiedTableName = TypeInfo.Container.CreateQualifiedName();
            QualifiedTableNameWithAlias = (string.IsNullOrWhiteSpace(Alias) ? QualifiedTableName : $"{QualifiedTableName} [{Alias}]");
        }

    }
}
