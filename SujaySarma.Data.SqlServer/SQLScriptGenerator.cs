using SujaySarma.Data.Core.Constants;
using SujaySarma.Data.Core.Reflection;

using System.Collections.Generic;
using System.Linq;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Generates SQL Scripts
    /// </summary>
    public static class SQLScriptGenerator
    {

        
        /// <summary>
        /// Generate MERGE statement
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">Instance of object</param>
        /// <param name="AdditionalData">Additional data to be merged</param>
        /// <param name="AdditionalConditions">Additional conditions to check -- will be merged with 'AND'</param>
        /// <returns>SQL MERGE string</returns>
        public static string GetMergeStatement<TObject>(TObject instance, Dictionary<string, object?>? AdditionalData = null, List<string>? AdditionalConditions = null)
        {
            if (instance == null)
            {
                return string.Empty;
            }

            object? refInstance = instance;

            List<string> columnAssignments = new List<string>();
            List<string> whereClauseConditions = new List<string>();
            List<string> insertColumns = new List<string>();
            List<string> insertValues = new List<string>();

            ContainerTypeInfo containerInfo = TypeDiscoveryFactory.Resolve<TObject>();
            foreach (MemberTypeInfo memberInfo in containerInfo.Members.Values)
            {
                string? value = ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref refInstance, memberInfo));
                if (memberInfo.Column.IncludeFor == ColumnInclusionStrategy.Never)
                {
                    if (memberInfo.Column.IsSearchKey)
                    {
                        whereClauseConditions.Add($"{memberInfo.Column.CreateQualifiedName()} = {value}");
                    }
                }
                else if (memberInfo.Column.IncludeFor == ColumnInclusionStrategy.Updates)
                {
                    columnAssignments.Add($"{memberInfo.Column.CreateQualifiedName()} = {value}");
                }
                else if (memberInfo.Column.IncludeFor == ColumnInclusionStrategy.Inserts)
                {
                    insertColumns.Add(memberInfo.Column.CreateQualifiedName());
                    insertValues.Add(value);
                }
            }

            if ((AdditionalData != null) && (AdditionalData.Count > 0))
            {
                foreach (KeyValuePair<string, object?> entry in AdditionalData)
                {
                    string value = ReflectionUtils.GetSQLStringValue(entry.Value);
                    columnAssignments.Add($"[{entry.Key}] = {value}");
                    insertColumns.Add($"[{entry.Key}]");
                    insertValues.Add(value);
                }
            }

            string setClause = string.Join(", ", columnAssignments);
            string whereClause = string.Join(" AND ", whereClauseConditions.Concat(AdditionalConditions ?? new List<string>()));
            string insertClause = $"({string.Join(", ", insertColumns)}) VALUES ({string.Join(", ", insertValues)})";

            return $"MERGE INTO {containerInfo.Container.CreateQualifiedName()} AS Target" +
                $" USING (SELECT {string.Join(", ", insertValues)}) AS Source ({string.Join(", ", insertColumns)})" +
                $" ON {whereClause}" +
                $" WHEN MATCHED THEN" +
                $"     UPDATE SET {setClause}" +
                $" WHEN NOT MATCHED THEN" +
                $"     INSERT {insertClause};";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public static string GetTruncateStatement<TObject>()
        {
            ContainerTypeInfo containerInfo = TypeDiscoveryFactory.Resolve<TObject>();
            return $"TRUNCATE TABLE {containerInfo.Container.CreateQualifiedName()};";
        }
    }
}
