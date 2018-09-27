using AzureSearchToolkit.Response.Materializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Request.Visitors
{
    /// <summary>
    /// Represents the result of a translated query including the
    /// remote <see cref="AzureSearchRequest"/> to select the data
    /// and the local <see cref="IAzureSearchMaterializer"/> necessary to
    /// instantiate objects.
    /// </summary>
    class AzureSearchTranslateResult
    {
        public AzureSearchRequest SearchRequest { get; }

        public IAzureSearchMaterializer Materializer { get; }

        public AzureSearchTranslateResult(AzureSearchRequest searchRequest, IAzureSearchMaterializer materializer)
        {
            SearchRequest = searchRequest;
            Materializer = materializer;
        }
    }
}
