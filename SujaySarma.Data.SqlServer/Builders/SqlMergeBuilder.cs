using System;
using System.Collections.Generic;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Helps build a SQL query (MERGE) statement. 
    /// Supports: specifying target and source tables, defining merge conditions, and handling INSERT, UPDATE, and DELETE actions.
    /// </summary>
    public sealed class SqlMergeBuilder : SqlStatementBuilder
    {

        /// <summary>
        /// Assembles the MERGE statement and returns it as a StringBuilder instance.
        /// </summary>
        /// <returns>The assembled MERGE statement.</returns>
        public override StringBuilder Build()
        {
            if (_usingQuery.Length == 0)
            {
                throw new InvalidOperationException("No query specified to select dataset to determine MERGE operation.");
            }

            if (_matchOperations.Count == 0)
            {
                throw new InvalidOperationException("No operations specified at all against MATCHED or NOT MATCHED conditions.");
            }

            StringBuilder builder = new StringBuilder();
            builder.Append($"MERGE {_destinationTableName} ");
            builder.Append("USING ");
            builder.Append(_usingQuery.ToString());
            builder.Append(' ');

            foreach(WhenMatchedDo action in _matchOperations)
            {
                builder.Append("WHEN ");
                if (action.Matched)
                {
                    builder.Append("MATCHED ");
                }
                else
                {
                    builder.Append("NOT MATCHED ");
                }

                if ((action.AdditionalCondition != null) && (action.AdditionalCondition.Length > 0))
                {
                    string resolve = action.AdditionalCondition.ToString();
                    if ((resolve != "BY TARGET") && (resolve != "BY SOURCE"))
                    {
                        builder.Append($"AND ({resolve})");
                    }
                    else
                    {
                        builder.Append(resolve);
                    }
                    builder.Append(' ');
                }

                string actionSql = action.Execute.Build().ToString();
                if (string.IsNullOrWhiteSpace(actionSql))
                {
                    throw new InvalidOperationException($"The action to execute for {(action.Matched ? string.Empty : "NOT ")}MATCHED is empty or invalid.");
                }

                builder.Append("THEN ");
                builder.Append(actionSql);
                builder.Append(' ');
            }

            builder.Append(';');
            return builder;
        }



        #region The query that switches between Insert/Update/Delete

        /// <summary>
        /// Provide the query or dataset that determines whether the MERGE statement executes the INSERT, UPDATE or DELETE portion.
        /// </summary>
        /// <param name="query">The fully-composed query as a SqlQueryBuilder.</param>
        /// <param name="queryResultAlias">An alias to use for the dataset resulting from <paramref name="query"/></param>
        /// <param name="filterCondition">The matching/filtering condition to be used with an "ON" sub-clause within the USING clause.</param>
        /// <returns>Self-instance</returns>
        public SqlMergeBuilder Using(SqlQueryBuilder query, string queryResultAlias = "TGT", string? filterCondition = null)
        {
            _usingQuery = query.Build();
            _usingQuery.Append($" AS {queryResultAlias}");
            if (! string.IsNullOrWhiteSpace(filterCondition))
            {
                _usingQuery.Append($" ON ({filterCondition})");
            }
            return this;
        }

        /// <summary>
        /// Provide the query or dataset that determines whether the MERGE statement executes the INSERT, UPDATE or DELETE portion.
        /// </summary>
        /// <param name="query">The fully-composed query as a StringBuilder.</param>
        /// <param name="queryResultAlias">An alias to use for the dataset resulting from <paramref name="query"/></param>
        /// <param name="filterCondition">The matching/filtering condition to be used with an "ON" sub-clause within the USING clause.</param>
        /// <returns>Self-instance</returns>
        public SqlMergeBuilder Using(StringBuilder query, string queryResultAlias = "TGT", string? filterCondition = null)
        {
            _usingQuery = query;
            _usingQuery.Append($" AS {queryResultAlias}");
            if (! string.IsNullOrWhiteSpace(filterCondition))
            {
                _usingQuery.Append($" ON ({filterCondition})");
            }
            return this;
        }

        /// <summary>
        /// Provide the query or dataset that determines whether the MERGE statement executes the INSERT, UPDATE or DELETE portion.
        /// </summary>
        /// <param name="query">The fully-composed query as a free-form string.</param>
        /// <param name="queryResultAlias">An alias to use for the dataset resulting from <paramref name="query"/></param>
        /// <param name="filterCondition">The matching/filtering condition to be used with an "ON" sub-clause within the USING clause.</param>
        /// <returns>Self-instance</returns>
        public SqlMergeBuilder Using(string query, string queryResultAlias = "TGT", string? filterCondition = null)
        {
            _usingQuery = new StringBuilder(query);
            _usingQuery.Append($" AS {queryResultAlias}");
            if (! string.IsNullOrWhiteSpace(filterCondition))
            {
                _usingQuery.Append($" ON ({filterCondition})");
            }
            return this;
        }

        #endregion

        #region Primary clauses

        /// <summary>
        /// Specifies the table that holds the data to be merged.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object that maps to this table.</typeparam>
        /// <returns>A newly created instance of SqlMergeBuilder</returns>
        public static SqlMergeBuilder Table<TTable>()
            => Table(typeof(TTable));

        /// <summary>
        /// Specifies the table that holds the data to be merged.
        /// </summary>
        /// <param name="tableType">Type of .NET object that maps to this table.</param>
        /// <returns>A newly created instance of SqlMergeBuilder</returns>
        public static SqlMergeBuilder Table(Type tableType)
        {
            SqlMergeBuilder builder = new SqlMergeBuilder();
            ClrToTableWithAlias map = builder.Map.Add(tableType, true);
            builder._destinationTableName = $"{map.QualifiedTableName} AS {map.Alias}";
            return builder;
        }

        /// <summary>
        /// Specifies the table that holds the data to be merged.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="tableAlias">A shorter name or alias for the table.</param>
        /// <returns>A newly created instance of SqlMergeBuilder</returns>
        public static SqlMergeBuilder Table(string tableName, string tableAlias)
        {
            SqlMergeBuilder builder = new SqlMergeBuilder();
            builder._destinationTableName = $"{tableName} AS {tableAlias}";
            return builder;
        }

        #endregion


        /// <summary>
        /// Private constructor
        /// </summary>
        private SqlMergeBuilder()
            : base()
        {
            _destinationTableName = default!;
            _usingQuery = default!;
            _matchOperations = new List<WhenMatchedDo>();
        }

        private string _destinationTableName;
        private StringBuilder _usingQuery;
        private List<WhenMatchedDo> _matchOperations;

        /// <summary>
        /// A structured manner to keep track of what to do 
        /// when conditions match (or not)
        /// </summary>
        private class WhenMatchedDo
        {
            /// <summary>
            /// When a provided condition matches.
            /// </summary>
            public bool Matched
            {
                get; set;

            } = false;

            /// <summary>
            /// An additional filter or condition to apply when matched/not.
            /// </summary>
            public StringBuilder? AdditionalCondition
            {
                get; set;

            } = null;

            /// <summary>
            /// The statement to execute
            /// </summary>
            public SqlStatementBuilder Execute
            {
                get; set;

            } = default!;
        }
    }
}
