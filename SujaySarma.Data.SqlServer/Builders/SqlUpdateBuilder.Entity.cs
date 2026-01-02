using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.SqlServer.Attributes;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SujaySarma.Data.SqlServer.Builders;

// Enables syntax such as SqlBuilder.Update<TEntity>(entity);
// This is syntactic sugar over SqlUpdateBuilder.Merge<TEntity>()....;
public sealed partial class SqlUpdateBuilder
{

    /// <summary>
    /// Creates an instance of SqlUpdateBuilder that will help update the provided <paramref name="entity"/>. This method works only when 
    /// <typeparamref name="TEntity"/> defines a primary key via <see cref="SqlTablePrimaryKeyColumn"/> attribute on one of its members.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entity"/>.</typeparam>
    /// <param name="entity">Instance of an entity of type <typeparamref name="TEntity"/>, that is to be updated.</param>
    /// <returns>A correctly populated instance of SqlUpdateBuilder.</returns>
    public static SqlUpdateBuilder Update<TEntity>(TEntity entity)
    {
        if (entity is null) 
        { 
            throw new ArgumentNullException(nameof(entity));
        }

        SqlUpdateBuilder builder = Into<TEntity>();

        if (!builder._primaryTable.TryGetMembers(new Type[] { typeof(SqlTablePrimaryKeyColumn) }, mustHaveAllAttributes: false, out PersistenceContainerMemberInfo[] primaryKeyMembers))
        {
            throw new InvalidOperationException($"Type '{builder._primaryTable.EntityType.GetUsableTypeName()}' does not have a primary key defined. Cannot build DELETE statement.");
        }

        // A SQL table can have only ONE primary key!
        if (primaryKeyMembers.Length > 1)
        {
            throw new InvalidOperationException($"Type '{builder._primaryTable.EntityType.GetUsableTypeName()}' has more than one primary key defined. This is invalid. Cannot build DELETE statement.");
        }

        // Build the WHERE condition dynamically: (e) => (e.PK1 == value1 && e.PK2 == value2 && ...)
        PersistenceContainerMemberInfo primaryKey = primaryKeyMembers[0];
        ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "e");

        // Build: e.PropertyName == pkValue
        BinaryExpression equality = Expression.Equal(
            Expression.PropertyOrField(parameter, primaryKey.Member.Name),
            Expression.Constant(
                entity.GetValue(primaryKey),
                primaryKey.Member switch
                {
                    PropertyInfo pi => pi.PropertyType,
                    FieldInfo fi => fi.FieldType,

                    _ => throw new InvalidOperationException($"Member type '{primaryKey.Member.MemberType}' is not supported.")
                })
        );

        // Create the lambda: (e) => combinedCondition
        Expression<Func<TEntity, bool>> whereCondition = Expression.Lambda<Func<TEntity, bool>>(equality, parameter);

        // Create a WHERE condition matching the lambda we just built.
        builder
            .Set<TEntity>(entity)
            .Where<TEntity>(whereCondition);

        return builder;
    }

    /// <summary>
    /// Creates instances of SqlUpdateBuilders that will help update the provided <paramref name="entities"/>. This method works only when 
    /// <typeparamref name="TEntity"/> defines a primary key via <see cref="SqlTablePrimaryKeyColumn"/> attribute on one of its members.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entities"/>.</typeparam>
    /// <param name="entities">One or more instances of entities of type <typeparamref name="TEntity"/>, that is to be updated.</param>
    /// <returns>Yield-returns one instance of SqlUpdateBuilder per <typeparamref name="TEntity"/> in the argument.</returns>
    /// <example>
    /// <code>
    /// // All these work equally well:
    /// IEnumerable{SqlUpdateBuilders} builders = SqlUpdateBuilder.UpdateMany(customer1, customer2, customer3);
    /// IEnumerable{SqlUpdateBuilders} builders = SqlUpdateBuilder.UpdateMany(customerList);
    /// IEnumerable{SqlUpdateBuilders} builders = SqlUpdateBuilder.UpdateMany(customers.Where(c => c.IsActive));
    /// </code>
    /// </example>
    public static IEnumerable<SqlUpdateBuilder> UpdateMany<TEntity>(params IEnumerable<TEntity> entities)
    {
        if (entities is null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        PersistenceContainerInfo container = typeof(TEntity).RetrievePersistenceContainerInfoOrThrowException();
        if (!container.TryGetMembers(new Type[] { typeof(SqlTablePrimaryKeyColumn) }, mustHaveAllAttributes: false, out PersistenceContainerMemberInfo[] primaryKeyMembers))
        {
            throw new InvalidOperationException($"Type '{container.EntityType.GetUsableTypeName()}' does not have a primary key defined. Cannot build DELETE statement.");
        }

        // A SQL table can have only ONE primary key!
        if (primaryKeyMembers.Length > 1)
        {
            throw new InvalidOperationException($"Type '{container.EntityType.GetUsableTypeName()}' has more than one primary key defined. This is invalid. Cannot build DELETE statement.");
        }

        // Build the WHERE condition dynamically: (e) => (e.PK1 == value1 && e.PK2 == value2 && ...)
        PersistenceContainerMemberInfo primaryKey = primaryKeyMembers[0];
        ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "e");

        foreach(TEntity entity in entities)
        {
            SqlUpdateBuilder builder = Into<TEntity>();

            // Build: e.PropertyName == pkValue
            BinaryExpression equality = Expression.Equal(
                Expression.PropertyOrField(parameter, primaryKey.Member.Name),
                Expression.Constant(
                    entity.GetValue(primaryKey),
                    primaryKey.Member switch
                    {
                        PropertyInfo pi => pi.PropertyType,
                        FieldInfo fi => fi.FieldType,

                        _ => throw new InvalidOperationException($"Member type '{primaryKey.Member.MemberType}' is not supported.")
                    })
            );

            // Create the lambda: (e) => combinedCondition
            Expression<Func<TEntity, bool>> whereCondition = Expression.Lambda<Func<TEntity, bool>>(equality, parameter);

            builder
                .Set<TEntity>(entity, additionalValues: null)
                .Where<TEntity>(whereCondition);

            yield return builder;
        }
    }

}
