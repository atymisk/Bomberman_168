using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// this is the script for determing the proper location of 
// player objects as well as the control management for
// one object to be bound to one player
// additionally it uses the client object functions from the client
// it is linked to to send and receive game updates
public class playerMovement : MonoBehaviour {
	//lots of variables
	public GameObject b;

	public float speed;
	Rigidbody rb, bomb, clone;
	public Vector3 movement;
	public int timer;
	public int dropTime;
	public int strength;
	public int playerid, clientid;
	public Client client;
	public bool active = false;
	public bool alive = true;
	string mydata;
	List<Game.Player> allplayer = Client.game.allPlayers;
	int readtime;
	int times;
	float oldx, oldz;
	void Start() 
	{
		// the client id is determined by the server
		rb = GetComponent<Rigidbody>();
		clientid = Client.getIndex();
	}

	void Update() 
	{
		//bombs drops are bound by left click and are capable on each player object
		// however each player object is also given a hard set object id known as playerid
		// if the playerid variable matches the server determined client id, the bomb
		// is placed from it and the others ignore the command
		// timer >= drop is a charge cap for bomb placement
		// timer caps at 100 and droptime is removed from it on each bomb
		// if the timer is less than the drop time, bombs wont fall and timer wont decrease
		if (Input.GetButtonDown("Fire1") && !active && playerid == clientid && timer >= dropTime) 
		{
			timer -= dropTime;
			//this is a client to server message indication a bomb drop
			// format "Bomb;x;z;strength;lobbyname"
			// strength would theoretically increase explosion radius but is unimplemented
			Client.lazySend("Bomb;" + rb.position[0].ToString() + ";" + rb.position[2].ToString() + ";" + strength.ToString() + ";"+client.getlobbyname()+";");
		}
		if (timer < 100) {
			timer++;;
		}
		//Debug.Log (timer);
	}
	
	void FixedUpdate () 
	{
		//case one
		// the current object of the 4 player objects is the one controlled
		// by this particular client and is not defeated
		if (playerid == clientid && alive) 
		{
			// player motion is controlled by wasd and adjusts the velocity
			// of the player object
			float moveHorizontal = Input.GetAxis ("Horizontal");
			float moveVertical = Input.GetAxis ("Vertical");

			movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
			rb.AddForce (movement * speed);
			//building a message for the client to send to server
			// messsage id for player information
			// Player;boolean alive;x coordinate; z coordinate; x velocity, y velocity; lobby name;
			string index = playerid.ToString ();
			string xs = rb.position [0].ToString ();
			string zs = rb.position [2].ToString ();
			times++;
			//messages are dispatched to the server every 10 frames
			if (times > 10) {
				//Debug.Log ("Player;" + index + ";T;" + xs + ";" + zs + ";" + rb.velocity.x + ";" + rb.velocity.y + ";"+client.getlobbyname()+";");
				Client.lazySend ("Player;" + index + ";T;" + xs + ";" + zs + ";" + rb.velocity.x + ";" + rb.velocity.y + ";"+client.getlobbyname()+";");
				times = 0;
			}
		} 
		//case 2 
		// the player has died
		// this is the final message a client sends out to indicate death
		// after it is sent no other messages for a particular game are sent
		// the buffer is freed up and the object is deleted
		else if (Client.connected && clientid == playerid && !alive) 
		{
			string index = playerid.ToString ();
			string xs = rb.position [0].ToString ();
			string zs = rb.position [2].ToString ();
			//Debug.Log ("Player;" + index + ";F;" + xs + ";" + zs + ";" + rb.velocity.x + ";" + rb.velocity.y + ";"+client.getlobbyname()+";");
			Client.lazySend ("Player;" + index + ";F;" + xs + ";" + zs + ";" + rb.velocity.x + ";" + rb.velocity.y + ";"+client.getlobbyname()+";");
			this.gameObject.SetActive (false);
		} 
		//case 3
		//not a player
		//for every other object that is not the player
		// their positions are updated via server information
		// the client has no control over the actions of these objects
		else if(Client.connected) 
		{
			//without this the objects strangely fall through the floor very slowly
			//even in spite of a y axis lock
			rb.useGravity = false;
			//checks to see if a message for a given player object exists and is alive
			if (allplayer.Count >= playerid + 1 && allplayer[playerid].active){
				//x and y positions are only managed if a change from the server is received
				float locx = allplayer[playerid].x;
				float locz = allplayer[playerid].z;
				if (oldx != locx || oldz != locz){
					transform.position = new Vector3 (locx, .5f, locz);
					oldx = locx;
					oldz = locz;
				}
				// this is supposed to handle dead reckoning by copying over an objects
				// velocity which would predict movement
				// however it doesnt appear to ever be close enough
				// 1:1 seems to have no impact but anything higher frequently races ahead
				float velx = allplayer[playerid].xv;
				float velz = allplayer[playerid].zv;

				movement = new Vector3 (velx, 0.0f, velz);
				//1.1 seems to bethe safest approcimation before it becomes 
				// bizarre looking
				rb.velocity = (movement * 1.1);
			}
			else 
			{
				//if theres no message or the player is registered defeated
				// object is deleted from scene
				this.gameObject.SetActive (false);
			}
		}
	}


	void OnApplicationQuit()
	{
		// informs the server of your defeat if you close the application
		//prevents undefined behavior server side
		this.gameObject.SetActive(false);
		this.alive = false;
	}

}


