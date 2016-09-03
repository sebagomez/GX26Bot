using GX26Bot.Helpers;

namespace GX26Bot.Controllers
{
	public class HelpMessage
	{
		public static string GetHelp(string lang, string user)
		{
			switch (lang)
			{
				case LanguageHelper.SPANISH:
					return $@"Hola {user}! Mi nombre es RUDI y puedo ayudarte a moverte durante en GX26.

Sé la ubicacíón de las salas, los baños y roperías.
También conozco la agenda del evento.
Ejemplo de estas preguntas son:  

- dónde están los baños?  
- dónde está la repería?
- dónde queda la sala Renoir?
- a qué hora habla Nicolás Jodal?

Cuando necesites volver a ver esto simplemente me saludas :)";
				case LanguageHelper.PORTUGUESE:
					return $@"Oi {user}! Mi nombre es RUDI y puedo ayudarte a moverte durante en GX26.

Sé la ubicacíón de las salas, los baños y roperías.
También conozco la agenda del evento.
Ejemplo de estas preguntas son:  

- dónde están los baños?  
- dónde está la repería?
- dónde queda la sala Renoir?
- a qué hora habla Nicolás Jodal?

Cuando necesites volver a ver esto simplemente me saludas :)";
				default:
					return $@"Hello {user}! My name is RUDI and I can help you move around during the GX26.

I know where the restrooms, session rooms and coat checks are.  
I also know the schedule of the event.  
These are some of the question I can answer:  

- where are the bathrooms?  
- where can I leave my coat?
- where is the Conference Room?
- when is Nicolás Jodal speaking?

Whenever you need to see this again, just say hi :)";
			}

			
		}
	}
}