using SujaySarma.Data.SqlServer.Builders;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SujaySarma.Data.SqlServer;

// Implementation of: async non-query operations.
public partial class SqlContext
{
    #region INSERT

    /// <summary>
    /// Insert the provided <paramref name="entity"/> into the database.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entity"/>.</typeparam>
    /// <param name="entity">Instance of an entity of <typeparamref name="TEntity"/> type.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected. Must equal 1 for a successful insert.</returns>
    public async Task<int> InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
    {
        Result result = await SqlExecuteAsync.NonQueryAsync(_connectionString, SqlInsertBuilder.Insert<TEntity>(entity), cancellationToken);
        if (result is ErrorResult error)
        {
            throw new SqlContextException($"Error inserting entity: {error.Message}", error.Error);
        }

        return ((NonQueryResult)result).RowsAffected;
    }

    /// <summary>
    /// Insert the provided <paramref name="entities"/> into the database.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entities"/>.</typeparam>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="entities">Instances of entities of <typeparamref name="TEntity"/> type.</param>
    /// <returns>The number of rows affected. Must equal the number of <paramref name="entities"/> for a successful insert.</returns>
    public async Task<int> InsertManyAsync<TEntity>(CancellationToken cancellationToken, params IEnumerable<TEntity> entities)
    {
        Result result = await SqlExecuteAsync.NonQueryAsync(_connectionString, SqlInsertBuilder.InsertMany<TEntity>(entities), cancellationToken);
        if (result is ErrorResult error)
        {
            throw new SqlContextException($"Error inserting entities: {error.Message}", error.Error);
        }

        return ((NonQueryResult)result).RowsAffected;
    }

    #endregion

    #region UPDATE

    /// <summary>
    /// Update the provided <paramref name="entity"/> into the database.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entity"/>.</typeparam>
    /// <param name="entity">Instance of an entity of <typeparamref name="TEntity"/> type.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected. Must equal 1 for a successful update.</returns>
    public async Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
    {
        Result result = await SqlExecuteAsync.NonQueryAsync(_connectionString, SqlUpdateBuilder.Update<TEntity>(entity), cancellationToken);
        if (result is ErrorResult error)
        {
            throw new SqlContextException($"Error updating entity: {error.Message}", error.Error);
        }

        return ((NonQueryResult)result).RowsAffected;
    }

    /// <summary>
    /// Update the provided <paramref name="entities"/> into the database.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entities"/>.</typeparam>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="entities">Instances of entities of <typeparamref name="TEntity"/> type.</param>
    /// <returns>The number of rows affected. Must equal the number of <paramref name="entities"/> for a successful update.</returns>
    public async Task<int> UpdateManyAsync<TEntity>(CancellationToken cancellationToken, params IEnumerable<TEntity> entities)
    {
        StringBuilder script = new StringBuilder();
        foreach(SqlUpdateBuilder builder in SqlUpdateBuilder.UpdateMany<TEntity>(entities))
        {
            script.AppendLine(builder.ToString());
        }

        Result result = await SqlExecuteAsync.NonQueryAsync(_connectionString, script, cancellationToken);
        if (result is ErrorResult error)
        {
            throw new SqlContextException($"Error updating entities: {error.Message}", error.Error);
        }

        return ((NonQueryResult)result).RowsAffected;
    }

    #endregion

    #region DELETE

    /// <summary>
    /// Delete the provided <paramref name="entity"/> into the database.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entity"/>.</typeparam>
    /// <param name="entity">Instance of an entity of <typeparamref name="TEntity"/> type.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected. Must equal 1 for a successful delete.</returns>
    public async Task<int> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
    {
        Result result = await SqlExecuteAsync.NonQueryAsync(_connectionString, SqlDeleteBuilder.Delete<TEntity>(entity), cancellationToken);
        if (result is ErrorResult error)
        {
            throw new SqlContextException($"Error deleting entity: {error.Message}", error.Error);
        }

        return ((NonQueryResult)result).RowsAffected;
    }

    /// <summary>
    /// Delete the provided <paramref name="entities"/> into the database.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entities"/>.</typeparam>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="entities">Instances of entities of <typeparamref name="TEntity"/> type.</param>
    /// <returns>The number of rows affected. Must equal the number of <paramref name="entities"/> for a successful delete.</returns>
    public async Task<int> DeleteManyAsync<TEntity>(CancellationToken cancellationToken, params IEnumerable<TEntity> entities)
    {
        Result result = await SqlExecuteAsync.NonQueryAsync(_connectionString, SqlDeleteBuilder.DeleteMany<TEntity>(entities), cancellationToken);
        if (result is ErrorResult error)
        {
            throw new SqlContextException($"Error deleting entities: {error.Message}", error.Error);
        }

        return ((NonQueryResult)result).RowsAffected;
    }

    #endregion

    #region MERGE

    /// <summary>
    /// Execute a MERGE using the provided <see cref="SqlMergeBuilder{TTarget}"/> instance <paramref name="builder"/> into the database.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of entities being merged.</typeparam>
    /// <param name="builder">A <see cref="SqlMergeBuilder{TTarget}"/> populated with the MERGE statement.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The number of rows affected. This is the total inserted + updated + deleted rows from the MERGE operation.</returns>
    public async Task<int> MergeAsync<TEntity>(SqlMergeBuilder<TEntity> builder, CancellationToken cancellationToken)
    {
        Result result = await SqlExecuteAsync.NonQueryAsync(_connectionString, builder.Build(), cancellationToken);
        if (result is ErrorResult error)
        {
            throw new SqlContextException($"Error executing merge: {error.Message}", error.Error);
        }

        return ((NonQueryResult)result).RowsAffected;
    }

    #endregion
}
