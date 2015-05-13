using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	List<string> collection;
	int times;
	void Start() {
		rb = GetComponent<Rigidbody>();
		clientid = client.GetComponent<Client> ().index();
		//Debug.Log ("hi");
		Debug.Log ("player script line 24: "+clientid);
		//Debug.Log ("bye");
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
			Debug.Log (message);
			times++;
			if (times > 100) {

				Client.lazySend (message);
				times = 0;
			}
		} else if (clientid == playerid && !alive) {
			string index = playerid.ToString ();
			string xs = rb.position [0].ToString ();
			string zs = rb.position [2].ToString ();
			//Debug.Log ("Player;" + index + ";F;" + xs + ";" + zs + ";");
			Client.lazySend ("Player;" + index + ";F;" + xs + ";" + zs + ";");
			this.gameObject.SetActive (false);
		} else {
			rb.useGravity = false;
			//Debug.Log (data);
			collection = split (data);
			//Debug.Log (data);
			if (playerid * 5 + 6 <= collection.Count){
				if (collection[playerid*5 + 2] == "F") {
					this.gameObject.SetActive (false);
				}
				float locx = float.Parse (collection[5*playerid + 3]);
				float locz = float.Parse (collection[5*playerid + 4]);
				//Debug.Log (locx);
				//Debug.Log (locz);
				transform.position = new Vector3 (locx, .5f, locz);
			}
		}
	}
	public List<string> split(string things)
	{
		List<string> message_parts = new List<string>();
		string[] strings = things.Split(';');
		foreach (string s in strings)//Null reference exception here
		{
			if (s != "")
			{
				message_parts.Add(s);
			}
		}
		return message_parts;
	}
}


