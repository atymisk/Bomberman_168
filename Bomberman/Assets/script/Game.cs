using System.Collections;
using System.Collections.Generic;
using System;


public class Game
{
	public List<Player> allPlayers = new List<Player>();
	public List<Bomb> allBombs = new List<Bomb>();
	public bool inprogress = false;
	public int myIndex = -1;
	
	public class Player
	{
		public string username;
		public float x;
		public float z;
		public float xv;
		public float zv;
		public int playerIndex = -1;
		public bool active = true;
		
		public Player(string username, float x, float z)
		{
			this.x = x;
			this.z = z;
			this.username = username;
		}
		
		public void setPosition(float x, float z, float xv, float zv)
		{
			this.x = x;
			this.z = z;
			//Jeffrey modified to also take and store in velocity for dead reckoning
			this.xv = xv;
			this.zv = zv;
		}
	}
	
	public class Bomb
	{
		public float x;
		public float z;
		public int strength;
		
		public Bomb(float x, float z, int strength)
		{
			this.x = x;
			this.z = z;
			this.strength = strength;
		}
	}
	
	public Player myself()
	{
		return allPlayers[myIndex];
	}
	
	public Bomb popBomb()
	{
		Bomb bomb = null;
		if (allBombs.Count > 0)
		{
			bomb = allBombs[0];
			allBombs.Remove (bomb);
		}
		return bomb;
	}
	
	public void addBomb(List<string> messageParts)
	{
		if (messageParts[0] != "Bomb")
		{
			return;
		}
		
		float x = Convert.ToSingle(messageParts[1]);
		float z = Convert.ToSingle(messageParts[2]);
		int strength = Convert.ToInt32(messageParts[3]);
		allBombs.Add (new Bomb(x, z, strength));
	}
	
	public void addPlayer(string username)//called by startgame
	{
		int numberOfPlayers = allPlayers.Count;
		float x, z;
		//Debug.Log ("Generating player");
		//Debug.Log(numberOfPlayers);
		switch (numberOfPlayers)
		{
		case 0:
			x = -9; z = -9;
			break;
		case 1:
			x = 9; z = 9;
			break;
		case 2:
			x = 9; z = -9;
			break;
		case 3:
			x = -9; z = 9;
			break;
		default:
			x = -9001; z = -9001;
			break;
		}
		
		allPlayers.Add (new Player(username, x, z));
		allPlayers[numberOfPlayers].playerIndex = numberOfPlayers;
	}
	
	public void updatePlayers(List<string> messageParts)
	{
		if (messageParts[0] != "Player")
		{
			return;
		}
		
		int playerNumber = 0;
		for (int messageindex = 1; messageindex < messageParts.Count; messageindex+=7)
		{
			allPlayers[playerNumber].active = Parser.convertBool(messageParts[messageindex+1]);
			//Debug.Log(allPlayers[playerNumber].active);
			allPlayers[playerNumber].setPosition(
				float.Parse(messageParts[messageindex+2]), float.Parse(messageParts[messageindex+3]), float.Parse(messageParts[messageindex+4]), float.Parse(messageParts[messageindex+5]));
			playerNumber++;
		}
	}
}
