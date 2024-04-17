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
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="parameters">The parameters for the WHERE clause. Each parameter is combined using 'AND'. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <param name="sorting">Sorting for columns. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <param name="rowCount">Number of rows (TOP ??) to select. Zero or NULL for all rows.</param>
        /// <returns>Enumeration of object instances</returns>
        public IEnumerable<TObject> Select<TObject>(IDictionary<string, object?>? parameters = null, IDictionary<string, SortOrderEnum>? sorting = null, int? rowCount = null)
            => Select<TObject>(SQLScriptGenerator.GetSqlCommandForSelect<TObject>(parameters, sorting, rowCount));

        /// <summary>
        /// Executes a SELECT
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="query">Query to run on the SQL Server</param>
        /// <returns>Enumeration of object instances</returns>
        public IEnumerable<TObject> Select<TObject>(string query)
            => Select<TObject>(new SqlCommand(query));

        /// <summary>
        /// Executes a SELECT
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="queryBuilder">An instance of <see cref="SqlQueryBuilder"/></param>
        /// <returns>Enumeration of object instances</returns>
        public IEnumerable<TObject> Select<TObject>(SqlQueryBuilder queryBuilder)
            => Select<TObject>(new SqlCommand(queryBuilder.Build()));

        /// <summary>
        /// Executes a SELECT
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="query">Query to run on the SQL Server</param>
        /// <returns>Enumeration of object instances</returns>
        public IEnumerable<TObject> Select<TObject>(SqlCommand query)
            => ExecuteQuery<TObject>(query);


        /// <summary>
        /// Executes a SELECT and returns a single result or NULL (if none found)
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="parameters">The parameters for the WHERE clause. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <param name="sorting">Sorting for columns. Key must be the TABLE COLUMN name and NOT the property name!</param>
        /// <returns>Single result or NULL</returns>
        public TObject? SelectOnlyResultOrNull<TObject>(IDictionary<string, object?>? parameters = null, IDictionary<string, SortOrderEnum>? sorting = null)           
            => SelectOnlyResultOrNull<TObject>(SQLScriptGenerator.GetSelectStatement<TObject>(parameters, sorting, 1));

        /// <summary>
        /// Executes a SELECT and returns a single result or NULL (if none found)
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="query">The query to run</param>
        /// <returns>Single result or NULL</returns>
        public TObject? SelectOnlyResultOrNull<TObject>(string query)           
            => SelectOnlyResultOrNull<TObject>(new SqlCommand(query));

        /// <summary>
        /// Executes a SELECT and returns a single result or NULL (if none found)
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="queryBuilder">An instance of <see cref="SqlQueryBuilder"/></param>
        /// <returns>Single result or NULL</returns>
        public TObject? SelectOnlyResultOrNull<TObject>(SqlQueryBuilder queryBuilder)
            => SelectOnlyResultOrNull<TObject>(new SqlCommand(queryBuilder.Top(1).Build()));

        /// <summary>
        /// Executes a SELECT and returns a single result or NULL (if none found)
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="query">The query to run</param>
        /// <returns>Single result or NULL</returns>
        public TObject? SelectOnlyResultOrNull<TObject>(SqlCommand query)
            => Select<TObject>(query).FirstOrDefault(default(TObject));
    }
}