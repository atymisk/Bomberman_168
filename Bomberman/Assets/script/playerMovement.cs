using UnityEngine;
using System.Collections;

public class playerMovement : MonoBehaviour {

	public float speed;
	public Rigidbody rb, bomb, clone, bullet, bullet1, bullet2, bullet3, bullet4;
	public Vector3 movement;
	public int timer = 100;
	public int strength;
	public bool active = false;
	void Start() {
		rb = GetComponent<Rigidbody>();
	}

	void Update() {

		if (Input.GetButtonDown("Fire1") && !active) {
			clone = Instantiate(bomb, rb.position, Quaternion.identity) as Rigidbody;
			var theScript = clone.GetComponent<bombLogic>();
			theScript.active = true;
			theScript.player = this.gameObject;
		}
	}
	
	void FixedUpdate () {

		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

		rb.AddForce (movement * speed);

	}
}