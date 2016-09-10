using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using GX26Bot.GX26.Data;
using GX26Bot.Helpers;

namespace GX26Bot.GX26
{
	public class SpeakerSessions
	{
		static WebClient s_httpClient = new WebClient();

		public static List<Session> Find(int speakerId, string lang)
		{
			string language = "E"; //English will be default
			if (lang == LanguageManager.SPANISH)
				language = "S";
			else if (lang == LanguageManager.PORTUGUESE)
				language = "P";

			string query = $"speakerid={speakerId}&sessionLang={language}";
			string url = $"{BotConfiguration.GX26_SERVICES}/rest/speakersessions?{query}";

			Stream response = s_httpClient.OpenRead(new Uri(url));

			GX26Session sdt = Utils.Deserialize<GX26Session>(response);

			return sdt.Sessions.ToList();
		}
	}
}