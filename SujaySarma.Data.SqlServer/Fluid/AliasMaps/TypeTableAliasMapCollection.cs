using System;
using System.Collections;
using System.Collections.Generic;

using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.SqlServer.Fluid.AliasMaps
{
    /// <summary>
    /// A collection of TypeTableAliasMap items
    /// </summary>
    internal class TypeTableAliasMapCollection : IEnumerable<TypeTableAliasMap>, IEnumerable
    {
        private readonly IList<TypeTableAliasMap> _list = new List<TypeTableAliasMap>();
        private uint tableId;

        /// <summary>
        /// Try and add the mapping
        /// </summary>
        /// <typeparam name="TClrObject">Clr type mapping to add</typeparam>
        /// <param name="isPrimaryTable">If set, this map is set as the primary table. If a previous one exists, it will be cleared</param>
        public void TryAdd<TClrObject>(bool isPrimaryTable = false)
        {
            TryAdd(typeof(TClrObject), isPrimaryTable);
        }

        /// <summary>
        /// Try and add the mapping
        /// </summary>
        /// <param name="typeOfClrObject">Clr type mapping to add</param>
        /// <param name="isPrimaryTable">If set, this map is set as the primary table. If a previous one exists, it will be cleared</param>
        public void TryAdd(Type typeOfClrObject, bool isPrimaryTable = false)
        {
            string fullName = typeOfClrObject.FullName!;
            bool flag = false;
            TypeTableAliasMap? map = null;

            foreach (TypeTableAliasMap listItem in _list)
            {
                if (listItem.IsPrimaryTable)
                {
                    // get a marker to the primary table in the set
                    map = listItem;
                }

                if (listItem.ClrObjectType.FullName!.Equals(fullName))
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                return;
            }

            TypeTableAliasMap newMapItem = new TypeTableAliasMap()
            {
                Alias = $"t{tableId}",
                ClrObjectType = typeOfClrObject,
                Discovery = TypeDiscoveryFactory.Resolve(typeOfClrObject),
                IsPrimaryTable = isPrimaryTable
            };

            // caller wants to set this as the primary table, but we already have one set
            // clear the earlier table's flag.
            if (isPrimaryTable && (map != null))
            {
                map.IsPrimaryTable = false;
            }

            _list.Add(newMapItem);
            ++tableId;
        }

        /// <summary>
        /// Gets the mapping for the provided CLR object
        /// </summary>
        /// <typeparam name="TClrObject">CLR object to fetch table name for</typeparam>
        /// <returns>Table mapping. Null if not found</returns>
        public TypeTableAliasMap? GetMap<TClrObject>() => GetMap(typeof(TClrObject));

        /// <summary>
        /// Gets the mapping for the provided CLR object
        /// </summary>
        /// <param name="typeOfClrObject">CLR object to fetch table name for</param>
        /// <returns>Table mapping. Null if not found</returns>
        public TypeTableAliasMap? GetMap(Type typeOfClrObject)
        {
            string fullName = typeOfClrObject.FullName!;
            foreach (TypeTableAliasMap map in _list)
            {
                if (map.ClrObjectType.FullName!.Equals(fullName))
                {
                    return map;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the alias for the table if the table is registered.
        /// </summary>
        /// <param name="tableName">Name of table</param>
        /// <returns>Alias for table, or NULL</returns>
        public string? GetAliasIfDefined(string tableName)
        {
            foreach (TypeTableAliasMap typeTableAliasMap in _list)
            {
                if (typeTableAliasMap.IsTable(tableName))
                {
                    return typeTableAliasMap.Alias;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the alias or the full table name
        /// </summary>
        /// <param name="typeOfClrObject">CLR object ot fetch name for</param>
        /// <returns>Alias registered for table. Full table name in schema.tableName format. Or Null if mapping is not present.</returns>
        public string? GetAliasOrName(Type typeOfClrObject)
        {
            TypeTableAliasMap? map = GetMap(typeOfClrObject);
            if (map == null)
            {
                return null;
            }

            return ((!string.IsNullOrWhiteSpace(map.Alias)) ? map.Alias : map.Discovery.Container.CreateQualifiedName());
        }

        /// <summary>
        /// Get the item defined as the primary table.
        /// </summary>
        /// <returns>Item map for Primary table.</returns>
        public TypeTableAliasMap GetPrimaryTable()
        {
            foreach (TypeTableAliasMap primaryTable in _list)
            {
                if (primaryTable.IsPrimaryTable)
                {
                    return primaryTable;
                }
            }
            throw new InvalidOperationException("Something is wrong. There is no primary table defined. This should not be the case!");
        }

        /// <summary>
        /// Convert the collection to the format required for our Expression to SQL parsers
        /// </summary>
        /// <returns>Dictionary with a string key (full table name) and string value (table alias as applicable)</returns>
        public Dictionary<string, string> ToParserMapFormat()
        {
            Dictionary<string, string> parserMapFormat = new Dictionary<string, string>();
            foreach (TypeTableAliasMap typeTableAliasMap in _list)
            {
                parserMapFormat.Add(typeTableAliasMap.GetQualifiedTableName(), typeTableAliasMap.Alias);
            }

            return parserMapFormat;
        }

        public IEnumerator<TypeTableAliasMap> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)_list.GetEnumerator();
    }
}
