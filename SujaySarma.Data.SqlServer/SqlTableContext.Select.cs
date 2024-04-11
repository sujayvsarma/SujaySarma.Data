using System.Collections.Generic;
using System.Linq;

using Microsoft.Data.SqlClient;

using SujaySarma.Data.SqlServer.Fluid;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// SqlTableContext.Select() functions only
    /// </summary>
    public partial class SqlTableContext
    {
        /// <summary>
        /// Executes a SELECT
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="parameters">The parameters for the WHERE clause. Each parameter is combined using 'AND'. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <param name="sorting">Sorting for columns. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <param name="rowCount">Number of rows (TOP ??) to select. Zero or NULL for all rows.</param>
        /// <returns>Enumeration of object instances</returns>
        public IEnumerable<T> Select<T>(IDictionary<string, object?>? parameters = null, IDictionary<string, SortOrderEnum>? sorting = null, int? rowCount = null)
            where T : class
            => Select<T>(SQLScriptGenerator.GetSqlCommandForSelect<T>(parameters, sorting, rowCount));

        /// <summary>
        /// Executes a SELECT
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="query">Query to run on the SQL Server</param>
        /// <returns>Enumeration of object instances</returns>
        public IEnumerable<T> Select<T>(string query) where T : class
            => Select<T>(new SqlCommand(query));

        /// <summary>
        /// Executes a SELECT
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="queryBuilder">An instance of <see cref="SqlQueryBuilder"/></param>
        /// <returns>Enumeration of object instances</returns>
        public IEnumerable<T> Select<T>(SqlQueryBuilder queryBuilder) where T : class
            => Select<T>(new SqlCommand(queryBuilder.Build()));

        /// <summary>
        /// Executes a SELECT
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="query">Query to run on the SQL Server</param>
        /// <returns>Enumeration of object instances</returns>
        public IEnumerable<T> Select<T>(SqlCommand query) where T : class
            => ExecuteQuery<T>(query);


        /// <summary>
        /// Executes a SELECT and returns a single result or NULL (if none found)
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="parameters">The parameters for the WHERE clause. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <param name="sorting">Sorting for columns. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <returns>Single result or NULL</returns>
        public T? SelectOnlyResultOrNull<T>(IDictionary<string, object?>? parameters = null, IDictionary<string, SortOrderEnum>? sorting = null)
            where T : class
            => SelectOnlyResultOrNull<T>(SQLScriptGenerator.GetSelectStatement<T>(parameters, sorting, 1));

        /// <summary>
        /// Executes a SELECT and returns a single result or NULL (if none found)
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="query">The query to run</param>
        /// <returns>Single result or NULL</returns>
        public T? SelectOnlyResultOrNull<T>(string query)
            where T : class
            => SelectOnlyResultOrNull<T>(new SqlCommand(query));

        /// <summary>
        /// Executes a SELECT and returns a single result or NULL (if none found)
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="queryBuilder">An instance of <see cref="SqlQueryBuilder"/></param>
        /// <returns>Single result or NULL</returns>
        public T? SelectOnlyResultOrNull<T>(SqlQueryBuilder queryBuilder) where T : class
            => SelectOnlyResultOrNull<T>(new SqlCommand(queryBuilder.Top(1).Build()));

        /// <summary>
        /// Executes a SELECT and returns a single result or NULL (if none found)
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="query">The query to run</param>
        /// <returns>Single result or NULL</returns>
        public T? SelectOnlyResultOrNull<T>(SqlCommand query) where T : class
            => Select<T>(query).FirstOrDefault((T?)null);
    }
}