using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.SqlServer.Attributes;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SujaySarma.Data.SqlServer.Builders;

// Enables syntax such as SqlBuilder.Insert<TEntity>(entity);
// This is syntactic sugar over SqlDeleteBuilder.From<TEntity>().Where((e) => (e.Id == xxx));
public sealed partial class SqlDeleteBuilder
{

    /// <summary>
    /// Creates an instance of SqlDeleteBuilder that will help delete the provided <paramref name="entity"/>. This method works only when 
    /// <typeparamref name="TEntity"/> defines a primary key via <see cref="SqlTablePrimaryKeyColumn"/> attribute on one of its members.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entity"/>.</typeparam>
    /// <param name="entity">Instance of an entity of type <typeparamref name="TEntity"/>, that is to be deleted.</param>
    /// <returns>A correctly populated instance of SqlDeleteBuilder.</returns>
    public static SqlDeleteBuilder Delete<TEntity>(TEntity entity)
    {
        if (entity is null) 
        { 
            throw new ArgumentNullException(nameof(entity)); 
        }

        SqlDeleteBuilder builder = From<TEntity>();
        if (!builder._primaryTable.TryGetMembers(new Type[] { typeof(SqlTablePrimaryKeyColumn)}, mustHaveAllAttributes: false, out PersistenceContainerMemberInfo[] primaryKeyMembers))
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
        builder.Where<TEntity>(whereCondition);

        return builder;
    }


    /// <summary>
    /// Creates an instance of SqlDeleteBuilder that will help delete the provided <paramref name="entities"/>. This method works only when 
    /// <typeparamref name="TEntity"/> defines a primary key via <see cref="SqlTablePrimaryKeyColumn"/> attribute on one of its members.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of <paramref name="entities"/>.</typeparam>
    /// <param name="entities">One or more instances of entities of type <typeparamref name="TEntity"/>, that is to be deleted.</param>
    /// <returns>A correctly populated instance of SqlDeleteBuilder.</returns>
    /// <example>
    /// <code>
    /// // All these work equally well:
    /// SqlDeleteBuilder builder = SqlDeleteBuilder.DeleteMany(customer1, customer2, customer3);
    /// SqlDeleteBuilder builder = SqlDeleteBuilder.DeleteMany(customerList);
    /// SqlDeleteBuilder builder = SqlDeleteBuilder.DeleteMany(customers.Where(c => !c.IsActive));
    /// </code>
    /// </example>
    public static SqlDeleteBuilder DeleteMany<TEntity>(params IEnumerable<TEntity> entities)
    {
        if (entities is null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        SqlDeleteBuilder builder = From<TEntity>();
        if (!builder._primaryTable.TryGetMembers(new Type[] { typeof(SqlTablePrimaryKeyColumn) }, mustHaveAllAttributes: false, out PersistenceContainerMemberInfo[] primaryKeyMembers))
        {
            throw new InvalidOperationException($"Type '{builder._primaryTable.EntityType.GetUsableTypeName()}' does not have a primary key defined. Cannot build DELETE statement.");
        }

        // A SQL table can have only ONE primary key!
        if (primaryKeyMembers.Length > 1)
        {
            throw new InvalidOperationException($"Type '{builder._primaryTable.EntityType.GetUsableTypeName()}' has more than one primary key defined. This is invalid. Cannot build DELETE statement.");
        }

        // Materialise.
        List<TEntity> entitiesList = entities.Materialise<TEntity>(acceptNullElements: false, throwExceptionOnNull: true);

        if (entitiesList.Count == 0)
        {
            throw new ArgumentException("At least one entity must be provided.", nameof(entities));
        }

        // Build the WHERE condition dynamically: (e) => (e.PK1 == value1 && e.PK2 == value2 && ...)
        PersistenceContainerMemberInfo primaryKey = primaryKeyMembers[0];
        ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "e");
        Expression? combinedCondition = null;

        foreach(TEntity entity in entitiesList)
        {
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

            // Combine conditions with OR
            combinedCondition = ((combinedCondition is null) ? equality : Expression.OrElse(combinedCondition, equality));
        }

        if (combinedCondition is null)
        {
            throw new InvalidOperationException($"Unable to build WHERE condition for type '{builder._primaryTable.EntityType.GetUsableTypeName()}'.");
        }

        // Create the lambda: (e) => combinedCondition
        Expression<Func<TEntity, bool>> whereCondition = Expression.Lambda<Func<TEntity, bool>>(combinedCondition, parameter);

        // Create a WHERE condition matching the lambda we just built.
        builder.Where<TEntity>(whereCondition);

        return builder;

    }

}
