using SujaySarma.Data.Core;

using System;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// Provide name of the table the data for the class is stored in. This attribute cannot be anotated on an
    /// interface as interface-attributes are NOT inherited via class-inheritance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TableAttribute : ContainerAttribute
    {
        /// <summary>
        /// Name of the table schema
        /// </summary>
        public string SchemaName { get; set; } = "dbo";

        /// <summary>
        /// Provides information about the table used to contain the data for an object.
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        public TableAttribute(string tableName)
          : base(tableName)
        {
            Name = ((!string.IsNullOrWhiteSpace(tableName)) ? tableName : throw new ArgumentNullException(nameof(tableName)));
        }

        /// <summary>
        /// Provides information about the table used to contain the data for an object.
        /// </summary>
        /// <param name="schemaName">Name of the table schema</param>
        /// <param name="tableName">Name of the table</param>
        public TableAttribute(string schemaName, string tableName)
          : base(tableName)
        {
            SchemaName = ((!string.IsNullOrWhiteSpace(schemaName)) ? schemaName : throw new ArgumentNullException(nameof(schemaName)));
        }

        /// <summary>
        /// This function is called to retrieve the usable name for the container. Implementing attributes can use it
        /// to apply prefixes, suffixes or even contextually modify the value of the 'Name' property to
        /// provide a different or better name for the operation.
        /// </summary>
        /// <returns>The qualified or usable name to use for the container</returns>
        public override string CreateQualifiedName() 
            => $"[{SchemaName}].[{Name}]";

        /// <summary>
        /// Returns the schema.table name of the table defined.
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() 
            => CreateQualifiedName();
    }
}
