using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.SqlServer.Fluid;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class SqlTableContextExtensions
    {

        /// <summary>
        /// Create an instance of the given <paramref name="TObject"/> type and populate it using the provided <see cref="DataRow"/>
        /// </summary>
        /// <param name="TObject">Type of business object</param>
        /// <param name="row"><see cref="DataRow"/> containing data to populate into the instance</param>
        /// <returns>Instance of object. Never NULL</returns>
        public static object HydrateFrom(Type TObject, DataRow row)
        {
            if ((row.Table == default) || (row.Table.Columns.Count == 0))
            {
                throw new TypeLoadException($"The DataRow passed is not attached to a table, or the table has no schema. Object: '{TObject.Name}'");
            }

            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve(TObject) ?? throw new TypeLoadException($"Type '{TObject.Name}' is not appropriately decorated.");
            object instance = Activator.CreateInstance(TObject) ?? new TypeLoadException($"Unable to instantiate object of type '{TObject.Name}'.");

            foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                string columnName = member.ContainerMemberDefinition.CreateQualifiedName();
                if (row.Table.Columns.Contains(columnName))
                {
                    object? value = row[columnName];
                    if ((value is DBNull) || (value == DBNull.Value))
                    {
                        value = null;
                    }

                    if (member.ContainerMemberDefinition.AllowSerializationAsJson)
                    {
                        value = JsonSerializer.Deserialize($"{value ?? string.Empty}", SujaySarma.Data.Core.Reflection.ReflectionUtils.GetFieldOrPropertyDataType(member.FieldOrPropertyInfo));
                    }

                    SujaySarma.Data.Core.Reflection.ReflectionUtils.SetValue(ref instance, member, value);
                }
            }            

            return instance;
        }


        /// <summary>
        /// Create an instance of the given <typeparamref name="TObject"/> type and populate it using the provided <see cref="DataRow"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="row"><see cref="DataRow"/> containing data to populate into the instance</param>
        /// <returns>Instance of object. Never NULL</returns>
        public static TObject? HydrateFrom<TObject>(DataRow row)           
            => (TObject?)HydrateFrom(typeof(TObject), row);

        /// <summary>
        /// Use the provided <paramref name="tableContext"/> to perform the query in <paramref name="queryBuilder"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="queryBuilder">Instance of SqlQueryBuilder with the necessary parameters</param>
        /// <param name="tableContext">SqlTableContext to use to perform the query</param>
        /// <returns>IEnumerable collection of [T] returned by the query</returns>
        public static IEnumerable<TObject> Query<TObject>(this SqlQueryBuilder queryBuilder, SqlTableContext tableContext)           
            => tableContext.Select<TObject>(queryBuilder);

        /// <summary>
        /// Use the provided <paramref name="tableContext"/> to perform the query in <paramref name="queryBuilder"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="queryBuilder">Instance of SqlQueryBuilder with the necessary parameters</param>
        /// <param name="tableContext">SqlTableContext to use to perform the query</param>
        /// <returns>Single instance [T] or NULL as returned by the query</returns>
        public static TObject? QueryOneOrNull<TObject>(this SqlQueryBuilder queryBuilder, SqlTableContext tableContext)           
            => tableContext.SelectOnlyResultOrNull<TObject>(queryBuilder);


        /// <summary>
        /// Execute the INSERT query provided in <paramref name="insertBuilder"/> against the <paramref name="tableContext"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="insertBuilder">Instance of SqlInsertBuilder</param>
        /// <param name="tableContext">SqlTableContext to use to execute the query</param>
        /// <returns>Number of rows affected on the SQL Server</returns>
        public static async Task<int> ExecuteAsync<TObject>(this SqlInsertBuilder<TObject> insertBuilder, SqlTableContext tableContext)           
            => await tableContext.ExecuteNonQueryAsync(insertBuilder.Build());

        /// <summary>
        /// Execute the INSERT query provided in <paramref name="insertBuilder"/> against the <paramref name="tableContext"/>
        /// </summary>
        /// <param name="insertBuilder">Instance of SqlInsertFromQueryBuilder</param>
        /// <param name="tableContext">SqlTableContext to use to execute the query</param>
        /// <returns>Number of rows affected on the SQL Server</returns>
        public static async Task<int> ExecuteAsync(this SqlInsertFromQueryBuilder insertBuilder, SqlTableContext tableContext)
            => await tableContext.ExecuteNonQueryAsync(insertBuilder.Build());

        /// <summary>
        /// Execute the UPDATE query provided in <paramref name="updateBuilder"/> against the <paramref name="tableContext"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="updateBuilder">Instance of SqlUpdateBuilder</param>
        /// <param name="tableContext">SqlTableContext to use to execute the query</param>
        /// <returns>Number of rows affected on the SQL Server</returns>
        public static async Task<int> ExecuteAsync<TObject>(this SqlUpdateBuilder<TObject> updateBuilder, SqlTableContext tableContext)           
            => await tableContext.ExecuteNonQueryAsync(updateBuilder.Build());

        /// <summary>
        /// Execute the UPDATE query provided in <paramref name="updateBuilder"/> against the <paramref name="tableContext"/>
        /// </summary>
        /// <param name="updateBuilder">Instance of SqlUpdateWithJoinsBuilder</param>
        /// <param name="tableContext">SqlTableContext to use to execute the query</param>
        /// <returns>Number of rows affected on the SQL Server</returns>
        public static async Task<int> ExecuteAsync(this SqlUpdateWithJoinsBuilder updateBuilder, SqlTableContext tableContext)
            => await tableContext.ExecuteNonQueryAsync(updateBuilder.Build());

        /// <summary>
        /// Execute the DELETE query provided in <paramref name="deleteBuilder"/> against the <paramref name="tableContext"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="deleteBuilder">Instance of SqlDeleteBuilder</param>
        /// <param name="tableContext">SqlTableContext to use to execute the query</param>
        /// <returns>Number of rows affected on the SQL Server</returns>
        public static async Task<int> ExecuteAsync<TObject>(this SqlDeleteBuilder<TObject> deleteBuilder, SqlTableContext tableContext)           
            => await tableContext.ExecuteNonQueryAsync(deleteBuilder.Build());


        /// <summary>
        /// Enables dumping of SQL statements to the console/log
        /// </summary>
        public static void EnableDebugging(this SqlTableContext _) 
            => Environment.SetEnvironmentVariable(SqlTableContext.DUMP_SQL_FLAG, "true");

        /// <summary>
        /// Disables dumping of SQL statements to the console/log
        /// </summary>
        /// <param name="_"></param>
        public static void DisableDebugging(this SqlTableContext _)
            => Environment.SetEnvironmentVariable(SqlTableContext.DUMP_SQL_FLAG, null);

    }
}
