using UnityEngine;
using System.Collections;

public class playerMovement : MonoBehaviour {

	public float speed;
	public Rigidbody rb, bomb, clone;
	public Vector3 movement;
	public int timer = 100;
	public int strength;
	public int playerid, clientid;
	public GameObject client;

	public bool active = false;
	void Start() {
		rb = GetComponent<Rigidbody>();
	}

	void Update() {

		if (Input.GetButtonDown("Fire1") && !active && playerid == clientid) {
			clone = Instantiate(bomb, rb.position, Quaternion.identity) as Rigidbody;
			var theScript = clone.GetComponent<bombLogic>();
			theScript.active = true;
			theScript.player = this.gameObject;
			//Client.lazySend(...)
		}
	}
	
	void FixedUpdate () {

		if (playerid == clientid) {
			client.GetComponent<Client>().x = rb.position [0];
			client.GetComponent<Client>().z = rb.position [2];
			float moveHorizontal = Input.GetAxis ("Horizontal");
			float moveVertical = Input.GetAxis ("Vertical");

			movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

			rb.AddForce (movement * speed);
		}
	}
}