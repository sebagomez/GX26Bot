using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GX26Bot.Helpers;

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

		static WebClient s_httpClient = new WebClient();

		public static async Task<Sentiment> Execute(string text)
		{
			try
			{
				string url = $"https://gateway-a.watsonplatform.net/calls/text/TextGetTextSentiment?apikey={BotConfiguration.ALCHEMY_API_KEY}&text={text}&outputMode=json";

				string response = await s_httpClient.UploadStringTaskAsync(url, "");
				SentimentObject body;
				using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(response)))
					body = Utils.Deserialize<SentimentObject>(stream);

				double score = 0;
				double min = -0.5d;
				if (!string.IsNullOrEmpty(body.docSentiment.score))
					score = double.Parse(body.docSentiment.score);

				Sentiment sent = (Sentiment)Enum.Parse(typeof(Sentiment), body.docSentiment.type);
				if (sent == Sentiment.negative && score > min)
					return Sentiment.neutral;

				return sent;
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