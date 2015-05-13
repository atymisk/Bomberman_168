using UnityEngine;
using System.Collections;

public class playerMovement : MonoBehaviour {
	
	public float speed;
	public Rigidbody rb, bomb, clone;
	public Vector3 movement;
	public int timer = 100;
	public int dropTime;
	public int strength;
	public int playerid, clientid;
	public GameObject client;
	public bool active = false;
	public bool alive = true;
	string data, mydata;
	void Start() {
		rb = GetComponent<Rigidbody>();
	}
	
	void Update() {
		
		if (Input.GetButtonDown("Fire1") && !active && playerid == clientid && timer >= dropTime) {
			timer -= dropTime;
			Client.lazySend("Bomb;" + rb.position[0].ToString() + ";" + rb.position[2].ToString() + ";" + strength.ToString() + ";");
			clone = Instantiate(bomb, rb.position, Quaternion.identity) as Rigidbody;
			var theScript = clone.GetComponent<bombLogic>();
			theScript.active = true;
			theScript.player = this.gameObject;
			//Client.lazySend(...)
		}
	}
	
	void FixedUpdate () {
		
		data = client.GetComponent<Client> ().GetData ();
		Debug.Log (data);
		int found = data.IndexOf("Player;");
		data = data.Substring(found + 7);
		if (timer < 100) {
			timer++;
		}
		if (playerid == clientid && alive) {
			client.GetComponent<Client> ().x = rb.position [0];
			client.GetComponent<Client> ().z = rb.position [2];
			float moveHorizontal = Input.GetAxis ("Horizontal");
			float moveVertical = Input.GetAxis ("Vertical");
			
			movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
			
			rb.AddForce (movement * speed);
			
			string index = playerid.ToString ();
			string xs = rb.position [0].ToString ();
			string zs = rb.position [2].ToString ();
			string message = "Player;" + index + ";T;" + xs + ";" + zs + ";";
			//Debug.Log (message);
			Client.lazySend (message);
		} else if (clientid == playerid && !alive) {
			string index = playerid.ToString ();
			string xs = rb.position [0].ToString ();
			string zs = rb.position [2].ToString ();
			//Debug.Log ("Player;" + index + ";F;" + xs + ";" + zs + ";");
			Client.lazySend ("Player;" + index + ";F;" + xs + ";" + zs + ";");
			this.gameObject.SetActive(false);
		} else {
			rb.useGravity = false;
			//Debug.Log (data);
			found = data.IndexOf (";" + playerid + ";") + 3;
			int foundEnd = data.IndexOf (";" + playerid + "end;");
			data = data.Substring(found, foundEnd-found);
			if (data.Substring(0,1) == "F"){
				//Debug.Log (data.Substring (0,1));
				this.gameObject.SetActive(false);
			}
			data = data.Substring(2);
			found = data.IndexOf(";");
			float locx = float.Parse(data.Substring(0, found));
			data = data.Substring (found + 1);
			float locz = float.Parse(data);
			//Debug.Log (locx);
			//Debug.Log (locz);
			transform.position = new Vector3(locx, .5f, locz);
			
		}
	}
}