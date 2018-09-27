using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using System;

namespace AzureSearchToolkit.Response.Materializers
{
    /// <summary>
    /// Materializes a count operation by obtaining the total document count from the response.
    /// </summary>
    class CountAzureSearchMaterializer : IAzureSearchMaterializer
    {
        readonly Type returnType;

        public CountAzureSearchMaterializer(Type returnType)
        {
            this.returnType = returnType;
        }

        /// <summary>
        /// Materialize the result count for a given response.
        /// </summary>
        /// <param name="response">The <see cref="AzureOperationResponse"/> to obtain the count value from.</param>
        /// <returns>The result count expressed as either an int or long depending on the size of the count.</returns>
        public object Materialize(AzureOperationResponse<DocumentSearchResult> response)
        {
            if (response.Body.Count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(response), "Contains a negative number of documents.");
            }  

            return Convert.ChangeType(response.Body.Count, returnType);
        }
    }
}
