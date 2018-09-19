using AzureSearchToolkit.Mapping;
using AzureSearchToolkit.Request;
using AzureSearchToolkit.Request.Criteria;
using AzureSearchToolkit.Request.Expressions;
using AzureSearchToolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AzureSearchToolkit.Request.Visitors
{
    internal abstract class CriteriaExpressionVisitor: ExpressionVisitor
    {
        protected readonly IAzureSearchMapping Mapping;
        protected readonly Type SourceType;

        static readonly RangeComparison[] invertedRangeComparison =
        {
            RangeComparison.LessThan,
            RangeComparison.LessThanOrEqual,
            RangeComparison.GreaterThan,
            RangeComparison.GreaterThanOrEqual
        };

        protected CriteriaExpressionVisitor(IAzureSearchMapping mapping, Type sourceType)
        {
            Mapping = mapping;
            SourceType = sourceType;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(string))
            {
                return VisitStringMethodCall(node);
            }             

            if (node.Method.DeclaringType == typeof(Enumerable))
            {
                return VisitEnumerableMethodCall(node);
            }
                

            if (node.Method.DeclaringType == typeof(AzureSearchMethods))
            {
                return VisitAzureSearchMethodsMethodCall(node);
            }
                
            return VisitDefaultMethodCall(node);
        }

        Expression VisitDefaultMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Equals":
                    if (m.Arguments.Count == 1)
                    {
                        return VisitEquals(Visit(m.Object), Visit(m.Arguments[0]));
                    }
                        
                    if (m.Arguments.Count == 2)
                    {
                        return VisitEquals(Visit(m.Arguments[0]), Visit(m.Arguments[1]));
                    }
                        
                    break;

                case "Contains":
                    if (TypeHelper.FindIEnumerable(m.Method.DeclaringType) != null)
                    {
                        return VisitEnumerableContainsMethodCall(m.Object, m.Arguments[0]);
                    }
                        
                    break;
            }

            return base.VisitMethodCall(m);
        }

        protected Expression VisitAzureSearchMethodsMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "ContainsAny":
                    if (m.Arguments.Count == 2)
                    {
                        return VisitContains("ContainsAny", m.Arguments[0], m.Arguments[1], TermsExecutionMode.Or);
                    }
                        
                    break;

                case "ContainsAll":
                    if (m.Arguments.Count == 2)
                    {
                        return VisitContains("ContainsAll", m.Arguments[0], m.Arguments[1], TermsExecutionMode.And);
                    }
                        
                    break;
            }

            throw new NotSupportedException($"AzureSearch.{m.Method.Name} method is not supported");
        }

        protected Expression VisitEnumerableMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Contains":
                    if (m.Arguments.Count == 2)
                        return VisitEnumerableContainsMethodCall(m.Arguments[0], m.Arguments[1]);
                    break;
            }

            throw new NotSupportedException($"Enumerable.{m.Method.Name} method is not supported");
        }

        protected Expression VisitStringMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Contains":  // Where(x => x.StringProperty.Contains(value))
                    if (m.Arguments.Count == 1)
                    {
                        return VisitStringPatternCheckMethodCall(m.Object, m.Arguments[0], "/.*{0}.*/", m.Method.Name);
                    }
                        
                    break;

                case "StartsWith": // Where(x => x.StringProperty.StartsWith(value))
                    if (m.Arguments.Count == 1)
                    {
                        return VisitStringPatternCheckMethodCall(m.Object, m.Arguments[0], "{0}*", m.Method.Name);
                    }
                        
                    break;

                case "EndsWith": // Where(x => x.StringProperty.EndsWith(value))
                    if (m.Arguments.Count == 1)
                    {
                        return VisitStringPatternCheckMethodCall(m.Object, m.Arguments[0], "/.*{0}/", m.Method.Name);
                    }
                       
                    break;
            }

            return VisitDefaultMethodCall(m);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                    return node.Operand;

                case ExpressionType.Not:
                    var subExpression = Visit(node.Operand) as CriteriaExpression;

                    if (subExpression != null)
                    {
                        return new CriteriaExpression(NotCriteria.Create(subExpression.Criteria));
                    }
                        
                    break;
            }

            return base.VisitUnary(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            switch (node.Expression.NodeType)
            {
                case ExpressionType.Parameter:
                case ExpressionType.MemberAccess:
                    return node;

                default:
                    var memberName = node.Member.Name;

                    if (node.Member.DeclaringType != null)
                    {
                        memberName = node.Member.DeclaringType.Name + "." + node.Member.Name;
                    }
                        
                    throw new NotSupportedException($"{memberName} is of unsupported type {node.Expression.NodeType}");
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.OrElse:
                    return VisitOrElse(node);

                case ExpressionType.AndAlso:
                    return VisitAndAlso(node);

                case ExpressionType.Equal:
                    return VisitEquals(Visit(node.Left), Visit(node.Right));

                case ExpressionType.NotEqual:
                    return VisitNotEqual(Visit(node.Left), Visit(node.Right));

                case ExpressionType.GreaterThan:
                    return VisitRange(RangeComparison.GreaterThan, Visit(node.Left), Visit(node.Right));

                case ExpressionType.GreaterThanOrEqual:
                    return VisitRange(RangeComparison.GreaterThanOrEqual, Visit(node.Left), Visit(node.Right));

                case ExpressionType.LessThan:
                    return VisitRange(RangeComparison.LessThan, Visit(node.Left), Visit(node.Right));

                case ExpressionType.LessThanOrEqual:
                    return VisitRange(RangeComparison.LessThanOrEqual, Visit(node.Left), Visit(node.Right));

                default:
                    throw new NotSupportedException($"Binary expression '{node.NodeType}' is not supported");
            }
        }

        protected Expression BooleanMemberAccessBecomesEquals(Expression e)
        {
            e = Visit(e);

            var c = e as ConstantExpression;

            if (c?.Value != null)
            {
                if (c.Value.Equals(true))
                {
                    return new CriteriaExpression(ConstantCriteria.True);
                }
                    
                if (c.Value.Equals(false))
                {
                    return new CriteriaExpression(ConstantCriteria.False);
                }
            }

            var wasNegative = e.NodeType == ExpressionType.Not;

            if (e is UnaryExpression)
                e = Visit(((UnaryExpression)e).Operand);

            if (e is MemberExpression && e.Type == typeof(bool))
                return Visit(Expression.Equal(e, Expression.Constant(!wasNegative)));

            return e;
        }

        Expression VisitEnumerableContainsMethodCall(Expression source, Expression match)
        {
            var matched = Visit(match);

            // Where(x => constantsList.Contains(x.Property))
            if (source is ConstantExpression && matched is MemberExpression)
            {
                var memberExpression = (MemberExpression)matched;
                var field = Mapping.GetFieldName(SourceType, memberExpression);
                var containsSource = ((IEnumerable)((ConstantExpression)source).Value);

                // If criteria contains a null create an Or criteria with Terms on one
                // side and Missing on the other.
                var values = containsSource.Cast<object>().Distinct().ToList();
                var nonNullValues = values.Where(v => v != null).ToList();

                ICriteria criteria = TermsCriteria.Build(field, memberExpression.Member, nonNullValues);

                if (values.Count != nonNullValues.Count)
                {
                    criteria = OrCriteria.Combine(criteria, new MissingCriteria(field));
                }

                return new CriteriaExpression(criteria);
            }

            // Where(x => x.SomeList.Contains(constantValue))
            if (source is MemberExpression && matched is ConstantExpression)
            {
                var memberExpression = (MemberExpression)source;
                var value = ((ConstantExpression)matched).Value;

                var field = Mapping.GetFieldName(SourceType, memberExpression);

                return new CriteriaExpression(TermsCriteria.Build(field, memberExpression.Member, value));
            }

            throw new NotSupportedException(source is MemberExpression
                ? $"Match '{match}' in Contains operation must be a constant"
                : $"Unknown source '{source}' for Contains operation");
        }

        protected virtual Expression VisitStringPatternCheckMethodCall(Expression source, Expression match, string pattern, string methodName)
        {
            var matched = Visit(match);

            if (source is MemberExpression && matched is ConstantExpression)
            {
                var field = Mapping.GetFieldName(SourceType, (MemberExpression)source);
                var value = ((ConstantExpression)matched).Value;

                return new CriteriaExpression(new QueryStringCriteria(string.Format(pattern, value), field));
            }

            throw new NotSupportedException(source is MemberExpression
                ? $"Match '{match}' in Contains operation must be a constant"
                : $"Unknown source '{source}' for Contains operation");
        }

        Expression VisitAndAlso(BinaryExpression b)
        {
            return new CriteriaExpression(
                AndCriteria.Combine(CombineExpressions<CriteriaExpression>(b.Left, b.Right).Select(f => f.Criteria).ToArray()));
        }

        Expression VisitOrElse(BinaryExpression b)
        {
            return new CriteriaExpression(
                OrCriteria.Combine(CombineExpressions<CriteriaExpression>(b.Left, b.Right).Select(f => f.Criteria).ToArray()));
        }

        IEnumerable<T> CombineExpressions<T>(params Expression[] expressions) where T : Expression
        {
            foreach (var expression in expressions.Select(BooleanMemberAccessBecomesEquals))
            {
                if ((expression as T) == null)
                    throw new NotSupportedException($"Unexpected binary expression '{expression}'");

                yield return (T)expression;
            }
        }

        Expression VisitContains(string methodName, Expression left, Expression right, TermsExecutionMode executionMode)
        {
            var cm = ConstantMemberPair.Create(left, right);

            if (cm != null)
            {
                var values = ((IEnumerable)cm.ConstantExpression.Value).Cast<object>().ToArray();

                return new CriteriaExpression(TermsCriteria.Build(executionMode, Mapping.GetFieldName(SourceType, cm.MemberExpression), cm.MemberExpression.Member, values));
            }

            throw new NotSupportedException(methodName + " must be between a Member and a Constant");
        }

        Expression CreateExists(ConstantMemberPair cm, bool positiveTest)
        {
            var fieldName = Mapping.GetFieldName(SourceType, UnwrapNullableMethodExpression(cm.MemberExpression));

            var value = cm.ConstantExpression.Value ?? false;

            if (value.Equals(positiveTest))
            {
                return new CriteriaExpression(new ExistsCriteria(fieldName));
            }

            if (value.Equals(!positiveTest))
            {
                return new CriteriaExpression(new MissingCriteria(fieldName));
            }

            throw new NotSupportedException("A null test Expression must have a member being compared to a bool or null");
        }

        Expression VisitEquals(Expression left, Expression right)
        {
            var booleanEquals = VisitCriteriaEquals(left, right, true);

            if (booleanEquals != null)
            {
                return booleanEquals;
            }        

            var cm = ConstantMemberPair.Create(left, right);

            if (cm != null)
            {
                return cm.IsNullTest
                    ? CreateExists(cm, true)
                    : new CriteriaExpression(new TermCriteria(Mapping.GetFieldName(SourceType, cm.MemberExpression), cm.MemberExpression.Member, cm.ConstantExpression.Value));
            }

            throw new NotSupportedException("Equality must be between a Member and a Constant");
        }

        static Expression VisitCriteriaEquals(Expression left, Expression right, bool positiveCondition)
        {
            var criteria = left as CriteriaExpression ?? right as CriteriaExpression;
            var constant = left as ConstantExpression ?? right as ConstantExpression;

            if (criteria == null || constant == null)
                return null;

            if (constant.Value.Equals(positiveCondition))
                return criteria;

            if (constant.Value.Equals(!positiveCondition))
                return new CriteriaExpression(NotCriteria.Create(criteria.Criteria));

            return null;
        }

        static MemberExpression UnwrapNullableMethodExpression(MemberExpression m)
        {
            var lhsMemberExpression = m.Expression as MemberExpression;
            if (lhsMemberExpression != null && m.Member.Name == "HasValue" && m.Member.DeclaringType.IsGenericOf(typeof(Nullable<>)))
                return lhsMemberExpression;

            return m;
        }

        Expression VisitNotEqual(Expression left, Expression right)
        {
            var booleanEquals = VisitCriteriaEquals(left, right, false);
            if (booleanEquals != null)
                return booleanEquals;

            var cm = ConstantMemberPair.Create(left, right);

            if (cm == null)
                throw new NotSupportedException("A not-equal expression must be between a constant and a member");

            return cm.IsNullTest
                ? CreateExists(cm, false)
                : new CriteriaExpression(NotCriteria.Create(new TermCriteria(Mapping.GetFieldName(SourceType, cm.MemberExpression), cm.MemberExpression.Member, cm.ConstantExpression.Value)));
        }

        Expression VisitRange(RangeComparison rangeComparison, Expression left, Expression right)
        {
            var inverted = left is ConstantExpression;
            var cm = ConstantMemberPair.Create(left, right);

            if (cm == null)
            {
                throw new NotSupportedException("A {0} must test a constant against a member");
            }

            if (inverted)
            {
                rangeComparison = invertedRangeComparison[(int)rangeComparison];
            }

            var field = Mapping.GetFieldName(SourceType, cm.MemberExpression);
            return new CriteriaExpression(new RangeCriteria(field, cm.MemberExpression.Member, rangeComparison, cm.ConstantExpression.Value));
        }
    }
}
