using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GX26Bot.Cognitive.LUIS;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Rest;

namespace GX26Bot.Controllers
{
	[BotAuthentication]
	public class MessagesController : ApiController
	{
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
					if (!string.IsNullOrEmpty(activity.Text))
						await Conversation.SendAsync(activity, MakeRoot);
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