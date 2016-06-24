using System;
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

		public static async Task<string> GetRestroomMessage(string text)
		{
			string lang = await GetLanguage(text);
			switch (lang)
			{
				case "unknown":
				case "spanish":
					return "Baños? si claro. Los baños están marcados en este mapa";
				case "portuguese":
					return "Banheiro? claro cara. Oss banheiros están marcados en estos mapas";
				case "english":
					return "Bathrooms? of course. Bathrooms are marked on this map";
				default:
					return $"Sorry, I don't speak {lang}, but you'll find the bathrooms marked in the image";
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