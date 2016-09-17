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
			try
			{
				ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
				if (activity.GetActivityType() == ActivityTypes.Message)
				{
					//resolver el cambio de idioma acá!
					if (!string.IsNullOrEmpty(activity.Text) && !m_doNotProcess.Contains(activity.Text.Trim().ToLower()))
					{
						LanguageManager lang;
						if (IsItLanguageCommand(activity.Text, out lang))
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
			catch (HttpOperationException hoe)
			{
				Trace.TraceError(hoe.Message);
			}

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		bool IsItLanguageCommand(string text, out LanguageManager lang)
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
					reply.Text = HelpMessage.GetHelp("", activity.From.Name);
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