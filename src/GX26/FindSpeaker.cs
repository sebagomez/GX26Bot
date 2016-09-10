using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using GX26Bot.GX26.Data;
using GX26Bot.Helpers;

namespace GX26Bot.GX26
{
	public class FindSpeaker
	{
		static WebClient s_httpClient = new WebClient();

		public static List<Speaker> Find(string pattern)
		{
			if (string.IsNullOrEmpty(pattern))
				return null;

			pattern = pattern.Trim();

			string[] names = pattern.Split(' ');

			string query = "";
			if (names.Length == 2)
				query = $"firstName={names[0]}&lastName={names[1]}";
			else
				query = $"fullName={names[0]}";
			
			string url = $"{BotConfiguration.GX26_SERVICES}/rest/findspeaker?{query}";

			Stream response = s_httpClient.OpenRead(new Uri(url));

			GX26Speaker sdt = Utils.Deserialize<GX26Speaker>(response);

			return sdt.Speakers.ToList();

		}
	}
}