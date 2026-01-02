using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders.Internal;

/// <summary>
/// Represents a collection of table WHERE conditions. Enumeration will yield a fully parsed WHERE clause as a STRING that can be added to a SQL query/statement.
/// </summary>
internal sealed class SqlWhere : SqlClauseCollection
{

    #region === AND ===

    /// <summary>
    /// Appends a WHERE condition using the provided expression, concatenating it with the previous one(s) using the AND operator.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of the table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    public void AndWhere<TTable>(Expression<Func<TTable, bool>> condition)
    {
        InitOrAddImpl<TTable>(condition, SqlOperators.ConditionConcatenator.And);
    }

    /// <summary>
    /// Appends a WHERE condition using the provided expression, concatenating it with the previous one(s) using the AND operator.
    /// </summary>
    /// <typeparam name="TTable1">The <see cref="Type"/> of one table-mapped entity participating in this condition.</typeparam>
    /// <typeparam name="TTable2">The <see cref="Type"/> of the other table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    public void AndWhere<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> condition)
    {
        InitOrAddImpl<TTable1, TTable2>(condition, SqlOperators.ConditionConcatenator.And);
    }

    #endregion

    #region === OR ===

    /// <summary>
    /// Appends a WHERE condition using the provided expression, concatenating it with the previous one(s) using the OR operator.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of the table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    public void OrWhere<TTable>(Expression<Func<TTable, bool>> condition)
    {
        InitOrAddImpl<TTable>(condition, SqlOperators.ConditionConcatenator.Or);
    }

    /// <summary>
    /// Appends a WHERE condition using the provided expression, concatenating it with the previous one(s) using the OR operator.
    /// </summary>
    /// <typeparam name="TTable1">The <see cref="Type"/> of one table-mapped entity participating in this condition.</typeparam>
    /// <typeparam name="TTable2">The <see cref="Type"/> of the other table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    public void OrWhere<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> condition)
    {
        InitOrAddImpl<TTable1, TTable2>(condition, SqlOperators.ConditionConcatenator.Or);
    }

    #endregion

    #region When it is the first condition being added

    /// <summary>
    /// Create a new WHERE condition using the provided expression.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type"/> of the table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    public static SqlWhere Where<TTable>(Expression<Func<TTable, bool>> condition)
    {
        SqlWhere where = new SqlWhere();
        where.InitOrAddImpl<TTable>(condition, null);
        return where;
    }

    /// <summary>
    /// Create a new WHERE condition using the provided expression.
    /// </summary>
    /// <typeparam name="TTable1">The <see cref="Type"/> of one table-mapped entity participating in this condition.</typeparam>
    /// <typeparam name="TTable2">The <see cref="Type"/> of the other table-mapped entity participating in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    public static SqlWhere Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> condition)
    {
        SqlWhere where = new SqlWhere();
        where.InitOrAddImpl<TTable1, TTable2>(condition, null);
        return where;
    }

    #endregion


    /// <summary>
    /// Initialise the collection. 
    /// (Private to prevent direct init!)
    /// </summary>
    private SqlWhere()
        : base()
    {
    }


    /// <summary>
    /// The implementation function called by the public API, validates and adds the proposed condition to the internal collection.
    /// </summary>
    /// <typeparam name="TTable">The <see cref="Type" /> of the entity mapped to the table for this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    /// <param name="concatenator">The operator to use to concatenate this condition with the previous one.</param>
    private void InitOrAddImpl<TTable>(Expression<Func<TTable, bool>> condition, SqlOperators.ConditionConcatenator? concatenator = null)
    {
        // Type validation
        typeof(TTable).ValidateForOrmWithException();

        // expression validation:
        if (!SqlExpressionParser.IsValidCondition(condition, out string? conditionValidationError))
        {
            throw new ArgumentException($"Cannot add condition to WHERE clause: {conditionValidationError}");
        }

        // parse the condition:
        string parsedCondition = SqlExpressionParser.Parse(condition);
        if (string.IsNullOrWhiteSpace(parsedCondition))
        {
            throw new ArgumentException("Condition evaluates to a NULL, blank or empty string.");
        }

        bool isFirstCondition = (base.HasItems ? false : true);
        List<ConditionInfo> newConditions = ConditionInfo.Split(parsedCondition);
        
        if (! isFirstCondition)
        {
            foreach(ConditionInfo existingCondition in _conditions)
            {
                foreach(ConditionInfo candidate in newConditions)
                {
                    if (existingCondition.IsEquivalentTo(candidate))
                    {
                        throw new ArgumentException($"Cannot add duplicate condition to WHERE clause: '{candidate.Left} {candidate.Operator} {candidate.Right}'.");
                    }
                }
            }

            // Because *we* are calling this function and set this argument!
            base.Add(concatenator!.Value.ToSQL());
        }

        base.Add(parsedCondition);
        _conditions.AddRange(newConditions);
    }

