using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.SqlServer.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of: DEFAULT VALUES, VALUES()
public sealed partial class SqlInsertBuilder
{

    /// <summary>
    /// Sets the INSERT to use DEFAULT VALUES for all columns of the destination table.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public SqlInsertBuilder UsingDefaultValues()
    {
        if (_insertFromQuery is not null)
        {
            throw new InvalidOperationException("Cannot set DEFAULT VALUES when INSERT FROM query is already specified.");
        }

        if (_values.Count > 0)
        {
            throw new InvalidOperationException("Cannot set DEFAULT VALUES when explicit VALUES are already specified.");
        }

        _insertDefaultValues = true;

        return this;
    }

    /// <summary>
    /// Sets the INSERT to serialise the provided <paramref name="value"/> and insert its values to the table.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of entity <paramref name="value"/>. This should match the 
    /// <see cref="Type"/> used to initialise this builder originally (via <see cref="Into{TTable}"/>).</typeparam>
    /// <param name="value">Business entity that is to be serialised to the table.</param>
    /// <returns>Instance of self.</returns>
    public SqlInsertBuilder Value<TTable>(TTable value)
    {
        if (_insertDefaultValues)
        {
            throw new InvalidOperationException("Cannot add VALUES when DEFAULT VALUES is already specified.");
        }

        if (_insertFromQuery is not null)
        {
            throw new InvalidOperationException("Cannot add VALUES when INSERT FROM query is already specified.");
        }

        if (!base.IsSameTableTarget(_primaryTable.EntityType, typeof(TTable)))
        {
            throw new InvalidOperationException("Cannot add values for a different table than the one specified for INSERT.");
        }

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        // serialise.
        _values.Add(SerializeEntityToRow(value));

        return this;
    }

    /// <summary>
    /// Sets the INSERT to serialise the provided <paramref name="values"/> and insert them into the table.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of entities <paramref name="values"/>. This should match the 
    /// <see cref="Type"/> used to initialise this builder originally (via <see cref="Into{TTable}"/>).</typeparam>
    /// <param name="values">Business entities that are to be serialised to the table.</param>
    /// <returns>Instance of self.</returns>
    public SqlInsertBuilder Values<TTable>(params IEnumerable<TTable> values)
    {
        if (_insertDefaultValues)
        {
            throw new InvalidOperationException("Cannot add VALUES when DEFAULT VALUES is already specified.");
        }

        if (_insertFromQuery is not null)
        {
            throw new InvalidOperationException("Cannot add VALUES when INSERT FROM query is already specified.");
        }

        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (!base.IsSameTableTarget(_primaryTable.EntityType, typeof(TTable)))
        {
            throw new InvalidOperationException("Cannot add values for a different table than the one specified for INSERT.");
        }

        foreach (TTable value in values)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(values), "One of the provided values is null.");
            }

            // serialise.
            _values.Add(SerializeEntityToRow(value));
        }

        return this;
    }

    /// <summary>
    /// Sets the statement to insert the provided <paramref name="values"/> into the table.
    /// </summary>
    /// <param name="values">A dictionary of values. Names should be the names of columns, and Values the values 
    /// to insert into those columns. You CANNOT use SQL functions in the values as they will be quoted/escaped before statement generation!</param>
    /// <returns>Instance of self.</returns>
    public SqlInsertBuilder Values(Dictionary<string, object?> values)
    {
        if (_insertDefaultValues)
        {
            throw new InvalidOperationException("Cannot add VALUES when DEFAULT VALUES is already specified.");
        }

        if (_insertFromQuery is not null)
        {
            throw new InvalidOperationException("Cannot add VALUES when INSERT FROM query is already specified.");
        }

        // rapid check:
        if (_values.Count > 0)
        {
            if (values.Count != _values[0].Count)
            {
                throw new InvalidOperationException("Cannot add VALUES with different number of columns than already specified.");
            }

            // Existing keys will contain quoted identifiers.
            HashSet<string> existingColumns = new HashSet<string>(_values[0].Keys);

            // New keys may or may not be quoted (caller dependent)!
            HashSet<string> newColumns = new HashSet<string>(values.Keys.Select(k => k.EnsureIdentifierIsQuoted()));

            if (!existingColumns.SetEquals(newColumns))
            {
                throw new InvalidOperationException("Cannot add VALUES with different column names than already specified.");
            }
        }

        Dictionary<string, string> row = new Dictionary<string, string>();
        foreach(KeyValuePair<string, object?> value in values)
        {
            row.Add(
                    value.Key.EnsureIdentifierIsQuoted(),
                    value.Value.GetSQLStringValue()
                );
        }

        _values.Add(row);

        return this;
    }


    /// <summary>
    /// Serialise the provided <paramref name="entity"/> to a row dictionary.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of entity <paramref name="entity"/>.</typeparam>
    /// <param name="entity">The entity object to serialise.</param>
    /// <returns>A row-dictionary: column name/value pairs with the identifiers and values suitably quoted.</returns>
    private Dictionary<string, string> SerializeEntityToRow<TTable>(TTable entity)
    {
        Dictionary<string, string> row = new Dictionary<string, string>();
        foreach (PersistenceContainerMemberInfo member in _primaryTable.Members)
        {
            // since we are INSERTing, skip the auto-populated colums.
            if (member.PersistenceInfo is not SqlTablePopulatedColumn)
            {
                object? value = entity.GetValue(member, useAutoPopulate: true);
                string stringifiedValue = value.GetSQLStringValue();

                row.Add(
                        member.PersistenceInfo.CreateQualifiedName(),
                        stringifiedValue
                    );
            }
        }

        return row;
    }

    /// <summary>
    /// Flag indicating if DEFAULT values are to be inserted for the columns of the destination table.
    /// Cannot set this when ANY other values option is set (i.e., values collection below, INSERT FROM query).
    /// </summary>
    private bool _insertDefaultValues = false;

    /// <summary>
    /// List of rows -- each element of list (dict[string,string]) is a row -- each element of dict is a column/value pair. 
    /// Column names are already quoted, values are stringified/quoted/escaped.
    /// Cannot set this when ANY other values option is set (i.e., DEFAULT values above, INSERT FROM query).
    /// </summary>
    private readonly List<Dictionary<string, string>> _values = new List<Dictionary<string, string>>();
}
