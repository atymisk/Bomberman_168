using UnityEngine;
using System.Collections;

public class playerMovement : MonoBehaviour {

	public float speed;
	public Rigidbody rb, bomb, clone, bullet, bullet1, bullet2, bullet3, bullet4;
	public Vector3 movement;
	public int timer = 100;
	public int strength;
	public bool active;
	void Start() {
		rb = GetComponent<Rigidbody>();
	}

	void Update() {

		if (Input.GetButtonDown("Fire1") && !active) {
			clone = Instantiate(bomb, rb.position, Quaternion.identity) as Rigidbody;
			active = true;
	
		}
	}
	
	void FixedUpdate () {

		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);

		rb.AddForce (movement * speed);

		if (active){
			timer--;
			if (timer <= 0){

				bullet1 = Instantiate (bullet, new Vector3(clone.position[0], clone.position[1], clone.position[2] + .5f), Quaternion.identity) as Rigidbody;
				bullet2 = Instantiate (bullet, new Vector3(clone.position[0] + .5f, clone.position[1], clone.position[2]), Quaternion.identity) as Rigidbody;
				bullet3 = Instantiate (bullet, new Vector3(clone.position[0], clone.position[1], clone.position[2] - .5f), Quaternion.identity) as Rigidbody;
				bullet4 = Instantiate (bullet, new Vector3(clone.position[0] - .5f, clone.position[1], clone.position[2]), Quaternion.identity) as Rigidbody;


				for (int i = 1; i < strength + 1; i++){
					bullet1.MovePosition(clone.gameObject.transform.position + bullet1.gameObject.transform.forward * i);
					bullet2.MovePosition(clone.gameObject.transform.position + bullet2.gameObject.transform.right * i);
					bullet3.MovePosition(clone.gameObject.transform.position - bullet3.gameObject.transform.forward * i);
					bullet4.MovePosition(clone.gameObject.transform.position - bullet4.gameObject.transform.right * i);
				}

				Destroy(bullet1.gameObject);
				Destroy(bullet2.gameObject);
				Destroy(bullet3.gameObject);
				Destroy(bullet4.gameObject);
				Destroy(clone.gameObject);
				timer = 100;
				active = false;


			}
		}
	}
}