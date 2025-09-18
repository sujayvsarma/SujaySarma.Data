using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.SqlServer.Attributes;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Base class implemented by our fluid statement builders.
    /// </summary>
    public abstract class SqlStatementBuilder
    {

        /// <summary>
        /// Build the statement as a SQL.
        /// </summary>
        /// <returns>SQL statement string OR empty string if there is no valid SQL statement.</returns>
        public virtual StringBuilder Build()
            => throw new NotImplementedException("Ouch! Someone wrote a Fluid-statement builder but forgot to implement the Build() function!");

        /// <summary>
        /// A helper function to parse lambda expressions to SQL
        /// </summary>
        /// <param name="expression">Lambda expression to parse</param>
        /// <param name="treatAssignmentsAsAlias">[Optional] When set, tells the parser to treat any assignments in the expression as aliases. For eg: 'a = s.Id' will turn into 's.Id as [a]'</param>
        /// <returns>SQL string expression</returns>
        protected string ExpressionToSQL(Expression expression, bool treatAssignmentsAsAlias = false)
            => _visitor.Tour(expression, treatAssignmentsAsAlias);

        /// <summary>
        /// Add a mapping for the given type.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to add mapping for.</typeparam>
        /// <param name="isPrimary">Flag to set this as a primary table for this statement sequence.</param>
        protected ClrToTableWithAlias Add<TObject>(bool isPrimary = false)
            => Add(typeof(TObject), isPrimary);

        /// <summary>
        /// Add a mapping for the given type.
        /// </summary>
        /// <param name="table">Type of .NET object to add mapping for.</param>
        /// <param name="isPrimary">Flag to set this as a primary table for this statement sequence.</param>
        protected ClrToTableWithAlias Add(Type table, bool isPrimary = false)
            => Map.Add(table, isPrimary);

        /// <summary>
        /// Retrieve a list of column names
        /// </summary>
        /// <param name="map">Discovered object.</param>
        /// <param name="skipFlags">Any columns with the mentioned flags (HasFlags) will be skipped.</param>
        /// <param name="columnNames">List of existing columns -- will be added to, uniquely.</param>
        protected void BuildColumnNames(ClrToTableWithAlias map, KeyTypesEnum skipFlags, List<string> columnNames)
        {
            foreach (MemberTypeInfo member in map.TypeInfo.Members.Values)
            {
                TableColumnAttribute? columnAttribute = member.FieldOrPropertyInfo.GetCustomAttribute<TableColumnAttribute>();
                if (columnAttribute != null)
                {
                    // shortcut: nothing can be OR'ed with None.
                    if (skipFlags != KeyTypesEnum.None)
                    {
                        bool skipMember = false;
                        foreach (KeyTypesEnum flag in Enum.GetValues<KeyTypesEnum>())
                        {
                            if (skipFlags.HasFlag(flag) && columnAttribute.TypeOfKey.HasFlag(flag))
                            {
                                skipMember = true;
                                break;
                            }
                        }

                        if (skipMember)
                        {
                            continue;
                        }
                    }

                    string columnName = $"{map.Alias}.{member.Column.CreateQualifiedName()}";
                    if (!columnNames.Contains(columnName))
                    {
                        columnNames.Add(columnName);
                    };
                }
            }
        }

        /// <summary>
        /// Retrieve a list of column names along with their values
        /// </summary>
        /// <param name="sourceObject">Object whose values to fetch.</param>
        /// <param name="map">Discovered object.</param>
        /// <param name="skipFlags">Any columns with the mentioned flags (HasFlags) will be skipped.</param>
        /// <param name="columnNamesWithValues">List of existing column/value pairs -- will be added to, uniquely.</param>
        protected void BuildColumnNamesWithValues(ref object? sourceObject, ClrToTableWithAlias map, KeyTypesEnum skipFlags, Dictionary<string, string> columnNamesWithValues)
        {
            foreach (MemberTypeInfo member in map.TypeInfo.Members.Values)
            {
                TableColumnAttribute? columnAttribute = member.FieldOrPropertyInfo.GetCustomAttribute<TableColumnAttribute>();
                if (columnAttribute != null)
                {
                    // nothing can be OR'ed with None.
                    if (skipFlags != KeyTypesEnum.None)
                    {
                        bool skipMember = false;
                        foreach (KeyTypesEnum flag in Enum.GetValues<KeyTypesEnum>())
                        {
                            if (skipFlags.HasFlag(flag) && columnAttribute.TypeOfKey.HasFlag(flag))
                            {
                                skipMember = true;
                                break;
                            }
                        }

                        if (skipMember)
                        {
                            continue;
                        }
                    }

                    string columnName = $"{map.Alias}.{member.Column.CreateQualifiedName()}";
                    if (!columnNamesWithValues.ContainsKey(columnName))
                    {
                        columnNamesWithValues.Add(columnName,
                            ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref sourceObject, member)));
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve a list of column names along with their values
        /// </summary>
        /// <param name="sourceObject">Object whose values to fetch.</param>
        /// <param name="map">Discovered object.</param>
        /// <param name="onlyFlags">Only the columns with the mentioned flags (HasFlags) will be processed.</param>
        protected Dictionary<string, string> BuildColumnNamesWithValues(ref object? sourceObject, ClrToTableWithAlias map, KeyTypesEnum onlyFlags)
        {
            Dictionary<string, string> columnNamesWithValues = new Dictionary<string, string>();
            foreach (MemberTypeInfo member in map.TypeInfo.Members.Values)
            {
                TableColumnAttribute? columnAttribute = member.FieldOrPropertyInfo.GetCustomAttribute<TableColumnAttribute>();
                if (columnAttribute != null)
                {
                    foreach (KeyTypesEnum flag in Enum.GetValues<KeyTypesEnum>())
                    {
                        if (onlyFlags.HasFlag(flag) && columnAttribute.TypeOfKey.HasFlag(flag))
                        {
                            string columnName = $"{map.Alias}.{member.Column.CreateQualifiedName()}";
                            if (!columnNamesWithValues.ContainsKey(columnName))
                            {
                                columnNamesWithValues.Add(columnName,
                                    ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref sourceObject, member)));
                            }

                            // Process member only once per flag!
                            break;
                        }
                    }                    
                }
            }

            return columnNamesWithValues;
        }


        /// <summary>
        /// Initialize. Only child classes are allowed to call me.
        /// </summary>
        protected SqlStatementBuilder()
        {
            Map = new ClrToTableWithAliasCollection();
            _visitor = new SqlLambdaVisitor(Map);
        }

        // Cached visitor to improve performance across ParseToSql calls.
        private readonly SqlLambdaVisitor _visitor;

        /// <summary>
        /// The table map
        /// </summary>
        protected ClrToTableWithAliasCollection Map;
    }
}
