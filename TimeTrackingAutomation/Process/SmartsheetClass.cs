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

namespace TimeTrackingAutomation.Process
{
	public class SmartsheetClass : SmartsheetAutomation
	{
		static Dictionary<string, long> columnMap = new Dictionary<string, long>();
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
							//LastRun = Convert.ToDateTime(tmpRow.Cells[2].Value),
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
							GetOpportunityRollupsheet(item.Value);
						}
						else
						{
							Logger.LogToConsole("Failed to get rollupsheetId.");
						}
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				var message = $"Unable to get configuration sheet due to exception: {ex.Message}";
				throw new ApplicationException(message, ex);
			}
		}
		public object SaveConfigSheetDetail()
		{
			string status = "";
			try
			{
				long sheetid = Convert.ToInt64(ConfigurationManager.AppSettings["JiraTempoConfigsheet"]);
				var sheet = Client.GetSheet(sheetid);
				List<Cell> cells = new List<Cell>();
				Cell[] cellsA = null;
				Row rowA = null;
				foreach (Row tmpRow in sheet.Rows)
				{
					foreach (Cell tmpCell in tmpRow.Cells)
					{
						if (tmpCell.DisplayValue == "Last Run TimeStamp")
						{
							
						}
						
					}
				}
			   
				   
					
					cellsA = new Cell[]
					{
					 //new Cell.UpdateCellBuilder(sheet.Columns[0].Id, "").Build()
					//,new Cell.UpdateCellBuilder(sheet.Columns[1].Id, ProjectKeyarr[0]).Build()
					//,new Cell.UpdateCellBuilder(sheet.Columns[2].Id, tm.issue.id).Build()
					//,new Cell.UpdateCellBuilder(sheet.Columns[3].Id, tm.issue.key).Build()

					};
					//rowA = new Row.AddRowBuilder(true, null, null, null, null).SetCells(cellsA).Build();
					//IList<Row> newRows = Client.SheetResources.RowResources.AddRows(sheetid, new Row[] { rowA });
					//Logger.LogToConsole($"Adding rows to sheet {sheet.Name} with {newRows.Count} rows");
					return status = "Data updated Successfully";
				

			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
			}
			return status;

		}



		public List<OpportunityRollupsheet> GetOpportunityRollupsheet(dynamic rollupsheetid)
		{
			try
			{
				long sheetid = Convert.ToInt64(rollupsheetid);
				Sheet sheet = Client.GetSheet(sheetid);
				Console.WriteLine("Loaded " + sheet.Rows.Count + " rows from sheet: " + sheet.Name);
				List<OpportunityRollupsheet> result = new List<OpportunityRollupsheet>();
				OpportunityRollupsheet Data = null;
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
							TimeTrackingSheetID= Convert.ToInt64(tmpRow.Cells[5].Value)
						};
					}
					
					result.Add(Data);
					//JiraTempoApi objapi = new JiraTempoApi();
					//objapi.Getteams();
					//objapi.GetworklogwithissueKey(IssueKey ? IssueKey);
				}
				return result;
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				var message = $"Unable to get rollup sheet data due to exception: {ex.Message}";
				throw new ApplicationException(message, ex);
			}
		}
		public object AddTempoSheetDetail(WorklogModel tm, long configsheetid)
		{
			Logger.LogToConsole($"Start {PROCESS}");
			string status = "";
			int totalrecordimported = 0;
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
					char[] spearator = {'-'};
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
					Logger.LogToConsole($"Adding rows to sheet {sheet.Name} with {newRows.Count} rows");
					Logger.LogToConsole($"Adding rows count {newRows.Count} rows");
					totalrecordimported += newRows.Count;
					Logger.LogToConsole($"Adding rows count {totalrecordimported} rows");
					//RowsImported += Client.SheetResources.RowResources.AddRows(sheetid, newRows.ToArray()).Count;
					RowsImported += Client.SheetResources.RowResources.AddRows(sheetid, newRows).Count;
					LogRun();
					return status = "Data inserted Successfully";
				}
				
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				Logger.LogJobRun(StartTime.ToString(CultureInfo.InvariantCulture),
				   DateTime.Now.ToString(CultureInfo.InvariantCulture), $"{PROCESS} failed.", true);
			}
			return status;

		}
		private void LogRun()
		{
			Logger.LogToConsole($"{PROCESS} complete");
			var startTime = StartTime.ToString(CultureInfo.InvariantCulture);
			var endTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
			var notes = $"{PROCESS} complete. rows imported: {RowsImported}";
			Logger.LogJobRun(startTime, endTime, notes, false);
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
