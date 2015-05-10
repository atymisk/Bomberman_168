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
		tosend = user.text + "|" + p.text;
	}
	public string strsend()
	{
		submit ();
		//Debug.Log(tosend);
		return tosend;
	}
}
