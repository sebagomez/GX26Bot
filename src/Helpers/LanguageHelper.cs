﻿using System;
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

			return  s_resmgr.GetString(message, s_cultures[name]);
		}

		//public static string GetRestroomMessage(string lang, int floor)
		//{
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return $"Los baños del piso {floor} están marcados en este mapa";
		//		case PORTUGUESE:
		//			return $"Os banheiros no piso {floor} están marcados en estos mapas";
		//		case ENGLISH:
		//		default:
		//			return $"Bathrooms on livel {floor} are marked on this map";
		//	}
		//}

		//public static string GetNotUnderstoodText(string lang)
		//{
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return "Lo lamento. No comprendo la pregunta :(";
		//		case PORTUGUESE:
		//			return "Meu error amigo, mais eu nao entendí";
		//		case ENGLISH:
		//		default:
		//			return "I'm sorry. I don't understand :(";
		//	}
		//}

		//public static string GetGreeting(string lang)
		//{
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return "Hola! Soy RUDI :)";
		//		case PORTUGUESE:
		//			return "Oi! eu so RUDI :)";
		//		case ENGLISH:
		//		default:
		//			return $"Hi! I'm RUDI :)";
		//	}
		//}

		//public static string GetClothesMessage(string lang)
		//{
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return "Ropería? si claro. Ubíquela en la siguiente imagen";
		//		case PORTUGUESE:
		//			return "Cómo se dice ropería en portuges?";
		//		case ENGLISH:
		//		default:
		//			return "Coat check? of course. Leave your shit with us";
		//	}
		//}

		//public static string GetLocationMessage(string lang)
		//{
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return "El 26to encuentro Genexus es en el hotel Radisson Victoria Plaza en Montevideo.";
		//		case PORTUGUESE:
		//			return "O 26to encontro Genexus es en el hotel Radisson Victoria Plaza en Montevideu.";
		//		case ENGLISH:
		//		default:
		//			return $"The 26th Genexus meeting is being held at the Radisson Victoria Plaza hotel in Montevideo";
		//	}
		//}

		//public static string GetRoomMessage(string lang, string room)
		//{
		//	int floor;
		//	ImageHelper.GetRoomImage(room, out floor);
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return $"{room} la puede ubicar en el piso {floor}";
		//		case PORTUGUESE:
		//			return $"A {room} esta no piso {floor}";
		//		case ENGLISH:
		//			return $"{room} is located on floor {floor}";
		//		default:
		//			return $"Sorry, I don't speak {lang}, but you'll find {room} on floor {floor}";
		//	}
		//}

		//public static string GetSpeakerQuestion(string lang)
		//{
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return "No entendí el nombre del orador. Por favor ingréselo nuevamente";
		//		case PORTUGUESE:
		//			return "Nao entendeu o nome. Por favor ingréselo nuevamente";
		//		case ENGLISH:
		//		default:
		//			return "I didn't get the name of the speaker. Please write it again.";
		//	}
		//}

		//public static string GetRoomQuestion(string lang)
		//{
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return "No entendí qué sala está buscando. Por favor dígame el nombre de la sala";
		//		case PORTUGUESE:
		//			return "Nao entendeu o nome. Por favor ingréselo nuevamente";
		//		case ENGLISH:
		//		default:
		//			return "I didn't get the name of the room. Please tell me the room you're looking for.";
		//	}
		//}

		//public static string GetFloorQuestion(string lang, int[] floors)
		//{
		//	return GetFloorQuestion(lang, floors, false);
		//}
		//public static string GetFloorQuestion(string lang, int[] floors, bool retry)
		//{
		//	string strFloors = string.Join(", ", floors);
		//	StringBuilder builder = new StringBuilder();
		//	if (!retry)
		//	{
		//		switch (lang)
		//		{
		//			case SPANISH:
		//				builder.AppendLine("En qué piso se encuentra?");
		//				break;
		//			case PORTUGUESE:
		//				builder.AppendLine("En qué piso esta vocé?");
		//				break;
		//			case ENGLISH:
		//			default:
		//				builder.AppendLine("What floor are you in?");
		//				break;
		//		}
		//	}
		//	else
		//	{
		//		switch (lang)
		//		{
		//			case SPANISH:
		//				builder.AppendLine("Piso inválido. Por favor reintente.");
		//				break;
		//			case PORTUGUESE:
		//				builder.AppendLine("Piso no válido.");
		//				break;
		//			case ENGLISH:
		//			default:
		//				builder.AppendLine("Invalid floor. Please try again.");
		//				break;
		//		}
		//	}

		//	builder.AppendLine(strFloors);
		//	return builder.ToString();
		//}


		//public static string GetNoSpeakersFound(string lang)
		//{
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return "Lo siento, no encontré oradores llamados '{0}'";
		//		case PORTUGUESE:
		//			return "Nao encontreu oradores llamados '{0}'";
		//		case ENGLISH:
		//		default:
		//			return "I'm sorry, I couldn't find speakers named '{0}'";
		//	}
		//}

		//public static string GetManySpeakersFound(string lang)
		//{
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return "Encontré {0} oradores llamados '{1}'. Cuál de ellos es el que te interesa?";
		//		case PORTUGUESE:
		//			return "Encontré {0} oradores llamados '{1}'. Cuál de ellos es el que te interesa?";
		//		case ENGLISH:
		//		default:
		//			return "I've found {0} speakers called '{1}'. Which one of them is the one you want?";
		//	}
		//}

		//public static string GetNoSessionsFound(string lang)
		//{
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return "Lo siento. No encontré sesiones para {0}.";
		//		case PORTUGUESE:
		//			return "Lo siento. No encontré sesiones para {0}";
		//		case ENGLISH:
		//		default:
		//			return "I'm sorry, I couldn't find session for {0}";
		//	}
		//}

		//public static string GetSessionsFound(string lang, bool many)
		//{
		//	switch (lang)
		//	{
		//		case SPANISH:
		//			return "Encontré {0} " + (many ? "sesiones" : "sesion") + " con {1} como participante.";
		//		case PORTUGUESE:
		//			return "Encontrée {0} " + (many ? "sessoes" : "sesson") +  " com {1} as a speaker.";
		//		case ENGLISH:
		//		default:
		//			return "I've found {0} " + (many ? "sessions" : "session") + " with {1} as a speaker.";
		//	}
		//}

	}
	
}