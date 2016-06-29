﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using GX26Bot.Congnitive.Watson;
using GX26Bot.Dialogs;
using GX26Bot.Images;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace GX26Bot.Congnitive.LUIS
{
	[Serializable]
	public class LUISManager : LuisDialog<GX26Manager>
	{
		readonly string QUERY_LANGUAGE = "QUERY_LANGUAGE";

		static string s_model { get; } = ConfigurationManager.AppSettings["LuisModelId"];
		static string s_key { get; } = ConfigurationManager.AppSettings["LuisSubscriptionKey"];

		static LuisService s_service;
		static LUISManager()
		{
			LuisModelAttribute model = new LuisModelAttribute(ConfigurationManager.AppSettings["LuisModelId"], ConfigurationManager.AppSettings["LuisSubscriptionKey"]);
			s_service = new LuisService(model);
		}

		public LUISManager() :base(s_service) { }

		[LuisIntent("")]
		public async Task None(IDialogContext context, LuisResult result)
		{
			await context.PostAsync($"No che, no hay caso. No se qué me estás pidiendo cuando dices _{result.Query}_");
			context.Wait(MessageReceived);
		}

		[LuisIntent("Bio")]
		public async Task SearchBio(IDialogContext context, LuisResult result)
		{
			await context.PostAsync("Bien, sé que quieres una 'bio'");
			StringBuilder builder = new StringBuilder();
			foreach (var entity in result.Entities)
				builder.AppendLine($"{entity.Type}: {entity.Entity}");

			if (builder.Length > 0)
				await context.PostAsync(builder.ToString());
			else
				await context.PostAsync($"pero no entendí de qué características :(");

			context.Wait(MessageReceived);
		}

		[LuisIntent("Restroom")]
		public async Task Restroom(IDialogContext context, LuisResult result)
		{
			TextLanguage lang = await LanguageHelper.GetTextLanguage(result.Query);

			context.UserData.SetValue<TextLanguage>(QUERY_LANGUAGE, lang);

			var floors = new[] { 2, 3, 4, 6, 25 };
			PromptDialog.Choice(context, RestroomFloorComplete, floors, LanguageHelper.GetFloorQuestion(lang, floors), LanguageHelper.GetFloorQuestion(lang, floors, true), 3, PromptStyle.Auto);
		}

		private async Task RestroomFloorComplete(IDialogContext context, IAwaitable<int> result)
		{
			try
			{
				int floor = await result;

				TextLanguage lang = context.UserData.Get<TextLanguage>(QUERY_LANGUAGE);
				context.UserData.RemoveValue(QUERY_LANGUAGE);

				Message msg = context.MakeMessage();
				msg.Text = LanguageHelper.GetRestroomMessage(lang, floor);
				msg.Attachments = new List<Attachment>();
				msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetBathroomImage(floor) });

				await context.PostAsync(msg);
			}
			catch (Exception) {  }

			context.Wait(MessageReceived);
		}

		[LuisIntent("Clothes")]
		public async Task Clothes(IDialogContext context, LuisResult result)
		{
			Message msg = context.MakeMessage();
			msg.Text = await LanguageHelper.GetClothesMessage(result.Query);
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetBathroomImage(2) });

			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}

		[LuisIntent("Room")]
		public async Task Rooms(IDialogContext context, LuisResult result)
		{
			if (result.Entities.Count == 0) {
				await context.PostAsync("No entendí qué sala está buscando");
				context.Wait(MessageReceived);
				return;
			}
			string room = result.Entities[0].Entity;

			Message msg = context.MakeMessage();
			msg.Text = await LanguageHelper.GetRoomMessage(result.Query, room);
			msg.Attachments = new List<Attachment>();
			msg.Attachments.Add(new Attachment { ContentType = "image/png", ContentUrl = ImageHelper.GetRoomImage(room) });

			await context.PostAsync(msg);

			context.Wait(MessageReceived);
		}
	}
}