using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeTrackingAutomation.Process.TempoModel
{
	public class OpportunityRollupsheet
	{
		public int ProjectId { get; set; }
		public string ProjectName { get; set; }
		public int IssueId { get; set; }
		public string IssueKey { get; set; }
		public string TimeTrackingSheetLinks { get; set; }
		public long TimeTrackingSheetID { get; set; }

	}
}
