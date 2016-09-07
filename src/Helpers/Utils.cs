using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

namespace GX26Bot.Helpers
{
	public class Utils
	{
		public static T Deserialize<T>(Stream stream)
		{
#if DEBUG
			string strData;
			using (StreamReader reader = new StreamReader(stream))
				strData = reader.ReadToEnd();

			System.Diagnostics.Debug.Write(strData);

			using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(strData)))
			{
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
				return (T)serializer.ReadObject(ms);
			}
#else

			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
			return (T)serializer.ReadObject(stream);
#endif
		}

		public static string ToJson<T>(T obj)
		{
			MemoryStream stream = new MemoryStream();
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
			serializer.WriteObject(stream, obj);
			stream.Position = 0;
			StreamReader reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}

		static string UNRESERVED_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

		//http://en.wikipedia.org/wiki/Percent-encoding
		//http://www.w3schools.com/tags/ref_urlencode.asp see 'Try It Yourself' to see if this function is encoding well
		//This should be encoded according to RFC3986 http://tools.ietf.org/html/rfc3986
		//I could not find any native .net function to achieve this
		/// <summary>
		/// Encodes a string according to RFC 3986
		/// </summary>
		/// <param name="value">string to encode</param>
		/// <returns></returns>
		public static string EncodeString(string value)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char c in value)
			{
				if (UNRESERVED_CHARS.IndexOf(c) != -1)
					sb.Append(c);
				else
				{
					byte[] encoded = Encoding.UTF8.GetBytes(new char[] { c });
					for (int i = 0; i < encoded.Length; i++)
					{
						sb.Append('%');
						sb.Append(encoded[i].ToString("X2"));
					}
				}
			}
			return sb.ToString();
		}
	}


}