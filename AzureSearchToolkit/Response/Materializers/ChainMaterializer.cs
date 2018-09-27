using AzureSearchToolkit.Utilities;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;

namespace AzureSearchToolkit.Response.Materializers
{
    abstract class ChainMaterializer : IAzureSearchMaterializer
    {
        protected ChainMaterializer(IAzureSearchMaterializer next)
        {
            Next = next;
        }

        public IAzureSearchMaterializer Next
        {
            get; set;
        }

        /// <summary>
        /// Process response, then translate it to next materializer.
        /// </summary>
        /// <param name="response">AzureOperationResponse to obtain the existence of a result.</param>
        /// <returns>Return result of previous materializer, previously processed by self</returns>
        public virtual object Materialize(AzureOperationResponse<DocumentSearchResult> response)
        {
            Argument.EnsureNotNull("Next materializer must be setted.", Next);

            return Next.Materialize(response);
        }
    }
}
