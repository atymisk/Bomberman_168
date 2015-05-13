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
    // Client socket.
    public Socket workSocket = null;

    // Size of receive buffer.
    public const int BufferSize = 256;

    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];

    // Received data string.
    public StringBuilder sb = new StringBuilder();

    // ManualResetEvent now changed to AutoResetEvent
    public AutoResetEvent connectDone = new AutoResetEvent(false);
    public AutoResetEvent receiveDone = new AutoResetEvent(false);
    public AutoResetEvent sendDone = new AutoResetEvent(false);

    // The response from the remote device.
    public String response = String.Empty;
}

//Player class for the client to see OTHER players
public class OtherPlayer
{
	public string username;
	public int x;
	public int z;

	public OtherPlayer(string username, int x, int z)
	{
		this.x = x;
		this.z = z;
		this.username = username;
	}
	public void updateXZ(int x,int z)
	{
		this.x = x;
		this.z = z;
	}
}

public class Client : MonoBehaviour
{
    public float x;
    public float z;
    static string data = "Player;0;T;0;0;0end;1;T;0;0;1end;2;T;0;0;2end;3;F;0;0;3end;";
    static string bdata = "";
	//int TimerCount = 0;
	//int TimeSent = 0;
	string message;
    // The port number for the remote device.
    private const int port = 11000;
    static string[] stringSeparators = new string[] { "<EOF>" };
	//--Anthony-- public IP's to consider for my device/router just in case
	//71.94.130.204
	//70.187.161.177

	private static string[] EOF = new string[]{"<EOF>"};

	private static Socket client;
	private static IPEndPoint remoteEP;
	private static IPHostEntry ipHostInfo;
	private static IPAddress ipAddress;
	private static StateObject send_so;
	private static StateObject recv_so;

	public static OtherPlayer[] otherplayers = new OtherPlayer[4];//needs to be only the others, not yourself
	// Registration & LogIn info #Anthony
	string registerinfo = "Registering: ";
    string logininfo = "Attempting Login: ";
	private static string myuser = "";
	private static int myindex = 0;
	//need notion of being connected --Anthony
	public static bool connected = false;
	//public static bool inlobby = false;
	private static bool registered = false;
	private static bool loggedin = false;

	private static List<string> msgbuffer = new List<string>();
	//Assuming that the method is Always called at the start of a scene since its not static
	void Start()
	{
		SceneEnter();
	}

