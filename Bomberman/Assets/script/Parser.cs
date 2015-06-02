using System;
using System.Collections.Generic;
using System.Text;

public static class Parser
{
	private static string[] EOF = new string[]{"<EOF>"};
	
	public static List<string> cleanEOF(string content)
	{
		//clean and put into buffer
		string[] messages = content.Split(EOF,StringSplitOptions.None);//EOF taken out
		List<string> listOfMessages = new List<string>();
		foreach (string message in messages)
		{
			if (message != "")
			{
				listOfMessages.Add (message);
			}
		}
		return listOfMessages;
	}
	
	public static bool convertBool(string tf)
	{
		if (tf == "T")
		{
			return true;
		}
		return false;
	}
	
	public static List<string> split(string content)
	{
		string[] strings = content.Split(';');
		List<string> messageParts = new List<string>();
		foreach (string s in strings)
		{
			if (s != "")
			{
				messageParts.Add(s);
			}
		}
		return messageParts;
	}

	public static List<string> split(string content, char c, bool clean)
	{
		string[] strings = content.Split(c);
		List<string> parts = new List<string>();
		foreach (string s in strings)
		{
			if (((clean) && (s != "")) || (!clean))
			{
				parts.Add(s);
			}
		}
		return parts;
	}
}