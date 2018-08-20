using AzureSearch.Linq.Request.Visitors;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AzureSearch.Linq.Request
{
    class AzureSearchQueryTranslator: CriteriaExpressionVisitor
    {
        SearchParameters Translate(Expression e)
        {
            /*
            var evaluated = PartialEvaluator.Evaluate(e);
            CompleteHitTranslation(evaluated);

            searchRequest.Query = ConstantCriteriaFilterReducer.Reduce(searchRequest.Query);
            ApplyTypeSelectionCriteria();

            return new ElasticTranslateResult(searchRequest, materializer);*/

            return null;
        }
    }
}
