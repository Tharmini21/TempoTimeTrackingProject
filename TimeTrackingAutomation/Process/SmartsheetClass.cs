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
		public Dictionary<string, string> RollupSheetIds;
		protected internal List<ConfigDictionary> ConfigDictionaries { get; private set; }
		//private const string CONFIGURATION_KEY_COLUMN = "Key";
		private const string CONFIGURATION_KEY_COLUMN = "ProjectSheetID";
		private const string CONFIGURATION_VALUE1_COLUMN = "Value";
		private const string CONFIGURATION_VALUE2_COLUMN = "LastRun";
		private const string PROCESS = "Smartsheet Api Process";
		public static string RollupSheetsIds = "OpportunityRollupsheetId";
		private const string CONFIGURATION_SECTION = "OpportunityRollupsheetId";
		private const string CONFIG_CONFIGURATION_SHEET_ID = "CONFIGURATION_SHEET_ID";

		private int RowsImported;
		public List<ConfigSheet> GetConfigSheetDetail()
		{
			List<ConfigSheet> result = new List<ConfigSheet>();
			ConfigSheet configsheet = null;
			try
			{
				Logger.LogToConsole("Fetching Configuration sheet");
				long sheetid = Convert.ToInt64(ConfigurationManager.AppSettings["JiraTempoConfigsheet"]);
				var sheetdata = Client.GetSheet(sheetid);
				foreach (Row tmpRow in sheetdata.Rows)
				{
					foreach (Cell tmpCell in tmpRow.Cells)
					{
						configsheet = new ConfigSheet()
						{
							Key = Convert.ToString(tmpRow.Cells[0].Value),
							Value = (tmpRow.Cells[1].Value),
						};

					}
					result.Add(configsheet);
				}
				if (result != null)
				{
					foreach (var item in result)
					{
						if (item.Key == "RollupSheetID")
						{
							GetOpportunityRollupsheet(item.Value,result);
						}
					}
				}
				
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				var message = $"Unable to get configuration sheet due to exception: {ex.Message}";
				throw new ApplicationException(message, ex);
			}
			Logger.LogToConsole("Configuration sheet initialized");
			return result;
		}

		public List<OpportunityRollupsheet> GetOpportunityRollupsheet(dynamic rollupsheetid, List<ConfigSheet> configdata)
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
				long sheetid = Convert.ToInt64(rollupsheetid);
				Sheet sheet = Client.GetSheet(sheetid);
				foreach (Row tmpRow in sheet.Rows)
				{
					foreach (Cell tmpCell in tmpRow.Cells)
					{
						Data = new OpportunityRollupsheet()
						{
							ProjectId = Convert.ToInt32(tmpRow.Cells[0].Value),
							ProjectKey = Convert.ToString(tmpRow.Cells[1].Value),
							IssueId = Convert.ToInt32(tmpRow.Cells[2].Value),
							IssueKey = Convert.ToString(tmpRow.Cells[3].Value),
							TimeTrackingSheetLinks = Convert.ToString(tmpRow.Cells[4].Value),
							TimeTrackingSheetID = Convert.ToInt64(tmpRow.Cells[5].Value)
						};
					}

					result.Add(Data);
				}
				if (configdata != null)
				{
					
					foreach (var item in configdata)
					{
						if (item.Key == "Last Run TimeStamp")
						{
							LastRunDate = Convert.ToString(item.Value);
						}
						if (item.Key == "TempoBulkSheetID")
						{
							TempoBulkSheetID = Convert.ToInt64(item.Value);
						}
					}
				}
				if (result.Count > 0)
				{
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
						fromdate = startdate;
						todate = DateTime.Now.ToString("yyyy-MM-dd");
						//fromdate >= LastRunDate;
						//todate <= todate;
					}
					
					//fromdate = Convert.ToString(ConfigurationManager.AppSettings["Fromdate"]);
					//todate = Convert.ToString(ConfigurationManager.AppSettings["Todate"]);
					RootObject data = objapi.Getworklog(fromdate,todate);
					if (data.results != null)
					{
						foreach (var item in data.results)
						{
							foreach (var rollup in result)
							{

								if (item.issue.key == rollup.IssueKey)
								{
									AddTempoSheetDetail(item, rollup.TimeTrackingSheetID);
								}
								else if (result.Where(x => x.IssueKey.Contains(item.issue.key)).Any() == false)
								{
									AddTempoSheetDetail(item, TempoBulkSheetID);
									break;
								}
							}
						}
					}
					else {
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
		public object AddTempoSheetDetail(WorklogModel tm, long configsheetid)
		{
			Logger.LogToConsole($"Start {PROCESS}");
			string status = "";
			try
			{
				long sheetid = Convert.ToInt64(configsheetid);
				Sheet sheet = Client.GetSheet(sheetid);

				if (tm == null)
				{
					return ("Given data is null");
				}
				else
				{
					columnMap.Clear();
					foreach (Column column in sheet.Columns)
						columnMap.Add(column.Title, (long)column.Id);

					List<Cell> cells = new List<Cell>();
					Cell[] cellsA = null;
					Row rowA = null;
					char[] spearator = { '-' };
					string[] ProjectKeyarr = tm.issue.key.Split(spearator);
					cellsA = new Cell[]
					{
					 new Cell.AddCellBuilder(sheet.Columns[0].Id, "").Build()
					,new Cell.AddCellBuilder(sheet.Columns[1].Id, ProjectKeyarr[0]).Build()
					,new Cell.AddCellBuilder(sheet.Columns[2].Id, tm.issue.id).Build()
					,new Cell.AddCellBuilder(sheet.Columns[3].Id, tm.issue.key).Build()
					,new Cell.AddCellBuilder(sheet.Columns[4].Id, tm.author.accountId).Build()
					,new Cell.AddCellBuilder(sheet.Columns[5].Id, tm.tempoWorklogId).Build()
					,new Cell.AddCellBuilder(sheet.Columns[6].Id, tm.timeSpentSeconds).Build()
					,new Cell.AddCellBuilder(sheet.Columns[7].Id, tm.description).Build()

					};
					rowA = new Row.AddRowBuilder(true, null, null, null, null).SetCells(cellsA).Build();
					IList<Row> newRows = Client.SheetResources.RowResources.AddRows(sheetid, new Row[] { rowA });
					//Logger.LogToConsole($"Adding rows to sheet {sheet.Name} with {newRows.Count} rows");
					RowsImported += newRows.Count;
					Logger.LogToConsole($"Adding RowsImported count {RowsImported} rows");
					status = "Data inserted Successfully";
				}
				LogRun();
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				LogJobRun(StartTime.ToString(CultureInfo.InvariantCulture),
				   DateTime.Now.ToString(CultureInfo.InvariantCulture), $"{PROCESS} failed.", false);
			}
			return status;

		}
		private void LogRun()
		{
			Logger.LogToConsole($"{PROCESS} complete");
			var startTime = StartTime.ToString(CultureInfo.InvariantCulture);
			var endTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
			var notes = $"{PROCESS} complete. rows imported: {RowsImported}";
			LogJobRun(startTime, endTime, notes, true);
		}
		public void LogJobRun(string startTime, string finishTime, string notes, bool failed)
		{
			long sheetid = Convert.ToInt64(ConfigurationManager.AppSettings["JiraTempoConfigsheet"]);
			Sheet RunLogSheet = Client.GetSheet(sheetid);
			var rowsToUpdate = new List<Row>();

			rowMap.Clear();
			foreach (Row row in RunLogSheet.Rows)
			{
				rowMap.Add((string)row.Cells[0].Value, (long)row.Id);
			}

			foreach (KeyValuePair<string, long> tmpRow in rowMap)
			{
				if (tmpRow.Key == "Last Run TimeStamp")
				{
					rowsToUpdate.Add(new Row {
						Id = tmpRow.Value,
						Cells = new List<Cell>()
							{
								new Cell()
								{
									ColumnId = RunLogSheet.GetColumnByTitle("Value").Id,
									Value = finishTime
								}
							}
					});
				}
				if (tmpRow.Key == "Last Run Status")
				{
					rowsToUpdate.Add(new Row
					{
						Id = tmpRow.Value,
						Cells = new List<Cell>()
							{
								new Cell()
								{
									ColumnId = RunLogSheet.GetColumnByTitle("Value").Id,
									Value = failed
								}
							}
					});
				}
				
				if (tmpRow.Key == "Last Success Time")
				{
					rowsToUpdate.Add(new Row
					{
						Id = tmpRow.Value,
						Cells = new List<Cell>()
							{
								new Cell()
								{
									ColumnId = RunLogSheet.GetColumnByTitle("Value").Id,
									Value = finishTime
								}
							}
					});
				}
				if (failed == false && tmpRow.Key == "Last Error Time")
				{
					rowsToUpdate.Add(new Row
					{
						Id = tmpRow.Value,
						Cells = new List<Cell>()
							{
								new Cell()
								{
									ColumnId = RunLogSheet.GetColumnByTitle("Value").Id,
									Value = finishTime
								}
							}
					});
				}
				if (tmpRow.Key == "Run Notes")
				{
					rowsToUpdate.Add(new Row {
						Id = tmpRow.Value,
						Cells = new List<Cell>()
							{
								new Cell()
								{
									ColumnId = RunLogSheet.GetColumnByTitle("Value").Id,
									Value = notes
								}
							}
					});
				}
			}
			IList<Row> updatedRow = Client.SheetResources.RowResources.UpdateRows(sheetid, rowsToUpdate);
		}
		private ConfigDictionary GetConfigDictionary(string key, Sheet ConfigSheet)
		{
			var rows = ConfigSheet.Rows;
			var row = rows.Where(x => x.GetCellForColumn(ConfigSheet, CONFIGURATION_KEY_COLUMN)?.Value != null)
				.FirstOrDefault(x => x.GetCellForColumn(ConfigSheet, CONFIGURATION_KEY_COLUMN)?.Value.Equals(key) ??
					throw new ConfigurationErrorsException(
						$"No configuration found for {key}. Please check configuration."));

			return new ConfigDictionary()
			{
				Key = key,
				//Values = rows.Where(r => r.ParentId == row?.Id)
				Values = rows
					.Select(r => new
					{
						key = r.GetCellForColumn(ConfigSheet, CONFIGURATION_KEY_COLUMN)?.Value,
						value = r.GetCellForColumn(ConfigSheet, CONFIGURATION_VALUE1_COLUMN)?.Value
					}).ToDictionary(kvp => kvp.key, kvp => kvp.value)
			};
		}
	}
}
