using Smartsheet.Api;
using Smartsheet.Api.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
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
		private static string Process;
		private static SmartsheetClient Client;
		private static readonly object Locker = new object();
		public static void LogToConsole(string message)
		{
			Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.CurrentCulture)}: {message}");
			LogWrite(message);
		}
		private static void Log(string logMessage, TextWriter txtWriter)
		{
			try
			{
				txtWriter.Write($"\r\n {DateTime.Now.ToString(CultureInfo.CurrentCulture)} : ");
				txtWriter.WriteLine("  :{0}", " " + logMessage);
			}
			catch (Exception ex)
			{
				LogWrite(ex.StackTrace);
			}
		}
		private static void LogWrite(string logMessage)
		{
			lock (Locker)
			{
				try
				{
					using (var writer = File.AppendText(GetRunLogFile()))
					{
						Log(logMessage, writer);
					}
				}
				catch (Exception ex)
				{
					Console.Write(ex.Message);
				}
			}
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

	}
}
