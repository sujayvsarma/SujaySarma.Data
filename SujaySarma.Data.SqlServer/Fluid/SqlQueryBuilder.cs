using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.SqlServer.Fluid.AliasMaps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
            TypeTableAliasMap primaryTable = this.TypeTableMap.GetPrimaryTable();

            return string.Join(' ', 
                    "SELECT",
                    (_isDistinct ? "DISTINCT" : string.Empty),
                    ((_topCount < uint.MaxValue) ? $"TOP {_topCount}" : string.Empty),
                    string.Join(',', _selectColumnsList),
                    "FROM",
                    primaryTable.GetQualifiedTableName(),
                    primaryTable.Alias,
                    "WITH (NOLOCK)",
                    (Joins.HasItems ? string.Join(' ', Joins) : string.Empty),
                    (Where.HasItems ? $"WHERE {string.Join(string.Empty, Where)}" : string.Empty),
                    (OrderBy.HasItems ? $"ORDER BY {string.Join(',', OrderBy)}" : string.Empty)
                );
        }


        /// <summary>
        /// 
        /// THIS IS A SUPPORT-METHOD FOR SqlInsertFromQueryBuilder. DO NOT USE THIS METHOD DIRECTLY.
        /// 
        /// Inject the provided columns and values as static values to the end of the existing column collection
        /// </summary>
        /// <param name="additionalValues">Dictionary of additional values to inject</param>
        internal void InjectAdditionalValues(Dictionary<string, object?> additionalValues)
        {
            foreach (string columnName in additionalValues.Keys)
            {
                if (!_selectColumnsList.Any(cn => cn.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
                {
                    _selectColumnsList.Add($"{ReflectionUtils.GetSQLStringValue(additionalValues[columnName])} as [{columnName}]");
                }
            }
        }

        /// <summary>
        /// Register a column selection (may be a literal constant as well).
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="selectors">One or more selectors for the column (eg: u =&gt; u.Id). Do NOT use object selectors such as 'u =&gt; u' this will result in empty selectors. Instead use the Select[TObject]() overload.</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Select<TObject>(params Expression<Func<TObject, object>>[] selectors)
        {
            TypeTableMap.TryAdd<TObject>();
            foreach (Expression selector in selectors)
            {
                _selectColumnsList.Add(base.ExpressionToSQL(selector, true));
            }
            return this;
        }

        /// <summary>
        /// Register all columns from the specified <typeparamref name="TObject" /> object.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Select<TObject>()
        {
            TypeTableMap.TryAdd<TObject>();
            TypeTableAliasMap map = TypeTableMap.GetMap<TObject>()!;
            foreach (MemberTypeInfo memberTypeInformation in map.Discovery.Members.Values)
            {
                AddColumnIfNotExistsOrSkip($"{map.Alias}.{memberTypeInformation.Column.CreateQualifiedName()}");
            }
            return this;
        }

        /// <summary>
        /// Register intention to select ALL columns from ALL tables and joins added to the builder so far.
        /// </summary>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Select()
        {
            _selectColumnsList.Clear();
            foreach (TypeTableAliasMap typeTable in TypeTableMap)
            {
                foreach (MemberTypeInfo memberTypeInformation in typeTable.Discovery.Members.Values)
                {
                    AddColumnIfNotExistsOrSkip($"{typeTable.Alias}.{memberTypeInformation.Column.CreateQualifiedName()}");
                }
            }
            return this;
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
        /// Register intention to select only the first <paramref name="count" /> number of rows.
        /// </summary>
        /// <param name="count">Number of rows to pick up. Zero will return only Schema!</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Top(uint count)
        {
            _topCount = count;
            return this;
        }


        /// <summary>
        /// Collection of WHERE conditions
        /// </summary>
        public SqlWhere Where { get; init; }

        /// <summary>
        /// Collection of JOINs
        /// </summary>
        public SqlJoin Joins { get; init; }

        /// <summary>
        /// Collection of ORDER BY conditions
        /// </summary>
        public SqlOrderBy OrderBy { get; init; }


        private readonly List<string> _selectColumnsList;
        private uint _topCount;
        private bool _isDistinct;

        /// <summary>
        /// Defines the primary CLR object (and hence it's backing SQL Server table) that should be used
        /// to populate the query. Any unqualified column references are implicitly assumed to be homed
        /// in this object/table.
        /// </summary>
        /// <typeparam name="TPrimaryObject">Type of CLR object. Must have the <see cref="T:SujaySarma.Data.SqlServer.Attributes.TableAttribute" /> attribute decoration.</typeparam>
        /// <returns>Created instance of SqlQueryBuilder</returns>
        public static SqlQueryBuilder From<TPrimaryObject>()
            => new SqlQueryBuilder(typeof(TPrimaryObject));

        /// <summary>
        /// Initializer. Marked as private to prevent direct initialization of the object.
        /// Object init should happen only via the From() static method.
        /// </summary>
        private SqlQueryBuilder(Type tPrimaryObject)
        {
            TypeTableMap.TryAdd(tPrimaryObject, true);
            _selectColumnsList = new List<string>();
            _topCount = uint.MaxValue;
            _isDistinct = false;
            Joins = new SqlJoin(this.TypeTableMap);
            Where = new SqlWhere(this.TypeTableMap);
            OrderBy = new SqlOrderBy(this.TypeTableMap);
        }


        /// <summary>
        /// Adds the column name to the list distinctly.
        /// </summary>
        /// <param name="columnName">Name of the column to add.</param>
        private void AddColumnIfNotExistsOrSkip(string columnName)
        {
            if (_selectColumnsList.Any(cn => cn.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            _selectColumnsList.Add(columnName);
        }
    }
}
