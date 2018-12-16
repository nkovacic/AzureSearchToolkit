using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AzureSearchIndexAttribute: Attribute
    {
        public readonly string Index;

        public AzureSearchIndexAttribute(string index)
        {
            Index = index;
        }
    }
}
