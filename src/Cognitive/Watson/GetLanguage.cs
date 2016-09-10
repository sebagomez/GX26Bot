using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using GX26Bot.Helpers;

namespace GX26Bot.Cognitive.Watson
{
	public class GetLanguage
	{

		public static async Task<string> Execute(string text)
		{
			try
			{
				string url = $"https://watson-api-explorer.mybluemix.net/alchemy-api/calls/text/TextGetLanguage?text={text}&apikey={BotConfiguration.WATSON_API_KEY}&outputMode=json";
				HttpClient http = new HttpClient();
				string stringData = await http.GetStringAsync(url);

				Language lang;
				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(stringData)))
					lang = Utils.Deserialize<Language>(stream);

				if (lang.language == LanguageManager.UNKNOWN)
					return LanguageManager.DEFAULT_LANG;

				return lang.language;
			}
			catch (Exception)
			{
				return LanguageManager.DEFAULT_LANG;
			}
		}
	}

	public class Language
	{
		public string status { get; set; }
		public string usage { get; set; }
		public string url { get; set; }
		public string language { get; set; }
		public string iso6391 { get; set; }
		public string iso6392 { get; set; }
		public string iso6393 { get; set; }
		public string ethnologue { get; set; }
		public string nativespeakers { get; set; }
		public string wikipedia { get; set; }
	}
}