using Smartsheet.Api;
using Smartsheet.Api.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TimeTrackingAutomation.Helpers;
using TimeTrackingAutomation.Process;

namespace TimeTrackingAutomation.Utilities
{
	public class Logger
	{
		private static readonly string AssemblyPath =
			System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		//private static Sheet ErrorSheet;
		private static SmartsheetClient Client;
		private static string Process;
		private static Sheet RunLogSheet;
		public static void LogToConsole(string message)
		{
			Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: {message}");
		}
		private static string GetRunLogFile()
		{
			return Path.Combine(AssemblyPath, $"{Process} - log.txt");
		}

		public static void ClearLogFileContents()
		{
			File.WriteAllText(GetRunLogFile(), string.Empty);
		}
		public static void InitLogging(SmartsheetClient client, SmartsheetAutomation automation)
		{
			Client = client;
			Process = automation.GetType().Name;
		}
		public static void LogJobRun(string startTime, string finishTime, string notes, bool failed)
		{
			var rowToAdd = new List<Row>
			{
				new Row
				{
					Cells = new List<Cell>()
					{
						new Cell()
						{
							ColumnId = RunLogSheet.GetColumnByTitle("Job Start Time").Id,
							Value = startTime
						},
						new Cell()
						{
							ColumnId = RunLogSheet.GetColumnByTitle("Job Finish Time").Id,
							Value = finishTime
						},
						new Cell()
						{
							ColumnId = RunLogSheet.GetColumnByTitle("Notes").Id,
							Value = notes
						},
						new Cell()
						{
							ColumnId = RunLogSheet.GetColumnByTitle("Failed").Id,
							Value = failed
						}
					}
				}
			};
		}
	}
}
