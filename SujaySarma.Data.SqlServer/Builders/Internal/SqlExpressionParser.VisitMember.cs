using SujaySarma.Data.Core.Attributes;
using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.SqlServer.Attributes;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SujaySarma.Data.SqlServer.Builders;

// Implementation of VisitMember.
// This is complicated, so it gets its own file!
internal sealed partial class SqlExpressionParser
{
    /// <summary>
    /// Resolves a member access (A.B) to its SQL table.column expression. If member is static or
    /// non-table mapped entity, its value is taken instead.
    /// </summary>
    protected override Expression VisitMember(MemberExpression node)
    {
        // We need the declaring type as that maps it to a table/column.
        // Early exit.
        if (node.Member.DeclaringType is null)
        {
            return node;
        }

        // This could be a static field access?
        if ((node.Member is FieldInfo fi) && fi.IsStatic)
        {
            object? staticFieldValue = fi.GetValue(null);
            if ((_currentEnum is not null) && _serialiseEnumsAsStrings)
            {
                staticFieldValue = staticFieldValue.ConvertTo(typeof(string));
            }

            _expressionBuffer.Push(staticFieldValue.GetSQLStringValue());
            return node;
        }

        // Try to determine the table alias
        Type discoverableType = node.Member.DeclaringType;
        Type originalExpressionType = _originalExpression.GetType();

        if (originalExpressionType.IsGenericType
            && (originalExpressionType.GetEntityTypesFromExpression() is Type[] entityTypesFromExpression)
            && node.Member.DeclaringType.TryFindAssignableType(entityTypesFromExpression, out Type? assignableTypeFromExpression))
        {
            discoverableType = assignableTypeFromExpression;
        }

        // Try to resolve the type to a persistence container
        bool isResolvable = false;
        PersistenceContainerInfo? pci = null;

        try
        {
            isResolvable = TypeDiscoveryFactory.TryResolve(discoverableType, out pci, SqlExtensions.GetSqlServerTypeDiscoveryOptions());
        }
        catch (TypeLoadException)
        {
            // Type resolution failed - this is expected for non-annotated types
            isResolvable = false;
        }

        if (!isResolvable || pci is null)
        {
            // No table alias, the member reference could be a static/constant reference (eg: string.Empty)
            object? constantValue = ResolveExpressionAsValue(node);
            if ((_currentEnum is not null) && _serialiseEnumsAsStrings)
            {
                constantValue = constantValue.ConvertTo(typeof(string));
            }

            _expressionBuffer.Push(constantValue.GetSQLStringValue());
            return node;
        }

        /*
            We have eliminated all other possibilities at this stage. What we have must be a property that maps to a table column.

            Now, these columns may be defined as two types:
            (1) a simple column. That we process in the nested ELSE block below.
            (2) a Foreign key into another object (table). This is defined on a proper type (struct/record/class) in the caller's business object, 
                meaning we need to retrieve the entire object (table) from the backend -- this requires us to pull all the columns of the foreign table 
                (not just the key column referenced).
        */
        if (node.Member.TryGetAttribute<SqlTableColumn>(out SqlTableColumn? columnAttribute) 
                && node.Member.TryGetPropertyOrFieldDataType(out Type? propertyOrFieldDataType))
        {
            bool isEnum = propertyOrFieldDataType.IsEnum;

            if (columnAttribute is SqlTableForeignKeyColumn fkColumnAttribute)
            {
                // Column is a foreign key. We need to add ALL columns from the referenced table 
                // to the original query. The called function takes care of un/typed references/etc.
                _expressionBuffer.Push(string.Join(',', fkColumnAttribute.GetAllColumnsForReferencedTable()));
            }
            else
            {
                string columnName = columnAttribute.CreateQualifiedName(quoteNames: true);
                string qualifiedColumn = $"{pci.ReferenceAlias}.{columnName}";

                // Check if it is a nullable:
                bool isNullable = propertyOrFieldDataType.IsNullable();
                // For nullable boolean types in boolean contexts, wrap with ISNULL to treat NULL as false
                if (isNullable && Nullable.GetUnderlyingType(propertyOrFieldDataType) == typeof(bool))
                {
                    qualifiedColumn = $"ISNULL({qualifiedColumn}, 0)";
                }

                // Do NOT quote things here as they are already properly quoted BEFORE we get here!
                _expressionBuffer.Push(qualifiedColumn);

                if (isEnum)
                {
                    _currentEnum = propertyOrFieldDataType;
                    _serialiseEnumsAsStrings = (columnAttribute.IfEnumSerialiseAs == PersistenceContainerMember.EnumSerialisationStrategy.AsString);
                }
            }
        }
        //else: It was a member without a SqlTableColumn annotation. We cannot process that!
        //      Leaving it alone is how it currently works, adding an exception here might wreck callers!

        return node;
    }


    /// <summary>
    /// When we need to find the VALUE pointed to by an A.B.C member access expression,
    /// this function recursively walks through to the final element and then walks back
    /// to resolve the value of A.B.C.
    /// </summary>
    /// <param name="expression">Expression to traverse.</param>
    /// <returns>The raw value of what we found.</returns>
    private static object? ResolveExpressionAsValue(MemberExpression expression)
    {
        object? parent = null;
        Stack<MemberInfo> members = new Stack<MemberInfo>();
        MemberExpression memberExpression = expression;

        while (true)
        {
            members.Push(memberExpression.Member);
            if ((memberExpression.Expression is not null) && (memberExpression.Expression is MemberExpression childExpression))
            {
                memberExpression = childExpression;
                continue;
            }

            break;
        }

        if ((memberExpression.Expression is not null) && (memberExpression.Expression is ConstantExpression ce) && (ce.Value is not null))
        {
            parent = ce.Value.GetValue(members.Pop());
        }

        while (members.Count > 0)
        {
            parent = parent.GetValue(members.Pop());
        }

        return parent;
    }

}
