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
			//string ProgramTitle = System.Diagnostics.Process.GetCurrentProcess().MainWindowTitle;
			//System.Diagnostics.Process[] Processes = System.Diagnostics.Process.GetProcesses();

			//for (int i = 0; i < Processes.Length; i++)
			//{
			//	if (Processes[i].MainWindowTitle == ProgramTitle)
			//	{
			//		Processes[i].CloseMainWindow();
			//	}
			//}
			Console.WriteLine("Task Schedular Start Process:" + DateTime.Now);

			//JiraTempoApi objapi = new JiraTempoApi();
			//Logger.ClearLogFileContents();
			//objapi.Getteams();
			SmartsheetClass smartsheet = new SmartsheetClass();
			Logger.ClearLogFileContents();
			smartsheet.GetConfigSheetDetail();
			Console.WriteLine("Task Schedular End Process:" +DateTime.Now);
			//smartsheet.SaveConfigSheetDetail();
			Console.ReadLine();
		}
		//public void CloseDuplicateApplications()
		//{
		//	string ProgramTitle = System.Diagnostics.Process.GetCurrentProcess().MainWindowTitle;
		//	System.Diagnostics.Process[] Processes = System.Diagnostics.Process.GetProcesses();

		//	for (int i = 0; i < Processes.Length; i++)
		//	{
		//		if (Processes[i].MainWindowTitle == ProgramTitle)
		//		{
		//			Processes[i].CloseMainWindow();
		//		}
		//	}
		//}
	}
}
