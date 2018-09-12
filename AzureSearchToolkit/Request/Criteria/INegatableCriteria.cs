using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Request.Criteria
{
    public interface INegatableCriteria
    {
        /// <summary>
        /// Provide a negative representation of this criteria.
        /// </summary>
        /// <returns>Negative represenation of this criteria.</returns>
        ICriteria Negate();
    }
}
