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
//	int index = -1;

	void Start () {
		texty = words.GetComponent<Text> ();
	}
	//public Client client;
	void FixedUpdate () 
	{
        texty.text = "";
        for (int i = 0; i < allplayer.Count; i++)
        {
            texty.text += allplayer[i].playerIndex;
            texty.text += ": ";
            if (allplayer[i].active)
            {
                texty.text += "ACTIVE";
            }
            else
            {
                texty.text += "WRECKT";
            }
            texty.text += "\n";
        }
	}
}
