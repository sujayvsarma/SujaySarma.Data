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

    //BUGFIX: Surface debugging methods in SqlContext.
    #region Manage Debugging

    /// <summary>
    /// Begin debugging.
    /// </summary>
    /// <param name="debugFileAbsolutePath">Absolute path to the file to write to.</param>
    /// <returns>Instance of self.</returns>
    public SqlContext BeginDebugging(string debugFileAbsolutePath)
    {
        Logger.BeginDebugging(debugFileAbsolutePath);
        return this;
    }

    /// <summary>
    /// End debugging.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlContext EndDebugging()
    {
        Logger.EndDebugging();
        return this;
    }

    #endregion


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

        //BUGFIX: table/alias names need not be a part of returned column name/schema.
        //        Allow partial and unquoted matches.
        foreach (PersistenceContainerMemberInfo member in container.Members)
        {
            string rawColumnName = member.PersistenceInfo.TableFieldName;
            string quotedColumnName = member.PersistenceInfo.CreateQualifiedName();
            string qualifiedColumnName = $"{container.ReferenceAlias}.{quotedColumnName}";

            if (row.Table.Columns.Contains(qualifiedColumnName))
            {
                entity.SetValue(member, row[qualifiedColumnName]);
            }
            else if (row.Table.Columns.Contains(quotedColumnName))
            {
                entity.SetValue(member, row[quotedColumnName]);
            }
            else if (row.Table.Columns.Contains(rawColumnName))
            {
                entity.SetValue(member, row[rawColumnName]);
            }
        }

        return entity;
    }
}
