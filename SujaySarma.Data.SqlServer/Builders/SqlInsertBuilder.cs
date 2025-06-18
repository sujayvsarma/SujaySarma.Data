using SujaySarma.Data.Core.Reflection;

using System;
using System.Collections.Generic;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Helps build a SQL INSERT statement.
    /// Supports: INSERT INTO VALUES, INSERT INTO FROM, TOP, WITH, injected values, injected columns
    /// </summary>
    public sealed class SqlInsertBuilder : SqlStatementBuilder
    {
        /// <summary>
        /// Assembles the INSERT statement from provided clauses and returns it as a StringBuilder instance.
        /// </summary>
        /// <returns>Instance of StringBuilder containing the assembled INSERT statement.</returns>
        public override StringBuilder Build()
        {
            if (_destinationColumnNames.Count == 0)
            {
                throw new InvalidOperationException("No destination columns selected for INSERT.");
            }

            if ((_columnsWithValues.Count > 0) && (_destinationColumnNames.Count != _columnsWithValues.Count))
            {
                throw new InvalidOperationException("There is a mismatch between columns selected for INSERT and the values provided.");
            }

            if ((_columnsWithValues.Count == 0) && (! _usingDefaultValues))
            {
                throw new InvalidOperationException("No values have been provided and DefaultValues() has not been set. Must perform one of the two.");
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("INSERT ");

            if (_topCount < uint.MaxValue)
            {
                builder.Append($"TOP {_topCount} ");
                if (_topIsPercent)
                {
                    builder.Append("PERCENT ");
                }
            }

            builder.Append($"INTO {_destinationTableName} ");

            if (_tableHints != SqlTableHints.None)
            {
                List<string> hints = new List<string>();
                foreach (SqlTableHints hint in Enum.GetValues<SqlTableHints>())
                {
                    if (_tableHints.HasFlag(hint))
                    {
                        hints.Add($"{hint.ToString().ToUpper()}");
                    }
                }

                if (hints.Count > 0)
                {
                    builder.Append($"WITH ({string.Join(',', hints)}) ");
                }
            }

            builder.Append('(').AppendJoin(',', _destinationColumnNames).Append(") ");

            if (_usingDefaultValues)
            {
                builder.Append("DEFAULT VALUES");
            }
            else
            {
                if (_sourceDataQuery != null)
                {
                    builder.Append("FROM ");
                    builder.Append(_sourceDataQuery.ToString());
                }
                else
                {
                    if (_columnsWithValues.Count == 0)
                    {
                        throw new InvalidOperationException("Columns/values are not provided.");
                    }                  

                    // now populate values
                    builder.Append("VALUES (");
                    List<string> values = new List<string>();
                    foreach (Dictionary<string, string> item in _columnsWithValues)
                    {
                        foreach (string name in _destinationColumnNames)
                        {
                            values.Add(item[name]);
                        }

                        builder.AppendJoin(',', values);
                        values.Clear();
                    }
                    builder.Append(')');
                }
            }

            builder.Append(';');
            return builder;
        }

        #region Support for INSERT FROM

        /// <summary>
        /// Set the datasource as a SQL SELECT query.
        /// </summary>
        /// <param name="query">An instance of a SqlQueryBuilder containing the query that will provide the data to insert.</param>
        /// <returns>Self-instance.</returns>
        public SqlInsertBuilder From(SqlQueryBuilder query)
        {
            _sourceDataQuery = query.Build();
            return this;
        }

        /// <summary>
        /// Set the datasource as a SQL SELECT query.
        /// </summary>
        /// <param name="query">An instance of a StringBuilder containing the query that will provide the data to insert.</param>
        /// <returns>Self-instance.</returns>
        public SqlInsertBuilder From(StringBuilder query)
        {
            _sourceDataQuery = query;
            return this;
        }

        /// <summary>
        /// Set the datasource as a SQL SELECT query.
        /// </summary>
        /// <param name="query">A free-form string query that will provide the data to insert.</param>
        /// <returns>Self-instance.</returns>
        public SqlInsertBuilder From(string query)
        {
            _sourceDataQuery = new StringBuilder(query);
            return this;
        }

        #endregion

        #region Specify columns and values

        /// <summary>
        /// Cause SQL Server to insert applicable DEFAULT values into each specified column.
        /// </summary>
        /// <returns>Self-instance.</returns>
        public SqlInsertBuilder DefaultValues()
        {
            _usingDefaultValues = true;
            return this;
        }

        /// <summary>
        /// Values to be inserted into the destination table are picked up from the provided <paramref name="obj"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object acting as the datasource.</typeparam>
        /// <param name="obj">Instance of object acting as the datasource.</param>
        /// <returns>Self-instance.</returns>
        public SqlInsertBuilder Values<TObject>(TObject obj)
        {
            if (_usingDefaultValues)
            {
                throw new InvalidOperationException("Cannot use Values() after using DefaultValues().");
            }

            ClrToTableWithAlias map = base.Map.Add<TObject>();
            object? refSource = obj;

            Dictionary<string, string> objValues = new Dictionary<string, string>();
            foreach (MemberTypeInfo member in map.TypeInfo.Members.Values)
            {
                string columnName = $"{map.Alias}.{member.Column.CreateQualifiedName()}";
                objValues.Add(
                        columnName,
                        ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref refSource, member))
                    );

                if (! _destinationColumnNames.Contains(columnName))
                {
                    _destinationColumnNames.Add(columnName);
                }
            }

            _columnsWithValues.Add(objValues);

            return this;
        }

        /// <summary>
        /// Values to be inserted into the destination table are picked up from the provided <paramref name="objList"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object acting as the datasource.</typeparam>
        /// <param name="objList">A collection of objects acting as the datasource.</param>
        /// <returns>Self-instance.</returns>
        public SqlInsertBuilder Values<TObject>(IEnumerable<TObject> objList)
        {
            if (_usingDefaultValues)
            {
                throw new InvalidOperationException("Cannot use Values() after using DefaultValues().");
            }

            ClrToTableWithAlias map = base.Map.Add<TObject>();
            Dictionary<string, string> objValues = new Dictionary<string, string>();

            /*
             * There are two approaches to extracting column names here:
             * 
             *  1. Extract them in a separate for-loop over map.TypeInfo.Members.Values as we only need the reflected members' TypeInfos.
             *  
             *  2. Do them inside the main for (over objList).
             *  
             *  We are preferring the #2 method here as it also allows us to deal with dynamic objects (like TObject = a generic TableEntities) 
             *  that *may* have differing property/field information between each instance of the same base type.
             * 
             */

            foreach (TObject obj in objList)
            {
                object? refSource = obj;
                foreach (MemberTypeInfo member in map.TypeInfo.Members.Values)
                {
                    string columnName = $"{map.Alias}.{member.Column.CreateQualifiedName()}";
                    objValues.Add(
                            columnName,
                            ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref refSource, member))
                        );

                    if (!_destinationColumnNames.Contains(columnName))
                    {
                        _destinationColumnNames.Add(columnName);
                    }
                }
                _columnsWithValues.Add(objValues);
                objValues.Clear();
            }

            return this;
        }

        /// <summary>
        /// Values to be inserted into the destination table are picked up from the provided <paramref name="objList"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object acting as the datasource.</typeparam>
        /// <param name="objList">A collection of objects acting as the datasource.</param>
        /// <returns>Self-instance.</returns>
        public SqlInsertBuilder Values<TObject>(params TObject[] objList)
        {
            return Values<TObject>((IEnumerable<TObject>)objList);
        }

        /// <summary>
        /// Values to be inserted into the destination table are picked up from the provided <paramref name="values"/> dictionary.
        /// </summary>
        /// <param name="values">An arbitrary dictionary containing name/value pairs.</param>
        /// <returns>Self-instance.</returns>
        public SqlInsertBuilder Values(Dictionary<string, object?> values)
        {
            Dictionary<string, string> objValues = new Dictionary<string, string>();
            foreach (KeyValuePair<string, object?> kvp in values)
            {
                objValues.Add(kvp.Key, ReflectionUtils.GetSQLStringValue(kvp.Value));

                if (!_destinationColumnNames.Contains(kvp.Key))
                {
                    _destinationColumnNames.Add(kvp.Key);
                }
            }
            _columnsWithValues.Add(objValues);
            return this;
        }

        /// <summary>
        /// Provide column name information for the destination table. Names thus provided are not changed and will be used as-is. It is on the 
        /// caller to provide the right strings. Can be called any number of times to append columns to the existing list.
        /// </summary>
        /// <param name="names">A collection of column names.</param>
        /// <returns>Self-instance.</returns>
        public SqlInsertBuilder Columns(IEnumerable<string> names)
        {
            foreach(string name in names)
            {
                if (! _destinationColumnNames.Contains(name))
                {
                    _destinationColumnNames.Add(name);
                }
            }
            return this;
        }

        /// <summary>
        /// Provide column name information for the destination table. Names thus provided are not changed and will be used as-is. It is on the 
        /// caller to provide the right strings. Can be called any number of times to append columns to the existing list.
        /// </summary>
        /// <param name="names">A collection of column names.</param>
        /// <returns>Self-instance.</returns>
        public SqlInsertBuilder Columns(params string[] names)
            => Columns((IEnumerable<string>)names);

        #endregion

        #region Primary Clauses

        /// <summary>
        /// Specify one or more table hints. You may call this method multiple times to add multiple table hints OR specify all the hints at once 
        /// using the OR pattern.
        /// </summary>
        /// <param name="tableHints">Table hints to specify: may be a single value, or OR'ed with other values.</param>
        /// <returns>Self-instance.</returns>
        public SqlInsertBuilder With(SqlTableHints tableHints)
        {
            // add only those that are not already added.
            foreach (SqlTableHints hint in Enum.GetValues<SqlTableHints>())
            {
                if (tableHints.HasFlag(hint) && (!_tableHints.HasFlag(hint)))
                {
                    _tableHints |= hint;
                }
            }

            return this;
        }

        /// <summary>
        /// Set this statement to insert only <paramref name="count"/> number of rows.
        /// </summary>
        /// <param name="count">Zero is a valid value.</param>
        /// <param name="percent">If true, <paramref name="count"/> is a percent value, returned rounded up to the next whole number.</param>
        /// <returns>Instance of self.</returns>
        public SqlInsertBuilder Top(uint count, bool percent = false)
        {
            _topCount = count;
            _topIsPercent = percent;
            return this;
        }

        /// <summary>
        /// Specify that the provided source dataset be inserted into this table.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object mapped to the destination table.</typeparam>
        /// <returns>A newly created instance of a SqlInsertBuilder.</returns>
        public static SqlInsertBuilder Into<TTable>()
        {
            SqlInsertBuilder builder = new SqlInsertBuilder();
            ClrToTableWithAlias map = builder.Map.Add<TTable>(isPrimaryTable: true);
            builder._destinationTableName = map.QualifiedTableName;
            return builder;
        }

        /// <summary>
        /// Specify that the provided source dataset be inserted into this table.
        /// </summary>
        /// <param name="primaryTable">Type of .NET object mapped to the destination table.</param>
        /// <returns>A newly created instance of a SqlInsertBuilder.</returns>
        public static SqlInsertBuilder Into(Type primaryTable)
        {
            SqlInsertBuilder builder = new SqlInsertBuilder();
            ClrToTableWithAlias map = builder.Map.Add(primaryTable, isPrimaryTable: true);
            builder._destinationTableName = map.QualifiedTableName;
            return builder;
        }

        /// <summary>
        /// Specify that the provided source dataset be inserted into this table.
        /// </summary>
        /// <param name="destinationTableName">Free-form name of the destination table.</param>
        /// <returns>A newly created instance of a SqlInsertBuilder.</returns>
        public static SqlInsertBuilder Into(string destinationTableName)
        {
            SqlInsertBuilder builder = new SqlInsertBuilder();
            builder._destinationTableName = destinationTableName;

            return builder;
        }

        #endregion

        /// <summary>
        /// Private constructor.
        /// </summary>
        private SqlInsertBuilder()
            : base()
        {
            _sourceDataQuery = null;
            _destinationTableName = null;
            _destinationColumnNames = new List<string>();
            _tableHints = SqlTableHints.None;
            _topCount = uint.MaxValue;
            _topIsPercent = false;
            _columnsWithValues = new List<Dictionary<string, string>>();
            _usingDefaultValues = false;
        }

        private string? _destinationTableName;
        private List<string> _destinationColumnNames;
        private StringBuilder? _sourceDataQuery;
        private SqlTableHints _tableHints;
        private uint _topCount = uint.MaxValue;
        private bool _topIsPercent = false;
        private List<Dictionary<string, string>> _columnsWithValues;
        private bool _usingDefaultValues;
    }
}
