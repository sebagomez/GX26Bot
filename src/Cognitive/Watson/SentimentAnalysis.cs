using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace GX26Bot.Cognitive.Watson
{
	public class SentimentAnalysis
	{
		public enum Sentiment
		{
			positive,
			neutral,
			negative
		}

		public static async Task<Sentiment> Execute(string text)
		{
			try
			{
				string url = $"https://gateway-a.watsonplatform.net/calls/text/TextGetTextSentiment?apikey={BotConfiguration.ALCHEMY_API_KEY}&text={text}&outputMode=json";
				string response;
				using (WebClient http = new WebClient())
					response = await http.UploadStringTaskAsync(url, "");

				SentimentObject body;
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SentimentObject));
				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(response)))
					body = (SentimentObject)serializer.ReadObject(stream);

				decimal score;
				if (!string.IsNullOrEmpty(body.docSentiment.score))
					score = decimal.Parse(body.docSentiment.score);

				return (Sentiment)Enum.Parse(typeof(Sentiment), body.docSentiment.type);
			}
			catch (Exception)
			{
				return Sentiment.neutral;
			}
		}
	}


	public class SentimentObject
	{
		public string status { get; set; }
		public string usage { get; set; }
		public string url { get; set; }
		public string totalTransactions { get; set; }
		public string language { get; set; }
		public Docsentiment docSentiment { get; set; }
	}

	public class Docsentiment
	{
		public string mixed { get; set; }
		public string score { get; set; }
		public string type { get; set; }
	}

}