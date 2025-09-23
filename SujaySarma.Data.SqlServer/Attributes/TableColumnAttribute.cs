using System;

using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.SqlServer.Attributes
{
    /// <summary>
    /// Provide the data table column name and other flags used the value for this property or field is stored in or retrieved from.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableColumnAttribute : ContainerMemberAttribute
    {
        /// <summary>
        /// Type of key if it is one, otherwise assign None.
        /// </summary>
        public KeyTypesEnum TypeOfKey
        {
            get; set;

        } = KeyTypesEnum.None;

        /// <summary>
        /// When <see cref="TypeOfKey"/> is <see cref="KeyTypesEnum.Foreign"/>, the referenced table's .NET type (must be decorated with Table/TableColumn attributes). 
        /// Else this should be NULL. 
        /// </summary>
        public Type? ReferencedTable
        {
            get => _refTable;
            set
            {
                if (value != null)
                {
                    try
                    {
                        TypeDiscoveryFactory.Resolve(value);
                    }
                    catch (TypeLoadException)
                    {
                        throw new ArgumentException($"The provided type '{value?.FullName}' is not a valid data table mapped type.");
                    }
                }

                _refTable = value;
            }
        }
        private Type? _refTable = null;


        /// <summary>
        /// Column referenced in <see cref="ReferencedTable"/>.
        /// </summary>
        public string? ReferencedColumn
        {
            get;
            set;

        } = null;


        /// <summary>
        /// Provide the data table column name and other flags used the value for this property or field is stored in or retrieved from.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        public TableColumnAttribute(string columnName)
          : base(columnName)
        {
            Name = ((!string.IsNullOrWhiteSpace(columnName)) ? columnName : throw new ArgumentNullException(nameof(columnName)));
        }

        /// <inheritdoc />
        public override string CreateQualifiedName()
            => $"{Name}";
    }

}
