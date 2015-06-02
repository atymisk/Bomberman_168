using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class chat : MonoBehaviour 
{
	public static string chatHeader = "<BOF>Chat;["+Client.getUser()+"]: ";
	public GameObject panel;
	public Text textBox;
	public Text inputField;
	public static string chatText = "";

	// Use this for initialization
	void Start () 
	{
		if(!Client.connected)
		{
			//t.gameObject.SetActive(false);
		//	panel.SetActive(false);
		}

		chatText = "~ Welcome to the Lobby Chat! ~\n";
	}

	// Upon hitting submit, send chat text to server first.
	// The server will signal back to you with the input.
	public void submit()
	{
		Client.lazySend(chatHeader + inputField.text);
		inputField.text = "";
	}

	private string processChatText()
	{
		int rows = 0;
		List<string> textByRows = Parser.split(chatText, '\n', false);
		string chatText2 = "";
		int index = textByRows.Count;
		while (true)
		{
			index--;
			if (index < 0) break;
			string s = textByRows[index];
			int numOfRowsThisTextTakesUp = s.Length/22;
			rows += numOfRowsThisTextTakesUp + 1;
			if (rows >= 7) break;
			chatText2 = s + "\n" + chatText2;
		}
		return chatText2;
	}

	// Update is called once per frame
	void Update () 
	{
		textBox.text = processChatText();
	}
}
