using System;
using System.Collections.Generic;
using System.Text;

namespace AzureSearchToolkit.IntegrationTest.Utilities
{
    enum EntityCrudType
    {
        Create = 0,
        CreateOrUpdate = 4,
        Retrieve = 3,
        Update = 1,
        Delete = 2
    }
}
