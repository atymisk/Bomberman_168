using UnityEngine;
using System.Collections;

//bomblogic is the class script determing proper bomb behavior
// this involvesmost specifically the self detonation timer
// and the creation of bullet boxes that destroy objects
public class bombLogic : MonoBehaviour {
	public Rigidbody rb, bullet, bullet1, bullet2, bullet3, bullet4;
	public int timer = 100;
	public int strength;
	public bool active;
	public GameObject player;

	void Start () 
	{
		rb = GetComponent<Rigidbody>();

	}
	
	// once a bomb is instantiated and enabled with the active tag
	// the timer counts down over 100 frames before the bomb detonates
	// when a bomb detonates it creates for bullets that are created and die
	// within the same frame
	// the bullets are formed around the bomb in the north south east and west directions

	void FixedUpdate () 
	{
		if (active) 
		{
			timer--;
			if (timer <= 0) 
			{
				// all 4 bullets are created
				//for behavior see the impact.cs script
				bullet1 = Instantiate (bullet, new Vector3 (rb.position [0], rb.position [1], rb.position [2] + 1), Quaternion.identity) as Rigidbody;
				bullet2 = Instantiate (bullet, new Vector3 (rb.position [0] + 1, rb.position [1], rb.position [2]), Quaternion.identity) as Rigidbody;
				bullet3 = Instantiate (bullet, new Vector3 (rb.position [0], rb.position [1], rb.position [2] - 1), Quaternion.identity) as Rigidbody;
				bullet4 = Instantiate (bullet, new Vector3 (rb.position [0] - 1, rb.position [1], rb.position [2]), Quaternion.identity) as Rigidbody;
				// bullets are moved in 4 location around the bombs original location
				for (int i = 1; i < strength + 1; i++) 
				{
					bullet1.MovePosition (rb.gameObject.transform.position + bullet1.gameObject.transform.forward * i);
					bullet2.MovePosition (rb.gameObject.transform.position + bullet2.gameObject.transform.right * i);
					bullet3.MovePosition (rb.gameObject.transform.position - bullet3.gameObject.transform.forward * i);
					bullet4.MovePosition (rb.gameObject.transform.position - bullet4.gameObject.transform.right * i);
				}
				//bullets are terminated and act only one things existing
				//where they are in that frame
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
