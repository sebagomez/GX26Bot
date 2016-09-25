using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace GX26Bot.Helpers
{
	public class ErrorLog : TableEntity
	{
		public string Message { get; set; }
		public string Question { get; set; }
		public string User { get; set; }

		public ErrorLog()
		{
			PartitionKey = "1";
			RowKey = DateTime.Now.Ticks.ToString();
		}

		public ErrorLog(Exception ex)
			: this()
		{
			Message = ex.Message;
			Exception inner = ex.InnerException;
			while (inner != null)
			{
				Message += $@"{Environment.NewLine}{inner.Message}";
				inner = inner.InnerException;
			}
		}
	}
}