using SujaySarma.Data.SqlServer.Fluid.AliasMaps;

using System;
using System.Collections.Generic;
using System.Text;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// Fluid builder for UPDATE statement and operations. Allows updating columns not a part of <typeparamref name="TTable" />.
    /// Handles simple UPDATEs, but we do not deal with complex statements like UPDATEs with JOINs and so on.
    /// </summary>
    /// <typeparam name="TTable">Type of business object mapped to the table being updated</typeparam>
    public class SqlUpdateBuilder<TTable> : SqlFluidStatementBuilder
    {

        /// <inheritdoc />
        public override string Build()
        {
            if (_updateList.Count == 0)
            {
                throw new InvalidOperationException("There are no values to update. Add some items using the AddItems() method and try again.");
            }

            string additionalColumnsSql = string.Empty;
            string whereClauseSql = string.Empty;
            StringBuilder returnSqlBuilder = new StringBuilder();
            
            if ((_additionalColumnsWithValues != null) && (_additionalColumnsWithValues.Count > 0))
            {
                List<string> additionalColumns = new List<string>();
                foreach (KeyValuePair<string, object?> kvp in _additionalColumnsWithValues)
                {
                    additionalColumns.Add($"{kvp.Key} = {ReflectionUtils.GetSQLStringValue(kvp.Value)}");
                }

                additionalColumnsSql = string.Join(',', additionalColumns);
            }

            if (Where.HasItems)
            {
                whereClauseSql = "WHERE " + Where.ToString(string.Empty);
            }

            TypeTableAliasMap map = TypeTableMap.GetMap<TTable>() 
                ?? throw new Exception($"Table for '{typeof(TTable).Name}' is not registered.");

            foreach (TTable table in _updateList)
            {
                StringBuilder tableSqlBuilder = new StringBuilder($"UPDATE [{map.GetQualifiedTableName()}] SET");
                List<string> columnValues = new List<string>();
                List<string> searchKeyConditions = new List<string>();
                string tableWhereClauseSql = whereClauseSql;

                base.ExtractValuesForUpdate<TTable>(table, ref columnValues, ref searchKeyConditions);

                tableSqlBuilder.Append($" {string.Join(',', columnValues)}");
                if (!string.IsNullOrEmpty(additionalColumnsSql))
                {
                    tableSqlBuilder.Append($", {additionalColumnsSql}");
                }

                if (searchKeyConditions.Count > 0)
                {
                    string searchKeySql = string.Join(" AND ", searchKeyConditions);
                    tableWhereClauseSql = string.IsNullOrEmpty(whereClauseSql)
                        ? $"WHERE {searchKeySql}"
                        : $"{whereClauseSql} AND {searchKeySql}";
                }

                if (!string.IsNullOrEmpty(tableWhereClauseSql))
                {
                    tableSqlBuilder.Append($" {tableWhereClauseSql}");
                }

                returnSqlBuilder.AppendLine($"{tableSqlBuilder.ToString()};");
            }

            return returnSqlBuilder.ToString();
        }

        /// <summary>
        /// Define additional columns to update (columns in table not defined in the <typeparamref name="TTable" /> object). These columns are added to the END of the list.
        /// </summary>
        /// <param name="additionalColumnsWithValues">Additional columns with values. Items with columns already defined are replaced. The same values are updated for every row!</param>
        /// <returns>Self-instance</returns>
        public SqlUpdateBuilder<TTable> WithAdditionalColumns(Dictionary<string, object?> additionalColumnsWithValues)
        {
            CopyTo(additionalColumnsWithValues, ref _additionalColumnsWithValues);
            return this;
        }

        /// <summary>
        /// Add the object instances that contain the data to be updated
        /// </summary>
        /// <param name="items">Object instances with data</param>
        /// <returns>Self-instance</returns>
        public SqlUpdateBuilder<TTable> AddItems(params TTable[] items)
        {
            _updateList.AddRange(items);
            return this;
        }

        /// <summary>Collection of WHERE conditions</summary>
        public SqlWhere Where { get; init; }

        private readonly List<TTable> _updateList;
        private Dictionary<string, object?>? _additionalColumnsWithValues;

        /// <summary>
        /// Defines the primary CLR object (and hence it's backing SQL Server table) that should be used
        /// to populate the query. Any unqualified column references are implicitly assumed to be homed
        /// in this object/table.
        /// </summary>
        /// <returns>Created instance of SqlQueryBuilder</returns>
        public static SqlUpdateBuilder<TTable> Begin() => new SqlUpdateBuilder<TTable>();

        /// <inheritdoc />
        private SqlUpdateBuilder()
        {
            TypeTableMap.TryAdd<TTable>(true);
            _updateList = new List<TTable>();
            _additionalColumnsWithValues = null;
            Where = new SqlWhere(TypeTableMap);
        }
    }
}
