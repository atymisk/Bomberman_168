using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class scoring : MonoBehaviour {

	public GameObject words;
	public Client client;
	Text texty;
	string data;
	List<Game.Player> allplayer = Client.game.allPlayers;
	int index = -1;

	void Start () {
		texty = words.GetComponent<Text> ();
	}
	//public Client client;
	void FixedUpdate () 
	{
		texty.text = "";
		if (1 <= allplayer.Count)
		{
			if (allplayer[0].active) 
			{
				texty.text = "Baller -        Pro\n";
			}
			else {
				//texty.text = "Baller -        Not Pro\n";
				texty.text = "\n";
			}
		}
		if (2 <= allplayer.Count)
		{
			if (allplayer[1].active) 
			{
				texty.text += "Ballistic -     Pro\n";
			}
			else {
				//texty.text += "Ballistic -     Not Pro\n";
				texty.text += "\n";
			}
		}
		if (3 <= allplayer.Count){
			if (allplayer[2].active) {
				texty.text += "Ballerina -   Pro\n";
			}
			else {
				//texty.text += "Ballerina -   Not Pro\n";
				texty.text += "\n";
			}
		}
		if (4 <= allplayer.Count){
			if (allplayer[3].active) {
				texty.text += "Balltimore - Pro\n";
			}
			else {
				//texty.text += "Balltimore - Not Pro\n";
				texty.text += "\n";
			}
		}
	}

	public List<string> split(string things)
	{
		List<string> message_parts = new List<string>();
		string[] strings = things.Split(';');
		foreach (string s in strings)//Null reference exception here
		{
			if (s != "")
			{
				message_parts.Add(s);
			}
		}
		return message_parts;
	}
}
