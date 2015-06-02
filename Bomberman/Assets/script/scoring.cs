using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// scoreboard script
/// scoreboard script reads off of the client player list
/// every object in the player list is read and continuously
/// rewrites the the board with updated information
/// it writes in the playerid and then reads the id's alive tag
/// if the player is alive then they are read as active, else WRECKT
/// players who are disconnected have their tags set to dead server side
/// and so are read as WRECKT
/// </summary>
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
