using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;

namespace GX26Bot.Dialogs
{
	[Serializable]
	public class RestroomDialog
	{
		[Prompt("What floor are you in? {||}", IsLocalizable = true)]
		public int Floor { get; set; }
	}
}