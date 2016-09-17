using System;
using System.IO;
using System.Net;
using GX26Bot.GX26.Data;
using GX26Bot.Helpers;

namespace GX26Bot.GX26
{
	public class FindSessions
	{
		static WebClient s_httpClient = new WebClient();

		public static GX26Session Find(string name, string lang)
		{
			string language = "S"; //Spanish will be default
			if (lang == LanguageManager.ENGLISH)
				language = "E";
			else if (lang == LanguageManager.PORTUGUESE)
				language = "P";

			name = name.Trim();

			string[] names = name.Split(' ');

			string query = "";
			if (names.Length == 2)
				query = $"firstName={names[0]}&lastName={names[1]}";
			else
				query = $"pattern={names[0]}";

			query += $"&sessionLang={language}";
			string url = $"{BotConfiguration.GX26_SERVICES}/rest/findsessions?{query}";

			s_httpClient.Headers.Remove("GeneXus-Language");
			s_httpClient.Headers.Add("GeneXus-Language", $"{lang.Substring(0, 1).ToUpper()}{lang.Substring(1)}");
			Stream response = s_httpClient.OpenRead(new Uri(url));

			return Utils.Deserialize<GX26Session>(response);
		}
	}
}