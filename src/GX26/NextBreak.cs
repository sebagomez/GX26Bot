using System;
using System.IO;
using System.Net;
using GX26Bot.Helpers;

namespace GX26Bot.GX26
{
	public class NextBreak
	{
		static WebClient s_httpClient = new WebClient();

		public static Break Find()
		{
			string url = $"{BotConfiguration.GX26_SERVICES}/rest/nextbreak";

			Stream response = s_httpClient.OpenRead(new Uri(url));

			Break[] breaks = Utils.Deserialize<Break[]>(response);

			if (breaks.Length > 0)
				return breaks[0];
			else
				return null;
		}
	}

	public class Break
	{
		public string Sessiondate { get; set; }
		public string Sessionstarttime { get; set; }
		public string Sessionstarttimetext { get; set; }
		public string Sessionendtime { get; set; }
		public string Sessionendtimetext { get; set; }
		public int Sessiondur { get; set; }
	}
}