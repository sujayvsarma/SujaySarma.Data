using SujaySarma.Data.SqlServer.Fluid.AliasMaps;

using System;
using System.Collections.Generic;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// Fluid builder for 'INSERT FROM SELECT' statements. Allows inserting data into columns not a part of the primary table.
    /// </summary>
    public class SqlInsertFromBuilder : SqlFluidStatementBuilder
    {
        /// <inheritdoc />
        public override string Build()
        {
            if (_queryBuilder == null)
            {
                throw new InvalidOperationException("FromQuery() should be called before calling Build()");
            }

            TypeTableAliasMap primaryTable = this.TypeTableMap.GetPrimaryTable();

            List<string> insertTableColumnNamesList = new List<string>();
            base.ExtractColumnNames(SqlStatementType.Insert, ref insertTableColumnNamesList, useTableAliasName: true);

            List<string> additionalValuesList = new List<string>();
            ProcessAdditionalColumns(_additionalColumnsWithValues, ref insertTableColumnNamesList, ref additionalValuesList);

            if ((_additionalColumnsWithValues != null) && (_additionalColumnsWithValues.Count > 0))
            {
                _queryBuilder.InjectAdditionalValues(_additionalColumnsWithValues);
            }

            return string.Join(' ', 
                    "INSERT INTO", 
                    primaryTable.GetQualifiedTableName(), 
                    "(", string.Join(',', insertTableColumnNamesList), ")", 
                    _queryBuilder.Build()
                );
        }

        private SqlQueryBuilder? _queryBuilder;
        private Dictionary<string, object?>? _additionalColumnsWithValues;

        /// <summary>
        /// Register a SELECT query that is to be executed to fetch data that is to be inserted.
        /// </summary>
        /// <param name="queryBuilder">The SELECT query that is to be executed</param>
        /// <returns>Self-instance</returns>
        public SqlInsertFromBuilder FromQuery(SqlQueryBuilder queryBuilder)
        {
            _queryBuilder = ((_queryBuilder == null) ? queryBuilder : throw new InvalidOperationException("FromQuery() has already been called for this builder sequence."));
            return this;
        }

        /// <summary>
        /// Define additional columns to insert (columns in table not defined in the primary CLR object). These columns are added to the END of the list.
        /// </summary>
        /// <param name="additionalColumnsWithValues">Additional columns with values. Items with columns already defined are replaced. The same values are inserted for every row!</param>
        /// <returns>Self-instance</returns>
        public SqlInsertFromBuilder WithAdditionalColumns(Dictionary<string, object?> additionalColumnsWithValues)
        {
            CopyTo(additionalColumnsWithValues, ref _additionalColumnsWithValues);
            return this;
        }

        /// <summary>
        /// Define the primary CLR object (and its backing SQL Server table) that should be inserted into.
        /// </summary>
        /// <typeparam name="TTable">Type of CLR object</typeparam>
        /// <returns>Created instance of SqlInsertFromQueryBuilder</returns>
        public static SqlInsertFromBuilder IntoTable<TTable>()
            => new SqlInsertFromBuilder(typeof(TTable));

        /// <inheritdoc />
        private SqlInsertFromBuilder(Type tTableType)
        {
            TypeTableMap.TryAdd(tTableType, true);
            _queryBuilder = null;
            _additionalColumnsWithValues = null;
        }

    }
}
