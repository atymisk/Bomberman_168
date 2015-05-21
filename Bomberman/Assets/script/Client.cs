using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class IP
{
	public enum Address { ANTHONY, FAYE, JEFFREY, MYSQL, LOCALHOST };
	public const string Anthony = "169.234.6.190";
    public const string Faye = "169.234.12.76";
	public const string Jeffrey = "169.234.22.25";
	public const string mySQL = IP.Anthony;

	// Whose IP Address are we using for the server?
    public static Address Server = Address.LOCALHOST;
}

// State object for receiving data from remote device.
public class StateObject
{
    // Client socket.
    public Socket workSocket = null;

    // Size of receive buffer.
    public const int BufferSize = 256;

    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];

    // Received data string.
    public StringBuilder sb = new StringBuilder();

	// Flush the StringBuilder so it's now empty
	public void flushSB()
	{
		sb = new StringBuilder();
	}

    // ManualResetEvent now changed to AutoResetEvent
    public AutoResetEvent connectDone = new AutoResetEvent(false);
    public AutoResetEvent receiveDone = new AutoResetEvent(false);
    public AutoResetEvent sendDone = new AutoResetEvent(false);

    // The response from the remote device.
    public String response = String.Empty;
}




public class Game
{
	public List<Player> allPlayers = new List<Player>();
	public List<Bomb> allBombs = new List<Bomb>();

	public int myIndex = -1;

	public class Player
	{
		public string username;
		public float x;
		public float z;
		public int playerIndex = -1;
		public bool active = false;

		public Player(string username, int x, int z)
		{
			this.x = x;
			this.z = z;
			this.username = username;
		}

		public void setPosition(int x, int z)
		{
			this.x = x;
			this.z = z;
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

	public void addPlayer(string username)
	{
		int numberOfPlayers = allPlayers.Count;
		int x, z;

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

		for (int playerNumber = 1; playerNumber < messageParts.Count; playerNumber+=5)
		{
			allPlayers[playerNumber].active = Parser.convertBool(messageParts[playerNumber+1]);
			allPlayers[playerNumber].setPosition(
				Convert.ToInt32(messageParts[playerNumber+2]), Convert.ToInt32(messageParts[playerNumber+3]));
		}
	}
}

public static class Parser
{
	private static string[] EOF = new string[]{"<EOF>"};

	public static List<string> cleanEOF(string content)
	{
		//clean and put into buffer
		string[] messages = content.Split(EOF,StringSplitOptions.None);//EOF taken out
		List<string> listOfMessages = new List<string>();
		foreach (string message in messages)
		{
			if (message != "")
			{
				listOfMessages.Add (message);
			}
		}
		return listOfMessages;
	}

	public static bool convertBool(string tf)
	{
		if (tf == "T")
		{
			return true;
		}
		return false;
	}

	public static List<string> split(string content)
	{
		string[] strings = content.Split(';');
		List<string> messageParts = new List<string>();
		foreach (string s in strings)
		{
			if (s != "")
			{
				messageParts.Add(s);
			}
		}
		return messageParts;
	}
}


public class Client : MonoBehaviour
{
	// The Game state object. Gets overwritten.
	public static Game game;

	// The port number for the remote device.
	private const int port = 11000;
	// Socket and IP information
	private static Socket client;
	private static IPEndPoint remoteEP;
	private static IPHostEntry ipHostInfo;
	private static IPAddress ipAddress;
	private static StateObject send_so;
	private static StateObject recv_so;

	// Registration & LogIn info #Anthony
	string registerinfo = "Registering: ";
    const string loginHeader = "Attempting Login: ";
	string logininfo = "";
	private static string myuser = "";
	private static int myindex = -1;
	//need notion of being connected --Anthony
	public static bool connected = false;
	//public static bool inlobby = false;
	private static bool registered = false;
	private static bool loggedin = false;
	private static bool gamestart = false;
	private static bool gameover = false;


	#region Unity Stuff: Start() & FixedUpdate()

	//Assuming that the method is Always called at the start of a scene since its not static
	void Start()
	{
		SceneEnter();
	}

