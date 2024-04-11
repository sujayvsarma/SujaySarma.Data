using System;
using System.Collections;
using System.Collections.Generic;

using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.SqlServer.Fluid.Tools
{
    /// <summary>
    /// A collection of TypeTableAliasMap items
    /// </summary>
    internal class TypeTableAliasMapCollection : IEnumerable<TypeTableAliasMap>
    {
        /// <summary>
        /// Try and add the mapping
        /// </summary>
        /// <typeparam name="TClrObject">Clr type mapping to add</typeparam>
        /// <param name="isPrimaryTable">If set, this map is set as the primary table. If a previous one exists, it will be cleared</param>
        public void TryAdd<TClrObject>(bool isPrimaryTable = false)
            => TryAdd(typeof(TClrObject), isPrimaryTable);

        /// <summary>
        /// Try and add the mapping
        /// </summary>
        /// <param name="typeOfClrObject">Clr type mapping to add</param>
        /// <param name="isPrimaryTable">If set, this map is set as the primary table. If a previous one exists, it will be cleared</param>
        public void TryAdd(Type typeOfClrObject, bool isPrimaryTable = false)
        {
            string tName = typeOfClrObject.FullName!;

            bool found = false;
            TypeTableAliasMap? previousPrimary = null;
            foreach (TypeTableAliasMap obj in _list)
            {
                if (obj.IsPrimaryTable)
                {
                    previousPrimary = obj;
                }

                if (obj.ClrObjectType.FullName!.Equals(tName))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                TypeTableAliasMap map = new()
                {
                    Alias = $"t{tableId}",
                    ClrObjectType = typeOfClrObject,
                    Discovery = TypeDiscoveryFactory.Resolve(typeOfClrObject),
                    IsPrimaryTable = isPrimaryTable
                };

                if (isPrimaryTable && previousPrimary != null)
                {
                    previousPrimary.IsPrimaryTable = false;
                }

                _list.Add(map);
                tableId++;
            }
        }

        /// <summary>
        /// Gets the mapping for the provided CLR object
        /// </summary>
        /// <typeparam name="TClrObject">CLR object to fetch table name for</typeparam>
        /// <returns>Table mapping. Null if not found</returns>
        public TypeTableAliasMap? GetMap<TClrObject>()
            => GetMap(typeof(TClrObject));

        /// <summary>
        /// Gets the mapping for the provided CLR object
        /// </summary>
        /// <param name="typeOfClrObject">CLR object to fetch table name for</param>
        /// <returns>Table mapping. Null if not found</returns>
        public TypeTableAliasMap? GetMap(Type typeOfClrObject)
        {
            string tName = typeOfClrObject.FullName!;
            foreach (TypeTableAliasMap obj in _list)
            {
                if (obj.ClrObjectType.FullName!.Equals(tName))
                {
                    return obj;
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
            foreach (TypeTableAliasMap obj in _list)
            {
                if (obj.IsTable(tableName))
                {
                    return obj.Alias;
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
            if (map != null)
            {
                if (!string.IsNullOrWhiteSpace(map.Alias))
                {
                    return map.Alias;
                }

                return map.Discovery.ContainerDefinition.CreateQualifiedName();
            }

            return null;
        }

        /// <summary>
        /// Get the item defined as the primary table.
        /// </summary>
        /// <returns>Item map for Primary table.</returns>
        public TypeTableAliasMap GetPrimaryTable()
        {
            foreach (TypeTableAliasMap obj in _list)
            {
                if (obj.IsPrimaryTable)
                {
                    return obj;
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
            Dictionary<string, string> result = new();
            foreach (TypeTableAliasMap obj in _list)
            {
                result.Add(obj.GetQualifiedTableName(), obj.Alias);
            }

            return result;
        }

        public IEnumerator<TypeTableAliasMap> GetEnumerator()
            => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _list.GetEnumerator();

        private readonly IList<TypeTableAliasMap> _list = new List<TypeTableAliasMap>();
        private uint tableId = 0;
    }
}
