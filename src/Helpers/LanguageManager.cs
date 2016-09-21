using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Threading.Tasks;
using GX26Bot.Cognitive.TextAnalytics;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace GX26Bot.Helpers
{
	public class LanguageManager
	{
		public const string SPANISH = "spanish";
		public const string ENGLISH = "english";
		public const string PORTUGUESE = "portuguese";
		public const string UNKNOWN = "unknown";
		public const string DEFAULT_LANG = SPANISH;

		public static readonly string QUERY_LANGUAGE = "LANGUAGE";


		static Dictionary<string, CultureInfo> s_cultures = new Dictionary<string, CultureInfo>();
		static ResourceManager s_resmgr = new ResourceManager("GX26Bot.Resources.Messages", typeof(LanguageManager).Assembly);

		static Random s_random = new Random();

		string m_language = DEFAULT_LANG;

		public LanguageManager(string lang)
		{
			m_language = lang;
		}

		public string Language
		{
			get { return m_language; }
		}

		string GetMessage(string message)
		{
			string name = "es";
			switch (m_language)
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

		public string SearchCode
		{
			get { return m_language == ENGLISH ? "E": m_language == PORTUGUESE ? "P" : "S"; }
		}

		string GetMessage(string message, int max)
		{
			int val = s_random.Next(1, max);
			return GetMessage($"{message}{val}");
		}

		public static async Task<LanguageManager> GetLanguage(IDialogContext context, IAwaitable<IMessageActivity> activity)
		{
			string lang;
			if (!context.UserData.TryGetValue<string>(QUERY_LANGUAGE, out lang))
			{
				if (activity == null)
					return new LanguageManager(DEFAULT_LANG);

				IMessageActivity iMessage = await activity;
				lang = await DetectLanguage.Execute(iMessage.Text);
				context.UserData.SetValue<string>(QUERY_LANGUAGE, lang);
			}

			return new LanguageManager(lang);
		}

		public string Hello
		{
			get { return GetMessage("Hello", 2); }
		}

		public string HelloMultiLine
		{
			get { return GetMessage("HelloMultiLine"); }
		}

		public string NoSpeakersFound
		{
			get { return GetMessage("NoSpeakersFound", 3); }
		}

		public string TooManyspeakers
		{
			get { return GetMessage("TooManySpeakers"); }
		}

		public string NoSpeaker
		{
			get { return GetMessage("NoSpeaker"); }
		}

		public string NoSpeakersSession
		{
			get { return GetMessage("NoSpeakerSessions"); }
		}

		public string SpeakerSessionsFound
		{
			get { return GetMessage("SpeakerSessionFound"); }
		}

		public string SpeakerSessionsFound1
		{
			get { return GetMessage("SpeakerSessionFound1"); }
		}

		public string SessionSpeaker
		{
			get { return GetMessage("SessionSpeaker"); }
		}

		public string SessionTrack
		{
			get { return GetMessage("SessionTrack"); }
		}

		public string SessionCompany
		{
			get { return GetMessage("SessionCompany"); }
		}

		public string WhatFloor
		{
			get { return GetMessage("WhatFloor"); }
		}

		public string InvalidFloor
		{
			get { return GetMessage("InvalidFloor"); }
		}

		public string BathroomLocation
		{
			get { return GetMessage("BathroomLocation"); }
		}

		public string CoatCheck
		{
			get { return GetMessage("CoatCheck"); }
		}

		public string MisunderstoodRoom
		{
			get { return GetMessage("MisunderstoodRoom"); }
		}

		public string RoomMap
		{
			get { return GetMessage("RoomMap"); }
		}

		public string Map
		{
			get { return GetMessage("Map"); }
		}

		public string Genexus
		{
			get { return GetMessage("Genexus", 4); }
		}

		public string Harassment
		{
			get { return GetMessage("Harassment", 11); }
		}

		public string Deep
		{
			get { return GetMessage("Deep", 6); }
		}

		public string MyBad1
		{
			get { return GetMessage("MyBad1", 3); }
		}

		public string MyBad2
		{
			get { return GetMessage("MyBad2"); }
		}

		public string MyBad4
		{
			get { return GetMessage("MyBad4"); }
		}

		public string MyBad6
		{
			get { return GetMessage("MyBad6"); }
		}

		public string YourWelcome
		{
			get { return GetMessage("YourWelcome", 3); }
		}

		public string Break
		{
			get { return GetMessage("Break"); }
		}

		public string Snack
		{
			get { return GetMessage("Snack"); }
		}

		public string NoMoreBreaks
		{
			get { return GetMessage("NoMoreBreaks"); }
		}

		public string LangChange
		{
			get { return GetMessage("LangChange"); }
		}

		public string FrontDesk
		{
			get { return GetMessage("FrontDesk"); }
		}

		public string NoLocation
		{
			get { return GetMessage("NoLocation"); }
		}

		public string SearchFound
		{
			get { return GetMessage("SearchFound"); }
		}
	}

}