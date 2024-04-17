using System;
using System.Collections.Generic;

using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.SqlServer.Fluid.Tools;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// Fluid builder for 'INSERT FROM SELECT' statements. Allows inserting data into columns not a part of the primary table. 
    /// </summary>
    public class SqlInsertFromQueryBuilder : SqlFluidStatementBuilder
    {
        /// <inheritdoc />
        public override string Build()
        {
            if (_queryBuilder == null)
            {
                throw new InvalidOperationException("FromQuery() should be called before calling Build()");
            }

            TypeTableAliasMap tableMap = base.TypeTableMap.GetPrimaryTable();
            List<string> columnNames = new List<string>();
            List<string?> additionalValues = new List<string?>();
            foreach (ContainerMemberTypeInformation member in tableMap.Discovery.Members.Values)
            {
                if (member.ContainerMemberDefinition.IncludeInDataModificationOperation == Core.Constants.DataModificationInclusionBehaviour.Never)
                {
                    continue;
                }

                columnNames.Add(member.ContainerMemberDefinition.CreateQualifiedName());
            }

            if ((_additionalColumnsWithValues != null) && (_additionalColumnsWithValues.Count > 0))
            {
                foreach (string addlColName in _additionalColumnsWithValues.Keys)
                {
                    columnNames.Add($"[{addlColName}]");
                    additionalValues.Add(
                            SujaySarma.Data.SqlServer.ReflectionUtils.GetSQLStringValue(
                                _additionalColumnsWithValues[addlColName]
                            )
                        );
                }
            }

            if ((_additionalColumnsWithValues != null) && (_additionalColumnsWithValues.Count > 0))
            {
                _queryBuilder.InjectAdditionalValues(_additionalColumnsWithValues);
            }

            string selectFromQuery = _queryBuilder.Build();
            return string.Join(
                    ' ',
                        "INSERT INTO",
                        tableMap.GetQualifiedTableName(),
                        "(",
                        string.Join(',', columnNames),
                        ")",
                        selectFromQuery
                );
        }

        /// <summary>
        /// Register a SELECT query that is to be executed to fetch data that is to be inserted.
        /// </summary>
        /// <param name="queryBuilder">The SELECT query that is to be executed</param>
        /// <returns>Self-instance</returns>
        public SqlInsertFromQueryBuilder FromQuery(SqlQueryBuilder queryBuilder)
        {
            if (_queryBuilder != null)
            {
                throw new InvalidOperationException("FromQuery() has already been called for this builder sequence.");
            }
            _queryBuilder = queryBuilder;
            return this;
        }

        /// <summary>
        /// Define additional columns to insert (columns in table not defined in the primary CLR object). These columns are added to the END of the list.
        /// </summary>
        /// <param name="additionalColumnsWithValues">Additional columns with values. Items with columns already defined are replaced. The same values are inserted for every row!</param>
        /// <returns>Self-instance</returns>
        public SqlInsertFromQueryBuilder WithAdditionalColumns(Dictionary<string, object?> additionalColumnsWithValues)
        {
            _additionalColumnsWithValues ??= new Dictionary<string, object?>();
            foreach (string colName in additionalColumnsWithValues.Keys)
            {
                _additionalColumnsWithValues[colName] = additionalColumnsWithValues[colName];
            }

            return this;
        }

        /// <summary>
        /// Define the primary CLR object (and its backing SQL Server table) that should be inserted into.
        /// </summary>
        /// <typeparam name="TTable">Type of CLR object</typeparam>
        /// <returns>Created instance of SqlInsertFromQueryBuilder</returns>
        public static SqlInsertFromQueryBuilder IntoTable<TTable>()
            => new SqlInsertFromQueryBuilder(typeof(TTable));

        /// <inheritdoc />
        private SqlInsertFromQueryBuilder(Type tTableType) : base()
        {
            base.TypeTableMap.TryAdd(tTableType, isPrimaryTable: true);

            _queryBuilder = null;
            _additionalColumnsWithValues = null;
        }

        private SqlQueryBuilder? _queryBuilder;
        private Dictionary<string, object?>? _additionalColumnsWithValues;
    }
}
