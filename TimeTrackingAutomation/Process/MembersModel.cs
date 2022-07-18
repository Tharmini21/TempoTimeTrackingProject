using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackingAutomation.Process
{
	public class MembersModel
	{
		public string self { get; set; }
		public team team { get; set; }
		public member member { get; set; }
	}
	public class metadata
	{
		public int count { get; set; }
	}
	public class member
	{
		public string self { get; set; }
		public string accountId { get; set; }
		public string displayName { get; set; }
	}
	public class team
	{
		public string self { get; set; }
	}
	public class MembersResultObject
	{
		public string self { get; set; }
		public metadata metadata { get; set; }
		public List<MembersModel> results { get; set; }

	}
}
