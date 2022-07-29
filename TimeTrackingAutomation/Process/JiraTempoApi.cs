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
					if (data.results.Count > 0)
					{
						foreach (var team in data.results)
						{
							GetteamsMembers(team.id);
						}
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
		public MembersResultObject GetteamsMembers(int teamid)
		{
			try
			{

				var Client = new HttpClient();
				string urlStr = "https://api.tempo.io/core/3/teams/" + teamid + "/members";
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlStr);
				request.Headers.Add("Authorization", "Bearer " + token);
				HttpResponseMessage response = Client.SendAsync(request).Result;
				string res = response.Content.ReadAsStringAsync().Result;
				MembersResultObject data = System.Text.Json.JsonSerializer.Deserialize<MembersResultObject>(res);
				foreach (var memobj in data.results)
				{
					if (memobj != null)
					{
						//string useraccountid = Convert.ToString(item.SelectToken("member.accountId"));
						string useraccountid = Convert.ToString(memobj.member.accountId);
						string fromdate = Convert.ToString(ConfigurationManager.AppSettings["Fromdate"]);
						string todate = Convert.ToString(ConfigurationManager.AppSettings["Todate"]);
						GetworklogwithDate(useraccountid, fromdate, todate);

					}
				}
				return data;

			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				throw ex;
			}
		}
		public RootObject GetworklogwithDate(string userId, string fromdate, string todate)
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
					RootObject responsedata = System.Text.Json.JsonSerializer.Deserialize<RootObject>(res);
					return responsedata;
				}
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				throw ex;
			}
		}
		public JObject GetteamsMembers1(int teamid)
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
				JArray jarr = (JArray)jObject["results"];
				foreach (var item in jarr)
				{
					if (item != null)
					{
						string useraccountid = Convert.ToString(item.SelectToken("member.accountId"));
						string fromdate = Convert.ToString("2022-06-01");
						string todate = Convert.ToString("2022-07-31");
						GetworklogwithDate(useraccountid, fromdate, todate);

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
		public RootObject Getworklog(string fromdate, string todate)
		{

			Logger.LogToConsole("Fetching Bulk Worklog Data.");
			string query = "https://api.tempo.io/core/3/worklogs"+"?from=" + fromdate + "&to=" + todate + "";
			RootObject data = null;
			try
			{
				{
					var Client = new HttpClient();
					string urlStr = query;
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlStr);
					request.Headers.Add("Authorization", "Bearer " + token);
					HttpResponseMessage response = Client.SendAsync(request).Result;
					string res = response.Content.ReadAsStringAsync().Result;
					var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(res);
					//var s = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
					data = System.Text.Json.JsonSerializer.Deserialize<RootObject>(res);
					//if (data.results.Count > 0)
					//{
					//	//var jsonTask = response.Content.ReadAsAsync<JsonObject>();
					//	//var jsonObject = jsonTask.Result;
					//}
				}
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				throw ex;
			}
			Logger.LogToConsole($"Cached {data.results?.Count} record from worklog from Tempo api Call.");
			return data;
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
					string fromdate = Convert.ToString(ConfigurationManager.AppSettings["Fromdate"]);
					string todate = Convert.ToString(ConfigurationManager.AppSettings["Todate"]);
					string query = "https://api.tempo.io/core/3/worklogs/project/" + projectkey + "?from=" + fromdate + "&to=" + todate + "";
					var Client = new HttpClient();
					//string urlStr = "https://api.tempo.io/core/3/worklogs/project/" + projectkey;
					HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, query);
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
