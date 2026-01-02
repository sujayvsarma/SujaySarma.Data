using SujaySarma.Data.SqlServer.Builders;

using System;
using System.Collections.Generic;
using System.Text;

namespace SujaySarma.Data.SqlServer;

// Implementation of: synchronous non-query operations.
public partial class SqlContext
{
    #region INSERT

    /// <summary>
    /// Insert the provided <paramref name="entity"/> into the database.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entity"/>.</typeparam>
    /// <param name="entity">Instance of an entity of <typeparamref name="TEntity"/> type.</param>
    /// <returns>The number of rows affected. Must equal 1 for a successful insert.</returns>
    public int Insert<TEntity>(TEntity entity)
    {
        Result result = SqlExecute.NonQuery(_connectionString, SqlInsertBuilder.Insert<TEntity>(entity));
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
    /// <param name="entities">Instances of entities of <typeparamref name="TEntity"/> type.</param>
    /// <returns>The number of rows affected. Must equal the number of <paramref name="entities"/> for a successful insert.</returns>
    public int InsertMany<TEntity>(params IEnumerable<TEntity> entities)
    {
        Result result = SqlExecute.NonQuery(_connectionString, SqlInsertBuilder.InsertMany<TEntity>(entities));
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
    /// <returns>The number of rows affected. Must equal 1 for a successful update.</returns>
    public int Update<TEntity>(TEntity entity)
    {
        Result result = SqlExecute.NonQuery(_connectionString, SqlUpdateBuilder.Update<TEntity>(entity));
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
    /// <param name="entities">Instances of entities of <typeparamref name="TEntity"/> type.</param>
    /// <returns>The number of rows affected. Must equal the number of <paramref name="entities"/> for a successful update.</returns>
    public int UpdateMany<TEntity>(params IEnumerable<TEntity> entities)
    {
        StringBuilder script = new StringBuilder();
        foreach(SqlUpdateBuilder builder in SqlUpdateBuilder.UpdateMany<TEntity>(entities))
        {
            script.AppendLine(builder.ToString());
        }

        Result result = SqlExecute.NonQuery(_connectionString, script);
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
    /// <returns>The number of rows affected. Must equal 1 for a successful delete.</returns>
    public int Delete<TEntity>(TEntity entity)
    {
        Result result = SqlExecute.NonQuery(_connectionString, SqlDeleteBuilder.Delete<TEntity>(entity));
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
    /// <param name="entities">Instances of entities of <typeparamref name="TEntity"/> type.</param>
    /// <returns>The number of rows affected. Must equal the number of <paramref name="entities"/> for a successful delete.</returns>
    public int DeleteMany<TEntity>(params IEnumerable<TEntity> entities)
    {
        Result result = SqlExecute.NonQuery(_connectionString, SqlDeleteBuilder.DeleteMany<TEntity>(entities));
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
    /// <returns>The number of rows affected. This is the total inserted + updated + deleted rows from the MERGE operation.</returns>
    public int Merge<TEntity>(SqlMergeBuilder<TEntity> builder)
    {
        Result result = SqlExecute.NonQuery(_connectionString, builder.Build());
        if (result is ErrorResult error)
        {
            throw new SqlContextException($"Error executing merge: {error.Message}", error.Error);
        }

        return ((NonQueryResult)result).RowsAffected;
    }

    #endregion
}
