using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using GX26Bot.GX26.Data;
using GX26Bot.Helpers;

namespace GX26Bot.GX26
{
	public class SpeakerSessions
	{
		public static List<Session> Find(int speakerId, string lang)
		{
			string language = "E"; //English will be default
			if (lang == LanguageHelper.SPANISH)
				language = "S";
			else if (lang == LanguageHelper.PORTUGUESE)
				language = "P";

			string query = $"speakerid={speakerId}&sessionLang={language}";
			string url = $"{BotConfiguration.GX26_SERVICES}/rest/speakersessions?{query}";

			WebClient wc = new WebClient();
			Stream response = wc.OpenRead(new Uri(url));

			DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(GX26Session));
			GX26Session sdt = ser.ReadObject(response) as GX26Session;

			return sdt.Sessions.ToList();
		}
	}
}