	//renaming to something other than start, client attempts to connect before login attempted --Anthony
    public void connecting()
    {
        // Connect to a remote device.
        try
        {
            // Establish the remote endpoint for the socket.
            // The name of the remote device is "host.contoso.com".
            ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
			ipAddress = IPAddress.Parse("169.234.29.80");
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
			logininfo += GameObject.Find ("login").GetComponentInChildren<login> ().strsend ();
			specifiedSend(logininfo, 1500);
			//lazySend(logininfo);
			if (connected) 
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

					state.sb = new StringBuilder("");

					Debug.Log("Received: \t"+content);
					cleanEOF(content);
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
	public static void cleanEOF(string content)
	{
		//clean and put into buffer
		string[] messages = content.Split(EOF,StringSplitOptions.None);//EOF taken out
		if(messages.Length > 0)
		{
			foreach(string m in messages)
			{
				parseMessages(m);
			}
		}
		//parseMessages();
	}
	public static void parseMessages(string content)
	{
		//--Anthony--added these various if's
		//Debug.Log(content);
		Debug.Log("Parsing "+content);
		if(content.Contains("Login Success"))//Login Success test<EOF>
		{
			Debug.Log("Login Success");
			connected = true;
			//Debug.Log(content);
			content = content.Substring(14);//username<EOF>
			myuser = content.Substring(0);
			//Debug.Log("Client.cs username: "+myuser);
			loggedin = true;
		}
		else if (content.Contains ("Bomb;"))
		{
			//Debug.Log (content);
			int found = content.IndexOf("Bomb;");
			content = content.Substring(found + 5);
			found = content.IndexOf(";");
			float x = float.Parse(content.Substring(0, found));
			content = content.Substring(found + 1);
			found = content.IndexOf(";");
			float z = float.Parse(content.Substring(0, found));
			content = content.Substring(found + 1);
			found = content.IndexOf(";");
			//int strength = int.Parse(content.Substring(0,found));
			//Jeffrey can't figure this out
			//Instantiate(bomb, new Vector3(x, .5f, z), Quaternion.identity);
			
		}
		else if (content.Contains ("Player;"))
		{
			data = content;
		}
		//for the lobby
		else if(content.Contains("P2L: ")||content.Contains("P1L: ")||content.Contains("P3L: ")||content.Contains("P4L: "))
		{
			
			//Debug.Log("Line 254 "+content);
			//Debug.Log("Line 255 "+content.Substring(1,1));
			int ind = int.Parse(content.Substring(1,1));//get the player number from the msg
			//Debug.Log(ind);
			ind--;
			string msg = content.Substring(5);//username|x|y<EOF>
			int index = msg.IndexOf("|");
			string user = msg.Substring(0,index);
			//Debug.Log("Line 262: "+user);
			msg = msg.Substring(index+1);//x|y<EOF>
			index = msg.IndexOf("|");
			int x = int.Parse (msg.Substring(0,index));
			
			msg = msg.Substring(index+1);//y<EOF>
			//index = msg.IndexOf("<");
			int y = int.Parse(msg.Substring(0));
			
			otherplayers[ind] = new OtherPlayer(user,x,y);
			if(user == myuser)//if this message happens to contain the client's info
			{
				myindex = ind;
			}
			lobby.setup(user,ind);
		}
		else if(content.Contains("P1R: ready")||content.Contains("P2R: ready")||content.Contains("P3R: ready")||content.Contains("P4R: ready"))
		{
			int ind = int.Parse(content.Substring(1,1));//get the player number from the msg
			ind--;
			lobby.readyupdates(ind);
		}
		else if(content.Contains("P1R: not")||content.Contains("P2R: not")||content.Contains("P3R: not")||content.Contains("P4R: not"))
		{
			Debug.Log("Client.cs not ready");
			int ind = int.Parse(content.Substring(1,1));//get the player number from the msg
			ind--;
			lobby.notreadyupdate(ind);
		}
		else if(content == "Login Failed<EOF>")
		{
			Debug.Log("Login Failed");
			connected = false;
		}
		else if(content.Contains("Registered"))
		{
			Debug.Log("Registering successful");
			registered = true;
			myuser = content.Substring(11);
		}
		else if(content == "Invalid Registry<EOF>")
		{
			Debug.Log("Registering failed");
			registered = false;
			connected = false;
		}
	}
	public static string getUser()
	{
		return myuser;
	}
	public static int getIndex()
	{
		return myindex;
	}
	
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
            Socket client = so.workSocket;

            // Complete sending the data to the remote device.
            int bytesSent = client.EndSend(ar);
            //Debug.Log("Sent " + bytesSent + " bytes to server.");

            // Signal that all bytes have been sent.
            so.sendDone.Set();
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
		
		// Write the response to the console.
		//string res = recv_so.response;
//		Debug.Log("Response received : " + res);
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

    // Update is called once per frame
    //private int timerCount = 0;
	//private int timesSent = 0;
	//Jeffrey
	//using this to send and build messages given by player object
    void FixedUpdate()
    {

		if(connected && GameObject.Find ("bullet") != null)//--Anthony-- trying to prevent null reference exceptions
		{
//			string alive;
////			Debug.Log("connected");
//			if (player.activeInHierarchy){
//				alive = "T";
//			} 
//			else {
//				alive = "F";
//			}
//			string index = myindex.ToString();
//			string xs = x.ToString();
//			string zs = z.ToString();
//			message = "Player;" + index + ";" + alive + ";" + xs + ";" + zs + ";";
//	        TimerCount++; // simple timer for testing purposes. delete later.
//	        if (TimerCount > 5)
//	        {
//	            TimerCount = 0;
//
//	            // send test message.
//				//++TimeSent;
//				//lazySend("Hi Server, my timer has reached 0. This is my "+ TimeSent + "th time!");
//				lazySend(message);
//	            //Debug.Log("Resetted "+ TimeSent + "th time.");
//	        } // END of timer implementation
		}
    }
	//Jeffrey protection levels are evil
	public string GetData(){
		return data;
	}
    public string GetBdata()
    {
        return bdata;
    }
    public void deleteBData()
    {
        bdata = "";
    }
}
