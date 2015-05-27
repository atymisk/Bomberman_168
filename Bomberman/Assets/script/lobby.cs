using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Net;

public class lobby : MonoBehaviour 
{
	public Text[] ptxt = new Text[4];
	public static string[] lists = new string[4];
	public Client c;
	public static bool ready = false;
	public static bool loaded = false;
	private string lobbyname = "";
	//not called automatically
	public static void Start ()
	{
		lists[0] = "...Awaiting Player";
		lists[1] = "...Awaiting Player";
		lists[2] = "...Awaiting Player";
		lists[3] = "...Awaiting Player";
	}

	//method reacts to the button press, will send a message to the server say it's ready
	//Server should have a record of all players
	public void readyup()
	{
		ready = !ready;
		Debug.Log((ready? "ready" : "not ready"));
		lobbyname = c.getlobbyname();
		if(ready)
		{
			c.sendmsg("This player is ready " + Client.getIndex() + "|" +lobbyname);
			//ready = !ready;
		}
		else
		{
			c.sendmsg("This player is not ready "+Client.getIndex () + "|" + lobbyname);
		}
	}
	public static void readyupdates(int index)
	{
		//if(!loaded || index != Client.getIndex())
		//{
			Debug.Log ("lobby.cs: readyupdates");
			lists[index] += " - Ready!";
			//loaded = true;
		//}
	}
	public static void notreadyupdate(int index)
	{
		Debug.Log("Lobby.cs notreadyupdate " + lists[index].IndexOf("-"));
		int i = lists[index].IndexOf("-");
		if(i != -1)
		{
			lists[index] = lists[index].Substring(0,lists[index].IndexOf("-")-1);
		}
	}

	public static void setup(string p,int index)
	{
		lists[index] = p;
		//Debug.Log("Lobby Script Line 62: "+lists[index]);
	}
	public void updatetxt()
	{
		ptxt[0].text = lists[0];
		ptxt[1].text = lists[1];
		ptxt[2].text = lists[2];
		ptxt[3].text = lists[3];
		//Debug.Log(lists[0]);
	}
	public void leavelobby()
	{
		//tell server, remove me from this lobby
		Client.lazySend("Remove me " + Client.getUser() +"|"+c.getlobbyname());
	}
	public static void resetlobby()
	{
		for(int i = 0; i < lists.Length; i++)
		{
			lists[i] = "...";
		}
	}
	void FixedUpdate () 
	{
		updatetxt();
	}
	/*
	 * Client connects to server on login
	 * Gets to lobby page, client data is intact
	 * Server sends back login success and its position in the player array
	 */

}