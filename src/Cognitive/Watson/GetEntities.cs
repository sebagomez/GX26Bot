using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace GX26Bot.Cognitive.Watson
{
	public class GetEntities
	{
		static string ALCHEMY_API_KEY { get; } = ConfigurationManager.AppSettings["AlchemyApiKey"];

		public static async Task<Entities> Execute(string text)
		{
			try
			{
				string url = $"https://gateway-a.watsonplatform.net/calls/text/TextGetRankedNamedEntities?apikey={ALCHEMY_API_KEY}&text={text}&outputMode=json";
				string response;
				using (WebClient http = new WebClient())
					response = await http.UploadStringTaskAsync(url, "");

				Entities body;
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Entities));
				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(response)))
					body = (Entities)serializer.ReadObject(stream);

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