using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GX26Bot.Helpers;

namespace GX26Bot.Cognitive.Watson
{
	public class GetKeywords
	{
		static WebClient s_httpClient = new WebClient();

		public static async Task<KeywordObject> Execute(string text)
		{
			try
			{
				string url = $"https://gateway-a.watsonplatform.net/calls/text/TextGetRankedNamedEntities?apikey={BotConfiguration.ALCHEMY_API_KEY}&text={text}&outputMode=json";
				string response = await s_httpClient.UploadStringTaskAsync(url, "");

				KeywordObject body;
				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(response)))
					body = Utils.Deserialize<KeywordObject>(stream);

				return body;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}


	public class KeywordObject
	{
		public string status { get; set; }
		public string usage { get; set; }
		public string url { get; set; }
		public string totalTransactions { get; set; }
		public string language { get; set; }
		public Keyword[] keywords { get; set; }
	}

	public class Keyword
	{
		public string relevance { get; set; }
		public string text { get; set; }
	}

}