using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using SujaySarma.Data.SqlServer.Fluid.Tools;

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
            List<string> query = new List<string>(), additionalInfo = new List<string>(), whereClause = new List<string>();
            TypeTableAliasMap map = base.TypeTableMap.GetPrimaryTable();

            if ((_additionalColumnsWithValues != null) && (_additionalColumnsWithValues.Count > 0))
            {
                foreach (string addlColName in _additionalColumnsWithValues.Keys)
                {
                    additionalInfo.Add($"[{addlColName}] = {ReflectionUtils.GetSQLStringValue(_additionalColumnsWithValues[addlColName])}");
                }
            }

            if (Where.HasConditions)
            {
                whereClause.Add(Where.ToString());
            }

            query.Add($"UPDATE [{map.GetQualifiedTableName()}] SET");
            query.Add(string.Join(',', _columnMappings));

            if (additionalInfo.Count > 0)
            {
                query.Add(string.Join(',', additionalInfo));
            }

            if (whereClause.Count > 0)
            {
                query.Add(
                        string.Join(
                            ' ',
                            "WHERE",
                            string.Join(" AND ", whereClause)
                        )
                    );
            }

            return string.Join(' ', query);
        }

        /// <summary>
        /// Define additional columns to update (columns in table not defined in the primary CLR object). These columns are added to the END of the list.
        /// </summary>
        /// <param name="additionalColumnsWithValues">Additional columns with values. Items with columns already defined are replaced. The same values are updated for every row!</param>
        /// <returns>Self-instance</returns>
        public SqlUpdateWithJoinsBuilder WithAdditionalColumns(Dictionary<string, object?> additionalColumnsWithValues)
        {
            _additionalColumnsWithValues ??= new Dictionary<string, object?>();
            foreach (string colName in additionalColumnsWithValues.Keys)
            {
                _additionalColumnsWithValues[colName] = additionalColumnsWithValues[colName];
            }

            return this;
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
            TypeTableAliasMap? primaryMap = base.TypeTableMap.GetMap<TPrimaryTable>();
            TypeTableAliasMap? joinMap = base.TypeTableMap.GetMap<TJoinTable>();

            if (typeof(TPrimaryTable) == typeof(TJoinTable))
            {
                throw new ArgumentException($"Type of '{typeof(TPrimaryTable).Name}' must be different from '{typeof(TJoinTable).Name}'.");
            }

            if ((primaryMap == null) || (!primaryMap.IsPrimaryTable))
            {
                throw new ArgumentException($"The primary table object '{typeof(TPrimaryTable).Name}' should have been added to the query using 'IntoTable()'.");
            }

            if ((joinMap == null) || joinMap.IsPrimaryTable)
            {
                throw new ArgumentException($"The primary table object '{typeof(TJoinTable).Name}' should have been added to the query using 'Joins.Add()'.");
            }

            foreach (Expression expression in selectors)
            {
                _columnMappings.Add(base.ParseToSql(expression, treatAssignmentsAsAlias: true));
            }

            return this;
        }

        /// <summary>
        /// Define the primary CLR object (and its backing SQL Server table) that should be updated into.
        /// </summary>
        /// <typeparam name="TTable">Type of CLR object</typeparam>
        /// <returns>Created instance of SqlInsertFromQueryBuilder</returns>
        public static SqlUpdateWithJoinsBuilder IntoTable<TTable>()
            where TTable : class
            => new(typeof(TTable));

        /// <summary>
        /// Collection of WHERE conditions
        /// </summary>
        public SqlTableWhereConditionsCollection Where
        {
            get;
            init;
        }

        /// <summary>
        /// Collection of JOINs
        /// </summary>
        public SqlTableJoinsCollection Joins
        {
            get;
            init;
        }

        /// <inheritdoc />
        private SqlUpdateWithJoinsBuilder(Type tPrimaryTable)
            : base()
        {
            base.TypeTableMap.TryAdd(tPrimaryTable, isPrimaryTable: true);

            _additionalColumnsWithValues = null;
            _columnMappings = new List<string>();

            Joins = new SqlTableJoinsCollection(base.TypeTableMap);
            Where = new SqlTableWhereConditionsCollection(base.TypeTableMap);
        }

        private Dictionary<string, object?>? _additionalColumnsWithValues;
        private readonly List<string> _columnMappings;
    }
}