    /// <summary>
    /// The implementation function called by the public API, validates and adds the proposed condition to the internal collection.
    /// </summary>
    /// <typeparam name="TTable1">The <see cref="Type" /> of the entity mapped to one of the tables in this condition.</typeparam>
    /// <typeparam name="TTable2">The <see cref="Type"/> of the entity mapped to the other of the tables in this condition.</typeparam>
    /// <param name="condition">The condition.</param>
    /// <param name="concatenator">The operator to use to concatenate this condition with the previous one.</param>
    private void InitOrAddImpl<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> condition, SqlOperators.ConditionConcatenator? concatenator = null)
    {
        // Type validation
        Type typeOf1 = typeof(TTable1);
        Type typeOf2 = typeof(TTable2);

        typeOf1.ValidateForOrmWithException();
        if (typeOf1 != typeOf2)
        {
            typeOf2.ValidateForOrmWithException();
        }

        // expression validation:
        if (!SqlExpressionParser.IsValidCondition(condition, out string? conditionValidationError))
        {
            throw new ArgumentException($"Cannot add condition to WHERE clause: {conditionValidationError}");
        }

        // parse the condition:
        string parsedCondition = SqlExpressionParser.Parse(condition);
        if (string.IsNullOrWhiteSpace(parsedCondition))
        {
            throw new ArgumentException("Condition evaluates to a NULL, blank or empty string.");
        }

        bool isFirstCondition = (base.HasItems ? false : true);
        List<ConditionInfo> newConditions = ConditionInfo.Split(parsedCondition);

        if (! isFirstCondition)
        {
            foreach (ConditionInfo existingCondition in _conditions)
            {
                foreach (ConditionInfo candidate in newConditions)
                {
                    if (existingCondition.IsEquivalentTo(candidate))
                    {
                        throw new ArgumentException($"Cannot add duplicate condition to WHERE clause: '{candidate.Left} {candidate.Operator} {candidate.Right}'.");
                    }
                }
            }

            // Because *we* are calling this function and set this argument!
            base.Add(concatenator!.Value.ToSQL());
        }

        base.Add(parsedCondition);
        _conditions.AddRange(newConditions);
    }


    // Local collection that maintains metadata without stringifying it to let us validate.
    private List<ConditionInfo> _conditions = new List<ConditionInfo>();

    /// <summary>
    /// Information about a single clause of a condition.
    /// </summary>
    private struct ConditionInfo
    {
        /// <summary>
        /// Operator.
        /// </summary>
        public string Operator;

        /// <summary>
        /// Operand 1
        /// </summary>
        public string Left;

        /// <summary>
        /// Operand 2
        /// </summary>
        public string Right;

        /// <summary>
        /// Split conditions (foo = bar) into its components (foo, =, bar).
        /// </summary>
        /// <param name="condition">The condition as parsed and returned by our SqlExpressionParser.</param>
        /// <returns>A list of JoinConditionInfo with all the conditions split to its components.</returns>
        public static List<ConditionInfo> Split(string condition)
        {
            List<ConditionInfo> conditions = new List<ConditionInfo>();
            string[] recognisedOperators = new string[] { "=", "<>", ">", ">=", "<", "<=", "IS", "IS NOT" };

            // remove all paranthesis.
            condition = condition.Replace("(", "").Replace(")", "");

            // seperate each condition, splitting at AND/OR.
            string[] segments = condition.Split(new string[] { "AND", "OR" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string segment in segments)
            {
                // split the condition into operands and operator.
                string[] opop = segment.Split(recognisedOperators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                ConditionInfo jci = new ConditionInfo();

                // Single boolean/unary expressions
                if (opop.Length < 2)
                {
                    if (opop[0].Equals("NOT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // NOT: treat as "c == 0"
                        jci.Left = opop[1];
                        jci.Operator = "=";
                        jci.Right = "0";
                    }
                    else
                    {
                        // IS: treat as "c == 1"
                        jci.Left = opop[0];
                        jci.Operator = "=";
                        jci.Right = "1";
                    }

                    conditions.Add(jci);
                    continue;
                }

                jci.Left = opop[0];
                jci.Right = opop[1];

                // operator goes missing during split. Find it!
                foreach (string opi in recognisedOperators)
                {
                    if (segment.Contains(opi))
                    {
                        jci.Operator = opi;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(jci.Operator))
                {
                    throw new ArgumentException($"No valid operator found in condition: '{segment}'.");
                }

                conditions.Add(jci);
            }

            return conditions;
        }

        /// <summary>
        /// Checks to see if the left/right pairs match up in any combination with what is stored in the instance.
        /// </summary>
        /// <param name="proposedCondition">The condition to check against.</param>
        /// <returns>True if the instance is equivalent to the provided information.</returns>
        public bool IsEquivalentTo(ConditionInfo proposedCondition)
        {
            if (Operator != proposedCondition.Operator)
            {
                return false;
            }

            if ((Left.Equals(proposedCondition.Left, StringComparison.InvariantCultureIgnoreCase) && Right.Equals(proposedCondition.Right, StringComparison.InvariantCultureIgnoreCase))
                || (Left.Equals(proposedCondition.Right, StringComparison.InvariantCultureIgnoreCase) && Right.Equals(proposedCondition.Left, StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }
    }
}
