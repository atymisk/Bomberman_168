using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
/*
public class scoring : MonoBehaviour {

	public GameObject words;
	public Client client;
	Text texty;
	string data;
	List<Game.Player> allplayer = Client.game.allPlayers;


	void Start () {
		texty = words.GetComponent<Text> ();
	}
	//public Client client;
	void FixedUpdate () {
		texty.text = "";
		if (6 <= allplayer.Count){
			if (collection[2] == "F") {
				texty.text = "Baller -        Not Pro\n";
			}
			else {
				texty.text = "Baller -        Pro\n";
			}
		}
		if (11 <= collection.Count){
			if (collection[7] == "F") {
				texty.text += "Ballistic -     Not Pro\n";
			}
			else {
				texty.text += "Ballistic -     Pro\n";
			}
		}
		if (16 <= collection.Count){
			if (collection[12] == "F") {
				texty.text += "Ballerina -   Not Pro\n";
			}
			else {
				texty.text += "Ballerina -   Pro\n";
			}
		}
		if (21 <= collection.Count){
			if (collection[17] == "F") {
				texty.text += "Balltimore - Not Pro\n";
			}
			else {
				texty.text += "Balltimore - Pro\n";
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
*/