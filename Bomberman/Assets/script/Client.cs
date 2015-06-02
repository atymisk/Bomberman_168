using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;


// State object for receiving data from remote device.
public class StateObject
{
    // Client socket
    public Socket workSocket = null;

    // Size of receive buffer
    public const int BufferSize = 256;

    // Receive buffer
    public byte[] buffer = new byte[BufferSize];

    // Received data string
    public StringBuilder sb = new StringBuilder();

	// Flush the StringBuilder so it's now empty
	public void flushSB()
	{
		sb = new StringBuilder();
	}

    // AutoResetEvents for connection and receiving and sending messages
    public AutoResetEvent connectDone = new AutoResetEvent(false);
    public AutoResetEvent receiveDone = new AutoResetEvent(false);
    public AutoResetEvent sendDone = new AutoResetEvent(false);

}


public class Client : MonoBehaviour
{
	// The Game state object. Gets overwritten later on.
	public static Game game = null;

	// The port number for the remote device.
	private const int port = 11000;
	// Socket and IP information
	private static Socket client;
	private static StateObject send_so;
	private static StateObject recv_so;

	// Registration & LogIn info #Anthony
	const string registerHeader = "Registering: ";
    const string loginHeader = "Attempting Login: ";
	private static string myuser = "";
	private static string lobbyname = "";
	private static int myindex = -1;
	//need notion of being connected --Anthony
	public static bool connected = false;
	//public static bool inlobby = false;
	private static bool registered = false;
	private static bool loggedin = false;
	private static bool gamestart = false;
	private static bool gameover = false;
	private static bool createdorjoined = false;


	#region Unity Stuff: Start() & FixedUpdate()

	// Set up so that the game can run in background
    void Awake()
    {
        Application.runInBackground = true;
    }

	//Assuming that the method is Always called at the start of a scene since its not static
	void Start()
	{
		SceneEnter();
	}

	//using this to send and build messages given by player object
	void FixedUpdate()
	{
		if(createdorjoined || gameover)
		{
			lobby.ready = false;
			Debug.Log("Go To Lobby");
			gameover = !gameover;
			createdorjoined = false;
			Application.LoadLevel("Lobby");
		}
		if(gamestart)
		{
			gamestart = false;
			Application.LoadLevel("Bomberman");//<-----Whatever scene that needs to be loaded
		}
	}

	void OnApplicationQuit()
	{
		Debug.Log("CLOSING GAME");
		DisconnectMe();
	}
	#endregion

	#region Connecting & Log-In

	//renaming to something other than start, client attempts to connect before login attempted --Anthony
	// Triggers by clicking the Log In button.
    public void connecting()
    {
        // Connect to a remote device.
		if(connected)//prevent trying to connect twice
		{
			return;
		}
        try
        {
            // Establish the remote endpoint for the socket.
            // The name of the remote device is "host.contoso.com".
            //ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            
			// You don't have to touch this. Determine server IP address:
			IPAddress ipAddress = IPAddress.Parse(IPManager.getServerIP());

			// Make the connection
			IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

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
			
			// Receive the response from the remote device.
            Receive(recv_so);
            recv_so.receiveDone.WaitOne(1000);

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
			string logininfo = loginHeader + GameObject.Find ("login").GetComponentInChildren<login> ().strsend ();
			specifiedSend(logininfo, 1000);
			//lazySend(logininfo);
			if (connected && loggedin) 
			{
				//Debug.Log("login sceneenter");
				Application.LoadLevel ("GameSelect");
			}
		} 
		else if (GameObject.Find ("lobby") != null) 
		{
			//contact server and tell it it's in the lobby
			lobby.resetlobby();//reset the list of names everytime it moves into scene
			lazySend ("Awaiting Game " + myuser + "|" + lobbyname);
			//Debug.Log("Client.cs lobbymsg: "+myuser);
		}
		else if (GameObject.Find ("reg") != null) 
		{
			//Debug.Log("Register page");
			string registerInfo =
				registerHeader + GameObject.Find ("reg").GetComponentInChildren<register> ().getsendinfo ();
			lazySend(registerInfo);
			if (registered) 
			{
				Application.LoadLevel ("GameSelect");
			} 
			else 
			{
				GameObject.Find ("reg").GetComponentInChildren<register> ().dbg = "Register Failed";
			}
		}
		else if(GameObject.Find("gameselect")!=null)
		{
			lobbyname = "";
		}		
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
		
		//Debug.Log("Parsing "+content);
		
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
			game.inprogress = false;
		}
		else if(content.Contains("Game Start"))
		{
			startGame(content);
		}

