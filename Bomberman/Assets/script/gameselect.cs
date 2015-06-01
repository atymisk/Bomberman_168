using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class gameselect : MonoBehaviour 
{
	//111,221,130
	public InputField findgame;
	public InputField creategamename;

	public GameObject gamenotfound;
	public GameObject popup;
	public GameObject mypanel;
	public Text err;
	public Text username;
	string gamename = "";

	static bool found = false;
	static bool showpopup = false;
	bool error = false;

	void Start () 
	{
		//Debug.Log("GameSelect Awake Called");
		gamenotfound.SetActive(false);
		mypanel.SetActive(false);
		popup.SetActive(false);
		username.text += Client.getUser();
		//Client.lazySend("Get Games");//receive all games that exists
	}

	public void searchgame()
	{
		string s = findgame.text;
		if(s.Length != 0)
		{
		//	Debug.Log("Searching for game... " + s);
			gamename = s;
			Client.lazySend("Find game: " + s);
			error = false;
			err.text = "";
		}
		else
		{
			error = true;
			err.text = "No name was entered";
		}
	}
	public static void gameexists()
	{
		found = true;
	}
	public void creategame()
	{
		mypanel.SetActive(true);
	}
	public void closepanel()
	{
		mypanel.SetActive(false);
	}
	public void submitlobby()
	{
		gamename = creategamename.text;
		if(gamename.Length == 0)
		{
			error = true;
			err.text = "No Name was specified";
		}
		mypanel.SetActive(false);
		if(!error)
		{
			Debug.Log("New Lobby: " + gamename);
			err.text = "";
			error = false;
			Client.lazySend("New Game " + gamename + "|" + Client.getUser());
		}
	}
	public void logout()
	{
		//tell client to logout/disconnect
		Client.DisconnectMe();
		Application.LoadLevel("Home");
	}
	public static void showpop()
	{
		showpopup = true;
	}
	public void join()
	{
		closepopup();
		Client.lazySend("Joining " + gamename +"|" + Client.getUser());
	}
	public void closepopup()
	{
		found = false;
		popup.SetActive(false);
	}
	public void closenotfound()
	{
		showpopup = false;
		gamenotfound.SetActive(false);
	}
	void Update()
	{
		if(found)
		{
			popup.SetActive(true);
		}
		if(showpopup)
		{
			gamenotfound.SetActive(true);
		}
	}
}
