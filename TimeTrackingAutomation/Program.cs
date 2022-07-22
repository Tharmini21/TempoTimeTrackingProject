using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTrackingAutomation.Process;
using TimeTrackingAutomation.Utilities;
using System.Diagnostics;
namespace TimeTrackingAutomation
{
	class Program
	{

		public static void Main(string[] args)
		{
			Console.WriteLine("Task Schedular Start Process:" + DateTime.Now);
			//JiraTempoApi objapi = new JiraTempoApi();
			//Logger.ClearLogFileContents();
			//objapi.Getteams();
			SmartsheetClass smartsheet = new SmartsheetClass();
			Logger.ClearLogFileContents();
			smartsheet.GetConfigSheetDetail();
			Console.WriteLine("Task Schedular End Process:" +DateTime.Now);
			Console.ReadLine();
		}
	}
}
