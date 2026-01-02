using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.SqlServer.Builders;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SujaySarma.Data.SqlServer;

// Implementation of: SELECT queries.
public partial class SqlContext
{
    /// <summary>
    /// Executes the provided SQL SELECT <paramref name="query"/> and returns a single hydrated instance 
    /// of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity to hydrate from the data returned by <paramref name="query"/>.</typeparam>
    /// <param name="query">The SQL SELECT query to execute.</param>
    /// <returns>An instance of type <typeparamref name="TEntity"/>. If the query returns multiple tables or rows, this function 
    /// will use only the first row of the first table. If no data was returned, a <see cref="SqlContextException"/> is thrown.</returns>
    public async Task<TEntity> SelectOneAsync<TEntity>(SqlQueryBuilder query)
    {
        PersistenceContainerInfo container = typeof(TEntity).RetrievePersistenceContainerInfoOrThrowException();

        Result result = SqlExecute.Query(_connectionString, query);
        if (result is ErrorResult error)
        {
            throw new SqlContextException($"Error selecting entity: {error.Message}", error.Error);
        }

        QueryResult data = (QueryResult)result;
        if ((data.Data.Tables.Count is 0) || (data.Data.Tables[0].Rows.Count is 0))
        {
            throw new SqlContextException("No data returned for select query.");
        }

        return MapRowToEntity<TEntity>(data.Data.Tables[0].Rows[0], container);
    }

    /// <summary>
    /// Executes the provided SQL SELECT <paramref name="query"/> and returns multiple hydrated instances 
    /// of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of entities to hydrate from the data returned by <paramref name="query"/>.</typeparam>
    /// <param name="query">The SQL SELECT query to execute.</param>
    /// <returns>An (yielded return) IEnumerable of instances of type <typeparamref name="TEntity"/>. If the query returns multiple tables, 
    /// this function will use only the first table. If no data was returned by the query, then nothing is returned.</returns>
    public async IAsyncEnumerable<TEntity> SelectMultipleAsync<TEntity>(SqlQueryBuilder query)
    {
        PersistenceContainerInfo container = typeof(TEntity).RetrievePersistenceContainerInfoOrThrowException();

        Result result = SqlExecute.Query(_connectionString, query);
        if (result is ErrorResult error)
        {
            throw new SqlContextException($"Error selecting entity: {error.Message}", error.Error);
        }

        QueryResult data = (QueryResult)result;
        if (data.Data.Tables.Count > 0)
        {
            DataTable entityTable = data.Data.Tables[0];
            
            foreach(DataRow row in entityTable.Rows)
            {
                yield return MapRowToEntity<TEntity>(row, container);
            }
        }
    }

    /// <summary>
    /// Creates and executes a SQL SELECT query to return data from the entire backing table as hydrated instances 
    /// of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of entities to hydrate from the data returned.</typeparam>
    /// <returns>An (yielded return) IEnumerable of instances of type <typeparamref name="TEntity"/>. If no data 
    /// was returned by the query, then nothing is returned.</returns>
    public async IAsyncEnumerable<TEntity> SelectAllAsync<TEntity>()
    {
        PersistenceContainerInfo container = typeof(TEntity).RetrievePersistenceContainerInfoOrThrowException();

        SqlQueryBuilder query = SqlQueryBuilder.From<TEntity>()
                                    .Select<TEntity>();

        await foreach(TEntity item in SelectMultipleAsync<TEntity>(query))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Executes the provided SQL SELECT <paramref name="query"/> and returns multiple hydrated instances 
    /// of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of entities to hydrate from the data returned by <paramref name="query"/>.</typeparam>
    /// <param name="query">The SQL SELECT query to execute.</param>
    /// <returns>A materialised List of instances of type <typeparamref name="TEntity"/>. If the query returns multiple tables, 
    /// this function will use only the first table. If no data was returned by the query, then an empty List is returned.</returns>
    public async Task<List<TEntity>> SelectMultipleAsListAsync<TEntity>(SqlQueryBuilder query)
    {
        List<TEntity> list = new List<TEntity>();
        await foreach(TEntity entity in SelectMultipleAsync<TEntity>(query))
        {
            list.Add(entity);
        }

        return list;
    }

    /// <summary>
    /// Creates and executes a SQL SELECT query to return data from the entire backing table as hydrated instances 
    /// of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of entities to hydrate from the data returned.</typeparam>
    /// <returns>A materialised List of instances of type <typeparamref name="TEntity"/>. If the query returns multiple tables, 
    /// this function will use only the first table. If no data was returned by the query, then an empty List is returned.</returns>
    public async Task<List<TEntity>> SelectAllAsListAsync<TEntity>()
    {
        List<TEntity> list = new List<TEntity>();
        await foreach (TEntity entity in SelectAllAsync<TEntity>())
        {
            list.Add(entity);
        }

        return list;
    }
}
