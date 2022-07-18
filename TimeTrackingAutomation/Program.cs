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

			JiraTempoApi objapi = new JiraTempoApi();
			Logger.ClearLogFileContents();
			Console.WriteLine("Task Schedular Start Process:" + DateTime.Now);
			objapi.Getteams();
			//SmartsheetClass smartsheet = new SmartsheetClass();
			//smartsheet.GetConfigSheetDetail();
			//Logger.ClearLogFileContents();
			Console.WriteLine("Task Schedular End Process:" +DateTime.Now);
			//smartsheet.SaveConfigSheetDetail();
			Console.ReadLine();
		}
	}
}
