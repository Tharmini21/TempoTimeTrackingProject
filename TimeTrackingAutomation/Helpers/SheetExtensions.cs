using Smartsheet.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeTrackingAutomation.Helpers
{
	public static class SheetExtensions
	{
        public static Row GetRowByKeyLookup(this Sheet sheet, string key, Column keyColumn)
        {
            return sheet.Rows.FirstOrDefault(r => r.Cells[keyColumn.Index.Value]?.Value?.ToString() == key);
        }
        public static Column GetColumnByTitle(this Sheet sheet, string columnTitle, bool caseSensitive = false)
        {
            var column = sheet.Columns.FirstOrDefault(c => String.Equals(c.Title, columnTitle, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase));
            if (column == null)
                throw new ArgumentException($"The sheet '{sheet.Name}' does not contain a column with the title '{columnTitle}'");

            return column;
        }

    }
}
