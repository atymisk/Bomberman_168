using UnityEngine;
using System.Collections;

public class impact : MonoBehaviour 
{
	public GameObject explosionfx;
	public GameObject bullet;
	GameObject myefct = null;
	void OnEnable()
	{
		//myefct = explosionfx;
		Vector3 pos = bullet.gameObject.transform.position;
		myefct = (GameObject)Instantiate(explosionfx,new Vector3(pos.x,0.5f,pos.z),Quaternion.identity);
		myefct.GetComponentInChildren<esplode>().active = true;
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player") 
		{
			other.gameObject.GetComponent<playerMovement>().alive = false;
		}
		if (other.gameObject.tag == "Destructable") 
		{
			Destroy (other.gameObject);
			Destroy (this.gameObject);
		}
		if (other.gameObject.tag == "bomb") 
		{
			//other.gameObject.GetComponent<bombLogic>().timer = 1;
			//Debug.Log("kk");
		}
	}

}
