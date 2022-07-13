using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TimeTrackingAutomation.Process.TempoModel;
using TimeTrackingAutomation.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimeTrackingAutomation.Process
{
	public class JiraTempoApi
	{

		private const string PROCESS = "Time Api Process";

		public string token = ConfigurationManager.AppSettings["TempoAccessToken"];

		public ResultObject Getteams()
		{
			Logger.LogToConsole($"Starting {PROCESS}");
			try
			{
				{
					var Client = new HttpClient();
					string urlStr = "https://api.tempo.io/core/3/teams";
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlStr);
					request.Headers.Add("Authorization", "Bearer " + token);
					HttpResponseMessage response = Client.SendAsync(request).Result;
					string res = response.Content.ReadAsStringAsync().Result;
					ResultObject data = System.Text.Json.JsonSerializer.Deserialize<ResultObject>(res);
					Logger.LogToConsole($"{PROCESS} complete");

					foreach (var mem in data.results)
					{
						GetteamsMembers(mem.id);
					}
					return data;
				}
			}

			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				throw ex;
			}
		}

		public JObject GetteamsMembers(int teamid)
		{
			try
			{

				var Client = new HttpClient();
				string urlStr = "https://api.tempo.io/core/3/teams/" + teamid + "/members";
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlStr);
				request.Headers.Add("Authorization", "Bearer " + token);
				HttpResponseMessage response = Client.SendAsync(request).Result;
				string res = response.Content.ReadAsStringAsync().Result;
				var jObject = JObject.Parse(res);
				foreach (var item in jObject)
				{
					if (item.Key == "results")
					{
						string userid = "62c55db91bb561c33795c2ee";
						DateTime fromdate = Convert.ToDateTime("2022-06-01");
						DateTime todate = Convert.ToDateTime("2022-07-31");
						GetworklogwithDate(userid, fromdate, todate);

					}
				}
				return jObject;

			}

			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				throw ex;
			}
		}
		public RootObject Getworklog()
		{
			try
			{
				{
					var Client = new HttpClient();
					string urlStr = "https://api.tempo.io/core/3/worklogs";
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlStr);
					request.Headers.Add("Authorization", "Bearer " + token);
					HttpResponseMessage response = Client.SendAsync(request).Result;
					string res = response.Content.ReadAsStringAsync().Result;
					RootObject data = System.Text.Json.JsonSerializer.Deserialize<RootObject>(res);

					//SmartsheetClass st = new SmartsheetClass();
					//List<OpportunityRollupsheet> result = st.GetOpportunityRollupsheet(4614195078555524);
					//foreach (var item in data.results)
					//{
					//		foreach (var rollup in result)
					//		{

					//		if (item.issue.id == rollup.IssueId)
					//		{
					//			st.AddTempoSheetDetail(item, rollup.TimeTrackingSheetID);

					//		}

					//	}
					//}
					return data;
				}
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				throw ex;
			}
		}
		public RootObject GetworklogwithissueKey(string issuekey)
		{
			try
			{
				{
					var Client = new HttpClient();
					string urlStr = "https://api.tempo.io/core/3/worklogs/issue/" + issuekey;
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlStr);
					request.Headers.Add("Authorization", "Bearer " + token);
					HttpResponseMessage response = Client.SendAsync(request).Result;
					string res = response.Content.ReadAsStringAsync().Result;
					RootObject data = System.Text.Json.JsonSerializer.Deserialize<RootObject>(res);

					return data;
				}
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				throw ex;
			}
		}
		public RootObject GetworklogwithProjectKey(string projectkey)
		{
			try
			{
				{
					var Client = new HttpClient();
					string urlStr = "https://api.tempo.io/core/3/worklogs/project/" + projectkey;
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlStr);
					request.Headers.Add("Authorization", "Bearer " + token);
					HttpResponseMessage response = Client.SendAsync(request).Result;
					string res = response.Content.ReadAsStringAsync().Result;
					RootObject data = System.Text.Json.JsonSerializer.Deserialize<RootObject>(res);
					return data;
				}
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				throw ex;
			}
		}

		public RootObject GetworklogwithDate(string userId, DateTime fromdate, DateTime todate)
		{
			string query = "https://api.tempo.io/core/3/worklogs/user/" + userId + "?from=" + fromdate + "&to=" + todate + "";
			try
			{
				{
					var Client = new HttpClient();
					string urlStr = query;
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlStr);
					request.Headers.Add("Authorization", "Bearer " + token);
					HttpResponseMessage response = Client.SendAsync(request).Result;
					string res = response.Content.ReadAsStringAsync().Result;
					RootObject data = System.Text.Json.JsonSerializer.Deserialize<RootObject>(res);
					return data;
				}
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				throw ex;
			}
		}

	}
}