	// Update is called once per frame
	//private int timerCount = 0;
	//private int timesSent = 0;
	//Jeffrey
	//using this to send and build messages given by player object
	void FixedUpdate()
	{
		if(gamestart)
		{
			gamestart = false;
			Application.LoadLevel("Bomberman");//<-----Whatever scene that needs to be loaded
		}
		if(gameover)
		{
			gameover = !gameover;
			Application.LoadLevel("Lobby");
		}
	}

	#endregion

	#region Connecting & Log-In

	//renaming to something other than start, client attempts to connect before login attempted --Anthony
	// Triggers by clicking the Log In button.
    public void connecting()
    {
        // Connect to a remote device.
        try
        {
            // Establish the remote endpoint for the socket.
            // The name of the remote device is "host.contoso.com".
            ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            
			// You don't have to touch this. Determine server IP address:
			switch (IP.Server)
			{
			case IP.Address.ANTHONY:
				ipAddress = IPAddress.Parse(IP.Anthony); break;
			case IP.Address.JEFFREY:
				ipAddress = IPAddress.Parse(IP.Jeffrey); break;
			case IP.Address.FAYE:
				ipAddress = IPAddress.Parse(IP.Faye); break;
			default:
				ipAddress = ipHostInfo.AddressList[0]; break;
			}

			// Make the connection
            remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			// Create the state object for sending.
            send_so = new StateObject();
            send_so.workSocket = client;
			// Create the state object for receiving.
			recv_so = new StateObject();
			recv_so.workSocket = client;
			
			// Connect to the remote endpoint.
            client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), send_so);
            // Waits for 5 seconds for connection to be done
            send_so.connectDone.WaitOne(5000);

            // Send test data to the remote device.
            Send(client, "This is a test<EOF>", send_so);
			send_so.sendDone.WaitOne(/*5000*/);
			// Send test data to the remote device.
            Send(client, "Test 2<EOF>", send_so);
			send_so.sendDone.WaitOne(/*5000*/);
			// Send test data to the remote device.
            Send(client, "Third test.<EOF>", send_so);
			send_so.sendDone.WaitOne(/*5000*/);
			
			// Receive the response from the remote device.
            Receive(recv_so);
            recv_so.receiveDone.WaitOne(1000);
            // Write the response to the console.
            Debug.Log("Response received : " + recv_so.response);
			connected = true;
			SceneEnter();//check the scene once a connection has been established
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

	//use Start()?
	public void SceneEnter()
	{
		Debug.Log("SceneEnter");
		if(!connected)
		{
			return;
		}
		//check which scene im in and do a thing
		// ------------------Anthony------------------------//
		if (GameObject.Find ("login") != null) 
		{
			logininfo = loginHeader + GameObject.Find ("login").GetComponentInChildren<login> ().strsend ();
			specifiedSend(logininfo, 1500);
			//lazySend(logininfo);
			if (connected && loggedin) 
			{
				//Debug.Log("login sceneenter");
				Application.LoadLevel ("Lobby");
			}
		} 
		else if (GameObject.Find ("lobby") != null) 
		{
			//contact server and tell it it's in the lobby
			Debug.Log("lobby sceneenter");
			lazySend ("Awaiting Game " + myuser);
			
			//Debug.Log("Client.cs lobbymsg: "+myuser);
		} 
		else if (GameObject.Find ("reg") != null) 
		{
			//Debug.Log("Register page");
			registerinfo += GameObject.Find ("reg").GetComponentInChildren<register> ().getsendinfo ();
			lazySend (registerinfo);
			if (registered) {
				//connected = true;
				Application.LoadLevel ("Lobby");
			} else {
				//connected = false;
				GameObject.Find ("reg").GetComponentInChildren<register> ().dbg = "Register Failed";
			}
		} 

		//-----------Anthony--------------------//
	}

	// When client connects with the server
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Create the state object.
            StateObject state = (StateObject)ar.AsyncState;
            // Retrieve the socket from the state object.
            Socket client = state.workSocket;

            // Complete the connection.
            client.EndConnect(ar);

            Debug.Log("Socket connected to " + client.RemoteEndPoint.ToString());

