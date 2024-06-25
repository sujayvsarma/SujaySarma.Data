using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Data.SqlClient;

using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Generates SQL scripts
    /// </summary>
    public static class SQLScriptGenerator
    {
        /// <summary>
        /// Generate SELECT statement - Can generate only simple (without JOINs etc) SELECT statements. If no parameters are provided, selects all rows as per 
        /// the database-driven sorting order.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="parameters">The parameters for the WHERE clause. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <param name="sorting">Sorting for columns. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <param name="rowCount">Number of rows (TOP ??) to select. Zero or NULL for all rows.</param>
        /// <returns>SQL SELECT string</returns>
        public static string GetSelectStatement<TObject>(IDictionary<string, object?>? parameters = null, IDictionary<string, SortOrderEnum>? sorting = null, int? rowCount = null) 
        {
            List<string> whereClause = new List<string>();

            if ((parameters != null) && (parameters != default) && (parameters != default(IDictionary<string, object?>)) && (parameters.Count > 0))
            {
                foreach(string colName in parameters.Keys)
                {
                    whereClause.Add($"([{colName}] = {ReflectionUtils.GetSQLStringValue(parameters[colName])})");
                }
            }

            return GetSelectStatement<TObject>(string.Join(" AND ", whereClause), sorting, rowCount);
        }

        /// <summary>
        /// Generate SELECT statement - Can generate only simple (without JOINs etc) SELECT statements. If no parameters are provided, selects all rows as per 
        /// the database-driven sorting order.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="whereClause">Pre-composed WHERE clause -- without the "WHERE" word</param>
        /// <param name="sorting">Sorting for columns. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <param name="rowCount">Number of rows (TOP ??) to select. Zero or NULL for all rows.</param>
        /// <returns>SQL SELECT string</returns>
        public static string GetSelectStatement<TObject>(string? whereClause = null, IDictionary<string, SortOrderEnum>? sorting = null, int? rowCount = null)
        {
            List<string> columnNames = new List<string>(), sortClause = new List<string>();
            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve<TObject>();
            foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                if (member.ContainerMemberDefinition.IncludeInDataModificationOperation == Core.Constants.DataModificationInclusionBehaviour.Never)
                {
                    continue;
                }

                columnNames.Add($"{member.ContainerMemberDefinition.CreateQualifiedName()}");
            }

            if ((sorting != null) && (sorting != default) && (sorting != default(IDictionary<string, SortOrderEnum?>)) && (sorting.Count > 0))
            {
                foreach (string colName in sorting.Keys)
                {
                    sortClause.Add($"[{colName}] {sorting[colName]}");
                }
            }

            return string.Join(
                    ' ',
                        "SELECT",
                        (((rowCount == null) || (rowCount == default) || (rowCount == default(int)) || (rowCount < 1)) ? "" : $"TOP {rowCount}"),
                        string.Join(',', columnNames),
                        "FROM",
                        metadata.ContainerDefinition.CreateQualifiedName(),
                        "WITH (NOLOCK)",
                        ((!string.IsNullOrWhiteSpace(whereClause)) ? string.Join(' ', "WHERE", whereClause) : ""),
                        ((sortClause.Count > 0) ? string.Join(' ', "ORDER BY", string.Join(',', sortClause)) : "")
                );
        }


        /// <summary>
        /// Generate SELECT statement - Can generate only simple (without JOINs etc) SELECT statements. If no parameters are provided, selects all rows as per 
        /// the database-driven sorting order.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="parameters">The parameters for the WHERE clause. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <param name="sorting">Sorting for columns. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <param name="rowCount">Number of rows (TOP ??) to select. Zero or NULL for all rows.</param>
        /// <returns>SqlCommand with the parameterized statement</returns>
        public static SqlCommand GetSqlCommandForSelect<TObject>(IDictionary<string, object?>? parameters = null, IDictionary<string, SortOrderEnum>? sorting = null, int? rowCount = null)
        {
            SqlCommand cmd = new SqlCommand(GetSelectStatement<TObject>(parameters, sorting, rowCount));
            if ((parameters != null) && (parameters != default) && (parameters != default(IDictionary<string, object?>)) && (parameters.Count > 0))
            {
                foreach (string colName in parameters.Keys)
                {
                    cmd.Parameters.AddWithValue($"@param{colName}", parameters[colName]);
                }
            }

            return cmd;
        }

        /// <summary>
        /// Generate INSERT statement
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">Instance of object</param>
        /// <param name="AdditionalData">Additional data to be inserted</param>
        /// <returns>SQL INSERT string</returns>
        public static string GetInsertStatement<TObject>(TObject instance, Dictionary<string, object?>? AdditionalData = null)
        {
            if (instance == null)
            {
                return string.Empty;
            }

            object duplInstance = instance;

            List<string> columnNames = new List<string>(), values = new List<string>();
            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve<TObject>();

            foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                if (member.ContainerMemberDefinition.IncludeInDataModificationOperation == Core.Constants.DataModificationInclusionBehaviour.Never)
                {
                    continue;
                }

                string sqlValue = ReflectionUtils.GetSQLStringValue(
                        SujaySarma.Data.Core.Reflection.ReflectionUtils.GetValue(ref duplInstance!, member)
                    );

                columnNames.Add(member.ContainerMemberDefinition.CreateQualifiedName());
                values.Add(sqlValue);
            }

            if ((AdditionalData != null) && (AdditionalData.Count > 0))
            {
                foreach(string key in AdditionalData.Keys)
                {
                    columnNames.Add($"[{key}]");
                    values.Add(ReflectionUtils.GetSQLStringValue(AdditionalData[key]));
                }
            }

            return string.Join(
                    ' ',
                        "INSERT INTO",
                        metadata.ContainerDefinition.CreateQualifiedName(),
                        "(",
                        string.Join(',', columnNames),
                        ") VALUES (",
                        string.Join(',', values),
                        ");"
                );
        }

        /// <summary>
        /// Generate UPDATE statement
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">Instance of object</param>
        /// <param name="AdditionalData">Additional data to be updated</param>
        /// <param name="AdditionalConditions">Additional conditions to check -- will be merged with 'AND'</param>
        /// <returns>SQL UPDATE string</returns>
        public static string GetUpdateStatement<TObject>(TObject instance, Dictionary<string, object?>? AdditionalData = null, List<string>? AdditionalConditions = null)
        {
            if (instance == null)
            {
                return string.Empty;
            }

            object duplInstance = instance;
            List<string> conditions = new List<string>(), updateValues = new List<string>();
            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve<TObject>();
            foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                string sqlValue = ReflectionUtils.GetSQLStringValue(
                        SujaySarma.Data.Core.Reflection.ReflectionUtils.GetValue(ref duplInstance!, member)
                    );

                switch (member.ContainerMemberDefinition.IncludeInDataModificationOperation)
                {
                    case Core.Constants.DataModificationInclusionBehaviour.Inserts:
                    case Core.Constants.DataModificationInclusionBehaviour.Never:
                        if (member.ContainerMemberDefinition.IsSearchKey)
                        {
                            conditions.Add($"({member.ContainerMemberDefinition.CreateQualifiedName()} = {sqlValue})");
                        }
                        break;

                    case Core.Constants.DataModificationInclusionBehaviour.Updates:
                        updateValues.Add($"({member.ContainerMemberDefinition.CreateQualifiedName()} = {sqlValue})");
                        break;
                }
            }

            if ((AdditionalData != null) && (AdditionalData.Count > 0))
            {
                foreach (string key in AdditionalData.Keys)
                {
                    updateValues.Add($"[{key}] = {ReflectionUtils.GetSQLStringValue(AdditionalData[key])}");
                }
            }

            if ((AdditionalConditions != null) && (AdditionalConditions.Count > 0))
            {
                foreach (string item in AdditionalConditions)
                {
                    conditions.Add(item);
                }
            }

            return string.Join(
                    ' ',
                    "UPDATE",
                    metadata.ContainerDefinition.CreateQualifiedName(),
                    "SET",
                    string.Join(',', updateValues),
                    ((conditions.Count > 0) ? $"WHERE ({string.Join(" AND ", conditions)})" : ""),
                    ";"
                );
        }

        /// <summary>
        /// Generate a T-SQL MERGE (UPSERT) statement
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">Instance of object</param>
        /// <returns>SQL MERGE string</returns>
        public static string GetMergeStatement<TObject>(TObject instance)
        {
            if (instance == null)
            {
                return string.Empty;
            }

            object duplInstance = instance;
            List<string> columnNames = new List<string>(), values = new List<string>(), joinConditions = new List<string>();
            string insertStatement = string.Empty, updateStatement = string.Empty;

            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve<TObject>();
            foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                if (member.ContainerMemberDefinition.IncludeInDataModificationOperation == Core.Constants.DataModificationInclusionBehaviour.Never)
                {
                    continue;
                }

                string columnName = member.ContainerMemberDefinition.CreateQualifiedName();
                string sqlValue = ReflectionUtils.GetSQLStringValue(
                        SujaySarma.Data.Core.Reflection.ReflectionUtils.GetValue(ref duplInstance!, member)
                    );

                columnNames.Add(columnName);
                values.Add(sqlValue);
                
                if (member.ContainerMemberDefinition.IsSearchKey)
                {
                    joinConditions.Add($"(target.{columnName} = source.{columnName})");
                }
            }

            insertStatement = string.Join(
                    ' ',
                        "INSERT",
                        "(",
                        string.Join(',', columnNames),
                        ") VALUES (",
                        string.Join(',', columnNames.Select(n => $"source.[{n}]")),
                        ");"
                );

            updateStatement = string.Join(
                    ' ',
                        "UPDATE SET",
                        string.Join(',', columnNames.Select(n => $"[{n}] = source.[{n}]"))
                );

            return string.Join(
                    ' ',
                        "MERGE",
                        $"{metadata.ContainerDefinition.CreateQualifiedName()} as target",
                        "USING (VALUES(",
                        string.Join(',', values),
                        ")) AS source (",
                        string.Join(',', columnNames),
                        ") ON (",
                        string.Join(" AND ", joinConditions),
                        ") WHEN MATCHED THEN",
                        updateStatement,
                        "WHEN NOT MATCHED THEN",
                        insertStatement
                );
        }


        /// <summary>
        /// Generate DELETE statement
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">Instance of object</param>
        /// <param name="AdditionalConditions">Additional conditions to check -- will be merged with 'AND'</param>
        /// <returns>SQL DELETE string</returns>
        public static string GetDeleteStatement<TObject>(TObject instance, List<string>? AdditionalConditions = null)
        {
            if (instance == null)
            {
                return string.Empty;
            }

            object duplInstance = instance;
            List<string> conditions = new List<string>();
            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve<TObject>();
            foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                if (member.ContainerMemberDefinition.IncludeInDataModificationOperation == Core.Constants.DataModificationInclusionBehaviour.Never)
                {
                    continue;
                }

                string sqlValue = ReflectionUtils.GetSQLStringValue(
                        SujaySarma.Data.Core.Reflection.ReflectionUtils.GetValue(ref duplInstance!, member)
                    );

                conditions.Add($"({member.ContainerMemberDefinition.CreateQualifiedName()} = {sqlValue})");
            }

            if ((AdditionalConditions != null) && (AdditionalConditions.Count > 0))
            {
                foreach (string item in AdditionalConditions)
                {
                    conditions.Add(item);
                }
            }

            if (metadata.ContainerDefinition.UseSoftDelete)
            {
                return string.Join(
                    ' ',
                    "UPDATE",
                    $"{metadata.ContainerDefinition.CreateQualifiedName()}",
                    $"SET [{ReservedNames.IsDeleted}] = 1",
                    ((conditions.Count > 0) ? $"WHERE ({string.Join(" AND ", conditions)})" : ""),
                    ";"
                );
            }

            return string.Join(
                    ' ',
                    "DELETE FROM",
                    $"{metadata.ContainerDefinition.CreateQualifiedName()}",
                    ((conditions.Count > 0) ? $"WHERE ({string.Join(" AND ", conditions)})" : ""),
                    ";"
                );
        }
    }
}
