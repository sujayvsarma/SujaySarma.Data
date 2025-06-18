using SujaySarma.Data.SqlServer.Fluid.AliasMaps;

using System;
using System.Collections.Generic;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// Fluid builder for 'INSERT INTO VALUES' statement and operations. Allows inserting data into columns not a part of <typeparamref name="TTable" />.
    /// Handles simple insert including multiple inserts into the same table.
    /// </summary>
    /// <typeparam name="TTable">Type of business object mapped to the table being inserted into</typeparam>
    public class SqlInsertBuilder<TTable> : SqlFluidStatementBuilder
    {

        /// <inheritdoc />
        public override string Build()
        {
            if (_insertSourceList.Count == 0)
            {
                throw new InvalidOperationException("Cannot build an INSERT statement without any items to insert!");
            }
            
            // Primary table map with metadata
            TypeTableAliasMap primaryTable = TypeTableMap.GetPrimaryTable();

            // Contains the names of the table's columns. We will APPEND the additional columns to the END of this list.
            List<string> tableColumnNamesList = new List<string>();
            base.ExtractColumnNames(SqlStatementType.Query, ref tableColumnNamesList, false);

            // Contains the values from the _additionalColumnsWithValues dictionary in the same order as the corresponding names appended in the tableColumnNamesList list.
            List<string> arbitraryInsertValuesList = new List<string>();

            ProcessAdditionalColumns(_additionalColumnsWithValues, ref tableColumnNamesList, ref arbitraryInsertValuesList);

            // Contains the values to be inserted. Each item in this list is a comma-separated string of values for each row to be inserted.
            List<string> insertCommandValuesList = new List<string>();

            // Add values
            foreach(TTable insertSource in _insertSourceList)
            {
                List<string> rowValuesList = new List<string>();
                base.ExtractValuesForInsert<TTable>(insertSource, ref rowValuesList);

                if (arbitraryInsertValuesList.Count > 0)
                {
                    rowValuesList.AddRange(arbitraryInsertValuesList);
                }

                // Combine all the values we collected into a single string.
                if (rowValuesList.Count > 0)
                {
                    insertCommandValuesList.Add("(" + string.Join(',', rowValuesList) + ")");
                }
            }

            // Don't combine if there are no rows to insert, instead, return an empty string.
            if (insertCommandValuesList.Count == 0)
            {
                return string.Empty;
            }

            // Combine everything
            return string.Join(' ', 
                    "INSERT INTO",
                    primaryTable.GetQualifiedTableName(),
                    "(",
                    string.Join(',', tableColumnNamesList),
                    ") VALUES ",
                    string.Join(',', insertCommandValuesList),
                    ";"
                );
        }


        /// <summary>
        /// Define additional columns to insert (columns in table not defined in the <typeparamref name="TTable" /> object). These columns are added to the END of the list.
        /// </summary>
        /// <param name="additionalColumnsWithValues">Additional columns with values. Items with columns already defined are replaced. The same values are inserted for every row!</param>
        /// <returns>Self-instance</returns>
        public SqlInsertBuilder<TTable> WithAdditionalColumns(Dictionary<string, object?> additionalColumnsWithValues)
        {
            CopyTo(additionalColumnsWithValues, ref _additionalColumnsWithValues);
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

        private readonly List<TTable> _insertSourceList;
        private Dictionary<string, object?>? _additionalColumnsWithValues;

        /// <summary>
        /// Defines the primary CLR object (and hence it's backing SQL Server table) that should be used
        /// to populate the query. Any unqualified column references are implicitly assumed to be homed
        /// in this object/table.
        /// </summary>
        /// <returns>Created instance of SqlQueryBuilder</returns>
        public static SqlInsertBuilder<TTable> Begin() 
            => new SqlInsertBuilder<TTable>();

        /// <inheritdoc />
        private SqlInsertBuilder()
        {
            TypeTableMap.TryAdd<TTable>(true);
            _insertSourceList = new List<TTable>();
            _additionalColumnsWithValues = null;
        }
    }
}
