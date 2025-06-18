using SujaySarma.Data.SqlServer.Fluid.AliasMaps;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// Fluid builder for an UPDATE FROM JOIN statement.
    /// </summary>
    public class SqlUpdateWithJoinsBuilder : SqlFluidStatementBuilder
    {
        /// <inheritdoc />
        public override string Build()
        {
            TypeTableAliasMap primaryTableMap = TypeTableMap.GetPrimaryTable();
            List<string> addlValuesList = new List<string>();

            if ((_additionalColumnsWithValues != null) && (_additionalColumnsWithValues.Count > 0))
            {
                foreach(KeyValuePair<string, object?> kvp in _additionalColumnsWithValues)
                {
                    addlValuesList.Add($"[{kvp.Key}]={ReflectionUtils.GetSQLStringValue(kvp.Value)}");
                }
            }

            return string.Join(' ',
                    $"UPDATE [{primaryTableMap.GetQualifiedTableName()}] SET",
                    string.Join(',', _columnMappings),
                    ((addlValuesList.Count > 0) ? string.Join(',', addlValuesList) : string.Empty),
                    (Where.HasItems ? $"WHERE {Where.ToString(string.Empty)}" : string.Empty)
                );
        }

        /// <summary>
        /// Set up the column mappings between the primary table and a join-table. These mappings are used as the update 'a = b' pairs
        /// </summary>
        /// <typeparam name="TPrimaryTable">Type of CLR object of the primary table</typeparam>
        /// <typeparam name="TJoinTable">Type of CLR object of one of the join tables</typeparam>
        /// <param name="selectors">Linq expressions to select one or more columns</param>
        /// <returns>Self-instance</returns>
        public SqlUpdateWithJoinsBuilder Set<TPrimaryTable, TJoinTable>(params Expression<Func<TPrimaryTable, TJoinTable>>[] selectors)
        {
            if (typeof(TPrimaryTable) == typeof(TJoinTable))
            {
                throw new ArgumentException($"Type of '{typeof(TPrimaryTable).Name}' must be different from '{typeof(TJoinTable).Name}'.");
            }

            TypeTableAliasMap? map1 = base.TypeTableMap.GetMap<TPrimaryTable>();
            TypeTableAliasMap? map2 = base.TypeTableMap.GetMap<TJoinTable>();

            if ((map1 == null) || (!map1.IsPrimaryTable))
            {
                throw new ArgumentException($"The primary table object '{typeof(TPrimaryTable).Name}' should have been added to the query using 'IntoTable()'.");
            }

            if ((map2 == null) || map2.IsPrimaryTable)
            {
                throw new ArgumentException($"The primary table object '{typeof(TJoinTable).Name}' should have been added to the query using 'Joins.Add()'.");
            }

            foreach (Expression selector in selectors)
            {
                _columnMappings.Add(base.ExpressionToSQL(selector, true));
            }

            return this;
        }

        /// <summary>
        /// Define additional columns to update (columns in table not defined in the primary CLR object). These columns are added to the END of the list.
        /// </summary>
        /// <param name="additionalColumnsWithValues">Additional columns with values. Items with columns already defined are replaced. The same values are updated for every row!</param>
        /// <returns>Self-instance</returns>
        public SqlUpdateWithJoinsBuilder WithAdditionalColumns(Dictionary<string, object?> additionalColumnsWithValues)
        {
            CopyTo(additionalColumnsWithValues, ref _additionalColumnsWithValues);
            return this;
        }


        private Dictionary<string, object?>? _additionalColumnsWithValues;
        private readonly List<string> _columnMappings;

        /// <summary>Collection of WHERE conditions</summary>
        public SqlWhere Where { get; init; }

        /// <summary>Collection of JOINs</summary>
        public SqlJoin Joins { get; init; }

        /// <summary>
        /// Define the primary CLR object (and its backing SQL Server table) that should be updated into.
        /// </summary>
        /// <typeparam name="TTable">Type of CLR object</typeparam>
        /// <returns>Created instance of SqlInsertFromQueryBuilder</returns>
        public static SqlUpdateWithJoinsBuilder IntoTable<TTable>()
        {
            return new SqlUpdateWithJoinsBuilder(typeof(TTable));
        }

        /// <inheritdoc />
        private SqlUpdateWithJoinsBuilder(Type tPrimaryTable)
        {
            TypeTableMap.TryAdd(tPrimaryTable, true);
            _additionalColumnsWithValues = null;
            _columnMappings = new List<string>();
            Joins = new SqlJoin(TypeTableMap);
            Where = new SqlWhere(TypeTableMap);
        }
    }
}
