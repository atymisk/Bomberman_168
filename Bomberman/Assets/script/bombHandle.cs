using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bombHandle : MonoBehaviour {
	public Client client;
	public GameObject bomb;
	void FixedUpdate(){
		Game.Bomb boom = Client.game.popBomb ();
		if (boom != null) {
			GameObject creation = Instantiate (bomb, new Vector3 (boom.x, .5f, boom.z), Quaternion.identity)as GameObject;
			creation.GetComponent<bombLogic> ().active = true;
			//remember to add the strength thing
			//IMPORTANT
		}
	}
}
