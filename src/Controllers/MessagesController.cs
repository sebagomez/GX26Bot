using System;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GX26Bot.Cognitive.LUIS;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Rest;
using GX26Bot.Helpers;
using System.Collections.Generic;

namespace GX26Bot.Controllers
{
	[BotAuthentication]
	public class MessagesController : ApiController
	{
		string[] m_doNotProcess = new string[] { "si", "no", "ok", "yes", "sim", "nao" };
		/// <summary>
		/// POST: api/Messages
		/// Receive a message from a user and reply to it
		/// </summary>
		public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
		{
			ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
			try
			{
				if (activity.GetActivityType() == ActivityTypes.Message)
				{
					if (!string.IsNullOrEmpty(activity.Text) && !m_doNotProcess.Contains(activity.Text.Trim().ToLower()))
					{
						LanguageManager lang;
						if (IsLanguageCommand(activity.Text, out lang))
						{
							StateClient state = activity.GetStateClient();
							BotData data = state.BotState.GetUserData(activity.ChannelId, activity.From.Id);
							data.SetProperty<string>(LanguageManager.QUERY_LANGUAGE, lang.Language);
							state.BotState.SetUserData(activity.ChannelId, activity.From.Id, data);
							await connector.Conversations.ReplyToActivityAsync(activity.CreateReply(lang.LangChange));
						}
						else
							await Conversation.SendAsync(activity, MakeRoot);
					}
					else // is it an emoji or attachment?
						await connector.Conversations.ReplyToActivityAsync(activity.CreateReply(":)"));
				}
				else
					await connector.Conversations.ReplyToActivityAsync(HandleSystemMessage(activity));
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);

				Activity msg = activity.CreateReply("BOT ERROR! Nooooooooooooooo");
				msg.Attachments = new List<Attachment>();
				msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetVader() });
				await connector.Conversations.ReplyToActivityAsync(msg);
			}

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		bool IsLanguageCommand(string text, out LanguageManager lang)
		{
			lang = null;
			string command = text.Trim().ToLower();
			if (command.StartsWith("/"))
			{
				command = command.Replace("/", "");
				switch (command)
				{
					case "english":
					case "ingles":
					case "inglés":
						lang = new LanguageManager(LanguageManager.ENGLISH);
						break;
					case "espanol":
					case "español":
					case "spanish":
						lang = new LanguageManager(LanguageManager.SPANISH);
						break;
					case "brasileiro":
					case "portuguese":
					case "português":
					case "portugues":
						lang = new LanguageManager(LanguageManager.PORTUGUESE);
						break;
					default:
						break;
				}
			}

			return lang != null;
		}

		internal static IDialog<object> MakeRoot()
		{
			return Chain.From(() => new LUISManager());
		}

		private Activity HandleSystemMessage(Activity activity)
		{
			Activity reply = null;
			switch (activity.GetActivityType())
			{
				case ActivityTypes.Ping:
					reply = activity.CreateReply();
					reply.Type = ActivityTypes.Ping;
					reply.Text = "Haga Pum!";
					break;
				case ActivityTypes.ConversationUpdate:
					reply = activity.CreateReply();
					reply.Type = ActivityTypes.Message;
					LanguageManager langMgr = new LanguageManager(LanguageManager.DEFAULT_LANG);
					reply.Text = string.Format(langMgr.Hello, activity.From.Name);
					break;
				case ActivityTypes.ContactRelationUpdate:
				case ActivityTypes.DeleteUserData:
				case ActivityTypes.Typing:
				default:
					break;
			}

			return reply;
		}
	}
}