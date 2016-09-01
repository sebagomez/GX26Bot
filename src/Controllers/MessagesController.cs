using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GX26Bot.Cognitive.LUIS;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

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
			ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
			if (activity.GetActivityType() == ActivityTypes.Message)
				await Conversation.SendAsync(activity, MakeRoot);
			else
				await connector.Conversations.ReplyToActivityAsync(HandleSystemMessage(activity));

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		internal static IDialog<GX26Manager> MakeRoot()
		{
			return Chain.From(() => new LUISManager());
		}

		private Activity HandleSystemMessage(Activity activity)
		{
			switch (activity.GetActivityType())
			{
				case ActivityTypes.Ping:
					Activity reply = activity.CreateReply();
					reply.Type = ActivityTypes.Ping;
					reply.Text = "Haga Pum!";
					return reply;
				case ActivityTypes.ContactRelationUpdate:
				case ActivityTypes.ConversationUpdate:
				case ActivityTypes.DeleteUserData:
				case ActivityTypes.Typing:
				default:
					break;
			}

			return null;
		}
	}
}