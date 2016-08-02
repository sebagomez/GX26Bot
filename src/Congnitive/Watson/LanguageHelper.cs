using System;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using GX26Bot.Images;

namespace GX26Bot.Congnitive.Watson
{
	public enum TextLanguage
	{
		spanish,
		english,
		portuguese,
		unknown
	}

	public class LanguageHelper
	{
		static string WATSON_API_KEY { get; } = ConfigurationManager.AppSettings["WatsonApiKey"];

		static string DEFAULT_LANG = "spanish";

		static async Task<string> GetLanguage(string text)
		{
			try
			{
				string url = $"https://watson-api-explorer.mybluemix.net/alchemy-api/calls/text/TextGetLanguage?text={text}&apikey={WATSON_API_KEY}&outputMode=json";
				HttpClient http = new HttpClient();
				string stringData = await http.GetStringAsync(url);

				Language lang = null;
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Language));
				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(stringData)))
					lang = (Language)serializer.ReadObject(stream);

				return lang.language;
			}
			catch (Exception ex)
			{
				return DEFAULT_LANG;
			}
		}

		public static async Task<TextLanguage> GetTextLanguage(string text)
		{
			string lang = await GetLanguage(text);

			TextLanguage language;
			if (Enum.TryParse<TextLanguage>(lang, out language))
				return language;

			return TextLanguage.unknown;

		}

		public static string GetRestroomMessage(TextLanguage lang, int floor)
		{
			switch (lang)
			{
				case TextLanguage.spanish:
					return $"Los baños del piso {floor} están marcados en este mapa";
				case TextLanguage.portuguese:
					return $"Os banheiros no piso {floor} están marcados en estos mapas";
				case TextLanguage.english:
				case TextLanguage.unknown:
				default:
					return $"Bathrooms on livel {floor} are marked on this map";
			}
		}

		public static async Task<string> GetNotUnderstoodText(string text)
		{
			string lang = await GetLanguage(text);
			switch (lang)
			{
				case "unknown":
				case "spanish":
					return "Lo lamento. No comprendo la pregunta :(";
				case "portuguese":
					return "Meu error amigo, mais eu nao entendí";
				case "english":
					return "I'm sory. I don't understand :(";
				default:
					return $"Sorry, I don't speak {lang}, I also couldn't understand your question :(";
			}
		}

		public static async Task<string> GetClothesMessage(string text)
		{
			string lang = await GetLanguage(text);
			switch (lang)
			{
				case "unknown":
				case "spanish":
					return "Ropería? si claro. Ubíquela en la siguiente imagen";
				case "portuguese":
					return "Cómo se dice ropería en portuges?";
				case "english":
					return "Coat check? of course. Leave your shit with us";
				default:
					return $"Sorry, I don't speak {lang}, but you'll find where to put your stuff away in the following image";
			}
		}

		public static async Task<string> GetRoomMessage(string text, string room)
		{
			string lang = await GetLanguage(text.Replace(room, ""));
			int floor;
			ImageHelper.GetRoomImage(room, out floor);
			switch (lang)
			{
				case "unknown":
				case "spanish":
					return $"{room} la puede ubicar en el piso {floor}";
				case "portuguese":
					return $"A {room} esta no piso {floor}";
				case "english":
					return $"{room} is located on floor {floor}";
				default:
					return $"Sorry, I don't speak {lang}, but you'll find {room} on floor {floor}";
			}
		}

		public static string GetFloorQuestion(TextLanguage lang, int[] floors)
		{
			return GetFloorQuestion(lang, floors, false);
		}
		public static string GetFloorQuestion(TextLanguage lang, int[] floors, bool retry)
		{
			string strFloors = string.Join(", ", floors);
			StringBuilder builder = new StringBuilder();
			if (!retry)
			{
				switch (lang)
				{
					case TextLanguage.spanish:
						builder.AppendLine("En qué piso se encuentra?");
						break;
					case TextLanguage.portuguese:
						builder.AppendLine("En qué piso esta vocé?");
						break;
					case TextLanguage.unknown:
					case TextLanguage.english:
					default:
						builder.AppendLine("What floor are you in?");
						break;
				}
			}
			else
			{
				switch (lang)
				{
					case TextLanguage.spanish:
						builder.AppendLine("Piso inválido. Por favor reintente.");
						break;
					case TextLanguage.portuguese:
						builder.AppendLine("Piso no válido.");
						break;
					case TextLanguage.unknown:
					case TextLanguage.english:
					default:
						builder.AppendLine("Invalid floor. Please try again.");
						break;
				}
			}

			builder.AppendLine(strFloors);
			return builder.ToString();
		}
	}



	[DataContract]
	public class Language
	{
		[DataMember]
		public string status { get; set; }
		[DataMember]
		public string usage { get; set; }
		[DataMember]
		public string url { get; set; }
		[DataMember]
		public string language { get; set; }
		[DataMember]
		public string iso6391 { get; set; }
		[DataMember]
		public string iso6392 { get; set; }
		[DataMember]
		public string iso6393 { get; set; }
		[DataMember]
		public string ethnologue { get; set; }
		[DataMember]
		public string nativespeakers { get; set; }
		[DataMember]
		public string wikipedia { get; set; }
	}
}