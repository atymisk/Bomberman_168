using UnityEngine;
using System.Collections;

public class bombLogic : MonoBehaviour {
	public Rigidbody rb, bullet, bullet1, bullet2, bullet3, bullet4;
	public int timer = 100;
	public int strength;
	public bool active;
	public GameObject player;

	// Use this for initialization
	void Start () 
	{
		rb = GetComponent<Rigidbody>();

	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		if (active) 
		{
			timer--;
			if (timer <= 0) 
			{
				
				bullet1 = Instantiate (bullet, new Vector3 (rb.position [0], rb.position [1], rb.position [2] + 1), Quaternion.identity) as Rigidbody;
				bullet2 = Instantiate (bullet, new Vector3 (rb.position [0] + 1, rb.position [1], rb.position [2]), Quaternion.identity) as Rigidbody;
				bullet3 = Instantiate (bullet, new Vector3 (rb.position [0], rb.position [1], rb.position [2] - 1), Quaternion.identity) as Rigidbody;
				bullet4 = Instantiate (bullet, new Vector3 (rb.position [0] - 1, rb.position [1], rb.position [2]), Quaternion.identity) as Rigidbody;

				for (int i = 1; i < strength + 1; i++) 
				{
					bullet1.MovePosition (rb.gameObject.transform.position + bullet1.gameObject.transform.forward * i);
					bullet2.MovePosition (rb.gameObject.transform.position + bullet2.gameObject.transform.right * i);
					bullet3.MovePosition (rb.gameObject.transform.position - bullet3.gameObject.transform.forward * i);
					bullet4.MovePosition (rb.gameObject.transform.position - bullet4.gameObject.transform.right * i);
				}
				
				Destroy (bullet1.gameObject);
				Destroy (bullet2.gameObject);
				Destroy (bullet3.gameObject);
				Destroy (bullet4.gameObject);

				active = false;
				Destroy (this.gameObject);
			}
		}
	}
}
