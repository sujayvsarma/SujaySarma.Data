using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.SqlServer.Builders;

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
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
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the async operation.</param>
    /// <returns>An instance of type <typeparamref name="TEntity"/>. If the query returns multiple tables or rows, this function 
    /// will use only the first row of the first table. If no data was returned, a <see cref="SqlContextException"/> is thrown.</returns>
    public async Task<TEntity> SelectOneAsync<TEntity>(SqlQueryBuilder query, CancellationToken cancellationToken)
    {
        PersistenceContainerInfo container = typeof(TEntity).RetrievePersistenceContainerInfoOrThrowException();

        Result result = await SqlExecuteAsync.QueryAsync(_connectionString, query, cancellationToken);
        if (result is ErrorResult error)
        {
            throw new SqlContextException($"Error selecting entity: {error.Message}", error.Error);
        }

        QueryResult data = (QueryResult)result;
        if ((data.Data.Tables.Count is 0) || (data.Data.Tables[0].Rows.Count is 0))
        {
            throw new SqlContextException("No data returned for select query.");
        }

        return MapRowToEntityAsync<TEntity>(data.Data.Tables[0].Rows[0], container, cancellationToken);
    }

    //BUGFIX: Add method to return new'ed instance of TEntity if there were no records returned.
    /// <summary>
    /// Executes the provided SQL SELECT <paramref name="query"/> and returns a single hydrated instance 
    /// of type <typeparamref name="TEntity"/> or returns a new instance of the entity.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity to hydrate from the data returned by <paramref name="query"/>.</typeparam>
    /// <param name="query">The SQL SELECT query to execute.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the async operation.</param>
    /// <returns>An instance of type <typeparamref name="TEntity"/>. If the query returns multiple tables or rows, this function 
    /// will use only the first row of the first table. If no data was returned, a <see cref="SqlContextException"/> is thrown.</returns>
    public async Task<TEntity> SelectOneOrNewAsync<TEntity>(SqlQueryBuilder query, CancellationToken cancellationToken)
    {
        TEntity entity;
        try
        {
            entity = await SelectOneAsync<TEntity>(query, cancellationToken);
        }
        catch (SqlContextException ex) when (ex.Message == "No data returned for select query.")
        {
            entity = (TEntity)(Activator.CreateInstance(typeof(TEntity), nonPublic: true)
            ?? throw new TypeLoadException($"Unable to create an instance of type '{typeof(TEntity).GetUsableTypeName()}'."));
        }

        return entity;
    }

    /// <summary>
    /// Executes the provided SQL SELECT <paramref name="query"/> and returns multiple hydrated instances 
    /// of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of entities to hydrate from the data returned by <paramref name="query"/>.</typeparam>
    /// <param name="query">The SQL SELECT query to execute.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the async operation.</param>
    /// <returns>An (yielded return) IEnumerable of instances of type <typeparamref name="TEntity"/>. If the query returns multiple tables, 
    /// this function will use only the first table. If no data was returned by the query, then nothing is returned.</returns>
    public async IAsyncEnumerable<TEntity> SelectMultipleAsync<TEntity>(SqlQueryBuilder query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        PersistenceContainerInfo container = typeof(TEntity).RetrievePersistenceContainerInfoOrThrowException();

        Result result = await SqlExecuteAsync.QueryAsync(_connectionString, query, cancellationToken);
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
                // Handle cancellation
                cancellationToken.ThrowIfCancellationRequested();

                yield return MapRowToEntity<TEntity>(row, container);
            }
        }
    }

    /// <summary>
    /// Creates and executes a SQL SELECT query to return data from the entire backing table as hydrated instances 
    /// of type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of entities to hydrate from the data returned.</typeparam>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the async operation.</param>
    /// <returns>An (yielded return) IEnumerable of instances of type <typeparamref name="TEntity"/>. If no data 
    /// was returned by the query, then nothing is returned.</returns>
    public async IAsyncEnumerable<TEntity> SelectAllAsync<TEntity>([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        PersistenceContainerInfo container = typeof(TEntity).RetrievePersistenceContainerInfoOrThrowException();

        SqlQueryBuilder query = SqlQueryBuilder.From<TEntity>()
                                    .Select<TEntity>();

        await foreach(TEntity item in SelectMultipleAsync<TEntity>(query, cancellationToken))
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
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the async operation.</param>
    /// <returns>A materialised List of instances of type <typeparamref name="TEntity"/>. If the query returns multiple tables, 
    /// this function will use only the first table. If no data was returned by the query, then an empty List is returned.</returns>
    public async Task<List<TEntity>> SelectMultipleAsListAsync<TEntity>(SqlQueryBuilder query, CancellationToken cancellationToken)
    {
        List<TEntity> list = new List<TEntity>();
        await foreach(TEntity entity in SelectMultipleAsync<TEntity>(query, cancellationToken))
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
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the async operation.</param>
    /// <returns>A materialised List of instances of type <typeparamref name="TEntity"/>. If the query returns multiple tables, 
    /// this function will use only the first table. If no data was returned by the query, then an empty List is returned.</returns>
    public async Task<List<TEntity>> SelectAllAsListAsync<TEntity>(CancellationToken cancellationToken)
    {
        List<TEntity> list = new List<TEntity>();
        await foreach (TEntity entity in SelectAllAsync<TEntity>(cancellationToken))
        {
            list.Add(entity);
        }

        return list;
    }



    /// <summary>
    /// Create an instance of <typeparamref name="TEntity"/> from a <see cref="DataRow"/>.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of entity to instantiate.</typeparam>
    /// <param name="row">A <see cref="DataRow"/> containing the data to use to hydrate the entity.</param>
    /// <param name="container">An instance of <see cref="PersistenceContainerInfo"/> containing the reflection metadata for <typeparamref name="TEntity"/>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Instance of the entity.</returns>
    private static TEntity MapRowToEntityAsync<TEntity>(DataRow row, PersistenceContainerInfo container, CancellationToken cancellationToken)
    {
        Type typeOfEntity = typeof(TEntity);

        TEntity entity = (TEntity)(Activator.CreateInstance(typeOfEntity, nonPublic: true)
            ?? throw new TypeLoadException($"Unable to create an instance of type '{typeOfEntity.GetUsableTypeName()}'."));

        cancellationToken.ThrowIfCancellationRequested();

        foreach (PersistenceContainerMemberInfo member in container.Members)
        {
            string qualifiedColumnName = $"{container.ReferenceAlias}.{member.PersistenceInfo.CreateQualifiedName()}";
            if (row.Table.Columns.Contains(qualifiedColumnName))
            {
                entity.SetValue(member, row[qualifiedColumnName]);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        return entity;
    }
}
