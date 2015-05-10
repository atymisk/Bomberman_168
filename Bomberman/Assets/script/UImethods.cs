using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UImethods : MonoBehaviour {

	public InputField passfield;
	public string pass="";
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
		//passfield.text = "";
	}
	public void tomulti()
	{
		Application.LoadLevel("MultiBomberman");
	}
	public void tolobby()
	{
		Application.LoadLevel("Lobby");
	}
	public void tosinglep()
	{
		Application.LoadLevel("Bomberman");
	}
	public void closegame()
	{
		Application.Quit();
	}
	public void homescreen()
	{
		Application.LoadLevel("Home");
	}
}
