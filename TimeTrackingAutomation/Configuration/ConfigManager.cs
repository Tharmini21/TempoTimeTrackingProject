using Smartsheet.Api.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTrackingAutomation.Helpers;
using TimeTrackingAutomation.Process;

namespace TimeTrackingAutomation.Configuration
{
    public class ConfigManager
    {
        private const string CONFIGURATION_KEY_COLUMN = "Key";
        private const string CONFIGURATION_VALUE1_COLUMN = "Value1";
        private const string CONFIGURATION_VALUE2_COLUMN = "Value2";

        private Sheet ConfigSheet;

        public ConfigManager(Sheet configSheet)
        {
            this.ConfigSheet = configSheet;
        }
        public ConfigItem GetConfigItem(string key)
        {
            var rows = ConfigSheet.Rows;
            var row = rows.Where(x => x.GetCellForColumn(ConfigSheet, CONFIGURATION_KEY_COLUMN)?.Value != null)
                .FirstOrDefault(x => x.GetCellForColumn(ConfigSheet, CONFIGURATION_KEY_COLUMN)?.Value?.Equals(key) ??
                    throw new ConfigurationErrorsException(
                        $"No configuration found for {key}. Please check configuration."));

            return new ConfigItem()
            {
                Key = key,
                Value1 = row.GetCellForColumn(ConfigSheet, CONFIGURATION_VALUE1_COLUMN)?.Value,
                Value2 = row.GetCellForColumn(ConfigSheet, CONFIGURATION_VALUE2_COLUMN)?.Value
            };
        }
        private ConfigDictionary GetConfigDictionary(string key)
        {
            var rows = ConfigSheet.Rows;
            var row = rows.Where(x => x.GetCellForColumn(ConfigSheet, CONFIGURATION_KEY_COLUMN)?.Value != null)
                .FirstOrDefault(x =>
                    x.GetCellForColumn(ConfigSheet, CONFIGURATION_KEY_COLUMN)?.Value.Equals(key) ??
                    throw new ConfigurationErrorsException(
                        $"No configuration found for {key}. Please check configuration."));

            return new ConfigDictionary()
            {
                Key = key,
                Values = rows.Where(r => r.ParentId == row?.Id)
                    .Select(r => new
                    {
                        key = r.GetCellForColumn(ConfigSheet, CONFIGURATION_VALUE1_COLUMN)?.Value,
                        value = r.GetCellForColumn(ConfigSheet, CONFIGURATION_VALUE2_COLUMN)?.Value
                    }).ToDictionary(kvp => kvp.key, kvp => kvp.value)
            };
        }

       
    }
}
