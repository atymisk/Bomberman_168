using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Net;

public class lobby : MonoBehaviour 
{
	public static Text[] ptxt = new Text[4];
	public Client c;
	public static bool ready = false;
	public static bool loaded = false;
	// Use this for initialization
	void Start () 
	{

	}

	//method reacts to the button press, will send a message to the server say it's ready
	//Server should have a record of all players
	public void readyup()
	{
		ready = !ready;
		Debug.Log((ready? "ready" : "not ready"));
		if(ready)
		{
			c.sendmsg("This player is ready");
		}
	}

	public static void setup(string p,int index)
	{
		ptxt[index].text = p;
	}

	void FixedUpdate () 
	{

	}

	/*
	 * Client connects to server on login
	 * Gets to lobby page, client data is intact
	 * Server sends back login success and its position in the player array
	 */


















}