            // Signal that the connection has been made.
            state.connectDone.Set();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

	#endregion

	#region Message Parsing Methods

	// Parse through the messages, usually just if-statements leading out to methods
	public static void parseMessage(string content)
	{
		if(content == "")
		{
			return;
		}
		
		Debug.Log("Parsing "+content);
		
		if(content.Contains("Login Success"))//Login Success test<EOF>
		{
			loginSuccess(content);
		}
		else if (content.Contains ("Bomb;"))
		{
			game.addBomb(Parser.split(content));

		}
		else if (content.Contains ("Player;"))
		{
			game.updatePlayers(Parser.split(content));
		}
		else if (content.Contains("Game Over;"))
		{
			gameover = true;
		}
		else if(content.Contains("Game Start"))
		{
			startGame(content);
		}
		
		//for the lobby
		else if(content.Contains("P2L: ")||content.Contains("P1L: ")||content.Contains("P3L: ")||content.Contains("P4L: "))
		{
			lobbyUpdate(content);
		}
		else if(content.Contains("P1R: ready")||content.Contains("P2R: ready")||content.Contains("P3R: ready")||content.Contains("P4R: ready"))
		{
			lobbyUpdateReady(content);
		}
		else if(content.Contains("P1R: not")||content.Contains("P2R: not")||content.Contains("P3R: not")||content.Contains("P4R: not"))
		{
			lobbyUpdateNotReady(content);
		}
		else if(content.Contains("Login Failed"))
		{
			loginFailed();
		}
		else if(content.Contains("Registered"))
		{
			registrationSuccess(content);
		}
		else if(content.Contains("Invalid Registry"))
		{
			registrationFailed();
		}
	}

	private static void startGame(string content)
	{
		List<string> messageParts = Parser.split (content);
		gamestart = true;
		// Create a new game object, overwriting the previous
		game = new Game();
		// Add the number of players
		int numberOfPlayers = Convert.ToInt32(messageParts[1]);
		for (int i = 0; i < numberOfPlayers; i++)
		{
			game.addPlayer ("");
		}
	}

	private static void loginSuccess(string content)
	{
		Debug.Log("Login Success");
		connected = true;
		//Debug.Log(content);
		content = content.Substring(14);//username<EOF>
		myuser = content.Substring(0);
		//Debug.Log("Client.cs username: "+myuser);
		loggedin = true;
	}
	
	private static void loginFailed()
	{
		Debug.Log("Login Failed");
		GameObject.Find("login").GetComponentInChildren<login>().wipe();
		connected = false;
	}
	
	private static void registrationSuccess(string content)
	{
		Debug.Log("Registering successful");
		registered = true;
		myuser = content.Substring(11);
	}
	
	private static void registrationFailed()
	{
		Debug.Log("Registering failed");
		registered = false;
		connected = false;
	}
	
	private static void lobbyUpdate(string content)
	{
		//Debug.Log ("Client.cs: Player is in the Lobby");
		//Debug.Log("Line 254 "+content);
		//Debug.Log("Line 255 "+content.Substring(1,1));
		int ind = int.Parse(content.Substring(1,1));//get the player number from the msg
		ind--;
		string msg = content.Substring(5);//username|x|y<EOF>
		int index = msg.IndexOf("|");
		string user = msg.Substring(0,index);
		//Debug.Log("Line 262: "+user);
		msg = msg.Substring(index+1);//x|y<EOF>
		index = msg.IndexOf("|");

		//index = msg.IndexOf("<");

		if(user == myuser)//if this message happens to contain the client's info
		{
			myindex = ind;
		}
		//Debug.Log("Client-Lobby: user: "  + user + " Index: " + ind);
		lobby.setup(user,ind);
	}
	
	private static void lobbyUpdateReady(string content)
	{
		int ind = int.Parse(content.Substring(1,1));//get the player number from the msg
		ind--;
		Debug.Log("Client.cs ready: "+ind);
		lobby.readyupdates(ind);
	}
	
