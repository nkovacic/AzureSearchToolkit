using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureSearchToolkit.IntegrationTest.Configuration
{
    public class LamaConfigurationModel
    {
        public string Environment { get; set; }

        public string SearchName { get; set; }
        public string SearchKey { get; set; }

        public bool IsLocalEnvironment()
        {
            return !string.IsNullOrWhiteSpace(Environment) && Environment.Equals("local", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsDevelopmentEnvironment()
        {
            var possibleNames = new[] { "develop", "development" };

            return !string.IsNullOrWhiteSpace(Environment) && possibleNames.Any(name => name.Equals(Environment, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsStagingEnvironment()
        {
            return !string.IsNullOrWhiteSpace(Environment) && Environment.Equals("staging", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsProductionEnvironment()
        {
            return !string.IsNullOrWhiteSpace(Environment) && Environment.Equals("production", StringComparison.OrdinalIgnoreCase);
        }
    }
}
