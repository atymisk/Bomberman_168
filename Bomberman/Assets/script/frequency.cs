using UnityEngine;
using System.Collections;

public class frequency : MonoBehaviour {

	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Player") {
			other.GetComponent<playerMovement>().dropTime -= 10;
			Destroy(this.gameObject);
		}
	}
}
