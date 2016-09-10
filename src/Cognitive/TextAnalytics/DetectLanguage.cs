using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GX26Bot.Helpers;

namespace GX26Bot.Cognitive.TextAnalytics
{
	public class DetectLanguage
	{
		static string[] s_allowedLanguages = new string[] { LanguageManager.SPANISH, LanguageManager.ENGLISH, LanguageManager.PORTUGUESE };
		static WebClient s_httpClient = new WebClient();

		public static async Task<string> Execute(string text)
		{
			try
			{
				string url = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/languages";
				string data = "{\"documents\": [ {\"id\": \"" + new Guid().ToString() + "\",\"text\": \"" + text + "\"	} ]}";
				byte[] payload = Encoding.ASCII.GetBytes(data);

				s_httpClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
				s_httpClient.Headers.Add("Ocp-Apim-Subscription-Key", BotConfiguration.TEXTANALYTICS_KEY);
				byte[] response = await s_httpClient.UploadDataTaskAsync(url, payload);

				ResponseBody body = null;
				using (var stream = new MemoryStream(response))
					body = Utils.Deserialize<ResponseBody>(stream);

				string returnedLanguage = body.documents[0].detectedLanguages[0].name.ToLower();
				if (s_allowedLanguages.Contains(returnedLanguage))
					return returnedLanguage;
				return LanguageManager.DEFAULT_LANG;
			}
			catch (Exception)
			{
				return LanguageManager.DEFAULT_LANG;
			}
		}
	}


	public class ResponseBody
	{
		public Document[] documents { get; set; }
		public object[] errors { get; set; }
	}

	public class Document
	{
		public string id { get; set; }
		public Detectedlanguage[] detectedLanguages { get; set; }
	}

	public class Detectedlanguage
	{
		public string name { get; set; }
		public string iso6391Name { get; set; }
		public decimal score { get; set; }
	}

}