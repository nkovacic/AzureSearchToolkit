using AzureSearchToolkit.Mapping;
using AzureSearchToolkit.Request.Criteria;
using AzureSearchToolkit.Request.Expressions;
using AzureSearchToolkit.Response.Materializers;
using AzureSearchToolkit.Utilities;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AzureSearchToolkit.Request.Visitors
{
    class AzureSearchQueryTranslator: CriteriaExpressionVisitor
    {
        readonly AzureSearchRequest searchRequest = new AzureSearchRequest();

        Type finalItemType;
        Func<Document, object> itemProjector;
        IAzureSearchMaterializer materializer;

        Func<Document, object> DefaultItemProjector
        {
            get { return document => Mapping.Materialize(document, SourceType); }
        }

        AzureSearchQueryTranslator(IAzureSearchMapping mapping, Type sourceType)
            : base(mapping, sourceType)
        {
        }

        static Type FindSourceType(Expression e)
        {
            var sourceQuery = QuerySourceExpressionVisitor.FindSource(e);

            if (sourceQuery == null)
            {
                throw new NotSupportedException("Unable to identify an IQueryable source for this query.");
            }

            return sourceQuery.ElementType;
        }

        internal static AzureSearchTranslateResult Translate(IAzureSearchMapping mapping, Expression e)
        {
            return new AzureSearchQueryTranslator(mapping, FindSourceType(e)).Translate(e);
        }

        AzureSearchTranslateResult Translate(Expression e)
        {           
            var evaluated = PartialEvaluator.Evaluate(e);

            CompleteHitTranslation(evaluated);
            
            //searchRequest.Query = ConstantCriteriaFilterReducer.Reduce(searchRequest.Query);
            ApplyTypeSelectionCriteria();

            return new AzureSearchTranslateResult(searchRequest, materializer);
        }

        void ApplyTypeSelectionCriteria()
        {
            var typeCriteria = Mapping.GetTypeSelectionCriteria(SourceType);

            searchRequest.Criteria = searchRequest.Criteria == null || searchRequest.Criteria == ConstantCriteria.True
                ? typeCriteria
                : AndCriteria.Combine(typeCriteria, searchRequest.Criteria);
        }

        void CompleteHitTranslation(Expression evaluated)
        {
            Visit(evaluated);

            if (materializer == null)
            {
                materializer = new ListAzureSearchMaterializer(itemProjector ?? DefaultItemProjector, finalItemType ?? SourceType);
            }              
            else if (materializer is ChainMaterializer && ((ChainMaterializer)materializer).Next == null)
            {
                ((ChainMaterializer)materializer).Next = new ListAzureSearchMaterializer(itemProjector ?? DefaultItemProjector, finalItemType ?? SourceType);
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable))
            {
                return VisitQueryableMethodCall(node);
            }   
            
            if (node.Method.DeclaringType == typeof(AzureSearchQueryExtensions))
            {
                return VisitAzureSearchExtensionsMethodCall(node);
            }
                
            if (node.Method.DeclaringType == typeof(AzureSearchMethods))
            {
                return VisitAzureSearchMethodsMethodCall(node);
            }
                

            return base.VisitMethodCall(node);
        }

        internal Expression VisitAzureSearchExtensionsMethodCall(MethodCallExpression m)
        {
            throw new NotSupportedException($"AzureSearch.{m.Method.Name} method is not supported");
        }

        internal Expression VisitQueryableMethodCall(MethodCallExpression m)
        {
            switch (m.Method.Name)
            {
                case "Select":
                    if (m.Arguments.Count == 2)
                    {
                        return VisitSelect(m.Arguments[0], m.Arguments[1]);
                    }
                        
                    throw GetOverloadUnsupportedException(m.Method);

                case "First":
                case "FirstOrDefault":
                case "Single":
                case "SingleOrDefault":
                    if (m.Arguments.Count == 1)
                    {
                        return VisitFirstOrSingle(m.Arguments[0], null, m.Method.Name);
                    }
                        
                    if (m.Arguments.Count == 2)
                    {
                        return VisitFirstOrSingle(m.Arguments[0], m.Arguments[1], m.Method.Name);
                    }
                       
                    throw GetOverloadUnsupportedException(m.Method);

                case "Where":
                    if (m.Arguments.Count == 2)
                    {
                        return VisitWhere(m.Arguments[0], m.Arguments[1]);
                    }
                        
                    throw GetOverloadUnsupportedException(m.Method);

                case "Skip":
                    if (m.Arguments.Count == 2)
                    {
                        return VisitSkip(m.Arguments[0], m.Arguments[1]);
                    }
                        
                    throw GetOverloadUnsupportedException(m.Method);

                case "Take":
                    if (m.Arguments.Count == 2)
                    {
                        return VisitTake(m.Arguments[0], m.Arguments[1]);
                    }
                        
                    throw GetOverloadUnsupportedException(m.Method);

                case "OrderBy":
                case "OrderByDescending":
                    if (m.Arguments.Count == 2)
                    {
                        return VisitOrderBy(m.Arguments[0], m.Arguments[1], m.Method.Name == "OrderBy");
                    }
                        
                    throw GetOverloadUnsupportedException(m.Method);

                case "ThenBy":
                case "ThenByDescending":
                    if (m.Arguments.Count == 2)
                    {
                        return VisitOrderBy(m.Arguments[0], m.Arguments[1], m.Method.Name == "ThenBy");
                    }
                        
                    throw GetOverloadUnsupportedException(m.Method);

                case "Count":
                case "LongCount":
                    if (m.Arguments.Count == 1)
                    {
                        return VisitCount(m.Arguments[0], null, m.Method.ReturnType);
                    }
                       
                    if (m.Arguments.Count == 2)
                    {
                        return VisitCount(m.Arguments[0], m.Arguments[1], m.Method.ReturnType);
                    }
                       
                    throw GetOverloadUnsupportedException(m.Method);

                case "Any":
                    if (m.Arguments.Count == 1)
                    {
                        return VisitAny(m.Arguments[0], null);
                    }
                       
                    if (m.Arguments.Count == 2)
                    {
                        return VisitAny(m.Arguments[0], m.Arguments[1]);
                    }
                        
                    throw GetOverloadUnsupportedException(m.Method);
            }

            throw new NotSupportedException($"Queryable.{m.Method.Name} method is not supported");
        }

        static NotSupportedException GetOverloadUnsupportedException(MethodInfo methodInfo)
        {
            return new NotSupportedException(
                $"Queryable.{methodInfo.GetSimpleSignature()} method overload is not supported");
        }

        Expression VisitAny(Expression source, Expression predicate)
        {
            materializer = new AnyAzureSearchMaterializer();
            searchRequest.SearchParameters.Top = 1;

            return predicate != null
                ? VisitWhere(source, predicate)
                : Visit(source);
        }


        Expression VisitCount(Expression source, Expression predicate, Type returnType)
        {
            materializer = new CountAzureSearchMaterializer(returnType);
            searchRequest.SearchParameters.IncludeTotalResultCount = true;
            searchRequest.SearchParameters.Top = 0;

            return predicate != null
                ? VisitWhere(source, predicate)
                : Visit(source);
        }

        Expression VisitFirstOrSingle(Expression source, Expression predicate, string methodName)
        {
            var single = methodName.StartsWith("Single", StringComparison.Ordinal);
            var orDefault = methodName.EndsWith("OrDefault", StringComparison.Ordinal);

            searchRequest.SearchParameters.Top = single ? 2 : 1;
            finalItemType = source.Type;
            materializer = new OneAzureSearchMaterializer(itemProjector ?? DefaultItemProjector, finalItemType, single, orDefault);

            return predicate != null
                ? VisitWhere(source, predicate)
                : Visit(source);
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

        Expression VisitWhere(Expression source, Expression lambdaPredicate)
        {
            var lambda = lambdaPredicate.GetLambda();
            var criteriaExpression = lambda.Body as CriteriaExpression ?? BooleanMemberAccessBecomesEquals(lambda.Body) as CriteriaExpression;

            if (criteriaExpression == null)
            {
                throw new NotSupportedException($"Where expression '{lambda.Body}' could not be translated");
            }

            searchRequest.Criteria = AndCriteria.Combine(searchRequest.Criteria, criteriaExpression.Criteria);

            return Visit(source);
        }

        Expression VisitOrderBy(Expression source, Expression orderByExpression, bool ascending)
        {
            var lambda = orderByExpression.GetLambda();
            var final = Visit(lambda.Body) as MemberExpression;

            if (final != null)
            {
                var fieldName = Mapping.GetFieldName(SourceType, final);

                if (!string.IsNullOrWhiteSpace(fieldName))
                {
                    fieldName += ascending ? "" : " desc";

                    searchRequest.AddOrderByField(fieldName);
                }
            }

            return Visit(source);
        }

        Expression VisitOrderByScore(Expression source, bool ascending)
        {
            //searc.SortOptions.Insert(0, new SortOption("_score", ascending));

            return Visit(source);
        }

        Expression VisitSelect(Expression source, Expression selectExpression)
        {
            var lambda = selectExpression.GetLambda();

            if (lambda.Parameters.Count != 1)
            {
                throw new NotSupportedException("Select method with T parameter is supported, additional parameters like index are not");
            }

            var selectBody = lambda.Body;

            if (selectBody is MemberExpression)
            {
                RebindPropertiesAndElasticFields(selectBody);
            }
                
            if (selectBody is NewExpression)
            {
                RebindSelectBody(selectBody, ((NewExpression)selectBody).Arguments, lambda.Parameters);
            }
                
            if (selectBody is MethodCallExpression)
            {
                RebindSelectBody(selectBody, ((MethodCallExpression)selectBody).Arguments, lambda.Parameters);
            }
                

            if (selectBody is MemberInitExpression)
            {
                RebindPropertiesAndElasticFields(selectBody);
            }
                
            finalItemType = selectBody.Type;

            return Visit(source);
        }

        void RebindSelectBody(Expression selectExpression, IEnumerable<Expression> arguments, IEnumerable<ParameterExpression> parameters)
        {
            var entityParameter = arguments.SingleOrDefault(parameters.Contains) as ParameterExpression;

            if (entityParameter == null)
            {
                RebindPropertiesAndElasticFields(selectExpression);
            }
            else
            {
                RebindElasticFieldsAndChainProjector(selectExpression, entityParameter);
            }  
        }

        /// <summary>
        /// We are using the whole entity in a new select projection. Re-bind any AzureSearchField references to JObject
        /// and ensure the entity parameter is a freshly materialized entity object from our default materializer.
        /// </summary>
        /// <param name="selectExpression">Select expression to re-bind.</param>
        /// <param name="entityParameter">Parameter that references the whole entity.</param>
        void RebindElasticFieldsAndChainProjector(Expression selectExpression, ParameterExpression entityParameter)
        {
            var projection = AzureSearchFieldsExpressionVisitor.Rebind(SourceType, Mapping, selectExpression);
            var compiled = Expression.Lambda(projection.Item1, entityParameter, projection.Item2).Compile();

            itemProjector = h => compiled.DynamicInvoke(DefaultItemProjector(h), h);
        }

        /// <summary>
        /// We are using just some properties of the entity. Rewrite the properties as JObject field lookups and
        /// record all the field names used to ensure we only select those.
        /// </summary>
        /// <param name="selectExpression">Select expression to re-bind.</param>
        void RebindPropertiesAndElasticFields(Expression selectExpression)
        {
            var projection = MemberProjectionExpressionVisitor.Rebind(SourceType, Mapping, selectExpression);          
            var compiled = Expression.Lambda(projection.Expression, projection.Parameter).Compile();
            
            itemProjector = h => compiled.DynamicInvoke(h);

            searchRequest.AddRangeToSelect(projection.Collected.ToArray());
        }

        Expression VisitSkip(Expression source, Expression skipExpression)
        {
            var skipConstant = Visit(skipExpression) as ConstantExpression;

            if (skipConstant != null)
            {
                searchRequest.SearchParameters.Skip = (int)skipConstant.Value;
            }
                
            return Visit(source);
        }

        Expression VisitTake(Expression source, Expression takeExpression)
        {
            var takeConstant = Visit(takeExpression) as ConstantExpression;

            if (takeConstant != null)
            {
                var takeValue = (int)takeConstant.Value;

                searchRequest.SearchParameters.Top = searchRequest.SearchParameters.Top.HasValue
                    ? Math.Min(searchRequest.SearchParameters.Top.GetValueOrDefault(), takeValue)
                    : takeValue;
            }

            return Visit(source);
        }
    }
}
