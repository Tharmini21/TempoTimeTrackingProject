using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTrackingAutomation.Process;
using TimeTrackingAutomation.Utilities;

namespace TimeTrackingAutomation
{
	class Program
	{
		public static void Main(string[] args)
		{

			//JiraTempoApi objapi = new JiraTempoApi();
			//Logger.ClearLogFileContents();
			//objapi.Getworklog();
			//objapi.Getteams();
			Console.WriteLine("Getting Config Sheet from Smartsheet :" + DateTime.Now);
			SmartsheetClass smartsheet = new SmartsheetClass();
			smartsheet.GetConfigSheetDetail();
			var configdata = new SmartsheetClass();
			Logger.ClearLogFileContents();
			configdata.GetConfigSheetDetail();
			Console.WriteLine("Task Schedular:" +DateTime.Now);
			Console.ReadLine();
		}
	}
}
