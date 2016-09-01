using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GX26Bot.Controllers
{
	public class HelpMessage
	{
		public static string GetHelp()
		{
			return $@"Hola! Mi nombre es RUDI y puedo ayudarte a moverte durante en GX26.

Sé la ubicacíón de las salas, los baños y roperías.
También conozco la agenda del evento.
Ejemplo de estas preguntas son:  

- dónde están los baños?  
- dónde está la repería?
- dónde queda la sala Renoir?
- a qué hora habla Nicolás Jodal?

Cuando necesites volver a ver esto simplemente me saludas :)";
		}
	}
}