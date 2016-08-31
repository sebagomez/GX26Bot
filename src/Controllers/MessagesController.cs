using System;
using System.Linq;
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
		string[] m_help = new string[] { "help", "hola", "hello", "oi" };
		/// <summary>
		/// POST: api/Messages
		/// Receive a message from a user and reply to it
		/// </summary>
		public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
		{
			ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
			if (activity.GetActivityType() == ActivityTypes.Message)
			{
				if (m_help.Contains(activity.Text.Trim().ToLower()))
				{
					await connector.Conversations.ReplyToActivityAsync(activity.CreateReply(HelpMessage.GetHelp()));
				}
				else
				{

					//if (message.Text.Trim().ToLower().StartsWith("muuu"))
					//{
					//	Message msg = message.CreateReplyMessage($"{message.Text} to you too");
					//	msg.Attachments = new List<Attachment>();
					//	msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = "http://www.sustained.ie/wp-content/uploads/2013/01/Close-up+of+Cow.jpg" });

					//	return msg;
					//}

					//if (message.Text.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length == 1)
					//await connector.Conversations.ReplyToActivityAsync(activity.CreateReply("aslgo mas complicado"));

					await Conversation.SendAsync(activity, MakeRoot);
				}
			}
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