using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackingAutomation.Process.TempoModel
{
	public class TeamModel
	{
		public string self { get; set; }
		public int id { get; set; }
		public string name { get; set; }
		public string summary { get; set; }
		public string accountId { get; set; }
		public string displayName { get; set; }
	}
	public class lead
	{
		public string self { get; set; }
	}
	public class Metadata
	{
		public int count { get; set; }
	}
	public class program
	{
		public string self { get; set; }
		public int id { get; set; }
		public string name { get; set; }

	}
	public class links
	{
		public string self { get; set; }
	}
	public class members
	{
		public string self { get; set; }
	}
	public class permissions
	{
		public string self { get; set; }
	}

	public class ResultObject
	{
		public string self { get; set; }
		public Metadata metadata { get; set; }
		public List<TeamModel> results { get; set; }

	}
}
