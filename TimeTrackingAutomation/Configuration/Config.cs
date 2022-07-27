using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackingAutomation.Configuration
{
    public class Config
    {
        public Config()
        {
            this.ConfigItems = new List<ConfigItem>();
            this.ConfigDictionaries = new List<ConfigDictionary>();
        }
        protected internal List<ConfigItem> ConfigItems { get; private set; }
        protected internal List<ConfigDictionary> ConfigDictionaries { get; private set; }

        public ConfigItem GetConfigItem(string key)
        {
            return this.ConfigItems.FirstOrDefault(x => x.Key == key);
        }
        public ConfigDictionary GetConfigDictionary(string key)
        {
            return this.ConfigDictionaries.FirstOrDefault(x => x.Key == key);
        }
    }
}
