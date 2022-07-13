using Smartsheet.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackingAutomation.Helpers
{
	public static class RowExtensions
	{
		public static Cell GetCellForColumn(this Row row, Sheet sheet, string columnName)
		{
			var cell = row.Cells.FirstOrDefault(c => c.ColumnId == sheet.Columns.FirstOrDefault(col => col.Title == columnName)?.Id);
			return cell;
		}
	}
}
