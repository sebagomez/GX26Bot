using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace GX26Bot.Helpers
{
	public class StorageLogger
	{
		static StorageCredentials s_credentials = null;
		static CloudStorageAccount s_storageAccount = null;
		static CloudTableClient s_tableClient = null;

		private static bool Initilized(string account)
		{
			return s_storageAccount != null && s_credentials.AccountName == account;
		}

		static void Initialize(string account, string key)
		{
			Uri blobUri = new Uri(string.Format("http://{0}.blob.core.windows.net", account));
			Uri queueUri = new Uri(string.Format("http://{0}.queue.core.windows.net", account));
			Uri tableUri = new Uri(string.Format("http://{0}.table.core.windows.net", account));

			s_credentials = new StorageCredentials(account, key);
			s_storageAccount = new CloudStorageAccount(s_credentials, true);

			s_tableClient = s_storageAccount.CreateCloudTableClient();
		}

		static CloudTableClient GetTableClient(string account, string key)
		{
			if (!Initilized(account))
				Initialize(account, key);

			return s_tableClient;
		}

		public static void LogData(DataLog log)
		{
			Insert("RUDILog", log);
		}

		public static void LogError(ErrorLog log)
		{
			Insert("RUDIErrors", log);
		}

		static void Insert(string tableName, TableEntity data)
		{
			try
			{
				CloudTableClient tableClient = GetTableClient(BotConfiguration.AZURE_STORAGE_ACCOUNT, BotConfiguration.AZURE_STORAGE_KEY);
				CloudTable table = tableClient.GetTableReference(tableName);
				TableOperation insert = TableOperation.Insert(data);
				table.ExecuteAsync(insert);
			}
			catch (Exception) { }
		}
	}
}