using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using GX26Bot.Congnitive.Watson;
using GX26Bot.GX26.Data;

namespace GX26Bot.GX26
{
	public class SpeakerSessions
	{
		public static List<Session> Find(int speakerId, TextLanguage lang)
		{
			string language = "E"; //English will be default
			if (lang == TextLanguage.spanish)
				language = "S";
			else if (lang == TextLanguage.portuguese)
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