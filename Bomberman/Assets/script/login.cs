using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class login : MonoBehaviour {

	public InputField user;
	public InputField pass;
	private string tosend = "testing";
	public void submit()
	{
		tosend = user.text + "|" + pass.text;
	}
	public string strsend()
	{
		submit ();
		return tosend;
	}
}
