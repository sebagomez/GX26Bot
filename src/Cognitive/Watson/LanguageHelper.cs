using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using GX26Bot.Images;

namespace GX26Bot.Cognitive.Watson
{
	public class LanguageHelper
	{
		static string WATSON_API_KEY { get; } = ConfigurationManager.AppSettings["WatsonApiKey"];

		public const string SPANISH = "spanish";
		public const string ENGLISH = "english";
		public const string PORTUGUESE = "portuguese";

		const string DEFAULT_LANG = ENGLISH;

		public static async Task<string> GetLanguage(string text)
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
			catch (Exception)
			{
				return DEFAULT_LANG;
			}
		}

		public static string GetRestroomMessage(string lang, int floor)
		{
			switch (lang)
			{
				case SPANISH:
					return $"Los baños del piso {floor} están marcados en este mapa";
				case PORTUGUESE:
					return $"Os banheiros no piso {floor} están marcados en estos mapas";
				case ENGLISH:
				default:
					return $"Bathrooms on livel {floor} are marked on this map";
			}
		}

		public static string GetNotUnderstoodText(string lang)
		{
			switch (lang)
			{
				case SPANISH:
					return "Lo lamento. No comprendo la pregunta :(";
				case PORTUGUESE:
					return "Meu error amigo, mais eu nao entendí";
				case ENGLISH:
					return "I'm sory. I don't understand :(";
				default:
					return $"Sorry, I don't speak {lang}, I also couldn't understand your question :(";
			}
		}

		public static string GetClothesMessage(string lang)
		{
			switch (lang)
			{
				case SPANISH:
					return "Ropería? si claro. Ubíquela en la siguiente imagen";
				case PORTUGUESE:
					return "Cómo se dice ropería en portuges?";
				case ENGLISH:
					return "Coat check? of course. Leave your shit with us";
				default:
					return $"Sorry, I don't speak {lang}, but you'll find where to put your stuff away in the following image";
			}
		}

		public static string GetRoomMessage(string lang, string room)
		{
			int floor;
			ImageHelper.GetRoomImage(room, out floor);
			switch (lang)
			{
				case SPANISH:
					return $"{room} la puede ubicar en el piso {floor}";
				case PORTUGUESE:
					return $"A {room} esta no piso {floor}";
				case ENGLISH:
					return $"{room} is located on floor {floor}";
				default:
					return $"Sorry, I don't speak {lang}, but you'll find {room} on floor {floor}";
			}
		}

		public static string GetSpeakerQuestion(string lang)
		{
			switch (lang)
			{
				case SPANISH:
					return "No entendí el nombre del orador. Por favor ingréselo nuevamente";
				case PORTUGUESE:
					return "Nao entendeu o nome. Por favor ingréselo nuevamente";
				case ENGLISH:
				default:
					return "I didn't get the name of the speaker. Please write it again.";
			}
		}

		public static string GetRoomQuestion(string lang)
		{
			switch (lang)
			{
				case SPANISH:
					return "No entendí qué sala está buscando. Por favor dígame el nombre de la sala";
				case PORTUGUESE:
					return "Nao entendeu o nome. Por favor ingréselo nuevamente";
				case ENGLISH:
				default:
					return "I didn't get the name of the room. Please tell me the room you're looking for.";
			}
		}

		public static string GetFloorQuestion(string lang, int[] floors)
		{
			return GetFloorQuestion(lang, floors, false);
		}
		public static string GetFloorQuestion(string lang, int[] floors, bool retry)
		{
			string strFloors = string.Join(", ", floors);
			StringBuilder builder = new StringBuilder();
			if (!retry)
			{
				switch (lang)
				{
					case SPANISH:
						builder.AppendLine("En qué piso se encuentra?");
						break;
					case PORTUGUESE:
						builder.AppendLine("En qué piso esta vocé?");
						break;
					case ENGLISH:
					default:
						builder.AppendLine("What floor are you in?");
						break;
				}
			}
			else
			{
				switch (lang)
				{
					case SPANISH:
						builder.AppendLine("Piso inválido. Por favor reintente.");
						break;
					case PORTUGUESE:
						builder.AppendLine("Piso no válido.");
						break;
					case ENGLISH:
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