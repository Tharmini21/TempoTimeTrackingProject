using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeTrackingAutomation.Process.TempoModel
{
	public class WorklogModel
	{
		public string self { get; set; }
		public int tempoWorklogId { get; set; }
		public int jiraWorklogId { get; set; }
		public int timeSpentSeconds { get; set; }
		public int billableSeconds { get; set; }
		public string description { get; set; }
		public DateTime createdAt { get; set; }
		public DateTime updatedAt { get; set; }
		public issue issue { get; set; }
		public author author { get; set; }
		public attributes attributes { get; set; }
	}
	public class issue
	{
		public string self { get; set; }
		public string key { get; set; }
		public int id { get; set; }
	}
	public class author
	{
		public string self { get; set; }
		public string accountId { get; set; }
		public string displayName { get; set; }
	}
	public class attributes
	{
		public string self { get; set; }
		//public Array values { get; set; }
	}
	public class metadata
	{
		public int count { get; set; }
		public int offset { get; set; }
		public int limit { get; set; }
	}

	public class RootObject
	{
		public string self { get; set; }
		public metadata metadata { get; set; }
		public List<WorklogModel> results { get; set; }

	}
}
