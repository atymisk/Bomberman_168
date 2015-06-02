using UnityEngine;
using System.Collections;
// this is the power up that increases the bomb drop frequency
// the player that hits it has its droptimer reduced by 10
// drop time begins at 50
// afterwards the object is removed
public class frequency : MonoBehaviour {

	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Player") {
			other.GetComponent<playerMovement>().dropTime -= 10;
			Destroy(this.gameObject);
		}
	}
}
