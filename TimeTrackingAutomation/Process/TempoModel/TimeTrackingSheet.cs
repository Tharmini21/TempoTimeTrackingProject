using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackingAutomation.Process.TempoModel
{
	public class TimeTrackingSheet
	{
		public string ProjectId { get; set; }
		public int    TaskId { get; set; }
		public int    AuthorId { get; set; }
		public string AuthorName { get; set; }
		public DateTime StartDate { get; set; }
		public int IssueId { get; set; }
		public string IssueKey { get; set; }
		public int tempoWorklogId { get; set; }
		public int jiraWorklogId { get; set; }
		public int timeSpentSeconds { get; set; }
		public int billableSeconds { get; set; }
	
	}
}
