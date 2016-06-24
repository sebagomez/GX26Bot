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
			return $@"Hola! Acá va a ir toda la ayuda que querramos meter. Seguramente vaya a estar en algún config para poder corregir y no tener que recompilar.

Lo que sé hacer es contestar preguntas sobre las ubicaciones de los baños, las roperías y las salas.
Ejemplo de estas preguntas son:  

- dónde están los baños?  
- dónde está la repería?
- dónde puedo dejar mi abrigo?  
- dónde queda la sala Renoir?
- cómo llego al Ballroom B?
- dónde está la sala 4CF?

También voy a saber, pero todavía no, cuales son las distintas charlas de las salas y cuales son las 'próximas' charlas en general o de una sala en particular";
		}
	}
}