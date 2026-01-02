using System;
using System.Collections;
using System.Collections.Generic;

namespace SujaySarma.Data.SqlServer.Builders;

/// <summary>
/// A collection of SQL clauses (JOIN, WHERE, etc) that can be used to build complex SQL queries in a fluid manner.
/// </summary>
public class SqlClauseCollection : IEnumerable<string>, IEnumerable
{

    /// <summary>
    /// Returns if the collection has any items
    /// </summary>
    public bool HasItems
        => ((_clauses.Count > 0) ? true : false);

    /// <summary>
    /// Returns the count of items in this collection.
    /// </summary>
    public int Count
        => _clauses.Count;

    /// <summary>
    /// Clear all added elements
    /// </summary>
    public void Clear()
        => _clauses.Clear();

    /// <summary>
    /// Adds the provided clause to the collection.
    /// </summary>
    /// <param name="clause">Clause to add</param>
    /// <returns>Instance of self</returns>
    protected SqlClauseCollection Add(string clause)
    {
        clause = clause.Trim();
        if (string.IsNullOrWhiteSpace(clause))
        {
            throw new ArgumentNullException(nameof(clause), "Clause cannot be null or whitespace.");
        }

        _clauses.Add(clause);
        return this;
    }

    /// <summary>
    /// Return the string equivalent of the collection, with clauses separated by the provided separator character.
    /// </summary>
    /// <returns>String. Empty string if there are no items in the collection.</returns>
    public override string ToString()
    {
        if (!HasItems)
        {
            return string.Empty;
        }

        return string.Join(' ', _clauses);
    }

    /// <summary>
    /// Initialise.
    /// </summary>
	protected SqlClauseCollection()
	{
        _clauses = new List<string>();
    }

    /// <summary>
    /// Collection of clauses added.
    /// </summary>
    private readonly List<string> _clauses;


    #region IEnumerable

    /// <inheritdoc />
    public IEnumerator<string> GetEnumerator()
        => (IEnumerator<string>)_clauses.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
        => (IEnumerator)_clauses.GetEnumerator();

    #endregion
}
