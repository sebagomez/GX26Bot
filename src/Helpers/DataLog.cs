using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace GX26Bot.Helpers
{
	public class DataLog : TableEntity
	{
		public DataLog()
		{
			PartitionKey = "1";
			RowKey = DateTime.Now.Ticks.ToString();
		}

		public string Question { get; set; }
		public string Intent { get; set; }
		public string Answer { get; set; }
		public string User { get; set; }
	}
}