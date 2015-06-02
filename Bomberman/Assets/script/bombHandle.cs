using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// simple script that manages bombs backlogged on the client
// script is linked to the client and uses the client function popBomb
// popbomb returns the bomb and removes it from the client queue
// it runs continuously checking for bombs and if one is found
// it instantiates one using the information gained from the game bomb
public class bombHandle : MonoBehaviour {
	public Client client;
	public GameObject bomb;
	void FixedUpdate()
	{
		Game.Bomb boom = Client.game.popBomb ();
		if (boom != null) 
		{
			//instantiates a bomb 
			GameObject creation = Instantiate (bomb, new Vector3 (boom.x, .5f, boom.z), Quaternion.identity)as GameObject;
			//starts the bomb ticker
			creation.GetComponent<bombLogic> ().active = true;
		}
	}
}
