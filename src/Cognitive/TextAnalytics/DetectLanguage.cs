using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using GX26Bot.Helpers;
using Microsoft.IdentityModel.Protocols;

namespace GX26Bot.Cognitive.TextAnalytics
{
	public class DetectLanguage
	{

		static string[] s_allowedLanguages = new string[] { LanguageHelper.SPANISH, LanguageHelper.ENGLISH, LanguageHelper.PORTUGUESE }; 

		public static async Task<string> Execute(string text)
		{
			try
			{
				string url = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/languages";
				string data = "{\"documents\": [ {\"id\": \"" + new Guid().ToString() + "\",\"text\": \"" + text + "\"	} ]}";
				byte[] payload = Encoding.ASCII.GetBytes(data);
				byte[] response;
				using (WebClient http = new WebClient())
				{
					http.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					http.Headers.Add("Ocp-Apim-Subscription-Key", BotConfiguration.TEXTANALYTICS_KEY);
					response = await http.UploadDataTaskAsync(url, payload);
				}

				ResponseBody body = null;
				using (var stream = new MemoryStream(response))
					body = Utils.Deserialize<ResponseBody>(stream);

				string returnedLanguage = body.documents[0].detectedLanguages[0].name.ToLower();
				if (s_allowedLanguages.Contains(returnedLanguage))
					return returnedLanguage;
				return LanguageHelper.DEFAULT_LANG;
			}
			catch (Exception)
			{
				return LanguageHelper.DEFAULT_LANG;
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