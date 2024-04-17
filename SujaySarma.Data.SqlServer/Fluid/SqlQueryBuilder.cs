using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.SqlServer.Fluid.Tools;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// Builder for SELECT query and operations.
    /// </summary>
    public class SqlQueryBuilder : SqlFluidStatementBuilder
    {

        /// <inheritdoc />
        public override string Build()
        {
            List<string> query = new List<string>()
            {
                "SELECT"
            };

            if (_isDistinct)
            {
                query.Add("DISTINCT");
            }

            if (_topCount < uint.MaxValue)
            {
                query.Add($"TOP {_topCount}");
            }

            TypeTableAliasMap primaryTableMap = base.TypeTableMap.GetPrimaryTable();
            if (_selectColumnsList.Count == 0)
            {
                // We could be here simply because the dev forgot to write .Select<TObject>() at the end of the fluid loop!

                //NOTE:
                //  We could do a "*" and avoid the 4 lines of code below. BUT... the class wanting the data 
                //  may not want all those columns. If the extra columns are calculated, virtual or just heavy, 
                //  we want to avoid fetching them unnecessarily (DB, network and local system overheads avoided).
                //  Hence, we will go through all the adorned properties/fields of the Type we are expected to 
                //  populate and fetch only those columns from the table. Neat, eh?

                // we get the columns from whatever was added in the From() call at the top of the fluid-buildout:
                foreach (ContainerMemberTypeInformation member in primaryTableMap.Discovery.Members.Values)
                {
                    AddColumnIfNotExistsOrSkip($"{primaryTableMap.Alias}.{member.ContainerMemberDefinition.CreateQualifiedName()}");
                }
            }
            query.Add(string.Join(',', _selectColumnsList));

            query.Add("FROM");            
            query.Add($"{primaryTableMap.GetQualifiedTableName()} {primaryTableMap.Alias} WITH (NOLOCK)");
            foreach (string j in Joins)
            {
                query.Add(j);
            }

            if (Where.HasConditions)
            {
                query.Add("WHERE");
                query.Add(Where.ToString());
            }

            if (OrderBy.HasItems)
            {
                query.Add("ORDER BY");
                query.Add(string.Join(',', OrderBy));
            }

            return string.Join(' ', query);
        }

        /// <summary>
        /// Inject the provided columns and values as static values to the end of the existing column collection
        /// </summary>
        /// <param name="additionalValues">Dictionary of additional values to inject</param>
        internal void InjectAdditionalValues(Dictionary<string, object?> additionalValues)
        {
            foreach (string colName in additionalValues.Keys)
            {
                if (!_selectColumnsList.Any(cn => (cn.Equals(colName, StringComparison.OrdinalIgnoreCase))))
                {
                    _selectColumnsList.Add(
                        $"{ReflectionUtils.GetSQLStringValue(additionalValues[colName])} as [{colName}]"
                    );
                }                
            }
        }

        /// <summary>
        /// Register a column selection (may be a literal constant as well).
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="selectors">One or more selectors for the column (eg: u => u.Id). Do NOT use object selectors such as 'u => u' this will result in empty selectors. Instead use the Select[TObject]() overload.</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Select<TObject>(params Expression<Func<TObject, object>>[] selectors)
        {
            base.TypeTableMap.TryAdd<TObject>();
            foreach (Expression selector in selectors)
            {
                // we cannot check/skip because these may contain aliased columns ('foo+1 as foobar')
                _selectColumnsList.Add(base.ParseToSql(selector, treatAssignmentsAsAlias: true));
            }

            return this;
        }

        /// <summary>
        /// Register all columns from the specified <typeparamref name="TObject"/> object.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Select<TObject>()
        {
            base.TypeTableMap.TryAdd<TObject>();
            TypeTableAliasMap map = base.TypeTableMap.GetMap<TObject>()!;

            foreach (ContainerMemberTypeInformation member in map.Discovery.Members.Values)
            {
                AddColumnIfNotExistsOrSkip($"{map.Alias}.{member.ContainerMemberDefinition.CreateQualifiedName()}");
            }

            return this;
        }

        /// <summary>
        /// Register intention to select ALL columns from all tables and joins added to the builder so far.
        /// </summary>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Select()
        {
            _selectColumnsList.Clear();
            foreach (TypeTableAliasMap map in base.TypeTableMap)
            {
                foreach (ContainerMemberTypeInformation member in map.Discovery.Members.Values)
                {
                    AddColumnIfNotExistsOrSkip($"{map.Alias}.{member.ContainerMemberDefinition.CreateQualifiedName()}");
                }
            }
            return this;
        }

        // Helper function -- adds the provided column to the SELECT list if it does not already exist
        private void AddColumnIfNotExistsOrSkip(string columnName)
        {
            if (!_selectColumnsList.Any(cn => (cn.Equals(columnName, StringComparison.OrdinalIgnoreCase))))
            {
                _selectColumnsList.Add(columnName);
            }
        }

        /// <summary>
        /// Register the intention to select only DISTINCT rows
        /// </summary>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Distinct()
        {
            _isDistinct = true;
            return this;
        }

        /// <summary>
        /// Register intention to select only the first <paramref name="count"/> number of rows.
        /// </summary>
        /// <param name="count">Number of rows to pick up. Zero will return only Schema!</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Top(uint count)
        {
            _topCount = count;
            return this;
        }

        /// <summary>
        /// Defines the primary CLR object (and hence it's backing SQL Server table) that should be used 
        /// to populate the query. Any unqualified column references are implicitly assumed to be homed 
        /// in this object/table.
        /// </summary>
        /// <typeparam name="TPrimaryObject">Type of CLR object. Must have the <see cref="Attributes.TableAttribute"/> attribute decoration.</typeparam>
        /// <returns>Created instance of SqlQueryBuilder</returns>
        public static SqlQueryBuilder From<TPrimaryObject>()
            => new SqlQueryBuilder(typeof(TPrimaryObject));

        /// <summary>
        /// Collection of WHERE conditions
        /// </summary>
        public SqlTableWhereConditionsCollection Where
        {
            get;
            init;
        }

        /// <summary>
        /// Collection of JOINs
        /// </summary>
        public SqlTableJoinsCollection Joins
        {
            get;
            init;
        }

        /// <summary>
        /// Collection of ORDER BY conditions
        /// </summary>
        public SqlTableOrderByCollection OrderBy
        {
            get;
            init;
        }

        /// <summary>
        /// Initializer. Marked as private to prevent direct initialization of the object. 
        /// Object init should happen only via the From() static method.
        /// </summary>
        private SqlQueryBuilder(Type tPrimaryObject) : base()
        {
            base.TypeTableMap.TryAdd(tPrimaryObject, isPrimaryTable: true);

            _selectColumnsList = new List<string>();

            _topCount = uint.MaxValue;  // because TOP 0 is a valid query!
            _isDistinct = false;

            Joins = new SqlTableJoinsCollection(base.TypeTableMap);
            Where = new SqlTableWhereConditionsCollection(base.TypeTableMap);
            OrderBy = new SqlTableOrderByCollection(base.TypeTableMap);
        }

        private readonly List<string> _selectColumnsList;
        private uint _topCount;
        private bool _isDistinct;
    }
}
