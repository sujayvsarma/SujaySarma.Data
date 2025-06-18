using System;
using System.Collections;
using System.Collections.Generic;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// A collection of <see cref="ClrToTableWithAlias"/> items.
    /// </summary>
    public class ClrToTableWithAliasCollection : IEnumerable<ClrToTableWithAlias>, IEnumerable, IDisposable
    {

        /// <summary>
        /// Add a mapping for the given type.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to add mapping for.</typeparam>
        /// <param name="isPrimaryTable">Flag to set this as a primary table for this statement sequence.</param>
        public ClrToTableWithAlias Add<TObject>(bool isPrimaryTable = false)
            => Add(typeof(TObject), isPrimaryTable);

        /// <summary>
        /// Add a mapping for the given type.
        /// </summary>
        /// <param name="type">Type of .NET object to add mapping for.</param>
        /// <param name="isPrimaryTable">Flag to set this as a primary table for this statement sequence.</param>
        public ClrToTableWithAlias Add(Type type, bool isPrimaryTable = false)
        {
            string typeName = type.Name;

            foreach(ClrToTableWithAlias item in _list)
            {
                if (item.TypeInfo.Name.Equals(typeName))
                {
                    // already exists, return it
                    return item;
                }
            }

            ClrToTableWithAlias newItem = new ClrToTableWithAlias(type, isPrimaryTable, $"t{tableId++}");
            _list.Add(newItem);

            if (isPrimaryTable)
            {
                if (_primaryTable != null)
                {
                    // we already have a primary table set, so clear it
                    _primaryTable.ClearIsPrimary();
                }

                _primaryTable = newItem;
            }

            return newItem;
        }

        /// <summary>
        /// Try to retrieve any added mapping for the provided .NET object type.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object.</typeparam>
        /// <returns>The map (if found), else NULL.</returns>
        public ClrToTableWithAlias? Get<TObject>()
            => Get(typeof(TObject));

        /// <summary>
        /// Try to retrieve any added mapping for the provided .NET object type.
        /// </summary>
        /// <param name="type">Type of .NET object.</param>
        /// <returns>The map (if found), else NULL.</returns>
        public ClrToTableWithAlias? Get(Type type)
        {
            string typeName = type.Name;
            foreach (ClrToTableWithAlias item in _list)
            {
                if (item.TypeInfo.Name.Equals(typeName))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the alias defined for the provided table name.
        /// </summary>
        /// <param name="tableName">Name of the SQL table.</param>
        /// <returns>Alias or NULL.</returns>
        public string? GetAlias(string tableName)
        {
            foreach(ClrToTableWithAlias map in _list)
            {
                if (map.QualifiedTableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                {
                    return map.Alias;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the name and alias defined for the provided table name.
        /// </summary>
        /// <param name="tableName">Name of the SQL table.</param>
        /// <returns>Alias or NULL.</returns>
        public string? GetNameWithAlias(string tableName)
        {
            foreach (ClrToTableWithAlias map in _list)
            {
                if (map.QualifiedTableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                {
                    return map.QualifiedTableNameWithAlias;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the alias or name of the SQL table, with a preference for the alias.
        /// </summary>
        /// <param name="type">Type of .NET object.</param>
        /// <returns>Alias, or Name, or NULL (if map was not found)</returns>
        public string? GetUsableMoniker(Type type)
        {
            ClrToTableWithAlias? item = Get(type);
            if (item == null)
            {
                return null;
            }

            return (!string.IsNullOrWhiteSpace(item.Alias)) ? item.Alias : item.QualifiedTableName;
        }

        /// <summary>
        /// Get the item defined as the primary table.
        /// </summary>
        /// <returns>Item map for Primary table, or NULL if no table is designated so.</returns>
        public ClrToTableWithAlias? GetPrimaryTable()
            => _primaryTable;


        private readonly IList<ClrToTableWithAlias> _list = new List<ClrToTableWithAlias>();
        private ClrToTableWithAlias? _primaryTable = null;
        private uint tableId = 0;

        #region IEnumerable

        /// <inheritdoc />
        public IEnumerator<ClrToTableWithAlias> GetEnumerator() => _list.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)_list.GetEnumerator();

        #endregion

        #region IDisposable

        /// <summary>
        /// Dispose this collection
        /// </summary>
        /// <param name="dispose">Actually dispose?</param>
        public void Dispose(bool dispose = true)
        {
            if (dispose && (! isDisposed))
            {
                _list.Clear();

                isDisposed = true;
            }
        }

        /// <summary>
        /// Dispose this collection
        /// </summary>
        public void Dispose()
            => Dispose(true);

        private bool isDisposed = false;
        #endregion
    }
}
