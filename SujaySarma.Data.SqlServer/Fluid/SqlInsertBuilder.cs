using System.Collections.Generic;

using SujaySarma.Data.Core.Constants;
using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.SqlServer.Fluid.Tools;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// Fluid builder for 'INSERT INTO VALUES' statement and operations. Allows inserting data into columns not a part of <typeparamref name="TTable"/>. 
    /// Handles simple insert including multiple inserts into the same table. 
    /// </summary>
    /// <typeparam name="TTable">Type of business object mapped to the table being inserted into</typeparam>
    public class SqlInsertBuilder<TTable> : SqlFluidStatementBuilder
    {

        /// <inheritdoc />
        public override string Build()
        {
            TypeTableAliasMap tableMap = base.TypeTableMap.GetPrimaryTable();

            List<string> columnNames = new List<string>();
            List<string?> values = new List<string?>(), additionalValues = new List<string?>();
            foreach (ContainerMemberTypeInformation member in tableMap.Discovery.Members.Values)
            {
                if (member.ContainerMemberDefinition.IncludeInDataModificationOperation == DataModificationInclusionBehaviour.Never)
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

            foreach (TTable item in _insertSourceList)
            {
                List<string?> rowValues = new List<string?>();
                foreach (ContainerMemberTypeInformation member in tableMap.Discovery.Members.Values)
                {
                    if (member.ContainerMemberDefinition.IncludeInDataModificationOperation == DataModificationInclusionBehaviour.Never)
                    {
                        continue;
                    }

                    rowValues.Add(
                            SujaySarma.Data.SqlServer.ReflectionUtils.GetSQLStringValue(
                                SujaySarma.Data.Core.Reflection.ReflectionUtils.GetValue(item, member)
                            )
                   );
                }

                if (additionalValues.Count > 0)
                {
                    rowValues.AddRange(additionalValues);
                }

                if (rowValues.Count > 0)
                {
                    values.Add(string.Join(',', rowValues));
                }
            }

            if (values.Count > 0)
            {
                return string.Join(
                        ' ',
                            "INSERT INTO",
                            tableMap.GetQualifiedTableName(),
                            "(",
                            string.Join(',', columnNames),
                            ") VALUES (",
                            string.Join(',', values),
                            ");"
                    );
            }

            return string.Empty;
        }        

        /// <summary>
        /// Define additional columns to insert (columns in table not defined in the <typeparamref name="TTable"/> object). These columns are added to the END of the list.
        /// </summary>
        /// <param name="additionalColumnsWithValues">Additional columns with values. Items with columns already defined are replaced. The same values are inserted for every row!</param>
        /// <returns>Self-instance</returns>
        public SqlInsertBuilder<TTable> WithAdditionalColumns(Dictionary<string, object?> additionalColumnsWithValues)
        {
            _additionalColumnsWithValues ??= new Dictionary<string, object?>();

            foreach (string colName in additionalColumnsWithValues.Keys)
            {
                _additionalColumnsWithValues[colName] = additionalColumnsWithValues[colName];
            }

            return this;
        }

        /// <summary>
        /// Add the object instances that contain the data to be inserted
        /// </summary>
        /// <param name="items">Object instances with data</param>
        /// <returns>Self-instance</returns>
        public SqlInsertBuilder<TTable> AddItems(params TTable[] items)
        {
            _insertSourceList.AddRange(items);
            return this;
        }

        /// <summary>
        /// Defines the primary CLR object (and hence it's backing SQL Server table) that should be used 
        /// to populate the query. Any unqualified column references are implicitly assumed to be homed 
        /// in this object/table.
        /// </summary>
        /// <returns>Created instance of SqlQueryBuilder</returns>
        public static SqlInsertBuilder<TTable> Begin()
            => new SqlInsertBuilder<TTable>();

        /// <inheritdoc />
        private SqlInsertBuilder() : base()
        {
            base.TypeTableMap.TryAdd<TTable>(isPrimaryTable: true);

            _insertSourceList = new List<TTable>();
            _additionalColumnsWithValues = null;
        }

        private readonly List<TTable> _insertSourceList;
        private Dictionary<string, object?>? _additionalColumnsWithValues;
        
    }
}
