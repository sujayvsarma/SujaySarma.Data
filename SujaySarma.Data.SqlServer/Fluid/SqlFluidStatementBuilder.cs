using SujaySarma.Data.Core.Constants;
using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.SqlServer.Fluid.AliasMaps;
using SujaySarma.Data.SqlServer.LinqParsers;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// Base class implemented by our fluid statement builders.
    /// </summary>
    public abstract class SqlFluidStatementBuilder
    {
        /// <summary>
        /// The table map. This is exposed to all the internal builders.
        /// </summary>
        internal TypeTableAliasMapCollection TypeTableMap { get; }

        /// <summary>
        /// Build the statement as a SQL.
        /// </summary>
        /// <returns>SQL statement string OR empty string if there is no valid SQL statement.</returns>
        public virtual string Build()
        {
            throw new NotImplementedException("Ouch! Someone wrote a Fluid-statement builder but forgot to implement the Build() function!");
        }

        /// <summary>
        /// A helper function to parse lambda expressions to SQL
        /// </summary>
        /// <param name="expression">Lambda expression to parse</param>
        /// <param name="treatAssignmentsAsAlias">[Optional] When set, tells the parser to treat any assignments in the expression as aliases. For eg: 'a = s.Id' will turn into 's.Id as [a]'</param>
        /// <returns>SQL string expression</returns>
        protected string ExpressionToSQL(Expression expression, bool treatAssignmentsAsAlias = false)
            => _visitor.ParseToSql(expression, treatAssignmentsAsAlias);

        /// <summary>
        /// Extracts the names of all column-possible members from the <typeparamref name="TTable"/> type and appends them to the <paramref name="columnNamesList"/> list.
        /// </summary>
        /// <typeparam name="TTable">Type of CLR object.</typeparam>
        protected void ExtractColumnNames<TTable>(SqlStatementType operationType, ref List<string> columnNamesList, bool useTableAliasName = false)
        {
            TypeTableAliasMap tableMap = TypeTableMap.GetMap<TTable>() 
                ?? throw new InvalidOperationException($"No table map found for type '{typeof(TTable).FullName}'! Please ensure you have registered the type with the builder before using it.");

            ExtractColumnNamesImpl(tableMap, operationType, ref columnNamesList, useTableAliasName);
        }

        /// <summary>
        /// Extracts the names of all column-possible members from the PRIMARY TABLE and appends them to the <paramref name="columnNamesList"/> list.
        /// </summary>
        protected void ExtractColumnNames(SqlStatementType operationType, ref List<string> columnNamesList, bool useTableAliasName = false)
        {
            TypeTableAliasMap tableMap = TypeTableMap.GetPrimaryTable()
                ?? throw new InvalidOperationException($"No primary table map found! Please ensure you have registered the type with the builder before using it.");

            ExtractColumnNamesImpl(tableMap, operationType, ref columnNamesList, useTableAliasName);
        }


        /// <summary>
        /// Split the information in the <paramref name="additionalColumns"/> dictionary into column names and a list of SQL-sanitised values.
        /// </summary>
        /// <param name="additionalColumns">Additional columns with values provided</param>
        /// <param name="columnNames">Names of columns only</param>
        /// <param name="sqlSanitisedValuesList">Values that have been processed into SQL-friendly strings</param>
        protected static void ProcessAdditionalColumns(Dictionary<string, object?>? additionalColumns, ref List<string> columnNames, ref List<string> sqlSanitisedValuesList)
        {
            if ((additionalColumns != null) && (additionalColumns.Count > 0))
            {
                foreach (KeyValuePair<string, object?> kvp in additionalColumns)
                {
                    columnNames.Add($"[{kvp.Key}]");
                    sqlSanitisedValuesList.Add(ReflectionUtils.GetSQLStringValue(kvp.Value));
                }
            }
        }

        /// <summary>
        /// Copy elements from <paramref name="source"/> to <paramref name="destination"/>
        /// </summary>
        /// <param name="source">Source dictionary</param>
        /// <param name="destination">Destination dictionary. May be NULL, if so, is initialised prior to population.</param>
        protected static void CopyTo(Dictionary<string, object?> source, ref Dictionary<string, object?>? destination)
        {
            destination ??= new Dictionary<string, object?>();
            foreach(KeyValuePair<string, object?> kvp in source)
            {
                destination[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Extracts values from a table-source object for an INSERT operation
        /// </summary>
        /// <typeparam name="TTable">Type of table object</typeparam>
        /// <param name="source">The source CLR object instance</param>
        /// <param name="sqlSanitisedValuesList">SQL-friendly values extracted from the object instance</param>
        protected void ExtractValuesForInsert<TTable>(TTable source, ref List<string> sqlSanitisedValuesList)
        {
            TypeTableAliasMap map = TypeTableMap.GetMap<TTable>()
                ?? throw new InvalidOperationException($"No table map found for type '{typeof(TTable).FullName}'! Please ensure you have registered the type with the builder before using it.");

            object? refSource = source;
            foreach (MemberTypeInfo member in map.Discovery.Members.Values)
            {
                if (member.Column.IncludeFor.HasFlag(ColumnInclusionStrategy.Inserts))
                {
                    sqlSanitisedValuesList.Add(ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref refSource, member)));
                }
            }
        }

        /// <summary>
        /// Extracts values from a table-source object for an INSERT operation
        /// </summary>
        /// <typeparam name="TTable">Type of table object</typeparam>
        /// <param name="source">The source CLR object instance</param>
        /// <param name="sqlSanitisedValuesList">SQL-friendly values extracted from the object instance</param>
        /// <param name="searchConditions">SQL-friendly list of COL=VALUE pairs to append to the primary WHERE clause</param>
        protected void ExtractValuesForUpdate<TTable>(TTable source, ref List<string> sqlSanitisedValuesList, ref List<string> searchConditions)
        {
            TypeTableAliasMap map = TypeTableMap.GetMap<TTable>()
                ?? throw new InvalidOperationException($"No table map found for type '{typeof(TTable).FullName}'! Please ensure you have registered the type with the builder before using it.");

            object? refSource = source;

            foreach (MemberTypeInfo member in map.Discovery.Members.Values)
            {
                if (member.Column.IncludeFor.HasFlag(ColumnInclusionStrategy.Updates))
                {
                    sqlSanitisedValuesList.Add(
                            $"[{member.Column.CreateQualifiedName()}]=" + ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref refSource, member))
                        );
                }

                if (member.Column.IsSearchKey)
                {
                    searchConditions.Add(
                        $"[{member.Column.CreateQualifiedName()}]=" + ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref refSource, member))
                    );
                }
            }
        }

        /// <summary>
        /// Extracts values from a table-source object for an INSERT operation
        /// </summary>
        /// <typeparam name="TTable">Type of table object</typeparam>
        /// <param name="source">The source CLR object instance</param>
        /// <param name="sqlSanitisedValuesList">SQL-friendly values extracted from the object instance</param>
        protected void ExtractValuesForMerge<TTable>(TTable source, ref Dictionary<string, string> sqlSanitisedValuesList)
        {
            TypeTableAliasMap map = TypeTableMap.GetMap<TTable>()
                ?? throw new InvalidOperationException($"No table map found for type '{typeof(TTable).FullName}'! Please ensure you have registered the type with the builder before using it.");

            object? refSource = source;

            foreach (MemberTypeInfo member in map.Discovery.Members.Values)
            {
                if (member.Column.IncludeFor.HasFlag(ColumnInclusionStrategy.Inserts)
                            || member.Column.IncludeFor.HasFlag(ColumnInclusionStrategy.Updates))
                {
                    sqlSanitisedValuesList.Add(
                            member.Column.CreateQualifiedName(),
                            ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref refSource, member))
                        );
                }
            }
        }


        /// <summary>
        /// Initialize. Only child classes are allowed to call me.
        /// </summary>
        protected SqlFluidStatementBuilder()
        {
            TypeTableMap = new TypeTableAliasMapCollection();
            _visitor = new SqlLambdaVisitor(TypeTableMap);
        }

        // Cached visitor to improve performance across ParseToSql calls.
        private readonly SqlLambdaVisitor _visitor;


        /// <summary>
        /// Extracts the names of all column-possible members from the PRIMARY TABLE and appends them to the <paramref name="columnNamesList"/> list.
        /// </summary>
        private static void ExtractColumnNamesImpl(TypeTableAliasMap tableMap, SqlStatementType operationType, ref List<string> columnNamesList, bool useTableAliasName = false)
        {
            foreach (MemberTypeInfo member in tableMap.Discovery.Members.Values)
            {
                string sqlColumnName = (useTableAliasName ? $"[{tableMap.Alias}]." : string.Empty) + member.Column.CreateQualifiedName();
                switch (operationType)
                {
                    case SqlStatementType.Query:
                        // All members are included in a query operation
                        columnNamesList.Add(sqlColumnName);
                        break;

                    case SqlStatementType.Insert:
                        if (member.Column.IncludeFor.HasFlag(ColumnInclusionStrategy.Inserts))
                        {
                            columnNamesList.Add(sqlColumnName);
                        }
                        break;

                    case SqlStatementType.Update:
                        if (member.Column.IncludeFor.HasFlag(ColumnInclusionStrategy.Updates))
                        {
                            columnNamesList.Add(sqlColumnName);
                        }
                        break;

                    case SqlStatementType.Upsert:
                        if (member.Column.IncludeFor.HasFlag(ColumnInclusionStrategy.Inserts)
                            || member.Column.IncludeFor.HasFlag(ColumnInclusionStrategy.Updates))
                        {
                            columnNamesList.Add(sqlColumnName);
                        }
                        break;

                    case SqlStatementType.Delete:
                        if (member.Column.IncludeFor.HasFlag(ColumnInclusionStrategy.Deletes))
                        {
                            columnNamesList.Add(sqlColumnName);
                        }
                        break;
                }
            }
        }
    }
}
