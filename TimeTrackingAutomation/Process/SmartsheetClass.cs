using Smartsheet.Api;
using Smartsheet.Api.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTrackingAutomation.Process.TempoModel;
using TimeTrackingAutomation.Utilities;
using TimeTrackingAutomation.Helpers;
using TimeTrackingAutomation.Configuration;
using System.Globalization;
using System.Collections.Specialized;
using System.Threading;

namespace TimeTrackingAutomation.Process
{
	public class SmartsheetClass : SmartsheetAutomation
	{
		static Dictionary<string, long> columnMap = new Dictionary<string, long>();
		static Dictionary<string, long> rowMap = new Dictionary<string, long>();
		//public Dictionary<dynamic, dynamic> SmartsheetColumnMapping = new Dictionary<dynamic, dynamic>();
		public Dictionary<string, string> SmartsheetColumnMapping = new Dictionary<string, string>();
		private const string CONFIGURATION_KEY_COLUMN = "Key";
		private const string CONFIGURATION_VALUE1_COLUMN = "Value1";
		private const string CONFIGURATION_VALUE2_COLUMN = "Value2";
		private const string PROCESS = "Smartsheet Api Process";
		private const string CONFIG_CONFIGURATION_SHEET_ID = "CONFIGURATION_SHEET_ID";
		public static string ReadSheetMapping = "Smartsheet Column Mapping";
		public static string Jobstatus = "JobStatus";
		public static string LastRunTime = "Last Run EndTime";
		public static string TempoBulkSheetId = "TempoBulkSheetID";
		private int RowsImported;
		private int RowsFailedImported;
		private int TotalRowsfromTimecards;
		private string ErrorTime;
		public List<ConfigItem> GetConfigSheetDetail()
		{
			List<ConfigItem> result = new List<ConfigItem>();
			ConfigItem configitem = null;
			try
			{
				Logger.LogToConsole("Fetching Configuration sheet");
				long sheetid = Convert.ToInt64(ConfigurationManager.AppSettings["JiraTempoConfigsheet"]);
				var sheetdata = Client.GetSheet(sheetid);
				var config = new Config();
				foreach (Row tmpRow in sheetdata.Rows)
				{
					if (tmpRow.ParentId != null && (tmpRow.Cells[0]?.DisplayValue == ReadSheetMapping))
					{
						List<Row> subitems = sheetdata.Rows.Where(x => x.ParentId == tmpRow.Id).ToList();
						if (subitems != null)
						{
							SmartsheetColumnMapping.Clear();
							foreach (Row row in subitems)
							{
								SmartsheetColumnMapping.Add((string)row.Cells[1].Value, (string)row.Cells[2].Value);
							}
						}
					}
					foreach (Cell tmpCell in tmpRow.Cells)
					{
						configitem = new ConfigItem()
						{
							Key = Convert.ToString(tmpRow.Cells[0].Value),
							Value1 = (tmpRow.Cells[1].Value),
							Value2 = (tmpRow.Cells[2].Value),
						};

					}
					result.Add(configitem);
				}

				if (result != null)
				{
					foreach (var item in result)
					{
						if (item.Key == "RollupSheetID")
						{
							GetOpportunityRollupsheet(item.Value1, result);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				var message = $"Unable to get configuration sheet due to exception: {ex.Message}";
			}
			return result;
		}

		public List<OpportunityRollupsheet> GetOpportunityRollupsheet(dynamic rollupsheetid, List<ConfigItem> configdata)
		{
			List<OpportunityRollupsheet> result = new List<OpportunityRollupsheet>();
			OpportunityRollupsheet Data = null;
			long TempoBulkSheetID = 0;
			string LastRunDate = string.Empty;
			string fromdate = string.Empty;
			string todate = string.Empty;
			try
			{
				Logger.LogToConsole($"Caching list of record from rollup sheets");
				if (configdata != null)
				{
					foreach (var item in configdata)
					{
						if (item.Key == LastRunTime)
						{
							LastRunDate = Convert.ToString(item.Value1);
						}
						if (item.Key == TempoBulkSheetId)
						{
							TempoBulkSheetID = Convert.ToInt64(item.Value1);
						}
					}
				}
				long sheetid = Convert.ToInt64(rollupsheetid);
				Sheet sheet = Client.GetSheet(sheetid);
				foreach (Row tmpRow in sheet.Rows)
				{
					foreach (Cell tmpCell in tmpRow.Cells)
					{
						Data = new OpportunityRollupsheet()
						{
							ProjectId = Convert.ToString(tmpRow.Cells[0].Value),
							TimeTrackingSheetLinks = Convert.ToString(tmpRow.Cells[1].Value),
							TimeTrackingSheetID = Convert.ToInt64(tmpRow.Cells[2].Value)
						};
					}
					result.Add(Data);
				}
				if (result.Count > 0)
				{
					InitialCheckSmartsheetColumns(result);
					JiraTempoApi objapi = new JiraTempoApi();
					if (string.IsNullOrEmpty(LastRunDate))
					{
						fromdate = Convert.ToString(ConfigurationManager.AppSettings["Fromdate"]);
						todate = Convert.ToString(ConfigurationManager.AppSettings["Todate"]);
					}
					else
					{
						char[] spearator = { ' ' };
						string[] datearry = LastRunDate.Split(spearator);
						string DateString = datearry[0];
						IFormatProvider culture = new CultureInfo("en-US", true);
						string startdate = DateTime.ParseExact(DateString, "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
						//fromdate = startdate;
						fromdate = "2022-07-21";
						todate = DateTime.Now.ToString("yyyy-MM-dd");
						//fromdate >= LastRunDate;
						//todate <= todate;
					}
					RootObject data = objapi.Getworklog(fromdate, todate);
					if (data.results?.Count > 0)
					{
						TotalRowsfromTimecards = data.results.Count;
						var rolluplist = new List<OpportunityRollupsheet>();
						char[] spearator = { '-' };
						var rollupdata = data.results.Select(x => new { ProjectID = x.issue.key.Split(spearator).First() }).Distinct();
						//var rollupdata = data.results.Select(x => new { x.issue.id, x.issue.key, ProjectID = x.issue.key.Split(spearator).First(), x.author.accountId, x.author.displayName, x.startDate }).Distinct();
						//SaveRollupSheetDetail(rollupdata, sheetid);
						foreach (var worklog in data.results)
						{
							foreach (var rollup in result)
							{
								if (worklog.issue.key?.Split(spearator).First() == rollup.ProjectId)
								{
									AddTempoSheetDetail(worklog, rollup.TimeTrackingSheetID);
								}
								else if (result.Where(x => x.ProjectId.Contains(worklog.issue.key?.Split(spearator).First())).Any() == false)
								{
									AddTempoSheetDetail(worklog, TempoBulkSheetID);
									break;
								}
							}
						}
					}
					else
					{
						Logger.LogToConsole("Unable to get worklog data.");
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				var message = $"Unable to get rollup sheet data due to exception: {ex.Message}";
				throw new ApplicationException(message, ex);
			}
			Logger.LogToConsole($"Cached {result.Count} record from Opportunity rollup sheets");
			return result;
		}

		public object SaveRollupSheetDetail(OpportunityRollupsheet sheetdata, long configsheetid)
		{
			string status = "";
			try
			{
				long sheetid = Convert.ToInt64(configsheetid);
				Sheet sheet = Client.GetSheet(sheetid);
				columnMap.Clear();
				foreach (Column column in sheet.Columns)
					columnMap.Add(column.Title, (long)column.Id);

				List<Cell> cells = new List<Cell>();
				Cell[] cellsA = null;
				Row rowA = null;
				cellsA = new Cell[]
				{
					 new Cell.AddCellBuilder(sheet.Columns[0].Id, sheetdata.ProjectId).Build()
					//,new Cell.AddCellBuilder(sheet.Columns[1].Id, sheetdata.TimeTrackingSheetID).Build()
					//,new Cell.AddCellBuilder(sheet.Columns[2].Id, sheetdata.TimeTrackingSheetLinks).Build()
				};
				rowA = new Row.AddRowBuilder(true, null, null, null, null).SetCells(cellsA).Build();
				IList<Row> newRows = Client.SheetResources.RowResources.AddRows(sheetid, new Row[] { rowA });
				Logger.LogToConsole($"Adding RowsImported count {RowsImported} rows");
				status = "Data inserted Successfully";
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
			}
			return status;
		}

		public object AddTempoSheetDetail(WorklogModel worklog, long configsheetid)
		{
			Logger.LogToConsole($"Start {PROCESS}");
			string status = "";
			try
			{
				long sheetid = Convert.ToInt64(configsheetid);
				Sheet sheet = Client.GetSheet(sheetid);
				if (worklog != null)
				{
					Row rowA = null;
					var cells = new List<Cell>();
					char[] spearator = { '-' };
					string[] ProjectKeyarr = worklog.issue.key.Split(spearator);

					foreach (var mapvalue in SmartsheetColumnMapping)
					{
						var keyValuePairdata = mapvalue;
						var columnTitle = keyValuePairdata.Key;
						var columnId = sheet.GetColumnByTitle(columnTitle).Id;
						if (keyValuePairdata.Value != null)
						{
							if (keyValuePairdata.Value.Contains("."))
							{
								char[] columnspearator = { '.' };
								var data = keyValuePairdata.Value.Split(columnspearator).First();
								var Arrayvalues = typeof(WorklogModel).GetProperties().First(p => p.Name == data).GetValue(worklog);
								var newvalue = Arrayvalues.GetType().GetProperties().First(x => x.Name == keyValuePairdata.Value.Split(columnspearator)[1]).GetValue(Arrayvalues);
								cells.Add(new Cell
								{
									ColumnId = columnId,
									Value = newvalue
								});
							}
							else
							{
								var value = typeof(WorklogModel).GetProperties().First(p => p.Name == keyValuePairdata.Value).GetValue(worklog);
								cells.Add(new Cell
								{
									ColumnId = columnId,
									Value = value
								});
							}
						}
						else
						{
							if (keyValuePairdata.Key == "Project Id" || keyValuePairdata.Key == "Task Id")
							{
								cells.Add(new Cell
								{
									ColumnId = columnId,
									Value = keyValuePairdata.Key == "Project Id" ? ProjectKeyarr[0] : ProjectKeyarr[1]
								});
							}
						}
					}
					rowA = new Row.AddRowBuilder(true, null, null, null, null).SetCells(cells).Build();
					IList<Row> newRows = Client.SheetResources.RowResources.AddRows(sheetid, new Row[] { rowA });
					RowsImported += newRows.Count;
					Logger.LogToConsole($"Adding RowsImported count {RowsImported} rows");
					LogRun();
					status = "Data inserted Successfully";
				}
				else
				{
					return ("Given time card data is null value.");
				}
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				RowsFailedImported = TotalRowsfromTimecards - RowsImported;
				var notes = $"{PROCESS} failed.\n" + "Total rows catched from timecard :" + TotalRowsfromTimecards + "\n" + "Rows imported :" + RowsImported + "\n" + "Rows Failed to Import :" + RowsFailedImported;
				ErrorTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
				LogJobRun(StartTime.ToString(CultureInfo.InvariantCulture),
				   DateTime.Now.ToString(CultureInfo.InvariantCulture), ErrorTime, notes, false,"","");
			}
			return status;
		}
		private void LogRun()
		{
			Logger.LogToConsole($"{PROCESS} complete");
			RowsFailedImported = TotalRowsfromTimecards - RowsImported;
			var startTime = StartTime.ToString(CultureInfo.InvariantCulture);
			var endTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
			var notes = $"{PROCESS} complete.\n" + "Total rows catched from timecard :" + TotalRowsfromTimecards + "\n" + "Rows imported:" + RowsImported + "\n" + "Rows Failed to Import:" + RowsFailedImported;
			LogJobRun(startTime, endTime, ErrorTime, notes, true,"","");
		}
		public void LogJobRun(string startTime, string finishTime, string ErrorTime, string notes, bool failed,string initialcheck, string reason)
		{
			long sheetid = Convert.ToInt64(ConfigurationManager.AppSettings["JiraTempoConfigsheet"]);
			Sheet RunLogSheet = Client.GetSheet(sheetid);
			string runstatus = string.Empty;
			if (failed == true)
			{
				runstatus = "Success";
			}
			else
			{
				runstatus = "Failed";
			}
			if (RowsFailedImported > 0 && RowsImported > 0)
			{
				runstatus = "Partially Success";
			}
			rowMap.Clear();
			var rowsToUpdate = new List<Row>();
			foreach (Row row in RunLogSheet.Rows)
			{
				if (row.Cells[0]?.DisplayValue == Jobstatus)
				{
					List<Row> subitems = RunLogSheet.Rows.Where(x => x.ParentId == row.Id).ToList();
					if (subitems != null)
					{
						foreach (Row r in subitems)
						{
							rowMap.Add((string)r.Cells[0]?.Value, (long)r.Id);
						}
					}
				}
			}
			if (rowMap.Count > 0)
			{
				foreach (KeyValuePair<string, long> tmpRow in rowMap)
				{
					rowsToUpdate.Add(new Row
					{
						Id = tmpRow.Value,
						Cells = new List<Cell>()
							{
								new Cell()
								{
									ColumnId = RunLogSheet.GetColumnByTitle(CONFIGURATION_VALUE1_COLUMN).Id,
									//Value = (tmpRow.Key == "Last Run EndTime" || tmpRow.Key == "Last Success Time" || tmpRow.Key == "Last Error Time") ? finishTime : 
									//(tmpRow.Key == "Last Run StartTime") ? startTime : 
									//(tmpRow.Key == "Run Notes") ? notes : 
									//(tmpRow.Key == "Last Run Status") ? runstatus : ""
									Value = (tmpRow.Key == "Last Run EndTime" || tmpRow.Key == "Last Success Time") ? finishTime :
									(tmpRow.Key == "Last Run StartTime") ? startTime :
									(tmpRow.Key == "Last Error Time") ? ErrorTime :
									(tmpRow.Key == "Run Notes") ? notes :
									(tmpRow.Key == "Last Run Status") ? runstatus : 
									(tmpRow.Key == "Initial Check") ? initialcheck :
									(tmpRow.Key == "Initial Check failed reason") ? reason : ""
								}
							}
					});
				}
			}
			IList<Row> updatedRow = Client.SheetResources.RowResources.UpdateRows(sheetid, rowsToUpdate);
		}
		public object InitialCheckSmartsheetColumns(List<OpportunityRollupsheet> result)
		{
			foreach (var rollup in result)
			{
				var sheetdata = Client.GetSheet(rollup.TimeTrackingSheetID);
				foreach (var mapvalue in SmartsheetColumnMapping.Keys)
				{
					    if (sheetdata.Columns.Where(x => x.Title.Contains(mapvalue)).Any() == false)
						{
						var initialreason = ($"The sheet '{sheetdata.Name}' does not contain a column with the title '{mapvalue}'");
						RowsFailedImported = TotalRowsfromTimecards - RowsImported;
						var notes = $"{PROCESS} failed.\n" + "Total rows catched from timecard :" + TotalRowsfromTimecards + "\n" + "Rows imported :" + RowsImported + "\n" + "Rows Failed to Import :" + RowsFailedImported;
						ErrorTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
						LogJobRun(StartTime.ToString(CultureInfo.InvariantCulture),
						   DateTime.Now.ToString(CultureInfo.InvariantCulture), ErrorTime, notes, false,"Failed",initialreason);
					    }
				}
			}
			return "";
		}
	}
}
