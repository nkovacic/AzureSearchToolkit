using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Request.Criteria
{
    /// <summary>
    /// Interface that all criteria must implement to be part of
    /// the query tree.
    /// </summary>
    public interface ICriteria
    {
        /// <summary>
        /// Name of this criteria as specified in the AzurSearch DSL.
        /// </summary>
        string Name { get; }
    }
}
