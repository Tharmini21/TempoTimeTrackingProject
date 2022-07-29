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
		private int RowsImported;
		public List<ConfigItem> GetConfigSheetDetail()
		{
			List<ConfigItem> result = new List<ConfigItem>();
			ConfigItem configitem = null;
			try
			{
				Logger.LogToConsole("Fetching Configuration sheet");
				long sheetid = Convert.ToInt64(ConfigurationManager.AppSettings["JiraTempoConfigsheet"]);
				var sheetdata = Client.GetSheet(sheetid);
				long parentid = 0; string parentname = string.Empty;
				var config = new Config();

				foreach (Row tmpRow in sheetdata.Rows)
				{
					if (tmpRow.ParentId != null && (tmpRow.Cells[0]?.DisplayValue == ReadSheetMapping))
					{
						parentid = (long)tmpRow.Id;
						parentname = tmpRow.Cells[0]?.DisplayValue;
						Console.WriteLine("ParentId:" + parentid + " ParentName:" + parentname);
						if (parentid > 0)
						{
							List<Row> subitems = sheetdata.Rows.Where(x => x.ParentId == parentid).ToList();
							if (subitems != null)
							{
								SmartsheetColumnMapping.Clear();
								foreach (Row row in subitems)
								{
									SmartsheetColumnMapping.Add((string)row.Cells[1].Value, (string)row.Cells[2].Value);
								}
							}
						//SmartsheetColumnMapping = config.GetConfigDictionary(ReadSheetMapping)?.Values;
						}
						//if (tmpRow.ParentId != null)
						//{
						//	Console.WriteLine("Row #" + (tmpRow.Cells[0].DisplayValue?.ToString() == null ? tmpRow.Cells[1].DisplayValue.ToString() : tmpRow.Cells[0].DisplayValue.ToString() ) + " (Id=" + tmpRow.Id.ToString() + ") is a child of Row Id " + tmpRow.ParentId.ToString() + "<br/><br/>");
						//}
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
						//if (item.Key == "Dictionaries")
						//{
						//	//var sheetReadValue = config.GetConfigDictionary("Dictionaries")?.Values;
						//	SmartsheetColumnMapping = result?.Select(x => new
						//	{
						//		key = (string)x.Value1,
						//		value = (string)x.Value2
						//	}).ToDictionary(kvp => kvp.key, kvp => kvp.value);
						//}
						if (item.Key == "RollupSheetID")
						{
							GetOpportunityRollupsheet(item.Value1, result);
						}
						
					}
				}
				//LogRun();
				Logger.LogToConsole($"{PROCESS} complete");
				var startTime = StartTime.ToString(CultureInfo.InvariantCulture);
				var endTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
				var notes = $"{PROCESS} complete. rows imported: {RowsImported}";
				//LogJobRun(startTime, endTime, notes, true, sheetdata);
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				var message = $"Unable to get configuration sheet due to exception: {ex.Message}";
				//throw new ApplicationException(message, ex);
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
				if (configdata != null)
				{

					foreach (var item in configdata)
					{
						if (item.Key == "Last Run TimeStamp")
						{
							LastRunDate = Convert.ToString(item.Value1);
						}
						if (item.Key == "TempoBulkSheetID")
						{
							TempoBulkSheetID = Convert.ToInt64(item.Value1);
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
						//fromdate = "2022-07-21";
						todate = DateTime.Now.ToString("yyyy-MM-dd");
						//fromdate >= LastRunDate;
						//todate <= todate;
					}

					RootObject data = objapi.Getworklog(fromdate, todate);
					if (data.results.Count > 0)
					{
						var rolluplist = new List<OpportunityRollupsheet>();
						char[] spearator = { '-' };
						var rollupdata = data.results.Select(x => new {ProjectID = x.issue.key.Split(spearator).First()}).Distinct();
						//var rollupdata = data.results.Select(x => new { x.issue.id, x.issue.key, ProjectID = x.issue.key.Split(spearator).First(), x.author.accountId, x.author.displayName, x.startDate }).Distinct();
						//Project ID and TaskID, Author, Start Date
						//SaveRollupSheetDetail(rollupdata, sheetid);
						//foreach (var rd in rollupdata)
						//{
						//	rolluplist.Add(rd);
						//}
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
					

					//List<Cell> cells = new List<Cell>();
					Cell[] cellsA = null;
					Row rowA = null;
					char[] spearator = { '-' };
					string[] ProjectKeyarr = tm.issue.key.Split(spearator);

					var cells = new List<Cell>();
					foreach (var key in SmartsheetColumnMapping)
					{
						//var targetColumn = SmartsheetColumnMapping[key];
						//var columnTitle = targetColumn;
						var keyValuePairdata = key;
						var columnTitle = keyValuePairdata.Key;
					    var columnId = sheet.GetColumnByTitle(columnTitle).Id;

						var value = tm.GetType().GetProperty(keyValuePairdata.Value)?.Name;
						
						//GetValue(tm.Equals(keyValuePairdata.Value));
						//meterReadRow.GetType().GetProperty(key)?.GetValue(meterReadRow)
						//Select(user => user.Name)
						//var value = typeof(WorklogModel).GetProperties().First(x => x.Name == keyValuePairdata.Value).GetValue(item);
						//var value = (from wl in tm where )
						//.GetProperties().First(x => x.Name == keyValuePairdata.Value).GetValue(item);

						//var value = typeof(UtilityAnalysisItem)
						//	.GetProperties()
						//	.First(p => p.Name == key)
						//	.GetValue(item);

						//if (value == null)
						//	continue;

						//cells.Add(new Cell
						//{
						//	ColumnId = columnId,
						//	Value = value
						//});
						//Project Id
						//Task Id
					}
					cellsA = new Cell[]
					{
					 //new Cell.AddCellBuilder(sheet.GetColumnByTitle(SmartsheetColumnMapping.Keys)?.Id, ProjectKeyarr[0]).Build()
					 new Cell.AddCellBuilder(sheet.GetColumnByTitle("Project Id")?.Id, ProjectKeyarr[0]).Build()
					,new Cell.AddCellBuilder(sheet.GetColumnByTitle("Task Id")?.Id, ProjectKeyarr[1]).Build()
					,new Cell.AddCellBuilder(sheet.GetColumnByTitle("Author Id")?.Id, tm.author.accountId).Build()
					,new Cell.AddCellBuilder(sheet.GetColumnByTitle("Author Name")?.Id, tm.author.displayName).Build()
					,new Cell.AddCellBuilder(sheet.GetColumnByTitle("Start Date")?.Id, tm.startDate).Build()
					,new Cell.AddCellBuilder(sheet.GetColumnByTitle("Issue Id")?.Id, tm.issue.id).Build()
					,new Cell.AddCellBuilder(sheet.GetColumnByTitle("Issue Key")?.Id, tm.issue.key).Build()
					,new Cell.AddCellBuilder(sheet.GetColumnByTitle("Tempo Worklog Id")?.Id, tm.tempoWorklogId).Build()
					,new Cell.AddCellBuilder(sheet.GetColumnByTitle("TimeSpent Seconds")?.Id, tm.timeSpentSeconds).Build()
					,new Cell.AddCellBuilder(sheet.GetColumnByTitle("Billable Seconds")?.Id, tm.billableSeconds).Build()
					,new Cell.AddCellBuilder(sheet.GetColumnByTitle("Description")?.Id, tm.description).Build()
				};

					rowA = new Row.AddRowBuilder(true, null, null, null, null).SetCells(cellsA).Build();
					IList<Row> newRows = Client.SheetResources.RowResources.AddRows(sheetid, new Row[] { rowA });
					//Logger.LogToConsole($"Adding rows to sheet {sheet.Name} with {newRows.Count} rows");
					RowsImported += newRows.Count;
					Logger.LogToConsole($"Adding RowsImported count {RowsImported} rows");
					status = "Data inserted Successfully";
				}
				
			}
			catch (Exception ex)
			{
				Logger.LogToConsole(ex.Message);
				//LogJobRun(StartTime.ToString(CultureInfo.InvariantCulture),
				//   DateTime.Now.ToString(CultureInfo.InvariantCulture), $"{PROCESS} failed.", false);
			}
			return status;

		}
		//private void LogRun()
		//{
		//	Logger.LogToConsole($"{PROCESS} complete");
		//	var startTime = StartTime.ToString(CultureInfo.InvariantCulture);
		//	var endTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
		//	var notes = $"{PROCESS} complete. rows imported: {RowsImported}";
		//	LogJobRun(startTime, endTime, notes, true);
		//}
		public void LogJobRun(string startTime, string finishTime, string notes, bool failed,Sheet sheetdata)
		{
			//long sheetid = Convert.ToInt64(ConfigurationManager.AppSettings["JiraTempoConfigsheet"]);
			//Sheet RunLogSheet = Client.GetSheet(sheetid);
			Sheet RunLogSheet = sheetdata;
			var rowsToUpdate = new List<Row>();

			//var rows = RunLogSheet.Rows;
			//var rowdata = rows.Where(x => x.GetCellForColumn(RunLogSheet, "Last Run TimeStamp")?.Value != null).Select(x)
			//RunLogSheet.Rows.Where(x=>x.Cells.Contains())
			//result.Where(x => x.ProjectId.Contains(worklog.issue.key?.Split(spearator).First())).Any() == false)
			//foreach (Row row in RunLogSheet.Rows)
			//{
			//	//(query.Any(c => c.country.Equals("England")))
			//	//if (row.Cells.Any(x => x.Value.Equals("Last Run TimeStamp"))==true)
			//	if (row.Cells.Any(x => x.Value.Equals("Last Run TimeStamp")))
			//	{
			//		rowsToUpdate.Add(new Row
			//		{
			//			Id = row.Id,
			//			Cells = new List<Cell>()
			//				{
			//					new Cell()
			//					{
			//						ColumnId = RunLogSheet.GetColumnByTitle(CONFIGURATION_VALUE1_COLUMN).Id,
			//						Value = finishTime
			//					}
			//				}
			//		});
			//	}
			//}
			rowMap.Clear();
			foreach (Row row in RunLogSheet.Rows)
			{
				rowMap.Add((string)row.Cells[0].Value, (long)row.Id);
			}
			if (rowMap.Count > 0)
			{
				foreach (KeyValuePair<string, long> tmpRow in rowMap)
				{
					if (tmpRow.Key == "Last Run TimeStamp")
					{
						rowsToUpdate.Add(new Row
						{
							Id = tmpRow.Value,
							Cells = new List<Cell>()
							{
								new Cell()
								{
									ColumnId = RunLogSheet.GetColumnByTitle(CONFIGURATION_VALUE1_COLUMN).Id,
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
									ColumnId = RunLogSheet.GetColumnByTitle(CONFIGURATION_VALUE1_COLUMN).Id,
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
									ColumnId = RunLogSheet.GetColumnByTitle(CONFIGURATION_VALUE1_COLUMN).Id,
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
									ColumnId = RunLogSheet.GetColumnByTitle(CONFIGURATION_VALUE1_COLUMN).Id,
									Value = finishTime
								}
							}
						});
					}
					if (tmpRow.Key == "Run Notes")
					{
						rowsToUpdate.Add(new Row
						{
							Id = tmpRow.Value,
							Cells = new List<Cell>()
							{
								new Cell()
								{
									ColumnId = RunLogSheet.GetColumnByTitle(CONFIGURATION_VALUE1_COLUMN).Id,
									Value = notes
								}
							}
						});
					}
				}
			}
			//IList<Row> updatedRow = Client.SheetResources.RowResources.UpdateRows(sheetid, rowsToUpdate);
			IList<Row> updatedRow = Client.SheetResources.RowResources.UpdateRows((long)sheetdata.Id, rowsToUpdate);
		}
	}
}