	private static void lobbyUpdateNotReady(string content)
	{
		int ind = int.Parse(content.Substring(1,1));//get the player number from the msg
		ind--;
		Debug.Log("Client.cs not ready: " + ind);
		lobby.notreadyupdate(ind);
	}
	
	#endregion

	#region Sending Methods

	private void Send(Socket client, String data, StateObject so)
	{
		// Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.
        client.BeginSend(byteData, 0, byteData.Length, 0,
                         new AsyncCallback(SendCallback), so);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            StateObject so = (StateObject)ar.AsyncState;
            //Socket client = so.workSocket;

            // Complete sending the data to the remote device.
            //int bytesSent = client.EndSend(ar);
            //Debug.Log("Sent " + bytesSent + " bytes to server.");

            // Signal that all bytes have been sent.
            so.sendDone.Set();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

	public static void lazySend(String content)
	{
		// Convert the string data to byte data using ASCII encoding.
		byte[] byteData = Encoding.ASCII.GetBytes(content+"<EOF>");
		
		// Begin sending the data to the remote device.
		client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), send_so);
		//Debug.Log("Sending: " + content + "<EOF>");
		//Send(client, content+"<EOF>", send_so);
		send_so.sendDone.WaitOne(100);
		
		lazyReceive();
		recv_so.receiveDone.WaitOne(100);
	}

	public static void specifiedSend(string content,int delay)
	{
		// Convert the string data to byte data using ASCII encoding.
		byte[] byteData = Encoding.ASCII.GetBytes(content+"<EOF>");
		
		// Begin sending the data to the remote device.
		client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), send_so);
		Debug.Log("Sending: " + content + "<EOF>");
		//Send(client, content+"<EOF>", send_so);
		send_so.sendDone.WaitOne(delay);
		
		lazyReceive();
		recv_so.receiveDone.WaitOne(delay);
	}

	//made a nonstatic wrapper for the lazysend so that other objects can use the send function
	public void sendmsg(string content)
	{
		lazySend(content);
	}

	#endregion

	#region Receiving Methods
	
	// Wrapper function called to receive messages
	private void Receive(StateObject state)
	{
		try
		{
			Socket client = state.workSocket;
			
			// Begin receiving the data from the remote device.
			client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
			                    new AsyncCallback(ReceiveCallback), state);
		}
		catch (Exception e)
		{
			Debug.Log(e.ToString());
		}
	}
	
	// The actual receiving
	private static void ReceiveCallback(IAsyncResult ar)
	{
		try
		{
			// Retrieve the state object and the client socket 
			// from the asynchronous state object.
			StateObject state = (StateObject)ar.AsyncState;
			Socket mclient = state.workSocket;
			
			// Read data from the remote device.
			int bytesRead = mclient.EndReceive(ar);
			
			if (bytesRead > 0)
			{
				// There might be more data, so store the data received so far.
				state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
				
				// Check for end-of-file tag. If it is not there, read more data.
				String content = state.sb.ToString();
				if (content.IndexOf("<EOF>") > -1)
				{
					state.flushSB();
					
					Debug.Log("Received: \t"+content);

					// Break messages into multiple ones and parse them
					List<string> messages = Parser.cleanEOF(content);
					foreach (string message in messages)
					{
						Debug.Log("Message: " + message);
						parseMessage(message);
					}

					// Setup a new state object and do a receive callback relay.
					StateObject newstate = new StateObject();
					newstate.workSocket = client;
					mclient.BeginReceive(newstate.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), newstate);
				}
				else
				{
					// Not all data received. Get more.
					mclient.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
				}
			}
			else
			{
				Debug.Log("Connection close has been requested.");
				// Signal that all bytes have been received.
			}
		}
		catch (Exception e)
		{
			Debug.Log(e.ToString());
		}
	}
	
	private static void lazyReceive()
	{
		try
		{
			// Begin receiving the data from the remote device.
			client.BeginReceive(recv_so.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), recv_so);
		}
		catch (Exception e)
		{
			Debug.Log(e.ToString());
		}
	}
	
	#endregion

	#region Accessors & Mutators
	
	public static string getUser()
	{
		return myuser;
	}
	public static int getIndex()
	{
		return myindex;
	}

	#endregion
	  
}
