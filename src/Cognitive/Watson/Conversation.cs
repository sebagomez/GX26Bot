using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using GX26Bot.Helpers;

namespace GX26Bot.Cognitive.Watson
{
	public class Conversation
	{
		public static async Task<ConversationObject> SendMessage(string text)
		{
			try
			{
				string url = $"https://gateway.watsonplatform.net/conversation/api/v1/workspaces/{BotConfiguration.CONVERSATION_WORKSPACE}/message?version=2016-07-11";
				InputObject reqData = new InputObject();
				reqData.input = new Input { text = text };

				string response;
				using (WebClient http = new WebClient())
				{
					http.Encoding = Encoding.UTF8;
					http.Credentials = new NetworkCredential(BotConfiguration.CONVERSATION_USERNAME, BotConfiguration.CONVERSATION_PASSWORD);
					http.Headers.Add(HttpRequestHeader.ContentType, "application/json");
					response = await http.UploadStringTaskAsync(url, Utils.ToJson(reqData));
				}
				ConversationObject body;
				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(response)))
					body = Utils.Deserialize<ConversationObject>(stream);
				
				return body;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}

	public class ConversationObject
	{
		public Input input { get; set; }
		public bool alternate_intents { get; set; }
		public Context context { get; set; }
		public ConversationEntity[] entities { get; set; }
		public Intent[] intents { get; set; }
		public Output output { get; set; }
	}

	public class InputObject
	{
		public Input input { get; set; }
	}

	public class Input
	{
		public string text { get; set; }
	}

	public class Context
	{
		public string conversation_id { get; set; }
		public System system { get; set; }
		public int defaultCounter { get; set; }
	}

	public class System
	{
		public string[] dialog_stack { get; set; }
		public int dialog_turn_counter { get; set; }
		public int dialog_request_counter { get; set; }
	}

	public class Output
	{
		public string[] text { get; set; }
		public string[] nodes_visited { get; set; }
	}

	public class ConversationEntity
	{
		public string entity { get; set; }
		public int[] location { get; set; }
		public string value { get; set; }
	}

	public class Intent
	{
		public string intent { get; set; }
		public float confidence { get; set; }
	}

}