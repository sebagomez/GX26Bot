using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GX26Bot.Helpers;

namespace GX26Bot.Cognitive.Watson
{
	public class GetEntities
	{
		static WebClient s_httpClient = new WebClient();

		public static async Task<Entities> Execute(string text)
		{
			try
			{
				string url = $"https://gateway-a.watsonplatform.net/calls/text/TextGetRankedNamedEntities?apikey={BotConfiguration.ALCHEMY_API_KEY}&text={text}&outputMode=json";
				string response = await s_httpClient.UploadStringTaskAsync(url, "");

				Entities body;
				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(response)))
					body = Utils.Deserialize<Entities>(stream);

				return body;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}


	public class Entities
	{
		public string status { get; set; }
		public string usage { get; set; }
		public string url { get; set; }
		public string language { get; set; }
		public Entity[] entities { get; set; }
	}

	public class Entity
	{
		public string type { get; set; }
		public string relevance { get; set; }
		public string count { get; set; }
		public string text { get; set; }
		public Disambiguated disambiguated { get; set; }
	}

	public class Disambiguated
	{
		public string[] subType { get; set; }
		public string name { get; set; }
		public string website { get; set; }
		public string dbpedia { get; set; }
		public string freebase { get; set; }
		public string opencyc { get; set; }
		public string yago { get; set; }
		public string crunchbase { get; set; }
	}

}