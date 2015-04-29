using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UImethods : MonoBehaviour {

	public InputField passfield;
	private string pass="";
	public void register()
	{
		Application.LoadLevel("Register");
	}
	public void backtologin()
	{
		Application.LoadLevel("LogInScreen");
	}
	public void censorpass()
	{
		pass += passfield.text;

		//Debug.Log(pass);
		passfield.text = "";
	}
}
