﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using GX26Bot.GX26.Data;

namespace GX26Bot.GX26
{
	public class FindSpeaker
	{
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

			WebClient wc = new WebClient();
			Stream response = wc.OpenRead(new Uri(url));

			DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(GX26Speaker));
			GX26Speaker sdt = ser.ReadObject(response) as GX26Speaker;

			return sdt.Speakers.ToList();

		}
	}
}