using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UImethods : MonoBehaviour {

	public InputField passfield;
	public Text censortext;
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
		if(passfield.text.Length >= censortext.text.Length)
		{
			//Debug.Log("password length greater than *");
			censortext.text += "*";
		}
		else if(passfield.text.Length <= censortext.text.Length)
		{
			//Debug.Log("password length smaller than *");
			censortext.text = censortext.text.Remove(passfield.text.Length);
		}
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
	public void gameselectscreen()
	{
		Application.LoadLevel("GameSelect");
	}
	//if the client is connected, go straight to lobby
	public void homepagemultipcheck()
	{
		if(Client.connected)
		{
			tolobby();
		}
		else
		{
			backtologin();
		}
	}
}
