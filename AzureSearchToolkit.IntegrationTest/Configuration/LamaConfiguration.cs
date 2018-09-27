using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureSearchToolkit.IntegrationTest.Configuration
{
    public class LamaConfiguration
    {
        private static LamaConfiguration _current;

        private IConfiguration _configuration;
        private LamaConfigurationModel _configurationModel;

        public static LamaConfiguration Current()
        {
            if (_current == null)
            {
                _current = new LamaConfiguration("local.settings.json");
            }

            return _current;
        }

        public LamaConfiguration(string jsonSettingsPath)
        {
            var currentDirectory = AppContext.BaseDirectory;
            var builder = new ConfigurationBuilder()
                .SetBasePath(currentDirectory);

            if (!string.IsNullOrWhiteSpace(jsonSettingsPath))
            {
                builder = builder
                    .AddJsonFile(jsonSettingsPath, optional: true, reloadOnChange: true);
            }

            builder = builder.AddEnvironmentVariables();

            _configuration = builder.Build();

            SetModel();
        }

        public LamaConfiguration(LamaConfigurationModel lamaConfigurationModel)
        {
            SetModel(lamaConfigurationModel);
        }

        public LamaConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;

            SetModel();
        }

        public LamaConfigurationModel GetModel()
        {
            if (_configurationModel == null)
            {
                SetModel();
            }

            return _configurationModel;
        }

        internal void SetModel(LamaConfigurationModel configurationModel = null)
        {
            if (configurationModel == null)
            {
                _configurationModel = _configuration.Get<LamaConfigurationModel>();
            }
            else
            {
                _configurationModel = configurationModel;
            }

            _current = this;
        }
    }
}
