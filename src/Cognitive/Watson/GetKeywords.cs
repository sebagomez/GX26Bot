using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace GX26Bot.Cognitive.Watson
{
	public class GetKeywords
	{
		public static async Task<KeywordObject> Execute(string text)
		{
			try
			{
				string url = $"https://gateway-a.watsonplatform.net/calls/text/TextGetRankedNamedEntities?apikey={BotConfiguration.ALCHEMY_API_KEY}&text={text}&outputMode=json";
				string response;
				using (WebClient http = new WebClient())
					response = await http.UploadStringTaskAsync(url, "");

				KeywordObject body;
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(KeywordObject));
				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(response)))
					body = (KeywordObject)serializer.ReadObject(stream);

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