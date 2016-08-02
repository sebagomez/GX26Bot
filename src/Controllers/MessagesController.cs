using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using GX26Bot.Congnitive.LUIS;
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
		public async Task<Message> Post([FromBody]Message message)
		{
			if (message.Type == "Message")
			{
				if (m_help.Contains(message.Text.Trim().ToLower()))
					return message.CreateReplyMessage(HelpMessage.GetHelp());

				//if (message.Text.Trim().ToLower().StartsWith("muuu"))
				//{
				//	Message msg = message.CreateReplyMessage($"{message.Text} to you too");
				//	msg.Attachments = new List<Attachment>();
				//	msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = "http://www.sustained.ie/wp-content/uploads/2013/01/Close-up+of+Cow.jpg" });

				//	return msg;
				//}

				//if (message.Text.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length == 1)
				//	return message.CreateReplyMessage("Soy mas que eso!, por favor escribe tu pregunta en lengaje natural!. Puedes escribir 'help' para obetener ayuda");

				return await Conversation.SendAsync(message, MakeRoot);
			}
			else
			{
				return HandleSystemMessage(message);
			}
		}

		internal static IDialog<GX26Manager> MakeRoot()
		{
			return Chain.From(() => new LUISManager());
		}

		private Message HandleSystemMessage(Message message)
		{
			if (message.Type == "Ping")
			{
				Message reply = message.CreateReplyMessage();
				reply.Type = "Ping";
				return reply;
			}
			else if (message.Type == "DeleteUserData")
			{
				// Implement user deletion here
				// If we handle user deletion, return a real message
			}
			else if (message.Type == "BotAddedToConversation")
			{
			}
			else if (message.Type == "BotRemovedFromConversation")
			{
			}
			else if (message.Type == "UserAddedToConversation")
			{
			}
			else if (message.Type == "UserRemovedFromConversation")
			{
			}
			else if (message.Type == "EndOfConversation")
			{
			}

			return null;
		}
	}
}