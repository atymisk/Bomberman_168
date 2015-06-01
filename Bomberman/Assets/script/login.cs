using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class login : MonoBehaviour {

	public InputField user;
	public InputField p;
	private string tosend = "testing";
	public void submit()
	{
		//hash password
		tosend = user.text + "|" + MD5Manager.hashPassword(p.text);
	}
	public string strsend()
	{
		submit ();
		//Debug.Log(tosend);
		string sendMe = tosend;
		wipe ();
		return sendMe;
	}
	public void wipe()
	{
		tosend = "";
	}
}