		else if(content.Contains("Game Created "))
		{
			creatinggame(content);
		}
		else if(content.Contains("Join Approved"))
		{
			joining(content);
		}
		else if(content == "Game Exists Already" || content == "Game DNE" || content == "Game FULL")
		{
			notfoundfullexists();
		}
		else if(content == "Game Found")
		{
			gamefound(content);
		}
		//for the lobby
		else if(content.Contains("P2L:...")||content.Contains("P1L:...")||content.Contains("P3L:...")||content.Contains("P4L:..."))
		{
			lobbyBUpdate(content);
		}
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
		else if(content =="Lobby Refresh")
		{
			lobby.resetlobby();
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
		game.inprogress = true;
		game.myIndex = myindex;
		// Add the number of players
		int numberOfPlayers = Convert.ToInt32(messageParts[1]);
		for (int i = 0; i < numberOfPlayers; i++)
		{
			game.addPlayer ("");
		}
	}

	private static void loginSuccess(string content)
	{
		//Debug.Log("Login Success");
		connected = true;
		//Debug.Log(content);
		content = content.Substring(14);//username<EOF>
		myuser = content.Substring(0);
		//Debug.Log("Client.cs username: "+myuser);
		loggedin = true;
	}
	
	private static void loginFailed()
	{
		connected = false;
		loggedin = false;
		Debug.Log("Login Failed");
		GameObject.Find("login").GetComponentInChildren<login>().wipe();
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
	private static void lobbyBUpdate(string content)
	{
		int ind = int.Parse(content.Substring(1,1));//get the player number from the msg
		ind--;
		lobby.setup("...",ind);
	}
	private static void lobbyUpdate(string content)
	{
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
		//lobby.resetlobby();//<- this causes only the last player who joined to show only their name only
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
	private static void gamefound(string content)
	{
		//11
		//lobbyname = content.Substring(11);//get the lobbyname
		//Debug.Log("FOUND " + lobbyname);
		gameselect.gameexists();
		Debug.Log("Game FOUND");
	}
	private static void notfoundfullexists()
	{
		gameselect.showpop();
	}
	private static void creatinggame(string content)
	{
		//Debug.Log("createdorjoined " + createdorjoined);
		createdorjoined = true;
		//Debug.Log("createdorjoined " + createdorjoined);
		lobbyname = content.Substring(13);//get the lobbyname
		Debug.Log("CREATED " + lobbyname);
	}
	private static void joining(string content)
	{
		//14
		createdorjoined = true;
		lobbyname = content.Substring(14);//get the lobbyname
		Debug.Log("JOINED " + lobbyname);
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
		if(!connected)
		{
			Debug.Log("Not Connected!");
			return;
		}
		//Debug.Log(content);
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
		//Debug.Log("Sending: " + content + "<EOF>");
		//Send(client, content+"<EOF>", send_so);
		send_so.sendDone.WaitOne(delay);
		
		lazyReceive();
		recv_so.receiveDone.WaitOne(delay);
	}

	//made a nonstatic wrapper for the lazysend so that other objects could use the send function
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
					
					//Debug.Log("Received: \t"+content);

					// Break messages into multiple ones and parse them
					List<string> messages = Parser.cleanEOF(content);
					foreach (string message in messages)
					{
						//Debug.Log("Message: " + message);
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
	public string getlobbyname()
	{
		return lobbyname;
	}
	#endregion

	#region Disconnecting
	public static void DisconnectMe()//untested
	{
		if(connected)
		{
			if(game != null && game.inprogress)//if in the middle of a game
			{
				//Game.Player p = game.myself();
				lazySend("Player;"+myindex+";F;"+0+";"+0+";"+0+";"+0+";"+lobbyname+";");//notify others i disconnected and am dead
			}
			lazySend("Disconnect Me " + myuser + "|" + lobbyname);
			client.Shutdown(SocketShutdown.Both);
			client.Close();
			connected = false;
			Debug.Log("Disconnected");
		}
	}
	#endregion
}
