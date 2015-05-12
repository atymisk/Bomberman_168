using UnityEngine;
using System.Collections;

public class impact : MonoBehaviour {



	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Player") {
			other.gameObject.SetActive(false);
		}
		if (other.gameObject.tag == "Destructable") {
			Destroy (other.gameObject);
			Destroy (this.gameObject);
		}
		if (other.gameObject.tag == "Wall") {
			//this.gameObject.SetActive (false);
		}
	}
}
