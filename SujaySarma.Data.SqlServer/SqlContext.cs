using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;

using System;
using System.Data;

namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Enables querying or interacting with data stored in SQL Server databases 
/// with a fluid syntax.
/// </summary>
public partial class SqlContext
{
    /// <summary>
    /// Initialise using a <see cref="SqlConnectionStringBuilder"/>.
    /// </summary>
    /// <param name="connectionStringBuilder">The instance of <see cref="SqlConnectionStringBuilder"/> to use.</param>
    /// <returns>Initialised instance of self.</returns>
    public static SqlContext Using(SqlConnectionStringBuilder connectionStringBuilder)
        => new SqlContext(connectionStringBuilder.Build());

    /// <summary>
    /// Initialise using a prepared connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to use.</param>
    /// <returns>Initialised instance of self.</returns>
    public static SqlContext Using(string connectionString)
        => new SqlContext(connectionString);


    /// <summary>
    /// Private constructor, initialises the connection string. 
    /// Consumers must use the Builder pattern.
    /// </summary>
    /// <param name="connectionString">Finalised connection string.</param>
    private SqlContext(string connectionString)
    {
        _connectionString = connectionString;        
    }

    private readonly string _connectionString;

    /// <summary>
    /// Create an instance of <typeparamref name="TEntity"/> from a <see cref="DataRow"/>.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="Type"/> of entity to instantiate.</typeparam>
    /// <param name="row">A <see cref="DataRow"/> containing the data to use to hydrate the entity.</param>
    /// <param name="container">An instance of <see cref="PersistenceContainerInfo"/> containing the reflection metadata for <typeparamref name="TEntity"/>.</param>
    /// <returns>Instance of the entity.</returns>
    private TEntity MapRowToEntity<TEntity>(DataRow row, PersistenceContainerInfo container)
    {
        Type typeOfEntity = typeof(TEntity);

        TEntity entity = (TEntity)(Activator.CreateInstance(typeOfEntity, nonPublic: true)
            ?? throw new TypeLoadException($"Unable to create an instance of type '{typeOfEntity.GetUsableTypeName()}'."));

        foreach (PersistenceContainerMemberInfo member in container.Members)
        {
            string qualifiedColumnName = $"{container.ReferenceAlias}.{member.PersistenceInfo.CreateQualifiedName()}";

            if (row.Table.Columns.Contains(qualifiedColumnName))
            {
                entity.SetValue(member, row[qualifiedColumnName]);
            }
        }

        return entity;
    }
}
