using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders.Internal;

/// <summary>
/// Represents a collection of table JOINs. Enumeration will yield a fully parsed JOIN clause as a STRING that can be added to a SQL query/statement.
/// </summary>
internal sealed class SqlJoin : SqlClauseCollection
{

    /// <summary>
    /// Add an INNER JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">The <see cref="Type" /> of entity mapped to the LEFT-side table for this JOIN.</typeparam>
    /// <typeparam name="TRight">The <see cref="Type" /> of the entity mapped to the right-side table for this JOIN clause.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">Hints for the join condition (only those that apply to SELECT are valid here!).</param>
    public void InnerJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints)
    {
        AddImpl<TLeft, TRight>(joinCondition, SqlOperators.Join.Inner, joinHints);
    }

    /// <summary>
    /// Add a LEFT (OUTER) JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">The <see cref="Type" /> of entity mapped to the LEFT-side table for this JOIN.</typeparam>
    /// <typeparam name="TRight">The <see cref="Type" /> of the entity mapped to the right-side table for this JOIN clause.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">Hints for the join condition (only those that apply to SELECT are valid here!).</param>
    public void LeftJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints)
    {
        AddImpl<TLeft, TRight>(joinCondition, SqlOperators.Join.Left, joinHints);
    }

    /// <summary>
    /// Add a RIGHT (OUTER) JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">The <see cref="Type" /> of entity mapped to the LEFT-side table for this JOIN.</typeparam>
    /// <typeparam name="TRight">The <see cref="Type" /> of the entity mapped to the right-side table for this JOIN clause.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">Hints for the join condition (only those that apply to SELECT are valid here!).</param>
    public void RightJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints)
    {
        AddImpl<TLeft, TRight>(joinCondition, SqlOperators.Join.Right, joinHints);
    }

    /// <summary>
    /// Add a FULL JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">The <see cref="Type" /> of entity mapped to the LEFT-side table for this JOIN.</typeparam>
    /// <typeparam name="TRight">The <see cref="Type" /> of the entity mapped to the right-side table for this JOIN clause.</typeparam>
    /// <param name="joinCondition">The join condition (ON) expression.</param>
    /// <param name="joinHints">Hints for the join condition (only those that apply to SELECT are valid here!).</param>
    public void FullJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> joinCondition, SqlHint joinHints)
    {
        AddImpl<TLeft, TRight>(joinCondition, SqlOperators.Join.Full, joinHints);
    }

    /// <summary>
    /// Add a CROSS JOIN between two tables.
    /// </summary>
    /// <typeparam name="TLeft">The <see cref="Type" /> of entity mapped to the LEFT-side table for this JOIN.</typeparam>
    /// <typeparam name="TRight">The <see cref="Type" /> of the entity mapped to the right-side table for this JOIN clause.</typeparam>
    /// <param name="joinHints">Hints for the join condition (only those that apply to SELECT are valid here!).</param>
    public void CrossJoin<TLeft, TRight>(SqlHint joinHints)
    {
        // As all expression related stuff do not apply to this ONE join type, we do everything in-function.

        // do we have cross joins between the two types already added?
        Type typeOfLeft = typeof(TLeft);
        Type typeOfRight = typeof(TRight);
        foreach (JoinInfo existingJoin in _joins)
        {
            if (existingJoin.Type is SqlOperators.Join.Cross)
            {
                // Since primary table must always be on the LEFT side, we only need to check one way.
                if (((existingJoin.Left == typeOfLeft) && (existingJoin.Right == typeOfRight))
                    || ((existingJoin.Right == typeOfLeft) && (existingJoin.Left == typeOfRight)))
                {
                    throw new InvalidOperationException($"A CROSS JOIN between the tables '{typeOfLeft.GetUsableTypeName()}' and '{typeOfRight.GetUsableTypeName()}' has already been added.");
                }
            }
        }

        typeOfRight.RetrievePersistenceContainerInfoOrThrowException().GetNameAndAlias(out string rightTableName, out string rightTableAlias);

        StringBuilder joinClause = new StringBuilder()
            .Append(SqlOperators.Join.Cross.ToSQL()).Append(' ')
                .Append(rightTableName).Append(' ').Append(rightTableAlias).Append(' ');

        List<SqlHint> localHints = new List<SqlHint>();
        if (!localHints.TryAdd(joinHints, SqlStatementType.Query, out string? errorMessage))
        {
            throw new InvalidOperationException(errorMessage);
        }

        if (localHints.Count > 0)
        {
            joinClause.Append(" WITH (")
                .AppendJoin(',', localHints.Select(h => h.ToSQL()))
                    .Append(')');
        }

        base.Add(joinClause.ToString());

        _joins.Add(new JoinInfo()
        {
            Type = SqlOperators.Join.Cross,
            Left = typeOfLeft,
            Right = typeOfRight,

            // a cross join has no conditions!
            Conditions = new List<JoinConditionInfo>()
        });
    }

    /// <summary>
    /// Initialise the collection.
    /// </summary>
    public SqlJoin()
        : base()
    {
    }

    /// <summary>
    /// Implementation of the Add operation.
    /// </summary>
    /// <typeparam name="TLeft">The <see cref="Type" /> of entity mapped to the LEFT-side table for this JOIN.</typeparam>
    /// <typeparam name="TRight">The <see cref="Type" /> of the entity mapped to the right-side table for this JOIN clause.</typeparam>
    /// <param name="expression">The join condition (ON) expression.</param>
    /// <param name="type">The type of join to add.</param>
    /// <param name="joinHints">Hints for the join condition (only those that apply to SELECT are valid here!).</param>
    private void AddImpl<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> expression, SqlOperators.Join type, SqlHint joinHints)
    {
        // expression validation:
        if (!SqlExpressionParser.IsValidCondition(expression, out string? conditionValidationError))
        {
            throw new ArgumentException($"Cannot add condition to JOIN clause: {conditionValidationError}");
        }

        // Resolve and retrieve metadata:
        Type typeOfLeft = typeof(TLeft);
        Type typeOfRight = typeof(TRight);

        string rightTableName = string.Empty, rightTableAlias = string.Empty;
        typeOfLeft.RetrievePersistenceContainerInfoOrThrowException().GetNameAndAlias(out string leftTableName, out string leftTableAlias);
        if (typeOfLeft == typeOfRight)
        {
            rightTableName = leftTableName;
            rightTableAlias = leftTableAlias;
        }
        else
        {
            typeOfRight.RetrievePersistenceContainerInfoOrThrowException().GetNameAndAlias(out rightTableName, out rightTableAlias);
        }

        // parse the condition:
        string parsedCondition = SqlExpressionParser.Parse(expression);
        if (string.IsNullOrWhiteSpace(parsedCondition))
        {
            throw new ArgumentException("Condition evaluates to a NULL, blank or empty string.");
        }

        // check for identical joins already added:
        List<JoinConditionInfo> splitConditions = ((parsedCondition is not null) ? JoinConditionInfo.Split(parsedCondition) : new List<JoinConditionInfo>());
        if (splitConditions.Count > 0)
        {
            foreach (JoinInfo join in _joins)
            {
                if (join.IsEquivalentTo(type, typeOfLeft, typeOfRight, splitConditions))
                {
                    throw new InvalidOperationException($"A(n) '{type.ToSQL()}' between the tables '{leftTableName}' and '{rightTableName}' has already been added.");
                }
            }
        }

        StringBuilder joinClause = new StringBuilder()
            .Append(type.ToSQL()).Append(' ')
                .Append(rightTableName).Append(' ').Append(rightTableAlias);

        List<SqlHint> localHints = new List<SqlHint>();
        if (!localHints.TryAdd(joinHints, SqlStatementType.Query, out string? errorMessage))
        {
            throw new InvalidOperationException(errorMessage);
        }

        if (localHints.Count > 0)
        {
            joinClause.Append(" WITH (")
                .AppendJoin(',', localHints.Select(h => h.ToSQL()))
                    .Append(')');
        }

        joinClause.Append(" ON ")
                .Append(parsedCondition);

        base.Add(joinClause.ToString());

        _joins.Add(new JoinInfo()
        {
            Type = type,
            Left = typeOfLeft,
            Right = typeOfRight,
            Conditions = splitConditions
        });
    }

    // Local collection that maintains metadata without stringifying it to let us compare.
    private readonly List<JoinInfo> _joins = new List<JoinInfo>();

    /// <summary>
    /// Information about a single join.
    /// </summary>
    private struct JoinInfo
    {
        /// <summary>
        /// Type of join.
        /// </summary>
        public SqlOperators.Join Type;

        /// <summary>
        /// Left table type.
        /// </summary>
        public Type Left;

        /// <summary>
        /// Right table type.
        /// </summary>
        public Type Right;

        /// <summary>
        /// The ON conditions.
        /// </summary>
        public List<JoinConditionInfo> Conditions;

        /// <summary>
        /// Returns if the stored instance is equivalent to the combination of provided arguments.
        /// </summary>
        /// <param name="type">Type of join.</param>
        /// <param name="left">Type of the left table entity.</param>
        /// <param name="right">Type of the right table entity.</param>
        /// <param name="conditions">The parsed join conditions split up into individual sets of operators/operands.</param>
        /// <returns>True if the instance is equivalent to the provided information.</returns>
        public bool IsEquivalentTo(SqlOperators.Join type, Type left, Type right, List<JoinConditionInfo> conditions)
        {
            if (Type != type)
            {
                return false;
            }

            if (((Left == left) && (Right == right))
                || ((Left == right) && (Right == left)))
            {
                return true;
            }

            foreach (JoinConditionInfo condition in Conditions)
            {
                foreach (JoinConditionInfo proposedCondition in conditions)
                {
                    if (condition.IsEquivalentTo(proposedCondition))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Information about a single clause of an ON condition in a JOIN.
    /// </summary>
    private struct JoinConditionInfo
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
        public static List<JoinConditionInfo> Split(string condition)
        {
            List<JoinConditionInfo> conditions = new List<JoinConditionInfo>();
            string[] recognisedOperators = new string[] { "=", "<>", ">", ">=", "<", "<=", "IS", "IS NOT" };

            // remove all paranthesis.
            condition = condition.Replace("(", "").Replace(")", "");

            // seperate each condition, splitting at AND/OR.
            string[] segments = condition.Split(new string[] { "AND", "OR" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string segment in segments)
            {
                // split the condition into operands and operator.
                string[] opop = segment.Split(recognisedOperators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                JoinConditionInfo jci = new JoinConditionInfo();

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
        public bool IsEquivalentTo(JoinConditionInfo proposedCondition)
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
