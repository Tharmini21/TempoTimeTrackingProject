using Smartsheet.Api;
using Smartsheet.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeTrackingAutomation.Helpers
{
	public static class SmartsheetClientExtension
	{
		public static Sheet GetSheet(this SmartsheetClient client, long? sheetId)
		{
			return client.SheetResources.GetSheet(sheetId.Value, null, null, null, null, null, null, null);
		}
	}
}
