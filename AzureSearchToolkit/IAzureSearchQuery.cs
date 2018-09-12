using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureSearchToolkit
{
    public interface IAzureSearchQuery<out T> : IOrderedQueryable<T>
    {
    }
}
