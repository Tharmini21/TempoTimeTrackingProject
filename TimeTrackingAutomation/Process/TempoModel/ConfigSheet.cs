using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeTrackingAutomation.Process.TempoModel
{
	public class ConfigSheet
	{
		public string Key { get; set; }
		public dynamic Value { get; set; }
		public DateTime LastRun { get; set; }
	}
}
