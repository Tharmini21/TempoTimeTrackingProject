using Smartsheet.Api;
using Smartsheet.Api.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using TimeTrackingAutomation.Helpers;
using TimeTrackingAutomation.Utilities;

namespace TimeTrackingAutomation.Process
{
	public class SmartsheetAutomation
	{

        private const string AUTH_TOKEN_CONFIG = "SMARTSHEET_AUTH_TOKEN";
        private const string CHANGE_AGENT_CONFIG = "SMARTSHEET_CHANGE_AGENT";

        private const string TOKEN1 = "TOKEN1";
        private const string TOKEN2 = "TOKEN2";
        private string AuthToken { get; set; }
        private string ChangeAgent { get; set; }
        protected long? ConfigSheetId { get; set; }

        protected SmartsheetClient Client;
        protected Sheet ConfigSheet;
        protected DateTime StartTime;

        private static readonly object Locker = new object();
       
        protected SmartsheetAutomation()
        {
            this.AuthToken = ConfigurationManager.AppSettings[AUTH_TOKEN_CONFIG];
            this.ChangeAgent = ConfigurationManager.AppSettings[CHANGE_AGENT_CONFIG];
            Client = new SmartsheetBuilder().SetAccessToken(this.AuthToken).Build();
            StartTime = DateTime.Now;
        }

		static Dictionary<string, long> columnMap = new Dictionary<string, long>();
		
		protected void InitLogs(SmartsheetAutomation automation)
        {
            Logger.InitLogging(Client, automation);
        }

    }
}
