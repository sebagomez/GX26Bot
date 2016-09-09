using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Resources;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using GX26Bot.Cognitive.TextAnalytics;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace GX26Bot.Helpers
{
	public class LanguageHelper
	{

		public const string SPANISH = "spanish";
		public const string ENGLISH = "english";
		public const string PORTUGUESE = "portuguese";
		public const string UNKNOWN = "unknown";
		public const string DEFAULT_LANG = SPANISH;

		public const string Hello = "Hello";
		public const string NoSpeakersFound = "NoSpeakersFound";
		public const string TooManySpeakers = "TooManySpeakers";
		public const string NoSpeaker = "NoSpeaker";
		public const string NoSpeakerSessions = "NoSpeakerSessions";
		public const string SpeakerSessionFound = "SpeakerSessionFound";
		public const string WhatFloor = "WhatFloor";
		public const string WhatFloor2 = "WhatFloor2";
		public const string BathroomLocation = "BathroomLocation";
		public const string CoatCheck = "CoatCheck";
		public const string MisunderstoodRoom = "MisunderstoodRoom";
		public const string RoomMap = "RoomMap";
		public const string Map = "Map";
		public const string Genexus = "Genexus";
		public const string Harassment = "Harassment";
		public const string Deep = "Deep";
		public const string MyBad = "MyBad";
		public const string MyBad2 = "MyBad2";
		public const string MyBad4 = "MyBad4";
		public const string MyBad6 = "MyBad6";
		public const string Negative = "Negative";
		public const string YourWelcome = "YourWelcome";


		static readonly string QUERY_LANGUAGE = "LANGUAGE";


		static Dictionary<string, CultureInfo> s_cultures = new Dictionary<string, CultureInfo>();
		static ResourceManager s_resmgr = new ResourceManager("GX26Bot.Resources.Messages", typeof(LanguageHelper).Assembly);

		public static string GetMessage(string message, string lang)
		{
			string name = "es";
			switch (lang)
			{
				case ENGLISH:
					name = "en";
					break;
				case PORTUGUESE:
					name = "pt";
					break;
				default:
					break;
			}

			if (!s_cultures.ContainsKey(name))
			{
				CultureInfo culture = CultureInfo.CreateSpecificCulture(name);
				s_cultures[name] = culture;
			}

			return s_resmgr.GetString(message, s_cultures[name]);
		}

		public static async Task<string> GetLanguage(IDialogContext context, IAwaitable<IMessageActivity> activity)
		{
			string lang;
			if (!context.UserData.TryGetValue<string>(QUERY_LANGUAGE, out lang))
			{
				if (activity == null)
					return DEFAULT_LANG;

				IMessageActivity iMessage = await activity;
				lang = await DetectLanguage.Execute(iMessage.Text);
				context.UserData.SetValue<string>(QUERY_LANGUAGE, lang);
			}

			return lang;
		}

	}

}