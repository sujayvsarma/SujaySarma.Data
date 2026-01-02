using System;
using System.Collections.Generic;

namespace SujaySarma.Data.SqlServer.Builders;

// Enables syntax such as SqlBuilder.Insert<TEntity>(entity);
// This is syntactic sugar over SqlInsertBuilder.Merge<TEntity>()....;
public sealed partial class SqlInsertBuilder
{

    /// <summary>
    /// Creates an instance of SqlInsertBuilder that will help insert the provided <paramref name="entity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entity"/>.</typeparam>
    /// <param name="entity">Instance of an entity of type <typeparamref name="TEntity"/>, that is to be inserted.</param>
    /// <returns>A correctly populated instance of SqlInsertBuilder.</returns>
    public static SqlInsertBuilder Insert<TEntity>(TEntity entity)
    {
        if (entity is null) 
        { 
            throw new ArgumentNullException(nameof(entity)); 
        }

        SqlInsertBuilder builder = Into<TEntity>()
            .Value<TEntity>(entity);

        return builder;
    }


    /// <summary>
    /// Creates an instance of SqlInsertBuilder that will help insert the provided <paramref name="entities"/>.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entities"/>.</typeparam>
    /// <param name="entities">One or more instances of entities of type <typeparamref name="TEntity"/>, that is to be inserted.</param>
    /// <returns>A correctly populated instance of SqlInsertBuilder.</returns>
    /// <example>
    /// <code>
    /// // All these work equally well:
    /// SqlInsertBuilder builder = SqlInsertBuilder.InsertMany(customer1, customer2, customer3);
    /// SqlInsertBuilder builder = SqlInsertBuilder.InsertMany(customerList);
    /// SqlInsertBuilder builder = SqlInsertBuilder.InsertMany(customers.Where(c => c.IsNew));
    /// </code>
    /// </example>
    public static SqlInsertBuilder InsertMany<TEntity>(params IEnumerable<TEntity> entities)
    {
        if (entities is null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        SqlInsertBuilder builder = Into<TEntity>()
            .Values(entities);

        return builder;

    }

}
